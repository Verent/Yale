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
        public ExpressionBuilderOptions Options { get; private set; }
        internal ComputeInstance ComputeInstance { get; set; }

        private ExpressionParser Parser { get; set; }
        private YaleExpressionAnalyzer Analyzer { get; set; }

        private const string DynamicMethodName = "DynamicMethod";

        internal VariableCollection Variables { get; } = new VariableCollection();

        public ImportCollection Imports { get; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ExpressionBuilder()
        {
            Options = new ExpressionBuilderOptions();
            Imports = new ImportCollection(Options);
            CreateParser();
        }

        public ExpressionBuilder(ExpressionBuilderOptions builderOptions)
        {
            Options = builderOptions;
            Imports = new ImportCollection(Options);
            CreateParser();
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        private void CreateParser()
        {
            Analyzer = new YaleExpressionAnalyzer();
            Parser = new ExpressionParser(TextReader.Null, Analyzer);
        }

        internal Expression<T> BuildExpression<T>(string expressionName, string expression)
        {
            object owner = DefaultExpressionOwner.Instance;
            Type ownerType = DefaultExpressionOwner.Type;

            Imports.ImportOwner(ownerType);

            ExpressionContext context = new ExpressionContext(Options, expressionName, owner)
            {
                Variables = Variables,
                Imports = Imports,
                ComputeInstance = ComputeInstance,
            };

            BaseExpressionElement topElement = Parse(expression, context);

            RootExpressionElement rootElement = new RootExpressionElement(topElement, typeof(T));
            DynamicMethod dynamicMethod = CreateDynamicMethod<T>(ownerType);

            YaleIlGenerator ilGenerator = new YaleIlGenerator(dynamicMethod.GetILGenerator());
            rootElement.Emit(ilGenerator, context);

#if DEBUG
            ilGenerator.ValidateLength();
#endif

            Type delegateType = typeof(ExpressionEvaluator<>).MakeGenericType(typeof(T));
            ExpressionEvaluator<T> evaluator = (ExpressionEvaluator<T>)dynamicMethod.CreateDelegate(delegateType);

            return new Expression<T>(expression, evaluator, context);
        }

        private BaseExpressionElement Parse(string expression, ExpressionContext context)
        {
            StringReader stringReader = new StringReader(expression);

            Parser.Reset(stringReader);
            YaleExpressionAnalyzer analyzer = (YaleExpressionAnalyzer)Parser.Analyzer;

            analyzer.SetContext(context);
            Node rootNode = Parse();
            analyzer.Reset();

            BaseExpressionElement topElement = (BaseExpressionElement)rootNode.Values[0];
            return topElement;
        }

        private static DynamicMethod CreateDynamicMethod<T>(Type ownerType)
        {
            Type[] parameterTypes = {
                typeof(object),
                typeof(ExpressionContext),
                typeof(VariableCollection)
            };
            return new DynamicMethod(DynamicMethodName, typeof(T), parameterTypes, ownerType);
        }

        private Node Parse()
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