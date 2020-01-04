using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class ExchangeCenterController : BaseController
    {

        #region 属性/公共字段/方法

        EBMSEntities db = new EBMSEntities();
        wdt_bakEntities db_ex = new wdt_bakEntities();
        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }

        //旺店通接口
        public static string GetTimeStamp()
        {
            return (GetTimeStamp(System.DateTime.Now));
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time, int length = 10)
        {
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString().Substring(0, length);
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为10位      
            return t;
        }

        public string CreateParam(Dictionary<string, string> dicReq, bool isLower = false)
        {
            //排序
            dicReq = dicReq.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in dicReq)
            {
                if (item.Key == "sign")
                    continue;
                if (i > 0)
                {
                    sb.Append(";");
                }
                i++;
                sb.Append(item.Key.Length.ToString("00"));
                sb.Append("-");
                sb.Append(item.Key);
                sb.Append(":");

                sb.Append(item.Value.Length.ToString("0000"));
                sb.Append("-");
                sb.Append(item.Value);
            }
            if (isLower)
                dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401").ToLower());
            else
            {
                dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401"));
            }
            sb = new StringBuilder();
            i = 0;
            foreach (var item in dicReq)
            {
                if (i == 0)
                {

                    sb.Append(string.Format("{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
                }
                else
                {
                    sb.Append(string.Format("&{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
                }
                i++;
            }
            // HttpUtility.UrlEncode(
            return sb.ToString();
        }
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
            string strMd5 = BitConverter.ToString(result);
            strMd5 = strMd5.Replace("-", "");
            return strMd5;// System.Text.Encoding.Default.GetString(result);
        }
        Dictionary<string, string> dic = new Dictionary<string, string>();


        [Description("得到管易的订单数据")]
        public JsonResult GetExchangeByGy(string code)
        {
            string repeat = "";
            //去重

            string scode = code.Substring(0, 1);
            GY gy = new GY();


            List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0" && a.Status != 3).ToList();
            if (modelList.Count > 0)
            {

                repeat += "_存在退货退款记录重复，";
            }
            //查是否有返现记录

            List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 3).ToList();
            if (cash.Count > 0)
            {
                repeat += "_存在返现记录重复，";
            }

            List<T_ExchangeCenter> exchangeList = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
            if (exchangeList.Count > 0)
            {
                repeat += "_存在换货记录重复，";
            }


            dic.Clear();

            dic.Add("src_tid", code);
            //dic.Add("trade_no", code);

            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());
            string cmd = CreateParam(dic, true);

            string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/trade_query.php", cmd, Encoding.UTF8);
            string ssx = Regex.Unescape(ret);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string iscode = jsonData["total_count"].ToString();
            //total_count
            if (iscode != "0")
            {
                JsonData jsontrades = jsonData["trades"];
                List<string> userInfo = new List<string>();
                if (jsontrades.Count != 0)
                {
                    JsonData trades = jsontrades[0];
                    //店铺名称
                    string shop_name = trades["shop_name"].ToString();
                    //仓库编码
                    string warehouse_no = trades["warehouse_no"].ToString();
                    //原始订单编号
                    string src_tids = trades["src_tids"].ToString();
                    //下单时间
                    string trade_time = trades["trade_time"].ToString();
                    //付款时间
                    string pay_time = trades["pay_time"].ToString()== "0000-00-00 00:00:00"?"": trades["pay_time"].ToString();
                    //旺旺号
                    string customer_name = trades["buyer_nick"].ToString();
                    if (string.IsNullOrEmpty(customer_name))
                    {
                        userInfo.Add("buyer_nick");
                    }
                    //收件人姓名
                    string receiver_name = trades["receiver_name"].ToString();
                    if (string.IsNullOrEmpty(receiver_name))
                    {
                        userInfo.Add("receiver_name");
                    }
                    //省
                    string receiver_province = trades["receiver_province"].ToString();
                    //市
                    string receiver_city = trades["receiver_city"].ToString();
                    //区
                    string receiver_district = trades["receiver_district"].ToString();
                    //详细地址
                    string receiver_address = trades["receiver_address"].ToString();
                    if (string.IsNullOrEmpty(receiver_address))
                    {
                        userInfo.Add("receiver_address");
                    }
                    //电话号码
                    string receiver_mobile = trades["receiver_mobile"].ToString();
                    if (string.IsNullOrEmpty(receiver_mobile))
                    {
                        userInfo.Add("receiver_mobile");
                    }
                    try
                    {
                        sales_trade retreat = db_ex.sales_trade.FirstOrDefault(a => a.src_tids == code);
                        if (retreat != null)
                        {
                            foreach (var item in userInfo)
                            {
                                switch (item)
                                {
                                    case "buyer_nick":
                                        customer_name = retreat.buyer_nick;
                                        break;
                                    case "receiver_name":
                                        receiver_name = retreat.receiver_name;
                                        break;
                                    case "receiver_address":
                                        receiver_address = retreat.receiver_address;
                                        break;
                                    case "receiver_mobile":
                                        receiver_mobile = retreat.receiver_mobile;
                                        break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        return Json("数据获取失败", JsonRequestBehavior.AllowGet);
                        //return Json(new { State = "Fail", Message = "数据获取失败," + ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                    //邮政编码
                    string receiver_zip = trades["receiver_zip"].ToString();
                    //省市县
                    string receiver_area = trades["receiver_area"].ToString();
                    //快递公司编号
                    string logistics_code = trades["logistics_code"].ToString();
                    //快递单号
                    string logistics_no = trades["logistics_no"].ToString();
                    //买家留言
                    string buyer_message = trades["buyer_message"].ToString();
                    //客服备注
                    string cs_remark = trades["cs_remark"].ToString();
                    //实付金额
                    double paid = 0.00;// trades["paid"].ToString();
                                       //商品详情
                                       // JsonData goods_list = trades["goods_list"];
                    string shop_no = trades["shop_no"].ToString();
                    List<T_ExchangeDetail> DetailsList = new List<T_ExchangeDetail>();
                    for (int c = 0; c < jsontrades.Count; c++)
                    {
                        paid += double.Parse(jsontrades[c]["paid"].ToString());
                        JsonData goods_list = jsontrades[c]["goods_list"];


                        for (int i = 0; i < goods_list.Count; i++)
                        {

                            string goods_no = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                            int qyt = Convert.ToInt32(Convert.ToDecimal(goods_list[i]["actual_num"].ToString()));
                            T_ExchangeDetail ExistDetail = DetailsList.FirstOrDefault(a => a.SendProductCode == goods_no);
                            if (ExistDetail != null)
                            {
                                ExistDetail.SendProductNum += qyt;

                            }
                            else
                            {
                                T_ExchangeDetail DetailsModel = new T_ExchangeDetail();
                                DetailsModel.SendProductCode = goods_no;

                                DetailsModel.SendProductName = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();

                                DetailsModel.SendProductNum = qyt;
                                if (qyt > 0)
                                {
                                    DetailsList.Add(DetailsModel);
                                }
                            }

                        }
                    }
                    //查询3次。对应到具体的省市区
                    if (receiver_province != null)
                    {

                        DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_province);
                        if (commonarea != null)
                        {
                            receiver_province = commonarea.REGION_NAME;
                        }
                    }
                    if (receiver_city != null)
                    {

                        DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_city);
                        if (commonarea != null)
                        {
                            receiver_city = commonarea.REGION_NAME;
                        }
                        if (receiver_city == "市辖区")
                        {
                            receiver_city = receiver_province;
                            receiver_province = receiver_province.Substring(0, receiver_province.Length - 1);


                        }
                    }
                    if (receiver_district != null)
                    {

                        DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_district);
                        if (commonarea != null)
                        {
                            receiver_district = commonarea.REGION_NAME;
                        }
                    }
                    string ssq = receiver_province + "-" + receiver_city + "-" + receiver_district;
                    //查询一次..
                    string shop_Code = shop_no;

                    T_ExchangeCenter exchange = new T_ExchangeCenter
                    {
                        OrderCode = code,
                        VipName = customer_name,
                        StoreName = shop_name,
                        ReceivingName = receiver_name,
                        VipCode = receiver_name,
                        NeedPostalCode = receiver_zip,
                        ReceivingTelPhone = receiver_mobile,
                        ReceivingAddress = receiver_address,
                        AddressMessage = ssq,
                        SingleTime = string.IsNullOrWhiteSpace(trade_time) ? DateTime.Now.ToString() : trade_time,
                        SystemRemark = repeat,
                        StoreCode = shop_Code
                    };



                    var json = new
                    {

                        rows = (from r in DetailsList
                                select new T_ExchangeDetail
                                {
                                    SendProductCode = r.SendProductCode,
                                    SendProductName = r.SendProductName,
                                    SendProductNum = r.SendProductNum
                                }).ToArray()
                    };

                    return Json(new { State = "Success", ModelList = exchange, Json = json }, JsonRequestBehavior.AllowGet);

                }




            }
            return Json(new { State = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region 视图


        [Description("订单列表")]
        public ActionResult ViewOrderList()
        {
            return View();
        }

        [Description("需发产品")]
        public ActionResult ViewProduct(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("寄回产品")]
        public ActionResult ViewSendProduct(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("换货申请")]
        public ActionResult ViewExchangeGoodsAdd()
        {
            //T_OrderList order = db.T_OrderList.Find(ID);
            //T_ExchangeCenter exchange = new T_ExchangeCenter
            //{
            //    OrderCode = order.platform_code,
            //    VipName = order.vip_name,
            //    StoreName = order.shop_name,
            //    ReceivingName = order.receiver_name,
            //    NeedPostalCode = order.receiver_zip,
            //    ReceivingPhone = order.receiver_phone,
            //    ReceivingTelPhone = order.receiver_mobile,
            //    VipCode = order.vip_code,
            //    ReceivingAddress = order.receiver_address,
            //    AddressMessage = order.receiver_area,
            //    OrderId = ID
            //};

            ViewData["returnwarhouseName"] = Com.Warehouses();
            ViewData["warhouse"] = Com.Warehouses();
            ViewData["ReturnExpressNameList"] = Com.ExpressName();
            ViewData["NeedExpressName"] = Com.WDTExpressName();
            ViewData["depId"] = UserModel.DepartmentId;
            //ViewData["code"] = order.code;
            ViewData["reson"] = Com.ExchangeReson();
            return View();
        }

        [Description("换货编辑")]
        public ActionResult ViewExchangeGoodsEdit(int exchangeId)
        {
            if (exchangeId == 0)
                return HttpNotFound();
            T_ExchangeCenter model = db.T_ExchangeCenter.Find(exchangeId);
            ViewData["returnwarhouseName"] = Com.Warehouses();
            ViewData["warhouse"] = Com.Warehouses(model.NeedWarhouse);
            ViewData["NeedexpressName"] = Com.WDTExpressName();
            ViewData["ReturnExpressName"] = Com.ExpressName(model.ReturnExpressName);
            ViewData["depId"] = UserModel.DepartmentId;
            ViewData["Rwarhouse"] = Com.Warehouses(model.ReturnWarhouse);
            ViewData["reson"] = Com.ExchangeReson(model.ExchangeReson);
            return View(model);
        }

        [Description("我的换货")]
        public ActionResult ViewExchangeGoodsForMy()
        {
            return View();
        }

        [Description("换货详情")]
        public ActionResult ViewExchangeGoodsDetail(int exchangeId)
        {
            if (exchangeId == 0)
                return HttpNotFound();
            var history = db.T_ExchangeCenterApprove.Where(a => a.Pid == exchangeId);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["exchangeId"] = exchangeId;
            return View();
        }

        [Description("换货管理列表")]
        public ActionResult ViewExchangeGoodsManager()
        {
            return View();
        }

        [Description("换货未审核")]
        public ActionResult ViewExchangeGoodsNotCheck()
        {
            ViewData["WarehouseList"] = Com.Warehouses();
            return View();
        }

        [Description("换货已审核")]
        public ActionResult ViewExchangeGoodsChecked()
        {
            return View();
        }


        [Description("仓库收货")]
        public ActionResult ViewExchangeGoodsWarhouse(int id)
        {
            if (id == 0)
                return HttpNotFound();
            T_ExchangeCenter model = db.T_ExchangeCenter.Find(id);
            model.NeedWarhouse = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.NeedWarhouse)).name;
            //  model.NeedExpress = db.T_Express.SingleOrDefault(s => s.Code.Equals(model.NeedExpress)).Name;
            model.ReturnWarhouse = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.ReturnWarhouse)).name;
            //获取审核表中的 审核记录ID
            T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.FirstOrDefault(a => !a.ApproveTime.HasValue && a.Pid == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
                ViewData["approveid"] = 0;
            return View(model);
        }

        [Description("换货审核")]
        public ActionResult ViewExchangeGoodsApprove(int id)
        {
            var model = db.T_ExchangeCenter.Find(id);
            if (model == null)
                return HttpNotFound();
            var history = db.T_ExchangeCenterApprove.Where(a => a.Pid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";

            int Step = db.T_ExchangeCenterConfig.ToList().Count;
            ViewData["Step"] = Step;
            //获取审核表中的 审核记录ID
            T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.FirstOrDefault(a => !a.ApproveTime.HasValue && a.Pid == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
                ViewData["approveid"] = 0;
            model.ReturnWarhouse = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.ReturnWarhouse)).name;
            //   model.ReturnExpressName = db.T_Express.SingleOrDefault(s => s.Code.Equals(model.ReturnExpressName)).Name;
            return View(model);
        }

        #endregion


        #region Post提交


        /// <summary>
        /// 导出excel
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string startDate, string endDate)
        {


            //List<T_ExchangeCenter> borrowList = new List<T_ExchangeCenter>();
            //IQueryable<T_ExchangeCenterApprove> list = db.T_ExchangeCenterApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveTime.HasValue);
            //List<int> itemIds = new List<int>();
            //foreach (var item in list.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            //{
            //    itemIds.Add(item.Key);
            //}

            //foreach (var item in itemIds)
            //{
            //    T_ExchangeCenter model = db.T_ExchangeCenter.SingleOrDefault(s => s.ID == item && s.IsDelete == 0);
            //    if (model != null)
            //        borrowList.Add(model);
            //}
            string sql = "select * from T_ExchangeCenter where ID in (select Pid from T_ExchangeCenterApprove where ApproveName='" + UserModel.Nickname + "' and ApproveTime is not null group by Pid) and IsDelete=0";
            List<T_ExchangeCenter> borrowList = db.Database.SqlQuery<T_ExchangeCenter>(sql).ToList();
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                borrowList = borrowList.Where(s => s.CreateDate >= start).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                borrowList = borrowList.Where(s => s.CreateDate <= end).ToList();
            }
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("店铺名称");
            row1.CreateCell(1).SetCellValue("会员名称");
            row1.CreateCell(2).SetCellValue("订单号");
            row1.CreateCell(3).SetCellValue("仓库名称");
            row1.CreateCell(4).SetCellValue("物流公司");
            row1.CreateCell(5).SetCellValue("卖家备注");
            row1.CreateCell(6).SetCellValue("拍单日期");
            row1.CreateCell(7).SetCellValue("订单类型");
            row1.CreateCell(8).SetCellValue("买家留言");
            row1.CreateCell(9).SetCellValue("收货人");
            row1.CreateCell(10).SetCellValue("固定电话");
            row1.CreateCell(11).SetCellValue("手机号码");
            row1.CreateCell(12).SetCellValue("邮政编码");
            row1.CreateCell(13).SetCellValue("地址信息");
            row1.CreateCell(14).SetCellValue("会员名称");
            row1.CreateCell(15).SetCellValue("制单人");
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
            for (int i = 0; i < borrowList.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].StoreName) ? "" : borrowList[i].StoreName);
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";

                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                string code = borrowList[i].NeedWarhouse.ToString();
                string code1 = borrowList[i].NeedExpress.ToString();
                string warhouseName = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(code)).name;
                //   string expressName = db.T_Express.SingleOrDefault(s => s.Code.Equals(code1)).Name;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].StoreName) ? "" : borrowList[i].StoreName.ToString());
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].VipName) ? "" : borrowList[i].VipName.ToString());
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].OrderCode) ? "" : borrowList[i].OrderCode.ToString());
                rowtemp.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].NeedWarhouse) ? "" : warhouseName);
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].NeedExpress) ? "" : code1);
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].SalesRemark) ? "" : borrowList[i].SalesRemark.ToString());
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].SingleTime.ToString()) ? "" : borrowList[i].SingleTime.ToString());
                string type = borrowList[i].NeedOrderType.ToString();
                rowtemp.CreateCell(7).SetCellValue(string.IsNullOrWhiteSpace(type) ? "" : "");
                rowtemp.CreateCell(8).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].SalesRemark) ? "" : borrowList[i].SalesRemark.ToString());
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].ReceivingName) ? "" : borrowList[i].ReceivingName.ToString());
                rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].ReceivingPhone) ? "" : borrowList[i].ReceivingPhone.ToString());
                rowtemp.CreateCell(11).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].ReceivingTelPhone) ? "" : borrowList[i].ReceivingTelPhone.ToString());
                rowtemp.CreateCell(12).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].NeedPostalCode) ? "" : borrowList[i].NeedPostalCode.ToString());
                rowtemp.CreateCell(13).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].AddressMessage) ? "" : borrowList[i].AddressMessage.ToString());
                rowtemp.CreateCell(14).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].VipCode) ? "" : borrowList[i].VipCode.ToString());
                rowtemp.CreateCell(15).SetCellValue(string.IsNullOrWhiteSpace(borrowList[i].PostUser) ? "" : borrowList[i].PostUser.ToString());
            }
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "换货数据.xls");
        }

        //获取附件
        public JsonResult GetExchangePic(int id)
        {

            List<T_ExchangePic> model = db.T_ExchangePic.Where(a => a.ExchangeId == id).ToList();
            string options = "";
            if (model.Count > 0)
            {
                options += "[";
                foreach (var item in model)
                {
                    options += "{\"ScName\":\"" + item.ScName + "\",\"Url\":\"" + item.Url + "\",\"Size\":\"" + item.Size + "\",\"Path\":\"" + item.Path + "\"},";
                }
                options = options.Substring(0, options.Length - 1);
                options += "]";
            }
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 附件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Upload()
        {
            string link = "";
            string filesName = "";
            if (Request.Files.Count > 0)
            {
                if (Request.Files.Count == 1)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        string title = string.Empty;
                        title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                        string path = "~/Upload/ExpressIndemnityImage/image/" + title;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        file.SaveAs(path);
                        link = "/Upload/ExpressIndemnityImage/image/" + title;
                        filesName = "~/Upload/ExpressIndemnityImage/image/" + title;
                        return Json(new { status = true, url = path, link = link, title = filesName });
                    }
                }
                else
                {
                    string[] urllist = new string[Request.Files.Count];

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file.ContentLength > 0)
                        {
                            string title = string.Empty;
                            title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                            string path = "~/Upload/ExpressIndemnityImage/image/" + title;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            file.SaveAs(path);
                            urllist[i] = path;
                            link = "/Upload/ExpressIndemnityImage/image/" + title;
                            filesName = "~/Upload/ExpressIndemnityImage/image/" + title;
                        }
                    }
                    return Json(new { status = true, url = urllist, link = link, title = filesName });
                }
            }
            else
            {
                return Json(new { status = false, url = "", msg = "没有文件" });
            }
            return Json(new { status = false, url = "", msg = "" });
        }

        //附件删除
        public void DeleteModelFile(string path, int id = 0)
        {
            string strPath = path;
            path = Server.MapPath(path);
            //获得文件对象
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }
            if (id != 0)
            {
                T_ExchangePic model = db.T_ExchangePic.FirstOrDefault(a => a.ExchangeId == id && a.Path == strPath);
                if (model != null)
                {
                    db.T_ExchangePic.Remove(model);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ContentResult GetOrdersList(Lib.GridPager pager, string code)
        {
            IQueryable<T_OrderList> list = db.T_OrderList.Where(s => s.platform_code.Equals(code.Trim())).AsQueryable();
            pager.totalRows = list.Count();

            List<T_OrderList> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
            public string ApproveName { get; set; }
        }
        /// <summary>
        /// 换货添加保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExchangeGoodsAddSave(T_ExchangeCenter model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                try
                {
                    string expressNumber = model.ReturnExpressCode;
                    string orderid = model.OrderCode;
                    List<T_ExchangeCenter> RmodelList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == expressNumber && a.OrderCode == orderid && a.IsDelete == 0).ToList();
                    if (RmodelList.Count > 0)
                    {
                        return Json(new { State = "Faile", Message = "同订单同快递单号已录入过，不允许重复" }, JsonRequestBehavior.AllowGet);

                    }
                    if (model.AddressMessage != null && model.AddressMessage.Contains("-"))
                    {
                        if (string.IsNullOrWhiteSpace(jsonStr))
                            return Json(new { State = "Faile", Message = "详情不能为空" });
                        List<T_ExchangeDetail> details = Com.Deserialize<T_ExchangeDetail>(jsonStr);
                        //T_OrderList order = db.T_OrderList.Find(model.OrderId);
                        T_ExchangeCenter exchange = db.T_ExchangeCenter.FirstOrDefault(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0);
                        int type = db.T_ExchangeReson.SingleOrDefault(s => s.Reson.Equals(model.ExchangeReson)).Type;
                        T_ExchangeCenterConfig config = db.T_ExchangeCenterConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type);
                        T_ExchangeGroup group = db.T_ExchangeGroup.SingleOrDefault(s => s.GroupName.Equals(config.ApproveType));
                        string departId = db.T_User.SingleOrDefault(s => s.Nickname.Equals(UserModel.Nickname)).DepartmentId;
                        int id = int.Parse(departId);
                        T_Department departMent = db.T_Department.Find(id);
                        string approneName = "";
                        if (departMent.supervisor == null && model.ExchangeReson.Equals("呼吸机专用"))
                            approneName = "见闻色";
                        else if (departMent.supervisor == null)
                            approneName = "成风";
                        else
                            approneName = db.T_User.Find(departMent.supervisor).Nickname;
                        if (exchange != null)
                            model.SystemRemark += "重复换货";
                        //model.SingleTime = string.IsNullOrWhiteSpace(order.dealtime) ? DateTime.Now : Convert.ToDateTime(order.dealtime);
                        model.NeedOrderType = "售后换货";
                        model.PostUser = UserModel.Nickname;
                        model.CreateDate = DateTime.Now;
                        model.ReturnExpressCode = model.ReturnExpressCode.Replace(" ", "");
                        model.Status = -1;
                        model.Step = 0;
                        #region 判断仓库是否收货
                        T_ReturnToStorage ReturnToStorage = db.T_ReturnToStorage.FirstOrDefault(a => a.Retreat_expressNumber.Equals(model.ReturnExpressCode) && a.IsDelete == 0);
                        if (ReturnToStorage != null)
                        {
                            if (ReturnToStorage.ModularName == "无")
                            {
                                ReturnToStorage.ModularName = "换货";
                            }
                            else if (!ReturnToStorage.ModularName.Contains("换货"))
                            {
                                ReturnToStorage.ModularName = ReturnToStorage.ModularName + "换货";
                            }
                            db.Entry<T_ReturnToStorage>(ReturnToStorage).State = System.Data.Entity.EntityState.Modified;
                        }




                        T_ReturnToStorage storge = db.T_ReturnToStorage.SingleOrDefault(s => s.Retreat_expressNumber.Equals(model.ReturnExpressCode) && s.isSorting == 1);
                        if (storge != null)
                        {
                            model.WarhouseStatus = 1;
                        }

                        else
                        {
                            model.WarhouseStatus = 0;
                        }

                        #endregion
                        model.IsDelete = 0;
                        //model.StoreCode = order.shop_code;
                        string remark = "";
                        foreach (var item in details)
                        {
                            remark += "补发编码:" + item.NeedProductCode + ",补发名称:" + item.NeedProductName + ";";
                        }
                        model.SalesRemark = remark;
                        db.T_ExchangeCenter.Add(model);
                        db.SaveChanges();
                        foreach (var item in details)
                        {
                            item.ExchangeCenterId = model.ID;
                            T_WDTGoods goods = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == item.SendProductCode && (s.spec_aux_unit_name == null || s.spec_aux_unit_name != "1"));

                            if (goods == null)
                            {
                                return Json(new { State = "Faile", Message = item.SendProductCode + "该产品不存在请核实" });
                            }
                            db.T_ExchangeDetail.Add(item);
                        }
                        db.SaveChanges();

                        T_ExchangeCenterApprove approve = new T_ExchangeCenterApprove
                        {
                            ApproveName = departMent.supervisor == null ? approneName : group.GroupName,
                            ApproveUser = group.GroupName.Equals("部门主管") ? approneName : "",
                            ApproveStatus = -1,
                            Pid = model.ID
                        };
                        db.T_ExchangeCenterApprove.Add(approve);
                        db.SaveChanges();
                        //if (exchange == null)
                        //{
                        //    order.ExchangeStatus = 1;
                        //    db.SaveChanges();
                        //}
                        //附件保存
                        if (!string.IsNullOrWhiteSpace(jsonStr1) && model.ExchangeReson.Equals("快递破损"))
                        {
                            List<T_ExchangePic> Enclosure = Com.Deserialize<T_ExchangePic>(jsonStr1);
                            foreach (var item in Enclosure)
                            {
                                item.Scdate = DateTime.Now;
                                item.ExchangeId = model.ID;
                                db.T_ExchangePic.Add(item);
                            }
                            db.SaveChanges();
                        }

                        // ModularByZP();


                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "换货").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = "  select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenterApprove where  Pid in ( select ID from T_ExchangeCenter where IsDelete=0 and  (status=-1 or status=0) ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "换货";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenter where Status='2' and IsDelete=0 GROUP BY PostUser  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "换货";
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
        /// 查询快递单号是否重复
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public JsonResult GetExpressNumberIsAgain(string number)
        {
            List<T_ExchangeCenter> modelList = db.T_ExchangeCenter.Where(s => s.ReturnExpressCode.Equals(number) && s.IsDelete == 0).ToList();
            if (modelList.Count() > 0)
                return Json(new { State = "Faile", Message = "快递单号:" + number + ",已存在" });
            else
                return Json(new { State = "Success" });
        }

        /// <summary>
        /// 访问订单详情表离开快递单查询方法
        /// </summary>
        /// <param name="expressNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult QuyerExchangeDetailsexpressBYcode(string expressNumber = "")
        {
            T_ReturnToStorage RetreatModel = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == expressNumber);
            if (RetreatModel != null)
            {
                int ID = RetreatModel.ID;
                List<T_ReturnToStorageDetails> Model = db.T_ReturnToStorageDetails.Where(a => a.Pid == ID).ToList();
                var json = new
                {

                    rows = (from r in Model
                            select new T_RetreatDetails
                            {
                                item_code = r.item_code,
                                item_name = r.item_name,
                                qty = r.qty,
                                Simplename = r.Simplename,
                            }).ToArray()
                };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <param name="nextApprove"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExchangeGoodsEditSave(T_ExchangeCenter model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ExchangeDetail> details = Com.Deserialize<T_ExchangeDetail>(jsonStr);
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "详情不能为空" });
                    T_ExchangeCenter editModel = db.T_ExchangeCenter.Find(model.ID);
                    int type = db.T_ExchangeReson.SingleOrDefault(s => s.Reson.Equals(model.ExchangeReson)).Type;
                    T_ExchangeCenterConfig config = db.T_ExchangeCenterConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type);
                    T_ExchangeGroup group = db.T_ExchangeGroup.SingleOrDefault(s => s.GroupName.Equals(config.ApproveType));
                    string departId = db.T_User.SingleOrDefault(s => s.Nickname.Equals(UserModel.Nickname)).DepartmentId;
                    int id = int.Parse(departId);
                    T_Department departMent = db.T_Department.Find(id);
                    string approneName = "";
                    if (departMent.supervisor == null && model.ExchangeReson.Equals("呼吸机专用"))
                        approneName = "见闻色";
                    else if (departMent.supervisor == null)
                        approneName = "成风";
                    else
                        approneName = db.T_User.Find(departMent.supervisor).Nickname;
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    string ExchangeReson = model.ExchangeReson;//记录原来的原因
                    T_ExchangeCenter PurMod = db.T_ExchangeCenter.Find(editID);
                    if (PurMod.Status != -1 && PurMod.Status != 2)
                    {
                        return Json(new { State = "Faile", Message = "该记录已经审核，不允许修改" }, JsonRequestBehavior.AllowGet);
                    }

                    editModel.OrderCode = model.OrderCode;
                    editModel.VipName = model.VipName;
                    editModel.StoreName = model.StoreName;
                    editModel.NeedWarhouse = model.NeedWarhouse;
                    editModel.NeedExpress = model.NeedExpress;
                    editModel.ReceivingName = model.ReceivingName;
                    editModel.ReturnWarhouse = model.ReturnWarhouse;
                    editModel.NeedPostalCode = model.NeedPostalCode;
                    editModel.ReceivingPhone = model.ReceivingPhone;
                    editModel.ReceivingTelPhone = model.ReceivingTelPhone;
                    editModel.VipCode = model.VipCode;
                    editModel.ReceivingAddress = model.ReceivingAddress;
                    editModel.ReturnExpressName = model.ReturnExpressName;
                    editModel.ReturnExpressCode = model.ReturnExpressCode;
                    editModel.BuyRemark = model.BuyRemark;
                    editModel.SalesRemark = model.SalesRemark;
                    editModel.AddressMessage = model.AddressMessage;
                    editModel.ExchangeReson = model.ExchangeReson;
                    #region 判断仓库是否收货

                    T_ReturnToStorage storge = db.T_ReturnToStorage.SingleOrDefault(s => s.Retreat_expressNumber.Equals(model.ReturnExpressCode) && s.isSorting == 1);
                    if (storge != null)
                        editModel.WarhouseStatus = 1;
                    #endregion

                    string remark = "";
                    foreach (var item in details)
                    {
                        remark += "补发编码:" + item.NeedProductCode + ",补发名称:" + item.NeedProductName + ";";
                    }
                    editModel.SalesRemark = remark;
                    db.SaveChanges();
                    //先删除详情再添加
                    List<T_ExchangeDetail> dl = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == model.ID).ToList();
                    if (dl.Count() > 0)
                    {
                        foreach (var item in dl)
                        {
                            T_ExchangeDetail detail = db.T_ExchangeDetail.Find(item.ID);
                            db.T_ExchangeDetail.Remove(detail);
                        }
                        db.SaveChanges();
                    }
                    foreach (var item in details)
                    {
                        item.ExchangeCenterId = model.ID;
                        T_WDTGoods goods = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == item.SendProductCode && s.spec_aux_unit_name == null);

                        if (goods == null)
                        {
                            return Json(new { State = "Faile", Message = item.SendProductCode + "该产品不存在请核实" });
                        }
                        db.T_ExchangeDetail.Add(item);
                    }
                    db.SaveChanges();
                    if (model.Step == 0)
                    {
                        T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.SingleOrDefault(a => a.Pid == model.ID && !a.ApproveTime.HasValue);
                        approve.ApproveName = departMent.supervisor == null ? approneName : group.GroupName;
                    }
                    else
                    {
                        T_ExchangeCenterApprove approve = new T_ExchangeCenterApprove();
                        approve.ApproveName = departMent.supervisor == null ? approneName : group.GroupName;
                        approve.ApproveStatus = -1;
                        approve.ApproveUser = group.GroupName.Equals("部门主管") ? approneName : "";
                        editModel.Step = 0;
                        editModel.Status = -1;
                        approve.Pid = model.ID;
                        db.T_ExchangeCenterApprove.Add(approve);
                    }
                    db.SaveChanges();
                    List<T_ExchangePic> enclosure = Com.Deserialize<T_ExchangePic>(jsonStr1);
                    //附件保存 先删除原有的附件
                    List<T_ExchangePic> delMod = db.T_ExchangePic.Where(a => a.ExchangeId == model.ID).ToList();
                    foreach (var item in delMod)
                    {
                        db.T_ExchangePic.Remove(item);
                    }
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(jsonStr1))
                    {
                        foreach (var item in enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ExchangeId = model.ID;
                            db.T_ExchangePic.Add(item);
                        }
                        db.SaveChanges();
                    }
                    // ModularByZP();



                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 仓库审核列表
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsWarhouseList(Lib.GridPager pager, string name)
        {
            List<T_ExchangeCenter> borrowList = new List<T_ExchangeCenter>();
            T_ExchangeGroup group = db.T_ExchangeGroup.SingleOrDefault(s => s.GroupUser.Contains(UserModel.Nickname));
            IQueryable<T_ExchangeCenterApprove> list = db.T_ExchangeCenterApprove.Where(s => s.ApproveName.Equals(group.GroupName) && !s.ApproveTime.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_ExchangeCenter model = db.T_ExchangeCenter.SingleOrDefault(s => s.ID == item && s.IsDelete == 0);
                if (model != null)
                    borrowList.Add(model);
            }
            if (!string.IsNullOrWhiteSpace(name))
                borrowList = borrowList.Where(s => s.PostUser.Equals(name)).ToList();
            pager.totalRows = borrowList.Count();
            List<T_ExchangeCenter> lists = new List<T_ExchangeCenter>();
            foreach (var item in borrowList)
            {
                T_ExchangeCenter model = new T_ExchangeCenter();
                model = item;
                model.ReturnWarhouse = Com.GetWarehouseName(model.ReturnWarhouse);
                lists.Add(model);
            }
            List<T_ExchangeCenter> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 换货列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsManagerList(Lib.GridPager pager, string code, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_ExchangeCenter> list = db.T_ExchangeCenter.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderCode.Equals(code) || s.VipName.Equals(code) || s.ReturnExpressCode.Equals(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.CreateDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CreateDate <= end);
            }
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ExchangeCenter> lists = new List<T_ExchangeCenter>();
            foreach (var item in list)
            {
                T_ExchangeCenter model = new T_ExchangeCenter();
                model = item;
                model.ReturnWarhouse = Com.GetWarehouseName(model.ReturnWarhouse);
                model.ReturnExpressName = Com.GetExpressName(model.ReturnExpressName);
                lists.Add(model);
            }


            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 我的换货
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsForMy(Lib.GridPager pager, string code, int status = -2)
        {
            IQueryable<T_ExchangeCenter> list = db.T_ExchangeCenter.Where(s => s.PostUser.Equals(UserModel.Nickname) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderCode.Equals(code) || s.VipName.Equals(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ExchangeCenter> lists = new List<T_ExchangeCenter>();
            foreach (var item in list)
            {
                T_ExchangeCenter model = new T_ExchangeCenter();
                model = item;
                model.ReturnWarhouse = Com.GetWarehouseName(model.ReturnWarhouse);
                model.ReturnExpressName = Com.GetExpressName(model.ReturnExpressName);
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 换货未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsNotcheckList(Lib.GridPager pager, string name, string Warehouse)
        {

            T_ExchangeGroup group = db.T_ExchangeGroup.SingleOrDefault(s => s.GroupUser.Contains(UserModel.Nickname));

            string sql = "select * from T_ExchangeCenter where ID in (select Pid from T_ExchangeCenterApprove where  ApproveUser='" + UserModel.Nickname + "' and ApproveTime is null group by Pid) and IsDelete=0"; ;
            if (group != null)
                sql = "select * from T_ExchangeCenter where ID in (select Pid from T_ExchangeCenterApprove where (ApproveName='" + group.GroupName + "' or ApproveUser='" + UserModel.Nickname + "') and ApproveTime is null group by Pid) and IsDelete=0";
            IQueryable<T_ExchangeCenter> borrowList = db.Database.SqlQuery<T_ExchangeCenter>(sql).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                borrowList = borrowList.Where(s => s.OrderCode.Contains(name) || s.ReturnExpressCode.Contains(name) || s.VipName.Equals(name));
            if (!string.IsNullOrWhiteSpace(Warehouse))
            {
                borrowList = borrowList.Where(s => s.ReturnWarhouse.Contains(Warehouse));
            }
            pager.totalRows = borrowList.Count();
            List<T_ExchangeCenter> lists = new List<T_ExchangeCenter>();
            foreach (var item in borrowList)
            {
                T_ExchangeCenter model = new T_ExchangeCenter();
                model = item;
                model.ReturnWarhouse = Com.GetWarehouseName(model.ReturnWarhouse);
                model.ReturnExpressName = Com.GetExpressName(model.ReturnExpressName);
                lists.Add(model);
            }
            List<T_ExchangeCenter> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 换货已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsCheckedList(Lib.GridPager pager, string name, string statedate, string EndDate)
        {

            string sql = "select * from T_ExchangeCenter where ID in (select Pid from T_ExchangeCenterApprove where ApproveName='" + UserModel.Nickname + "' and ApproveTime is not null group by Pid) and IsDelete=0";
            IQueryable<T_ExchangeCenter> borrowList = db.Database.SqlQuery<T_ExchangeCenter>(sql).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                borrowList = borrowList.Where(s => s.OrderCode.Contains(name) || s.ReturnExpressCode.Contains(name) || s.VipName.Equals(name));
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                borrowList = borrowList.Where(s => s.CreateDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                borrowList = borrowList.Where(s => s.CreateDate <= end);
            }
            pager.totalRows = borrowList.Count();
            List<T_ExchangeCenter> lists = new List<T_ExchangeCenter>();
            foreach (var item in borrowList)
            {
                T_ExchangeCenter model = new T_ExchangeCenter();
                model = item;
                model.ReturnWarhouse = Com.GetWarehouseName(model.ReturnWarhouse);
                model.ReturnExpressName = Com.GetExpressName(model.ReturnExpressName);
                lists.Add(model);
            }
            List<T_ExchangeCenter> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 换货商品详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="exchangeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsDetail(Lib.GridPager pager, int exchangeId)
        {
            IQueryable<T_ExchangeDetail> list = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == exchangeId).AsQueryable();
            pager.totalRows = list.Count();
            List<T_ExchangeDetail> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetOrderDetail(string oid)
        {
            IQueryable<T_OrderDetail> detailList = db.T_OrderDetail.Where(s => s.oid.Equals(oid));
            return Json(detailList, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取需发产品
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewProductList(string code, Lib.GridPager pager)
        {
            IQueryable<T_WDTGoods> modellist = db.T_WDTGoods.Where(s => ((s.goods_no != null && s.goods_no.Contains(code)) || (s.goods_name != null && s.goods_name.Contains(code)) || (s.spec_name != null && s.spec_name.Contains(code))) && (s.spec_aux_unit_name == null || s.spec_aux_unit_name != "1"));
            pager.totalRows = modellist.Count();
            List<T_WDTGoods> querData = modellist.OrderBy(s => s.goods_no).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExchangeGoodsDelete(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_ExchangeCenter model = db.T_ExchangeCenter.Find(id);
                    model.IsDelete = 1;
                    db.SaveChanges();

                    //   ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <param name="nextapprove"></param>
        /// <param name="BorrowerFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Check(int approveID, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    int TotalStep = db.T_ExchangeCenterConfig.ToList().Count;

                    List<T_ExchangeGroup> RetreatGroupList = db.Database.SqlQuery<T_ExchangeGroup>("select  * from T_ExchangeGroup where GroupUser like '%" + Nickname + "%'").ToList();
                    string GroupName = "";
                    for (int i = 0; i < RetreatGroupList.Count; i++)
                    {
                        if (i == 0)
                        {
                            GroupName += "'" + RetreatGroupList[i].GroupName + "'";
                        }
                        else
                        {
                            GroupName += "," + "'" + RetreatGroupList[i].GroupName + "'";
                        }
                    }

                    string sql = "select * from T_ExchangeCenterApprove where ID='" + approveID + "' and ApproveTime is null ";
                    if (GroupName != "" && GroupName != null)
                    {
                        sql += " and (ApproveName='" + Nickname + "'or ApproveUser='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                    }
                    else
                    {
                        sql += " and (ApproveName='" + Nickname + "'  or ApproveUser='" + Nickname + "') ";
                    }

                    List<T_ExchangeCenterApprove> AppRoveListModel = db.Database.SqlQuery<T_ExchangeCenterApprove>(sql).ToList();

                    if (AppRoveListModel.Count == 0)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" }, JsonRequestBehavior.AllowGet);
                    }

                    T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1);
                    string name = approve.ApproveName;
                    T_ExchangeCenter model = db.T_ExchangeCenter.Find(approve.Pid);
                    approve.ApproveUser = approve.ApproveName;
                    approve.ApproveName = UserModel.Nickname;
                    approve.ApproveStatus = status;
                    approve.ApproveTime = DateTime.Now;
                    approve.Memo = memo;
                    db.SaveChanges();
                    if (status == 2)//不同意
                    {
                        model.Status = status;
                        model.Step = model.Step + 1;
                        db.SaveChanges();
                    }
                    else//同意
                    {
                        int type = db.T_ExchangeReson.SingleOrDefault(s => s.Reson.Equals(model.ExchangeReson)).Type;
                        int LastStep = db.T_ExchangeCenterConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
                        if (LastStep > model.Step)//判断是否存在下一级
                        {
                            //获得下一级审核部门
                            string nextapproveType = db.T_ExchangeCenterConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
                            T_ExchangeCenterApprove newApprove = new T_ExchangeCenterApprove();
                            newApprove.ApproveStatus = -1;
                            newApprove.ApproveName = nextapproveType;
                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_ExchangeCenterApprove.Add(newApprove);
                            db.SaveChanges();
                            model.Status = 0;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }
                        if (name.Equals("仓库"))
                        {

                            model.Status = 1;
                            model.WarhouseStatus = 1;
                        }

                        if (name.Equals("售后主管") || name.Equals("呼吸机主管"))//售后主管审核后直接加入补发货
                        {
                            List<T_ExchangeCenterApprove> ApproveModel = db.T_ExchangeCenterApprove.Where(a => a.Pid == approve.Pid && (a.ApproveUser == "售后主管" || a.ApproveUser == "呼吸机主管") && a.ApproveStatus == 1).ToList();
                            if (ApproveModel.Count == 1)
                            {

                                List<T_ExchangeDetail> exchangedetail = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == model.ID).ToList();
                                #region 加入补发货


                                string remark = "";
                                List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                                if (reissue.Count > 1)
                                {
                                    var date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
                                    var modeldate = Convert.ToDateTime(reissue[0].CreatDate).ToString("yyyyMMdd");

                                    if (reissue.Count > 1 && int.Parse(date) - int.Parse(modeldate) <= 3)
                                        remark = model.SystemRemark + "3天内补发货重复";
                                }
                                //更改订单主表补发货状态
                                //else
                                //{
                                //    order.ReissueStatus = 1;
                                //    db.SaveChanges();
                                //}
                                List<T_ExchangeCenter> exch = db.T_ExchangeCenter.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                                string t = "";
                                if (exch.Count < 2)
                                {
                                    t = "88001";

                                }
                                else if (exch.Count >= 2 && exch.Count <= 100)
                                {
                                    int co = (int)exch.Count;
                                    t = "8800" + co;
                                }
                                else
                                {
                                    int co = (int)exch.Count;
                                    t = "880" + co;
                                }

                                string OrderScode = model.OrderScode;
                                T_Reissue reissues = new T_Reissue
                                {
                                    OrderSCode = OrderScode,
                                    OrderCode = model.OrderCode,
                                    NewOrderCode = t + model.OrderCode,
                                    //NewOrderCode = "8" + DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                                    VipName = model.VipName,
                                    StoreName = model.StoreName,
                                    WarhouseName = model.NeedWarhouse,
                                    ExpressName = model.NeedExpress,
                                    OrderType = "售后换货",
                                    SingleTime = model.SingleTime,
                                    ReceivingName = model.ReceivingName,
                                    PostalCode = model.NeedPostalCode,
                                    Phone = model.ReceivingPhone,
                                    TelPhone = model.ReceivingTelPhone,
                                    VipCode = model.VipCode,
                                    Address = model.ReceivingAddress,
                                    AddressMessage = model.AddressMessage,
                                    SalesRemark = model.SalesRemark,
                                    BuyRemark = model.BuyRemark,
                                    StoreCode = model.StoreCode,
                                    Step = 0,
                                    Status = -1,
                                    BusinessName = UserModel.Nickname,
                                    PostUser = model.PostUser,
                                    DraftName = UserModel.Nickname,
                                    CreatDate = DateTime.Now,
                                    IsDelete = 0,
                                    ReissueReson = model.ExchangeReson,
                                    SystemRemark = remark
                                };
                                model.NewOrderCode = t + model.OrderCode;
                                db.T_Reissue.Add(reissues);
                                db.SaveChanges();
                                IQueryable<T_ExchangeDetail> detail = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == model.ID);
                                foreach (var item in detail)
                                {
                                    T_ReissueDetail items = new T_ReissueDetail
                                    {
                                        ProductCode = item.NeedProductCode,
                                        ProductName = item.NeedProductName,
                                        Num = item.NeedProductNum,
                                        ReissueId = reissues.ID
                                    };
                                    db.T_ReissueDetail.Add(items);
                                }
                                T_ReissueApprove Reissueapprove = new T_ReissueApprove
                                {
                                    ApproveName = Com.GetReissueName(model.StoreCode, model.ExchangeReson),
                                    ApproveUser = Com.GetReissueName(model.StoreCode, model.ExchangeReson),
                                    ApproveStatus = -1,
                                    Pid = reissues.ID
                                };
                                db.T_ReissueApprove.Add(Reissueapprove);
                                db.SaveChanges();
                                #endregion

                                #region 新增订单数据

                                #endregion
                                #region 判断仓库是否收货

                                T_ReturnToStorage storge = db.T_ReturnToStorage.SingleOrDefault(s => s.Retreat_expressNumber.Equals(model.ReturnExpressCode) && s.isSorting == 1);
                                if (storge != null)
                                {

                                    //得到当前退款记录
                                    string exNumber = model.ReturnExpressCode;
                                    int modelID = model.ID;
                                    List<T_ExchangeCenter> RetreatList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == exNumber && a.ID == modelID).ToList();
                                    T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber);
                                    if (Tostorage != null)
                                    {
                                        int TostorageID = Tostorage.ID;
                                        List<T_ExchangeDetail> RetreatDetailsList = db.T_ExchangeDetail.Where(a => a.ExchangeCenterId == modelID).ToList();
                                        List<string> goods = RetreatDetailsList.Select(a => a.SendProductCode).ToList();
                                        List<T_ReturnToStoragelet> OverReceive = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && !goods.Contains(a.item_code)).ToList();
                                        for (int z = 0; z < OverReceive.Count; z++)
                                        {
                                            T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                            ReceivedAfterModel.Type = "换货";
                                            ReceivedAfterModel.OrderNumber = RetreatList[0].OrderCode;
                                            ReceivedAfterModel.ProductCode = OverReceive[z].item_code;
                                            ReceivedAfterModel.ProductName = OverReceive[z].item_name;
                                            ReceivedAfterModel.CollectExpressName = RetreatList[0].ReturnExpressName;
                                            ReceivedAfterModel.CollectExpressNumber = exNumber;
                                            ReceivedAfterModel.ShopName = RetreatList[0].StoreName;
                                            ReceivedAfterModel.CustomerCode = RetreatList[0].VipCode;
                                            ReceivedAfterModel.CustomerName = RetreatList[0].VipName;
                                            ReceivedAfterModel.ProductNumber = -OverReceive[z].qty;
                                            ReceivedAfterModel.CreatTime = DateTime.Now;
                                            ReceivedAfterModel.IsHandle = 0;
                                            ReceivedAfterModel.ShouldReceiveQty = 0;
                                            ReceivedAfterModel.ActualReceiveQty = OverReceive[z].qty;
                                            db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                            db.SaveChanges();
                                        }

                                        for (int x = 0; x < RetreatDetailsList.Count; x++)
                                        {
                                            string itemCode = RetreatDetailsList[x].SendProductCode;

                                            List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                                            if (ReturnToStorageletList.Count == 0)
                                            {
                                                List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == exNumber).ToList();
                                                if (ReceivedAfterList.Count == 0)
                                                {
                                                    T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                    ReceivedAfterModel.Type = "换货";
                                                    ReceivedAfterModel.OrderNumber = RetreatList[0].OrderCode;
                                                    ReceivedAfterModel.ShopName = RetreatList[0].StoreName; ;
                                                    ReceivedAfterModel.CustomerCode = RetreatList[0].VipCode;
                                                    ReceivedAfterModel.CustomerName = RetreatList[0].VipName;
                                                    ReceivedAfterModel.CollectExpressName = RetreatList[0].ReturnExpressName;
                                                    ReceivedAfterModel.ProductCode = RetreatDetailsList[x].SendProductCode;
                                                    ReceivedAfterModel.ProductName = RetreatDetailsList[x].SendProductName;
                                                    ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                    ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].SendProductNum;
                                                    ReceivedAfterModel.IsHandle = 0;
                                                    ReceivedAfterModel.ShouldReceiveQty = RetreatDetailsList[x].SendProductNum;
                                                    ReceivedAfterModel.ActualReceiveQty = 0;

                                                    ReceivedAfterModel.CreatTime = DateTime.Now;
                                                    db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    int? receiveQty = ReturnToStorageletList.Sum(a => a.qualified + a.Unqualified);
                                                    if (receiveQty != RetreatDetailsList[x].SendProductNum)
                                                    {

                                                        if (ReceivedAfterList.Count == 0)
                                                        {
                                                            T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                            ReceivedAfterModel.Type = "换货";
                                                            ReceivedAfterModel.OrderNumber = RetreatList[0].OrderCode;
                                                            ReceivedAfterModel.ProductCode = RetreatDetailsList[x].SendProductCode;
                                                            ReceivedAfterModel.ProductName = RetreatDetailsList[x].SendProductName;
                                                            ReceivedAfterModel.CollectExpressName = RetreatList[0].ReturnExpressName;
                                                            ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                            ReceivedAfterModel.ShopName = RetreatList[0].StoreName;
                                                            ReceivedAfterModel.CustomerCode = RetreatList[0].VipCode;
                                                            ReceivedAfterModel.CustomerName = RetreatList[0].VipName;
                                                            ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].SendProductNum - receiveQty;
                                                            ReceivedAfterModel.CreatTime = DateTime.Now;
                                                            ReceivedAfterModel.IsHandle = 0;
                                                            ReceivedAfterModel.ShouldReceiveQty = RetreatDetailsList[x].SendProductNum;
                                                            ReceivedAfterModel.ActualReceiveQty = receiveQty;
                                                            db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //List<T_ExchangeCenter> exchange = db.T_ExchangeCenter.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();

                                    model.WarhouseStatus = 1;
                                    model.Status = 1;
                                    T_ExchangeCenterApprove approve1 = db.T_ExchangeCenterApprove.SingleOrDefault(s => s.ApproveName.Equals("仓库") && !s.ApproveTime.HasValue && s.Pid == model.ID);
                                    approve1.ApproveName = "仓库";
                                    approve1.ApproveStatus = 1;
                                    approve1.ApproveTime = DateTime.Now;
                                    //判断是否第一次换货，如果是则修改订单状态为已收货
                                    //if (exchange.Count() == 1)
                                    //{
                                    //    order.ExchangeStatus = 2;
                                    //}
                                }


                                #endregion
                            }

                        }
                    }
                    List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "换货").ToList();
                    if (ModularNotaudited.Count > 0)
                    {
                        foreach (var item in ModularNotaudited)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
                        db.SaveChanges();
                    }
                    //ModularByZP();



                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 仓库收货详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="exchangeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetExchangeGoodsWarhouseDetail(Lib.GridPager pager, int exchangeId)
        {
            IQueryable<T_ExchangeDetail> list = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == exchangeId).AsQueryable();
            pager.totalRows = list.Count();
            List<T_ExchangeDetail> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 查询换货/补发货是否重复
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetExchangeOrder(string code)
        {
            T_ExchangeCenter model = db.T_ExchangeCenter.FirstOrDefault(s => s.OrderCode.Equals(code) && s.IsDelete == 0);
            T_Reissue reissue = db.T_Reissue.FirstOrDefault(s => s.OrderCode.Equals(code) && s.IsDelete == 0);
            var date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
            var modeldate = "";
            if (reissue != null)
                modeldate = Convert.ToDateTime(reissue.CreatDate).ToString("yyyyMMdd");
            if (model != null)
                return Json(new { State = "Success", Message = "该单号已经换货是否继续?" }, JsonRequestBehavior.AllowGet);
            else if (reissue != null && int.Parse(date) - int.Parse(modeldate) <= 3)
                return Json(new { State = "Success", Message = "三天内补发货重复否继续?" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }




        //换货导出
        public partial class getExcels
        {

            public string ReturnExpressCode { get; set; }
            public string SendProductName { get; set; }
            public string VipCode { get; set; }
            public string ExchangeReson { get; set; }
            public string NeedProductCode { get; set; }
            public string VipName { get; set; }
            public string PostUser { get; set; }
            public string NeedProductName { get; set; }
            public string OrderCode { get; set; }
            public DateTime CreateDate { get; set; }
            public int NeedProductNum { get; set; }
            public string NewOrderCode { get; set; }
            public string SalesRemark { get; set; }
            public string SingleTime { get; set; }
            public string SendProductCode { get; set; }
            public string ReturnExpressName { get; set; }
            public int SendProductNum { get; set; }
            public string StoreCode { get; set; }
            public string ReturnWarhouse { get; set; }


        }

        //导出excel
        public FileResult getExcelManager(string queryStr, string statedate, string EndDate)
        {
            List<getExcels> queryData = null;
            int temID = 0;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
            string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //  string sql = "select a.ID   from T_Retreat a  join T_RetreatAppRove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";

            string sql = "  select    (select shop_name  From T_WDTshop where  shop_no=a.StoreCode) as StoreCode,VipCode,VipName,OrderCode,NewOrderCode,SingleTime,(select name  From T_Express where  code=a.ReturnExpressName) as ReturnExpressName," +
                                "ReturnExpressCode,(select name From T_Warehouses where  code = a.ReturnWarhouse) as ReturnWarhouse,ExchangeReson,PostUser,CreateDate,SalesRemark,SendProductCode, " +
                                "SendProductName,SendProductNum,NeedProductCode,NeedProductName,NeedProductNum " +
                                "From  T_ExchangeCenter a left join T_ExchangeDetail b on a.id = b.ExchangeCenterId  where Status = 1   and CreateDate>='" + sdate + "' and CreateDate<='" + edate + "'    ";
            queryData = db.Database.SqlQuery<getExcels>(sql).ToList();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderCode.Contains(queryStr) || a.VipName.Contains(queryStr) || a.ReturnExpressCode.Contains(queryStr)).ToList(); ;
            }

            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {

                //string csvIds = string.Join(",", ids.ToArray());
                //var ret = db.Database.SqlQuery<T_Retreat>("select * from T_Retreat where ID in (" + csvIds + ") order by ID desc");

                //List<T_Retreat> result = ret.ToList();
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                row1.CreateCell(0).SetCellValue("店铺");
                row1.CreateCell(1).SetCellValue("会员账号");
                row1.CreateCell(2).SetCellValue("旺旺号");
                row1.CreateCell(3).SetCellValue("订单编号");
                row1.CreateCell(4).SetCellValue("新订单编号");
                row1.CreateCell(5).SetCellValue("下单时间");
                row1.CreateCell(6).SetCellValue("寄回快递");
                row1.CreateCell(7).SetCellValue("寄回快递单号");
                row1.CreateCell(8).SetCellValue("寄回仓库");
                row1.CreateCell(9).SetCellValue("换货原因");
                row1.CreateCell(10).SetCellValue("申请人");
                row1.CreateCell(11).SetCellValue("创建时间");
                row1.CreateCell(12).SetCellValue("买家备注");
                row1.CreateCell(13).SetCellValue("寄回产品代码");
                row1.CreateCell(14).SetCellValue("寄回产品名称");
                row1.CreateCell(15).SetCellValue("寄回产品数量");
                row1.CreateCell(16).SetCellValue("须发产品代码");
                row1.CreateCell(17).SetCellValue("须发产品名称");
                row1.CreateCell(18).SetCellValue("须发产品数量");
                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    rowtemp.CreateCell(0).SetCellValue(queryData[i].StoreCode.ToString());
                    rowtemp.CreateCell(1).SetCellValue(queryData[i].VipCode.ToString());
                    rowtemp.CreateCell(2).SetCellValue(queryData[i].VipName.ToString());
                    rowtemp.CreateCell(3).SetCellValue(queryData[i].OrderCode.ToString());
                    rowtemp.CreateCell(4).SetCellValue(queryData[i].NewOrderCode.ToString());
                    rowtemp.CreateCell(5).SetCellValue(queryData[i].SingleTime.ToString());
                    rowtemp.CreateCell(6).SetCellValue(queryData[i].ReturnExpressName.ToString());
                    rowtemp.CreateCell(7).SetCellValue(queryData[i].ReturnExpressCode.ToString());
                    rowtemp.CreateCell(8).SetCellValue(queryData[i].ReturnWarhouse.ToString());
                    rowtemp.CreateCell(9).SetCellValue(queryData[i].ExchangeReson.ToString());
                    rowtemp.CreateCell(10).SetCellValue(queryData[i].PostUser.ToString());
                    rowtemp.CreateCell(11).SetCellValue(queryData[i].CreateDate.ToString());
                    rowtemp.CreateCell(12).SetCellValue(queryData[i].SalesRemark.ToString());
                    rowtemp.CreateCell(13).SetCellValue(queryData[i].SendProductCode);
                    rowtemp.CreateCell(14).SetCellValue(queryData[i].SendProductName);
                    rowtemp.CreateCell(15).SetCellValue(queryData[i].SendProductNum);
                    rowtemp.CreateCell(16).SetCellValue(queryData[i].NeedProductCode);
                    rowtemp.CreateCell(17).SetCellValue(queryData[i].NeedProductName.ToString());
                    rowtemp.CreateCell(18).SetCellValue(queryData[i].NeedProductNum.ToString());
                }
                T_OperaterLog log = new T_OperaterLog()
                {
                    Module = "导出",
                    OperateContent = "导出换货Excel" + queryStr + statedate + EndDate,
                    Operater = Server.UrlDecode(Request.Cookies["Nickname"].Value),
                    OperateTime = DateTime.Now,
                    PID = 1
                };
                db.T_OperaterLog.Add(log);
                db.SaveChanges();

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "换货列表导出.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "换货列表导出.xls");
            }
        }
        #endregion

    }
}
