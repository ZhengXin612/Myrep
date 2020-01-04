using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ManualExpress
    {
        class MOD
        {
         
          
            public int ID { get; set; }
              [Display(Name = "申请人")]
            public string ApplyName { get; set; }
                [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
               [Display(Name = "申请公司")]
            public string ApplyCompany { get; set; }
             [Display(Name = "申请店铺")]
            public string ApplyShop { get; set; }
               [Display(Name = "手工录费类型")]
            public string EntryType { get; set; }
              [Display(Name = "原因")]
            public string Reason { get; set; }
              [Display(Name = "总票数")]
            public Nullable<int> votes { get; set; }
             [Display(Name = "对方公司名称")]
            public string OtherCompanyName { get; set; }
              [Display(Name = "对方公司库房")]
            public string OtherCompanyLibrary { get; set; }
              [Display(Name = "库房地址")]
            public string LibraryAddress { get; set; }
              [Display(Name = "库房详细地址")]
              public string DetailedAddress { get; set; }
              [Display(Name = "库房电话")]
            public string LibraryPhone { get; set; }
             [Display(Name = "状态")]
            public Nullable<int> Status { get; set; }
              [Display(Name = "步奏")]
            public Nullable<int> Step { get; set; }
              [Display(Name = "是否删除")]
            public Nullable<int> Isdelete { get; set; }
           
        }
    }
}