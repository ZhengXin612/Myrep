using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_InvoiceApply
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "公司名称")]
            public string ShopName { get; set; }
            [Display(Name = "普票数量")]
            public Nullable<decimal> InvoiceMoney { get; set; }
            [Display(Name = "专票数量")]
            public Nullable<decimal> majorMoney { get; set; }
              [Display(Name = "申请人")]
            public string ApplyName { get; set; }
              [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
              [Display(Name = "状态")]
            public int Status { get; set; }
              [Display(Name = "步奏")]
            public int Step { get; set; }
              [Display(Name = "是否删除")]
            public int Isdelete { get; set; }
              [Display(Name = "备注")]
            public string Reason { get; set; }
        }
    }
}