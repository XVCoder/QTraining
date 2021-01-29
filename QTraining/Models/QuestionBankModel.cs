namespace QTraining.Models
{
    public class QuestionBankModel
    {
        /// <summary>
        /// 题组名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 模拟考分钟数
        /// </summary>
        public int SimulationMinutes { get; set; }
        /// <summary>
        /// 题组大小（模拟测试题组）
        /// </summary>
        public int SimulationRangeCount { get; set; }
        /// <summary>
        /// 顺序练习分钟数
        /// </summary>
        public int OrderTrainingMinutes { get; set; }
        /// <summary>
        /// 题组路径
        /// </summary>
        public string QuestionBankRootPath { get; set; }
    }
}
