using System.Threading.Tasks;
using System.Windows.Input;

namespace Onwo.Input
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
