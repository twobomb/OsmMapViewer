using OsmMapViewer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.IO.Compression;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OsmMapViewer.Converter
{
    public class TreeViewImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is TreeViewTagValue){
                    var root = Utils.GetExeDir() + @"\tags_images.zip";
                    var v = value as TreeViewTagValue;
                    if (parameter != null){
                        string linicon = v.GetType().GetProperty(parameter.ToString()).GetValue(v) as string;
                        if (!string.IsNullOrWhiteSpace(linicon) && File.Exists(root)){
                            using (var stream = ZipFile.OpenRead(root).GetEntry(linicon.Replace(@"\", @"/").Substring(1)).Open())
                                return Utils.GetImageFromStream(stream);
                        }
                    }
                }
            }
            catch (Exception e) {
                    Console.WriteLine(e);
            }
            
             return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
