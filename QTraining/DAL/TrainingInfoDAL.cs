using QTraining.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.DAL
{
    public class TrainingInfoDAL : DALBase
    {
        /*==============================================================================================================*/
        #region Get
        /// <summary>
        /// 获取数据条数
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            string sql = "select count(0) from traininginfo";
            return int.Parse(sqliteHelper.Query(sql).Rows[0][0].ToString());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        public List<TrainingInfoModel> GetAll()
        {
            var lst = new List<TrainingInfoModel>();
            string sql = "select * from traininginfo";
            var dt = sqliteHelper.Query(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var model = new TrainingInfoModel
                    {
                        Id = int.Parse(dr["id"]?.ToString()),
                        QIds = dr["qids"]?.ToString(),
                        BeginTime = DateTime.Parse(dr["begintime"]?.ToString()),
                        Duration = double.Parse(dr["duration"]?.ToString()),
                        DoubtIds = dr["doubtids"]?.ToString(),
                        DoubtResults = dr["doubtresults"]?.ToString()
                    };
                    lst.Add(model);
                }
            }
            return lst;
        }
        #endregion
        /*==============================================================================================================*/
        #region Add
        public int Add(TrainingInfoModel model, out int newId)
        {
            var sql = new StringBuilder();
            sql.AppendLine("insert into traininginfo(");
            sql.Append("id");
            sql.Append(",qids");
            sql.Append(",begintime");
            sql.Append(",duration");
            sql.Append(",doubtids");
            sql.Append(",doubtresults");
            sql.AppendLine(") values(");
            sql.Append(model.Id);
            sql.Append("," + model.QIds);
            sql.Append("," + sqliteHelper.DateTimeDataFormat(model.BeginTime));
            sql.Append("," + model.Duration.ToString());
            sql.Append("," + model.DoubtIds);
            sql.Append("," + model.DoubtResults);
            sql.Append(")");
            sqliteHelper.DoExecute(sql.ToString(), out int affectedRows, out newId);
            return affectedRows;
        }
        #endregion
        /*==============================================================================================================*/
        #region Edit
        public int Edit(TrainingInfoModel model)
        {
            return 0;
        }
        #endregion
        /*==============================================================================================================*/
    }
}
