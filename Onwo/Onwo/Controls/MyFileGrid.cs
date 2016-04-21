using System.Collections.ObjectModel;
using System.IO;

namespace Onwo.Controls
{
    public class MyFileGrid:MyDataGrid
    {
        public MyFileGrid() : base(new ObservableCollection<FileInfo>())
        {

        }
    }
}
