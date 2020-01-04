using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_AssetsType
    {
         class MOD
         {
              [Display(Name = "类型名称")]
             [Required(ErrorMessage = "不能为空")]
             [MaxLength(200)]
             public string AssetsTypeName { get; set; }
              [Display(Name = "类型编码")]
              [Required(ErrorMessage = "不能为空")]
              [MaxLength(100)]
             public string AssetsTypecode { get; set; }
         }
    }
}