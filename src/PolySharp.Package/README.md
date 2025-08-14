![image](https://user-images.githubusercontent.com/10199417/197341200-3810e13c-9104-4911-90fc-b8add0862458.png)

# What is it? 🚀

**PolySharp** provides generated, source-only polyfills for C# language features, to easily use all runtime-agnostic features downlevel. The package is distributed as a source generator, so that it will automatically detect which polyfills are needed depending on the target framework and project in use: just add a reference to **PolySharp**, set your C# language version to latest, and have fun!

# TLDR? What is this for? ✨

Put simply: are you working on .NET Framework, or UWP, or some other older .NET runtime and still would like to use all the cool new features that C# 13 has? Well this library lets you do just that! It will generate for you all the "magic types" that the C# compiler needs to "see" in order for it to allow using new language features even if you're not using the latest framework out there.

Here's an example of some of the new features that **PolySharp** can enable downlevel:

![image](https://user-images.githubusercontent.com/10199417/198630498-df1e215c-6788-4aef-8ba5-b0b71772233e.png)

> **Note**: not all the new C# features can be "tricked" this way (eg. those requiring runtime support, such as [static abstract members](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/static-virtual-interface-members), still won't work). But almost everything else will (eg. nullability annotations, pattern matching, etc.), and this applies to a big number of new C# features. Try **PolySharp** out, don't get stuck on C# 6.0 or other older versions! 🎉

> **Note**: use on .NET Framework 3.5 is particularly limited due to shortcomings of the BCL there. In particular, the `System.Range` type will not be generated unless `System.ValueTuple` is referenced (meaning that eg. list patterns won't work by default), and some features such as records will not be usable at all due to the C# compiler missing some necessary APIs that cannot be polyfilled. All other features should work as expected.

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
- `[StringSyntax]` (needed to enable [syntax highlight in the IDE](https://github.com/dotnet/runtime/issues/62505))
- `[ModuleInitializer]` (needed to enable [custom module initializers](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers))
- `[RequiresLocation]` (needed to enable [ref readonly parameters](https://github.com/dotnet/csharplang/issues/6010))
- `[CollectionBuilder]` (needed for [collection expressions](https://github.com/dotnet/csharplang/issues/5354))
- `[Experimental]` (needed for [experimental features](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/experimental-attribute))
- `[OverloadResolutionPriority]` (needed for [overload resolution priority](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#overload-resolution-priority))
- `[ParamsCollection]` (needed for [params collection](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#params-collections))
- `[ConstantExpected]` (see [proposal](https://github.com/dotnet/runtime/issues/33771))

To leverage them, make sure to bump your C# language version. You can do this by setting the `<LangVersion>` MSBuild property in your project. For instance, by adding `<LangVersion>13.0</LangVersion>` (or your desired C# version) to the first `<PropertyGroup>` of your .csproj file. For more info on this, [see here](https://sergiopedri.medium.com/enabling-and-using-c-9-features-on-older-and-unsupported-runtimes-ce384d8debb), but remember that you don't need to manually copy polyfills anymore: simply adding a reference to **PolySharp** will do this for you automatically.

It also includes the following optional runtime-supported polyfills:
- Reflection annotation attributes (see [docs](https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)):
  - `[DynamicallyAccessedMembers]`
  - `[DynamicDependency]`
  - `[RequiresUnreferencedCode]`
  - `[RequiresDynamicCode]`
  - `[UnconditionalSuppressMessage]`
  - `[RequiresAssemblyFiles]`
- `[StackTraceHidden]` (see [here](https://makolyte.com/csharp-exclude-exception-throw-helper-methods-from-the-stack-trace/))
- `[UnmanagedCallersOnly]` (see [docs](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.unmanagedcallersonlyattribute))
- Platform support annotation attributes (see [docs](https://learn.microsoft.com/dotnet/standard/analyzers/platform-compat-analyzer)):
  - `[ObsoletedOSPlatform]`
  - `[SupportedOSPlatform]`
  - `[SupportedOSPlatformGuard]`
  - `[TargetPlatform]`
  - `[UnsupportedOSPlatform]`
  - `[UnsupportedOSPlatformGuard]`
- `[SuppressGCTransition]` (see [here](https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/))
- `[DisableRuntimeMarshalling]` (see [here](https://learn.microsoft.com/dotnet/standard/native-interop/disabled-marshalling))
- `[UnsafeAccessor]` (see [here](https://github.com/dotnet/runtime/issues/81741))
- `[InlineArray]` (see [here](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-12.0/inline-arrays))
- `[DisableUserUnhandledExceptions]` (see [here](https://github.com/dotnet/runtime/issues/103105))
- Attribute model for feature switches with trimming support (see [docs](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/runtime#attribute-model-for-feature-switches-with-trimming-support)):
  - `[FeatureGuard]`
  - `[FeatureSwitchDefinition]`
- `[WasmImportLinkage]` (see [here](https://github.com/dotnet/runtime/pull/93823))

# Options ⚙️

**PolySharp**'s generation can be configured through some MSBuild properties to set in consuming projects.

The following properties are available:
- "PolySharpUsePublicAccessibilityForGeneratedTypes": makes all generated types public.
- "PolySharpIncludeRuntimeSupportedAttributes": enables polyfills for (dummy) runtime-supported attributes too.
- "PolySharpUseInteropServices2NamespaceForUnmanagedCallersOnlyAttribute": moves `[UnmanagedCallersOnly]`.
- "PolySharpExcludeGeneratedTypes": excludes specific types from generation (';' or ',' separated type names).
- "PolySharpIncludeGeneratedTypes": only includes specific types for generation (';' or ',' separated type names).
- "PolySharpExcludeTypeForwardedToDeclarations": never generates any `[TypeForwardedTo]` declarations.

# Debugging

If you suspect the generator does not work as expected, you can debug its operation relatively easily. Let us assume we want to debug the polyfills generation for a project producing assembly named `X`.

## One time setup
1. Clone this repo, e.g. to **C:\work\PolySharp**
1. Add **C:\work\PolySharp\artifacts** to the package sources. E.g. you can use the nuget.config like this:
    ```
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <solution>
        <add key="disableSourceControlIntegration" value="true" />
      </solution>
      <packageSources>
        <add key="Temporary" value="C:\work\PolySharp\artifacts" />
      </packageSources>
    </configuration>
    ```
1. Modify the version of the PolySharp package used by `X` to `1.0.0-alpha`
1. Install [DebugView](https://learn.microsoft.com/en-us/sysinternals/downloads/debugview) or any other trace log viewer.
1. Open a console window where debug iterations will take place. Let us assume it is pwsh and refer it as **the console**.
1. Run the trace viewer, filter messages containing **POLYSP**

## Iteration

On the console at **C:\work\PolySharp**:
1. Delete the NuGet package
    ```
    del -r -force -EA SilentlyContinue ~\.nuget\packages\polysharp\1.0.0-alpha\,.\artifacts\,$env:TEMP\VBCSCompiler
    ```
1. Build the debug version of the package
    ```
    dotnet pack -c:Debug .\src\PolySharp.Package\PolySharp.Package.msbuildproj
    ```
1. Request debug
   ```
   $env:POLYSHARP_DEBUG = "AssemblyName[:TargetFramework]:DebugMode"
   ```
   where:
   - `AssemblyName` is the assembly name of the project in question, in our case `X`
   - `TargetFramework` is optional - denotes the target framework of the project in question. Helpful when multiple target frameworks are built.
   - `DebugMode` is one of the following:
     - `1` or `launch` - results in a `Debugger.Launch()` call
     - `2` or `attach` - results in an infinite loop, you are expected to attach the debugger and break out of the loop. The PID to attach to is in the trace.
     - `3` or `trace` - currently just outputs the values of the build variables to the trace.
1. Build the project X only with the binary logs
   ```
   msbuild /bl /v:m PATH_TO_X_CSPROJ /p:BuildProjectReferences=false
   ```

The steps (1) and (2) are needed to get the debug version of the analyzer, which is easier to debug. They must be run each time you change the analyzer code or just once at the beginning.