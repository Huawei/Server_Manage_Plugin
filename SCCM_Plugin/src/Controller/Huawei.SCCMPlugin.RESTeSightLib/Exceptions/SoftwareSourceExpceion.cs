using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// 软件源 错误类
    /// </summary>
    [Serializable]
    public class SoftwareSourceExpceion : BaseException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="softwareSource">错误对象</param>
        /// <param name="message">正文</param>
        public SoftwareSourceExpceion( string code, ISoftwareSourceWorker softwareSource, string message) : base("deploy.error.", code, softwareSource, message) {
        }
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="errorModel"></param>
        /// <param name="code">错误码</param>
        /// <param name="softwareSource">错误对象</param>
        /// <param name="message">正文</param>
        public SoftwareSourceExpceion( string errorModel, string code, ISoftwareSourceWorker softwareSource, string message) : base(errorModel, code, softwareSource, message)
        {

        }

    }
}
