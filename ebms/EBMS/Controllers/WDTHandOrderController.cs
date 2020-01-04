using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class WDTHandOrderController : Controller
    {
        //
        // GET: /WDTHandOrder/
        EBMSEntities db = new EBMSEntities();

        public ActionResult WDTHandOrderGrid()
        {
            return View();
        }
        public ActionResult WDTHandOrderAdd()
        {
            ViewData["ShopNameList"] = App_Code.Com.WDTShop();
            return View();
        }
        public ActionResult WDTGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult WDTHandOrderList()
        {
       
            return View();
        }
        public ActionResult WDTHandOrderCheck()
        {

            return View();
        }
        public ActionResult WDTHandOrderCheckReport(int ID)
        {

          
            T_WDTHandOrder Model = db.T_WDTHandOrder.SingleOrDefault(a => a.ID == ID);
            T_WDTHandOrderApprove modelApprove = db.T_WDTHandOrderApprove.SingleOrDefault(a => a.Pid == ID && a.ApproveTime == null);
            if (Model.ProvinceAddress == "null")
            {
                Model.ProvinceAddress = "";

            }
            if (Model.CityAddress == "null")
            {
                Model.CityAddress = "";
            }
            if (Model.AreaAddress == "null")
            {
                Model.AreaAddress = "";
            }
            List<T_WDTHandOrderApprove> approve = db.T_WDTHandOrderApprove.Where(a => a.Pid == ID).ToList();
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in approve)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=#d02e2e>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=#1fc73a>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=#d02e2e>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["approveid"] =Model.OrderID;
            ViewData["ID"] = Model.OrderID;
            ViewData["Approve"] = modelApprove.ApproveUser;
            return View(Model);

        }
        public ActionResult WDTHandOrderChecken()
        {

            return View();
        }
        public ActionResult WDTHandOrderEdit(int ID)
        {
            ViewData["ShopNameList"] = App_Code.Com.WDTShop();
            T_WDTHandOrder model = db.T_WDTHandOrder.SingleOrDefault(a => a.ID == ID);
            ViewData["ID"] = model.OrderID;
            return View(model);
        }
        //手工订单已审核列表  
        [HttpPost]
        public ContentResult GetWDTHandOrderCheckenList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_WDTHandOrderApprove> ApproveMod = db.T_WDTHandOrderApprove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.ApproveStatus == 1 || a.ApproveStatus == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
             
                Arry[i] = int.Parse(ApproveMod[i].Pid.ToString());
            }
            IQueryable<T_WDTHandOrder> queryData = from r in db.T_WDTHandOrder
                                                    where Arry.Contains(r.ID) && r.Isdelete == 0
                                                    select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.OrderID != null && a.OrderID.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WDTHandOrder> list = new List<T_WDTHandOrder>();
            foreach (var item in queryData)
            {
                T_WDTHandOrder i = new T_WDTHandOrder();
                i = item;
            
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("手工订单审核查询")]
        public ContentResult GetWDTHandOrderCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_WDTHandOrderGroup> GroupModel = db.T_WDTHandOrderGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_WDTHandOrderApprove> ApproveMod = db.T_WDTHandOrderApprove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_WDTHandOrder r  where isdelete=0  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_WDTHandOrder> queryData = db.Database.SqlQuery<T_WDTHandOrder>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderID != null && a.OrderID.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WDTHandOrder> list = new List<T_WDTHandOrder>();
            foreach (var item in queryData)
            {
                T_WDTHandOrder i = new T_WDTHandOrder();
              
                i = item;
                list.Add(i);
            }
            //分页
            //  List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("手工订单管理")]
        public ContentResult GetWDTHandOrderList(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_WDTHandOrder> queryData = db.T_WDTHandOrder.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderID != null && a.OrderID.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WDTHandOrder> list = new List<T_WDTHandOrder>();
            foreach (var item in queryData)
            {
                T_WDTHandOrder i = new T_WDTHandOrder();
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("手工订单详情")]
        public ContentResult GetWDTHandOrderDetail(Lib.GridPager pager, string ID)
        {
            IQueryable<T_WDTHandOrderDetail> queryData = db.T_WDTHandOrderDetail.Where(a => a.PorderID == ID).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<T_WDTHandOrderDetail> list = queryData.ToList();//.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetWDTGoodsGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_WDTGoods> queryData = db.T_WDTGoods.AsQueryable();
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
        [Description("我的旺店通手工订单")]
        public ContentResult GetWDTHandOrderGrid(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_WDTHandOrder> queryData = db.T_WDTHandOrder.Where(a => a.CreateName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderID != null && a.OrderID.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_WDTHandOrder> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [HttpPost]
        [Description("旺店通手工订单删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            try
            {
            T_WDTHandOrder model = db.T_WDTHandOrder.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_WDTHandOrder>(model).State = System.Data.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(0,ex.Message);
            }
        }
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, string ID)
        {
            IQueryable<T_WDTHandOrderDetail> queryData = db.T_WDTHandOrderDetail.Where(a => a.PorderID == ID);
            pager.totalRows = queryData.Count();
          
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData.ToList(), Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //public string ShopFromByNAME(string CODE)
        //{
        //    T_WDTshop queryData = db.T_WDTshop.SingleOrDefault(a => a.shop_no==CODE);
        //    string name = queryData.shop_name;
        //    return name;
        //}

        [Description("新增保存")]
        public JsonResult WDTHOrderAdd(T_WDTHandOrder model, string jsonStr, string province, string city, string area)
        {
            //如果进入数据库失败就全部回滚
            using (TransactionScope sc = new TransactionScope())
            {
                //监控插入语句的异常
                try
                {
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    //给主表T_WDTHandOrder缺少值的字段赋值
                    string orderNum = "7" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    model.OrderID = orderNum;
                    model.SingleShotTime = DateTime.Now;
                    model.deliverGoodsTime = DateTime.Now;
                    if (province == null)
                    {
                        province = "";
                    }
                    if (city == null)
                    {
                        city = "";
                    }
                    if (area == "")
                    {
                        area = "";
                    }
                    model.ProvinceAddress = province;
                    model.CityAddress = city;
                    model.AreaAddress = area;
                    model.CreateTime = DateTime.Now;
                    model.CreateName = Nickname;
                    model.Isdelete = 0;
                    model.Status = -1;
                    model.Step = 0;
                    decimal cost = 0;
                 
                    List<T_WDTHandOrderDetail> details = App_Code.Com.Deserialize<T_WDTHandOrderDetail>(jsonStr);
                    foreach (var item in details)
                    {
                        decimal num =  decimal.Parse(item.num.ToString());
                        decimal Price = decimal.Parse(item.UnitPrice.ToString());
                        cost = (num * Price) + cost;
                     
                        item.PorderID = model.OrderID;
                        db.T_WDTHandOrderDetail.Add(item);
                    }
                   
                    cost +=decimal.Parse(model.PostAmount.ToString());
                    //把计算的总金额赋值
                    model.Cost = cost;
                    db.T_WDTHandOrder.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_WDTHandOrderConfig modelconfig = db.T_WDTHandOrderConfig.SingleOrDefault(a => a.Step == 0 && a.Reson == 1);

                        T_WDTHandOrderApprove AppRoveModel = new T_WDTHandOrderApprove();
                        AppRoveModel.ApproveStatus = -1;

                        if (modelconfig.Name == null || modelconfig.Name == "")
                        {
                            AppRoveModel.ApproveName = modelconfig.ApproveType;
                        }
                        else
                        {
                            AppRoveModel.ApproveName = modelconfig.Name;
                        }
                        AppRoveModel.ApproveUser = modelconfig.ApproveType;
                        AppRoveModel.Pid = model.ID;
                        db.T_WDTHandOrderApprove.Add(AppRoveModel);
                        db.SaveChanges();
                      
                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex )
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }

            }
         
        }

        //审核 
        public JsonResult WDTHandOrderCheckCheck(T_WDTHandOrder model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_WDTHandOrder Invoicemodel = db.T_WDTHandOrder.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_WDTHandOrderApprove modelApprove = db.T_WDTHandOrderApprove.FirstOrDefault(a => a.Pid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.ApproveStatus = int.Parse(status);
                db.Entry<T_WDTHandOrderApprove>(modelApprove).State = System.Data.EntityState.Modified;
                int i = db.SaveChanges();
                string Upde = "0";
                if (i > 0)
                {
                    if (status == "1")
                    {
                        T_WDTHandOrderApprove newApprove = new T_WDTHandOrderApprove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_WDTHandOrderConfig> config = db.T_WDTHandOrderConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_WDTHandOrderConfig stepMod = db.T_WDTHandOrderConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            newApprove.Memo = "";
                            newApprove.Pid = ID;
                            newApprove.ApproveStatus = -1;
                        
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                                newApprove.ApproveUser = stepMod.ApproveType;
                            }
                            else
                            {
                                newApprove.ApproveName = stepMod.ApproveType;
                                newApprove.ApproveUser = stepMod.ApproveType;
                            }
                            db.T_WDTHandOrderApprove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {

                          Upde=UpToGY(Invoicemodel.ID);
                            Invoicemodel.Status = int.Parse(status);

                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_WDTHandOrder>(Invoicemodel).State = System.Data.EntityState.Modified;
                        int j = db.SaveChanges();
                        if (Upde == "1"&&j > 0)
                        {
                            sc.Complete();
                            result = "保存成功";
                        }
                        else
                        {
                            if (Upde == "0")
                            {
                                result = "上传旺店通失败导致保存失败";
                            }
                            else
                            {
                                result = "保存失败";
                            }
                        }
                    }
                    else
                    {

                        //不同意
                        Invoicemodel.Step = 0;
                        Invoicemodel.Status = 2;
                        db.Entry<T_WDTHandOrder>(Invoicemodel).State = System.Data.EntityState.Modified;
                        int j = db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        sc.Complete();
                        if (j > 0)
                        {
                            result = "保存成功";
                        }
                        else
                        {
                            result = "保存失败";
                        }

                    }
                }
                else
                {
                    result = "保存失败";
                }


                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public string UpToGY(int ID)
        {

          
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                var aa = "";
                try
                {

                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    T_WDTHandOrder ordermodel = db.T_WDTHandOrder.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                    string tid = ordermodel.OrderID;
                    //int trade_status = 10;
                    //int pay_status = 2;
                    //int delivery_term = 1;
                    DateTime trade_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                    DateTime pay_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                    string buyer_nick = ordermodel.MemberName;
                    string receiver_name = ordermodel.DeliveryName;

                    string ShopName = ordermodel.shopName;
                    string receiver_province =ordermodel.ProvinceAddress;
                    string receiver_city =ordermodel.CityAddress;
                    string receiver_district =ordermodel.AreaAddress;
                    string receiver_address = ordermodel.address;
                    string receiver_mobile = ordermodel.DeliveryNumber;
                    // decimal post_amount = decimal.Parse(ordermodel.PostAmount.ToString());
                    // int logistics_type = -1;
                    string seller_memo = ordermodel.Remarks;
                    decimal paid = decimal.Parse(ordermodel.Cost.ToString());

                    List<T_WDTHandOrderDetail> orderItems = db.T_WDTHandOrderDetail.Where(a => a.PorderID == tid).ToList();
                    string order_list = "";
                    for (int i = 0; i < orderItems.Count; i++)
                        {
                            decimal num = decimal.Parse(orderItems[i].num.ToString());
                            decimal UnitPrice = decimal.Parse(orderItems[i].UnitPrice.ToString());
                            string goods_no = orderItems[i].ProductCode;
                            string spec_no = orderItems[i].ProductCode;
                            //    string Guid = System.Guid.NewGuid().ToString();
                            T_WDTGoods cofig = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == goods_no);
                            string goods_name = "";
                            if (cofig != null)
                            {
                                goods_name = cofig.goods_name;
                            }
                            if (order_list == "")
                            {
                                order_list += "{" +
                                   "\"oid\": \"" + tid + "\"," +
                                   "\"num\": \"" + num + "\"," +
                                   "\"price\": \"" + UnitPrice + "\"," +
                                   "\"status\": \"40\"," +
                                   "\"refund_status\": \"0\"," +
                                      "\"adjust_amount\": \"0\"," +
                                          "\"discount\": \"0\"," +
                                                "\"share_discount\": \"0\"," +
                                   "\"goods_id\": \"" + goods_no + "\"," +
                                   "\"goods_no\": \"" + goods_no + "\"," +
                                   "\"spec_no\": \"" + goods_no + "\"," +
                                   "\"goods_name\": \"" + goods_name + "\"," +
                                   "\"cid\": \"\"" +
                               "}";
                            }
                            else
                            {
                                order_list += ",{" +
                                     "\"oid\": \"" + tid + "\"," +
                                     "\"num\": \"" + num + "\"," +
                                     "\"price\": \"" + UnitPrice + "\"," +
                                     "\"status\": \"40\"," +
                                     "\"refund_status\": \"0\"," +
                                         "\"adjust_amount\": \"0\"," +
                                           "\"discount\": \"0\"," +
                                              "\"share_discount\": \"0\"," +
                                      "\"goods_id\": \"" + goods_no + "\"," +
                                     "\"goods_no\": \"" + goods_no + "\"," +
                                     "\"spec_no\": \"" + goods_no + "\"," +
                                     "\"goods_name\": \"" + goods_name + "\"," +
                                     "\"cid\": \"\"" +
                                 "}";
                            }
                        }
                        //旺店通
                    T_WDTshop queryData = db.T_WDTshop.SingleOrDefault(a => a.shop_name == ShopName);
                    if (queryData != null)
                    {
                        string Code = queryData.shop_no;
                        dic.Add("shop_no", Code);
                    }
                    else
                    {
                        return "0";
                    }
                 
                  
                    
                        string cmd = "[{" +
                            "\"tid\": \"" + tid + "\"," +
                    "\"trade_status\": \"30\"," +
                    "\"pay_status\": \"2\"," +
                    "\"delivery_term\": \"1\"," +
                    "\"trade_time\": \"" + trade_time + "\"," +
                    "\"pay_time\": \"" + pay_time + "\"," +
                    "\"buyer_nick\": \"" + buyer_nick + "\"," +
                    "\"buyer_email\": \"\"," +
                    "\"receiver_name\": \"" + receiver_name + "\"," +
                    "\"receiver_province\": \"" + receiver_province + "\"," +
                    "\"receiver_city\": \"" + receiver_city + "\"," +
                    "\"receiver_district\": \"" + receiver_district + "\"," +
                    "\"receiver_address\": \"" + receiver_address + "\"," +
                    "\"receiver_mobile\": \"" + receiver_mobile + "\"," +
                    "\"receiver_zip\": \"\"," +
                    "\"logistics_type\": \"8\"," +
                    "\"buyer_message\": \"\"," +
                    "\"seller_memo\": \"" + seller_memo + "\"," +
                    "\"post_amount\": \"0\"," +
                    "\"cod_amount\": \"0\"," +
                    "\"ext_cod_fee\": \"0\"," +
                    "\"paid\": \"" + paid + "\"," +
                    "\"order_list\": [" + order_list + "]}]";
                        dic.Remove("trade_list");
                        dic.Remove("sid");
                        dic.Remove("appkey");
                        dic.Remove("timestamp");
                        dic.Add("trade_list", cmd);
                        dic.Add("sid", "hhs2");
                        dic.Add("appkey", "hhs2-ot");
                        dic.Add("timestamp", GetTimeStamp());
                        aa = CreateParam(dic, true);

                        string ret = Post("http://api.wangdian.cn/openapi2/trade_push.php", aa);


                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string sd = jsonData[0].ToString();
                        if (sd == "0")
                        {
                            int sdz = int.Parse(jsonData[2].ToString());
                            sc.Complete();
                            if (sdz > 0)
                            {
                                return "1";
                            }
                           
                        }
                        return "0";
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
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


        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
            string strMd5 = BitConverter.ToString(result);
            strMd5 = strMd5.Replace("-", "");
            return strMd5;// System.Text.Encoding.Default.GetString(result);
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
        public string Post(string url, string postData)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream serviceRequestBodyStream = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bodyBytes = encoding.GetBytes(postData);
                request.ContentLength = bodyBytes.Length;
                using (serviceRequestBodyStream = request.GetRequestStream())
                {
                    serviceRequestBodyStream.Write(bodyBytes, 0, bodyBytes.Length);
                    serviceRequestBodyStream.Close();
                    using (response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string result = reader.ReadToEnd();
                            reader.Close();
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }

        }
    }
}
