namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model to hold information on an available type that could be generated, if requested.
/// </summary>
/// <param name="FullyQualifiedMetadataName">The fully qualified type name of the type.</param>
/// <param name="Fixup">An optional fixup for an available type.</param>
internal sealed record AvailableType(string FullyQualifiedMetadataName, AvailableTypeFixup Fixup);
