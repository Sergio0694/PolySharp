// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PolySharp.SourceGenerators.Constants;
using PolySharp.SourceGenerators.Extensions;
using static PolySharp.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace PolySharp.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates warnings whenever a PolySharp MSBuild property is set incorrectly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InvalidPolySharpMSBuildOptionAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(InvalidBoolMSBuildProperty, InvalidPolyfillFullyQualifiedMetadataName);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        // Register a compilation start callback to access the analyzer options
        context.RegisterCompilationStartAction(static context =>
        {
            // Validate "UsePublicAccessibilityForGeneratedTypes" is a valid MSBuild bool property (or none)
            if (!context.Options.AnalyzerConfigOptionsProvider.IsValidMSBuildProperty(PolySharpMSBuildProperties.UsePublicAccessibilityForGeneratedTypes))
            {
                // TODO
            }

            // Do the same for "IncludeRuntimeSupportedAttributes"
            if (!context.Options.AnalyzerConfigOptionsProvider.IsValidMSBuildProperty(PolySharpMSBuildProperties.IncludeRuntimeSupportedAttributes))
            {
                // TODO
            }

            ImmutableArray<string> excludeGeneratedTypes = context.Options.AnalyzerConfigOptionsProvider.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.ExcludeGeneratedTypes);

            foreach (string type in excludeGeneratedTypes)
            {
                if (!PolyfillsGenerator.FullyQualifiedTypeNamesToResourceNames.ContainsKey(type))
                {
                    // TODO
                }
            }

            ImmutableArray<string> includeGeneratedTypes = context.Options.AnalyzerConfigOptionsProvider.GetStringArrayMSBuildProperty(PolySharpMSBuildProperties.IncludeGeneratedTypes);

            foreach (string type in includeGeneratedTypes)
            {
                if (!PolyfillsGenerator.FullyQualifiedTypeNamesToResourceNames.ContainsKey(type))
                {
                    // TODO
                }
            }
        });
    }
}
