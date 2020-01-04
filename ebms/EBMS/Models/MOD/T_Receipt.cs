using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Receipt
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "订单号")]
            [Required(ErrorMessage = "订单号不能为空")]
            public string OrderCode { get; set; }
            [Display(Name = "收货人")]
            [Required(ErrorMessage = "收货人不能为空")]
            public string Receivings { get; set; }
            [Display(Name = "客户")]
            [Required(ErrorMessage="客户不能为空")]
            public string Vip_Name { get; set; }
            [Display(Name = "销售单位")]
            [Required(ErrorMessage = "销售单位不能为空")]
            public string StorName { get; set; }
            [Display(Name = "金额")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public decimal Money { get; set; }
            public System.DateTime Date { get; set; }
            public System.DateTime ShopDate { get; set; }
            public int PrintCount { get; set; }
            [Display(Name = "收款单号")]
            public string Code { get; set; }
            public string PostUser { get; set; }
        }
    }
}