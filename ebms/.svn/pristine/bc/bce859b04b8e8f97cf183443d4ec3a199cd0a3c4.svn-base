using EBMS.Models;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class InterceptLogisticsController : Controller
    {
        //
        // GET: /InterceptLogistics/
        EBMSEntities db = new EBMSEntities();
        /// <summary>
        /// 获取用户角色
        /// </summary>
        public void GetWho()
        {
            ViewData["Who"] = "";
            var power = Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserPower"].Value));
            var nick = Server.UrlDecode(Request.Cookies["Nickname"].Value).ToString();
            var user = db.T_InterceptLogisticsRole.ToList();
            if (user.Where(a => a.UserDoc.Contains(nick)).ToList().Count > 0)
            {
                if (user.Where(a => a.Name.Equals("财务") && a.UserDoc.Contains(nick)).ToList().Count > 0)//power=11
                {
                    ViewData["Who"] = "财务";
                }
                if (user.Where(a => a.Name.Equals("快递组") && a.UserDoc.Contains(nick)).ToList().Count > 0)//power=25
                {
                    ViewData["Who"] = "快递组";
                }
                if (user.Where(a => a.Name.Equals("售后") && a.UserDoc.Contains(nick)).ToList().Count > 0)
                {
                    ViewData["Who"] = "售后";
                }
                if (user.Where(a => a.Name.Equals("仓库") && a.UserDoc.Contains(nick)).ToList().Count > 0)
                {
                    ViewData["Who"] = "仓库";
                }
            }
            else
            {
                if (power == 12)
                {
                    ViewData["Who"] = "售后";
                }
                if (power == 5)
                {
                    ViewData["Who"] = "仓库";
                }
            }
        }
        public ActionResult InterceptLogisticsList()//物流订单拦截管理列表
        {
            return View();
        }
        public ActionResult InterceptLogisticsApproveList()//物流订单拦截审批列表
        {
            GetWho();
            return View();
        }
        public ActionResult InterceptLogisticsAdd()//创建物流订单拦截视图
        {
            return View();
        }
        /// <summary>
        /// 获取旺店通
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public JsonResult QuyerOrderBYcode(string code)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (code == "" || code == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            App_Code.GY gy = new App_Code.GY();
            string repeat = "";
            List<T_InterceptLogistics> modelList = db.T_InterceptLogistics.Where(a => a.OrderNumber.Equals(code.Trim()) && a.Del == 0).ToList();
            if (modelList.Count > 0)
            {
                repeat += "物流拦截记录已存在此订单号";
            }
            #region
            ////查询旺店通
            //List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0").ToList();
            //if (modelList.Count > 0)
            //{

            //    repeat += "退货退款记录重复，";
            //}
            ////查是否有返现记录

            //List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 2).ToList();
            //if (cash.Count > 0)
            //{
            //    repeat += "有返现记录重复，";
            //}
            //List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            //if (Reissue.Count > 0)
            //{
            //    repeat += "有补发记录重复，";
            //}
            //List<T_ExchangeCenter> ExchangeCenter = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            //if (ExchangeCenter.Count > 0)
            //{
            //    repeat += "有换货记录重复，";
            //}
            //List<T_Intercept> Intercept = db.T_Intercept.Where(a => a.OrderNumber.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            //if (Intercept.Count > 0)
            //{
            //    repeat += "拦截模块有记录，";
            //}
            #endregion

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
            if (iscode != "0")
            {
                JsonData jsontrades = jsonData["trades"];

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
                    string pay_time = trades["pay_time"].ToString();
                    //旺旺号
                    string customer_name = trades["buyer_nick"].ToString();
                    //订单状态
                    string trade_status = trades["trade_status"].ToString();

                    //收件人姓名
                    string receiver_name = trades["receiver_name"].ToString();
                    //省
                    string receiver_province = trades["receiver_province"].ToString();
                    //市
                    string receiver_city = trades["receiver_city"].ToString();
                    //区
                    string receiver_district = trades["receiver_district"].ToString();
                    //详细地址
                    string receiver_address = trades["receiver_address"].ToString();
                    //电话号码
                    string receiver_mobile = trades["receiver_mobile"].ToString();
                    //邮政编码
                    string receiver_zip = trades["receiver_zip"].ToString();
                    //省市县
                    string receiver_area = trades["receiver_area"].ToString();
                    //快递公司编号
                    string logistics_code = trades["logistics_code"].ToString();
                    //快递公司名称
                    string logistics_name = trades["logistics_name"].ToString();
                    //快递单号
                    string logistics_no = trades["logistics_no"].ToString();
                    //买家留言
                    string buyer_message = trades["buyer_message"].ToString();
                    //客服备注
                    string cs_remark = trades["cs_remark"].ToString();
                    //实付金额
                    // string paid = trades["paid"].ToString();
                    //商品详情
                    List<T_RetreatDetails> DetailsList = new List<T_RetreatDetails>();
                    double paid = 0.00;
                    for (int c = 0; c < jsontrades.Count; c++)
                    {
                        paid += double.Parse(jsontrades[c]["paid"].ToString());
                        //JsonData goods_list = jsontrades[c]["goods_list"];
                        //for (int i = 0; i < goods_list.Count; i++)
                        //{
                        //    T_RetreatDetails DetailsModel = new T_RetreatDetails();
                        //    string ss = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                        //    DetailsModel.item_code = ss;
                        //    DetailsModel.item_name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                        //    //   double ssds=double.Parse(goods_list[i]["paid"].ToString()) / double.Parse(goods_list[i]["actual_num"].ToString());

                        //    decimal dec = Convert.ToDecimal(Math.Round(double.Parse(goods_list[i]["share_amount"].ToString()), 2));
                        //    DetailsModel.amount = (double)dec;//分摊邮费 


                        //    int qyt = Convert.ToInt32(Convert.ToDecimal(goods_list[i]["actual_num"].ToString()));
                        //    if (qyt != 0)
                        //    {
                        //        DetailsModel.qty = qyt;
                        //        DetailsModel.price = (double)dec / DetailsModel.qty;
                        //    }
                        //    else
                        //    {
                        //        DetailsModel.qty = 0;
                        //        DetailsModel.price = (double)dec;
                        //    }
                        //    if (qyt > 0)
                        //    {
                        //        DetailsList.Add(DetailsModel);
                        //    }

                        //}
                    }
                    T_InterceptLogistics model = new T_InterceptLogistics();
                    model.OrderNumber = code;
                    model.ExpressName = logistics_name;
                    model.ExpressNumber = logistics_no;
                    model.OrderMoney = Convert.ToDecimal(paid);


                    //var json = new
                    //{

                    //    rows = (from r in DetailsList
                    //            select new T_RetreatDetails
                    //            {
                    //                item_code = r.item_code,
                    //                item_name = r.item_name,
                    //                price = r.price,
                    //                amount = r.amount,
                    //                qty = r.qty,
                    //                Simplename = r.Simplename,
                    //            }).ToArray()
                    //};
                    return Json(new { ModelList = model, Repeat = repeat }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json("-1", JsonRequestBehavior.AllowGet);

        }
        #region 旺店通接口
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
        #endregion
        public JsonResult InterceptLogisticsSave(T_InterceptLogistics model)//创建物流订单拦截
        {
            try
            {
                var Order = db.T_InterceptLogistics.SingleOrDefault(a => a.OrderNumber == model.OrderNumber && a.Del == 0);
                if (Order == null)
                {
                    model.FinanceApproveStatus = -1;
                    model.ExpressApproveStatus = -1;
                    model.WarehouseApproveStatus = -1;
                    model.Creator = Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserId"].Value));
                    model.CreateTime = DateTime.Now;
                    db.T_InterceptLogistics.Add(model);
                    db.SaveChanges();
                    return Json(new { State = "Success", Msg = "创建成功" });
                }
                else
                {
                    return Json(new { State = "Fail", Msg = "创建失败,该订单拦截审批已存在！" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { State = "Fail", Msg = "创建失败," + ex.Message });
            }
        }
        public JsonResult InterceptLogisticsDel(int ID)//删除物流拦截申请
        {
            try
            {
                var Order = db.T_InterceptLogistics.SingleOrDefault(a => a.ID == ID && a.Del == 0);
                if (Order != null)
                {
                    if (Order.FinanceApproveStatus == -1)
                    {
                        if (Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserId"].Value)) == Order.Creator)
                        {
                            Order.Del = 1;
                            db.SaveChanges();
                            return Json(new { State = "Success", Msg = "删除成功" });
                        }
                        else
                        {
                            return Json(new { State = "Fail", Msg = "无法删除他人申请！" });
                        }
                    }
                    else
                    {
                        return Json(new { State = "Fail", Msg = "该物流拦截财务已处理，无法删除！" });
                    }
                }
                else
                {
                    return Json(new { State = "Fail", Msg = "删除失败,该物流拦截已删除！" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { State = "Fail", Msg = "删除失败," + ex.Message });
            }
        }
        public JsonResult InterceptLogistics(Lib.GridPager pager, string queryStr,string who, string Status)//拦截物流列表
        {
            try
            {
                //var InterceptLogistics = db.T_InterceptLogistics.Where(a => a.Del == 0).ToList();
                //左连
                var InterceptLogistics = (from mod in db.T_InterceptLogistics
                                          join user in db.T_User on mod.Creator equals user.ID
                                          join user2 in db.T_User on mod.FinanceApproveUser equals user2.ID into JoinedEmpFinance
                                          from user2 in JoinedEmpFinance.DefaultIfEmpty()
                                          join user3 in db.T_User on mod.ExpressApproveUser equals user3.ID into JoinedEmpExpress
                                          from user3 in JoinedEmpExpress.DefaultIfEmpty()
                                          join user4 in db.T_User on mod.WarehouseApproveUser equals user4.ID into JoinedEmpWarehouse
                                          from user4 in JoinedEmpWarehouse.DefaultIfEmpty()
                                          select new
                                          {
                                              ID = mod.ID,
                                              OrderNumber = mod.OrderNumber,
                                              ExpressName = mod.ExpressName,
                                              ExpressNumber = mod.ExpressNumber,
                                              OrderMoney = mod.OrderMoney,
                                              FinanceApproveStatus = mod.FinanceApproveStatus,
                                              FinanceApproveUser = user2.Nickname,
                                              FinanceApproveTime = mod.FinanceApproveTime,
                                              FinanceReason = mod.FinanceReason,
                                              ExpressApproveStatus = mod.ExpressApproveStatus,
                                              ExpressApproveUser = user3.Nickname,
                                              ExpressApproveTime = mod.ExpressApproveTime,
                                              ExpressReason = mod.ExpressReason,
                                              WarehouseApproveStatus = mod.WarehouseApproveStatus,
                                              WarehouseApproveUser = user4.Nickname,
                                              WarehouseApproveTime = mod.WarehouseApproveTime,
                                              WarehouseReason = mod.WarehouseReason,
                                              Creator = user.Nickname,
                                              CreateTime = mod.CreateTime,
                                              Del = mod.Del
                                          }).Where(a => a.Del == 0).OrderByDescending(a=>a.CreateTime).ToList();
                if (!string.IsNullOrEmpty(who))
                {
                    if (who == "快递组")
                    {
                        InterceptLogistics = InterceptLogistics.Where(a => a.FinanceApproveStatus == 1).ToList();
                    }
                    else if (who == "仓库")
                    {
                        InterceptLogistics = InterceptLogistics.Where(a => a.ExpressApproveStatus == 1).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(Status))
                {
                    int status = Convert.ToInt32(Status);
                    if (status != -2)
                    {
                        if (who == "财务")
                        {
                            InterceptLogistics = InterceptLogistics.Where(a => a.FinanceApproveStatus == status).ToList();
                        }
                        else if (who == "快递组")
                        {
                            InterceptLogistics = InterceptLogistics.Where(a => a.ExpressApproveStatus == status).ToList();
                        }
                        else if (who == "仓库")
                        {
                            InterceptLogistics = InterceptLogistics.Where(a => a.WarehouseApproveStatus == status).ToList();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(queryStr))
                {
                    InterceptLogistics = InterceptLogistics.Where(a => a.OrderNumber.Contains(queryStr) || a.ExpressNumber.Contains(queryStr)||a.Creator.Contains(queryStr)||a.FinanceApproveUser.Contains(queryStr)||a.ExpressApproveUser.Contains(queryStr)||a.WarehouseApproveUser.Contains(queryStr)).ToList();
                }
                pager.totalRows = InterceptLogistics.Count;
                InterceptLogistics = InterceptLogistics.Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                return Json(new { total = pager.totalRows, rows = InterceptLogistics });
            }
            catch (Exception ex)
            {
                return Json(new { total = 0, rows = new int[] { } });
            }
        }
        public ActionResult InterceptLogisticsApprove(int ID)//审批视图
        {
            var model = db.T_InterceptLogistics.Find(ID);
            GetWho();
            GetApproveHistory(ID);
            return View(model);
        }
        /// <summary>
        /// 获取拦截物流审核详情历史记录
        /// </summary>
        /// <param name="ID"></param>
        private void GetApproveHistory(int ID)
        {
            var model = (from mod in db.T_InterceptLogistics
                         join user in db.T_User on mod.Creator equals user.ID
                         join user2 in db.T_User on mod.FinanceApproveUser equals user2.ID into JoinedEmpFinance
                         from user2 in JoinedEmpFinance.DefaultIfEmpty()
                         join user3 in db.T_User on mod.ExpressApproveUser equals user3.ID into JoinedEmpExpress
                         from user3 in JoinedEmpExpress.DefaultIfEmpty()
                         join user4 in db.T_User on mod.WarehouseApproveUser equals user4.ID into JoinedEmpWarehouse
                         from user4 in JoinedEmpWarehouse.DefaultIfEmpty()
                         select new
                         {
                             ID = mod.ID,
                             OrderNumber = mod.OrderNumber,
                             ExpressName = mod.ExpressName,
                             ExpressNumber = mod.ExpressNumber,
                             OrderMoney = mod.OrderMoney,
                             FinanceApproveStatus = mod.FinanceApproveStatus,
                             FinanceApproveUser = user2.Nickname,
                             FinanceApproveTime = mod.FinanceApproveTime,
                             FinanceReason = mod.FinanceReason,
                             ExpressApproveStatus = mod.ExpressApproveStatus,
                             ExpressApproveUser = user3.Nickname,
                             ExpressApproveTime = mod.ExpressApproveTime,
                             ExpressReason = mod.ExpressReason,
                             WarehouseApproveStatus = mod.WarehouseApproveStatus,
                             WarehouseApproveUser = user4.Nickname,
                             WarehouseApproveTime = mod.WarehouseApproveTime,
                             WarehouseReason = mod.WarehouseReason,
                             Creator = user.Nickname,
                             CreateTime = mod.CreateTime,
                             Del = mod.Del
                         }).SingleOrDefault(a => a.Del == 0 && a.ID == ID);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td></td><td>审核结果</td><td>审核人</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            //财务
            string s = "";
            if (model.FinanceApproveStatus == -1) s = "<font color=#d02e2e>未审核</font>";
            if (model.FinanceApproveStatus == 0) s = "<font color=#2299ee>审核中</font>";
            if (model.FinanceApproveStatus == 1) s = "<font color=#1fc73a>已同意</font>";
            if (model.FinanceApproveStatus == 2) s = "<font color=#d02e2e>不同意</font>";
            tr += string.Format("<tr><td>财务</td><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", s, model.FinanceApproveUser, model.FinanceApproveTime, model.FinanceReason);
            //快递组
            s = "";
            if (model.ExpressApproveStatus == -1) s = "<font color=#d02e2e>未审核</font>";
            if (model.ExpressApproveStatus == 0) s = "<font color=#2299ee>审核中</font>";
            if (model.ExpressApproveStatus == 1) s = "<font color=#1fc73a>已同意</font>";
            if (model.ExpressApproveStatus == 2) s = "<font color=#d02e2e>不同意</font>";
            tr += string.Format("<tr><td>快递组</td><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", s, model.ExpressApproveUser, model.ExpressApproveTime, model.ExpressReason);
            //仓库
            s = "";
            if (model.WarehouseApproveStatus == -1) s = "<font color=#d02e2e>未复核</font>";
            if (model.WarehouseApproveStatus == 0) s = "<font color=#2299ee>复核中</font>";
            if (model.WarehouseApproveStatus == 1) s = "<font color=#1fc73a>已收货</font>";
            if (model.WarehouseApproveStatus == 2) s = "<font color=#d02e2e>未收货</font>";
            tr += string.Format("<tr><td>仓库</td><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", s, model.WarehouseApproveUser, model.WarehouseApproveTime, model.WarehouseReason);

            ViewData["history"] = table + tr + "</tbody></table>";
        }
        public ActionResult ApproveDetail(int ID)//审批详情视图
        {
            GetApproveHistory(ID);
            var model = db.T_InterceptLogistics.Find(ID);
            return View(model);
        }
        public JsonResult Approve(T_InterceptLogistics model)//审批
        {
            try
            {
                var mod = db.T_InterceptLogistics.Find(model.ID);
                if (model.FinanceApproveStatus == 1 || model.FinanceApproveStatus == 2)
                {
                    mod.FinanceApproveStatus = model.FinanceApproveStatus;
                    mod.FinanceApproveTime = DateTime.Now;
                    mod.FinanceReason = model.FinanceReason;
                    mod.FinanceApproveUser= Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserId"].Value));
                }
                if (model.ExpressApproveStatus == 1 || model.ExpressApproveStatus == 2)
                {
                    mod.ExpressApproveStatus = model.ExpressApproveStatus;
                    mod.ExpressApproveTime = DateTime.Now;
                    mod.ExpressReason = model.FinanceReason;
                    mod.ExpressApproveUser = Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserId"].Value));
                }
                if (model.WarehouseApproveStatus == 1 || model.WarehouseApproveStatus == 2)
                {
                    mod.WarehouseApproveStatus = model.WarehouseApproveStatus;
                    mod.WarehouseApproveTime = DateTime.Now;
                    mod.WarehouseReason = model.WarehouseReason;
                    mod.WarehouseApproveUser = Convert.ToInt32(Server.UrlDecode(Request.Cookies["UserId"].Value));
                }
                db.SaveChanges();
                return Json(new { State = "Success", Msg = "审批成功" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Fail", Msg = "审批失败," + ex.Message });
            }
        }
        public FileResult ExportExcel(string Status,string who,string flag)
        {
            var queryData = (from mod in db.T_InterceptLogistics
                                      join user in db.T_User on mod.Creator equals user.ID
                                      join user2 in db.T_User on mod.FinanceApproveUser equals user2.ID into JoinedEmpFinance
                                      from user2 in JoinedEmpFinance.DefaultIfEmpty()
                                      join user3 in db.T_User on mod.ExpressApproveUser equals user3.ID into JoinedEmpExpress
                                      from user3 in JoinedEmpExpress.DefaultIfEmpty()
                                      join user4 in db.T_User on mod.WarehouseApproveUser equals user4.ID into JoinedEmpWarehouse
                                      from user4 in JoinedEmpWarehouse.DefaultIfEmpty()
                                      select new
                                      {
                                          ID = mod.ID,
                                          OrderNumber = mod.OrderNumber,
                                          ExpressName = mod.ExpressName,
                                          ExpressNumber = mod.ExpressNumber,
                                          OrderMoney = mod.OrderMoney,
                                          FinanceApproveStatus = mod.FinanceApproveStatus,
                                          FinanceApproveUser = user2.Nickname,
                                          FinanceApproveTime = mod.FinanceApproveTime,
                                          FinanceReason = mod.FinanceReason,
                                          ExpressApproveStatus = mod.ExpressApproveStatus,
                                          ExpressApproveUser = user3.Nickname,
                                          ExpressApproveTime = mod.ExpressApproveTime,
                                          ExpressReason = mod.ExpressReason,
                                          WarehouseApproveStatus = mod.WarehouseApproveStatus,
                                          WarehouseApproveUser = user4.Nickname,
                                          WarehouseApproveTime = mod.WarehouseApproveTime,
                                          WarehouseReason = mod.WarehouseReason,
                                          Creator = user.Nickname,
                                          CreateTime = mod.CreateTime,
                                          Del = mod.Del
                                      }).Where(a => a.Del == 0).OrderByDescending(a => a.CreateTime).ToList();
            if (flag == "1") //flag = 1从审批页面导出
            {
                if (!string.IsNullOrEmpty(who))
                {
                    if (who == "快递组")
                    {
                        queryData = queryData.Where(a => a.FinanceApproveStatus == 1).ToList();
                    }
                    else if (who == "仓库")
                    {
                        queryData = queryData.Where(a => a.ExpressApproveStatus == 1).ToList();
                    }
                }
            }
            if (!string.IsNullOrEmpty(who) && !string.IsNullOrEmpty(Status))
            {
                int status = Convert.ToInt32(Status);
                if (status != -2)
                {
                    if (who == "财务")
                    {
                        queryData = queryData.Where(a => a.FinanceApproveStatus == status).ToList();
                    }
                    else if (who == "快递组")
                    {
                        queryData = queryData.Where(a => a.ExpressApproveStatus == status).ToList();
                    }
                    else if (who == "仓库")
                    {
                        queryData = queryData.Where(a => a.WarehouseApproveStatus == status).ToList();
                    }
                }
            }
            if (queryData.Count > 0)
            {
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                for (int inirows = 0; queryData.Count - inirows > 0; inirows += 65000)
                {
                    string a = "";
                    if (inirows / 65000 >= 1)
                    {
                        a = "(" + (inirows / 65000 + 1) + ")";
                    }
                    NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet" + a);
                    //给sheet1添加第一行的头部标题
                    NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                    row1.CreateCell(0).SetCellValue("申请时间");
                    row1.CreateCell(1).SetCellValue("订单号");
                    row1.CreateCell(2).SetCellValue("快递公司");
                    row1.CreateCell(3).SetCellValue("快递单号");
                    row1.CreateCell(4).SetCellValue("订单金额");
                    row1.CreateCell(5).SetCellValue("申请人");
                    row1.CreateCell(6).SetCellValue("财务审批状态");
                    row1.CreateCell(7).SetCellValue("财务审批人");
                    row1.CreateCell(8).SetCellValue("财务备注");
                    row1.CreateCell(9).SetCellValue("快递组审批状态");
                    row1.CreateCell(10).SetCellValue("快递组审批人");
                    row1.CreateCell(11).SetCellValue("快递组备注");
                    sheet1.SetColumnWidth(0, 20 * 256);
                    sheet1.SetColumnWidth(1, 20 * 330);
                    sheet1.SetColumnWidth(2, 20 * 256);
                    sheet1.SetColumnWidth(3, 20 * 330);
                    sheet1.SetColumnWidth(4, 20 * 156);
                    sheet1.SetColumnWidth(5, 20 * 156);
                    sheet1.SetColumnWidth(6, 20 * 200);
                    sheet1.SetColumnWidth(7, 20 * 156);
                    sheet1.SetColumnWidth(8, 20 * 256);
                    sheet1.SetColumnWidth(9, 20 * 200);
                    sheet1.SetColumnWidth(10, 20 * 156);
                    sheet1.SetColumnWidth(11, 20 * 200);
                    int pagerows = 0;
                    pagerows = queryData.Count - inirows;
                    if (pagerows > 65000)
                    {
                        pagerows = 65000;
                    }
                    for (int i = inirows; i < inirows + pagerows; i++)
                    {
                        NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i - inirows + 1);
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].OrderNumber);
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].ExpressName);
                        rowtemp.CreateCell(3).SetCellValue(queryData[i].ExpressNumber);
                        rowtemp.CreateCell(4).SetCellValue(queryData[i].OrderMoney.ToString());
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].Creator);
                        rowtemp.CreateCell(6).SetCellValue(TransferStatus(queryData[i].FinanceApproveStatus.ToString()));
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].FinanceApproveUser);
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].FinanceReason);
                        rowtemp.CreateCell(9).SetCellValue(TransferStatus(queryData[i].ExpressApproveStatus.ToString()));
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].ExpressApproveUser);
                        rowtemp.CreateCell(11).SetCellValue(queryData[i].ExpressReason);
                    }
                }
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "特殊物流拦截.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "特殊物流拦截.xls");
            }
        }

        public string TransferStatus(string Status)
        {
            string Statusname = "";
            //-1未审核  0审核中  1已同意  2不同意
            if (Status == "-1")
            {
                Statusname = "未审核";
            }
            else if (Status == "0")
            {
                Statusname = "审核中";
            }
            else if (Status == "1")
            {
                Statusname = "已同意";
            }
            else if (Status == "2")
            {
                Statusname = "不同意";
            }
            return Statusname;
        }
    }
}
