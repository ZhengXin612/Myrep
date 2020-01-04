using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public class ReturnToStorageController : BaseController
    {
        //访问数据库接口
        EBMSEntities db = new EBMSEntities();

        //仓库ID转换中文名
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
        //快递ID转换中文名
        public string GetExpressString(string code)
        {
           
            if (!string.IsNullOrEmpty(code))
            {

                List<T_Express> model = db.T_Express.Where(a => a.Code == code).ToList();
                if (model.Count > 0)
                {
                    return model[0].Name;
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
        //勾兑类型转换
        public string GetReturnToStorageBlendingType(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {

                List<T_ReturnToStorageBlendingType> model = db.T_ReturnToStorageBlendingType.Where(a => a.TypeCode == code).ToList();
                if (model.Count > 0)
                {
                    return model[0].TypeName;
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
        //APP 上传照片
        [HttpPost]
        public JsonResult Upload()
        {
            string link = "";
            string filesName = "";
            if (Request.Files.Count > 0)
            {
                if (Request.Files.Count == 1)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        string title = string.Empty;
                        title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                        string path = "~/Upload/ReturnStorage/" + title;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        file.SaveAs(path);
                        link = "/Upload/ReturnStorage/" + title;
                        filesName = "~/Upload/ReturnStorage/" + title;
                        return Json(new { status = true, url = path, link = link, title = filesName });
                    }
                }
                else
                {
                    string[] urllist = new string[Request.Files.Count];

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file.ContentLength > 0)
                        {
                            string title = string.Empty;
                            title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                            string path = "~/Upload/ReturnStorage/" + title;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            file.SaveAs(path);
                            urllist[i] = path;
                            link = "/Upload/ReturnStorage/" + title;
                            filesName = "~/Upload/ReturnStorage/" + title;
                        }
                    }
                    return Json(new { status = true, url = urllist, link = link, title = filesName });
                }
            }
            else
            {
                return Json(new { status = false, url = "", msg = "没有文件" });
            }
            return Json(new { status = false, url = "", msg = "" });
        }
        //APP 上传照片附件删除
        public void DeleteFile(string path)
        {
            path = Server.MapPath(path);
            //获得文件对象
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }
            //file.Create();//文件创建方法
        }
        public partial class Modular
        {

            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }
            public string simple_name { get; set; }

            public string UnitName { get; set; }
        }
        public partial class ModularQuery
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }

        //
        // GET: /ReturnToStorage/

        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        public ActionResult ViewNoWithinRangeYiSheGrid()
        {

            return View();
        }
        public ActionResult ViewNoWithinRange(string ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        public ActionResult ViewNoWithinRangeCheck(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }

        public ActionResult ViewBlending()
        {

            return View();
        }
        public ActionResult ViewBlendingQuery()
        {

            return View();
        }
        public ActionResult ViewReturnToStorageOutsourcingList()
        {
     
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
        }


        public ActionResult ViewReturnToStorage()
        {
            //得到是谁进来
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            //得到是否是财务部门
            T_RetreatGroup ExpressRecordModel = db.T_RetreatGroup.SingleOrDefault(a => a.Crew.Contains(Nickname) && a.GroupName == "财务");
            if (ExpressRecordModel != null)
            {
                ViewData["ExpressRecord"] = ExpressRecordModel.GroupName;
            }
            else
            {
                ViewData["ExpressRecord"] = "";
            }
            
            return View();
        }
        [Description("订单查询")]
        public ActionResult ViewOrderList(string queryStr)
        {
            ViewData["queryStr"] = queryStr;
            return View();
        }
        [Description("订单详情查询")]
        public ActionResult ViewOrderDetail(string queryStr)
        {
            ViewData["queryStr"] = queryStr;
            return View();
        }
        public ActionResult ViewReturnNotReceivedAdd(string ID)
        {
            ViewData["ID"] = ID;
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();

            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
        }

        public ActionResult ViewSorting()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            ViewData["name"] = name;
           
            return View();
        }
        public ActionResult ViewReturnNotReceived()
        {
            return View();
        }
        public ActionResult ViewReturnToStorageletDetails(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        public ActionResult ViewBlendingDetails(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        public ActionResult ViewNoWithinRangeGrid()
        {

            return View();
        }

        public ActionResult ViewBlendingExamine(int ID)
        {



            ViewData["ID"] = ID;
            T_ReturnToStorageBlending Model = db.T_ReturnToStorageBlending.SingleOrDefault(a => a.ID == ID);
            Model.AccountType = GetReturnToStorageBlendingType(Model.AccountType);
            return View(Model);
        }

        public ActionResult ViewBlendingAdd(string ID)
        {

            //List<T_ReceivedAfter> after = db.Database.SqlQuery<T_ReceivedAfter>("select OrderNumber from  T_ReceivedAfter where ID in (" + ID + ") GROUP BY  OrderNumber").ToList();


            ViewData["AccountTypeList"] = App_Code.Com.BlendingType();
            ViewData["ID"] = ID;
            return View();
        }
        public partial class ReceivedAfterQueryCount
        {
            public string OrderNumber { get; set; }



        }
        public int QueryReceivedAfterCount(string ID)
        {
            List<ReceivedAfterQueryCount> after = db.Database.SqlQuery<ReceivedAfterQueryCount>("select OrderNumber from  T_ReceivedAfter where ID in (" + ID + ") GROUP BY  OrderNumber").ToList();
            int s = 0;
            if (after.Count == 1)
            {
                s = 1;
            }
            else
            {
                s = 2;
            }


            return s;
        }

        public ActionResult ViewSortingAdd(int ID)
        {
            T_ReturnToStorage model = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == ID);

            model.Retreat_expressName = GetExpressString(model.Retreat_expressName);
            model.Retreat_Warehouse = GetWarehouseString(model.Retreat_Warehouse);
            ViewData["ID"] = ID;

            return View(model);
        }

		public ActionResult ViewSortingEdit(int ID)
		{
			T_ReturnToStorage model = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == ID);

			model.Retreat_expressName = GetExpressString(model.Retreat_expressName);
			model.Retreat_Warehouse = GetWarehouseString(model.Retreat_Warehouse);
			ViewData["ID"] = ID;

			return View(model);
		}

		[Description("分拣统计")]
        public ActionResult ViewStatistics()
        {
           
            return View();
        }
         public class Statistics
         {
             public string SortingName { get; set; }
            public string item_name { get; set; }
            public string Simplename { get; set; }
            public int qty { get; set; }
            public string item_code { get; set; }
            
        }

         [Description("分拣明细")]
         public ActionResult ViewSortingDetail()
         {
             return View();
         }
        public ActionResult ViewGoodsStatus()
        {

            return View();
        }
        
        //获取分拣统计数据
         public ContentResult GetStatisticsList(Lib.GridPager pager, string queryStr, string startDate, string endDate)
         {
 
           string csql = "select SortingName,count(*) qty from T_ReturnToStorage where 1=1 and isSorting=1";//统计
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 //// rsql += " and SortingName= " + queryStr;
                 //ssql += " and SortingName= '" + queryStr+"'";
                 csql += " and SortingName= '" + queryStr + "'";
             }
             if (!string.IsNullOrWhiteSpace(startDate))
             {
                 DateTime sDate = Convert.ToDateTime(startDate);
                 //rsql += " and SortingDate >'" + startDate + "'";
                 //ssql += " and SortingDate >'" + startDate + "'";
                 csql += " and SortingDate >'" + startDate + "'";
             }
             if (!string.IsNullOrWhiteSpace(endDate))
             {
                 DateTime eDate = Convert.ToDateTime(endDate).AddDays(1);
                 //rsql += " and SortingDate <'" + eDate + "'";
                 //ssql += " and SortingDate <'" + eDate + "'";
                 csql += " and SortingDate <'" + eDate + "'";
             }
             csql += "group by SortingName";
             IQueryable<Statistics> queryData = db.Database.SqlQuery<Statistics>(csql).AsQueryable();
            
             pager.totalRows = queryData.Count();
             //分页
             List<Statistics> list = queryData.OrderByDescending(c => c.qty).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList(); ;
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
             return Content(json);
         }


        public ContentResult GetGoodsStatisticsList(Lib.GridPager pager, string queryStr, string startDate, string endDate)
        {
            string csql = "with tab as (select b.item_code,b.item_name,b.Simplename, b.Unqualified + b.qualified as qty,a.SortingDate,a.SortingName" +
                         "  From  T_ReturnToStorage a join T_ReturnToStoragelet b on a.id = b.Pid" +
                         " where  1=1 and isSorting=1  ";
            //string csql = "select SortingName,count(*) qty from T_ReturnToStorage where 1=1 and isSorting=1";//统计
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                //// rsql += " and SortingName= " + queryStr;
                //ssql += " and SortingName= '" + queryStr+"'";
                csql += " and a.SortingName= '" + queryStr + "'";
            }
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime sDate = Convert.ToDateTime(startDate);
                //rsql += " and SortingDate >'" + startDate + "'";
                //ssql += " and SortingDate >'" + startDate + "'";
                csql += " and a.SortingDate >'" + startDate + "'";
            }
            else {
                csql += " and a.SortingDate >'" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00") + "'";
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime eDate = Convert.ToDateTime(endDate).AddDays(1);
                //rsql += " and SortingDate <'" + eDate + "'";
                //ssql += " and SortingDate <'" + eDate + "'";
                csql += " and a.SortingDate <'" + eDate + "'";
            }
            else
            {
                csql += " and a.SortingDate <'" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00") + "'";
            }
            csql += "  ) select SortingName, item_name, Simplename, sum(qty) qty,item_code From tab group by SortingName, item_name, Simplename,item_code";//统计
            IQueryable<Statistics> queryData = db.Database.SqlQuery<Statistics>(csql).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<Statistics> list = queryData.OrderByDescending(c => c.SortingName).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList(); ;
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }
        public JsonResult GetStatisticsData(string startDate, string endDate)
         {
             string rsql = "select SortingName='收货数', count(*) qty  from T_ReturnToStorage where 1=1";//收货数量
             string ssql = "select SortingName='分拣数', count(*) qty from T_ReturnToStorage where isSorting=1";//分拣数量

             if (!string.IsNullOrWhiteSpace(startDate))
             {
                 DateTime sDate = Convert.ToDateTime(startDate);
                 rsql += " and SortingDate >'" + startDate + "'";
                 ssql += " and SortingDate >'" + startDate + "'";

             }
             if (!string.IsNullOrWhiteSpace(endDate))
             {
                 DateTime eDate = Convert.ToDateTime(endDate).AddDays(1);
                 rsql += " and SortingDate <'" + eDate + "'";
                 ssql += " and SortingDate <'" + eDate + "'";

             }
             int rQty = db.Database.SqlQuery<Statistics>(rsql).FirstOrDefault().qty;
            // ViewData["rQty"] = rQty;
             int sQty = db.Database.SqlQuery<Statistics>(ssql).FirstOrDefault().qty;
            // ViewData["sQty"] = sQty;
             return Json(new { rQty = rQty, sQty = sQty });
         }
        //获取T_ReturnToStorageBlendingDetails详情列表  
        public JsonResult ReturnToStorageBlendingDetailsAdd(Lib.GridPager pager, int ID)
        {



            List<T_ReturnToStorageBlendingDetails> queryData = db.T_ReturnToStorageBlendingDetails.Where(a => a.Pid == ID).ToList();
            pager.totalRows = queryData.Count();

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData.ToList(), Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //编辑获取详情列表  
        public JsonResult SortingAdd(Lib.GridPager pager, int ID)
        {
            T_ReturnToStorage Model = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == ID);

            string daodi = "";
            string sql = "";
            string sql1 = "";
            string sql2 = "";
            string Rid = "";
            string Eid = "";
            string expressNumber = Model.Retreat_expressNumber;
            List<T_ExchangeCenter> ExchangeCenterList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == expressNumber && a.IsDelete == 0).ToList();
            List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNumber && a.Isdelete == "0" && a.Status != 3).ToList();
            List<T_Intercept> interceptList = db.T_Intercept.Where(a => a.MailNo == expressNumber && a.IsDelete == 0).ToList();
            List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.CollectExpressNumber == expressNumber).ToList();
            if (RetreatList.Count > 0)
            {

                for (int z = 0; z < RetreatList.Count; z++)
                {
                    if (z == 0)
                    {
                        Rid += RetreatList[z].ID.ToString();
                    }
                    else
                    {
                        Rid += ',' + RetreatList[z].ID.ToString();
                    }
                }


                sql = "  select item_code as item_code ,item_name as item_name,qty as qty,(select top 1 spec_name from T_WDTGoods where goods_no=item_code) as simple_name, (select top 1 unit_name from T_WDTGoods where goods_no = item_code) as unit_name from T_RetreatDetails where oid in  (" + Rid + ")";
                daodi += "退货退款";
            }
            if (ExchangeCenterList.Count > 0)
            {
                for (int z = 0; z < ExchangeCenterList.Count; z++)
                {
                    if (z == 0)
                    {
                        Eid += ExchangeCenterList[z].ID.ToString();
                    }
                    else
                    {
                        Eid += ',' + ExchangeCenterList[z].ID.ToString();
                    }
                }
                sql1 = "  select SendProductCode as item_code,SendProductName as item_name,SendProductNum as qty,(select top 1 spec_name from T_WDTGoods where goods_no=SendProductCode) as simple_name, (select top 1 unit_name from T_WDTGoods where goods_no = SendProductCode) as unit_name from T_ExchangeDetail where ExchangeCenterId in  (" + Eid + ") ";
                daodi += "换货";
            }
            if (interceptList.Count > 0)
            {
                for (int z = 0; z < interceptList.Count; z++)
                {
                    if (z == 0)
                    {
                        Eid += interceptList[z].ID.ToString();
                    }
                    else
                    {
                        Eid += ',' + interceptList[z].ID.ToString();
                    }
                }
                sql2 = "  select Code as item_code,Name as item_name,Num as qty from,(select top 1 spec_name from T_WDTGoods where goods_no=Code) as simple_name, (select top 1 unit_name from T_WDTGoods where goods_no = Code) as unit_name T_InterceptDetail where InterceptId in  (" + Eid + ") ";
                daodi += "拦截";
            }
            if (daodi == "")
            {
                daodi = "无";
            }

            List<T_ReturnToStorageDetails> Details = db.T_ReturnToStorageDetails.Where(a => a.Pid == ID).ToList();
            if (Details.Count > 0)
            {
                for (int x = 0; x < Details.Count; x++)
                {
                    db.T_ReturnToStorageDetails.Remove(Details[x]);
                    db.SaveChanges();
                }
            }
            Model.ModularName = daodi;
            int i = db.SaveChanges();
            if (sql != "")
            {
                List<Modular> queryDatas = db.Database.SqlQuery<Modular>(sql).ToList();
                for (int z = 0; z < queryDatas.Count; z++)
                {

                    List<T_ReturnToStorageDetails> queryReturnToStorageDetails = db.Database.SqlQuery<T_ReturnToStorageDetails>("select * from T_ReturnToStorageDetails where Pid='" + ID + "' and item_code='" + queryDatas[z].item_code + "'").ToList();

                    if (queryReturnToStorageDetails.Count == 0)
                    {
                        T_ReturnToStorageDetails Dmodel = new T_ReturnToStorageDetails();
                       // T_WDTGoods goods= db.T_WDTGoods.FirstOrDefault(a => a.goods_no == Dmodel.item_code);
                        Dmodel.Pid = ID;
                        Dmodel.item_code = queryDatas[z].item_code;
                        Dmodel.item_name = queryDatas[z].item_name;
                        Dmodel.qty = queryDatas[z].qty;
                        T_WDTGoods goods = db.T_WDTGoods.FirstOrDefault(a => a.goods_no == Dmodel.item_code);
                        //  Dmodel.Simplename = queryDatas[z].simple_name;
                        // Dmodel.Simplename = db.T_WDTGoods.SingleOrDefault(a => a.goods_no == queryDatas[z].item_code).unit_name;
                        Dmodel.Simplename = goods.unit_name;
                         db.T_ReturnToStorageDetails.Add(Dmodel);
                        db.SaveChanges();
                    }
                    else
                    {
                        string code=queryDatas[z].item_code;
                        T_ReturnToStorageDetails EditReturnToStorageDetails = db.T_ReturnToStorageDetails.Single(a => a.Pid == ID && a.item_code == code);
                        EditReturnToStorageDetails.qty = EditReturnToStorageDetails.qty + queryDatas[z].qty;
                        db.Entry<T_ReturnToStorageDetails>(EditReturnToStorageDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            if (sql1 != "")
            {
                List<Modular> queryDatas = db.Database.SqlQuery<Modular>(sql1).ToList();
                for (int z = 0; z < queryDatas.Count; z++)
                {
                    List<T_ReturnToStorageDetails> queryReturnToStorageDetails = db.Database.SqlQuery<T_ReturnToStorageDetails>("select * from T_ReturnToStorageDetails where Pid='" + ID + "' and item_code='" + queryDatas[z].item_code + "'").ToList();

                    if (queryReturnToStorageDetails.Count == 0)
                    {
                        string code = queryDatas[z].item_code;
                        T_ReturnToStorageDetails Dmodel = new T_ReturnToStorageDetails();
                        
                        Dmodel.Pid = ID;
                        
                        Dmodel.item_code = queryDatas[z].item_code;
                        Dmodel.item_name = queryDatas[z].item_name;
                        T_WDTGoods goods = db.T_WDTGoods.FirstOrDefault(a => a.goods_no == Dmodel.item_code);
                        //  Dmodel.Simplename = queryDatas[z].simple_name;
                        // Dmodel.Simplename = db.T_WDTGoods.SingleOrDefault(a => a.goods_no == queryDatas[z].item_code).unit_name;
                        Dmodel.Simplename = goods.unit_name;
                        Dmodel.qty = queryDatas[z].qty;
                        db.T_ReturnToStorageDetails.Add(Dmodel);
                        db.SaveChanges();
                    }
                    else
                    {
                        string code = queryDatas[z].item_code;
                        T_ReturnToStorageDetails EditReturnToStorageDetails = db.T_ReturnToStorageDetails.Single(a => a.Pid == ID && a.item_code == code);
                        EditReturnToStorageDetails.qty = EditReturnToStorageDetails.qty + queryDatas[z].qty;
                        db.Entry<T_ReturnToStorageDetails>(EditReturnToStorageDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                 
                }
            }

            if (sql2 != "")
            {
                List<Modular> queryDatas = db.Database.SqlQuery<Modular>(sql2).ToList();
                for (int z = 0; z < queryDatas.Count; z++)
                {
                    List<T_ReturnToStorageDetails> queryReturnToStorageDetails = db.Database.SqlQuery<T_ReturnToStorageDetails>("select * from T_ReturnToStorageDetails where Pid='" + ID + "' and item_code='" + queryDatas[z].item_code + "'").ToList();

                    if (queryReturnToStorageDetails.Count == 0)
                    {
                        string code = queryDatas[z].item_code;
                        T_ReturnToStorageDetails Dmodel = new T_ReturnToStorageDetails();
 
                        Dmodel.Pid = ID;
                         
                        Dmodel.item_code = queryDatas[z].item_code;
                        Dmodel.item_name = queryDatas[z].item_name;
                        T_WDTGoods goods = db.T_WDTGoods.FirstOrDefault(a => a.goods_no == Dmodel.item_code);
                        //  Dmodel.Simplename = queryDatas[z].simple_name;
                        // Dmodel.Simplename = db.T_WDTGoods.SingleOrDefault(a => a.goods_no == queryDatas[z].item_code).unit_name;
                        Dmodel.Simplename = goods.unit_name;
                        Dmodel.qty = queryDatas[z].qty;
                        db.T_ReturnToStorageDetails.Add(Dmodel);
                        db.SaveChanges();
                    }
                    else
                    {
                        string code = queryDatas[z].item_code;
                        T_ReturnToStorageDetails EditReturnToStorageDetails = db.T_ReturnToStorageDetails.Single(a => a.Pid == ID && a.item_code == code);
                        EditReturnToStorageDetails.qty = EditReturnToStorageDetails.qty + queryDatas[z].qty;
                        db.Entry<T_ReturnToStorageDetails>(EditReturnToStorageDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            List<Modular> queryData = db.Database.SqlQuery<Modular>(" select item_code,item_name,qty,(select top 1 spec_name from T_WDTGoods where goods_no=item_code) as simple_name ,(select top 1 unit_name from T_WDTGoods where goods_no = item_code) as UnitName  from T_ReturnToStorageDetails  where Pid = '" + ID + "'").ToList();
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

        public ActionResult ViewReturnToStorageAdd()
        {

            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();

            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
        }
        public ActionResult AppViewReturnToStorageAdd()
        {

            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();

            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr,string queryStrs,string queryCode)
        {
            IQueryable<T_WDTGoods> queryData = db.T_WDTGoods.Where(a=>a.spec_aux_unit_name!="1" || string.IsNullOrEmpty(a.spec_aux_unit_name)).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.goods_name != null && a.goods_name.Contains(queryStr) || a.goods_no != null && a.goods_no.Contains(queryStr) || a.spec_name != null && a.spec_name.Contains(queryStr) );
            }
            if (!string.IsNullOrEmpty(queryStrs))
            {
                queryData = queryData.Where(a => a.spec_name != null && a.spec_name.Contains(queryStrs));
            }
            if (!string.IsNullOrEmpty(queryCode))
            {
                queryData = queryData.Where(a => a.barcode != null && a.barcode.Equals(queryCode));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
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
        //订单信息 
        [HttpPost]
        public JsonResult GetOrderListGY(Lib.GridPager pager, string queryStr)
        {

            App_Code.GY gy = new App_Code.GY();
            string cmd = "";

            cmd = "{" +
                  "\"appkey\":\"171736\"," +
                  "\"method\":\"gy.erp.trade.get\"," +
                  "\"page_no\":1," +
                  "\"page_size\":10," +
                  "\"receiver_mobile\":\"" + queryStr + "\"," +
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
                "\"receiver_mobile\":\"" + queryStr + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                {
                    // ViewData["msg"] = " $.messager.alert('提示', '订单号不存在','info')";
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            int total = int.Parse(jsonData["total"].ToString());

            List<T_OrderList> DetailsList = new List<T_OrderList>();
            for (int i = 0; i < total; i++)
            {
                T_OrderList DetailsModel = new T_OrderList();
                JsonData jsonOrders = jsonData["orders"][i];
                DetailsModel.code = isNULL(jsonOrders["code"]).ToString();
                DetailsModel.platform_code = isNULL(jsonOrders["platform_code"]).ToString();
                DetailsModel.shop_name = isNULL(jsonOrders["shop_name"]).ToString();
                DetailsModel.vip_code = isNULL(jsonOrders["vip_code"]).ToString();
                DetailsModel.vip_name = isNULL(jsonOrders["vip_name"]).ToString();
                DetailsModel.receiver_mobile = isNULL(jsonOrders["receiver_mobile"]).ToString();
                DetailsModel.receiver_name = isNULL(jsonOrders["receiver_name"]).ToString();
                DetailsModel.receiver_address = isNULL(jsonOrders["receiver_address"]).ToString();
                DetailsModel.qty = isNULL(jsonOrders["qty"]).ToString();
                DetailsList.Add(DetailsModel);
            }

            if (jsonData.Count != 6 || jsonData["orders"].Count != 0)
            {
                cmd = "{" +
                "\"appkey\":\"171736\"," +
                "\"method\":\"gy.erp.trade.history.get\"," +
                "\"page_no\":1," +
                "\"page_size\":10," +
                "\"receiver_mobile\":\"" + queryStr + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                int totals = int.Parse(jsonData["total"].ToString());
                for (int i = 0; i < totals; i++)
                {
                    T_OrderList DetailsModel = new T_OrderList();
                    JsonData jsonOrders = jsonData["orders"][i];
                    DetailsModel.code = isNULL(jsonOrders["code"]).ToString();
                    DetailsModel.platform_code = isNULL(jsonOrders["platform_code"]).ToString();
                    DetailsModel.shop_name = isNULL(jsonOrders["shop_name"]).ToString();
                    DetailsModel.vip_code = isNULL(jsonOrders["vip_code"]).ToString();
                    DetailsModel.vip_name = isNULL(jsonOrders["vip_name"]).ToString();
                    DetailsModel.receiver_mobile = isNULL(jsonOrders["receiver_mobile"]).ToString();
                    DetailsModel.receiver_name = isNULL(jsonOrders["receiver_name"]).ToString();
                    DetailsModel.receiver_address = isNULL(jsonOrders["receiver_address"]).ToString();
                    DetailsModel.qty = isNULL(jsonOrders["qty"]).ToString();
                    DetailsList.Add(DetailsModel);
                }
            }

            var json = new
            {
                rows = (from r in DetailsList
                        select new T_OrderList
                        {
                            code = r.code,
                            platform_code = r.platform_code,
                            shop_name = r.shop_name,
                            vip_code = r.vip_code,
                            vip_name = r.vip_name,
                            receiver_mobile = r.receiver_mobile,
                            receiver_name = r.receiver_name,
                            receiver_address = r.receiver_address,
                            qty = r.qty,
                        }).ToArray()
            };

            return Json(json, JsonRequestBehavior.AllowGet);

            //IQueryable<T_OrderList> queryData = db.T_OrderList.AsQueryable();
            //if (!string.IsNullOrEmpty(queryStr))
            //{
            //    queryData = queryData.Where(a => a.receiver_mobile != null && a.receiver_mobile.Contains(queryStr));
            //}
            //pager.totalRows = queryData.Count();
            ////分页
            //queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            //List<T_OrderList> list = new List<T_OrderList>();
            //foreach (var item in queryData)
            //{
            //    T_OrderList i = new T_OrderList();
            //    i = item;
            //    list.Add(i);
            //}
            //string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            //return Content(json);

        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }


        //订单详情信息 
        [HttpPost]
        public JsonResult GetOrderDetailGY(Lib.GridPager pager, string queryStr)
        {

            App_Code.GY gy = new App_Code.GY();
            string cmd = "";

            cmd = "{" +
                  "\"appkey\":\"171736\"," +
                  "\"method\":\"gy.erp.trade.get\"," +
                  "\"page_no\":1," +
                  "\"page_size\":10," +
                  "\"platform_code\":\"" + queryStr + "\"," +
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
                "\"platform_code\":\"" + queryStr + "\"," +
                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                "}";
                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                {
                    ViewData["msg"] = " $.messager.alert('提示', '订单号不存在','info')";
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
            }

            JsonData jsonOrders = jsonData["orders"][0];
            List<T_OrderDetail> DetailsList = new List<T_OrderDetail>();

            JsonData jsonDetails = jsonOrders["details"];
            for (int i = 0; i < jsonDetails.Count; i++)
            {
                T_OrderDetail DetailsModel = new T_OrderDetail();
                string ss = jsonDetails[i]["item_code"] == null ? "" : jsonDetails[i]["item_code"].ToString();
                DetailsModel.item_code = ss;
                DetailsModel.item_name = jsonDetails[i]["item_name"] == null ? "" : jsonDetails[i]["item_name"].ToString();

                DetailsModel.qty = int.Parse(jsonDetails[i]["qty"].ToString());
                DetailsList.Add(DetailsModel);

            }

            var json = new
            {
                rows = (from r in DetailsList
                        select new T_OrderDetail
                        {
                            item_code = r.item_code,
                            item_name = r.item_name,
                            qty = r.qty,
                        }).ToArray()
            };


            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //退回委外单列表
        [HttpPost]
        public ContentResult GetReturnOutsourcingList(Lib.GridPager pager, string queryStr, string RetreatWarehouseList, string RetreatexpressNameList, string Fjstatethisdate, string FjEndthisdate)
        {
            IQueryable<T_ReturnToStorage> queryData = null;
                queryData = db.T_ReturnToStorage.Where(a => a.IsDelete == 0 && a.ExternalSingle=="").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.CollectName != null && a.CollectName.Contains(queryStr) || a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr) || a.receivermobile != null && a.receivermobile.Contains(queryStr) || a.SortingName != null && a.SortingName.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList);
            }
            if (!string.IsNullOrWhiteSpace(Fjstatethisdate))
            {
                DateTime start = DateTime.Parse(Fjstatethisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(FjEndthisdate))
            {
                DateTime end = DateTime.Parse(FjEndthisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate <= end);
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorage> list = new List<T_ReturnToStorage>();
            foreach (var item in queryData)
            {
                T_ReturnToStorage i = new T_ReturnToStorage();

                i = item;
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                i.Retreat_expressName = GetExpressString(item.Retreat_expressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        //京东退回委外单列表
        [HttpPost]
        public ContentResult GetReturnOutsourcingLists(Lib.GridPager pager, string queryStr, string Fjstatethisdate, string FjEndthisdate)
        {
            IQueryable<T_ReturnToStorage> queryData = null;
            queryData = db.T_ReturnToStorage.Where(a => a.IsDelete == 0 && a.ExternalSingle == ""&&a.Retreat_Warehouse=="2").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.CollectName != null && a.CollectName.Contains(queryStr) || a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr) || a.receivermobile != null && a.receivermobile.Contains(queryStr) || a.SortingName != null && a.SortingName.Contains(queryStr));
            }
            
            if (!string.IsNullOrWhiteSpace(Fjstatethisdate))
            {
                DateTime start = DateTime.Parse(Fjstatethisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(FjEndthisdate))
            {
                DateTime end = DateTime.Parse(FjEndthisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate <= end);
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorage> list = new List<T_ReturnToStorage>();
            foreach (var item in queryData)
            {
                T_ReturnToStorage i = new T_ReturnToStorage();

                i = item;
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                i.Retreat_expressName = GetExpressString(item.Retreat_expressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        //退回列表
        [HttpPost]
        public ContentResult GetReturnToStorageList(Lib.GridPager pager, string queryStr, string status, string RetreatWarehouseList, string RetreatexpressNameList, string DetailsQuery, string statedate, string EndDate, string Fjstatethisdate, string FjEndthisdate,string selSorting)
        {

            IQueryable<T_ReturnToStorage> queryData = null;
             if (!string.IsNullOrEmpty(DetailsQuery))
             {
                 queryData = db.Database.SqlQuery<T_ReturnToStorage>("select * from T_ReturnToStorage where ID in (select Pid from T_ReturnToStoragelet where item_code like '%" + DetailsQuery + "%'  or item_name like  '%" + DetailsQuery + "%'  or ExternalSingle like '%"+ DetailsQuery + "%' ) and IsDelete='0'").AsQueryable();
             }
             else
             {
                 queryData = db.T_ReturnToStorage.Where(a => a.IsDelete == 0).AsQueryable();
             }
            if (selSorting != "" && selSorting != null)
            {
                int ss = int.Parse(selSorting);
                queryData = queryData.Where(a => a.isSorting== ss);

            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.CollectName != null && a.CollectName.Contains(queryStr) || a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr) || a.receivermobile != null && a.receivermobile.Contains(queryStr) || a.SortingName != null && a.SortingName.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList);
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            if (!string.IsNullOrEmpty(status) && status != "9999")
            {
                queryData = queryData.Where(a => a.ModularName == status);
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                queryData = queryData.Where(a => a.GoodsReceiptDate != null && a.GoodsReceiptDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(a => a.GoodsReceiptDate != null && a.GoodsReceiptDate <= end);
            }
            if (!string.IsNullOrWhiteSpace(Fjstatethisdate))
            {
                DateTime start = DateTime.Parse(Fjstatethisdate + " 00:00:00");
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(FjEndthisdate))
            {
                DateTime end = DateTime.Parse(FjEndthisdate + " 23:59:59");
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate <= end);
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorage> list = new List<T_ReturnToStorage>();
            foreach (var item in queryData)
            {
                T_ReturnToStorage i = new T_ReturnToStorage();

                i = item;
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                i.Retreat_expressName = GetExpressString(item.Retreat_expressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        //财务已勾兑列表
        [HttpPost]
        public ContentResult GetViewBlendingQueryList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_ReturnToStorageBlending> queryData = db.T_ReturnToStorageBlending.Where(a => a.IsBlending == 1).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || a.CustomerCode != null && a.CustomerCode.Contains(queryStr) || a.CustomerName != null && a.CustomerName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorageBlending> list = new List<T_ReturnToStorageBlending>();
            foreach (var item in queryData)
            {
                T_ReturnToStorageBlending i = new T_ReturnToStorageBlending();

                i = item;
                i.AccountType = GetReturnToStorageBlendingType(i.AccountType);
                i.CollectExpressName = GetExpressString(item.CollectExpressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        public partial class ReturnNoWithinRange
        {
            public int ID { get; set; }
            public int status { get; set; }
            public string Remarks { get; set; }
            public string OrderNumber { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string CollectExpressNumber { get; set; }
            public string ShopName { get; set; }
            public string CustomerName { get; set; }
            public string Type { get; set; }
            public int ProductNumber { get; set; }
            public string ShenQName { get; set; }

        }
        //不在退货范围内
        [HttpPost]
        public ContentResult GetViewReturnNoWithinRangeList(Lib.GridPager pager, string queryStr)
        {
            string sql = "select ID,status,Remarks,ShenQName,isnull((select ProductNumber from T_ReceivedAfter where ID=w.AfterID and w.status=0),0) as ProductNumber,(select Type from T_ReceivedAfter where ID=w.AfterID and w.status=0) as Type,(select OrderNumber from T_ReceivedAfter where ID=w.AfterID and w.status=0) as OrderNumber ,(select ProductCode from T_ReceivedAfter where ID=w.AfterID and w.status=0) as ProductCode,(select ProductName from T_ReceivedAfter where ID=w.AfterID and w.status=0) as ProductName,(select CollectExpressNumber from T_ReceivedAfter where ID=w.AfterID and w.status=0) as CollectExpressNumber,(select ShopName from T_ReceivedAfter where ID=w.AfterID and w.status=0) as ShopName ,(select CustomerName from T_ReceivedAfter where ID=w.AfterID and w.status=0) as CustomerName  from T_ReturnNoWithinRange  w where  status='0'";
            IQueryable<ReturnNoWithinRange> queryData = db.Database.SqlQuery<ReturnNoWithinRange>(sql).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || a.CollectExpressNumber != null && a.CollectExpressNumber.Contains(queryStr) || a.CustomerName != null && a.CustomerName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<ReturnNoWithinRange> list = new List<ReturnNoWithinRange>();
            foreach (var item in queryData)
            {
                ReturnNoWithinRange i = new ReturnNoWithinRange();

                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //不在退货范围内
        [HttpPost]
        public ContentResult GetViewReturnNoWithinRangeNOList(Lib.GridPager pager, string queryStr)
        {
            string sql = "select ID,status,Remarks,ShenQName,isnull((select ProductNumber from T_ReceivedAfter where ID=w.AfterID and w.status=0),0)  as ProductNumber,(select Type from T_ReceivedAfter where ID=w.AfterID ) as Type,(select OrderNumber from T_ReceivedAfter where ID=w.AfterID ) as OrderNumber ,(select ProductCode from T_ReceivedAfter where ID=w.AfterID ) as ProductCode,(select ProductName from T_ReceivedAfter where ID=w.AfterID ) as ProductName,(select CollectExpressNumber from T_ReceivedAfter where ID=w.AfterID ) as CollectExpressNumber,(select ShopName from T_ReceivedAfter where ID=w.AfterID ) as ShopName ,(select CustomerName from T_ReceivedAfter where ID=w.AfterID ) as CustomerName  from T_ReturnNoWithinRange  w";
            IQueryable<ReturnNoWithinRange> queryData = db.Database.SqlQuery<ReturnNoWithinRange>(sql).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || a.CollectExpressNumber != null && a.CollectExpressNumber.Contains(queryStr) || a.CustomerName != null && a.CustomerName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<ReturnNoWithinRange> list = new List<ReturnNoWithinRange>();
            foreach (var item in queryData)
            {
                ReturnNoWithinRange i = new ReturnNoWithinRange();

                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }

        //财务勾兑列表
        [HttpPost]
        public ContentResult GetViewBlendingList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_ReturnToStorageBlending> queryData = db.T_ReturnToStorageBlending.Where(a => a.IsBlending == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber != null && a.OrderNumber.Contains(queryStr) || a.CustomerCode != null && a.CustomerCode.Contains(queryStr) || a.CustomerName != null && a.CustomerName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorageBlending> list = new List<T_ReturnToStorageBlending>();
            foreach (var item in queryData)
            {
                T_ReturnToStorageBlending i = new T_ReturnToStorageBlending();

                i = item;
                i.AccountType = GetReturnToStorageBlendingType(i.AccountType);
                i.CollectExpressName = GetExpressString(item.CollectExpressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //财务勾兑详情列表
        [HttpPost]
        public ContentResult GetViewBlendingDetailsList(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_ReturnToStorageBlendingDetails> queryData = db.T_ReturnToStorageBlendingDetails.Where(a => a.Pid == ID).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductCode != null && a.ProductCode.Contains(queryStr) || a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStorageBlendingDetails> list = new List<T_ReturnToStorageBlendingDetails>();
            foreach (var item in queryData)
            {
                T_ReturnToStorageBlendingDetails i = new T_ReturnToStorageBlendingDetails();

                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }

        [HttpPost]
        [Description("退货退款删除")]
        public JsonResult DeleteRetreatFinance(int del)
        {
            T_ReturnToStorage model = db.T_ReturnToStorage.Find(del);
            model.IsDelete = 1;
            db.Entry<T_ReturnToStorage>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Description("修改仓库")]
        public JsonResult ChangeHouse(int ID,string house)
        {
            T_ReturnToStorage model = db.T_ReturnToStorage.Find(ID);
            model.Retreat_Warehouse = house;
            db.Entry<T_ReturnToStorage>(model).State = System.Data.Entity.EntityState.Modified;
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退回件分拣",
                OperateContent = "修改仓库",
                Operater = Server.UrlDecode(Request.Cookies["Nickname"].Value),
                OperateTime = DateTime.Now,
                PID = model.ID
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //收货详情
        [HttpPost]
        public ContentResult GetViewReturnToStorageletList(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_ReturnToStoragelet> queryData = db.T_ReturnToStoragelet.Where(a => a.Pid == ID).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.item_code != null && a.item_code.Contains(queryStr) || a.item_name != null && a.item_name.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReturnToStoragelet> list = new List<T_ReturnToStoragelet>();
            foreach (var item in queryData)
            {
                T_ReturnToStoragelet i = new T_ReturnToStoragelet();

                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //售后问题列表
        [HttpPost]
        public ContentResult GetReturnNotReceivedList(Lib.GridPager pager, string queryStr,string shop,string codename,string statedate,string EndDate)
        {
            IQueryable<T_ReceivedAfter> queryData = db.T_ReceivedAfter.Where(a => a.ProductNumber != null && a.ProductNumber!= 0 && a.IsHandle == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.CollectExpressNumber != null && a.CollectExpressNumber.Contains(queryStr) || a.OrderNumber != null && a.OrderNumber.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(shop))
            {
                queryData = queryData.Where(a => a.ShopName.Contains(shop));
            }
            if (!string.IsNullOrEmpty(codename))
            {
                queryData = queryData.Where(a => a.ProductName.Contains(codename));
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                queryData = queryData.Where(a => a.CreatTime != null && a.CreatTime >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(a => a.CreatTime  <= end);//!= null && a.CreatTime
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ReceivedAfter> list = new List<T_ReceivedAfter>();
            foreach (var item in queryData)
            {
                T_ReceivedAfter i = new T_ReceivedAfter();

                i = item;

                i.CollectExpressName = GetExpressString(item.CollectExpressName);

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list,Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }

        //匹配退款和换货表
        [HttpPost]
        public JsonResult ModularRT(string expressNumber)
        {
            T_ReturnToStorage queryData = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == expressNumber);

            List<T_Retreat> Rmodle = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNumber).ToList();

            List<T_ExchangeCenter> Emodel = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == expressNumber).ToList();

            int i = 0;
            if (Rmodle.Count > 0)
            {
                queryData.ModularName = "退货退款";
                //queryData.OrderNumber = Rmodle[0].Retreat_OrderNumber;
                //queryData.ShopName = Rmodle[0].Retreat_dianpName;
                //queryData.CustomerName = Rmodle[0].Retreat_wangwang;
                //queryData.CustomerCode = Rmodle[0].Retreat_CustomerName;

                db.Entry<T_ReturnToStorage>(queryData).State = System.Data.Entity.EntityState.Modified;
                i = db.SaveChanges();

            }
            else if (Emodel.Count > 0)
            {
                queryData.ModularName = "换货";
                //queryData.OrderNumber = Emodel[0].OrderCode;
                //queryData.ShopName = Emodel[0].StoreName;
                //queryData.ShopCode = Emodel[0].StoreCode;
                //queryData.CustomerName = Emodel[0].VipCode;
                //queryData.CustomerCode = Emodel[0].VipName;
                db.Entry<T_ReturnToStorage>(queryData).State = System.Data.Entity.EntityState.Modified;
                i = db.SaveChanges();
            }




            return Json(i, JsonRequestBehavior.AllowGet);

        }

        //查询订单详情
        [HttpPost]
        public JsonResult GetOrderDetailList(Lib.GridPager pager, string code)
        {

            App_Code.GY gy = new App_Code.GY();
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
                    ViewData["msg"] = " $.messager.alert('提示', '订单号不存在','info')";
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
            }

            JsonData jsonOrders = jsonData["orders"][0];
            List<T_OrderDetail> DetailsList = new List<T_OrderDetail>();

            JsonData jsonDetails = jsonOrders["details"];
            for (int i = 0; i < jsonDetails.Count; i++)
            {
                T_OrderDetail DetailsModel = new T_OrderDetail();
                string ss = jsonDetails[i]["item_code"] == null ? "" : jsonDetails[i]["item_code"].ToString();
                DetailsModel.item_code = ss;
                DetailsModel.item_name = jsonDetails[i]["item_name"] == null ? "" : jsonDetails[i]["item_name"].ToString();

                DetailsModel.qty = int.Parse(jsonDetails[i]["qty"].ToString());
                DetailsList.Add(DetailsModel);

            }

            var json = new
            {
                rows = (from r in DetailsList
                        select new T_OrderDetail
                        {
                            item_code = r.item_code,
                            item_name = r.item_name,
                            qty = r.qty,
                        }).ToArray()
            };


            return Json(json, JsonRequestBehavior.AllowGet);

        }
        //未分拣列表
        [HttpPost]
        public ContentResult GetSortingList(Lib.GridPager pager, string queryStr, string RetreatWarehouseList, string statedate, string EndDate)
        {
            IQueryable<T_ReturnToStorage> queryData = db.T_ReturnToStorage.Where(a => a.isSorting == 0 && a.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Retreat_expressNumber == queryStr);
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse != null && a.Retreat_Warehouse.Contains(RetreatWarehouseList));
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                queryData = queryData.Where(a => a.GoodsReceiptDate != null && a.GoodsReceiptDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(a => a.GoodsReceiptDate != null && a.GoodsReceiptDate <= end);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_ReturnToStorage> list = queryData.ToList() ;
			foreach (var item in list)
			{
				//T_ReturnToStorage i = new T_ReturnToStorage();

				//i = item;
				item.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
				item.Retreat_expressName = GetExpressString(item.Retreat_expressName);

				//list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        //app下拉框获取
        public JsonResult getAppSelect()
        {

            IQueryable<T_Warehouses> modWarehouses = db.T_Warehouses.AsQueryable();
            string Warehouses = JsonConvert.SerializeObject(modWarehouses, Lib.Comm.setTimeFormat());

            IQueryable<T_Express> modExpress = db.T_Express.AsQueryable();
            string Express = JsonConvert.SerializeObject(modExpress, Lib.Comm.setTimeFormat());


            string Result = "{\"Warehouses\":" + Warehouses + ",\"Express\":" + Express + "}";
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        //新增保存
        [HttpPost]
        [Description("退回件新增保存")]
        public JsonResult ViewReturnToStorageAddSave(T_ReturnToStorage model, string detailList, string curUser, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {



                try
                {
                    string expressNumber = model.Retreat_expressNumber.Trim();
                    List<T_ReturnToStorage> RmodelList = db.T_ReturnToStorage.Where(a => a.Retreat_expressNumber == expressNumber && a.IsDelete == 0).ToList();
                    if (RmodelList.Count > 0)
                    {
                        return Json(new { State = "Faile", Message = "该快递已录入过退回件" }, JsonRequestBehavior.AllowGet);
                    }


                    string daodi = "";
                    // string sql = "";
                    // string Rid = "";
                    List<T_ExchangeCenter> ExchangeCenterList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == expressNumber).ToList();
                    List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNumber).ToList();
                    List<T_Intercept> interceptList = db.T_Intercept.Where(a => a.MailNo == expressNumber).ToList();
                    //   List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.CollectExpressNumber == expressNumber).ToList();
                    if (RetreatList.Count > 0)
                    {
                        //for (int z = 0; z < RetreatList.Count; z++)
                        //{
                        //    if (z == 0)
                        //    {
                        //        Rid += RetreatList[z].ID.ToString();
                        //    }
                        //    else
                        //    {
                        //        Rid += ',' + RetreatList[z].ID.ToString();
                        //    }
                        //}
                        //sql = "  select item_code as item_code ,item_name as item_name,qty as qty from T_RetreatDetails where oid in (" + Rid + ")";
                        daodi += "退货退款";
                    }
                    if (ExchangeCenterList.Count > 0)
                    {
                        //for (int z = 0; z < ExchangeCenterList.Count; z++)
                        //{
                        //    if (z == 0)
                        //    {
                        //        Rid += ExchangeCenterList[z].ID.ToString();
                        //    }
                        //    else
                        //    {
                        //        Rid += ',' + ExchangeCenterList[z].ID.ToString();
                        //    }
                        //}
                        //sql = "  select SendProductCode as item_code,SendProductName as item_name,SendProductNum as qty from T_ExchangeDetail where ExchangeCenterId in (" + Rid + ")";
                        daodi += "换货";
                    }
                    if (interceptList.Count > 0)
                    {
                        //for (int z = 0; z < ExchangeCenterList.Count; z++)
                        //{
                        //    if (z == 0)
                        //    {
                        //        Rid += ExchangeCenterList[z].ID.ToString();
                        //    }
                        //    else
                        //    {
                        //        Rid += ',' + ExchangeCenterList[z].ID.ToString();
                        //    }
                        //}
                        //sql = "  select SendProductCode as item_code,SendProductName as item_name,SendProductNum as qty from T_ExchangeDetail where ExchangeCenterId in (" + Rid + ")";
                        daodi += "拦截";
                    }
                    if (daodi == "")
                    {
                        daodi = "无";
                    }
                    model.IsDelete = 0;
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    model.GoodsReceiptDate = DateTime.Now;
                    if (curUser != null && curUser != "")
                    {
                        model.GoodsReceiptName = curUser;
                    }
                    else
                    {
                        model.GoodsReceiptName = Nickname;
                    }
                    model.Retreat_expressNumber = model.Retreat_expressNumber.Trim();
                    model.ModularName = daodi;
                    model.isSorting = 0;
                    //主表保存
                    db.T_ReturnToStorage.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {

                        //if (sql != "")
                        //{
                        //    List<Modular> queryData = db.Database.SqlQuery<Modular>(sql).ToList();
                        //    for (int z = 0; z < queryData.Count; z++)
                        //    {
                        //        T_ReturnToStorageDetails Dmodel = new T_ReturnToStorageDetails();
                        //        Dmodel.Pid = model.ID;
                        //        Dmodel.item_code = queryData[z].item_code;
                        //        Dmodel.item_name = queryData[z].item_name;
                        //        Dmodel.qty = queryData[z].qty;
                        //        db.T_ReturnToStorageDetails.Add(Dmodel);
                        //        db.SaveChanges();
                        //    }
                        //}

                        //wwt 2017 2 10 保存图片
                        List<T_ReTurnStorageImg> returnImg = App_Code.Com.Deserialize<T_ReTurnStorageImg>(jsonStr);
                        if (returnImg != null)
                        {
                            foreach (var item in returnImg)
                            {
                                item.Oid = model.ID;
                                db.T_ReTurnStorageImg.Add(item);
                            }
                            db.SaveChanges();
                        }

                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //得到用户的部门
        public string DepartmentQuer(string name)
        {
            T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name);

            return userModel.DepartmentId;
        }
        //生成报损Code
        public string CodeQuery()
        {
            //自动生成批号
            string code = "BS-DS-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_LossReport> listLoss = db.T_LossReport.Where(a => a.Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (listLoss.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = listLoss[0].Code.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            return code;
        }


        public decimal QueryUnitPrice(string Code)
        {
            T_goodsGY model = db.T_goodsGY.SingleOrDefault(a => a.code == Code);
            string price = model.cost_price.ToString();
            if (price == null || price == "")
            {
                price = "0";
            }

            return decimal.Parse(price);
        }

        //报损新增保存
        [HttpPost]
        [Description("报损新增保存")]
        public JsonResult LossReportAddSave(string ID)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string sql = "select * from T_ReceivedAfter where ID in (" + ID + ") ";
                    List<T_ReceivedAfter> AfterModel = db.Database.SqlQuery<T_ReceivedAfter>(sql).ToList();
                    if (AfterModel.Count <= 0)
                    {
                        return Json(new { State = "Faile", Message = "保存报损失败" }, JsonRequestBehavior.AllowGet);
                    }

                    T_LossReport model = new T_LossReport();


                    string DepartmentID = DepartmentQuer(name);

                    List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();

                    if (ZgModel.Count > 0)
                    {
                        model.ApproveFirst = ZgModel[0].DepartmentId;
                    }
                    else
                    {
                        return Json(new { State = "Faile", Message = "你没有主管，请联系管理员设置主管!" }, JsonRequestBehavior.AllowGet);

                    }

                    //主表保存
                    model.PostUser = name;
                    model.Department = DepartmentQuer(name);
                    model.Shop = AfterModel[0].ShopName;
                    model.Code = CodeQuery();
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.IsPzStatus = 0;
                    model.IsDelete = 0;
                    db.T_LossReport.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName = ZgModel[0].Name;
                        Approvemodel.Memo = "";
                        Approvemodel.Oid = model.ID;
                        db.T_LossReportApprove.Add(Approvemodel);
                        db.SaveChanges();
                        decimal price = 0;
                        for (int z = 0; z < AfterModel.Count; z++)
                        {
                            string order = AfterModel[z].OrderNumber;
                            string Pcode = AfterModel[z].ProductCode;
                            List<T_LossReportDetail> listDetail = db.Database.SqlQuery<T_LossReportDetail>("select * from T_LossReportDetail where  oid in (select ID from T_LossReport where IsDelete=0) and OderNumber='" + order + "' and  ProductCode='" + Pcode + "'").ToList();
                            if (listDetail.Count() > 0)
                                return Json(new { State = "Faile", Message = "保存失败订单号:" + AfterModel[z].OrderNumber + "与商品编码:" + AfterModel[z].ProductCode + "已报损" }, JsonRequestBehavior.AllowGet);

                            T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                            LossReportDetailModel.Oid = model.ID;

                            LossReportDetailModel.ProductCode = AfterModel[z].ProductCode;
                            LossReportDetailModel.ProductName = AfterModel[z].ProductName;
                            LossReportDetailModel.WangWang = AfterModel[z].CustomerCode;
                            LossReportDetailModel.OderNumber = AfterModel[z].OrderNumber;
                            LossReportDetailModel.Reason = "售后问题处理报损";
                            LossReportDetailModel.Unit = "无";
                            LossReportDetailModel.UnitPrice = QueryUnitPrice(AfterModel[z].ProductCode);
                            LossReportDetailModel.Qty = int.Parse(AfterModel[z].ProductNumber.ToString());
                            LossReportDetailModel.Amount = int.Parse(AfterModel[z].ProductNumber.ToString()) * decimal.Parse(QueryUnitPrice(AfterModel[z].ProductCode).ToString());
                            price += LossReportDetailModel.Amount;
                            db.T_LossReportDetail.Add(LossReportDetailModel);
                            db.SaveChanges();
                        }


                        //总价
                        T_LossReport modelLoss = db.T_LossReport.Find(model.ID);
                        modelLoss.Total = price;
                        db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                      //  ModularByZPBS();

                        for (int d = 0; d < AfterModel.Count; d++)
                        {
                            int ZgID = AfterModel[d].ID;
                            T_ReceivedAfter ReceivedAfterModel = db.T_ReceivedAfter.SingleOrDefault(a => a.ID == ZgID);
                            ReceivedAfterModel.IsHandle = 1;
                            db.Entry<T_ReceivedAfter>(ReceivedAfterModel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }



                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult QueryReceiptCount(int ID)
        {
            T_ReturnNoWithinRange model = db.T_ReturnNoWithinRange.SingleOrDefault(a => a.ID == ID);

            model.status = 1;

            db.Entry<T_ReturnNoWithinRange>(model).State = System.Data.Entity.EntityState.Modified;

            int s = db.SaveChanges();

            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult btnNOReceipt(int ID, string ToRemarks)
        {
            T_ReturnNoWithinRange model = db.T_ReturnNoWithinRange.SingleOrDefault(a => a.ID == ID);
            model.status = 2;
            model.ToRemarks = ToRemarks;
            db.Entry<T_ReturnNoWithinRange>(model).State = System.Data.Entity.EntityState.Modified;
            int s = db.SaveChanges();
            if (s > 0)
            {
                int Aid = int.Parse(model.AfterID.ToString());
                T_ReceivedAfter AfterModel = db.T_ReceivedAfter.SingleOrDefault(a => a.ID == Aid);
                AfterModel.IsHandle = 0;
                db.Entry<T_ReceivedAfter>(AfterModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult btnReceipt(int ID, string ToRemarks)
        {
            T_ReturnNoWithinRange model = db.T_ReturnNoWithinRange.SingleOrDefault(a => a.ID == ID);
            model.status = 1;

            model.ToRemarks = ToRemarks;
            db.Entry<T_ReturnNoWithinRange>(model).State = System.Data.Entity.EntityState.Modified;
            int s = db.SaveChanges();

            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        //不在退货范围内
        [HttpPost]
        [Description("不在退货范围内新增保存")]
        public JsonResult ViewReturnNoWithinRangeAddSave(T_ReturnNoWithinRange model, string ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {


                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                try
                {
                    int s = 0;
                    string[] AIDList = ID.Split(',');
                    for (int i = 0; i < AIDList.Length; i++)
                    {
                        int Aid = int.Parse(AIDList[i]);
                        T_ReceivedAfter After = db.T_ReceivedAfter.SingleOrDefault(a => a.ID == Aid);
                        After.IsHandle = 1;
                        db.Entry<T_ReceivedAfter>(After).State = System.Data.Entity.EntityState.Modified;
                        s = db.SaveChanges();
                        model.AfterID = Aid;
                        model.status = 0;
                        model.ShenQName = Nickname;
                        //主表保存
                        db.T_ReturnNoWithinRange.Add(model);
                        db.SaveChanges();
                    }

                    if (s > 0)
                    {
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public partial class ReturnToStoragegetExcel
        {
            public int ID { get; set; }
            public string Retreat_expressName { get; set; }
            public string Retreat_expressNumber { get; set; }
            public string Retreat_Warehouse { get; set; }
            public string CollectName { get; set; }
            public string ModularName { get; set; }
            public DateTime GoodsReceiptDate { get; set; }
            public DateTime SortingDate { get; set; }
            public string CollectAddress { get; set; }
            public string receivermobile { get; set; }
            public string Retreat_Remarks { get; set; }
            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }
            public int qualified { get; set; }
            public int Unqualified { get; set; }
            public string Remarks { get; set; }
            public string Retreat_dianpName { get; set; }
            public string Retreat_OrderNumber { get; set; }
            public string Simplename { get; set; }

            public string SortingName { get; set; }
             
        }
        /// <summary>
        ///退回件列表导出excel
        /// </summary>
        /// <param name="queryStr"></param>
        /// <param name="statedate"></param>
        /// <param name="EndDate"></param>
        /// <param name="RetreatWarehouseList"></param>
        /// <param name="RetreatexpressNameList"></param>
        /// <returns></returns>
        public FileResult getExcel(string queryStr, string statedate, string EndDate, string RetreatWarehouseList, string RetreatexpressNameList)
        {
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			DateTime logTime = DateTime.Now.AddSeconds(-15);
			string LogContent = "导出excel getExcel 条件 startDate:" + statedate + ",endDate:" + EndDate;
			var exilstLog= db.T_OperaterLog.FirstOrDefault(a => a.Module == "退回件导出" && a.Operater == Nickname && a.OperateContent.Contains(LogContent) &&a.OperateTime> logTime);
			if (exilstLog!=null)
			{
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				
				return File(ms, "text/plain", "刚刚没导出来吗.txt");
			}
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "退回件导出",
				OperateContent = string.Format("导出excel getExcel 条件 startDate:{0},endDate:{1},queryStr:{2},RetreatWarehouseList:{3},RetreatexpressNameList:{4}", statedate, EndDate, queryStr, RetreatWarehouseList, RetreatexpressNameList),
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();
			List<ReturnToStoragegetExcel> queryData = null;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
        
            //string sql = "select  isnull((select Name From  T_Express  where code = ISNULL(r.Retreat_expressName, '')),'') as Retreat_expressName,isnull(r.Retreat_expressNumber, '') as Retreat_expressNumber,isnull((select  name From  T_Warehouses where code = r.Retreat_Warehouse ),'') as Retreat_Warehouse ,isnull(r.Retreat_Remarks, '') as Retreat_Remarks,r.ModularName,r.GoodsReceiptDate,r.SortingDate, t.item_code,t.item_name,t.qualified,t.Unqualified,t.Remarks,isnull(t.qty, '') as qty ,t.Simplename as Simplename, " +
            //                "isnull((select  top 1 * From((select top 1 Retreat_dianpName From  T_Retreat where Retreat_expressNumber = r.Retreat_expressNumber and Retreat_dianpName is not null union  select top 1 StoreName From  T_ExchangeCenter where ReturnExpressCode = r.Retreat_expressNumber and StoreName is not null  union  select shop_name From  T_WDTshop where  shop_no = (select top 1 StoreCode From  T_Intercept where MailNo = r.Retreat_expressNumber and shop_name is not null)))as Retreat_OrderNumber order by Retreat_dianpName desc ),'') as Retreat_dianpName , " +
            //                "isnull((select  top 1 * From((select  top 1 Retreat_OrderNumber From  T_Retreat where Retreat_expressNumber = r.Retreat_expressNumber and Retreat_OrderNumber is not null " +
            //                "union  select top 1 OrderCode From  T_ExchangeCenter where ReturnExpressCode = r.Retreat_expressNumber and OrderCode is not null   union  select top 1 OrderNumber From  T_Intercept where MailNo = r.Retreat_expressNumber and OrderNumber is not null )) as Retreat_OrderNumber order by Retreat_OrderNumber desc),'') as Retreat_OrderNumber " +
            //                 ",SortingName From T_ReturnToStorage r  join T_ReturnToStoragelet t  on r.ID = t.Pid  where r.IsDelete = 0 and " +
            //            "r.SortingDate >= '" + sdate + "' and r.SortingDate <= '" + edate + "' and r.isSorting = 1 ";


			string sql2 = "with tab  as(select* from  T_ReturnToStorage r where r.IsDelete = 0 and r.SortingDate >='" + sdate + "' and r.SortingDate <=  '" + edate + "' and r.isSorting = 1";



				
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
				sql2 += " and r.Retreat_expressName ='"+ RetreatexpressNameList + "'";
                //queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList).ToList();

            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
				sql2 += " and r.Retreat_Warehouse = '"+ RetreatWarehouseList + "'";
                //queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList).ToList();
            }
			sql2 += "),tab2 as (select * From  T_ReturnToStoragelet where  Pid in (select  ID From tab)) select isnull((select Name From  T_Express  where code = ISNULL(tab.Retreat_expressName, '')),'') as Retreat_expressName,isnull(tab.Retreat_expressNumber, '') as Retreat_expressNumber,isnull((select  name From  T_Warehouses where code = tab.Retreat_Warehouse),'') as Retreat_Warehouse ,isnull(tab.Retreat_Remarks, '') as Retreat_Remarks,tab.ModularName,tab.GoodsReceiptDate,tab.SortingDate, tab2.item_code,tab2.item_name, tab2.qualified,tab2.Unqualified,tab2.Remarks,isnull(tab2.qty, '') as qty ,tab2.Simplename as Simplename,SortingName From  tab, tab2 where  tab.ID = tab2.Pid";
			queryData = db.Database.SqlQuery<ReturnToStoragegetExcel>(sql2).ToList();
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                row1.CreateCell(0).SetCellValue("快递名称");
                row1.CreateCell(1).SetCellValue("快递单号");
                row1.CreateCell(2).SetCellValue("仓库");
                row1.CreateCell(3).SetCellValue("收货备注");
                row1.CreateCell(4).SetCellValue("模块");
                row1.CreateCell(5).SetCellValue("收货时间");
                row1.CreateCell(6).SetCellValue("分拣时间");
                row1.CreateCell(7).SetCellValue("产品代码");
                row1.CreateCell(8).SetCellValue("产品名称");
                row1.CreateCell(9).SetCellValue("合格品数量");
                row1.CreateCell(10).SetCellValue("不合格品数量");
                row1.CreateCell(11).SetCellValue("产品备注");
                row1.CreateCell(12).SetCellValue("数量");
                //row1.CreateCell(13).SetCellValue("店铺");
                //row1.CreateCell(14).SetCellValue("订单编号");
                row1.CreateCell(15).SetCellValue("产品规格");
                 row1.CreateCell(16).SetCellValue("分拣员");
                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_expressName.ToString());
                    rowtemp.CreateCell(1).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                    rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_Warehouse.ToString());
                    rowtemp.CreateCell(3).SetCellValue(queryData[i].Retreat_Remarks.ToString());
                    rowtemp.CreateCell(4).SetCellValue(queryData[i].ModularName.ToString());
                    rowtemp.CreateCell(5).SetCellValue(queryData[i].GoodsReceiptDate.ToString());
                    rowtemp.CreateCell(6).SetCellValue(queryData[i].SortingDate.ToString()); 
                    rowtemp.CreateCell(7).SetCellValue(queryData[i].item_code.ToString());
                    rowtemp.CreateCell(8).SetCellValue(queryData[i].item_name.ToString());
                    rowtemp.CreateCell(9).SetCellValue(queryData[i].qualified.ToString());
                    rowtemp.CreateCell(10).SetCellValue(queryData[i].Unqualified.ToString());
                    rowtemp.CreateCell(11).SetCellValue(queryData[i].Remarks.ToString());
                    rowtemp.CreateCell(12).SetCellValue(queryData[i].qty.ToString());
                    //rowtemp.CreateCell(13).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                    //rowtemp.CreateCell(14).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                    rowtemp.CreateCell(15).SetCellValue(queryData[i].Simplename.ToString());
                     rowtemp.CreateCell(16).SetCellValue(queryData[i].SortingName.ToString());
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退回件数据.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退回件数据.xls");
            }
        }
        public partial class ReturnToStoragegetExcelList
        {
            public int ID { get; set; }
            public string Retreat_expressName { get; set; }
            public string Retreat_expressNumber { get; set; }
            public string Retreat_Warehouse { get; set; }
            public DateTime GoodsReceiptDate { get; set; }
            public string GoodsReceiptName { get; set; }
        }
        public FileResult getExcelList(string queryStr, string statedate, string EndDate, string RetreatWarehouseList, string RetreatexpressNameList)
        {
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "退回件导出",
				OperateContent = string.Format("导出excel getExcelList 条件 startDate:{0},endDate:{1},queryStr:{2},RetreatWarehouseList:{3},RetreatexpressNameList:{4}", statedate, EndDate, queryStr, RetreatWarehouseList, RetreatexpressNameList),
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();

			List<ReturnToStoragegetExcelList> queryData = null;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
              
            string sql = "select (select Name From  T_Express where code = ISNULL(r.Retreat_expressName, '')) as Retreat_expressName ,Retreat_expressNumber ,(select  name From  T_Warehouses where code = r.Retreat_Warehouse ) as Retreat_Warehouse  ,GoodsReceiptDate ,GoodsReceiptName From  T_ReturnToStorage r WHERE IsDelete =0 AND  isSorting =0 and  GoodsReceiptDate>='" + sdate + "' and  GoodsReceiptDate<='" + edate + "' ";
            queryData = db.Database.SqlQuery<ReturnToStoragegetExcelList>(sql).ToList();
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                T_Express express = db.T_Express.SingleOrDefault(s => s.Code == RetreatexpressNameList);

                queryData = queryData.Where(a => a.Retreat_expressName == express.Name).ToList();
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                T_Warehouses house = db.T_Warehouses.SingleOrDefault(s => s.code == RetreatWarehouseList);
                queryData = queryData.Where(a => a.Retreat_Warehouse == house.name).ToList();
            }
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                row1.CreateCell(0).SetCellValue("快递名称");
                row1.CreateCell(1).SetCellValue("快递单号");
                row1.CreateCell(2).SetCellValue("仓库");
                row1.CreateCell(3).SetCellValue("收货时间");
                row1.CreateCell(4).SetCellValue("收货人");
                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_expressName.ToString());
                    rowtemp.CreateCell(1).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                    rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_Warehouse.ToString());
                    rowtemp.CreateCell(3).SetCellValue(queryData[i].GoodsReceiptDate.ToString());
                    rowtemp.CreateCell(4).SetCellValue(queryData[i].GoodsReceiptName.ToString());
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退回件未分拣数据.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退回件数据.xls");
            }
        }      
        //售后处理
        [HttpPost]
        [Description("售后处理新增保存")]
        public JsonResult ViewReturnNotReceivedAddSave(T_ReturnToStorage model, string ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {


                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                try
                {
                    string expressNumber = model.Retreat_expressNumber;
                    List<T_ReturnToStorage> RmodelList = db.T_ReturnToStorage.Where(a => a.Retreat_expressNumber == expressNumber && a.IsDelete == 0).ToList();
                    if (RmodelList.Count > 0)
                    {
                        return Json(new { State = "Faile", Message = "该快递已录入过退回件" }, JsonRequestBehavior.AllowGet);
                    }


                    string daodi = "无";

                    string sql = "select * from T_ReceivedAfter where ID in (" + ID + ")";
                    List<T_ReceivedAfter> RetreatList = db.Database.SqlQuery<T_ReceivedAfter>(sql).ToList();
                    if (RetreatList.Count <= 0)
                    {
                        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                    }

                    model.yuan_expressNumber = RetreatList[0].CollectExpressNumber;
                    model.ModularName = daodi;
                    model.isSorting = 0;
                    model.IsDelete = 0;
                    //主表保存
                    db.T_ReturnToStorage.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        for (int x = 0; x < RetreatList.Count; x++)
                        {
                            T_ReturnToStorageDetails Dmodel = new T_ReturnToStorageDetails();
                            Dmodel.Pid = model.ID;
                            Dmodel.item_code = RetreatList[x].ProductCode;
                            Dmodel.item_name = RetreatList[x].ProductName;
                            Dmodel.qty = RetreatList[x].ProductNumber;
                            db.T_ReturnToStorageDetails.Add(Dmodel);
                            db.SaveChanges();


                            RetreatList[x].IsHandle = 1;
                            db.Entry<T_ReceivedAfter>(RetreatList[x]).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }




                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public partial class RetreatSorting
        {
            public string item_code { get; set; }
            public string item_name { get; set; }
            public string Simplename { get; set; }
            public int qty { get; set; }
            public int qualified { get; set; }
            public int Unqualified { get; set; }
            public int Notreceived { get; set; }
            public string Remarks { get; set; }
            public string UnitName { get; set; }

        }
        public partial class RetreatBi
        {
            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }
            public string type { get; set; }

        }
        public partial class ReturnToStoragelet
        { 
            public int id { get; set; }
            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }

        }
        [HttpPost]
        [Description("委外单推送")]
        public JsonResult ViewSortingOutsourcing(string RetreatWarehouseList,string queryStr, string RetreatexpressNameList, string Fjstatethisdate, string FjEndthisdate)
        {
			if (RetreatWarehouseList == "3") //厂家代发仓需推其他入库单
			{
				return ViewSortingOutsourcings(queryStr, Fjstatethisdate, FjEndthisdate, "3");
			}
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

			IQueryable<T_ReturnToStorage> queryData = null;
            queryData = db.T_ReturnToStorage.Where(a => a.IsDelete == 0 && a.ExternalSingle == "").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {


                queryData = queryData.Where(a => a.CollectName != null && a.CollectName.Contains(queryStr) || a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr) || a.receivermobile != null && a.receivermobile.Contains(queryStr) || a.SortingName != null && a.SortingName.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList);
            }
            if (!string.IsNullOrWhiteSpace(Fjstatethisdate))
            {
                DateTime start = DateTime.Parse(Fjstatethisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(FjEndthisdate))
            {
                DateTime end = DateTime.Parse(FjEndthisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate <= end);
            }
            List<T_ReturnToStorage> ReturnToStorageList = queryData.ToList();
            string shenheID = "";
            for (int i = 0; i < ReturnToStorageList.Count; i++)
            {
                if (i == 0 )
                {
                    shenheID += "'" + ReturnToStorageList[i].ID + "'";
                }
                else
                {
                    shenheID += "," + "'" + ReturnToStorageList[i].ID + "'";
                }
            }

            List<ReturnToStoragelet> ReturnToStorageDetailsList = db.Database.SqlQuery<ReturnToStoragelet>("with tab as( select item_code,sum(qualified+Unqualified) as qty from T_ReturnToStoragelet where Pid in (" + shenheID + ") group by item_code )select * From   tab  where qty != 0 ").ToList();


            string cpcode = "[";
            for (int x = 0; x < ReturnToStorageDetailsList.Count; x++)
            {
                //    int qtyGY =int.Parse(ReturnToStorageDetailsList[x].qualified.ToString()) + int.Parse(ReturnToStorageDetailsList[x].Unqualified.ToString());
                int qtyGY = int.Parse(ReturnToStorageDetailsList[x].qty.ToString());
                if (x == ReturnToStorageDetailsList.Count - 1)
                {
                    cpcode += "{\"spec_no\": \"" + ReturnToStorageDetailsList[x].item_code + "\",\"price\": \"0\",\"num\": \"" + qtyGY + "\"}";
                    // cpcode += "{\"item_code\":\"" + details[z].item_code + "\",\"qty\":" + qtyGY + "}";
                }
                else
                {

                    cpcode += "{\"spec_no\": \"" + ReturnToStorageDetailsList[x].item_code + "\",\"price\": \"0\",\"num\": \"" + qtyGY + "\"},";

                }
            }
            cpcode += "]";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string WarehouseCode = ReturnToStorageList[0].Retreat_Warehouse;
            string order_type = "2";
            string api_outer_no = ReturnToStorageList[0].Retreat_expressNumber;
            EBMS.App_Code.GY gy = new App_Code.GY();
            dic.Add("warehouse_no", WarehouseCode);
            dic.Add("order_type", order_type);
            dic.Add("api_outer_no", "EBMS"+api_outer_no);
            dic.Add("goods_list", cpcode);
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());
            var aa = CreateParam(dic, true);
			
			T_OperaterLog pushStartRecord = new T_OperaterLog()
			{
				Module = "委外单推送记录",
				OperateContent = "{" +
					 "\"开始推送\":\""+ DateTime.Now.ToString() +"\"," +
					 "\"仓库\":\"" + RetreatWarehouseList + "\" ," +
					 "\"快递\":\"" + RetreatexpressNameList + "\" ," +
					 "\"开始时间\":\"" + Fjstatethisdate + "\" ," +
					 "\"结束时间\":\"" + FjEndthisdate + " \"," +
					 "\"IDS\":\"" + shenheID +"\""+
					 "}",
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
			};
			try
			{
				db.T_OperaterLog.Add(pushStartRecord);
				db.SaveChanges();
			}
			catch (Exception ex)
			{
				return Json(new { State = "Faile", Message = "提交失败！" }, JsonRequestBehavior.AllowGet);
			}
			string ret = Post("http://api.wangdian.cn/openapi2/wms_stockinout_order_push.php", aa);
			T_OperaterLog pushRecord = new T_OperaterLog()
			{
				Module = "委外单推送记录",
				OperateContent = "{" +
								 "\"返回内容\":\"" + ret + "\" ," +
								 "\"仓库\":\"" + RetreatWarehouseList + "\" ," +
								 "\"快递\":\"" + RetreatexpressNameList + " \"," +
								 "\"开始时间\":\"" + Fjstatethisdate + " \"," +
								 "\"结束时间\":\"" + FjEndthisdate + "\" ," +
								 "\"IDS\":\"" + shenheID + "\""+
								 "}",
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = pushStartRecord.ID
			};
			db.T_OperaterLog.Add(pushRecord);
			db.SaveChanges();
			JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string code = jsonData["code"].ToString();
			
			
			if (code == "0")
            {
                string stockout = jsonData["data"][0].ToString();

				
				//for (int i = 0; i < ReturnToStorageList.Count; i++)
    //            {
					string updateSql = "update T_ReturnToStorage set ExternalSingle=@ExternalSingle,ExternalSingleTime=@ExternalSingleTime where ID in (" + shenheID + ")";
					SqlParameter[] sqlPara = new SqlParameter[] {
						new SqlParameter ("@ExternalSingle",stockout),
						new SqlParameter ("@ExternalSingleTime",DateTime.Now)
					};
					db.Database.ExecuteSqlCommand(updateSql, sqlPara);
                    //int QueryID = ReturnToStorageList[i].ID;
                    //T_ReturnToStorage ToStorageModles = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == QueryID);
                    //ToStorageModles.ExternalSingle = stockout;
                    //ToStorageModles.ExternalSingleTime = DateTime.Now;
                    //db.Entry<T_ReturnToStorage>(ToStorageModles).State = System.Data.Entity.EntityState.Modified;

					T_OperaterLog operater = new T_OperaterLog()
					{
						Module = "退回件",
						OperateContent = "推送委外单:" + stockout+ shenheID,
						Operater = Nickname,
						OperateTime = DateTime.Now,
						PID = -1
					};
					db.T_OperaterLog.Add(operater);
                    db.SaveChanges();

              //  }

                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else if (code == "1644")
            {
				string message = jsonData["message"].ToString();

				return Json(new { State = "Faile", Message = "提交失败"+ message }, JsonRequestBehavior.AllowGet);
            }
            else if (code == "110")
            {
                return Json(new { State = "Faile", Message = "数量有为0的产品代码，请处理" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
				string message = jsonData["message"].ToString();

				return Json(new { State = "Faile", Message = "提交失败！"+ message }, JsonRequestBehavior.AllowGet);
            }

         
        }

        [HttpPost]
        [Description("京东外仓其他入库单推送")]
        public JsonResult ViewSortingOutsourcings( string queryStr, string Fjstatethisdate, string FjEndthisdate,string Retreat_Warehouse="2")
        {
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

			IQueryable<T_ReturnToStorage> queryData = null;
            queryData = db.T_ReturnToStorage.Where(a => a.IsDelete == 0 && a.ExternalSingle == ""&&a.Retreat_Warehouse== Retreat_Warehouse).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {


                queryData = queryData.Where(a => a.CollectName != null && a.CollectName.Contains(queryStr) || a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr) || a.receivermobile != null && a.receivermobile.Contains(queryStr) || a.SortingName != null && a.SortingName.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(Fjstatethisdate))
            {
                DateTime start = DateTime.Parse(Fjstatethisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate >= start);
                //
            }
            if (!string.IsNullOrWhiteSpace(FjEndthisdate))
            {
                DateTime end = DateTime.Parse(FjEndthisdate);
                queryData = queryData.Where(a => a.SortingDate != null && a.SortingDate <= end);
            }
            List<T_ReturnToStorage> ReturnToStorageList = queryData.ToList();
            string shenheID = "";
            for (int i = 0; i < ReturnToStorageList.Count; i++)
            {
                if (i == 0)
                {
                    shenheID += "'" + ReturnToStorageList[i].ID + "'";
                }
                else
                {
                    shenheID += "," + "'" + ReturnToStorageList[i].ID + "'";
                }
            }

            List<ReturnToStoragelet> ReturnToStorageDetailsList = db.Database.SqlQuery<ReturnToStoragelet>("with tab as( select item_code,item_name,sum(qualified+Unqualified) as qty from T_ReturnToStoragelet where Pid in (" + shenheID + ") group by item_code,item_name )select * From   tab  where qty != 0 ").ToList();
           

            string cpcode = "[";
            for (int x = 0; x < ReturnToStorageDetailsList.Count; x++)
            {
                int qtyGY = int.Parse(ReturnToStorageDetailsList[x].qty.ToString());
                if (x == ReturnToStorageDetailsList.Count - 1)
                {
                    cpcode += "{\"spec_no\": \"" + ReturnToStorageDetailsList[x].item_code + "\",\"src_price\": \"0\",\"stockin_num\": \"" + qtyGY + "\"}";
                }
                else
                {
                    cpcode += "{\"spec_no\": \"" + ReturnToStorageDetailsList[x].item_code + "\",\"src_price\": \"0\",\"stockin_num\": \"" + qtyGY + "\"},";
                }
            }
            cpcode += "]";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string WarehouseCode = ReturnToStorageList[0].Retreat_Warehouse;
            string order_type = "2";
            string api_outer_no = ReturnToStorageList[0].Retreat_expressNumber;
            EBMS.App_Code.GY gy = new App_Code.GY();
            string cmd = "";
            cmd = "{" +
                "\"outer_no\":\"" + "EBMS" + api_outer_no + "\"," +//入库单号
                "\"warehouse_no\":\"" + WarehouseCode + "\"," +//入库仓库
                "\"num\":\"1\"," +//出库数量 
                  "\"is_check\":\"1\"," +//1：审核 0：不审核 默认1（前提是详情数据不存在出库数量num=0的出库单）
                    "\"goods_list\":" + cpcode + "" +
                "}";
            dic.Add("stockin_info", cmd);
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());

            
            var aa = CreateParam(dic, true);
			T_OperaterLog pushStartRecord = new T_OperaterLog()
			{
				Module = "委外单推送记录",
				OperateContent = "{" +
					 "\"开始推送\":\"" + DateTime.Now.ToString() + "\"," +
					 "\"仓库\":\"" + Retreat_Warehouse + "\" ," +
					
					 "\"开始时间\":\"" + Fjstatethisdate + "\" ," +
					 "\"结束时间\":\"" + FjEndthisdate + " \"," +
					 "\"IDS\":\"" + shenheID + "\"" +
					 "}",
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
			};
			try
			{
				db.T_OperaterLog.Add(pushStartRecord);
				db.SaveChanges();
			}
			catch (Exception ex)
			{
				return Json(new { State = "Faile", Message = "提交失败！" }, JsonRequestBehavior.AllowGet);
			}
			string ret = Post("http://api.wangdian.cn/openapi2/stockin_order_push.php", aa);
			T_OperaterLog pushRecord = new T_OperaterLog()
			{
				Module = "委外单推送记录",
				OperateContent = "{" +
								 "\"返回内容\":\"" + ret + "\" ," +
								 "\"仓库\":\"" + Retreat_Warehouse + "\" ," +
								
								 "\"开始时间\":\"" + Fjstatethisdate + " \"," +
								 "\"结束时间\":\"" + FjEndthisdate + "\" ," +
								 "\"IDS\":\"" + shenheID + "\"" +
								 "}",
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = pushStartRecord.ID
			};
			JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string code = jsonData["code"].ToString();
            if (code == "0")
            {
                string stockout = jsonData["stockin_no"].ToString();
                for (int i = 0; i < ReturnToStorageList.Count; i++)
                {
                    int QueryID = ReturnToStorageList[i].ID;
                    T_ReturnToStorage ToStorageModles = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == QueryID);
                    ToStorageModles.ExternalSingle = stockout;
                    ToStorageModles.ExternalSingleTime = DateTime.Now;
                    db.Entry<T_ReturnToStorage>(ToStorageModles).State = System.Data.Entity.EntityState.Modified;

					T_OperaterLog operater = new T_OperaterLog()
					{
						Module = "退回件",
						OperateContent = "推送委外单:" + stockout,
						Operater = Nickname,
						OperateTime = DateTime.Now,
						PID = QueryID
					};
					db.T_OperaterLog.Add(operater);
					db.SaveChanges();
                }
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else if (code == "1644")
            {
                return Json(new { State = "Faile", Message = "提交失败,该记录已存在旺店通！" }, JsonRequestBehavior.AllowGet);
            }
            else if (code == "110")
            {
                return Json(new { State = "Faile", Message = "数量有为0的产品代码，请处理" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "提交失败！" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Description("退货退款分拣保存")]
        public JsonResult ViewSortingAddSave(T_ReturnToStorage model, string jsonStr)
        {
           
            string ExprNum = "";
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                   
                    int QueryID = model.ID;
                    T_ReturnToStorage ToStorageModle = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == QueryID);
                    if(ToStorageModle.isSorting==1)
                    {
                        return Json(new { State = "Faile", Message = "该数据已分拣,请勿重复分拣" }, JsonRequestBehavior.AllowGet);
                    }
                    //T_ReturnToStorage ToStorageModles = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == QueryID);
                    ToStorageModle.ExternalSingle = "";
                    //db.Entry<T_ReturnToStorage>(ToStorageModle).State = System.Data.Entity.EntityState.Modified;
                    ToStorageModle.CollectAddress = model.CollectAddress;
                    ToStorageModle.CollectName = model.CollectName;
                    ToStorageModle.receivermobile = model.receivermobile;
                    ToStorageModle.Retreat_Remarks = model.Retreat_Remarks;
                    ToStorageModle.SortingName = model.SortingName;
                    ToStorageModle.yuan_expressNumber = model.yuan_expressNumber;
                    ToStorageModle.SortingDate = DateTime.Now;
                    db.Entry<T_ReturnToStorage>(ToStorageModle).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                    List<RetreatSorting> details = App_Code.Com.Deserialize<RetreatSorting>(jsonStr);
                    string cpcode = "[";
                    //主表保存
                    int editID = model.ID;//原记录的ID
                    T_ReturnToStorage PurMod = db.T_ReturnToStorage.Find(editID);
                    PurMod.isSorting = 1;
                    PurMod.SortingName = Nickname;
                    db.Entry<T_ReturnToStorage>(PurMod).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)  
                    {
                        List<T_ReturnToStoragelet> LetList = new List<T_ReturnToStoragelet>();
                        string CkCode = PurMod.Retreat_Warehouse;
                        for (int z = 0; z < details.Count; z++)
                        {
                            T_ReturnToStoragelet LetModel = new T_ReturnToStoragelet();
                            LetModel.item_code = details[z].item_code;
                            LetModel.item_name = details[z].item_name;
                            LetModel.Simplename = details[z].Simplename;
                            LetModel.qty = details[z].qty;
                            LetModel.qualified = details[z].qualified;
                            LetModel.Unqualified = details[z].Unqualified;
                            LetModel.Notreceived = 0;
                            LetModel.Pid = model.ID;
                            LetModel.Remarks = details[z].Remarks;
                            LetModel.UnitName = details[z].UnitName;
                            db.T_ReturnToStoragelet.Add(LetModel);
                            db.SaveChanges();

                            string CpCode = details[z].item_code;
                            T_RetreatSorting SortingModel = db.T_RetreatSorting.SingleOrDefault(a => a.ProductCode == CpCode && a.WarehouseCode == CkCode);
                            if (SortingModel != null)
                            {
                                if (details[z].qualified != 0 || details[z].Unqualified != 0)
                                {

                                    SortingModel.QualifiedNumber = SortingModel.QualifiedNumber + details[z].qualified;
                                    SortingModel.UnqualifiedNumber = SortingModel.UnqualifiedNumber + details[z].Unqualified;
                                    SortingModel.Simplename = details[z].Simplename;
                                    SortingModel.UnitName = details[z].UnitName;
                                    db.Entry<T_RetreatSorting>(SortingModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    T_ReturnToStorageDetails DetailsModel = new T_ReturnToStorageDetails();
                                    DetailsModel.item_code = details[z].item_code;
                                    DetailsModel.item_name = details[z].item_name;
                                    DetailsModel.qty = details[z].qualified + details[z].Unqualified;
                                    DetailsModel.Pid = model.ID;
                                    db.T_ReturnToStorageDetails.Add(DetailsModel);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                if (details[z].qualified != 0 || details[z].Unqualified != 0)
                                {
                                    T_RetreatSorting Smodle = new T_RetreatSorting();
                                    Smodle.ProductCode = details[z].item_code;
                                    Smodle.ProductName = details[z].item_name;
                                    Smodle.QualifiedNumber = details[z].qualified;
                                    Smodle.UnqualifiedNumber = details[z].Unqualified;
                                    Smodle.Simplename = details[z].Simplename;
                                    Smodle.WarehouseCode = PurMod.Retreat_Warehouse;
                                    Smodle.UnitName = details[z].UnitName;
                                    db.T_RetreatSorting.Add(Smodle);
                                    db.SaveChanges();

                                    T_ReturnToStorageDetails DetailsModel = new T_ReturnToStorageDetails();
                                    DetailsModel.item_code = details[z].item_code;
                                    DetailsModel.item_name = details[z].item_name;
                                    DetailsModel.qty = details[z].qualified + details[z].Unqualified;
                                    DetailsModel.Pid = model.ID;
                                    db.T_ReturnToStorageDetails.Add(DetailsModel);
                                    db.SaveChanges();
                                }
                            }



                            // T_ReturnToStorageDetails DetailsMode = db.T_ReturnToStorageDetails.SingleOrDefault(a => a.Pid == editID && a.item_code == CpCode);
                            int qty = details[z].qty-(details[z].qualified + details[z].Unqualified + details[z].Notreceived);
                            ExprNum = PurMod.Retreat_expressNumber;
                            string modekuai = PurMod.ModularName;


                            #region 退货退款换货
                            if (modekuai == "退货退款" || modekuai == "退货退款换货")
                            {
                                //得到当前退款记录
                                List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == ExprNum).ToList();

                                T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == ExprNum&&a.IsDelete==0);

                                for (int j = 0; j < RetreatList.Count; j++)
                                {




                                    int RetreatID = RetreatList[j].ID;


                                    //得到退款id
                                    // int RetreatID = RetreatList[0].ID;
                                    //看该退款记录走到那里如果是仓库就审核并且下一步
                                    T_RetreatAppRove AppRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == RetreatID && a.ApproveTime == null);
                                    if (AppRoveModel != null && AppRoveModel.ApproveName == "仓库")
                                    {
                                        AppRoveModel.ApproveTime = DateTime.Now;
                                        AppRoveModel.Status = 1;
                                        db.Entry<T_RetreatAppRove>(AppRoveModel).State = System.Data.Entity.EntityState.Modified;
                                        int w = db.SaveChanges();

                                        if (w > 0)
                                        {
                                            T_Retreat Rmodel = db.T_Retreat.Find(RetreatID);
                                            T_RetreatAppRove newApprove = new T_RetreatAppRove();

                                            string type = AppRoveModel.Type;
                                            int step = int.Parse(Rmodel.Step.ToString());
                                            step++;
                                            IQueryable<T_RetreatConfig> config = db.T_RetreatConfig.Where(a => a.Reason == type);
                                            int stepLength = config.Count();//总共步骤
                                            if (step < stepLength)
                                            {
                                                //不是最后一步，主表状态为0 =>审核中
                                                Rmodel.Status = 0;
                                                T_RetreatConfig stepMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                                string nextName = stepMod.Name;
                                                //下一步审核人不是null  审核记录插入一条新纪录
                                                newApprove.Memo = "";
                                                newApprove.Oid = RetreatID;
                                                newApprove.Status = -1;
                                                newApprove.Type = type;
                                                newApprove.ApproveDName = stepMod.Type;
                                                if (nextName != null && nextName != "")
                                                {
                                                    newApprove.ApproveName = nextName;
                                                }
                                                else
                                                {
                                                    newApprove.ApproveName = stepMod.Type;
                                                }
                                                string shenheName = newApprove.ApproveDName;
                                                if (shenheName == "财务")
                                                {
                                                    T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == RetreatID && a.ApproveDName == "财务" && a.Status == 1);

                                                    if (appRoveModel != null)
                                                    {
                                                        // int stepAppRove = step + 1;
                                                        step++;
                                                        T_RetreatConfig stepAppRoveMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                                        if (stepAppRoveMod.Name != null && stepAppRoveMod.Name != "")
                                                        {
                                                            newApprove.ApproveName = stepAppRoveMod.Name;
                                                        }
                                                        else
                                                        {
                                                            newApprove.ApproveName = stepAppRoveMod.Type;
                                                        }
                                                    }
                                                }
                                                db.T_RetreatAppRove.Add(newApprove);
                                                db.SaveChanges();

                                            }
                                            else
                                            {
                                                string OrderNumber = Rmodel.Retreat_OrderNumber;

                                                //最后一步，主表状态改为 1 => 同意
                                                Rmodel.Status = 1;
                                            }
                                            Rmodel.Step = step;
                                            db.Entry<T_Retreat>(Rmodel).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }

                                       // ModularByZPHH();
                                        //ModularByZPTH();
                                    }
                                }
                            }
                            #endregion
                            #region 换货
                            else if (modekuai == "换货" || modekuai == "退货退款换货")
                            {
                                //得到当前换货记录
                                List<T_ExchangeCenter> RetreatLists = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == ExprNum).ToList();
                                foreach (var item in RetreatLists)
                                {
                                    int RetreatID = item.ID;
                                    //看该退款记录走到那里如果是仓库就审核并且下一步
                                    T_ExchangeCenterApprove AppRoveModel = db.T_ExchangeCenterApprove.SingleOrDefault(a => a.Pid == RetreatID && a.ApproveTime == null);
                                    if (AppRoveModel != null)
                                    {
                                        string approveruser = AppRoveModel.ApproveName;
                                        T_ExchangeGroup group = db.T_ExchangeGroup.SingleOrDefault(a => a.GroupName.Contains(approveruser));

                                        if (AppRoveModel != null && group != null)
                                        {
                                            if (group.GroupName == "仓库")
                                            {
                                                T_ExchangeCenter ExchangeCentermodel = db.T_ExchangeCenter.Find(RetreatID);
                                                try
                                                {
                                                    T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.Find(AppRoveModel.ID);
                                                    T_ExchangeCenter models = db.T_ExchangeCenter.Find(approve.Pid);
                                                    List<T_ExchangeCenter> exchange = db.T_ExchangeCenter.Where(s => s.OrderCode.Equals(models.OrderCode) && s.IsDelete == 0).ToList();
                                                    // T_OrderList order = db.T_OrderList.SingleOrDefault(s => s.platform_code.Equals(models.OrderCode));
                                                    List<T_ExchangeDetail> exchangedetail = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == models.ID).ToList();
                                                    if (string.IsNullOrWhiteSpace(jsonStr))
                                                        return Json(new { State = "Faile", Message = "详情不能为空" });
                                                    else
                                                    {
                                                        models.WarhouseStatus = 1;
                                                        models.Status = 1;
                                                        approve.ApproveName = "仓库";
                                                        approve.ApproveStatus = 1;
                                                        approve.ApproveTime = DateTime.Now;
                                                        //判断是否第一次仓库收货，如果是则修改订单状态为已收货
                                                        //if (exchange.Count() == 1)
                                                        //{
                                                        //    order.ExchangeStatus = 2;
                                                        //}
                                                        //修改订单详情状态和换货数量
                                                        //foreach (var item in exchangedetail)
                                                        //{
                                                        //    T_OrderDetail detail = db.T_OrderDetail.FirstOrDefault(s => s.oid.Equals(order.code) && s.item_code.Equals(item.SendProductCode));
                                                        //    if (detail != null)//如果没有详情需要加订单详情
                                                        //    {
                                                        //        detail.ExchangeStatus = 1;
                                                        //        detail.ExchangeQty += item.SendProductNum;
                                                        //        db.SaveChanges();
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        T_OrderDetail t = new T_OrderDetail
                                                        //        {
                                                        //            oid = order.code,
                                                        //            refund = 0,
                                                        //            item_code = item.NeedProductCode,
                                                        //            item_name = item.NeedProductName,
                                                        //            item_simple_name = "",
                                                        //            sku_code = "",
                                                        //            sku_name = "",
                                                        //            qty = 0,
                                                        //            price = 0,
                                                        //            amount = 0,
                                                        //            discount_fee = 0,
                                                        //            amount_after = 0,
                                                        //            post_fee = 0,
                                                        //            platform_item_name = "",
                                                        //            platform_sku_name = "",
                                                        //            note = "",
                                                        //            ExchangeStatus = 0,
                                                        //            ExchangeQty = 0,
                                                        //            ReissueStatus = 0,
                                                        //            ReissueQty = 0,
                                                        //            RetreatQty = 0,
                                                        //            RetreatStatus = 0
                                                        //        };
                                                        //        db.T_OrderDetail.Add(t);
                                                        //    }
                                                        //}
                                                        db.SaveChanges();
                                                    }
                                                   // ModularByZPHH();
                                                    //ModularByZPTH();
                                                    sc.Complete();
                                                   // return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                                                }
                                                catch (Exception ex)
                                                {

                                                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                                                }

                                            }

                                        }


                                    }
                                }
                            }
                            #endregion
                            #region 拦截
                            else if (modekuai == "拦截")
                            {
                                //得到当前拦截记录
                                List<T_Intercept> RetreatLists = db.T_Intercept.Where(a => a.MailNo == ExprNum).ToList();
                                foreach (var item in RetreatLists)
                                {
                                    int RetreatID = item.ID;

                                    //看该退款记录走到那里如果是仓库就审核并且下一步
                                    T_InterceptApprove AppRoveModel = db.T_InterceptApprove.SingleOrDefault(a => a.Pid == RetreatID && a.ApproveTime == null&&a.ApproveName== "仓库");
                                    if (AppRoveModel != null)
                                    {
                                        string approveruser = AppRoveModel.ApproveName;
                                        if (approveruser == "仓库")
                                        {
                                            try
                                            {
                                                T_InterceptApprove approve = db.T_InterceptApprove.Find(AppRoveModel.ID);
                                                T_Intercept models = db.T_Intercept.Find(approve.Pid);
                                                List<T_Intercept> exchange = db.T_Intercept.Where(s => s.OrderNumber.Equals(models.OrderNumber) && s.IsDelete == 0).ToList();
                                                List<T_InterceptDetail> exchangedetail = db.T_InterceptDetail.Where(s => s.InterceptId == models.ID).ToList();
                                                if (string.IsNullOrWhiteSpace(jsonStr))
                                                    return Json(new { State = "Faile", Message = "详情不能为空" });
                                                else
                                                {
                                                    models.Status = 1;
                                                    approve.ApproveUser = "仓库";
                                                    approve.ApproveStatus = 1;
                                                    approve.ApproveTime = DateTime.Now;
                                                    db.SaveChanges();
                                                }
                                                //string RetreatAppRoveSql = "   select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenterApprove where  Pid in ( select ID from T_ExchangeCenter where IsDelete=0 ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser";
                                                //List<ModularQuery> RetreatAppRoveQuery = db.Database.SqlQuery<ModularQuery>(RetreatAppRoveSql).ToList();
                                                //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                                                //{
                                                //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                                                //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货未审核" && a.PendingAuditName == PendingAuditName);
                                                //    if (NotauditedModel != null)
                                                //    {
                                                //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                                                //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                                                //    }
                                                //    else
                                                //    {
                                                //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                //        ModularNotauditedModel.ModularName = "换货未审核";
                                                //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                                                //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                                                //        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                                //    }
                                                //    db.SaveChanges();
                                                //}
                                                sc.Complete();
                                                //return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (Exception ex)
                                            {

                                                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                                            }

                                        }


                                    }
                                }
                            }

                            #endregion
                            #region 不管
                            if ( qty != 0&& modekuai!="无")
                            {
                                T_ReceivedAfter AfterModel = new T_ReceivedAfter();

                          



								if (modekuai == "退货退款" || modekuai == "退货退款换货")
								{
									//得到当前退款记录
									List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == ExprNum).ToList();



									AfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
									AfterModel.ShopName = RetreatList[0].Retreat_dianpName;
									AfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
									AfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
								}
								else if (modekuai == "换货")
								{
									List<T_ExchangeCenter> ExchangeCenterList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == ExprNum).ToList();
									AfterModel.OrderNumber = ExchangeCenterList[0].OrderCode;
									AfterModel.ShopName = ExchangeCenterList[0].StoreName;
									AfterModel.ShopCode = ExchangeCenterList[0].StoreCode;
									AfterModel.CustomerName = ExchangeCenterList[0].VipName;
									AfterModel.CustomerCode = ExchangeCenterList[0].VipCode;

								}
								else if (modekuai == "拦截")
								{
									List<T_Intercept> interceptList = db.T_Intercept.Where(a => a.MailNo == ExprNum).ToList();
									AfterModel.OrderNumber = interceptList[0].OrderNumber;
									AfterModel.ShopName = Com.GetShopName(interceptList[0].StoreCode);
									AfterModel.ShopCode = interceptList[0].StoreCode;
									AfterModel.CustomerName = interceptList[0].VipName;
									AfterModel.CustomerCode = interceptList[0].VipCode;
								}
								else
								{
									string yuanExnum = PurMod.yuan_expressNumber;
									List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.CollectExpressNumber == yuanExnum).ToList();
									if (ReceivedAfterList.Count > 0)
									{


                                        AfterModel.OrderNumber = ReceivedAfterList[0].OrderNumber;
                                        AfterModel.ShopName = ReceivedAfterList[0].ShopName;
                                        AfterModel.ShopCode = ReceivedAfterList[0].ShopCode;
                                        AfterModel.CustomerName = ReceivedAfterList[0].CustomerName;
                                        AfterModel.CustomerCode = ReceivedAfterList[0].CustomerCode;
                                    }
                                }
                                AfterModel.IsHandle = 0;
                                AfterModel.Type = PurMod.ModularName;
                                AfterModel.ProductCode = details[z].item_code;
                                AfterModel.ProductName = details[z].item_name;
                                AfterModel.CollectExpressName = PurMod.Retreat_expressName;
                                AfterModel.CollectExpressNumber = PurMod.Retreat_expressNumber;
                                AfterModel.CreatTime = DateTime.Now;
								AfterModel.ProductNumber = qty;
								AfterModel.ShouldReceiveQty = details[z].qty;
								AfterModel.ActualReceiveQty = details[z].Unqualified + details[z].qualified;
								db.T_ReceivedAfter.Add(AfterModel);
                                db.SaveChanges();
                            }
                            #endregion
                            #region 上传旺店通
                     
                        }
                       
                       
                        T_OperaterLog log = new T_OperaterLog()


                        {
                            Module = "退回件分拣",
                            OperateContent = "分拣",
                            Operater = Nickname,
                            OperateTime = DateTime.Now,
                            PID = model.ID
                        };
                        db.T_OperaterLog.Add(log);
                        db.SaveChanges();
                        sc.Complete();
                        #endregion

                    }
                    else
                    {
                        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

          //  charushuju(ExprNum);

            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
        }


		[Description("退回件编辑保存")]
		public JsonResult ViewSortingEditSave(T_ReturnToStorage model, string jsonStr)
		{

			string ExprNum = "";
			using (TransactionScope sc = new TransactionScope())
			{
				try
				{
					
					string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
					string name = Server.UrlDecode(Request.Cookies["Name"].Value);
					int QueryID = model.ID;
					T_ReturnToStorage ToStorageModle = db.T_ReturnToStorage.SingleOrDefault(a => a.ID == QueryID);
					
					if (!string.IsNullOrWhiteSpace(ToStorageModle.ExternalSingle))
					{
						return Json(new { State = "Faile", Message = "该数据已推送,不允许修改" }, JsonRequestBehavior.AllowGet);
					}
					ExprNum = ToStorageModle.Retreat_expressNumber;
					ToStorageModle.ExternalSingle = "";
				
					ToStorageModle.CollectAddress = model.CollectAddress;
					ToStorageModle.CollectName = model.CollectName;
					ToStorageModle.receivermobile = model.receivermobile;
					ToStorageModle.Retreat_Remarks = model.Retreat_Remarks;
					ToStorageModle.SortingName = model.SortingName;
					ToStorageModle.yuan_expressNumber = model.yuan_expressNumber;
					ToStorageModle.SortingDate = DateTime.Now;
					ToStorageModle.isSorting = 1;
					ToStorageModle.SortingName = Nickname;
					db.Entry<T_ReturnToStorage>(ToStorageModle).State = System.Data.Entity.EntityState.Modified;
					List<T_ReturnToStoragelet> deleteDetail = db.T_ReturnToStoragelet.Where(a => a.Pid == QueryID).ToList();
					if(deleteDetail!=null)
					{

						foreach (T_ReturnToStoragelet item in deleteDetail)
						{
							db.T_ReturnToStoragelet.Remove(item);
						}
					}
					List<T_ReceivedAfter> deleteReceivedAfter = db.T_ReceivedAfter.Where(a => a.CollectExpressNumber == ExprNum).ToList();
					if (deleteReceivedAfter != null)
					{
						foreach (T_ReceivedAfter item in deleteReceivedAfter)
						{
							db.T_ReceivedAfter.Remove(item);
						}
					}
					int i=db.SaveChanges();
					


					List<RetreatSorting> details = App_Code.Com.Deserialize<RetreatSorting>(jsonStr);
				
					
					if (i > 0)
					{
						
						List<T_ReturnToStoragelet> LetList = new List<T_ReturnToStoragelet>();
						string CkCode = ToStorageModle.Retreat_Warehouse;
						for (int z = 0; z < details.Count; z++)
						{
							T_ReturnToStoragelet LetModel = new T_ReturnToStoragelet();
							LetModel.item_code = details[z].item_code;
							LetModel.item_name = details[z].item_name;
							LetModel.Simplename = details[z].Simplename;
							LetModel.qty = details[z].qty;
							LetModel.qualified = details[z].qualified;
							LetModel.Unqualified = details[z].Unqualified;
							LetModel.Notreceived = 0;
							LetModel.Pid = model.ID;
							LetModel.Remarks = details[z].Remarks;
							LetModel.UnitName = details[z].UnitName;
							db.T_ReturnToStoragelet.Add(LetModel);
							db.SaveChanges();

							string CpCode = details[z].item_code;
							T_RetreatSorting SortingModel = db.T_RetreatSorting.SingleOrDefault(a => a.ProductCode == CpCode && a.WarehouseCode == CkCode);
							if (SortingModel != null)
							{
								if (details[z].qualified != 0 || details[z].Unqualified != 0)
								{

									SortingModel.QualifiedNumber = SortingModel.QualifiedNumber + details[z].qualified;
									SortingModel.UnqualifiedNumber = SortingModel.UnqualifiedNumber + details[z].Unqualified;
									SortingModel.Simplename = details[z].Simplename;
									SortingModel.UnitName = details[z].UnitName;
									db.Entry<T_RetreatSorting>(SortingModel).State = System.Data.Entity.EntityState.Modified;
									db.SaveChanges();
									T_ReturnToStorageDetails DetailsModel = new T_ReturnToStorageDetails();
									DetailsModel.item_code = details[z].item_code;
									DetailsModel.item_name = details[z].item_name;
									DetailsModel.qty = details[z].qualified + details[z].Unqualified;
									DetailsModel.Pid = model.ID;
									db.T_ReturnToStorageDetails.Add(DetailsModel);
									db.SaveChanges();
								}
							}
							else
							{
								if (details[z].qualified != 0 || details[z].Unqualified != 0)
								{
									T_RetreatSorting Smodle = new T_RetreatSorting();
									Smodle.ProductCode = details[z].item_code;
									Smodle.ProductName = details[z].item_name;
									Smodle.QualifiedNumber = details[z].qualified;
									Smodle.UnqualifiedNumber = details[z].Unqualified;
									Smodle.Simplename = details[z].Simplename;
									Smodle.WarehouseCode = ToStorageModle.Retreat_Warehouse;
									Smodle.UnitName = details[z].UnitName;
									db.T_RetreatSorting.Add(Smodle);
									db.SaveChanges();

									T_ReturnToStorageDetails DetailsModel = new T_ReturnToStorageDetails();
									DetailsModel.item_code = details[z].item_code;
									DetailsModel.item_name = details[z].item_name;
									DetailsModel.qty = details[z].qualified + details[z].Unqualified;
									DetailsModel.Pid = model.ID;
									db.T_ReturnToStorageDetails.Add(DetailsModel);
									db.SaveChanges();
								}
							}



							// T_ReturnToStorageDetails DetailsMode = db.T_ReturnToStorageDetails.SingleOrDefault(a => a.Pid == editID && a.item_code == CpCode);
							int qty = details[z].qty - (details[z].qualified + details[z].Unqualified + details[z].Notreceived);
							
							string modekuai = ToStorageModle.ModularName;


							#region 不管
							if (qty != 0 && modekuai != "无")
							{
								T_ReceivedAfter AfterModel = new T_ReceivedAfter();





								if (modekuai == "退货退款" || modekuai == "退货退款换货")
								{
									//得到当前退款记录
									List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == ExprNum).ToList();



									AfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
									AfterModel.ShopName = RetreatList[0].Retreat_dianpName;
									AfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
									AfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
								}
								else if (modekuai == "换货")
								{
									List<T_ExchangeCenter> ExchangeCenterList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == ExprNum).ToList();
									AfterModel.OrderNumber = ExchangeCenterList[0].OrderCode;
									AfterModel.ShopName = ExchangeCenterList[0].StoreName;
									AfterModel.ShopCode = ExchangeCenterList[0].StoreCode;
									AfterModel.CustomerName = ExchangeCenterList[0].VipName;
									AfterModel.CustomerCode = ExchangeCenterList[0].VipCode;

								}
								else if (modekuai == "拦截")
								{
									List<T_Intercept> interceptList = db.T_Intercept.Where(a => a.MailNo == ExprNum).ToList();
									AfterModel.OrderNumber = interceptList[0].OrderNumber;
									AfterModel.ShopName = Com.GetShopName(interceptList[0].StoreCode);
									AfterModel.ShopCode = interceptList[0].StoreCode;
									AfterModel.CustomerName = interceptList[0].VipName;
									AfterModel.CustomerCode = interceptList[0].VipCode;
								}
								else
								{
									string yuanExnum = ToStorageModle.yuan_expressNumber;
									List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.CollectExpressNumber == yuanExnum).ToList();
									if (ReceivedAfterList.Count > 0)
									{


										AfterModel.OrderNumber = ReceivedAfterList[0].OrderNumber;
										AfterModel.ShopName = ReceivedAfterList[0].ShopName;
										AfterModel.ShopCode = ReceivedAfterList[0].ShopCode;
										AfterModel.CustomerName = ReceivedAfterList[0].CustomerName;
										AfterModel.CustomerCode = ReceivedAfterList[0].CustomerCode;
									}
								}
								AfterModel.IsHandle = 0;
								AfterModel.Type = ToStorageModle.ModularName;
								AfterModel.ProductCode = details[z].item_code;
								AfterModel.ProductName = details[z].item_name;
								AfterModel.CollectExpressName = ToStorageModle.Retreat_expressName;
								AfterModel.CollectExpressNumber = ToStorageModle.Retreat_expressNumber;
								AfterModel.CreatTime = DateTime.Now;
								AfterModel.ProductNumber = qty;
								AfterModel.ShouldReceiveQty = details[z].qty;
								AfterModel.ActualReceiveQty = details[z].Unqualified + details[z].qualified;
								db.T_ReceivedAfter.Add(AfterModel);
								db.SaveChanges();
							}
							#endregion
							#region 上传旺店通

						}


						T_OperaterLog log = new T_OperaterLog()


						{
							Module = "退回件分拣",
							OperateContent = "分拣编辑",
							Operater = Nickname,
							OperateTime = DateTime.Now,
							PID = model.ID
						};
						db.T_OperaterLog.Add(log);
						db.SaveChanges();
						sc.Complete();
						#endregion

					}
					else
					{
						return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
					}

				}
				catch (Exception ex)
				{
					return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
				}
			}

			//  charushuju(ExprNum);

			return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
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


        public void ModularByZPHH()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "换货").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = "   select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenterApprove where  Pid in ( select ID from T_ExchangeCenter where IsDelete=0 and (Status=0 or Status=-1)  ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser";
            List<ModularQuery> RetreatAppRoveQuery = db.Database.SqlQuery<ModularQuery>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "换货";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenter where Status='2' and IsDelete=0 GROUP BY PostUser ";
            List<ModularQuery> RejectNumberQuery = db.Database.SqlQuery<ModularQuery>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "换货";
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

        public void ModularByZPTH()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "退货退款").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_RetreatAppRove where Oid in (select ID from T_Retreat where Isdelete=0 and (Status=0 or Status=-1) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<ModularQuery> RetreatAppRoveQuery = db.Database.SqlQuery<ModularQuery>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "退货退款";
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
            string RejectNumberSql = "select Retreat_ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Retreat where Status='2'  and Isdelete=0 GROUP BY Retreat_ApplyName ";
            List<ModularQuery> RejectNumberQuery = db.Database.SqlQuery<ModularQuery>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "退货退款";
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

        public void ModularByZPBS()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "报损").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where (Status=0 or Status=-1) and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
            List<ModularQuery> RetreatAppRoveQuery = db.Database.SqlQuery<ModularQuery>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "报损";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReport where Status='2' and IsDelete=0 GROUP BY PostUser ";
            List<ModularQuery> RejectNumberQuery = db.Database.SqlQuery<ModularQuery>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "报损";
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
        public void charushuju(string ExprNum)
        {
            //看是否在退货范围内
            if (ExprNum != "" && ExprNum != null)
            {

                List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == ExprNum).ToList();

                List<RetreatBi> RetreatBiList = new List<RetreatBi>();
                //退货退款部分循环
                if (RetreatList.Count > 0)
                {
                    for (int v = 0; v < RetreatList.Count; v++)
                    {
                        int RetreatID = RetreatList[v].ID;
                        List<T_RetreatDetails> DetailsList = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();

                        for (int h = 0; h < DetailsList.Count; h++)
                        {
                            RetreatBi RetreatBiListModel = new RetreatBi();
                            RetreatBiListModel.item_name = DetailsList[h].item_name;
                            RetreatBiListModel.item_code = DetailsList[h].item_code;
                            RetreatBiListModel.qty = int.Parse(DetailsList[h].qty.ToString());
                            RetreatBiListModel.type = "退货退款";
                            RetreatBiList.Add(RetreatBiListModel);
                        }
                    }
                }
                List<T_ExchangeCenter> ExchangeCenterList = db.T_ExchangeCenter.Where(a => a.ReturnExpressCode == ExprNum).ToList();
                //换货部分循环
                if (ExchangeCenterList.Count > 0)
                {
                    for (int v = 0; v < ExchangeCenterList.Count; v++)
                    {
                        int RetreatID = ExchangeCenterList[v].ID;
                        List<T_ExchangeDetail> DetailsList = db.T_ExchangeDetail.Where(a => a.ExchangeCenterId == RetreatID).ToList();

                        for (int h = 0; h < DetailsList.Count; h++)
                        {
                            RetreatBi RetreatBiListModel = new RetreatBi();
                            RetreatBiListModel.item_name = DetailsList[h].SendProductName;
                            RetreatBiListModel.item_code = DetailsList[h].SendProductCode;
                            RetreatBiListModel.qty = int.Parse(DetailsList[h].SendProductNum.ToString());
                            RetreatBiListModel.type = "换货";
                            RetreatBiList.Add(RetreatBiListModel);
                        }
                    }
                }

                List<T_Intercept> InterceptList = db.T_Intercept.Where(a => a.MailNo == ExprNum).ToList();

                //快递拦截部分循环
                if (InterceptList.Count > 0)
                {
                    for (int v = 0; v < InterceptList.Count; v++)
                    {
                        int RetreatID = InterceptList[v].ID;
                        List<T_InterceptDetail> DetailsList = db.T_InterceptDetail.Where(a => a.InterceptId == RetreatID).ToList();

                        for (int h = 0; h < DetailsList.Count; h++)
                        {
                            RetreatBi RetreatBiListModel = new RetreatBi();
                            RetreatBiListModel.item_name = DetailsList[h].Name;
                            RetreatBiListModel.item_code = DetailsList[h].Code;
                            RetreatBiListModel.qty = int.Parse(DetailsList[h].Num.ToString());
                            RetreatBiListModel.type = "快递拦截";
                            RetreatBiList.Add(RetreatBiListModel);
                        }
                    }
                }
                //判断是否有数据
                if (RetreatBiList.Count > 0)
                {
                    //有数据就判断是否有这个快递单号
                    T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == ExprNum);
                    if (Tostorage != null)
                    {
                        //有快递单号的情况下循环
                        for (int w = 0; w < RetreatBiList.Count; w++)
                        {
                            string itemCode = RetreatBiList[w].item_code;
                            int qtys = int.Parse(RetreatBiList[w].qty.ToString());
                            int TostorageID = Tostorage.ID;
                            List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                            //如果不存在时直接插入一条新的
                            if (ReturnToStorageletList.Count == 0)
                            {
                                List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == ExprNum).ToList();
                                if (ReceivedAfterList.Count == 0)
                                {
                                    T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();
                                    if (RetreatBiList[w].type == "快递拦截")
                                    {
                                        ReceivedAfterModel.OrderNumber = InterceptList[0].OrderNumber;
                                        ReceivedAfterModel.CollectExpressName = ShopCodeBy(InterceptList[0].StoreCode);
                                        ReceivedAfterModel.ShopName = InterceptList[0].StoreCode;
                                        ReceivedAfterModel.CustomerCode = InterceptList[0].Receiver;
                                        ReceivedAfterModel.CustomerName = InterceptList[0].Receiver;
                                        ReceivedAfterModel.CreatTime = DateTime.Now;
                                    }
                                    else if (RetreatBiList[w].type == "换货")
                                    {
                                        ReceivedAfterModel.OrderNumber = ExchangeCenterList[0].OrderCode;
                                        ReceivedAfterModel.CollectExpressName = ExchangeCenterList[0].ReturnExpressName;
                                        ReceivedAfterModel.ShopName = ExchangeCenterList[0].StoreName;
                                        ReceivedAfterModel.CustomerCode = ExchangeCenterList[0].VipCode;
                                        ReceivedAfterModel.CustomerName = ExchangeCenterList[0].VipCode;
                                        ReceivedAfterModel.CreatTime = DateTime.Now;
                                    }
                                    else
                                    {
                                        ReceivedAfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
                                        ReceivedAfterModel.CollectExpressName = RetreatList[0].Retreat_expressName;
                                        ReceivedAfterModel.ShopName = RetreatList[0].Retreat_dianpName;
                                        ReceivedAfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
                                        ReceivedAfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
                                        ReceivedAfterModel.CreatTime = DateTime.Now;

                                    }

                                    ReceivedAfterModel.Type = RetreatBiList[w].type;

                                    ReceivedAfterModel.ProductCode = RetreatBiList[w].item_code;
                                    ReceivedAfterModel.ProductName = RetreatBiList[w].item_name;

                                    ReceivedAfterModel.CollectExpressNumber = ExprNum;

                                    ReceivedAfterModel.ProductNumber = RetreatBiList[w].qty;
                                    ReceivedAfterModel.IsHandle = 0;
                                    ReceivedAfterModel.CreatTime = DateTime.Now;
                                    db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                //存在时判断数量是否正确
                                for (int f = 0; f < ReturnToStorageletList.Count; f++)
                                {
                                    if (ReturnToStorageletList[f].qty != qtys)
                                    {

                                        int sqtys = qtys - int.Parse(ReturnToStorageletList[f].qty.ToString());
                                        List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == ExprNum).ToList();
                                        if (ReceivedAfterList.Count == 0)
                                        {
                                            T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();
                                            if (RetreatBiList[w].type == "快递拦截")
                                            {
                                                ReceivedAfterModel.OrderNumber = InterceptList[0].OrderNumber;
                                                ReceivedAfterModel.CollectExpressName = InterceptList[0].ExpressName;
                                                ReceivedAfterModel.ShopName = ShopCodeBy(InterceptList[0].StoreCode);
                                                ReceivedAfterModel.CustomerCode = InterceptList[0].Receiver;
                                                ReceivedAfterModel.CustomerName = InterceptList[0].Receiver;
                                                ReceivedAfterModel.CreatTime = DateTime.Now;
                                            }
                                            else if (RetreatBiList[w].type == "换货")
                                            {
                                                ReceivedAfterModel.OrderNumber = ExchangeCenterList[0].OrderCode;
                                                ReceivedAfterModel.CollectExpressName = ExchangeCenterList[0].ReturnExpressName;
                                                ReceivedAfterModel.ShopName = ExchangeCenterList[0].StoreName;
                                                ReceivedAfterModel.CustomerCode = ExchangeCenterList[0].VipCode;
                                                ReceivedAfterModel.CustomerName = ExchangeCenterList[0].VipCode;
                                                ReceivedAfterModel.CreatTime = DateTime.Now;
                                            }
                                            else
                                            {
                                                ReceivedAfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
                                                ReceivedAfterModel.CollectExpressName = RetreatList[0].Retreat_expressName;
                                                ReceivedAfterModel.ShopName = RetreatList[0].Retreat_dianpName;
                                                ReceivedAfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
                                                ReceivedAfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
                                                ReceivedAfterModel.CreatTime = DateTime.Now;

                                            }
                                            ReceivedAfterModel.Type = RetreatBiList[w].type;
                                            ReceivedAfterModel.ProductCode = RetreatBiList[w].item_code;
                                            ReceivedAfterModel.ProductName = RetreatBiList[w].item_name;
                                            ReceivedAfterModel.CollectExpressNumber = ExprNum;
                                            ReceivedAfterModel.ProductNumber = sqtys;
                                            ReceivedAfterModel.IsHandle = 0;
                                            ReceivedAfterModel.CreatTime = DateTime.Now;
                                            db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                            db.SaveChanges();
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }
        public string ShopCodeBy(string ShopCode)
        {
            List<T_ShopFromGY> shopmodel = db.T_ShopFromGY.Where(a => a.code == ShopCode).ToList();
            if (shopmodel.Count > 0)
            {
                return shopmodel[0].name;
            }
            else
            {
                return "";
            }
        }

        [HttpPost]
        [Description("售后处理推送财务保存")]
        public JsonResult ViewBlendingAddSave(T_ReturnToStorageBlending model, string ID)
        {
            List<T_ReceivedAfter> AfterModel = db.Database.SqlQuery<T_ReceivedAfter>("select * from  T_ReceivedAfter where ID in (" + ID + ") ").ToList();
            if (AfterModel.Count > 0)
            {
                model.Type = AfterModel[0].Type;
                model.OrderNumber = AfterModel[0].OrderNumber;
                model.CollectExpressName = AfterModel[0].CollectExpressName;
                model.CollectExpressNumber = AfterModel[0].CollectExpressNumber;
                model.ShopName = AfterModel[0].ShopName;
                model.CustomerCode = AfterModel[0].CustomerCode;
                model.CustomerName = AfterModel[0].CustomerName;
                model.SurplusProductNumber = AfterModel[0].SurplusProductNumber;
                model.IsBlending = 0;
                db.T_ReturnToStorageBlending.Add(model);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    for (int x = 0; x < AfterModel.Count; x++)
                    {
                        T_ReturnToStorageBlendingDetails BlendingDetailsModel = new T_ReturnToStorageBlendingDetails();
                        BlendingDetailsModel.ProductCode = AfterModel[x].ProductCode;
                        BlendingDetailsModel.ProductName = AfterModel[x].ProductName;
                        BlendingDetailsModel.ProductNumber = AfterModel[x].ProductNumber;
                        BlendingDetailsModel.Pid = model.ID;
                        db.T_ReturnToStorageBlendingDetails.Add(BlendingDetailsModel);
                        db.SaveChanges();


                        int sid = AfterModel[x].ID;
                        T_ReceivedAfter AfterByIdModel = db.T_ReceivedAfter.SingleOrDefault(a => a.ID == sid);

                        AfterByIdModel.IsHandle = 1;
                        db.Entry<T_ReceivedAfter>(AfterByIdModel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);




        }

        [HttpPost]
        [Description("勾兑保存")]
        public JsonResult ViewBlendingExamineAddSave(T_ReturnToStorageBlending model)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    model.BlendingDate = DateTime.Now;
                    model.BlendingName = Nickname;
                    model.IsBlending = 1;
                    db.Entry<T_ReturnToStorageBlending>(model).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    sc.Complete();
                    if (i > 0)
                    {
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Description("驳回勾兑保存")]
        public JsonResult ViewBlendingExamineAddNoSave(T_ReturnToStorageBlending model)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    model.BlendingDate = DateTime.Now;
                    model.BlendingName = Nickname;
                    model.IsBlending = 2;
                    db.Entry<T_ReturnToStorageBlending>(model).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        int ID = model.ID;
                        List<T_ReturnToStorageBlendingDetails> Deteils = db.T_ReturnToStorageBlendingDetails.Where(a => a.Pid == ID).ToList();
                        for (int x = 0; x < Deteils.Count; x++)
                        {
                            T_ReceivedAfter AfterModel = new T_ReceivedAfter();
                            AfterModel.Type = model.Type;
                            AfterModel.OrderNumber = model.OrderNumber;
                            AfterModel.CollectExpressName = model.CollectExpressName;
                            AfterModel.CollectExpressNumber = model.CollectExpressNumber;
                            AfterModel.ShopName = model.ShopName;
                            AfterModel.CustomerCode = model.CustomerCode;
                            AfterModel.CustomerName = model.CustomerName;
                            AfterModel.ProductCode = Deteils[x].ProductCode;
                            AfterModel.ProductName = Deteils[x].ProductName;
                            AfterModel.ProductNumber = Deteils[x].ProductNumber;
                            AfterModel.IsHandle = 0;
                            db.T_ReceivedAfter.Add(AfterModel);
                            db.SaveChanges();
                        }

                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }


		public FileResult getExcelAfter( string StartDate, string EndDate)
		{
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "退回件导出",
				OperateContent = string.Format("导出excel getExcelAfter 条件 startDate:{0},endDate:{1}", StartDate, EndDate),
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();

			IQueryable<T_ReceivedAfter> queryData = db.T_ReceivedAfter.Where(a=>a.IsHandle==0);
			//显示当前用户的数据
			DateTime sdate = DateTime.Now.AddDays(-7);
			DateTime edate = DateTime.Now.AddDays(1);
			if (!string.IsNullOrEmpty(StartDate))
			{
				sdate =Convert.ToDateTime(StartDate);
				queryData= queryData.Where(a => a.CreatTime > sdate);
			}
			if (!string.IsNullOrEmpty(EndDate))
			{
				edate = Convert.ToDateTime(EndDate).AddDays(1);
				queryData = queryData.Where(a => a.CreatTime < edate);
			}
			List<T_ReceivedAfter> list = queryData.ToList();
			if (list.Count > 0)
			{
				NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
				//添加一个sheet
				NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
				//给sheet1添加第一行的头部标题
				NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

				row1.CreateCell(0).SetCellValue("类型");
				row1.CreateCell(1).SetCellValue("订单号");
				row1.CreateCell(2).SetCellValue("产品编码");
				row1.CreateCell(3).SetCellValue("产品名称");
				row1.CreateCell(4).SetCellValue("快递名称");
				row1.CreateCell(5).SetCellValue("快递单号");
				row1.CreateCell(6).SetCellValue("店铺名称");
				row1.CreateCell(7).SetCellValue("客户名称");
				row1.CreateCell(8).SetCellValue("会员名称/旺旺");
				row1.CreateCell(9).SetCellValue("售后建单数");
				row1.CreateCell(10).SetCellValue("仓库实收数");
				row1.CreateCell(11).SetCellValue("差异数量");
				row1.CreateCell(12).SetCellValue("新增时间");
				

				for (int i = 0; i < list.Count; i++)
				{


					NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
					rowtemp.CreateCell(0).SetCellValue(list[i].Type);
					rowtemp.CreateCell(1).SetCellValue(list[i].OrderNumber);
					rowtemp.CreateCell(2).SetCellValue(list[i].ProductCode);
					rowtemp.CreateCell(3).SetCellValue(list[i].CollectExpressName);
					rowtemp.CreateCell(4).SetCellValue(list[i].CollectExpressNumber);
					rowtemp.CreateCell(5).SetCellValue(list[i].ShopName);
					rowtemp.CreateCell(6).SetCellValue(list[i].CustomerName);
					rowtemp.CreateCell(7).SetCellValue(list[i].CustomerCode);
					
					rowtemp.CreateCell(9).SetCellValue(list[i].ShouldReceiveQty.ToString());
					rowtemp.CreateCell(10).SetCellValue(list[i].ActualReceiveQty.ToString());
					rowtemp.CreateCell(11).SetCellValue(list[i].ProductNumber.ToString());
					rowtemp.CreateCell(12).SetCellValue(list[i].CreatTime.ToString());
					
				}

				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				book.Write(ms);
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "售后问题件.xls");
			}
			else
			{
				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "售后问题件.xls");
			}
		}
	}
}
