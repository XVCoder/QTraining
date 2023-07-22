using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Data.SQLite;

namespace QTraining.Common
{
    public class SqliteHelper
    {
        private SQLiteTransaction trs;
        private bool _isOpen = false;
        private bool _isTran = false;
        private string _connectionString;
        private SQLiteConnection _cnn;
        private SQLiteCommand _cmd;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SqliteHelper(string connectString = "")
        {
            if (String.IsNullOrWhiteSpace(connectString))
            {
                _connectionString = GetconnectString();
            }
            else
            {
                _connectionString = connectString;
            }
            _cnn = new SQLiteConnection();
            _cmd = new SQLiteCommand();
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
                _cmd.CommandText = sSql;

                if (oParameter != null)
                {
                    _cmd.Parameters.Clear();
                    foreach (string Key in oParameter.Keys)
                    {
                        _cmd.Parameters.AddWithValue(Key, oParameter[Key]);
                    }
                }

                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(_cmd);
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
                _cmd.CommandText = sql;

                if (oParameter != null)
                {
                    _cmd.Parameters.Clear();
                    foreach (string Key in oParameter.Keys)
                    {
                        _cmd.Parameters.AddWithValue(Key, oParameter[Key]);
                    }
                }
                affectedRows = _cmd.ExecuteNonQuery();
                _cmd.CommandText = "select last_insert_rowid();";
                newid = Convert.ToInt32(_cmd.ExecuteScalar());
                if (!_isTran)
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
                _cmd.CommandText = sql;
                affectedRows = _cmd.ExecuteNonQuery();
                _cmd.CommandText = "select last_insert_rowid();";
                newid = Convert.ToInt32(_cmd.ExecuteScalar());
                if (!_isTran)
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
                trs = _cnn.BeginTransaction();
                _cmd.Transaction = trs;
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
                _isTran = true;
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
                _isTran = true;
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
                if (_isTran)
                {
                    ExecRollbackTrans();
                }
                _isTran = false;
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
            if (_isOpen)
            {
                return;
            }

            if (_cnn == null)
            {
                _cnn = new SQLiteConnection();
            }
            _cnn.ConnectionString = _connectionString;
            _cnn.Open();
            _cmd.Connection = _cnn;
            _isOpen = true;

        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void DbClose()
        {
            if (_cnn != null && _isOpen)
            {
                _cnn.Close();
                _isOpen = false;
            }
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
                _cmd.CommandText = sSql;

                SQLiteDataAdapter oracleDataAdapter = new SQLiteDataAdapter(_cmd);
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