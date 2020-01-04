using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_RespiratorOrder
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "客户名称")]
            public string CustomerName { get; set; }
             [Display(Name = "客户电话")]
            public string Customerphone { get; set; }
             [Display(Name = "客户地址")]
            public string Customeraddress { get; set; }
             [Display(Name = "日期")]
            public Nullable<System.DateTime> SaleDate { get; set; }
             [Display(Name = "出货单号")]
            public string SaleNumbers { get; set; }
             [Display(Name = "备注")]
            public string Remarks { get; set; }
             [Display(Name = "订单号")]
            public string OrderCode { get; set; }
             [Display(Name = "创建人")]
            public string CreateUser { get; set; }
        }
    }
}