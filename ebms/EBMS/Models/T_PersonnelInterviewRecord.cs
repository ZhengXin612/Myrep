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
    
    public partial class T_PersonnelInterviewRecord
    {
        public int ID { get; set; }
        public string Interviewer { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> State { get; set; }
        public string Memo { get; set; }
        public Nullable<int> PID { get; set; }
        public int Step { get; set; }
    }
}