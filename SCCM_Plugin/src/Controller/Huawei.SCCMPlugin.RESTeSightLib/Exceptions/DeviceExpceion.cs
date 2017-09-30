using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions
{
    /// <summary>
    /// 服务器查询错误类
    /// </summary>
    [Serializable]
    public class DeviceExpceion : BaseException
    {
        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="deviceWorker">错误对象</param>
        /// <param name="message">正文</param>
        public DeviceExpceion(string code, IDeviceWorker deviceWorker, string message) : base(code, deviceWorker, message) { }

        /// <summary>
        /// 错误类构造方法
        /// </summary>
        /// <param name="errorModel"></param>
        /// <param name="code">错误码</param>
        /// <param name="deviceWorker">错误对象</param>
        /// <param name="message">正文</param>
        public DeviceExpceion(string errorModel, string code, IDeviceWorker deviceWorker, string message) : base(errorModel, code, deviceWorker, message)
        {

        }
    }
}
