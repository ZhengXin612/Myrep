using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
    [MetadataType(typeof(MOD))]
    public partial class T_Freeze
    {
        class MOD
        {
            public int ID { get; set; }
            [Display(Name = "店铺名称")]
            [Required(ErrorMessage = "店铺名称不能为空")]
            public string shopName { get; set; }
            [Display(Name = "支付宝账号")]
            [Required(ErrorMessage = "支付宝账号不能为空")]
            public string alipay { get; set; }
            [Display(Name = "冻结金额")]
            [Required(ErrorMessage = "冻结金额不能为空")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public double freezeMoney { get; set; }
            [Display(Name = "冻结原因")]
            [Required(ErrorMessage = "冻结原因不能为空")]
            public string freezeReason { get; set; }
            [Display(Name = "备注")]
            public string remark { get; set; }
            [Display(Name = "操作人")]
            public string userName { get; set; }
            [Display(Name = "操作时间")]
            public System.DateTime datetime { get; set; }
            [Display(Name = "状态")]
            public int state { get; set; }
            [Display(Name = "是否已经删除")]
            public int isDelete { get; set; }
            [Display(Name = "使用金额")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public Nullable<double> usedMoney { get; set; }
            [Display(Name = "剩余金额")]
            [RegularExpression(@"^(([0-9]+)|([0-9]+\.[0-9]{1,2}))$", ErrorMessage = "金额格式输入错误")]
            public Nullable<double> surplusMoney { get; set; }
            public int Step { get; set; }
        }
    }
}