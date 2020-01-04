using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_CheckAttendace
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "类型")]
            public string Type { get; set; }
             [Display(Name = "申请人")]
            public string PostName { get; set; }
             [Display(Name = "申请时间")]
            public System.DateTime PostTime { get; set; }
             [Display(Name = "原因")]
            public string Reason { get; set; }
             [Display(Name = "请假类型")]
            public string LeaveType { get; set; }
             [Display(Name = "开始时间")]
            public System.DateTime StartTime { get; set; }
             [Display(Name = "结束时间")]
            public Nullable<System.DateTime> EndTime { get; set; }
             [Display(Name = "天数")]
            public Nullable<double> Days { get; set; }
             [Display(Name = "工作开始时间")]
            public Nullable<System.DateTime> WorkStartTime { get; set; }
             [Display(Name = "工作结束时间")]
            public Nullable<System.DateTime> WorkEndTime { get; set; }
             [Display(Name = "状态")]
            public int Status { get; set; }
             [Display(Name = "步骤")]
            public int Step { get; set; }

                 [Display(Name = "审核人")]
            public string CurrentApprove { get; set; }
        }
    }
}