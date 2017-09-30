using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace LogUtil
{
    /// <summary>
    /// 日志接口实现类
    /// </summary>
    class Logger : ILogger
    {

        private NLog.Logger log4 = null;
        /// <summary>
        /// NLOG 日志类。
        /// </summary>
        public NLog.Logger RawLog
        {
            get { return log4; }
        }
        /// <summary>
        /// 日志类构造方法
        /// </summary>
        /// <param name="logname">日志的名称,建议直接使用HWLogger类来写日志</param>
        public Logger(string logname)
        {
            log4 = LogManager.GetLogger(logname);
        }
        
        /// <summary>
        /// 将日志中密码隐藏。
        /// </summary>
        /// <param name="obj">日志正文</param>
        /// <returns></returns>
        private string EncryptPassword(object obj)
        {
            try
            {
                if (obj == null) obj = string.Empty;
                string str = obj.ToString();
                Dictionary<string, Regex> regexes = new Dictionary<string, Regex>();
                regexes[@"password=********"] = new Regex(@"password=[\w.]+\b", RegexOptions.IgnoreCase);
                regexes[@"password=********&"] = new Regex(@"password=[\w.]+\&", RegexOptions.IgnoreCase);
                regexes[@"$1******** "] = new Regex(@"(mysql.exe.*-p)[\w]+ ", RegexOptions.IgnoreCase);
                regexes[@"$1********"] = new Regex(@"(mysql.exe.*-p)[\w]+\b", RegexOptions.IgnoreCase);

                foreach (KeyValuePair<string, Regex> keyValuePair in regexes)
                {
                    str = keyValuePair.Value.Replace(str, keyValuePair.Key);
                }
                return str;
            }
            catch
            {
                return GetObjTranNull<string>(obj);
            }
        }
        /// <summary>
        /// 注：因为日志类不依赖任何类，因此此处copy了一份CoreUtil的去空转类型函数。
        /// 1. 类型转换
        /// 2. 对象null值处理，方便一些对象为null时，返回缺省的类型初值。
        /// 如：string 为null时，返回空字符串。        
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="obj">需要转换的对象</param>
        /// <returns></returns>
        private static T GetObjTranNull<T>(Object obj)
        {
            try
            {
                //处理数据库空值返回空字符串转换的结果;字符串为Null返回空字符.
                if ((obj == null || obj == DBNull.Value) && (typeof(T) == typeof(string)))
                {
                    return (T)Convert.ChangeType("", typeof(T));
                }
                //如果对象是转换类型时,直接返回.
                if (obj is T)
                {
                    return (T)obj;
                }
                //处理可空类型.
                if (typeof(T).IsGenericType)
                {
                    Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                    {
                        return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeof(T)));
                    }
                }
                //强制转换.
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (InvalidCastException)
            {
                try
                {
                    //自动转换类型失败时,强制转换.
                    if (obj != null)
                    {
                        return (T)obj;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //返回缺省值.
            return default(T);
        }
        #region "log interface."
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="msg"></param>
        public void Log(object message)
        {
            log4.Error(EncryptPassword(message));
        }
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="ex">错误类</param>
        public void Log(Exception e)
        {
            log4.Error(e);
        }
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Log(object message, Exception e)
        {
            log4.ErrorException(EncryptPassword(message), e);
        }
        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        public void Debug(object msg)
        {
            log4.Debug(EncryptPassword(msg));
        }
        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Debug(object msg, Exception ex)
        {
            log4.DebugException(EncryptPassword(msg), ex);
        }
        /// <summary>
        /// 输出错误日志 Debug 级别。
        /// 支持string.format 的占位符方式.
        /// 如：DebugFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        public void DebugFormat(string msg, params object[] args)
        {
            log4.Debug(EncryptPassword(string.Format(msg, args)));
        }
        /// <summary>
        /// 输出错误日志 Info 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        public void Info(object msg)
        {
            log4.Info(EncryptPassword(msg));
        }
        /// <summary>
        /// 输出错误日志 Info 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Info(object msg, Exception ex)
        {
            log4.InfoException(EncryptPassword(msg), ex);
        }
        /// <summary>
        /// 输出错误日志 Info 级别。
        /// 支持string.format 的占位符方式.
        /// 如：InfoFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        public void InfoFormat(string msg, params object[] args)
        {
            log4.Info(EncryptPassword(string.Format(msg, args)));
        }

        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        public void Warn(object msg)
        {
            log4.Warn(EncryptPassword(msg));
        }
        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Warn(object msg, Exception ex)
        {
            log4.WarnException(EncryptPassword(msg), ex);
        }
        /// <summary>
        /// 输出错误日志 Warn 级别。
        /// 支持string.format 的占位符方式.
        /// 如：InfoFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        public void WarnFormat(string msg, params object[] args)
        {
            log4.Warn(string.Format(msg, args));
        }

        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        public void Error(object msg)
        {
            log4.Error(EncryptPassword(msg));
        }
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Error(object msg, Exception ex)
        {
            log4.ErrorException(EncryptPassword(msg), ex);
        }
        /// <summary>
        /// 输出错误日志 Error 级别。
        /// 支持string.format 的占位符方式.
        /// 如：ErrorFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        public void ErrorFormat(string msg, params object[] args)
        {
            log4.Error(EncryptPassword(string.Format(msg, args)));
        }

        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// </summary>>
        /// <param name="msg">错误消息体</param>
        public void Fatal(object msg)
        {
            log4.Fatal(EncryptPassword(msg));
        }
        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// </summary>
        /// <param name="msg">错误消息体</param>
        /// <param name="ex">错误类</param>
        public void Fatal(object msg, Exception ex)
        {
            log4.FatalException(EncryptPassword(msg), ex);
        }
        /// <summary>
        /// 输出错误日志 Fatal 级别。
        /// 支持string.format 的占位符方式.
        /// 如：FatalFormat("title:{0}",titleVar)
        /// </summary>
        /// <param name="msg">消息正文</param>
        /// <param name="args">带入参数,数组类型</param>
        public void FatalFormat(string msg, params object[] args)
        {
            log4.Fatal(EncryptPassword(string.Format(msg, args)));
        }

        #endregion
    }
}
