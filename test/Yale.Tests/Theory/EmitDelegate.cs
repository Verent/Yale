﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yale.Tests.Theory;

[TestClass]
public class Emit
{
    private const string Key = "Hei";
    private const string Value = "Hello";

    internal delegate object MyMethod();

    [TestMethod]
    public void CreateBasicDynamicMethod()
    {
        var value = "Hello world!";
        DynamicMethod dynamicMethod = new("my_method", typeof(string), null);
        var ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldstr, value);
        ilGenerator.Emit(OpCodes.Ret);
        var result = dynamicMethod.Invoke(null, null);

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void CreateDynamicMethodThatCallsAStaticMethodNoArguments()
    {
        DynamicMethod dynamicMethod = new("my_method", typeof(int), null);
        var ilGenerator = dynamicMethod.GetILGenerator();

        var staticMethodInfo = typeof(Emit).GetMethod(
            "StaticMethodNoParamsThatReturnInt",
            BindingFlags.Public | BindingFlags.Static
        );
        ilGenerator.Emit(OpCodes.Call, staticMethodInfo);
        ilGenerator.Emit(OpCodes.Ret);
        var result = dynamicMethod.Invoke(null, null);

        Assert.AreEqual(5, result);
    }

    public static int StaticMethodNoParamsThatReturnInt() => 5;

    [TestMethod]
    public void CreateDynamicMethodThatCallsAStaticMethod()
    {
        const int value = 2;
        DynamicMethod dynamicMethod = new("my_method", typeof(int), null);
        var ilGenerator = dynamicMethod.GetILGenerator();

        var staticMethodInfo = typeof(Emit).GetMethod(
            "StaticMethodThatReturnInt",
            BindingFlags.Public | BindingFlags.Static
        );
        ilGenerator.Emit(OpCodes.Ldc_I4, value);
        ilGenerator.Emit(OpCodes.Call, staticMethodInfo ?? throw new InvalidOperationException());
        ilGenerator.Emit(OpCodes.Ret);
        var result = dynamicMethod.Invoke(null, null);

        Assert.AreEqual(value * 2, result);
    }

    public static int StaticMethodThatReturnInt(int value) => value * value;

    [TestMethod]
    public void CreateDynamicMethodThatCallsAnInstanceMethod()
    {
        DynamicMethod dynamicMethod =
            new("my_method", typeof(object), new Type[] { typeof(InternalClassForTest) });

        var ilGenerator = dynamicMethod.GetILGenerator();

        var methodInfo = typeof(InternalClassForTest).GetMethod(
            "GetValue",
            BindingFlags.Public | BindingFlags.Instance
        );
        Assert.AreEqual(false, methodInfo != null && methodInfo.IsStatic);

        ilGenerator.Emit(OpCodes.Ldarg_0); //Load instance (InternalClassForTest) that has the method
        ilGenerator.Emit(OpCodes.Ldstr, Key); //Load method arguments
        ilGenerator.Emit(OpCodes.Callvirt, methodInfo); //Call method
        ilGenerator.Emit(OpCodes.Ret); //Return value

        var result = dynamicMethod.Invoke(null, new object[] { new InternalClassForTest() });

        Assert.AreEqual(Value, result);
    }

    public class InternalClassForTest
    {
        public InternalClassForTest() => _values.Add(Key, Value);

        private readonly Dictionary<string, object> _values = new();

        public object GetValue(string key) => _values[key];

        public T DynamicGetVariableValueInternal<T>(string key) => (T)_values[key];
    }

    private DynamicMethod CreateDynamicMethod(Type ownerType)
    {
        Type[] parameterTypes = { };

        return new DynamicMethod("MethodName", typeof(object), parameterTypes, ownerType);
    }
}
