using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
      [MetadataType(typeof(MOD))]
    public partial class T_Assets
    {
          class MOD
          {
              [Display(Name = "资产代码")]
              [Required(ErrorMessage = "不能为空")]
              [MaxLength(50)]
              public string Code { get; set; }
                [Display(Name = "图片地址")]
                [MaxLength(200,ErrorMessage="图片名称太长请修改")]
              public string Pic { get; set; }
              [Display(Name = "资产名称")]
              [Required(ErrorMessage = "不能为空")]
              [MaxLength(200)]
              public string Name { get; set; }
              [Display(Name = "资产规格")]
              [MaxLength(300)]
              public string Spec { get; set; }
               [Display(Name = "购买价格")]
              public Nullable<decimal> Cost { get; set; }
              [Display(Name = "购买人")]
              [MaxLength(100)]
              public string Buyer { get; set; }
               [Display(Name = "供应商")]
               [MaxLength(200)]
              public string BuyFrom { get; set; }
               [Display(Name = "保修期")]
               [MaxLength(100)]
              public string Guarantee { get; set; }
              [Display(Name = "购买日期")]
              public Nullable<System.DateTime> BuyDate { get; set; }
               [Display(Name = "归属部门")]
               [MaxLength(200)]
              public string Department { get; set; }
                [Display(Name = "使用人")]
                [MaxLength(300)]
              public string Owner { get; set; }
               [Display(Name = "使用地点")]
               [MaxLength(200)]
              public string Place { get; set; }
               [Display(Name = "责任人")]
               [MaxLength(300)]
              public string Responsible { get; set; }
               [Display(Name = "备注")]
               [MaxLength(300)]
              public string Memo { get; set; }
               [Display(Name = "审批编码")]
               [MaxLength(50)]
              public string Barcode { get; set; }
              [Display(Name = "是否删除")]
              public string isDelete { get; set; }
               [Display(Name = "是否报废")]
              public string isScrap { get; set; }
               [Display(Name = "报废时间")]
              public Nullable<System.DateTime> ScrapDate { get; set; }
                [Display(Name = "类型")]
                [Required(ErrorMessage = "不能为空")]
              public string TypeCode { get; set; }
             
             
          }
    }
}