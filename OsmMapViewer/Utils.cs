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
using System.Net;
using System.Xml;
using System.Net.Http;
using System.Net.Http.Headers;
using DevExpress.Map.Kml.Model;
using DevExpress.Xpf.Map.Native;
using Point = System.Windows.Point;

namespace OsmMapViewer{

    public static class Utils{

        public static List<MapObject> SearchObjectsWithRestrictPoint(GeoPoint p, string tagsJson = "[]", int rad = 50,bool IsIntersectsType = false, bool isLine = true, bool isPolygon = true, bool isPoint = true) {
            string jsonReq = $@"{{""tags"":{tagsJson},""params"":{{""line"":{ (isLine?"1":"0")},""polygon"":{(isPolygon? "1" : "0")},""point"":{(isPoint? "1" : "0")}}}";

            jsonReq += ",\"restrict_point_radius\":{ \"lon\":" + p.GetX().ToString().Replace(',', '.') +
                           ",\"lat\":" + p.GetY().ToString().Replace(',', '.') + ",\"radius_meter\":" + rad+
                           "}";
            if (IsIntersectsType)
                jsonReq += ",\"restrict_type\":\"intersects\"";
            jsonReq += "}";
            var task = httpClient.PostAsync(Config.GET_DATA, new StringContent(jsonReq, Encoding.UTF8));
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK) {
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait();
                string res = tz.Result;
                List<MapObject> resultArr = new List<MapObject>();
                resultArr.AddRange(Utils.ParseObjects(res));

                return resultArr;
            }
            else
                throw new Exception("Произошла ошибка при обращении к серверу!");
        }

        public static MapObject NominatimReverse(GeoPoint p) {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $@"{Config.NOMINATIM_HOST}{Config.NOMINATIM_REVERSE}?lat={p.Latitude.ToString().Replace(",", ".")}&lon={p.Longitude.ToString().Replace(",", ".")}&format=json&accept-language=ru&polygon_geojson=1&extratags=1");
            requestMessage.Headers.Add("user-agent","OsmMapViewer");
            requestMessage.Headers.Add("user-agent", "OsmMapViewer");
            requestMessage.Headers.Referrer = new Uri("https://www.openstreetmap.org/");
            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK) {
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait();
                string res = tz.Result;
                List<MapObject> resultArr = Utils.ParseObjects("["+ res+"]");
                if(resultArr.Count > 0)
                    return resultArr[0];
                else
                    throw new Exception("Объекты не найдены");

            }
            else
                throw new Exception("Произошла ошибка при обращении к серверу!");
        }
        public static List<RouteItem> GetRoutes(GeoPoint p1,GeoPoint p2) {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $@"{Config.OSRM_HOST}v1/driving/{p1.Longitude.ToString().Replace(",",".")},{p1.Latitude.ToString().Replace(",", ".")};{p2.Longitude.ToString().Replace(",", ".")},{p2.Latitude.ToString().Replace(",", ".")}?alternatives=true&steps=true&geometries=geojson&overview=full&annotations=true");
            requestMessage.Headers.Add("user-agent","OsmMapViewer");
            requestMessage.Headers.Add("user-agent", "OsmMapViewer");
            requestMessage.Headers.Referrer = new Uri("https://www.openstreetmap.org/");
            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK) {
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait();
                string res = tz.Result;
                dynamic data = JObject.Parse(res);
                if(data.code.ToString() != "Ok")
                    throw new Exception("Произошла ошибка сервер вернул код "+ data.code.ToString());
                List<RouteItem> routes = new List<RouteItem>();
                foreach (var route in data.routes){
                    routes.Add(new RouteItem(){
                        DistMetr = (int)Math.Round(Utils.ParseDouble(route.distance.ToString())),
                        DurationSec = (int)Math.Round(Utils.ParseDouble(route.duration.ToString())),
                        Object = Utils.MapItemFromGeoJson(route.geometry.ToString()),
                        DisplayName = "через "+route.legs[0].summary.ToString()
                    });
                }
                return routes;
            }
            else
                throw new Exception("Произошла ошибка при обращении к серверу!");
        }




        //osm API-------------------------------------------------------

        private static readonly HttpClient httpClient = new HttpClient();
        public static string GetOsmData(string type, string osmId) {
            type = type.ToLower().Trim();
            osmId = osmId.Trim();
            if (type != "way" && type != "node" && type != "relation")
                throw new Exception("Неверный тип :"+type);
            if(string.IsNullOrEmpty(osmId))
                throw new Exception("OSM ID не может быть пустым!");

            var task = httpClient.GetAsync(String.Format("{0}api/0.6/{1}/{2}", Config.API_OSM, type, osmId));
            task.Wait(15000);
            if (task.Status == TaskStatus.RanToCompletion){
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait(15000);
                var res = tz.Result;
                if (task.Result.StatusCode == HttpStatusCode.OK){
                    return res;
                }
                else
                    throw new Exception("Ошибка, ответ сервера " + task.Result.StatusCode.ToString() + "\r\n" + res);
            }
            else if (task.Exception != null)
                throw task.Exception;
            else
                throw new Exception("Не удалось получить ответ от сервера");
        }

        public static string CreateChangesetOsm(string comment,string login,string pwd)
        {
            var content = new StringContent($@"
<osm>
    <changeset>
        <tag k=""created_by"" v=""OsmMapViewer"" />
		<tag k=""comment"" v=""{comment}""/>
    </changeset>
</osm>");

            var authenticationString = $"{login}:{pwd}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, String.Format("{0}api/0.6/changeset/create", Config.API_OSM));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = content;
            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion){
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait(20000);
                var res = tz.Result;
                if (task.Result.StatusCode == HttpStatusCode.OK){
                    return res;
                }else
                    throw new Exception("Ошибка, ответ сервера " + task.Result.StatusCode.ToString() + "\r\n" + res);
            }
            else if (task.Exception != null)
                throw task.Exception;
            else
                throw new Exception("Не удалось получить ответ от сервера");
        }
        public static bool CloseChangesetOsm(string changeset,string login,string pwd){
            var authenticationString = $"{login}:{pwd}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, String.Format("{0}api/0.6/changeset/{1}/close", Config.API_OSM, changeset));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            return task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK;
        }
        public static string UpdateOsmData(string xml, string type, string osm_id, string login,string pwd){
            var content = new StringContent(xml, Encoding.UTF8);

            var authenticationString = $"{login}:{pwd}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, String.Format("{0}api/0.6/{1}/{2}", Config.API_OSM, type, osm_id));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = content;


            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion){
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait(20000);
                var res = tz.Result;
                if (task.Result.StatusCode == HttpStatusCode.OK){
                    return res;
                }else
                    throw new Exception("Ошибка, ответ сервера " + task.Result.StatusCode.ToString() + "\r\n" + res);
            }
            else if (task.Exception != null)
                throw task.Exception;
            else
                throw new Exception("Не удалось получить ответ от сервера");
        }
        public static string CreateOsmData(string xml, string type,string login,string pwd){
            var content = new StringContent(xml, Encoding.UTF8);
            var authenticationString = $"{login}:{pwd}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, String.Format("{0}api/0.6/{1}/create", Config.API_OSM, type));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = content;

            var task = httpClient.SendAsync(requestMessage);
            task.Wait(20000);
            if (task.Status == TaskStatus.RanToCompletion){
                var tz = task.Result.Content.ReadAsStringAsync();
                tz.Wait(20000);
                var res = tz.Result;
                if (task.Result.StatusCode == HttpStatusCode.OK){
                    return res;
                }else
                    throw new Exception("Ошибка, ответ сервера " + task.Result.StatusCode.ToString() + "\r\n" + res);
            }
            else if (task.Exception != null)
                throw task.Exception;
            else
                throw new Exception("Не удалось получить ответ от сервера");
        }
        public static bool ChangeHouseNumber(string type,string osm_id, string newNumber) {
            var xml = Utils.GetOsmData(type,osm_id);

            var changeset = Utils.CreateChangesetOsm("Изменение номера дома", Config.login_osm,Config.pwd_osm);

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode node = doc.SelectSingleNode($"//osm/{type}[@id='{osm_id}']");
            if (node != null) {
                XmlElement nodeElement = (XmlElement) node;
                if (nodeElement.Attributes["timestamp"] != null)
                    nodeElement.Attributes.Remove(nodeElement.Attributes["timestamp"]);
                if (nodeElement.Attributes["changeset"] != null)
                    nodeElement.Attributes.Remove(nodeElement.Attributes["changeset"]);
                if (nodeElement.Attributes["user"] != null)
                    nodeElement.Attributes.Remove(nodeElement.Attributes["user"]);
                if (nodeElement.Attributes["uid"] != null)
                    nodeElement.Attributes.Remove(nodeElement.Attributes["uid"]);

                nodeElement.SetAttribute("changeset", changeset);


                XmlElement tag = (XmlElement)node.SelectSingleNode("tag[@k='addr:housenumber']");
                if (tag != null)
                    tag.SetAttribute("v", newNumber);
                else
                {
                    tag = doc.CreateElement("tag");
                    tag.SetAttribute("k", "addr:housenumber");
                    tag.SetAttribute("v", newNumber);
                    node.AppendChild(tag);
                }
                Utils.UpdateOsmData(doc.InnerXml, type,osm_id,Config.login_osm,Config.pwd_osm);
                try{
                    CloseChangesetOsm(changeset, Config.login_osm, Config.pwd_osm);
                }
                catch (Exception e){}

                return true;
            }
            else
                throw new Exception("Не найден элемент "+type);
        }
        public static bool CreateHouse(string addr,string street,string housetype,List<GeoPoint> points){
            var changeset = Utils.CreateChangesetOsm("Добавление дома", Config.login_osm,Config.pwd_osm);

            string nodes = "";
            string firstNode = "";
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                string node = $@"
<osm>
	<node changeset=""{changeset}"" lat=""{p.GetY().ToString().Replace(",",".")}"" lon=""{p.GetX().ToString().Replace(",", ".")}"">
	</node>
</osm>";
                nodes += $"<nd ref=\"{CreateOsmData(node, "node", Config.login_osm, Config.pwd_osm)}\"/>\r\n";
                if (i == 0)
                    firstNode = nodes;
            }

            nodes += firstNode;

            string way = $@"
<osm>
	<way changeset=""{changeset}"">
		<tag k=""addr:housenumber"" v=""{addr}""/>
		<tag k=""addr:street"" v=""{street}""/>
		<tag k=""building"" v=""{housetype}""/>
		{nodes}
	</way>
</osm>";
            CreateOsmData(way, "way", Config.login_osm, Config.pwd_osm);
            try{
                CloseChangesetOsm(changeset, Config.login_osm, Config.pwd_osm);
            }
            catch (Exception e){}

            return true;
        }

        //osm API END-------------------------------------------------------



        //Получить список тайлов для скачивания из зоны, isCrop = true, тайлы на пределами полигона будут пустые
        public static List<Point> GetAreaTileList(MapPolygon area,int zoom, bool isCrop = true){
            var b = area.GetBounds();
            var polygon = area.Points.Select(point => GetPixelFromCoord((GeoPoint)point, zoom)).ToArray();

            var lt = GetTileFromCoord(new GeoPoint(b.Top, b.Left), zoom);
            var rb = GetTileFromCoord(new GeoPoint(b.Bottom, b.Right), zoom, false);

            int xCount = (int)Math.Abs(lt.X - rb.X);
            int yCount = (int)Math.Abs(lt.Y - rb.Y);

            int xStart = (int)Math.Min(lt.X, rb.X);
            int yStart = (int)Math.Min(lt.Y, rb.Y);

            List<Point> res  = new List<Point>();

            for (int x = 0; x < xCount; x++)
                for (int y = 0; y < yCount; y++){
                    if (isCrop) {
                        //var bnd = GetCoordsBoundsFromTile(xStart + x, yStart + y, zoom);
                        Rect boundPixels = new Rect((xStart + x) * 256, (yStart + y) * 256, 256, 256);
                        if (!pointRect(polygon[0].X, polygon[0].Y, boundPixels.Left, boundPixels.Top, boundPixels.Width, boundPixels.Height) && !polygonPoint(polygon, boundPixels.Left, boundPixels.Top) && !polyRect(polygon, boundPixels.Left, boundPixels.Top, boundPixels.Width, boundPixels.Height))
                            continue;
                    }
                    res.Add(new Point(xStart+x,yStart+y));
                }

            return res;
        }

        // POINT/RECTANGLE
        public static bool pointRect(double px, double py, double rx, double ry, double rw, double rh) {
            if (px >= rx &&        // right of the left edge AND
                px <= rx + rw &&   // left of the right edge AND
                py >= ry &&        // below the top AND
                py <= ry + rh)
            {   // above the bottom
                return true;
            }
            return false;
        }
        // POLYGON/RECTANGLE
        public static bool polyRect(Point[] vertices, double rx, double ry, double rw, double rh){
            int next = 0;
            for (int current = 0; current < vertices.Length; current++) {
                next = current + 1;
                if (next == vertices.Length) next = 0;
                Point vc = vertices[current];    // c for "current"
                Point vn = vertices[next];       // n for "next"
                bool collision = lineRect(vc.X, vc.Y, vn.X, vn.Y, rx, ry, rw, rh);
                if (collision) return true;
                bool inside = polygonPoint(vertices, rx, ry);
                if (inside) return true;
            }
            return false;
        }

        // LINE/RECTANGLE
        public static bool lineRect(double x1, double y1, double x2, double y2, double rx, double ry, double rw, double rh){
            bool left = lineLine(x1, y1, x2, y2, rx, ry, rx, ry + rh);
            bool right = lineLine(x1, y1, x2, y2, rx + rw, ry, rx + rw, ry + rh);
            bool top = lineLine(x1, y1, x2, y2, rx, ry, rx + rw, ry);
            bool bottom = lineLine(x1, y1, x2, y2, rx, ry + rh, rx + rw, ry + rh);
            if (left || right || top || bottom)
            {
                return true;
            }
            return false;
        }
        // LINE/LINE
        public static bool lineLine(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4){

            // calculate the direction of the lines
            double uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            double uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return true;
            }
            return false;
        }
        // POLYGON/POINT
        public static bool polygonPoint(Point[] vertices, double px, double py){
            bool collision = false;
            int next = 0;
            for (int current = 0; current < vertices.Length; current++){
                next = current + 1;
                if (next == vertices.Length) next = 0;
                Point vc = vertices[current];    // c for "current"
                Point vn = vertices[next];       // n for "next"
                if (((vc.Y > py && vn.Y < py) || (vc.Y < py && vn.Y > py)) &&
                    (px < (vn.X - vc.X) * (py - vc.Y) / (vn.Y - vc.Y) + vc.X))
                {
                    collision = !collision;
                }
            }
            return collision;
        }
        //Получить координаты тайла из координатов geppoint
        public static Point GetTileFromCoord(GeoPoint gp,int zoom, bool roundFloor = true, int tileSize = 256 ){
            var pixel = GetPixelFromCoord(gp, zoom, tileSize);
            if (roundFloor)
                return new Point(Math.Floor(pixel.X/ tileSize), Math.Floor(pixel.Y/ tileSize));
            else
                return new Point(Math.Ceiling(pixel.X/ tileSize), Math.Ceiling(pixel.Y/ tileSize));
        }
        //Получить координаты пикселя из координатов geppoint
        public static Point GetPixelFromCoord(GeoPoint gp,int zoom, int tileSize = 256 ){
            var sinLatitude = Math.Sin(gp.Latitude * Math.PI / 180);
            var pixelX = ((gp.Longitude + 180) / 360) * tileSize * Math.Pow(2, zoom);
            var pixelY = (0.5f - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)) *tileSize * Math.Pow(2, zoom);

            return new Point(pixelX, pixelY);
        }
        //Получить координаты широта\долгота середины тайла
        public static GeoPoint GetCoordCenterFromTile(int tileX, int tileY, int zoom, int tileSize = 256) {
            var pixel  = new double[]
            {
                tileX * tileSize + tileSize / 2,
                tileY * tileSize + tileSize / 2
            };
            var mapSize = MapSize(zoom, tileSize);
            var x = (Clip(pixel[0], 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixel[1], 0, mapSize - 1) / mapSize);
            return new GeoPoint(
                90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI,  //Latitude
                360 * x   //Longitude
            );
        }
        //Получить bbox тайла
        public static MapBounds GetCoordsBoundsFromTile(int tileX, int tileY, int zoom, int tileSize = 256){
            var pixel  = new double[]{
                tileX * tileSize ,
                tileY * tileSize 
            };
            var mapSize = MapSize(zoom, tileSize);
            var x = (Clip(pixel[0], 0, mapSize - 1) / mapSize) - 0.5;
            var y = 0.5 - (Clip(pixel[1], 0, mapSize - 1) / mapSize);
            var g1 = new GeoPoint(
                90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI,  //Latitude
                360 * x   //Longitude
            );
            pixel  = new double[]{
                tileX * tileSize + tileSize,
                tileY * tileSize + tileSize
            };
            x = (Clip(pixel[0], 0, mapSize - 1) / mapSize) - 0.5;
            y = 0.5 - (Clip(pixel[1], 0, mapSize - 1) / mapSize);
            var g2 = new GeoPoint(
                90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI,  //Latitude
                360 * x   //Longitude
            );
            return new MapBounds(g1, g2);
        }
        
        public static double MapSize(double zoom, int tileSize)
        {
            return Math.Ceiling(tileSize * Math.Pow(2, zoom));
        }
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }
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
        public static MapItem ApplyStrokeSelectedStyle(MapItem item, Brush stroke){
            SetPropertyObject<Brush>(item, "SelectedStroke", stroke);
            return item;
        }
        public static MapItem ApplyStrokeHighlightStyle(MapItem item, Brush stroke){
            SetPropertyObject<Brush>(item, "HighlightStroke", stroke);
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
        public static MapItem ApplyBorderSelectedSize(MapItem item, double size)
        {
            SetPropertyObject<StrokeStyle>(item, "SelectedStrokeStyle", new StrokeStyle()
            {
                Thickness = size
            });
            return item;
        }
        public static MapItem ApplyBorderHighlightSize(MapItem item, double size)
        {
            SetPropertyObject<StrokeStyle>(item, "HighlightStrokeStyle", new StrokeStyle()
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
                if (item.address != null)
                    foreach (var v in item.address)
                        mo.Tags.Add(new TagValue() { Tag = v.Name.ToString(), Key = v.Value.ToString() });
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
