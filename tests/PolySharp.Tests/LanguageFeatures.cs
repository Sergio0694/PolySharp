using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace PolySharp.Tests;

internal class TestClass
{
    private string? name;

    // AllowNullAttribute
    public void TakeValue1([AllowNull] string name)
    {
    }

    // DisallowNullAttribute
    public void TakeValue2([DisallowNull] string? name)
    {
    }

    // DoesNotReturnAttribute
    [DoesNotReturn]
    public void Throws()
    {
        throw null!;
    }

    // DoesNotReturnIfAttribute
    public void Throws([DoesNotReturnIf(true)] bool value)
    {
    }

    // MaybeNullAttribute
    [return: MaybeNull]
    public string ModifyValue()
    {
        return null;
    }

    // MaybeNullWhenAttribute
    public bool ModifyValue([MaybeNullWhen(true)] out string result)
    {
        result = null;

        return true;
    }

    // MemberNotNullAttribute
    [MemberNotNull(nameof(name))]
    public void AssignsField()
    {
        this.name = "";
    }

    // MemberNotNullWhenAttribute
    [MemberNotNullWhen(true, nameof(name))]
    public bool ConditionallyAssignsField()
    {
        this.name = "";

        return true;
    }

    // NotNullAttribute
    public void TakeValue3([NotNull] string? value)
    {
        throw null!;
    }

    // NotNullIfNotNullAttribute
    [return: NotNullIfNotNull(nameof(value))]
    public string? TakeValue4(string? value)
    {
        return value;
    }

    // NotNullWhenAttribute
    public bool TakeValue([NotNullWhen(true)] out string? value)
    {
        value = this.name ?? "";

        return true;
    }

    [RequiresPreviewFeatures]
    public void PreviewApi()
    {
    }
}

internal class TestClassWithRequiredMembers
{
    // SetsRequiredMembersAttribute
    [SetsRequiredMembers]
    public TestClassWithRequiredMembers()
    {
        Name = "";
    }

    // CompilerFeatureRequiredAttribute, RequiredMemberAttribute
    public required string Name { get; init; }
}

// IsExternalInit
internal record Person(string Name);

internal readonly struct TestStruct
{
    private readonly int number;

    // UnscopedRefAttribute
    [UnscopedRef]
    public readonly ref readonly int GetRef()
    {
        return ref this.number;
    }
}

internal static class IndexAndRangeTests
{
    // Index
    public static int TestIndex(ReadOnlySpan<int> numbers)
    {
        return numbers[^1];
    }

    // Range
    public static ReadOnlySpan<int> TestRange(ReadOnlySpan<int> numbers)
    {
        return numbers[1..^4];
    }
}

internal class AnotherTestClass
{
    // CallerArgumentExpressionAttribute
    public string AutomaticName(int number, [CallerArgumentExpression(nameof(number))] string name = "")
    {
        return name;
    }

    [SkipLocalsInit]
    public void Stackalloc()
    {
        _ = stackalloc int[8];
    }

    public void Handler(string name, [InterpolatedStringHandlerArgument(nameof(name))] ref TestHandler handler)
    {
    }
}

[InterpolatedStringHandler]
internal struct TestHandler
{
}