using System.ComponentModel;

namespace Onwo.ComponentModel
{
    public delegate void MyNotifyPropertChangedEventHandler<in T>(
        T sender, string propertyName, object oldVal, object newVal);
    public interface INotifyPropertyChangedWithValues<T> : INotifyPropertyChanged
    {
        event MyNotifyPropertChangedEventHandler<T> NotifyPropertyChangedWithValues;
    }
}
