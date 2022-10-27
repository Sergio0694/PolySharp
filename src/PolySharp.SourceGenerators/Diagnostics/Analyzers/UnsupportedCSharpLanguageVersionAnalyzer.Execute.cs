// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Models;
using static PolySharp.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace PolySharp.SourceGenerators;

/// <inheritdoc/>
partial class UnsupportedCSharpLanguageVersionAnalyzer
{
    /// <summary>
    /// Extracts the <see cref="DiagnosticInfo"/> values for the current generation.
    /// </summary>
    /// <param name="compilation">The input <see cref="Compilation"/> instance.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <returns>The <see cref="DiagnosticInfo"/> values for the current generation.</returns>
    private static ImmutableArray<DiagnosticInfo> GetOptionsDiagnostics(Compilation compilation, CancellationToken token)
    {
        // Check that the language version is not high enough, otherwise no diagnostic should ever be produced
        if (!compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
        {
            return ImmutableArray.Create(DiagnosticInfo.Create(UnsupportedCSharpLanguageVersionError));
        }

        return ImmutableArray<DiagnosticInfo>.Empty;
    }
}
