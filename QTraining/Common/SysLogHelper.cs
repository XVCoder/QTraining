using System;
using System.IO;

namespace QTraining.Common
{
    public static class SysLogHelper
    {
        private readonly static string logDir = Environment.CurrentDirectory.ToString() + "\\syslog";

        /// <summary>
        /// 输出系统日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            string filePath = logDir + "\\syslog";
            StreamWriter tw = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
                using (tw = File.AppendText(filePath))
                {
                    tw.WriteLine(DateTime.Now.ToString() + "> " + msg);
                    tw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                tw?.Close();
            }
        }
    }
}
