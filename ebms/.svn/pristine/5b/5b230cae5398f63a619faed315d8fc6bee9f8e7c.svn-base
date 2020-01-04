using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ExpressIndemnityApprove
    {
        class MOD
        {
            
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Display(Name = "处理人")]
            public string ApproveName { get; set; }
             [Display(Name = "处理时间")]
            public Nullable<System.DateTime> ApproveData { get; set; }
             [Display(Name = "处理结果")]
            public string State { get; set; }
             [Display(Name = "步骤")]
            public int Step { get; set; }
             [Display(Name = "处理备注")]
            public string Memo { get; set; }
            
            public int EID { get; set; }
             [Display(Name = "赔付金额")]
            public Nullable<double> Money { get; set; }
           
             [Display(Name = "钱款去向")]
            public string MoneyWhereAbout { get; set; }
             [Display(Name = "地址")]
             public string Address { get; set; }
             
        }
    }
}