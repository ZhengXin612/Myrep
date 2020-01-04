using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_LazadaOrderConfig
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "名称")]
            [Required(ErrorMessage = "名称不能为空")]
            public string Name { get; set; }
            [Display(Name = "电话号码")]
            [Required(ErrorMessage = "电话号码不能为空")]
            public string Phone { get; set; }
            [Display(Name = "地址信息")]
            [Required(ErrorMessage = "地址信息不能为空")]
            public string AddressMessage { get; set; }
            [Display(Name = "详细地址")]
            [Required(ErrorMessage = "详细地址不能为空")]
            public string Address { get; set; }
            [Display(Name = "快递")]
            [Required(ErrorMessage = "快递不能为空")]
            public string Express { get; set; }
            [Display(Name = "仓库")]
            [Required(ErrorMessage = "仓库不能为空")]
            public string Warhouse { get; set; }
            [Display(Name = "是否默认")]
            public int IsDefault { get; set; }
        }
    }
}