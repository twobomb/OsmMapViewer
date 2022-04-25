using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DevExpress.Map;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.Printing;
using DevExpress.Xpf.Map;
using OsmMapViewer.Dialogs;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;
using OsmMapViewer.ViewModel;

namespace OsmMapViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {

        public ImageLayer imageLayer;
        public MainWindow(){
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            InitializeComponent();

            DataContext = new MainWindowViewModel(this);

            this.Loaded += MainWindow_Loaded;

        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {

            mapControl.MinZoomLevel = 1;
            mapControl.MaxZoomLevel = 18;
            mapControl.ShowSearchPanel = true;
            mapControl.ScrollButtonsOptions = new ScrollButtonsOptions()
            {
                Visible = false
            };
            mapControl.CoordinatesPanelOptions = new CoordinatesPanelOptions()
            {
                Visible = false
            };
            
            mapControl.ScalePanelOptions = new ScalePanelOptions()
            {
                Visible = false
            };
            imageLayer = new ImageLayer();
            mapControl.Layers.Insert(0, imageLayer);
            OpenStreetMapDataProvider provider = new OpenStreetMapDataProvider();

            imageLayer.DataProvider = provider;

            provider.TileUriTemplate = Config.TILE_SERVER_TEMPLATE;
            provider.WebRequest += Provider_WebRequest;

            mapControl.ZoomLevel = 16;
            mapControl.CenterPoint = new GeoPoint( 48.5684458000654, 39.3150812432244);
            mapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            
        }

        private void MapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e){

        }




        private void Provider_WebRequest(object sender, MapWebRequestEventArgs e)
        {
            e.Referer = "https://www.openstreetmap.org/";
            e.UserAgent = "OsmMapViewer";
        }

        //Автоподгрузка элементов по скроллу 
        private void lb_layerItemsBox_ScrollChanged(object sender, ScrollChangedEventArgs e){
            int myOffset = 5;
            if(e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - myOffset){
                (DataContext as MainWindowViewModel).ShowMoreSelectedLayerList();
            }
        }
    }
}
