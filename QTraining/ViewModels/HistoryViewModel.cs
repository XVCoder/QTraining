using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
