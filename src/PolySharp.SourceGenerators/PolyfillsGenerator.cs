using System.Linq;
using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A source generator injecting all needed C# polyfills at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class PolyfillsGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit any sources that are needed right after initialization
        context.RegisterPostInitializationOutput(EmitPostInitializationSources);

        // Prepare all the generation options in a single incremental model
        IncrementalValueProvider<GenerationOptions> generationOptions =
            context.AnalyzerConfigOptionsProvider
            .Select(GetGenerationOptions);

        // Get the sequence of all available types that could be generated
        IncrementalValuesProvider<AvailableType> availableTypes =
            context.CompilationProvider
            .SelectMany(GetAvailableTypes);

        // Gather the sequence of all types to generate after filtering
        IncrementalValuesProvider<GeneratedType> generatedTypes =
            availableTypes
            .Combine(generationOptions)
            .Where(IsAvailableTypeSelected)
            .Select(GetGeneratedType);

        // Generate source for the current type depending on current accessibility
        context.RegisterSourceOutput(generatedTypes, EmitGeneratedType);

        // Get all potential type forwarded type names
        IncrementalValuesProvider<string> typeForwardNames =
            context.CompilationProvider
            .SelectMany(GetCoreLibTypes);

        // Filter the type forward names with the current generation options
        IncrementalValuesProvider<string> filteredTypeForwardNames =
            typeForwardNames
            .Combine(generationOptions)
            .Where(IsCoreLibTypeSelected)
            .Select(GetCoreLibType);

        // Generate the type forwards, if any
        context.RegisterImplementationSourceOutput(filteredTypeForwardNames, EmitTypeForwards);
    }
}