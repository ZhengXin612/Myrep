using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
      [MetadataType(typeof(MOD))]
    public partial class T_ShopFromGY
    {
          class MOD
          {
              [Display(Name = "主键")]
              public int ID { get; set; }
               [Display(Name = "店铺名称")]
              public string name { get; set; }
               [Display(Name = "店铺代码")]
              public string code { get; set; }
               [Display(Name = "平台")]
              public string type_name { get; set; }
               [Display(Name = "别称")]
              public string nick { get; set; }
               [Display(Name = "公司")]
              public string company_Name { get; set; }
               [Display(Name = "账号")]
              public string number { get; set; }
               [Display(Name = "法人")]
              public string DutyFinance { get; set; }
              public string Isdelete { get; set; }
          }
    }
}