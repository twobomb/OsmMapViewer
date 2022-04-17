using System;
using System.Collections.Generic;
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
using DevExpress.Xpf.Core;
using Newtonsoft.Json;
using OsmMapViewer.Annotations;
using OsmMapViewer.ViewModel;


namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for AddKitSelectionDlg.xaml
    /// </summary>
    public partial class AddKitSelectionDlg : ThemedWindow, INotifyPropertyChanged
    {
        private Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();
        public string NameLb { get; set; }

        public string LabelCount
        {
            get
            {
                return "Выбрано тегов: "+list.Count;
            }
        }

        public string Json
        {
            get
            {
                return JsonConvert.SerializeObject(list);
            }
        }

        
        public AddKitSelectionDlg()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SimpleButton_Click_1(object sender, RoutedEventArgs e)
        {
            if(list.Count == 0)
            {
                Utils.MsgBoxWarning("Не указано ни одного тега!");
            }
            else if (string.IsNullOrWhiteSpace(NameLb))
            {

                Utils.MsgBoxWarning("Введите имя набора");
            }
            else
            {
                Name = Name.Trim();
                DialogResult = true;
            }
        }

        private void SimpleButton_Click_2(object sender, RoutedEventArgs e) {
            Selector sel = new Selector();
            if (sel.ShowDialog().GetValueOrDefault(false)) {
                var vm = sel.DataContext as SelectorViewModel;
                list = vm.CheckedItems;
                OnPropertyChanged("LabelCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
