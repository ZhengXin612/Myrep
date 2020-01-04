using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelInterviewRecord
    {
        class MOD
        {
        [Display(Name="主键")]
        public int ID { get; set; }
             [Display(Name = "面试官")]
        public string Interviewer { get; set; }
             [Display(Name="时间")]
        public Nullable<System.DateTime> Date { get; set; }
             [Display(Name = "面试结果")]
        public Nullable<int> State { get; set; }
             [Display(Name = "备注")]
        public string Memo { get; set; }
            
        public Nullable<int> PID { get; set; }
             [Display(Name = "步骤")]
        public Nullable<int> Step { get; set; }
            
        }
    }
}