using PolySharp.SourceGenerators.Helpers;

namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model to hold all generation options for the source generator.
/// </summary>
/// <param name="UsePublicAccessibilityForGeneratedTypes">Whether to use public accessibility for the generated types.</param>
/// <param name="IncludeRuntimeSupportedAttributes">Whether to also generated dummy runtime supported attributes.</param>
/// <param name="UseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute">Whether to move the <c>[UnmanagedCallersOnly]</c> type to a dummy <c>InteropServices2</c> namespace.</param>
/// <param name="ExcludeTypeForwardedToDeclarations">Whether to never generate any <c>[TypeForwardedTo]</c> declarations automatically.</param>
/// <param name="ExcludeGeneratedTypes">The collection of fully qualified type names of types to exclude from generation.</param>
/// <param name="IncludeGeneratedTypes">The collection of fully qualified type names of types to include in the generation.</param>
internal sealed record GenerationOptions(
    bool UsePublicAccessibilityForGeneratedTypes,
    bool IncludeRuntimeSupportedAttributes,
    bool UseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute,
    bool ExcludeTypeForwardedToDeclarations,
    EquatableArray<string> ExcludeGeneratedTypes,
    EquatableArray<string> IncludeGeneratedTypes);
