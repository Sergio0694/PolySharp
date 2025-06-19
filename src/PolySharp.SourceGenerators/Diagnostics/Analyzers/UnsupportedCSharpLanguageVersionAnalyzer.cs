// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using PolySharp.SourceGenerators.Extensions;
using static PolySharp.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates an error whenever a source-generator attribute is used with not high enough C# version enabled.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class UnsupportedCSharpLanguageVersionAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [UnsupportedCSharpLanguageVersionError];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        // Check that the language version is not high enough, otherwise no diagnostic should ever be produced
        context.RegisterCompilationAction(static context =>
        {
            if (!context.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsupportedCSharpLanguageVersionError, null));
            }
        });
    }
}
