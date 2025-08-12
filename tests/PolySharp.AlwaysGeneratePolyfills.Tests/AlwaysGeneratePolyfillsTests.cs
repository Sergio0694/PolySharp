using PolySharp.TestLibraryA;
using PolySharp.TestLibraryB;
using System;

namespace PolySharp.AlwaysGeneratePolyfills.Tests;

public class ConsumerClass
{
    public Type TestLibraryA => typeof(LibraryAClass);
    public Type TestLibraryB => typeof(LibraryBClass);
    public required string RequiredValue { get; init; }
    public string? OptionalValue { get; init; }
}
