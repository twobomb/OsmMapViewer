using DevExpress.Xpf.Core;
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
using DevExpress.Xpf.Bars;


namespace OsmMapViewer
{
    /// <summary>
    /// Interaction logic for AddUserTags.xaml
    /// </summary>
    public partial class AddUserTags : ThemedWindow
    {
        public AddUserTags()
        {
            InitializeComponent();
        }


        private void BarItem_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if(WND_ViewModel.CheckItems())
                DialogResult = true;
        }

        private void BarItem_OnItemClick1(object sender, ItemClickEventArgs e)
        {
            DialogResult = false;
        }
    }
}
