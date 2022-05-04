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
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid.TreeList;
using OsmMapViewer.Models;
using OsmMapViewer.Properties;


namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateHouse.xaml
    /// </summary>
    public partial class CreateHouse : ThemedWindow, INotifyPropertyChanged
    {
        public string HouseNumber { get; set; }
        public string SelectedStreet { get; set; }


        public TreeViewTagValue tagSelected = null;
        public TreeViewTagValue TagSelected
        {
            get
            {
                return tagSelected;
            }
            set
            {
                tagSelected = value;
                OnPropertyChanged("TagSelected");
            }
        }


        public ObservableCollection<TreeViewTagValue> tagList { get; } 
        public ObservableCollection<string> Streets { get; set; } = new ObservableCollection<string>();
        public CreateHouse()
        {
            InitializeComponent();
            this.DataContext = this;
            var l = Utils.GetTagsTree();
            foreach (var treeViewTagValue in l)
            {
                if (treeViewTagValue.Tag == "building")
                {
                    l = treeViewTagValue.ChildrenItems;
                    break;
                }
            }
            tagList = l;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string CheckedType
        {
            get
            {
                foreach (var tv1 in tagList)
                {
                    foreach (var tv2 in tv1.ChildrenItems)
                    {
                        if (tv2.IsCheckedMe && !tv2.Variable && !string.IsNullOrWhiteSpace(tv2.Key) &&
                            !string.IsNullOrWhiteSpace(tv2.Tag))
                        {
                            if (tv2.Tag == "building")
                                return tv2.Key;
                        }

                        foreach (var tv3 in tv2.ChildrenItems)
                        {
                            if (tv3.IsCheckedMe && !tv3.Variable && !string.IsNullOrWhiteSpace(tv3.Key) &&
                                !string.IsNullOrWhiteSpace(tv3.Tag))
                            {
                                if (tv2.Tag == "building")
                                    return tv2.Key;
                            }
                        }
                    }
                }
                return null;
            }
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedStreet))
            {
                Utils.MsgBoxError("Введите улицу");
                return;
            }
            if (string.IsNullOrWhiteSpace(HouseNumber))
            {
                Utils.MsgBoxError("Введите номер дома");
                return;
            }
            if (string.IsNullOrWhiteSpace(CheckedType))
            {
                Utils.MsgBoxError("Выберите типа строения");
                return;
            }
            

            DialogResult = true;
        }

        private void SimpleButton_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Tvm_list_OnNodeCheckStateChanged(object sender, TreeViewNodeEventArgs e)
        {
            var row = (e.Row as TreeViewTagValue);
            if (string.IsNullOrWhiteSpace(row.Key) || row.Variable)
            {
                row.IsCheckedMe = false;
                Utils.MsgBoxWarning("Нельзя выбрать, откройте список и выберите конкретный тег!");
                return;
            }
            if (row.IsCheckedMe)
                foreach (var treeViewTagValue in tagList) {
                    if (row != treeViewTagValue)
                        treeViewTagValue.IsCheckedMe = false;
                    foreach (var childrenItem in treeViewTagValue.ChildrenItems)
                    {
                        if (row != childrenItem)
                            childrenItem.IsCheckedMe = false;
                    }
                }
        }
    }
}
