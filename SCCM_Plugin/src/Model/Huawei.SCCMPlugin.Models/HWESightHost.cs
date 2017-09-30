using CommonUtil.ModelHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huawei.SCCMPlugin.Models
{
  [Serializable]  
  [DbTableName("HWESightHosts")]
  public class HWESightHost : BaseModel
  {
    int _hostPort = 32102;

    //[Key]
    //public long ID { get; set; }

    [JsonProperty(PropertyName = "hostIp")]
    [DbColumn("HOST_IP")]
    public String HostIP { get; set; }
    [JsonProperty(PropertyName = "hostPort")]
    [DbColumn("HOST_PORT")]
    public int HostPort
    {
      get { return _hostPort; }
      set { _hostPort = value; }
    }
    [JsonProperty(PropertyName = "aliasName")]
    [DbColumn("ALIAS_NAME")]
    public string AliasName
    {
      get;set;
    }
    [JsonProperty(PropertyName = "loginAccount")]
    [DbColumn("LOGIN_ACCOUNT")]
    public String LoginAccount { get; set; }

    [JsonProperty(PropertyName = "loginPwd")]
    [DbColumn("LOGIN_PWD")]
    public String LoginPwd { get; set; }

    [JsonIgnore]
    [DbColumn("CERT_PATH")]
    public String CertPath { get; set; }
    [JsonProperty(PropertyName = "latestStatus")]
    [DbColumn("LATEST_STATUS")]
    public String LatestStatus { get; set; }
    [JsonProperty(PropertyName = "reservedInt1")]
    [DbColumn("RESERVED_INT1")]
    public int ReservedInt1 { get; set; }
    [JsonProperty(PropertyName = "reservedInt2")]
    [DbColumn("RESERVED_INT2")]
    public int ReservedInt2 { get; set; }

    [JsonProperty(PropertyName = "reservedStr1")]
    [DbColumn("RESERVED_STR1")]
    public string ReservedStr1 { get; set; }

    [JsonProperty(PropertyName = "reservedStr2")]
    [DbColumn("RESERVED_STR2")]
    public string ReservedStr2 { get; set; }
    [JsonConverter(typeof(CustomJsonDateTimeConverter))]
    [JsonProperty(PropertyName = "lastModify")]
    [DbColumn("LAST_MODIFY_TIME")]
    public DateTime LastModifyTime { get; set; }
    [JsonConverter(typeof(CustomJsonDateTimeConverter))]
    [JsonProperty(PropertyName = "createTime")]
    [DbColumn("CREATE_TIME")]
    public DateTime CreateTime { get; set; }
  }
}
