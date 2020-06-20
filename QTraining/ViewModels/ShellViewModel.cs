using Caliburn.Micro;
using QTraining.DAL;
using QTraining.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTraining.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        #region Constructors
        public ShellViewModel()
        {
            doubtInfoDAL = new DoubtInfoDAL();
            questionInfoDAL = new QuestionInfoDAL();
            trainingInfoDAL = new TrainingInfoDAL();
        }
        #endregion

        #region Fields & Properties
        private DoubtInfoDAL doubtInfoDAL;
        private QuestionInfoDAL questionInfoDAL;
        private TrainingInfoDAL trainingInfoDAL;
        private int questionRangeCount = 65;  //题组大小
        public int QuestionRangeCount
        {
            get { return questionRangeCount; }
            set { questionRangeCount = value; NotifyOfPropertyChange(() => QuestionRangeCount); }
        }

        private List<int> randomQuestionBank;  //题目索引随机组合
        private string[] answers;  //用户回答结果

        private QuestionInfoModel currentQuestion { get { return QuestionInfoModels[randomQuestionBank[CurrentQuestionIndex]]; } }

        private int currentQuestionIndex = 0;
        /// <summary>
        /// 当前题目索引值
        /// </summary>
        public int CurrentQuestionIndex
        {
            get { return currentQuestionIndex; }
            set
            {
                currentQuestionIndex = value;
                NotifyOfPropertyChange(() => CurrentQuestionIndex);
                NotifyOfPropertyChange(() => CurrentQuestionImage);
                NotifyOfPropertyChange(() => CurrentQuestionIndexForDisplay);
            }
        }

        /// <summary>
        /// 当前题目索引值 - 用于显示
        /// </summary>
        public int CurrentQuestionIndexForDisplay { get { return CurrentQuestionIndex + 1; } }

        /// <summary>
        /// 当前显示题目的图片路径
        /// </summary>
        public string CurrentQuestionImage
        {
            get { return randomQuestionBank == null ? "" : $"{Environment.CurrentDirectory}/Resources/QuestionBank/Images/Q{ randomQuestionBank[currentQuestionIndex] + 1}.png"; }
        }

        private ObservableCollection<QuestionInfoModel> questionInfoModels;
        /// <summary>
        /// 题目信息集合
        /// </summary>
        public ObservableCollection<QuestionInfoModel> QuestionInfoModels
        {
            get { return questionInfoModels; }
            set { questionInfoModels = value; NotifyOfPropertyChange(() => QuestionInfoModels); }
        }

        private bool isMultiSelect;
        /// <summary>
        /// 是否为多选题型
        /// </summary>
        public bool IsMultiSelect
        {
            get { return isMultiSelect; }
            set { isMultiSelect = value; NotifyOfPropertyChange(() => IsMultiSelect); }
        }

        private bool canPreQuestion;
        /// <summary>
        /// 上一题按钮可用性
        /// </summary>
        public bool CanPreQuestion
        {
            get { return canPreQuestion; }
            set { canPreQuestion = value; NotifyOfPropertyChange(() => CanPreQuestion); }
        }

        private bool canNextQuestion;
        /// <summary>
        /// 下一题按钮可用性
        /// </summary>
        public bool CanNextQuestion
        {
            get { return canNextQuestion; }
            set { canNextQuestion = value; NotifyOfPropertyChange(() => CanNextQuestion); }
        }

        private bool canCommit;
        /// <summary>
        /// 交卷按钮可用性
        /// </summary>
        public bool CanCommit
        {
            get { return canCommit; }
            set { canCommit = value; NotifyOfPropertyChange(() => CanCommit); }
        }

        private bool isRadioASelected;
        public bool IsRadioASelected
        {
            get { return isRadioASelected; }
            set { isRadioASelected = value; NotifyOfPropertyChange(() => IsRadioASelected); }
        }

        private bool isRadioBSelected;
        public bool IsRadioBSelected
        {
            get { return isRadioBSelected; }
            set { isRadioBSelected = value; NotifyOfPropertyChange(() => IsRadioBSelected); }
        }

        private bool isRadioCSelected;
        public bool IsRadioCSelected
        {
            get { return isRadioCSelected; }
            set { isRadioCSelected = value; NotifyOfPropertyChange(() => IsRadioCSelected); }
        }

        private bool isRadioDSelected;
        public bool IsRadioDSelected
        {
            get { return isRadioDSelected; }
            set { isRadioDSelected = value; NotifyOfPropertyChange(() => IsRadioDSelected); }
        }

        private bool isCheckASelected;
        public bool IsCheckASelected
        {
            get { return isCheckASelected; }
            set { isCheckASelected = value; NotifyOfPropertyChange(() => IsCheckASelected); }
        }

        private bool isCheckBSelected;
        public bool IsCheckBSelected
        {
            get { return isCheckBSelected; }
            set { isCheckBSelected = value; NotifyOfPropertyChange(() => IsCheckBSelected); }
        }

        private bool isCheckCSelected;
        public bool IsCheckCSelected
        {
            get { return isCheckCSelected; }
            set { isCheckCSelected = value; NotifyOfPropertyChange(() => IsCheckCSelected); }
        }

        private bool isCheckDSelected;
        public bool IsCheckDSelected
        {
            get { return isCheckDSelected; }
            set { isCheckDSelected = value; NotifyOfPropertyChange(() => IsCheckDSelected); }
        }

        private bool isCheckESelected;
        public bool IsCheckESelected
        {
            get { return isCheckESelected; }
            set { isCheckESelected = value; NotifyOfPropertyChange(() => IsCheckESelected); }
        }

        private bool isRadioCVisible = true;
        /// <summary>
        /// 单选C可见性
        /// </summary>
        public bool IsRadioCVisible
        {
            get { return isRadioCVisible; }
            set { isRadioCVisible = value; NotifyOfPropertyChange(() => IsRadioCVisible); }
        }

        private bool isRadioDVisible = true;
        /// <summary>
        /// 单选D可见性
        /// </summary>
        public bool IsRadioDVisible
        {
            get { return isRadioDVisible; }
            set { isRadioDVisible = value; NotifyOfPropertyChange(() => IsRadioDVisible); }
        }
        #endregion

        #region Events
        public void Loaded()
        {
            var dataCount = questionInfoDAL.GetDataCount();
            if (dataCount == 0)
            {//从TXT文件中加载数据
                var questionInfoPath = "./Resources/QuestionBank/QuestionInfo.txt";
                using (FileStream fsRead = new FileStream(questionInfoPath, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[fsRead.Length];
                    fsRead.Read(buffer, 0, buffer.Length);
                    var questionInfo = Encoding.UTF8.GetString(buffer);
                    var lines = questionInfo.Split('\n');
                    var lst = new ObservableCollection<QuestionInfoModel>();
                    foreach (var item in lines)
                    {
                        var dataItem = item.Replace("\r", "");
                        var dataParams = dataItem.Split(';');
                        var model = new QuestionInfoModel();
                        model.Id = dataParams[0];
                        model.ResultCount = int.Parse(dataParams[1]);
                        model.RealResult = dataParams[2];
                        model.Note = dataParams.Length == 4 ? dataParams[3] : "";
                        lst.Add(model);
                    }
                    QuestionInfoModels = lst;
                }
            }
            else
            {//从数据库中加载数据
                var lst = questionInfoDAL.GetAll();
                var ocollection = new ObservableCollection<QuestionInfoModel>();
                lst.ForEach(x => ocollection.Add(x));
                QuestionInfoModels = ocollection;
            }
            GenerateRandomQuestionBank(questionRangeCount);  //根据设定好的题组大小生成随机题组
            CurrentQuestionIndex = 0;
            CanPreQuestion = false;
            CanNextQuestion = true;
            answers = new string[questionRangeCount];  //初始化用户回答的结果
            if (currentQuestion.ResultCount < 4)
            {
                IsRadioDVisible = false;
                if (currentQuestion.ResultCount < 3)
                    IsRadioCVisible = false;
            }
            else
            {
                IsRadioCVisible = true;
                IsRadioDVisible = true;
            }
            IsMultiSelect = currentQuestion.RealResult.Length > 1;
        }

        /// <summary>
        /// 上一题
        /// </summary>
        public void PreQuestion()
        {
            if (CurrentQuestionIndex == 0)
                CanPreQuestion = false;
            else
            {
                SaveCurrentQuestionAnswer();
                CurrentQuestionIndex--;
                CurrentQuestionInitial();
                CanNextQuestion = true;
                if (CurrentQuestionIndex == 0)
                    CanPreQuestion = false;
            }
        }

        /// <summary>
        /// 下一题
        /// </summary>
        public void NextQuestion()
        {
            if (CurrentQuestionIndex == questionRangeCount - 1)
                CanNextQuestion = false;
            else
            {
                SaveCurrentQuestionAnswer();
                //切换
                CurrentQuestionIndex++;
                CurrentQuestionInitial();
                CanPreQuestion = true;
                if (CurrentQuestionIndex == questionRangeCount - 1)
                    CanNextQuestion = false;
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Exit()
        {
            Environment.Exit(0);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// 根据题库和题组大小生成随机题目
        /// </summary>
        /// <param name="questionRangeCount">题组中包含题目的个数</param>
        private void GenerateRandomQuestionBank(int questionRangeCount)
        {
            var questionRange = new int[questionRangeCount];
            for (int i = 0; i < questionRangeCount; i++)
            {
                int randomNum;
                do
                    randomNum = new Random(Guid.NewGuid().GetHashCode()).Next(0, QuestionInfoModels.Count - 1);
                while (questionRange.Contains(randomNum));
                questionRange[i] = randomNum;
            }
            randomQuestionBank = questionRange.ToList();
        }

        /// <summary>
        /// 当前题目初始化
        /// </summary>
        private void CurrentQuestionInitial()
        {
            var answer = answers[CurrentQuestionIndex];  //当前题目用户选择的答案
            if (currentQuestion.ResultCount < 5)
            {//单选题，需要判断是否显示C、D选项
                if (currentQuestion.ResultCount < 4)
                {
                    IsRadioDVisible = false;
                    if (currentQuestion.ResultCount < 3)
                        IsRadioCVisible = false;
                }
                else
                {
                    IsRadioCVisible = true;
                    IsRadioDVisible = true;
                }
                IsMultiSelect = false;
            }
            else
                IsMultiSelect = true;
            if (answer == null || answer.Length == 0)
                return;
            if (answer.Contains("A"))
            {
                IsRadioASelected = true;
                IsCheckASelected = true;
            }
            else
            {
                IsRadioASelected = false;
                IsCheckASelected = false;
            }
            if (answer.Contains("B"))
            {
                IsRadioBSelected = true;
                IsCheckBSelected = true;
            }
            else
            {
                IsRadioBSelected = false;
                IsCheckBSelected = false;
            }
            if (answer.Contains("C"))
            {
                IsRadioCSelected = true;
                IsCheckCSelected = true;
            }
            else
            {
                IsRadioCSelected = false;
                IsCheckCSelected = false;
            }
            if (answer.Contains("D"))
            {
                IsRadioDSelected = true;
                IsCheckCSelected = true;
            }
            else
            {
                IsRadioDSelected = false;
                IsCheckCSelected = false;
            }
            if (answer.Contains("E"))
                IsCheckESelected = true;
            else
                IsCheckESelected = false;
        }

        /// <summary>
        /// 保存当前题目答案
        /// </summary>
        private void SaveCurrentQuestionAnswer()
        {
            string answer = "";
            if (currentQuestion.RealResult.Length == 1)
            {//单选
                if (IsRadioASelected)
                    answer += "A";
                if (IsRadioBSelected)
                    answer += "B";
                if (IsRadioCSelected)
                    answer += "C";
                if (IsRadioDSelected)
                    answer += "D";
            }
            else
            {//多选
                if (IsCheckASelected)
                    answer += "A";
                if (IsCheckBSelected)
                    answer += "B";
                if (IsCheckCSelected)
                    answer += "C";
                if (IsCheckDSelected)
                    answer += "D";
                if (IsCheckESelected)
                    answer += "E";
            }
            //切换前先保存当前题目的答案
            answers[CurrentQuestionIndex] = answer;
            IsRadioASelected = IsRadioBSelected = IsRadioCSelected = IsRadioDSelected = false;
            IsCheckASelected = IsCheckBSelected = IsCheckCSelected = IsCheckDSelected = IsCheckESelected = false;
        }
        #endregion
    }
}
