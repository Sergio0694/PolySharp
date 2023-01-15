// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PolySharp.SourceGenerators.Constants;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Helpers;
using PolySharp.SourceGenerators.Models;
using static PolySharp.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace PolySharp.SourceGenerators;

/// <inheritdoc/>
partial class InvalidPolySharpMSBuildOptionAnalyzer
{
    /// <summary>
    /// Extracts the <see cref="DiagnosticInfo"/> values for the current generation.
    /// </summary>
    /// <param name="options">The input <see cref="AnalyzerConfigOptionsProvider"/> instance.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <returns>The <see cref="DiagnosticInfo"/> values for the current generation.</returns>
    private static ImmutableArray<DiagnosticInfo> GetOptionsDiagnostics(AnalyzerConfigOptionsProvider options, CancellationToken token)
    {
        using ImmutableArrayBuilder<DiagnosticInfo> builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

        // Validate "UsePublicAccessibilityForGeneratedTypes" is a valid MSBuild bool property (or none)
        if (!options.IsValidMSBuildProperty(PolySharpMSBuildProperties.UsePublicAccessibilityForGeneratedTypes, out string? usePublicAccessibilityForGeneratedTypes))
        {
            builder.Add(InvalidBoolMSBuildProperty, usePublicAccessibilityForGeneratedTypes, PolySharpMSBuildProperties.UsePublicAccessibilityForGeneratedTypes);
        }

        token.ThrowIfCancellationRequested();

        // Do the same for "IncludeRuntimeSupportedAttributes"
        if (!options.IsValidMSBuildProperty(PolySharpMSBuildProperties.IncludeRuntimeSupportedAttributes, out string? includeRuntimeSupportedAttributes))
        {
            builder.Add(InvalidBoolMSBuildProperty, includeRuntimeSupportedAttributes, PolySharpMSBuildProperties.IncludeRuntimeSupportedAttributes);
        }

        token.ThrowIfCancellationRequested();

        // And for "UseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute" as well
        if (!options.IsValidMSBuildProperty(PolySharpMSBuildProperties.UseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute, out string? useInteropServices2NamespaceForUnmanagedCallersOnlyAttribute))
        {
            builder.Add(InvalidBoolMSBuildProperty, useInteropServices2NamespaceForUnmanagedCallersOnlyAttribute, PolySharpMSBuildProperties.UseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute);
        }

        token.ThrowIfCancellationRequested();

        // And for "ExcludeTypeForwardedToDeclarations" as well
        if (!options.IsValidMSBuildProperty(PolySharpMSBuildProperties.ExcludeTypeForwardedToDeclarations, out string? excludeTypeForwardedToDeclarations))
        {
            builder.Add(InvalidBoolMSBuildProperty, excludeTypeForwardedToDeclarations, PolySharpMSBuildProperties.ExcludeTypeForwardedToDeclarations);
        }

        token.ThrowIfCancellationRequested();

        ImmutableArray<string> excludeGeneratedTypes = options.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.ExcludeGeneratedTypes);

        // Validate the fully qualified type names for "ExcludeGeneratedTypes"
        foreach (string type in excludeGeneratedTypes)
        {
            if (!PolyfillsGenerator.FullyQualifiedTypeNamesToResourceNames.ContainsKey(type))
            {
                builder.Add(InvalidPolyfillFullyQualifiedMetadataName, type, PolySharpMSBuildProperties.ExcludeGeneratedTypes);
            }
        }

        token.ThrowIfCancellationRequested();

        ImmutableArray<string> includeGeneratedTypes = options.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.IncludeGeneratedTypes);

        // Do the same for "IncludeGeneratedTypes"
        foreach (string type in includeGeneratedTypes)
        {
            if (!PolyfillsGenerator.FullyQualifiedTypeNamesToResourceNames.ContainsKey(type))
            {
                builder.Add(InvalidPolyfillFullyQualifiedMetadataName, type, PolySharpMSBuildProperties.IncludeGeneratedTypes);
            }
        }

        token.ThrowIfCancellationRequested();

        return builder.ToImmutable();
    }
}
