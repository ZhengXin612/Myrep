using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{ 
    [MetadataType(typeof(MOD))]
    public partial class T_EmployDemand
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Display(Name = "需求人")]
            public string DemandUser { get; set; }
             [Display(Name = "岗位")]
            public string Job { get; set; }
             [Display(Name = "人数")]
            public int PeopleNum { get; set; }
             [Display(Name = "备注")]
            public string Memo { get; set; }
             [Display(Name = "审核状态")]
            public int State { get; set; }
             [Display(Name = "人事备注")]
            public string PerMemo { get; set; }
             [Display(Name = "建议薪资")]
            public string RecommendSalary { get; set; }
             [Display(Name = "已分配人数")]
            public int DistributionNum { get; set; }
             [Display(Name = "当前审核人")]
            public string CurrentApproveName { get; set; }
             [Display(Name = "当前步骤")]
            public int Step { get; set; }
             [Display(Name = "需求部门")]
             public string DemandDepartment { get; set; }
        }
    }
}