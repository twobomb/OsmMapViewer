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
    public class MainWindowViewModel: ViewModelBase{
        #region MapObject

        //Выбранный объект карты
        private MapObject selectedMapObject = null;
        public MapObject SelectedMapObject
        {
            get
            {
                return selectedMapObject;
            }
            set
            {

                SetProperty(ref selectedMapObject, value);
            }
        }
        //Выбранный объект из списка объектов слоёв
        private MapObject selectedLayerObject = null;
        public MapObject SelectedLayerObject
        {
            get
            {
                return selectedLayerObject;
            }
            set
            {
                if (value != null)
                    SelectedMapObject = value;
                else if (SelectedMapObject == selectedLayerObject)
                    SelectedMapObject = null;
                SetProperty(ref selectedLayerObject, value);
            }
        }
        //Выбранный адрес из списка
        private MapObject selectedAddress = null;
        public MapObject SelectedAddress
        {
            get
            {
                return selectedAddress;
            }
            set
            {
                if (value != null)
                    SelectedMapObject = value;
                else if (SelectedMapObject == selectedAddress)
                    SelectedMapObject = null;

                if (selectedAddress != null && selectedAddress.Geometry != null)
                    (SearchResultVector.Data as MapItemStorage).Items.Remove(selectedAddress.Geometry);

                if (value != null && value.Geometry != null)
                {
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

        #endregion
        #region int,double


        //Координаты панели позиция
        public double CoordPosLon { get; set; }
        public double CoordPosLat { get; set; }


        //По сколько объектов отображать подробности слоёв
        public int LIMIT_LOAD_LAYER_OBJECTS = 100;

        //При создании слоя делает приписку
        public int LayerIndexCreate = 1;

        //идекс выбранной вкладки
        private int _SelectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get
            {
                return _SelectedTabIndex;
            }
            set
            {
                SetProperty(ref _SelectedTabIndex, value);
            }
        }
        #endregion

        #region bools

        //Настройки нового слоя
        public bool IsShowPushpin { get; set; } = true;
        public bool IsShowGeometry{ get; set; } = true;
        
        //Показывать список содержимого слоя
        private bool _IsShowLayerObjectList= false;
        public bool IsShowLayerObjectList
        {
            get
            {
                return _IsShowLayerObjectList;
            }
            set
            {
                if (!SetProperty(ref _IsShowLayerObjectList, value))
                    return;
                if (!value)
                    SelectedLayerList.Clear();
                else if(SelectedLayer != null)
                    ShowMoreSelectedLayerList();

                OnPropertyChanged("ShowLayerObjectsShown");

            }
        }


        //Отображение панели с тегами
        private  bool _IsTagsPanel = false;
        public bool IsTagsPanel
        {
            get
            {
                return _IsTagsPanel;
            }
            set
            {
                SetProperty(ref _IsTagsPanel, value);
            }
        }

        //Нужен ли детальный поиск с использованием nominatim
        public bool IsDetailSearch { get; set; }

        //Идёт ли поиск для autocomplete
        private bool searching = false;
        public bool Searching
        {
            get
            {
                return searching;
            }
            private set
            {
                SetProperty(ref searching, value);
            }
        }
        //что выбирать?
        public bool IsLineChecked { get; set; } = true;
        public bool IsPolygonChecked { get; set; } = true;
        public bool IsPointChecked { get; set; } = true;

        #endregion

        #region stringvars

        //Имя нового слоя
        public string LayerNewName { get; set; }
        //Лейбл сколько объектов показано
        public string ShowLayerObjectsShown
        {
            get {
                if (SelectedLayer == null)
                    return "";
                return String.Format("Отображено объектов {0}/{1}",SelectedLayerList.Count,SelectedLayer.Objects.Count);
            }
        }

        public string searchText = "";
        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    if (searchTimer.Enabled)
                        searchTimer.Stop();

                    if (!string.IsNullOrWhiteSpace(SearchText) && SearchText != "Введите адрес для поиска...")
                        searchTimer.Start();
                    else
                        SearchResults.Clear();
                }
            }
        }
        public string displayDownText = "";
        public string DisplayDownText
        {
            get
            {
                return displayDownText;
            }
            set
            {
                SetProperty(ref displayDownText, value);
                if (!string.IsNullOrWhiteSpace(displayDownText))
                    displayDownTextHide.Start();
            }
        }
        #endregion

#region Collections

        //Выводимые объекты выбранного слоя
        public ObservableCollection<MapObject> SelectedLayerList { get; set; } = new ObservableCollection<MapObject>();
        //Пользовательские слои
        public ObservableCollection<LayerData> Layers { get; set; } = new ObservableCollection<LayerData>();
        //Результаты поиска для autocomplete
        public ObservableCollection<MapObject> SearchResults { get; set; } = new ObservableCollection<MapObject>();
        //Выводимые адреса поиска для listbox
        public ObservableCollection<MapObject> AddressesResults { get; set; }
        #endregion


        public MsgPrinterViewModel MsgPrinterVM { get; set; } = new MsgPrinterViewModel();
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly MainWindow Window;
        //display down text
        private Timer displayDownTextHide = new Timer(10000) {
            AutoReset = false
        };


        //Показать больше записей объектов выбранного слоя
        public void ShowMoreSelectedLayerList(){
            if (SelectedLayer != null && SelectedLayer.Objects.Count > SelectedLayerList.Count){
                int showCnt = SelectedLayer.Objects.Count - SelectedLayerList.Count;
                if (showCnt > LIMIT_LOAD_LAYER_OBJECTS)
                    showCnt = LIMIT_LOAD_LAYER_OBJECTS;
                foreach (var v in SelectedLayer.Objects.Skip(SelectedLayerList.Count).Take(showCnt).ToArray())
                    SelectedLayerList.Add(v);
                OnPropertyChanged("ShowLayerObjectsShown");
            }
        }

        public WaitViewModel WaitVMM { get; set; } = new WaitViewModel();

        
        public Brush displayDownTextColor = Brushes.Red;
        public Brush DisplayDownTextColor {
            get => displayDownTextColor;
            set => SetProperty(ref displayDownTextColor, value);
        }


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


        //Выбранный слой
        public LayerData _SelectedLayer = null;
        public LayerData SelectedLayer { 
            get{
                return _SelectedLayer;
            }
            set{
                if(SetProperty(ref _SelectedLayer, value)){
                    SelectedLayerList.Clear();
                }
            }
        }



        //Задержка поиска для autocomplete
        private Timer searchTimer = new Timer(200)
        {
            AutoReset = false
        };

        //Слой поиска по адресам
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
            Layers.CollectionChanged += Layers_CollectionChanged;
            Window.mapControl.Layers.Add(SearchResultVector);
            AddressesResults = new ObservableCollection<MapObject>();
            //Камеру на выбранный адрес
            Window.lb_searchBox.PreviewMouseUp += (o, e) =>{                
                if (SelectedAddress != null && SelectedAddress.CenterPoint != null){
                    Window.mapControl.ZoomToRegion(new MapBounds(SelectedAddress.BBoxLt, SelectedAddress.BBoxRb));
                    Window.mapControl.CenterPoint = SelectedAddress.CenterPoint;
                }
            };
            //Камеру на выбранный объект слоя
            Window.lb_layerItemsBox.PreviewMouseUp += (o, e) =>{                
                if (SelectedLayerObject != null && SelectedLayerObject.CenterPoint != null){
                    Window.mapControl.ZoomToRegion(new MapBounds(SelectedLayerObject.BBoxLt, SelectedLayerObject.BBoxRb));
                    Window.mapControl.CenterPoint = SelectedLayerObject.CenterPoint;
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

        private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action.ToString())
            {
                case "Add":
                    foreach (var eNewItem in e.NewItems)
                        Window.mapControl.Layers.Add((LayerData) eNewItem);
                    break;
                case "Reset":
                    foreach (var eOldItem in e.OldItems)
                        Window.mapControl.Layers.Remove((LayerData)eOldItem);
                    break;
                case "Remove":
                    foreach (var eOldItem in e.OldItems)
                        Window.mapControl.Layers.Remove((LayerData)eOldItem);
                    break;
            }
        }



public void SearchObjects(string json){

            var cancelToken = WaitVMM.ShowWithCancel();
            List<MapObject> resultArr = new List<MapObject>();

            var task = httpClient.PostAsync(string.Format("{0}?line={1}&polygon={2}&point={3}", Config.GET_DATA, IsLineChecked ? "1" : "0", IsPolygonChecked ? "1" : "0", IsPointChecked ? "1" : "0"), new StringContent(json, Encoding.UTF8));
            task.GetAwaiter().OnCompleted(() => {
                if (cancelToken.IsCancellationRequested)
                {
                    WaitVMM.WaitVisible = false;
                    return;
                }
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Task.Run(() => {
                        var tz = task.Result.Content.ReadAsStringAsync();
                        tz.Wait();
                        string res = tz.Result;
                        if (task.Result.StatusCode != HttpStatusCode.OK)
                        {
                            string err = "Произошла на сервере. Код: " +
                                                  task.Result.StatusCode.ToString() + " Ответ :" + res.ToString();
                            DisplayDownText = err;
                            Utils.pushCrashLog(new Exception(err));
                            WaitVMM.WaitVisible = false;
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(res))
                        {
                            Utils.MsgBoxInfo("Не найдено ни одного объекта");
                            WaitVMM.WaitVisible = false;
                            return;
                        }

                        Window.Dispatcher.Invoke(new Action(() => {
                            try
                            {
                                resultArr.AddRange(Utils.ParseObjects(res));
                            }
                            catch (Exception ex)
                            {
                                DisplayDownText = "Произошла ошибка при обработке запроса  " + ex.Message;
                                Utils.pushCrashLog(ex);
                            }

                        }));
                        if (IsDetailSearch)
                        {//Поиск с nominatim
                            int limit = 50;
                            List<MapObject> detailArr = new List<MapObject>();
                            for (int i = 0; i < resultArr.Count; i += limit)
                            {
                                if (cancelToken.IsCancellationRequested)
                                {
                                    WaitVMM.WaitVisible = false;
                                    return;
                                }
                                WaitVMM.WaitText = String.Format("Запрос детальной информации. Обработано {0} из {1}", i,
                                           resultArr.Count);
                                WebClient client = new WebClient();
                                client.Encoding = Encoding.UTF8;
                                client.Headers.Add(HttpRequestHeader.UserAgent, "OsmMapViewer");
                                var cnt = i + limit > resultArr.Count
                                    ? resultArr.Count % limit
                                    : limit;
                                var vlsArr = resultArr.Skip(i).Take(cnt).ToArray(); ;
                                var osm_ids_req = string.Join(",", vlsArr.Select(s => (s.Type == "node" ? "N" : "W")+s.OsmId).ToArray());
                                try
                                {
                                    var stringRes = client.DownloadString(
                                        String.Format("{0}{1}?osm_ids={2}&format=json&accept-language=ru&polygon_geojson=1&extratags=1", Config.NOMINATIM_HOST, Config.NOMINATIM_LOOKUP, osm_ids_req));
                                    Window.Dispatcher.Invoke(new Action(() => {
                                        try
                                        {
                                            var p = Utils.ParseObjects(stringRes);
                                            foreach (var v in vlsArr)
                                            {
                                                var f = p.FirstOrDefault(pp => pp.OsmId == v.OsmId);
                                                if (f != null)
                                                    detailArr.Add(f);
                                                else
                                                    detailArr.Add(v);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            DisplayDownText = "Произошла ошибка при обработке запроса Nominatim. " + ex.Message;
                                            Utils.pushCrashLog(ex);
                                        }
                                    }));
                                }
                                catch (Exception e)
                                {
                                    DisplayDownText = "Произошла ошибка при запросе на Nominatim. " + e.Message;
                                    Utils.pushCrashLog(e);
                                    WaitVMM.WaitVisible = false;
                                    //throw e;
                                    return;
                                }
                            }
                            resultArr = detailArr;
                        }

                        Window.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                            WaitVMM.WaitVisible = false;
                            if (cancelToken.IsCancellationRequested)
                                return;
                            LayerData ld = new LayerData(){
                                IsShowPushpin = IsShowPushpin,
                                IsShowGeometry = IsShowGeometry
                            };
                            if(string.IsNullOrWhiteSpace(LayerNewName))
                                ld.DisplayName = "Слой " + (LayerIndexCreate++);
                            else
                                ld.DisplayName = LayerNewName;

                            foreach (var a in resultArr)
                                ld.Objects.Add(a);
                            Layers.Add(ld);
                            //SelectedLayer = ld;
                            SelectedTabIndex = 1;

                            /*AddressesResults.Clear();
                            foreach (var a in resultArr)
                                AddressesResults.Add(a);*/
                        }));
                    });
                }
                else
                {
                    if (task.Exception != null)
                        DisplayDownText = "Произошла ошибка запроса " +
                                          task.Exception.GetBaseException().Message;
                    else
                        DisplayDownText = "Произошла ошибка запроса!";
                    WaitVMM.WaitVisible = false;
                }
            });
        }

#region Commands
        private RelayCommand religion;
        public RelayCommand Religion
        {
            get
            {
                return religion ??
                       (religion = new RelayCommand(obj =>
                       {
                               SearchObjects("{\"building\":[\"cathedral\",\"chapel\",\"church\",\"kingdom_hall\",\"monastery\",\"mosque\",\"presbytery\",\"religious\",\"shrine\",\"synagogue\",\"temple\"]}");
                       }));
            }
        }
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

                               var json = JsonConvert.SerializeObject(t);
                               SearchObjects(json);
                           }
                       }));
            }
        }
        /* public RelayCommand SelectFromDicts
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
                                                           DisplayDownText = "Произошла ошибка при обработке запроса Nominatim. " + ex.Message;
                                                           Utils.pushCrashLog(ex);
                                                       }
                                                   }));
                                               }
                                               catch (Exception e) {
                                                   DisplayDownText = "Произошла ошибка при запросе на Nominatim. " + e.Message;
                                                   Utils.pushCrashLog(e);
                                                   WaitVMM.WaitVisible = false;
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
        }*/
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
        private RelayCommand _ShowHideLayersList;
        public RelayCommand ShowHideLayersList
        {
            get
            {
                return _ShowHideLayersList ??
                       (_ShowHideLayersList = new RelayCommand(obj =>
                       {
                           if (!IsShowLayerObjectList && obj != null)
                               SelectedLayer = obj as LayerData;
                           IsShowLayerObjectList = !IsShowLayerObjectList;

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
                           MsgPrinterVM.Warning("что лалалалал","eeeeeeeeeeeeeeeeeeeeee eeeeeeeeeeeee eeeeeeeeeeeeeee eeeeeeeeeeee");
                           MsgPrinterVM.Error("что лалалалал1 213313 12 3 21");
                           MsgPrinterVM.Info("Hello world!");
                           MsgPrinterVM.Success("!!!Hello world!");
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
        private RelayCommand deleteLayer;
        public RelayCommand DeleteLayer
        {
            get
            {
                return deleteLayer ??
                       (deleteLayer = new RelayCommand(obj =>
                       {
                           Layers.Remove((LayerData) obj);
                       }));
            }
        }

#endregion
    }
}
