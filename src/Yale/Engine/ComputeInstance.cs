using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Core;
using Yale.Engine.Interface;
using Yale.Engine.Internal;
using Yale.Expression;
using Yale.Parser.Internal;

namespace Yale.Engine
{
    public class ComputeInstance : IComputeInstance
    {
        private readonly ComputeInstanceOptions _options;
        public ExpressionBuilder Builder { get; }
        private readonly DependencyManager _dependencies = new DependencyManager();

        /// <summary>
        /// Variables available in expressions
        /// </summary>
        private ValueCollection Values => Builder.Values;

        /// <summary>
        /// Expression results
        /// </summary>
        private readonly Dictionary<string, IExpressionResult> _nameNodeMap = new Dictionary<string, IExpressionResult>();

        public ComputeInstance()
        {
            Builder = new ExpressionBuilder
            {
                ComputeInstance = this
            };

            _options = ComputeInstanceOptions.Default;
            _nameNodeMap = new Dictionary<string, IExpressionResult>(StringComparer.OrdinalIgnoreCase);

            BindToValuesEvents();
        }

        public ComputeInstance(ComputeInstanceOptions options)
        {
            Builder = new ExpressionBuilder
            {
                ComputeInstance = this
            };
            _options = options ?? throw new ArgumentNullException(nameof(options));

            BindToValuesEvents();
        }

        #region Recalculate

        private void BindToValuesEvents()
        {
            if (_options.AutoRecalculate && _options.LazyRecalculate == false)
            {
                Values.PropertyChanged += RecalculateValues;
            }
            else
            {
                Values.PropertyChanged += TagResultsAsDirty;
            }
        }

        private void TagResultsAsDirty(object sender, PropertyChangedEventArgs e)
        {
            foreach (var dependent in _dependencies.GetDependents(e.PropertyName))
            {
                TagNodeAndDependentsAsDirty(dependent);
            }
        }

        private void TagNodeAndDependentsAsDirty(string key)
        {
            var node = _nameNodeMap[key];
            node.Dirty = true;

            foreach (var dependent in _dependencies.GetDependents(key))
            {
                RecalculateNodeAndDependents(dependent);
            }
        }

        private void RecalculateValues(object sender, PropertyChangedEventArgs e)
        {
            foreach (var dependent in _dependencies.GetDependents(e.PropertyName))
            {
                RecalculateNodeAndDependents(dependent);
            }
        }

        private void RecalculateNodeAndDependents(string key)
        {
            var node = _nameNodeMap[key];
            node.Recalculate();

            foreach (var dependent in _dependencies.GetDependents(key))
            {
                RecalculateNodeAndDependents(dependent);
            }
        }

        private void RecalculateIfNeeded(string key)
        {
            if (_nameNodeMap.TryGetValue(key, out var node) && node.Dirty)
            {
                foreach (var dependent in _dependencies.GetDirectPrecedents(key))
                {
                    RecalculateIfNeeded(dependent);
                }
                node.Recalculate();
            }
        }

        #endregion Recalculate

        /// <summary>
        /// Adds a value to this compute instance. This can be referenced in expressions.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Values[key] = value;
        }

        /// <summary>
        /// Returns the current value registered to a variable in this instance.
        /// </summary>
        /// <param name="key"></param>
        public object GetValue(string key)
        {
            return Values[key];
        }

        /// <summary>
        /// Returns the current value registered to a variable in this instance.
        /// </summary>
        /// <param name="key"></param>
        public T GetValue<T>(string key)
        {
            return (T)GetValue(key);
        }

        public int ValueCount => Values.Count;

        /// <summary>
        /// Add an expression that follows the Flee syntax
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expression"></param>
        public void AddExpression(string key, string expression)
        {
            var result = Builder.BuildExpression<object>(key, expression);
            _nameNodeMap.Add(key, new ExpressionResult<object>(key, result));
        }

        /// <summary>
        /// Add an expression
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expression"></param>
        public void AddExpression<T>(string key, string expression)
        {
            var result = Builder.BuildExpression<T>(key, expression);
            _nameNodeMap.Add(key, new ExpressionResult<T>(key, result));
        }

        /// <summary>
        /// Get expression result
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetResult(string key)
        {
            if (_options.AutoRecalculate)
            {
                RecalculateIfNeeded(key);
            }
            return _nameNodeMap[key].ResultAsObject;
        }

        /// <summary>
        /// Get expression result
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetResult<T>(string key)
        {
            return (T)GetResult(key);
        }

        /// <summary>
        /// Get the object type of an expression result
        /// </summary>
        /// <param name="expressionKey"></param>
        /// <returns></returns>
        public Type ResultType(string expressionKey)
        {
            return _nameNodeMap[expressionKey].ResultType;
        }

        public string GetExpression<T>(string key)
        {
            var result = (ExpressionResult<T>)_nameNodeMap[key];
            return result.Expression.ExpressionText;
        }

        /// <summary>
        /// Removes all expressions and values
        /// </summary>
        public void Clear()
        {
            _nameNodeMap.Clear();
            _dependencies.Clear();
            Values.Clear();
        }

        public bool ContainsExpression(string key) => _nameNodeMap.ContainsKey(key);

        public int ExpressionCount => _nameNodeMap.Count;

        public string DependencyGraph => _dependencies.DependencyGraph;

        #region Dependencies

        internal void AddDependency(string expressionKey, string dependsOn)
        {
            if (ContainsExpression(dependsOn) == false &&
                Values.ContainsKey(dependsOn) == false) throw new InvalidOperationException("Can not depend an an expression that is not added to the instance");

            _dependencies.AddDependency(expressionKey, dependsOn);
        }

        /// <summary>
        /// Create the IL used to load the result from another expression
        /// </summary>
        /// <param name="expressionKey"></param>
        /// <param name="ilGenerator"></param>
        internal void EmitLoad(string expressionKey, YaleIlGenerator ilGenerator)
        {
            var propertyInfo = typeof(ExpressionContext).GetProperty("ComputeInstance");
            // ReSharper disable once PossibleNullReferenceException
            ilGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());

            //Find and load expression result
            var members = typeof(ComputeInstance).FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, Type.FilterNameIgnoreCase, "GetResult");
            var methodInfo = members.Cast<MethodInfo>().First(method => method.IsGenericMethod);
            var resultType = ResultType(expressionKey);
            methodInfo = methodInfo.MakeGenericMethod(resultType);

            ilGenerator.Emit(OpCodes.Ldstr, expressionKey);
            ilGenerator.Emit(OpCodes.Call, methodInfo);
        }

        #endregion Dependencies
    }
}