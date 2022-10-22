// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using ComputeSharp.SourceGeneration.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PolySharp.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="AnalyzerConfigOptionsProvider"/> type.
/// </summary>
internal static class AnalyzerConfigOptionsProviderExtensions
{
    /// <summary>
    /// Gets the value of a <see cref="bool"/> MSBuild property.
    /// </summary>
    /// <param name="options">The input <see cref="AnalyzerConfigOptionsProvider"/> instance.</param>
    /// <param name="propertyName">The MSBuild property name.</param>
    /// <returns>The value of the specified MSBuild property.</returns>
    /// <remarks>The return value is equivalent to a <c>'$(PropertyName)' == 'true'</c> check.</remarks>
    public static bool GetBoolMSBuildProperty(this AnalyzerConfigOptionsProvider options, string propertyName)
    {
        // MSBuild property that are visible to the compiler are available here with the "build_property." prefix
        return
            options.GlobalOptions.TryGetValue($"build_property.{propertyName}", out string? propertyValue) &&
            string.Equals(propertyValue, bool.TrueString, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the value of an MSBuild property representing a semicolon-separated list of <see cref="string"/>-s.
    /// </summary>
    /// <param name="options">The input <see cref="AnalyzerConfigOptionsProvider"/> instance.</param>
    /// <param name="propertyName">The MSBuild property name.</param>
    /// <returns>The value of the specified MSBuild property.</returns>
    public static ImmutableArray<string> GetStringArrayMSBuildProperty(this AnalyzerConfigOptionsProvider options, string propertyName)
    {
        if (options.GlobalOptions.TryGetValue($"build_property.{propertyName}", out string? propertyValue))
        {
            using ImmutableArrayBuilder<string> builder = ImmutableArrayBuilder<string>.Rent();

            foreach (string part in propertyValue.Split(',', ';'))
            {
                string trimmed = part.Trim();

                if (trimmed.Length > 0)
                {
                    builder.Add(trimmed);
                }
            }

            return builder.ToImmutable();
        }

        return ImmutableArray<string>.Empty;
    }
}