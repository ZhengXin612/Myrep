using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Generalize
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "创建时间")]
            public Nullable<DateTime> CreateDate { get; set; }
            [Display(Name = "平台")]
            public string PlatformCode { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "店铺名称")]
            public string StoreName { get; set; }
            [Display(Name = "订单号")]
            public string OrderNumber { get; set; }
            [Display(Name = "宝贝名称")]
            public string ProductName { get; set; }
            [Display(Name = "金额")]
            [Required(ErrorMessage = "金额不能为空")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public Nullable<decimal> Cost { get; set; }
            [Display(Name = "佣金")]
            [Required(ErrorMessage = "佣金不能为空")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "佣金格式输入错误")]
            public Nullable<decimal> CommissionCost { get; set; }
            [Display(Name = "刷手信息")]
            public string DKUserMessage { get; set; }
            [Display(Name = "旺旺号")]
            public string AliNumber { get; set; }
            [Display(Name = "银行卡")]
            public string BankNumber { get; set; }
            [Display(Name = "财付通")]
            public string TenPay { get; set; }
            public string IsCheck { get; set; }
            [Display(Name = "订单类型")]
            public string OrderType { get; set; }
            [Display(Name = "付佣账号")]
            public string PayCommissionNumber { get; set; }
            [Display(Name = "备注")]
            public string Memo { get; set; }
            [Display(Name = "借支批号")]
            public string BorrowCode { get; set; }
            [Display(Name = "负责人")]
            public string ResponsibleName { get; set; }
            public string IsSend { get; set; }
            public string IsCancel { get; set; }
            public string PlatformFlag { get; set; }
            [Display(Name = "仓库名称")]
            public string WarhouseName { get; set; }
            [Display(Name = "上传人")]
            public string UploadName { get; set; }
            public Nullable<int> Status { get; set; }
            public Nullable<int> IsDelete { get; set; }
        }
    }
}