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
    public class StockDetailController : BaseController
    {
        //
        // GET: /StockDetail/
        EBMSEntities db = new EBMSEntities();
        /**
         * wwt 2017-3-14 
         * APP绑定仓库和供应商的下拉列表框 
        **/
        public JsonResult getSelect() 
        {
            string Warehouse = JsonConvert.SerializeObject(App_Code.Com.Warehouses());
            string Suppliers = JsonConvert.SerializeObject(App_Code.Com.SuppliersResonGy());
            return Json("{\"Warehouse\":" + Warehouse + ",\"Suppliers\":" + Suppliers + "}");
        }
       
        public ActionResult ViewStockDetailAdd()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();
            ViewData["SupplierNameList"] = App_Code.Com.SuppliersResonGy();
            return View();
        }
        public ActionResult ViewStockDetail()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();

            return View();
        }
        public ActionResult ViewStockStorageStockDetail(int ID)
        {
            T_StockStorage Model = db.T_StockStorage.SingleOrDefault(a => a.ID==ID);
            Model.WarehouseCode = WarehouseByName(Model.WarehouseCode);
            Model.SupplierName = SupplierGY(Model.SupplierName);
            ViewData["ID"] = Model.ID;
            return View(Model);
        }
        
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewWarehouseCodeGy(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public JsonResult QueryGoodsByBarCode(string BarCode)
        {
            T_goodsGY goodsModel = db.T_goodsGY.SingleOrDefault(a => a.barcode == BarCode);

            if (goodsModel != null)
            {

                return Json(new { Name = goodsModel.name, Code = goodsModel.code }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Name ="", Code = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        //产品入库详情列表 
        [HttpPost]
        public ContentResult GetStockStorageStockDetaillList(Lib.GridPager pager, int ID)
        {
            IQueryable<T_StockDetail> queryData = db.T_StockDetail.Where(a => a.Oid == ID);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_StockDetail> list = new List<T_StockDetail>();
            foreach (var item in queryData)
            {
                T_StockDetail i = new T_StockDetail();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        [Description("产品入库查询")]
        public ContentResult GetStockDetail(Lib.GridPager pager, string queryStr, string WarehouseCode)
        {
       
            IQueryable<T_StockStorage> queryData = db.T_StockStorage.Where(a=>a.Type=="入库").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.SupplierName != null && a.SupplierName.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(WarehouseCode))
            {
                queryData = queryData.Where(a => a.WarehouseCode.Contains(WarehouseCode));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_StockStorage> list = new List<T_StockStorage>();
            foreach (var item in queryData)
            {
                T_StockStorage i = new T_StockStorage();
                item.WarehouseCode = WarehouseByName(item.WarehouseCode);
                item.SupplierName = SupplierGY(item.SupplierName);
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
        //仓库列表 
        [HttpPost]
        public ContentResult GetWarehouseCodeGy(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_Warehouses> queryData = db.T_Warehouses.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr) || a.code != null && a.code.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Warehouses> list = new List<T_Warehouses>();
            foreach (var item in queryData)
            {
                T_Warehouses i = new T_Warehouses();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //[Description("新增保存")]
        public JsonResult StockDetailAdd(T_StockStorage Model, string jsonStr, string CurUser)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    string Nickname ="";
                    if (!string.IsNullOrWhiteSpace(CurUser))
                    {
                        Nickname = CurUser;
                    }else{
                         Nickname =  Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    }
                    Model.Type = "入库";
                    Model.ApplyName = Nickname;
                    Model.ApplyDate = DateTime.Now;
                    db.T_StockStorage.Add(Model);
                    db.SaveChanges();
                    List<T_StockDetail> details = App_Code.Com.Deserialize<T_StockDetail>(jsonStr);

                      
                    foreach (var item in details)
                    {
                        item.Oid = Model.ID;
                        db.T_StockDetail.Add(item);
                        db.SaveChanges();
                        string code = item.Code;
                        string WarehouseCode = Model.WarehouseCode;
                        T_Stock Smodel = db.T_Stock.SingleOrDefault(a => a.Code == code && a.WarehouseName == WarehouseCode);
                        if (Smodel != null)
                        {
                            Smodel.Qty += item.Qty;
                            db.Entry<T_Stock>(Smodel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            T_Stock StModel = new T_Stock();
                            StModel.Qty = item.Qty;
                            StModel.Name = item.Name;
                            StModel.Code = item.Code;
                            StModel.WarehouseName = WarehouseCode;
                            db.T_Stock.Add(StModel);
                            db.SaveChanges();
                        }

                        T_StockOutstorage StorageModel = new T_StockOutstorage();
                        StorageModel.Code = item.Code;
                        StorageModel.Name = item.Name;
                        StorageModel.WarehouseCode = WarehouseCode;
                        StorageModel.number = Model.ID.ToString();
                        StorageModel.Qty = item.Qty;
                        StorageModel.Type = "入库";
                        db.T_StockOutstorage.Add(StorageModel);
                        db.SaveChanges();
                    }
                    string cpcode = "";
                    for (int z = 0; z < details.Count; z++)
                    {
                           if (z == details.Count - 1)
                            {
                                cpcode += "{\"barcode\":\"" + details[z].Code + "\",\"qty\":" + details[z].Qty + "}";
                            }
                            else
                            {
                                cpcode += "{\"barcode\":\"" + details[z].Code + "\",\"qty\":" + details[z].Qty + "},";
                            }
                    }
                    EBMS.App_Code.GY gy = new App_Code.GY();
                    string cmd = "";
                    cmd = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.new.purchase.arrive.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                 "\"warehouse_code\":\"" + Model.WarehouseCode + "\"," +
                                 "\"supplier_code\":\"" + Model.SupplierName + "\"," +
                                 "\"order_type\":\"001\"," +
                                  "\"detail_list\":[" + cpcode + "]" +
                                "}";
                    string sign = gy.Sign(cmd);
                    string comcode = "";
                    comcode = "{" +
                           "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.new.purchase.arrive.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                   "\"sign\":\"" + sign + "\"," +
                                 "\"warehouse_code\":\"" + Model.WarehouseCode + "\"," +
                                 "\"supplier_code\":\"" + Model.SupplierName + "\"," +
                                 "\"order_type\":\"001\"," +
                                  "\"detail_list\":[" + cpcode + "]" +
                             "}";
                    string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                    JsonData jsonData = null;
                    jsonData = JsonMapper.ToObject(ret);

                    if (jsonData.Count == 6)
                    {
                        return Json(new { State = "Faile", Message = "写入管易失败，请联系管理员"}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public string WarehouseByCode(string Name)
        {
            List<T_Warehouses> model = db.T_Warehouses.Where(a => a.name == Name).ToList() ;
            if (model.Count > 0)
            {
                return model[0].code;
            }
            return "";
        }
        public string WarehouseByName(string Code)
        {
            List<T_Warehouses> model = db.T_Warehouses.Where(a => a.code == Code).ToList();
            if (model.Count > 0)
            {
                return model[0].name;
            }
            return "";
        }
        public string SupplierGY(string Code)
        {
            List<T_SupplierGY> model = db.T_SupplierGY.Where(a => a.code == Code).ToList();
            if (model.Count > 0)
            {
                return model[0].name;
            }
            return "";
        }
    }
}
