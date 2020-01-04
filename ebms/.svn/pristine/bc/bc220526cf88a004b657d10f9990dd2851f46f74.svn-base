using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_ExchangeCenter
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "店铺名称")]
            [Required(ErrorMessage = "店铺名称不能为空")]
            public string StoreName { get; set; }
            [Display(Name = "会员名称")]
            [Required(ErrorMessage = "会员名称不能为空")]
            public string VipName { get; set; }
            [Display(Name = "会员账号")]
            [Required(ErrorMessage = "会员账号不能为空")]
            public string VipCode { get; set; }
            [Display(Name = "订单号")]
            [Required(ErrorMessage = "订单号不能为空")]
            public string OrderCode { get; set; }
            [Display(Name = "拍单时间")]
            public System.DateTime SingleTime { get; set; }
            [Display(Name = "补发货订单类型")]
            public string NeedOrderType { get; set; }
            [Display(Name = "退回快递名称")]
            [Required(ErrorMessage = "退回快递名称不能为空")]
            public string ReturnExpressName { get; set; }
            [Display(Name = "退回快递单号")]
            [Required(ErrorMessage = "退回快递单号不能为空")]
            public int ReturnExpressCode { get; set; }
            [Display(Name = "退回仓库")]
            [Required(ErrorMessage = "退回仓库不能为空")]
            public string ReturnWarhouse { get; set; }
            [Display(Name = "换货原因")]
            [Required(ErrorMessage = "换货原因不能为空")]
            public string ExchangeReson { get; set; }
            [Display(Name = "收货人")]
            [Required(ErrorMessage = "收货人不能为空")]
            public string ReceivingName { get; set; }
            [Display(Name = "收货人国定电话")]
            public int ReceivingPhone { get; set; }
            [Display(Name = "收货人移动电话")]
            public int ReceivingTelPhone { get; set; }
            [Display(Name = "收货人地址")]
            [Required(ErrorMessage = "收货人地址不能为空")]
            public string ReceivingAddress { get; set; }
            [Display(Name = "省市区")]
            [Required(ErrorMessage = "省市区不能为空")]
            public string AddressMessage { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "申请时间")]
            public System.DateTime CreateDate { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            [Display(Name = "补发货仓库")]
            [Required(ErrorMessage = "补发货仓库不能为空")]
            public string NeedWarhouse { get; set; }
            [Display(Name = "补发货快递")]
            [Required(ErrorMessage = "补发货快递不能为空")]
            public string NeedExpress { get; set; }
            [Display(Name = "补发货邮政编码")]
            [Required(ErrorMessage = "补发货邮政编码不能为空")]
            public string NeedPostalCode { get; set; }
            [Display(Name = "卖家备注")]
            public string SalesRemark { get; set; }
            [Display(Name = "买家备注")]
            public string BuyRemark { get; set; }
        }
    }
}