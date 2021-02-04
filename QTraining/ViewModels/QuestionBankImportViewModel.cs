using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
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

            var cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            cofd.DefaultDirectory = Environment.CurrentDirectory;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var qPath = cofd.FileName;
                if (!Directory.Exists(qPath + "\\Images") || Directory.GetFiles(qPath + "\\Images").Length == 0)
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionImagesNotFound"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                if (!File.Exists(qPath + "\\QustionInfo.txt"))
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionInfoNotFound"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                var qInfo = File.ReadAllLines(qPath + "\\QustionInfo.txt");
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
