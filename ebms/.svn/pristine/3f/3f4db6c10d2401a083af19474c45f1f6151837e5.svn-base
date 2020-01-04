using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_FundAllot
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "操作人")]
            public string PostUser { get; set; }
            [Display(Name = "部门")]
            [Required(ErrorMessage = "部门不能为空")]
            public string Department { get; set; }
            [Display(Name = "调入单位")]
            [Required(ErrorMessage = "调入单位不能为空")]
            public string CompanyIn { get; set; }
            [Display(Name = "收款银行")]
            [Required(ErrorMessage = "收款银行不能为空")]
            public string TheReceivingBank { get; set; }
            [Display(Name = "收款账号")]
            [Required(ErrorMessage = "收款账号不能为空")]
            public string AccountNumber { get; set; }
            [Display(Name = "调出单位")]
            public string CompanyOut { get; set; }
            [Display(Name = "付款银行")]
            public string ThePaymentBank { get; set; }
            [Display(Name = "付款账号")]
            public string PaymentNumber { get; set; }
            [Display(Name = "审核状态")]
            public int Status { get; set; }
            [Display(Name = "资金用途")]
            [Required(ErrorMessage = "资金用途不能为空")]
            public string UseOfProceeds { get; set; }
            public System.DateTime PostTime { get; set; }
            [Display(Name = "审核级别")]
            public int Step { get; set; }
            [Display(Name = "调拨金额")]
            [Required(ErrorMessage = "调拨金额不能为空")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public double theMoney { get; set; }
            [Display(Name = "调拨单号")]
            public string FundAllotCode { get; set; }
        }
    }
}