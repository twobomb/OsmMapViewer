using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using DevExpress.Map;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Map;
using OsmMapViewer.Misc;
using OsmMapViewer.ViewModel;

namespace OsmMapViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {

        public MainWindow()
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            InitializeComponent();
            DataContext = new MainWindowViewModel(this);

            this.Loaded += MainWindow_Loaded;
            VectorLayer vl = new VectorLayer();
            var mp = new MapPolygon();
            foreach (var g in new GeoPoint[]{
                new GeoPoint(47.000751662342765, 28.782015716126669),
                new GeoPoint(47.003499105949331, 28.779784489235578),
                new GeoPoint(47.005060446204077, 28.776670571833844),
                new GeoPoint(47.002626050570065, 28.774355254904908),
                new GeoPoint(47.000264065129222, 28.779487220183803),
                new GeoPoint(47.000751662342765, 28.782015716126669)
            })
                mp.Points.Add(g);

            mp.SelectedFill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            var z = new MapItemStorage();
            //z.Items.Add(mp);
            vl.Data = z;

            mapControl.Layers.Add(vl);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

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
            
            
            
            var layer = new ImageLayer();
            mapControl.Layers.Insert(0,layer);
            

            OpenStreetMapDataProvider provider = new OpenStreetMapDataProvider();
            layer.DataProvider = provider;

            
            
            


            //provider.TileUriTemplate = "http://10.113.0.183/{1}/{2}/{3}.png";
            provider.TileUriTemplate = "https://tile.openstreetmap.org/{tileLevel}/{tileX}/{tileY}.png";
            provider.WebRequest += Provider_WebRequest;

            mapControl.ZoomLevel = 16;
            mapControl.CenterPoint = new GeoPoint( 48.5684458000654, 39.3150812432244);
            mapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;

            
        }

        private void MapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            CoordPoint t = mapControl.ScreenPointToCoordPoint(e.GetPosition(mapControl));
            Console.WriteLine(t.GetX() + " " + t.GetY());
        }




        private void Provider_WebRequest(object sender, MapWebRequestEventArgs e)
        {
            e.Referer = "https://www.openstreetmap.org/";
            e.UserAgent = "com.mycompany.myapp";
        }

    }
}
