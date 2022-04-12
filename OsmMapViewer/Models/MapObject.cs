﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Map;
using DevExpress.Map.Native;
using DevExpress.Xpf.Map;
using OsmMapViewer.Annotations;

namespace OsmMapViewer.Models
{

    //https://nominatim.openstreetmap.org/lookup?osm_ids=N4692746907&format=xml&addressdetails=1&extratags=1&accept-language=ru&polygon_geojson=1
    public class MapObject: INotifyPropertyChanged {
        public enum OsmTypes {
            Way,
            Node,
            Relation
        }
        private MapPushpin mapCenter = null;
        public MapPushpin MapCenter
        { get{
                if (mapCenter == null)
                    mapCenter = new MapPushpin() { Location = CenterPoint,  Brush = new SolidColorBrush(Color.FromRgb(0,27,232)),EnableSelection = false};
                return mapCenter;
            }
        }
        public string PlaceId { get; set; }
        public string OsmId { get; set; }
        public string Type { get; set; }
        public string Class { get; set; }
        public string DisplayName { get; set; }

        public string DisplayNameLabel
        {
            get {
                if (string.IsNullOrWhiteSpace(DisplayName))
                    return "{БЕЗ НАЗВАНИЯ}";
                return DisplayName;
            }
        }

        public string Icon { get; set; }
        public GeoPoint CenterPoint { get; set; } 
        public GeoPoint BBoxLt { get; set; } 
        public GeoPoint BBoxRb { get; set; }

        public string GeometryType { get; set; }
        private MapItem geometry = null;
        public MapItem Geometry { 
            get
            {
                if (geometry == null && !string.IsNullOrEmpty(RawGeoJson))
                    geometry = Utils.MapItemFromGeoJson(RawGeoJson);
                return geometry;
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
        }
    }
}
