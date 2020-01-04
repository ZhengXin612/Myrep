using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelEduBackgroud
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "就读学校")]
            public string School { get; set; }
             [Display(Name = "专业")]
            public string Specialty { get; set; }
             [Display(Name = "起止时间")]
            public string StartFinishTime { get; set; }
            public string PID { get; set; }
        }
    }
}