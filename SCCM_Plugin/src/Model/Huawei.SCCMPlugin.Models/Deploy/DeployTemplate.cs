using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models.Deploy
{
  [Serializable]
  public class DeployTemplate
  {
    [JsonProperty(PropertyName = "templateName")]
    public string TemplateName { get; set; }
    [JsonProperty(PropertyName = "templateType")]
    public string TemplateType { get; set; }
    [JsonProperty(PropertyName = "templateDesc")]
    public string TemplateDesc { get; set; }
    [JsonProperty(PropertyName = "templateProp")]
    public JObject TemplateProp { get; set; }
  }
}
