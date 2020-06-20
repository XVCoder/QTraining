using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.Models
{
    public class TrainingInfoModel
    {
        public int Id { get; set; }
        public string QIds { get; set; }
        public DateTime BeginTime { get; set; }
        public double Duration { get; set; }
        public string DoubtIds { get; set; }
        public string DoubtResults { get; set; }
    }
}
