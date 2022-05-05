using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Data.Mask;
using DevExpress.Xpf.Core;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;
using OsmMapViewer.Properties;


namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for ShowLayersDlg.xaml
    /// </summary>
    public partial class ShowLayersDlg : ThemedWindow, INotifyPropertyChanged
    {
        public ObservableCollection<LayerData> Layers { get; set; }
        public LayerData LayerToLoad = null;
        public ShowLayersDlg()
        {
            Layers = new ObservableCollection<LayerData>(Utils.LoadLayers());
            this.DataContext = this;
            InitializeComponent();
            Layers.CollectionChanged += Layers_CollectionChanged;
        }

        private RelayCommand deleteLayer;

        public RelayCommand DeleteLayer {
            get
            {
                return deleteLayer ??
                       (deleteLayer = new RelayCommand(obj =>
                       {
                           if(Utils.MsgBoxQuestion("Удалить слой '"+((LayerData)obj).DisplayName+"'?") == MessageBoxResult.Yes)
                            Layers.Remove((LayerData)obj);
                       }));
            }
        }
        private RelayCommand loadLayer;

        public RelayCommand LoadLayer {
            get
            {
                return loadLayer ??
                       (loadLayer = new RelayCommand(obj =>
                       {
                           LayerToLoad = obj as LayerData;
                           DialogResult = true;
                       }));
            }
        }
        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Utils.SaveLayers(Layers.ToList());
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
