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
    public class TreeViewTagValue
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

        public bool IsCheckedMe { get; set; }

        public ObservableCollection<TreeViewTagValue> ChildrenItems { get; } = new ObservableCollection<TreeViewTagValue>();
     
    }
}
