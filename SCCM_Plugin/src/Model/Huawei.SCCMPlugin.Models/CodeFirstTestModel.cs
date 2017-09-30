using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtil.ModelHelper;

namespace Huawei.SCCMPlugin.Models
{
    /// <summary>
    /// Class CodeFirstTestModel.
    /// </summary>
    [DbTableName("CodeFirstTestModels")]
    public class CodeFirstTestModel:BaseModel
    {
      
        //public long ID { get; set; }

     
        public string Name { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
