using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class ElectronicInvoiceController : Controller
    {
        //
        // GET: /ElectronicInvoice/
        EBMSEntities db = new EBMSEntities();

        public ActionResult ViewElectronicInvoiceAdd()
        {
            return View();
        }
        public ActionResult ViewElectronicInvoice()
        {
            return View();
        }
        public ActionResult ViewElectronicInvoiceList()
        {
            return View();
        }
        public ActionResult ViewElectronicInvoiceCheck()
        {
            return View();
        }
        public ActionResult ViewElectronicInvoiceChecken()
        {
            return View();
        }
        
        public ActionResult ViewElectronicInvoiceReportCheck(int ID)
        {
            ViewData["ID"] = ID;
            T_ElectronicInvoice Model = db.T_ElectronicInvoice.SingleOrDefault(a => a.ID == ID);
            T_ElectronicInvoiceAppRove modelApprove = db.T_ElectronicInvoiceAppRove.SingleOrDefault(a => a.Oid == ID && a.ApproveTime == null);
            T_ElectronicInvoice MajorInvoice = new T_ElectronicInvoice();
            MajorInvoice = db.T_ElectronicInvoice.Single(a => a.ID == ID);
            List<T_ElectronicInvoiceAppRove> approve = db.T_ElectronicInvoiceAppRove.Where(a => a.Oid == ID).ToList();
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
          //  ViewData["Approve"] = modelApprove.ApproveDName;
            return View(Model);
        }
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewElectronicInvoiceDetail(int ID)
        {
            T_ElectronicInvoice Model = db.T_ElectronicInvoice.SingleOrDefault(a => a.ID == ID);
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_ElectronicInvoiceAppRove.Where(a => a.Oid == ID);
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
        [Description("电子发票详情")]
        public ContentResult GetElectronicInvoiceDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_ElectronicInvoiceDetails> queryData = db.T_ElectronicInvoiceDetails.Where(a => a.Oid == ID).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<T_ElectronicInvoiceDetails> list = queryData.ToList();//.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_WDTGoods> queryData = db.T_WDTGoods.Where(a => a.goods_type == 1).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.goods_name != null && a.goods_name.Contains(queryStr) || a.goods_no != null && a.goods_no.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.goods_no).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WDTGoods> list = new List<T_WDTGoods>();
            foreach (var item in queryData)
            {
                T_WDTGoods i = new T_WDTGoods();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

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

           string [] codes= code.Split(',');
            List<T_ElectronicInvoice> ElectronicModel = new List<T_ElectronicInvoice>();
            List<T_ElectronicInvoiceDetails> DetailsList = new List<T_ElectronicInvoiceDetails>();
            for (int x = 0; x < codes.Length; x++)
            {
               string codez = codes[x];
                App_Code.GY gy = new App_Code.GY();
                string cmd = "";
                string repeat = "";


                dic.Clear();

                dic.Add("src_tid", codez);
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


                        //查询一次..
                        string shop_Code = "";


                        T_ElectronicInvoice model = new T_ElectronicInvoice();
                        model.OrderNumber = codez;
                        model.ShopName = shop_name;
                        model.TheInvoiceAmount = decimal.Parse(paid.ToString());
                        ElectronicModel.Add(model);
                      


                        for (int i = 0; i < goods_list.Count; i++)
                        {
                            T_ElectronicInvoiceDetails DetailsModel = new T_ElectronicInvoiceDetails();


                            DetailsModel.Code = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                            DetailsModel.Name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                            DetailsModel.specname = goods_list[i]["spec_name"] == null ? "" : goods_list[i]["spec_name"].ToString();
                            
                            DetailsModel.UnitPrice = decimal.Parse(goods_list[i]["order_price"].ToString());

                            decimal qyt = decimal.Parse(goods_list[i]["actual_num"].ToString());
                            DetailsModel.qty = int.Parse(Math.Round(qyt).ToString());
                            DetailsList.Add(DetailsModel);
                        }
                       

                      

                    }
                }
            }
            var json = new
            {

                rows = (from r in DetailsList
                        select new T_ElectronicInvoiceDetails
                        {
                            Code = r.Code,
                            Name = r.Name,
                            specname = r.specname,
                            UnitPrice = r.UnitPrice,
                            qty = r.qty,
                        }).ToArray()
            };
            if (ElectronicModel.Count>0)
            {
                string ShopName = ElectronicModel[0].ShopName;
                decimal TheInvoiceAmount = 0.00m;
                for (int d = 0; d < ElectronicModel.Count; d++)
                {
                    if (ShopName != ElectronicModel[d].ShopName)
                    {
                        return Json("-1", JsonRequestBehavior.AllowGet);
                    }
                    TheInvoiceAmount += decimal.Parse(ElectronicModel[d].TheInvoiceAmount.ToString());
                }
                T_ElectronicInvoice model = new T_ElectronicInvoice();
                model.OrderNumber = code;
                model.ShopName = ShopName;
                model.TheInvoiceAmount = TheInvoiceAmount;
                return Json(new { ModelList = model, Json = json }, JsonRequestBehavior.AllowGet);
            }
            else
            { 
            return Json("", JsonRequestBehavior.AllowGet);
            }


        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        [Description("电子发票管理")]
        public ContentResult GetElectronicInvoiceList(Lib.GridPager pager, string queryStr, string selStatus, string txtShop,string txtinvoiceNum,string txtName)
        {

            IQueryable<T_ElectronicInvoice> queryData = db.T_ElectronicInvoice.Where(a => a.Isdelete == "0").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || (a.PostUser != null && a.PostUser.Contains(queryStr)));
            }
            if (!string.IsNullOrEmpty(txtinvoiceNum))
            {
                queryData = queryData.Where(a => a.invoiceNum != null && a.invoiceNum.Contains(txtinvoiceNum));
            }
            if (!string.IsNullOrEmpty(txtShop))
            {
                queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(txtShop) );
            }
            if (!string.IsNullOrEmpty(selStatus) && selStatus != "-2")
            {
                int status = int.Parse(selStatus);
                queryData = queryData.Where(a => a.Status == status);
            }
            if (!string.IsNullOrEmpty(txtName))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(txtName));
            }

            pager.totalRows = queryData.Count();
            //分页
            List<T_ElectronicInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("新增保存")]
        public JsonResult ElectronicInvoiceAdd(T_ElectronicInvoice model,string picUrls, string jsonStr)
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
            model.Invoicetype = model.ShopName;
            db.T_ElectronicInvoice.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                List<T_ElectronicInvoiceDetails> details = App_Code.Com.Deserialize<T_ElectronicInvoiceDetails>(jsonStr);
                foreach (var item in details)
                {
                    if (item.specname == "undefined")
                    {
                        item.specname = "/";
                    }
                    item.Oid = model.ID;
                    db.T_ElectronicInvoiceDetails.Add(item);
                }
                db.SaveChanges();
                string InvoiceReason = model.Invoicetype;
               
                //先查看是选择的什么
                T_ElectronicInvoiceReason modelReason = db.T_ElectronicInvoiceReason.SingleOrDefault(a => a.InvoiceReason == InvoiceReason);

                string modelReasontype = "";
                if (modelReason == null)
                {
                    modelReasontype = "1";
                }
                else
                {
                    modelReasontype = modelReason.Type;
                }
                T_ElectronicInvoiceConfig modelconfig = db.T_ElectronicInvoiceConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == modelReasontype);
                T_ElectronicInvoiceAppRove AppRoveModel = new T_ElectronicInvoiceAppRove();
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
                db.T_ElectronicInvoiceAppRove.Add(AppRoveModel);
                db.SaveChanges();
                if (picUrls.Length > 0)
                {
                    string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Purl in picArr)
                    {
                        T_ElectronicInvoiceEnclosure IndemnityPic = new T_ElectronicInvoiceEnclosure();
                        IndemnityPic.oid = model.ID;
                        IndemnityPic.URL = Purl;
                        IndemnityPic.uploadDate = DateTime.Now;
                        IndemnityPic.uploadName = Nickname;
                        db.T_ElectronicInvoiceEnclosure.Add(IndemnityPic);
                    }
                    db.SaveChanges();
                }

                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }


        }
        //获取附件
        public JsonResult GetExpenseEnclosure(int id)
        {
            List<T_ElectronicInvoiceEnclosure> model = db.T_ElectronicInvoiceEnclosure.Where(a => a.oid == id).ToList();
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
        [Description("我的电子发票")]
        public ContentResult GetElectronicInvoice(Lib.GridPager pager, string queryStr,string txtShop)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_ElectronicInvoice> queryData = db.T_ElectronicInvoice.Where(a => a.PostUser == Nickname && a.Isdelete == "0").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(txtShop))
            {
                queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(txtShop));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_ElectronicInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [HttpPost]
        [Description("电子开票删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_ElectronicInvoice model = db.T_ElectronicInvoice.Find(ID);
            model.Isdelete = "1";
            db.Entry<T_ElectronicInvoice>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }

        [Description("电子发票未审核")]
        public ContentResult GetElectronicInvoiceCheck(Lib.GridPager pager, string queryStr,string txtShop)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_ElectronicInvoiceGroup> GroupModel = db.T_ElectronicInvoiceGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_ElectronicInvoiceAppRove> ApproveMod = db.T_ElectronicInvoiceAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_ElectronicInvoice r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_ElectronicInvoice> queryData = db.Database.SqlQuery<T_ElectronicInvoice>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(txtShop))
            {
                queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(txtShop));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_ElectronicInvoice> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //审核 
        public JsonResult ElectronicInvoiceCheck(T_MajorInvoice model, string status, string Memo,string Memos)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_ElectronicInvoice Invoicemodel = db.T_ElectronicInvoice.SingleOrDefault(a => a.ID == ID && a.Isdelete == "0");
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                List<T_ElectronicInvoiceGroup> RetreatGroupList = db.Database.SqlQuery<T_ElectronicInvoiceGroup>("select  * from T_ElectronicInvoiceGroup where Crew like '%" + Nickname + "%'").ToList();
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

                string sql = "select * from T_ElectronicInvoiceAppRove where Oid='" + ID + "' and ApproveTime is null ";
                if (GroupName != "" && GroupName != null)
                {
                    sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                }
                else
                {
                    sql += "    and ApproveName='" + Nickname + "'  ";
                }
                List<T_ElectronicInvoiceAppRove> AppRoveListModel = db.Database.SqlQuery<T_ElectronicInvoiceAppRove>(sql).ToList();
                if (AppRoveListModel.Count == 0)
                {
                    return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
                }


                T_ElectronicInvoiceAppRove modelApprove = db.T_ElectronicInvoiceAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
             



                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_ElectronicInvoiceAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int z = db.SaveChanges();
                if (z > 0)
                {
                    if (status == "1")
                    {

                        T_ElectronicInvoiceAppRove newApprove = new T_ElectronicInvoiceAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;

                        T_ElectronicInvoiceReason Reason = db.T_ElectronicInvoiceReason.SingleOrDefault(a => a.InvoiceReason == Invoicemodel.Invoicetype);
                        string Reasontype = "";
                        if (Reason == null)
                        {
                            Reasontype = "1";
                        }
                        else
                        {
                            Reasontype = Reason.Type;
                        }
                        List<T_ElectronicInvoiceConfig> config = db.T_ElectronicInvoiceConfig.Where(a => a.Reason == Reasontype).ToList();

                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_ElectronicInvoiceConfig stepMod = db.T_ElectronicInvoiceConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_ElectronicInvoiceAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.invoiceNum = Memos;
                        Invoicemodel.Step = step;
                        db.Entry<T_ElectronicInvoice>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
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
                        db.Entry<T_ElectronicInvoice>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }
             
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //专票已审核列表  
        [HttpPost]
        public ContentResult GetElectronicInvoiceChecken(Lib.GridPager pager, string queryStr,string txtShop)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_ElectronicInvoiceAppRove> ApproveMod = db.T_ElectronicInvoiceAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_ElectronicInvoice> queryData = from r in db.T_ElectronicInvoice
                                                        where Arry.Contains(r.ID) && r.Isdelete == "0"
                                                   select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.OrderNumber != null && a.OrderNumber.Contains(queryStr)));
            }
            if (!string.IsNullOrEmpty(txtShop))
            {
                queryData = queryData.Where(a => (a.ShopName != null && a.ShopName.Contains(txtShop)));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ElectronicInvoice> list = new List<T_ElectronicInvoice>();
            foreach (var item in queryData)
            {
                T_ElectronicInvoice i = new T_ElectronicInvoice();
                i = item;

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
    }
}
