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
    
    public partial class WDTStockin_to_NCStockin_Log
    {
        public int ID { get; set; }
        public string WDTStockin_no { get; set; }
        public string NCStockin_no { get; set; }
        public Nullable<System.DateTime> CreateTme { get; set; }
        public Nullable<int> IsSuccess { get; set; }
        public string FailReaon { get; set; }
        public string Warehouse_no { get; set; }
        public Nullable<System.DateTime> startTime { get; set; }
        public Nullable<System.DateTime> endTime { get; set; }
        public string goodsList { get; set; }
    }
}