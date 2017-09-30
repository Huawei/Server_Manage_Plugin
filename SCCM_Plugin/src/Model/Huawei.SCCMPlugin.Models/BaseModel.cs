using CommonUtil.ModelHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
   public class BaseModel
    {
    [JsonProperty(PropertyName = "id",IsReference =true)]
        [DbColumn("ID")]
        public int ID { get; set; }
    }
}
