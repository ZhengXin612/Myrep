using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Express
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Display(Name = "快递代码")]
            public string Code { get; set; }
             [Display(Name = "快递名称")]
             [Required(ErrorMessage = "不能为空")]
            public string Name { get; set; }
            public string Isdelete { get; set; }
        }
    }
}