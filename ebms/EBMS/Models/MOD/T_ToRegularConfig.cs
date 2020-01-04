using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ToRegularConfig
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "名称")]
            public string Name { get; set; }
             [Display(Name = "步骤")]
            public int Step { get; set; }
             [Display(Name = "类型")]
            public string Type { get; set; }
             [Display(Name = "多人审核")]
            public string isMultiple { get; set; }
            
       
        }
    }
}