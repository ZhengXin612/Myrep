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
    public class ReissueController : BaseController
    {

        #region 属性/公共字段/方法

        EBMSEntities db = new EBMSEntities();

        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }

        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
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
        public JsonResult GetReissueByGy(string code)
        {
            string repeat = "";
            //去重
       
            GY gy = new GY();
            
            
                  List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0").ToList();
                  if (modelList.Count > 0)
                  {

                      repeat += "_存在退货退款记录重复，";
                  }
                  //查是否有返现记录

                  List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0).ToList();
                  if (cash.Count > 0)
                  {
                      repeat += "_存在返现记录重复，";
                  }

                  List<T_Reissue> reissueList = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
                  if (reissueList.Count > 0)
                  {
                      repeat += "_存在补发记录重复，";
                  }
                  List<T_Intercept> InterceptList = db.T_Intercept.Where(a => a.OrderNumber.Equals(code.Trim()) && a.IsDelete == 0).ToList();
                  if (InterceptList.Count > 0)
                  {
                      repeat += "_存在拦截记录重复，";
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
                  if (iscode != "0")
                  {
                      JsonData jsontrades = jsonData["trades"];

                      if (jsontrades.Count != 0)
                      {
                    JsonData trades = jsontrades[0];


                    //订单状态
                    string trade_status = trades["trade_status"].ToString();
                    if(trade_status!="95"&& trade_status != "100"&&trade_status!= "110" && trade_status != "105")
                    {
                        return Json(new { State = "Faile", Message ="此订单不是能补发的状态,请核实" }, JsonRequestBehavior.AllowGet);
                    }
                    

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
					double paid =Convert.ToDouble( trades["paid"].ToString());
					
					//商品详情
					//    JsonData goods_list = trades["goods_list"];
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
                    string shop_Code = "";

                    if (shop_name != null)
                    {
                        T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
                        shop_Code = commonarea.shop_no;
                        // shop_Code = "tm004";
                    }
                    
                    T_Reissue reissue = new T_Reissue
                    {
                        OrderCode = code,
                        VipName = customer_name,
                        StoreName = shop_name,
                        ReceivingName = receiver_name,
                        PostalCode = receiver_zip,
                        Phone = receiver_mobile,
                        TelPhone = receiver_mobile,
                        VipCode = receiver_name,
                        Address = receiver_address,
                        AddressMessage = ssq,
                        SystemRemark = repeat,
                        ExpressName = "原快递:" + logistics_name + logistics_no + "，",
                        SingleTime = string.IsNullOrWhiteSpace(trade_time) ? DateTime.Now.ToString() : trade_time,
                        BusinessName = UserModel.Nickname,
                        StoreCode = shop_Code,
						Cost=(decimal?) paid

					};
                    List<T_ReissueDetail> DetailsList = new List<T_ReissueDetail>();
                    for (int c = 0; c < jsontrades.Count; c++)
                    {
                       // paid += double.Parse(jsontrades[c]["paid"].ToString());
                        JsonData goods_list = jsontrades[c]["goods_list"];



						for (int i = 0; i < goods_list.Count; i++)
						{
							decimal dec = Convert.ToDecimal(Math.Round(double.Parse(goods_list[i]["share_amount"].ToString()), 2));
							decimal qyt = decimal.Parse(goods_list[i]["actual_num"].ToString());
							decimal price = dec;
							if (qyt >0)
							{
								 price = dec / qyt;
							}
							
							T_ReissueDetail DetailsModel = new T_ReissueDetail();
							DetailsModel.ProductCode = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
							DetailsModel.ProductName = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
							DetailsModel.Price = price;
							DetailsList.Add(DetailsModel);
						}
					}

                    var json = new
                    {

                        rows = (from r in DetailsList
                                select new T_ReissueDetail
                                {
                                    ProductCode = r.ProductCode,
                                    ProductName = r.ProductName,
                                    Price = r.Price
                                }).ToArray()
                    };
                    return Json(new { State = "Success", ModelList = reissue, Json = json }, JsonRequestBehavior.AllowGet);
                    }
                   
                 
          
              }
              return Json(new { State = "" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交管易
        /// </summary>
        /// <returns></returns>
        public string PostGy(T_Reissue model)
        {
            string shangp = "";
            List<T_ReissueDetail> commodel = db.T_ReissueDetail.Where(a => a.ReissueId == model.ID).ToList();
            for (int i = 0; i < commodel.Count; i++)
            {
                if (i == commodel.Count - 1)
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].ProductCode + "\"}";
                }
                else
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].ProductCode + "\"},";
                }
            }
            //.ToString("yyyy-MM-dd hh:mm:ss")

            DateTime singletime =DateTime.Parse(model.SingleTime);

            string datetime = singletime.ToString("yyyy-MM-dd hh:mm:ss");
            string sellerremarks = "";
            if (!string.IsNullOrWhiteSpace(model.SalesRemark))
            {
                sellerremarks = Regex.Replace(model.SalesRemark.ToUpper().Replace((char)32, ' ').Replace((char)13, ' ').Replace((char)10, ' '), "[ \\[ \\] \\^ \t \\-×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；;\"‘’“”-]", "☆").Replace("☆", "").Replace(" ", "");
            }
            else
            {
                sellerremarks = "";
            }
            string BuyersRemarks = "";
            if (!string.IsNullOrWhiteSpace(model.BuyRemark))
            {
                BuyersRemarks = Regex.Replace(model.BuyRemark.ToUpper().Replace((char)32, ' ').Replace((char)13, ' ').Replace((char)10, ' '), "[ \\[ \\] \\^ \t \\-×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；;\"‘’“”-]", "☆").Replace("☆", "").Replace(" ", "");
            }
            else
            {
                BuyersRemarks = "";
            }
            string AddressMessage="";
            // queryData = queryData.Where(a => a.item_code != null && a.item_code.Contains(queryStr));
            if (model.AddressMessage != null && model.AddressMessage.Contains("-"))
            {
                AddressMessage = model.AddressMessage;
            }
           
            string[] address = AddressMessage.Split('-');
            string receiver_province = "";
            string receiver_city = "";
            string receiver_district = "";
            if (address.Length >= 1)
            {
                receiver_province = address[0];
            }
            if (address.Length >= 2)
            {
                receiver_city = address[1];
            }
            if (address.Length >= 3)
            {
                receiver_district = address[2];
            }
            DateTime dtshottime = DateTime.Now;
            DateTime shoptime = dtshottime.AddDays(-3);

            List<T_Reissue> ReissOrederModelList = db.T_Reissue.Where(a => a.OrderCode == model.OrderCode && a.CreatDate >= shoptime).ToList();
            string sellerRemarksList = "";
            if (ReissOrederModelList.Count >= 2)
            {
                sellerRemarksList = "系统备注:" + model.SystemRemark + "三天内多次补发换货," + sellerremarks + "制单人：" + model.PostUser;

            }
            else
            {
                sellerRemarksList = "系统备注:" + model.SystemRemark + sellerremarks + "制单人：" + model.PostUser;
            }
          

            GY gy = new GY();
            string cmd = "";
            cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.trade.add\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                        "\"platform_code\":\"" + model.NewOrderCode + "\"," +
                        "\"order_type_code\":\"" + model.OrderType + "\"," +
                        "\"shop_code\":\"" + model.StoreCode + "\"," +
                        "\"express_code\":\"" + model.ExpressName + "\"," +
                            "\"receiver_province\":\"" + receiver_province + "\"," +
                                "\"receiver_city\":\"" + receiver_city + "\"," +
                                    "\"receiver_district\":\"" + receiver_district + "\"," +
                        "\"warehouse_code\":\"" + model.WarhouseName + "\"," +
                        "\"vip_code\":\"" + model.VipCode + "\"," +
                        "\"vip_name\":\"" + model.VipName + "\"," +
                        "\"receiver_name\":\"" + model.ReceivingName + "\"," +
                        "\"receiver_address\":\"" + model.Address + "\"," +
                        "\"receiver_zip\":\"" + model.PostalCode + "\"," +
                        "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                        "\"receiver_phone\":\"" + model.Phone + "\"," +
                        "\"deal_datetime\":\"" + datetime + "\"," +
                        "\"buyer_memo\":\"" + BuyersRemarks + "\"," +
                        "\"seller_memo\":\"" + sellerRemarksList + "\"," +
                           "\"business_man_code\":\"" + model.BusinessName + "\"," +
                          "\"details\":[" + shangp + "]" +
                        "}";
            string sign = gy.Sign(cmd);
            string comcode = "{" +
                    "\"appkey\":\"171736\"," +
                    "\"method\":\"gy.erp.trade.add\"," +
                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                     "\"platform_code\":\"" + model.NewOrderCode + "\"," +
                    "\"order_type_code\":\"" + model.OrderType + "\"," +
                    "\"shop_code\":\"" + model.StoreCode + "\"," +
                    "\"express_code\":\"" + model.ExpressName + "\"," +
                    "\"receiver_province\":\"" + receiver_province + "\"," +
                    "\"receiver_city\":\"" + receiver_city + "\"," +
                    "\"receiver_district\":\"" + receiver_district + "\"," +
                    "\"warehouse_code\":\"" + model.WarhouseName + "\"," +
                    "\"vip_code\":\"" + model.VipCode + "\"," +
                    "\"vip_name\":\"" + model.VipName + "\"," +
                    "\"receiver_name\":\"" + model.ReceivingName + "\"," +
                    "\"receiver_address\":\"" + model.Address + "\"," +
                    "\"receiver_zip\":\"" + model.PostalCode + "\"," +
                    "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                    "\"receiver_phone\":\"" + model.Phone + "\"," +
                    "\"deal_datetime\":\"" + datetime + "\"," +
                    "\"buyer_memo\":\"" + BuyersRemarks + "\"," +
                    "\"sign\":\"" + sign + "\"," +
                    "\"seller_memo\":\"" + sellerRemarksList + "\"," +
                //   "\"receiver_province\":\"" + listmodel.address + "\"," +
                 "\"business_man_code\":\"" + model.BusinessName + "\"," +
                    "\"details\":[" + shangp + "]" +
                    "}";
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string sd = jsonData[0].ToString();

            if (sd == "False")
            {
                string ssaa = jsonData["errorDesc"].ToString();
                if (ssaa == "该订单已创建")
                {
                    return "True";
                }
            }

            return sd;
        }

        #endregion

        #region 视图

        #region 售后补发货

        [Description("售后补发货申请")]
        public ActionResult ViewReissueForAfterSaleAdd()
        {
            //T_OrderList order = db.T_OrderList.Find(ID);
            //T_Reissue model = new T_Reissue();
            //model.OrderCode = order.platform_code;
            //model.VipName = order.vip_name;
            //model.StoreName = order.shop_name;
            //model.ReceivingName = order.receiver_name;
            //model.PostalCode = order.receiver_zip;
            //model.Phone = order.receiver_phone;
            //model.TelPhone = order.receiver_mobile;
            //model.VipCode = order.vip_code;
            //model.Address = order.receiver_address;
            //model.AddressMessage = order.receiver_area;
            //model.BusinessName = UserModel.Nickname;
            //model.OrderId = ID;
            ViewData["order_Type"] = Com.OrderType();
            ViewData["warhouse_Name"] = Com.Warehouses();
            ViewData["express_Name"] = Com.ExpressName();
            //ViewData["code"] = order.code;
            ViewData["reson"] = Com.ReissueReson();
            return View();
        }


        [Description("售后补发货编辑")]
        public ActionResult ViewReissueForAfterSaleEdit(int id)
        {
            if (id == 0)
                return HttpNotFound();
            T_Reissue model = db.T_Reissue.Find(id);
            ViewData["order_Type"] = Com.OrderType(model.OrderType);
            ViewData["warhouse_Name"] = Com.Warehouses(model.WarhouseName);
            ViewData["express_Name"] = Com.WDTExpressName(model.ExpressName);
            ViewData["reson"] = Com.ReissueReson(model.ReissueReson);
            return View(model);
        }

        [Description("我的售后补发货")]
        public ActionResult ViewReissueForAfterSaleMy()
        {
            ViewData["orderType"] = Com.OrderType();
            return View();
        }

        #endregion

        #region 非售后补发货

        [Description("补发货申请")]
        public ActionResult ViewReissueAdd()
        {
            //T_OrderList order = db.T_OrderList.Find(ID);
            //T_Reissue model = new T_Reissue();
            //model.OrderCode = order.platform_code;
            //model.VipName = order.vip_name;
            //model.StoreName = order.shop_name;
            //model.ReceivingName = order.receiver_name;
            //model.PostalCode = order.receiver_zip;
            //model.Phone = order.receiver_phone;
            //model.TelPhone = order.receiver_mobile;
            //model.VipCode = order.vip_code;
            //model.Address = order.receiver_address;
            //model.AddressMessage = order.receiver_area;
            //model.BusinessName = UserModel.Nickname;
            //model.OrderId = ID;
            ViewData["order_Type"] = Com.OrderType();
            ViewData["warhouse_Name"] = Com.Warehouses();
            ViewData["express_Name"] = Com.WDTExpressName();
            //ViewData["code"] = order.code;
            ViewData["reson"] = Com.ReissueReson();
            return View();
        }

        [Description("补发货订单列表")]
        public ActionResult ViewOrderList()
        {
            return View();
        }

        [Description("售后补发货订单列表")]
        public ActionResult ViewAfterOrderList()
        {
            return View();
        }

        [Description("补发货编辑")]
        public ActionResult ViewReissueEdit(int id)
        {
            if (id == 0)
                return HttpNotFound();
            T_Reissue model = db.T_Reissue.Find(id);
            ViewData["order_Type"] = Com.OrderType(model.OrderType);
            ViewData["warhouse_Name"] = Com.Warehouses(model.WarhouseName);
            ViewData["express_Name"] = Com.WDTExpressName(model.ExpressName);
            ViewData["reson"] = Com.ReissueReson(model.ReissueReson);
            return View(model);
        }


        [Description("我的补发货")]
        public ActionResult ViewReissueForMy()
        {
            ViewData["orderType"] = Com.OrderType();
            return View();
        }
        //绑定原因
        public List<SelectListItem> GetRetreatReasonList()
        {
            var list = db.T_ReissueReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        [Description("补发货列表")]
        public ActionResult ViewReissueManager()
        {
            ViewData["orderType"] = Com.OrderType();
            ViewData["RetreatReasonList"] = GetRetreatReasonList();
            return View();
        }

        [Description("补发货未审核")]
        public ActionResult ViewReissueNotChecked()
        {
            ViewData["orderType"] = Com.OrderType();
            return View();
        }

        [Description("补发货已审核")]
        public ActionResult ViewReissueChecked()
        {
            ViewData["orderType"] = Com.OrderType();
            return View();
        }

        [Description("补发货详情")]
        public ActionResult ViewReissueDetail(int reissueId)
        {
            if (reissueId == 0)
                return HttpNotFound();
            var history = db.T_ReissueApprove.Where(a => a.Pid == reissueId);   
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
            ViewData["reissueId"] = reissueId;
            return View();
        }

        [Description("补发货审核")]
        public ActionResult ViewReissueApprove(int reissueId)
        {
            if (reissueId == 0)
                return HttpNotFound();
            var model = db.T_Reissue.Find(reissueId);
            var history = db.T_ReissueApprove.Where(a => a.Pid == reissueId);
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
            ViewData["reissueId"] = reissueId;
            T_ReissueApprove approve = db.T_ReissueApprove.FirstOrDefault(a => !a.ApproveTime.HasValue && a.Pid == reissueId);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
                ViewData["approveid"] = 0;
            model.WarhouseName = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.WarhouseName)).name;
         //   model.ExpressName = db.T_Express.SingleOrDefault(s => s.Code.Equals(model.ExpressName)).Name;
            return View(model);
        }

        #endregion

        #endregion

        #region Post提交


        #region 非售后

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ContentResult GetOrdersList(Lib.GridPager pager, string code)
        {
            IQueryable<T_OrderList> list = db.T_OrderList.Where(s => s.platform_code.Equals(code)).AsQueryable();
            pager.totalRows = list.Count();

            List<T_OrderList> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public List<T_Reissue> lits(int status)
        {
            //List<T_Reissue> listReissue = new List<T_Reissue>();
            //IQueryable<T_ReissueApprove> listapprove = null;
            //if (status == 0)//未审核
            //{
            //    listapprove = db.T_ReissueApprove.Where(s => s.ApproveUser.Equals(UserModel.Nickname) && !s.ApproveTime.HasValue);
            T_ReissueGroup group = db.T_ReissueGroup.SingleOrDefault(s => s.GroupUser.Contains(UserModel.Nickname));
            //    if (group != null)
            //        listapprove = db.T_ReissueApprove.Where(s => (s.ApproveName.Equals(group.GroupName) || s.ApproveUser.Equals(UserModel.Nickname)) && !s.ApproveTime.HasValue);
            //}
            //else
            //{
            //    listapprove = db.T_ReissueApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveTime.HasValue);
            //}
            //List<int> itemIds = new List<int>();
            //foreach (var item in listapprove.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            //{
            //    itemIds.Add(item.Key);
            //}

            //foreach (var item in itemIds)
            //{
            //    T_Reissue model = db.T_Reissue.SingleOrDefault(s => s.ID == item && s.IsDelete == 0);
            //    if (model != null)
            //        listReissue.Add(model);
            //}
            string sql = "";
            if (status == 0)
            {
                sql = "select * from T_Reissue where ID in(select Pid from T_ReissueApprove where  ApproveUser='" + UserModel.Nickname + "' and ApproveTime is null group by Pid) and IsDelete=0 ";
                if (group != null)
                    sql = "select * from T_Reissue where ID in(select Pid from T_ReissueApprove where (ApproveName ='" + group.GroupName + "' or ApproveUser='" + UserModel.Nickname + "') and ApproveTime is null group by Pid) and IsDelete=0";
            }
            else
                sql = "select * from T_Reissue where ID in(select Pid from T_ReissueApprove where  ApproveName='" + UserModel.Nickname + "' and ApproveTime is not null group by Pid) and IsDelete=0 ";

            List<T_Reissue> listReissue = db.Database.SqlQuery<T_Reissue>(sql).ToList();
            return listReissue;
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string query, string orderType, int my, string startDate, string endDate, string RetreatReason)
        {
			T_OperaterLog log = new T_OperaterLog() {
				Module="补发导出",
				OperateContent = string.Format("导出excel OutPutExcel 条件 query:{0},orderType:{1},my:{2},startDate:{3},endDate:{4},RetreatReason:{4}", query, orderType, my, startDate, endDate, RetreatReason),
				Operater= UserModel.Nickname,
				OperateTime=DateTime.Now,
				PID=-1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();
            List<T_Reissue> listReissue = new List<T_Reissue>();
            if (my == 1)
                listReissue = db.T_Reissue.Where(s => (s.PostUser.Equals(UserModel.Nickname) || s.DraftName.Equals(UserModel.Nickname)) && s.IsDelete == 0).ToList();
            else
                listReissue = lits(1);
            if (!string.IsNullOrWhiteSpace(query))
                listReissue = listReissue.Where(s => s.PostUser.Contains(query) || s.OrderCode.Contains(query) || s.VipName.Equals(query)).ToList();
            if (!string.IsNullOrWhiteSpace(orderType))
                listReissue = listReissue.Where(s => s.OrderType.Equals(orderType)).ToList();
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                listReissue = listReissue.Where(s => s.CreatDate >= start).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                listReissue = listReissue.Where(s => s.CreatDate <= end).ToList();
            }
            if (!string.IsNullOrWhiteSpace(RetreatReason))
                listReissue = listReissue.Where(s => s.ReissueReson.Contains(RetreatReason)).ToList();
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
            row1.CreateCell(16).SetCellValue("补发金额");
            row1.CreateCell(17).SetCellValue("补发原因");
            row1.CreateCell(18).SetCellValue("ID");
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



            //详情
            ISheet sheet2 = book.CreateSheet("详情表");
            IRow row2 = sheet2.CreateRow(0);
            row2.Height = 3 * 265;
            row2.CreateCell(0).SetCellValue("订单编号");
            row2.CreateCell(1).SetCellValue("产品代码");
            row2.CreateCell(2).SetCellValue("产品名称");
            row2.CreateCell(3).SetCellValue("数量");
            row2.CreateCell(4).SetCellValue("备注");
            row2.CreateCell(5).SetCellValue("主表ID");
            row2.CreateCell(6).SetCellValue("补发原因");
            row2.CreateCell(7).SetCellValue("系统备注");
            row2.CreateCell(8).SetCellValue("补发类型");
            for (int i = 0; i < listReissue.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName);
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";

                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                string code = listReissue[i].WarhouseName.ToString();
                string code1 = listReissue[i].ExpressName.ToString();
                string warhouseName = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(code)).name;
              //  string expressName = db.T_Express.SingleOrDefault(s => s.Code.Equals(code1)).Name;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName.ToString());
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].VipName) ? "" : listReissue[i].VipName.ToString());
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].OrderCode) ? "" : listReissue[i].OrderCode.ToString());
                rowtemp.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].WarhouseName) ? "" : warhouseName);
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ExpressName) ? "" : code1);
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].SalesRemark) ? "" : listReissue[i].SalesRemark.ToString());
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].SingleTime.ToString()) ? "" : listReissue[i].SingleTime.ToString());
                string type = listReissue[i].OrderType.ToString();
                rowtemp.CreateCell(7).SetCellValue(string.IsNullOrWhiteSpace(type) ? "" : db.T_OrderType.SingleOrDefault(s => s.Code.Equals(type)).Name);
                rowtemp.CreateCell(8).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].SalesRemark) ? "" : listReissue[i].SalesRemark.ToString());
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ReceivingName) ? "" : listReissue[i].ReceivingName.ToString());
                rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].Phone) ? "" : listReissue[i].Phone.ToString());
                rowtemp.CreateCell(11).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].TelPhone) ? "" : listReissue[i].TelPhone.ToString());
                rowtemp.CreateCell(12).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].PostalCode) ? "" : listReissue[i].PostalCode.ToString());
                rowtemp.CreateCell(13).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].AddressMessage) ? "" : listReissue[i].AddressMessage.ToString());
                rowtemp.CreateCell(14).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].VipCode) ? "" : listReissue[i].VipCode.ToString());
                rowtemp.CreateCell(15).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].PostUser) ? "" : listReissue[i].PostUser.ToString());
                rowtemp.CreateCell(16).SetCellValue(listReissue[i].Cost.ToString());
                rowtemp.CreateCell(17).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ReissueReson) ? "" : listReissue[i].ReissueReson.ToString());
                rowtemp.CreateCell(18).SetCellValue(listReissue[i].ID);

                NPOI.SS.UserModel.IRow rowtemp2 = sheet2.CreateRow(i + 1);
                int id = listReissue[i].ID;
                List<T_ReissueDetail> detail = db.T_ReissueDetail.Where(s=>s.ID==id).ToList();
                for (int dd=0;dd<detail.Count;dd++)
                {
                    rowtemp2.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].OrderCode) ? "" : listReissue[i].OrderCode.ToString());
                    rowtemp2.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(detail[dd].ProductCode) ? "" : detail[dd].ProductCode.ToString());
                    rowtemp2.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(detail[dd].ProductName) ? "" : detail[dd].ProductName.ToString());
                    rowtemp2.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(detail[dd].Num.ToString()) ? "" : detail[dd].Num.ToString());
                    rowtemp2.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(detail[dd].Remark) ? "" : detail[dd].Remark);
                    
                    rowtemp2.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(detail[dd].ReissueId.ToString()) ? "" : detail[dd].ReissueId.ToString());
                    rowtemp2.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ReissueReson) ? "" : listReissue[i].ReissueReson.ToString());
                    rowtemp2.CreateCell(7).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].SystemRemark) ? "" : listReissue[i].SystemRemark.ToString());
                    rowtemp2.CreateCell(8).SetCellValue(string.IsNullOrWhiteSpace(type) ? "" : db.T_OrderType.SingleOrDefault(s => s.Code.Equals(type)).Name);
                     
                }
                


            }
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "补发数据.xls");
        }

        //获取附件
        public JsonResult GetReissuePic(int id)
        {
            List<T_ReissuePic> model = db.T_ReissuePic.Where(a => a.ReissueId == id).ToList();
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
                T_ReissuePic model = db.T_ReissuePic.FirstOrDefault(a => a.ReissueId == id && a.Path == strPath);
                if (model != null)
                {
                    db.T_ReissuePic.Remove(model);
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 补发货保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public JsonResult ReissueAddSave(T_Reissue model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (model.AddressMessage != null && model.AddressMessage.Contains("-"))
                    {
                    List<T_ReissueDetail> details = Com.Deserialize<T_ReissueDetail>(jsonStr);
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "详情不能为空" });
                        if (string.IsNullOrWhiteSpace(model.ExpressName))
                            return Json(new { State = "Faile", Message = "发货快递必须选择" });
                        //T_OrderList order = db.T_OrderList.Find(model.OrderId);
                        List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                    string date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
                    string modeldate = "";
                    if (  reissue.Count>1)
                        modeldate = Convert.ToDateTime(reissue[0].CreatDate).ToString("yyyyMMdd");
                    string remark = "";
                    int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
                    T_ReissueConfig config = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type);
                    T_ReissueGroup group = db.T_ReissueGroup.SingleOrDefault(s => s.GroupName.Equals(config.ApproveType));
                    string departId = db.T_User.SingleOrDefault(s => s.Nickname.Equals(UserModel.Nickname)).DepartmentId;
                    int id = int.Parse(departId);
                    T_Department departMent = db.T_Department.Find(id);
                    string approneName = "";
                    if (departMent.supervisor == null && model.ReissueReson.Equals("呼吸机专用"))
                        approneName = "阿奎";
                    else if (departMent.supervisor == null)
                        approneName = "成风";
                    else
                        approneName = db.T_User.Find(departMent.supervisor).Nickname;
                        if (reissue.Count > 1 && int.Parse(date) - int.Parse(modeldate) <= 3  )
                            remark = model.SystemRemark + "3天内补发货重复";
                        else
                            remark = model.SystemRemark;
                    model.SystemRemark = remark;
                    model.Step = 0;
                        //model.NewOrderCode = "8" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        string t = "";
                        
                        if (reissue.Count < 1) 
                        {
                            t = "8001";
                            
                        }
                        else if (reissue.Count >= 1 && reissue.Count < 100)
                        {
                            int co = (int)reissue.Count + 1;
                            t = "800" + co;
                        }
                        else {
                            int co = (int)reissue.Count + 1;
                            t = "80" + co;
                        }
                        model.NewOrderCode = t + model.OrderCode;
                        model.Status = -1;
                    model.CreatDate = DateTime.Now;
                    model.PostUser = UserModel.Nickname;
                    model.IsDelete = 0;
                    model.Cost = details.Sum(s => s.Num * s.Price);
                    db.T_Reissue.Add(model);
                    db.SaveChanges();
                    foreach (var item in details)
                    {
                        item.ReissueId = model.ID;
                            T_WDTGoods goods = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == item.ProductCode && s.spec_aux_unit_name == null);
                              
                            if(goods==null)
                            {
                                return Json(new { State = "Faile", Message = item.ProductCode+ "该产品不存在请核实" });
                            }
                            
                            if (item.Remark == "null")
                            item.Remark = "";
                        db.T_ReissueDetail.Add(item);
                    }
                    db.SaveChanges();
                    T_ReissueApprove approve = new T_ReissueApprove
                    {
                        ApproveName = departMent.supervisor == null ? approneName : group.GroupName,
                        ApproveUser = group.GroupName.Equals("部门主管") ? approneName : "",
                        ApproveStatus = -1,
                        Pid = model.ID
                    };
                    db.T_ReissueApprove.Add(approve);
                    db.SaveChanges();
                    //if (reissue == null)
                    //{
                    //    order.ReissueStatus = 1;
                    //    db.SaveChanges();
                    //}
                    //附件保存
                    if (!string.IsNullOrWhiteSpace(jsonStr1))
                    {
                        //&& model.ReissueReson.Equals("快递破损")
                        List<T_ReissuePic> Enclosure = Com.Deserialize<T_ReissuePic>(jsonStr1);
                        foreach (var item in Enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ReissueId = model.ID;
                            db.T_ReissuePic.Add(item);
                        }
                        db.SaveChanges();
                    }

                   // ModularByZP();


                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                    else { 
                    return Json(new { State = "Faile", Message = "请填写正确的省市区信息" }, JsonRequestBehavior.AllowGet);
                }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReissueEditSave(T_Reissue model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ReissueDetail> details = Com.Deserialize<T_ReissueDetail>(jsonStr);
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "详情不能为空" });
                    T_Reissue reissue = db.T_Reissue.Find(model.ID);
                    int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
                    T_ReissueConfig config = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type);
                    T_ReissueGroup group = db.T_ReissueGroup.SingleOrDefault(s => s.GroupName.Equals(config.ApproveType));
                    string departId = db.T_User.SingleOrDefault(s => s.Nickname.Equals(UserModel.Nickname)).DepartmentId;
                    int id = int.Parse(departId);
                    T_Department departMent = db.T_Department.Find(id);
                    string approneName = "";
                    if (departMent.supervisor == null && model.ReissueReson.Equals("呼吸机专用"))
                        approneName = "阿奎";
                    else if (departMent.supervisor == null)
                        approneName = "成风";
                    else
                        approneName = db.T_User.Find(departMent.supervisor).Nickname;
                    int editStatus = int.Parse(model.Status.ToString());//原记录的状态
                    int editID = model.ID;//原记录的ID
                    string ReissueReson = model.ReissueReson;//记录原来的原因
                    T_Reissue PurMod = db.T_Reissue.Find(editID);
                    if (PurMod.Status != -1 && PurMod.Status != 2)
                    {
                        return Json(new { State = "Faile", Message = "该记录已经审核，不允许修改" }, JsonRequestBehavior.AllowGet);
                    }
                    reissue.ExpressName = model.ExpressName;
                    reissue.WarhouseName = model.WarhouseName;
                    reissue.OrderType = model.OrderType;
                    reissue.ReceivingName = model.ReceivingName;
                    reissue.BusinessName = model.BusinessName;
                    reissue.PostalCode = model.PostalCode;
                    reissue.Phone = model.Phone;
                    reissue.TelPhone = model.TelPhone;
                    reissue.VipCode = model.VipCode;
                    reissue.VipName = model.VipName;
                    reissue.AddressMessage = model.AddressMessage;
                    reissue.Address = model.Address;
                    reissue.SalesRemark = model.SalesRemark;
                    reissue.BuyRemark = model.BuyRemark;
                    reissue.ReissueReson = model.ReissueReson;
                    reissue.Cost = details.Sum(s => s.Price * s.Num);
                    db.SaveChanges();
                    //先删除详情再添加
                    List<T_ReissueDetail> dl = db.T_ReissueDetail.Where(s => s.ReissueId == model.ID).ToList();
                    if (dl.Count() > 0)
                    {
                        foreach (var item in dl)
                        {
                            T_ReissueDetail detail = db.T_ReissueDetail.Find(item.ID);
                            db.T_ReissueDetail.Remove(detail);
                        }
                        db.SaveChanges();
                    }
                    foreach (var item in details)
                    {
                        item.ReissueId = model.ID;
                        T_WDTGoods goods = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == item.ProductCode && s.spec_aux_unit_name == null);

                        if (goods == null)
                        {
                            return Json(new { State = "Faile", Message = item.ProductCode + "该产品不存在请核实" });
                        }
                        if (item.Remark == "null")
                            item.Remark = "";
                        db.T_ReissueDetail.Add(item);
                    }
                    db.SaveChanges();
                    if (model.Status == 2)
                    {
                        T_ReissueApprove approve = new T_ReissueApprove();
                        approve.ApproveName = departMent.supervisor == null ? approneName : group.GroupName;
                        approve.ApproveStatus = -1;
                        approve.ApproveUser = group.GroupName.Equals("部门主管") ? approneName : "";
                        reissue.Step = 0;
                        reissue.Status = -1;
                        approve.Pid = model.ID;
                        db.T_ReissueApprove.Add(approve);
                        db.SaveChanges();
                    }
                    List<T_ReissuePic> enclosure = Com.Deserialize<T_ReissuePic>(jsonStr1);
                    //附件保存 先删除原有的附件
                    List<T_ReissuePic> delMod = db.T_ReissuePic.Where(a => a.ReissueId == model.ID).ToList();
                    foreach (var item in delMod)
                    {
                        db.T_ReissuePic.Remove(item);
                    }
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(jsonStr1) && model.ReissueReson.Equals("快递破损"))
                    {
                        foreach (var item in enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ReissueId = model.ID;
                            db.T_ReissuePic.Add(item);
                        }
                        db.SaveChanges();
                    }
                    //ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }


        /// <summary>
        /// 提交到审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SubmitApprove(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_Reissue model = db.T_Reissue.Find(id);
                    model.Status = -1;
                    db.SaveChanges();
                    //int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
                    //T_ReissueConfig config = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type);
                    //T_ReissueGroup group = db.T_ReissueGroup.SingleOrDefault(s => s.GroupName.Equals(config.ApproveType));
                    //string departId = db.T_User.SingleOrDefault(s => s.Nickname.Equals(UserModel.Nickname)).DepartmentId;
                    //int ids = int.Parse(departId);
                    //T_Department departMent = db.T_Department.Find(ids);
                    //string approneName = "";
                    //if (departMent.supervisor == null && model.ReissueReson.Equals("呼吸机专用"))
                    //    approneName = "阿奎";
                    //else if (departMent.supervisor == null)
                    //    approneName = "成风";
                    //else
                    //    approneName = db.T_User.Find(departMent.supervisor).Nickname;
                    T_ReissueApprove Reissueapprove = new T_ReissueApprove
                    {
                        ApproveName = Com.GetReissueName(model.StoreCode, model.ReissueReson),
                        ApproveUser = Com.GetReissueName(model.StoreCode, model.ReissueReson),
                        ApproveStatus = -1,
                        Pid = model.ID
                    };
                    db.T_ReissueApprove.Add(Reissueapprove);
                    db.SaveChanges();
                    //T_ReissueApprove approve = new T_ReissueApprove();
                    //approve.ApproveName = departMent.supervisor == null ? approneName : group.GroupName;
                    //approve.ApproveStatus = -1;
                    //approve.Pid = model.ID;
                    //approve.ApproveUser = group.GroupName.Equals("部门主管") ? approneName : "";
                    //db.T_ReissueApprove.Add(approve);
                    //db.SaveChanges();
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



        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "补发货").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                  
                }
                db.SaveChanges();
               
            }

            string RetreatAppRoveSql = " select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ReissueApprove where  Pid in ( select ID from T_Reissue where IsDelete=0  and ( status=-1 or status=0)) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser  ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "补发货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "补发货";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_Reissue where Status='2' and IsDelete=0 GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "补发货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "补发货";
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
        /// 我的补发货
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueForMyList(Lib.GridPager pager, string code, string orderType, string startDate, string endDate, int status = -3)
        {
            IQueryable<T_Reissue> list = db.T_Reissue.Where(s => s.PostUser.Equals(UserModel.Nickname) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderCode.Equals(code) || s.VipName.Equals(code) || s.NewOrderCode.Equals(code));
            if (status != -3)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(orderType))
                list = list.Where(s => s.OrderType.Equals(orderType));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.CreatDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CreatDate <= end);
            }
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in list)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
               // model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }



        /// <summary>
        ///  审核
        /// </summary>
        /// <param name="approveID"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
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
                    int TotalStep = db.T_ReissueConfig.ToList().Count;

                    List<T_ReissueGroup> RetreatGroupList = db.Database.SqlQuery<T_ReissueGroup>("select  * from T_ReissueGroup where GroupUser like '%" + Nickname + "%'").ToList();
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

                    string sql = "select * from T_ReissueApprove where ID='" + approveID + "' and ApproveTime is null ";
                    if (GroupName != "" && GroupName != null)
                    {
                        sql += "  and (ApproveName='" + Nickname + "' or ApproveUser='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                    }
                    else
                    {
                        sql += "    and (ApproveName='" + Nickname + "' or ApproveUser='" + Nickname + "' ) ";
                    }
                    List<T_ReissueApprove> AppRoveListModel = db.Database.SqlQuery<T_ReissueApprove>(sql).ToList();
                    if (AppRoveListModel.Count == 0)
                    {
                        return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
                    }
                    T_ReissueApprove approve = db.T_ReissueApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1);
                  
                    string name = approve.ApproveName;
                    T_Reissue model = db.T_Reissue.Find(approve.Pid);
                    approve.ApproveName = UserModel.Nickname;
                    approve.ApproveStatus = status;
                    approve.ApproveTime = DateTime.Now;
                    approve.Memo = memo;



                    db.SaveChanges();


                    model.ISReissue = "0";
                    model.Status = status;
                    if (status == 2)//不同意
                    {
                        model.Step = model.Step + 1;
                        db.SaveChanges();
                       
                    }
                    else//同意
                    {
                        int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
                        int LastStep = db.T_ReissueConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
                        if (LastStep > model.Step)//判断是否存在下一级
                        {
                            //获得下一级审核部门
                            string nextapproveType = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
                            T_ReissueApprove newApprove = new T_ReissueApprove();
                            newApprove.ApproveStatus = -1;
                            newApprove.ApproveName = nextapproveType;
                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_ReissueApprove.Add(newApprove);
                            db.SaveChanges();
                            model.Status = 0;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }
                        if (name.Equals("售后主管") || name.Equals("呼吸机主管"))//售后主管审核后直接加入补发货
                        {
                            //T_OrderList order = db.T_OrderList.Find(model.OrderId);

                            //List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                            ////判断是否为第一次补发
                            //if (reissue.Count() == 1)
                            //{
                            //    order.ReissueStatus = 2;
                            //}
                            //List<T_ReissueDetail> reiDetails = db.T_ReissueDetail.Where(s => s.ReissueId == model.ID).ToList();
                            //foreach (var item in reiDetails)
                            //{
                            //    T_OrderDetail Orderdetail = db.T_OrderDetail.FirstOrDefault(s => s.oid.Equals(order.code) && s.item_code.Equals(item.ProductCode));
                            //    if (Orderdetail != null)
                            //    {
                            //        Orderdetail.ReissueStatus = 1;
                            //        Orderdetail.ReissueQty += item.Num;
                            //    }
                            //}
                            db.SaveChanges();
                            #region 加入快递赔付

                            //if (model.OrderType != "Return")
                            //{
                            //    if (model.ReissueReson.Equals("快递破损") || model.ReissueReson.Equals("丢件"))
                            //    {
                            //        GY gy = new GY();
                            //        string cmd = "";
                            //        string code="";

                            //        if (model.OrderSCode == null || model.OrderSCode == "")
                            //        {
                            //            code = model.OrderSCode;
                            //            cmd = "{" +
                            //            "\"appkey\":\"171736\"," +
                            //            "\"method\":\"gy.erp.trade.get\"," +
                            //            "\"page_no\":1," +
                            //            "\"page_size\":10," +
                            //            "\"platform_code\":\"" + model.OrderCode + "\"," +
                            //            "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                            //            "}";

                            //            string sign = gy.Sign(cmd);
                            //            cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                            //            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                            //            JsonData jsonData = null;
                            //            jsonData = JsonMapper.ToObject(ret);

                            //            if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                            //            {
                            //                cmd = "{" +
                            //                "\"appkey\":\"171736\"," +
                            //                "\"method\":\"gy.erp.trade.history.get\"," +
                            //                "\"page_no\":1," +
                            //                "\"page_size\":10," +
                            //                "\"platform_code\":\"" + model.OrderCode + "\"," +
                            //                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                            //                "}";
                            //                sign = gy.Sign(cmd);
                            //                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                            //                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                            //                jsonData = null;
                            //                jsonData = JsonMapper.ToObject(ret);
                            //                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                            //                {
                            //                    return Json(new { State = "Faile", Message = "订单号不存在" });
                            //                }
                            //            }
                            //            JsonData jsonOrders = jsonData["orders"][0];
                            //            var deliver = jsonOrders["deliverys"][0];
                            //            //快递单号
                            //            string mail_no = isNULL(deliver["mail_no"]).ToString();
                            //            //订单金额
                            //            string amount = isNULL(jsonOrders["amount"]).ToString();
                            //            //快递名称
                            //            string express_name = isNULL(deliver["express_name"]).ToString();
                            //            List<T_ExpressIndemnity> indeList = db.T_ExpressIndemnity.Where(a => a.RetreatExpressNum == mail_no && a.IsDelete == 0).ToList();
                            //            string IType = "丢件";
                            //            if (model.ReissueReson.Equals("快递破损"))
                            //            {
                            //                IType = "破损";
                            //            }
                            //            if (indeList.Count == 0)//快递单号不存在于快递赔付记录中
                            //            {
                            //                T_ExpressIndemnity Inde = new T_ExpressIndemnity
                            //                {
                            //                    PostUserName = model.PostUser,
                            //                    Date = DateTime.Now,
                            //                    OrderNum = model.OrderCode,
                            //                    wangwang = model.VipCode,
                            //                    ShopName = model.StoreName,
                            //                    Module = "补发",
                            //                    RetreatExpressNum = mail_no,
                            //                    State = "0",
                            //                    OrderMoney = Convert.ToDouble(amount),
                            //                    Type = IType,
                            //                    Second = "0",
                            //                    CurrentApproveName = "快递组",
                            //                    IsDelete = 0,
                            //                    ExpressName = express_name,
                            //                    IndemnityMoney = 0
                            //                };
                            //                int isrepeat = db.T_ExpressIndemnity.Where(a => a.OrderNum == model.OrderCode && a.IsDelete == 0).Count();
                            //                if (isrepeat > 0)
                            //                {
                            //                    Inde.Repeat = "订单号重复";
                            //                }
                            //                db.T_ExpressIndemnity.Add(Inde);
                            //                db.SaveChanges();
                            //                List<T_ReissuePic> picList = db.T_ReissuePic.Where(s => s.ReissueId == model.ID).ToList();
                            //                foreach (var item in picList)
                            //                {
                            //                    T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
                            //                    {
                            //                        EID = Inde.ID,
                            //                        PicURL = item.Url
                            //                    };
                            //                    db.T_ExpressIndemnityPic.Add(expressPic);
                            //                }
                            //                db.SaveChanges();
                            //            }
                            //            else
                            //            {
                            //                List<T_ReissuePic> picList = db.T_ReissuePic.Where(s => s.ReissueId == model.ID).ToList();
                            //                foreach (var item in picList)
                            //                {
                            //                    T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
                            //                    {
                            //                        EID = indeList.First().ID,
                            //                        PicURL = item.Url
                            //                    };
                            //                    db.T_ExpressIndemnityPic.Add(expressPic);
                            //                }
                            //                db.SaveChanges();
                            //            }
                            //          }
                            //        else
                            //        {
                            //            code = model.OrderSCode ;
                            //             cmd = "v=1.0&sign=&message={\"ordercodes\": ['" + code + "']}";
                            //            string ret = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/QuerySaleOrder", cmd, Encoding.UTF8);
                            //            JsonData jsonData = null;
                            //            jsonData = JsonMapper.ToObject(ret);

                            //            JsonData jsonMessage = jsonData["message"];
                            //            int RecordCount = int.Parse(jsonMessage["RecordCount"].ToString());
                            //            if (RecordCount > 0)
                            //            {
                            //                JsonData jsonMessageData = jsonMessage["Data"][0];
                            //                JsonData jsonproductdetails = jsonMessageData["productdetails"];
                            //                JsonData jsonpaymentdetails = jsonMessageData["paymentdetails"];
                                          
                            //                string express_name = jsonproductdetails[0]["ExpressNameActual"].ToString();
                            //                string mail_no= jsonproductdetails[0]["ExpressNumber"].ToString();
                            //                string amount = isNULL(jsonpaymentdetails[0]["amount"]).ToString();
                            //                List<T_ExpressIndemnity> indeList = db.T_ExpressIndemnity.Where(a => a.RetreatExpressNum == mail_no && a.IsDelete == 0).ToList();
                            //                string IType = "丢件";
                            //                if (model.ReissueReson.Equals("快递破损"))
                            //                {
                            //                    IType = "破损";
                            //                }

                            //                if (indeList.Count == 0)//快递单号不存在于快递赔付记录中
                            //                {
                            //                    T_ExpressIndemnity Inde = new T_ExpressIndemnity
                            //                    {
                            //                        PostUserName = model.PostUser,
                            //                        Date = DateTime.Now,
                            //                        OrderNum = model.OrderCode,
                            //                        wangwang = model.VipCode,
                            //                        ShopName = model.StoreName,
                            //                        Module = "补发",
                            //                        RetreatExpressNum = mail_no,
                            //                        State = "0",
                            //                        OrderMoney = Convert.ToDouble(amount),
                            //                        Type = IType,
                            //                        Second = "0",
                            //                        CurrentApproveName = "快递组",
                            //                        IsDelete = 0,
                            //                        ExpressName = express_name,
                            //                        IndemnityMoney = 0
                            //                    };
                            //                    int isrepeat = db.T_ExpressIndemnity.Where(a => a.OrderNum == model.OrderCode && a.IsDelete == 0).Count();
                            //                    if (isrepeat > 0)
                            //                    {
                            //                        Inde.Repeat = "订单号重复";
                            //                    }
                            //                    db.T_ExpressIndemnity.Add(Inde);
                            //                    db.SaveChanges();
                            //                    List<T_ReissuePic> picList = db.T_ReissuePic.Where(s => s.ReissueId == model.ID).ToList();
                            //                    foreach (var item in picList)
                            //                    {
                            //                        T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
                            //                        {
                            //                            EID = Inde.ID,
                            //                            PicURL = item.Url
                            //                        };
                            //                        db.T_ExpressIndemnityPic.Add(expressPic);
                            //                    }
                            //                    db.SaveChanges();
                            //                }
                            //                else
                            //                {
                            //                    List<T_ReissuePic> picList = db.T_ReissuePic.Where(s => s.ReissueId == model.ID).ToList();
                            //                    foreach (var item in picList)
                            //                    {
                            //                        T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
                            //                        {
                            //                            EID = indeList.First().ID,
                            //                            PicURL = item.Url
                            //                        };
                            //                        db.T_ExpressIndemnityPic.Add(expressPic);
                            //                    }
                            //                    db.SaveChanges();
                            //                }

                            //            }
                            //            else
                            //            {
                            //                return Json(new { State = "Faile", Message = "订单号不存在" });
                            //            }


                            //        }
                            //    }
                                  
                            //    //else if (model.ReissueReson.Equals("丢件"))
                            //    //{
                            //    //    GY gy = new GY();
                            //    //    string cmd = "";

                            //    //    cmd = "{" +
                            //    //          "\"appkey\":\"171736\"," +
                            //    //          "\"method\":\"gy.erp.trade.get\"," +
                            //    //          "\"page_no\":1," +
                            //    //          "\"page_size\":10," +
                            //    //          "\"platform_code\":\"" + model.OrderCode + "\"," +
                            //    //          "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                            //    //          "}";

                            //    //    string sign = gy.Sign(cmd);
                            //    //    cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                            //    //    string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                            //    //    JsonData jsonData = null;
                            //    //    jsonData = JsonMapper.ToObject(ret);

                            //    //    if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                            //    //    {
                            //    //        cmd = "{" +
                            //    //        "\"appkey\":\"171736\"," +
                            //    //        "\"method\":\"gy.erp.trade.history.get\"," +
                            //    //        "\"page_no\":1," +
                            //    //        "\"page_size\":10," +
                            //    //        "\"platform_code\":\"" + model.OrderCode + "\"," +
                            //    //        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                            //    //        "}";
                            //    //        sign = gy.Sign(cmd);
                            //    //        cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                            //    //        ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                            //    //        jsonData = null;
                            //    //        jsonData = JsonMapper.ToObject(ret);
                            //    //        if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                            //    //        {
                            //    //            return Json(new { State = "Faile", Message = "订单号不存在" });
                            //    //        }
                            //    //    }
                            //    //    JsonData jsonOrders = jsonData["orders"][0];
                            //    //    var deliver = jsonOrders["deliverys"][0];
                            //    //    //快递单号
                            //    //    string mail_no = isNULL(deliver["mail_no"]).ToString();
                            //    //    //订单金额
                            //    //    string amount = isNULL(jsonOrders["amount"]).ToString();
                            //    //    //快递名称
                            //    //    string express_name = isNULL(jsonOrders["express_name"]).ToString();
                            //    //    T_ExpressIndemnity Inde = new T_ExpressIndemnity
                            //    //    {
                            //    //        PostUserName = model.PostUser,
                            //    //        Date = DateTime.Now,
                            //    //        OrderNum = model.OrderCode,
                            //    //        wangwang = model.VipCode,
                            //    //        ShopName = model.StoreName,
                            //    //        RetreatExpressNum = mail_no,
                            //    //        State = "0",
                            //    //        Module = "补发",
                            //    //        OrderMoney = Convert.ToDouble(amount),
                            //    //        Type = "丢件",
                            //    //        Second = "0",
                            //    //        CurrentApproveName = "快递组",
                            //    //        IsDelete = 0,
                            //    //        ExpressName = express_name,
                            //    //        IndemnityMoney = 0
                            //    //    };
                            //    //    db.T_ExpressIndemnity.Add(Inde);
                            //    //    db.SaveChanges();
                            //    //}
                            //}

                            #endregion

                            
                        }
                        db.SaveChanges();
                    }
                    db.SaveChanges();

                 

                   // ModularByZP();

                    //if ((name.Equals("售后主管") || name.Equals("呼吸机主管"))&&status==1)//售后主管审核后直接加入补发货
                    //{
                    //    if (PostGy(model) != "True")
                    //    {
                    //        return Json(new { State = "Faile", Message = "上传管易错误,请联系管理员" }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}

                 

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
        /// 补发货列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueManagerList(Lib.GridPager pager, string code, string startDate, string endDate, string orderType, string RetreatReason, int status = -3)
        {
            IQueryable<T_Reissue> list = db.T_Reissue.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderCode.Equals(code) || s.NewOrderCode.Equals(code) || s.PostUser.Equals(code) || s.VipName.Equals(code));
            if (status != -3)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(orderType))
                list = list.Where(s => s.OrderType.Equals(orderType));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.CreatDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CreatDate <= end);
            }
            if (!string.IsNullOrWhiteSpace(RetreatReason))
            {
                list = list.Where(s => s.ReissueReson.Equals(RetreatReason));
            }
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in list)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
                //if (model.OrderType != null)
                //{
                //    model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                //}
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 补发货未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueNotCheckedList(Lib.GridPager pager, string name, string orderType)
        {
            IQueryable<T_Reissue> reissueList = lits(0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                reissueList = reissueList.Where(s => s.PostUser.Equals(name) || s.OrderCode.Contains(name) || s.NewOrderCode.Equals(name));
            pager.totalRows = reissueList.Count();
            reissueList = reissueList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in reissueList)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
                //if (model.OrderType != null)
                //{
                //    model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                //}
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 补发货已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueCheckedList(Lib.GridPager pager, string name, string orderType, string startDate, string endDate)
        {
            IQueryable<T_Reissue> reissueList = lits(1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                reissueList = reissueList.Where(s => s.PostUser.Equals(name) || s.OrderCode.Equals(name) || s.VipName.Equals(name));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                reissueList = reissueList.Where(s => s.CreatDate >= start);
            }
            else if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                reissueList = reissueList.Where(s => s.CreatDate <= end);
            }
            pager.totalRows = reissueList.Count();
            reissueList = reissueList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in reissueList)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
              //  model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获取补发货详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="reissueId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueDetail(Lib.GridPager pager, int reissueId)
        {
            IQueryable<T_ReissueDetail> list = db.T_ReissueDetail.Where(s => s.ReissueId == reissueId).AsQueryable();
            pager.totalRows = list.Count();
            List<T_ReissueDetail> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReissueDelete(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_Reissue model = db.T_Reissue.Find(id);
                    model.IsDelete = 1;
                    db.SaveChanges();
                    //List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                    //if (reissue.Count() == 0)
                    //{
                    //    T_OrderList order = db.T_OrderList.Find(model.OrderId);
                    //    order.ReissueStatus = 0;
                    //    db.SaveChanges();
                    //}
                    ////删除新订单数据
                    //if (!string.IsNullOrWhiteSpace(model.NewOrderCode))
                    //{
                    //    T_OrderList o = db.T_OrderList.SingleOrDefault(s => s.platform_code.Equals(model.NewOrderCode));
                    //    List<T_OrderDetail> details = db.T_OrderDetail.Where(s => s.oid.Equals(o.code)).ToList();
                    //    foreach (var item in details)
                    //    {
                    //        T_OrderDetail detail = db.T_OrderDetail.Find(item.id);
                    //        db.T_OrderDetail.Remove(detail);
                    //    }
                    //    db.T_OrderList.Remove(o);
                    //    db.SaveChanges();
                    //}
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
        /// 查询补发货是否重复
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReissueOrder(string code)
        {
            T_Reissue model = db.T_Reissue.OrderByDescending(s => s.ID).FirstOrDefault(s => s.OrderCode.Contains(code) && s.IsDelete == 0);
            var date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
            var modeldate = "";
            if (model != null)
                modeldate = Convert.ToDateTime(model.CreatDate).ToString("yyyyMMdd");
            if (model != null && int.Parse(date) - int.Parse(modeldate) <= 3)
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 售后补发货

        /// <summary>
        /// 售后补发货保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        public JsonResult ReissueAfterSaleAddSave(T_Reissue model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ReissueDetail> details = Com.Deserialize<T_ReissueDetail>(jsonStr);
                    if (string.IsNullOrWhiteSpace(jsonStr) && string.IsNullOrWhiteSpace(model.SalesRemark))
                        return Json(new { State = "Faile", Message = "详情或卖家备注必须填一个" });
                    //T_OrderList order = db.T_OrderList.Find(model.OrderId);
                    string remark = "";
                    T_Reissue re = db.T_Reissue.FirstOrDefault(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0);
                    if (re != null)
                        remark = model.SystemRemark + "补发货重复";
                    model.SystemRemark = remark;
                    model.Step = 0;
                    model.Status = -2;
                    model.CreatDate = DateTime.Now;
                    model.PostUser = UserModel.Nickname;
                    model.IsDelete = 0;
                    //model.StoreCode = order.shop_code;
                    model.DraftName = UserModel.Nickname;
                    model.Cost = details.Sum(s => s.Num * s.Price);
                    db.T_Reissue.Add(model);
                    db.SaveChanges();
                    if (!string.IsNullOrWhiteSpace(jsonStr))
                    {
                        foreach (var item in details)
                        {
                            item.ReissueId = model.ID;
                            if (item.Remark == "null")
                                item.Remark = "";
                            db.T_ReissueDetail.Add(item);
                        }
                        db.SaveChanges();
                    }
                    //if (re == null)
                    //{
                    //    order.ReissueStatus = 1;
                    //    db.SaveChanges();
                    //}
                    //附件保存
                    if (!string.IsNullOrWhiteSpace(jsonStr1))
                    {
                        List<T_ReissuePic> Enclosure = Com.Deserialize<T_ReissuePic>(jsonStr1);
                        foreach (var item in Enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ReissueId = model.ID;
                            db.T_ReissuePic.Add(item);
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
        /// 售后编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReissueForAfterSaleEditSave(T_Reissue model, string jsonStr, string jsonStr1)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_Reissue reissue = db.T_Reissue.Find(model.ID);
                    List<T_ReissueDetail> details = Com.Deserialize<T_ReissueDetail>(jsonStr);
                    if (string.IsNullOrWhiteSpace(jsonStr) && string.IsNullOrWhiteSpace(model.SalesRemark))
                        return Json(new { State = "Faile", Message = "详情或卖家备注必须填一个" });
                    reissue.ExpressName = model.ExpressName;
                    reissue.WarhouseName = model.WarhouseName;
                    reissue.OrderType = model.OrderType;
                    reissue.ReceivingName = model.ReceivingName;
                    reissue.BusinessName = model.BusinessName;
                    reissue.PostalCode = model.PostalCode;
                    reissue.Phone = model.Phone;
                    reissue.TelPhone = model.TelPhone;
                    reissue.VipCode = model.VipCode;
                    reissue.VipName = model.VipName;
                    reissue.AddressMessage = model.AddressMessage;
                    reissue.Address = model.Address;
                    reissue.SalesRemark = model.SalesRemark;
                    reissue.BuyRemark = model.BuyRemark;
                    reissue.Cost = details.Sum(s => s.Num * s.Price);
                    reissue.ReissueReson = model.ReissueReson;
                    db.SaveChanges();
                    List<T_ReissueDetail> dl = db.T_ReissueDetail.Where(s => s.ReissueId == model.ID).ToList();
                    if (dl.Count() > 0)
                    {
                        foreach (var item in dl)
                        {
                            T_ReissueDetail detail = db.T_ReissueDetail.Find(item.ID);
                            db.T_ReissueDetail.Remove(detail);
                        }
                        db.SaveChanges();
                    }
                    if (!string.IsNullOrWhiteSpace(jsonStr))
                    {
                        foreach (var item in details) 
                        {
                            item.ReissueId = model.ID;
                            if (item.Remark == "null")
                                item.Remark = "";
                            db.T_ReissueDetail.Add(item);
                        }
                    }
                    db.SaveChanges();
                    List<T_ReissuePic> enclosure = Com.Deserialize<T_ReissuePic>(jsonStr1);
                    //附件保存 先删除原有的附件
                    List<T_ReissuePic> delMod = db.T_ReissuePic.Where(a => a.ReissueId == model.ID).ToList();
                    foreach (var item in delMod)
                    {
                        db.T_ReissuePic.Remove(item);
                    }
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(jsonStr1))
                    {
                        foreach (var item in enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ReissueId = model.ID;
                            db.T_ReissuePic.Add(item);
                        }
                        db.SaveChanges();
                    }
                   // ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }

        /// <summary>
        /// 我的售后补发货
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueAfterForSaleMyList(Lib.GridPager pager, string code, string orderType, string startDate, string endDate, int status = -3)
        {
            IQueryable<T_Reissue> list = db.T_Reissue.Where(s => s.PostUser.Equals(UserModel.Nickname) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderCode.Equals(code) || s.VipName.Equals(code));
            if (status != -3)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(orderType))
                list = list.Where(s => s.OrderType.Equals(orderType));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.CreatDate >= start);
            }
            else if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CreatDate <= end);
            }
            pager.totalRows = list.Count();
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in list)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
                model.ExpressName = Com.GetExpressName(model.ExpressName);
               // model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                lists.Add(model);
            }
            List<T_Reissue> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

        #endregion
        
        [Description("补发未发货")]
        public ActionResult ViewReissueEverywhere()
        {
            
            return View();
        }
        [Description("补发已导出")]
        public ActionResult ViewExportedEverywhere()
        {

            return View();
        }
        /// <summary>
        /// 导出Excel查询
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReissueEverywhereList(Lib.GridPager pager, string name,string Repeat)
        {
            IQueryable<T_Reissue> reissueList = db.T_Reissue.Where(s => s.Status == 1 && s.ISReissue == "0").AsQueryable();
            if (Repeat=="1")
            {
                reissueList = reissueList.Where(s => s.SystemRemark.Contains("重复")  ); 
            }
             //IQueryable<T_Reissue> reissueList = db.Database.SqlQuery<T_Reissue>(sql).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                reissueList = reissueList.Where(s => s.PostUser.Equals(name) || s.OrderCode.Equals(name) || s.VipName.Equals(name));
            pager.totalRows = reissueList.Count();
            reissueList = reissueList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in reissueList)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
               // model.OrderType = db.T_OrderType.SingleOrDefault(s => s.Code.Equals(model.OrderType)).Name;
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [HttpPost]
        [Description("修改为不导出状态")]
        public JsonResult VoidReissue(string ids)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            int s = 0;
            string[] ListID = ids.Split(',');
            for (int i = 0; i < ListID.Length - 1; i++)
            {
                if (i == 0)
                {
                    ids = "'" + ListID[i] + "'";
                }
                else
                {
                    ids += ",'" + ListID[i] + "'";
                }
                int Rid = int.Parse(ListID[i]);
                T_Reissue model = db.T_Reissue.SingleOrDefault(a => a.ID == Rid);
                model.ISReissue = "2";
                model.ISReissueDate = DateTime.Now;
                model.ReissueName = Nickname;
                db.Entry<T_Reissue>(model).State = System.Data.Entity.EntityState.Modified;
                s += db.SaveChanges();
            }

            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
        }
        public partial class ReissueEverywhere
        {
            public int ID { get; set; }
            public int ReissueId { get; set; }
            public string  ProductCode { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Num { get; set; }
            public string Remark { get; set; }
            public string NewOrderCode { get; set; }
            public string StoreName { get; set; }
            public string ReceivingName { get; set; }
            public string VipName { get; set; }
            public string AddressMessage { get; set; }
            public string TelPhone { get; set; }

            public string Phone { get; set; }
            
            public string Address { get; set; }
            public string VipCode { get; set; }
            public string ExpressName { get; set; }
            public string OrderType { get; set; }
            public string SalesRemark { get; set; }
            public string BuyRemark { get; set; }
            public string SystemRemark { get; set; }
        
        }


        [HttpPost]
        public JsonResult EditReissue(string ids,int repeat = 0)
        {
           
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            int s = 0;
            string[] ListID = ids.Split(',');

            for (int i = 0; i < ListID.Length - 1; i++)
            {
                if (i == 0)
                {
                    ids = "'" + ListID[i] + "'";
                }
                else
                {
                    ids += ",'" + ListID[i] + "'";
                }
                int Rid = int.Parse(ListID[i]);
                T_Reissue model = db.T_Reissue.SingleOrDefault(a => a.ID == Rid);
                if (repeat == 1)
                {
                    model.ReissueName += Nickname + ",";
                }
                else {
                    model.ISReissue = "1";
                    model.ISReissueDate = DateTime.Now;
                    model.ReissueName += Nickname + ",";
                }
                
                db.Entry<T_Reissue>(model).State = System.Data.Entity.EntityState.Modified;
               s+=db.SaveChanges();

            }
            
            if (s>0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 导出补发excel至旺店通
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public FileResult OutPutExcelEverywhere(string ids)
        {
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "补发导出",
				OperateContent = string.Format("导出excel OutPutExcelEverywhere 条件 ids:{0}", ids),
				Operater = UserModel.Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();
			string[] ListID = ids.Split(',');

            for (int i = 0; i < ListID.Length-1; i++)
            {
                if (i == 0)
                {
                    ids = "'" + ListID[i] + "'";
                }
                else
                {
                    ids += ",'" + ListID[i] + "'";
                }
            }
           
            List<ReissueEverywhere> listReissue = db.Database.SqlQuery<ReissueEverywhere>("select *,(select ReceivingName from  T_Reissue where ID=d.ReissueId) ReceivingName,(select NewOrderCode from  T_Reissue where ID=d.ReissueId) NewOrderCode,(select StoreName from  T_Reissue where ID=d.ReissueId) StoreName,(select VipName from  T_Reissue where ID=d.ReissueId) VipName,(select AddressMessage from  T_Reissue where ID=d.ReissueId) AddressMessage,(select TelPhone from  T_Reissue where ID=d.ReissueId) TelPhone,(select Phone from  T_Reissue where ID=d.ReissueId) Phone,(select Address from  T_Reissue where ID=d.ReissueId) Address,(select VipCode from  T_Reissue where ID=d.ReissueId) VipCode,(select ExpressName from  T_Reissue where ID=d.ReissueId) ExpressName,(select OrderType from  T_Reissue where ID=d.ReissueId) OrderType,(select SalesRemark from  T_Reissue where ID=d.ReissueId) SalesRemark,(select BuyRemark from  T_Reissue where ID=d.ReissueId) BuyRemark,(select SystemRemark from  T_Reissue where ID=d.ReissueId) SystemRemark from T_ReissueDetail d    where ReissueId in (" + ids + ")").ToList();

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
            row1.CreateCell(1).SetCellValue("原始单号");
            row1.CreateCell(2).SetCellValue("收件人");
            row1.CreateCell(3).SetCellValue("省");
            row1.CreateCell(4).SetCellValue("市");
            row1.CreateCell(5).SetCellValue("区");
            row1.CreateCell(6).SetCellValue("手机");
            row1.CreateCell(7).SetCellValue("固话");
            row1.CreateCell(8).SetCellValue("邮编");
            row1.CreateCell(9).SetCellValue("网名");
            row1.CreateCell(10).SetCellValue("地址");
            row1.CreateCell(11).SetCellValue("发货条件");
            row1.CreateCell(12).SetCellValue("应收合计");
            row1.CreateCell(13).SetCellValue("邮费");
            row1.CreateCell(14).SetCellValue("优惠金额");
            row1.CreateCell(15).SetCellValue("COD买家费用");
            row1.CreateCell(16).SetCellValue("仓库名称");
            row1.CreateCell(17).SetCellValue("物流公司");
            row1.CreateCell(18).SetCellValue("下单时间");
            row1.CreateCell(19).SetCellValue("付款时间");
            row1.CreateCell(20).SetCellValue("买家备注");
            row1.CreateCell(21).SetCellValue("客服备注");
            row1.CreateCell(22).SetCellValue("发票抬头");
            row1.CreateCell(23).SetCellValue("发票内容");
            row1.CreateCell(24).SetCellValue("支付方式");
            row1.CreateCell(25).SetCellValue("业务员");
            row1.CreateCell(26).SetCellValue("商家编码");
            row1.CreateCell(27).SetCellValue("货品数量");
            row1.CreateCell(28).SetCellValue("货品价格");
            row1.CreateCell(29).SetCellValue("货品总价");
            row1.CreateCell(30).SetCellValue("货品优惠");
            row1.CreateCell(31).SetCellValue("源子订单号");
            row1.CreateCell(32).SetCellValue("是否赠品");
            row1.CreateCell(33).SetCellValue("预计结账时间");
            row1.CreateCell(34).SetCellValue("备注");
            row1.CreateCell(35).SetCellValue("订单类别");
            row1.CreateCell(36).SetCellValue("证件号码");
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
            sheet1.SetColumnWidth(16, 25 * 256);
            sheet1.SetColumnWidth(17, 20 * 256);
            sheet1.SetColumnWidth(18, 20 * 256);
            sheet1.SetColumnWidth(19, 20 * 256);
            sheet1.SetColumnWidth(20, 20 * 256);
            sheet1.SetColumnWidth(21, 20 * 256);
            sheet1.SetColumnWidth(22, 20 * 256);
            sheet1.SetColumnWidth(23, 20 * 256);
            sheet1.SetColumnWidth(24, 20 * 256);
            sheet1.SetColumnWidth(25, 20 * 256);
            sheet1.SetColumnWidth(26, 20 * 256);
            sheet1.SetColumnWidth(27, 20 * 256);
            sheet1.SetColumnWidth(28, 20 * 256);
            sheet1.SetColumnWidth(29, 20 * 256);
            sheet1.SetColumnWidth(30, 20 * 256);
            sheet1.SetColumnWidth(31, 20 * 256);
            sheet1.SetColumnWidth(32, 20 * 256);
            sheet1.SetColumnWidth(33, 20 * 256);
            sheet1.SetColumnWidth(34, 20 * 256);
            sheet1.SetColumnWidth(35, 20 * 256);
            sheet1.SetColumnWidth(36, 20 * 256);
            for (int i = 0; i < listReissue.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName);
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName.ToString());
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].NewOrderCode) ? "" : listReissue[i].NewOrderCode.ToString());
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ReceivingName) ? "" : listReissue[i].ReceivingName.ToString());
                string[] AddressMessageList = listReissue[i].AddressMessage.ToString().Split('-');
                string province = "";
                if (AddressMessageList.Length >= 1)
                {
                    province = AddressMessageList[0];
                }
                string city = "";
                if (AddressMessageList.Length >= 2)
                {
                    city = AddressMessageList[1];
                }
                string area = "";
                if (AddressMessageList.Length >= 3)
                {
                    area = AddressMessageList[2];
                }
                rowtemp.CreateCell(3).SetCellValue(province);
                rowtemp.CreateCell(4).SetCellValue(city);
                rowtemp.CreateCell(5).SetCellValue(area);
                string phone = "";
                if(listReissue[i].TelPhone==null)
                {
                    phone = listReissue[i].Phone.ToString();

                }
                else {
                    phone = listReissue[i].TelPhone.ToString();
                }
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(phone) ? "" : phone);
                rowtemp.CreateCell(7).SetCellValue("");
                rowtemp.CreateCell(8).SetCellValue("");
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].VipName) ? "" : listReissue[i].VipName.ToString());
                rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].Address) ? "" : listReissue[i].Address.ToString());
                rowtemp.CreateCell(11).SetCellValue("款到发货");
                rowtemp.CreateCell(12).SetCellValue("0");
                rowtemp.CreateCell(13).SetCellValue("0");
                rowtemp.CreateCell(14).SetCellValue("0");
                rowtemp.CreateCell(15).SetCellValue("");
                rowtemp.CreateCell(16).SetCellValue("");
                rowtemp.CreateCell(17).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ExpressName) ? "" : listReissue[i].ExpressName.ToString());
                rowtemp.CreateCell(18).SetCellValue("");
                rowtemp.CreateCell(19).SetCellValue("");
                rowtemp.CreateCell(20).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].BuyRemark) ? "" : listReissue[i].BuyRemark.ToString());
                string SellerRemark = "";
                if (listReissue[i].SystemRemark != null && listReissue[i].SalesRemark != null)
                {
                    SellerRemark = "系统备注：" + listReissue[i].SystemRemark.ToString() + ",客服备注：" + listReissue[i].SalesRemark.ToString();
                }
                else if (listReissue[i].SalesRemark == null&& listReissue[i].SystemRemark != null)
                {
                    SellerRemark = "系统备注：" + listReissue[i].SystemRemark.ToString();
                }
                else if (listReissue[i].SystemRemark == null && listReissue[i].SalesRemark != null)
                {
                    SellerRemark = "客服备注：" + listReissue[i].SalesRemark.ToString();

                }
                rowtemp.CreateCell(21).SetCellValue(SellerRemark);
                rowtemp.CreateCell(22).SetCellValue("");
                rowtemp.CreateCell(23).SetCellValue("");
                rowtemp.CreateCell(24).SetCellValue("");
                rowtemp.CreateCell(25).SetCellValue("");
                rowtemp.CreateCell(26).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ProductCode) ? "" : listReissue[i].ProductCode.ToString());
                rowtemp.CreateCell(27).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].Num.ToString()) ? "" : listReissue[i].Num.ToString());
                rowtemp.CreateCell(28).SetCellValue("");
                rowtemp.CreateCell(29).SetCellValue("");
                rowtemp.CreateCell(30).SetCellValue("");
                rowtemp.CreateCell(31).SetCellValue("");
                rowtemp.CreateCell(32).SetCellValue("");
                rowtemp.CreateCell(33).SetCellValue("");
                rowtemp.CreateCell(34).SetCellValue("");
                string OrderType = "售后补发";
                if (listReissue[i].OrderType != null)
                {
                    OrderType = listReissue[i].OrderType.ToString();
                }
                rowtemp.CreateCell(35).SetCellValue(OrderType);
                rowtemp.CreateCell(36).SetCellValue("");
            }
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "补发数据.xls");
        }

        /// <summary>
        /// 导出补发excel至旺店通 第二种方法
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public ActionResult OutPutExcelEverywheres(string queryStr,string Repeat,string statedate,string EndDate,string names)
        {

            try
            {
                using (TransactionScope sc = new TransactionScope(TransactionScopeOption.Required,new TimeSpan(0, 10, 0)))
                {

                    //获取list数据
                    IQueryable<T_Reissue> queryData = null;
                    queryData = db.T_Reissue.Where(a => a.Status == 1 && a.ISReissue == "1").AsQueryable();
                    if (Repeat == "1")
                    {
                        queryData = queryData.Where(a => a.SystemRemark.Contains("重复"));
                    }
                    if (!string.IsNullOrEmpty(queryStr))
                    {
                        queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr) || a.OrderCode != null && a.OrderCode.Contains(queryStr) || a.VipName != null && a.VipName.Contains(queryStr));
                    }
                    if (!string.IsNullOrEmpty(names))
                    {
                        queryData = queryData.Where(a => a.ReissueName != null && a.ReissueName.Contains(names));
                    }
                    if (!string.IsNullOrWhiteSpace(statedate))
                    {
                        DateTime start = DateTime.Parse(statedate);
                        queryData = queryData.Where(a => a.ISReissueDate != null && a.ISReissueDate >= start);
                        //
                    }
                    else
                    {
                        DateTime start = DateTime.Now.AddDays(-1);
                        queryData = queryData.Where(a => a.ISReissueDate != null && a.ISReissueDate >= start);
                    }
                    if (!string.IsNullOrWhiteSpace(EndDate))
                    {
                        DateTime end = DateTime.Parse(EndDate);
                        queryData = queryData.Where(a => a.ISReissueDate != null && a.ISReissueDate <= end);
                    }
                    else
                    {
                        DateTime end = DateTime.Now;
                        queryData = queryData.Where(a => a.ISReissueDate != null && a.ISReissueDate <= end);
                    }

                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    int s = 0;
                    List<T_Reissue> List = queryData.ToList();
                    string shenheID = "";
                    for (int i = 0; i < List.Count; i++)
                    {
                        if (i == 0)
                        {
                            shenheID += "'" + List[i].ID + "'";
                        }
                        else
                        {
                            shenheID += "," + "'" + List[i].ID + "'";
                        }
                    }
                    
                    List<ReissueEverywhere> listReissue = db.Database.SqlQuery<ReissueEverywhere>("select *,(select ReceivingName from  T_Reissue where ID=d.ReissueId) ReceivingName,(select NewOrderCode from  T_Reissue where ID=d.ReissueId) NewOrderCode,(select StoreName from  T_Reissue where ID=d.ReissueId) StoreName,(select VipName from  T_Reissue where ID=d.ReissueId) VipName,(select AddressMessage from  T_Reissue where ID=d.ReissueId) AddressMessage,(select TelPhone from  T_Reissue where ID=d.ReissueId) TelPhone,(select Phone from  T_Reissue where ID=d.ReissueId) Phone,(select Address from  T_Reissue where ID=d.ReissueId) Address,(select VipCode from  T_Reissue where ID=d.ReissueId) VipCode,(select ExpressName from  T_Reissue where ID=d.ReissueId) ExpressName,(select OrderType from  T_Reissue where ID=d.ReissueId) OrderType,(select SalesRemark from  T_Reissue where ID=d.ReissueId) SalesRemark,(select BuyRemark from  T_Reissue where ID=d.ReissueId) BuyRemark,(select SystemRemark from  T_Reissue where ID=d.ReissueId) SystemRemark from T_ReissueDetail d    where ReissueId in (" + shenheID + ")").ToList();
                    #region 创建Excel文件的对象
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
                    row1.CreateCell(1).SetCellValue("原始单号");
                    row1.CreateCell(2).SetCellValue("收件人");
                    row1.CreateCell(3).SetCellValue("省");
                    row1.CreateCell(4).SetCellValue("市");
                    row1.CreateCell(5).SetCellValue("区");
                    row1.CreateCell(6).SetCellValue("手机");
                    row1.CreateCell(7).SetCellValue("固话");
                    row1.CreateCell(8).SetCellValue("邮编");
                    row1.CreateCell(9).SetCellValue("网名");
                    row1.CreateCell(10).SetCellValue("地址");
                    row1.CreateCell(11).SetCellValue("发货条件");
                    row1.CreateCell(12).SetCellValue("应收合计");
                    row1.CreateCell(13).SetCellValue("邮费");
                    row1.CreateCell(14).SetCellValue("优惠金额");
                    row1.CreateCell(15).SetCellValue("COD买家费用");
                    row1.CreateCell(16).SetCellValue("仓库名称");
                    row1.CreateCell(17).SetCellValue("物流公司");
                    row1.CreateCell(18).SetCellValue("下单时间");
                    row1.CreateCell(19).SetCellValue("付款时间");
                    row1.CreateCell(20).SetCellValue("买家备注");
                    row1.CreateCell(21).SetCellValue("客服备注");
                    row1.CreateCell(22).SetCellValue("发票抬头");
                    row1.CreateCell(23).SetCellValue("发票内容");
                    row1.CreateCell(24).SetCellValue("支付方式");
                    row1.CreateCell(25).SetCellValue("业务员");
                    row1.CreateCell(26).SetCellValue("商家编码");
                    row1.CreateCell(27).SetCellValue("货品数量");
                    row1.CreateCell(28).SetCellValue("货品价格");
                    row1.CreateCell(29).SetCellValue("货品总价");
                    row1.CreateCell(30).SetCellValue("货品优惠");
                    row1.CreateCell(31).SetCellValue("源子订单号");
                    row1.CreateCell(32).SetCellValue("是否赠品");
                    row1.CreateCell(33).SetCellValue("预计结账时间");
                    row1.CreateCell(34).SetCellValue("备注");
                    row1.CreateCell(35).SetCellValue("订单类别");
                    row1.CreateCell(36).SetCellValue("证件号码");
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
                    sheet1.SetColumnWidth(16, 25 * 256);
                    sheet1.SetColumnWidth(17, 20 * 256);
                    sheet1.SetColumnWidth(18, 20 * 256);
                    sheet1.SetColumnWidth(19, 20 * 256);
                    sheet1.SetColumnWidth(20, 20 * 256);
                    sheet1.SetColumnWidth(21, 20 * 256);
                    sheet1.SetColumnWidth(22, 20 * 256);
                    sheet1.SetColumnWidth(23, 20 * 256);
                    sheet1.SetColumnWidth(24, 20 * 256);
                    sheet1.SetColumnWidth(25, 20 * 256);
                    sheet1.SetColumnWidth(26, 20 * 256);
                    sheet1.SetColumnWidth(27, 20 * 256);
                    sheet1.SetColumnWidth(28, 20 * 256);
                    sheet1.SetColumnWidth(29, 20 * 256);
                    sheet1.SetColumnWidth(30, 20 * 256);
                    sheet1.SetColumnWidth(31, 20 * 256);
                    sheet1.SetColumnWidth(32, 20 * 256);
                    sheet1.SetColumnWidth(33, 20 * 256);
                    sheet1.SetColumnWidth(34, 20 * 256);
                    sheet1.SetColumnWidth(35, 20 * 256);
                    sheet1.SetColumnWidth(36, 20 * 256);
                    for (int i = 0; i < listReissue.Count; i++)
                    {
                        
                        int Rid = listReissue[i].ReissueId;
                        T_Reissue model = db.T_Reissue.SingleOrDefault(a => a.ID == Rid);
                        if (Repeat == "1")
                        {
                            model.ReissueName += Nickname + ",";
                        }
                        else
                        {
                            model.ISReissue = "1";
                            //model.ISReissueDate = DateTime.Now;
                            model.ReissueName += Nickname + ",";
                        }

                        db.Entry<T_Reissue>(model).State = System.Data.Entity.EntityState.Modified;



                        NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                        rowtemp.Height = 3 * 265;
                        rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName);
                        rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                        rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                        rowtemp.Cells[0].CellStyle.WrapText = true;
                        rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                        rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                        rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].StoreName) ? "" : listReissue[i].StoreName.ToString());
                        rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].NewOrderCode) ? "" : listReissue[i].NewOrderCode.ToString());
                        rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ReceivingName) ? "" : listReissue[i].ReceivingName.ToString());
                        string[] AddressMessageList = listReissue[i].AddressMessage.ToString().Split('-');
                        string province = "";
                        if (AddressMessageList.Length >= 1)
                        {
                            province = AddressMessageList[0];
                        }
                        string city = "";
                        if (AddressMessageList.Length >= 2)
                        {
                            city = AddressMessageList[1];
                        }
                        string area = "";
                        if (AddressMessageList.Length >= 3)
                        {
                            area = AddressMessageList[2];
                        }
                        rowtemp.CreateCell(3).SetCellValue(province);
                        rowtemp.CreateCell(4).SetCellValue(city);
                        rowtemp.CreateCell(5).SetCellValue(area);
                        string phone = "";
                        if (listReissue[i].TelPhone == null)
                        {
                            phone = listReissue[i].Phone.ToString();

                        }
                        else
                        {
                            phone = listReissue[i].TelPhone.ToString();
                        }
                        rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(phone) ? "" : phone);
                        rowtemp.CreateCell(7).SetCellValue("");
                        rowtemp.CreateCell(8).SetCellValue("");
                        rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].VipName) ? "" : listReissue[i].VipName.ToString());
                        rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].Address) ? "" : listReissue[i].Address.ToString());
                        rowtemp.CreateCell(11).SetCellValue("款到发货");
                        rowtemp.CreateCell(12).SetCellValue("0");
                        rowtemp.CreateCell(13).SetCellValue("0");
                        rowtemp.CreateCell(14).SetCellValue("0");
                        rowtemp.CreateCell(15).SetCellValue("");
                        rowtemp.CreateCell(16).SetCellValue("");
                        rowtemp.CreateCell(17).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ExpressName) ? "" : listReissue[i].ExpressName.ToString());
                        rowtemp.CreateCell(18).SetCellValue("");
                        rowtemp.CreateCell(19).SetCellValue("");
                        rowtemp.CreateCell(20).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].BuyRemark) ? "" : listReissue[i].BuyRemark.ToString());
                        string SellerRemark = "";
                        if (listReissue[i].SystemRemark != null && listReissue[i].SalesRemark != null)
                        {
                            SellerRemark = "系统备注：" + listReissue[i].SystemRemark.ToString() + ",客服备注：" + listReissue[i].SalesRemark.ToString();
                        }
                        else if (listReissue[i].SalesRemark == null && listReissue[i].SystemRemark != null)
                        {
                            SellerRemark = "系统备注：" + listReissue[i].SystemRemark.ToString();
                        }
                        else if (listReissue[i].SystemRemark == null && listReissue[i].SalesRemark != null)
                        {
                            SellerRemark = "客服备注：" + listReissue[i].SalesRemark.ToString();

                        }
                        rowtemp.CreateCell(21).SetCellValue(SellerRemark);
                        rowtemp.CreateCell(22).SetCellValue("");
                        rowtemp.CreateCell(23).SetCellValue("");
                        rowtemp.CreateCell(24).SetCellValue("");
                        rowtemp.CreateCell(25).SetCellValue("");
                        rowtemp.CreateCell(26).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].ProductCode) ? "" : listReissue[i].ProductCode.ToString());
                        rowtemp.CreateCell(27).SetCellValue(string.IsNullOrWhiteSpace(listReissue[i].Num.ToString()) ? "" : listReissue[i].Num.ToString());
                        rowtemp.CreateCell(28).SetCellValue("");
                        rowtemp.CreateCell(29).SetCellValue("");
                        rowtemp.CreateCell(30).SetCellValue("");
                        rowtemp.CreateCell(31).SetCellValue("");
                        rowtemp.CreateCell(32).SetCellValue("");
                        rowtemp.CreateCell(33).SetCellValue("");
                        rowtemp.CreateCell(34).SetCellValue("");
                        string OrderType = "售后补发";
                        if (listReissue[i].OrderType != null)
                        {
                            OrderType = listReissue[i].OrderType.ToString();
                        }
                        rowtemp.CreateCell(35).SetCellValue(OrderType);
                        rowtemp.CreateCell(36).SetCellValue("");
                    }
                    #endregion

 
                    db.SaveChanges();
                    sc.Complete();
                    Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                    // 写入到客户端 
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();

                    book.Write(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Flush();
                    ms.Position = 0;
                    return File(ms, "application/vnd.ms-excel", "补发数据.xls");
                }


            }
            catch (Exception e)
            {
                return Json(e.Message,JsonRequestBehavior.AllowGet);
            }

           
        }


        //已导出数据再导出
        [HttpPost]
        public ContentResult GetExportedEverywhereList(Lib.GridPager pager, string name, string Repeat, string statedate, string EndDate, string names)
        {
            IQueryable<T_Reissue> reissueList = db.T_Reissue.Where(s => s.Status == 1&&s.ISReissue=="1").AsQueryable();
            //IQueryable<T_Reissue> reissueList = db.Database.SqlQuery<T_Reissue>("select * from T_Reissue where Status=1 and ISReissue=1 ").AsQueryable();
            if (Repeat == "1")
            {
                reissueList = reissueList.Where(s => s.SystemRemark.Contains("重复"));
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                reissueList = reissueList.Where(s => s.PostUser.Contains(name) || s.OrderCode.Contains(name) || s.VipName.Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(names))
            {
                reissueList = reissueList.Where(s => s.ReissueName.Contains(names));
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate);
                reissueList = reissueList.Where(s => s.ISReissueDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate);
                reissueList = reissueList.Where(s => s.ISReissueDate <= end);
            }
            pager.totalRows = reissueList.Count();
            reissueList = reissueList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Reissue> lists = new List<T_Reissue>();
            foreach (var item in reissueList)
            {
                T_Reissue model = new T_Reissue();
                model = item;
                model.WarhouseName = Com.GetWarehouseName(model.WarhouseName);
                lists.Add(model);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

    }
}
