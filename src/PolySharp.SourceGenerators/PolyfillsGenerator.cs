using System.Linq;
using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A source generator injecting all needed C# polyfills at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
internal sealed partial class PolyfillsGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Prepare all the generation options in a single incremental model
        IncrementalValueProvider<GenerationOptions> generationOptions =
            context.AnalyzerConfigOptionsProvider
            .Select(GetGenerationOptions);

        // Gather the sequence of all types to generate, along with their additional info
        IncrementalValuesProvider<GeneratedType> requestedTypes =
            context.CompilationProvider
            .Combine(generationOptions)
            .SelectMany(GetGeneratedTypes);

        // Generate source for the current type depending on current accessibility
        context.RegisterSourceOutput(requestedTypes, EmitGeneratedType);
    }
}