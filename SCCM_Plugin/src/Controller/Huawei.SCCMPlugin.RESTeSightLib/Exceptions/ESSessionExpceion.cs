using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// eSession 错误类
    /// </summary>
    [Serializable]
    public class ESSessionExpceion : BaseException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="esSession">错误对象</param>
        /// <param name="message">正文</param>
        public ESSessionExpceion(string code, IESSession esSession, string message) : base(code, esSession, message) { }

    }
}
