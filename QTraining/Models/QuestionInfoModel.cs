namespace QTraining.Models
{
    public class QuestionInfoModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 答案个数
        /// </summary>
        public int ResultCount { get; set; }
        /// <summary>
        /// 正确答案
        /// </summary>
        public string RealResult { get; set; }
        /// <summary>
        /// 笔记/解析
        /// </summary>
        public string Note { get; set; }
    }
}
