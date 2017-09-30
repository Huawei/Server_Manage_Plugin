using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// 自定义错误，基类Exception
    /// </summary>
    [Serializable]    
    public class BaseException : ApplicationException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="eventSrcObject">错误对象</param>
        /// <param name="message">正文</param>
        public BaseException(string code, Object eventSrcObject, string message) : base(message)
        {
            Code = code;
            Message = message;
        }
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="errorModel">错误模块名</param>
        /// <param name="code">错误码</param>
        /// <param name="eventSrcObject">错误对象</param>
        /// <param name="message">正文</param>
        public BaseException(string errorModel,string code, Object eventSrcObject, string message) : base(message)
        {
            ErrorModel = errorModel;
            Code = code;
            Message = message;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public string Code { get; set; }

        private string _errorModel = "";
        
        public string ErrorModel
        {
            get { return _errorModel; }
            set { _errorModel = value; }
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }
    }
}
