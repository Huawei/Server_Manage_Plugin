using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
    [Serializable]
    public class SoftwareSource
    {

        /// <summary>
        /// 软件源名称，必选，可由大小写字母、数字或- _构成的6-32字符。
        /// </summary>
        [JsonProperty(PropertyName = "softwareName")]
        public string SoftwareName { get; set; }

        /// <summary>
        /// 模板描述，必选，0-128字符。
        /// </summary>
        [JsonProperty(PropertyName = "softwareDescription")]
        public string SoftwareDescription { get; set; }

        /// <summary>
        /// OS类型，必选，上传Windows OS软件源填写 "Windows"
        /// </summary>
        [JsonProperty(PropertyName = "softwareType")]
        public string SoftwareType { get; set; }

        /// <summary>
        /// String	软件源版本，必选，
        /// </summary>
        [JsonProperty(PropertyName = "softwareVersion")]
        public string SoftwareVersion { get; set; }

        [JsonProperty(PropertyName = "softwareEdition")]
        public string SoftwareEdition { get; set; }
        /// <summary>
        /// 软件源语言，必选
        /// </summary>
        [JsonProperty(PropertyName = "softwareLanguage")]
        public string SoftwareLanguage { get; set; }

        /// <summary>
        /// 镜像名称，必选。必须放到sftp服务器的默认路径下
        /// </summary>
        [JsonProperty(PropertyName = "sourceName")]
        public string SourceName { get; set; }

        /// <summary>
        /// 客户搭建的sftp服务器的IP
        /// </summary>
        [JsonProperty(PropertyName = "sftpServerIP")]
        public string SftpserverIP { get; set; }

        /// <summary>
        /// Sftp服务器的用户名
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string SftpUserName { get; set; }

        /// <summary>
        /// Sftp服务器的密码
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string SftpPassword { get; set; }

        [JsonProperty(PropertyName = "port")]
        public string SftpPort { get; set; }
    }
}
