//using System;
//using Yale.Expression;
//using Yale.Expression.Flee;

//namespace Yale.Test.ExpressionTests
//{
//    public class Core
//    {
//        protected IDynamicExpression CreateDynamicExpression(string expression, ExpressionContext context)
//        {
//            return context.CompileDynamic(expression);
//        }

//        protected void WriteMessage(string msg, params object[] args)
//        {
//            msg = string.Format(msg, args);
//            Console.WriteLine(msg);
//        }
//    }
//}