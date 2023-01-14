using PolySharp.SourceGenerators.Helpers;

namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model to hold all generation options for the source generator.
/// </summary>
/// <param name="UsePublicAccessibilityForGeneratedTypes">Whether to use public accessibility for the generated types.</param>
/// <param name="IncludeRuntimeSupportedAttributes">Whether to also generated dummy runtime supported attributes.</param>
/// <param name="UseTypeAliasForUnmanagedCallersOnlyAttribute">Whether to move the <c>[UnmanagedCallersOnly]</c> type to another namespace and add a type alias for it.</param>
/// <param name="ExcludeGeneratedTypes">The collection of fully qualified type names of types to exclude from generation.</param>
/// <param name="IncludeGeneratedTypes">The collection of fully qualified type names of types to include in the generation.</param>
internal sealed record GenerationOptions(
    bool UsePublicAccessibilityForGeneratedTypes,
    bool IncludeRuntimeSupportedAttributes,
    bool UseTypeAliasForUnmanagedCallersOnlyAttribute,
    EquatableArray<string> ExcludeGeneratedTypes,
    EquatableArray<string> IncludeGeneratedTypes);
