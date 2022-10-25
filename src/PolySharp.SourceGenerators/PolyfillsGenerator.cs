using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Helpers;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A source generator injecting all needed C# polyfills at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class PolyfillsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// A regex to extract the fully qualified type name of a type from its embedded resource name.
    /// </summary>
    private const string EmbeddedResourceNameToFullyQualifiedTypeNameRegex = @"^PolySharp\.SourceGenerators\.EmbeddedResources(?:\.RuntimeSupported)?\.(System(?:\.\w+)+)\.cs$";

    /// <summary>
    /// The collection of fully qualified type names for language support types.
    /// </summary>
    private readonly ImmutableArray<string> languageSupportTypeNames = ImmutableArray.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        where !resourceName.StartsWith("PolySharp.SourceGenerators.EmbeddedResources.RuntimeSupported.")
        select Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value);

    /// <summary>
    /// The collection of fully qualified type names for runtime supported types.
    /// </summary>
    private readonly ImmutableArray<string> runtimeSupportedTypeNames = ImmutableArray.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        where resourceName.StartsWith("PolySharp.SourceGenerators.EmbeddedResources.RuntimeSupported.")
        select Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value);

    /// <summary>
    /// The mapping of fully qualified type names to embedded resource names.
    /// </summary>
    private readonly ImmutableDictionary<string, string> fullyQualifiedTypeNamesToResourceNames = ImmutableDictionary.CreateRange(
        from string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames()
        select new KeyValuePair<string, string>(Regex.Match(resourceName, EmbeddedResourceNameToFullyQualifiedTypeNameRegex).Groups[1].Value, resourceName));

    /// <summary>
    /// The dictionary of cached sources to produce.
    /// </summary>
    private readonly ConcurrentDictionary<GeneratedType, SourceText> manifestSources = new();

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Prepare all the generation options in a single incremental model
        IncrementalValueProvider<GenerationOptions> generationOptions =
            context.AnalyzerConfigOptionsProvider
            .Select((options, _) =>
            {
                // Check whether the generated types should use public accessibility. Consuming projects can define the
                // $(PolySharpUsePublicAccessibilityForGeneratedTypes) MSBuild property to configure this however they need.
                bool usePublicAccessibilityForGeneratedTypes = options.GetBoolMSBuildProperty("PolySharpUsePublicAccessibilityForGeneratedTypes");

                // Do the same as above but for the $(PolySharpIncludeRuntimeSupportedAttributes) property
                bool includeRuntimeSupportedAttributes = options.GetBoolMSBuildProperty("PolySharpIncludeRuntimeSupportedAttributes");

                // Gather the list of any polyfills to exclude from generation (this can help to avoid conflicts with other generators). That's because
                // generators see the same compilation and can't know what others will generate, so $(PolySharpExcludeGeneratedTypes) can solve this issue.
                ImmutableArray<string> excludeGeneratedTypes = options.GetStringArrayMSBuildProperty("PolySharpExcludeGeneratedTypes");

                // Gather the list of polyfills to explicitly include in the generation. This will override combinations expressed above.
                ImmutableArray<string> includeGeneratedTypes = options.GetStringArrayMSBuildProperty("PolySharpIncludeGeneratedTypes");

                return new GenerationOptions(usePublicAccessibilityForGeneratedTypes, includeRuntimeSupportedAttributes, excludeGeneratedTypes, includeGeneratedTypes);
            });

        // Gather the sequence of all types to generate, along with their additional info
        IncrementalValuesProvider<GeneratedType> requestedTypes =
            context.CompilationProvider
            .Combine(generationOptions)
            .SelectMany((info, _) =>
            {
                // Helper function to check whether a type should be included for generation
                bool ShouldIncludeGeneratedType(string name)
                {
                    // First check whether the type is accessible, and if it is already then there is nothing left to do
                    if (info.Left.HasAccessibleTypeWithMetadataName(name))
                    {
                        return false;
                    }

                    // If the explicit list of types to generate isn't empty, take it into account.
                    // Types will be generated only if explicitly requested and not explicitly excluded.
                    if (info.Right.IncludeGeneratedTypes.Length > 0)
                    {
                        return
                            info.Right.IncludeGeneratedTypes.AsImmutableArray().Contains(name) &&
                            !info.Right.ExcludeGeneratedTypes.AsImmutableArray().Contains(name);
                    }

                    // Otherwise, check that the type is not in the list of excluded types
                    return !info.Right.ExcludeGeneratedTypes.AsImmutableArray().Contains(name);
                }

                using ImmutableArrayBuilder<GeneratedType> builder = ImmutableArrayBuilder<GeneratedType>.Rent();

                // First go through the language support types
                foreach (string name in this.languageSupportTypeNames)
                {
                    if (ShouldIncludeGeneratedType(name))
                    {
                        builder.Add(new GeneratedType(name, IsPublicAccessibilityRequired: info.Right.UsePublicAccessibilityForGeneratedTypes));
                    }
                }

                // Only go through the runtime supported attributes if explicitly requested or if the explicit set of included types is not empty.
                // That is, attributes from this category are only emitted if opted-in, or if any of them has explicitly been requested by the user.
                if (info.Right.IncludeRuntimeSupportedAttributes ||
                    info.Right.IncludeGeneratedTypes.Length > 0)
                {
                    foreach (string name in this.runtimeSupportedTypeNames)
                    {
                        if (ShouldIncludeGeneratedType(name))
                        {
                            builder.Add(new GeneratedType(name, IsPublicAccessibilityRequired: info.Right.UsePublicAccessibilityForGeneratedTypes));
                        }
                    }
                }

                return builder.ToImmutable();
            });

        // Generate source for the current type depending on current accessibility
        context.RegisterSourceOutput(requestedTypes, (context, info) =>
        {
            // Get the source text from the cache, or load it if needed
            if (!this.manifestSources.TryGetValue(info, out SourceText? sourceText))
            {
                string resourceName = this.fullyQualifiedTypeNamesToResourceNames[info.FullyQualifiedMetadataName];

                using Stream stream = typeof(PolyfillsGenerator).Assembly.GetManifestResourceStream(resourceName);

                // If public accessibility has been requested, we need to update the loaded source files
                if (info.IsPublicAccessibilityRequired)
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
                _ = this.manifestSources.TryAdd(info, sourceText);
            }

            // Finally generate the source text
            context.AddSource($"{info.FullyQualifiedMetadataName}.g.cs", sourceText);
        });
    }
}