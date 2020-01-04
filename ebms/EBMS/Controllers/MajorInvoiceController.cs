using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class MajorInvoiceController : BaseController
    {
        //
        // GET: /MajorInvoice/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewMajorInvoiceAdd()
        {
            ViewData["ShopNameList"] = App_Code.Com.Shop();
            ViewData["InvoicetypeList"] = Invoice();

            return View();
        }

        /// <summary>
        /// 绑定步奏无默认值
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Invoice()
        {

            EBMSEntities db = new EBMSEntities();
            var list = db.T_MajorInvoiceReason.AsQueryable();
            var selectList = new SelectList(list, "InvoiceReason", "InvoiceReason");
            List<SelectListItem> selecli = new List<SelectListItem>();

            selecli.AddRange(selectList);
            return selecli;
        }
        public ActionResult ViewMajorInvoice()
        {
            return View();
        }
        public ActionResult ViewMajorInvoiceList()
        {
            return View();
        }
        public ActionResult ViewMajorInvoiceCheck()
        {
            return View();
        }
        public ActionResult ViewMajorInvoiceChecken()
        {
            return View();
        }
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewMajorInvoiceEdit(int ID)
        {
            ViewData["InvoicetypeList"] = Invoice();
            T_MajorInvoice model = db.T_MajorInvoice.SingleOrDefault(a => a.ID == ID);
            ViewData["ID"] = ID;
            return View(model);
        }
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_MajorInvoiceDetails> queryData = db.T_MajorInvoiceDetails.Where(a => a.Oid == ID);
            pager.totalRows = queryData.Count();
            //List<T_PurchaseDetails> list = new List<T_PurchaseDetails>();
            //foreach (var item in queryData)
            //{
            //    T_PurchaseDetails i = new T_PurchaseDetails();
            //    i = item;
            //    list.Add(i);
            //}
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData.ToList(), Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewMajorInvoiceDetail(int ID)
        {
            T_MajorInvoice Model = db.T_MajorInvoice.SingleOrDefault(a => a.ID == ID);
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_MajorInvoiceAppRove.Where(a => a.Oid == ID);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=blue>未审核</font>";
                if (item.Status == 1) s = "<font color=green>已同意</font>";
                if (item.Status == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View(Model);
        }
        //获取附件
        public JsonResult GetExpenseEnclosure(int id)
        {
            List<T_MajorInvoiceEnclosure> model = db.T_MajorInvoiceEnclosure.Where(a => a.oid == id).ToList();
            string options = "";
            if (model.Count > 0)
            {
                options += "[";
                foreach (var item in model)
                {
                    options += "{\"Url\":\"" + item.URL + "\"},";
                }
                options = options.Substring(0, options.Length - 1);
                options += "]";
            }
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewMajorInvoiceReportCheck(int ID)
        {
            ViewData["ID"] = ID;
            T_MajorInvoice Model = db.T_MajorInvoice.SingleOrDefault(a => a.ID == ID);
            T_MajorInvoiceAppRove modelApprove = db.T_MajorInvoiceAppRove.SingleOrDefault(a => a.Oid == ID && a.ApproveTime == null);
            T_MajorInvoice MajorInvoice = new T_MajorInvoice();
            MajorInvoice = db.T_MajorInvoice.Single(a => a.ID == ID);
            List<T_MajorInvoiceAppRove> approve = db.T_MajorInvoiceAppRove.Where(a => a.Oid == ID).ToList();
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in approve)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=#d02e2e>未审核</font>";
                if (item.Status == 1) s = "<font color=#1fc73a>已同意</font>";
                if (item.Status == 2) s = "<font color=#d02e2e>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["approveid"] = ID;
            ViewData["Approve"] = modelApprove.ApproveDName;
            return View(Model);
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

        [Description("得到管易的订单详情")]
        public JsonResult QuyerRetreatDetailBYcode(string code = "")
        {



            App_Code.GY gy = new App_Code.GY();
            string cmd = "";
            string repeat = "";
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
                cmd = CreateParam(dic, true);

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
                    //客服备注
                    string cs_remark = trades["cs_remark"].ToString();
                    //实付金额
                    string paid = trades["paid"].ToString();
                    //商品详情
                    JsonData goods_list = trades["goods_list"];
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
                        //T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
                        //shop_Code = commonarea.shop_no;
                        shop_Code = "tm004";
                    }

                    T_MajorInvoice model = new T_MajorInvoice();
                    model.OrderNumber = code;
                    model.ShopName = shop_name;


                    List<T_MajorInvoiceDetails> DetailsList = new List<T_MajorInvoiceDetails>();


                    for (int i = 0; i < goods_list.Count; i++)
                    {
                        T_MajorInvoiceDetails DetailsModel = new T_MajorInvoiceDetails();


                        DetailsModel.Code = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                        DetailsModel.Name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();

                        DetailsModel.UnitPrice = decimal.Parse(goods_list[i]["paid"].ToString());

                        decimal qyt = decimal.Parse(goods_list[i]["actual_num"].ToString());
                        DetailsModel.qty = int.Parse(Math.Round(qyt).ToString());
                        DetailsList.Add(DetailsModel);
                    }
                    var json = new
                    {

                        rows = (from r in DetailsList
                                select new T_MajorInvoiceDetails
                                {
                                    Code = r.Code,
                                    Name = r.Name,
                                    UnitPrice = r.UnitPrice,
                                    qty = r.qty,
                                }).ToArray()
                    };
                    return Json(new { ModelList = model, Json = json }, JsonRequestBehavior.AllowGet);

                     

                    }


                }

             


              
           

            return Json("", JsonRequestBehavior.AllowGet);


        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_WDTGoods> queryData = db.T_WDTGoods.Where(a=>a.spec_aux_unit_name==null || a.spec_aux_unit_name!="1");
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.goods_name != null && a.goods_name.Contains(queryStr) || a.goods_no != null && a.goods_no.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.goods_no).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WDTGoods> list = queryData.ToList();
          
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        [Description("专票未审核")]
        public ContentResult GetMajorInvoiceCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_MajorInvoiceGroup> GroupModel = db.T_MajorInvoiceGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_MajorInvoiceAppRove> ApproveMod = db.T_MajorInvoiceAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                if (i == 0)
                {
                    arrID += ApproveMod[i].Oid.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Oid.ToString();
                }
            }
            string sql = "select * from T_MajorInvoice r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_MajorInvoice> queryData = db.Database.SqlQuery<T_MajorInvoice>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_MajorInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //专票已审核列表  
        [HttpPost]
        public ContentResult GetMajorInvoiceCheckenList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_MajorInvoiceAppRove> ApproveMod = db.T_MajorInvoiceAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_MajorInvoice> queryData = from r in db.T_MajorInvoice
                                                   where Arry.Contains(r.ID) && r.Isdelete == "0"
                                                   select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.OrderNumber != null && a.OrderNumber.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MajorInvoice> list = new List<T_MajorInvoice>();
            foreach (var item in queryData)
            {
                T_MajorInvoice i = new T_MajorInvoice();
                i = item;

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("专票详情")]
        public ContentResult GetMajorInvoiceDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_MajorInvoiceDetails> queryData = db.T_MajorInvoiceDetails.Where(a => a.Oid == ID).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<T_MajorInvoiceDetails> list = queryData.ToList();//.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("专票管理")]
        public ContentResult GetMajorInvoiceList(Lib.GridPager pager, string queryStr, string selStatus, string statedate, string EndDate)
        {

            IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a => a.Isdelete == "0").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || (a.PostUser != null && a.PostUser.Contains(queryStr)));
            }
            if (!string.IsNullOrEmpty(selStatus) && selStatus != "-2")
            {
                int status = int.Parse(selStatus);
                queryData = queryData.Where(a => a.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                queryData = queryData.Where(s => s.PostDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.PostDate <= end);
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_MajorInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("我的专票")]
        public ContentResult GetMajorInvoice(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a => a.PostUser == Nickname && a.Isdelete == "0").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_MajorInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("新增保存")]
        public JsonResult MajorInvoiceAdd(T_MajorInvoice model, string picUrls, string jsonStr)
        {

            string orderNumBer = model.OrderNumber;

            List<T_MajorInvoice> Nmodel = db.T_MajorInvoice.Where(a => a.OrderNumber == orderNumBer && a.Isdelete == "0").ToList();
            if (Nmodel.Count > 0)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.PostUser = Nickname;
            model.PostDate = DateTime.Now;
            model.Status = -1;
            model.Step = 0;
            model.Isdelete = "0";
            db.T_MajorInvoice.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                List<T_MajorInvoiceDetails> details = App_Code.Com.Deserialize<T_MajorInvoiceDetails>(jsonStr);
                foreach (var item in details)
                {

                    item.Oid = model.ID;
                    db.T_MajorInvoiceDetails.Add(item);
                }
                db.SaveChanges();
                string InvoiceReason = model.Invoicetype;
                //先查看是选择的什么
                T_MajorInvoiceReason modelReason = db.T_MajorInvoiceReason.SingleOrDefault(a => a.InvoiceReason == InvoiceReason);

                string modelReasontype = modelReason.Type;

                T_MajorInvoiceConfig modelconfig = db.T_MajorInvoiceConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == modelReasontype);
                T_MajorInvoiceAppRove AppRoveModel = new T_MajorInvoiceAppRove();
                AppRoveModel.Status = -1;
                AppRoveModel.Step = "0";
                if (modelconfig.Name == null || modelconfig.Name == "")
                {
                    AppRoveModel.ApproveName = modelconfig.Type;
                }
                else
                {
                    AppRoveModel.ApproveName = modelconfig.Name;
                }
                AppRoveModel.ApproveDName = modelconfig.Type;
                AppRoveModel.Oid = model.ID;
                db.T_MajorInvoiceAppRove.Add(AppRoveModel);
                db.SaveChanges();

                if (picUrls.Length > 0)
                {
                    string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Purl in picArr)
                    {
                        T_MajorInvoiceEnclosure IndemnityPic = new T_MajorInvoiceEnclosure();
                        IndemnityPic.oid = model.ID;
                        IndemnityPic.URL = Purl;
                        IndemnityPic.uploadDate = DateTime.Now;
                        IndemnityPic.uploadName = Nickname;
                        db.T_MajorInvoiceEnclosure.Add(IndemnityPic);
                    }
                    db.SaveChanges();
                }

                //ModularByZP();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }


        }
        public void ModularByZP()
        {

            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "专票").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_MajorInvoiceAppRove where Oid in (select ID from T_MajorInvoice where  Isdelete='0'  and (Status = -1 or Status = 0) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "专票" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "专票";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_MajorInvoice where Status='2' and Isdelete=0 GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "专票" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "专票";
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
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //编辑保存
        [HttpPost]
        [Description("专票编辑保存")]
        public JsonResult MajorInvoiceEditSave(T_MajorInvoice model, string jsonStr, string picUrls)
        {


            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            int ID = model.ID;
            T_MajorInvoice MajModel = db.T_MajorInvoice.Find(ID);

            if (MajModel.Status != -1 && MajModel.Status != 2)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }


            MajModel.Status = -1;
            MajModel.Step = 0;
            MajModel.Invoicetype = model.Invoicetype;
            MajModel.TheInvoiceAmount = model.TheInvoiceAmount;
            MajModel.InvoiceContent = model.InvoiceContent;
            MajModel.Address = model.Address;
            MajModel.CorporateName = model.CorporateName;
            MajModel.TaxNumber = model.TaxNumber;
            MajModel.InvoiceAddress = model.InvoiceAddress;
            MajModel.Telephone = model.Telephone;
            MajModel.BankAccount = model.BankAccount;
            MajModel.BankAddress = model.BankAddress;
			MajModel.GoodsAddress = model.GoodsAddress;
			MajModel.Remarks = model.Remarks;
            db.Entry<T_MajorInvoice>(MajModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                List<T_MajorInvoiceDetails> delMod = db.T_MajorInvoiceDetails.Where(a => a.Oid == ID).ToList();
                foreach (var item in delMod)
                {
                    db.T_MajorInvoiceDetails.Remove(item);
                }
                List<T_MajorInvoiceDetails> details = App_Code.Com.Deserialize<T_MajorInvoiceDetails>(jsonStr);
                foreach (var item in details)
                {
                    item.Oid = model.ID;
                    db.T_MajorInvoiceDetails.Add(item);
                }
                db.SaveChanges();
                if (picUrls.Length > 0)
                {
                    string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Purl in picArr)
                    {
                        T_MajorInvoiceEnclosure IndemnityPic = new T_MajorInvoiceEnclosure();
                        IndemnityPic.oid = model.ID;
                        IndemnityPic.URL = Purl;
                        IndemnityPic.uploadDate = DateTime.Now;
                        IndemnityPic.uploadName = Nickname;
                        db.T_MajorInvoiceEnclosure.Add(IndemnityPic);
                    }
                    db.SaveChanges();
                }

                string InvoiceReason = model.Invoicetype;
                T_MajorInvoiceReason modelReason = db.T_MajorInvoiceReason.SingleOrDefault(a => a.InvoiceReason == InvoiceReason);

                T_MajorInvoiceConfig modelconfig = db.T_MajorInvoiceConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == modelReason.Type);
                T_MajorInvoiceAppRove Approvemodel = db.T_MajorInvoiceAppRove.SingleOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                if (Approvemodel == null)
                {
                    //不同意 修改
                    T_MajorInvoiceAppRove modelApprove = new T_MajorInvoiceAppRove();
                    modelApprove.Status = -1;
                    modelApprove.Step = "0";
                    modelApprove.Memo = "";
                    modelApprove.Oid = model.ID;


                    if (modelconfig.Name != "" && modelconfig.Name != null)
                    {
                        modelApprove.ApproveName = modelconfig.Name;
                    }
                    else
                    {
                        modelApprove.ApproveName = modelconfig.Type;

                    }
                    modelApprove.ApproveDName = modelconfig.Type;
                    db.T_MajorInvoiceAppRove.Add(modelApprove);
                    db.SaveChanges();
                }
                else
                {
                    //新增修改
                    if (modelconfig.Name != "" && modelconfig.Name != null)
                    {
                        Approvemodel.ApproveName = modelconfig.Name;
                    }
                    else
                    {
                        Approvemodel.ApproveName = modelconfig.Type;

                    }
                    Approvemodel.ApproveDName = modelconfig.Type;
                    db.Entry<T_MajorInvoiceAppRove>(Approvemodel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                //T_MajorInvoiceConfig modelconfig = db.T_MajorInvoiceConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                //T_MajorInvoiceAppRove AppRoveModel = new T_MajorInvoiceAppRove();
                //AppRoveModel.Status = -1;
                //AppRoveModel.Step = "0";
                //if (modelconfig.Name == null || modelconfig.Name == "")
                //{
                //    AppRoveModel.ApproveName = modelconfig.Type;
                //}
                //else
                //{
                //    AppRoveModel.ApproveName = modelconfig.Name;
                //}
                //AppRoveModel.ApproveDName = modelconfig.Type;
                //AppRoveModel.Oid = model.ID;
                //db.T_MajorInvoiceAppRove.Add(AppRoveModel);
                //db.SaveChanges();
            }
            //ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Description("开专票删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_MajorInvoice model = db.T_MajorInvoice.Find(ID);
            model.Isdelete = "1";
            db.Entry<T_MajorInvoice>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
           // ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //审核 
        public JsonResult MajorInvoiceCheck(T_MajorInvoice model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_MajorInvoice Invoicemodel = db.T_MajorInvoice.SingleOrDefault(a => a.ID == ID && a.Isdelete == "0");
                string ShopName = db.T_ShopFromGY.SingleOrDefault(a => a.name == Invoicemodel.ShopName).code;
                string Name = Com.GetReissueName(ShopName, ShopName);


                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                List<T_MajorInvoiceGroup> RetreatGroupList = db.Database.SqlQuery<T_MajorInvoiceGroup>("select  * from T_MajorInvoiceGroup where Crew like '%" + Nickname + "%'").ToList();
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

                string sql = "select * from T_MajorInvoiceAppRove where Oid='" + ID + "' and ApproveTime is null ";
                if (GroupName != "" && GroupName != null)
                {
                    sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                }
                else
                {
                    sql += "    and ApproveName='" + Nickname + "'  ";
                }
                List<T_MajorInvoiceAppRove> AppRoveListModel = db.Database.SqlQuery<T_MajorInvoiceAppRove>(sql).ToList();
                if (AppRoveListModel.Count == 0)
                {
                    return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
                }


                T_MajorInvoiceAppRove modelApprove = db.T_MajorInvoiceAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                //if (modelApprove == null )
                //{ 

                //}



                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_MajorInvoiceAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int z = db.SaveChanges();
                if (z > 0)
                {
                    if (status == "1")
                    {

                        T_MajorInvoiceAppRove newApprove = new T_MajorInvoiceAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;

                        T_MajorInvoiceReason Reason = db.T_MajorInvoiceReason.SingleOrDefault(a => a.InvoiceReason == Invoicemodel.Invoicetype);

                        List<T_MajorInvoiceConfig> config = db.T_MajorInvoiceConfig.Where(a => a.Reason == Reason.Type).ToList();

                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_MajorInvoiceConfig stepMod = db.T_MajorInvoiceConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            newApprove.Memo = "";
                            newApprove.Oid = ID;
                            newApprove.Status = -1;
                            newApprove.Step = step.ToString();
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                                newApprove.ApproveDName = stepMod.Type;
                            }
                            else
                            {
                                newApprove.ApproveName = stepMod.Type;
                                newApprove.ApproveDName = stepMod.Type;
                            }
                            db.T_MajorInvoiceAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {



                            //加数据到补发

                            #region 加入补发货
                            string code = "";
                            if (Invoicemodel.OrderSCode == null || Invoicemodel.OrderSCode == "")
                            {
                                code = Invoicemodel.OrderNumber;

                                App_Code.GY gy = new App_Code.GY();
                             //   code = Invoicemodel.OrderSCode;
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
                                        //快递公司名称
                                        string logistics_name = trades["logistics_name"].ToString();
                                        //快递单号
                                        string logistics_no = trades["logistics_no"].ToString();
                                        //买家留言
                                        string buyer_message = trades["buyer_message"].ToString();
                                        //客服备注
                                        string cs_remark = trades["cs_remark"].ToString();
                                        //实付金额
                                        string paid = trades["paid"].ToString();
                                        //商品详情
                                        JsonData goods_list = trades["goods_list"];

                                        string remark = "";
                                        List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(code) && s.IsDelete == 0).ToList();
                                        if (  reissue.Count > 1)
                                        {
                                            var date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
                                            var modeldate = Convert.ToDateTime(reissue[0].CreatDate).ToString("yyyyMMdd");

                                            if (reissue.Count > 1 && int.Parse(date) - int.Parse(modeldate) <= 3  )
                                                remark =   "3天内补发货重复";
                                        }
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
                                        else
                                        {
                                            int co = (int)reissue.Count + 1;
                                            t = "80" + co;
                                        }

                                        string shop_Code = "";
                                        if (shop_name != null)
                                        {
                                            T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
                                            shop_Code = commonarea.shop_no;
                                            //shop_Code = "tm004";
                                        }

                                        T_Reissue reissues = new T_Reissue
                                        {

                                            OrderCode = code,
                                            NewOrderCode = t + code,
                                            //NewOrderCode = "8" + DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                                            VipName = customer_name,
                                            StoreName = shop_name,
                                            WarhouseName = "7310002",
                                            ExpressName = "韵达快递市场部（省内）",
                                            OrderType = "售后补发",
                                            SingleTime = DateTime.Now.ToString(),
                                            ReceivingName = customer_name,
                                            PostalCode = receiver_zip,
                                            Phone = receiver_mobile,
                                            TelPhone = receiver_mobile,
                                            VipCode = customer_name,
                                            Address = Invoicemodel.Address,//详细地址
                                            AddressMessage = Invoicemodel.GoodsAddress, //省市区
                                            SalesRemark = "补发编码:654321,补发" + Invoicemodel.Invoicetype + Invoicemodel.Remarks+ remark,
                                            BuyRemark = "补发专票",
                                            StoreCode = shop_Code,
                                            Step = 0,
                                            Status = -1,
                                            BusinessName = Name,
                                            PostUser = Name,
                                            DraftName = Name,
                                            CreatDate = DateTime.Now,
                                            IsDelete = 0,
                                            ReissueReson = "其它"
                                            // SystemRemark = remark
                                        };
                                        db.T_Reissue.Add(reissues);
                                        db.SaveChanges();

                                        IQueryable<T_MajorInvoiceDetails> detail = db.T_MajorInvoiceDetails.Where(s => s.Oid == model.ID);
                                        foreach (var item in detail)
                                        {
                                            T_ReissueDetail items = new T_ReissueDetail
                                            {
                                                ProductCode = "654321",
                                                ProductName = "电商补发发票代码",
                                                Num = 1,
                                                ReissueId = reissues.ID
                                            };
                                            db.T_ReissueDetail.Add(items);
                                        }

                                        T_ReissueApprove Reissueapprove = new T_ReissueApprove
                                        {

                                            ApproveName = Name,
                                            ApproveUser = Name,
                                            ApproveStatus = -1,
                                            Pid = reissues.ID
                                        };
                                        db.T_ReissueApprove.Add(Reissueapprove);
                                        db.SaveChanges();
                                    }

                                }
                                }

                            #endregion
                            Invoicemodel.Status = int.Parse(status);

                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_MajorInvoice>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        if (j > 0)
                        {
                            result += "保存成功";
                        }
                        else
                        {
                            result += "保存失败";
                        }
                    }
                    else
                    {

                        //不同意
                        Invoicemodel.Step = 0;
                        Invoicemodel.Status = 2;
                        db.Entry<T_MajorInvoice>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }
               // ModularByZP();
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
