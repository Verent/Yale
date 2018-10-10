using PerCederberg.Grammatica.Runtime;
using System;
using System.IO;
using System.Reflection.Emit;
using Yale.Core;
using Yale.Engine;
using Yale.Expression.Elements;
using Yale.Expression.Elements.Base;
using Yale.Parser;
using Yale.Parser.Internal;

namespace Yale.Expression
{
    public class ExpressionBuilder
    {
        private readonly ExpressionBuilderOptions _builderOptions;
        private static readonly object SyncRoot = new object();
        internal ComputeInstance ComputeInstance { get; set; }

        private ExpressionParser Parser { get; set; }
        private YaleExpressionAnalyzer Analyzer { get; set; }

        private string DynamicMethodName { get; } = "DynamicMethod";

        internal ValueCollection Values { get; } = new ValueCollection();

        public TypeImports Imports { get; }

        public ExpressionBuilder()
        {
            _builderOptions = new ExpressionBuilderOptions();
            Imports = new TypeImports(_builderOptions);
            CreateParser();
        }

        public ExpressionBuilder(ExpressionBuilderOptions builderOptions)
        {
            _builderOptions = builderOptions;
            Imports = new TypeImports(_builderOptions);
            CreateParser();
        }

        private void CreateParser()
        {
            lock (SyncRoot)
            {
                Analyzer = new YaleExpressionAnalyzer();
                Parser = new ExpressionParser(TextReader.Null, Analyzer);
            }
        }

        internal Expression<T> BuildExpression<T>(string expressionName, string expression)
        {
            var owner = DefaultExpressionOwner.Instance;
            var ownerType = DefaultExpressionOwner.Type;

            Imports.ImportOwner(ownerType);

            var context = new ExpressionContext(_builderOptions, expressionName, owner)
            {
                Values = Values,
                Imports = Imports,
                ComputeInstance = ComputeInstance,
            };

            var topElement = Parse(expression, context);

            var rootElement = new RootExpressionElement(topElement, typeof(T));
            var dynamicMethod = CreateDynamicMethod<T>(ownerType);

            var ilGenerator = new YaleIlGenerator(dynamicMethod.GetILGenerator());
            rootElement.Emit(ilGenerator, context);
            ilGenerator.ValidateLength();

            var delegateType = typeof(ExpressionEvaluator<>).MakeGenericType(typeof(T));
            var evaluator = (ExpressionEvaluator<T>)dynamicMethod.CreateDelegate(delegateType);

            return new Expression<T>(expression, evaluator, context);
        }

        private ExpressionElement Parse(string expression, ExpressionContext context)
        {
            lock (SyncRoot)
            {
                var stringReader = new StringReader(expression);

                Parser.Reset(stringReader);
                var analyzer = (YaleExpressionAnalyzer)Parser.Analyzer;

                analyzer.SetContext(context);
                var rootNode = Parse();
                analyzer.Reset();

                var topElement = (ExpressionElement)rootNode.Values[0];
                return topElement;
            }
        }

        private DynamicMethod CreateDynamicMethod<T>(Type ownerType)
        {
            Type[] parameterTypes = {
                typeof(object),
                typeof(ExpressionContext),
                typeof(ValueCollection)
            };
            return new DynamicMethod(DynamicMethodName, typeof(T), parameterTypes, ownerType);
        }

        internal Node Parse()
        {
            try
            {
                return Parser.Parse();
            }
            catch (ParserLogException ex)
            {
                throw new ExpressionCompileException(ex);
            }
        }
    }
}