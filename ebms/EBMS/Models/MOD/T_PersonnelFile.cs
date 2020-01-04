using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_PersonnelFile
    {
        class MOD
        {
            [Display(Name = "主键")]
            public int ID { get; set; }
            [Display(Name = "寸照")]
            public string Pic { get; set; }
            [Display(Name = "员工编号")]
            [MaxLength(50)]
            public string Code { get; set; }
            [Display(Name = "身份证号")]
            [MaxLength(20)]
            //[Required(ErrorMessage = "身份证号不能为空")]
            public string IDnum { get; set; }
            [Display(Name = "花名")]
            [MaxLength(50)]
            public string NickName { get; set; }
            [Display(Name = "姓名")]
            [MaxLength(50)]
            [Required(ErrorMessage = "姓名不能为空")]
            public string TrueName { get; set; }
            [Display(Name = "性别")]
            public string Sex { get; set; }
            [Display(Name = "联系方式")]
            [MaxLength(50)]
            public string Tel { get; set; }
            [Display(Name = "工作组")]
            [MaxLength(50)]
            public string Department { get; set; }
            [Display(Name = "岗位")]
            [MaxLength(50)]
            public string Job { get; set; }
            [Display(Name = "学历")]
            [MaxLength(50)]
            public string Edu { get; set; }
            [Display(Name = "毕业院校")]
            [MaxLength(50)]
            public string School { get; set; }
            [Display(Name = "专业")]
            [MaxLength(50)]
            public string Profession { get; set; }
            [Display(Name = "英语等级")]
            [MaxLength(50)]
            public string CET { get; set; }
            [Display(Name = "计算机等级")]
            [MaxLength(50)]
            public string NCRE { get; set; }
            [Display(Name = "其他技能")]
            [MaxLength(50)]
            public string OtherCertificate { get; set; }
            [Display(Name = "婚姻状况")]
            public string MaritalStatus { get; set; }
            [Display(Name = "民族")]
            [MaxLength(50)]
            public string Nation { get; set; }
            [Display(Name = "籍贯")]
            [MaxLength(100)]
            public string NativePlace { get; set; }
            [Display(Name = "政治面貌")]
            [MaxLength(50)]
            public string PoliticsStatus { get; set; }
            [Display(Name = "户籍地址")]
            [MaxLength(100)]
            public string HouseholdRegister { get; set; }
            [Display(Name = "现居住地址")]
            [MaxLength(100)]
            public string PresentAddress { get; set; }
            [Display(Name = "紧急联系人姓名")]
            [MaxLength(50)]
            public string EmergencyContactName { get; set; }
            [Display(Name = "紧急联系人电话")]
            [MaxLength(50)]
            public string EmergencyContactTel { get; set; }
            [Display(Name = "入职时间")]
            public Nullable<System.DateTime> Hiredate { get; set; }
            [Display(Name = "入职师傅")]
            public string HireMaster { get; set; }
            [Display(Name = "转正时间")]
            public Nullable<System.DateTime> zhuanzheng_date { get; set; }
            [Display(Name = "社保缴纳时间")]
            public Nullable<System.DateTime> PaySSDate { get; set; }
            [Display(Name = "第一次劳动合同开始时间")]
            public Nullable<System.DateTime> ContractFirstStartTime { get; set; }
            [Display(Name = "第一次劳动合同截止时间")]
            public Nullable<System.DateTime> ContractFirstDeadline { get; set; }
            [Display(Name = "第二次劳动合同期限")]
            public Nullable<System.DateTime> ContractSecondDeadline { get; set; }
            [Display(Name = "第三次劳动合同期限")]
            public Nullable<System.DateTime> ContractThirdDeadline { get; set; }
            [Display(Name = "宿舍号")]
            [MaxLength(50)]
            public string DormitoryNum { get; set; }
            [Display(Name = "食堂碗柜号")]
            [MaxLength(50)]
            public string CupboardNun { get; set; }
            [Display(Name = "工位号")]
            [MaxLength(50)]
            public string WorkNum { get; set; }
            [Display(Name = "固定资产")]
            [MaxLength(100)]
            public string FixedAssets { get; set; }
            [Display(Name = "入职资料")]
            [MaxLength(100)]
            public string EntryData { get; set; }
            [Display(Name = "在职状态")]
            public string OnJob { get; set; }
            [Display(Name = "本单位工龄")]
            public Nullable<int> OwnUnitsWorkYears { get; set; }
            [Display(Name = "保险办理")]
            [MaxLength(50)]
            public string Insurance { get; set; }
            [Display(Name = "工龄")]
            public Nullable<int> WorkYears { get; set; }
            [Display(Name = "备注")]
            [MaxLength(200)]
            public string Memo { get; set; }
            [Display(Name = "是否办理工资卡")]
            [MaxLength(20)]
            public string SalaryCard { get; set; }
            [Display(Name = "出生年月")]
            public Nullable<System.DateTime> Birthday { get; set; }
            [Display(Name = "部门")]
            [MaxLength(50)]//比如：电子商务部
            public string online { get; set; }
             [Display(Name = "性格")]
            public string Character { get; set; }
             [Display(Name = "应聘岗位")]
            public string ApplyJob { get; set; }
             [Display(Name = "填表时间")]
            public Nullable<System.DateTime> ApplyDate { get; set; }
             [Display(Name = "面试状态")]
            public Nullable<int> InterviewState { get; set; }
             [Display(Name = "当前面试官")]
            public string CurrentInterviewer { get; set; }
             [Display(Name = "面试步骤")]
            public Nullable<int> InterviewStep { get; set; }
            public Nullable<int> DemandID { get; set; }
             [Display(Name = "健康状况")]
            public string HealthState { get; set; }
             [Display(Name = "何时可入职")]
            public Nullable<System.DateTime> CanEntryTime { get; set; }
             [Display(Name = "期望薪资")]
            public string ExpectedSalary { get; set; }
             [Display(Name = "能否出差")]
            public string CanBusinessTravel { get; set; }
             [Display(Name = "阴历或阳历")]
             public string BirthdayType { get; set; }
            [Display(Name = "入职公司")]
            public string SignCompany { get; set; }
        }

    }
}