using Caliburn.Micro;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using QTraining.Common;
using QTraining.DAL;
using QTraining.Models;
using QTraining.Views;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static QTraining.Common.EnumDefine;

namespace QTraining.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        #region Constructors
        public ShellViewModel()
        {
            doubtInfoDAL = new DoubtInfoDAL();
            questionInfoDAL = new QuestionInfoDAL();
            trainingInfoDAL = new TrainingInfoDAL();

            //根据试题类型设置题数
            var questionBankType = (QuestionBankType)Enum.Parse(typeof(QuestionBankType), Properties.Settings.Default.QuestionBankType);
            switch (questionBankType)
            {
                case QuestionBankType.SAA:
                    QuestionRangeCount = 65;
                    break;
                case QuestionBankType.SAP:
                    QuestionRangeCount = 75;
                    break;
                default:
                    QuestionRangeCount = 100;
                    break;
            }
        }
        #endregion

        #region Fields & Properties
        private const string QUESTIONBANK_SAA = "./Resources/QuestionBank/SAA";
        private const string QUESTIONBANK_SAP = "./Resources/QuestionBank/SAP";

        private DoubtInfoDAL doubtInfoDAL;
        private QuestionInfoDAL questionInfoDAL;
        private TrainingInfoDAL trainingInfoDAL;
        DispatcherTimer countDownTimer;  //倒计时

        private int questionRangeCount;
        /// <summary>
        /// 题组大小
        /// </summary>
        public int QuestionRangeCount
        {
            get => questionRangeCount;
            set
            {
                questionRangeCount = value;
                NotifyOfPropertyChange(nameof(QuestionRangeCount));
            }
        }

        private int selectedQuestionTypeIndex;
        /// <summary>
        /// 选中实体类型索引值
        /// </summary>
        public int SelectedQuestionTypeIndex
        {
            get
            {
                var index = (int)Enum.Parse(typeof(QuestionBankType), Properties.Settings.Default.QuestionBankType);
                return index < 0 ? 0 : index;
            }
            set
            {
                selectedQuestionTypeIndex = value;
                Properties.Settings.Default.QuestionBankType = ((QuestionBankType)value).ToString();
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(nameof(SelectedQuestionTypeIndex));
            }
        }

        private string currentQuestionBankPath = "";

        private List<int> randomQuestionBank;  //题目索引随机组合
        private string[] answers;  //用户回答结果

        public QuestionInfoModel CurrentQuestion
        {
            get => QuestionInfoModels == null || randomQuestionBank == null ? null : QuestionInfoModels[randomQuestionBank[CurrentQuestionIndex]];
        }

        private int currentQuestionIndex = 0;
        /// <summary>
        /// 当前题目索引值
        /// </summary>
        public int CurrentQuestionIndex
        {
            get => currentQuestionIndex;
            set
            {
                currentQuestionIndex = value;
                NotifyOfPropertyChange(nameof(CurrentQuestionIndex));
                NotifyOfPropertyChange(nameof(CurrentQuestionImage));
                NotifyOfPropertyChange(nameof(CurrentQuestionIndexForDisplay));
                NotifyOfPropertyChange(nameof(CurrentQuestion));
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
            get => randomQuestionBank == null ? "" : $"{Path.GetFullPath(currentQuestionBankPath)}/Images/Q{ randomQuestionBank[currentQuestionIndex] + 1}.png";
        }

        private ObservableCollection<QuestionInfoModel> questionInfoModels;
        /// <summary>
        /// 题目信息集合
        /// </summary>
        public ObservableCollection<QuestionInfoModel> QuestionInfoModels
        {
            get => questionInfoModels;
            set { questionInfoModels = value; NotifyOfPropertyChange(nameof(QuestionInfoModels)); }
        }

        private bool isMultiSelect;
        /// <summary>
        /// 是否为多选题型
        /// </summary>
        public bool IsMultiSelect
        {
            get => isMultiSelect;
            set { isMultiSelect = value; NotifyOfPropertyChange(nameof(IsMultiSelect)); }
        }

        private bool canPreQuestion;
        /// <summary>
        /// 上一题按钮可用性
        /// </summary>
        public bool CanPreQuestion
        {
            get => canPreQuestion;
            set { canPreQuestion = value; NotifyOfPropertyChange(nameof(CanPreQuestion)); }
        }

        private bool canNextQuestion;
        /// <summary>
        /// 下一题按钮可用性
        /// </summary>
        public bool CanNextQuestion
        {
            get => canNextQuestion;
            set { canNextQuestion = value; NotifyOfPropertyChange(nameof(CanNextQuestion)); }
        }

        private bool isRadioASelected;
        public bool IsRadioASelected
        {
            get => isRadioASelected;
            set { isRadioASelected = value; NotifyOfPropertyChange(nameof(IsRadioASelected)); }
        }

        private bool isRadioBSelected;
        public bool IsRadioBSelected
        {
            get => isRadioBSelected;
            set { isRadioBSelected = value; NotifyOfPropertyChange(nameof(IsRadioBSelected)); }
        }

        private bool isRadioCSelected;
        public bool IsRadioCSelected
        {
            get => isRadioCSelected;
            set { isRadioCSelected = value; NotifyOfPropertyChange(nameof(IsRadioCSelected)); }
        }

        private bool isRadioDSelected;
        public bool IsRadioDSelected
        {
            get => isRadioDSelected;
            set { isRadioDSelected = value; NotifyOfPropertyChange(nameof(IsRadioDSelected)); }
        }

        private bool isRadioOrderTrainingSelected;
        /// <summary>
        /// 顺序练习模式选中
        /// </summary>
        public bool IsRadioOrderTrainingSelected
        {
            get => isRadioOrderTrainingSelected;
            set
            {
                isRadioOrderTrainingSelected = value;
                NotifyOfPropertyChange(nameof(IsRadioOrderTrainingSelected));
            }
        }

        private bool isRadioSimulationTrainingSelected;
        /// <summary>
        /// 仿真模拟模式选中
        /// </summary>
        public bool IsRadioSimulationTrainingSelected
        {
            get => isRadioSimulationTrainingSelected;
            set
            {
                isRadioSimulationTrainingSelected = value;
                NotifyOfPropertyChange(nameof(IsRadioSimulationTrainingSelected));
            }
        }

        private bool isCheckASelected;
        public bool IsCheckASelected
        {
            get => isCheckASelected;
            set
            {
                isCheckASelected = value;
                NotifyOfPropertyChange(nameof(IsCheckASelected));
            }
        }

        private bool isCheckBSelected;
        public bool IsCheckBSelected
        {
            get => isCheckBSelected;
            set
            {
                isCheckBSelected = value;
                NotifyOfPropertyChange(nameof(IsCheckBSelected));
            }
        }

        private bool isCheckCSelected;
        public bool IsCheckCSelected
        {
            get => isCheckCSelected;
            set
            {
                isCheckCSelected = value;
                NotifyOfPropertyChange(nameof(IsCheckCSelected));
            }
        }

        private bool isCheckDSelected;
        public bool IsCheckDSelected
        {
            get => isCheckDSelected;
            set
            {
                isCheckDSelected = value;
                NotifyOfPropertyChange(nameof(IsCheckDSelected));
            }
        }

        private bool isCheckESelected;
        public bool IsCheckESelected
        {
            get => isCheckESelected;
            set
            {
                isCheckESelected = value;
                NotifyOfPropertyChange(nameof(IsCheckESelected));
            }
        }

        private bool isCheckFSelected;
        public bool IsCheckFSelected
        {
            get => isCheckFSelected;
            set
            {
                isCheckFSelected = value;
                NotifyOfPropertyChange(nameof(IsCheckFSelected));
            }
        }

        private bool isRadioCVisible = true;
        /// <summary>
        /// 单选C可见性
        /// </summary>
        public bool IsRadioCVisible
        {
            get => isRadioCVisible;
            set { isRadioCVisible = value; NotifyOfPropertyChange(nameof(IsRadioCVisible)); }
        }

        private bool isRadioDVisible = true;
        /// <summary>
        /// 单选D可见性
        /// </summary>
        public bool IsRadioDVisible
        {
            get => isRadioDVisible;
            set { isRadioDVisible = value; NotifyOfPropertyChange(nameof(IsRadioDVisible)); }
        }

        private string countDown;
        /// <summary>
        /// 倒计时
        /// </summary>
        public string CountDown
        {
            get => countDown;
            set { countDown = value; NotifyOfPropertyChange(nameof(CountDown)); }
        }

        /// <summary>
        /// 倒计时秒数
        /// </summary>
        private int countSecond
        {
            get
            {
                switch ((QuestionBankType)Enum.Parse(typeof(QuestionBankType), Properties.Settings.Default.QuestionBankType))
                {
                    case QuestionBankType.SAA:
                        return 130 * 60;
                    case QuestionBankType.SAP:
                        return 190 * 60;
                    default:
                        return 120 * 60;
                }
            }
        }
        /// <summary>
        /// 当前秒数
        /// </summary>
        private long currentSeconds;
        /// <summary>
        /// 开始时的计时周期数
        /// </summary>
        private long startTimeTicks;

        private bool isTrainingStart = false;
        /// <summary>
        /// 练习是否开始
        /// </summary>
        public bool IsTrainingStart
        {
            get => isTrainingStart;
            set { isTrainingStart = value; NotifyOfPropertyChange(nameof(IsTrainingStart)); }
        }

        private bool isRealResultVisible = false;
        /// <summary>
        /// 是否显示正确答案
        /// </summary>
        public bool IsRealResultVisible
        {
            get => isRealResultVisible;
            set { isRealResultVisible = value; NotifyOfPropertyChange(nameof(IsRealResultVisible)); }
        }

        private bool isCommited;
        /// <summary>
        /// 是否已交卷
        /// </summary>
        public bool IsCommited
        {
            get => isCommited;
            set { isCommited = value; NotifyOfPropertyChange(nameof(IsCommited)); }
        }

        private Visibility countDownVisibility = Visibility.Collapsed;
        /// <summary>
        /// 倒计时可见性
        /// </summary>
        public Visibility CountDownVisibility
        {
            get => countDownVisibility;
            set
            {
                countDownVisibility = value;
                NotifyOfPropertyChange(nameof(CountDownVisibility));
            }
        }

        private int turnToNum = 1;
        /// <summary>
        /// 跳转题号
        /// </summary>
        public int TurnToNum
        {
            get => turnToNum;
            set
            {
                turnToNum = value;
                NotifyOfPropertyChange(nameof(TurnToNum));
            }
        }

        /// <summary>
        /// 最后一次浏览时的题目的索引（用于顺序练习）
        /// </summary>
        public int LastReadingIndex
        {
            get => Properties.Settings.Default.LastReadingIndex;
            set
            {
                Properties.Settings.Default.LastReadingIndex = value;
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(nameof(LastReadingIndex));
                NotifyOfPropertyChange(nameof(LastReadingIndexHint));
            }
        }

        /// <summary>
        /// 最后一次浏览时的题目的索引提示（用于顺序练习）
        /// </summary>
        public string LastReadingIndexHint
        {
            get => string.Format(ResourceHelper.GetStrings("Format_LastReadingIndexHint"), LastReadingIndex + 1);
        }

        private bool isLastReadingIndexHintVisible;
        /// <summary>
        /// 最后一次浏览时的题目的索引提示的可见性
        /// </summary>
        public bool IsLastReadingIndexHintVisible
        {
            get => isLastReadingIndexHintVisible;
            set
            {
                isLastReadingIndexHintVisible = value;
                NotifyOfPropertyChange(nameof(IsLastReadingIndexHintVisible));
            }
        }
        #endregion

        #region Events
        public void Loaded()
        {
            #region Old
            //var dataCount = questionInfoDAL.GetDataCount();
            //if (dataCount == 0)
            //{//从TXT文件中加载数据
            //    var questionInfoPath = "./Resources/QuestionBank/QuestionInfo.txt";
            //    using (FileStream fsRead = new FileStream(questionInfoPath, FileMode.Open, FileAccess.Read))
            //    {
            //        var buffer = new byte[fsRead.Length];
            //        fsRead.Read(buffer, 0, buffer.Length);
            //        var questionInfo = Encoding.UTF8.GetString(buffer);
            //        var lines = questionInfo.Split('\n');
            //        var lst = new ObservableCollection<QuestionInfoModel>();
            //        foreach (var item in lines)
            //        {
            //            var dataItem = item.Replace("\r", "");
            //            var dataParams = dataItem.Split(';');
            //            var model = new QuestionInfoModel
            //            {
            //                Id = dataParams[0],
            //                ResultCount = int.Parse(dataParams[1]),
            //                RealResult = dataParams[2],
            //                Note = dataParams.Length == 4 ? dataParams[3] : ""
            //            };
            //            lst.Add(model);
            //        }
            //        QuestionInfoModels = lst;
            //    }
            //}
            //else
            //{//从数据库中加载数据
            //    var lst = questionInfoDAL.GetAll();
            //    var ocollection = new ObservableCollection<QuestionInfoModel>();
            //    lst.ForEach(x => ocollection.Add(x));
            //    QuestionInfoModels = ocollection;
            //} 
            #endregion

            ////从TXT文件中加载数据
            //var questionInfoPath = "./Resources/QuestionBank/QuestionInfo.txt";
            //using (FileStream fsRead = new FileStream(questionInfoPath, FileMode.Open, FileAccess.Read))
            //{
            //    var buffer = new byte[fsRead.Length];
            //    fsRead.Read(buffer, 0, buffer.Length);
            //    var questionInfo = Encoding.UTF8.GetString(buffer);
            //    var lines = questionInfo.Split('\n');
            //    var lst = new ObservableCollection<QuestionInfoModel>();
            //    foreach (var item in lines)
            //    {
            //        var dataItem = item.Replace("\r", "");
            //        var dataParams = dataItem.Split(';');
            //        var model = new QuestionInfoModel
            //        {
            //            Id = dataParams[0],
            //            ResultCount = int.Parse(dataParams[1]),
            //            RealResult = dataParams[2],
            //            Note = dataParams.Length == 4 ? dataParams[3] : ""
            //        };
            //        lst.Add(model);
            //    }
            //    QuestionInfoModels = lst;
            //}
            IsRadioOrderTrainingSelected = true;
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
                IsRadioASelected = IsRadioBSelected = IsRadioCSelected = IsRadioDSelected = false;
                IsCheckASelected = IsCheckBSelected = IsCheckCSelected = IsCheckDSelected = IsCheckESelected = false;
                CurrentQuestionIndex--;
                CurrentQuestionInitial();
                CanNextQuestion = true;
                if (CurrentQuestionIndex == 0)
                    CanPreQuestion = false;
                //隐藏上次浏览位置提示
                if (IsRadioOrderTrainingSelected && CurrentQuestionIndex == LastReadingIndex)
                    IsLastReadingIndexHintVisible = false;
            }
            IsRealResultVisible = false;
        }

        /// <summary>
        /// 下一题
        /// </summary>
        public void NextQuestion()
        {
            if (CurrentQuestionIndex == QuestionRangeCount - 1)
                CanNextQuestion = false;
            else
            {
                SaveCurrentQuestionAnswer();
                IsRadioASelected = IsRadioBSelected = IsRadioCSelected = IsRadioDSelected = false;
                IsCheckASelected = IsCheckBSelected = IsCheckCSelected = IsCheckDSelected = IsCheckESelected = false;
                //切换
                CurrentQuestionIndex++;
                CurrentQuestionInitial();
                CanPreQuestion = true;
                if (CurrentQuestionIndex == QuestionRangeCount - 1)
                    CanNextQuestion = false;
                //隐藏上次浏览位置提示
                if (IsRadioOrderTrainingSelected && CurrentQuestionIndex == LastReadingIndex)
                    IsLastReadingIndexHintVisible = false;
            }
            IsRealResultVisible = false;
        }

        /// <summary>
        /// 开始练习
        /// </summary>
        public void StartTraining()
        {
            string questionInfoPath = "";
            switch ((QuestionBankType)SelectedQuestionTypeIndex)
            {
                case QuestionBankType.SAA:
                    currentQuestionBankPath = QUESTIONBANK_SAA;
                    questionInfoPath = QUESTIONBANK_SAA + "/QuestionInfo.txt";
                    break;
                case QuestionBankType.SAP:
                    currentQuestionBankPath = QUESTIONBANK_SAP;
                    questionInfoPath = QUESTIONBANK_SAP + "/QuestionInfo.txt";
                    break;
                default:
                    break;
            }
            //从TXT文件中加载数据
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
                    var model = new QuestionInfoModel
                    {
                        Id = dataParams[0],
                        ResultCount = int.Parse(dataParams[1]),
                        RealResult = dataParams[2],
                        Note = dataParams.Length == 4 ? dataParams[3] : ""
                    };
                    lst.Add(model);
                }
                QuestionInfoModels = lst;
            }

            if (IsRadioSimulationTrainingSelected)
            {//模拟仿真
                CountDownVisibility = Visibility.Visible;
                //设置题数
                switch ((QuestionBankType)Enum.Parse(typeof(QuestionBankType), Properties.Settings.Default.QuestionBankType))
                {
                    case QuestionBankType.SAA:
                        QuestionRangeCount = 65;
                        break;
                    case QuestionBankType.SAP:
                        QuestionRangeCount = 75;
                        break;
                    default:
                        QuestionRangeCount = 100;
                        break;
                }
                //根据设定好的题组大小生成随机题组
                GenerateRandomQuestionBank();
                //初始化答题板
                AnswerBoardInitial();
                //练习开始
                IsTrainingStart = true;
                //开始计时
                CountDownStart();
            }
            else if (IsRadioOrderTrainingSelected)
            {//顺序练习
                //设置题数
                QuestionRangeCount = QuestionInfoModels.Count;
                //隐藏倒计时
                CountDownVisibility = Visibility.Collapsed;
                //生成题库
                GenerateOrderQuestionBank();
                //初始化答题板
                AnswerBoardInitial();
                //根据当前题组总数判断是否显示上次阅读
                if (LastReadingIndex == 0 || LastReadingIndex > QuestionRangeCount)
                    IsLastReadingIndexHintVisible = false;
                else
                    IsLastReadingIndexHintVisible = true;
                //练习开始
                IsTrainingStart = true;
            }
            else
            {//提示选择模式
                MessageBoxX.Show("请选择模式", "提示", MessageBoxButton.OK, MessageBoxIcon.Info);
            }
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
                //保存当前查看试题的回答
                SaveCurrentQuestionAnswer();
                var trainingResult = new string[QuestionRangeCount];  //练习结果
                for (int i = 0; i < answers.Length; i++)
                {
                    trainingResult[i] = string.IsNullOrEmpty(answers[i]) ? "noanswer" :
                        (QuestionInfoModels[randomQuestionBank[i]].RealResult.ToUpper() == answers[i].ToUpper()).ToString();
                }
                //整理答题结果
                var wrongAnswers = new List<int>();
                var noAnswers = new List<int>();
                for (int i = 0; i < trainingResult.Length; i++)
                {//筛选用户的回答
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
                StringBuilder commitConfirmMsg = new StringBuilder();  //确认交卷提示消息
                commitConfirmMsg.AppendLine(ResourceHelper.GetStrings("Text_CommitConfirm"));
                commitConfirmMsg.AppendLine($"[ {ResourceHelper.GetStrings("Text_NoAnswer")} ]  {noAnswers.Count}个");
                for (int i = 0; i < noAnswers.Count; i++)
                {//未答
                    commitConfirmMsg.Append($"({noAnswers[i] + 1}/{QuestionRangeCount}) Q{randomQuestionBank[noAnswers[i]]};  ");
                }
                //确认交卷弹窗提示
                if (MessageBoxX.Show(App.Current.MainWindow, commitConfirmMsg.ToString(),
                    ResourceHelper.GetStrings("Common_ProgramName"), MessageBoxButton.YesNo
                    , MessageBoxIcon.Warning, DefaultButton.CancelNo) == MessageBoxResult.No)
                    return;
                StringBuilder trainingResultStr = new StringBuilder();  //练习结果消息
                trainingResultStr.AppendLine($"[ {ResourceHelper.GetStrings("Text_TrueAnswer")} ]  {QuestionRangeCount - wrongAnswers.Count - noAnswers.Count}个");
                trainingResultStr.AppendLine();
                trainingResultStr.AppendLine($"[ {ResourceHelper.GetStrings("Text_WrongAnswer")} ]  {wrongAnswers.Count}个");
                for (int i = 0; i < wrongAnswers.Count; i++)
                {//错题
                    trainingResultStr.AppendLine($"({wrongAnswers[i] + 1}/{QuestionRangeCount}) Q{randomQuestionBank[wrongAnswers[i]] + 1} {answers[wrongAnswers[i]]} [×]  {QuestionInfoModels[randomQuestionBank[wrongAnswers[i]]].RealResult} [√]");
                }
                trainingResultStr.AppendLine();
                trainingResultStr.AppendLine($"[ {ResourceHelper.GetStrings("Text_NoAnswer")} ]  {noAnswers.Count}个");
                for (int i = 0; i < noAnswers.Count; i++)
                {//未答
                    trainingResultStr.Append($"({noAnswers[i] + 1}/{QuestionRangeCount}) Q{randomQuestionBank[noAnswers[i]] + 1};  ");
                }
                trainingResultStr.AppendLine();
                countDownTimer.Stop();
                //弹窗显示练习结果
                MessageHelper.Info(trainingResultStr.ToString(), MessageBoxButton.OK);
                IsCommited = true;
                //将练习记录写到txt文件中
                var trainingRecorderPath = Environment.CurrentDirectory + "\\training_recorder.txt";
                StreamWriter tw = null;
                try
                {
                    if (!File.Exists(trainingRecorderPath))
                        File.Create(trainingRecorderPath);
                    using (tw = File.AppendText(trainingRecorderPath))
                    {
                        var str = $"【{DateTime.Now:yyyy-MM-dd HH:mm}】{ResourceHelper.GetStrings("Text_TimeCosts")}  {currentSeconds / 60}':{(currentSeconds % 60).ToString().PadLeft(2, '0')}''\n";
                        str += trainingResultStr.ToString();
                        str += "\r\n";
                        tw.WriteLine(str);
                        tw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (tw != null)
                        tw.Close();
                }
            }
            else
            {//已交卷，重置试题
                IsRadioASelected = IsRadioBSelected = IsRadioCSelected = IsRadioDSelected = false;
                IsCheckASelected = IsCheckBSelected = IsCheckCSelected = IsCheckDSelected = IsCheckESelected = false;
                //GenerateRandomQuestionBank();
                //answers = new string[QuestionRangeCount];
                //CurrentQuestionIndex = 0;
                IsCommited = false;
                IsTrainingStart = false;
            }
        }

        /// <summary>
        /// 跳转
        /// </summary>
        public void TurnTo(object e)
        {
            if (e != null)
            {//调用事件来自题号输入框
                if (e is KeyEventArgs eKey)
                {//事件类型为KeyUp，且按键为回车  →  直接执行跳转的代码
                    eKey.Handled = eKey.Key == Key.Space;
                    if ((eKey.Key >= Key.D0 && eKey.Key <= Key.D9) || eKey.Key == Key.Enter)
                    {
                        if (eKey.Key != Key.Enter)
                            return;
                        if (int.TryParse((eKey.OriginalSource as TextBox).Text, out int value))
                            TurnToNum = value;
                        if (value == 0)
                            TurnToNum = 1;
                    }
                    else
                        return;
                }
            }
            if (TurnToNum < 1 || TurnToNum > QuestionRangeCount)
            {//跳转题号合法性监测
                TurnToNum = 0;
                return;
            }
            SaveCurrentQuestionAnswer();
            IsRadioASelected = IsRadioBSelected = IsRadioCSelected = IsRadioDSelected = false;
            IsCheckASelected = IsCheckBSelected = IsCheckCSelected = IsCheckDSelected = IsCheckESelected = false;
            //跳转到指定题号
            CurrentQuestionIndex = TurnToNum - 1;
            CurrentQuestionInitial();
            CanPreQuestion = CurrentQuestionIndex != 0;
            CanNextQuestion = CurrentQuestionIndex != QuestionRangeCount - 1;
            IsRealResultVisible = false;
        }

        /// <summary>
        /// 只允许输入数字
        /// </summary>
        /// <param name="e"></param>
        public void PreviewTextInput(TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");  // new Regex("[^0-9.\\-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        /// <summary>
        /// 在跳转题号输入框中按下按键时
        /// </summary>
        /// <param name="o"></param>
        public void PreviewKeyDown(KeyEventArgs e)
        {
            var ctl = e.OriginalSource as TextBox;
            e.Handled = e.Key == Key.Space || e.Key == Key.OemPeriod;  //忽略空格键的输入
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (ctl.Text.Length <= 1 || ctl.SelectedText == ctl.Text)
                {
                    ctl.Text = "1";
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 查看历史记录
        /// </summary>
        public void ShowHistory()
        {
            string trainingRecorderPath = $"{Environment.CurrentDirectory}/training_recorder.txt";
            if (!File.Exists(trainingRecorderPath))
            {
                MessageHelper.Error(ResourceHelper.GetStrings("Common_FileNotExist"), MessageBoxButton.OK);
                return;
            }
            var history = "";
            using (FileStream fsRead = new FileStream(trainingRecorderPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fsRead.Length];
                fsRead.Read(buffer, 0, buffer.Length);
                history = Encoding.UTF8.GetString(buffer);
            }
            var historyView = new HistoryView
            {
                DataContext = new HistoryViewModel { History = history }
            };
            historyView.Show();
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Exit(object o)
        {
            //退出提醒
            if (MessageBoxX.Show(App.Current.MainWindow, ResourceHelper.GetStrings("Text_ExitConfirm"),
                    ResourceHelper.GetStrings("Common_ProgramName"), MessageBoxButton.YesNo
                    , MessageBoxIcon.Warning, DefaultButton.CancelNo) == MessageBoxResult.No)
            {
                if (o is CancelEventArgs args)
                    args.Cancel = true;
            }
            else
            {
                if (IsTrainingStart)
                    Commit();  //交卷
            }

            if (IsTrainingStart && IsRadioOrderTrainingSelected && IsRadioOrderTrainingSelected)
            {//顺序练习模式，保存最后浏览的题号
                LastReadingIndex = CurrentQuestionIndex;
            }
        }

        /// <summary>
        /// 跳转到上次浏览的位置
        /// </summary>
        public void TurnToLastReadingIndex()
        {
            CurrentQuestionIndex = LastReadingIndex;
            CurrentQuestionInitial();
            CanPreQuestion = CurrentQuestionIndex != 0;
            CanNextQuestion = CurrentQuestionIndex != QuestionRangeCount - 1;
            IsRealResultVisible = false;
            IsLastReadingIndexHintVisible = false;
        }

        /// <summary>
        /// 隐藏上次阅读位置提醒
        /// </summary>
        public void HideLastReadingIndexHint()
        {
            IsLastReadingIndexHintVisible = false;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// 计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            currentSeconds = (DateTime.Now.Ticks - startTimeTicks) / TimeSpan.TicksPerSecond;
            if (currentSeconds >= countSecond)
            {
                (sender as DispatcherTimer).Stop();
                Commit();  //自动交卷计算结果
                return;
            }
            CountDown = $"{(countSecond - currentSeconds) / 60}:{((countSecond - currentSeconds) % 60).ToString().PadLeft(2, '0')} / {countSecond / 60}:{(countSecond % 60).ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        private void CountDownStart()
        {
            countDownTimer = new DispatcherTimer();
            countDownTimer.Interval = new TimeSpan(0, 0, 0, 1);
            countDownTimer.Tick += Timer_Tick;
            startTimeTicks = DateTime.Now.Ticks;
            countDownTimer.Start();
        }

        /// <summary>
        /// 初始化答题板
        /// </summary>
        private void AnswerBoardInitial()
        {
            CurrentQuestionIndex = 0;
            CanPreQuestion = false;
            CanNextQuestion = true;
            answers = new string[QuestionRangeCount];  //初始化用户回答的结果
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
            CountDown = $"{(countSecond - startTimeTicks) / 60}:{((countSecond - startTimeTicks) % 60).ToString().PadLeft(2, '0')}/{countSecond / 60}:{(countSecond % 60).ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// 根据题库和题组大小生成随机题组
        /// </summary>
        private void GenerateRandomQuestionBank()
        {
            int[] questionRange = new int[QuestionRangeCount];
            for (int i = 0; i < QuestionRangeCount; i++)
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
        /// 根据题库生成顺序练习题组
        /// </summary>
        private void GenerateOrderQuestionBank()
        {
            int[] questionRange = new int[QuestionRangeCount];
            for (int i = 0; i < QuestionRangeCount; i++)
            {
                questionRange[i] = i;
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
                CommitA(true);
            else
                CommitA(false);
            if (answer.Contains("B"))
                CommitB(true);
            else
                CommitB(false);
            if (answer.Contains("C"))
                CommitC(true);
            else
                CommitC(false);
            if (answer.Contains("D"))
                CommitD(true);
            else
                CommitD(false);
            if (answer.Contains("E"))
                CommitE(true);
            else
                CommitE(false);
            if (answer.Contains("F"))
                CommitF(true);
            else
                CommitF(false);
        }

        private void CommitA(bool isCheckOrSelected)
        {
            IsRadioASelected = isCheckOrSelected;
            IsCheckASelected = isCheckOrSelected;
        }

        private void CommitB(bool isCheckOrSelected)
        {
            IsRadioBSelected = isCheckOrSelected;
            IsCheckBSelected = isCheckOrSelected;
        }

        private void CommitC(bool isCheckOrSelected)
        {
            IsRadioCSelected = isCheckOrSelected;
            IsCheckCSelected = isCheckOrSelected;
        }

        private void CommitD(bool isCheckOrSelected)
        {
            IsRadioDSelected = isCheckOrSelected;
            IsCheckDSelected = isCheckOrSelected;
        }

        private void CommitE(bool isCheckOrSelected)
        {
            IsCheckESelected = isCheckOrSelected;
        }

        private void CommitF(bool isCheckOrSelected)
        {
            IsCheckFSelected = isCheckOrSelected;
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
                if (IsCheckFSelected)
                    answer += "F";
            }
            //切换前先保存当前题目的答案
            answers[CurrentQuestionIndex] = answer;
        }
        #endregion
    }
}
