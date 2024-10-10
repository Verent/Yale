using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Yale.Core;
using Yale.Engine.Interface;
using Yale.Engine.Internal;
using Yale.Expression;
using Yale.Parser.Internal;

namespace Yale.Engine;

public class ComputeInstance
{
    private readonly ComputeInstanceOptions options;
    internal ExpressionBuilder Builder { get; }

    public ImportCollection Imports => Builder.Imports;

    private readonly DependencyManager dependencies = new();

    /// <summary>
    /// Variables available in expressions
    /// </summary>
    public VariableCollection Variables => Builder.Variables;

    public IEnumerable<string> ExpressionKeys => nameNodeMap.Keys;

    /// <summary>
    /// Expression results
    /// </summary>
    private readonly Dictionary<string, IExpressionResult> nameNodeMap = new();

    public ComputeInstance()
    {
        Builder = new ExpressionBuilder(instance: this);

        options = ComputeInstanceOptions.Default;
        nameNodeMap = new Dictionary<string, IExpressionResult>();

        BindToValuesEvents();
    }

    public ComputeInstance(ComputeInstanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Builder = new ExpressionBuilder(options: options.ExpressionOptions, instance: this);
        this.options = options;
        nameNodeMap = new Dictionary<string, IExpressionResult>(
            options.ExpressionOptions.StringComparer
        );

        BindToValuesEvents();
    }

    #region Recalculate

    private void BindToValuesEvents()
    {
        if (options.Recalculate == ComputeInstanceOptions.RecalculateMode.Auto)
        {
            Variables.PropertyChanged += RecalculateValues;
        }
        else if (options.Recalculate == ComputeInstanceOptions.RecalculateMode.Lazy)
        {
            Variables.PropertyChanged += TagResultsAsDirty;
        }
    }

    private void TagResultsAsDirty(object? sender, PropertyChangedEventArgs e)
    {
        foreach (var dependent in dependencies.GetDirectDependents(e.PropertyName!))
        {
            TagNodeAndDependentsAsDirty(dependent);
        }
    }

    private void TagNodeAndDependentsAsDirty(string key)
    {
        var node = nameNodeMap[key];

        if (node.Dirty)
            return;

        node.Dirty = true;

        foreach (var dependent in dependencies.GetDependents(key))
        {
            TagNodeAndDependentsAsDirty(dependent);
        }
    }

    private void RecalculateValues(object? sender, PropertyChangedEventArgs e)
    {
        foreach (var dependentKey in dependencies.GetDependents(e.PropertyName!))
        {
            RecalculateNodeAndDependents(dependentKey);
        }
    }

    private void RecalculateNodeAndDependents(string key)
    {
        IExpressionResult node = nameNodeMap[key];
        object result = node.ResultAsObject;
        node.Recalculate();

        //No need to recalculate dependents if value is the same.
        if (result.Equals(node.ResultAsObject))
            return;

        foreach (var dependent in dependencies.GetDependents(key))
        {
            RecalculateNodeAndDependents(dependent);
        }
    }

    private void RecalculateIfNeeded(string key)
    {
        if (nameNodeMap.TryGetValue(key, out var node) && node.Dirty)
        {
            foreach (var dependent in dependencies.GetDirectPrecedents(key))
            {
                RecalculateIfNeeded(dependent);
            }
            node.Recalculate();
        }
    }

    #endregion Recalculate

    public void SetExpression(string key, string expression)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(expression);

        SetExpression<object>(key, expression);
    }

    public void SetExpression<T>(string key, string expression)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(expression);

        dependencies.RemovePrecedents(key);
        nameNodeMap.Remove(key);

        AddExpression<T>(key, expression);

        foreach (var dependent in dependencies.GetDependents(key))
        {
            if (options.Recalculate == ComputeInstanceOptions.RecalculateMode.Auto)
            {
                RecalculateNodeAndDependents(dependent);
            }
            if (options.Recalculate == ComputeInstanceOptions.RecalculateMode.Lazy)
            {
                TagNodeAndDependentsAsDirty(dependent);
            }
        }
    }

    /// <summary>
    /// Add an expression that follows the Flee syntax
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expression"></param>
    public void AddExpression(string key, string expression)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(expression);

        var result = Builder.BuildExpression<object>(key, expression);
        nameNodeMap.Add(key, new ExpressionResult<object>(key, result));
    }

    /// <summary>
    /// Add an expression
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expression"></param>
    public void AddExpression<T>(string key, string expression)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(expression);

        var result = Builder.BuildExpression<T>(key, expression);
        nameNodeMap.Add(key, new ExpressionResult<T>(key, result));
    }

    /// <summary>
    /// Get expression result
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetResult(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (options.Recalculate == ComputeInstanceOptions.RecalculateMode.Lazy)
        {
            RecalculateIfNeeded(key);
        }
        return nameNodeMap[key].ResultAsObject;
    }

    /// <summary>
    /// Get expression result
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T GetResult<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return (T)GetResult(key);
    }

    public bool TryGetResult(string key, [NotNullWhen(true)] out object? result)
    {
        ArgumentNullException.ThrowIfNull(key);

        result = nameNodeMap.ContainsKey(key) ? GetResult(key) : default;
        return result != null;
    }

    public bool TryGetResult<T>(string key, [NotNullWhen(true)] out T? result)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        result = nameNodeMap.ContainsKey(key) ? (T)GetResult(key) : default;
        return result != null;
    }

    /// <summary>
    /// Get the object type of an expression result
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Type? ResultType(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return nameNodeMap[key].ResultType;
    }

    public string GetExpression(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var result = (ExpressionResult<object>)nameNodeMap[key];
        return result.Expression.ExpressionText;
    }

    public string GetExpression<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var result = (ExpressionResult<T>)nameNodeMap[key];
        return result.Expression.ExpressionText;
    }

    /// <summary>
    /// Removes all expressions and variables
    /// </summary>
    public void Clear()
    {
        nameNodeMap.Clear();
        dependencies.Clear();
        Variables.Clear();
    }

    public bool ContainsExpression(string key) => nameNodeMap.ContainsKey(key);

    public int ExpressionCount => nameNodeMap.Count;

    public string DependencyGraph => dependencies.DependencyGraph;

    #region Dependencies

    internal void AddDependency(string expressionKey, string dependsOn)
    {
        if (ContainsExpression(dependsOn) == false && Variables.ContainsKey(dependsOn) == false)
        {
            throw new InvalidOperationException(
                $"Dependent expression or variable {dependsOn} does not exist"
            );
        }

        dependencies.AddDependency(expressionKey, dependsOn);
    }

    /// <summary>
    /// Create the IL used to load the result from another expression
    /// </summary>
    /// <param name="expressionKey"></param>
    /// <param name="ilGenerator"></param>
    internal void EmitLoad(string expressionKey, YaleIlGenerator ilGenerator)
    {
        var propertyInfo = typeof(ExpressionContext).GetProperty(
            nameof(ExpressionContext.ComputeInstance)
        );

        ilGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());

        //Find and load expression result
        var members = typeof(ComputeInstance).FindMembers(
            MemberTypes.Method,
            BindingFlags.Instance | BindingFlags.Public,
            Type.FilterName,
            "GetResult"
        );
        var methodInfo = members.Cast<MethodInfo>().First(method => method.IsGenericMethod);
        var resultType = ResultType(expressionKey);
        methodInfo = methodInfo.MakeGenericMethod(resultType);

        ilGenerator.Emit(OpCodes.Ldstr, expressionKey);
        ilGenerator.Emit(OpCodes.Call, methodInfo);
    }

    #endregion Dependencies
}
