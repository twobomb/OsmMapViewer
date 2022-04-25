using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Core.Native;
using OsmMapViewer.Annotations;

namespace OsmMapViewer.Models
{
    public class TreeViewTagValue: INotifyPropertyChanged
    {

        public string Name { get; set; }
        public string Tag { get; set; }
        public string TagWiki { get; set; }
        public string Key{ get; set; }
        public string KeyWiki{ get; set; }
        public string Description{ get; set; }
        public string Photo{ get; set; }
        public string Icon{ get; set; }
        public bool Variable{ get; set; }

        public bool _IsCheckedMe { get; set; }

        public bool IsCheckedMe
        {
            get
            {
                return _IsCheckedMe;
            }
            set
            {
                if (_IsCheckedMe != value)
                {
                    _IsCheckedMe = value;
                    OnPropertyChanged("IsCheckedMe");
                }
            }
        }

        public ObservableCollection<TreeViewTagValue> ChildrenItems { get; } = new ObservableCollection<TreeViewTagValue>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));          
        }
    }
}
