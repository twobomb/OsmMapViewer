using DevExpress.Xpf.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OsmMapViewer.Models
{
    public class LayerData: VectorLayer, INotifyPropertyChanged
    {
        public string Name { get; set; }

        public ObservableCollection<MapObject> Objects { get; set; } = new ObservableCollection<MapObject>();

        public bool _Visible = true;
        public bool Visible { 
            get {
                return _Visible;
            } 
            set {
                _Visible = value;
                if (value)
                    this.Visibility = Visibility.Visible;
                else
                    this.Visibility = Visibility.Collapsed;
                OnPropertyChanged("Visible");
            }
        }

        public LayerData()
        {
            Objects.CollectionChanged += Objects_CollectionChanged;
        }

        private void Objects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e){
            switch (e.Action.ToString()){
                case "Add":
                    break;
                case "Reset":
                    break;
                case "Remove":
                    break;
           }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
