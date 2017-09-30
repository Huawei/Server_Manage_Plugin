using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// 固件升级错误类
    /// </summary>
    [Serializable]
    public class BasePackageExpceion : BaseException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="basePackageWorker">错误对象</param>
        /// <param name="message">正文</param>
        public BasePackageExpceion(string code, IBasePackageWorker basePackageWorker, string message) : base(code, basePackageWorker, message) { }
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="errorModel"></param>
        /// <param name="code">错误码</param>
        /// <param name="basePackageWorker">错误对象</param>
        /// <param name="message">正文</param>
        public BasePackageExpceion(string errorModel, string code, IBasePackageWorker basePackageWorker, string message) : base(errorModel, code, basePackageWorker, message)
        {

        }

    }
}
