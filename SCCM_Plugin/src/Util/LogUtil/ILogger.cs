using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtil
{
    /// <summary>
    /// 日志接口类
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="msg"></param>
        void Log(object msg);
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="ex">错误类</param>
        void Log(Exception ex);
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Log(object msg, Exception ex);

        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        void Debug(object msg);
        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Debug(object msg, Exception ex);
        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// 支持string.format 的占位符方式.
        /// 如：DebugFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        void DebugFormat(string msg, params object[] args);

        /// <summary>
        /// 输出错误日志 Info 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        void Info(object msg);
        /// <summary>
        /// 输出错误日志 Info 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Info(object msg, Exception ex);
        /// <summary>
        /// 输出错误日志 Info 级别。
        /// 支持string.format 的占位符方式.
        /// 如：InfoFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        void InfoFormat(string msg, params object[] args);

        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        void Warn(object msg);
        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Warn(object msg, Exception ex);
        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// 支持string.format 的占位符方式.
        /// 如：InfoFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        void WarnFormat(string msg, params object[] args);

        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        void Error(object msg);
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Error(object msg, Exception ex);
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// 支持string.format 的占位符方式.
        /// 如：ErrorFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        void ErrorFormat(string msg, params object[] args);

        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        void Fatal(object msg);
        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        void Fatal(object msg, Exception ex);
        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// 支持string.format 的占位符方式.
        /// 如：FatalFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        void FatalFormat(string msg, params object[] args);

        /// <summary>
        /// Nlog对象实例
        /// </summary>
        NLog.Logger RawLog { get; }
    }
}
