using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_PersonnelTransfer
    {
         class MOD
         {
             [Display(Name = "主键")]
             public int ID { get; set; }
             [Display(Name = "父id")]
             public int Pid { get; set; }
              [Display(Name = "日期")]
             public Nullable<System.DateTime> TransferDate { get; set; }
              [Display(Name = "原因")]
              public string TransferReason { get; set; }
              [Display(Name = "类型")]
             public string Type { get; set; }
              [Display(Name = "原部门")]
              public string Department { get; set; }
              [Display(Name = "原职位")]
              public string Job { get; set; }
              [Display(Name = "申请调整部门")]
              public string TransDepartment { get; set; }
              [Display(Name = "申请调整职位")]
              public string TransJob { get; set; }
             [Display(Name = "申请人")]
              public string PostUser { get; set; }
             [Display(Name = "审核人")]
             public string CurrentApprove { get; set; }
              
         }
    }
}