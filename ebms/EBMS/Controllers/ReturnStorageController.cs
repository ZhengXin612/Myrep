using EBMS.Models;
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
    public class ReturnStorageController : BaseController
    {
        //
        // GET: /ReturnStorage/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewReturnStorageAdd()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();
            return View();
        }

        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewReturnStorage()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();
            return View();
        }
        [Description("产品入库查询")]
        public ContentResult GetStockDetail(Lib.GridPager pager, string queryStr, string WarehouseCode)
        {

            IQueryable<T_StockStorage> queryData = db.T_StockStorage.Where(a => a.Type == "退货").AsQueryable();
            //if (!string.IsNullOrEmpty(queryStr))
            //{
            //    queryData = queryData.Where(a => a.SupplierName != null && a.SupplierName.Contains(queryStr));
            //}
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
        [Description("新增保存")]
        public JsonResult StockDetailAdd(T_StockStorage Model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    Model.Type = "退货";
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
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
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
                return Json(new { Name = "", Code = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public string WarehouseByCode(string Name)
        {
            List<T_Warehouses> model = db.T_Warehouses.Where(a => a.name == Name).ToList();
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
    }
}
