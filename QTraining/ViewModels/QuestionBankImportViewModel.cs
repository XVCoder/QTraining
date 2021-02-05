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
        private QuestionBankModel model;
        public QuestionBankModel Model
        {
            get => model;
            set { model = value; NotifyOfPropertyChange(nameof(Model)); }
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
                if (!File.Exists(qPath + "\\QuestionInfo.txt"))
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionInfoNotFound"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                var imageNames = Directory.GetFiles(qPath + "\\Images");
                var qInfo = File.ReadAllLines(qPath + "\\QuestionInfo.txt");
                if (imageNames.Length != qInfo.Length
                    || imageNames.ToList().Where(x => !string.IsNullOrWhiteSpace(x)).Count() != qInfo.Length)
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionInfoMismatch"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                Model.QuestionBankRootPath = qPath;
                Model.SimulationRangeCount = imageNames.Length < 60 ? imageNames.Length : 60;
                NotifyOfPropertyChange(nameof(Model));
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Model.QuestionBankRootPath)
                || string.IsNullOrWhiteSpace(Model.Name)
                || Model.SimulationMinutes < 1
                || Model.OrderTrainingMinutes < 1
                || Model.SimulationRangeCount < 1)
            {
                MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionInfoNotAvaliable"), null, System.Windows.MessageBoxButton.OK);
                return;
            }
            (GetView() as Views.QuestionBankImportView).DialogResult = true;
        }
        #endregion
    }
}
