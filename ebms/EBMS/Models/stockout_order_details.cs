//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EBMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class stockout_order_details
    {
        public int ID { get; set; }
        public int stockout_id { get; set; }
        public Nullable<int> rec_id { get; set; }
        public string spec_no { get; set; }
        public Nullable<decimal> goods_count { get; set; }
        public Nullable<decimal> sell_price { get; set; }
        public string goods_name { get; set; }
        public string goods_no { get; set; }
        public string spec_name { get; set; }
        public Nullable<decimal> cost_price { get; set; }
        public Nullable<decimal> total_amount { get; set; }
        public string remark { get; set; }
        public Nullable<System.DateTime> TimeAdd { get; set; }
        public int PageNo { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public int IsImport { get; set; }
        public string StockOutCode { get; set; }
        public System.DateTime STime { get; set; }
        public System.DateTime Etime { get; set; }
        public Nullable<System.DateTime> consign_time { get; set; }
        public Nullable<int> IsInsertToNC { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        public string order_no { get; set; }
        public string src_order_no { get; set; }
        public string warehouse_no { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> order_type { get; set; }
        public string order_type_name { get; set; }
        public string subtype { get; set; }
        public string stockout_reason { get; set; }
        public Nullable<int> trade_type { get; set; }
        public Nullable<decimal> goods_total_amount { get; set; }
        public Nullable<decimal> goods_total_cost { get; set; }
        public Nullable<decimal> post_fee { get; set; }
        public string logistics_no { get; set; }
        public Nullable<decimal> package_fee { get; set; }
        public string Reason { get; set; }
    }
}