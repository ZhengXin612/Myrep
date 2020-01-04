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
    
    public partial class sql_sostockout_detail
    {
        public int ID { get; set; }
        public int wms_status { get; set; }
        public Nullable<int> warehouse_type { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> consign_status { get; set; }
        public Nullable<decimal> goods_count { get; set; }
        public Nullable<decimal> goods_total_amount { get; set; }
        public Nullable<decimal> goods_total_cost { get; set; }
        public string logistics_no { get; set; }
        public Nullable<System.DateTime> consign_time { get; set; }
        public Nullable<int> block_reason { get; set; }
        public Nullable<int> src_order_id { get; set; }
        public string src_order_no { get; set; }
        public Nullable<int> stockout_id { get; set; }
        public Nullable<int> src_order_detail_id { get; set; }
        public Nullable<decimal> num { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> total_amount { get; set; }
        public Nullable<decimal> cost_price { get; set; }
        public string goods_no { get; set; }
        public string goods_name { get; set; }
        public string spec_name { get; set; }
        public Nullable<int> rec_id { get; set; }
        public string gift_type { get; set; }
        public Nullable<decimal> paid { get; set; }
        public string src_tid { get; set; }
        public Nullable<int> from_mask { get; set; }
        public Nullable<decimal> discount { get; set; }
        public Nullable<decimal> share_amount { get; set; }
        public Nullable<decimal> share_post { get; set; }
        public string src_oid { get; set; }
        public Nullable<int> refund_status { get; set; }
        public string warehouse_no { get; set; }
        public string shop_no { get; set; }
        public string platform_name { get; set; }
        public string warehouse_name { get; set; }
        public string shop_name { get; set; }
        public Nullable<int> platform_id { get; set; }
        public Nullable<System.DateTime> TimeAdd { get; set; }
        public Nullable<int> PageNo { get; set; }
        public Nullable<System.DateTime> modified { get; set; }
        public Nullable<int> IsImport { get; set; }
        public string StockOutCode { get; set; }
        public Nullable<System.DateTime> STime { get; set; }
        public Nullable<System.DateTime> Etime { get; set; }
        public string src_tids { get; set; }
        public Nullable<int> IsInsertToNC { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        public string Reason { get; set; }
        public Nullable<int> IsNCOtherin { get; set; }
    }
}