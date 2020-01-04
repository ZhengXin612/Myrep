using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_EmployDemandApprove
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
            [Display(Name = "审核人")]
            public string ApproveUser { get; set; }
            [Display(Name = "审核时间")]
            public Nullable<System.DateTime> ApproveDate { get; set; }
            [Display(Name = "需求id")]
            public int DemandID { get; set; }
            [Display(Name = "审核结果")]
            public int ApproveState { get; set; }
            [Display(Name = "审核备注")]
            public string ApproveMemo { get; set; }
            [Display(Name = "审核步骤")]
            public int Step { get; set; }
        }
    }
}