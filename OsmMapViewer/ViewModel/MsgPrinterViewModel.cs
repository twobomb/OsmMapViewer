using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsmMapViewer.Models;

namespace OsmMapViewer.ViewModel
{
    public class MsgPrinterViewModel: ViewModelBase {
        public ObservableCollection<DataMsgPrinter> Items { get; set; } = new ObservableCollection<DataMsgPrinter>();

        public void ShowMessage(string title, string text, DataMsgPrinter.MsgType type)
        {
            DataMsgPrinter dmp = new DataMsgPrinter()
            {
                Title = title,
                Message = text,
                Type = type
            };
            Items.Add(dmp);
        }

        public void Success(string text, string title = "Успех"){
            ShowMessage(title,text,DataMsgPrinter.MsgType.Success);
        }
        public void Error(string text, string title = "Ошибка"){
            ShowMessage(title,text,DataMsgPrinter.MsgType.Error);
        }
        public void Warning(string text, string title = "Предупреждение"){
            ShowMessage(title,text,DataMsgPrinter.MsgType.Warning);
        }
        public void Info(string text, string title = "Информация"){
            ShowMessage(title,text,DataMsgPrinter.MsgType.Info);
        }
    }
}
