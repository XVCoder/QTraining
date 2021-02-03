using Caliburn.Micro;
using QTraining.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.ViewModels
{
    [Export(typeof(IShell))]
    public class QuestionBankManageViewModel : Screen
    {
        #region Constructor
        public QuestionBankManageViewModel(IWindowManager windowManager, List<QuestionBankModel> questionBankModels)
        {
            this.windowManager = windowManager;
            this.questionBankModels = questionBankModels;
        }
        #endregion

        #region Fields & Properties
        private readonly IWindowManager windowManager;
        private List<QuestionBankModel> questionBankModels;
        private BindableCollection<QuestionBankModel> lstQuestionBankModel;
        /// <summary>
        /// 图库列表
        /// </summary>
        public BindableCollection<QuestionBankModel> LstQuestionBankModel
        {
            get => lstQuestionBankModel;
            set { lstQuestionBankModel = value; NotifyOfPropertyChange(nameof(LstQuestionBankModel)); }
        }
        #endregion

        #region Events
        /// <summary>
        /// 加载事件
        /// </summary>
        public void Loaded()
        {
            this.LstQuestionBankModel = new BindableCollection<QuestionBankModel>(questionBankModels);
        }

        /// <summary>
        /// 导入题库
        /// </summary>
        public void ImportQuesionBank()
        {
            if (windowManager.ShowDialog(new QuestionBankImportViewModel(new QuestionBankModel { })) == true)
            {

            }
        }
        #endregion
    }
}
