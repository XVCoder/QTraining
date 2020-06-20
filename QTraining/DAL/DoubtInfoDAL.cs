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
    public class DoubtInfoDAL : DALBase
    {
        /*==============================================================================================================*/
        #region Get
        /// <summary>
        /// 获取数据条数
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            string sql = "select count(0) from doubtinfo";
            return int.Parse(sqliteHelper.Query(sql).Rows[0][0].ToString());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        public List<DoubtInfoModel> GetAll()
        {
            var lst = new List<DoubtInfoModel>();
            string sql = "select * from doubtinfo";
            var dt = sqliteHelper.Query(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var model = new DoubtInfoModel
                    {
                        Id = int.Parse(dr["id"]?.ToString()),
                        QId = dr["qid"]?.ToString(),
                        Validated = bool.Parse(dr["validated"]?.ToString()),
                        Description = dr["description"]?.ToString()
                    };
                    lst.Add(model);
                }
            }
            return lst;
        }
        #endregion
        /*==============================================================================================================*/
        #region Add
        public int Add(DoubtInfoModel model, out int newId)
        {
            var sql = new StringBuilder();
            sql.AppendLine("insert into doubtinfo(");
            sql.Append("id");
            sql.Append(",qid");
            sql.Append(",validated");
            sql.Append(",description");
            sql.AppendLine(") values(");
            sql.Append(model.Id);
            sql.Append("," + sqliteHelper.SqlStringFormat(model.QId));
            sql.Append("," + model.Validated.ToString());
            sql.Append(",@Description");
            sql.Append(")");
            var oParams = new ConcurrentDictionary<string, object>();
            oParams.TryAdd("@Description", model.Description);
            sqliteHelper.DoExecute(sql.ToString(), oParams, out int affectedRows, out newId);
            return affectedRows;
        }
        #endregion
        /*==============================================================================================================*/
        #region Edit
        public int Edit(DoubtInfoModel model)
        {
            var sql = new StringBuilder();
            sql.AppendLine("update doubtinfo set ");
            sql.Append("validated=" + model.Validated);
            sql.Append(",description=@Description");
            sql.Append(" where id=" + model.Id);
            var oParams = new ConcurrentDictionary<string, object>();
            oParams.TryAdd("@Description", model.Description);
            sqliteHelper.DoExecute(sql.ToString(), oParams, out int affectedRows, out _);
            return affectedRows;
        }
        #endregion
        /*==============================================================================================================*/
    }
}
