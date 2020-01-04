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
    public class MailSampleController : BaseController
    {
        //
        // GET: /MailSample/
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
        public ActionResult ViewMailSampleAdd()
        {
            ViewData["ExpressCodeList"] = App_Code.Com.ExpressName();
            ViewData["DeliverGoodsWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["ShopNameList"] = App_Code.Com.Shop();
            return View();
        }


        public ActionResult ViewMailSampleReportCheck(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_MailSample model = db.T_MailSample.SingleOrDefault(a => a.ID == ID);
            model.DeliverGoodsWarehouse = GetWarehouseString(model.DeliverGoodsWarehouse);
            if (model.IsReturn == "0")
            {
                model.IsReturn = "是";
            }
            else
            {
                model.IsReturn = "否";
            }
            
            return View(model);
        }
        public ActionResult ViewMailSampleCheck()
        {
            return View();
        }
        public ActionResult ViewMailSampleDetail(int ID)
        {
            ViewData["ID"] = ID;
            GetApproveHistory(ID);

            T_MailSample model = db.T_MailSample.SingleOrDefault(a => a.ID == ID);
            model.DeliverGoodsWarehouse = GetWarehouseString(model.DeliverGoodsWarehouse);
            if (model.IsReturn == "0")
            {
                model.IsReturn = "是";
            }
            else
            {
                model.IsReturn = "否";
            }
            return View(model);
        }
        public ActionResult ViewMailSampleList()
        {
           
            return View();
        }

        [HttpPost]
        [Description("邮寄样品删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_MailSample model = db.T_MailSample.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_MailSample>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }

        //邮寄样品详情列表 
        [HttpPost]
        public ContentResult GetMailSampleDetailList(Lib.GridPager pager, int ID)
        {
            IQueryable<T_MailSampleDetails> queryData = db.T_MailSampleDetails.Where(a => a.Oid == ID);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MailSampleDetails> list = new List<T_MailSampleDetails>();
            foreach (var item in queryData)
            {
                T_MailSampleDetails i = new T_MailSampleDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        [Description("邮寄样品管理")]
        public ContentResult GetMailSampleList(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_MailSample> queryData = db.T_MailSample.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MailSample> list = new List<T_MailSample>();
            foreach (var item in queryData)
            {
                T_MailSample i = new T_MailSample();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_MailSampleAppRove.Where(a => a.Oid == id);
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
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewMailSample()
        {
            return View();
        }
        public ActionResult ViewMailSampleChecken()
        {
            return View();
        }
        

        [Description("我的邮寄样品")]
        public ContentResult GetMailSample(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_MailSample> queryData = db.T_MailSample.Where(a => a.ApplyName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MailSample> list = new List<T_MailSample>();
            foreach (var item in queryData)
            {
                T_MailSample i = new T_MailSample();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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

        [Description("邮寄样品审核查询")]
        public ContentResult GetMailSampleCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_MailSampleGroup> GroupModel = db.T_MailSampleGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_MailSampleAppRove> ApproveMod = db.T_MailSampleAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_MailSample r  where isdelete=0  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_MailSample> queryData = db.Database.SqlQuery<T_MailSample>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MailSample> list = new List<T_MailSample>();
            foreach (var item in queryData)
            {
                T_MailSample i = new T_MailSample();
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                i = item;
                list.Add(i);
            }
            //分页
            //  List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("新增保存")]
        public JsonResult MailSampleAdd(T_MailSample model, string jsonStr, string ExpenseNextApprove, string p_c_a)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    string orderNum = model.OrderNumber;
                    //if (orderNum == null || orderNum == "")
                    //{ 

                    //}
                    string Order = "8" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                  
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    model.GoodsAddress = p_c_a;
                    model.OrderNumber = Order;
                    model.ApplyName = Nickname;
                    model.ApplyDate = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.Isdelete = 0;
                    db.T_MailSample.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        List<T_MailSampleDetails> details = App_Code.Com.Deserialize<T_MailSampleDetails>(jsonStr);
                        foreach (var item in details)
                        {

                            item.Oid = model.ID;
                            db.T_MailSampleDetails.Add(item);
                        }
                        db.SaveChanges();
                      
                        T_MailSampleAppRove AppRoveModel = new T_MailSampleAppRove();
                        AppRoveModel.Status = -1;
                        AppRoveModel.Step = "0";
                         AppRoveModel.ApproveName = ExpenseNextApprove;
                        AppRoveModel.ApproveDName ="部门主管";
                        AppRoveModel.Oid = model.ID;
                        db.T_MailSampleAppRove.Add(AppRoveModel);
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


        //邮寄样品已审核列表  
        [HttpPost]
        public ContentResult GetMailSampleCheckenList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_MailSampleAppRove> ApproveMod = db.T_MailSampleAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_MailSample> queryData = from r in db.T_MailSample
                                              where Arry.Contains(r.ID) && r.Isdelete == 0
                                              select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.OrderNumber != null && a.OrderNumber.Contains(queryStr)));
            }
         
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_MailSample> list = new List<T_MailSample>();
            foreach (var item in queryData)
            {
                T_MailSample i = new T_MailSample();
                i = item;
                item.DeliverGoodsWarehouse = GetWarehouseString(item.DeliverGoodsWarehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 根据部门选择下级审核人
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetApproveByDeptID()
        {

            T_MailSampleGroup list = db.T_MailSampleGroup.SingleOrDefault(s => s.GroupName == "部门主管");
            string[] listCrew = list.Crew.Split(',');
            Dictionary<string, string> listCrewModel = new Dictionary<string, string>();
            for (int i = 0; i < listCrew.Length; i++)
            {
                string name = listCrew[i];

                listCrewModel.Add(name, name);
            }

            return Json(listCrewModel.ToArray());

        }
        //审核 
        public JsonResult MailSampleCheckCheck(T_MailSample model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_MailSample Invoicemodel = db.T_MailSample.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_MailSampleAppRove modelApprove = db.T_MailSampleAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_MailSampleAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_MailSampleAppRove newApprove = new T_MailSampleAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_MailSampleConfig> config = db.T_MailSampleConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_MailSampleConfig stepMod = db.T_MailSampleConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_MailSampleAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {

                          

                            Invoicemodel.Status = int.Parse(status);
                          
                                List<T_MailSampleDetails> DetailsList = db.T_MailSampleDetails.Where(a => a.Oid == ID).ToList();
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
                                                "\"express_code\":\"" + Invoicemodel.ExpressCode + "\"," +
                                                "\"warehouse_code\":\"" + Invoicemodel.DeliverGoodsWarehouse + "\"," +
                                                "\"receiver_province\":\"" + receiver_province + "\"," +
                                                "\"receiver_city\":\"" + receiver_city + "\"," +
                                                "\"receiver_district\":\"" + receiver_district + "\"," +
                                                "\"vip_code\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"vip_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"receiver_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"receiver_address\":\"" + Invoicemodel.GoodsReceiptAddress + "\"," +
                                                "\"receiver_mobile\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                                "\"receiver_phone\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                                "\"deal_datetime\":\"" + datetime + "\"," +
                                                "\"seller_memo\":\"" + Invoicemodel.DeliverGoodsReason + "\"," +
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
                                                "\"express_code\":\"" + Invoicemodel.ExpressCode + "\"," +
                                                "\"warehouse_code\":\"" + Invoicemodel.DeliverGoodsWarehouse + "\"," +
                                                "\"receiver_province\":\"" + receiver_province + "\"," +
                                                "\"receiver_city\":\"" + receiver_city + "\"," +
                                                "\"receiver_district\":\"" + receiver_district + "\"," +
                                                "\"vip_code\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"vip_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"receiver_name\":\"" + Invoicemodel.GoodsReceiptName + "\"," +
                                                "\"receiver_address\":\"" + Invoicemodel.GoodsReceiptAddress + "\"," +
                                                "\"receiver_mobile\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                                "\"receiver_phone\":\"" + Invoicemodel.GoodsReceiptPhone + "\"," +
                                                "\"deal_datetime\":\"" + datetime + "\"," +
                                                "\"seller_memo\":\"" + Invoicemodel.DeliverGoodsReason + "\"," +
                                             "\"sign\":\"" + sign + "\"," +
                                             "\"details\":[" + shangp + "]" +
                                        "}";
                                string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                                JsonData jsonData = null;
                                jsonData = JsonMapper.ToObject(ret);
                                string sd = jsonData[0].ToString();
                          



                        }
                       

                        Invoicemodel.Step = step;
                        db.Entry<T_MailSample>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        if (j > 0)
                        {
                            sc.Complete();
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
                        db.Entry<T_MailSample>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        sc.Complete();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }


                return Json(result, JsonRequestBehavior.AllowGet);
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
    }
}
