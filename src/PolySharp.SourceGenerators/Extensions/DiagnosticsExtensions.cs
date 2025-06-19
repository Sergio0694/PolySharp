using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Helpers;
using PolySharp.SourceGenerators.Models;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for <see cref="DiagnosticInfo"/>, in various scenarios.
/// </summary>
internal static class DiagnosticsExtensions
{
    /// <summary>
    /// Adds a new diagnostics to the target builder.
    /// </summary>
    /// <param name="diagnostics">The collection of produced <see cref="DiagnosticInfo"/> instances.</param>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    public static void Add(
        this ImmutableArrayBuilder<DiagnosticInfo> diagnostics,
        DiagnosticDescriptor descriptor,
        params object[] args)
    {
        diagnostics.Add(DiagnosticInfo.Create(descriptor, args));
    }

    /// <summary>
    /// Registers an output node into an <see cref="IncrementalGeneratorInitializationContext"/> to output diagnostics.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> instance.</param>
    /// <param name="diagnostics">The input <see cref="IncrementalValuesProvider{TValues}"/> sequence of diagnostics.</param>
    public static void ReportDiagnostics(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<DiagnosticInfo> diagnostics)
    {
        context.RegisterSourceOutput(diagnostics, static (context, diagnostic) => context.ReportDiagnostic(diagnostic.ToDiagnostic()));
    }
}