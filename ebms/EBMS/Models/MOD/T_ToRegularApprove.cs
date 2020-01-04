using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ToRegularApprove
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
          
            public int PID { get; set; }
          
            public string ApproveName { get; set; }
            
            public Nullable<System.DateTime> ApproveTime { get; set; }
           [Display(Name = "状态")]
            public int Status { get; set; }
          
            public int Step { get; set; }
            [Display(Name = "备注")]
            public string Memo { get; set; }
        }
    }
}