using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.Models
{
    public class QuestionBankModel
    {
        /// <summary>
        /// 题组名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 题组大小（模拟测试题组）
        /// </summary>
        public int QuestionRangeCount { get; set; }
        /// <summary>
        /// 分钟数
        /// </summary>
        public int Minutes { get; set; }
    }
}
