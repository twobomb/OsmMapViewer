using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;

namespace OsmMapViewer.ViewModel
{
    public class MsgPrinterViewModel : ViewModelBase {
        public ObservableCollection<DataMsgPrinter> Items { get; set; } = new ObservableCollection<DataMsgPrinter>();

        public ObservableCollection<DataMsgPrinter> HistoryItems { get; set; } = new ObservableCollection<DataMsgPrinter>();

        int MAX_SHOW_COUNT = 3;

        private readonly MainWindow Window;

        private bool _IsShowHistory = false;
        public bool IsShowHistory
        {
            get
            {
                return _IsShowHistory;
            }
            set
            {
                SetProperty(ref _IsShowHistory, value);
            }
        }

        public MsgPrinterViewModel(MainWindow Window) {
            this.Window = Window;
        }

        public void ShowMessage(string title, string text, DataMsgPrinter.MsgType type,int delay = 5000){
            Window.Dispatcher.Invoke(new Action(() =>
            {
                DataMsgPrinter dmp = new DataMsgPrinter()
                {
                    Title = title,
                    Message = text,
                    Type = type,
                    Delay = delay
                };
                dmp.timer = new Timer(dmp.Delay) { AutoReset = false };
                dmp.timer.Elapsed += (o, e) =>
                {
                    Window.Dispatcher.Invoke(new Action(() =>
                    {
                        Items.Remove(dmp);
                        HistoryItems.Insert(0,dmp);
                    }));
                };
                dmp.timer.Start();
                dmp.Delete = new Misc.RelayCommand(new Action<object>(obj =>
                {
                    Window.Dispatcher.Invoke(new Action(() =>
                    {
                        dmp.timer.Stop();
                        dmp.timer = null;
                        Items.Remove(dmp);
                        HistoryItems.Insert(0, dmp);
                    }));
                }));
                dmp.DeleteFromHistory = new Misc.RelayCommand(new Action<object>(obj =>
                {
                    Window.Dispatcher.Invoke(new Action(() =>
                    {
                        if (dmp.timer != null)
                        {
                            dmp.timer.Stop();
                            dmp.timer = null;
                        }
                        HistoryItems.Remove(dmp);
                    }));
                }));
                Items.Add(dmp);
                while (Items.Count > MAX_SHOW_COUNT)
                {
                    var item = Items[0];
                    if (item.timer != null)
                    {
                        item.timer.Stop();
                        item.timer = null;
                    }
                    Items.Remove(item);
                    HistoryItems.Add(item);
                }
            }));
        }

       /* public void Success(string text, string title = "Успех", int delay = 3000)
        {
            ShowMessage(title, text, DataMsgPrinter.MsgType.Success, delay);
        }
        public void Error(string text, string title = "Ошибка", int delay = 10000)
        {
            ShowMessage(title, text, DataMsgPrinter.MsgType.Error, delay);
        }
        public void Warning(string text, string title = "Предупреждение", int delay = 5000)
        {
            ShowMessage(title, text, DataMsgPrinter.MsgType.Warning, delay);
        }
        public void Info(string text, string title = "Информация", int delay = 5000)
        {
            ShowMessage(title, text, DataMsgPrinter.MsgType.Info, delay);
        }*/


        public void Success(string text, int delay = 3000,string title = "Успех"){
            ShowMessage(title,text,DataMsgPrinter.MsgType.Success,delay);
        }
        public void Error(string text, int delay = 10000,string title = "Ошибка" )
        {
            ShowMessage(title,text,DataMsgPrinter.MsgType.Error, delay);
        }
        public void Warning(string text, int delay = 5000,string title = "Предупреждение")
        {
            ShowMessage(title,text,DataMsgPrinter.MsgType.Warning, delay);
        }
        public void Info(string text, int delay = 5000, string title = "Информация")
        {
            ShowMessage(title,text,DataMsgPrinter.MsgType.Info, delay);
        }


        private RelayCommand clearHistory;
        public RelayCommand ClearHistory
        {
            get
            {
                return clearHistory ??
                       (clearHistory = new RelayCommand(obj => {
                           HistoryItems.Clear();
                       }));
            }
        }
    }
}
