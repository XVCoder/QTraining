using System.Windows;
using QTraining.Common;

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
                MessageHelper.Info(ResourceHelper.GetStrings("Message_ProgramIsRunning"));
                this.Shutdown();
            }
        }
    }
}
