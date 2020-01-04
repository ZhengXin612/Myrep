using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
     [MetadataType(typeof(MOD))]
    public partial class T_AssetsTransferApply
    {
         class MOD
         {

             [Display(Name = "主键")]
             public int ID { get; set; }
             [Display(Name = "资产编码")]
             public string Code { get; set; }
             [Display(Name = "资产名称")]
             public string Name { get; set; }
             [Display(Name = "使用人")]
             public string Owner { get; set; }
             [Display(Name = "部门")]
             public string Department { get; set; }
             [Display(Name = "地点")]
             public string Place { get; set; }
             [Display(Name = "负责人")]
             public string Responsible { get; set; }
             //以下为转移后的
             [Display(Name = "使用人")]
             public string TransferOwner { get; set; }
             [Display(Name = "部门")]
             public string TransferDepartment { get; set; }
             [Display(Name = "地点")]
             public string TransferPlace { get; set; }
             [Display(Name = "申请时间")]
             public Nullable<System.DateTime> PostDate { get; set; }
             [Display(Name = "花名")]
             public string UserName { get; set; }
             [Display(Name = "负责人")]
             public string LastApproveName { get; set; }
             [Display(Name = "备注")]
             public string Memo { get; set; }
             [Display(Name = "类型")]
             public string TransferType { get; set; }
         }
    }
}