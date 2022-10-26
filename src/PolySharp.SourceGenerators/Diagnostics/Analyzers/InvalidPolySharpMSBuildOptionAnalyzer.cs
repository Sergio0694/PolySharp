// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Extensions;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates warnings whenever a PolySharp MSBuild property is set incorrectly.
/// </summary>
/// <remarks>
/// This has to be a generator because emitting diagnostics for analyzer options from an analyzer is not currently supported.
/// See <see href="https://github.com/dotnet/roslyn/issues/64213"/>.
/// </remarks>
[Generator(LanguageNames.CSharp)]
internal sealed partial class InvalidPolySharpMSBuildOptionAnalyzer : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Prepare all the diagnostics for the analyzer options being used
        IncrementalValuesProvider<DiagnosticInfo> analyzerOptionsDiagnostics =
            context.AnalyzerConfigOptionsProvider
            .SelectMany(GetOptionsDiagnostics);

        // Output the diagnostics
        context.ReportDiagnostics(analyzerOptionsDiagnostics);
    }
}
