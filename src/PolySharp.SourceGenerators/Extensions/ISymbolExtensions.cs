using Microsoft.CodeAnalysis;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extensions for <see cref="ISymbol"/>.
/// </summary>
internal static class ISymbolExtensions
{
    /// <summary>
    /// Checks whether a type has the <c>Microsoft.CodeAnalysis.EmbeddedAttribute</c> annotation.
    /// </summary>
    /// <param name="symbol">The <see cref="ISymbol"/> instance to check for the attribute.</param>
    /// <returns>Whether or not <paramref name="symbol"/> has the embedded attribute.</returns>
    public static bool HasEmbeddedAttribute(this ISymbol symbol)
    {
        foreach (AttributeData attribute in symbol.GetAttributes())
        {
            // The '[Embedded]' attribute is special, so we just match it by name.
            // There could be any number of copies of it across different assemblies.
            if (attribute.AttributeClass is
                {
                    ContainingNamespace:
                    {
                        ContainingNamespace:
                        {
                            ContainingNamespace.IsGlobalNamespace: true,
                            Name: "Microsoft",
                        },
                        Name: "CodeAnalysis",

                    },
                    Name: "EmbeddedAttribute",
                })
            {
                return true;
            }
        }

        return false;
    }
}