using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_PersonnelReward
    {
         class MOD
         {
             [Display(Name = "主键")]
             public int ID { get; set; }
             [Display(Name = "父id")]
             public int Pid { get; set; }
              [Display(Name = "日期")]
             public Nullable<System.DateTime> RewardPunishDate { get; set; }
              [Display(Name = "奖惩明细")]
             public string RewardPunishiDetails { get; set; }
         }
    }
}