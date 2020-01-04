using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Suppliers
    {
        class MOD
        {
           
           
           
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Required(ErrorMessage = "不能为空")]
            [MaxLength(100)]
             [Display(Name = "供应商名称")]
            public string SuppliersName { get; set; }
             [Display(Name = "联系人")]
             [MaxLength(100)]
            public string ContactName { get; set; }
             [Display(Name = "联系电话")]
             [MaxLength(100)]
            public string ContactiTelephone { get; set; }


        }
    }
}