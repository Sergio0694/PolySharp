using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="Compilation"/> type.
/// </summary>
internal static class CompilationExtensions
{
    /// <summary>
    /// Checks whether a given compilation (assumed to be for C#) is using at least a given language version.
    /// </summary>
    /// <param name="compilation">The <see cref="Compilation"/> to consider for analysis.</param>
    /// <param name="languageVersion">The minimum language version to check.</param>
    /// <returns>Whether <paramref name="compilation"/> is using at least the specified language version.</returns>
    public static bool HasLanguageVersionAtLeastEqualTo(this Compilation compilation, LanguageVersion languageVersion)
    {
        return ((CSharpCompilation)compilation).LanguageVersion >= languageVersion;
    }

    /// <summary>
    /// <para>
    /// Checks whether or not a type with a specified metadata name is accessible from a given <see cref="Compilation"/> instance.
    /// </para>
    /// <para>
    /// This method enumerates candidate type symbols to find a match in the following order:
    /// <list type="number">
    ///   <item><description>
    ///     If only one type with the given name is found within the compilation and its referenced assemblies, check its accessibility.
    ///   </description></item>
    ///   <item><description>
    ///     If the current <paramref name="compilation"/> defines the symbol, check its accessibility.
    ///   </description></item>
    ///   <item><description>
    ///     Otherwise, check whether the type exists and is accessible from any of the referenced assemblies.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="compilation">The <see cref="Compilation"/> to consider for analysis.</param>
    /// <param name="fullyQualifiedMetadataName">The fully-qualified metadata type name to find.</param>
    /// <returns>Whether a type with the specified metadata name can be accessed from the given compilation.</returns>
    public static bool HasAccessibleTypeWithMetadataName(this Compilation compilation, string fullyQualifiedMetadataName)
    {
        // Helper to check for accessiblity and '[Embedded]' at the same time (Roslyn doesn't check the latter here)
        static bool IsSymbolAccessibleAndNotEmbedded(Compilation compilation, INamedTypeSymbol symbol)
        {
            // First use the built-in Roslyn check (this doesn't check for '[Embedded]').
            // If the symbol is already not embedded (e.g. it's internal), stop here.
            if (!compilation.IsSymbolAccessibleWithin(symbol, compilation.Assembly))
            {
                return false;
            }

            // If the symbol is defined in this same assembly, then it's considered accessible
            if (SymbolEqualityComparer.Default.Equals(compilation.Assembly, symbol.ContainingAssembly))
            {
                return true;
            }

            // If the type has '[Embedded]' on it, then it is not accessible, because Roslyn
            // will just ignore it completely (even if its accessiblity is 'public' or similar).
            return !symbol.HasEmbeddedAttribute();
        }

        // If there is only a single matching symbol, check its accessibility
        if (compilation.GetTypeByMetadataName(fullyQualifiedMetadataName) is INamedTypeSymbol typeSymbol)
        {
            return IsSymbolAccessibleAndNotEmbedded(compilation, typeSymbol);
        }

        // Otherwise, check all available types
        foreach (INamedTypeSymbol currentTypeSymbol in compilation.GetTypesByMetadataName(fullyQualifiedMetadataName))
        {
            if (IsSymbolAccessibleAndNotEmbedded(compilation, currentTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }
}