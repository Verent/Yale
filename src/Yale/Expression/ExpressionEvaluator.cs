using Yale.Core;

namespace Yale.Expression
{
    internal delegate T ExpressionEvaluator<out T>(
        object owner,
        ExpressionContext context,
        VariableCollection variables
    );
}
