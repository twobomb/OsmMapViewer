using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace OsmMapViewer.Models
{
    public class DataMsgPrinter {
        private static int _indexId = 0;

        public string Title { get; set; }
        public string Message{ get; set; }
        public readonly int Id = _indexId++;
        public DateTime Date { get; set; } = new DateTime();
        public enum MsgType
        {
            Info,
            Error,
            Success,
            Warning
        }

        public MsgType Type { get; set; } = MsgType.Info;
        public Brush Background {
            get{
                switch (Type){
                    case MsgType.Warning: return new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FFFFBC00"));
                    case MsgType.Success: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00C95E"));
                    case MsgType.Error: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF94415"));
                    case MsgType.Info: default: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0064BB"));
                }
            }
        }

        public Brush LineBackground {
            get{
                switch (Type){
                    case MsgType.Warning: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFDE81"));
                    case MsgType.Success: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00C95E"));
                    case MsgType.Error: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF94415"));
                    case MsgType.Info: default: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF65BAEE"));
                }
            }
        }
        public string Icon{
            get{
                switch (Type){
                    case MsgType.Warning: return "M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
                    case MsgType.Success: return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                    case MsgType.Error: return "M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z";
                    case MsgType.Info: default: return "M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
                }
            }
        }

    }
}
