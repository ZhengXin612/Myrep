using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Joined
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
            [Display(Name = "审批编号")]
            public string Code { get; set; }
            [Display(Name = "申请人")]
            public string PostUser { get; set; }
            [Display(Name = "申请部门")]
            public string PostDepartment { get; set; }
            [Display(Name = "花名")]
            public string Nickname { get; set; }
            [Display(Name = "入职真实姓名")]
            public string Name { get; set; }
            [Display(Name = "性别")]
            public string Sex { get; set; }
            [Display(Name = "电话号码")]
            public string Tel { get; set; }
            [Display(Name = "入职部门")]
            public string Department { get; set; }
            [Display(Name = "入职分组")]
            public string SetGroup { get; set; }
            [Display(Name = "岗位")]
            public string Job { get; set; }
            [Display(Name = "身份证地址")]
            public string IdcardAddress { get; set; }
            [Display(Name = "现在地址")]
            public string Address { get; set; }
            [Display(Name = "紧急联系人")]
            public string EmergencyContact { get; set; }
            [Display(Name = "紧急联系人电话")]
            public string EmergencyTel { get; set; }
            [Display(Name = "其他资料")]
            public string Information { get; set; }
            [Display(Name = "申请内容")]
            public string Memo { get; set; }
            [Display(Name = "审批人")]
            public string FirstApprove { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            [Display(Name = "员工登记")]
            public bool DataUserRegister { get; set; }
            [Display(Name = "应聘申请")]
            public bool DataPost { get; set; }
            [Display(Name = "简历")]
            public bool DataResult { get; set; }
            [Display(Name = "身份证复印")]
            public bool DataIdcard { get; set; }
            [Display(Name = "学历证书")]
            public bool DataQualifications { get; set; }
            [Display(Name = "寸照")]
            public bool DataPhoto { get; set; }
            [Display(Name = "驾驶证")]
             public bool DataDrivingLicense { get; set; }
            [Display(Name = "体检报告")]
           public bool DataExaminationReport { get; set; }
            [Display(Name = "提交时间")]
            public Nullable<System.DateTime> PostTime { get; set; }
            [Display(Name = "入职时间")]
            public Nullable<System.DateTime> Hiredate { get; set; }
            [Display(Name = "试用期")]
            public double Probation { get; set; }
            [Display(Name = "合同期限")]
            public Nullable<System.DateTime> ContractFirstStartTime { get; set; }
            public Nullable<System.DateTime> ContractFirstDeadline { get; set; }
             [Display(Name = "试用期薪酬")]
            public Nullable<double> ProbationSalary { get; set; }
             [Display(Name = "转正后工资")]
            public Nullable<double> MinSalary { get; set; }
        }
    }
}
  
        
        
        
        
        
        
        