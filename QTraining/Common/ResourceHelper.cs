using System.Collections.Generic;
using System.Text;

namespace QTraining.Common
{
    public class ResourceHelper
    {
        /// <summary>
        /// 获取一个或多个静态字符串资源
        /// </summary>
        /// <param name="keys">字符串对应的键</param>
        /// <returns></returns>
        public static string GetStrings(params string[] keys)
        {
            return GetStringsWithSeparate(" ", keys);
        }
        /// <summary>
        /// 获取多个静态字符串资源，字符串之间用指定的隔断符隔开
        /// </summary>
        /// <param name="separateStr">字符串之间的隔断符</param>
        /// <param name="keys">字符串对应的键</param>
        /// <returns></returns>
        public static string GetStringsWithSeparate(string separateStr = " ", params string[] keys)
        {
            List<string> lstStr = new List<string>();
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    lstStr.Add(App.Current.TryFindResource(key)?.ToString());
                }
            }
            return string.Join(separateStr, lstStr.ToArray());
        }
    }
}
