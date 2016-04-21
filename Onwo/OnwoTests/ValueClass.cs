using System;
using System.ComponentModel;

namespace OnwoTests
{
    public class ValueClass : INotifyPropertyChanged, IComparable<ValueClass>, IComparable
    {
        private string _text;

        public ValueClass() : this("", 0)
        { }
        public ValueClass(int number):this("Text"+number,number)
        {
        }
        public ValueClass(string text, int number)
        {
            this.Text = text;
            this.Number = number;
        }

        public ValueClass(Random rand)
        {
            this.Text = "0x" + rand.Next().ToString("X");
            this.Number = rand.Next();
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (Equals(value, _text)) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                if (Equals(value, _number)) return;
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        public int CompareTo(ValueClass other)
        {
            int compare = this.Number.CompareTo(other.Number); 
            if (compare == 0)
                compare = string.Compare(this.Text, other.Text, StringComparison.InvariantCulture);
            return compare;
        }

        public override string ToString()
        {
            return $"{this.Number}: {this.Text}";
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as ValueClass);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override int GetHashCode()
        {
            var h1 = this.Text.GetHashCode();
            var h2 = this.Number.GetHashCode();
            var h3 = ((h1 + h2) >> 8) ^ ((h1 << 24) ^ (h2 << 16));
            return h3;
        }

        public override bool Equals(object obj)
        {
            var val = obj as ValueClass;
            if (val == null)
                return false;
            return this.CompareTo(val) == 0;
        }
    }
}
