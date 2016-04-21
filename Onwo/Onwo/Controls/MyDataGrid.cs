using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Onwo.Controls
{
    public class MyDataGrid : DataGrid
    {
        private IList tmpList;
        public MyDataGrid():this(new ObservableCollection<object>())
        {
        }
        public MyDataGrid(IList selectedItemList) : base()
        {
            this.Loaded += MyDataGrid_Loaded;
            tmpList = selectedItemList;
        }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            foreach (var item in e.RemovedItems)
                this.SelectedItemsList.Remove(item);
            foreach (var item in e.AddedItems)
                this.SelectedItemsList.Add(item);
            e.Handled = true;
        }

        private void MyDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.SelectedItemsList = tmpList;
            tmpList = null;
            this.Loaded -= MyDataGrid_Loaded;
        }
        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MyDataGrid), new PropertyMetadata(null));
    }
}
