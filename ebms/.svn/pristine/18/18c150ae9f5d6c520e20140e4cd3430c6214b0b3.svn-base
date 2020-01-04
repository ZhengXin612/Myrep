using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelTransferApprove
    {
        class MOD
        {
            public int ID { get; set; }
            public int PTID { get; set; }
            public string ApproveName { get; set; }
            public Nullable<System.DateTime> ApproveDate { get; set; }
           
            public int Step { get; set; }
            [Display(Name="状态")]
            public int Status { get; set; }
             [Display(Name = "备注")]
            public string Memo { get; set; }
        }
    }
}