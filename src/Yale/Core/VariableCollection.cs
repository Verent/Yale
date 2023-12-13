using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Yale.Core.Interfaces;

namespace Yale.Core;

public sealed class VariableCollection
    : INotifyPropertyChanged,
        IEnumerable<KeyValuePair<string, object>>
{
    private readonly IDictionary<string, IVariable> _values = new Dictionary<string, IVariable>();

    public void Clear() => _values.Clear();

    public int Count => _values.Keys.Count;

    /// <summary>
    /// Adds a value to this compute instance. This can be referenced in expressions.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, object value) => _values.Add(key, new Variable(value));

    /// <summary>
    /// Returns the current value registered to a variable in this instance.
    /// </summary>
    /// <param name="key"></param>
    ///
    public object Get(string key) => _values[key].ValueAsObject;

    /// <summary>
    /// Returns the current value registered to a variable in this instance.
    /// </summary>
    /// <param name="key"></param>
    public T Get<T>(string key) => (T)Get(key);

    public bool ContainsKey(string key) => _values.ContainsKey(key);

    public bool Remove(string key) => _values.Remove(key);

    public bool TryGetValue(string key, out object? value)
    {
        bool success = _values.TryGetValue(key, out IVariable? result);
        value = result?.ValueAsObject;
        return success;
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        bool success = _values.TryGetValue(key, out IVariable? result);
        value = (T)result;
        return success;
    }

    public object this[string key]
    {
        get => _values[key].ValueAsObject;
        set
        {
            if (_values.ContainsKey(key) && _values[key].Equals(value))
                return;

            _values[key] = new Variable(value);
            if (value is INotifyPropertyChanged nValue)
            {
                nValue.PropertyChanged += (sender, args) =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
                };
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
        MethodInfo methodInfo = typeof(VariableCollection).GetMethod(
            "GetVariableValueInternal",
            BindingFlags.Public | BindingFlags.Instance
        );
        // ReSharper disable once PossibleNullReferenceException
        return methodInfo.MakeGenericMethod(variableType);
    }

    public T GetVariableValueInternal<T>(string name) => (T)_values[name].ValueAsObject;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        new VariableEnumerator(_values);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public event PropertyChangedEventHandler? PropertyChanged;
}
