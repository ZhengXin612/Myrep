using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class ManualBillingController : BaseController
    {
        //
        // GET: /ManualBilling/
        EBMSEntities db = new EBMSEntities();

        public string GetWarehouseString(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {

                List<T_Warehouses> model = db.T_Warehouses.Where(a => a.code == code).ToList();
                if (model.Count > 0)
                {
                    return model[0].name;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        public string GetShopFromString(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {

                List<T_ShopFromGY> model = db.T_ShopFromGY.Where(a => a.name == code).ToList();
                if (model.Count > 0)
                {
                    return model[0].code;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        public ActionResult ManualBillingAdd()
        {
            ViewData["PaymentMethodList"] = App_Code.Com.PaymentType();
            ViewData["ExpressCodeList"] = App_Code.Com.ExpressName();
            ViewData["DeliverGoodsWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["ShopNameList"] = App_Code.Com.Shop();
            return View();
        }
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ManualBillingMy()
        {
        
            return View();
        }
        public ActionResult ManualBillingList()
        {

            return View();
        }
        public ActionResult ManualBillingCheck()
        {

            return View();
        }
        public ActionResult ManualBillingChecken()
        {

            return View();
        }
        
        public ActionResult ManualBillingDetail(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_ManualBilling model = db.T_ManualBilling.SingleOrDefault(a => a.ID == ID);
            model.DeliverGoodsWarehouse = GetWarehouseString(model.DeliverGoodsWarehouse);    

            return View(model);
        }
        
        public ActionResult ManualBillingReportCheck(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_ManualBilling model = db.T_ManualBilling.SingleOrDefault(a=>a.ID==ID);
            model.DeliverGoodsWarehouse = GetWarehouseString(model.DeliverGoodsWarehouse);    

            return View(model);
        }
        
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_goodsGY> queryData = db.T_goodsGY.Where(a => a.combine == "False").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr) || a.code != null && a.code.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_goodsGY> list = new List<T_goodsGY>();
            foreach (var item in queryData)
            {
                T_goodsGY i = new T_goodsGY();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_ManualBillingAppRove.Where(a => a.Oid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=#d02e2e>未审核</font>";
                if (item.Status == 1) s = "<font color=#1fc73a>已同意</font>";
                if (item.Status == 2) s = "<font color=#d02e2e>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
        }

        //手工单详情列表 
        [HttpPost]
        public ContentResult GetManualBillingDetailList(Lib.GridPager pager, int ID)
        {
            IQueryable<T_ManualBillingDetails> queryData = db.T_ManualBillingDetails.Where(a => a.Oid == ID);
        
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualBillingDetails> list = new List<T_ManualBillingDetails>();
            foreach (var item in queryData)
            {
                T_ManualBillingDetails i = new T_ManualBillingDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }

        [Description("我的手工订单")]
        public ContentResult GetManualBilling(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_ManualBilling> queryData = db.T_ManualBilling.Where(a => a.ApplyName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualBilling> list = new List<T_ManualBilling>();
            foreach (var item in queryData)
            {
                T_ManualBilling i = new T_ManualBilling();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
           // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("手工订单管理")]
        public ContentResult GetManualBillingList(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_ManualBilling> queryData = db.T_ManualBilling.Where(a=>a.Isdelete==0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualBilling> list = new List<T_ManualBilling>();
            foreach (var item in queryData)
            {
                T_ManualBilling i = new T_ManualBilling();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //手工订单已审核列表  
        [HttpPost]
        public ContentResult GetManualBillingCheckenList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_ManualBillingAppRove> ApproveMod = db.T_ManualBillingAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_ManualBilling> queryData = from r in db.T_ManualBilling
                                                 where Arry.Contains(r.ID) && r.Isdelete == 0
                                                 select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.OrderNumber != null && a.OrderNumber.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualBilling> list = new List<T_ManualBilling>();
            foreach (var item in queryData)
            {
                T_ManualBilling i = new T_ManualBilling();
                i = item;
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("手工订单审核查询")]
        public ContentResult GetManualBillingCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_ManualBillingGroup> GroupModel = db.T_ManualBillingGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_ManualBillingAppRove> ApproveMod = db.T_ManualBillingAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_ManualBilling r  where isdelete=0  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_ManualBilling> queryData = db.Database.SqlQuery<T_ManualBilling>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualBilling> list = new List<T_ManualBilling>();
            foreach (var item in queryData)
            {
                T_ManualBilling i = new T_ManualBilling();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
            //分页
          //  List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [HttpPost]
        [Description("手工单删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_ManualBilling model = db.T_ManualBilling.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_ManualBilling>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Description("新增保存")]
        public JsonResult ViewManualBillingAdd(T_ManualBilling model, string jsonStr, string p_c_a)
        {
              using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    string orderNum = model.OrderNumber;
                    //if (orderNum == null || orderNum == "")
                    //{ 
                    
                    //}
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    model.ApplyName = Nickname;
                    model.ApplyDate = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.Isdelete = 0;
                    model.GoodsAddress = p_c_a;
                    db.T_ManualBilling.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        List<T_ManualBillingDetails> details = App_Code.Com.Deserialize<T_ManualBillingDetails>(jsonStr);
                        foreach (var item in details)
                        {

                            item.Oid = model.ID;
                            db.T_ManualBillingDetails.Add(item);
                        }
                        db.SaveChanges();
                        T_ManualBillingConfig modelconfig = db.T_ManualBillingConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                        T_ManualBillingAppRove AppRoveModel = new T_ManualBillingAppRove();
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
                        db.T_ManualBillingAppRove.Add(AppRoveModel);
                        db.SaveChanges();

                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);


                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //审核 
        public JsonResult ManualBillingCheckCheck(T_ManualBilling model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_ManualBilling Invoicemodel = db.T_ManualBilling.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_ManualBillingAppRove modelApprove = db.T_ManualBillingAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_ManualBillingAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_ManualBillingAppRove newApprove = new T_ManualBillingAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_ManualBillingConfig> config = db.T_ManualBillingConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_ManualBillingConfig stepMod = db.T_ManualBillingConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_ManualBillingAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                           
                            List<T_ManualBillingDetails> DetailsList = db.T_ManualBillingDetails.Where(a => a.Oid == ID).ToList();
                            string shangp = "";
                            for (int e = 0; e < DetailsList.Count; e++)
                            {
                                if (e == DetailsList.Count - 1)
                                {
                                   
                                    shangp += "{\"qty\":" + DetailsList[e].qty + ",\"note\":\"\",\"refund\":0,\"item_code\":\"" + System.Web.HttpContext.Current.Server.UrlEncode(DetailsList[e].Code) + "\"}";

                                }
                                else
                                {
                                   
                                    shangp += "{\"qty\":" + DetailsList[e].qty + ",\"note\":\"\",\"refund\":0,\"item_code\":\"" + System.Web.HttpContext.Current.Server.UrlEncode(DetailsList[e].Code) + "\"},";
                                }
                            }
                           decimal  PaymentAmount = 0;
                            if (Invoicemodel.PaymentAmount != null)
                            {
                                PaymentAmount =decimal.Parse(Invoicemodel.PaymentAmount.ToString());
                            }
                           
                         //   string payments = "{\"pay_type_code\":wangyin,\"payment\":" + PaymentAmount + "},";
                            string  payments = "{\"pay_type_code\":\"wangyin\",\"payment\":\"" + PaymentAmount + "\"}";
                            string[] address = Invoicemodel.GoodsAddress.Split('-');
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


                            EBMS.App_Code.GY gy = new App_Code.GY();
                            string ShopCode = GetShopFromString(Invoicemodel.ShopName);
                            string datetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            string cmd = "";
                            cmd = "{" +
                                           "\"appkey\":\"171736\"," +
                                            "\"method\":\"gy.erp.trade.add\"," +
                                            "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                            "\"platform_code\":\"" + Invoicemodel.OrderNumber + "\"," +
                                            "\"order_type_code\":\"Sales\"," +
                                            "\"shop_code\":\"" + ShopCode + "\"," +
                                             "\"post_fee\":\"" + Invoicemodel.ExpressFee + "\"," +
                                        
                                            "\"receiver_province\":\"" + receiver_province + "\"," +
                                            "\"receiver_city\":\"" + receiver_city + "\"," +
                                            "\"receiver_district\":\"" + receiver_district + "\"," +
                                            "\"express_code\":\"" + Invoicemodel.ExpressCode + "\"," +
                                            "\"warehouse_code\":\"" + Invoicemodel.DeliverGoodsWarehouse + "\"," +
                                            "\"vip_code\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"vip_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"receiver_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"receiver_address\":\"" + Invoicemodel.GoodsReceiptAddress + "\"," +
                                            "\"receiver_mobile\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                            "\"receiver_phone\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                            "\"deal_datetime\":\"" + datetime + "\"," +
                                            "\"seller_memo\":\"" + Invoicemodel.DeliverGoodsReason + "\"," +
                                           "\"payments\":[" + payments + "]," +
                                              "\"details\":[" + shangp + "]" +
                                    "}";
                            string sign = gy.Sign(cmd);
                            string comcode = "{" +
                                            "\"appkey\":\"171736\"," +
                                            "\"method\":\"gy.erp.trade.add\"," +
                                            "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                            "\"platform_code\":\"" + Invoicemodel.OrderNumber + "\"," +
                                            "\"order_type_code\":\"Sales\"," +
                                            "\"shop_code\":\"" + ShopCode + "\"," +
                                            "\"post_fee\":\"" + Invoicemodel.ExpressFee + "\"," +
                                         
                                            "\"receiver_province\":\"" + receiver_province + "\"," +
                                            "\"receiver_city\":\"" + receiver_city + "\"," +
                                            "\"receiver_district\":\"" + receiver_district + "\"," +
                                            "\"express_code\":\"" + Invoicemodel.ExpressCode + "\"," +
                                            "\"warehouse_code\":\"" + Invoicemodel.DeliverGoodsWarehouse + "\"," +
                                            "\"vip_code\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"vip_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"receiver_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                            "\"receiver_address\":\"" + Invoicemodel.GoodsReceiptAddress + "\"," +
                                            "\"receiver_mobile\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                            "\"receiver_phone\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                            "\"deal_datetime\":\"" + datetime + "\"," +
                                            "\"seller_memo\":\"" + Invoicemodel.DeliverGoodsReason + "\"," +
                                         "\"sign\":\"" + sign + "\"," +
                                         "\"payments\":[" + payments + "]," +
                                         "\"details\":[" + shangp + "]" +
                                    "}";
                            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                            JsonData jsonData = null;
                            jsonData = JsonMapper.ToObject(ret);
                            string sd = jsonData[0].ToString();
                            if (jsonData.Count == 6)
                            {
                               // return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                                result = "管理调整单失败，请核对信息";
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                                Invoicemodel.Status = int.Parse(status);

                           


                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_ManualBilling>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
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
                    else
                    {
                       
                        //不同意
                        Invoicemodel.Step = 0;
                        Invoicemodel.Status = 2;
                        db.Entry<T_ManualBilling>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                       int j= db.SaveChanges();
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

    }
}
