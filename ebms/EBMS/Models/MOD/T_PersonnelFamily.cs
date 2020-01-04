using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelFamily
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Display(Name = "父id")]
            public int Pid { get; set; }
            [Display(Name = "姓名")]
            public string Name { get; set; }
             [Display(Name = "关系")]
            public string Relation { get; set; }
            [Display(Name = "工作单位")]
            public string WorkUnit { get; set; }
             [Display(Name = "岗位")]
            public string Job { get; set; }
        }
    }
}