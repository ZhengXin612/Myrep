using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class GeneralizeController : BaseController
    {

        #region 属性/字段/公共方法

        EBMSEntities db = new EBMSEntities();

        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }

        public class GeneralizeFlag
        {
            /// <summary>
            /// 支付时间
            /// </summary>
            public string paytime { get; set; }
            /// <summary>
            /// 店铺名称
            /// </summary>
            public string shop_name { get; set; }
            /// <summary>
            /// 平台单号
            /// </summary>
            public string platform_code { get; set; }
            /// <summary>
            /// 会员名称
            /// </summary>
            public string vip_name { get; set; }
            /// <summary>
            /// 仓库名称
            /// </summary>
            public string warehouse_name { get; set; }
            /// <summary>
            /// 支付金额
            /// </summary>
            public Nullable<decimal> payment { get; set; }
            /// <summary>
            /// 订单类型
            /// </summary>
            public string order_type_name { get; set; }
        }

        public List<T_Generalize> lists(int status)
        {
            //IQueryable<T_GeneralizeApprove> listapprove = db.T_GeneralizeApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && !s.ApproveTime.HasValue);
            //if (status == 1)//已审核
            //    listapprove = db.T_GeneralizeApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveTime.HasValue);
            //List<string> itemIds = new List<string>();
            //foreach (var item in listapprove.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            //{
            //    itemIds.Add(item.Key);
            //}

            //foreach (var item in itemIds)
            //{
            //    T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.Guid.Equals(item) && s.IsDelete == 0);
            //    if (model != null)
            //        listGeneralize.Add(model);
            //}
            string sql = "select * from T_Generalize where Guid in (select Pid from T_GeneralizeApprove where ApproveName='" + UserModel.Nickname + "' and ApproveTime is  null group by Pid) and IsDelete=0";
            if (status == 1)//已审核
                sql = "select * from T_Generalize where Guid in (select Pid from T_GeneralizeApprove where ApproveName='" + UserModel.Nickname + "' and ApproveTime is not null group by Pid) and IsDelete=0";
            List<T_Generalize> listGeneralize = db.Database.SqlQuery<T_Generalize>(sql).ToList();
            return listGeneralize;
        }

        public string GetOrderByShopBYcode(string platform_code)
        {
            GY gy = new GY();
            string cmd = "{" +
                "\"appkey\":\"171736\"," +
                "\"method\":\"gy.erp.trade.get\"," +
                "\"page_no\":1," +
                "\"page_size\":10," +
                "\"platform_code\":\"" + platform_code + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
            string sign = gy.Sign(cmd);
            cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            Thread.Sleep(1600);
            if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
            {
                cmd = "{" +
                "\"appkey\":\"171736\"," +
                "\"method\":\"gy.erp.trade.history.get\"," +
                "\"page_no\":1," +
                "\"page_size\":10," +
                "\"platform_code\":\"" + platform_code + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
            }
            return ret;
        }

        #endregion

        #region 视图

        [Description("推广列表")]
        public ActionResult ViewGeneralizeOrderList()
        {
            return View();
        }

        [Description("我的推广列表")]
        public ActionResult ViewGeneralizeForMyList()
        {
            return View();
        }

        [Description("编辑")]
        public ActionResult ViewGeneralizeEdit(int id)
        {
            if (id == 0)
                return HttpNotFound();
            T_Generalize model = db.T_Generalize.Find(id);
            ViewData["store"] = Com.Shop(model.StoreName);
            ViewData["type"] = Com.OrderType(model.OrderType);
            ViewData["thisDate"] = Convert.ToDateTime(model.CreateDate).ToString("yyyy-MM-dd");
            return View(model);
        }

        [Description("订单旗帜")]
        public ActionResult ViewGeneralizeFlag()
        {
            ViewData["shop"] = Com.Shop();
            return View();
        }

        [Description("未审核推广列表")]
        public ActionResult ViewGeneralizeNotCheckedList()
        {
            ViewData["warhouse"] = Com.Warehouses();
            ViewData["storeName"] = Com.Shop();
            return View();
        }

        [Description("已审核推广列表")]
        public ActionResult ViewGeneralizeCheckedList()
        {
            ViewData["warhouse"] = Com.Warehouses();
            return View();
        }

        [Description("推广异常记录")]
        public ActionResult ViewGeneralizeExceptionList()
        {
            ViewData["user"] = db.T_GeneralizeApproveConfig.OrderByDescending(s => s.ID).First().ApproveUser == UserModel.Nickname ? "true" : "false";
            return View();
        }

        [Description("审核")]
        public ActionResult ViewGeneralizeApprove(string ids)
        {
            ViewData["ids"] = ids;
            return View();
        }

        [Description("批量异常处理")]
        public ActionResult ViewGeneralizeExceptionApprove(string ids)
        {
            ViewData["ids"] = ids;
            return View();
        }

        [Description("审核详情")]
        public ActionResult ViewGrnrralizeApproveDetail(string pid)
        {
            if (pid == "")
                return HttpNotFound();
            var history = db.T_GeneralizeApprove.Where(a => a.Pid.Equals(pid));
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                if (item.ApproveStatus == 3) s = "<font color=red>驳回</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View();
        }

        #endregion


        #region Post提交

        /// <summary>
        /// 获取订单旗帜列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public JsonResult GetOrderListFlag(Lib.GridPager pager, string query, string startDate, string endDate)
        {
            IQueryable<GeneralizeFlag> list = null;
            string sql = "select paytime,shop_name,platform_code,vip_name,warehouse_name,payment,order_type_name from T_OrderList where platform_flag='4' and paytime>='" + startDate + "' and paytime<='" + endDate + "' ";

            if (!string.IsNullOrWhiteSpace(query))
                sql += " and shop_name = '" + query + "'";
            list = db.Database.SqlQuery<GeneralizeFlag>(sql).AsQueryable();
            pager.totalRows = list.Count();
            List<GeneralizeFlag> querData = list.OrderByDescending(c => c.paytime).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            var json = new
            {
                total = pager.totalRows,
                rows = (from r in querData
                        select new GeneralizeFlag
                        {
                            paytime = r.paytime,
                            shop_name = r.shop_name,
                            platform_code = r.platform_code,
                            vip_name = r.vip_name,
                            warehouse_name = r.warehouse_name,
                            payment = r.payment,
                            order_type_name = r.order_type_name
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 旗帜导出
        /// </summary>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public FileResult OutPutFlagExcel(string query, string startDate, string EndDate)
        {
            List<GeneralizeFlag> list = null;
            string sql = "select paytime,shop_name,platform_code,vip_name,warehouse_name,payment,order_type_name from T_OrderList where platform_flag='4' and paytime>='" + startDate + "' and paytime<='" + EndDate + "' ";

            if (!string.IsNullOrWhiteSpace(query))
                sql += " and shop_name = '" + query + "'";
            list = db.Database.SqlQuery<GeneralizeFlag>(sql).ToList();
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            row1.CreateCell(0).SetCellValue("支付时间");
            row1.CreateCell(1).SetCellValue("店铺");
            row1.CreateCell(2).SetCellValue("平台单号");
            row1.CreateCell(3).SetCellValue("会员名称");
            row1.CreateCell(4).SetCellValue("仓库");
            row1.CreateCell(5).SetCellValue("支付金额");
            row1.CreateCell(6).SetCellValue("订单类型");
            sheet1.SetColumnWidth(0, 25 * 256);
            sheet1.SetColumnWidth(1, 20 * 256);
            sheet1.SetColumnWidth(2, 20 * 256);
            sheet1.SetColumnWidth(3, 20 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].paytime) ? "" : list[i].paytime.ToString());
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                rowtemp.CreateCell(0).SetCellValue(list[i].paytime.ToString());
                rowtemp.CreateCell(1).SetCellValue(list[i].shop_name.ToString());
                rowtemp.CreateCell(2).SetCellValue(list[i].platform_code.ToString());
                rowtemp.CreateCell(3).SetCellValue(list[i].vip_name.ToString());
                rowtemp.CreateCell(4).SetCellValue(list[i].warehouse_name.ToString());
                rowtemp.CreateCell(5).SetCellValue(list[i].payment == null ? "0" : list[i].payment.ToString());
                rowtemp.CreateCell(6).SetCellValue(list[i].order_type_name.ToString());
            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(4);
            ICell heji2 = heji.CreateCell(5);
            heji1.SetCellValue("合计:");
            heji2.SetCellValue(list.Sum(s => s.payment).ToString());
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "推广数据.xls");
        }

        /// <summary>
        /// 标记
        /// </summary>
        /// <param name="id"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public JsonResult updateIscheck(int id, string IsCheck)
        {
            T_Generalize model = db.T_Generalize.Find(id);
            if (IsCheck == "0")
                model.IsCheck = "1";
            else
                model.IsCheck = "0";
            db.SaveChanges();
            return Json(new { State = "Success" });
        }

        /// <summary>
        /// 匹配管易
        /// </summary>
        /// <returns></returns>
        public JsonResult MatchingGy(string ids)
        {
            try
            {
                List<T_Generalize> listGeneralize = new List<T_Generalize>();
                foreach (var item in ids.Split(','))
                {
                    T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.Guid.Equals(item) && s.IsDelete == 0);
                    if (model != null)
                        listGeneralize.Add(model);
                }
                if (listGeneralize.Count > 150)
                    return Json(new { State = "Faile", Message = "数据超过150条" });
                foreach (var item in listGeneralize)
                {
                    if (string.IsNullOrWhiteSpace(item.OrderNumber))
                        return Json(new { State = "Faile", Message = "匹配失败，订单号为空" });
                    JsonData jsonData = JsonMapper.ToObject(GetOrderByShopBYcode(item.OrderNumber));
                    if (jsonData["orders"].Count > 0)
                    {
                        JsonData jsonOrders = jsonData["orders"][0];
                        decimal jine = decimal.Parse(jsonOrders["payment"].ToString());
                        string shouName = jsonOrders["shop_name"].ToString();
                        if (item.Cost == jine && item.StoreName == shouName)
                            item.IsCheck = "1";
                        else
                            item.IsCheck = "0";
                        item.PlatformFlag = jsonOrders["platform_flag"].ToString();
                        item.IsCancel = jsonOrders["cancle"].ToString();
                        item.IsSend = jsonOrders["delivery_state"].ToString();
                        item.WarhouseName = jsonOrders["warehouse_name"].ToString();
                    }
                    else
                    {
                        item.PlatformFlag = "已过期";
                        item.IsCancel = "已过期";
                        item.IsSend = "已过期";
                    }
                    T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.OrderNumber.Equals(item.OrderNumber) && s.IsDelete == 0);
                    model.IsCheck = item.IsCheck;
                    model.PlatformFlag = item.PlatformFlag;
                    model.IsCancel = item.IsCancel;
                    model.IsSend = item.IsSend;
                    model.WarhouseName = item.WarhouseName;
                    db.SaveChanges();
                }
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }



        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string query, string statedate, string EndDate, int status = 0, int caiwu = 0)
        {
            List<T_Generalize> listGeneralize = lists(status);
            if (!string.IsNullOrWhiteSpace(query))
                listGeneralize = listGeneralize.Where(s => s.ResponsibleName.Contains(query)).ToList();
            DateTime start = Convert.ToDateTime(statedate);
            DateTime end = Convert.ToDateTime(EndDate);
            listGeneralize = listGeneralize.Where(s => s.CreateDate >= start && s.CreateDate <= end).ToList();
            List<T_Generalize> list = listGeneralize;
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            row1.CreateCell(0).SetCellValue("时间");
            row1.CreateCell(1).SetCellValue("上传人");
            row1.CreateCell(2).SetCellValue("平台");
            row1.CreateCell(3).SetCellValue("店铺名称");
            row1.CreateCell(4).SetCellValue("仓库");
            row1.CreateCell(5).SetCellValue("操作人");
            row1.CreateCell(6).SetCellValue("订单号");
            row1.CreateCell(7).SetCellValue("宝贝名称");
            row1.CreateCell(8).SetCellValue("订单类型");
            row1.CreateCell(9).SetCellValue("银行卡");
            row1.CreateCell(10).SetCellValue("金额");
            row1.CreateCell(11).SetCellValue("付佣账号");
            row1.CreateCell(12).SetCellValue("佣金");
            row1.CreateCell(13).SetCellValue("刷手信息");
            row1.CreateCell(14).SetCellValue("旺旺号");
            row1.CreateCell(15).SetCellValue("财付通");
            row1.CreateCell(16).SetCellValue("借支批号");
            row1.CreateCell(17).SetCellValue("是否取消");
            row1.CreateCell(18).SetCellValue("是否发货");
            row1.CreateCell(19).SetCellValue("审核备注");
            row1.CreateCell(21).SetCellValue("备注");
            if (caiwu == 1)
                row1.CreateCell(20).SetCellValue("财务审核备注");
            sheet1.SetColumnWidth(0, 25 * 256);
            sheet1.SetColumnWidth(1, 20 * 256);
            sheet1.SetColumnWidth(2, 20 * 256);
            sheet1.SetColumnWidth(3, 20 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 20 * 256);
            sheet1.SetColumnWidth(8, 20 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            sheet1.SetColumnWidth(10, 20 * 256);
            sheet1.SetColumnWidth(11, 20 * 256);
            sheet1.SetColumnWidth(12, 20 * 256);
            sheet1.SetColumnWidth(13, 20 * 256);
            sheet1.SetColumnWidth(14, 20 * 256);
            sheet1.SetColumnWidth(15, 20 * 256);
            sheet1.SetColumnWidth(16, 20 * 256);
            sheet1.SetColumnWidth(17, 20 * 256);
            sheet1.SetColumnWidth(18, 20 * 256);
            sheet1.SetColumnWidth(19, 20 * 256);
            sheet1.SetColumnWidth(21, 20 * 256);
            for (int i = 0; i < list.Count; i++)
            {
                string guid = list[i].Guid;
                string name = db.T_GeneralizeApproveConfig.OrderBy(s => s.ID).First().ApproveUser;
                string memo = string.IsNullOrWhiteSpace(db.T_GeneralizeApprove.SingleOrDefault(s => s.Pid == guid && s.ApproveName.Equals(name)).Memo) ? "" : db.T_GeneralizeApprove.SingleOrDefault(s => s.Pid == guid && s.ApproveName.Equals(name)).Memo.ToString();
                string memo1 = "";
                if (caiwu == 1)//财务导出需要加财务审核备注
                {
                    memo1 = string.IsNullOrWhiteSpace(db.T_GeneralizeApprove.SingleOrDefault(s => s.Pid == guid && s.ApproveName.Equals(UserModel.Nickname)).Memo) ? "" : db.T_GeneralizeApprove.SingleOrDefault(s => s.Pid == guid && s.ApproveName.Equals(UserModel.Nickname)).Memo.ToString();
                }
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].StoreName) ? "" : list[i].StoreName);
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                rowtemp.CreateCell(0).SetCellValue(list[i].CreateDate == null ? "" : list[i].CreateDate.ToString());
                rowtemp.CreateCell(1).SetCellValue(list[i].UploadName == null ? "" : list[i].UploadName.ToString());
                rowtemp.CreateCell(2).SetCellValue(list[i].PlatformCode == null ? "" : list[i].PlatformCode.ToString());
                rowtemp.CreateCell(3).SetCellValue(list[i].StoreName == null ? "" : list[i].StoreName.ToString());
                rowtemp.CreateCell(4).SetCellValue(list[i].WarhouseName == null ? "" : list[i].WarhouseName.ToString());
                rowtemp.CreateCell(5).SetCellValue(list[i].PostUser == null ? "" : list[i].PostUser.ToString());
                rowtemp.CreateCell(6).SetCellValue(list[i].OrderNumber == null ? "" : list[i].OrderNumber.ToString());
                rowtemp.CreateCell(7).SetCellValue(list[i].ProductName == null ? "" : list[i].ProductName.ToString());
                rowtemp.CreateCell(8).SetCellValue(list[i].OrderType == null ? "" : list[i].OrderType.ToString());
                rowtemp.CreateCell(9).SetCellValue(list[i].BankNumber == null ? "" : list[i].BankNumber.ToString());
                rowtemp.CreateCell(10).SetCellValue(Convert.ToDouble(list[i].Cost).ToString("0.00"));
                rowtemp.CreateCell(11).SetCellValue(list[i].PayCommissionNumber == null ? "" : list[i].PayCommissionNumber.ToString());
                rowtemp.CreateCell(12).SetCellValue(Convert.ToDouble(list[i].CommissionCost).ToString("0.00"));
                rowtemp.CreateCell(13).SetCellValue(list[i].DKUserMessage == null ? "" : list[i].DKUserMessage.ToString());
                rowtemp.CreateCell(14).SetCellValue(list[i].AliNumber == null ? "" : list[i].AliNumber.ToString());
                rowtemp.CreateCell(15).SetCellValue(list[i].TenPay == null ? "" : list[i].TenPay.ToString());
                rowtemp.CreateCell(16).SetCellValue(list[i].BorrowCode == null ? "" : list[i].BorrowCode.ToString());
                string cancel = "";
                string send = "";
                switch (list[i].IsCancel)
                {
                    case "True":
                        cancel = "取消";
                        break;
                    case "已过期":
                        cancel = "已过期";
                        break;
                    case "":
                        cancel = "未匹配";
                        break;
                    default:
                        cancel = "未取消";
                        break;
                }
                switch (list[i].IsSend)
                {
                    case "2":
                        send = "已发货";
                        break;
                    case "已过期":
                        send = "已过期";
                        break;
                    case "":
                        send = "未匹配";
                        break;
                    default:
                        send = "未发货";
                        break;
                }
                rowtemp.CreateCell(17).SetCellValue(cancel);
                rowtemp.CreateCell(18).SetCellValue(send);
                rowtemp.CreateCell(19).SetCellValue(memo);
                rowtemp.CreateCell(21).SetCellValue(list[i].Memo == null ? "" : list[i].Memo);
                if (caiwu == 1)
                    rowtemp.CreateCell(20).SetCellValue(memo1);
            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(9);
            ICell heji2 = heji.CreateCell(10);
            ICell heji3 = heji.CreateCell(12);
            heji1.SetCellValue("合计");
            heji2.SetCellValue(list.Sum(s => s.Cost).ToString());
            heji3.SetCellValue(list.Sum(s => s.CommissionCost).ToString());
            if (caiwu == 1)//财务导出
            {
                #region 表头

                IRow caiwushuju = sheet1.CreateRow(list.Count() + 5);
                ICell caiwushuju1 = caiwushuju.CreateCell(0);
                ICell caiwushuju12 = caiwushuju.CreateCell(1);
                ICell caiwushuju13 = caiwushuju.CreateCell(2);
                ICell caiwushuju14 = caiwushuju.CreateCell(3);
                ICell caiwushuju15 = caiwushuju.CreateCell(4);
                caiwushuju12.SetCellValue("银行卡");
                caiwushuju13.SetCellValue("财付通");
                caiwushuju14.SetCellValue("合计");
                caiwushuju15.SetCellValue("备注");

                #endregion

                #region 子项

                IRow child = sheet1.CreateRow(list.Count() + 6);
                ICell childs = child.CreateCell(0);
                childs.SetCellValue("期初");

                IRow child1 = sheet1.CreateRow(list.Count() + 7);
                ICell child1s = child1.CreateCell(0);
                child1s.SetCellValue("唐艳");

                IRow child2 = sheet1.CreateRow(list.Count() + 8);
                ICell child2s = child2.CreateCell(0);
                child2s.SetCellValue("退款");

                IRow child3 = sheet1.CreateRow(list.Count() + 9);
                ICell child3s = child3.CreateCell(0);
                child3s.SetCellValue("转给财付通");

                IRow child4 = sheet1.CreateRow(list.Count() + 10);
                ICell child4s = child4.CreateCell(0);
                ICell child4s1 = child4.CreateCell(1);
                ICell child4s2 = child4.CreateCell(2);
                ICell child4s3 = child4.CreateCell(3);
                child4s.SetCellValue("推广回款");
                child4s1.SetCellValue(list.Sum(s => s.Cost).ToString());
                child4s2.SetCellValue(list.Sum(s => s.CommissionCost).ToString());
                child4s3.SetCellValue(Convert.ToDecimal(list.Sum(s => s.Cost) + list.Sum(s => s.CommissionCost)).ToString());

                IRow child5 = sheet1.CreateRow(list.Count() + 11);
                ICell child5s = child5.CreateCell(0);
                child5s.SetCellValue("浏览费");

                IRow child6 = sheet1.CreateRow(list.Count() + 12);
                ICell child6s = child6.CreateCell(0);
                child6s.SetCellValue("短信费");

                IRow child7 = sheet1.CreateRow(list.Count() + 13);
                ICell child7s = child7.CreateCell(0);
                child7s.SetCellValue("末期");

                #endregion

                #region 店铺

                IRow store = sheet1.CreateRow(list.Count() + 18);
                ICell store1 = store.CreateCell(0);
                ICell store2 = store.CreateCell(1);
                ICell store3 = store.CreateCell(2);
                ICell store4 = store.CreateCell(3);
                store1.SetCellValue("店铺名称");
                store2.SetCellValue("推广费用汇款金额");
                store3.SetCellValue("佣金");
                store4.SetCellValue("汇总");
                var listStore = list.GroupBy(s => s.StoreName).Select(a => new { name = a.Key, cost = a.Sum(s => s.Cost), com = a.Sum(s => s.CommissionCost), heji = a.Sum(s => s.Cost) + a.Sum(s => s.CommissionCost) });
                int j = 19;
                foreach (var item in listStore)
                {
                    IRow stores = sheet1.CreateRow(list.Count() + j);
                    ICell stores1 = stores.CreateCell(0);
                    ICell stores2 = stores.CreateCell(1);
                    ICell stores3 = stores.CreateCell(2);
                    ICell stores4 = stores.CreateCell(3);
                    stores1.SetCellValue(item.name.ToString());
                    stores2.SetCellValue(item.cost.ToString());
                    stores3.SetCellValue(item.com.ToString());
                    stores4.SetCellValue(item.heji.ToString());
                    j++;
                }
                IRow storesum = sheet1.CreateRow(list.Count() + j);
                ICell storesum1 = storesum.CreateCell(0);
                ICell storesum2 = storesum.CreateCell(1);
                ICell storesum3 = storesum.CreateCell(2);
                ICell storesum4 = storesum.CreateCell(3);
                storesum1.SetCellValue("合计");
                storesum2.SetCellValue(listStore.Sum(s => s.cost).ToString());
                storesum3.SetCellValue(listStore.Sum(s => s.com).ToString());
                storesum4.SetCellValue(listStore.Sum(s => s.heji).ToString());
                #endregion

            }

            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "推广数据.xls");
        }

        /// <summary>
        /// 获取推广列表信息
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ContentResult GetGeneralizeOrderList(Lib.GridPager pager, string query, string startDate, string endDate)
        {
            IQueryable<T_Generalize> list = db.T_Generalize.Where(s => s.IsDelete != 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.StoreName.Contains(query) || s.ProductName.Contains(query) || s.PostUser.Contains(query) || s.UploadName.Contains(query));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = Convert.ToDateTime(startDate);
                list = list.Where(s => s.CreateDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = Convert.ToDateTime(endDate);
                list = list.Where(s => s.CreateDate <= end);
            }
            pager.totalRows = list.Count();
            List<T_Generalize> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 推广异常记录
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ContentResult GetGeneralizeExceptionList(Lib.GridPager pager, string query, string startDate, string endDate)
        {
            IQueryable<T_Generalize> list = db.T_Generalize.Where(s => s.IsDelete != 1 && s.Status == 2).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.StoreName.Contains(query) || s.ProductName.Contains(query) || s.PostUser.Contains(query) || s.UploadName.Contains(query));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = Convert.ToDateTime(startDate);
                list = list.Where(s => s.CreateDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = Convert.ToDateTime(endDate);
                list = list.Where(s => s.CreateDate <= end);
            }
            //pager.totalRows = list.Count();
            //List<GrneralizeCost> footerList = new List<GrneralizeCost>();
            //GrneralizeCost cost = new GrneralizeCost();
            //cost.BankNumber = "合计：";
            //if (list.Count() > 0)
            //{
            //    cost.Cost = list.Sum(s => s.Cost);
            //    cost.CommissionCost = list.Sum(s => s.CommissionCost);
            //}
            //else
            //{
            //    cost.Cost = 0;
            //    cost.CommissionCost = 0;
            //}
            //footerList.Add(cost);
            List<T_Generalize> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GeneralizeDelete(string ids)
        {
            try
            {
                string[] Ids = ids.Split(',');
                for (int i = 0; i < Ids.Length; i++)
                {
                    int id = int.Parse(Ids[i]);
                    T_Generalize model = db.T_Generalize.Find(id);
                    if (model.Status != -1 && model.Status != 3)
                        return Json(new { State = "Faile", Message = "只有未审批/被驳回状态才可删除" });
                    model.IsDelete = 1;
                    //if (!string.IsNullOrEmpty(model.OrderNumber))
                    //{
                    //    T_OrderList order = db.T_OrderList.SingleOrDefault(s => s.platform_code.Equals(model.OrderNumber));
                    //    order.IsGeneralize = 0;
                    //    db.SaveChanges();
                    //}

                  //  ModularByZP();
                }
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "未审核推广列表").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_GeneralizeApprove where  Pid in ( select Guid from T_Generalize where isDelete=0  and ( status=-1 or status=0)) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "未审核推广列表" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "未审核推广列表";
                    ModularNotauditedModel.RejectNumber = 0;
                    ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    ModularNotauditedModel.ToupdateName = Nickname;
                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
                }
                db.SaveChanges();
            }

            //增加驳回数据
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_Generalize where Status='2' and IsDelete=0  GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "未审核推广列表" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "未审核推广列表";
                    ModularNotauditedModel.NotauditedNumber = 0;
                    ModularNotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RejectNumberQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    ModularNotauditedModel.ToupdateName = Nickname;
                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
                }
                db.SaveChanges();
            }
        }
        /// <summary>
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewGeneralizeEditSave(T_Generalize model, string thisdate)
        {
            try
            {
                T_Generalize ge = db.T_Generalize.Find(model.ID);
                ge.CreateDate = Convert.ToDateTime(thisdate);
                ge.PlatformCode = model.PlatformCode;
                ge.StoreName = model.StoreName;
                ge.OrderNumber = model.OrderNumber;
                ge.ProductName = model.ProductName;
                ge.Cost = model.Cost;
                ge.CommissionCost = model.CommissionCost;
                ge.DKUserMessage = model.DKUserMessage;
                ge.AliNumber = model.AliNumber;
                ge.TenPay = model.TenPay;
                ge.BankNumber = model.BankNumber;
                ge.OrderType = model.OrderType;
                ge.PayCommissionNumber = model.PayCommissionNumber;
                ge.BorrowCode = model.BorrowCode;
                ge.Memo = model.Memo;
                ge.ResponsibleName = model.ResponsibleName;
                ge.Status = -1;
                db.SaveChanges();
                if (model.Status == 3)//驳回
                {
                    T_GeneralizeApprove approve = new T_GeneralizeApprove();
                    approve.ApproveName = db.T_GeneralizeApproveConfig.First().ApproveUser;
                    approve.ApproveStatus = -1;
                    approve.Pid = ge.Guid;
                    db.T_GeneralizeApprove.Add(approve);
                    db.SaveChanges();
                }
                //ModularByZP();

                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public JsonResult CheckException(string ids, int status, string memo)
        {
            string[] Ids = ids.Split(',');
            for (int i = 0; i < Ids.Length; i++)
            {
                string guid = Ids[i];
                T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.Guid.Equals(guid));
                if (model.IsDispose == 1)
                    return Json(new { State = "Faile", Message = "该数据已处理" });
                model.IsDispose = 1;
                db.SaveChanges();
                T_GeneralizeApprove approve = new T_GeneralizeApprove
                {
                    ApproveName = UserModel.Nickname,
                    ApproveTime = DateTime.Now,
                    ApproveStatus = status,
                    Memo = memo,
                    Pid = guid
                };
                db.T_GeneralizeApprove.Add(approve);
                db.SaveChanges();
            }
            return Json(new { State = "Success" });
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="approveIDs"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public JsonResult Check(string ids, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string[] Ids = ids.Split(',');
                    for (int i = 0; i < Ids.Length; i++)
                    {
                        string guid = Ids[i];
                        T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.Guid.Equals(guid));
                        model.Status = status;
                        db.SaveChanges();
                        T_GeneralizeApprove approve = db.T_GeneralizeApprove.SingleOrDefault(s => !s.ApproveTime.HasValue && s.Pid == guid);
                        approve.Memo = memo;
                        approve.ApproveTime = DateTime.Now;
                        approve.ApproveStatus = status;
                        db.SaveChanges();
                        //同意
                        if (status == 1)
                        {
                            //如果存在下一级审核人
                            int step = db.T_GeneralizeApproveConfig.SingleOrDefault(s => s.ApproveUser.Equals(UserModel.Nickname)).Step;
                            int stepLast = db.T_GeneralizeApproveConfig.OrderByDescending(s => s.Step).First().Step;
                            if (stepLast - 1 > step)
                            {
                                model.Status = 0;
                                db.SaveChanges();
                                T_GeneralizeApprove approveModel = new T_GeneralizeApprove
                                {
                                    ApproveName = db.T_GeneralizeApproveConfig.OrderBy(s => s.Step).First(s => s.Step > step).ApproveUser,
                                    ApproveStatus = -1,
                                    Pid = guid
                                };
                                db.T_GeneralizeApprove.Add(approveModel);
                                db.SaveChanges();
                            }
                        }
                    }

                    //ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }

        /// <summary>
        /// 我的推广列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ContentResult GetGeneralizeForMyList(Lib.GridPager pager, string query, int status = -2)
        {
            IQueryable<T_Generalize> list = db.T_Generalize.Where(s => s.UploadName.Equals(UserModel.Nickname) && s.IsDelete != 1).AsQueryable();
            if (status != -2)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.StoreName.Contains(query) || s.ProductName.Contains(query) || s.ResponsibleName.Contains(query) || s.PlatformCode.Contains(query) || s.BorrowCode.Contains(query));
            pager.totalRows = list.Count();
            //List<GrneralizeCost> footerList = new List<GrneralizeCost>();
            //GrneralizeCost cost = new GrneralizeCost();
            //cost.BankNumber = "合计：";
            //if (list.Count() > 0)
            //{
            //    cost.Cost = list.Sum(s => s.Cost);
            //    cost.CommissionCost = list.Sum(s => s.CommissionCost);
            //}
            //else
            //{
            //    cost.Cost = 0;
            //    cost.CommissionCost = 0;
            //}
            //footerList.Add(cost);
            List<T_Generalize> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 审核列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGeneralizeApproveList(Lib.GridPager pager, string query, string startDate, string endDate, string warhouse, string storeName, string sendStatus, int staus = 0)
        {
            List<T_Generalize> list = lists(staus);
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.ResponsibleName.Contains(query) || s.OrderNumber.Equals(query) || s.AliNumber.Equals(query)).ToList();
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = Convert.ToDateTime(startDate);
                list = list.Where(s => s.CreateDate >= start).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = Convert.ToDateTime(endDate);
                list = list.Where(s => s.CreateDate <= end).ToList();
            }
            if (storeName != "==请选择==" && storeName != null)
                list = list.Where(s => s.StoreName.Equals(storeName)).ToList();
            if (warhouse != "==请选择==" && warhouse != null)
                list = list.Where(s => s.WarhouseName.Equals(warhouse)).ToList();
            if (sendStatus != "==请选择==" && sendStatus != null)
            {
                if (sendStatus == "未发货")
                    list = list.Where(s => s.IsSend != "2" && s.IsSend != "已过期" && s.IsSend != "").ToList();
                else
                    list = list.Where(s => s.IsSend.Equals(sendStatus)).ToList();
            }
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

    }
}
