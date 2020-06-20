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
    public class QuestionInfoDAL : DALBase
    {
        /*==============================================================================================================*/
        #region Get
        /// <summary>
        /// 获取数据条数
        /// </summary>
        /// <returns></returns>
        public int GetDataCount()
        {
            string sql = "select count(0) from questioninfo";
            return int.Parse(sqliteHelper.Query(sql).Rows[0][0].ToString());
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        public List<QuestionInfoModel> GetAll()
        {
            var lst = new List<QuestionInfoModel>();
            string sql = "select * from questioninfo";
            var dt = sqliteHelper.Query(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var model = new QuestionInfoModel
                    {
                        Id = dr["id"]?.ToString(),
                        ResultCount = int.Parse(dr["questioncount"]?.ToString()),
                        RealResult = dr["realresult"]?.ToString(),
                        Note = dr["note"]?.ToString()
                    };
                    lst.Add(model);
                }
            }
            return lst;
        }
        #endregion
        /*==============================================================================================================*/
        #region Add
        public int Add(QuestionInfoModel model, out int newId)
        {
            var sql = new StringBuilder();
            sql.AppendLine("insert into questioninfo(");
            sql.Append("id");
            sql.Append(",resultcount");
            sql.Append(",realresult");
            sql.Append(",note");
            sql.AppendLine(") values(");
            sql.Append(model.Id);
            sql.Append("," + model.ResultCount.ToString());
            sql.Append("," + model.RealResult);
            sql.Append(",@Note");
            sql.Append(")");
            var oParams = new ConcurrentDictionary<string, object>();
            oParams.TryAdd("@Note", model.Note);
            sqliteHelper.DoExecute(sql.ToString(), oParams, out int affectedRows, out newId);
            return affectedRows;
        }
        #endregion
        /*==============================================================================================================*/
        #region Edit
        public int Edit(QuestionInfoModel model)
        {
            var sql = new StringBuilder();
            sql.AppendLine("update questioninfo set ");
            sql.Append("validated=@Note");
            sql.Append(" where id=" + model.Id);
            var oParams = new ConcurrentDictionary<string, object>();
            oParams.TryAdd("@Note", model.Note);
            sqliteHelper.DoExecute(sql.ToString(), oParams, out int affectedRows, out _);
            return affectedRows;
        }
        #endregion
        /*==============================================================================================================*/
    }
}
