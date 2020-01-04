using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_MailSample
    {
        class MOD
        {
         
         
            public int ID { get; set; }
               [Display(Name = "申请人")]
            public string ApplyName { get; set; }
              [Display(Name = "申请时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
                   [Display(Name = "店铺名称")]
            public string ShopName { get; set; }
                  [Display(Name = "发货仓库")]
            public string DeliverGoodsWarehouse { get; set; }
                  [Display(Name = "订单号")]
            public string OrderNumber { get; set; }
                [Display(Name = "收货人姓名")]
            public string GoodsReceiptName { get; set; }
                [Display(Name = "提供原因")]
            public string DeliverGoodsReason { get; set; }
              [Display(Name = "收货人电话")]
            public string GoodsReceiptPhone { get; set; }
               [Display(Name = "收货人地址")]
            public string GoodsReceiptAddress { get; set; }
             [Display(Name = "备注")]
            public string ApplyRemarks { get; set; }
               [Display(Name = "要求到货时间")]
            public Nullable<System.DateTime> GoodsReceiptDate { get; set; }
              [Display(Name = "是否寄回")]
            public Nullable<int> IsReturn { get; set; }
             [Display(Name = "快递公司")]
            public string ExpressCode { get; set; }
             [Display(Name = "状态")]
            public Nullable<int> Status { get; set; }
             [Display(Name = "步奏")]
            public Nullable<int> Step { get; set; }
            [Display(Name = "是否删除")]
            public Nullable<int> Isdelete { get; set; }
        }
    }
}