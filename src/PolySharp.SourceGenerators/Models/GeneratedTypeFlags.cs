namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// Fixups for <see cref="GeneratedType"/>.
/// </summary>
internal enum GeneratedTypeFixup
{
    /// <inheritdoc cref="AvailableTypeFixup.None"/>
    None,

    /// <inheritdoc cref="AvailableTypeFixup.RemoveMethodImplAttributes"/>
    RemoveMethodImplAttributes,

    /// <summary>
    /// Generates the <c>[UnmanagedCallersOnly]</c> type in a different namespace and adds a <c>global using</c> type alias for it with the same name.
    /// </summary>
    /// <remarks>This is needed when methods annotated with the attribute have to be assigned to delegates, which Roslyn will otherwise block.</remarks>
    AliasUnmanagedCallersOnlyAttributeType,

    /// <summary>
    /// Skips generating <c>[UnmanagedCallersOnly]</c> and only emit a type alias for it.
    /// </summary>
    GenerateUnmanagedCallersOnlyAttributeTypeAliasOnly
}
