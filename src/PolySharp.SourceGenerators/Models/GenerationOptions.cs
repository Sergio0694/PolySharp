using PolySharp.SourceGenerators.Helpers;

namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model to hold all generation options for the source generator.
/// </summary>
/// <param name="UsePublicAccessibilityForGeneratedTypes">Whether to use public accessibility for the generated types.</param>
/// <param name="IncludeRuntimeSupportedAttributes">Whether to also generated dummy runtime supported attributes.</param>
/// <param name="ExcludeGeneratedTypes">The collection of fully qualified type names of types to exclude from generation.</param>
internal sealed record GenerationOptions(
    bool UsePublicAccessibilityForGeneratedTypes,
    bool IncludeRuntimeSupportedAttributes,
    EquatableArray<string> ExcludeGeneratedTypes);
