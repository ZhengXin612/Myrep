using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_Company
    {
         class MOD
         {
             [Display(Name = "主键")]
             public int ID { get; set; }
             [Required(ErrorMessage = "不能为空")]
             [MaxLength(100)]
             [Display(Name = "单位名称")]
             public string CompanyName { get; set; }
              [Display(Name = "备注")]
             public string Remarks { get; set; }
             public string Isdelete { get; set; }
         }
    }
}