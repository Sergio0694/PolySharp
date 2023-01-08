using System;

namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// An enum indicating a type of syntax fixup to apply to a generated source.
/// </summary>
[Flags]
internal enum SyntaxFixupType
{
    /// <summary>
    /// No syntax fixup is needed
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Remove all <c>[MethodImpl]</c> attributes.
    /// </summary>
    RemoveMethodImplAttributes = 0x1
}
