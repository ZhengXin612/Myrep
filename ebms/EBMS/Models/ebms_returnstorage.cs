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
    
    public partial class ebms_returnstorage
    {
        public int ID { get; set; }
        public int oid { get; set; }
        public int detail_id { get; set; }
        public System.DateTime created { get; set; }
        public string warehouse { get; set; }
        public string expressnumber { get; set; }
        public Nullable<System.DateTime> sortingdate { get; set; }
        public string typ { get; set; }
        public string shop_no { get; set; }
        public string ordernumber { get; set; }
        public string wangwang { get; set; }
        public string goods_no { get; set; }
        public string goods_name { get; set; }
        public string spec_name { get; set; }
        public int qty { get; set; }
        public decimal share_amount { get; set; }
        public Nullable<int> deliverstatus { get; set; }
        public int snum { get; set; }
        public string housename { get; set; }
        public string express { get; set; }
        public System.DateTime receiptdate { get; set; }
        public Nullable<int> IsInsertToNC { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        public string Reason { get; set; }
    }
}