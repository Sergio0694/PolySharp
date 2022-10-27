// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates an error whenever a source-generator attribute is used with not high enough C# version enabled.
/// </summary>
/// <remarks>
/// This has to be a generator because emitting diagnostics for analyzer options from an analyzer is not currently supported.
/// See <see href="https://github.com/dotnet/roslyn/issues/64213"/>.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed partial class UnsupportedCSharpLanguageVersionAnalyzer : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Prepare all the diagnostics for the analyzer options being used
        IncrementalValuesProvider<DiagnosticInfo> analyzerOptionsDiagnostics =
            context.CompilationProvider
            .SelectMany(GetOptionsDiagnostics);

        // Output the diagnostics
        context.ReportDiagnostics(analyzerOptionsDiagnostics);
    }
}
