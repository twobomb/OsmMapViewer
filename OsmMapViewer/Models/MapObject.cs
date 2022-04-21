using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Map;
using DevExpress.Map.Native;
using DevExpress.Xpf.Map;
using OsmMapViewer.Annotations;
using GeoUtils = DevExpress.Map.Native.GeoUtils;

namespace OsmMapViewer.Models {

    //https://nominatim.openstreetmap.org/lookup?osm_ids=N4692746907&format=xml&addressdetails=1&extratags=1&accept-language=ru&polygon_geojson=1
    [Serializable]
    public class MapObject : INotifyPropertyChanged, ISerializable{

        private MapPushpin mapCenter = null;

        public MapPushpin MapCenter {
            get {
                if (mapCenter == null && CenterPoint != null && TypeData != "draw")
                    mapCenter = new MapPushpin(){
                        Location = CenterPoint, Brush = new SolidColorBrush(Color.FromRgb(0, 27, 232)),
                        EnableSelection = false
                    };
                return mapCenter;
            }
        }

        public string TypeData { get; set; } = "map";//map - слой выборки, draw - слой рисования,search - слой поиска
        public string PlaceId { get; set; }
        public string OsmId { get; set; }
        public string Type { get; set; }
        public string Class { get; set; }
        public string _DisplayName = "";
        public string DisplayName { 
            get {
                return _DisplayName;
            }
            set
            {
                _DisplayName = value;
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("DisplayNameLabel");
            }
            }
        public LayerData Layer { get; set; } 

        public string DisplayNameLabel{
            get
            {
                if (string.IsNullOrWhiteSpace(DisplayName))
                    return "{БЕЗ НАЗВАНИЯ}";
                return DisplayName;
            }
        }

        public string Icon { get; set; }
        public GeoPoint CenterPoint { get; set; }
        public GeoPoint _BBoxLt = null;
        public GeoPoint BBoxLt
        {
            get {
                if (_BBoxLt == null) {
                    var m= Geometry.GetType().GetMethods().FirstOrDefault(info => info.Name == "GetBounds");
                    if (m != null && m.Invoke(Geometry, new object[0]) is MapBounds mb) {
                        return new GeoPoint(mb.Top, mb.Left);
                    }

                    if (CenterPoint != null)
                        return new GeoPoint(CenterPoint.GetY() - 0.0001, CenterPoint.GetX() -0.0001);
                }
                return _BBoxLt;
            }
            set
            {
                _BBoxLt = value;
                OnPropertyChanged("BBoxLt");
            }
        }

        public GeoPoint _BBoxRb = null;
        public GeoPoint BBoxRb
        {
            get {
                if (_BBoxRb == null) {
                    var m= Geometry.GetType().GetMethods().FirstOrDefault(info => info.Name == "GetBounds");
                    if (m!= null && m.Invoke(Geometry, new object[0]) is MapBounds mb)
                        return new GeoPoint(mb.Bottom, mb.Right);
                    if (CenterPoint != null)
                        return new GeoPoint(CenterPoint.GetY() + 0.0001, CenterPoint.GetX() +0.0001);
                }
                return _BBoxRb;
            }
            set
            {
                _BBoxRb = value;
                OnPropertyChanged("BBoxRb");
            }
        }

        public SolidColorBrush FillGeometry { get; set; } = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0));
        public SolidColorBrush StrokeGeometry { get; set; } = new SolidColorBrush(Color.FromArgb(180, 255, 0, 0));
        public double BorderGeometry { get; set; } = 2;
        private MapItem geometry = null;
        public MapItem Geometry{
            get{
                if (geometry == null && !string.IsNullOrEmpty(RawGeoJson)){
                    geometry = Utils.MapItemFromGeoJson(RawGeoJson);
                    Utils.ApplyFillStyle(geometry, FillGeometry);
                    Utils.ApplyStrokeStyle(geometry, StrokeGeometry);
                    Utils.ApplyBorderSize(geometry, BorderGeometry);
                }

                return geometry;
            }
            set
            {
                geometry = value;
                OnPropertyChanged("Geometry");
            }
        }

        public string RawGeoJson { get; set; }
        public List<TagValue> Tags { get; set; } = new List<TagValue>();

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
                return "{БЕЗ НАЗВАНИЯ}";
            return DisplayName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null){
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("DisplayName", DisplayName, typeof(string));
            info.AddValue("TypeData", TypeData, typeof(string));
            info.AddValue("PlaceId", PlaceId, typeof(string));
            info.AddValue("OsmId", OsmId, typeof(string));
            info.AddValue("Type", Type, typeof(string));
            info.AddValue("Class", Class, typeof(string));
            info.AddValue("Icon", Icon, typeof(string));
            info.AddValue("CenterPoint", Utils.SerializeGeoPoint(CenterPoint));
            info.AddValue("BBoxLt", Utils.SerializeGeoPoint(BBoxLt));
            info.AddValue("BBoxRb", Utils.SerializeGeoPoint(BBoxRb));
            info.AddValue("FillGeometry", Utils.SerializeBrush(FillGeometry));
            info.AddValue("StrokeGeometry", Utils.SerializeBrush(StrokeGeometry));
            info.AddValue("BorderGeometry", BorderGeometry,typeof(double));
            info.AddValue("RawGeoJson", RawGeoJson, typeof(string));
            info.AddValue("Tags", Tags);
        }
        protected MapObject(SerializationInfo info, StreamingContext context){
            DisplayName = (string)info.GetValue("DisplayName", typeof(string));
            TypeData = (string)info.GetValue("TypeData", typeof(string));
            PlaceId = (string)info.GetValue("PlaceId", typeof(string));
            OsmId = (string)info.GetValue("OsmId", typeof(string));
            Type = (string)info.GetValue("Type", typeof(string));
            Class = (string)info.GetValue("Class", typeof(string));
            Icon = (string)info.GetValue("Icon", typeof(string));
            CenterPoint = Utils.DeSerializeGeoPoint(info.GetString("CenterPoint"));
            BBoxLt = Utils.DeSerializeGeoPoint(info.GetString("BBoxLt"));
            BBoxRb = Utils.DeSerializeGeoPoint(info.GetString("BBoxRb"));
            FillGeometry = Utils.DeSerializeBrush(info.GetString("FillGeometry"));
            StrokeGeometry = Utils.DeSerializeBrush(info.GetString("StrokeGeometry"));

            BorderGeometry = (double)info.GetValue("BorderGeometry", typeof(double));
            RawGeoJson = (string)info.GetValue("RawGeoJson", typeof(string));
            Tags = (List<TagValue>)info.GetValue("Tags", typeof(object));
        }
        public MapObject()
        {
        }

    }

}
