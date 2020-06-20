using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.Models
{
    public class QuestionInfoModel
    {
        public string Id { get; set; }
        public int ResultCount { get; set; }
        public string RealResult { get; set; }
        public string Note { get; set; }
    }
}
