using System.Collections.Concurrent;
using System.IO;
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
    private readonly ConcurrentDictionary<string, SourceText> manifestSources = new();

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Enumerate each embedded resource: each will map to a type that can be polyfilled. Types are
        // polyfilled one by one if not available, to avoid errors if users had some manual polyfills already.
        foreach (string resourceName in typeof(PolyfillsGenerator).Assembly.GetManifestResourceNames())
        {
            // Strip the "EmbeddedResource." prefix and the ".cs" suffix from each resource name
            string fullyQualifiedMetadataName = resourceName.Substring("EmbeddedResources.".Length, resourceName.Length - ("EmbeddedResources.".Length + ".cs".Length));

            // Get an IncrementalValueProvider<bool> representing whether the current type is already available
            IncrementalValueProvider<bool> isTypeAccessible =
                context.CompilationProvider
                .Select((compilation, _) => compilation.HasAccessibleTypeWithMetadataName(fullyQualifiedMetadataName));

            // Generate source for the current type depending on current accessibility
            context.RegisterSourceOutput(isTypeAccessible, (context, isTypeAccessible) =>
            {
                // The type is already accessible, so nothing more to do
                if (isTypeAccessible)
                {
                    return;
                }

                // Get the source text from the cache, or load it if needed
                if (!this.manifestSources.TryGetValue(fullyQualifiedMetadataName, out SourceText? sourceText))
                {
                    using Stream stream = typeof(PolyfillsGenerator).Assembly.GetManifestResourceStream(resourceName);

                    sourceText = SourceText.From(stream);

                    _ = this.manifestSources.TryAdd(fullyQualifiedMetadataName, sourceText);
                }

                // Finally generate the source text
                context.AddSource($"{fullyQualifiedMetadataName}.g.cs", sourceText);
            });
        }
    }
}