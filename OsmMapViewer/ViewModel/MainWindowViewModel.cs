using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Drawing.Printing;
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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Map;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Map;
using DevExpress.Xpf.Printing;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json;
using OsmMapViewer.Dialogs;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;

namespace OsmMapViewer.ViewModel
{
    public class MainWindowViewModel: ViewModelBase{


#region Получить координаты R4

        public bool _IsCopyCoordActive = false;

        public bool IsCopyCoordActive
        {
            get
            {
                return _IsCopyCoordActive;
            }
            set {
                if (SetProperty(ref _IsCopyCoordActive, value)){
                    if (value) {
                        IsWhatThisActive = false;
                        IsRulerActive = false;
                        MsgPrinterVM.Info("Кликните на любое место на карте чтобы скопировать широту и долготу в буффер обмена", 8000, "Помощь");
                    }
                        
                }
            }
        }

#endregion
#region Что здесь находится R3

        public bool _IsWhatThisActive = false;

        public bool IsWhatThisActive
        {
            get
            {
                return _IsWhatThisActive;
            }
            set {
                if (SetProperty(ref _IsWhatThisActive, value)){
                    if (value)
                    {
                        IsCopyCoordActive= false;
                        IsRulerActive = false;
                        MsgPrinterVM.Info("Кликните на любое место на карте чтобы получить информацию о ближайших объектах", 8000, "Помощь");
                    }
                }
            }
        }

#endregion

        #region Линейка R2

        public ObservableCollection<GeoPoint> RulerList { get; set; } = new ObservableCollection<GeoPoint>();
        
        public bool _IsRulerActive = false;
        public bool IsRulerActive { 
            get{
                return _IsRulerActive;
            }
            set{
                if(SetProperty(ref _IsRulerActive, value)) {
                    ServiceLayerVector.ShapeTitleOptions = new ShapeTitleOptions()
                    {
                        VisibilityMode = VisibilityMode.Visible
                    };
                    RulerList.Clear();
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    if (value){
                        IsWhatThisActive = false;
                        IsCopyCoordActive= false;
                        MsgPrinterVM.Info("Используйте левую кнопку мыши для установки точек на карте, а правую кнопку мыши для удаления последней точки", 10000, "Помощь");
                    }
                }
            }
        } 

#endregion

        #region  DRAWING R1

        //Рисование
        public bool IsDotDrawing { get; set; }
        public bool IsLineDrawing { get; set; }

        public bool _IsPolygonDrawing = false;
        public bool IsPolygonDrawing
        {
            get
            {
                return _IsPolygonDrawing;
            }
            set
            {
                if (SetProperty(ref _IsPolygonDrawing, value) && value)
                {
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                }
            }
        }
        public bool IsSelectDrawing { get; set; } = true;
        public bool _IsEllipseDrawing = false;
        public bool IsEllipseDrawing
        {
            get
            {
                return _IsEllipseDrawing;
            }
            set
            {
                if (SetProperty(ref _IsEllipseDrawing, value) && value)
                {
                }
            }
        }


        public Decimal RadiusDrawEllipse { get; set; } = 1000;
public bool IsDrawBegin = false;
public GeoPoint DrawBeginDot = null;
public string DrawingNameFigure { get; set; }

public Color _FillDrawing = Color.FromArgb(40, 255, 0, 0);
public Color FillDrawing {
    get
    {
        return _FillDrawing;
    }
    set
    {
        if (SetProperty(ref _FillDrawing, value)){
            if (SelectedMapObject != null && SelectedMapObject.TypeData == "draw") {
                SelectedMapObject.FillGeometry = new SolidColorBrush(value);
                Utils.ApplyFillStyle(SelectedMapObject.Geometry, SelectedMapObject.FillGeometry);
            }
        }
    }
}

public Color _StrokeDrawing = Color.FromArgb(180, 255, 0, 0);
public Color StrokeDrawing {
    get
    {
        return _StrokeDrawing;
    }
    set
    {
        if (SetProperty(ref _StrokeDrawing, value)){
            if (SelectedMapObject != null && SelectedMapObject.TypeData == "draw") {
                SelectedMapObject.StrokeGeometry= new SolidColorBrush(value);
                Utils.ApplyStrokeStyle(SelectedMapObject.Geometry, SelectedMapObject.StrokeGeometry);
            }
        }
    }
}

public Decimal _SizeBorderDrawing = 2;
public Decimal SizeBorderDrawing {
    get
    {
        return _SizeBorderDrawing;
    }
    set {
        if (SetProperty(ref _SizeBorderDrawing, value)){
            if (SelectedMapObject != null && SelectedMapObject.TypeData == "draw") {
                SelectedMapObject.BorderGeometry = (double) value;
                Utils.ApplyBorderSize(SelectedMapObject.Geometry, SelectedMapObject.BorderGeometry);
            }
        }
    }
}

        #endregion

        #region MapObject

        public string SelectedMapObjectTitle { get; set; } = "Нет выбранного объекта";
        //Предыдущий стиль выбраа
        private Brush _prevBrushSelectedMapObject = null;
        private Brush _prevBrushStrokeSelectedMapObject = null;
        //Выбранный объект карты
        private MapObject selectedMapObject = null;
        public MapObject SelectedMapObject{
            get {
                return selectedMapObject;
            }
            set {
                if (_prevBrushSelectedMapObject != null && selectedMapObject != null) {
                    Utils.ApplyFillStyle(selectedMapObject.Geometry, _prevBrushSelectedMapObject);
                    _prevBrushSelectedMapObject = null;
                }

                if (_prevBrushStrokeSelectedMapObject != null && selectedMapObject != null) {
                    Utils.ApplyStrokeStyle(selectedMapObject.Geometry, _prevBrushStrokeSelectedMapObject);
                    _prevBrushStrokeSelectedMapObject = null;
                }

                if (SetProperty(ref selectedMapObject, value)){
                    if (selectedMapObject == null)
                        SelectedMapObjectTitle = "Нет выбранного объекта";
                    else {
                        if (value.TypeData == "draw"){
                            FillDrawing = (Utils.GetFillStyle(value.Geometry) as SolidColorBrush).Color;
                            StrokeDrawing = (Utils.GetStrokeStyle(value.Geometry) as SolidColorBrush).Color;
                            SizeBorderDrawing = (decimal)Utils.GetBorderSize(value.Geometry);
                        }

                        string geomType = "Без геометрии";
                        if (selectedMapObject.Geometry != null){
                            if (selectedMapObject.Geometry is MapPolygon)
                                geomType = "Полигон";
                            else if (selectedMapObject.Geometry is MapPolyline)
                                geomType = "Полилиния";
                            else if (selectedMapObject.Geometry is MapLine)
                                geomType = "Линия";
                            else if (selectedMapObject.Geometry is MapDot)
                                geomType = "Точка";
                            else if (selectedMapObject.Geometry is MapEllipse)
                                geomType = "Элипс";
                            else if (selectedMapObject.Geometry is MapPath)
                                geomType = "Мультиполигон";
                            else
                                geomType = selectedMapObject.Geometry.GetType().Name;
                        }
                        SelectedMapObjectTitle = $"Выбран объект ({geomType}): {selectedMapObject.DisplayNameLabel}";
                    }
                    OnPropertyChanged("SelectedMapObjectTitle");
                }

                if (selectedMapObject != null && selectedMapObject.TypeData != "draw"){
                    _prevBrushSelectedMapObject = Utils.GetFillStyle(selectedMapObject.Geometry);
                    _prevBrushStrokeSelectedMapObject = Utils.GetStrokeStyle(selectedMapObject.Geometry);
                    Utils.ApplyFillStyle(selectedMapObject.Geometry, new SolidColorBrush(Color.FromArgb(70, 0, 255, 64)));
                    Utils.ApplyStrokeStyle(selectedMapObject.Geometry, new SolidColorBrush(Color.FromArgb(255, 0, 255, 64)));
                }

            }
        }
        //Выбранный объект рисования
        private MapObject selectedDrawObject= null;
        public MapObject SelectedDrawObject
        {
            get
            {
                return selectedDrawObject;
            }
            set {
                if (value != null)
                    SelectedMapObject = value;
                else if (SelectedMapObject == selectedDrawObject)
                    SelectedMapObject = null;
                if (SetProperty(ref selectedDrawObject, value) && value.Geometry != null)
                {
                }

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
                        Config.DEFAULT_DOT_SIZE,
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


        public Point __mousedownpos = new Point(0,0);
        //Настройка РВТ радиус вокруг точки
        public double _RadPosLon = 0;
        public double RadPosLon { //x
            get{
                return _RadPosLon;
            }
            set
            {
                try
                {
                    if (SetProperty(ref _RadPosLon, value) && MapPointSelection != null)
                {
                    MapPointSelection.Location = new GeoPoint(RadPosLat, RadPosLon);
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(MapPointSelection);
                    var el = MapEllipse.CreateByCenter(Window.mapControl.CoordinateSystem, MapPointSelection.Location, (double)RadiusMetres * 2f / 1000f, (double)RadiusMetres * 2f / 1000f);
                    el.Fill = new SolidColorBrush(Color.FromArgb(80, 121, 50, 168));
                    el.Stroke = new SolidColorBrush(Color.FromArgb(255, 121, 50, 168));
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(el);

                    }
                }
                catch (Exception e)
                {
                    MsgPrinterVM.Error(e.Message);
                }
            }
        } 

        public double _RadPosLat = 0;//y
        public double RadPosLat{ 
            get{
                return _RadPosLat;
            }
            set
            {
                try
                {
                    if (SetProperty(ref _RadPosLat, value) && MapPointSelection != null)
                    {
                        MapPointSelection.Location = new GeoPoint(RadPosLat, RadPosLon);
                        (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                        (ServiceLayerVector.Data as MapItemStorage).Items.Add(MapPointSelection);
                        var el = MapEllipse.CreateByCenter(Window.mapControl.CoordinateSystem, MapPointSelection.Location, (double)RadiusMetres * 2f / 1000f, (double)RadiusMetres * 2f / 1000f);
                        el.Fill = new SolidColorBrush(Color.FromArgb(80, 121, 50, 168));
                        el.Stroke = new SolidColorBrush(Color.FromArgb(255, 121, 50, 168));
                        (ServiceLayerVector.Data as MapItemStorage).Items.Add(el);
                    }
                }catch(Exception e)
                {
                    MsgPrinterVM.Error(e.Message);
                }
            }
        } 

        public double _RadiusMetres = 1000;
        public double RadiusMetres{ 
            get{
                return _RadiusMetres;
            }
            set
            {
                if (SetProperty(ref _RadiusMetres, value) && MapPointSelection != null){
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(MapPointSelection);
                    var el = MapEllipse.CreateByCenter(Window.mapControl.CoordinateSystem, MapPointSelection.Location, (double)RadiusMetres * 2f / 1000f, (double)RadiusMetres * 2f / 1000f);
                    el.Fill = new SolidColorBrush(Color.FromArgb(80, 121, 50, 168));
                    el.Stroke = new SolidColorBrush(Color.FromArgb(255, 121, 50, 168));
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(el);
                }
            }
        } 

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



        public bool CanRotateMap = false;

        public bool _IsFindAllMap = true;
        public bool IsFindAllMap
        {
            get
            {
                return _IsFindAllMap;
            }
            set
            {
                if (SetProperty(ref _IsFindAllMap, value) && value){
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                }
            }
        }


        public bool _IsFindRect = false;
        public bool IsFindRect{
            get
            {
                return _IsFindRect;
            }
            set
            {
                if(SetProperty(ref _IsFindRect, value) && value)
                {
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    MapPolygonSelection = new MapPolygon()
                    {
                        EnableSelection = false,
                        EnableHighlighting = false,
                        Fill = new SolidColorBrush(Color.FromArgb(80, 3, 32, 252)),
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 3, 32, 252)),
                        Visible = false
                    };
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(MapPolygonSelection);
                    MsgPrinterVM.Info("Удерживайте левый Shift и нажимайте левую кнопку мыши чтобы добавить точку или правую кнопку мыши чтобы удалить точку",10000,"Подсказка");
                }
            }
        }

        public bool _IsFindCircle = false;
        public bool IsFindCircle
        {
            get
            {
                return _IsFindCircle;
            }
            set
            {
                if (SetProperty(ref _IsFindCircle, value) && value){

                    MapPointSelection = new MapDot() { 
                        Location = new GeoPoint(RadPosLat, RadPosLon),
                        EnableSelection = false,
                        EnableHighlighting = false,
                        Size = Config.DEFAULT_DOT_SIZE,
                        Fill = new SolidColorBrush(Color.FromArgb(80, 3, 32, 252)),
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 3, 32, 252))
                    };
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(MapPointSelection);
                    var el = MapEllipse.CreateByCenter(Window.mapControl.CoordinateSystem, MapPointSelection.Location, (double)RadiusMetres * 2f / 1000f, (double)RadiusMetres * 2f / 1000f);
                    el.Fill = new SolidColorBrush(Color.FromArgb(80, 121, 50, 168));
                    el.Stroke = new SolidColorBrush(Color.FromArgb(255, 121, 50, 168));
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(el);
                    MsgPrinterVM.Info("Удерживайте левый Shift и нажимайте левую кнопку мыши чтобы установить точку или используйте 'Настройка РВТ' на панели для ручного ввода параметров", 10000, "Подсказка");
                }
            }
        }


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
        //Выводимые адреса поиска для listbox
        public ObservableCollection<KitSelection> KitSelectionList { get; set; } = new ObservableCollection<KitSelection>(Utils.GetKitSelectionList());
        #endregion


        //Слои
        public VectorLayer SearchResultVector { get; set; }
        public VectorLayer ServiceLayerVector { get; set; }

        public MsgPrinterViewModel MsgPrinterVM { get; set; }
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly MainWindow Window;

        //Зона выделенного полигона
        public MapPolygon MapPolygonSelection;
        //Выделенная точка РВТ
        public MapDot MapPointSelection;

        //Показать больше записей объектов выбранного слоя
        public void ShowMoreSelectedLayerList(){
            if(SelectedLayer != null && SelectedLayer.Objects.Count > SelectedLayerList.Count){
                int showCnt = SelectedLayer.Objects.Count - SelectedLayerList.Count;
                if (showCnt > LIMIT_LOAD_LAYER_OBJECTS)
                    showCnt = LIMIT_LOAD_LAYER_OBJECTS;
                foreach (var v in SelectedLayer.Objects.Skip(SelectedLayerList.Count).Take(showCnt).ToArray())
                    SelectedLayerList.Add(v);
                OnPropertyChanged("ShowLayerObjectsShown");
            }
        }

        public WaitViewModel WaitVMM { get; set; } = new WaitViewModel();

        

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
                        MsgPrinterVM.Error("Произошла ошибка при поиске. " + task.Exception.GetBaseException().Message,10000);
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
                        MsgPrinterVM.Error("Произошла ошибка при поиске. " + task.Exception.GetBaseException().Message, 10000);
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
                if(_SelectedLayer != null && _SelectedLayer != value && _SelectedLayer.TypeData == "draw")
                    _SelectedLayer.Objects.CollectionChanged -= Objects_CollectionChanged;
                if (SetProperty(ref _SelectedLayer, value)){
                    SelectedLayerList.Clear();
                    if (_SelectedLayer != null && _SelectedLayer.TypeData == "draw")
                        _SelectedLayer.Objects.CollectionChanged += Objects_CollectionChanged;
                }
            }
        }


        private void Objects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e){
            SelectedLayerList.Clear();
            ShowMoreSelectedLayerList();
            OnPropertyChanged("ShowLayerObjectsShown");
        }
        //Задержка поиска для autocomplete
        private Timer searchTimer = new Timer(200)
        {
            AutoReset = false
        };


        public MainWindowViewModel(MainWindow Window) {
            this.Window = Window;
            Window.KeyUp += (sender, args) =>
            {
                if(args.Key == Key.Delete)
                    RemoveSelectedLayer.Execute(null);
            };
            MsgPrinterVM = new MsgPrinterViewModel(Window);
            searchTimer.Elapsed += (sender, args) => Search(SearchText);
            //Слой для результатов поиска
            SearchResultVector = new VectorLayer();
            SearchResultVector.Data = new MapItemStorage();
            Window.mapControl.Layers.Add(SearchResultVector);

            Window.mapControl.SelectionMode = ElementSelectionMode.None;
            Window.mapControl.EnableRotation = CanRotateMap;


            RulerList.CollectionChanged += (sender, args) =>{
                (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                MapPolyline mp = new MapPolyline()
                {
                    StrokeStyle = new StrokeStyle()
                    {
                        Thickness = 6
                    },                    
                    Fill= Brushes.Black,
                    Stroke= new SolidColorBrush(Color.FromRgb(252, 152, 3))
                };
                (ServiceLayerVector.Data as MapItemStorage).Items.Add(mp);

                double dist = 0;
                for(var i = 0; i < RulerList.Count; i++){
                    mp.Points.Add(RulerList[i]);

                    MapDot md = new MapDot() {
                        Location = RulerList[i],
                        Size = 15,
                        StrokeStyle = new StrokeStyle() {
                            Thickness = 2
                        },
                        Stroke = Brushes.Black,
                        Fill = new SolidColorBrush(Color.FromRgb(252, 152, 3))
                        };
                    if (i > 0){
                        dist += GeoUtils.CalculateDistance(RulerList[i - 1], RulerList[i], false);
                        string distText = "";
                        if (Math.Floor(dist / 1000f) > 0)
                            distText = $"{Math.Floor(dist / 1000f).ToString()}км {Math.Round(dist % 1000f).ToString()}м";
                        else
                            distText = $"{Math.Round(dist).ToString()}м";
                        md.TitleOptions = new ShapeTitleOptions()
                        {
                            Template = TemplateGenerator.CreateDataTemplate(() =>
                            {
                                var tb = new TextBlock()
                                {
                                    Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                                    Padding = new Thickness(3),
                                    FontSize = 14,
                                    Text = distText,
                                    FontWeight = FontWeight.FromOpenTypeWeight(600),
                                    Margin = new Thickness(0, 0, 0, 35)
                                };
                                return tb;
                            }),
                            VisibilityMode = VisibilityMode.Visible
                        };
                    }
                    (ServiceLayerVector.Data as MapItemStorage).Items.Add(md);
                }
            };
            KitSelectionList.CollectionChanged += (sender, args) =>
            {
                Utils.SetKitSelectionList(KitSelectionList.ToList());
            };

            SelectedLayerList.CollectionChanged += (sender, args) =>{
                OnPropertyChanged("ShowLayerObjectsShown");
            };

            ServiceLayerVector = new VectorLayer();
            ServiceLayerVector.IsHitTestVisible = false;
            ServiceLayerVector.Data = new MapItemStorage();

            Window.mapControl.Layers.Add(ServiceLayerVector);

            Window.RibbonControl.SelectedPageChanged += (sender, args) =>
            {
                IsFindAllMap = true;
                OnPropertyChanged("IsFindAllMap");
                IsSelectDrawing = true;
                OnPropertyChanged("IsSelectDrawing");
                
            };
            //M1
            Window.mapControl.PreviewMouseDown += (s, e) => {
                __mousedownpos = e.GetPosition(Window.mapControl);

                CoordPoint p = Window.mapControl.ScreenPointToCoordPoint(e.GetPosition(Window.mapControl));
                if ((IsEllipseDrawing  || IsPolygonDrawing || IsLineDrawing || IsDotDrawing) && e.LeftButton == MouseButtonState.Pressed){
                    if (SelectedLayer == null || SelectedLayer.TypeData != "draw") {
                        MsgPrinterVM.Warning("Для начала рисования добавьте и выберите слой для рисования!");
                        return;
                    }
                }

                //Рисование Точки
                if (IsDotDrawing && e.LeftButton== MouseButtonState.Pressed) {
                        var me = new MapDot(){
                            Stroke = new SolidColorBrush(StrokeDrawing),
                            Fill = new SolidColorBrush(FillDrawing),
                            Size = Config.DEFAULT_DOT_SIZE,
                            StrokeStyle = new StrokeStyle()
                            {
                                Thickness = (double) SizeBorderDrawing
                            },
                            Location = p,
                            CanMove = false,
                            EnableHighlighting = true,
                            HighlightStroke = Config.SELECT_BRUSH_BORDER,
                            HighlightStrokeStyle = new StrokeStyle()
                            {
                                Thickness = 3
                            },
                            EnableSelection = false
                        };
                        SelectedDrawObject = new MapObject()
                        {
                            StrokeGeometry = new SolidColorBrush(StrokeDrawing),
                            FillGeometry = new SolidColorBrush(FillDrawing),
                            BorderGeometry = (double)SizeBorderDrawing,
                            TypeData = "draw",
                            CenterPoint = (GeoPoint) p,
                            DisplayName = DrawingNameFigure,
                            Geometry = me,
                            RawGeoJson = Utils.GeoJsonFromObject(me)
                };
                        SelectedLayer.Objects.Add(SelectedDrawObject);
                }

                //Рисование линии
                if (IsLineDrawing && e.LeftButton== MouseButtonState.Pressed) {
                    if (!IsDrawBegin) {
                        var me = new MapPolyline()
                        {
                            Stroke = new SolidColorBrush(StrokeDrawing),
                            Fill = new SolidColorBrush(FillDrawing),
                            StrokeStyle = new StrokeStyle()
                            {
                                Thickness = (double) SizeBorderDrawing
                            },
                            CanMove = false,
                            EnableHighlighting = true,
                            HighlightStroke = Config.SELECT_BRUSH_BORDER,
                            HighlightStrokeStyle = new StrokeStyle()
                            {
                                Thickness = 3
                            },
                            EnableSelection = false
                        };
                        IsDrawBegin = true;
                        DrawBeginDot = (GeoPoint) p;
                        me.Points.Add(p);
                        me.Points.Add(p);
                        Window.mapControl.EnableScrolling = false;

                        SelectedDrawObject = new MapObject()
                        {
                            StrokeGeometry = new SolidColorBrush(StrokeDrawing),
                            FillGeometry = new SolidColorBrush(FillDrawing),
                            TypeData = "draw",
                            CenterPoint = (GeoPoint) p,
                            DisplayName = DrawingNameFigure,
                            BorderGeometry = (double)SizeBorderDrawing,
                            Geometry = me
                        };
                        SelectedLayer.Objects.Add(SelectedDrawObject);
                    }
                    else if (SelectedMapObject != null && SelectedMapObject.Geometry is MapPolyline mp){
                        mp.Points.Add(p);
                        DrawBeginDot = (GeoPoint)p;
                        SelectedMapObject.CenterPoint = (GeoPoint)mp.GetCenter();
                    }
                }
                if (IsLineDrawing && IsDrawBegin && e.RightButton == MouseButtonState.Pressed){
                    IsDrawBegin = false;
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    if (SelectedDrawObject.Geometry is MapPolyline pl) {
                        pl.Points.RemoveAt(pl.Points.Count - 1);
                        SelectedDrawObject.CenterPoint = (GeoPoint)pl.GetCenter();
                        if (pl.Points.Count < 2)
                        {
                            MsgPrinterVM.Error("У линии не может быть менее двух точек");
                            SelectedLayer.Objects.Remove(SelectedDrawObject);
                        }
                        else
                            SelectedDrawObject.RawGeoJson = Utils.GeoJsonFromObject(SelectedDrawObject.Geometry);
                    }
                }

                //рисование полигона
                if (IsPolygonDrawing && IsDrawBegin && e.RightButton == MouseButtonState.Pressed){
                    IsDrawBegin = false;
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    if (SelectedDrawObject.Geometry is MapPolygon polygon){
                        polygon.Points.RemoveAt(polygon.Points.Count-1);
                        SelectedDrawObject.CenterPoint = (GeoPoint)polygon.GetCenter();
                        if (polygon.Points.Count < 3) {
                            MsgPrinterVM.Error("У полигона не может быть менее трёх точек");
                            SelectedLayer.Objects.Remove(SelectedDrawObject);
                        }
                        else
                            SelectedDrawObject.RawGeoJson = Utils.GeoJsonFromObject(SelectedDrawObject.Geometry);
                    }
                }

                if (IsPolygonDrawing && e.LeftButton == MouseButtonState.Pressed){
                    if (!IsDrawBegin) {
                        var me = new MapPolygon()
                        {
                            Fill = new SolidColorBrush(FillDrawing),
                            Stroke = new SolidColorBrush(StrokeDrawing),
                            StrokeStyle = new StrokeStyle()
                            {
                                Thickness = (double) SizeBorderDrawing
                            },
                            CanMove = false,
                            EnableHighlighting = true,
                            HighlightStroke = Config.SELECT_BRUSH_BORDER,
                            HighlightStrokeStyle = new StrokeStyle()
                            {
                                Thickness = 3
                            },
                            EnableSelection = false
                        };
                        IsDrawBegin = true;
                        DrawBeginDot = (GeoPoint) p;
                        me.Points.Add(p);
                        Window.mapControl.EnableScrolling = false;


                        SelectedDrawObject = new MapObject()
                        {
                            FillGeometry = new SolidColorBrush(FillDrawing),
                            StrokeGeometry = new SolidColorBrush(StrokeDrawing),
                            TypeData = "draw",
                            CenterPoint = (GeoPoint) p,
                            DisplayName = DrawingNameFigure,
                            BorderGeometry = (double)SizeBorderDrawing,
                            Geometry = me
                        };
                        SelectedLayer.Objects.Add(SelectedDrawObject);
                    }
                    else if (SelectedMapObject != null && SelectedMapObject.Geometry is MapPolygon polygon){
                        polygon.Points.Add(p);
                        if(polygon.Points.Count == 2)
                            polygon.Points.Add(p);
                        DrawBeginDot = (GeoPoint)p;
                        SelectedMapObject.CenterPoint = (GeoPoint)polygon.GetCenter();
                    }
                }
                //рисование окружности
                if (IsEllipseDrawing && e.LeftButton == MouseButtonState.Pressed) {
                        var me = new MapEllipse(){
                        Location = p,
                        Fill = new SolidColorBrush(FillDrawing),
                        Stroke = new SolidColorBrush(StrokeDrawing),
                        StrokeStyle = new StrokeStyle(){
                            Thickness = (double)SizeBorderDrawing
                        },
                        CanMove = false,
                        EnableHighlighting = true,
                        HighlightStroke = Config.SELECT_BRUSH_BORDER,
                        HighlightStrokeStyle = new StrokeStyle()
                        {
                            Thickness = 3
                        }
                    };
                    if (Keyboard.Modifiers == ModifierKeys.Control) {
                        var tmpMe= MapEllipse.CreateByCenter(Window.mapControl.CoordinateSystem, p,
                            (double) RadiusDrawEllipse * 2f / 1000f, (double) RadiusDrawEllipse * 2f / 1000f);
                        me.Location = tmpMe.Location;
                        me.Width= tmpMe.Width;
                        me.Height= tmpMe.Height;
                    }
                    else {
                        IsDrawBegin = true;
                        DrawBeginDot = (GeoPoint)p;
                        Window.mapControl.EnableScrolling = false;
                    }

                    SelectedDrawObject = new MapObject(){
                        FillGeometry = new SolidColorBrush(FillDrawing),
                        StrokeGeometry= new SolidColorBrush(StrokeDrawing),
                        BorderGeometry = (double)SizeBorderDrawing,
                        TypeData= "draw",
                        CenterPoint = (GeoPoint) p,
                        DisplayName = DrawingNameFigure,
                        Geometry = me,
                        RawGeoJson = Utils.GeoJsonFromObject(me)
                };
                    SelectedLayer.Objects.Add(SelectedDrawObject);
                    
                }
              
                
                if (Keyboard.Modifiers == ModifierKeys.Control){
                    CoordPosLon = p.GetX();
                    CoordPosLat = p.GetY();
                    OnPropertyChanged("CoordPosLon");
                    OnPropertyChanged("CoordPosLat");
                }
                if (Keyboard.Modifiers == ModifierKeys.Shift){
                    if (IsFindRect) {
                        Window.mapControl.EnableRotation = false;
                        Window.mapControl.EnableScrolling = false;
                        MapPolygonSelection.Visible = true;
                        var storage = (ServiceLayerVector.Data as MapItemStorage).Items;
                        for (int i = storage.Count - 1; i >= 0; i--)
                            if (storage[i] is MapDot)
                                storage.RemoveAt(i);
                        if (e.ChangedButton == MouseButton.Left)
                            MapPolygonSelection.Points.Add(p);
                        if (e.ChangedButton == MouseButton.Right && MapPolygonSelection.Points.Count > 0)
                            MapPolygonSelection.Points.RemoveAt(MapPolygonSelection.Points.Count - 1);
                        foreach (var pnt in MapPolygonSelection.Points)
                            storage.Add(new MapDot() {
                                Location = pnt,
                                Size = Config.DEFAULT_DOT_SIZE,
                                Fill = new SolidColorBrush(Color.FromArgb(80, 3, 32, 252)),
                                Stroke = new SolidColorBrush(Color.FromArgb(255, 3, 32, 252))
                            });
                    }

                    if (IsFindCircle){
                        RadPosLon = p.GetX();
                        RadPosLat = p.GetY();
                    }
                }
            };
            Window.mapControl.MouseMove += (sender, e) => {
                CoordPoint p = Window.mapControl.ScreenPointToCoordPoint(e.GetPosition(Window.mapControl));

                //Линейка
                if (IsRulerActive && RulerList.Count > 0){
                    RulerList.RemoveAt(RulerList.Count - 1);
                    RulerList.Add(new GeoPoint(p.GetY(), p.GetX()));
                }

                //Динамическое рисование элипса по mousemove
                if (IsEllipseDrawing && e.LeftButton == MouseButtonState.Pressed && IsDrawBegin && SelectedMapObject.Geometry is MapEllipse ellipse){
                    var tmp = MapEllipse.FromLeftTopRightBottom(Window.mapControl.CoordinateSystem, DrawBeginDot, p);
                    SelectedMapObject.CenterPoint = (GeoPoint) tmp.GetCenter();
                    if (tmp.Width> 0)
                        ellipse.Width = tmp.Width;
                    if(tmp.Height > 0)
                        ellipse.Height = tmp.Height;
                    SelectedMapObject.RawGeoJson = Utils.GeoJsonFromObject(SelectedDrawObject.Geometry);
                }
                //Динамическое рисование полигона по Mousemove
                if (IsPolygonDrawing && IsDrawBegin && SelectedMapObject.Geometry is MapPolygon polygon){
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    if (polygon.Points.Count < 2) {
                        var ml = new MapLine()
                        {
                            Point1 = DrawBeginDot,
                            Point2 = p,
                            CanMove = false,
                            CanResize = false,
                            CanRotate = false,
                            EnableSelection = false,
                            EnableHighlighting = false,
                        };
                        (ServiceLayerVector.Data as MapItemStorage).Items.Add(ml);
                        Utils.ApplyStrokeStyle(ml, polygon.Stroke);
                        Utils.ApplyBorderSize(ml, Math.Max(polygon.StrokeStyle.Thickness, 2));
                    }
                    else
                    {
                        polygon.Points.RemoveAt(polygon.Points.Count -1);
                        polygon.Points.Add(p);
                    }
                }
                //Динамическое рисование полигона по Mousemove
                if (IsLineDrawing && IsDrawBegin && SelectedMapObject.Geometry is MapPolyline pl){
                    (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                    pl.Points.RemoveAt(pl.Points.Count -1);
                    pl.Points.Add(p);
                }
            };
            Window.mapControl.MouseLeave+= (s, e) =>{
                Window.mapControl.EnableRotation = CanRotateMap;
                Window.mapControl.EnableScrolling= true;
                if (IsEllipseDrawing) {
                    IsDrawBegin = false;
                    DrawBeginDot = null;
                }
            };
            Window.mapControl.PreviewMouseUp+= (sender, e) =>{
                
                bool isClick = Utils.GetDistance(e.GetPosition(Window.mapControl),__mousedownpos) < 5;
                CoordPoint p = Window.mapControl.ScreenPointToCoordPoint(e.GetPosition(Window.mapControl));
                //Что здесь
                if (IsCopyCoordActive && isClick){
                    var str = (p.GetY() + " " + p.GetX()).Replace(",", ".");
                    Clipboard.SetText(str);
                    MsgPrinterVM.Success($"Координаты точки ({str}) были скопированы в буффер обмена!");
                    IsCopyCoordActive = false;
                }
                //Что здесь
                if (IsWhatThisActive && isClick){
                    string jsonReq = "{\"tags\":[],\"params\":{\"line\":1,\"polygon\":1,\"point\":1}";
                    jsonReq += ",\"restrict_point_radius\":{ \"lon\":" + p.GetX().ToString().Replace(',', '.') +
                                   ",\"lat\":" + p.GetY().ToString().Replace(',', '.') + ",\"radius_meter\":" + 50+
                                   "}";
                    jsonReq += "}";
                    var task = httpClient.PostAsync(Config.GET_DATA, new StringContent(jsonReq, Encoding.UTF8));
                    task.Wait(20000);
                    if (task.Status == TaskStatus.RanToCompletion && task.Result.StatusCode == HttpStatusCode.OK){
                        var tz = task.Result.Content.ReadAsStringAsync();
                        tz.Wait();
                        string res = tz.Result;

                        List<MapObject> resultArr = new List<MapObject>();
                        resultArr.AddRange(Utils.ParseObjects(res));
                        int limit = 50;
                        List<MapObject> detailArr = new List<MapObject>();
                        for (int i = 0; i < resultArr.Count; i += limit)
                        {
                            WebClient client = new WebClient();
                            client.Encoding = Encoding.UTF8;
                            client.Headers.Add(HttpRequestHeader.UserAgent, "OsmMapViewer");
                            var cnt = i + limit > resultArr.Count
                                ? resultArr.Count % limit
                                : limit;
                            var vlsArr = resultArr.Skip(i).Take(cnt).ToArray();
                            ;
                            var osm_ids_req = string.Join(",",
                                vlsArr.Select(s => (s.Type == "node" ? "N" : "W") + s.OsmId).ToArray());
                            var stringRes = client.DownloadString(
                                String.Format(
                                    "{0}{1}?osm_ids={2}&format=json&accept-language=ru&polygon_geojson=1&extratags=1",
                                    Config.NOMINATIM_HOST, Config.NOMINATIM_LOOKUP, osm_ids_req));
                            var px = Utils.ParseObjects(stringRes);
                            foreach (var v in vlsArr)
                            {
                                var f = px.FirstOrDefault(pp => pp.OsmId == v.OsmId);
                                if (f != null)
                                    detailArr.Add(f);
                                else
                                    detailArr.Add(v);
                            }
                        }
                        detailArr.Sort((o, o1) => (string.IsNullOrWhiteSpace(o.DisplayName) && !string.IsNullOrWhiteSpace(o1.DisplayName))?1: (!string.IsNullOrWhiteSpace(o.DisplayName) && string.IsNullOrWhiteSpace(o1.DisplayName))?-1:0);

                        if (detailArr.Count > 0)
                        {
                            LayerData ld = new LayerData()
                            {
                                IsShowPushpin = false,
                                IsShowGeometry = IsShowGeometry
                            };
                            if (string.IsNullOrWhiteSpace(LayerNewName))
                                ld.DisplayName = "Слой 'что здесь?' " + (LayerIndexCreate++);

                            foreach (var a in detailArr)
                                ld.Objects.Add(a);
                            Layers.Add(ld);
                            SelectedTabIndex = 1;
                            MsgPrinterVM.Success($"Поиск завершен, найдено объектов: {detailArr.Count}!");
                        }
                        else
                            MsgPrinterVM.Warning("Не найдено ни одного объекта поблизости!");

                    }
                    else {
                        MsgPrinterVM.Error("Произошла ошибка при обращении к серверу!");
                    }

                    IsWhatThisActive = false;
                }
                //Линейка
                if (IsRulerActive && isClick){
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        if (RulerList.Count == 0)
                        {
                            RulerList.Add(new GeoPoint(p.GetY(), p.GetX()));
                            RulerList.Add(new GeoPoint(p.GetY(), p.GetX()));
                        }
                        else
                            RulerList.Add(new GeoPoint(p.GetY(), p.GetX()));
                    }
                    if (e.ChangedButton == MouseButton.Right)
                    {
                        if (RulerList.Count > 2)
                        {
                            RulerList.RemoveAt(RulerList.Count - 1);
                            RulerList.RemoveAt(RulerList.Count - 1);
                            RulerList.Add(new GeoPoint(p.GetY(), p.GetX()));
                        }
                        else
                            RulerList.Clear();
                    }
                }

                Window.mapControl.EnableRotation = CanRotateMap;
                Window.mapControl.EnableScrolling= true;
                if (IsEllipseDrawing) {
                    IsDrawBegin = false;
                    DrawBeginDot = null;
                }
            };
            Layers.CollectionChanged += Layers_CollectionChanged;
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
                        foreach(var ss in ee.NewItems)
                            if (ss is MapObject mo) {
                                mo.TypeData = "search";
                                //(SearchResultVector.Data as MapItemStorage).Items.Add(mo.Geometry);
                                (SearchResultVector.Data as MapItemStorage).Items.Add(mo.MapCenter);
                            }
                        break;
                    case "Reset":
                        SearchResultVector.Data = new MapItemStorage();
                        SelectedAddress = null;
                        break;
                    case "Remove":
                        foreach (var ss in ee.OldItems)
                            if (ss is MapObject mo1){
                                 //(SearchResultVector.Data as MapItemStorage).Items.Remove(mo1.Geometry);
                                (SearchResultVector.Data as MapItemStorage).Items.Remove(mo1.MapCenter);
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
                    {
                        var ld = (LayerData) eNewItem;
                        ld.ClickDrawObject += o =>
                        {
                            if(!IsSelectDrawing)
                                return;
                            SelectedLayer = ld;
                            SelectedDrawObject = o;
                            Window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                Window.mapControl.ZoomToRegion(new MapBounds(o.BBoxLt, o.BBoxRb));
                                Window.mapControl.CenterPoint = o.CenterPoint;
                            }));
                        };
                        Window.mapControl.Layers.Add(ld);
                    }

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
            string jsonReq = "{\"tags\":" + json+",\"params\":";
            jsonReq += "{" +string.Format("\"line\":{0},\"polygon\":{1},\"point\":{2}", IsLineChecked ? "1" : "0", IsPolygonChecked ? "1" : "0", IsPointChecked ? "1" : "0")+"}";
            if (IsFindRect){
                if (MapPolygonSelection.Points.Count < 3) {
                    MsgPrinterVM.Error("Ошибка, зона выборки по полигону. В полигоне не может быть менее трёх точек!", 10000);
                    WaitVMM.WaitVisible = false;
                    return;
                }
                jsonReq += ",\"restrict_polygon\":\""+ Utils.PointToTextGeometry(MapPolygonSelection.Points.ToList())+"\"";
            }

            if (IsFindCircle) {
                jsonReq += ",\"restrict_point_radius\":{ \"lon\":" + RadPosLon.ToString().Replace(',', '.') +
                           ",\"lat\":" + RadPosLat.ToString().Replace(',', '.') + ",\"radius_meter\":" + RadiusMetres +
                           "}";
            }
            jsonReq += "}";
            var task = httpClient.PostAsync(string.Format("{0}", Config.GET_DATA), new StringContent(jsonReq, Encoding.UTF8));
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
                        if (task.Result.StatusCode != HttpStatusCode.OK) {
                            string err = "Произошла на сервере. Код: " +
                                                  task.Result.StatusCode.ToString() + " Ответ :" + res.ToString();

                            MsgPrinterVM.Error(err, 10000);
                            Utils.pushCrashLog(new Exception(err));
                            WaitVMM.WaitVisible = false;
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(res))
                        {

                            MsgPrinterVM.Warning("Не найдено ни одного объекта", 5000);
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
                                MsgPrinterVM.Error("Произошла ошибка при обработке запроса!\r\nОшибка: " + ex.Message+ "\r\n\r\nОтвет сервера: "+res, 10000);
                                Utils.pushCrashLog(ex);
                            }

                        }));
                        if (IsDetailSearch){//Поиск с nominatim
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

                                            MsgPrinterVM.Error("Произошла ошибка при обработке запроса Nominatim. " + ex.Message, 10000);
                                            Utils.pushCrashLog(ex);
                                        }
                                    }));
                                }
                                catch (Exception e){
                                    MsgPrinterVM.Error("Произошла ошибка при запросе на Nominatim. " + e.Message, 10000);
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

                            SelectedTabIndex = 1;
                        }));
                    });
                }
                else
                {
                    if (task.Exception != null)

                        MsgPrinterVM.Error("Произошла ошибка запроса " +
                                          task.Exception.GetBaseException().Message, 10000);
                    else
                        MsgPrinterVM.Error("Произошла ошибка запроса!", 10000);
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
        private RelayCommand selectFromUserTags;
        public RelayCommand SelectFromUserTags{
            get{
                return selectFromUserTags ??
                       (selectFromUserTags = new RelayCommand(obj =>{
                           AddUserTags aut = new AddUserTags();
                           if (aut.ShowDialog().GetValueOrDefault(false)){
                               var vm = aut.DataContext as AddUserTagsViewModel;
                               var t = vm.ItemsStringArray;
                               if (t.Count == 0) {
                                   MsgPrinterVM.Warning("Не указано ни одного тега!", 5000);
                                   return;
                               }

                               var json = JsonConvert.SerializeObject(t);
                               SearchObjects(json);
                           }
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
                                   MsgPrinterVM.Warning("Не указано ни одного тега!", 5000);
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

                           Window.mapControl.ZoomLevel = Window.mapControl.MaxZoomLevel;
                           Window.mapControl.CenterPoint = new GeoPoint(CoordPosLat,CoordPosLon);
                           (ServiceLayerVector.Data as MapItemStorage).Items.Clear();
                           (ServiceLayerVector.Data as MapItemStorage).Items.Add(new MapPushpin()
                           {
                               Location = Window.mapControl.CenterPoint,
                               EnableSelection = false,
                               Brush = new SolidColorBrush(Color.FromArgb(128, 252, 3, 86))
                           });

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
                           if(Utils.MsgBoxQuestion("Вы уверены что хотите удалить слой '"+((LayerData)obj).DisplayName +"'?","Удалить слой?") == MessageBoxResult.Yes)
                               Layers.Remove((LayerData) obj);
                       }));
            }
        }
        private RelayCommand openSaveLayers;
        public RelayCommand OpenSaveLayers
        {
            get
            {
                return openSaveLayers ??
                       (openSaveLayers = new RelayCommand(obj =>
                       {
                           ShowLayersDlg d = new ShowLayersDlg();
                           d.Owner = Window;
                           if (d.ShowDialog().GetValueOrDefault(false))
                           {
                               Layers.Add(d.LayerToLoad);
                           }
                       }));
            }
        }
        private RelayCommand addDrawingLayer;
        public RelayCommand AddDrawingLayer
        {
            get
            {
                return addDrawingLayer ??
                       (addDrawingLayer = new RelayCommand(obj => {
                           LayerData ld = new LayerData() {
                               IsShowPushpin = false,
                               IsShowGeometry = IsShowGeometry,
                               TypeData = "draw"
                           };
                            ld.DisplayName = "Слой рисования" + (LayerIndexCreate++);

                           Layers.Add(ld);
                           SelectedTabIndex = 1;
                           SelectedLayer = ld;
                           MsgPrinterVM.Success("Слой рисования добавлен и выбран!");
                       }));
            }
        }
        private RelayCommand saveLayer;
        public RelayCommand SaveLayer
        {
            get
            {
                return saveLayer ??
                       (saveLayer = new RelayCommand(obj =>
                       {
                           var layer = (LayerData) obj;
                           var ls = Utils.LoadLayers();
                           bool isAny = ls.Any(data => data.ID == layer.ID);
                           ls.RemoveAll(data => data.ID == layer.ID);
                           ls.Add(layer);
                           Utils.SaveLayers(ls);
                           if(isAny)
                            MsgPrinterVM.Success("Слой был перезаписан!");
                           else
                            MsgPrinterVM.Success("Слой был сохранен!");
                       }));
            }
        }
        private RelayCommand deleteKit;

        public RelayCommand DeleteKit
        {
            get
            {
                return deleteKit ??
                       (deleteKit = new RelayCommand(obj => {
                           if(obj != null && obj is KitSelection ks) {
                               if (Utils.MsgBoxQuestion("Вы уверены что хотите удалить набор '" + ks.Name + "'?") ==
                                   MessageBoxResult.Yes)
                               {
                                   KitSelectionList.Remove(ks);
                                   MsgPrinterVM.Success("Набор '"+ks.Name+"' удален!");
                               }
                           }
                       }));
            }
        }

        private RelayCommand selectKit;

        public RelayCommand SelectKit
        {
            get
            {
                return selectKit ??
                       (selectKit = new RelayCommand(obj =>
                       {
                           if(obj != null && obj is KitSelection ks)
                            SearchObjects(ks.Json);
                       }));
            }
        }
        private RelayCommand focusOnSelectedMapObject;
        public RelayCommand FocusOnSelectedMapObject {
            get {
                return focusOnSelectedMapObject ??
                       (focusOnSelectedMapObject = new RelayCommand(obj => {
                           if (SelectedMapObject != null && SelectedMapObject.CenterPoint != null) {
                               Window.mapControl.ZoomToRegion(new MapBounds(SelectedMapObject.BBoxLt, SelectedMapObject.BBoxRb));
                               Window.mapControl.CenterPoint = SelectedMapObject.CenterPoint;
                           }
                           else
                           {
                               MsgPrinterVM.Warning("Нет выбранного объекта");
                           }
                       }));
            }
        }

        private RelayCommand addKit;
        public RelayCommand AddKit
        {
            get
            {
                return addKit ??
                       (addKit = new RelayCommand(obj =>
                       {
                           AddKitSelectionDlg dlg = new AddKitSelectionDlg();
                           dlg.Owner = Window;
                           if (dlg.ShowDialog().GetValueOrDefault(false))
                           {
                               int id = 0;
                               foreach (var kitSelection in KitSelectionList)
                                   if (kitSelection.ID >= id)
                                       id = kitSelection.ID + 1;
                               KitSelection ks = new KitSelection()
                               {
                                   ID = id,
                                   Name = dlg.NameLb,
                                   Json = dlg.Json
                               };
                               KitSelectionList.Add(ks);
                           }
                       }));
            }
        }
        private RelayCommand removeSelectedLayer;
        public RelayCommand RemoveSelectedLayer {
            get {
                return removeSelectedLayer ??
                       (removeSelectedLayer = new RelayCommand(obj => {
                           if (SelectedMapObject != null)
                           {
                               if (SelectedMapObject.TypeData == "search")
                               {
                                   AddressesResults.Remove(SelectedMapObject);
                                   SelectedLayerList.Remove(SelectedMapObject);
                                   SelectedMapObject = null;
                               }else if (SelectedMapObject.Layer == null)
                                   MsgPrinterVM.Error("Не удалось найти слой привязки");
                               else
                               {
                                   SelectedMapObject.Layer.Objects.Remove(SelectedMapObject);
                                   SelectedLayerList.Remove(SelectedMapObject);
                                   SelectedMapObject = null;
                               }
                           }
                       }));
            }
        }
        private RelayCommand printMap;
        public RelayCommand PrintMap {
            get {
                return printMap ??
                       (printMap = new RelayCommand(obj => {
                         PrintableControlLink link = new PrintableControlLink(Window.mapControl);
                         link.Landscape = true;
                         link.PaperKind = PaperKind.A4;
                         link.ShowRibbonPrintPreviewDialog(Window);
                       }));
            }
        }
        private RelayCommand layerToExcel;
        public RelayCommand LayerToExcel {
            get {
                return layerToExcel ??
                       (layerToExcel = new RelayCommand(obj => {
                           if (obj is LayerData ld) {
                               ReportLayer rl = new ReportLayer();

                               rl.DataSource = ld.Objects.Select(o => new ReportLayerModel()
                               {
                                   DisplayName = o.DisplayNameLabel,
                                   Latitude = o.CenterPoint == null ? 0 : o.CenterPoint.Latitude,
                                   Longitude = o.CenterPoint == null ? 0 : o.CenterPoint.Longitude
                               });
                               PrintHelper.ShowRibbonPrintPreviewDialog(Window, rl);
                           }
                       }));
            }
        }

#endregion
    }
}
