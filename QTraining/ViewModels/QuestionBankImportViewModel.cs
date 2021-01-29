using Caliburn.Micro;
using QTraining.Common;
using QTraining.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.ViewModels
{
    [Export(typeof(IShell))]
    public class QuestionBankImportViewModel : Screen
    {
        #region Constructor
        public QuestionBankImportViewModel(QuestionBankModel model)
        {
            Model = model;
        }

        #endregion

        #region Fields & Properties
        private QuestionBankModel questionBankModel;
        public QuestionBankModel Model
        {
            get => questionBankModel;
            set { questionBankModel = value; NotifyOfPropertyChange(nameof(Model)); }
        }
        #endregion

        #region Events
        /// <summary>
        /// 选择题库路径
        /// </summary>
        public void SelectQuestionBankPath()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Environment.CurrentDirectory
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var qPath = fbd.SelectedPath;
                if (!Directory.Exists(qPath + "\\Images"))
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionImagesNotFound"));
                    return;
                }
                File.ReadAllLines(qPath + "\\QustionInfo.txt");
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {

        }
        #endregion
    }
}
