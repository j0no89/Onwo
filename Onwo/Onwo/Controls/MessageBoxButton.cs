using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using Onwo.Input;

namespace Onwo.Controls
{
    public class MessageBoxButton:INotifyPropertyChanged
    {
        private string _text;
        private ICommand _action;

        public String Text
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged();
            }
        }

        public ICommand Action
        {
            get { return _action; }
            set
            {
                if (Equals(value, _action)) return;
                _action = value;
                OnPropertyChanged();
            }
        }

        public MessageBoxButton(String text,Action<object> action):this(text,new DelegateCommand(action)) { }

        public MessageBoxButton(String text, ICommand action)
        {
            Text = text;
            Action = action;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
