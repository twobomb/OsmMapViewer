using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Core.Native;

namespace OsmMapViewer.Converter
{
    class FilterAutocompleteConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Dictionary<string, List<string>> && values[1] is string)
            {
                var v = values[0] as Dictionary<string, List<string>>;
                if (v.ContainsKey(values[1].ToString()))
                    return v[values[1].ToString()];
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
