using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Borrow
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "借支姓名")]
            public string BorrowName { get; set; }
            [Display(Name = "资金来源")]
            public string BorrowerFrom { get; set; }
            [Display(Name = "部门")]
            [Required(ErrorMessage = "请选择部门")]
            public string BorrowerDep { get; set; }
            [Display(Name = "借支事由")]
            [Required(ErrorMessage = "请填写借支事由")]
            public string BorrowReason { get; set; }
            [Display(Name = "金额")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public Nullable<decimal> BorrowMoney { get; set; }
            [Display(Name = "申请日期")]
            public Nullable<System.DateTime> BorrowDate { get; set; }
            [Display(Name = "状态")]
            public int BorrowState { get; set; }
            [Display(Name = "借支批号")]
            public string BorrowCode { get; set; }
            [Required(ErrorMessage = "请填写银行卡号")]
            [Display(Name = "银行卡号")]
            public string BorrowAccountID { get; set; }
            [Required(ErrorMessage = "请填写开户行")]
            [Display(Name = "开户行")]
            public string BorrowBank { get; set; }
            [Display(Name = "需款时间")]
            public Nullable<System.DateTime> BorrowNeedDate { get; set; }
            [Display(Name = "收款人")]
            [Required(ErrorMessage = "请输入收款人")]
            public string BorrowAccountName { get; set; }
            [Display(Name = "下级审核人")]
            public int BorrowNextApprove { get; set; }
            [Display(Name = "账号")]
            public string SpendingNumber { get; set; }
            [Display(Name = "公司")]
            public string SpendingCompany { get; set; }
        }
    }
}