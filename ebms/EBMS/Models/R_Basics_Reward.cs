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
    
    public partial class R_Basics_Reward
    {
        public int ID { get; set; }
        public Nullable<int> BasicsID { get; set; }
        public Nullable<System.DateTime> HappenDate { get; set; }
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string RewardType { get; set; }
        public string Content { get; set; }
    }
}