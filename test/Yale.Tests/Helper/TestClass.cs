﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Yale.Tests.Helper;

internal class TestClass<T> : INotifyPropertyChanged
{
    private readonly string _caller;
    private T _value;

    public TestClass(string caller) => _caller = caller;

    public string GetCaller() => _caller;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
