using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Yale.Core.Interface;

namespace Yale.Core
{
    public sealed class ValueCollection : IDictionary<string, object>
    {
        private readonly IDictionary<string, IValue> _values = new Dictionary<string, IValue>();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            //Todo: Implement
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _values.Add(new KeyValuePair<string, IValue>(item.Key, new Value(item.Value)));
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public int Count => _values.Count;
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

        public bool TryGetValue(string key, out IValue value)
        {
            var success = _values.TryGetValue(key, out var result);
            value = result;
            return success;
        }

        public object this[string key]
        {
            get => _values[key].ValueAsObject;
            set => _values[key] = new Value(value);
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
            return (T) _values[name].ValueAsObject;
        }
    }
}