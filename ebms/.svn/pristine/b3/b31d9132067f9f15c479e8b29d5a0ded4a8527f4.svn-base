using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_ExpressIndemnity
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
             [Display(Name = "申请人")]
            public string PostUserName { get; set; }
             [Display(Name = "申请时间")]
            public Nullable<System.DateTime> Date { get; set; }
             [Display(Name = "订单号")]
            [MaxLength(50)]
            public string OrderNum { get; set; }
             [Display(Name = "旺旺")]
             [MaxLength(100)]
            public string wangwang { get; set; }
             [Display(Name = "退货原因")]
             [MaxLength(50)]
            public string RetreatReason { get; set; }
             [Display(Name = "店铺名称")]
             [MaxLength(100)]
            public string ShopName { get; set; }
             [Display(Name = "物流单号")]
             [MaxLength(50)]
            public string RetreatExpressNum { get; set; }
             [Display(Name = "备注")]
             [MaxLength(200)]
            public string Memo { get; set; }
             [Display(Name = "状态")]
             [MaxLength(50)]
            public string State { get; set; }
             [Display(Name = "订单金额")]
            public Nullable<double> OrderMoney { get; set; }
             [Display(Name = "类型")]
            public string Type { get; set; }
             [Display(Name = "二次审核")]
            public string Second { get; set; }
             [Display(Name = "图片")]
            public string Pic { get; set; }
              [Display(Name = "快递名称")]
             public string ExpressName { get; set; }
              [Display(Name = "赔付金额")]
              public Nullable<double> IndemnityMoney { get; set; }

              [Display(Name = "钱款去向")]
              public string MoneyWhereAbout { get; set; }
              [Display(Name = "地址")]
              public string Address { get; set; }
             
        }
    }
}