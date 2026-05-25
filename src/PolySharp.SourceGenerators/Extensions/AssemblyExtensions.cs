// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Reflection;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="Assembly"/> type.
/// </summary>
internal static class AssemblyExtensions
{
    /// <summary>
    /// Reads a manifest resue from the specified assembly and returns its content as a <see cref="string"/>.
    /// </summary>
    /// <param name="assembly">The assemlby to read from.</param>
    /// <param name="resourceName">The name of the resource to read.</param>
    /// <returns>The resource contents.</returns>
    public static string ReadManifestResource(this Assembly assembly, string resourceName)
    {
        using Stream stream = typeof(PolyfillsGenerator).Assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }
}