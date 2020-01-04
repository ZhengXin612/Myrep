using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.IO;
using System.Data;
using EBMS.App_Code;
using Newtonsoft.Json;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using NPOI.SS.UserModel;
using LitJson;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Data.Entity;

namespace EBMS.Controllers
{
    public class AliExpressOrderController : BaseController
    {
        //
        // GET: /AliExpressOrder/
        EBMSEntities db = new EBMSEntities();

        public ActionResult Index()
        {
            return View();
        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        #region 视图


        [Description("速卖通订单新增")]
        public ActionResult ViewAliExpressOrderAdd()
        {
            return View();
        }
        [Description("汇率新增")]
        public ActionResult HTExchangeRateAdd()
        {
            return View();
        }
        [Description("汇率管理")]
        public ActionResult HTExchangeRateList()
        {
            return View();
        }
        [Description("编辑Lazada备注")]
        public ActionResult ViewLazadaEditRemark(int id)
        {
            T_LazadaOrder lazada = db.T_LazadaOrder.Find(id);
            ViewData["id"] = lazada.ID;
            ViewData["remark"] = lazada.SellRemark;
            return View();
        }
        [Description("编辑Ebey备注")]
        public ActionResult ViewEbeyEditRemark(int id)
        {
            T_HtEbey lazada = db.T_HtEbey.Find(id);
            ViewData["id"] = lazada.ID;
            ViewData["remark"] = lazada.SellRemark;
            return View();
        }

        [Description("编辑Shopee备注")]
        public ActionResult ViewShopeeEditRemark(int id)
        {
            T_HtShopee lazada = db.T_HtShopee.Find(id);
            ViewData["id"] = lazada.ID;
            ViewData["remark"] = lazada.Remarks;
            return View();
        }
        [Description("速卖通订单列表")]
        public ActionResult ViewAliExpressOrderList()
        {
            return View();
        }
        [Description("AliExpress订单列表")]
        public ActionResult AliExpressGrid(string platform="")
        {
			ViewData["platform"] = platform;

			return View();
        }
        [Description("Lazada订单列表")]
        public ActionResult ViewLazadaOrderList()
        {
            return View();
        }


        [Description("Ebey订单列表")]
        public ActionResult EbeyGrid()
        {
            return View();
        }
        [Description("Shopee订单列表")]
        public ActionResult ShopeeGuid()
        {
            return View();
        }

        [Description("Eyey订单详情列表")]
        public ActionResult ViewEbeyItemsList(string orderCode, string shopName)
        {
            ViewData["OrderCode"] = orderCode;
            ViewData["shopName"] = shopName;
            return View();
        }
        [Description("Shopee订单详情列表")]
        public ActionResult ViewShopeeItemsList(string orderCode, string shopName)
        {
            ViewData["OrderCode"] = orderCode;
            ViewData["shopName"] = shopName;
            return View();
        }
        [Description("导入excel")]
        public ActionResult ViewAliImportExcel(int flag)
        {
            ViewData["flag"] = flag;//flag区分是导入科源速卖通还是好护士速卖通还是运单excel
            return View();
        }
        public ActionResult ViewAliExcel()
        {
          
            return View();
        }
        
        [Description("速卖通编辑")]
        public ActionResult ViewAliExpressEdit(int ID)
        {
            T_AliExpressOrder model = db.T_AliExpressOrder.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("速卖通详情")]
        public ActionResult ViewAliExpressDetail(int ID)
        {
            T_AliExpressOrder model = db.T_AliExpressOrder.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("Lazada订单详情列表")]
        public ActionResult ViewLazadaItemsList(string orderCode, string shopName)
        {
            ViewData["OrderCode"] = orderCode;
            ViewData["shopName"] = shopName;
            return View();
        }
        [Description("Lazada订单编辑")]
        public ActionResult ViewLazadaOrderEdit(int ID)
        {
            T_LazadaOrder model = db.T_LazadaOrder.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("Lazada地址维护")]
        public ActionResult LazadaConfigAddress()
        {
            return View();
        }
        [Description("LazadaSku维护")]
        public ActionResult LazadaConfigSku()
        {
            return View();
        }
        [Description("Lazada地址编辑")]
        public ActionResult ViewLazadaConfigEdit(int ID)
        {
            T_LazadaOrderConfig model = db.T_LazadaOrderConfig.Find(ID);
            if (model != null)
            {
                ViewData["expres"] = Com.WDTExpressName(model.Express);
                ViewData["warehouse"] = Com.Warehouses(model.Warhouse);
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("Lazada地址新增")]
        public ActionResult ViewLazadaConfigAdd()
        {
            ViewData["express"] = Com.ExpressName();
            ViewData["warhouse"] = Com.Warehouses();
            return View();
        }

        [Description("LazadaSKu新增")]
        public ActionResult ViewLazadaSkuAdd()
        {
            return View();
        }

        [Description("LazadaSKu编辑")]
        public ActionResult ViewLazadaSkuEdit(int ID)
        {
            T_LazadaGyCode model = db.T_LazadaGyCode.Find(ID);
            if (model == null)
                return HttpNotFound();
            else
                return View(model);
        }

        #endregion
        #region 绑定数据
        public IQueryable<T_AliExpressOrder> getQueryData(string OrderNO, string startDate, string EndDate, string shopName)
        {
            IQueryable<T_AliExpressOrder> queryData = db.T_AliExpressOrder.Where(a => a.isDelete == "0");
            if (!string.IsNullOrWhiteSpace(OrderNO))
            {
                queryData = queryData.Where(a => a.orderNumber != null && a.orderNumber.Contains(OrderNO));
            }
            if (startDate != "" && startDate != null)
            {
                DateTime sdate = DateTime.Parse(startDate);
                queryData = queryData.Where(a => a.paymentTime >= sdate);
            }
            if (EndDate != "" && EndDate != null)
            {
                DateTime Edate = DateTime.Parse(EndDate);
                Edate = Edate.AddDays(1);
                queryData = queryData.Where(a => a.paymentTime <= Edate);
            }

            if (shopName == "1")
            {
                queryData = queryData.Where(a => a.isKeyuan == "1" || a.isKeyuan == null);
            }
            else if (shopName == "0")
            {
                queryData = queryData.Where(a => a.isKeyuan == "0");
            }
            return queryData;
        }
        //获取速卖通订单数据
        public ContentResult GetAliExpressList(Lib.GridPager pager, string OrderNO, string startDate, string EndDate, string shopName)
        {
            IQueryable<T_AliExpressOrder> queryData = getQueryData(OrderNO, startDate, EndDate, shopName);
            double orsum = 0;
            double toSum = 0;
            double plaSum = 0;
            double homeSum = 0;
            double forSum = 0;
            double proSum = 0;
            double refSum = 0;
            orsum = Convert.ToDouble(queryData.Sum(a => a.orderMoney == null ? 0 : a.orderMoney));
            toSum = Convert.ToDouble(queryData.Sum(a => a.totalProductValue == null ? 0 : a.totalProductValue));
            plaSum = Convert.ToDouble(queryData.Sum(a => a.platformCommission == null ? 0 : a.platformCommission));
            homeSum = Convert.ToDouble(queryData.Sum(a => a.homeFreight == null ? 0 : a.homeFreight));
            forSum = Convert.ToDouble(queryData.Sum(a => a.foreignFreight == null ? 0 : a.foreignFreight));
            proSum = Convert.ToDouble(queryData.Sum(a => a.profit == null ? 0 : a.profit));
            refSum = Convert.ToDouble(queryData.Sum(a => a.refund == null ? 0 : a.refund));


            List<T_AliExpressOrder> footers = new List<T_AliExpressOrder>();
            T_AliExpressOrder footer = new T_AliExpressOrder();
            footer.salesman = "合计:";
            footer.orderMoney = Math.Round(orsum, 2);
            footer.totalProductValue = Math.Round(toSum, 2);
            footer.platformCommission = Math.Round(plaSum, 2);
            footer.homeFreight = Math.Round(homeSum, 2);
            footer.foreignFreight = Math.Round(forSum, 2);
            footer.profit = Math.Round(proSum, 2);
            footer.refund = Math.Round(refSum, 2);
            footers.Add(footer);

            if (queryData != null)
            {
                List<T_AliExpressOrder> list = queryData.OrderByDescending(a => a.paymentTime).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footers, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取T_HTExchangeRate订单数据
        public ContentResult GetHTExchangeRateList(Lib.GridPager pager, string OrderNO, string shopName)
        {
            IQueryable<T_HTExchangeRate> queryData = db.Database.SqlQuery<T_HTExchangeRate>("select * from T_HTExchangeRate where ID in (select max(id) from  T_HTExchangeRate  group by Code ) ").AsQueryable();
            if (!string.IsNullOrWhiteSpace(OrderNO))
                queryData = queryData.Where(a => a.Code.Contains(OrderNO));
            if (!string.IsNullOrWhiteSpace(shopName))
                queryData = queryData.Where(a => a.TypeName.Contains(shopName));
            if (queryData != null)
            {
                List<T_HTExchangeRate> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取Ebey订单数据
        public ContentResult GetEbeyList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_HtEbey> queryData = db.T_HtEbey.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
                queryData = queryData.Where(a => a.OrderID.Contains(queryStr));
            if (queryData != null)
            {
                List<T_HtEbey> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取Shopee订单数据
        public ContentResult GetShopeeList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_HtShopee> queryData = db.T_HtShopee.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
                queryData = queryData.Where(a => a.OrderID.Contains(queryStr));
            if (queryData != null)
            {
                List<T_HtShopee> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取Ebey订单明细数据
        public ContentResult GetEbeyItemList(Lib.GridPager pager, string OrderCode, string shopname)
        {

            IQueryable<T_HtEbeyItem> queryData = db.T_HtEbeyItem.Where(a => a.PorderID == OrderCode);
            if (queryData != null)
            {
                List<T_HtEbeyItem> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取Shopee订单明细数据
        public ContentResult GetShopeeItemList(Lib.GridPager pager, string OrderCode, string shopname)
        {

            IQueryable<T_HtShopeeItem> queryData = db.T_HtShopeeItem.Where(a => a.PorderID == OrderCode);
            if (queryData != null)
            {
                List<T_HtShopeeItem> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取lazada订单数据
        public ContentResult GetLazadaList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_LazadaOrder> queryData = db.T_LazadaOrder.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
                queryData = queryData.Where(a => a.OrderNumber.Contains(queryStr));
            if (queryData != null)
            {
                List<T_LazadaOrder> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                foreach (T_LazadaOrder lazada in list)
                {
                    lazada.WarehouseName = Com.GetWarehouseName(lazada.WarehouseName);

                    lazada.logisticsName = Com.GetExpressName(lazada.logisticsName);
                }
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }

        //获取lazada订单明细数据
        public ContentResult GetLazadaItemList(Lib.GridPager pager, string OrderCode, string shopname)
        {

            IQueryable<T_LazadaOrderItem> queryData = db.T_LazadaOrderItem.Where(a => a.POrderId == OrderCode);
            if (queryData != null)
            {
                List<T_LazadaOrderItem> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }

        public ContentResult GetAliList(Lib.GridPager pager, string queryStr,int isDown=0,string platform="")
        {
            IQueryable<T_AliSumai> queryData = db.T_AliSumai;
            if (isDown == 0 || isDown == 1)
            {
                queryData = queryData.Where(a => a.IsDown == isDown);
            }
            if (!string.IsNullOrWhiteSpace(queryStr))
			{
				queryData = queryData.Where(a => a.OrderNO.Contains(queryStr));
			}

			if (!string.IsNullOrWhiteSpace(platform))
			{
				queryData = queryData.Where(a => a.Platform==platform);
			}
                List<T_AliSumai> list = queryData.OrderByDescending(a => a.OrderNO).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
               
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
           
        }
        #endregion
        #region 增删改
        #region 速卖通
        public JsonResult AliExpressOrderEditSave(T_AliExpressOrder model)//编辑
        {
            T_AliExpressOrder editModel = db.T_AliExpressOrder.Find(model.ID);
            if (editModel != null)
            {
                editModel.purchasePriceProduct = model.purchasePriceProduct;
                editModel.platformCommission = model.platformCommission;
                editModel.homeFreight = model.homeFreight;
                editModel.foreignFreight = model.foreignFreight;
                editModel.refund = model.refund;
                editModel.refundReason = model.refundReason;
                editModel.state = model.state;
                if (model.purchasePriceProduct != null && model.homeFreight != null && model.foreignFreight != null && model.refund != null)
                {
                    editModel.profit = Math.Round((double)editModel.orderMoney - (double)editModel.purchasePriceProduct - (double)editModel.platformCommission - (double)editModel.homeFreight - (double)editModel.foreignFreight - (double)editModel.refund, 2);
                }
                if (editModel.profit != null)
                {
                    editModel.profitRate = Math.Round(((double)editModel.profit / (double)editModel.orderMoney), 4);
                }
                db.Entry<T_AliExpressOrder>(editModel).State = System.Data.Entity.EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { State = "Faile", Message = "没有相关记录" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AliExpressOrderDelete(int ID)//删除
        {
            T_AliExpressOrder editModel = db.T_AliExpressOrder.Find(ID);
            if (editModel != null)
            {
                editModel.isDelete = "1";
                db.Entry<T_AliExpressOrder>(editModel).State = System.Data.Entity.EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { State = "Faile", Message = "没有相关记录" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region lazada订单
        //Shopee
        [HttpPost]
        public JsonResult ViewShopeeEditSave(string remark, int id)
        {
            try
            {
                T_HtShopee order = db.T_HtShopee.Find(id);
                order.Remarks = remark;
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Success", Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult ViewEbeyEditSave(string remark, int id)
        {
            try
            {
                T_HtEbey order = db.T_HtEbey.Find(id);
                order.SellRemark = remark;
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Success", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ViewLazadaEditSave(string remark, int id)
        {
            try
            {
                T_LazadaOrder order = db.T_LazadaOrder.Find(id);
                order.SellRemark = remark;
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Success", Message = ex.Message });
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



        [HttpPost]
        public JsonResult EbeyUpToGY(string ids)//上传到旺店通
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                var aa = "";
                try
                {
                    string[] Ids = ids.Split(',');
                    for (int j = 0; j < Ids.Length; j++)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        int id = Convert.ToInt32(Ids[j]);
                        T_HtEbey ordermodel = db.T_HtEbey.SingleOrDefault(a => a.ID == id);
                        if (ordermodel.Istijiao == 1)
                            return Json(new { State = "Faile", Message = "" + ordermodel.OrderID + "已提交到旺店通，不能重复提交" }, JsonRequestBehavior.AllowGet);


                        string ShopName = "";
                        if (ordermodel.shopName.Trim() == "Ebay美国")
                        {
                            ShopName = "4001";

                        }
                        else if (ordermodel.shopName.Trim() == "Ebay澳洲")
                        {
                            ShopName = "4002";

                        }
                        else if (ordermodel.shopName.Trim() == "Ebay英国")
                        {
                            ShopName = "4003";
                        }
                        else
                        {
                            return Json(new { State = "Faile", Message = "店铺问题请核查店铺名称" }, JsonRequestBehavior.AllowGet);
                        }
                        string tid = ordermodel.OrderID;
                        //int trade_status = 10;
                        //int pay_status = 2;
                        //int delivery_term = 1;
                        DateTime trade_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        DateTime pay_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        string buyer_nick = ordermodel.MemberName;
                        string receiver_name = ordermodel.DeliveryName;
                        string[] address = ordermodel.collectAddress.Split('-');
                        string receiver_province = address[0];
                        string receiver_city = address[1];
                        string receiver_district = address[2];
                        string receiver_address = ordermodel.address;
                        string receiver_mobile = ordermodel.DeliveryNumber;
                        // decimal post_amount = decimal.Parse(ordermodel.PostAmount.ToString());
                        // int logistics_type = -1;
                        string seller_memo = ordermodel.SellRemark;
                        decimal paid = decimal.Parse(ordermodel.Cost.ToString());

                        List<T_HtEbeyItem> orderItems = db.T_HtEbeyItem.Where(a => a.PorderID == tid).ToList();
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
                            string oid = Guid.NewGuid().ToString();
                            if (order_list == "")
                            {
                                order_list += "{" +
                                   "\"oid\": \"" + oid + "\"," +
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
                                     "\"oid\": \"" + oid + "\"," +
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
                        dic.Remove("shop_no");
                        dic.Add("shop_no", ShopName);

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
                            if (sdz > 0)
                            {
                                T_HtEbey model = db.T_HtEbey.Single(a => a.ID == id);
                                model.Istijiao = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                shibai = tid + ",";

                            }
                        }
                        else
                            shibai = tid + ",";
                    }
                    sc.Complete();
                    if (shibai != "0")
                    {
                        return Json(new { State = "Faile", Message = "" + shibai + "提交旺店通失败，请与技术人员联系" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult ShopeeUpToGY(string ids)//上传到旺店通
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                try
                {
                    var aa = "";
                    string[] Ids = ids.Split(',');
                    for (int j = 0; j < Ids.Length; j++)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        int id = Convert.ToInt32(Ids[j]);
                        T_HtShopee ordermodel = db.T_HtShopee.SingleOrDefault(a => a.ID == id);
                        if (ordermodel.istijiao == 1)
                            return Json(new { State = "Faile", Message = "" + ordermodel.OrderID + "已提交到旺店通，不能重复提交" }, JsonRequestBehavior.AllowGet);


                        string ShopName = "";
                        if (ordermodel.shopName == "shopee可孚")
                        {
                            ShopName = "4007";

                        }
                        else if (ordermodel.shopName == "shopee好护士")
                        {
                            ShopName = "4008";

                        }
                        else
                        {
                            return Json(new { State = "Faile", Message = "店铺问题请核查店铺名称" }, JsonRequestBehavior.AllowGet);
                        }
                        string tid = ordermodel.OrderID;
                        //int trade_status = 10;
                        //int pay_status = 2;
                        //int delivery_term = 1;
                        DateTime trade_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        DateTime pay_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        string buyer_nick = ordermodel.MemberName;
                        string receiver_name = ordermodel.DeliveryName;
                        string[] address = ordermodel.collectAddress.Split('-');
                        string receiver_province = address[0];
                        string receiver_city = address[1];
                        string receiver_district = address[2];
                        string receiver_address = ordermodel.address;
                        string receiver_mobile = ordermodel.DeliveryNumber;
                        decimal post_amount = decimal.Parse(ordermodel.PostAmount.ToString());
                        // int logistics_type = -1;
                        string seller_memo = ordermodel.Remarks;
                        decimal paid = decimal.Parse(ordermodel.Cost.ToString());

                        List<T_HtShopeeItem> orderItems = db.T_HtShopeeItem.Where(a => a.PorderID == tid).ToList();
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
                            string oid = tid + i.ToString();
                            if (order_list == "")
                            {

                                order_list += "{" +
                                   "\"oid\": \"" + oid + "\"," +
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
                                     "\"oid\": \"" + oid + "\"," +
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
                        dic.Remove("shop_no");
                        dic.Add("shop_no", ShopName);

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
                    "\"post_amount\": \"" + post_amount + "\"," +
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
                            if (sdz > 0)
                            {
                                T_HtShopee model = db.T_HtShopee.Single(a => a.ID == id);
                                model.istijiao = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                shibai += tid + ",";

                            }
                        }
                        else
                            shibai += tid + ",";
                    }
                    sc.Complete();
                    if (shibai != "0")
                    {
                        return Json(new { State = "Faile", Message = "" + shibai + "提交旺店通失败，请与技术人员联系" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult UpToGY(string ids)//上传到旺店通
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                try
                {
                    var aa = "";

                    string[] Ids = ids.Split(',');
                    for (int j = 0; j < Ids.Length; j++)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        int id = Convert.ToInt32(Ids[j]);
                        T_LazadaOrder ordermodel = db.T_LazadaOrder.SingleOrDefault(a => a.ID == id);
                        if (ordermodel.istijiao == 1)
                            return Json(new { State = "Faile", Message = "" + ordermodel.OrderNumber + "已提交到旺店通，不能重复提交" }, JsonRequestBehavior.AllowGet);


                        string ShopName = "";
                        if (ordermodel.shopName == "LAZADA科源")
                        {
                            ShopName = "4005";

                        }
                        else if (ordermodel.shopName == "LAZADA可孚")
                        {
                            ShopName = "4006";

                        }
                        else
                        {
                            return Json(new { State = "Faile", Message = "店铺问题请核查店铺名称" }, JsonRequestBehavior.AllowGet);
                        }
                        string tid = ordermodel.OrderNumber;
                        //int trade_status = 10;
                        //int pay_status = 2;
                        //int delivery_term = 1;
                        DateTime trade_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        DateTime pay_time = DateTime.Parse(ordermodel.SingleShotTime.ToString());
                        string buyer_nick = ordermodel.MemberCode;
                        string receiver_name = ordermodel.DeliveryName;
                        string[] address = ordermodel.collectAddress.Split('-');
                        string receiver_province = address[0];
                        string receiver_city = address[1];
                        string receiver_district = address[2];
                        string receiver_address = ordermodel.address;
                        string receiver_mobile = ordermodel.DeliveryNumber;

                        // int logistics_type = -1;
                        string seller_memo = ordermodel.SellRemark;
                        decimal paid = decimal.Parse(ordermodel.Cost.ToString());

                        List<T_LazadaOrderItem> orderItems = db.T_LazadaOrderItem.Where(a => a.POrderId == tid && a.shopName == ordermodel.shopName).ToList();
                        string order_list = "";

                        for (int i = 0; i < orderItems.Count; i++)
                        {
                            int num = int.Parse(orderItems[i].num.ToString());
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
                            string oid = Guid.NewGuid().ToString();
                            if (order_list == "")
                            {
                                order_list += "{" +
                                   "\"oid\": \"" + oid + "\"," +
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
                                     "\"oid\": \"" + oid + "\"," +
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
                        dic.Remove("shop_no");
                        dic.Add("shop_no", ShopName);

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
                            if (sdz > 0)
                            {
                                T_LazadaOrder model = db.T_LazadaOrder.Single(a => a.ID == id);
                                model.istijiao = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                shibai += tid + ",";
                            }
                        }
                        else
                            shibai += tid + ",";
                    }
                    sc.Complete();
                    if (shibai != "0")
                    {
                        return Json(new { State = "Faile", Message = "" + shibai + "提交旺店通失败，请与技术人员联系" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public string getShopCode(string shopName)
        {
            string ShopCode = "";
            switch (shopName)
            {
                case "haohushi":
                    ShopCode = "4014";
                    break;
                case "Cofoe medical":
                    ShopCode = "4016";
                    break;
                case "Cofoe technology":
                    ShopCode = "4017";
                    break;
                case "Cofe 官方":
                    ShopCode = "4018";
                    break;
                case "Cofoe001":
                    ShopCode = "5005";
                    break;
                case "Cofoe002":
                    ShopCode = "5006";
                    break;
                default:
                    break;
            }
            return ShopCode;
        }

        public string getShopNameCH(string shopName)
        {
            string ShopNameCH = "";
			T_Directory model= db.T_Directory.FirstOrDefault(a => a.KeyType == "海外店铺" && a.KeyName == shopName);
			if (model != null)
			{
				ShopNameCH = model.KeyValue;
			}
			return ShopNameCH;
        }
        [HttpPost]
        //AliUpToGY
        public JsonResult AliUpToGY(string ids)//上传到旺店通
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                try
                {
                    var aa = "";

                    string[] Ids = ids.Split(',');
                    for (int j = 0; j < Ids.Length; j++)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        int id = Convert.ToInt32(Ids[j]);
                        T_AliSumai ordermodel = db.T_AliSumai.SingleOrDefault(a => a.ID == id);
                        if (ordermodel.IsSubmit == 1)
                            return Json(new { State = "Faile", Message = "" + ordermodel.OrderNO + "已提交到旺店通，不能重复提交" }, JsonRequestBehavior.AllowGet);


                        string ShopName = getShopCode(ordermodel.ShopName);
                       
                       
                        if(string.IsNullOrEmpty(ShopName))
                        {
                            return Json(new { State = "Faile", Message = "店铺问题请核查店铺名称" }, JsonRequestBehavior.AllowGet);
                        }
                        string tid = ordermodel.OrderNO;
                        //int trade_status = 10;
                        //int pay_status = 2;
                        //int delivery_term = 1;
                        DateTime trade_time =Convert.ToDateTime( ordermodel.OrderTime);
                        DateTime pay_time = Convert.ToDateTime(ordermodel.PayTime);
                        string buyer_nick = ordermodel.BuyerName;
                        T_AliSumaiConfig config = db.T_AliSumaiConfig.FirstOrDefault(a=> ordermodel.LogisticsType.Contains(a.LogisticsType));
                        string receiver_name = config.receiver_name;
                      
                        string receiver_province = config.receiver_province;
                        string receiver_city = config.receiver_city;
                        string receiver_district = config.receiver_district;
                        string receiver_address = config.receiver_address;
                        string receiver_mobile = config.receiver_mobile;

                        // int logistics_type = -1;
                        string seller_memo = ordermodel.MailNO;
                        decimal paid = decimal.Parse(ordermodel.OrderMoney.ToString());

                        List<T_AliSumaiGoods> orderItems = db.T_AliSumaiGoods.Where(a => a.OrderNO == tid).ToList();
                        string order_list = "";

                        for (int i = 0; i < orderItems.Count; i++)
                        {
                            int num=Convert.ToInt32(  orderItems[i].ProductQty);
                            double? UnitPrice = orderItems[i].ProductPrice;
                            string goods_no = orderItems[i].SKU;
                            string spec_no = orderItems[i].SKU;
                            //    string Guid = System.Guid.NewGuid().ToString();
                            T_WDTGoods cofig = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == goods_no);
                            string goods_name = orderItems[i].ProductName;
                           
                            string oid = Guid.NewGuid().ToString();
                            if (order_list == "")
                            {
                                order_list += "{" +
                                   "\"oid\": \"" + oid + "\"," +
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
                                     "\"oid\": \"" + oid + "\"," +
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
                        dic.Remove("shop_no");
                        dic.Add("shop_no", ShopName);

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
                    "\"logistics_type\": \"-1\"," +
                    "\"buyer_message\": \"\"," +
                    "\"seller_memo\": \"" + ordermodel.MailNO + "\"," +
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
                            if (sdz > 0)
                            {
                                T_AliSumai model = db.T_AliSumai.Single(a => a.ID == id);
                                model.IsSubmit = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                shibai += tid + ",";
                            }
                        }
                        else
                            shibai += tid + ",";
                    }
                    sc.Complete();
                    if (shibai != "0")
                    {
                        return Json(new { State = "Faile", Message = "" + shibai + "提交旺店通失败，请重试" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
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
        [HttpPost]
        public JsonResult LazadaDelete(string ids)//删除lazada订单
        {
            try
            {
                string[] Ids = ids.Split(',');
                for (int j = 0; j < Ids.Length; j++)
                {
                    int id = Convert.ToInt32(Ids[j]);
                    T_LazadaOrder delModel = db.T_LazadaOrder.Find(id);
                    T_LazadaOrderItem item = db.T_LazadaOrderItem.FirstOrDefault(s => s.POrderId.Equals(delModel.OrderNumber));
                    db.T_LazadaOrderItem.Remove(item);
                    db.T_LazadaOrder.Remove(delModel);
                }
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }

        }
        [HttpPost]
        public JsonResult EbeyDelete(string ids)//删除Ebey订单
        {
            try
            {
                string[] Ids = ids.Split(',');
                for (int j = 0; j < Ids.Length; j++)
                {
                    int id = Convert.ToInt32(Ids[j]);
                    T_HtEbey delModel = db.T_HtEbey.Find(id);
                    T_HtEbeyItem item = db.T_HtEbeyItem.FirstOrDefault(s => s.PorderID.Equals(delModel.OrderID));
                    db.T_HtEbeyItem.Remove(item);
                    db.T_HtEbey.Remove(delModel);
                }
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }

        }
        [HttpPost]
        public JsonResult ShopeeDelete(string ids)//删除lazada订单
        {
            try
            {
                string[] Ids = ids.Split(',');
                for (int j = 0; j < Ids.Length; j++)
                {
                    int id = Convert.ToInt32(Ids[j]);
                    T_HtShopee delModel = db.T_HtShopee.Find(id);
                    T_HtShopeeItem item = db.T_HtShopeeItem.FirstOrDefault(s => s.PorderID.Equals(delModel.OrderID));
                    db.T_HtShopeeItem.Remove(item);
                  
                    db.T_HtShopee.Remove(delModel);
                    db.SaveChanges();
                }
             
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult AliDelete(string ids)//删除Aliexpress订单
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            try
            {
                string[] Ids = ids.Split(',');
                for (int j = 0; j < Ids.Length; j++)
                {
                    int id = Convert.ToInt32(Ids[j]);
                    T_AliSumai delModel = db.T_AliSumai.FirstOrDefault(a=>a.ID==id);
                    T_AliSumaiShip ship = db.T_AliSumaiShip.Find(delModel.OrderNO);
                    List<T_AliSumaiGoods> goods = db.T_AliSumaiGoods.Where(s => s.OrderNO.Equals(delModel.OrderNO)).ToList();
                    foreach (T_AliSumaiGoods item in goods)
                    {
                        db.T_AliSumaiGoods.Remove(item);
                    }
                    T_OperaterLog log = new T_OperaterLog()
                    {
                        Module = "AliSumai",
                        OperateContent = "删除"+ delModel.OrderNO,
                        Operater = name,
                        OperateTime = DateTime.Now,
                        PID = 0,
                    };
                    db.T_OperaterLog.Add(log);
                    db.T_AliSumai.Remove(delModel);
                    db.T_AliSumaiShip.Remove(ship);

                }
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }

        }
        #endregion
        #region lazada配置

        public ContentResult GetLazadaSkuList(Lib.GridPager pager, string sku)
        {
            IQueryable<T_LazadaGyCode> list = db.T_LazadaGyCode.AsQueryable();
            if (!string.IsNullOrWhiteSpace(sku))
                list = list.Where(s => s.Sku.Contains(sku) || s.ItemCode.Contains(sku));
            pager.totalRows = list.Count();
            List<T_LazadaGyCode> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public JsonResult LazadaSkuDelete(int id)
        {
            try
            {
                T_LazadaGyCode model = db.T_LazadaGyCode.Find(id);
                db.T_LazadaGyCode.Remove(model);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        public JsonResult HTEDelete(int id)
        {
            try
            {
                T_HTExchangeRate model = db.T_HTExchangeRate.Find(id);
                db.T_HTExchangeRate.Remove(model);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        public JsonResult LazadaConfigSkuAddSave(string sku, string code)
        {
            try
            {
                T_LazadaGyCode model = db.T_LazadaGyCode.SingleOrDefault(s => s.Sku.Equals(sku));
                if (model != null)
                    return Json(new { State = "Faile", Message = "SKU已存在" });
                T_LazadaGyCode models = new T_LazadaGyCode
                {
                    Sku = sku,
                    ItemCode = code
                };
                db.T_LazadaGyCode.Add(models);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        public JsonResult LazadaConfigSkuEditSave(int id, string sku, string code)
        {
            try
            {
                T_LazadaGyCode model = db.T_LazadaGyCode.SingleOrDefault(s => s.Sku.Equals(sku));
                if (model != null)
                    return Json(new { State = "Faile", Message = "SKU已存在" });
                T_LazadaGyCode models = db.T_LazadaGyCode.Find(id);
                models.Sku = sku;
                models.ItemCode = code;
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        public ContentResult GetLazadaConfigList(Lib.GridPager pager)
        {
            IQueryable<T_LazadaOrderConfig> list = db.T_LazadaOrderConfig.AsQueryable();
            pager.totalRows = list.Count();
            List<T_LazadaOrderConfig> querData = new List<T_LazadaOrderConfig>();
            foreach (var item in list)
            {
                T_LazadaOrderConfig model = new T_LazadaOrderConfig();
                model = item;
                model.Express = Com.GetExpressName(item.Express);
                model.Warhouse = Com.GetWarehouseName(item.Warhouse);
                querData.Add(model);
            }
            querData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public JsonResult LazadaConfigEditSave(T_LazadaOrderConfig model)
        {
            try
            {
                if (model.IsDefault == 1)
                {
                    T_LazadaOrderConfig cofig = db.T_LazadaOrderConfig.SingleOrDefault(s => s.IsDefault == 1);
                    if (cofig != null)
                        return Json(new { State = "Faile", Message = "已存在默认地址，不能编辑" });
                }
                T_LazadaOrderConfig editModel = db.T_LazadaOrderConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Warhouse = model.Warhouse;
                editModel.Express = model.Express;
                editModel.Phone = model.Phone;
                editModel.AddressMessage = model.AddressMessage;
                editModel.Address = model.Address;
                editModel.IsDefault = model.IsDefault;
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        public JsonResult LazadaConfigAddSave(T_LazadaOrderConfig model)
        {
            try
            {
                if (model.IsDefault == 1)
                {
                    T_LazadaOrderConfig cofig = db.T_LazadaOrderConfig.SingleOrDefault(s => s.IsDefault == 1);
                    if (cofig != null)
                        return Json(new { State = "Faile", Message = "已存在默认地址，不能重复添加" });
                }
                db.T_LazadaOrderConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        //新增汇率
        public JsonResult HTExchangeRateAddSave(T_HTExchangeRate model,string TypeName)
        {
            try
            {
                model.CDate = DateTime.Now;
                model.TypeName = TypeName;
                db.T_HTExchangeRate.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        public JsonResult LazadaConfigDelete(int ID)
        {
            T_LazadaOrderConfig delModel = db.T_LazadaOrderConfig.Find(ID);
            if (delModel != null)
            {
                db.T_LazadaOrderConfig.Remove(delModel);
                int i = db.SaveChanges();
                return Json(i);
            }
            else
            {
                return Json(-1);
            }
        }
        #endregion
        #endregion
        #region 导入导出excel
        [HttpPost]
        public String ImportExcel(string flag)//导入excel
        {
            AboutExcel AE = new AboutExcel();
            int s = 0;
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file];//get post file 
                if (postFile.ContentLength == 0)
                {
                    return " <script > alert('文件为空，请重新选择');window.history.go(-1);  </script>";
                }
                if (flag == "2")//导入的是物流信息文件
                {
                    string newFilePath = Server.MapPath("~/Upload/AliExpressExcel/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName);
                    postFile.SaveAs(newFilePath + fileName);//save file 
                    DataTable dt = AE.ImportExcelFile(newFilePath + fileName);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() != "")
                        {
                            string freightNO = dt.Rows[i][0].ToString().Trim();

                            List<T_AliExpressOrder> ccList = db.T_AliExpressOrder.Where(a => a.dispatchID != null && a.dispatchID.Contains(freightNO)).ToList();//一个物流单号对应多个订单时订单平分物流成本和重量
                            int countCC = ccList.Count;
                            foreach (T_AliExpressOrder cc in ccList)
                            {
                                if (!Convert.IsDBNull(dt.Rows[i][1]))
                                {
                                    cc.weight = Convert.ToDouble(dt.Rows[i][1]) / countCC;
                                }

                                if (!Convert.IsDBNull(dt.Rows[i][2]))
                                {
                                    cc.foreignFreight = Convert.ToDouble(dt.Rows[i][2]) / countCC;
                                }
                                if (!Convert.IsDBNull(dt.Rows[i][3]))
                                {
                                    cc.homeFreight = Convert.ToDouble(dt.Rows[i][3]) / countCC;
                                }
                                db.Entry<T_AliExpressOrder>(cc).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    db.SaveChanges();
                }
                else
                {
                    string newFilePath = Server.MapPath("~/Upload/AliExpressExcel/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName);
                    postFile.SaveAs(newFilePath + fileName);//save file 
                    DataTable dt = AE.ImportExcelFile(newFilePath + fileName);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (dt.Rows[i][0].ToString() != "")
                        {
                            //查重
                            string OrderNO = dt.Rows[i][0].ToString().Trim();
                            T_AliExpressOrder cc = db.T_AliExpressOrder.SingleOrDefault(a => a.orderNumber == OrderNO);
                            if (cc == null)
                            {
                                T_AliExpressOrder model = new T_AliExpressOrder();
                                //model.purchasePriceProduct = 0;
                                //model.homeFreight = 0;
                                //model.foreignFreight = 0;
                                //model.profit = 0;
                                //model.profitRate = 0;
                                //model.weight = 0; //订单导入时excel中没有相关信息 ;

                                model.isKeyuan = flag;
                                model.isDelete = "0";
                                model.orderNumber = OrderNO;
                                model.state = dt.Rows[i][1].ToString();
                                model.salesman = dt.Rows[i][2].ToString();
                                model.buyer = dt.Rows[i][3].ToString();
                                model.buyerMail = dt.Rows[i][4].ToString().Trim();

                                model.orderTime = Convert.ToDateTime(dt.Rows[i][5].ToString());
                                if (dt.Rows[i][6] != null && dt.Rows[i][6].ToString() != "")
                                {
                                    model.paymentTime = Convert.ToDateTime(dt.Rows[i][6].ToString());
                                }
                                else { model.paymentTime = null; }


                                string zonge = dt.Rows[i][7].ToString().Substring(1);
                                model.totalProductValue = Convert.ToDouble(zonge);

                                string wuliufeiyong = dt.Rows[i][8].ToString().Substring(1);
                                model.logisticsCost = Convert.ToDouble(wuliufeiyong);

                                string dingdanjine = dt.Rows[i][9].ToString().Substring(1);
                                model.orderMoney = Convert.ToDouble(dingdanjine);
                                model.productInformation = dt.Rows[i][11].ToString().Trim();
                                model.receiveAddress = dt.Rows[i][13].ToString().Trim();
                                model.receiver = dt.Rows[i][14].ToString().Trim();
                                model.receiveCountry = dt.Rows[i][15].ToString().Trim();
                                model.canton = dt.Rows[i][16].ToString().Trim();
                                model.city = dt.Rows[i][17].ToString().Trim();
                                model.address = dt.Rows[i][18].ToString().Trim();
                                model.postcode = dt.Rows[i][19].ToString().Trim();
                                if (dt.Rows[i][20] != null && dt.Rows[i][20].ToString() != "")
                                {
                                    model.telephone = dt.Rows[i][20].ToString().Trim();
                                }
                                else { model.telephone = ""; }
                                if (dt.Rows[i][21] != null && dt.Rows[i][21].ToString() != "")
                                {
                                    model.mobile = dt.Rows[i][21].ToString().Trim();
                                }
                                else { model.mobile = ""; }
                                model.buyerChooseLogistics = dt.Rows[i][22].ToString();
                                model.platformCommission = Math.Round(Convert.ToDouble(dingdanjine) * 0.05, 4);
                                //model.platform_commission = Convert.ToDouble(dingdanjine) * 0.05;
                                model.periodDispatch = dt.Rows[i][23].ToString();
                                model.addTime = DateTime.Now;  //记录人和记录时间
                                model.userName = name;
                                model.refund = 0;
                                if (dt.Rows[i][4] != null && dt.Rows[i][4].ToString() != "")
                                {
                                    model.buyerMail = dt.Rows[i][4].ToString().Trim();
                                }
                                else
                                {
                                    model.buyerMail = null;
                                }
                                if (dt.Rows[i][10] != null && dt.Rows[i][10].ToString().Trim() != "")
                                {
                                    string full_reduction = dt.Rows[i][8].ToString().Substring(1);
                                    model.fullReduction = Convert.ToDouble(full_reduction); ;
                                }
                                else
                                {
                                    model.fullReduction = 0;
                                }
                                if (dt.Rows[i][12] != null && dt.Rows[i][12].ToString().Trim() != "")
                                {
                                    model.orderRemark = dt.Rows[i][12].ToString().Trim();
                                }
                                else
                                {
                                    model.orderRemark = "";
                                }
                                if (dt.Rows[i][24] != null && dt.Rows[i][24].ToString() != "")
                                {
                                    model.dispatchID = dt.Rows[i][24].ToString().Trim();
                                }
                                else
                                {
                                    model.dispatchID = "";
                                }
                                if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString() != "")
                                {
                                    model.dispatchTime = Convert.ToDateTime(dt.Rows[i][25].ToString().Trim());
                                }
                                else
                                {
                                    model.dispatchTime = null;
                                }

                                if (dt.Rows[i][26] != null && dt.Rows[i][26].ToString() != "")
                                {

                                    model.confirmReceiveTime = Convert.ToDateTime(dt.Rows[i][26].ToString());
                                }
                                else
                                {
                                    model.confirmReceiveTime = null;
                                }

                                db.T_AliExpressOrder.Add(model);
                            }
                            else
                            {
                                cc.isKeyuan = flag;
                                cc.isDelete = "0";
                                //return " <script > parent.$('#openDivNew').dialog('close');  alert('订单号：" + dingdanNO + "已存在,请确认所有订单无重复后重新导入')  </script>";
                                cc.orderNumber = dt.Rows[i][0].ToString().Trim();
                                if (cc.state.Contains("纠纷")) { }
                                else
                                {
                                    cc.state = dt.Rows[i][1].ToString().Trim();
                                }
                                cc.salesman = dt.Rows[i][2].ToString().Trim();
                                cc.buyer = dt.Rows[i][3].ToString().Trim();
                                //model.purchaser_mailbox = dt.Rows[i][4].ToString().Trim();
                                cc.orderTime = Convert.ToDateTime(dt.Rows[i][5].ToString());

                                if (dt.Rows[i][6] != null && dt.Rows[i][6].ToString() != "")
                                {
                                    cc.paymentTime = Convert.ToDateTime(dt.Rows[i][6].ToString());
                                }
                                //  string   dt.Rows[i][7].ToString().Substring(0);

                                string zonge = dt.Rows[i][7].ToString().Substring(1);
                                cc.totalProductValue = Convert.ToDouble(zonge);

                                string wuliufeiyong = dt.Rows[i][8].ToString().Substring(1);
                                cc.logisticsCost = Convert.ToDouble(wuliufeiyong);

                                string dingdanjine = dt.Rows[i][9].ToString().Substring(1);
                                cc.orderMoney = Convert.ToDouble(dingdanjine);
                                cc.productInformation = dt.Rows[i][11].ToString().Trim();
                                cc.receiveAddress = dt.Rows[i][13].ToString().Trim();
                                cc.receiver = dt.Rows[i][14].ToString().Trim();
                                cc.receiveCountry = dt.Rows[i][15].ToString().Trim();
                                cc.canton = dt.Rows[i][16].ToString().Trim();
                                cc.city = dt.Rows[i][17].ToString().Trim();
                                cc.address = dt.Rows[i][18].ToString().Trim();
                                cc.postcode = dt.Rows[i][19].ToString().Trim();
                                if (dt.Rows[i][20] != null && dt.Rows[i][20].ToString() != "")
                                {
                                    cc.telephone = dt.Rows[i][20].ToString().Trim();
                                }

                                cc.mobile = dt.Rows[i][21].ToString().Trim();
                                cc.buyerChooseLogistics = dt.Rows[i][22].ToString().Trim();

                                cc.periodDispatch = dt.Rows[i][23].ToString().Trim();
                                // cc.platform_commission = Convert.ToDouble(dingdanjine)*0.05;
                                cc.addTime = DateTime.Now;   //添加记录的时间
                                cc.userName = name;      //记录人

                                if (dt.Rows[i][4] != null && dt.Rows[i][4].ToString() != "")
                                {
                                    cc.buyerMail = dt.Rows[i][4].ToString().Trim();
                                }
                                else
                                {
                                    cc.buyerMail = null;
                                }
                                if (dt.Rows[i][10] != null && dt.Rows[i][10].ToString() != "")
                                {
                                    string full_reduction = dt.Rows[i][10].ToString().Substring(1);
                                    cc.fullReduction = Convert.ToDouble(full_reduction);
                                }
                                else
                                {
                                    cc.fullReduction = null;
                                }
                                if (dt.Rows[i][12] != null && dt.Rows[i][12].ToString().Trim() != "")
                                {
                                    cc.orderRemark = dt.Rows[i][12].ToString().Trim();
                                }
                                else
                                {
                                    cc.orderRemark = null;
                                }
                                if (dt.Rows[i][24] != null && dt.Rows[i][24].ToString() != "")
                                {
                                    cc.dispatchID = dt.Rows[i][24].ToString().Trim();
                                }
                                else
                                {
                                    cc.dispatchID = null;
                                }
                                if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString() != "")
                                {
                                    cc.dispatchTime = Convert.ToDateTime(dt.Rows[i][25].ToString().Trim());
                                }
                                else
                                {
                                    cc.dispatchTime = null;
                                }
                                if (dt.Rows[i][26] != null && dt.Rows[i][26].ToString() != "")
                                {
                                    cc.confirmReceiveTime = Convert.ToDateTime(dt.Rows[i][26].ToString().Trim());
                                }
                                else
                                {
                                    cc.confirmReceiveTime = null;
                                }
                                db.Entry<T_AliExpressOrder>(cc).State = System.Data.Entity.EntityState.Modified;
                            }

                        }
                        s++;
                    }

                    //try
                    //{
                    db.SaveChanges();
                    //}
                    //catch (DbEntityValidationException e)
                    //{
                    //e.EntityValidationErrors.ToString();
                    //}

                }

            }
            return "<script>parent.$(\"#List\").datagrid('reload'); parent.$('#DivAdd').dialog('close');  alert('导入成功！');</script>";//parent.query();


        }

        [HttpPost]

        public String ImportAliExcel()
        {
            AboutExcel AE = new AboutExcel();
            int s = 0;
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            string message = "导入成功！";
            string RepeatNO = "";
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file];//get post file 
                if (postFile.ContentLength == 0)
                {
                    return " <script > alert('文件为空，请重新选择');window.history.go(-1);  </script>";
                }
               
                else
                {
                    string newFilePath = Server.MapPath("~/Upload/AliExpressExcel/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName)+(DateTime.Now.Ticks/100000);
                    postFile.SaveAs(newFilePath + fileName);//save file 
                    DataTable dt = AE.ImportExcelFile(newFilePath + fileName);
                    List<T_AliSumai> tempList = new List<T_AliSumai>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (dt.Rows[i][0].ToString() != "")
                        {
                            //查重
                            string OrderNO = dt.Rows[i][0].ToString().Trim();
                            int Repeat = db.T_AliSumai.Where(a => a.OrderNO == OrderNO).Count();
                            if (Repeat == 0)
                            {
                                int isNewOrder = tempList.Where(a => a.OrderNO == OrderNO).Count();
                                if (isNewOrder == 0)
                                {
                                    T_AliSumai Order = new T_AliSumai();
                                    T_AliSumaiGoods goods = new T_AliSumaiGoods();
                                    T_AliSumaiShip ship = new T_AliSumaiShip();
                                    Order.OrderNO = OrderNO;
                                    ship.OrderNO = OrderNO;
                                    goods.OrderNO = OrderNO;
                                    Order.IsSubmit = 0;

                                    Order.TransactionNO = isNULL(dt.Rows[i][1]);
                                    Order.OrderState = isNULL(dt.Rows[i][2]);

                                    goods.SKU = isNULL(dt.Rows[i][3]);

                                    Order.Platform = isNULL(dt.Rows[i][4]);
                                    Order.BuyerAccount = isNULL(dt.Rows[i][5]);
                                    Order.BuyerName = isNULL(dt.Rows[i][6]);

                                    goods.ProductNameEng = isNULL(dt.Rows[i][7]);
                                    goods.ProductQty = Convert.ToDouble(dt.Rows[i][8].ToString());

                                    Order.Currency = isNULL(dt.Rows[i][9]);
                                    Order.OrderMoney = Convert.ToDouble(dt.Rows[i][10]);
                                    Order.PayType = dt.Rows[i][11].ToString().Trim();
                                    Order.RetreatReason = dt.Rows[i][12].ToString().Trim();
                                    Order.LogisticsCost = Convert.ToDouble(dt.Rows[i][13].ToString());
                                    Order.OrderMemo = dt.Rows[i][14].ToString().Trim();
                                    Order.PickingMemo = dt.Rows[i][15].ToString().Trim();
                                    Order.SellerMemo = dt.Rows[i][16].ToString().Trim();
                                    Order.Email = dt.Rows[i][17].ToString().Trim();
                                    ship.ReceiverName = dt.Rows[i][18].ToString().Trim();
                                    ship.Address = dt.Rows[i][19].ToString().Trim();
                                    ship.Address1 = dt.Rows[i][20].ToString().Trim();
                                    ship.Address2 = dt.Rows[i][21].ToString().Trim();

                                    //ship.Address12 = dt.Rows[i][22].ToString();

                                    ship.City = dt.Rows[i][23].ToString();
                                    ship.Province = dt.Rows[i][24].ToString();
                                    ship.ZipCode = dt.Rows[i][25].ToString();
                                    ship.Country = dt.Rows[i][26].ToString();
                                    ship.CountryChineseName = dt.Rows[i][27].ToString();
                                    ship.CountryCharacterCode = dt.Rows[i][28].ToString();
                                    ship.Tel = dt.Rows[i][29].ToString();
                                    ship.PhoneNum = dt.Rows[i][30].ToString();

                                    Order.PackageNO = dt.Rows[i][31].ToString();
                                    Order.LogisticsType = dt.Rows[i][32].ToString();
                                    Order.BuyerSpecifiedLogistics = dt.Rows[i][33].ToString();
                                    Order.MailNO = dt.Rows[i][34].ToString();

                                    goods.CustomsName = dt.Rows[i][35].ToString();
                                    goods.CustomsName2 = dt.Rows[i][36].ToString();
                                    goods.DeclarationUnitPrice = Convert.ToDouble(dt.Rows[i][37].ToString());
                                    goods.CustomsCode = dt.Rows[i][38].ToString();

                                    Order.PayTime = Convert.ToDateTime(dt.Rows[i][39]);
                                    Order.OrderTime = Convert.ToDateTime(dt.Rows[i][40]);
                                    Order.DeliverTime = dt.Rows[i][41].ToString();

                                    goods.ProductSpec = dt.Rows[i][42].ToString();
                                    goods.CustomsWeight = Convert.ToDouble(dt.Rows[i][43]);

                                    Order.ShopName = dt.Rows[i][45].ToString();
                                    goods.ProductName = dt.Rows[i][47].ToString();
                                    goods.ProductPrice = Convert.ToDouble(dt.Rows[i][54].ToString());

                                    Order.PostTime = DateTime.Now;  //记录人和记录时间
                                    Order.PostName = name;

                                    tempList.Add(Order);
                                    db.T_AliSumai.Add(Order);
                                    db.T_AliSumaiGoods.Add(goods);
                                    db.T_AliSumaiShip.Add(ship);
                                }
                                else
                                {
                                    T_AliSumaiGoods goods = new T_AliSumaiGoods();
                                    goods.OrderNO = OrderNO;
                                    goods.SKU = isNULL(dt.Rows[i][3]);
                                    goods.ProductNameEng = isNULL(dt.Rows[i][7]);
                                    goods.ProductQty = Convert.ToDouble(dt.Rows[i][8].ToString());
                                    goods.CustomsName = dt.Rows[i][35].ToString();
                                    goods.CustomsName2 = dt.Rows[i][36].ToString();
                                    goods.DeclarationUnitPrice = Convert.ToDouble(dt.Rows[i][37].ToString());
                                    goods.CustomsCode = dt.Rows[i][38].ToString();
                                    goods.ProductSpec = dt.Rows[i][42].ToString();
                                    goods.CustomsWeight = Convert.ToDouble(dt.Rows[i][43]);
                                    goods.ProductName = dt.Rows[i][47].ToString();
                                    goods.ProductPrice = Convert.ToDouble(dt.Rows[i][54].ToString());
                                    db.T_AliSumaiGoods.Add(goods);

                                }

                            }
                            else
                            {
                                RepeatNO += OrderNO;
                            }
                           


                        }
                        s++;
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        message += e.InnerException.Message;
                    }

                }

            }
            if(!string.IsNullOrEmpty(RepeatNO))
                {
                message +="订单" +RepeatNO + "重复,其他导入成功！";
            }
            return "<script> alert('" + message + "'); parent.$('#AddDiv').dialog('close'); </script>";//parent.query();
        }

        public ActionResult DownAliExcel( string startDate, string EndDate,int isDown=0,string platform="")
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            try
            {
                using (TransactionScope sc = new TransactionScope(TransactionScopeOption.Required,
																		new TimeSpan(0, 5, 0)))
                {
                    //创建Excel文件的对象
                    NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                    //添加一个sheet
                    NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

                    //获取list数据
                    IQueryable<T_AliSumai> qs = db.T_AliSumai;
                    if (isDown == 0 || isDown == 1)
                    {
                        qs = qs.Where(a => a.IsDown == isDown);
                    }
					if (!string.IsNullOrEmpty(platform))
					{
						qs = qs.Where(a => a.Platform == platform);
					}
                    if (!string.IsNullOrEmpty(startDate))
                    {
                        DateTime sdate = DateTime.Parse(startDate);
                        qs = qs.Where(a => a.PostTime >= sdate);
                    }
                    if (!string.IsNullOrEmpty(EndDate))
                    {
                        DateTime Edate = DateTime.Parse(EndDate);
                        Edate = Edate.AddDays(1);
                        qs = qs.Where(a => a.PostTime <= Edate);
                    }



                    List<T_AliSumai> ListInfo = qs.ToList();
                    NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                    row1.Height = 3 * 265;
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
                    int row = 0;
                    for (int i = 0; i < ListInfo.Count; i++)
                    {
                        T_AliSumai editModel = ListInfo[i];
                        editModel.IsDown = 1;
                        db.Entry<T_AliSumai>(editModel).State = System.Data.Entity.EntityState.Modified;

                        string OrderNO = ListInfo[i].OrderNO;

                        List<T_AliSumaiGoods> goods = db.T_AliSumaiGoods.Where(a => a.OrderNO == OrderNO).ToList();
                        for (int j = 0; j < goods.Count; j++)
                        {
                            NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(row + 1);
                            rowtemp.Height = 3 * 265;

                            rowtemp.CreateCell(0).SetCellValue(getShopNameCH(ListInfo[i].ShopName));

                            rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                            rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                            rowtemp.Cells[0].CellStyle.WrapText = true;
                            rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                            rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                            rowtemp.CreateCell(1).SetCellValue(OrderNO);
                            string LogisticsType = ListInfo[i].LogisticsType;
                            T_AliSumaiConfig config = db.T_AliSumaiConfig.FirstOrDefault(a => LogisticsType.Contains(a.LogisticsType));
                            if (config != null)
                            {
                                rowtemp.CreateCell(2).SetCellValue(config.receiver_name);
                                rowtemp.CreateCell(3).SetCellValue(config.receiver_province);
                                rowtemp.CreateCell(4).SetCellValue(config.receiver_city);
                                rowtemp.CreateCell(5).SetCellValue(config.receiver_district);
                                rowtemp.CreateCell(6).SetCellValue(config.receiver_mobile);
                                rowtemp.CreateCell(8).SetCellValue(config.receiver_zip);
                                rowtemp.CreateCell(10).SetCellValue(config.receiver_address);
                                rowtemp.CreateCell(21).SetCellValue(ListInfo[i].SellerMemo + config.LogisticsType + ListInfo[i].MailNO);
                            }
                            else {
                                rowtemp.CreateCell(2).SetCellValue("");
                                rowtemp.CreateCell(3).SetCellValue("");
                                rowtemp.CreateCell(4).SetCellValue("");
                                rowtemp.CreateCell(5).SetCellValue("");
                                rowtemp.CreateCell(6).SetCellValue("");
                                rowtemp.CreateCell(8).SetCellValue("");
                                rowtemp.CreateCell(10).SetCellValue("");
                                rowtemp.CreateCell(21).SetCellValue(ListInfo[i].SellerMemo  + ListInfo[i].MailNO);
                            }
                            
                            rowtemp.CreateCell(7).SetCellValue("");
                          
                            rowtemp.CreateCell(9).SetCellValue(ListInfo[i].BuyerName);
                            
                            rowtemp.CreateCell(11).SetCellValue("款到发货");
                            rowtemp.CreateCell(12).SetCellValue(ListInfo[i].OrderMoney.Value.ToString());
                            rowtemp.CreateCell(13).SetCellValue(ListInfo[i].LogisticsCost.Value.ToString());

                            rowtemp.CreateCell(14).SetCellValue("");
                            rowtemp.CreateCell(15).SetCellValue("");
                            rowtemp.CreateCell(16).SetCellValue("海外仓");

                            rowtemp.CreateCell(17).SetCellValue("顺丰电商特惠（陆运）");
                            rowtemp.CreateCell(18).SetCellValue(ListInfo[i].OrderTime.Value.ToString());
                            rowtemp.CreateCell(19).SetCellValue(ListInfo[i].PayTime.Value.ToString());
                            rowtemp.CreateCell(20).SetCellValue(ListInfo[i].OrderMemo);
                            
                            rowtemp.CreateCell(22).SetCellValue("");
                            rowtemp.CreateCell(23).SetCellValue("");
                            rowtemp.CreateCell(24).SetCellValue("");
                            rowtemp.CreateCell(25).SetCellValue("");

                            rowtemp.CreateCell(26).SetCellValue(goods[j].SKU);
                            rowtemp.CreateCell(27).SetCellValue(goods[j].ProductQty);
                            rowtemp.CreateCell(28).SetCellValue(goods[j].ProductPrice.Value);
                            rowtemp.CreateCell(29).SetCellValue(goods[j].ProductQty * goods[j].ProductPrice.Value);
                            rowtemp.CreateCell(30).SetCellValue("");
                            rowtemp.CreateCell(31).SetCellValue("");
                            rowtemp.CreateCell(32).SetCellValue("");

                            rowtemp.CreateCell(33).SetCellValue("");
                            rowtemp.CreateCell(34).SetCellValue("");
                            rowtemp.CreateCell(35).SetCellValue("");
                            rowtemp.CreateCell(36).SetCellValue("");
                            row++;

                        }



                    }
                    T_OperaterLog log = new T_OperaterLog()
                    {
                        Module = "AliSumai",
                        OperateContent = "导出" + startDate+"-"+EndDate+","+isDown+ platform,
                        Operater = name,
                        OperateTime = DateTime.Now,
                        PID = 0,
                    };
                    db.T_OperaterLog.Add(log);
                    db.SaveChanges();
                    sc.Complete();
                    Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                    // 写入到客户端 
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();

                    book.Write(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Flush();
                    ms.Position = 0;

                    return File(ms, "application/vnd.ms-excel", platform + (DateTime.Now.Ticks / 100000) + ".xls");
                }


            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }

        }
        //导出excel
        public FileResult DownExcel(string OrderNO, string startDate, string EndDate, string shopName)
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

            //获取list数据
            IQueryable<T_AliExpressOrder> qs = getQueryData(OrderNO, startDate, EndDate, shopName);

            if (startDate != "" && startDate != null)
            {
                DateTime sdate = DateTime.Parse(startDate);
                qs = qs.Where(a => a.paymentTime >= sdate);
            }
            if (EndDate != "" && EndDate != null)
            {
                DateTime Edate = DateTime.Parse(EndDate);
                Edate = Edate.AddDays(1);
                qs = qs.Where(a => a.paymentTime <= Edate);
            }

            if (shopName == "1")
            {
                qs = qs.Where(a => a.isKeyuan == "1" || a.isKeyuan == null);
            }
            if (shopName == "0")
            {
                qs = qs.Where(a => a.isKeyuan == "0");
            }


            List<T_AliExpressOrder> ListInfo = qs.ToList();
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            row1.CreateCell(0).SetCellValue("订单号");
            row1.CreateCell(1).SetCellValue("订单状态");
            row1.CreateCell(2).SetCellValue("负责人(业务员)");
            row1.CreateCell(3).SetCellValue("买家名称");
            row1.CreateCell(4).SetCellValue("买家邮箱");
            row1.CreateCell(5).SetCellValue("下单时间");
            row1.CreateCell(6).SetCellValue("付款时间");
            row1.CreateCell(7).SetCellValue("产品总金额");
            row1.CreateCell(8).SetCellValue("物流费用");
            row1.CreateCell(9).SetCellValue("订单金额");
            row1.CreateCell(10).SetCellValue("产品进价");
            row1.CreateCell(11).SetCellValue("平台佣金");
            row1.CreateCell(12).SetCellValue("国内运费");
            row1.CreateCell(13).SetCellValue("国外运费(汇率6.5)");
            row1.CreateCell(14).SetCellValue("利润");
            row1.CreateCell(15).SetCellValue("利润率");
            row1.CreateCell(16).SetCellValue("重量(kg)");
            row1.CreateCell(17).SetCellValue("满立减");
            row1.CreateCell(18).SetCellValue("产品信息(双击单元格展开所有产品信息)");
            row1.CreateCell(19).SetCellValue("订单备注");
            row1.CreateCell(20).SetCellValue("收货地址");
            row1.CreateCell(21).SetCellValue("收货人名称");
            row1.CreateCell(22).SetCellValue("收货国家");
            row1.CreateCell(23).SetCellValue("州/省");
            row1.CreateCell(24).SetCellValue("城市");
            row1.CreateCell(25).SetCellValue("地址");
            row1.CreateCell(26).SetCellValue("邮编");
            row1.CreateCell(27).SetCellValue("联系电话");
            row1.CreateCell(28).SetCellValue("手机");
            row1.CreateCell(29).SetCellValue("买家选择物流");
            row1.CreateCell(30).SetCellValue("发货期限");
            row1.CreateCell(31).SetCellValue("实际发货物流:运单号");
            row1.CreateCell(32).SetCellValue("发货时间");
            row1.CreateCell(33).SetCellValue("确认收货时间");
            row1.CreateCell(34).SetCellValue("备注");
            row1.CreateCell(35).SetCellValue("扣款");
            row1.CreateCell(36).SetCellValue("扣款原因");
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 15 * 256);
            sheet1.SetColumnWidth(2, 15 * 256);
            sheet1.SetColumnWidth(3, 15 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 15 * 256);
            sheet1.SetColumnWidth(8, 15 * 256);
            sheet1.SetColumnWidth(9, 15 * 256);
            sheet1.SetColumnWidth(10, 15 * 256);
            sheet1.SetColumnWidth(11, 15 * 256);
            sheet1.SetColumnWidth(12, 15 * 256);
            sheet1.SetColumnWidth(13, 15 * 256);
            sheet1.SetColumnWidth(14, 15 * 256);
            sheet1.SetColumnWidth(15, 15 * 256);
            sheet1.SetColumnWidth(16, 15 * 256);
            sheet1.SetColumnWidth(17, 15 * 256);
            sheet1.SetColumnWidth(18, 20 * 256);
            sheet1.SetColumnWidth(19, 15 * 256);
            sheet1.SetColumnWidth(20, 30 * 256);
            sheet1.SetColumnWidth(21, 15 * 256);
            sheet1.SetColumnWidth(22, 15 * 256);
            sheet1.SetColumnWidth(23, 15 * 256);
            sheet1.SetColumnWidth(24, 15 * 256);
            sheet1.SetColumnWidth(25, 15 * 256);
            sheet1.SetColumnWidth(26, 15 * 256);
            sheet1.SetColumnWidth(27, 15 * 256);
            sheet1.SetColumnWidth(28, 15 * 256);
            sheet1.SetColumnWidth(29, 30 * 256);
            sheet1.SetColumnWidth(30, 15 * 256);
            sheet1.SetColumnWidth(31, 30 * 256);
            sheet1.SetColumnWidth(32, 20 * 256);
            sheet1.SetColumnWidth(33, 20 * 256);
            sheet1.SetColumnWidth(34, 20 * 256);
            sheet1.SetColumnWidth(35, 15 * 256);
            sheet1.SetColumnWidth(36, 15 * 256);

            for (int i = 0; i < ListInfo.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;

                rowtemp.CreateCell(0).SetCellValue(ListInfo[i].orderNumber);

                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                rowtemp.CreateCell(1).SetCellValue(ListInfo[i].state);
                rowtemp.CreateCell(2).SetCellValue(ListInfo[i].salesman);
                rowtemp.CreateCell(3).SetCellValue(ListInfo[i].buyer);
                rowtemp.CreateCell(4).SetCellValue(ListInfo[i].buyerMail);
                if (ListInfo[i].orderTime != null && ListInfo[i].orderTime.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(5).SetCellValue(ListInfo[i].orderTime.ToString());
                }
                rowtemp.CreateCell(6).SetCellValue(ListInfo[i].paymentTime.ToString());
                rowtemp.CreateCell(7).SetCellValue('$' + ListInfo[i].totalProductValue.Value.ToString());
                rowtemp.CreateCell(8).SetCellValue('$' + ListInfo[i].logisticsCost.Value.ToString());
                rowtemp.CreateCell(9).SetCellValue('$' + ListInfo[i].orderMoney.Value.ToString());
                if (ListInfo[i].purchasePriceProduct != null && ListInfo[i].purchasePriceProduct.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(10).SetCellValue('$' + ListInfo[i].purchasePriceProduct.Value.ToString());
                }
                rowtemp.CreateCell(11).SetCellValue('$' + ListInfo[i].platformCommission.Value.ToString());
                if (ListInfo[i].homeFreight != null && ListInfo[i].homeFreight.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(12).SetCellValue('$' + ListInfo[i].homeFreight.Value.ToString());
                }
                if (ListInfo[i].foreignFreight != null && ListInfo[i].foreignFreight.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(13).SetCellValue('$' + ListInfo[i].foreignFreight.Value.ToString());
                }
                if (ListInfo[i].profit != null && ListInfo[i].profit.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(14).SetCellValue('$' + ListInfo[i].profit.Value.ToString());
                }
                if (ListInfo[i].profitRate != null && ListInfo[i].profitRate.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(15).SetCellValue(ListInfo[i].profitRate.Value);
                }
                if (ListInfo[i].weight != null && ListInfo[i].weight.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(16).SetCellValue(ListInfo[i].weight.Value);
                }
                if (ListInfo[i].fullReduction != null && ListInfo[i].fullReduction.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(17).SetCellValue(ListInfo[i].fullReduction.Value);
                }
                rowtemp.CreateCell(18).SetCellValue(ListInfo[i].productInformation);
                if (ListInfo[i].orderRemark != null && ListInfo[i].orderRemark.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(19).SetCellValue(ListInfo[i].orderRemark);
                }
                rowtemp.CreateCell(20).SetCellValue(ListInfo[i].receiveAddress);
                rowtemp.CreateCell(21).SetCellValue(ListInfo[i].receiver);
                rowtemp.CreateCell(22).SetCellValue(ListInfo[i].receiveCountry);
                rowtemp.CreateCell(23).SetCellValue(ListInfo[i].canton);
                rowtemp.CreateCell(24).SetCellValue(ListInfo[i].city);
                rowtemp.CreateCell(25).SetCellValue(ListInfo[i].address);
                rowtemp.CreateCell(26).SetCellValue(ListInfo[i].postcode);
                if (ListInfo[i].telephone != null && ListInfo[i].telephone.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(27).SetCellValue(ListInfo[i].telephone);
                }
                if (ListInfo[i].mobile != null && ListInfo[i].mobile.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(28).SetCellValue(ListInfo[i].mobile);
                }
                if (ListInfo[i].buyerChooseLogistics != null && ListInfo[i].buyerChooseLogistics.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(29).SetCellValue(ListInfo[i].buyerChooseLogistics);
                }
                if (ListInfo[i].periodDispatch != null && ListInfo[i].periodDispatch.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(30).SetCellValue(ListInfo[i].periodDispatch);
                }
                if (ListInfo[i].dispatchID != null && ListInfo[i].dispatchID.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(31).SetCellValue(ListInfo[i].dispatchID);
                }
                if (ListInfo[i].dispatchTime != null && ListInfo[i].dispatchTime.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(32).SetCellValue(ListInfo[i].dispatchTime.ToString());
                }
                if (ListInfo[i].confirmReceiveTime != null && ListInfo[i].confirmReceiveTime.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(33).SetCellValue(ListInfo[i].confirmReceiveTime.ToString());
                }
                if (ListInfo[i].remark != null && ListInfo[i].remark.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(34).SetCellValue(ListInfo[i].remark);
                }
                if (ListInfo[i].refund != null && ListInfo[i].refund.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(35).SetCellValue('$' + ListInfo[i].refund.Value.ToString());
                }
                if (ListInfo[i].refund != null && ListInfo[i].refund.ToString().Trim() != "")
                {
                    rowtemp.CreateCell(36).SetCellValue(ListInfo[i].refundReason);
                }
            }

            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;

            return File(ms, "application/vnd.ms-excel", "AliExpress.xls");

        }
        #endregion
    }
}
