using System;
using System.ComponentModel.Composition;
using System.IO;
using Caliburn.Micro;
using QTraining.Common;

namespace QTraining.ViewModels
{
    [Export(typeof(IShell))]
    public class HistoryViewModel : Screen
    {
        #region Constructor
        public HistoryViewModel()
        {

        }
        #endregion

        #region Fields & Properties
        private string history;
        /// <summary>
        /// 历史记录
        /// </summary>
        public string History
        {
            get { return history; }
            set { history = value; NotifyOfPropertyChange(() => History); }
        }
        #endregion

        #region Events
        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            if (MessageHelper.Warning(ResourceHelper.GetStrings("Text_ClearConfirm")) == System.Windows.MessageBoxResult.Yes)
            {
                string trainingRecorderPath = $"{Environment.CurrentDirectory}\\training_recorder.txt";
                using (FileStream fsWrite = new FileStream(trainingRecorderPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    fsWrite.SetLength(0);
                }
                History = "";
            }
        }
        #endregion
    }
}
