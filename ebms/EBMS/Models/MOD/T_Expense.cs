using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Expense
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "部门")]
            [Required(ErrorMessage = "部门不能为空")]
            public string Department { get; set; }
            [Display(Name = "报销原因")]
            [Required(ErrorMessage = "报销原因不能为空")]
            public string Reun_Reason { get; set; }
            [Display(Name = "报销金额")]
            public decimal Reun_Cost { get; set; }
            [Display(Name = "下级审核人")]
            public int ExpenseNextApprove { get; set; }
            [Display(Name = "操作人")]
            public string PostUser { get; set; }
            [Display(Name = "创建时间")]
            public System.DateTime CrateDate { get; set; }
            [Display(Name = "报销单号")]
            public string Reun_Code { get; set; }
            [Display(Name = "账号类型")]
            public string AccountType { get; set; }
            [Display(Name = "后台申请")]
            public string Shop { get; set; }
            [Display(Name = "开户行")]
            [Required(ErrorMessage = "开户行不能为空")]
            public string Reun_Bank { get; set; }
            [Display(Name = "开户人名称")]
            [Required(ErrorMessage = "开户人不能为空")]
            public string Reun_Name { get; set; }
            [Display(Name = "卡号")]
            [Required(ErrorMessage = "卡号不能为空")]
            public string Car_Number { get; set; }
            [Display(Name = "冲抵借支批号")]
            public string MatchBorrowNumber { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            [Display(Name = "账号")]
            public string SpendingNumber { get; set; }
            [Display(Name = "公司")]
            public string SpendingCompany { get; set; }
            [Display(Name = "存在发票")]
            public int IsBlending { get; set; }
            [Display(Name = "发票备注")]
            public string Memo { get; set; }
            [Display(Name = "标签")]
            public string ExpStatus { get; set; }
        }
    }
}