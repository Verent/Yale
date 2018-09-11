namespace Yale.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ////Sample Scenario 1
            //var context = new ExpressionContext();
            //var variables = context.Variables;
            //variables.Add("a", 1);
            //variables.Add("b", 1);

            //var e = context.CompileGeneric<bool>("a=1 AND b=0");
            //var result = e.Evaluate();

            ////Sample Scenario 2
            //var context2 = new ExpressionContext();
            //var variables2 = context2.Variables;
            //variables2.Add("a", 100);
            //variables2.Add("b", 1);
            //variables2.Add("c", 24);

            //var ge = context2.CompileGeneric<bool>("(a = 100 OR b > 0) AND c <> 2");
            //var result2 = ge.Evaluate();

            System.Console.ReadKey();
        }
    }
}