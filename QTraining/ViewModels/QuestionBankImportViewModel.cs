using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using QTraining.Common;
using QTraining.Models;

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
            var cofd = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.CurrentDirectory
            };
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
                if (imageNames.Length != qInfo.Length || imageNames.Count(x => !string.IsNullOrWhiteSpace(x)) != qInfo.Length)
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionInfoMismatch"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                Model.QuestionBankRootPath = qPath;
                Model.Name = qPath.Split('\\')[qPath.Split('\\').Length - 1];
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
