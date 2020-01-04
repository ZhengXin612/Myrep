using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_MajorInvoice
    {
        class MOD
        {
        
    
            public int ID { get; set; }
            [Display(Name = "店铺名称")]
            public string ShopName { get; set; }
               [Display(Name = "订单号")]
            public string OrderNumber { get; set; }
               [Display(Name = "开票金额")]
            public Nullable<decimal> TheInvoiceAmount { get; set; }
                  [Display(Name = "开票内容")]
            public string InvoiceContent { get; set; }
                 [Display(Name = "单价")]
            public Nullable<double> Number { get; set; }
              [Display(Name = "数量")]
            public Nullable<decimal> UnitPrice { get; set; }
                [Display(Name = "发票邮寄地址")]
            public string Address { get; set; }
                    [Display(Name = "公司名称")]
            public string CorporateName { get; set; }
                  [Display(Name = "税号")]
            public string TaxNumber { get; set; }
                [Display(Name = "地址")]
            public string InvoiceAddress { get; set; }
              [Display(Name = "电话")]
            public string Telephone { get; set; }
               [Display(Name = "银行帐号")]
            public string BankAccount { get; set; }
              [Display(Name = "开户行")]
            public string BankAddress { get; set; }
            [Display(Name = "状态")]
            public int Status { get; set; }
             [Display(Name = "步揍")]
            public int Step { get; set; }
               [Display(Name = "是否删除")]
            public string Isdelete { get; set; }
                   [Display(Name = "申请人")]
            public string PostUser { get; set; }
                [Display(Name = "申请时间")]
            public Nullable<System.DateTime> PostDate { get; set; }
                 [Display(Name = "新管易编码")]
            public string OrderSCode { get; set; }
			[Display(Name = "邮寄省-市-区")]
			public string GoodsAddress { get; set; }
		}
    }
}