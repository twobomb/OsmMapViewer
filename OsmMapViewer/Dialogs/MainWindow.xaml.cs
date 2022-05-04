using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Map;
using OsmMapViewer.Misc;
using OsmMapViewer.Properties;
using OsmMapViewer.ViewModel;

namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {

        public MainWindow(){
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //ApplicationThemeHelper.ApplicationThemeName = Settings.Default.theme;
            InitializeComponent();
            Config.InitData();

            

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
            



            mapControl.ZoomLevel = 16;
            mapControl.CenterPoint = new GeoPoint( 48.5684458000654, 39.3150812432244);
            mapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            
        }

        private void MapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e){

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
