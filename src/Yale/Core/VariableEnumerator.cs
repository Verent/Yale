using System.Collections;
using System.Collections.Generic;
using Yale.Core.Interface;

namespace Yale.Core
{
    internal sealed class VariableEnumerator : IEnumerator<KeyValuePair<string, object>>
    {
        private IEnumerator<KeyValuePair<string, IVariable>> _enumerator;

        public VariableEnumerator(IDictionary<string, IVariable> values)
        {
            _enumerator = values.GetEnumerator();
        }

        public KeyValuePair<string, object> Current => new KeyValuePair<string, object>(_enumerator.Current.Key, _enumerator.Current.Value.ValueAsObject);

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {
        }
    }
}