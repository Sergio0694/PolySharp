using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if COMP_NETSTANDARD1_1
using System.Collections;
using System.Collections.Generic;

#if !NETSTANDARD2_1
using System.Threading.Tasks;
#endif
#endif

#pragma warning disable CA2255

#if INTERNALS_VISIBLE_TO_TESTS
namespace PolySharp.InternalsVisibleTo.Tests;
#else
namespace PolySharp.Tests;
#endif

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

    public void TakeRegex([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
    }

    [ModuleInitializer]
    public static void InitializeModule()
    {
    }

    public void RefReadonlyMethod(ref readonly int x)
    {
    }

    [Experimental("PS0001")]
    public void ExperimentalMethod()
    {
    }
}

internal class TestClassWithRequiredMembers
{
    // SetsRequiredMembersAttribute
    [SetsRequiredMembers]
#pragma warning disable IDE0290
    public TestClassWithRequiredMembers()
#pragma warning restore IDE0290
    {
        Name = "";
    }

    // CompilerFeatureRequiredAttribute, RequiredMemberAttribute
    public required string Name { get; init; }
}

// IsExternalInit
internal record struct Person(string Name);

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
    public static char TestIndex(string chars)
    {
        return chars[^1];
    }

#if COMP_NETSTANDARD1_1
    // Range
    public static string TestRange(string chars)
    {
        return chars[1..^4];
    }
#endif
}

#if COMP_NETSTANDARD1_1
[CollectionBuilder(typeof(CollectionClass), nameof(Create))]
internal class CollectionClass : IEnumerable<int>
{
    public static CollectionClass Test()
    {
        Test2(1, 2, 3);

        return [1, 2, 3];
    }

    public static void Test2(params CollectionClass collection)
    {

    }

    public static CollectionClass Create(ReadOnlySpan<int> values)
    {
        return new();
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator()
    {
        return null!;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return null!;
    }
}
#endif

internal class AnotherTestClass
{
    // CallerArgumentExpressionAttribute
    public string AutomaticName(int number, [CallerArgumentExpression(nameof(number))] string name = "")
    {
        return name;
    }

#if COMP_NETSTANDARD1_1
    [SkipLocalsInit]
    public void Stackalloc()
    {
        unsafe
        {
            _ = stackalloc int[8];
        }
    }
#endif

    public void Handler(string name, [InterpolatedStringHandlerArgument(nameof(name))] ref TestHandler handler)
    {
    }
}

[InterpolatedStringHandler]
internal struct TestHandler
{
}

#if COMP_NETSTANDARD1_1 && !NETSTANDARD2_1
internal struct TaskLikeType
{
    [AsyncMethodBuilder(typeof(CustomAsyncMethodBuilder))]
    public static async TaskLikeType TestAsync()
    {
        await Task.Delay(1);
    }

    private sealed class CustomAsyncMethodBuilder
    {
        public static CustomAsyncMethodBuilder Create()
        {
            return null!;
        }

        public TaskLikeType Task => default;

        public void SetException(Exception e)
        {
        }

        public void SetResult()
        {
        }

        public void SetStateMachine(IAsyncStateMachine _)
        {
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {

        }
    }
}
#endif

internal static class OverloadResolutionPriorityTests
{
    public static void Test()
    {
        TestOverload(1);
    }

    [Obsolete("Do not use", error: true)]
    [OverloadResolutionPriority(-1)]
    public static void TestOverload(int x)
    {
    }

    public static void TestOverload(int x, int y = 0)
    {
    }
}

internal static class ConstantExpectedTests
{
    public static void CpuIntrinsic([ConstantExpected] int value)
    {
    }

    public static void AnotherCpuIntrinsic([ConstantExpected(Min = 0, Max = 8)] int value)
    {
    }
}

// UnionAttribute, IUnion
internal static class UnionTests
{
    public static int Test()
    {
        IntOrString number = 42;
        IntOrString text = "PolySharp";
        IUnion union = number;

        _ = union.Value;
        _ = GetValue(number);
        _ = GetValue(default);

        return GetValue(text);
    }

    public static int GetValue(IntOrString value)
    {
        return value switch
        {
            int number => number,
            string text => text.Length,
            null => 0
        };
    }

    public static int TestManualUnion()
    {
        IntOrBool number = 42;
        IntOrBool boolean = true;

        _ = GetManualValue(number);
        _ = GetManualValue(default);

        return GetManualValue(boolean);
    }

    public static int GetManualValue(IntOrBool value)
    {
        return value switch
        {
            int number => number,
            bool boolean => boolean ? 1 : 0,
            null => -1
        };
    }

    public readonly union IntOrString(int, string);

    [Union]
    public readonly struct IntOrBool : IUnion
    {
        private readonly int number;
        private readonly bool boolean;
        private readonly byte tag;

        public IntOrBool(int value)
        {
            this.number = value;
            this.tag = 1;
        }

        public IntOrBool(bool value)
        {
            this.boolean = value;
            this.tag = 2;
        }

        public object? Value => this.tag switch
        {
            1 => this.number,
            2 => this.boolean,
            _ => null
        };

        public bool HasValue => this.tag != 0;

        public bool TryGetValue(out int value)
        {
            value = this.number;

            return this.tag == 1;
        }

        public bool TryGetValue(out bool value)
        {
            value = this.boolean;

            return this.tag == 2;
        }
    }
}

[CompilerLoweringPreserve]
internal sealed class TestFlowingAttribute : Attribute;

internal static class ExtensionContainer
{
    // Test for '[Extension]'
    public static bool Test(this string s)
    {
        return s == "PolySharp";
    }

    // Test for '[ExtensionMarker]'
    extension(string s)
    {
        public bool M => s == "PolySharp";
    }
}

// IsClosedTypeAttribute
internal static class ClosedHierarchyTests
{
    public static int Test(ClosedClass value)
    {
        return value switch
        {
            FirstDerivedClass => 1,
            SecondDerivedClass => 2
        };
    }

    public closed class ClosedClass;

    public sealed class FirstDerivedClass : ClosedClass;

    public sealed class SecondDerivedClass : ClosedClass;
}

// RequiresUnsafeAttribute
internal static class RequiresUnsafeTests
{
    public static void Test()
    {
        unsafe
        {
            UnsafeMethod();
        }
    }

#pragma warning disable IDE0380
    public static unsafe void UnsafeMethod()
#pragma warning restore IDE0380
    {
        // The unsafe modifier makes the compiler emit '[RequiresUnsafe]', which cannot be applied explicitly
    }
}
