using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_AccountantProject
    {
        class MOD
        {
            public int ID { get; set; }
            public int ParentID { get; set; }
            [Display(Name = "会计科目编码")]
            [Required(ErrorMessage = "请输入会计科目编码")]
            public string Code { get; set; }
            [Display(Name = "会计科目名称")]
            [Required(ErrorMessage = "请输入会计科目名称")]
            public string Name { get; set; }
        }
    }
}