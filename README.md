![image](https://user-images.githubusercontent.com/10199417/197341200-3810e13c-9104-4911-90fc-b8add0862458.png)

# What is it? 🚀

**PolySharp** provides Source-only polyfills for C# language features, to easily use all runtime-agnostic features downlevel. The package is distributed as a source generator, so that it will automatically detect which polyfills are needed depending on the target framework and project in use: just add a reference to **PolySharp**, set your C# language version to latest, and have fun!

# Available packages 📦

| Name | Description | Latest version |
| ------ | ------  | ------ |
| **PolySharp** | Source-only polyfills for C# language features | [![NuGet](https://img.shields.io/nuget/vpre/PolySharp.svg)](https://www.nuget.org/packages/PolySharp/) |

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

# Options ⚙️

**PolySharp**'s generation can be configured through some MSBuild properties to set in consuming projects.

The following properties are available:
- `PolySharpUsePublicAccessibilityForGeneratedTypes`: changes the accessibility of generated types from `internal` to `public`