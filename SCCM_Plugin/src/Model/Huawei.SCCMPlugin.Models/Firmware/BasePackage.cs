using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Huawei.SCCMPlugin.Models
{
    [Serializable]
    public class BasePackage
    {
        /// <summary>
        /// 升级包名称，必选，可由大小写字母、数字或- _构成的6-32字符。
        /// </summary>
        [JsonProperty(PropertyName = "basepackageName")]
        public string BasepackageName { get; set; }

        /// <summary>
        /// 升级包描述，必选，0-128字符。
        /// </summary>
        [JsonProperty(PropertyName = "basepackageDescription")]
        public string BasepackageDescription { get; set; }

        /// <summary>
        /// 升级包类型，必选，”Firmware”,”Driver”,”Bundle”
        /// </summary>
        [JsonProperty(PropertyName = "basepackageType")]
        public string BasepackageType { get; set; }

        /// <summary>
        /// 升级包文件列表，必选。必须放到sftp服务器的默认路径下。固件包和驱动包需要上传数字签名证书，Bundle包上传zip包
        /// </summary>
        [JsonProperty(PropertyName = "fileList")]
        public string FileList { get; set; }

        /// <summary>
        /// 客户搭建的sftp服务器的IP
        /// </summary>
        [JsonProperty(PropertyName = "sftpserverIP")]
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
