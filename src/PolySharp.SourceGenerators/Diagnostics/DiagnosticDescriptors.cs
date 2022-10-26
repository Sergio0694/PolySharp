// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace PolySharp.SourceGenerators.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when an invalid value has been set for a <see cref="bool"/> MSBuild property.
    /// <para>
    /// Format: <c>"The value "{0}" is not valid for property "{1}" (it has to be a valid MSBuild bool value)"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidBoolMSBuildProperty = new DiagnosticDescriptor(
        id: "POLYSP0001",
        title: "Invalid PolySharp bool MSBuild property",
        messageFormat: "The value \"{0}\" is not valid for property \"{1}\" (it has to be a valid MSBuild bool value)",
        category: typeof(InvalidPolySharpMSBuildOptionAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "PolySharp MSBuild properties that are meant to have a bool value can only be set to \"true\" or \"false\" (case invariant).",
        helpLinkUri: "https://github.com/Sergio0694/PolySharp");

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> indicating when an invalid fully qualified metadata name has been used for a targeted polyfill.
    /// <para>
    /// Format: <c>"The fully qualified metadata name "{0}" used in property "{1}" is not valid, and it does not match any available polyfill type"</c>.
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPolyfillFullyQualifiedMetadataName = new DiagnosticDescriptor(
        id: "POLYSP0002",
        title: "Invalid fully qualified metadata name for polyfill",
        messageFormat: "The fully qualified metadata name \"{0}\" used in property \"{1}\" is not valid, and it does not match any available polyfill type",
        category: typeof(InvalidPolySharpMSBuildOptionAnalyzer).FullName,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only fully qualified metadata names for existing polyfill types should be used as options to configure PolySharp's generation.",
        helpLinkUri: "https://github.com/Sergio0694/PolySharp");
}
