﻿using System;
using NUnit.Framework;
using Yale.Engines;
using Yale.Expression;

namespace Flee.Test.CalcEngineTests
{
    [TestFixture]
    public class SimpleCalcEngineTests
    {
        private SimpleCalcEngine _myEngine;
        public SimpleCalcEngineTests()
        {
            var engine = new SimpleCalcEngine();
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Imports.AddType(typeof(Math), "math");
            engine.Context = context;
            _myEngine = engine;
        }

        [Test]
        public void TestScripts()
        {
            
        }
    }
}
