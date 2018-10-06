using Yale.Expression;

namespace Yale.Engine
{
    internal interface IComputeInstance
    {
        void SetValue(string key, object value);

        object GetValue(string key);

        T GetValue<T>(string key);

        int ValueCount { get; }

        void AddExpression(string key, string expression);

        bool ContainsExpression(string key);

        string GetExpression<T>(string key);

        int ExpressionCount { get; }

        object GetResult(string key);

        T GetResult<T>(string key);

        void Clear();
    }
}