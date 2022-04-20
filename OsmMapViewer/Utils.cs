using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OsmMapViewer.Models;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Map;
using DevExpress.Xpf.Map;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;
using OsmMapViewer.Misc;
using OsmMapViewer.Properties;

namespace OsmMapViewer
{
    public static class Utils
    {
        public static void pushCrashLog(Exception e) {
            UnhandledExceptionEventArgs ex = new UnhandledExceptionEventArgs(e, false);
            ((App)App.Current).CurrentDomain_UnhandledException(null, ex);
        }
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch            {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
        public static double GetDistance(Point p1,Point p2)        {
            return Math.Sqrt(Math.Pow(p2.X-p1.X,2) + Math.Pow(p2.Y - p1.Y,2));
        }
        public static string PointToTextGeometry(List<CoordPoint> list, string type = "POLYGON"){
            type = type.ToUpper();
            switch (type){
                case "POLYGON":
                    if (list[0].GetY() != list[list.Count - 1].GetY() || list[0].GetX() != list[list.Count - 1].GetX()){
                        list = list.ToArray().ToList();
                        list.Add(list.First());
                    }

                    return String.Format("POLYGON(({0}))",string.Join(",", list.Select(point => point.GetX().ToString().Replace(',','.')+" " + point.GetY().ToString().Replace(',','.'))));
                    break;
                default:
                    throw new Exception("Неизвестный тип " + type);
            }
        }
        public static BitmapImage GetImageFromStream(Stream s){
            using (var stream = new MemoryStream()){
                byte[] buff = new byte[1024 * 1024];
                while (s.CanRead){
                    int len = s.Read(buff, 0, buff.Length);
                    stream.Write(buff, 0, len);
                    if (len == 0)
                        break;
                }
                stream.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
        public static string GetExeDir(){
            return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
        }

        public static Dictionary<string,List<string>> GetTagsDictionary(){
            Dictionary<string, List<string>> dicts = new Dictionary<string, List<string>>();
            foreach (var tn in GetTagsList()) {
                if(string.IsNullOrWhiteSpace(tn.Tag))
                    continue;
                if (!dicts.ContainsKey(tn.Tag))
                    dicts.Add(tn.Tag, new List<string>());
                if (string.IsNullOrWhiteSpace(tn.Key))
                    continue;
                if(!dicts[tn.Tag].Contains(tn.Key))
                    dicts[tn.Tag].Add(tn.Key);
            }
            return dicts;
        }

        public static ObservableCollection<TreeViewTagValue> GetTagsList() {
            ObservableCollection<TreeViewTagValue> list = new ObservableCollection<TreeViewTagValue>();
            var tags = GetTagsTree();
            foreach (var tn1 in tags) {
                list.Add(tn1);
                foreach (var tn2 in tn1.ChildrenItems) {
                    list.Add(tn2);
                    foreach (var tn3 in tn2.ChildrenItems)
                        list.Add(tn3);
                }
            }
            return list;
        }

        public static ObservableCollection<TreeViewTagValue> GetTagsTree(){
            ObservableCollection<TreeViewTagValue> values = new ObservableCollection<TreeViewTagValue>();
            try
            {
                string json = File.ReadAllText(Config.JSON_TAGS);
                dynamic obj = JArray.Parse(json);
                foreach (var h in obj){
                    TreeViewTagValue tn = new TreeViewTagValue() { 
                        Name = h.title.ToString(),
                        Description = h.description.ToString(),
                        Tag = h.tag.ToString(),
                        TagWiki = h.tagwiki.ToString(),
                    };
                    List<string> groups = new List<string>();
                    foreach (var key in h.keys)
                    {
                        string subgroup = key.subgroup.ToString();
                        if (!groups.Contains(subgroup))
                            groups.Add(subgroup);
                    }

                    if (groups.Count > 1){
                        foreach (var group in groups){
                            TreeViewTagValue tn1 = new TreeViewTagValue() {Name = group };
                            foreach (var key in h.keys){
                                if (key.subgroup != group)
                                    continue;
                                TreeViewTagValue tn2 = new TreeViewTagValue()
                                {
                                    Tag = key.tag.ToString(),
                                    Key= key.key,
                                    TagWiki = key.tagwiki,
                                    Icon = key.icon,
                                    Photo = key.photo,
                                    Name = (key.title.ToString() == key.key.ToString() ? "" : key.title.ToString()) + " (" + key.tag.ToString() + ":" + key.key.ToString() + ")",
                                    Description = key.description,
                                    KeyWiki = key.keywiki,
                                    Variable = key.variable
                                };
                                tn1.ChildrenItems.Add(tn2);
                            }
                            tn.ChildrenItems.Add(tn1);
                        }
                    }
                    else{
                        foreach (var key in h.keys){
                            TreeViewTagValue tn2 = new TreeViewTagValue()
                            {
                                Tag = key.tag.ToString(),
                                Key = key.key.ToString(),
                                TagWiki = h.tagwiki,
                                Icon = key.icon,
                                Photo = key.photo,
                                Name = (key.title.ToString() == key.key.ToString()?"": key.title.ToString())+ " (" + key.tag.ToString()+":"+ key.key.ToString()+")",
                                Description = key.description,
                                KeyWiki = key.keywiki,
                                Variable = key.variable
                            };
                            tn.ChildrenItems.Add(tn2);
                        }
                    }
                    values.Add(tn);
                }
            }
            catch (Exception e)
            {
                Utils.pushCrashLog(e);
                Console.WriteLine(e);
            }
            return values;
        }



        
        public static VectorLayer VectorLayerFromMapObjects(List<MapObject> list)        {
            VectorLayer vl = new VectorLayer();
            var z = new MapItemStorage();
            z.Items.AddRange(list.Select(e=>e.Geometry).ToList());
            vl.Data = z;
            return vl;
        }

        public static Brush GetStrokeStyle(MapItem item){
            return GetPropertyObject<Brush>(item,"Stroke");
        }

        public static T DeserializeJson<T>(string toDeserialize)
        {
            return JsonConvert.DeserializeObject<T>(toDeserialize);
        }

        public static string SerializeJson<T>(T toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize);
        }
        public static void SetKitSelectionList(List<KitSelection> list) {
            Settings.Default.KitSelections = SerializeJson(list);
            Settings.Default.Save();
        }
        public static List<KitSelection> GetKitSelectionList() {
            List<KitSelection> list = null;
            try {
                list = DeserializeJson<List<KitSelection>>(Settings.Default.KitSelections);
            }
            catch (Exception e){
                Utils.pushCrashLog(e);
                Settings.Default.KitSelections = "";
                Settings.Default.Save();
            }

            if (list == null)
                list = new List<KitSelection>();
            return list;
        }
        public static MapItem ApplyStrokeStyle(MapItem item, Brush stroke){
            SetPropertyObject<Brush>(item, "Stroke", stroke);
            return item;
        }
        public static Brush GetFillStyle(MapItem item) {
            return GetPropertyObject<Brush>(item, "Fill");
        }

        public static T GetPropertyObject<T>(object obj,string propertyName) where T: class{
            foreach (var attr in obj.GetType().GetProperties())
                if (attr.Name == propertyName && attr.PropertyType == typeof(T))
                    return (T)attr.GetValue(obj);
            return null;
        }
        public static bool SetPropertyObject<T>(object obj,string propertyName, object value) where T:class{
            foreach (var attr in obj.GetType().GetProperties())
                if (attr.Name == propertyName && attr.PropertyType == typeof(T)) {
                    attr.SetValue(obj, value);
                    return true;
                }
            return false;
        }
        public static MapItem ApplyFillStyle(MapItem item, Brush fill) {
            SetPropertyObject<Brush>(item, "Fill", fill);
            return item;
        }

        public static string ByteToString(byte[] b) {
            return System.Convert.ToBase64String(b);
        }
        public static byte[] StringToByte(string s) {
            return System.Convert.FromBase64String(s);
        }
        public static List<LayerData> LoadLayers()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.Layers))
                return new List<LayerData>();
            try {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(StringToByte(Settings.Default.Layers))){
                    return (List<LayerData>)bf.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                Utils.pushCrashLog(e);
            }
            return new List<LayerData>();
        }
        public static void SaveLayers(List<LayerData> data) {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, data);
                Settings.Default.Layers =  Utils.ByteToString(ms.ToArray());
                Settings.Default.Save();
            }
        }
        public static String GetTimestamp(DateTime? value = null) {
            return value.GetValueOrDefault(DateTime.Now).ToString("yyyyMMddHHmmssffff");
        }
        public static string SerializeGeoPoint(GeoPoint b) {
            if (b == null)
                return null;
            return $"{b.GetX()};{b.GetY()}";
        }

        public static SolidColorBrush HexToColorBrush(string hex) {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        public static string ColorBrushToHex(SolidColorBrush brush){
            return brush.Color.ToString();
        }
        public static GeoPoint DeSerializeGeoPoint(string s) {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            var q = s.Split(new[] {';'}).Select(Utils.ParseDouble).ToArray();
            return new GeoPoint(q[1],q[0]);
        }
        public static string SerializeBrush(SolidColorBrush b) {
            if (b == null)
                return null;
            return $"{b.Color.A};{b.Color.R};{b.Color.G};{b.Color.B}";
        }
        public static SolidColorBrush DeSerializeBrush(string s) {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            var q = s.Split(new[] {';'}).Select(byte.Parse).ToArray();
            return new SolidColorBrush(Color.FromArgb(q[0],q[1],q[2],q[3]));
        }
        public static double GetBorderSize(MapItem item)
        {
            return GetPropertyObject<StrokeStyle>(item, "StrokeStyle").Thickness;
        }
        public static MapItem ApplyBorderSize(MapItem item, double size)
        {
            SetPropertyObject<StrokeStyle>(item, "StrokeStyle", new StrokeStyle()
            {
                Thickness = size
            });
            return item;
        }
        public static MapItem ApplyStyle(MapItem item,Brush stroke, Brush fill,Brush selectedStroke, Brush selectedFill,Brush hightlightStroke, Brush hightlightFill, double dotSize , StrokeStyle ss){
            if (item is MapPolygon mp) {
                mp.Stroke= stroke;
                mp.Fill = fill;
                mp.StrokeStyle = ss;
                mp.SelectedStrokeStyle = ss;
                mp.HighlightStrokeStyle = ss;
                mp.SelectedStroke= selectedStroke;
                mp.SelectedFill = selectedFill;
                mp.HighlightStroke = hightlightStroke;
                mp.HighlightFill = hightlightFill;
            }
            if (item is MapDot md) {
                md.Stroke= stroke;
                md.Fill = fill;
                md.Size = dotSize;
                md.StrokeStyle = ss;
                md.SelectedStrokeStyle = ss;
                md.HighlightStrokeStyle = ss;
                md.SelectedStroke = selectedStroke;
                md.SelectedFill = selectedFill;
                md.HighlightStroke = hightlightStroke;
                md.HighlightFill= hightlightFill;
            }
            if (item is MapPolyline mpl) {
                mpl.Stroke= stroke;
                mpl.StrokeStyle = ss;
                mpl.SelectedStrokeStyle = ss;
                mpl.HighlightStrokeStyle = ss;
                mpl.Fill = fill;
                mpl.SelectedStroke = selectedStroke;
                mpl.SelectedFill = selectedFill;
                mpl.HighlightStroke = hightlightStroke;
                mpl.HighlightFill = hightlightFill;
            }
            if (item is MapPath mpg) {
                mpg.Stroke= stroke;
                mpg.Fill = fill;
                mpg.StrokeStyle = ss;
                mpg.SelectedStrokeStyle= ss;
                mpg.HighlightStrokeStyle= ss;
                mpg.SelectedStroke = selectedStroke;
                mpg.SelectedFill = selectedFill;
                mpg.HighlightStroke = hightlightStroke;
                mpg.HighlightFill = hightlightFill;
            }
            return item;
        }
        public static List<MapObject> ParseObjects(string json) {
            List<MapObject> list = new List<MapObject>();
            //try            {
                dynamic arr = JArray.Parse(json);
                foreach (var item in arr)
                {
                    MapObject mo = new MapObject() {
                        CenterPoint = new GeoPoint(Utils.ParseDouble(item.lat.ToString()), Utils.ParseDouble(item.lon.ToString())),
                        DisplayName = item.display_name,
                        Class = item["class"],
                        Type = item.osm_type,
                        OsmId = item.osm_id,
                        PlaceId = item.place_id,
                        Icon = item.icon,
                        BBoxLt = new GeoPoint(Utils.ParseDouble(item.boundingbox[0].ToString()), Utils.ParseDouble(item.boundingbox[2].ToString())),
                        BBoxRb= new GeoPoint(Utils.ParseDouble(item.boundingbox[1].ToString()), Utils.ParseDouble(item.boundingbox[3].ToString())),
                    };
                    

                mo.Tags.Add(new TagValue() { Tag = "osm_id", Key = mo.OsmId });
                if (item.extratags != null)
                        foreach (var v in item.extratags)
                            mo.Tags.Add(new TagValue(){Tag = v.Name.ToString(),Key = v.Value.ToString()});

                    if(item.geojson != null){
                        mo.RawGeoJson = item.geojson.ToString();
                    }
                    list.Add(mo);
                }
          /*  }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }*/

            return list;
        }

        //MapItem to geojson с моим типом mapellipse
        public static string GeoJsonFromObject(MapItem mi){
            string geojson = "";
            if (mi is MapPolygon mp) 
                return "{ \"type\": \"Polygon\", \"coordinates\": ["+
                       JsonConvert.SerializeObject(mp.Points.Select(point => new double[] { point.GetX(), point.GetY() }).ToArray())
                       + "]}";

            if (mi is MapPolyline mpl) 
                return "{ \"type\": \"LineString\", \"coordinates\": " +
                       JsonConvert.SerializeObject(mpl.Points.Select(point => new double[] { point.GetX(), point.GetY() }).ToArray())
                       + "}";

            if (mi is MapDot md) 
                return "{ \"type\": \"Point\", \"coordinates\": " +
                       JsonConvert.SerializeObject(new double[]{md.Location.GetX(), md.Location.GetY()})
                       + "}";
            if (mi is MapEllipse me) 
                return "{ \"type\": \"MapEllipse\", \"coordinates\": " +
                       JsonConvert.SerializeObject(new double[] { me.Location.GetX(), me.Location.GetY(), me.Width,me.Height })
                       + "}";
            return geojson;

        }
        public static MapItem MapItemFromGeoJson(string json){
            if (string.IsNullOrWhiteSpace(json))
                return null;
            dynamic geojson = JObject.Parse(json);
            MapItem item = null;
            if (geojson.type == "MapEllipse")//мой тип
            {
                MapEllipse ellipse = new MapEllipse();
                ellipse.Location = new GeoPoint(Utils.ParseDouble(geojson.coordinates[1].ToString()),Utils.ParseDouble(geojson.coordinates[0].ToString()));
                ellipse.Width= Utils.ParseDouble(geojson.coordinates[2].ToString());
                ellipse.Height= Utils.ParseDouble(geojson.coordinates[3].ToString());
                item = ellipse;
            }else if (geojson.type == "Polygon")
            {
                MapPolygon poly = new MapPolygon();
                foreach (var coord in geojson.coordinates[0])
                    poly.Points.Add(new GeoPoint(Utils.ParseDouble(coord[1].ToString()), Utils.ParseDouble(coord[0].ToString())));
                item = poly;
            }
            else if (geojson.type == "Point")
            {
                MapDot dot = new MapDot()
                {
                    Size = Config.DEFAULT_DOT_SIZE
                };
                dot.Location = new GeoPoint(Utils.ParseDouble(geojson.coordinates[1].ToString()), Utils.ParseDouble(geojson.coordinates[0].ToString()));
                item = dot;
            }
            else if (geojson.type == "LineString")
            {
                MapPolyline line = new MapPolyline();
                foreach (var coord in geojson.coordinates)
                    line.Points.Add(new GeoPoint(Utils.ParseDouble(coord[1].ToString()), Utils.ParseDouble(coord[0].ToString())));
                item = line;
            }
            else if (geojson.type == "MultiLineString")
            {
                var mapPath = new MapPathGeometry();
                foreach (var coord in geojson.coordinates)
                {
                    var mpf = new MapPathFigure();
                    mpf.IsClosed = false;
                    mpf.IsFilled = false;
                    bool isStart = true;
                    foreach (var coord1 in coord)
                    {
                        MapPolyLineSegment mp = new MapPolyLineSegment();
                        mp.Points.Add(new GeoPoint(Utils.ParseDouble(coord1[1].ToString()),
                            Utils.ParseDouble(coord1[0].ToString())));
                        if (isStart)
                        {
                            mpf.StartPoint = mp.Points[0];
                            isStart = false;
                        }
                        mpf.Segments.Add(mp);
                    }
                    mapPath.Figures.Add(mpf);
                }
                item = new MapPath() { Data = mapPath };
            }
            else if (geojson.type == "MultiPolygon")
            {
                var mapPath = new MapPathGeometry();
                foreach (var coord in geojson.coordinates)
                {
                    var mpf = new MapPathFigure();
                    bool isStart = true;
                    foreach (var coord1 in coord)
                    {
                        MapPolyLineSegment mp = new MapPolyLineSegment();
                        foreach (var coord2 in coord1)
                            mp.Points.Add(new GeoPoint(Utils.ParseDouble(coord2[1].ToString()),
                                Utils.ParseDouble(coord2[0].ToString())));
                        if (isStart)
                        {
                            mpf.StartPoint = mp.Points[0];
                            isStart = false;
                        }

                        mpf.Segments.Add(mp);
                    }
                    mapPath.Figures.Add(mpf);
                }
                item = new MapPath() { Data = mapPath };
            }
            else
            {
                Utils.pushCrashLog(new Exception("Не удалось распознать тип " +
                                                 geojson.type.ToString() + "  \r\n" + json));
                MessageBox.Show("Не удалось распознать тип " + geojson.type.ToString() +
                                "Геометрия будет пропущена!", "Ошибка парсинга", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //throw new Exception("Не удалось распознать тип "+ item.geojson.type.ToString());
            }
            return item;
        }

        public static MessageBoxResult MsgBoxInfo(string msg, string title = "Информация") {
            return MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static MessageBoxResult MsgBoxWarning(string msg, string title = "Предупреждение") {
            return MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        public static MessageBoxResult MsgBoxError(string msg, string title = "Ошибка") {
            return MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static MessageBoxResult MsgBoxQuestion(string msg, string title = "Вопрос") {
            return MessageBox.Show(msg, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }
        public static double ParseDouble(string s){
            s = s.Replace(",", ".");
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
        /*
        public static GMapRoute StringLineToRoute(string s, string routeName)
        {
            if (s.ToLower().IndexOf("linestring") != 0)
                throw new Exception("Ожидается LINESTRING!");
            s = Regex.Match(s, @"\d[^()]+").Value;
            var route = new GMapRoute(s.Split(new char[] { ',' }).Select(s1 =>
                  new PointLatLng(double.Parse(s1.Split(new char[] { ' ' })[1], CultureInfo.InvariantCulture),
                      double.Parse(s1.Split(new char[] { ' ' })[0], CultureInfo.InvariantCulture))), routeName);
            route.Stroke = Pens.Red;
            return route;
        }
        public static GMapPolygon StringPolygonToPolygon(string s, string polygonName)
        {
            if (s.ToLower().IndexOf("polygon") != 0)
                throw new Exception("Ожидается POLYGON!");
            s = Regex.Match(s, @"\d[^()]+").Value;

            var poly = new GMapPolygon(s.Split(new char[] { ',' }).Select(s1 =>
                new PointLatLng(double.Parse(s1.Split(new char[] { ' ' })[1], CultureInfo.InvariantCulture),
                    double.Parse(s1.Split(new char[] { ' ' })[0], CultureInfo.InvariantCulture))).ToList(), polygonName);

            poly.Stroke = Pens.Red;
            poly.Fill = new SolidBrush(Color.FromArgb(70, 255, 0, 0));
            return poly;
        }

        public static List<string> SelectAround(string tbl, PointLatLng point, int radiusMetr)
        {
            List<string> o = new List<string>();
            string sql = String.Format(
                "SELECT \"name\",\"addr:housenumber\" FROM planet_osm_{0} WHERE ST_Contains(ST_Buffer(ST_GeomFromText('POINT({1} {2})', 4326)::geography, {3})::geometry, ST_Centroid(ST_Transform(way, 4326)))",
                tbl, point.Lng.ToString(CultureInfo.InvariantCulture), point.Lat.ToString(CultureInfo.InvariantCulture), radiusMetr);
            using (var con = DBController.GetConnection())
            {
                con.Open();
                var cmd = new NpgsqlCommand(sql, con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    o.Add(reader["name"].ToString() + " " + reader["addr:housenumber"].ToString());
                reader.Close();
            }

            return o;

        }
        public static GMarkerGoogle StringPointToMarker(string s, GMarkerGoogleType type)
        {
            if (s.ToLower().IndexOf("point") != 0)
                throw new Exception("Ожидается POINT!");
            s = Regex.Match(s, @"\d[^()]+").Value;

            return new GMarkerGoogle(
                new PointLatLng(double.Parse(s.Split(new char[] { ' ' })[1], CultureInfo.InvariantCulture),
                    double.Parse(s.Split(new char[] { ' ' })[0], CultureInfo.InvariantCulture)), type);
        }
        public static PointLatLng[] FromText(string txt)
        {
            return txt.Split(new char[] { ',' }).Select(s =>
            {
                var m = s.Trim().Split(new char[] { ' ' });
                return new PointLatLng(double.Parse(m[1], CultureInfo.InvariantCulture),
                    double.Parse(m[0], CultureInfo.InvariantCulture));

            }).ToArray();
        }
        /**
       * FindPointAtDistanceFrom находит и возвращает координаты точки которые находятся в {distanceKilometres} км от точки {startPoint} под углом {initialBearingRadians}
       *Угол верх 0 и по часовой
       */
        /*
        public static GMap.NET.PointLatLng FindPointAtDistanceFrom(GMap.NET.PointLatLng startPoint, double initialBearingRadians, double distanceKilometres)
        {

            const double radiusEarthKilometres = 6378;
            var distRatio = distanceKilometres / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);
            var startLatRad = MercatorProjection.DegreesToRadians(startPoint.Lat);
            var startLonRad = MercatorProjection.DegreesToRadians(startPoint.Lng);
            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));
            var endLonRads = startLonRad + Math.Atan2(
                Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
                distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new GMap.NET.PointLatLng(MercatorProjection.RadiansToDegrees(endLatRads), MercatorProjection.RadiansToDegrees(endLonRads));
        }*/
    }
}
