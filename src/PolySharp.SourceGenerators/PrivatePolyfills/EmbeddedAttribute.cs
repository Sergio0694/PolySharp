// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.CodeAnalysis;

/// <summary>
/// Marks a type as "embedded", meaning it won't ever be visible from other assemblies.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
internal sealed class EmbeddedAttribute : Attribute;