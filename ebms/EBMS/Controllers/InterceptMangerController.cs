using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
    public class InterceptMangerController : BaseController
    {

        #region 公共属性/字段/方法

        EBMSEntities db = new EBMSEntities();
        //绑定原因
        public List<SelectListItem> GetInterceptReasonList()
        {


            var list = db.T_InterceptReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
         //绑定快递记录无默认值
        public List<SelectListItem> GetExpressTypeList()
        {
            List<SelectListItem> ExpressType = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==可选择处理结果==", Value = "" },
                 new SelectListItem { Text = "未处理", Value = "wcl"},
                 new SelectListItem { Text = "未达到顾客地址", Value = "未达到顾客地址"},
                 new SelectListItem { Text = "已有返回物流信息", Value = "已有返回物流信息"},
                 new SelectListItem { Text = "问题件", Value = "问题件"},
                 new SelectListItem { Text = "已内网留言", Value = "已内网留言" },
                 new SelectListItem { Text = "已做退回标签", Value = "已做退回标签" },
                 new SelectListItem { Text = "已投诉仲裁", Value = "已投诉仲裁" },
                 new SelectListItem { Text = "修改成功", Value = "修改成功" }
            };
            return ExpressType;
        }
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
        public JsonResult GetInterceptByGy(string code)
        {


            GY gy = new GY();

            
            
            



                 string repeat = "";


                 List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0" && a.Status != 2).ToList();
                 if (modelList.Count > 0)
                 {

                     repeat += "退货退款记录重复，";
                 }
                 //查是否有返现记录

                 List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 2).ToList();
                 if (cash.Count > 0)
                 {
                     repeat += "有返现记录重复，";
                 }
                 List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
                 if (Reissue.Count > 0)
                 {
                     repeat += "有补发记录重复，";
                 }
                 List<T_ExchangeCenter> ExchangeCenter = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
                 if (ExchangeCenter.Count > 0)
                 {
                     repeat += "有换货记录重复，";
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
                    //快递单号
                    string logistics_no = trades["logistics_no"].ToString();
                    //买家留言
                    string buyer_message = trades["buyer_message"].ToString();

                    //快递公司名称
                    string logistics_name = trades["logistics_name"].ToString();
                    //客服备注
                    string cs_remark = trades["cs_remark"].ToString();
                    //实付金额
                    string paid = trades["paid"].ToString();
                    //商品详情
                    //JsonData goods_list = trades["goods_list"];
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
                    string shop_Code = "";
                    if (shop_name != null)
                    {
                        T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
                        shop_Code = commonarea.shop_no;
                      //  shop_Code = "tm004";
                    }

                    T_Intercept intercept = new T_Intercept
                    {
                        OrderNumber = code,
                        Receiver = receiver_name,
                        Address = receiver_address,
                        Phone = receiver_mobile,
                        TelPhone = receiver_mobile,
                        Postalcode = receiver_zip,
                        AddressMessage = receiver_area,
                        MailNo = logistics_no,
                        ExpressName = logistics_code,
                        Warhouse = warehouse_no,
                        LoadWarhouse = "长沙电商总仓",
                        LoadAddress = receiver_address,
                        LoadExpressName = logistics_name,
                        VipCode = customer_name,
                        VipName = receiver_name,
                        StoreCode = shop_Code,
                        Memo = repeat,
                        SingleTime = string.IsNullOrWhiteSpace(trade_time) ? DateTime.Now.ToString() : trade_time,
                    };
                    List<T_InterceptDetail> DetailsList = new List<T_InterceptDetail>();

                    for (int c = 0; c < jsontrades.Count; c++)
                    {
                       // paid += double.Parse(jsontrades[c]["paid"].ToString());
                        JsonData goods_list = jsontrades[c]["goods_list"];
                        for (int i = 0; i < goods_list.Count; i++)
                    {
                        T_InterceptDetail DetailsModel = new T_InterceptDetail();
                        DetailsModel.Code = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                        DetailsModel.Name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                        decimal qyt = decimal.Parse(goods_list[i]["actual_num"].ToString());
                        DetailsModel.Num = int.Parse(Math.Round(qyt).ToString());
                        DetailsModel.LoadCode = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                        DetailsModel.LoadName = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                        DetailsModel.LoadNum = int.Parse(Math.Round(qyt).ToString());
                        DetailsList.Add(DetailsModel);
                    }
                    }
                    var json = new
                    {

                        rows = (from r in DetailsList
                                select new T_InterceptDetail
                                {
                                    Code = r.Code,
                                    Name = r.Name,
                                    Num = r.Num
                                }).ToArray()
                    };
                    T_Intercept model = db.T_Intercept.FirstOrDefault(s => s.OrderNumber.Equals(code) && s.IsDelete == 0);
                    if (model != null)
                    {
                        return Json(new { State = "Warring", ModelList = intercept, Json = json });
                    }
                    else
                    {
                        return Json(new { State = "Success", ModelList = intercept, Json = json });
                    }
                    
                     }

                 }



            return Json("-2", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交管易
        /// </summary>
        /// <returns></returns>
        public string PostGy(T_Intercept model)
        {
            string shangp = "";
            List<T_InterceptDetail> commodel = db.T_InterceptDetail.Where(a => a.InterceptId == model.ID && a.Code != null && a.Code != "").ToList();
            for (int i = 0; i < commodel.Count; i++)
            {
                if (i == commodel.Count - 1)
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].Code + "\"}";
                }
                else
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].Code + "\"},";
                }
            }


            string datetime = model.SingleTime;
            string sellerremarks = "";
            if (!string.IsNullOrWhiteSpace(model.Memo))
            {
                sellerremarks = "系统备注:" + model.SystemRemark + "制单人:" + model.PostUSer + Regex.Replace(model.Memo.ToUpper().Replace((char)32, ' ').Replace((char)13, ' ').Replace((char)10, ' '), "[ \\[ \\] \\^ \t \\-×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；;\"‘’“”-]", "☆").Replace("☆", "").Replace(" ", "");
            }
            else
            {
                sellerremarks = "系统备注:" + model.SystemRemark + "制单人:" + model.PostUSer;
            }

            string[] address = model.AddressMessage.Split('-');
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


            GY gy = new GY();
            string cmd = "";
            string type = "Sales";
            cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.trade.add\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                        "\"platform_code\":\"" + model.NewOrderNumber + "\"," +
                        "\"order_type_code\":\"" + type + "\"," +
                        "\"shop_code\":\"" + model.StoreCode + "\"," +
                        "\"express_code\":\"" + model.ExpressName + "\"," +
                            "\"receiver_province\":\"" + receiver_province + "\"," +
                                "\"receiver_city\":\"" + receiver_city + "\"," +
                                    "\"receiver_district\":\"" + receiver_district + "\"," +
                        "\"warehouse_code\":\"" + model.Warhouse + "\"," +
                        "\"vip_code\":\"" + model.VipCode + "\"," +
                        "\"vip_name\":\"" + model.VipName + "\"," +
                        "\"receiver_name\":\"" + model.Receiver + "\"," +
                        "\"receiver_address\":\"" + model.Address + "\"," +
                        "\"receiver_zip\":\"" + model.Postalcode + "\"," +
                        "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                        "\"receiver_phone\":\"" + model.Phone + "\"," +
                        "\"deal_datetime\":\"" + datetime + "\"," +
                        "\"seller_memo\":\"" + sellerremarks + "\"," +
                           "\"business_man_code\":\"" + model.PostUSer + "\"," +
                          "\"details\":[" + shangp + "]" +
                        "}";
            string sign = gy.Sign(cmd);
            string comcode = "{" +
                    "\"appkey\":\"171736\"," +
                    "\"method\":\"gy.erp.trade.add\"," +
                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                     "\"platform_code\":\"" + model.NewOrderNumber + "\"," +
                    "\"order_type_code\":\"" + type + "\"," +
                    "\"shop_code\":\"" + model.StoreCode + "\"," +
                    "\"express_code\":\"" + model.ExpressName + "\"," +
                    "\"receiver_province\":\"" + receiver_province + "\"," +
                    "\"receiver_city\":\"" + receiver_city + "\"," +
                    "\"receiver_district\":\"" + receiver_district + "\"," +
                    "\"warehouse_code\":\"" + model.Warhouse + "\"," +
                    "\"vip_code\":\"" + model.VipCode + "\"," +
                    "\"vip_name\":\"" + model.VipName + "\"," +
                    "\"receiver_name\":\"" + model.Receiver + "\"," +
                    "\"receiver_address\":\"" + model.Address + "\"," +
                    "\"receiver_zip\":\"" + model.Postalcode + "\"," +
                    "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                    "\"receiver_phone\":\"" + model.Phone + "\"," +
                    "\"deal_datetime\":\"" + datetime + "\"," +
                    "\"sign\":\"" + sign + "\"," +
                    "\"seller_memo\":\"" + sellerremarks + "\"," +
                 "\"business_man_code\":\"" + model.PostUSer + "\"," +
                    "\"details\":[" + shangp + "]" +
                    "}";
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string sd = jsonData[0].ToString();
            return sd;
        }

        #endregion
        /// <summary>
        /// 绑定原因
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> RetreatReason()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_InterceptReson.AsQueryable();
            var selectList = new SelectList(list, "Reson", "Reson");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        #region 视图

        [Description("拦截列表")]
        public ActionResult ViewInterceptList()
        {
            ViewData["RetreatReason"] = RetreatReason();
            return View();
        }

        [Description("拦截新增订单")]
        public ActionResult ViewInterceptOrderAdd()
        {
            return View();
        }

        [Description("拦截新增")]
        public ActionResult ViewInterceptAdd()
        {

            //T_OrderList order = db.T_OrderList.SingleOrDefault(s => s.platform_code.Equals(orderNum));
            //T_Intercept intercept = new T_Intercept
            //{
            //    OrderNumber = order.platform_code,
            //    Receiver = order.receiver_name,
            //    Address = order.receiver_address,
            //    Phone = order.receiver_phone,
            //    TelPhone = order.receiver_mobile,
            //    Postalcode = order.receiver_zip,
            //    MailNo = order.mail_no
            //};
            ViewData["express"] = Com.ExpressName();
            //string[] address = order.receiver_area.Split('-');
            //ViewData["province"] = address[0];
            //ViewData["city"] = address[1];
            //if (address.Count() > 2)
            //    ViewData["distrct"] = address[2];
            //else
            //    ViewData["distrct"] = "";
            ViewData["war"] = Com.Warehouses();
            ViewData["re"] = Com.InterceptReson();
            //ViewData["code"] = order.code;
            return View();
        }


        [Description("拦截编辑")]
        public ActionResult ViewInterceptEdit(int interceptId)
        {


            if (interceptId == 0)
                return HttpNotFound();
            T_Intercept model = db.T_Intercept.Find(interceptId);
            ViewData["express"] = Com.ExpressName(model.ExpressName);
            //string[] address = model.AddressMessage.Split('-');
            //ViewData["province"] = address[0];
            //ViewData["city"] = address[1];
            //if (address.Count() > 2)
            //    ViewData["distrct"] = address[2];
            //else
            //    ViewData["distrct"] = "";
            ViewData["war"] = Com.Warehouses(model.Warhouse);
            ViewData["re"] = Com.InterceptReson();
            return View(model);
        }

        [Description("未发货拦截详情")]
        public ActionResult ViewInterceptDetail(int interceptId)
        {
            if (interceptId == 0)
                return HttpNotFound();
            var history = db.T_InterceptApprove.Where(a => a.Pid == interceptId);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveUser, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["interceptId"] = interceptId;
            return View();
        }

        [Description("我的未发货拦截")]
        public ActionResult ViewInterceptForMy()
        {
            return View();
        }

        [Description("审核页面")]
        public ActionResult ViewInterceptApprove(int id)
        {
            var model = db.T_Intercept.Find(id);
            if (model == null)
                return HttpNotFound();
            var history = db.T_InterceptApprove.Where(a => a.Pid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveUser, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            //获取审核表中的 审核记录ID
            T_InterceptApprove approve = db.T_InterceptApprove.FirstOrDefault(a => !a.ApproveTime.HasValue && a.Pid == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
                ViewData["approveid"] = 0;
            model.Warhouse = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.Warhouse)).name;
            model.ExpressName = db.T_Express.SingleOrDefault(s => s.Code.Equals(model.ExpressName)).Name;
            int reson = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(model.Reson)).Type;
            ViewData["Step"] = db.T_InterceptApproveConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == reson).Step;
            return View(model);
        }

        [Description("拦截未审核")]
        public ActionResult ViewInterceptNotCheck()
        {
            //得到是谁进来
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //得到是否快递组
            T_InterceptGroup ExpressRecordModel = db.T_InterceptGroup.SingleOrDefault(a => a.GroupUser.Contains(Nickname) && a.GroupName == "快递组");
            if (ExpressRecordModel != null)
            {
                ViewData["ExpressRecord"] = ExpressRecordModel.GroupName;
            }
            else
            {
                ViewData["ExpressRecord"] = "";
            }

            ViewData["ReasonType"] = GetInterceptReasonList();
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouse();
            ViewData["ExpressType"] = GetExpressTypeList();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
            
        }

        [Description("拦截已审核")]
        public ActionResult ViewInterceptChecked()
        {
            return View();
        }

        //[Description("需发产品")]
        //public ActionResult ViewProduct(int index)
        //{
        //    ViewData["index"] = index;
        //    return View();
        //}

        [Description("手工修改")]
        public ActionResult ViewInterceptUpdateAddress()
        {
            return View();
        }
        [Description("快递记录")]
        public ActionResult InterceptJilu(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("快递记录新增")]
        public ActionResult InterceptJiluAdd(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("快递记录新增保存")]
        public JsonResult JiluSave(T_InterceptExpressRecord model, string id, string selected_val)
        {
            int ID = int.Parse(id);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            model.TrackRecord_Name = name;
            model.TrackRecord_Date = DateTime.Now;
            model.Oid = ID;
            model.TrackRecord_Situation = selected_val;
            db.T_InterceptExpressRecord.Add(model);

            int i = db.SaveChanges();


            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //[Description("快递记录删除")]
        //public JsonResult JiluDel(int ID)
        //{
        //    T_InterceptExpressRecord editModel = db.T_InterceptExpressRecord.Find(ID);
        //    db.T_InterceptExpressRecord.Remove(editModel);
        //    try
        //    {
        //        db.SaveChanges();
        //        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [Description("快递记录数据查询")]
        public ContentResult GetInterceptJiluList(Lib.GridPager pager, string ID)
        {
            int id = int.Parse(ID);
            IQueryable<T_InterceptExpressRecord> queryData = db.T_InterceptExpressRecord.Where(a => a.Oid == id);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_InterceptExpressRecord> list = new List<T_InterceptExpressRecord>();
            foreach (var item in queryData)
            {
                T_InterceptExpressRecord i = new T_InterceptExpressRecord();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);


        }

        #endregion

        #region Post提交


        /// <summary>
        /// 获取需发产品
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ContentResult GetViewProductList(string code, Lib.GridPager pager)
        //{
        //    IQueryable<T_goodsGY> modellist = db.T_goodsGY.Where(s => s.code.StartsWith(code) || s.name.StartsWith(code));
        //    pager.totalRows = modellist.Count();
        //    List<T_goodsGY> querData = modellist.OrderBy(s => s.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
        //    string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
        //    return Content(json);
        //}


        /// <summary>
        /// 根据原因选择审核人员
        /// </summary>
        /// <param name="reson"></param>
        /// <returns></returns>
        public JsonResult GetApproveByReson(string reson)
        {
            if (!string.IsNullOrWhiteSpace(reson))
            {
                int type = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(reson)).Type;
                string approveType = db.T_InterceptApproveConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type).ApproveType;
                string approveUser = db.T_InterceptGroup.SingleOrDefault(s => s.GroupName.Equals(approveType)).GroupUser;
                Dictionary<string, string> list = new Dictionary<string, string>();
                foreach (var item in approveUser.Split(','))
                {
                    list.Add(item, item);
                }
                return Json(list.ToArray());
            }
            return Json(null);
        }

        /// <summary>
        /// 根据原因选择审核人员
        /// </summary>
        /// <param name="reson"></param>
        /// <returns></returns>
        public JsonResult GetApproveByResons(string reson, int step)
        {
            if (!string.IsNullOrWhiteSpace(reson))
            {
                int type = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(reson)).Type;
                string approveType = db.T_InterceptApproveConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > step).ApproveType;
                string approveUser = db.T_InterceptGroup.SingleOrDefault(s => s.GroupName.Equals(approveType)).GroupUser;
                Dictionary<string, string> list = new Dictionary<string, string>();
                foreach (var item in approveUser.Split(','))
                {
                    list.Add(item, item);
                }
                return Json(list.ToArray());
            }
            return Json(null);
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ContentResult GetViewInterceptOrderAdd(Lib.GridPager pager, string code)
        {
            IQueryable<T_OrderList> list = db.T_OrderList.Where(s => s.platform_code.Contains(code)).AsQueryable();
            pager.totalRows = list.Count();

            List<T_OrderList> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 拦截列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public ContentResult GetViewInterceptList(Lib.GridPager pager, string code, string startDate, string endDate, string RetreatReason, int status = -2)
        {
            IQueryable<T_Intercept> list = db.T_Intercept.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderNumber.Contains(code)|| s.MailNo != null && s.MailNo.Contains(code)|| s.VipName != null && s.VipName.Contains(code) || s.VipCode != null && s.VipCode.Contains(code) || s.PostUSer != null && s.PostUSer.Contains(code));
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
            if (!string.IsNullOrWhiteSpace(RetreatReason))
            {
                list = list.Where(s => s.Reson == RetreatReason);
            }
            pager.totalRows = list.Count();
            List<T_Intercept> lists = new List<T_Intercept>();
            foreach (var item in list)
            {
                T_Intercept model = new T_Intercept();
                model = item;
           //   model.Warhouse = Com.GetWarehouseName(model.Warhouse);
               // model.ExpressName = Com.GetExpressName(model.ExpressName);
                lists.Add(model);
            }
            List<T_Intercept> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IntercepotDelete(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_Intercept model = db.T_Intercept.Find(id);
                    model.IsDelete = 1;
                    db.SaveChanges();
                  //  ModularByZP();
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
        /// 我的拦截
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetInterceptForMy(Lib.GridPager pager, string code, int status = -2)
        {
            IQueryable<T_Intercept> list = db.T_Intercept.Where(s => s.PostUSer.Equals(UserModel.Nickname) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.OrderNumber.Equals(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            pager.totalRows = list.Count();
            List<T_Intercept> lists = new List<T_Intercept>();
            foreach (var item in list)
            {
                T_Intercept model = new T_Intercept();
                model = item;
                model.Warhouse = Com.GetWarehouseName(model.Warhouse);
                model.ExpressName = Com.GetExpressName(model.ExpressName);
                lists.Add(model);
            }
            List<T_Intercept> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="exchangeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetInterceptDetail(int interceptId)
        {
            IQueryable<T_InterceptDetail> list = db.T_InterceptDetail.Where(s => s.InterceptId == interceptId).AsQueryable();
            List<T_InterceptDetail> querData = list.ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 新增保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult InterceptAddSave(T_Intercept model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "详情不能为空" });
                    //if (!model.AddressMessage.Contains("-"))
                    //    return Json(new { State = "Faile", Message = "地址信息请以-区分" });
                    //string provin = province;
                    //string cit = city;
                    //string distri = district;
                    //if (hidden == true)
                    //{
                    //    if (provin != "香港" && provin != "澳门" && provin != "台湾省")
                    //        provin = provin.Split('市').First();
                    //    else
                    //        provin = province;
                    //    provin = provin.Split('市').First();
                    //    cit = province;
                    //    distri = city;
                    //}
                    List<T_InterceptDetail> details = Com.Deserialize<T_InterceptDetail>(jsonStr);
                    model.PostUSer = UserModel.Nickname;
                    model.CreateDate = DateTime.Now;
                    model.IsDelete = 0;
                    model.Step = 0;  
                    model.Status = -1;
                    T_InterceptReson reson = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(model.Reson));
                    T_InterceptApproveConfig cofig = db.T_InterceptApproveConfig.SingleOrDefault(s => s.Reson == reson.Type && s.Step == 0);
                    if (model.Reson == "快递赔付")
                    { 
                    
                    }
                    model.NextApproveName = cofig.ApproveType;
                    model.NewOrderNumber = "8" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    //if (distri == "请选择")
                    //    model.AddressMessage = provin + "-" + cit;
                    //else
                    //    model.AddressMessage = provin + "-" + cit + "-" + distri;
                    db.T_Intercept.Add(model);
                    db.SaveChanges();
                    foreach (var item in details)
                    {
                        item.InterceptId = model.ID;
                        db.T_InterceptDetail.Add(item);
                    }
					T_ReturnToStorage ReturnToStorage = db.T_ReturnToStorage.FirstOrDefault(a => a.Retreat_expressNumber == model.MailNo && a.IsDelete == 0);
					if (ReturnToStorage != null)
					{
						if (ReturnToStorage.ModularName == "无")
						{
							ReturnToStorage.ModularName = "拦截";
						}
						else if (!ReturnToStorage.ModularName.Contains("拦截"))
						{
							ReturnToStorage.ModularName = ReturnToStorage.ModularName+"拦截" ;
						}
						db.Entry<T_ReturnToStorage>(ReturnToStorage).State = System.Data.Entity.EntityState.Modified;
						db.SaveChanges();
					}

					T_InterceptApprove approve = new T_InterceptApprove();
                    approve.ApproveName = cofig.ApproveType;
                    approve.ApproveUser = cofig.ApproveType;
                    approve.Pid = model.ID;
                    approve.ApproveStatus = -1;
                    approve.Memo = "";
                    db.T_InterceptApprove.Add(approve);
                    db.SaveChanges();
                  //  ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }

        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }

        public void ModularByZP()
        {
            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_InterceptApprove where Pid in (select ID from T_Intercept where  Isdelete='0'  and (Status = -1 or Status = 0) )  and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "拦截" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "拦截";
                    ModularNotauditedModel.RejectNumber = 0;
                    ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    ModularNotauditedModel.ToupdateName = Nickname;
                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
                }
                db.SaveChanges();
            }
        }

        public string Tz(T_Intercept model, T_InterceptDetail detail)
        {
            //   上传管易。调整单新增
            int? qtyGY = detail.LoadNum;
            string cpcode = "{\"item_code\":\"" + detail.LoadCode + "\",\"qty\":" + qtyGY + "}";
            string WarehouseCode = db.T_Warehouses.SingleOrDefault(s => s.name.Equals(model.LoadWarhouse)).code;
            EBMS.App_Code.GY gy = new App_Code.GY();
            string cmd = "";
            cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.stock.adjust.add\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                             "\"order_type\":\"001\"," +
                              "\"note\":\"原平台单号:" + model.OrderNumber + "来自拦截\"," +
                               "\"warehouse_code\":\"" + WarehouseCode + "\"," +
                          "\"detail_list\":[" + cpcode + "]" +
                        "}";
            string sign = gy.Sign(cmd);
            string comcode = "";
            comcode = "{" +
                     "\"appkey\":\"171736\"," +
                     "\"method\":\"gy.erp.stock.adjust.add\"," +
                     "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                        "\"sign\":\"" + sign + "\"," +
                          "\"order_type\":\"001\"," +
                          "\"note\":\"原平台单号:" + model.OrderNumber + "来自拦截\"," +
                            "\"warehouse_code\":\"" + WarehouseCode + "\"," +
                       "\"detail_list\":[" + cpcode + "]" +
                     "}";
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);

            return ret;
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="approveID"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public JsonResult Check(int approveID, int status, string memo, string NextApprove)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {


                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    List<T_InterceptGroup> RetreatGroupList = db.Database.SqlQuery<T_InterceptGroup>("select  * from T_InterceptGroup where GroupUser like '%" + Nickname + "%'  or   GroupUser like '" + curName + "'").ToList();
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
                    string sql = "select * from T_InterceptApprove where ID='" + approveID + "' and ApproveTime is null ";
                    if (GroupName != "" && GroupName != null)
                    {
                        sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                    }
                    else
                    {
                        sql += "    and ApproveName='" + Nickname + "'  ";
                    }
                    List<T_InterceptApprove> approves = db.Database.SqlQuery<T_InterceptApprove>(sql).ToList();
                    if (approves.Count == 0)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" }, JsonRequestBehavior.AllowGet);
                    }
                    T_InterceptApprove approve = db.T_InterceptApprove.SingleOrDefault(a => a.ID == approveID);
                    int TotalStep = db.T_ExchangeCenterConfig.ToList().Count;
                 
                    T_Intercept model = db.T_Intercept.Find(approve.Pid);
                    approve.ApproveUser = UserModel.Nickname;
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
                        int type = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(model.Reson)).Type;
                        int LastStep = db.T_InterceptApproveConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
                        if (LastStep > model.Step)//判断是否存在下一级
                        {
                            //获得下一级审核部门
                            string nextapproveType = db.T_InterceptApproveConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
                            T_InterceptApprove newApprove = new T_InterceptApprove();
                            newApprove.ApproveStatus = -1;
                            newApprove.ApproveName = nextapproveType;
                            newApprove.ApproveUser = nextapproveType;
                            //if (nextapproveType == "部门主管")
                            //{
                            //    int departid = int.Parse(UserModel.DepartmentId);
                            //    T_Department depart = db.T_Department.Find(departid);
                            //    if (depart.supervisor == null)
                            //        newApprove.ApproveUser = "成风";
                            //    else
                            //        newApprove.ApproveUser = db.T_User.Find(depart.supervisor).Nickname;
                            //}
                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_InterceptApprove.Add(newApprove);
                            db.SaveChanges();
                            model.Status = 0;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }
                        else
                        {
                            model.Status = status;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }

                        #region 判断仓库是否收货

                        if (model.Reson.Equals("快递拦截") && approve.ApproveName.Equals("快递组"))
                        {
                            T_ReturnToStorage storge = db.T_ReturnToStorage.SingleOrDefault(s => s.Retreat_expressNumber.Equals(model.MailNo) && s.isSorting == 1);
                            if (storge != null)
                            {
                                string exNumber = model.MailNo;
                                int modelID = model.ID;
                                List<T_Intercept> RetreatList = db.T_Intercept.Where(a => a.MailNo == exNumber && a.ID == modelID).ToList();
                                T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber);
                                if (Tostorage != null)
                                {
                                    List<T_InterceptDetail> RetreatDetailsList = db.T_InterceptDetail.Where(a => a.InterceptId == modelID).ToList();
                                    for (int x = 0; x < RetreatDetailsList.Count; x++)
                                    {
                                        string itemCode = RetreatDetailsList[x].Code;
                                        int TostorageID = Tostorage.ID;
                                        List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                                        if (ReturnToStorageletList.Count == 0)
                                        {
                                            List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == exNumber).ToList();
                                            if (ReceivedAfterList.Count == 0)
                                            {
                                                T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                ReceivedAfterModel.Type = "拦截";
                                                ReceivedAfterModel.OrderNumber = RetreatList[0].OrderNumber;
                                                ReceivedAfterModel.ShopCode = RetreatList[0].StoreCode; ;
                                                ReceivedAfterModel.CustomerCode = RetreatList[0].VipCode;
                                                ReceivedAfterModel.CustomerName = RetreatList[0].VipName;
                                                ReceivedAfterModel.CollectExpressName = RetreatList[0].ExpressName;
                                                ReceivedAfterModel.CreatTime = DateTime.Now;
                                                ReceivedAfterModel.ProductCode = RetreatDetailsList[x].Code;
                                                ReceivedAfterModel.ProductName = RetreatDetailsList[x].Name;
                                                ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].Num;
                                                ReceivedAfterModel.IsHandle = 0;
                                                db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                model.Status = 1;
                                T_InterceptApprove approve1 = db.T_InterceptApprove.SingleOrDefault(s => s.ApproveName.Equals("仓库") && !s.ApproveTime.HasValue && s.Pid == model.ID);
                                approve1.ApproveUser = "仓库";
                                approve1.ApproveStatus = 1;
                                approve1.ApproveTime = DateTime.Now;
                            }
                        }
                        #endregion

                        #region 进管易

                        if (model.Reson != "仅作废不进管易" && approve.ApproveName.Equals("审单组") && db.T_InterceptApprove.Where(s => s.Pid == model.ID && s.ApproveName.Equals("审单组") && s.ApproveStatus == 1).Count() == 1)
                        {

                            //if (PostGy(model) != "True")
                            //    return Json(new { State = "Faile", Message = "上传管易错误,请联系管理员" }, JsonRequestBehavior.AllowGet);
                            //if (model.Reson.Equals("换仓库发货") || model.Reson.Equals("换发货单") || model.Reson.Equals("缺货换产品"))//调整单新增
                            //{
                            //    foreach (var item in db.T_InterceptDetail.Where(s => s.InterceptId == model.ID))
                            //    {
                            //        Tz(model, item);
                            //    }
                            //}
                        }

                        #endregion

                    }
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
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="jsonStr"></param>
        /// <param name="hidden"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="district"></param>
        /// <returns></returns>
        public JsonResult InterceptEditSave(T_Intercept model, string jsonStr, string nextApprove)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "详情不能为空" });
                    if (!model.AddressMessage.Contains("-"))
                        return Json(new { State = "Faile", Message = "地址信息请以-区分" });
                    List<T_InterceptDetail> details = Com.Deserialize<T_InterceptDetail>(jsonStr);
                    //string provin = province;
                    //string cit = city;
                    //string distri = district;
                    //if (hidden == true)
                    //{
                    //    if (provin != "香港" && provin != "澳门" && provin != "台湾省")
                    //        provin = provin.Split('市').First();
                    //    else
                    //        provin = province;
                    //    provin = provin.Split('市').First();
                    //    cit = province;
                    //    distri = city;
                    //}
                    T_InterceptReson reson = db.T_InterceptReson.SingleOrDefault(s => s.Reson.Equals(model.Reson));
                    T_InterceptApproveConfig cofig = db.T_InterceptApproveConfig.SingleOrDefault(s => s.Reson == reson.Type && s.Step == 0);
                    T_Intercept editModel = db.T_Intercept.Find(model.ID);
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    string Reson = model.Reson;//记录原来的原因
                    T_Intercept PurMod = db.T_Intercept.Find(editID);

                    if (PurMod.Status != -1 && PurMod.Status != 2)
                    {
                        return Json(new { State = "Faile", Message = "该记录已经审核，不允许修改" }, JsonRequestBehavior.AllowGet);
                    }

                    editModel.Receiver = model.Receiver;
                    editModel.ExpressName = model.ExpressName;
                    editModel.TelPhone = model.TelPhone;
                    editModel.Address = model.Address;
                    editModel.Warhouse = model.Warhouse;
                    editModel.MailNo = model.MailNo;
                    editModel.Memo = model.Memo;
                    editModel.Reson = model.Reson;
                    editModel.AddressMessage = model.AddressMessage;
                    editModel.NextApproveName = cofig.ApproveType;
                    //if (distri == "请选择")
                    //    editModel.AddressMessage = provin + "-" + cit;
                    //else
                    //    editModel.AddressMessage = provin + "-" + cit + "-" + distri;
                    db.SaveChanges();
                    //先删除详情再添加
                    List<T_InterceptDetail> dl = db.T_InterceptDetail.Where(s => s.InterceptId == model.ID).ToList();
                    if (dl.Count() > 0)
                    {
                        foreach (var item in dl)
                        {
                            T_InterceptDetail detail = db.T_InterceptDetail.Find(item.ID);
                            db.T_InterceptDetail.Remove(detail);
                        }
                        db.SaveChanges();
                    }
                    foreach (var item in details)
                    {
                        item.InterceptId = model.ID;
                        db.T_InterceptDetail.Add(item);
                    }
                    db.SaveChanges();
                   
                  
                    if (model.Step != 0)
                    {

                        T_InterceptApprove approve = new T_InterceptApprove();
                        approve.ApproveName = cofig.ApproveType;
                        approve.ApproveUser = cofig.ApproveType;
                        approve.ApproveStatus = -1;
                   
                        //if (cofig.ApproveType == "部门主管")
                        //{
                        //    int departid = int.Parse(UserModel.DepartmentId);
                        //    T_Department depart = db.T_Department.Find(departid);
                        //    if (depart.supervisor == null)
                        //        approve.ApproveUser = "成风";
                        //    else
                        //        approve.ApproveUser = db.T_User.Find(depart.supervisor).Nickname;
                        //}
                        editModel.Step = 0;
                        editModel.Status = -1;
                        approve.Pid = model.ID;
                        db.T_InterceptApprove.Add(approve);
                        db.SaveChanges();
                    }
                    else
                    {

                        T_InterceptApprove approve = db.T_InterceptApprove.SingleOrDefault(s => s.Pid == model.ID && !s.ApproveTime.HasValue);
                        approve.ApproveName = cofig.ApproveType;
                        approve.ApproveUser = cofig.ApproveType;
                        approve.ApproveStatus = -1;
                        db.SaveChanges();
                    }
                  //  ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }

        }
        public partial class InterceptgetExcel
        {
            public int ID { get; set; }
            public string VipCode { get; set; }
            public string VipName { get; set; }
            public string OrderNumber { get; set; }
            public DateTime CreateDate { get; set; }
            public string Receiver { get; set; }
            public string LoadExpressName { get; set; }
            public string MailNo { get; set; }
            public string LoadAddress { get; set; }
            public string TelPhone { get; set; }
            public string Memo { get; set; }
            public string Phone { get; set; }
            public string loadWarhouse { get; set; }
            public string Reson { get; set; }
            public string StoreCode { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public int Num { get; set; }
            public string LoadCode { get; set; }
            public string LoadName { get; set; }
            public int LoadNum { get; set; }
            public string SystemRemark { get; set; }
            

        }
        /// <summary>
        /// 拦截列表导出excel
        /// </summary>
        /// <param name="queryStr"></param>
        /// <param name="statedate"></param>
        /// <param name="EndDate"></param>
        /// <param name="RetreatReason"></param>
        /// <returns></returns>
        public FileResult getExcel(string queryStr, string statedate, string EndDate, string RetreatReason)
        {
            List<InterceptgetExcel> queryData = null;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
            string sql = "select  r.VipCode,r.VipName,r.OrderNumber,r.Receiver,isnull(r.SystemRemark,'') as SystemRemark,isnull(r.LoadExpressName,'') as LoadExpressName,isnull(r.MailNo,'') as MailNo,isnull(r.LoadAddress,'') as LoadAddress,isnull(r.TelPhone,'') as TelPhone,r.Memo,isnull(r.Phone,'') as Phone,r.loadWarhouse,r.Reson,CreateDate,r.StoreCode,isnull(t.Code,'') as Code ,t.Name,isnull(t.Num,'') as Num,isnull(t.LoadCode,'') as LoadCode ,t.LoadName,isnull(t.LoadNum,'') as LoadNum From  T_Intercept r join T_InterceptDetail  t ON  r.ID = t.InterceptId  where r.IsDelete = 0 and r.Status = 1 and  r.CreateDate>='" + sdate + "' and r.CreateDate<='" + edate + "' ";
            queryData = db.Database.SqlQuery<InterceptgetExcel>(sql).ToList();
            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Reson == RetreatReason).ToList();
            }
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                row1.CreateCell(0).SetCellValue("旺旺号");
                row1.CreateCell(1).SetCellValue("会员名称");
                row1.CreateCell(2).SetCellValue("订单编号");
                row1.CreateCell(3).SetCellValue("新增时间");
                row1.CreateCell(4).SetCellValue("收货人");
                row1.CreateCell(5).SetCellValue("原快递名称");
                row1.CreateCell(6).SetCellValue("原快递单号");
                row1.CreateCell(7).SetCellValue("原地址");
                row1.CreateCell(8).SetCellValue("电话号码");
                row1.CreateCell(9).SetCellValue("电话");
                row1.CreateCell(10).SetCellValue("原仓库");
                row1.CreateCell(11).SetCellValue("原因");
                row1.CreateCell(12).SetCellValue("店铺");
                row1.CreateCell(13).SetCellValue("需发产品代码");
                row1.CreateCell(14).SetCellValue("需发产品名称");
                row1.CreateCell(15).SetCellValue("需发产品数量");
                row1.CreateCell(16).SetCellValue("需发产品代码");
                row1.CreateCell(17).SetCellValue("需发产品名称");
                row1.CreateCell(18).SetCellValue("需发产品数量");
                row1.CreateCell(19).SetCellValue("备注");
                row1.CreateCell(20).SetCellValue("系统备注"); 
                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    rowtemp.CreateCell(0).SetCellValue(queryData[i].VipCode.ToString());
                    rowtemp.CreateCell(1).SetCellValue(queryData[i].VipName.ToString());
                    rowtemp.CreateCell(2).SetCellValue(queryData[i].OrderNumber.ToString());
                    rowtemp.CreateCell(3).SetCellValue(queryData[i].CreateDate.ToString());
                    rowtemp.CreateCell(4).SetCellValue(queryData[i].Receiver.ToString());
                    rowtemp.CreateCell(5).SetCellValue(queryData[i].LoadExpressName.ToString());
                    rowtemp.CreateCell(6).SetCellValue(queryData[i].LoadAddress.ToString());
                    rowtemp.CreateCell(7).SetCellValue(queryData[i].MailNo.ToString());
                    rowtemp.CreateCell(8).SetCellValue(queryData[i].TelPhone.ToString());
                    rowtemp.CreateCell(9).SetCellValue(queryData[i].Phone.ToString());
                    rowtemp.CreateCell(10).SetCellValue(queryData[i].loadWarhouse.ToString());
                    rowtemp.CreateCell(11).SetCellValue(queryData[i].Reson.ToString());
                    rowtemp.CreateCell(12).SetCellValue(queryData[i].StoreCode.ToString());
                    rowtemp.CreateCell(13).SetCellValue(queryData[i].Code.ToString());
                    rowtemp.CreateCell(14).SetCellValue(queryData[i].Name.ToString());
                    rowtemp.CreateCell(15).SetCellValue(queryData[i].Num.ToString());
                    rowtemp.CreateCell(16).SetCellValue(queryData[i].LoadCode.ToString());
                    rowtemp.CreateCell(17).SetCellValue(queryData[i].LoadName.ToString());
                    rowtemp.CreateCell(18).SetCellValue(queryData[i].LoadNum.ToString());
                    rowtemp.CreateCell(19).SetCellValue(queryData[i].Memo.ToString()); 
                    rowtemp.CreateCell(20).SetCellValue(queryData[i].SystemRemark.ToString()); 
    }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "拦截已同意数据.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "拦截已同意数据.xls");
            }
        }
        /// <summary>
        /// 拦截未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public partial class InterceptgetWshenhe
        {
            public int ID { get; set; }
            public string PostUSer { get; set; }
            public string OrderNumber { get; set; }
            public string NewOrderNumber { get; set; }
            public string Receiver { get; set; }
            public string ExpressName { get; set; }
            public string MailNo { get; set; }
            public string AddressMessage { get; set; }
            public string Address { get; set; }
            public string TelPhone { get; set; }
            public int Status { get; set; }
            public string Memo { get; set; }
            public string Postalcode { get; set; }
            public string Phone { get; set; }
            public string Warhouse { get; set; }
            public string Reson { get; set; }
            public System.DateTime CreateDate { get; set; }
            public int Step { get; set; }
            public int IsDelete { get; set; }
            public string LoadWarhouse { get; set; }
            public string LoadExpressName { get; set; }
            public string LoadAddress { get; set; }
            public string SystemRemark { get; set; }
            public string VipCode { get; set; }
            public string VipName { get; set; }
            public string StoreCode { get; set; }
            public string SingleTime { get; set; }
            public string NextApproveName { get; set; }
            public string KDJL { get; set; }
        }
        //未审核列表  
        [HttpPost]
        public ContentResult GetInterceptNotcheckList(Lib.GridPager pager, string name,  string ExpressType, string ReasonType, string RetreatWarehouseList, string RetreatexpressNameList)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_InterceptGroup> GroupModel = db.T_InterceptGroup.Where(a => a.GroupUser != null && (a.GroupUser.Contains(Nickname) || a.GroupUser.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }

            List<T_InterceptApprove> ApproveMod = db.T_InterceptApprove.Where(a => (shenheName.Contains(a.ApproveUser) || a.ApproveUser == name || a.ApproveUser == Nickname) && a.ApproveTime == null).ToList();
            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                if (i == 0)
                {
                    arrID += ApproveMod[i].Pid.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Pid.ToString();
                }
            }
            string sql = "select  *,isnull((select top 1 TrackRecord_Situation from T_InterceptExpressRecord where Oid=r.ID order by T_InterceptExpressRecord.ID desc ),'') as KDJL From T_Intercept r where Isdelete = '0'  and(Status = -1 or Status = 0 or Status = 2)";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<InterceptgetWshenhe> queryData = db.Database.SqlQuery<InterceptgetWshenhe>(sql).AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                queryData = queryData.Where(a => (a.OrderNumber != null && a.OrderNumber.Contains(name)) || a.MailNo != null && a.MailNo.Contains(name) || a.VipCode != null && a.VipCode.Contains(name) || a.VipName != null && a.VipName.Contains(name) || a.PostUSer != null && a.PostUSer.Contains(name));
            }
            if (ReasonType != null && ReasonType != "")
            {
                queryData = queryData.Where(a => a.Reson == ReasonType);
            }
            if (ExpressType != null && ExpressType != "")

            {
                if (ExpressType == "wcl")
                {
                    queryData = queryData.Where(a => a.KDJL == "");
                }
                else
                {
                    queryData = queryData.Where(a => a.KDJL == ExpressType);
                }
            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.ExpressName == RetreatexpressNameList);
            }
            if (RetreatWarehouseList != null && RetreatWarehouseList != "")
            {
                queryData = queryData.Where(a => a.LoadWarhouse == RetreatWarehouseList);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<InterceptgetWshenhe> list = new List<InterceptgetWshenhe>();
            foreach (var item in queryData)
            {
                InterceptgetWshenhe i = new InterceptgetWshenhe();
                i = item;
                i.Warhouse = Com.GetWarehouseName(item.Warhouse);
                i.ExpressName =Com.GetExpressName(item.ExpressName);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //public ContentResult GetInterceptNotcheckList(Lib.GridPager pager, string name, string ReasonType, string RetreatWarehouseList)
        //{
        //    List<T_Intercept> borrowList = new List<T_Intercept>();
        //    List<InterceptgetWshenhe> borrowLists = new List<InterceptgetWshenhe>();
        //    List<T_InterceptGroup> grouplist = db.T_InterceptGroup.Where(s => s.GroupUser.Contains(UserModel.Nickname)).ToList();
        //    string[] shenheName = new string[grouplist.Count];
        //    for (int z = 0; z < grouplist.Count; z++)
        //    {
        //        shenheName[z] = grouplist[z].GroupName;
        //    }
        //    IQueryable<T_InterceptApprove> list = db.T_InterceptApprove.Where(s => shenheName.Contains(s.ApproveUser) ||s.ApproveUser.Equals(UserModel.Nickname) && !s.ApproveTime.HasValue);
        //    List<int> itemIds = new List<int>();
        //    foreach (var item in list.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
        //    {
        //        itemIds.Add(item.Key);
        //    }

        //    foreach (var item in itemIds)
        //    {
        //        T_Intercept model = db.T_Intercept.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && (s.Status==0||s.Status==-1));
        //        if (model != null)
        //            borrowList.Add(model);
        //    }
        //    string sql = "select  *,isnull((select top 1 TrackRecord_Situation from T_InterceptExpressRecord where Oid=r.ID order by T_InterceptExpressRecord.ID desc ),'') as KDJL From T_Intercept R where Isdelete = '0'  and(Status = -1 or Status = 0 or Status = 2)";
        //    IQueryable<InterceptgetWshenhe> queryData = db.Database.SqlQuery<InterceptgetWshenhe>(sql).AsQueryable();
        //    if (!string.IsNullOrWhiteSpace(name))
        //        borrowList = borrowList.Where(s => s.OrderNumber.Contains(name)).ToList();
        //    if (ReasonType != null && ReasonType != "")
        //    {
        //        borrowList = borrowList.Where(a => a.Reson == ReasonType).ToList();
        //    }
        //    if (RetreatWarehouseList != null && RetreatWarehouseList != "")
        //    {
        //        borrowList = borrowList.Where(a => a.LoadWarhouse == RetreatWarehouseList).ToList();
        //    }
        //    pager.totalRows = borrowList.Count();
        //    List<T_Intercept> lists = new List<T_Intercept>();
        //    foreach (var item in borrowList)
        //    {
        //        T_Intercept model = new T_Intercept();
        //        model = item;
        //        model.Warhouse = Com.GetWarehouseName(model.Warhouse);
        //        model.ExpressName = Com.GetExpressName(model.ExpressName);
        //        lists.Add(model);
        //    }
        //    List<T_Intercept> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
        //    string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
        //    return Content(json);
        //}

        /// <summary>
        /// 已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ContentResult GetInterceptcheckList(Lib.GridPager pager, string name)
        {
            List<T_Intercept> borrowList = new List<T_Intercept>();
            IQueryable<T_InterceptApprove> list = db.T_InterceptApprove.Where(s => s.ApproveUser.Equals(UserModel.Nickname) && s.ApproveTime.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Intercept model = db.T_Intercept.SingleOrDefault(s => s.ID == item && s.IsDelete == 0);
                if (model != null)
                    borrowList.Add(model);
            }
            if (!string.IsNullOrWhiteSpace(name))
                borrowList = borrowList.Where(s => s.OrderNumber.Contains(name)).ToList();
            pager.totalRows = borrowList.Count();
            List<T_Intercept> lists = new List<T_Intercept>();
            foreach (var item in borrowList)
            {
                T_Intercept model = new T_Intercept();
                model = item;
                model.Warhouse = Com.GetWarehouseName(model.Warhouse);
                model.ExpressName = Com.GetExpressName(model.ExpressName);
                lists.Add(model);
            }
            List<T_Intercept> querData = lists.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

    }
}
