using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelPayRaise
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "申请人")]
            public string PostUser { get; set; }
             [Display(Name = "申请时间")]
            public System.DateTime PostTime { get; set; }
             [Display(Name = "状态")]
            public int Status { get; set; }
             [Display(Name = "步骤")]
            public int Step { get; set; }
             [Display(Name = "当前薪资")]
            public Nullable<double> PresentSalary { get; set; }
             [Display(Name = "调整薪资")]
            public Nullable<double> PayRaise { get; set; }
             [Display(Name = "原因")]
            public string Reason { get; set; }
            public int PFID { get; set; }
            [Display(Name = "审核人")]
            public string CurrentApproveName { get; set; }
        }
    }
}