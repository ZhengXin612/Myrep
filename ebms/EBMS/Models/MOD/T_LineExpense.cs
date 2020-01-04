using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_LineExpense
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "部门")]
     
            public string Department { get; set; }
            [Display(Name = "报销原因")]
         
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
            public string Reun_Bank { get; set; }
            [Display(Name = "开户人名称")]
            public string Reun_Name { get; set; }
            [Display(Name = "卡号")]
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
        }
    }
}