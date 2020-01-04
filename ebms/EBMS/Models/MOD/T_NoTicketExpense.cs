using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_NoTicketExpense
    {
        class MOD
        { 
             public int ID { get; set; }
             [Display(Name = "流水号")]
             public string Code { get; set; }
             [Display(Name = "报销人")]
             public string PostUser { get; set; }
             [Display(Name = "报销日期")]
             public Nullable<System.DateTime> PostTime { get; set; }
             [Display(Name = "报销内容")]
             public string TheContent { get; set; }
             [Display(Name = "金额")]
             public Nullable<decimal> Total { get; set; }
             [Display(Name = "备注")]
             public string Note { get; set; }
             [Display(Name = "是否审核")]
             public Nullable<int> Status { get; set; }
             [Display(Name = "报销账号")]
             public Nullable<int> PostAccountInfo { get; set; }
             [Display(Name = "支出账号")]
             public Nullable<int> PayAccount { get; set; }
             [Display(Name = "审核时间")]
             public Nullable<System.DateTime> ApproveTime { get; set; }
        }
    }
}