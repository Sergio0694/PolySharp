using System;
using Microsoft.CodeAnalysis.CSharp;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extensions for the <see cref="CSharpCompilation"/> type.
/// </summary>
internal static class CSharpCompilationExtensions
{
    extension(CSharpCompilation)
    {
        /// <summary>
        /// Gets the version of the loaded Roslyn C# compiler assembly.
        /// </summary>
        public static Version AssemblyVersion => typeof(CSharpCompilation).Assembly.GetName().Version;

        /// <summary>
        /// Gets whether the loaded Roslyn C# compiler supports user-defined embedded attributes.
        /// </summary>
        public static bool IsEmbeddedAttributeSupported => CSharpCompilation.AssemblyVersion >= new Version(4, 14);
    }
}
