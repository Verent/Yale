using System.ComponentModel;
using System.Runtime.CompilerServices;
using Yale.Tests.Annotations;

namespace Yale.Tests.Helper
{
    internal class TestClass : INotifyPropertyChanged
    {
        private readonly string _caller;
        private string _value;

        public TestClass(string caller)
        {
            _caller = caller;
        }

        public string GetCaller()
        {
            return _caller;
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}