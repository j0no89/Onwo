using System.Collections.Generic;
using System.IO;

namespace Onwo.Collections.Special
{
    public class LinkedDirectoryTree : ObservableSortedTree<DirectoryInfo>
    {
        private LazyDirectoryTree _source;
        public LazyDirectoryTree Source
        {
            get { return _source; }
            set
            {
                if (ReferenceEquals(_source, value)) return;
                var old = _source;
                _source = value;
                OnPropertyChanged(nameof(Source));
                onSourceChanged(old);
            }
        }

        private void onSourceChanged(LazyDirectoryTree oldSource)
        {
            if (oldSource != null)
            {
                oldSource.ComparerChanged -= Source_ComparerChanged;

            }
            if (this.Source != null)
            {
                Source.ComparerChanged += Source_ComparerChanged;
            }
        }

        private void Source_ComparerChanged(ObservableSortedTree<DirectoryInfo> sender, IComparer<ObservableSortedTree<DirectoryInfo>> oldCOmparer)
            => this.Comparer = Source.Comparer;
        public LinkedDirectoryTree(LazyDirectoryTree source)
        {
            this.Source = source;
        }
    }
    
}
