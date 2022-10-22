// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Runtime.CompilerServices;

/// <summary>
/// Used to indicate to the compiler that the <c>.locals init</c> flag should not be set in method headers.
/// </summary>
/// <remarks>Ported from <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/SkipLocalsInitAttribute.cs"/>.</remarks>
[AttributeUsage(
    AttributeTargets.Module |
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Interface |
    AttributeTargets.Constructor |
    AttributeTargets.Method |
    AttributeTargets.Property |
    AttributeTargets.Event,
    Inherited = false)]
public sealed class SkipLocalsInitAttribute : Attribute
{
}