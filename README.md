![image](https://user-images.githubusercontent.com/10199417/197341200-3810e13c-9104-4911-90fc-b8add0862458.png)
[![.NET](https://github.com/Sergio0694/PolySharp/workflows/.NET/badge.svg)](https://github.com/Sergio0694/PolySharp/actions) [![NuGet](https://img.shields.io/nuget/dt/PolySharp.svg)](https://www.nuget.org/stats/packages/PolySharp?groupby=Version) [![NuGet](https://img.shields.io/nuget/vpre/PolySharp.svg)](https://www.nuget.org/packages/PolySharp/)

# What is it? 🚀

**PolySharp** provides generated, source-only polyfills for C# language features, to easily use all runtime-agnostic features downlevel. The package is distributed as a source generator, so that it will automatically detect which polyfills are needed depending on the target framework and project in use: just add a reference to **PolySharp**, set your C# language version to latest, and have fun!

# Documentation 📖

**PolySharp** includes the following polyfills:
- Nullability attributes (for [nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)):
  - `[AllowNull]`
  - `[DisallowNull]`
  - `[DoesNotReturn]`
  - `[DoesNotReturnIf]`
  - `[MaybeNull]`
  - `[MaybeNullWhen]`
  - `[MemberNotNull]`
  - `[MemberNotNullWhen]`
  - `[NotNull]`
  - `[NotNullIfNotNull]`
  - `[NotNullWhen]`
- `Index` and `Range` (see [indices and ranges](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/ranges-indexes))
- `[UnscopedRef]` (see [low-level struct improvements](https://github.com/dotnet/csharplang/blob/main/proposals/low-level-struct-improvements.md))
- Required members (see [required modifier](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/required))
  - `[RequiredMember]`
  - `[SetsRequiredMembers]`
- `[CompilerFeatureRequired]` (needed to support several features)
- `[IsExternalInit]` (needed for [init-only properties](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/init))
- `[SkipLocalsInit]` (see [docs](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/general#skiplocalsinit-attribute))
- Interpolated string handlers (see [docs](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/interpolated-string-handler))
  - `[InterpolatedStringHandler]`
  - `[InterpolatedStringHandlerArgument]`
- `[CallerArgumentExpression]` (see [docs](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-10.0/caller-argument-expression))
- `[RequiresPreviewFeatures]` (needed for [preview features](https://github.com/dotnet/designs/blob/main/accepted/2021/preview-features/preview-features.md))
- `[AsyncMethodBuilder]` (needed for [custom method builder types](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-10.0/async-method-builders))

To leverage them, make sure to bump your C# language version. You can do this by setting the `<LangVersion>` MSBuild property in your project. For instance, by adding `<LangVersion>11.0</LangVersion>` (or your desired C# version) to the first `<PropertyGroup>` of your .csproj file. For more info on this, [see here](https://sergiopedri.medium.com/enabling-and-using-c-9-features-on-older-and-unsupported-runtimes-ce384d8debb), but remember that you don't need to manually copy polyfills anymore: simply adding a reference to **PolySharp** will do this for you automatically.

It also includes the following optional runtime-supported polyfills:
- Reflection annotation attributes (see [docs](https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)):
  - `[DynamicallyAccessedMembers]`
  - `[DynamicDependency]`
  - `[RequiresUnreferencedCode]`
  - `[UnconditionalSuppressMessage]`
- `[StackTraceHidden]` (see [here](https://makolyte.com/csharp-exclude-exception-throw-helper-methods-from-the-stack-trace/))
- `[UnmanagedCallersOnly]` (see [docs](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.unmanagedcallersonlyattribute)))
- Platform support annotation attributes (see [docs](https://learn.microsoft.com/dotnet/standard/analyzers/platform-compat-analyzer)):
  - `[ObsoletedOSPlatform]`
  - `[SupportedOSPlatform]`
  - `[SupportedOSPlatformGuard]`
  - `[TargetPlatform]`
  - `[UnsupportedOSPlatform]`
  - `[UnsupportedOSPlatformGuard]`

# Options ⚙️

**PolySharp**'s generation can be configured through some MSBuild properties to set in consuming projects.

The following properties are available:
- "PolySharpUsePublicAccessibilityForGeneratedTypes": makes all generated types public.
- "PolySharpIncludeRuntimeSupportedAttributes": enables polyfills for (dummy) runtime-supported attributes too.
- "PolySharpExcludeGeneratedTypes": excludes specific types from generation (';' or ',' separated type names).
- "PolySharpIncludeGeneratedTypes": only includes specific types for generation (';' or ',' separated type names).
