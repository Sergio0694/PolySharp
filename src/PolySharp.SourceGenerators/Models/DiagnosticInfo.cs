using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using PolySharp.SourceGenerators.Helpers;

namespace PolySharp.SourceGenerators.Models;

/// <summary>
/// A model for a serializeable diagnostic info.
/// </summary>
/// <param name="Descriptor">The wrapped <see cref="DiagnosticDescriptor"/> instance.</param>
/// <param name="Arguments">The diagnostic arguments.</param>
internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, EquatableArray<string> Arguments)
{
    /// <summary>
    /// Creates a new <see cref="Diagnostic"/> instance with the state from this model.
    /// </summary>
    /// <returns>A new <see cref="Diagnostic"/> instance with the state from this model.</returns>
    public Diagnostic ToDiagnostic()
    {
        return Diagnostic.Create(Descriptor, null, Arguments.ToArray());
    }

    /// <summary>
    /// Creates a new <see cref="DiagnosticInfo"/> instance with the specified parameters.
    /// </summary>
    /// <param name="descriptor">The input <see cref="DiagnosticDescriptor"/> for the diagnostics to create.</param>
    /// <param name="args">The optional arguments for the formatted message to include.</param>
    /// <returns>A new <see cref="DiagnosticInfo"/> instance with the specified parameters.</returns>
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, params object[] args)
    {
        return new(descriptor, args.Select(static arg => arg.ToString()).ToImmutableArray());
    }
}