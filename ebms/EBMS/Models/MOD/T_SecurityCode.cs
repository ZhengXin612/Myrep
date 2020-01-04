using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_SecurityCode
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
               [Display(Name = "申请人")]
            public string ApplyName { get; set; }
               [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
               [Display(Name = "厂家地址")]
            public string ManufactorAddress { get; set; }
               [Display(Name = "商品名称")]
            public string ProductName { get; set; }
               [Display(Name = "标签数量")]
            public Nullable<int> LabelNumber { get; set; }
               [Display(Name = "产品数量")]
            public Nullable<int> ProductNumber { get; set; }
            [Display(Name = "状态")]
            public Nullable<int> Status { get; set; }
            [Display(Name = "步奏")]
            public Nullable<int> Step { get; set; }
            [Display(Name = "是否删除")]
            public Nullable<int> Isdelete { get; set; }
        }
    }
}