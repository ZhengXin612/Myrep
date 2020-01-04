using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Deliver
    {
        class MOD
        {
            [Display(Name="主键")]
            public int ID { get; set; }
            [Display(Name = "复核人")]
            public string PostName { get; set; }
            [Display(Name = "仓库名称")]
            public string WarehouseCode { get; set; }
             [Display(Name = "操作时间")]
            public Nullable<System.DateTime> PostTime { get; set; }
             [Display(Name = "备注")]
            public string Note { get; set; }
            public string IP { get; set; }
             [Display(Name = "订单号")]
            public string OrderNum { get; set; }
            [Display(Name = "单据编号")]
             public string MailNo { get; set; }
        }
    }
}