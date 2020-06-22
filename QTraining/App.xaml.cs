using QTraining.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QTraining
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "QTraining");
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageHelper.Info(ResourceHelper.GetStrings("Message_ProgramIsRunning"), MessageBoxButton.OK, Panuon.UI.Silver.DefaultButton.YesOK, App.Current.MainWindow);
                this.Shutdown();
            }
        }
    }
}
