using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Onwo.Input
{
    public interface IAsyncCommand : ICommand,INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);
    }
}
