using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_DepartmentActivity
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
            [Display(Name = "申请时间")]
            public System.DateTime PostTime { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "活动原因")]
            public string Reason { get; set; }
            [Display(Name = "活动负责人")]
            public string ChargePerson { get; set; }
            [Display(Name = "负责人电话")]
            public string Tel { get; set; }
            [Display(Name = "参加人员")]
            public string Persons { get; set; }
            [Display(Name = "活动人数")]
            public int PeopleNum { get; set; }
            [Display(Name = "活动地点")]
            public string Address { get; set; }
            [Display(Name = "活动时间")]
            public System.DateTime StartTime { get; set; }
            [Display(Name = "活动时间")]
            public System.DateTime EndTime { get; set; }
            [Display(Name = "活动内容")]
            public string Contents { get; set; }
            [Display(Name = "预计费用")]
            public double Expense { get; set; }
           
            public int Step { get; set; }
            
            public int Status { get; set; }
            [Display(Name = "审批人")]
            public string CurrentApprove { get; set; }
            [Display(Name = "申请部门")]
            public string Dept { get; set; }
        }
    }
}