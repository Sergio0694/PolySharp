using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using PolySharp.SourceGenerators.Constants;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Helpers;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <inheritdoc/>
partial class PolyfillsGenerator
{
    /// <summary>
    /// A regex to extract the fully qualified type name of a type from its embedded resource name.
    /// </summary>
    private const string EmbeddedResourceNameToFullyQualifiedTypeNameRegex = @"^PolySharp\.SourceGenerators\.EmbeddedResources(?:\.RuntimeSupported)?\.(System(?:\.\w+)+)\.cs$";

    /// <summary>
    /// The collection of fully qualified type names for language support types.
    /// </summary>
    private static readonly ImmutableArray<string> LanguageSupportTypeNames = ImmutableArray.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        where !resourceName.StartsWith("PolySharp.SourceGenerators.EmbeddedResources.RuntimeSupported.")
        select Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value);

    /// <summary>
    /// The collection of fully qualified type names for runtime supported types.
    /// </summary>
    private static readonly ImmutableArray<string> RuntimeSupportedTypeNames = ImmutableArray.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        where resourceName.StartsWith("PolySharp.SourceGenerators.EmbeddedResources.RuntimeSupported.")
        select Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value);

    /// <summary>
    /// The mapping of fully qualified type names to embedded resource names.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> FullyQualifiedTypeNamesToResourceNames = ImmutableDictionary.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        select new KeyValuePair<string, string>(Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value, resourceName));

    /// <summary>
    /// The dictionary of cached sources to produce.
    /// </summary>
    private readonly ConcurrentDictionary<GeneratedType, SourceText> manifestSources = new();

    /// <summary>
    /// Extracts the <see cref="GenerationOptions"/> value for the current generation.
    /// </summary>
    /// <param name="options">The input <see cref="AnalyzerConfigOptionsProvider"/> instance.</param>
    /// <param name="_">The cancellation token for the operation.</param>
    /// <returns>The <see cref="GenerationOptions"/> for the current generation.</returns>
    private static GenerationOptions GetGenerationOptions(AnalyzerConfigOptionsProvider options, CancellationToken _)
    {
        // Check whether the generated types should use public accessibility. Consuming projects can define the
        // $(PolySharpUsePublicAccessibilityForGeneratedTypes) MSBuild property to configure this however they need.
        bool usePublicAccessibilityForGeneratedTypes = options.GetBoolMSBuildProperty(PolySharpMSBuildProperties.UsePublicAccessibilityForGeneratedTypes);

        // Do the same as above but for the $(PolySharpIncludeRuntimeSupportedAttributes) property
        bool includeRuntimeSupportedAttributes = options.GetBoolMSBuildProperty(PolySharpMSBuildProperties.IncludeRuntimeSupportedAttributes);

        // Gather the list of any polyfills to exclude from generation (this can help to avoid conflicts with other generators). That's because
        // generators see the same compilation and can't know what others will generate, so $(PolySharpExcludeGeneratedTypes) can solve this issue.
        ImmutableArray<string> excludeGeneratedTypes = options.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.ExcludeGeneratedTypes);

        // Gather the list of polyfills to explicitly include in the generation. This will override combinations expressed above.
        ImmutableArray<string> includeGeneratedTypes = options.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.IncludeGeneratedTypes);

        return new(usePublicAccessibilityForGeneratedTypes, includeRuntimeSupportedAttributes, excludeGeneratedTypes, includeGeneratedTypes);
    }

    /// <summary>
    /// Calculates the collection of <see cref="GeneratedType"/>-s to emit.
    /// </summary>
    /// <param name="info">The input info for the current generation.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <returns>The collection of <see cref="GeneratedType"/>-s to emit.</returns>
    private static ImmutableArray<GeneratedType> GetGeneratedTypes((Compilation Compilation, GenerationOptions Options) info, CancellationToken token)
    {
        // Helper function to check whether a type should be included for generation
        static bool ShouldIncludeGeneratedType(Compilation compilation, GenerationOptions options, string name, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // First check whether the type is accessible, and if it is already then there is nothing left to do
            if (compilation.HasAccessibleTypeWithMetadataName(name))
            {
                return false;
            }

            // If the explicit list of types to generate isn't empty, take it into account.
            // Types will be generated only if explicitly requested and not explicitly excluded.
            if (options.IncludeGeneratedTypes.Length > 0)
            {
                return
                    options.IncludeGeneratedTypes.AsImmutableArray().Contains(name) &&
                    !options.ExcludeGeneratedTypes.AsImmutableArray().Contains(name);
            }

            // Otherwise, check that the type is not in the list of excluded types
            return !options.ExcludeGeneratedTypes.AsImmutableArray().Contains(name);
        }

        using ImmutableArrayBuilder<GeneratedType> builder = ImmutableArrayBuilder<GeneratedType>.Rent();

        // First go through the language support types
        foreach (string name in LanguageSupportTypeNames)
        {
            if (ShouldIncludeGeneratedType(info.Compilation, info.Options, name, token))
            {
                builder.Add(new GeneratedType(name, info.Options.UsePublicAccessibilityForGeneratedTypes));
            }
        }

        // Only go through the runtime supported attributes if explicitly requested or if the explicit set of included types is not empty.
        // That is, attributes from this category are only emitted if opted-in, or if any of them has explicitly been requested by the user.
        if (info.Options.IncludeRuntimeSupportedAttributes ||
            info.Options.IncludeGeneratedTypes.Length > 0)
        {
            foreach (string name in RuntimeSupportedTypeNames)
            {
                if (ShouldIncludeGeneratedType(info.Compilation, info.Options, name, token))
                {
                    builder.Add(new GeneratedType(name, info.Options.UsePublicAccessibilityForGeneratedTypes));
                }
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Emits the source for a given <see cref="GeneratedType"/> object.
    /// </summary>
    /// <param name="context">The input <see cref="SourceProductionContext"/> instance to use to emit code.</param>
    /// <param name="type">The <see cref="GeneratedType"/> object with info on the source to emit.</param>
    private void EmitGeneratedType(SourceProductionContext context, GeneratedType type)
    {
        // Get the source text from the cache, or load it if needed
        if (!this.manifestSources.TryGetValue(type, out SourceText? sourceText))
        {
            string resourceName = FullyQualifiedTypeNamesToResourceNames[type.FullyQualifiedMetadataName];

            using Stream stream = typeof(PolyfillsGenerator).Assembly.GetManifestResourceStream(resourceName);

            // If public accessibility has been requested, we need to update the loaded source files
            if (type.IsPublicAccessibilityRequired)
            {
                using StreamReader reader = new(stream);

                // Read the source and replace all internal keywords with public. Use a space before and after the identifier
                // to avoid potential false positives. This could also be done by loading the source tree and using a syntax
                // rewriter, or just by retrieving the type declaration syntax and updating the modifier tokens, but since the
                // change is so minimal, it can very well just be done this way to keep things simple, that's fine in this case.
                string originalSource = reader.ReadToEnd();
                string adjustedSource = originalSource.Replace(" internal ", " public ");

                sourceText = SourceText.From(adjustedSource, Encoding.UTF8);
            }
            else
            {
                // If the default accessibility is used, we can load the source directly
                sourceText = SourceText.From(stream, Encoding.UTF8, canBeEmbedded: true);
            }

            // Cache the generated source (if we raced against another thread, just discard the result)
            _ = this.manifestSources.TryAdd(type, sourceText);
        }

        // Finally generate the source text
        context.AddSource($"{type.FullyQualifiedMetadataName}.g.cs", sourceText);
    }
}