namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model to hold information on a type to generate.
/// </summary>
/// <param name="FullyQualifiedMetadataName">The fully qualified type name of the type to generate.</param>
/// <param name="FixupType">The types of syntax fixups to apply.</param>
internal sealed record GeneratedType(string FullyQualifiedMetadataName, SyntaxFixupType FixupType);
