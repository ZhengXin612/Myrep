
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_CashBack
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "订单号")]
            public string OrderNum { get; set; }
            [Display(Name = "会员名称")]
            public string VipName { get; set; }
            [Display(Name = "店铺名称")]
            public string ShopName { get; set; }
            [Display(Name = "旺旺帐号")]
            public string WangWang { get; set; }
            [Display(Name = "返现原因")]
            public string Reason { get; set; }
            [Display(Name = "返现金额")]
            public decimal BackMoney { get; set; }
            [Display(Name = "同意人")]
            public string ApproveName { get; set; }
            [Display(Name = "订单实付金额")]
            public Nullable<decimal> OrderMoney { get; set; }
            [Display(Name = "支付宝名称")]
            public string AlipayName { get; set; }
            [Display(Name = "支付宝账户")]
            public string AlipayAccount { get; set; }
            [Display(Name = "备注")]
            public string Note { get; set; }
            public Nullable<System.DateTime> PostTime { get; set; }
            public Nullable<int> For_Delete { get; set; }
            public Nullable<int> Status { get; set; }
            public Nullable<int> Step { get; set; }
            [Display(Name = "系统备注")]
            public string Repeat { get; set; }
            [Display(Name = "支付帐号")]
           
            public string BackFrom { get; set; }
        }
    }
}