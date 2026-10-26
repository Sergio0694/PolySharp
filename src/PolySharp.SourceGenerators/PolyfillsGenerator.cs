using System.Linq;
using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Extensions;
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
        // Prepare all the generation options in a single incremental model
        IncrementalValueProvider<GenerationOptions> generationOptions =
            context.AnalyzerConfigOptionsProvider
            .Select(GetGenerationOptions);

        // Gather the options and framework support needed by pre-initialization types
        IncrementalValueProvider<(GenerationOptions Options, bool HasExcludeFromCodeCoverageAttribute)> preInitializationOptions =
            generationOptions.Combine(
                context.CompilationProvider.Select(static (compilation, _) =>
                    compilation.HasAccessibleTypeWithMetadataName("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute")));

        // Generate sources for pre-initialization types
        context.RegisterSourceOutput(preInitializationOptions, EmitPreInitializationTypes);

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