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
    public class DeliverController : BaseController
    {
        //
        // GET: /Deliver/
        #region 视图
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        [Description("发货复核新增页面")]
        public ActionResult DeliverAdd()
        {
           ViewData["WarehouseCode"] = Com.Warehouses();
            return View();
        }
         [Description("发货复核查询页面")]
        public ActionResult DeliverList()
        {
           ViewData["WarehouseCode"] = Com.Warehouses();
            return View();
        }
         [Description("发货复核详情页面")]
         public ActionResult Detail(int ID)
         {
             ViewData["ID"] = ID;
             return View();
         }
        #endregion
        #region 查询方法  PC 端
        //查询订单号返回详情
        //public JsonResult getOrderDetail(string num)
        //{
        //    try
        //    {
        //        T_OrderList list = db.T_OrderList.SingleOrDefault(a => a.platform_code == num);
        //        string result = "";
        //        if (list != null)
        //        {                   
        //           var  detail = db.T_OrderDetail.Where(a => a.oid == list.code).Select(item => new
        //           {
        //               item.item_name,
        //               item.item_code,
        //               item.item_simple_name,
        //               item.qty
        //           });
        //            result = "{\"rows\":" + JsonConvert.SerializeObject(detail) + "}";
        //        }
        //        else
        //        {
        //            result = "请输入正确的订单号";
        //            return Json(new { State = "Faile", Message = result }, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(new { State = "Success", result = result }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        class AddMod{
            public string item_name { get; set; }
            public string item_code { get; set; }
            public string sku_name { get; set; }
            public Nullable<int> qty { get; set; }
            public string  tiaoma { get; set; }
        }
        class TiaoMaAddMod
        {
            public string tiaoma { get; set; }
        }
        //查询单据编号返回详情 数据来自管易
        public JsonResult getOrderDetail(string num)
        {
            try {
                string result = "";
                if (num != null)
                {
                    App_Code.GY gy = new App_Code.GY();
                    string cmd = "";
                   
                    cmd = "{" +
                          "\"appkey\":\"171736\"," +
                          "\"method\":\"gy.erp.trade.deliverys.get\"," +
                          "\"page\":1," +
                          "\"page_size\":3," +
                          "\"code\":\"" + num + "\"," +
                          "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                          "}";

                    string sign = gy.Sign(cmd);
                    cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                    string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                    JsonData jsonData = null;
                    jsonData = JsonMapper.ToObject(ret);
                    DateTime dateStar = DateTime.Parse("2017-02-10 16:07:50.473");
                    DateTime dateEnd = DateTime.Parse("2017-02-11 16:07:50.473");
                    if (jsonData["deliverys"].Count == 0)
                    {
                        cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.trade.deliverys.history.get\"," +
                        "\"page\":1," +
                        "\"page_size\":3," +
                        "\"code\":\"" + num + "\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                        "}";                     
                        sign = gy.Sign(cmd);
                        cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                        ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                        jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        if (jsonData.Count == 6 || jsonData["deliverys"].Count == 0)
                        {
                            result = "单据编号不存在";
                            return Json(new { State = "Faile", Message = result }, JsonRequestBehavior.AllowGet);                         
                        }
                    }
                    JsonData jsonOrders = jsonData["deliverys"][0];
                    //查询管易发货字段 可是不能修改
                    //int state = int.Parse((jsonOrders["delivery_state"]).ToString());

                    //if (state != 0) {
                    //    result = "该订单已经发货";
                    //    return Json(new { State = "Faile", Message = result }, JsonRequestBehavior.AllowGet);
                    //}
                    // 查询发货表 订单是否发货
                    T_Deliver deliverFlag = db.T_Deliver.FirstOrDefault(a => a.MailNo == num);
                    if (deliverFlag != null) {
                        return Json(new { State = "Faile", Message = "该单据编号已经发货" }, JsonRequestBehavior.AllowGet);
                    }
                    JsonData details = jsonOrders["details"];
                    List<AddMod> detalislist = new List<AddMod>();
                    for (int s = 0; s < details.Count; s++)
                    {
                        //返回的数据构造
                        AddMod detalis = new AddMod();
                        JsonData detailItem = details[s];
                        string Code = isNULL(detailItem["item_code"]).ToString();
                        detalis.item_code = isNULL(detailItem["item_code"]).ToString();
                        detalis.item_name = isNULL(detailItem["item_name"]).ToString();
                        detalis.qty = int.Parse(detailItem["qty"].ToString());
                        detalis.sku_name = isNULL(detailItem["sku_name"]).ToString();
                        string  Tiaoma = db.T_goodsGY.SingleOrDefault(a => a.code == Code).barcode;
                        detalis.tiaoma = Tiaoma;
                        detalislist.Add(detalis);
                    }
                    string Warehouse = isNULL(jsonOrders["warehouse_name"]).ToString();
                    string OrderNum = isNULL(jsonOrders["platform_code"]).ToString();

                    result = "{\"rows\":" + JsonConvert.SerializeObject(detalislist) + ",\"Warehouse\":\"" + Warehouse + "\",\"OrderNum\":\"" + OrderNum + "\"}";
                    
                }
                else {
                    result = "请输入正确的单据编号";
                    return Json(new { State = "Faile", Message = result }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { State = "Success", result = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //发货保存方法
        [HttpPost]
        public JsonResult Save(T_Deliver model, string jsonStr, string CurUser, string TiaoMa, string type)
        {
           
            using (TransactionScope sc = new TransactionScope())
            {
                try {
                    // 查询发货表 订单是否发货
                    T_Deliver deliverFlag = db.T_Deliver.FirstOrDefault(a => a.MailNo == model.MailNo);
                    if (deliverFlag != null)
                    {
                        return Json(new { State = "Faile", Message = "该单据编号已经发货" }, JsonRequestBehavior.AllowGet);
                    }
                    T_Warehouses warehouseMod = db.T_Warehouses.SingleOrDefault(a=>a.name==model.WarehouseCode);
                    //发货记录表增加发货记录
                    string curUser ="";
                    if (!string.IsNullOrWhiteSpace(CurUser))
                    {
                        curUser = CurUser;
                    }
                    else {
                        curUser = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    }

                    string IP = Request.UserHostAddress;
                    T_Deliver MOD = new T_Deliver();
                    MOD.PostTime = DateTime.Now;
                    MOD.PostName = curUser;
                    MOD.OrderNum = model.OrderNum;
                    MOD.MailNo = model.MailNo;
                    MOD.WarehouseCode = warehouseMod.code;
                    MOD.IP = IP;
                    MOD.Note = model.Note;
                    db.T_Deliver.Add(MOD);
                    db.SaveChanges();
                    /*  
                     * 解析jsonStr
                     * 修改库存表发货详情记录表
                     * 库存表是否有该商品的库存信息？直接修改库存数量：新增一条库存信息qty为负数
                     */
                    List<AddMod> detail = Com.Deserialize<AddMod>(jsonStr);
                
                    if (type == "1")
                    {
                        List<TiaoMaAddMod> TiaoMaAddModList = new List<TiaoMaAddMod>();
                        string[] tiaomaList = TiaoMa.Split('\n');
                        int Qty = 0;
                     
                        for (int i = 0; i < detail.Count; i++)
                        {
                            Qty += int.Parse(detail[i].qty.ToString());
                            int tiaomalistQty = 0;
                            int detailcount = int.Parse(detail[i].qty.ToString());
                            for (int z = 0; z < tiaomaList.Length ; z++)
                            {

                                if (tiaomaList[z] == "")
                                {
                                    break;
                                }
                                
                                string tiaomazz = "";
                                //if (z == tiaomaList.Length - 1)
                                //{
                                //     tiaomazz = tiaomaList[z].Substring(0, tiaomaList[z].Length);

                                //}
                                //else
                                //{
                                //     tiaomazz = tiaomaList[z].Substring(0, tiaomaList[z].Length - 1);
                                //}
                                tiaomazz = tiaomaList[z].Substring(0, tiaomaList[z].Length - 1);
                                if (detail[i].tiaoma == tiaomazz)
                                {
                                    tiaomalistQty++;
                                }
                            }
                            if (detailcount != tiaomalistQty)
                            {
                                TiaoMaAddMod TiaoMaAddModModel = new TiaoMaAddMod();
                                TiaoMaAddModModel.tiaoma = detail[i].tiaoma;
                                TiaoMaAddModList.Add(TiaoMaAddModModel);
                            }
                        }
                       
                   
                
                    if (TiaoMaAddModList.Count > 0)
                    {
                        string nottiaoma = "";
                        for (int i = 0; i < TiaoMaAddModList.Count; i++)
                        {
                            if (i == TiaoMaAddModList.Count - 1)
                            {
                                nottiaoma += TiaoMaAddModList[i].tiaoma;
                            }
                            else
                            {
                                nottiaoma += TiaoMaAddModList[i].tiaoma+",";
                            }
                        }
                        return Json(new { State = "Faile", Message = "该"+nottiaoma+"数量与详情不符合",stype="0" }, JsonRequestBehavior.AllowGet);
                    }
                    int tiaomaQty = 0;
                    for (int i = 0; i < tiaomaList.Length; i++)
                    {
                        if (tiaomaList[i] != "")
                        {
                            tiaomaQty++;
                        }
                    }
                    if (Qty != tiaomaQty)
                    {
                        return Json(new { State = "Faile", Message = "存在多发货物", stype = "0" }, JsonRequestBehavior.AllowGet);
                    }
                    }

                    for (int i = 0; i < detail.Count; i++)
                    {
                        //操作库存表
                        string _code = detail[i].item_code;
                        T_Stock StocMOD = db.T_Stock.SingleOrDefault(a => a.Code == _code);
                        if (StocMOD != null)
                        {
                            int _qty =  int.Parse(detail[i].qty.ToString());
                            StocMOD.Qty -= _qty;
                            db.Entry<T_Stock>(StocMOD).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else {
                            T_Stock createMod = new T_Stock();
                            createMod.Name = detail[i].item_name;
                            createMod.Code = detail[i].item_code;
                            createMod.Qty = -int.Parse(detail[i].qty.ToString());
                            createMod.WarehouseName = warehouseMod.code;
                            db.T_Stock.Add(createMod);
                            db.SaveChanges();
                        }
                        //操作发货详情记录表
                        T_DeliverDetail detailMOD = new T_DeliverDetail();
                        detailMOD.Code = detail[i].item_code;
                        detailMOD.Name = detail[i].item_name;
                        detailMOD.Qty = detail[i].qty;
                        detailMOD.Oid = MOD.ID;
                        db.T_DeliverDetail.Add(detailMOD);
                        db.SaveChanges();


                        T_StockOutstorage StorageModel = new T_StockOutstorage();
                        StorageModel.Code = detail[i].item_code;
                        StorageModel.Name = detail[i].item_name;
                        StorageModel.WarehouseCode = model.WarehouseCode;
                        StorageModel.number = model.MailNo;
                        StorageModel.Qty = detail[i].qty;
                        StorageModel.Type = "出库";
                        db.T_StockOutstorage.Add(StorageModel);
                        db.SaveChanges();
                    }
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        class listResult
        {
            public int ID { set; get; } 
            public string MailNo { set; get; }
            public string WarehouseCode { set; get; }
            public string Note { set; get; }
            public string OrderNum { set; get; }
            public string PostName { set; get; }
            public Nullable<System.DateTime> PostTime { get; set; }
        }
        //根据仓库code返回仓库名字
        public string getWarehouse(string code)
        {
            string result = "";
            T_Warehouses mod = db.T_Warehouses.FirstOrDefault(a => a.code == code);
            if (mod != null)
            {
                result = mod.name;
            }
            return result;
        }
        //查询  
         [HttpPost]
        public ContentResult GetList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime) 
        {
            var list = db.T_Deliver.AsQueryable();
            //根据名称或者单号查询
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                list = list.Where(s => s.MailNo.Equals(queryStr) || s.PostName.Equals(queryStr));
            }
            //根据日期查询
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {

                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                list = list.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                list = list.Where(s => s.PostTime >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                list = list.Where(s => s.PostTime <= endTime);
            }
            //分页
            pager.totalRows = list.Count();
            var  queryData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).Select(item => new
            {
                item.ID,
                item.MailNo,
                item.WarehouseCode,
                item.Note,
                item.OrderNum,
                item.PostName,
                item.PostTime
            });
            //结果集合重新包装成需要的数据返回
            List<listResult> ResultMod = new List<listResult>();
            foreach (var item in queryData)
            {
                listResult ietmMOD = new listResult();
                ietmMOD.ID = item.ID;
                ietmMOD.MailNo = item.MailNo;
                ietmMOD.Note = item.Note;
                ietmMOD.OrderNum = item.OrderNum;
                ietmMOD.PostName = item.PostName;
                ietmMOD.PostTime = item.PostTime;
                ietmMOD.WarehouseCode = getWarehouse(item.WarehouseCode);
                ResultMod.Add(ietmMOD);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(ResultMod, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //查询详情
         [HttpPost]
         public ContentResult GetDetailList(Lib.GridPager pager, int ID)
         {
             var list = db.T_DeliverDetail.Where(a => a.Oid == ID);
             pager.totalRows = list.Count();
             var queryData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData) + "}";
             return Content(json);
         }
        #endregion
        #region 仓库发货终端

        #endregion
    }
}
