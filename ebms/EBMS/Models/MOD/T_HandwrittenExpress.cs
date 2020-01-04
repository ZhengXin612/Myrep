using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_HandwrittenExpress
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "申请人")]
            public string ApplyName { get; set; }
            [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
            [Display(Name = "申请用途")]
            public string ApplyPurpose { get; set; }
            [Display(Name = "申请数量")]
            public Nullable<int> ApplyNumber { get; set; }
            [Display(Name = "地址")]
            public string Address { get; set; }
             [Display(Name = "电话")]
            public string Telephone { get; set; }
            [Display(Name = "状态")]
            public int Status { get; set; }
             [Display(Name = "步奏")]
            public int Step { get; set; }
              [Display(Name = "是否删除")]
            public Nullable<int> Isdelete { get; set; }
             [Display(Name = "收件人姓名")]
            public string AddressName { get; set; }
            [Display(Name = "邮编号码")]
            public string Zip { get; set; }
            [Display(Name = "仓库")]
            public string Warehouse { get; set; }
        }
    }
}