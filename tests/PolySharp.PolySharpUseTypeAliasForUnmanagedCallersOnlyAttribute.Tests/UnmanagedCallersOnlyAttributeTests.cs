namespace PolySharp.PolySharpUseTypeAliasForUnmanagedCallersOnlyAttribute.Tests;

internal class UnmanagedCallersOnlyAttributeTests
{
    [global::System.Runtime.InteropServices2.UnmanagedCallersOnly]
    public static void NativeFunction()
    {
    }
}
