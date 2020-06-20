using QTraining.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.DAL
{
    public class DALBase
    {
        public DALBase()
        {
            sqliteHelper = new SqliteHelper();
        }

        public SqliteHelper sqliteHelper;
    }
}
