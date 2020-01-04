using EBMS.App_Code;
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
    public class ReceiptController : BaseController
    {


        #region 公共属性/字段/方法

        EBMSEntities db = new EBMSEntities();

        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }

        public class Recipts
        {
            public int ID { get; set; }
            public string OrderCode { get; set; }
            public string Receivings { get; set; }
            public string Vip_Name { get; set; }
            public string StorName { get; set; }
            public decimal Money { get; set; }
            public System.DateTime Date { get; set; }
            public System.DateTime ShopDate { get; set; }
            public int PrintCount { get; set; }
            public string Code { get; set; }
            public string PostUser { get; set; }
            public List<T_ReceiptDetails> receiptDetails { get; set; }
        }
        #endregion


        #region 视图


        [Description("收款收据新增")]
        public ActionResult ViewReceiptAdd()
        {
            //T_OrderList order = db.T_OrderList.Find(ID);
            //T_Receipt model = new T_Receipt();
            //model.OrderCode = order.platform_code;
            //model.Receivings = order.receiver_name;
            //model.Money = Convert.ToDecimal(order.payment);
            //model.Vip_Name = order.vip_name;
            //model.StorName = order.shop_name;
            //model.OrderId = ID;
            return View();
        }

        [Description("收款收据编辑")]
        public ActionResult ViewReceiptEdit(int id = 0)
        {
            if (id == 0)
                return HttpNotFound();
            var model = db.T_Receipt.Find(id);
            return View(model);
        }

        [Description("收款收据详情")]
        public ActionResult ViewReceiptDetail(int receiptId)
        {
            ViewData["receiptId"] = receiptId;
            return View();
        }

        [Description("收款收据列表")]
        public ActionResult ViewReceiptList()
        {
            return View();
        }

        [Description("订单列表")]
        public ActionResult ViewOrderList()
        {
            return View();
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult ViewReceiptPrint(int id, int page = 1)
        {
            if (id == 0)
                return HttpNotFound();
            T_Receipt model = db.T_Receipt.Find(id);
            IQueryable<T_ReceiptDetails> list = db.T_ReceiptDetails.Where(s => s.ReceiptOrderID == model.ID);
            int totalRows = list.Count();
            int total = 0;
            string href = "";
            if (totalRows % 10 != 0)
                total = totalRows / 10 + 1;
            else
                total = totalRows / 6;
            for (int i = 1; i <= total; i++)
            {
                href += "<a href=\"?page=" + i + "&id=" + id + "\">   " + i + "   </a>";
            }
            Recipts models = new Recipts
            {
                ID = model.ID,
                StorName = model.StorName,
                ShopDate = model.ShopDate,
                Vip_Name = model.Vip_Name,
                Receivings = model.Receivings,
                Money = model.Money,
                receiptDetails = list.OrderByDescending(s => s.ID).Skip((page - 1) * 10).Take(10).ToList()
            };
            ViewData["pager"] = href;
            ViewData["page"] = page;
            ViewData["total"] = total;
            return View(models);
        }

        #endregion


        #region Post提交

        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }

        /// <summary>
        /// 查询管易订单
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public JsonResult GetReceiptByGy(string code)
        {
            GY gy = new GY();
            string cmd = "";

            cmd = "{" +
                  "\"appkey\":\"171736\"," +
                  "\"method\":\"gy.erp.trade.get\"," +
                  "\"page_no\":1," +
                  "\"page_size\":10," +
                  "\"platform_code\":\"" + code + "\"," +
                  "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +  
                  "}";

            string sign = gy.Sign(cmd);
            cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);

            if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
            {
                cmd = "{" +
                "\"appkey\":\"171736\"," +
                "\"method\":\"gy.erp.trade.history.get\"," +
                "\"page_no\":1," +
                "\"page_size\":10," +
                "\"platform_code\":\"" + code + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                {
                    return Json(new { State = "Faile", Message = "订单号不存在" });
                }
            }
            JsonData jsonOrders = jsonData["orders"][0];
            //订单信息 
            string platform_code = isNULL(jsonOrders["platform_code"]).ToString();
            //店铺名称
            string shop_name = isNULL(jsonOrders["shop_name"]).ToString();
            //旺旺帐号 
            string vip_name = isNULL(jsonOrders["vip_name"]).ToString();
            string receiver_name = isNULL(jsonOrders["receiver_name"]).ToString();
            string payment = isNULL(jsonOrders["payment"]).ToString();
            T_Receipt model = new T_Receipt();
            model.OrderCode = code;
            model.Receivings = receiver_name;
            model.Money = decimal.Parse(payment);
            model.Vip_Name = receiver_name;
            model.StorName = shop_name;
            List<T_ReceiptDetails> DetailsList = new List<T_ReceiptDetails>();

            JsonData jsonDetails = jsonOrders["details"];
            for (int i = 0; i < jsonDetails.Count; i++)
            {
                T_ReceiptDetails DetailsModel = new T_ReceiptDetails();
                DetailsModel.ProductName = jsonDetails[i]["item_name"] == null ? "" : jsonDetails[i]["item_name"].ToString();
                DetailsModel.Unit = jsonDetails[i]["item_simple_name"] == null ? "" : jsonDetails[i]["item_simple_name"].ToString();
                DetailsModel.Qty = jsonDetails[i]["qty"] == null ? 0 : int.Parse(jsonDetails[i]["qty"].ToString());
                DetailsList.Add(DetailsModel);
            }
            var json = new
            {

                rows = (from r in DetailsList
                        select new T_ReceiptDetails
                        {
                            ProductName = r.ProductName,
                            Unit = r.Unit,
                            Qty = r.Qty
                        }).ToArray()
            };
            return Json(new { State = "Success", ModelList = model, Json = json }, JsonRequestBehavior.AllowGet);
        }

        public ContentResult GetViewReciptList(Lib.GridPager pager, string orderCode, string startTime, string endTime)
        {
            IQueryable<T_Receipt> list = db.T_Receipt.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(orderCode))
                list = list.Where(s => s.OrderCode.Equals(orderCode));
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = Convert.ToDateTime(startTime + " 00:00:00");
                list = list.Where(s => s.Date >= start);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = Convert.ToDateTime(endTime + " 23:59:59");
                list = list.Where(s => s.Date <= end);
            }

            pager.totalRows = list.Count();
            List<T_Receipt> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

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

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetOrderDetail(int ID)
        {
            T_OrderList order = db.T_OrderList.Find(ID);
            var List = db.T_OrderDetail.Where(s => s.oid == order.code).Select(s => new { Name = s.item_name, Unit = s.item_simple_name, Qty = s.qty });
            return Json(List, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 收据详情
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetReceiptDetail(int receiptId, Lib.GridPager pager)
        {
            List<T_ReceiptDetails> list = db.T_ReceiptDetails.Where(s => s.ReceiptOrderID == receiptId).ToList();
            pager.totalRows = list.Count();
            List<T_ReceiptDetails> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData) + "}";
            return Content(json);
        }

        /// <summary>
        /// Add保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewReceiptAddSave(T_Receipt model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    GY gy = new GY();
                    string cmd = "";

                    cmd = "{" +
                          "\"appkey\":\"171736\"," +
                          "\"method\":\"gy.erp.trade.get\"," +
                          "\"page_no\":1," +
                          "\"page_size\":10," +
                          "\"platform_code\":\"" + model.OrderCode + "\"," +
                          "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                          "}";

                    string sign = gy.Sign(cmd);
                    cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                    string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                    JsonData jsonData = null;
                    jsonData = JsonMapper.ToObject(ret);

                    if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                    {
                        cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.trade.history.get\"," +
                        "\"page_no\":1," +
                        "\"page_size\":10," +
                        "\"platform_code\":\"" + model.OrderCode + "\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                        "}";
                        sign = gy.Sign(cmd);
                        cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                        ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                        jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                        {
                            return Json(new { State = "Faile", Message = "订单号不存在" });
                        }
                    }
                    JsonData jsonOrders = jsonData["orders"][0];
                    List<T_ReceiptDetails> details = Com.Deserialize<T_ReceiptDetails>(jsonStr);
                    //T_OrderList order = db.T_OrderList.Find(model.OrderId);
                    string code = "SKSJ";
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    List<T_Receipt> list = db.T_Receipt.Where(s => s.Code.Contains(date)).OrderByDescending(s => s.ID).ToList();
                    if (list.Count == 0)
                    {
                        code += date + "0001";
                    }
                    else
                    {
                        string old = list[0].Code.Substring(12);
                        int newCode = int.Parse(old) + 1;
                        code += date + newCode.ToString().PadLeft(4, '0');
                    }
                    model.Code = code;
                    model.Date = DateTime.Now;
                    model.IsDelete = 0;
                    model.ShopDate = DateTime.Parse(jsonOrders["createtime"].ToString());
                    model.PostUser = UserModel.Nickname;
                    model.PrintCount = 0;
                    db.T_Receipt.Add(model);
                    db.SaveChanges();
                    if (details != null)
                    {
                        foreach (var item in details)
                        {
                            item.ReceiptOrderID = model.ID;
                            db.T_ReceiptDetails.Add(item);
                        }
                        db.SaveChanges();
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

        /// <summary>
        /// Edit保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewReceiptEditSave(T_Receipt model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ReceiptDetails> details = Com.Deserialize<T_ReceiptDetails>(jsonStr);
                    var receipt = db.T_Receipt.Find(model.ID);
                    receipt.Receivings = model.Receivings;
                    receipt.Vip_Name = model.Vip_Name;
                    receipt.StorName = model.StorName;
                    receipt.Money = model.Money;
                    db.SaveChanges();
                    if (details != null)
                    {
                        foreach (var item in details)
                        {
                            var itemdetail = db.T_ReceiptDetails.Find(item.ID);
                            itemdetail.ProductName = item.ProductName;
                            itemdetail.Qty = item.Qty;
                            itemdetail.Unit = item.Unit;
                        }
                        db.SaveChanges();
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


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RecieptDelete(int id)
        {
            try
            {
                T_Receipt model = db.T_Receipt.Find(id);
                model.IsDelete = 1;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 修改打印次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateprintCountByID(int id)
        {
            try
            {
                T_Receipt model = db.T_Receipt.Find(id);
                model.PrintCount += 1;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        #endregion

    }
}
