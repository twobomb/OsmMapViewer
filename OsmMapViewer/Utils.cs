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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Map;
using Color = System.Windows.Media.Color;
using OsmMapViewer.Misc;

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


        public static MapShape ParseGeoJson(string geojson)
        {
            return null;
        }
        
        public static VectorLayer VectorLayerFromMapObjects(List<MapObject> list)        {
            VectorLayer vl = new VectorLayer();
            var z = new MapItemStorage();
            z.Items.AddRange(list.Select(e=>e.Geometry).ToList());
            vl.Data = z;
            return vl;
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
                        if(item.geojson.type == "Polygon"){
                            MapPolygon poly = new MapPolygon();
                            foreach (var coord in item.geojson.coordinates[0])
                                poly.Points.Add(new GeoPoint(Utils.ParseDouble(coord[1].ToString()), Utils.ParseDouble(coord[0].ToString())));
                            mo.Geometry = poly;
                        }else if(item.geojson.type == "Point"){
                            MapDot dot= new MapDot();
                            dot.Location = new GeoPoint(Utils.ParseDouble(item.geojson.coordinates[1].ToString()), Utils.ParseDouble(item.geojson.coordinates[0].ToString()));
                            mo.Geometry = dot;
                        }else if(item.geojson.type == "LineString"){
                            MapPolyline line = new MapPolyline();
                            foreach (var coord in item.geojson.coordinates)
                                line.Points.Add(new GeoPoint(Utils.ParseDouble(coord[1].ToString()), Utils.ParseDouble(coord[0].ToString())));
                            mo.Geometry = line;
                        }else if(item.geojson.type == "MultiLineString") {
                            var mapPath = new MapPathGeometry();
                            foreach (var coord in item.geojson.coordinates) {
                                var mpf = new MapPathFigure();
                                mpf.IsClosed = false;
                                mpf.IsFilled = false;
                                bool isStart = true;
                                foreach (var coord1 in coord){
                                    MapPolyLineSegment mp = new MapPolyLineSegment();
                                    mp.Points.Add(new GeoPoint(Utils.ParseDouble(coord1[1].ToString()),
                                        Utils.ParseDouble(coord1[0].ToString())));
                                    if (isStart) {
                                        mpf.StartPoint = mp.Points[0];
                                        isStart = false;
                                    }
                                    mpf.Segments.Add(mp);
                                }
                                mapPath.Figures.Add(mpf);
                            }
                            mo.Geometry = new MapPath() { Data = mapPath };
                        }else if(item.geojson.type == "MultiPolygon"){
                            var mapPath = new MapPathGeometry();
                            foreach (var coord in item.geojson.coordinates) {
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
                            mo.Geometry = new MapPath() { Data = mapPath };
                        }
                        else{
                            Utils.pushCrashLog(new Exception("Не удалось распознать тип " +
                                                             item.geojson.type.ToString() + "  \r\n" + json));
                            MessageBox.Show("Не удалось распознать тип " + item.geojson.type.ToString() +
                                            "Геометрия будет пропущена!","Ошибка парсинга",MessageBoxButton.OK,MessageBoxImage.Exclamation);
                            //throw new Exception("Не удалось распознать тип "+ item.geojson.type.ToString());
                        }
                        if (mo.Geometry != null)
                            mo.Geometry.Tag = mo;
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
