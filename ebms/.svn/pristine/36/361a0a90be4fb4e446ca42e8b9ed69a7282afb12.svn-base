using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_InterceptLogistics
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "订单号")]
            public string OrderNumber { get; set; }
            [Display(Name = "快递公司")]
            public string ExpressName { get; set; }
            [Display(Name = "快递单号")]
            public string ExpressNumber { get; set; }
            [Display(Name = "订单金额")]
            public decimal OrderMoney { get; set; }
            [Display(Name = "财务审核状态")]
            public int FinanceApproveStatus { get; set; }
            [Display(Name = "财务审核人")]
            public Nullable<int> FinanceApproveUser { get; set; }
            [Display(Name = "财务审核时间")]
            public Nullable<System.DateTime> FinanceApproveTime { get; set; }
            [Display(Name = "快递组审核状态")]
            public int ExpressApproveStatus { get; set; }
            [Display(Name = "快递组审核人")]
            public Nullable<int> ExpressApproveUser { get; set; }
            [Display(Name = "快递组审核时间")]
            public Nullable<System.DateTime> ExpressApproveTime { get; set; }
            [Display(Name = "仓库审核状态")]
            public int WarehouseApproveStatus { get; set; }
            [Display(Name = "仓库审核人")]
            public Nullable<int> WarehouseApproveUser { get; set; }
            [Display(Name = "仓库审核时间")]
            public Nullable<System.DateTime> WarehouseApproveTime { get; set; }
            [Display(Name = "财务审核备注")]
            public string FinanceReason { get; set; }
            [Display(Name = "快递组审核备注")]
            public string ExpressReason { get; set; }
            [Display(Name = "仓库复核备注")]
            public string WarehouseReason { get; set; }
            [Display(Name ="备注")]
            public string Remark { get; set; }
            [Display(Name = "创建人")]
            public int Creator { get; set; }
            [Display(Name = "创建时间")]
            public System.DateTime CreateTime { get; set; }
            public int Del { get; set; }
        }
    }
}