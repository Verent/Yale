using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Engine;

namespace Yale.Test.ExpressionTests
{
    [TestClass]
    public class ExpressionBuildingTest
    {
        private readonly ComputeInstance _instance = new ComputeInstance();


        //[TestMethod]
        //public void BitShift()
        //{
        //    var context = new ExpressionContext();
        //    var e1 = context.CompileDynamic("0x80 >> 2");

        //    Assert.AreEqual(32, (int)e1.Evaluate());
        //}

        //[TestMethod]
        //public void ComparisonEq()
        //{
        //    var context = new ExpressionContext();
        //    var e1 = context.CompileDynamic("2 = 2");
        //    Assert.AreEqual(true, (bool)e1.Evaluate());

        //    e1 = context.CompileDynamic("2 eq 2");
        //    Assert.AreEqual(true, (bool)e1.Evaluate());
        //}

        //[TestMethod]
        //public void IntegerLiterals()
        //{
        //    var context = new ExpressionContext();
        //    //Unsigned 32/64 bit
        //    var e1 = context.CompileDynamic("100U + 100LU");
        //    Assert.AreEqual((UInt64)200, (UInt64)e1.Evaluate());
        //    //Signed 32/64 bit
        //    e1 = context.CompileDynamic("100 + 100L");
        //    Assert.AreEqual(200, (Int64)e1.Evaluate());
        //}

        //[TestMethod]
        //public void RealLiteral()
        //{
        //    var context = new ExpressionContext();
        //    //Double and single
        //    var e1 = context.CompileDynamic("100.25 + 100.25f");
        //    Assert.AreEqual(200.5, (double)e1.Evaluate());
        //}
    }
}