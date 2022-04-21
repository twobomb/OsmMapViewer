using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsmMapViewer.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для Prompt.xaml
    /// </summary>
    public partial class Prompt : Window{
        public string Text { 
            set
            {
                tb_text.Text = value;
            }
            get
            {
                return tb_text.Text;
            }
        }
        public string Label{ 
            set
            {
                lb_label.Content= value;
            }
            get
            {
                return lb_label.Content.ToString();
            }
        }
        public Prompt()
        {
            InitializeComponent();
            this.KeyUp += Prompt_KeyUp;
            this.Loaded += Prompt_Loaded;
        }

        private void Prompt_Loaded(object sender, RoutedEventArgs e)
        {
            tb_text.Focus();
            tb_text.SelectAll();
        }

        private void Prompt_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                SimpleButton_Click(null, null); 
            if(e.Key == Key.Escape)
                SimpleButton_Click_1(null, null); 
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SimpleButton_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
