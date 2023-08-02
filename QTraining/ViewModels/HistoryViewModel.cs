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
        private readonly IEventAggregator _eventAggregator;

        #region Constructor
        public HistoryViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
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

        public void Closed()
        {
            _eventAggregator.PublishOnUIThread("HISTORY CLOSED");
        }
        #endregion
    }
}
