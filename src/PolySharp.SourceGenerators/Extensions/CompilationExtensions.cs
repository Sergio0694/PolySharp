// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        // If there is only a single matching symbol, check its accessibility
        if (compilation.GetTypeByMetadataName(fullyQualifiedMetadataName) is INamedTypeSymbol typeSymbol)
        {
            return compilation.IsSymbolAccessibleWithin(typeSymbol, compilation.Assembly);
        }

        // Otherwise, check all available types
        foreach (INamedTypeSymbol currentTypeSymbol in compilation.GetTypesByMetadataName(fullyQualifiedMetadataName))
        {
            if (compilation.IsSymbolAccessibleWithin(currentTypeSymbol, compilation.Assembly))
            {
                return true;
            }
        }

        return false;
    }
}