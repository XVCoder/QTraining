using Caliburn.Micro;
using Panuon.UI.Silver;
using QTraining.Common;
using QTraining.Models;
using QTraining.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        public List<QuestionBankModel> questionBankModels;
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
            var vm = new QuestionBankImportViewModel(new QuestionBankModel
            {
                SimulationMinutes = 90
            });
            if (windowManager.ShowDialog(vm) == true)
            {
                if (questionBankModels.Where(x => x.QuestionBankRootPath == vm.Model.QuestionBankRootPath
                || x.Name == vm.Model.Name).Count() > 0)
                {
                    MessageHelper.Warning(ResourceHelper.GetStrings("Text_QuestionBankAlreadyExist"), null, System.Windows.MessageBoxButton.OK);
                    return;
                }
                questionBankModels.Add(vm.Model);
                this.LstQuestionBankModel = new BindableCollection<QuestionBankModel>(questionBankModels);
            }
        }

        /// <summary>
        /// 窗体关闭时
        /// </summary>
        public void Closing()
        {
            (GetView() as QuestionBankManageView).DialogResult = true;
        }

        /// <summary>
        /// 移除
        /// </summary>
        public void Remove(QuestionBankModel selectedModel)
        {
            if (selectedModel == null)
            {
                MessageHelper.Warning(ResourceHelper.GetStrings("Text_RemovingQuestionBankNotSelected"), null, System.Windows.MessageBoxButton.OK);
                return;
            }
            if (MessageHelper.Warning(String.Format(ResourceHelper.GetStrings("Format_RemoveConfirmHint"), selectedModel.Name)) == System.Windows.MessageBoxResult.Yes)
            {
                questionBankModels.Remove(selectedModel);
                this.LstQuestionBankModel = new BindableCollection<QuestionBankModel>(questionBankModels);
            }
        }

        /// <summary>
        /// 检索
        /// </summary>
        public void SearchTextChanged(string keyword)
        {
            if (keyword != null)
                this.LstQuestionBankModel = new BindableCollection<QuestionBankModel>(
                    questionBankModels.Where(x => x.Name.ToLower().Contains(keyword.ToLower())
                    || x.QuestionBankRootPath.ToLower().Contains(keyword.ToLower()))
                    .ToList()
                    );
        }

        /// <summary>
        /// 题库导入说明
        /// </summary>
        public void QuestionBankImportHint()
        {
            System.Windows.MessageBox.Show(@"【题库目录结构】
.../[题库根目录]/Images/*
.../[题库根目录]/QuestionInfo.txt

1. Images文件夹下只能包含题目的截图文件：Q1.png、Q2.png、···
2. 行数必须与Images文件夹下题目截图文件的个数一致，否则导入时会出错

【题库目录示例】
SAA
 ├ Images
   ├ Q1.png
   ├ Q1.png
   ├ ...
   └ Q999.png
 └ QuestionInfo.txt


【QuestionInfo.txt 格式】
[题号];[选项数];[正确答案];[笔记或解析]

1. [题号]必须是 Q* （*为题目的索引值，如Q1，表示第一个题目）
   [题号]必须按顺序排列（Q1、Q2、Q3···），每往下一行，题号增1
2. [选项数]即题目实际的选项个数，与题目截图对应即可，不超过6
3. [正确答案]为A~F六个字母的组合
4. [笔记或解析]在点击'显示答案'按钮时显示在正确答案的下方，可在查看时对其进行编辑
5.不能出现空行，否则可能会导致题库录入失败或题目与选项错位的情况发生

【QuestionInfo.txt 内容示例】
Q1; 4; B; this is a test
Q2; 5; AD; 这是一个测试
Q3; 6; ABC; 这是笔记或解析...
Q999; 4; A; 解析解析解析", ResourceHelper.GetStrings("Text_QuestionBankImportHint"));
        }
        #endregion
    }
}
