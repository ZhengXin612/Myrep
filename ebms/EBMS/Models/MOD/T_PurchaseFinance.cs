using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PurchaseFinance
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
                [Display(Name = "需付金额")]
            public Nullable<decimal> needpay { get; set; }
              [Display(Name = "采购单号")]
                public Nullable<int> PurchaseOddNum { get; set; }
              [Display(Name = "支付金额")]
            public Nullable<decimal> payment { get; set; }
              [Display(Name = "支付人员")]
            public string paymentName { get; set; }
               [Display(Name = "是否支付")]
            public Nullable<int> ispayment { get; set; }
               [Display(Name = "需付款帐号")]
            public string paymentaccounts { get; set; }
             [Display(Name = "需支付方式")]
            public string paymenmode { get; set; }
              [Display(Name = "主管是否审核")]
            public Nullable<int> isFinanceToexamine { get; set; }
               [Display(Name = "财务主管")]
            public string FinanceToexamineName { get; set; }
              [Display(Name = "财务主管审核时间")]
            public Nullable<System.DateTime> FinanceToexaminedate { get; set; }
             [Display(Name = "支付时间")]
            public Nullable<System.DateTime> paymendate { get; set; }
               [Display(Name = "申请备注")]
            public string ApplyRemarks { get; set; }
              [Display(Name = "财务主管备注")]
            public string FinanceToexaminRemarks { get; set; }
               [Display(Name = "支付备注")]
            public string paymenRemarks { get; set; }
              [Display(Name = "申请人")]
            public string ApplyName { get; set; }
             [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
        }
    }
}











