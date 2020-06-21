using Caliburn.Micro;
using Panuon.UI.Silver;
using QTraining.Common;
using QTraining.DAL;
using QTraining.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        DispatcherTimer countDownTimer;  //倒计时

        private int questionRangeCount = 65;  //题组大小
        public int QuestionRangeCount
        {
            get { return questionRangeCount; }
            set { questionRangeCount = value; NotifyOfPropertyChange(() => QuestionRangeCount); }
        }

        private List<int> randomQuestionBank;  //题目索引随机组合
        private string[] answers;  //用户回答结果

        public QuestionInfoModel CurrentQuestion
        {
            get
            {
                return QuestionInfoModels == null || randomQuestionBank == null ? null : QuestionInfoModels[randomQuestionBank[CurrentQuestionIndex]];
            }
        }

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
                NotifyOfPropertyChange(() => CurrentQuestion);
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
            set
            {
                isCheckASelected = value;
                NotifyOfPropertyChange(() => IsCheckASelected);
            }
        }

        private bool isCheckBSelected;
        public bool IsCheckBSelected
        {
            get { return isCheckBSelected; }
            set
            {
                isCheckBSelected = value;
                NotifyOfPropertyChange(() => IsCheckBSelected);
            }
        }

        private bool isCheckCSelected;
        public bool IsCheckCSelected
        {
            get { return isCheckCSelected; }
            set
            {
                isCheckCSelected = value;
                NotifyOfPropertyChange(() => IsCheckCSelected);
            }
        }

        private bool isCheckDSelected;
        public bool IsCheckDSelected
        {
            get { return isCheckDSelected; }
            set
            {
                isCheckDSelected = value;
                NotifyOfPropertyChange(() => IsCheckDSelected);
            }
        }

        private bool isCheckESelected;
        public bool IsCheckESelected
        {
            get { return isCheckESelected; }
            set
            {
                isCheckESelected = value;
                NotifyOfPropertyChange(() => IsCheckESelected);
            }
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

        private string countDown;
        /// <summary>
        /// 倒计时
        /// </summary>
        public string CountDown
        {
            get { return countDown; }
            set { countDown = value; NotifyOfPropertyChange(() => CountDown); }
        }

        /// <summary>
        /// 倒计时秒数
        /// </summary>
        private int countSecond = 130 * 60;
        /// <summary>
        /// 当前秒数
        /// </summary>
        private long startTimeTicks;

        private bool isTrainingStart = false;
        /// <summary>
        /// 练习是否开始
        /// </summary>
        public bool IsTrainingStart
        {
            get { return isTrainingStart; }
            set { isTrainingStart = value; NotifyOfPropertyChange(() => IsTrainingStart); }
        }

        private bool isRealResultVisible = false;
        /// <summary>
        /// 是否显示正确答案
        /// </summary>
        public bool IsRealResultVisible
        {
            get { return isRealResultVisible; }
            set { isRealResultVisible = value; NotifyOfPropertyChange(() => IsRealResultVisible); }
        }

        private bool isCommited;
        /// <summary>
        /// 是否已交卷
        /// </summary>
        public bool IsCommited
        {
            get { return isCommited; }
            set { isCommited = value; NotifyOfPropertyChange(() => IsCommited); }
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
            if (CurrentQuestion.ResultCount < 4)
            {
                IsRadioDVisible = false;
                if (CurrentQuestion.ResultCount < 3)
                    IsRadioCVisible = false;
            }
            else
            {
                IsRadioCVisible = true;
                IsRadioDVisible = true;
            }
            IsMultiSelect = CurrentQuestion.RealResult.Length > 1;
            CountDown = $"{(countSecond - startTimeTicks) / 60}:{(countSecond - startTimeTicks) % 60}/{countSecond / 60}:{countSecond % 60}";
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
            IsRealResultVisible = false;
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
            IsRealResultVisible = false;
        }

        /// <summary>
        /// 开始练习
        /// </summary>
        public void StartTraining()
        {
            IsTrainingStart = true;
            countDownTimer = new DispatcherTimer();
            countDownTimer.Interval = new TimeSpan(0, 0, 0, 1);
            countDownTimer.Tick += Timer_Tick;
            startTimeTicks = DateTime.Now.Ticks;
            countDownTimer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var currentSeconds = (DateTime.Now.Ticks - startTimeTicks) / TimeSpan.TicksPerSecond;
            if (currentSeconds >= countSecond)
            {
                (sender as DispatcherTimer).Stop();
                Commit();  //自动交卷计算结果
                return;
            }
            CountDown = $"{(countSecond - currentSeconds) / 60}:{(countSecond - currentSeconds) % 60} / {countSecond / 60}:{countSecond % 60}";
        }

        /// <summary>
        /// 显示/隐藏答案
        /// </summary>
        public void ShowRealResult()
        {
            IsRealResultVisible = !IsRealResultVisible;
        }

        /// <summary>
        /// 交卷/重置
        /// </summary>
        public void Commit()
        {
            if (!IsCommited)
            {//没交卷
                if (MessageBoxX.Show(App.Current.MainWindow, ResourceHelper.GetStrings("Text_CommitConfirm"),
                    ResourceHelper.GetStrings("Common_ProgramName"), MessageBoxButton.YesNo
                    , MessageBoxIcon.Warning, DefaultButton.CancelNo) == MessageBoxResult.No)
                    return;
                var trainingResult = new string[questionRangeCount];  //练习结果
                for (int i = 0; i < answers.Length; i++)
                {
                    trainingResult[i] = string.IsNullOrEmpty(answers[i]) ? "noanswer" :
                        (QuestionInfoModels[randomQuestionBank[i]].RealResult == answers[i]).ToString();
                }

                //整理答题结果
                var wrongAnswers = new List<int>();
                var noAnswers = new List<int>();
                for (int i = 0; i < trainingResult.Length; i++)
                {
                    switch (trainingResult[i])
                    {
                        case "True":
                            break;
                        case "False":
                            wrongAnswers.Add(i);
                            break;
                        default:
                            noAnswers.Add(i);
                            break;
                    }
                }
                StringBuilder trainingResultStr = new StringBuilder();
                trainingResultStr.AppendLine($"[ {QTraining.Common.ResourceHelper.GetStrings("Text_TrueAnswer")} ]  {questionRangeCount - wrongAnswers.Count - noAnswers.Count}个");
                trainingResultStr.AppendLine();
                trainingResultStr.AppendLine($"[ {QTraining.Common.ResourceHelper.GetStrings("Text_WrongAnswer")} ]  {wrongAnswers.Count}个");
                for (int i = 0; i < wrongAnswers.Count; i++)
                {
                    trainingResultStr.AppendLine($"Q{randomQuestionBank[wrongAnswers[i]] + 1} {answers[i]}[×]; {QuestionInfoModels[randomQuestionBank[i]].RealResult} [√]");
                }
                trainingResultStr.AppendLine();
                trainingResultStr.AppendLine($"[ {QTraining.Common.ResourceHelper.GetStrings("Text_NoAnswer")} ]  {noAnswers.Count}个");
                for (int i = 0; i < noAnswers.Count; i++)
                {
                    trainingResultStr.Append($"Q{randomQuestionBank[noAnswers[i]] + 1}; ");
                }
                trainingResultStr.AppendLine();
                countDownTimer.Stop();
                QTraining.Common.MessageHelper.Info(trainingResultStr.ToString(), MessageBoxButton.OK);
                IsCommited = true;
                //将练习记录写到txt文件中
                var trainingRecorderPath = Environment.CurrentDirectory + "\\training_recorder.txt";
                using (FileStream fsWrite = new FileStream(trainingRecorderPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    var buffer = new byte[fsWrite.Length];
                    fsWrite.Read(buffer, 0, buffer.Length);
                    var str = Encoding.UTF8.GetString(buffer);
                    str += $"\r\n[{DateTime.Now:yyyy-MM-dd HH:mm}]\n";
                    str += trainingResultStr.ToString();
                    var bufferNew = Encoding.UTF8.GetBytes(str);
                    fsWrite.Write(bufferNew, 0, bufferNew.Length);
                }
            }
            else
            {//已交卷，重置试题
                GenerateRandomQuestionBank(questionRangeCount);
                answers = new string[questionRangeCount];
                CurrentQuestionIndex = 0;
                IsCommited = false;
                IsTrainingStart = false;
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
            if (CurrentQuestion.ResultCount < 5)
            {//单选题，需要判断是否显示C、D选项
                if (CurrentQuestion.ResultCount < 4)
                {
                    IsRadioDVisible = false;
                    if (CurrentQuestion.ResultCount < 3)
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
                IsCheckDSelected = true;
            }
            else
            {
                IsRadioDSelected = false;
                IsCheckDSelected = false;
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
            if (CurrentQuestion.RealResult.Length == 1)
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
