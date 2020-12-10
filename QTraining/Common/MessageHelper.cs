using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QTraining.Common
{
    public static class MessageHelper
    {
        /// <summary>
        /// 消息弹窗Base
        /// </summary>
        /// <param name="msg">消息主体</param>
        /// <param name="title">标题</param>
        /// <param name="button">按钮</param>
        /// <param name="icon">图标</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        private static MessageBoxResult MessageBase(string msg, string title, MessageBoxButton button, MessageBoxIcon icon, DefaultButton defaultButton, Window owner)
        {
            MessageBoxResult result = MessageBoxResult.No;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (owner == null)
                    owner = App.Current.MainWindow;
                result = MessageBoxX.Show(owner, msg, title, button, icon, defaultButton);
            }));
            return result;
        }


        /// <summary>
        /// Info消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="button">按钮</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        public static MessageBoxResult Info(string msg, Window owner = null, MessageBoxButton button = MessageBoxButton.OK, DefaultButton defaultButton = DefaultButton.YesOK)
        {
            return MessageBase(msg, ResourceHelper.GetStrings("Common_Info"), button, MessageBoxIcon.Info, defaultButton, owner);
        }

        /// <summary>
        /// Warning消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="button">按钮</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        public static MessageBoxResult Warning(string msg, Window owner = null, MessageBoxButton button = MessageBoxButton.YesNo, DefaultButton defaultButton = DefaultButton.CancelNo)
        {
            return MessageBase(msg, ResourceHelper.GetStrings("Common_Warning"), button, MessageBoxIcon.Warning, defaultButton, owner);
        }

        /// <summary>
        /// Error消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="button">按钮</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        public static MessageBoxResult Error(string msg, Window owner = null, MessageBoxButton button = MessageBoxButton.OK, DefaultButton defaultButton = DefaultButton.YesOK)
        {
            return MessageBase(msg, ResourceHelper.GetStrings("Common_Error"), button, MessageBoxIcon.Error, defaultButton, owner);
        }

        /// <summary>
        /// Question消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="button">按钮</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        public static MessageBoxResult Question(string msg, Window owner = null, MessageBoxButton button = MessageBoxButton.YesNo, DefaultButton defaultButton = DefaultButton.CancelNo)
        {
            return MessageBase(msg, ResourceHelper.GetStrings("Common_Question"), button, MessageBoxIcon.Question, defaultButton, owner);
        }

        /// <summary>
        /// Success消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="button">按钮</param>
        /// <param name="owner">窗体拥有者（为空则取当前的MainWindow）</param>
        public static MessageBoxResult Success(string msg, Window owner = null, MessageBoxButton button = MessageBoxButton.OK, DefaultButton defaultButton = DefaultButton.YesOK)
        {
            return MessageBase(msg, ResourceHelper.GetStrings("Common_Success"), button, MessageBoxIcon.Success, defaultButton, owner);
        }
    }
}
