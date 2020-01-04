using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    /// <summary>
    /// 仓库 库存 盘点
    /// </summary>
    public class StockController : BaseController
    {
        //
        // GET: /Stock/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewStock()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();
            return View();
        }
        public ActionResult ViewStockOutstorage()
        {
            ViewData["WarehouseCodeList"] = App_Code.Com.Warehouses();
            return View();
        }
        [Description("产品入库查询")]
        public ContentResult GetStock(Lib.GridPager pager, string queryStr, string WarehouseCode)
        {

            IQueryable<T_Stock> queryData = db.T_Stock.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.Name != null && a.Name.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(WarehouseCode))
            {
                queryData = queryData.Where(a => a.WarehouseName.Contains(WarehouseCode));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Stock> list = new List<T_Stock>();
            foreach (var item in queryData)
            {
                T_Stock i = new T_Stock();
                item.WarehouseName = WarehouseByName(item.WarehouseName);
                i = item;
                list.Add(i);
            }
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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
      
        [Description("产品出入库查询")]
        public ContentResult GetStockOutstorage(Lib.GridPager pager, string queryStr, string WarehouseCode)
        {

            IQueryable<T_StockOutstorage> queryData = db.T_StockOutstorage.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.Name != null && a.Name.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(WarehouseCode))
            {
                queryData = queryData.Where(a => a.WarehouseCode.Contains(WarehouseCode));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_StockOutstorage> list = new List<T_StockOutstorage>();
            foreach (var item in queryData)
            {
                T_StockOutstorage i = new T_StockOutstorage();
                item.WarehouseCode = WarehouseByName(item.WarehouseCode);
                i = item;
                list.Add(i);
            }
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        

    }
}
