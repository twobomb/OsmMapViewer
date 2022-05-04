using DevExpress.Xpf.Core;

namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class Selector : ThemedWindow
    {
        public Selector()
        {
            InitializeComponent();



        }
        private void BarButtonItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            DialogResult = false;
        }

        private void BarButtonItem_ItemClick_1(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            DialogResult = true;
        }
    }

}