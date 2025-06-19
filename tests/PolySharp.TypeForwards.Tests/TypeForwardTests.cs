using System;
using System.Linq;
using System.Reflection;
#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PolySharp.TypeForwards.Tests;

[TestClass]
public class TypeForwardTests
{
    [TestMethod]
    public void Index_IsForwarded()
    {
#if NET8_0_OR_GREATER 
        Assert.AreEqual(typeof(object).Assembly, typeof(Index).Assembly);
        Assert.AreEqual(typeof(Index).Assembly.GetName().Name!, typeof(TypeForwardTests).Assembly.GetType("System.Index")!.Assembly.GetName().Name);
#else
        Assert.AreEqual("PolySharp.TypeForwards.Tests", typeof(Index).Assembly.GetName().Name);
#endif
    }

    [TestMethod]
    public void Range_IsForwarded()
    {
#if NET8_0_OR_GREATER 
        Assert.AreEqual(typeof(object).Assembly, typeof(Range).Assembly);
        Assert.AreEqual(typeof(Range).Assembly.GetName().Name!, typeof(TypeForwardTests).Assembly.GetType("System.Range")!.Assembly.GetName().Name);
#else
        Assert.AreEqual("PolySharp.TypeForwards.Tests", typeof(Range).Assembly.GetName().Name);
#endif
    }

    [TestMethod]
    public void IsExternalInit_IsForwarded()
    {
        Type personType = typeof(Person);
        MethodInfo nameSetterMethod = personType.GetMethod("set_Name", BindingFlags.Public | BindingFlags.Instance)!;
        Type[] returnOptionalModifiers = nameSetterMethod.ReturnParameter.GetOptionalCustomModifiers();
        Type[] returnRequiredModifiers = nameSetterMethod.ReturnParameter.GetRequiredCustomModifiers();

        CollectionAssert.AreEquivalent(Array.Empty<Type>(), returnOptionalModifiers); // There are no modopt-s
        Assert.AreEqual(1, returnRequiredModifiers.Length); // There is a single modreq
        Assert.AreEqual("System.Runtime.CompilerServices.IsExternalInit", returnRequiredModifiers[0].FullName); // The modreq is IsExternalInit

#if NET8_0_OR_GREATER
        Assert.AreEqual(typeof(object).Assembly, typeof(IsExternalInit).Assembly); // The IsExternalInit should be the one from the BCL

        string isExternalInitAssemblyName = typeof(IsExternalInit).Assembly.GetName().Name!;

        // Verify the type has been forwarded correctly
        Assert.AreEqual(isExternalInitAssemblyName, returnRequiredModifiers[0].Assembly.GetName().Name); // Check the one we have on our type is the one we expect
        Assert.AreEqual(isExternalInitAssemblyName, personType.Assembly.GetType("System.Runtime.CompilerServices.IsExternalInit")!.Assembly.GetName().Name); // Check that type forwarding is working as expected
#else
        // If IsExternalInit is not available, it should be polyfilled in this project
        Assert.AreEqual("PolySharp.TypeForwards.Tests", returnRequiredModifiers[0].Assembly.GetName().Name);
#endif
    }

    [TestMethod]
    public void RequiresLocationAttribute_IsForwarded()
    {
        MethodInfo method = typeof(TypeForwardTests).GetMethod(nameof(MethodWithRefReadonlyParameter), BindingFlags.Static | BindingFlags.NonPublic)!;
        ParameterInfo parameter = method.GetParameters()[0];
        CustomAttributeData attribute = parameter.CustomAttributes.Last();

        Assert.AreEqual("System.Runtime.CompilerServices.RequiresLocationAttribute", attribute.AttributeType.FullName);

#if NET8_0_OR_GREATER
        Assert.AreEqual(typeof(object).Assembly, typeof(RequiresLocationAttribute).Assembly);

        string requiresLocationAttributeAssemblyName = typeof(RequiresLocationAttribute).Assembly.GetName().Name!;

        // Verify the type has been forwarded correctly
        Assert.AreEqual(requiresLocationAttributeAssemblyName, attribute.AttributeType.Assembly.GetName().Name);
        Assert.AreEqual(requiresLocationAttributeAssemblyName, typeof(TypeForwardTests).Assembly.GetType("System.Runtime.CompilerServices.RequiresLocationAttribute")!.Assembly.GetName().Name);
#else
        // If RequiresLocationAttribute is not available, it should be polyfilled in this project
        Assert.AreEqual("PolySharp.TypeForwards.Tests", attribute.AttributeType.Assembly.GetName().Name);
#endif
    }

    private sealed record Person(string Name);

    private static void MethodWithRefReadonlyParameter(ref readonly int x)
    {
    }
}
