using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;

namespace OsmMapViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App() {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public static string DIR_CRASHLOG = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                            @"\OsmMapViewer Crash Logs";
        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            if (!Directory.Exists(DIR_CRASHLOG))
                Directory.CreateDirectory(DIR_CRASHLOG);

            var f = File.CreateText(DIR_CRASHLOG + @"\crash-" + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".log");
            f.WriteLine("ВРЕМЯ");
            f.WriteLine(DateTime.Now.ToString("G"));
            f.WriteLine("СИСТЕМА");
            f.WriteLine(System.Environment.OSVersion.ToString());
            f.WriteLine("ИСКЛЮЧЕНИЕ В ИСХОДНОМ ВИДЕ");
            f.WriteLine(e.ExceptionObject.ToString());
            f.WriteLine("ТРАССИРОВКА СТЕКА");
            f.WriteLine(System.Environment.StackTrace);
            f.WriteLine("СООБЩЕНИЕ");
            f.WriteLine((e.ExceptionObject as Exception).Message);
            f.WriteLine("ВНУТРЕННЕЕ ИСКЛЮЧЕНИЕ");
            if ((e.ExceptionObject as Exception).InnerException == null)
                f.WriteLine("Не найдено");
            else
                f.WriteLine((e.ExceptionObject as Exception).InnerException.ToString());

            f.Close();



        }
    }
}
