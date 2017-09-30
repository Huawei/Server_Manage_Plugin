using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Softwares
{
    [Serializable]
    public class SourceItem
    {
        private string _softwareName = "";
        private string _softwareDescription = "";
        private string _softwareType = "";

        /// <summary>
        /// 软件模板名称
        /// </summary>
        [JsonProperty(PropertyName = "softwareName")]
        public string softwareName {
            get { return _softwareName; }
            set {
                _softwareName = value;
            }
        }

        [JsonProperty(PropertyName = "softwarsourceName")]
        public string softwarName
        {
            get { return _softwareName; }
            set
            {
                _softwareName = value;
            }
        }
        /// <summary>
        /// 软件模板描述
        /// </summary>
        [JsonProperty(PropertyName = "softwareDescription")]
        public string softwareDescription
        {
            get { return _softwareDescription; }
            set
            {
                _softwareDescription = value;
            }
        }

        [JsonProperty(PropertyName = "softwarsourceDesc")]
        public string softwarDescription
        {
            get { return _softwareDescription; }
            set
            {
                _softwareDescription = value;
            }
        }
        /// <summary>
        /// 软件模板明细
        /// </summary>
        [JsonProperty(PropertyName = "softwareType")]
        public string softwareType
        {
            get { return _softwareType; }
            set
            {
                _softwareType = value;
            }
        }
        [JsonProperty(PropertyName = "softwarsourceDetail")]
        public string softwarsourceDetail
        {
            get { return _softwareType; }
            set
            {
                _softwareType = value;
            }
        }
    }
}
