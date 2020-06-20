using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.Models
{
    public class DoubtInfoModel
    {
        public int Id { get; set; }
        public string QId { get; set; }
        public bool Validated { get; set; }
        public string Description { get; set; }
    }
}
