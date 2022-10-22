using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PolySharp.SourceGenerators.Extensions;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A source generator injecting all needed C# polyfills at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed class PolyfillsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// The dictionary of cached sources to produce.
    /// </summary>
    private readonly ConcurrentDictionary<(string FullyQualifiedMetadataName, bool UsePublicAccessibility), SourceText> manifestSources = new();

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Check whether the generated types should use public accessibility. Consuming projects can define the
        // $(PolySharpUsePublicAccessibilityForGeneratedTypes) MSBuild property to configure this however they need.
        IncrementalValueProvider<bool> usePublicAccessibilityForGeneratedTypes =
            context.AnalyzerConfigOptionsProvider
            .Select((options, _) => options.GetBoolMSBuildProperty("PolySharpUsePublicAccessibilityForGeneratedTypes"));

        // Enumerate each embedded resource: each will map to a type that can be polyfilled. Types are
        // polyfilled one by one if not available, to avoid errors if users had some manual polyfills already.
        foreach (string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames())
        {
            // Strip the "PolySharp.SourceGenerators.EmbeddedResources." prefix and the ".cs" suffix from each resource name
            int prefixLength = "PolySharp.SourceGenerators.EmbeddedResources.".Length;
            int suffixLength = ".cs".Length;
            string fullyQualifiedMetadataName = resourceName.Substring(prefixLength, resourceName.Length - (prefixLength + suffixLength));

            // Get an IncrementalValueProvider<bool> representing whether the current type is already available
            IncrementalValueProvider<bool> isTypeAccessible =
                context.CompilationProvider
                .Select((compilation, _) => compilation.HasAccessibleTypeWithMetadataName(fullyQualifiedMetadataName));

            // Prepare the generation options for the current polyfill
            IncrementalValueProvider<(bool IsTypeAccessible, bool UsePublicAccessibilityForGeneratedTypes)> generationOptions =
                isTypeAccessible.Combine(usePublicAccessibilityForGeneratedTypes);

            // Generate source for the current type depending on current accessibility
            context.RegisterSourceOutput(generationOptions, (context, generationOptions) =>
            {
                // The type is already accessible, so nothing more to do
                if (generationOptions.IsTypeAccessible)
                {
                    return;
                }

                // Get the source text from the cache, or load it if needed
                if (!this.manifestSources.TryGetValue((fullyQualifiedMetadataName, generationOptions.UsePublicAccessibilityForGeneratedTypes), out SourceText? sourceText))
                {
                    using Stream stream = typeof(PolyfillsGenerator).Assembly.GetManifestResourceStream(resourceName);

                    // If public accessibility has been requested, we need to update the loaded source files
                    if (!generationOptions.UsePublicAccessibilityForGeneratedTypes)
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
                    _ = this.manifestSources.TryAdd((fullyQualifiedMetadataName, generationOptions.UsePublicAccessibilityForGeneratedTypes), sourceText);
                }

                // Finally generate the source text
                context.AddSource($"{fullyQualifiedMetadataName}.g.cs", sourceText);
            });
        }
    }
}