using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Yale.Test.Concepts
{
    [TestClass]
    public class Emit
    {
        private const string Key = "Hei";
        private const string Value = "Hello";

        public Emit()
        {
        }

        internal delegate object MyMethod();

        private MyMethod _basicDelegate;

        [TestMethod]
        public void CreateBasicDynamicMethod()
        {
            var value = "Hello world!";
            var dynamicMethod = new DynamicMethod("my_method", typeof(string), null);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldstr, value);
            ilGenerator.Emit(OpCodes.Ret);
            var result = dynamicMethod.Invoke(null, null);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void CreateDynamicMethodThatCallsAStaticMethodNoArguments()
        {
            var dynamicMethod = new DynamicMethod("my_method", typeof(int), null);
            var ilGenerator = dynamicMethod.GetILGenerator();

            var staticMethodInfo = typeof(Emit).GetMethod("StaticMethodNoParamsThatReturnInt", BindingFlags.Public | BindingFlags.Static);
            ilGenerator.Emit(OpCodes.Call, staticMethodInfo);
            ilGenerator.Emit(OpCodes.Ret);
            var result = dynamicMethod.Invoke(null, null);

            Assert.AreEqual(5, result);
        }

        public static int StaticMethodNoParamsThatReturnInt()
        {
            return 5;
        }

        [TestMethod]
        public void CreateDynamicMethodThatCallsAStaticMethod()
        {
            var value = 2;
            var dynamicMethod = new DynamicMethod("my_method", typeof(int), null);
            var ilGenerator = dynamicMethod.GetILGenerator();

            var staticMethodInfo = typeof(Emit).GetMethod("StaticMethodThatReturnInt", BindingFlags.Public | BindingFlags.Static);
            ilGenerator.Emit(OpCodes.Ldc_I4, value);
            ilGenerator.Emit(OpCodes.Call, staticMethodInfo);
            ilGenerator.Emit(OpCodes.Ret);
            var result = dynamicMethod.Invoke(null, null);

            Assert.AreEqual(value * 2, result);
        }

        public static int StaticMethodThatReturnInt(int value)
        {
            return value * value;
        }

        [TestMethod]
        public void CreateDynamicMethodThatCallsAnInstanceMethod()
        {
            var dynamicMethod = new DynamicMethod("my_method", typeof(object),
                new Type[] { typeof(InternalClassForTest) });

            var ilGenerator = dynamicMethod.GetILGenerator();

            var methodInfo = typeof(InternalClassForTest).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(false, methodInfo.IsStatic);

            ilGenerator.Emit(OpCodes.Ldarg_0); //Load instance (InternalClassForTest) that has the method
            ilGenerator.Emit(OpCodes.Ldstr, Key); //Load method arguments
            ilGenerator.Emit(OpCodes.Callvirt, methodInfo); //Call method
            ilGenerator.Emit(OpCodes.Ret); //Return value

            var result = dynamicMethod.Invoke(null, new object[] { new InternalClassForTest() });

            Assert.AreEqual(Value, result);
        }

        public class InternalClassForTest
        {
            public InternalClassForTest()
            {
                _values.Add(Key, Value);
            }

            private Dictionary<string, object> _values = new Dictionary<string, object>();

            public object GetValue(string key)
            {
                return _values[key];
            }

            public T DynamicGetVariableValueInternal<T>(string key)
            {
                return (T)_values[key];
            }
        }

         private DynamicMethod CreateDynamicMethod(Type ownerType)
        {
            Type[] parameterTypes = { };

            return new DynamicMethod("MethodName", typeof(object), parameterTypes, ownerType);
        }
    }
}