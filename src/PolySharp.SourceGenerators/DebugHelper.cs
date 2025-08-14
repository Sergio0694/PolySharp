using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PolySharp.SourceGenerators;

internal static class DebugHelper
{
    internal static bool IsTraceEnabled { get; private set; }
    internal static Action<string> TraceWriteLine { get; private set; } = default!;

#pragma warning disable IDE0044 // Add readonly modifier
    private static bool s_debug = true;
#pragma warning restore IDE0044 // Add readonly modifier

    private enum DebugMode
    {
        None,
        Launch,
        Attach,
        Trace,
    }

    internal static void SetupDebugging(this IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.CompilationProvider,
            static (context, compilation) =>
            {
                switch (GetDebugMode(compilation, out string projectInfo))
                {
                    case DebugMode.Launch:
                        Trace.WriteLine($"[POLYSP] {projectInfo} - Launching debugger...");
                        _ = Debugger.Launch();
                        break;
                    case DebugMode.Attach:
                        Trace.WriteLine($"[POLYSP] {projectInfo} - Waiting for debugger attachment to PID {Process.GetCurrentProcess().Id} ...");
                        while (s_debug)
                        {
                            Thread.Sleep(1000);
                        }

                        break;
                    case DebugMode.Trace:
                        IsTraceEnabled = true;
                        TraceWriteLine = msg => Trace.WriteLine($"[POLYSP] {projectInfo} - {msg}");
                        break;
                }
            });
    }

    private static DebugMode GetDebugMode(Compilation compilation, out string projectInfo)
    {
        string asmName = compilation.AssemblyName ?? "Unknown";
        string targetFramework = GetTargetFramework(compilation) ?? "unknown";
        projectInfo = $"{asmName} ({targetFramework})";

        string? debug = Environment.GetEnvironmentVariable("POLYSHARP_DEBUG");
        Trace.WriteLine($"[POLYSP] {projectInfo} - Environment.GetEnvironmentVariable(\"POLYSHARP_DEBUG\") == {debug}");

        if (string.IsNullOrEmpty(debug))
        {
            return DebugMode.None;
        }

        int i = debug.IndexOf(':');
        if (i < 0)
        {
            return DebugMode.None;
        }

        string requestedAsmName = debug[..i];
        if (!requestedAsmName.Equals(asmName, StringComparison.OrdinalIgnoreCase))
        {
            return DebugMode.None;
        }

        ++i;
        int j = debug.IndexOf(':', i);
        if (j >= 0)
        {
            string requestedTargetFramework = debug[i..j];
            if (!requestedTargetFramework.Equals(targetFramework, StringComparison.OrdinalIgnoreCase))
            {
                return DebugMode.None;
            }

            i = j + 1;
        }

        string mode = debug[i..];
        if (int.TryParse(mode, out int parsedMode))
        {
            switch (parsedMode)
            {
                case 1:
                    return DebugMode.Launch;
                case 2:
                    return DebugMode.Attach;
                case 3:
                    return DebugMode.Trace;
                default:
                    return DebugMode.None;
            }
        }

        if (Enum.TryParse(mode, true, out DebugMode parsedDebugMode))
        {
            return parsedDebugMode;
        }

        return DebugMode.None;
    }

    /// <summary>
    /// Extracts the target framework from the compilation using TargetFrameworkAttribute.
    /// </summary>
    /// <param name="compilation">The compilation object.</param>
    /// <returns>The target framework string, or null if not found.</returns>
    private static string? GetTargetFramework(Compilation compilation)
    {
        AttributeData? targetFrameworkAttribute = compilation.Assembly
            .GetAttributes()
            .FirstOrDefault(attr =>
                attr.AttributeClass?.Name == "TargetFrameworkAttribute" &&
                attr.AttributeClass.ContainingNamespace?.ToDisplayString() == "System.Runtime.Versioning");

        if (targetFrameworkAttribute?.ConstructorArguments.Length > 0)
        {
            string? frameworkName = targetFrameworkAttribute.ConstructorArguments[0].Value?.ToString();

            // Parse framework name to extract just the TFM part
            // Examples: ".NETFramework,Version=v4.7.2" -> "net472"
            //           ".NETCoreApp,Version=v6.0" -> "net6.0" 
            //           ".NETStandard,Version=v2.0" -> "netstandard2.0"
            if (!string.IsNullOrEmpty(frameworkName))
            {
                return ParseTargetFrameworkMoniker(frameworkName);
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a full framework name into a target framework moniker.
    /// </summary>
    /// <param name="frameworkName">The full framework name from TargetFrameworkAttribute.</param>
    /// <returns>The target framework moniker (e.g., "net472", "net6.0").</returns>
    private static string ParseTargetFrameworkMoniker(string? frameworkName)
    {
        if (string.IsNullOrEmpty(frameworkName))
        {
            return "unknown";
        }

        // Handle common framework patterns
        if (frameworkName!.StartsWith(".NETFramework,Version=v"))
        {
            string version = frameworkName.Substring(".NETFramework,Version=v".Length);
            return version switch
            {
                "4.7.2" => "net472",
                "4.8" => "net48",
                "4.8.1" => "net481",
                _ => $"net{version.Replace(".", "")}"
            };
        }

        if (frameworkName.StartsWith(".NETCoreApp,Version=v"))
        {
            string version = frameworkName.Substring(".NETCoreApp,Version=v".Length);
            return $"net{version}";
        }

        if (frameworkName.StartsWith(".NETStandard,Version=v"))
        {
            string version = frameworkName.Substring(".NETStandard,Version=v".Length);
            return $"netstandard{version}";
        }

        return frameworkName;
    }
}