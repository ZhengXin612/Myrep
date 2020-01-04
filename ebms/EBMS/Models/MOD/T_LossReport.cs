using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_LossReport
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "报损部门")]
            public string Department { get; set; }
            [Display(Name = "报损店铺")]
            public string Shop { get; set; }
            [Display(Name = "报损编码")]
            public string Code { get; set; }
            [Display(Name = "状态")]
            public Nullable<int> Status { get; set; }
            [Display(Name = "报损金额")]
            public Nullable<decimal> Total { get; set; }
             [Display(Name = "审核步骤")]
            public Nullable<int> Step { get; set; }
             [Display(Name = "一级审核人")]
            public string ApproveFirst { get; set; }
        }
    }
}