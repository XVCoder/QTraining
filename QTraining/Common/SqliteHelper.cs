using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTraining.Common
{
    public class SqliteHelper
    {
        private SQLiteTransaction trs;
        private bool bIsOpen = false;
        private bool bIsTran = false;
        private string sConnectionString;
        private SQLiteConnection cnn;
        private SQLiteCommand cmd;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SqliteHelper(string connectString = "")
        {
            if (String.IsNullOrWhiteSpace(connectString))
            {
                sConnectionString = GetconnectString();
            }
            else
            {
                sConnectionString = connectString;
            }
            cnn = new SQLiteConnection();
            cmd = new SQLiteCommand();
        }

        #region Public method
        /// <summary>
        /// 指定SQL文进行查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Query(string sSql, ConcurrentDictionary<string, object> oParameter)
        {
            DataTable dt = new DataTable();
            try
            {
                DbOpen();
                cmd.CommandText = sSql;

                if (oParameter != null)
                {
                    cmd.Parameters.Clear();
                    foreach (string Key in oParameter.Keys)
                    {
                        cmd.Parameters.AddWithValue(Key, oParameter[Key]);
                    }
                }

                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd);
                dataAdapter.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
                dt = null;
            }
            finally
            {
                DbClose();
            }
            return dt;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="oParameter">参数</param>
        /// <param name="affectedRows">受影响的行数</param>
        /// <param name="newid">获取插入后的数据ID</param>
        /// <returns>0成功,-1失败</returns>
        public int DoExecute(string sql, ConcurrentDictionary<String, object> oParameter, out int affectedRows, out int newid)
        {
            int ret = 0;
            try
            {
                // 打开数据库
                DbOpen();


                // 执行SQL
                cmd.CommandText = sql;

                if (oParameter != null)
                {
                    cmd.Parameters.Clear();
                    foreach (string Key in oParameter.Keys)
                    {
                        cmd.Parameters.AddWithValue(Key, oParameter[Key]);
                    }
                }
                affectedRows = cmd.ExecuteNonQuery();
                cmd.CommandText = "select last_insert_rowid();";
                newid = Convert.ToInt32(cmd.ExecuteScalar());
                if (!bIsTran)
                {
                    DbClose();
                }
            }
            catch
            {
                ret = -1;
                affectedRows = 0;
                newid = -1;
            }
            return ret;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="affectedRows">受影响的行数</param>
        /// <param name="newid">获取插入后的数据ID</param>
        /// <returns>0成功,-1失败</returns>
        public int DoExecute(String sql, out int affectedRows, out int newid)
        {
            int ret = 0;
            try
            {
                // 打开数据库
                DbOpen();

                // 执行SQL
                cmd.CommandText = sql;
                affectedRows = cmd.ExecuteNonQuery();
                cmd.CommandText = "select last_insert_rowid();";
                newid = Convert.ToInt32(cmd.ExecuteScalar());
                if (!bIsTran)
                {
                    DbClose();
                }
            }
            catch (SQLiteException ex)
            {
                ret = -1;
                affectedRows = 0;
                newid = -1;
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
            catch (Exception ex)
            {
                ret = -1;
                affectedRows = 0;
                newid = -1;
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
            return ret;

        }
        /// <summary>
        /// 事务提交
        /// </summary>
        public void ExecCommitTrans()
        {
            try
            {
                trs.Commit();
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 事务回滚
        /// </summary>
        public void ExecRollbackTrans()
        {
            try
            {
                DbOpen();
                trs.Rollback();
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// Start Transaction
        /// </summary>
        public void ExecBeginTrans()
        {
            try
            {
                trs = cnn.BeginTransaction();
                cmd.Transaction = trs;
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTrans()
        {
            try
            {
                DbOpen();
                ExecBeginTrans();
                bIsTran = true;
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTrans()
        {
            try
            {
                DbOpen();
                ExecCommitTrans();
                bIsTran = true;
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTrans()
        {
            try
            {
                DbOpen();
                if (bIsTran)
                {
                    ExecRollbackTrans();
                }
                bIsTran = false;
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
            }
            finally
            {
                DbClose();
            }
        }

        /// <summary>
        /// SQL字符串格式化（将值为""或null转化为"null"，并给非空字符串套上单引号）
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>'value' or null</returns>
        public string SqlStringFormat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "null";
            else
                return "'" + value + "'";
        }

        /// <summary>
        /// 将DataColumn中的DateTime数据格式化
        /// </summary>
        /// <param name="dateTimeData">DateTime相关的DataColumn的数据</param>
        /// <returns>C#中的DateTime类型数据</returns>
        public DateTime DateTimeDataFormat(object dateTimeData)
        {
            if (dateTimeData == null)
                return DateTime.MinValue;
            try
            {
                return DateTime.Parse(dateTimeData?.ToString());
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
                return DateTime.MinValue;
            }
        }
        #endregion


        #region Private method
        /// <summary>
        /// 取得mysql连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetconnectString()
        {
            return ConfigurationManager.ConnectionStrings["sqlite"].ToString();
        }
        /// <summary>
        /// 打开连接
        /// </summary>
        public void DbOpen()
        {
            if (bIsOpen)
            {
                return;
            }

            if (cnn == null)
            {
                cnn = new SQLiteConnection();
            }
            cnn.ConnectionString = sConnectionString;
            cnn.Open();
            cmd.Connection = cnn;
            bIsOpen = true;

        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void DbClose()
        {
            if (cnn != null && bIsOpen)
            {
                cnn.Close();
                bIsOpen = false;
            }
            return;
        }

        /// <summary>
        /// 指定SQL文进行查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Query(String sSql)
        {
            DataTable dt = new DataTable();
            try
            {
                DbOpen();
                cmd.CommandText = sSql;

                SQLiteDataAdapter oracleDataAdapter = new SQLiteDataAdapter(cmd);
                oracleDataAdapter.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
                SysLogHelper.WriteLog(ex.Message);
                dt = null;
            }
            finally
            {
                DbClose();
            }
            return dt;
        }
        #endregion
    }
}