// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
/// <remarks>Ported from <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Runtime/CompilerServices/IsExternalInit.cs"/>.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit
{
}