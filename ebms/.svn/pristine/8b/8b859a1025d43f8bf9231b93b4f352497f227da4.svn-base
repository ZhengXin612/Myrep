using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PZ_JZ
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
              [Display(Name = "编码")]
            public string PZ_OrderNum { get; set; }
             [Display(Name = "摘要")]
            public string PZ_Summary { get; set; }
               [Display(Name = "科目")]
            public string PZ_Subject { get; set; }
              [Display(Name = "部门")]
            public string PZ_Department { get; set; }
             [Display(Name = "时间")]
            public Nullable<System.DateTime> PZ_Time { get; set; }
             [Display(Name = "金额")]
            public Nullable<double> PZ_Money { get; set; }
              [Display(Name = "借贷方向")]
            public Nullable<int> PZ_Direction { get; set; }
        }
    }
}