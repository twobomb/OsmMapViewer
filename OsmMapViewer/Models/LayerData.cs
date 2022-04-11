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
using System.Windows.Media;
using OsmMapViewer.Misc;

namespace OsmMapViewer.Models
{
    public class LayerData: VectorLayer, INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        public ObservableCollection<MapObject> Objects { get; set; } = new ObservableCollection<MapObject>();

        private MapItemStorage mapItemStorage;
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
            this.mapItemStorage = new MapItemStorage();
            this.Data = this.mapItemStorage;
            Objects.CollectionChanged += Objects_CollectionChanged;
            this.EnableSelection = false;
            this.EnableHighlighting = false;
            this.ToolTipEnabled = false;
        }

        private void Objects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e){
            switch (e.Action.ToString()){
                case "Add":
                    foreach (var eNewItem in e.NewItems)
                    {
                        var geom = ((MapObject) eNewItem).Geometry;
                        Utils.ApplyStyle(geom,
                            new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
                            new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),
                            new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
                            new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),
                            new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                            new SolidColorBrush(Color.FromArgb(60, 255, 0, 0)),
                            18,
                            new StrokeStyle()
                            {
                                Thickness = 2
                            });
                        geom.CanMove = false;
                        this.mapItemStorage.Items.Add(geom);

                        this.mapItemStorage.Items.Add(((MapObject)eNewItem).MapCenter);
                    }

                    break;
                case "Reset":
                    this.mapItemStorage.Items.Clear();
                    break;
                case "Remove":
                    foreach (var eNewItem in e.NewItems)
                    {
                        this.mapItemStorage.Items.Remove(((MapObject) eNewItem).MapCenter);
                        this.mapItemStorage.Items.Remove(((MapObject) eNewItem).Geometry);
                    }

                    break;
           }
        }

            private RelayCommand hide;
            public RelayCommand Hide
            {
                get
                {
                    return hide ??
                           (hide = new RelayCommand(obj =>
                           {
                               Visible = !Visible;

                           }));
                }
            }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
