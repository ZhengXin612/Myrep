using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ToRegularWorker
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "申请人")]
            public string PostName { get; set; }
             [Display(Name = "申请时间")]
            public System.DateTime PostTime { get; set; }
             [Display(Name = "试用岗位")]
            public string ProbationJob { get; set; }
             [Display(Name = "转正岗位")]
            public string Job { get; set; }
             [Display(Name = "试用期")]
            public System.DateTime StartProbationTime { get; set; }
           [Display(Name = "试用期")]
            public System.DateTime EndProbationTime { get; set; }
             [Display(Name = "申请内容")]
            public string ApplyContent { get; set; }
             [Display(Name = "审核人")]
            public string CurrentApprove { get; set; }
            
            public int Status { get; set; }
            
            public int Step { get; set; }
            
            public int isDelete { get; set; }
             [Display(Name = "转正薪资")]
            public double Salary { get; set; }
        }
    }
}