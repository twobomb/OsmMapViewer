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

        private bool _IsShowPushpin = true;
        public bool IsShowPushpin
        {
            get
            {
                return _IsShowPushpin;
            }
            set
            {
                if (_IsShowPushpin != value)
                {
                    if (value)
                        this.mapItemStorage.Items.AddRange(Objects.Select(e => e.MapCenter).ToArray());
                    else
                    {
                        foreach (var item in Objects)
                            this.mapItemStorage.Items.Remove(item.MapCenter);
                    }
                }
                _IsShowPushpin = value;
                OnPropertyChanged("IsShowPushpin");
            }
        }
        public bool _IsShowGeometry = true;
        public bool IsShowGeometry{
            get{
                return _IsShowGeometry;
            }
            set
            {
                if (_IsShowGeometry != value){
                    if (value)
                        this.mapItemStorage.Items.AddRange(Objects.Select(e => e.Geometry).ToArray());
                    else{
                        foreach (var item in Objects)
                            this.mapItemStorage.Items.Remove(item.Geometry);
                    }
                }
                _IsShowGeometry = value;
                OnPropertyChanged("IsShowGeometry");
            }
        }

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
                        if(IsShowGeometry)
                            this.mapItemStorage.Items.Add(geom);
                        if(IsShowPushpin)
                            this.mapItemStorage.Items.Add(((MapObject)eNewItem).MapCenter);
                    }

                    break;
                case "Reset":
                    this.mapItemStorage.Items.Clear();
                    break;
                case "Remove":
                    foreach (var eNewItem in e.NewItems){
                        if(IsShowPushpin)
                            this.mapItemStorage.Items.Remove(((MapObject) eNewItem).MapCenter);
                        if (IsShowGeometry)
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
