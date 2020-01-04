using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{ 
    [MetadataType(typeof(MOD))]
    public partial class T_MeetingRoom
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
             [Display(Name = "会议时间")]
            public System.DateTime StartTime { get; set; }
             
            public System.DateTime EndTime { get; set; }
             [Display(Name = "会议主题")]
            public string MeetingTheme { get; set; }
             [Display(Name = "主持人")]
            public string Host { get; set; }
             [Display(Name = "会议室")]
            public string MeetingRoom { get; set; }
             [Display(Name = "参会人数")]
            public int PeopleNum { get; set; }
             [Display(Name = "备注")]
            public string Memo { get; set; }
             [Display(Name = "审批人")]
             public string CurrentApprove { get; set; }
        }
    }
}