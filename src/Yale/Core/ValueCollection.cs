using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Yale.Core.Interface;

namespace Yale.Core
{
    internal sealed class ValueCollection : INotifyPropertyChanged
    {
        private readonly IDictionary<string, IValue> _values = new Dictionary<string, IValue>();

        public void Clear()
        {
            _values.Clear();
        }

        public int Count => _values.Keys.Count;
        public bool IsReadOnly => false;

        // ReSharper disable once MethodNameNotMeaningful
        public void Add(string key, object value)
        {
            _values.Add(key, new Value(value));
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _values.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            var success = _values.TryGetValue(key, out var result);
            value = result?.ValueAsObject;
            return success;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var success = _values.TryGetValue(key, out var result);
            value = (T)result;
            return success;
        }

        public object this[string key]
        {
            get => _values[key].ValueAsObject;
            set
            {
                _values[key] = new Value(value);
                if (value is INotifyPropertyChanged nValue)
                {
                    nValue.PropertyChanged += (sender, args) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key)); };
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            }
        }

        public ICollection<string> Keys => _values.Keys;
        public ICollection<object> Values => _values.Values.Select(v => v.ValueAsObject).ToList();

        /// <summary>
        /// This is used to crate a method call that can retrieve a value from the value collection
        /// </summary>
        /// <param name="variableType">Return value</param>
        /// <returns></returns>
        internal static MethodInfo GetVariableLoadMethod(Type variableType)
        {
            var methodInfo = typeof(ValueCollection).GetMethod("GetVariableValueInternal", BindingFlags.Public | BindingFlags.Instance);
            // ReSharper disable once PossibleNullReferenceException
            return methodInfo.MakeGenericMethod(variableType);
        }

        public T GetVariableValueInternal<T>(string name)
        {
            return (T)_values[name].ValueAsObject;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}