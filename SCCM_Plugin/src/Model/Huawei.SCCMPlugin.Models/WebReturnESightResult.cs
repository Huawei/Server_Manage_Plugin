using CommonUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
    [Serializable]
    public class WebReturnESightResult
    {
        /// <summary>
        /// 服务器。
        /// </summary>
        [JsonProperty(PropertyName = "esight")]
        public string ESightIp { get; set; }
        /// <summary>
        /// 操作返回码。可以是如下值之一：
        ///	0：成功
        ///	非0：失败
        /// </summary>
        [JsonIgnore]
        public int Code { get; set; }

        /// <summary>
        /// deploy.error.+ -1
        /// ErrorModel+Code
        /// 返回给前端的错误码。
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string RetCode
        {
            get { return ErrorModel + CoreUtil.GetObjTranNull<string>(Code); }
        }
        private string _errorModel = "";

        /// <summary>
        /// deploy.error.
        /// 错误模块
        /// </summary>
        [JsonProperty(PropertyName = "errorModel")]
        public string ErrorModel
        {
            get { return _errorModel; }
            set { _errorModel = value; }
        }
        /// <summary>
        /// 接口调用结果的描述信息。
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
