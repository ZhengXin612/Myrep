using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_BusinessTravel
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }

            public string PostName { get; set; }
            public System.DateTime PostTime { get; set; }
            [Display(Name = "出差地点")]
            public string Address { get; set; }
            [Display(Name = "出差事由")]
            public string Reason { get; set; }
            [Display(Name = "预计往返时间")]
            public System.DateTime StartTime { get; set; }
            public System.DateTime EndTime { get; set; }
            [Display(Name = "交通工具")]
            public string Transport { get; set; }
            [Display(Name = "预计借支差旅费")]
            public double Money { get; set; }
            [Display(Name = "审批人")]
            public string CurrentApprove { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public int isDelete { get; set; }
        }
    }
}