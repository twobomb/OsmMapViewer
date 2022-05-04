using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;

namespace OsmMapViewer.Dialogs
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
