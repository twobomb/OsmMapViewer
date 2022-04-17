using DevExpress.Xpf.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OsmMapViewer.Misc;


namespace OsmMapViewer.Models
{
    [Serializable]
    public class LayerData: VectorLayer, INotifyPropertyChanged, ISerializable{

        public string TypeData { get; set; } = "map";//map - слой выборки, draw - слой рисования
        public string ID = Utils.GetTimestamp();
        private string _DisplayName = "";
        public string DisplayName{
            get{
                return _DisplayName;
            }
            set {
                _DisplayName= value;
                OnPropertyChanged("DisplayName");
            }
        }


        public ObservableCollection<MapObject> Objects { get; set; } = new ObservableCollection<MapObject>();
        private bool _IsRenameActive = false;
        public bool IsRenameActive
        {
            get
            {
                return _IsRenameActive;
            }
            set
            {
                _IsRenameActive = value;
                OnPropertyChanged("IsRenameActive");
            }
        }

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

        private void InitData()
        {
            this.mapItemStorage = new MapItemStorage();
            this.Data = this.mapItemStorage;
            Objects.CollectionChanged += Objects_CollectionChanged;
            this.EnableSelection = true;
            this.EnableHighlighting = false;
            this.ToolTipEnabled = false;
        }
        public LayerData(){
            InitData();
        }

        private int __clickTimestamp = 0;
        private void Objects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e){
            switch (e.Action.ToString()){
                case "Add":
                    foreach (var eNewItem in e.NewItems)
                    {
                        var item = eNewItem as MapObject;
                        var geom = ((MapObject) eNewItem).Geometry;
                        if (geom != null){
                            geom.CanMove = false;
                            if (IsShowGeometry) 
                                this.mapItemStorage.Items.Add(geom);
                            if (TypeData == "draw")
                            {
                                geom.MouseLeftButtonDown += (o, args) =>
                                {
                                    __clickTimestamp = args.Timestamp;
                                };
                                geom.MouseLeftButtonUp += (o, args) =>
                                {
                                    if (args.Timestamp - __clickTimestamp <= 250)
                                        OnClickDrawObject(item);
                                };
                            }
                        }

                        if(IsShowPushpin && ((MapObject)eNewItem).MapCenter !=null)
                            this.mapItemStorage.Items.Add(((MapObject)eNewItem).MapCenter);
                    }

                    break;
                case "Reset":
                    this.mapItemStorage.Items.Clear();
                    break;
                case "Remove":
                    foreach (var eNewItem in e.OldItems){
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
        public void GetObjectData(SerializationInfo info, StreamingContext context){
            info.AddValue("ID", ID, typeof(string));
            info.AddValue("DisplayName", DisplayName, typeof(string));
            info.AddValue("IsShowPushpin", IsShowPushpin, typeof(bool));
            info.AddValue("IsShowGeometry", IsShowGeometry, typeof(bool));
            info.AddValue("Objects", Objects);
        }

        [OnDeserialized]
        private void SetValuesOnDeserialized(StreamingContext context)
        {
            if(_beforeLoadObject != null)
                foreach (var c in _beforeLoadObject) 
                    Objects.Add(c);
        }

        private ObservableCollection<MapObject> _beforeLoadObject = null;
        protected LayerData(SerializationInfo info, StreamingContext context){
            InitData();
            ID = (string)info.GetValue("ID", typeof(string));
            DisplayName = (string)info.GetValue("DisplayName", typeof(string));
            IsShowPushpin = (bool)info.GetValue("IsShowPushpin", typeof(bool));
            IsShowGeometry = (bool)info.GetValue("IsShowGeometry", typeof(bool));
            _beforeLoadObject = (ObservableCollection<MapObject>) info.GetValue( "Objects", typeof(object));
        }

        public event Action<MapObject> ClickDrawObject;
        protected virtual void OnClickDrawObject(MapObject obj) {
            ClickDrawObject?.Invoke(obj); 
        }
    }

}
