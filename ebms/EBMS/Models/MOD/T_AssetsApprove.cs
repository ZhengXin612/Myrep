using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_AssetsApprove
    {
        class MOD
        {

            [Display(Name = "主键")]
            public int ID { get; set; }
            [Display(Name = "资产编码")]
            public string Code { get; set; }
           
            public int ApplyID { get; set; }
            [Display(Name = "审核人")]
            public string ApproveName { get; set; }
            [Display(Name = "审核时间")]
            public Nullable<System.DateTime> ApproveDate { get; set; }
            [Display(Name = "审核状态")]
            public int State { get; set; }
            [Display(Name = "审核备注")]
            public string Memo { get; set; }
            [Display(Name = "步骤")]
            public string Step { get; set; }
        }
    }
}