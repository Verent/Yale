using Yale.Core.Interfaces;

namespace Yale.Core;

internal sealed class VariableEnumerator : IEnumerator<KeyValuePair<string, object>>
{
    private readonly IEnumerator<KeyValuePair<string, IVariable>> enumerator;

    public VariableEnumerator(IDictionary<string, IVariable> values) =>
        enumerator = values.GetEnumerator();

    public KeyValuePair<string, object> Current =>
        new(enumerator.Current.Key, enumerator.Current.Value.ValueAsObject);

    object IEnumerator.Current => Current;

    public bool MoveNext() => enumerator.MoveNext();

    public void Reset() => enumerator.Reset();

    public void Dispose() => enumerator.Dispose();
}
