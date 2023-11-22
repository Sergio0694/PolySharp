using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: TargetPlatform("windows6.1")]

[assembly: DisableRuntimeMarshalling]

namespace PolySharp.Tests;

internal class RandomApis
{
    [StackTraceHidden]
    public void HideMe()
    {
    }
}

internal class PlatformSpecificApis
{
    [UnmanagedCallersOnly]
    [SuppressGCTransition]
    public static void NativeFunction()
    {
    }

    [ObsoletedOSPlatform("windows6.1")]
    public void Obsoleted()
    {
    }

    [SupportedOSPlatform("windows6.1")]
    public void Supported()
    {
    }

    [SupportedOSPlatformGuard("windows6.1")]
    public void SupportedGuard()
    {
    }

    [UnsupportedOSPlatform("windows6.1")]
    public void Unsupported()
    {
    }

    [UnsupportedOSPlatformGuard("windows6.1")]
    public void UnsupportedGuard()
    {
    }
}

internal class ReflectionApis
{
    [RequiresUnreferencedCode("No idea what I'll reflect on")]
    public void ReflectOnSomethingCrazy()
    {
    }

    public void ReflectOnInputType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
    }

    [DynamicDependency("PolySharp.Tests.ReflectionApis.ReflectOnSomethingCrazy()")]
    public void ReflectDependingOnStuff()
    {
    }

    [UnconditionalSuppressMessage("Don't worry about it", "0000")]
    public void SuppressEverything(Type type)
    {
        _ = type.GetProperties();
    }

    [RequiresDynamicCode("This method creates some type at runtime")]
    public void MakeUpSomeNewType()
    {
    }

    [RequiresAssemblyFiles("This method needs access to the assembly files")]
    public void ReferenceSomeAssemblyFile()
    {
    }
}

internal class AccessorApis
{
#pragma warning disable IDE0044
    private int field;
#pragma warning restore IDE0044

    [StackTraceHidden]
    public void HideMe()
    {
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = nameof(field))]
    public static extern ref int GetField(RandomApis obj);
}