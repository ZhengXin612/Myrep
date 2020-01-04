using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Intercept
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "订单号")]
            public string OrderNumber { get; set; }
            public string NewOrderNumber { get; set; }
            [Display(Name = "收货人")]
            [Required(ErrorMessage = "请输入收货人")]
            public string Receiver { get; set; }
            [Display(Name = "需发快递")]
            [Required(ErrorMessage = "请选择快递名称")]
            public string ExpressName { get; set; }
            [Display(Name = "运单号")]
            public string MailNo { get; set; }
            [Display(Name = "地址信息")]
            [Required(ErrorMessage = "请输入地址信息")]
            public string AddressMessage { get; set; }
            [Display(Name = "需发地址")]
            [Required(ErrorMessage = "请输入详细地址")]
            public string Address { get; set; }
            [Display(Name = "移动电话号码")]
            public string TelPhone { get; set; }
            public int Status { get; set; }
            [Display(Name = "备注")]
            [Required(ErrorMessage = "备注不能为空")]
            public string Memo { get; set; }
            [Display(Name = "固定电话")]
            public string Phone { get; set; }
            [Display(Name = "邮政编码")]
            public string Postalcode { get; set; }
            [Display(Name = "需发仓库")]
            [Required(ErrorMessage = "仓库不能为空")]
            public string Warhouse { get; set; }
            [Display(Name = "原因")]
            [Required(ErrorMessage = "原因不能为空")]
            public string Reson { get; set; }
            [Display(Name = "操作人")]
            public string PostUSer { get; set; }
            [Display(Name = "原地址")]
            public string LoadAddress { get; set; }
            [Display(Name = "原快递")]
            public string LoadExpressName { get; set; }
            [Display(Name = "原仓库")]
            public string LoadWarhouse { get; set; }
           
        }
    }
}