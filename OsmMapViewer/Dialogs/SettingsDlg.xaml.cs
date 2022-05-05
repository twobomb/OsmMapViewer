using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using OsmMapViewer.Properties;


namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDlg.xaml
    /// </summary>
    public partial class SettingsDlg : ThemedWindow {

        public bool IsHaveChanges { get; set; } = false;
        public SettingsDlg()
        {
            InitializeComponent();
            UpdateData();

         /*   var q = typeof(Theme).GetFields().Where(info => info.FieldType == typeof(Theme)).ToList();
            q.Reverse();


            foreach (var fieldInfo in q)
            {
                Theme t = (Theme) fieldInfo.GetValue(null);
                cb_themes.Items.Add(t);
                if (t.Name == Settings.Default.theme)
                    cb_themes.SelectedIndex = cb_themes.Items.IndexOf(t);
            }

            cb_themes.SelectionChanged += (sender, args) =>
            {
                Settings.Default.theme = (cb_themes.SelectedItem as Theme).Name;
                ApplicationThemeHelper.ApplicationThemeName = Settings.Default.theme;
                Settings.Default.Save();
            };*/

        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e){
            Settings.Default.login_osm = tb_login.Text.Trim();
            Settings.Default.pwd_osm = tb_pwd.Password;
            Settings.Default.API_OSM= tb_osm_api.Text.Trim();
            Settings.Default.Save();
            IsHaveChanges = true;
        }

        public void UpdateData(){
            //1
            tb_login.Text = Settings.Default.login_osm;
            tb_pwd.Password = Settings.Default.pwd_osm;
            tb_osm_api.Text = Settings.Default.API_OSM;

            //2

            tb_nom_host.Text = Settings.Default.NOMINATIM_HOST;
            tb_nom_lookup.Text = Settings.Default.NOMINATIM_LOOKUP;
            tb_nom_reverse.Text = Settings.Default.NOMINATIM_REVERSE;
            tb_nom_search.Text = Settings.Default.NOMINATIM_SEARCH;

            //3
            tb_tile_pattern.Text = Settings.Default.TILE_SERVER_TEMPLATE;
            //4
            tb_route_host.Text = Settings.Default.OSRM_HOST;

            //5

            tb_bing_key.Text = Settings.Default.BingKey;
            tb_get_data.Text = Settings.Default.GET_DATA;

        }

        private void SimpleButton_Click_1(object sender, RoutedEventArgs e){
            Settings.Default.login_osm = Settings.Default.Properties["login_osm"].DefaultValue.ToString();
            Settings.Default.pwd_osm = Settings.Default.Properties["pwd_osm"].DefaultValue.ToString();
            Settings.Default.API_OSM = Settings.Default.Properties["API_OSM"].DefaultValue.ToString();
            Settings.Default.Save();
            IsHaveChanges = true;
            UpdateData();
        }

        private void SimpleButton_Click_2(object sender, RoutedEventArgs e)
        {
            Settings.Default.NOMINATIM_HOST = tb_nom_host.Text.Trim();
            Settings.Default.NOMINATIM_LOOKUP= tb_nom_lookup.Text.Trim();
            Settings.Default.NOMINATIM_REVERSE= tb_nom_reverse.Text.Trim();
            Settings.Default.NOMINATIM_SEARCH= tb_nom_search.Text.Trim();
            Settings.Default.Save();
            IsHaveChanges = true;
        }

        private void SimpleButton_Click_3(object sender, RoutedEventArgs e){
            Settings.Default.NOMINATIM_HOST = Settings.Default.Properties["NOMINATIM_HOST"].DefaultValue.ToString();
            Settings.Default.NOMINATIM_SEARCH = Settings.Default.Properties["NOMINATIM_SEARCH"].DefaultValue.ToString();
            Settings.Default.NOMINATIM_LOOKUP = Settings.Default.Properties["NOMINATIM_LOOKUP"].DefaultValue.ToString();
            Settings.Default.NOMINATIM_REVERSE = Settings.Default.Properties["NOMINATIM_REVERSE"].DefaultValue.ToString();
            Settings.Default.Save();
            IsHaveChanges = true;
            UpdateData();
        }

        private void SimpleButton_Click_4(object sender, RoutedEventArgs e)
        {
            Settings.Default.TILE_SERVER_TEMPLATE= tb_tile_pattern.Text.Trim();
            Settings.Default.Save();
            IsHaveChanges = true;
        }

        private void SimpleButton_Click_5(object sender, RoutedEventArgs e)
        {
            Settings.Default.TILE_SERVER_TEMPLATE = Settings.Default.Properties["TILE_SERVER_TEMPLATE"].DefaultValue.ToString();
            Settings.Default.Save();
            IsHaveChanges = true;
            UpdateData();
        }

        private void SimpleButton_Click_6(object sender, RoutedEventArgs e)
        {
            Settings.Default.BingKey = tb_bing_key.Text.Trim();
            Settings.Default.GET_DATA= tb_get_data.Text.Trim();
            Settings.Default.Save();
            IsHaveChanges = true;
        }

        private void SimpleButton_Click_7(object sender, RoutedEventArgs e)
        {
            Settings.Default.GET_DATA = Settings.Default.Properties["GET_DATA"].DefaultValue.ToString();
            Settings.Default.BingKey= Settings.Default.Properties["BingKey"].DefaultValue.ToString();
            Settings.Default.Save();
            IsHaveChanges = true;
            UpdateData();

        }

        private void SimpleButton_Click_8(object sender, RoutedEventArgs e)
        {
            Settings.Default.OSRM_HOST = tb_route_host.Text.Trim();
            Settings.Default.Save();
            IsHaveChanges = true;

        }

        private void SimpleButton_Click_9(object sender, RoutedEventArgs e)
        {
            Settings.Default.OSRM_HOST = Settings.Default.Properties["OSRM_HOST"].DefaultValue.ToString();
            Settings.Default.Save();
            IsHaveChanges = true;
            UpdateData();

        }
    }
}
