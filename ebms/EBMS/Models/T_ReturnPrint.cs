//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EBMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_ReturnPrint
    {
        public int ID { get; set; }
        public System.DateTime PostTime { get; set; }
        public string PostUser { get; set; }
        public string PrintNO { get; set; }
        public string PrintType { get; set; }
        public string InWarehouse { get; set; }
        public string OutWarehouse { get; set; }
        public string InGoodsAllocation { get; set; }
        public string OutGoodsAllocation { get; set; }
        public Nullable<int> IsInsertToNC { get; set; }
        public string CheckUser { get; set; }
        public Nullable<System.DateTime> CheckTime { get; set; }
        public string NCBillCode { get; set; }
        public string InsertToNCRemark { get; set; }
    }
}