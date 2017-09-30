using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.Models
{
  class CustomJsonDateTimeConverter: IsoDateTimeConverter
  {
    public CustomJsonDateTimeConverter()
    {
      base.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    }
  }
}
