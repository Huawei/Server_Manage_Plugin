using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// 模板错误类
    /// </summary>
    [Serializable]
    public class DeployException : BaseException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="deployWorker">错误对象</param>
        /// <param name="message">正文</param>
        public DeployException(string code, IHWDeployWorker deployWorker, string message) : base("deploy.error.", code, deployWorker, message) { }


        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="errorModel"></param>
        /// <param name="code">错误码</param>
        /// <param name="deployWorker">错误对象</param>
        /// <param name="message">正文</param>
        public DeployException(string errorModel, string code, IHWDeployWorker deployWorker, string message) : base(errorModel, code, deployWorker, message)
        {

        }
    }
}
