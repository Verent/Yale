//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Globalization;
//using Yale.Expression;
//using Yale.Expression.Flee;

//namespace Yale.Test.ExpressionTests
//{
//    [TestClass]
//    public class ExpressionBuildingTest
//    {
//        [TestMethod]
//        public void ExpressionsAsVariables()
//        {
//            var context = new ExpressionContext();
//            context.Imports.AddType(typeof(Math));
//            context.Variables.Add("a", 3.14);
//            var e1 = context.CompileDynamic("cos(a) ^ 2");

//            context = new ExpressionContext();
//            context.Imports.AddType(typeof(Math));
//            context.Variables.Add("a", 3.14);

//            var e2 = context.CompileDynamic("sin(a) ^ 2");

//            // Use the two expressions as variables in another expression
//            context = new ExpressionContext();
//            context.Variables.Add("a", e1);
//            context.Variables.Add("b", e2);
//            var e = context.CompileDynamic("a + b");

//            Console.WriteLine(e.Evaluate());
//        }

//        [TestMethod]
//        public void NullCheck()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", "stringObject");
//            var e1 = context.CompileDynamic("a = null");

//            Assert.IsFalse((bool)e1.Evaluate());
//        }

//        [TestMethod]
//        public void NullIsNullCheck()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", "stringObject");
//            var e1 = context.CompileDynamic("null = null");

//            Assert.IsTrue((bool)e1.Evaluate());
//        }

//        [TestMethod]
//        public void IfTestTrue()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", 1);
//            var e1 = context.CompileDynamic("If(a < 100; true; false)");

//            Assert.IsTrue((bool)e1.Evaluate());
//        }

//        [TestMethod]
//        public void IfTestFalse()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", 1);
//            var e1 = context.CompileDynamic("if(a > 100; true; false)");

//            Assert.IsFalse((bool)e1.Evaluate());
//        }

//        [TestMethod]
//        public void CastToInt()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", 1);
//            var e1 = context.CompileDynamic("cast(100.25; int)");

//            Assert.AreEqual(100, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void PowerInt()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", 1);
//            var e1 = context.CompileDynamic("10^2");

//            Assert.AreEqual(100, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void PowerFloat()
//        {
//            var context = new ExpressionContext();
//            var e1 = context.CompileDynamic("4.0^2");

//            Assert.AreEqual(16, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void PowerFloatVariable()
//        {
//            var context = new ExpressionContext();
//            context.Variables.Add("a", 4.0);
//            var e1 = context.CompileDynamic("a^2");

//            Assert.AreEqual(16, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void Negation()
//        {
//            var context = new ExpressionContext();
//            var e1 = context.CompileDynamic("-6 + 10");

//            Assert.AreEqual(4, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void StringConcatenation()
//        {
//            var context = new ExpressionContext();
//            var e1 = context.CompileDynamic("\"abc\" + \"def\"");

//            Assert.AreEqual("abcdef", (string)e1.Evaluate());
//        }

//        [TestMethod]
//        public void BitShift()
//        {
//            var context = new ExpressionContext();
//            var e1 = context.CompileDynamic("0x80 >> 2");

//            Assert.AreEqual(32, (int)e1.Evaluate());
//        }

//        [TestMethod]
//        public void ComparisonEq()
//        {
//            var context = new ExpressionContext();
//            var e1 = context.CompileDynamic("2 = 2");
//            Assert.AreEqual(true, (bool)e1.Evaluate());

//            e1 = context.CompileDynamic("2 eq 2");
//            Assert.AreEqual(true, (bool)e1.Evaluate());
//        }

//        [TestMethod]
//        public void IntegerLiterals()
//        {
//            var context = new ExpressionContext();
//            //Unsigned 32/64 bit
//            var e1 = context.CompileDynamic("100U + 100LU");
//            Assert.AreEqual((UInt64)200, (UInt64)e1.Evaluate());
//            //Signed 32/64 bit
//            e1 = context.CompileDynamic("100 + 100L");
//            Assert.AreEqual(200, (Int64)e1.Evaluate());
//        }

//        [TestMethod]
//        public void RealLiteral()
//        {
//            var context = new ExpressionContext();
//            //Double and single
//            var e1 = context.CompileDynamic("100.25 + 100.25f");
//            Assert.AreEqual(200.5, (double)e1.Evaluate());
//        }
//    }
//}