using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Map;
using DevExpress.Xpf.Map;
using Newtonsoft.Json;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;
using Timer = System.Timers.Timer;

namespace OsmMapViewer.ViewModel
{
    public class MainWindowViewModel: ViewModelBase {


        
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly MainWindow Window;
        //display down text
        private Timer displayDownTextHide = new Timer(5000) {
            AutoReset = false
        };

        public WaitViewModel WaitVMM { get; set; } = new WaitViewModel();
        public MapObject selectedAddress = null;

        public MapObject SelectedAddress {
            get {
                return selectedAddress;
            }
            set{
                if(selectedAddress != null && selectedAddress.Geometry != null)  
                    (SearchResultVector.Data as MapItemStorage).Items.Remove(selectedAddress.Geometry);

                if (value != null && value.Geometry != null) {
                    Utils.ApplyStyle(value.Geometry,
                        new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(60, 255, 0, 0)),
                        18,
                        new StrokeStyle()
                        {
                            Thickness = 3
                        });
                    value.Geometry.EnableSelection = false;
                    (SearchResultVector.Data as MapItemStorage).Items.Add(value.Geometry);
                }

                SetProperty(ref selectedAddress, value);
            }
        }

        public string displayDownText = "";
        public string DisplayDownText {
            get {
                return displayDownText;
            }
            set {
                SetProperty(ref displayDownText, value);
                if(!string.IsNullOrWhiteSpace(displayDownText))
                    displayDownTextHide.Start();
            }
        }
        public Brush displayDownTextColor = Brushes.Red;
        public Brush DisplayDownTextColor {
            get => displayDownTextColor;
            set => SetProperty(ref displayDownTextColor, value);
        }


        public bool searching = false;

        public bool Searching {
            get {
                return searching;
            }
            private set {
                SetProperty(ref searching, value);
            }
        }

        public double CoordPosLon { get; set; }
        public double CoordPosLat { get; set; }

        public void ShowAddresses(string query){
            Window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>{
                SearchText = "";
                Window.cbe_search.IsPopupOpen = false;
            }));
            if(string.IsNullOrWhiteSpace(query) || query == "Введите адрес для поиска...")
                return;
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add(HttpRequestHeader.UserAgent, "OsmMapViewer");
            
            var task = client.DownloadStringTaskAsync(String.Format("{0}{1}?q={2}&format=json&limit=10&accept-language=ru&polygon_geojson=1&extratags=1", Config.NOMINATIM_HOST, Config.NOMINATIM_SEARCH, query));
            
            task.GetAwaiter().OnCompleted(() => {
                if (task.Status == TaskStatus.RanToCompletion){
                    var res = Utils.ParseObjects(task.Result);
                    Window.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                        AddressesResults.Clear();
                        foreach (var a in res)
                            AddressesResults.Add(a);
                    }));
                    Searching = false;
                }
                else {
                    if (task.Exception != null)
                        DisplayDownText = "Произошла ошибка при поиске. " + task.Exception.GetBaseException().Message;
                    Searching = false;
                }

            });
        }
        public void Search(string text, bool openPopup = true){
            if(Searching)
                return;
            Searching = true;
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add(HttpRequestHeader.UserAgent,"OsmMapViewer");
            var task = client.DownloadStringTaskAsync(String.Format("{0}{1}?q={2}&format=json&limit=10&accept-language=ru", Config.NOMINATIM_HOST, Config.NOMINATIM_SEARCH, text));
            task.GetAwaiter().OnCompleted(() => {
                if (task.Status == TaskStatus.RanToCompletion) {
                    var res = Utils.ParseObjects(task.Result);
                    Window.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                        SearchResults.Clear();
                        foreach (var mapObject in res)
                            SearchResults.Add(mapObject);
                        if(openPopup)
                            Window.cbe_search.IsPopupOpen = SearchResults.Count > 0;
                        else
                            Window.cbe_search.IsPopupOpen = false;
                    }));
                    Searching = false;
                }
                else {
                    if (task.Exception != null)
                        DisplayDownText = "Произошла ошибка при поиске. " + task.Exception.GetBaseException().Message;
                    Searching = false;
                }

            });
        }

        public ObservableCollection<MapObject> SearchResults { get; set; } = new ObservableCollection<MapObject>();
        
        public ObservableCollection<MapObject> AddressesResults{ get; set; }

        private Timer searchTimer = new Timer(200)
        {
            AutoReset = false
        };

        public VectorLayer SearchResultVector { get; set; }
        public MainWindowViewModel(MainWindow Window) {
            this.Window = Window;
            searchTimer.Elapsed += (sender, args) => Search(SearchText);
            displayDownTextHide.Elapsed += (sender, args) => { DisplayDownText = ""; };
            SearchResultVector = new VectorLayer();
            SearchResultVector.Data = new MapItemStorage();
            /*Window.mapControl.SelectionChanged += (sender, args) =>
            {
                Console.WriteLine(args.Selection.Count);
            };*/
            Window.mapControl.Layers.Add(SearchResultVector);
            AddressesResults = new ObservableCollection<MapObject>();
            Window.lb_searchBox.PreviewMouseUp += (o, e) =>{
                if (selectedAddress != null && selectedAddress.CenterPoint != null){
                    Window.mapControl.ZoomToRegion(new MapBounds(selectedAddress.BBoxLt, selectedAddress.BBoxRb));
                    Window.mapControl.CenterPoint = selectedAddress.CenterPoint;
                }
            };
            
            
            AddressesResults.CollectionChanged+= (arr,ee)=>{
                switch (ee.Action.ToString())                {
                    case "Add":
                        foreach(var ss in arr as IEnumerable<object>)
                            if (ss is MapObject mo) {
                                //(SearchResultVector.Data as MapItemStorage).Items.Add(mo.Geometry);
                                if (mo.GetType() != typeof(MapDot))
                                    (SearchResultVector.Data as MapItemStorage).Items.Add(mo.MapCenter);
                            }
                        break;
                    case "Reset":
                        SearchResultVector.Data = new MapItemStorage();
                        SelectedAddress = null;
                        break;
                    case "Remove":
                        foreach (var ss in arr as IEnumerable<object>)
                            if (ss is MapObject mo1){
                                 //(SearchResultVector.Data as MapItemStorage).Items.Remove(mo1.Geometry);
                                if (mo1.GetType() != typeof(MapDot))
                                        (SearchResultVector.Data as MapItemStorage).Items.Add(mo1.MapCenter);
                                }
                        break;
                }
                
            };


            Window.cbe_search.KeyUp+= (o, e) => {
                if (e.Key != System.Windows.Input.Key.Enter)
                    return;                
                Task.Run(new Action(()=>{
                    Task.Delay(400).Wait();
                    Window.Dispatcher.BeginInvoke(new Action(()=>
                    {
                        ShowAddresses(SearchText);
                    }));
                }));
            };

        }


        public string searchText = "";
        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value)) {
                    if (searchTimer.Enabled)
                        searchTimer.Stop();

                    if (!string.IsNullOrWhiteSpace(SearchText) && SearchText != "Введите адрес для поиска...")
                        searchTimer.Start();
                    else
                        SearchResults.Clear();
                }
            }
        }

        //что выбирать?
        public bool IsLineChecked { get; set; } = true;
        public bool IsPolygonChecked { get; set; } = true;
        public bool IsPointChecked { get; set; } = true;

        

        private RelayCommand selectFromDicts;
        public RelayCommand SelectFromDicts
        {
            get
            {
                return selectFromDicts ??
                       (selectFromDicts = new RelayCommand(obj =>
                       {
                           Selector sel = new Selector();
                           if (sel.ShowDialog().GetValueOrDefault(false))
                           {
                               var vm = sel.DataContext as SelectorViewModel;
                               var t = vm.CheckedItems;
                               if (t.Count == 0) {
                                   Utils.MsgBoxWarning("Не указано ни одного тега!");
                                   return;
                               }

                               var cancelToken = WaitVMM.ShowWithCancel();
                               
                               var json = JsonConvert.SerializeObject(t);

                               List<MapObject> resultArr = new List<MapObject>();

                               var task= httpClient.PostAsync(string.Format("{0}?line={1}&polygon={2}&point={3}", Config.EXTRA_SEARCH_STRING,IsLineChecked?"1":"0",IsPolygonChecked?"1":"0",IsPointChecked?"1":"0"), new StringContent(json,Encoding.UTF8));
                               task.GetAwaiter().OnCompleted(() => {
                                   if (cancelToken.IsCancellationRequested) {
                                       WaitVMM.WaitVisible = false;
                                       return;
                                   }
                                   if (task.Status == TaskStatus.RanToCompletion) {
                                       Task.Run(() => {
                                           var tz = task.Result.Content.ReadAsStringAsync();
                                           tz.Wait();
                                           string res = tz.Result;
                                           if (string.IsNullOrWhiteSpace(res))
                                           {
                                               Utils.MsgBoxInfo("Не найдено ни одного объекта");
                                               WaitVMM.WaitVisible = false;
                                               return;
                                           }
                                           if (task.Result.StatusCode != HttpStatusCode.OK){
                                               string err = "Произошла на сервере. Код: " +
                                                                     task.Result.StatusCode.ToString() + " Ответ :" + res.ToString();
                                                DisplayDownText = err;
                                               Utils.pushCrashLog(new Exception(err));
                                               WaitVMM.WaitVisible = false;
                                               return;
                                           }
                                           var osm_ids = res.Split(new char[] { ',' }).ToArray();
                                           int limit = 50;

                                           for (int i = 0; i < osm_ids.Length; i += limit) {
                                               if (cancelToken.IsCancellationRequested){
                                                   WaitVMM.WaitVisible = false;
                                                   return;
                                               }
                                               WaitVMM.WaitText = String.Format("Обработано {0} из {1}", i,
                                                           osm_ids.Length);
                                               WebClient client = new WebClient();
                                               client.Encoding = Encoding.UTF8;
                                               client.Headers.Add(HttpRequestHeader.UserAgent, "OsmMapViewer");
                                               var cnt = i + limit > osm_ids.Length
                                                   ? osm_ids.Length % limit
                                                   : limit;
                                               var vls = osm_ids.Skip(i).Take(cnt).ToArray();
                                               var osm_ids_req = string.Join(",", vls);
                                               try{
                                                   var stringRes = client.DownloadString(
                                                       String.Format("{0}{1}?osm_ids={2}&format=json&accept-language=ru&polygon_geojson=1&extratags=1", Config.NOMINATIM_HOST, Config.NOMINATIM_LOOKUP, osm_ids_req));
                                                   Window.Dispatcher.Invoke(new Action(() => {
                                                       try{
                                                        resultArr.AddRange(Utils.ParseObjects(stringRes));

                                                        }
                                                       catch(Exception ex)
                                                       {
                                                           DisplayDownText = "Произошла ошибка при запросе на Nominatim. " + ex.Message;
                                                           Utils.pushCrashLog(ex);
                                                       }
                                                   }));
                                               }
                                               catch (Exception e)
                                               {
                                                   Console.WriteLine(e);
                                                   Utils.pushCrashLog(e);
                                                   //throw e;
                                                   return;
                                               }
                                           }

                                           Window.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                                               WaitVMM.WaitVisible = false;
                                               if (cancelToken.IsCancellationRequested)
                                                   return;
                                               AddressesResults.Clear();
                                               foreach (var a in resultArr)
                                                   AddressesResults.Add(a);
                                           }));
                                       });
                                   }
                                   else {
                                       if (task.Exception != null)
                                           DisplayDownText = "Произошла ошибка запроса " +
                                                             task.Exception.GetBaseException().Message;
                                       else
                                           DisplayDownText = "Произошла ошибка запроса!";
                                       WaitVMM.WaitVisible = false;
                                   }
                               });
                           }
                       }));
            }
        }
        private RelayCommand searchBtn;
        public RelayCommand SearchBtn
        {
            get
            {
                return searchBtn ??
                       (searchBtn = new RelayCommand(obj =>
                       {
                           ShowAddresses(SearchText);

                       }));
            }
        }
        private RelayCommand gotoPoint;
        public RelayCommand GotoPoint
        {
            get
            {
                return gotoPoint ??
                       (gotoPoint = new RelayCommand(obj =>
                       {

                           Window.mapControl.CenterPoint = new GeoPoint(CoordPosLat,CoordPosLon);

                       }));
            }
        }
        private RelayCommand gotoPointReplace;
        public RelayCommand GotoPointReplace
        {
            get
            {
                return gotoPointReplace ??
                       (gotoPointReplace = new RelayCommand(obj =>
                       {
                           double tmp = CoordPosLon;
                           CoordPosLon = CoordPosLat;
                           CoordPosLat = tmp;
                           OnPropertyChanged("CoordPosLon");
                           OnPropertyChanged("CoordPosLat");

                       }));
            }
        }
        private RelayCommand clearResults;
        public RelayCommand ClearResults
        {
            get
            {
                return clearResults ??
                       (clearResults = new RelayCommand(obj =>
                       {
                           AddressesResults.Clear();
                       }));
            }
        }
    }
}
