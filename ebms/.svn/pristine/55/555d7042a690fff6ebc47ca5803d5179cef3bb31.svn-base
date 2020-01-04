using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_PersonnelQuit
    {
         class MOD
         {
              [Display(Name = "主键")]
             public int ID { get; set; }
               [Display(Name = "父id")]
             public int Pid { get; set; }
              [Display(Name = "离职日期")]
             public Nullable<System.DateTime> QuitDate { get; set; }
              [Display(Name = "离职资料")]
             public string QuitData { get; set; }
         }
    }
}