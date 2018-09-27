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
    public class ComputeInstance : IEngine
    {
        private readonly ComputeInstanceOptions _options;
        public ExpressionBuilder Builder { get; }
        private readonly DependencyManager _dependencies = new DependencyManager();

        private ValueCollection Values => Builder.Values;

        /// <summary>
        /// Map of name to node
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

        private void BindToValuesEvents()
        {
            if (_options.AutoRecalculate)
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
            throw new NotImplementedException();
        }

        private void RecalculateValues(object sender, PropertyChangedEventArgs e)
        {
            RecalculateDependents(e.PropertyName);
        }

        private void RecalculateDependents(string key)
        {
            foreach (var dependent in _dependencies.GetDependents(key))
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
                RecalculateDependents(dependent);
            }
        }


        /// <summary>
        /// Adds a value to that engine that is not to be parsed.
        /// Any expressions will be stored as strings.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Values[key] = value;
        }

        /// <summary>
        /// Returns the current value registered to a key in this instance.
        /// </summary>
        /// <param name="key"></param>
        public object GetValue(string key)
        {
            return Values[key];
        }

        /// <summary>
        /// Returns the current value registered to a key in this instance.
        /// </summary>
        /// <param name="key"></param>
        public T GetValue<T>(string key)
        {
            return (T)GetValue(key);
        }

        /// <summary>
        /// Return the number of values added to this compute instance.
        /// </summary>
        public int ValueCount => Values.Count;

        /// <summary>
        /// Add an expression that follows the Flee syntax
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expression"></param>
        public void AddExpression(string key, string expression)
        {
            var result = Builder.BuildExpression<object>(key, expression, null);
            _nameNodeMap.Add(key, new ExpressionResult<object>(key, result));
        }

        /// <summary>
        /// Add an expression that follows the Flee syntax
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expression"></param>
        public void AddExpression<T>(string key, string expression)
        {
            var result = Builder.BuildExpression<T>(key, expression, null);
            _nameNodeMap.Add(key, new ExpressionResult<T>(key, result));
        }

        public object GetResult(string key)
        {
            return _nameNodeMap[key].ResultAsObject;
        }

        public T GetResult<T>(string key)
        {
            return (T) _nameNodeMap[key].ResultAsObject;
        }

        public Type ExpressionType(string expressionKey)
        {
            return _nameNodeMap[expressionKey].ResultType;
        }

        public Expression<T> GetExpression<T>(string key)
        {
            var result = (ExpressionResult<T>)_nameNodeMap[key];
            return result.Expression;
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

        internal void AddDependency(string expressionKey, string dependsOn)
        {
            if(ContainsExpression(dependsOn) == false && 
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
            ilGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());

            //Find and load expression result
            var members = typeof(ComputeInstance).FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, Type.FilterNameIgnoreCase, "GetResult");
            var methodInfo = members.Cast<MethodInfo>().First(method => method.IsGenericMethod);
            var resultType = ExpressionType(expressionKey);
            methodInfo = methodInfo.MakeGenericMethod(resultType);

            ilGenerator.Emit(OpCodes.Ldstr, expressionKey);
            ilGenerator.Emit(OpCodes.Call, methodInfo);
        }
    }
}