using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lib
{
    public static class Comm
    {

        /// <summary>
        /// 借支来源
        /// </summary>
        public static List<SelectListItem> BorrowForm = new List<SelectListItem> 
        {
             new SelectListItem { Text = "陈一聘处支付", Value = "陈一聘处支付" },
               new SelectListItem { Text = "haohushi8888@163.com", Value = "haohushi8888@163.com" },
               new SelectListItem { Text = "yjkdyk888@163.com", Value = "yjkdyk888@163.com" }
        };

        //资金调拨 收款银行
        public static List<SelectListItem> ReceivingBank = new List<SelectListItem> 
        {
             new SelectListItem { Text = "==请选择==", Value = "" },
             new SelectListItem { Text = "支付宝",Value="支付宝"},
               new SelectListItem { Text = "中国农业银行",Value="中国农业银行"},
               new SelectListItem { Text = "中国邮政储银行",Value="中国邮政储银行"}
        };



        //资金调拨 付款银行
        public static List<SelectListItem> PayBank = new List<SelectListItem> 
        {
             new SelectListItem { Text = "==请选择==", Value = "" },
             new SelectListItem { Text = "支付宝",Value="支付宝"},
             new SelectListItem { Text = "银行卡",Value="银行卡"},
        };

        //采购 支付方式
        public static List<SelectListItem> Paymen = new List<SelectListItem> 
        {
             new SelectListItem { Text = "==请选择==", Value = "" },
             new SelectListItem { Text = "电汇", Value = "电汇"},
                new SelectListItem { Text = "现金", Value = "现金" },
                new SelectListItem  {Text="月结",Value="月结"}
        };
        //采购 支付方式
        public static List<SelectListItem> PaymenName = new List<SelectListItem> 
        {
            
             new SelectListItem { Text = "向日葵", Value = "李明霞"},
                new SelectListItem { Text = "木兰", Value = "段志红" },
                    new SelectListItem { Text = "悦兮", Value = "肖艳" },
                        new SelectListItem { Text = "菠菜", Value = "王洪波" },
                new SelectListItem  {Text="唐艳",Value="唐艳"}
        };
        /// <summary>
        /// 报销费用类别
        /// </summary>
        public static List<SelectListItem> ExpenseCostType = new List<SelectListItem>
        {
           new SelectListItem { Text = "其它", Value = "0" },
           new SelectListItem { Text = "房租费", Value = "1" },
           new SelectListItem { Text = "工资费用", Value = "2" },
           new SelectListItem { Text = "手续费", Value = "3" },
           new SelectListItem { Text = "福利费", Value = "4" },
           new SelectListItem { Text = "通讯费", Value = "5" },
           new SelectListItem { Text = "差旅费", Value = "6" },
           new SelectListItem { Text = "车辆使用费", Value = "7" },
           new SelectListItem { Text = "业务招待费", Value = "8" },
           new SelectListItem { Text = "误餐费", Value = "9" },
           new SelectListItem { Text = "运杂费", Value = "10" },
           new SelectListItem { Text = "水电费", Value = "11" },
           new SelectListItem { Text = "办公费", Value = "12" },
           new SelectListItem { Text = "返利费用", Value = "13" },
           new SelectListItem { Text = "利息", Value = "14" },
           new SelectListItem { Text = "咨询费", Value = "15" },
           new SelectListItem { Text = "杂费", Value = "16" },
           new SelectListItem { Text = "商业保险费", Value = "17" },
           new SelectListItem { Text = "广告宣传费", Value = "18" },
           new SelectListItem { Text = "培训费", Value = "19" },
           new SelectListItem { Text = "促销推广费", Value = "20" },
           new SelectListItem { Text = "装修费", Value = "21" },
           new SelectListItem { Text = "服装费", Value = "22" },
           new SelectListItem { Text = "新品费", Value = "23" },
           new SelectListItem { Text = "交通费用", Value = "24" },
           new SelectListItem { Text = "制作费", Value = "25" },
           new SelectListItem { Text = "技术费", Value = "26" },
            new SelectListItem { Text = "捐赠", Value = "27" },
            new SelectListItem { Text = "固定资产", Value = "27" },
        };

        //JS时间格式化
        public static IsoDateTimeConverter setTimeFormat()
        {
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return timeFormat;
        }

    }

    /// <summary>
    /// 分页
    /// </summary>
    public class GridPager
    {


        public int rows { get; set; }//每页行数
        public int page { get; set; }//当前页是第几页
        public string order { get; set; }//排序方式
        public string sort { get; set; }//排序列
        public int totalRows { get; set; }//总行数

        public int totalPages //总页数
        {
            get
            {
                return (int)Math.Ceiling((float)totalRows / (float)rows);
            }
        }

    }

    public static class MessageRes
    {
        public static string PleaseSelect { get { return "请选择要操作的记录"; } }
        public static string DelConfirm { get { return "确定删除吗？"; } }
        public static string FaHuoConfirm { get { return "是否强制发货？"; } }
        public static string DelOK { get { return "删除成功"; } }
        public static string SaveOK { get { return "保存成功"; } }
        public static string Empty { get { return "请填写完整"; } }

    }
}
