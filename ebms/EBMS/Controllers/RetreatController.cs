using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class RetreatController : BaseController
    {
        //访问数据库接口  
        EBMSEntities db = new EBMSEntities();
        wdt_bakEntities db_bak = new wdt_bakEntities();
        //
        // GET: /Retreat/
        #region 公共属性/字段/方法

        //绑定支付帐号
        public List<SelectListItem> GetRetreatPaymentList(string ShopName)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            var list = db.Database.SqlQuery<T_RetreatPayment>(" select * from T_RetreatPayment  where ShopName='" + ShopName + "'  order by  IsBlending desc ").AsQueryable();

            var selectList = new SelectList(list, "PaymentAccounts", "PaymentAccounts");
            List<SelectListItem> selecli = new List<SelectListItem>();
            //selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        //绑定原因无默认值
        public List<SelectListItem> GetRetreatReasonList()
        {


            var list = db.Database.SqlQuery<T_RetreatReason>("select * from T_RetreatReason order by ID desc");
            var selectList = new SelectList(list, "RetreatReason", "RetreatReason");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        //绑定快递记录无默认值
        public List<SelectListItem> GetExpressTypeList()
        {
            List<SelectListItem> ExpressType = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==可选择处理结果==", Value = "" },
                 new SelectListItem { Text = "未处理", Value = "wcl"},
                 new SelectListItem { Text = "未达到顾客地址", Value = "未达到顾客地址"},
                 new SelectListItem { Text = "已有返回物流信息", Value = "已有返回物流信息"},
                 new SelectListItem { Text = "已做退回标签", Value = "已做退回标签"},
                 new SelectListItem { Text = "已留言", Value = "已留言" },
                 new SelectListItem { Text = "问题件", Value = "问题件" },
                 new SelectListItem { Text = "投诉仲裁", Value = "投诉仲裁" },
                 new SelectListItem { Text = "完成", Value = "完成" }
            };
            return ExpressType;
        }
        //绑定原因
        public List<SelectListItem> GetRetreatReasonList(object obj)
        {


            var list = db.T_RetreatReason.AsQueryable();
            var selectList = new SelectList(list, "RetreatReason", "RetreatReason", obj);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
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
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        public partial class ckshul
        {

            public int ID { get; set; }
            public int shul { get; set; }
            public string WarehouseCode { get; set; }
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


        }
        //快递根据COde查询名称
        public string GetexpressString(string code)
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

        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            List<T_RetreatAppRove> history = db.Database.SqlQuery<T_RetreatAppRove>("select * from T_RetreatAppRove where oid='" + id + "' order by ID").ToList();
            history.OrderBy(a => a.ID);
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
        #endregion
        #region 视图，查询

        [Description("未审核多行查询")]
        public ActionResult ViewRetreatQueryMultiLine()
        {

            return View();
        }


        [Description("委外单号")]
        public ActionResult ViewRetreatOutsourcingList()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatReason"] = RetreatReason();
            return View();
        }
        [Description("退货无头件")]
        public ActionResult ViewRetreatNoheadpartsAdd()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatReasonList"] = GetRetreatReasonList();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            return View();
        }

        [Description("退货出库记录")]
        public ActionResult ViewRetreatWarehouseRecord()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            return View();
        }
        [Description("退货移库退货")]
        public ActionResult ViewRetreatWarehouseEdit(string ID, string type)
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehousesz();
            ViewData["ID"] = ID;
            ViewData["type"] = type;
            return View();
        }


        [Description("退货库存")]
        public ActionResult ViewRetreatWarehouse()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            return View();
        }

        [Description("无头件")]
        public ActionResult ViewRetreatNoheadparts()
        {
            return View();
        }


        [Description("退货退款分拣新增")]
        public ActionResult ViewRetreatSortingAdd(int ID)
        {
            T_Retreat RetreatModel = db.T_Retreat.SingleOrDefault(a => a.ID == ID);
            ViewData["ID"] = ID;
            RetreatModel.Retreat_expressName = GetexpressString(RetreatModel.Retreat_expressName);
            return View(RetreatModel);

        }
        [Description("退货退款分拣")]
        public ActionResult ViewRetreatsorting()
        {
            return View();
        }
        [Description("退货退款已审核")]
        public ActionResult ViewRetreatChecken()
        {
            ViewData["RetreatReason"] = RetreatReason();
            return View();
        }

        public ActionResult RetreatEdit(string code)
        {


            return View();
        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        public List<SelectListItem> GetOrd(object obj)
        {

            List<SelectListItem> OrderSatuslist = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==订单状态==", Value = "" },
                 new SelectListItem { Text = "未完成订单", Value = "未完成订单"},


                 new SelectListItem { Text = "已完成订单", Value = "已完成订单" }
            };


            List<SelectListItem> selecli = new List<SelectListItem>();
            //遍历集合，如果当前Domain model的某个属性与SelectListItem的Value属性相等，把SelectListItem的Selected属性设置为true
            foreach (SelectListItem item in OrderSatuslist)
            {
                if (item.Value == Convert.ToString(obj))
                {
                    item.Selected = true;
                }
            }

            selecli.AddRange(OrderSatuslist);
            return selecli;

        }
        public List<SelectListItem> Getstatus(object obj)
        {

            List<SelectListItem> OrderSatuslist = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==请选择==", Value = "" },
                 new SelectListItem { Text = "是", Value = "1"},


                 new SelectListItem { Text = "否", Value = "0" }
            };


            List<SelectListItem> selecli = new List<SelectListItem>();
            //遍历集合，如果当前Domain model的某个属性与SelectListItem的Value属性相等，把SelectListItem的Selected属性设置为true
            foreach (SelectListItem item in OrderSatuslist)
            {
                if (item.Value == Convert.ToString(obj))
                {
                    item.Selected = true;
                }
            }

            selecli.AddRange(OrderSatuslist);
            return selecli;

        }
        [Description("编辑退货退款")]
        public ActionResult ViewRetreatEdit(int ID)
        {
            T_Retreat RetreatModel = db.T_Retreat.SingleOrDefault(a => a.ID == ID);
            ViewData["ID"] = ID;
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses(RetreatModel.Retreat_Warehouse);
            ViewData["RetreatReasonList"] = GetRetreatReasonList(RetreatModel.Retreat_Reason);
            //ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName(RetreatModel.Retreat_expressName);
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();


            ViewData["OrderSatuss"] = GetOrd(RetreatModel.OrderSatus);
            ViewData["IsReturns"] = Getstatus(RetreatModel.IsReturn);
            ViewData["IsRefunds"] = Getstatus(RetreatModel.IsRefund);
            return View(RetreatModel);
        }
        [Description("快递记录")]
        public ActionResult RetreatJilu(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("快递记录新增")]
        public ActionResult RetreatJiluAdd(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }


        [Description("退款财务修改")]
        public ActionResult RetreatModify()
        {
            return View();
        }
        public ActionResult ViewModify(int ID = 0)
        {
            T_Retreat EditModel = db.T_Retreat.Find(ID);
            string ShopName = EditModel.Retreat_dianpName;
            ViewData["Retreat_Payment"] = GetRetreatPaymentList(ShopName);
            if (EditModel != null)
            {
                return View(EditModel);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Description("快递记录新增")]
        public JsonResult JiluSave(T_RetreatExpressRecord model, string id, string selected_val)
        {
            int ID = int.Parse(id);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            model.TrackRecord_Name = name;
            model.TrackRecord_Date = DateTime.Now;
            model.Oid = ID;
            model.TrackRecord_Situation = selected_val;
            db.T_RetreatExpressRecord.Add(model);

            int i = db.SaveChanges();


            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Description("快递记录数据查询")]
        public ContentResult GetRetreatJiluList(Lib.GridPager pager, string ID)
        {
            int id = int.Parse(ID);
            IQueryable<T_RetreatExpressRecord> queryData = db.T_RetreatExpressRecord.Where(a => a.Oid == id);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_RetreatExpressRecord> list = new List<T_RetreatExpressRecord>();
            foreach (var item in queryData)
            {
                T_RetreatExpressRecord i = new T_RetreatExpressRecord();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);


        }
        [Description("我的退货退款")]
        public ActionResult ViewRetreat()
        {
            ViewData["ReasonType"] = GetRetreatReasonList();
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            return View();
        }
        /// <summary>
        /// 绑定退货原因
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> RetreatReason()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_RetreatReason.AsQueryable();
            var selectList = new SelectList(list, "RetreatReason", "RetreatReason");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        [Description("退货退款管理")]
        public ActionResult ViewRetreatList()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatReason"] = RetreatReason();
            return View();
        }

        [Description("退货退款管理")]
        public ActionResult ViewRetreatManage()
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatReason"] = RetreatReason();
            return View();
        }
        [Description("产品管理")]
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("未审核退货退款")]
        public ActionResult ViewRetreatCheck()
        {
            //得到是谁进来
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["ReasonType"] = GetRetreatReasonList();
            ViewData["ExpressType"] = GetExpressTypeList();
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();
            ViewData["RetreatShop"] = App_Code.Com.Shop();
            ViewData["RetreatBackFrom"] = App_Code.Com.RetreatBackFrom();
            //得到是否快递组
            T_RetreatGroup ExpressRecordModel = db.T_RetreatGroup.SingleOrDefault(a => a.Crew.Contains(Nickname) && a.GroupName == "快递");
            if (ExpressRecordModel != null)
            {
                ViewData["ExpressRecord"] = ExpressRecordModel.GroupName;
            }
            else
            {
                ViewData["ExpressRecord"] = "";
            }
            // 查询超过6天没审核的仓库数据自动驳回

            //查询超过2小时自动审核
            List<T_RetreatReason> ReasonList = db.T_RetreatReason.Where(a => a.Type == "14").ToList();


            string[] Arry = new string[ReasonList.Count];
            for (int x = 0; x < ReasonList.Count; x++)
            {
                Arry[x] = ReasonList[x].RetreatReason.ToString();
            }
            List<T_Retreat> RetreatList = db.T_Retreat.Where(a => Arry.Contains(a.Retreat_Reason) && a.Status == -1 && a.Step == 0).ToList();
            if (RetreatList.Count > 0)
            {

                for (int c = 0; c < RetreatList.Count; c++)
                {
                    int id = RetreatList[c].ID;
                    T_RetreatAppRove AppRoveList = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.Status == 1 && a.ApproveDName == "财务");

                    if (AppRoveList == null)
                    {
                        DateTime dt = DateTime.Parse(RetreatList[c].Retreat_date.ToString());
                        TimeSpan diff = DateTime.Now.Subtract(dt);
                        if (diff.Days > 0 || diff.Hours >= 1)
                        {


                            T_RetreatAppRove AppRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null && a.ApproveDName == "仓库售后");
                            if (AppRoveModel != null)
                            {
                                AppRoveModel.ApproveName = "仓库售后";
                                AppRoveModel.Memo = "审核超时，自动审核";
                                AppRoveModel.ApproveTime = DateTime.Now;
                                AppRoveModel.Status = 1;
                                db.Entry<T_RetreatAppRove>(AppRoveModel).State = System.Data.Entity.EntityState.Modified;

                            }
                            int i = db.SaveChanges();

                            T_RetreatAppRove AppRoveCWModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);

                            if (i > 0 && AppRoveCWModel == null)
                            {

                                T_Retreat model = db.T_Retreat.Find(id);
                                T_RetreatAppRove newApprove = new T_RetreatAppRove();

                                string type = AppRoveModel.Type;
                                int step = int.Parse(model.Step.ToString());
                                step++;
                                IQueryable<T_RetreatConfig> config = db.T_RetreatConfig.Where(a => a.Reason == type);
                                int stepLength = config.Count();//总共步骤
                                if (step < stepLength)
                                {
                                    //不是最后一步，主表状态为0 =>审核中
                                    model.Status = 0;
                                    T_RetreatConfig stepMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                    string nextName = stepMod.Name;
                                    //下一步审核人不是null  审核记录插入一条新纪录
                                    newApprove.Memo = "";
                                    newApprove.Oid = id;
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
                                        T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveDName == "财务" && a.Status == 1);

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
                                    //if (shenheName == "财务")
                                    //{
                                    //T_OrderList OrderModel = db.T_OrderList.Find(model.OrderId);
                                    //OrderModel.Status_Retreat = 2;
                                    //db.Entry<T_OrderList>(OrderModel).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    //  List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();
                                    //string code = OrderModel.code;
                                    //foreach (var item in details)
                                    //{
                                    //    string itemcode = item.item_code;
                                    //    T_OrderDetail OrderDetailModel = db.T_OrderDetail.SingleOrDefault(a => a.oid == code && a.item_code == itemcode);
                                    //    OrderDetailModel.RetreatQty = item.qty;
                                    //    OrderDetailModel.RetreatStatus = 1;
                                    //    db.Entry<T_OrderDetail>(OrderDetailModel).State = System.Data.Entity.EntityState.Modified;
                                    //    db.SaveChanges();
                                    //}
                                    // }

                                }
                                else
                                {
                                    //最后一步，主表状态改为 1 => 同意
                                    model.Status = 1;
                                }
                                model.Step = step;
                                db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            //ModularByZP();


                        }
                    }


                }
            }
            return View();
        }
        [Description("访问退货退款详情页面")]
        public ActionResult ViewRetreatDetail(int tid)
        {
            ViewData["ID"] = tid;
            T_Retreat model = db.T_Retreat.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            model.Retreat_Warehouse = GetWarehouseString(model.Retreat_Warehouse);
            model.Retreat_expressName = App_Code.Com.GetExpressName(model.Retreat_expressName);
            GetApproveHistory(tid);
            ViewData["OrderNumber"] = model.Retreat_OrderNumber;

            return View(model);
        }

        [Description("审核退货退款")]
        public ActionResult ViewRetreatReportCheck(int ID)
        {



            T_RetreatAppRove modelApprove = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == ID && a.ApproveTime == null);
            T_Retreat Retreat = new T_Retreat();
            Retreat = db.T_Retreat.Single(a => a.ID == ID);
            string ShopName = Retreat.Retreat_dianpName;
            ViewData["Retreat_Payment"] = GetRetreatPaymentList(ShopName);

            List<T_RetreatAppRove> approve = db.Database.SqlQuery<T_RetreatAppRove>("select * from T_RetreatAppRove where oid='" + ID + "' order by ID").ToList();

            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in approve)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=#d02e2e>未审核</font>";
                if (item.Status == 1) s = "<font color=#1fc73a>已同意</font>";
                if (item.Status == 2) s = "<font color=#d02e2e>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["approveid"] = ID;
            ViewData["Approve"] = modelApprove.ApproveDName;
            ViewData["RetreatReason"] = Retreat.Retreat_Reason;
            if (Retreat.Retreat_expressName != null && Retreat.Retreat_expressName != "")
            {
                Retreat.Retreat_expressName = GetexpressString(Retreat.Retreat_expressName);
            }
            if (Retreat.Retreat_Warehouse != null && Retreat.Retreat_Warehouse != "")
            {
                Retreat.Retreat_Warehouse = GetWarehouseString(Retreat.Retreat_Warehouse);
            }
            return View(Retreat);


        }


        //退货详情列表 
        [HttpPost]
        public ContentResult GetRetreatDetailList(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_RetreatDetails> queryData = db.T_RetreatDetails.Where(a => a.Oid == ID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.item_code != null && a.item_code.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_RetreatDetails> list = new List<T_RetreatDetails>();
            foreach (var item in queryData)
            {
                T_RetreatDetails i = new T_RetreatDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
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

        class listCrewModel
        {
            public string Name { get; set; }
        }
        /// <summary>
        /// 根据部门选择下级审核人
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetApproveByDeptID()
        {

            T_RetreatGroup list = db.T_RetreatGroup.SingleOrDefault(s => s.GroupName == "部门主管");
            string[] listCrew = list.Crew.Split(',');
            Dictionary<string, string> listCrewModel = new Dictionary<string, string>();
            for (int i = 0; i < listCrew.Length; i++)
            {
                string name = listCrew[i];

                listCrewModel.Add(name, name);
            }

            return Json(listCrewModel.ToArray());

        }

        [Description("新增退货退款")]
        public ActionResult ViewRetreatAdd(string code)
        {
            ViewData["RetreatWarehouseList"] = App_Code.Com.Warehouses();
            ViewData["RetreatReasonList"] = GetRetreatReasonList();
            ViewData["RetreatexpressNameList"] = App_Code.Com.ExpressName();

            List<SelectListItem> OrderSatuslist = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==订单状态==", Value = "" },
               new SelectListItem { Text = "未完成订单", Value = "未完成订单"},


                 new SelectListItem { Text = "已完成订单", Value = "已完成订单" }
            };
            ViewData["OrderSatuss"] = OrderSatuslist;

            List<SelectListItem> isoklist = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==请选择==", Value = "" },
                 new SelectListItem { Text = "是", Value = "1"},
                 new SelectListItem { Text = "否", Value = "0" }
            };
            ViewData["IsRefunds"] = isoklist;
            ViewData["IsReturns"] = isoklist;

            return View();
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

        public string RepeatMemo(string code)
        {
            string repeat = "";
            List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0").ToList();
            if (modelList.Count > 0)
            {

                repeat += "退货退款记录重复，";
            }
            //查是否有返现记录

            List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 2).ToList();
            if (cash.Count > 0)
            {
                repeat += "有返现记录重复，";
            }
            List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (Reissue.Count > 0)
            {
                repeat += "有补发记录重复，";
            }
            List<T_ExchangeCenter> ExchangeCenter = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (ExchangeCenter.Count > 0)
            {
                repeat += "有换货记录重复，";
            }
            List<T_Intercept> Intercept = db.T_Intercept.Where(a => a.OrderNumber.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (Intercept.Count > 0)
            {
                repeat += "拦截模块有记录，";
            }
            return repeat;
        }



        [Description("得到旺店通的订单详情")]
        public JsonResult  QuyerRetreatDetailBYcode(string code = "")
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (code == "" || code == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            App_Code.GY gy = new App_Code.GY();
            string repeat = RepeatMemo(code);
            //查询旺店通

            dic.Clear();
            dic.Add("src_tid", code);
            //dic.Add("trade_no", code);
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());
            string cmd = CreateParam(dic, true);
            string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/trade_query.php", cmd, Encoding.UTF8);
            string ssx = Regex.Unescape(ret);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string iscode = jsonData["total_count"].ToString();
            if (iscode != "0")
            {
                JsonData jsontrades = jsonData["trades"];
                List<string> userInfo = new List<string>();
                if (jsontrades.Count != 0)
                {

                    JsonData trades = jsontrades[0];
                    //店铺名称
                    string shop_name = trades["shop_name"].ToString();
                    //仓库编码
                    string warehouse_no = trades["warehouse_no"].ToString();
                    //原始订单编号
                    string src_tids = trades["src_tids"].ToString();
                    //下单时间
                    string trade_time = trades["trade_time"].ToString();
                    //付款时间
                    string pay_time = trades["pay_time"].ToString()== "0000-00-00 00:00:00" ? "": trades["pay_time"].ToString();
                    //旺旺号
                    string customer_name = trades["buyer_nick"].ToString();
                    if (string.IsNullOrEmpty(customer_name))
                    {
                        userInfo.Add("buyer_nick");
                    }
                    //订单状态
                    string trade_status = trades["trade_status"].ToString();
                    //收件人姓名
                    string receiver_name = trades["receiver_name"].ToString();
                    if (string.IsNullOrEmpty(receiver_name))
                    {
                        userInfo.Add("receiver_name");
                    }
                    //省
                    string receiver_province = trades["receiver_province"].ToString();
                    //市
                    string receiver_city = trades["receiver_city"].ToString();
                    //区
                    string receiver_district = trades["receiver_district"].ToString();
                    //详细地址
                    string receiver_address = trades["receiver_address"].ToString();
                    if (string.IsNullOrEmpty(receiver_address))
                    {
                        userInfo.Add("receiver_address");
                    }
                    try
                    {
                        //string retreatUserSql = "select * from sales_trade where trade_id=(SELECT trade_id FROM wdt_bak.sales_trade where find_in_set(" + code + ",src_tids))";
                        //List<sales_trade> retreat = db_bak.Database.SqlQuery<sales_trade>(retreatUserSql).ToList();
                        sales_trade retreat = db_bak.sales_trade.FirstOrDefault(a => a.src_tids == code);
                        if (retreat != null)
                        {
                            foreach (var item in userInfo)
                            {
                                switch (item)
                                {
                                    case "buyer_nick":
                                        customer_name = retreat.buyer_nick;
                                        break;
                                    case "receiver_name":
                                        receiver_name = retreat.receiver_name;
                                        break;
                                    case "receiver_address":
                                        receiver_address = retreat.receiver_address;
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //return Json("数据获取失败", JsonRequestBehavior.AllowGet);
                        return Json(new { State = "Fail", Message = "数据获取失败," + ex.Message }, JsonRequestBehavior.AllowGet);
                    }

                    //电话号码
                    string receiver_mobile = trades["receiver_mobile"].ToString();
                    //邮政编码
                    string receiver_zip = trades["receiver_zip"].ToString();
                    //省市县
                    string receiver_area = trades["receiver_area"].ToString();
                    //快递公司编号
                    string logistics_code = trades["logistics_code"].ToString();
                    //快递公司名称
                    string logistics_name = trades["logistics_name"].ToString();
                    //快递单号
                    string logistics_no = trades["logistics_no"].ToString();
                    //快递方式
                    string logistics_type = trades["logistics_type"].ToString();
                    string Retreat_expressName = "";
                    T_Express express = db.T_Express.FirstOrDefault(a => a.wdt_logistics_name == logistics_name);
                    if (express != null)
                    {
                        Retreat_expressName = express.Code;

                    }
                    else
                    {
                        express = db.T_Express.FirstOrDefault(a => a.wdt_logistics_type == logistics_type);
                        if (express != null)
                        {
                            Retreat_expressName = express.Code;

                        }
                    }
                    //买家留言
                    string buyer_message = trades["buyer_message"].ToString();
                    //客服备注
                    string cs_remark = trades["cs_remark"].ToString();
                    //实付金额
                    // string paid = trades["paid"].ToString();
                    //商品详情
                    List<T_RetreatDetails> DetailsList = new List<T_RetreatDetails>();
                    double paid = 0.00;
                    for (int c = 0; c < jsontrades.Count; c++)
                    {
                        paid += double.Parse(jsontrades[c]["paid"].ToString());
                        JsonData goods_list = jsontrades[c]["goods_list"];
                        for (int i = 0; i < goods_list.Count; i++)
                        {
                            T_RetreatDetails DetailsModel = new T_RetreatDetails();
                            string ss = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                            DetailsModel.item_code = ss;
                            DetailsModel.item_name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                            //   double ssds=double.Parse(goods_list[i]["paid"].ToString()) / double.Parse(goods_list[i]["actual_num"].ToString());

                            decimal dec = Convert.ToDecimal(Math.Round(double.Parse(goods_list[i]["share_amount"].ToString()), 2));
                            DetailsModel.amount = (double)dec;//分摊邮费 


                            int qyt = Convert.ToInt32(Convert.ToDecimal(goods_list[i]["actual_num"].ToString()));
                            if (qyt != 0)
                            {
                                DetailsModel.qty = qyt;
                                DetailsModel.price = (double)dec / DetailsModel.qty;
                            }
                            else
                            {
                                DetailsModel.qty = 0;
                                DetailsModel.price = (double)dec;
                            }
                            if (qyt > 0)
                            {
                                DetailsList.Add(DetailsModel);
                            }

                        }
                    }
                    T_Retreat model = new T_Retreat();
                    model.Retreat_OrderNumber = code;
                    model.Retreat_dianpName = shop_name;
                    model.Retreat_wangwang = customer_name;
                    model.Retreat_CustomerName = receiver_name;
                    model.CollectName = receiver_name;
                    model.CollectAddress = receiver_address;
                    model.OrderMoney = paid;
                    model.OrderpaymentMoney = paid;
                    model.repeat = repeat;
                    model.DeliverStatus = Convert.ToInt32(trade_status);

                    model.Retreat_expressNumber = logistics_no;
                    model.Retreat_expressName = Retreat_expressName;

                    var json = new
                    {

                        rows = (from r in DetailsList
                                select new T_RetreatDetails
                                {
                                    item_code = r.item_code,
                                    item_name = r.item_name,
                                    price = r.price,
                                    amount = r.amount,
                                    qty = r.qty,
                                    Simplename = r.Simplename,
                                }).ToArray()
                    };
                    return Json(new { ModelList = model, Json = json }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json("-1", JsonRequestBehavior.AllowGet);

        }
        public ActionResult ViewRetreatOrderQuery()
        {
            return View();
        }



        [Description("访问订单详情表查询方法")]
        public JsonResult QuyerOrderDetailBYcode(string Code = "")
        {
            List<T_OrderDetail> Model = db.T_OrderDetail.Where(a => a.oid == Code).ToList();
            var json = new
            {

                rows = (from r in Model
                        select new T_OrderDetail
                        {
                            item_code = r.item_code,
                            item_name = r.item_name,
                            qty = r.qty,
                            RetreatQty = isnull(r.RetreatQty.ToString()),
                            item_simple_name = r.item_simple_name,
                            price = r.price,
                            amount = r.amount,
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);

        }
        public int isnull(string Qty)
        {
            if (Qty == null || Qty == "")
            {
                return 0;
            }
            return int.Parse(Qty);
        }
        public string Stringisnull(string Qty)
        {
            if (Qty == null || Qty == "")
            {
                return "无";
            }
            return Qty;
        }
        [Description("访问订单详情表离开快递单查询方法")]
        public JsonResult QuyerRetreatDetailsexpressBYcode(string expressNumber = "")
        {
            T_ReturnToStorage RetreatModel = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == expressNumber);
            if (RetreatModel != null)
            {
                int ID = RetreatModel.ID;
                List<T_ReturnToStorageDetails> Model = db.T_ReturnToStorageDetails.Where(a => a.Pid == ID).ToList();
                var json = new
                {

                    rows = (from r in Model
                            select new T_RetreatDetails
                            {
                                item_code = r.item_code,
                                item_name = r.item_name,
                                qty = r.qty,
                                Simplename = r.Simplename,
                            }).ToArray()
                };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);

        }

        [Description("访问退货退款快递单号查询方法")]
        public JsonResult QuyerRetreatsexpressBYcode(string expressNumber = "")
        {
            List<T_Retreat> RetreatModel = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNumber && a.Isdelete == "0" && a.Status != 3).ToList();
            if (RetreatModel.Count > 0)
            {
                return Json("该快递单号已经在退货退款中存在，确认无误可继续保存", JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);

        }
        [Description("查询应移库退货退顾客的数据")]
        public JsonResult RetreatbuliangpCKDetails(string ID, string type)
        {


            List<T_RetreatSorting> Model = db.Database.SqlQuery<T_RetreatSorting>("select * from  T_RetreatSorting where ID in (" + ID + ")").ToList();
            var json = new
            {

                rows = (from r in Model
                        select new T_RetreatSorting
                        {
                            ID = r.ID,
                            ProductCode = r.ProductCode,
                            ProductName = r.ProductName,
                            QualifiedNumber = r.QualifiedNumber,
                            Simplename = Stringisnull(r.Simplename),
                            UnqualifiedNumber = r.UnqualifiedNumber,
                            WarehouseCode = GetWarehouseString(r.WarehouseCode),
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);

        }
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_RetreatDetails> queryData = db.T_RetreatDetails.Where(a => a.Oid == ID);
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

        //编辑获取详情列表  
        public JsonResult EditGetSortingAdd(Lib.GridPager pager, int ID)
        {
            IQueryable<T_RetreatDetails> queryData = db.T_RetreatDetails.Where(a => a.Oid == ID);
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

        //退货退款无头件列表  
        [HttpPost]
        public ContentResult ShowNoheadpartsRetreatList(Lib.GridPager pager, string queryStr)
        {

            IQueryable<T_Retreat> queryData = db.T_Retreat.Where(a => a.isNoheadparts == "1");
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Equals(queryStr) || a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Equals(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = new List<T_Retreat>();
            foreach (var item in queryData)
            {
                T_Retreat i = new T_Retreat();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //我的退货退款列表  
        [HttpPost]
        public ContentResult ShowRetreatList(Lib.GridPager pager, string queryStr, int status, string ReasonType, string RetreatWarehouseList, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_Retreat> queryData = db.T_Retreat.Where(a => (a.Retreat_ApplyName == name || a.Retreat_ApplyName == Nickname) && a.Isdelete == "0");
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Equals(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Equals(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Equals(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Equals(queryStr)));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            if (ReasonType != null && ReasonType != "")
            {
                queryData = queryData.Where(a => a.Retreat_Reason == ReasonType);
            }
            if (RetreatWarehouseList != null && RetreatWarehouseList != "")
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            //根据日期查询
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {

                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.Retreat_date >= startTime && s.Retreat_date <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                queryData = queryData.Where(s => s.Retreat_date >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.Retreat_date <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = new List<T_Retreat>();
            foreach (var item in queryData)
            {
                T_Retreat i = new T_Retreat();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class RetreatgetWshenhe
        {
            public int ID { get; set; }
            public Nullable<System.DateTime> Retreat_date { get; set; }
            public string Retreat_dianpName { get; set; }
            public string Retreat_wangwang { get; set; }
            public string Retreat_CustomerName { get; set; }
            public string Retreat_Remarks { get; set; }
            public Nullable<decimal> Retreat_Shouldjine { get; set; }
            public Nullable<decimal> Retreat_Actualjine { get; set; }
            public string Retreat_OrderNumber { get; set; }
            public string Retreat_expressName { get; set; }
            public string Retreat_expressNumber { get; set; }
            public string Retreat_GoodsSituation { get; set; }
            public string Retreat_Warehouse { get; set; }
            public string Retreat_ApplyName { get; set; }
            public string Retreat_Reason { get; set; }
            public Nullable<double> OrderMoney { get; set; }
            public Nullable<double> OrderpaymentMoney { get; set; }
            public string Retreat_PaymentAccounts { get; set; }
            public string CollectName { get; set; }
            public string CollectAddress { get; set; }
            public string Isdelete { get; set; }
            public string DeleteRemarks { get; set; }
            public string OpenPieceName { get; set; }
            public Nullable<System.DateTime> OpenPieceDate { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public string repeat { get; set; }
            public string receivermobile { get; set; }
            public string isNoheadparts { get; set; }
            public string KDJL { get; set; }
        }

        public partial class Retasd
        {
            public int ID { get; set; }
            public string OrderNumber { get; set; }
            public string ProductCode { get; set; }
            public int Rid { get; set; }
            public int ProductNumber { get; set; }
        }

        //测试方法  
        [HttpPost]
        public ContentResult btnCS(Lib.GridPager pager)
        {
            GY gy = new GY();
            //  string cmd = "v=1.0&sign=&message={\"status\": false}";
            //店铺
            //string ret = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/shoplist","", Encoding.UTF8);
            //JsonData jsonData = null;
            //jsonData = JsonMapper.ToObject(ret);
            //仓库
            //string retss = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/WarehouseList", "", Encoding.UTF8);
            //JsonData jsonDataretss = null;
            //jsonDataretss = JsonMapper.ToObject(retss);

            //商品
            //string cmdSP = "v=1.0&sign=234&message=[   {\"startnum\":\"1\",\"endnum\":\"1999999\"   }]";
            //string retSP = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/product", cmdSP, Encoding.UTF8);
            //JsonData jsonDataretSP = null;
            //jsonDataretss = JsonMapper.ToObject(retSP);

            //写入库存异动
            //string code = "";
            //string Guid = System.Guid.NewGuid().ToString();
            //string whcode = "GX-001";
            //string SP = "6925510609250";
            //string fromno = "21978990167263899";
            //int qty = -5;
            //string cmd = "v=&sign=&message=[{\"aptime\": \"2017/7/22 13:44:59\",\"apuser\": \"明少测试\",\"descri\": \"\",\"fromno\": \"" + fromno + "\",\"fromty\": \"04\",\"id\": \"" + Guid + "\",\"qty\": " + qty + ",\"sku\": \"" + SP + "\",\"whcode\":\"" + whcode + "\",\" isbad\": false,\"isgift\": false } ]";
            //string retssz = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/InventoryChanged", cmd, Encoding.UTF8);
            //JsonData jsonDataretssz = null;
            //jsonDataretssz = JsonMapper.ToObject(retssz);


            ////订单接口
            ////订单交易号
            //string TradeId = "199002045533333";
            ////联系人
            //string Consignee = "明少";
            ////店铺编码
            //string ShopCode = "12";
            ////店铺名称
            //string ShopName = "Jp平价批发湖南好护士";
            ////付款时间
            //DateTime PayDate = DateTime.Now;
            ////买家昵称
            //string buyer_nick = "明少";
            ////支付总金额
            //decimal PayAmount = 1;
            ////会员名称
            //string MemberName = "明少";

            ////数量，不明确什么数量
            //int Quantity = 2;
            ////收货人手机
            //string ConsigneeMobile = "13974844561";
            ////收货人电话
            //string ConsigneeTelephone = "1397484561";
            ////收货人地址
            //string ConsigneeAddress = "湖南省长沙市芙蓉区东岸乡东吨村";
            ////收货人省
            //string ConsigneeProvince = "430000";
            ////收货人市
            //string ConsigneeCity = "430100";
            ////收货人区
            //string ConsigneeCounty = "430102";


            ////商品
            ////商品代码
            //string ProductCode = "20504600026091";
            ////商品名称
            //string ProductName = "仙鹤TDP特定电磁波治疗器";
            ////规格代码
            //string SkuCode = "20504600026091";
            ////规格名称
            //string SkuName = "CQ-27M";
            ////数量
            //decimal QuantitySp = 2;
            ////单价
            //decimal PriceSelling = 2;

            //string cmd = "v=1.0&sign=&message=[{\"TradeId\" : '" + TradeId + "',\"Consignee\" : '" + Consignee + "',\"ShopCode\" : " + ShopCode + ",\"ShopName\" : '" + ShopName + "',\"PayDate\" :'" + PayDate + "' ,\"buyer_nick\" : '" + buyer_nick + "',\"PayAmount\" : " + PayAmount + ",\"MemberName\" : '" + MemberName + "',\"Quantity\" : " + Quantity + ",\"ConsigneeMobile\" : '" + ConsigneeMobile + "',\"ConsigneeTelephone\" :'" + ConsigneeTelephone + "',\"ConsigneeAddress\" :'" + ConsigneeAddress + "',\"ConsigneeProvince\" : '" + ConsigneeProvince + "',\"ConsigneeCity\" :'" + ConsigneeCity + "',\"ConsigneeCounty\" : '" + ConsigneeCounty + "',\"TagName\":\"测试标记\",\"PayDetail\":[{\"TenderCode\" : \"ALIPAY\",\"TenderName\" : \"支付宝\",\"PayTime\" : '"+PayDate+"',\"Amount\" : 0}],\"InvoiceDetail\":[],\"PromotionDetail\":[], \"ProductDetail\" : [{\"ProductCode\" :'" + ProductCode + "',\"ProductName\" : '" + ProductName + "',\"SkuCode\" :'" + SkuCode + "',\"SkuName\" :'" + SkuName + "',\"Quantity\" :" + QuantitySp + ",\"PriceSelling\" : " + PriceSelling + "}],\"BuyerMemo\" : \"测试数据\",\"NeedInvoice\" : false}]";
            //string ret = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/AddSalesOrderOffline", cmd, Encoding.UTF8);
            //JsonData jsonData = null;
            //jsonData = JsonMapper.ToObject(ret);

            //退款单
            ////交易号
            //string TradeId = "199002045533333";
            ////会员代码
            //string MemberCode = "明少";
            ////店铺代码
            //string ShopCode = "12";
            ////备注
            //string Note = "明少测试";
            ////其他费用
            //decimal OtherFees = 0;
            ////退款原因
            //string RefundResonCode = "测试数据";
            ////退款类型
            //string RefundType = "01";
            ////手机号
            //string Mobile = "13974844561";
            ////退款完成状态
            //string RefundState = "03";

            ////商品代码
            //string ProductCode = "20504600026091";
            ////规格代码
            //string SkuCode = "20504600026091";
            ////数量
            //int Quantity = -2;
            ////实际销售单价
            //decimal PriceSold = -22;

            ////付款明细类
            ////费用类型代码
            //string ExpenseCode = "01";
            ////支付方式代码
            //string TenderCode = "ALIPAY";
            ////支付金额
            //decimal PaymentFee = -44;
            ////收款帐号
            //string RevAccount = "测试";
            ////收款人
            //string RevName = "测试";

            //string cmd = "v=1.0&sign=&message=[{\"TradeId\": '" + TradeId + "',\"MemberCode\": '" + MemberCode + "',\"ShopCode\": '" + ShopCode + "',\"Note\": null,\"OtherFees\": 0,\"RefundResonCode\": '" + RefundResonCode + "',\"RefundType\": null,\"Mobile\": '" + Mobile + "',\"RefundState\": '" + RefundState + "',\"RefundProducts\": [{\"ProductCode\": '" + ProductCode + "',\"SkuCode\": '" + SkuCode + "',\"Quantity\": " + Quantity + ",\"PriceSold\":" + PriceSold + "}],\"RefundPayments\": [{\"ExpenseCode\": '" + ExpenseCode + "',\"TenderCode\": '" + TenderCode + "',\"PaymentFee\": " + PaymentFee + ",\"RevAccount\": '" + RevAccount + "',\"RevName\": '" + RevName + "'}]}]";

            //string retssz = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/AddRefundOrder", cmd, Encoding.UTF8);
            //JsonData jsonDataretssz = null;
            //jsonDataretssz = JsonMapper.ToObject(retssz);

            ////退货单（AddReturnedOrder）

            ////退货单ID
            //string Returned_Order_Id=System.Guid.NewGuid().ToString();
            ////交易号
            //string TradeId="";
            ////会员编码
            //string MemberCode="";
            ////退入仓库代码
            //string WareHouseInCode="";
            ////订单编码
            //string SalesOrderCode="";
            ////店铺代码
            //string ShopCode="";
            ////退回快递
            //string RtnExpressName="";
            ////退回快递单号
            //string RtnExpressNo="";
            ////退换货类型编号
            //string ReturnedOrderTypeCode="";
            ////备注
            //string Note="";
            ////换出仓库代码
            //string WareHouseOutCode="";




            //string cmd = "v=1.0&sign=&message=[{\"Returned_Order_Id\":"DCBF0F95-70E3-4D42-AA71-BE4F0FF31D37","TradeId" : "T201410101600","MemberCode" : "ran1437戾","SalesOrderCode" : "SO2014101000000013","ShopCode" : "Shop001","WareHouseInCode" : "01","WareHouseOutCode" : "A01","RtnExpressNo" : "3817713542","RtnExpressName" : "圆通","ReturnedOrderTypeCode":"004","ReturnedOrderState":null,"productdetails" : [{"ProductCode" : "M_ts","ProductName" : "赠品001","SkuCode" : "M_ts001","SkuName" : "红色T_S","Price_Sold" : 20.00000,"Quantity" : 1,"IsGift" : true}],"paymentdetails" : [{"ExpenseCode" : "001","ExpenseName" : "运费","TenderCode" : "ALIPAY","TenderName" : "支付宝","PaymentFee" : 10,"ReceivablesAccount" : ""}],"Note" : "gcctest1"} ]";

            //string retssz = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/AddRefundOrder", cmd, Encoding.UTF8);
            //JsonData jsonDataretssz = null;
            //jsonDataretssz = JsonMapper.ToObject(retssz);


            //string OrderNo="111222333";
            //string StorerNo="01";
            //string WarehouseNo="CK001";
            //string Memo="明少测试";
            //string LocationNo="LS-01-01-01-01";
            //string SKU="20504600026091";
            //string Qty="1";


            //string cmd = "appkey=&appsecret=&clientno=&data=[{OrderNo:\"111222333\",StorerNo: \"01\", WarehouseNo: \"CK001\", Memo: \"明少测试\", Details: [{LocationNo: \"LS-01-01-01-01\", SKU:\"20504600026091\",Qty: 11, Description: \"明少测试\"}]}]";
            //string retssz = gy.DoPostnew("http://36.111.200.145:3333/WebApi/Api/Adjustment/CreateAdjustment", cmd, Encoding.UTF8);

            //调拨单号
            //string asnorder="2311111";
            ////来源单号
            //string fromorder="2222";
            ////经手人编码
            //string headeruser="";
            ////部门编码
            //string deptcode="";
            ////部门名称
            //string deptname="";
            ////调出仓库编码
            //string fromwhcode = "CK002";
            ////调入仓库编号
            //string targetwhcode = "CK001";
            //////创建人编码
            ////string operuserid="";
            //////创建人名称
            ////string operusername="";
            ////创建日期(yyyy/MM/dd HH:mm:ss)
            //string operdate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            ////业务类型
            //string ordertype="1";
            ////商品类型
            //string goodstype="0";
            ////备注
            //string remark="明少测试";
            ////SKU
            //string sku = "20504600026091";
            ////数量
            //int qty = 1;
            ////已去除的字段tocode: null,  MasterName: null,  PlanOutput: 0,operuserid: "创建人编码", operusername: "创建人名称",deptcode: '" + deptcode + "', deptname: '" + deptname + "', headeruser: '" + headeruser + "',
            //string cmd = "appkey=&appsecret=&clientno=&data=[{ asnorder: '" + asnorder + "', fromorder: '" + fromorder + "',  fromwhcode: '" + fromwhcode + "', targetwhcode: '" + targetwhcode + "',  operdate: '" + operdate + "', ordertype:'" + ordertype + "', goodstype: '" + goodstype + "',remark: '" + remark + "',  detail: [{sku: '" + sku + "', qty: " + qty + "}]}] ";
            //string retssz = gy.DoPostnew("http://36.111.200.145:3333/WebApi/Api/Transfer/CreateTransfer", cmd, Encoding.UTF8);




            return Content("");
        }
        //退货退款未审核列表  
        [HttpPost]
        public ContentResult ShowRetreatCheckList(Lib.GridPager pager, string queryStr, string ReasonType, string ExpressType, string RetreatWarehouseList, string queryStrS, string RetreatexpressNameList, string RetreatBackFrom)
        {
            //List<Retasd> RetasdList = db.Database.SqlQuery<Retasd>("select ID,ProductNumber,OrderNumber,ProductCode,(select top 1 id from T_Retreat where Retreat_OrderNumber=a.OrderNumber) as Rid from T_ReceivedAfter a where  OrderNumber is not null and OrderNumber in (select Retreat_OrderNumber from T_Retreat )").ToList();

            //for (int i = 0; i < RetasdList.Count; i++)
            //{
            //    int Rid =int.Parse(RetasdList[i].Rid.ToString());
            //    string itemcode = RetasdList[i].ProductCode;
            //    int ProductNumber = RetasdList[i].ProductNumber;
            //    T_RetreatDetails DetailsModel = db.T_RetreatDetails.SingleOrDefault(a => a.item_code == itemcode && a.Oid == Rid && a.qty == ProductNumber);
            //    if (DetailsModel != null)
            //    {
            //        int ID= RetasdList[i].ID;

            //        T_ReceivedAfter afterModel = db.T_ReceivedAfter.SingleOrDefault(a=>a.ID==ID);
            //        afterModel.IsHandle = 1;
            //        db.Entry<T_ReceivedAfter>(afterModel).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();

            //    }
            //}


            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_RetreatGroup> GroupModel = db.T_RetreatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }

            List<T_RetreatAppRove> ApproveMod = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
            // int[] Arry = new int[ApproveMod.Count];


            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                //  Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
                if (i == 0)
                {
                    arrID += ApproveMod[i].Oid.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Oid.ToString();
                }
            }
            string sql = "select ID,Retreat_date,Retreat_dianpName,Retreat_wangwang,Retreat_CustomerName,Retreat_Remarks,Retreat_Shouldjine,Retreat_Actualjine,Retreat_OrderNumber,Retreat_expressName,Retreat_expressNumber,Retreat_GoodsSituation,Retreat_Warehouse,Retreat_ApplyName,Retreat_Reason,OrderMoney,OrderpaymentMoney,Retreat_PaymentAccounts,CollectName,CollectAddress,Isdelete,DeleteRemarks,OpenPieceName,OpenPieceDate,Status,Step,repeat,receivermobile,isNoheadparts ";//  from T_Retreat r  where Isdelete='0' ";
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            //,isnull((select top 1 TrackRecord_Situation from T_RetreatExpressRecord where Oid=r.ID order by T_RetreatExpressRecord.ID desc ),'') as KDJL
            if (ExpressType != null && ExpressType != "") //对追货情况进行查询时  联查 追货情况
            {
                sql += ",isnull((select top 1 TrackRecord_Situation from T_RetreatExpressRecord where Oid=r.ID order by T_RetreatExpressRecord.ID desc ),'') as KDJL ";

            }
            sql += " from T_Retreat r  where Isdelete='0' ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ") ";
            }
            else
            {
                sql += " and 1=2 ";
            }


            sql += "  and (Status = -1 or Status = 0 or Status = 2) ";


            IQueryable<RetreatgetWshenhe> queryData = db.Database.SqlQuery<RetreatgetWshenhe>(sql).AsQueryable();
            //queryData = from r in db.T_Retreat
            //          where Arry.Contains(r.ID) && r.Isdelete == "0" && (r.Status == -1 || r.Status == 0 || r.Status == 2)
            //          select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Equals(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Equals(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Equals(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Equals(queryStr)));
            }
            if (ReasonType != null && ReasonType != "")
            {
                queryData = queryData.Where(a => a.Retreat_Reason == ReasonType);
            }
            if (ExpressType != null && ExpressType != "")
            {
                if (ExpressType == "wcl")
                {
                    queryData = queryData.Where(a => a.KDJL == "");
                }
                else
                {
                    queryData = queryData.Where(a => a.KDJL == ExpressType);

                }

            }
            if (RetreatBackFrom != null && RetreatBackFrom != "")
            {
                List<T_RetreatPayment> BackFromList = db.T_RetreatPayment.Where(a => a.PaymentAccounts == RetreatBackFrom && a.IsBlending == "1").ToList();

                string dianp = "";
                for (int i = 0; i < BackFromList.Count; i++)
                {
                    if (i == 0)
                    {
                        dianp += BackFromList[i].ShopName;
                    }
                    else
                    {
                        dianp += ",'" + BackFromList[i].ShopName + "'";
                    }

                }
                queryData = queryData.Where(a => dianp.Contains(a.Retreat_dianpName));


            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList);
            }
            if (RetreatWarehouseList != null && RetreatWarehouseList != "")
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            if (queryStrS != null && queryStrS != "")
            {
                queryData = queryData.Where(a => a.Retreat_dianpName.Contains(queryStrS));
            }
            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => (a.Retreat_dianpName == "Tmall快乐老人医疗器械专营店") || (a.Retreat_dianpName == "Tb快乐老人经营产业有限公司") || (a.Retreat_dianpName.Contains("快乐老人医疗器械专营店")));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<RetreatgetWshenhe> list = queryData.OrderBy(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

            // List<RetreatgetWshenhe> list = queryData.ToList();
            foreach (var item in list)
            {
                //RetreatgetWshenhe i = new RetreatgetWshenhe();
                //i = item;
                item.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                item.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                T_User x = db.T_User.FirstOrDefault(s => s.Name.Equals(item.Retreat_ApplyName) || s.Nickname.Equals(item.Retreat_ApplyName));
                if (x != null)
                {
                    item.Retreat_ApplyName = x.Nickname;

                }
                // 
                //list.Add(i);
            }

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //退货退款未审核列表  
        [HttpPost]
        public ContentResult ShowRetreatCheckLists(Lib.GridPager pager, string queryStr)
        {
            string[] OrderNubList = queryStr.Split('\n');
            string OrderNub = "";
            for (int i = 0; i < OrderNubList.Length; i++)
            {
                if (i == 0)
                {
                    OrderNub = "'" + OrderNubList[i] + "'";
                }
                else
                {
                    OrderNub += ",'" + OrderNubList[i] + "'";
                }
            }

            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_RetreatGroup> GroupModel = db.T_RetreatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }

            List<T_RetreatAppRove> ApproveMod = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
            // int[] Arry = new int[ApproveMod.Count];
            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                //  Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
                if (i == 0)
                {
                    arrID += ApproveMod[i].Oid.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Oid.ToString();
                }
            }
            string sql = "select ID,Retreat_date,Retreat_dianpName,Retreat_wangwang,Retreat_CustomerName,Retreat_Remarks,Retreat_Shouldjine,Retreat_Actualjine,Retreat_OrderNumber,Retreat_expressName,Retreat_expressNumber,Retreat_GoodsSituation,Retreat_Warehouse,Retreat_ApplyName,Retreat_Reason,OrderMoney,OrderpaymentMoney,Retreat_PaymentAccounts,CollectName,CollectAddress,Isdelete,DeleteRemarks,OpenPieceName,OpenPieceDate,Status,Step,repeat,receivermobile,isNoheadparts,isnull((select top 1 TrackRecord_Situation from T_RetreatExpressRecord where Oid=r.ID order by T_RetreatExpressRecord.ID desc ),'') as KDJL from T_Retreat r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            if (OrderNub != null && OrderNub != "")
            {
                sql += " and Retreat_OrderNumber in (" + OrderNub + ")";
            }
            IQueryable<RetreatgetWshenhe> queryData = db.Database.SqlQuery<RetreatgetWshenhe>(sql).AsQueryable();
            //queryData = from r in db.T_Retreat
            //          where Arry.Contains(r.ID) && r.Isdelete == "0" && (r.Status == -1 || r.Status == 0 || r.Status == 2)
            //          select r;


            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => (a.Retreat_dianpName == "Tmall快乐老人医疗器械专营店") || (a.Retreat_dianpName == "Tb快乐老人经营产业有限公司") || (a.Retreat_dianpName.Contains("快乐老人医疗器械专营店")));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<RetreatgetWshenhe> list = new List<RetreatgetWshenhe>();
            foreach (var item in queryData)
            {
                RetreatgetWshenhe i = new RetreatgetWshenhe();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                T_User x = db.T_User.SingleOrDefault(s => s.Name.Equals(i.Retreat_ApplyName) || s.Nickname.Equals(i.Retreat_ApplyName));
                if (x != null)
                {
                    i.Retreat_ApplyName = x.Nickname;
                }
                // 
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        public partial class RetreatgetExcel
        {
            public int ID { get; set; }
            public double OrderMoney { get; set; }
            public double OrderpaymentMoney { get; set; }
            public DateTime Retreat_date { get; set; }
            public string Retreat_wangwang { get; set; }
            public string Retreat_Reason { get; set; }
            public Decimal Retreat_Actualjine { get; set; }
            public string Retreat_dianpName { get; set; }
            public string Retreat_PaymentAccounts { get; set; }
            public string Retreat_Remarks { get; set; }
            public string Retreat_OrderNumber { get; set; }
            public string Retreat_expressNumber { get; set; }
            public DateTime shenheSJ { get; set; }
            public string ApproveName { get; set; }
            public string shenhebeizhu { get; set; }
            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }
            public int TotalQty { get; set; }
            public string Retreat_expressName { get; set; }
            public string CollectAddress { get; set; }
            public string OrderSatus { get; set; }

            public int IsRefund { get; set; }

            public int IsReturn { get; set; }

        }



        public FileResult getExcel(string queryStr, string statedate, string EndDate, string RetreatReason)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退货退款导出",
                OperateContent = string.Format("导出excel getExcel 条件 queryStr:{0},statedate:{1},EndDate:{2},RetreatReason:{3}", queryStr, statedate, EndDate, RetreatReason),
                Operater = Nickname,
                OperateTime = DateTime.Now,
                PID = -1
                //"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();
            List<RetreatgetExcel> queryData = null;
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
            string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);



            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            List<T_RetreatGroup> GroupModel = db.T_RetreatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string shenheName = "";
            for (int z = 0; z < GroupModel.Count; z++)
            {
                string[] ss = GroupModel[z].Crew.Split(',');
                for (int i = 0; i < ss.Length; i++)
                {
                    if (i == 0 && z == 0)
                    {
                        shenheName += "'" + ss[i] + "'";
                    }
                    else
                    {
                        shenheName += "," + "'" + ss[i] + "'";
                    }
                }

            }


            //  string sql = "select a.ID   from T_Retreat a  join T_RetreatAppRove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";


            string sql = "select r.ID,isnull(r.OrderMoney,0) as OrderMoney,isnull(r.OrderpaymentMoney,0) as OrderpaymentMoney,Retreat_date,Retreat_wangwang,Retreat_Reason,ISNULL(Retreat_Actualjine,0) as Retreat_Actualjine,Retreat_dianpName,Retreat_PaymentAccounts,Retreat_Remarks,Retreat_OrderNumber,Retreat_expressNumber,(select top 1 ApproveTime from T_RetreatAppRove  where  Status=1 and  ApproveName in (" + shenheName + ") and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenheSJ,(select top 1  ApproveName  from T_RetreatAppRove  where  Status=1 and  ApproveName in (" + shenheName + ") and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as ApproveName,(select top 1  Memo  from T_RetreatAppRove  where  Status=1 and  ApproveName in (" + shenheName + ") and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenhebeizhu,isnull(OrderSatus,'') as OrderSatus,isnull(IsRefund,'') as IsRefund,isnull(IsReturn,'') as IsReturn from T_Retreat r   where r.ID in (select  Oid from T_RetreatAppRove  where  Status=1 and  ApproveName in (" + shenheName + ") and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null)  ";

            queryData = db.Database.SqlQuery<RetreatgetExcel>(sql).ToList();
            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason == RetreatReason).ToList();
            }
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {
                //string csvIds = string.Join(",", ids.ToArray());
                //var ret = db.Database.SqlQuery<T_Retreat>("select * from T_Retreat where ID in (" + csvIds + ") order by ID desc");

                //List<T_Retreat> result = ret.ToList();
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                row1.CreateCell(0).SetCellValue("申请时间");
                row1.CreateCell(1).SetCellValue("旺旺号");
                row1.CreateCell(2).SetCellValue("退款原因");
                row1.CreateCell(3).SetCellValue("实际退款金额");
                row1.CreateCell(4).SetCellValue("店铺");
                row1.CreateCell(5).SetCellValue("出款方式");
                row1.CreateCell(6).SetCellValue("备注");
                row1.CreateCell(7).SetCellValue("审核备注");
                row1.CreateCell(8).SetCellValue("订单号");
                row1.CreateCell(9).SetCellValue("审核日期");
                row1.CreateCell(10).SetCellValue("审核人");
                row1.CreateCell(11).SetCellValue("退货物流单号");
                row1.CreateCell(12).SetCellValue("订单状态");
                row1.CreateCell(13).SetCellValue("需要退款");
                row1.CreateCell(14).SetCellValue("货物退回");

                for (int i = 0; i < queryData.Count; i++)
                {


                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                    rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_date.ToString());
                    rowtemp.CreateCell(1).SetCellValue(queryData[i].Retreat_wangwang);
                    rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_Reason.ToString());
                    rowtemp.CreateCell(3).SetCellValue(double.Parse(queryData[i].Retreat_Actualjine.ToString()));
                    rowtemp.CreateCell(4).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                    rowtemp.CreateCell(5).SetCellValue(queryData[i].Retreat_PaymentAccounts);
                    rowtemp.CreateCell(6).SetCellValue(queryData[i].Retreat_Remarks);
                    rowtemp.CreateCell(7).SetCellValue(queryData[i].shenhebeizhu.ToString());
                    rowtemp.CreateCell(8).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                    rowtemp.CreateCell(9).SetCellValue(queryData[i].shenheSJ.ToString());
                    rowtemp.CreateCell(10).SetCellValue(queryData[i].ApproveName.ToString());
                    rowtemp.CreateCell(12).SetCellValue(queryData[i].OrderSatus.ToString());
                    rowtemp.CreateCell(13).SetCellValue(queryData[i].IsRefund.ToString() == "1" ? "是" : "否");
                    rowtemp.CreateCell(14).SetCellValue(queryData[i].IsReturn.ToString() == "1" ? "是" : "否");
                    if (queryData[i].Retreat_expressNumber != null)
                    {
                        rowtemp.CreateCell(11).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                    }
                    else
                    {
                        rowtemp.CreateCell(11).SetCellValue("");
                    }
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
        }

        public FileResult getExcelManager(string queryStr, string statedate, string EndDate, string RetreatReason)
        {

            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退货退款导出",
                OperateContent = string.Format("导出excel getExcelManager 条件 queryStr:{0},statedate:{1},EndDate:{2},RetreatReason:{3}", queryStr, statedate, EndDate, RetreatReason),
                Operater = Nickname,
                OperateTime = DateTime.Now,
                PID = -1
                //"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();

            List<RetreatgetExcel> queryData = null;
            int temID = 0;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
            string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //  string sql = "select a.ID   from T_Retreat a  join T_RetreatAppRove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";

            string sql = "select r.ID,isnull(r.OrderMoney,0) as OrderMoney,r.Retreat_expressName,r.CollectAddress,isnull(r.OrderpaymentMoney,0) as OrderpaymentMoney, item_code,item_name,qty,Retreat_date,Retreat_wangwang,Retreat_Reason,ISNULL(Retreat_Actualjine,0) as Retreat_Actualjine,Retreat_dianpName,Retreat_PaymentAccounts,Retreat_Remarks,Retreat_OrderNumber,Retreat_expressNumber,(select top 1 ApproveTime from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenheSJ,(select top 1  ApproveName  from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as ApproveName,(select top 1  Memo  from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenhebeizhu from T_Retreat r inner join T_RetreatDetails rd on rd.Oid=r.ID  where r.ID in (select  Oid from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null)  order by r.ID  ";
            queryData = db.Database.SqlQuery<RetreatgetExcel>(sql).ToList();
            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason != null && a.Retreat_Reason.Contains(RetreatReason)).ToList();
            }
            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => a.Retreat_Reason == "").ToList();
            }
            //if (!string.IsNullOrEmpty(queryStr))
            //{
            //    queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Contains(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Contains(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Contains(queryStr)));
            //}
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {

                //string csvIds = string.Join(",", ids.ToArray());
                //var ret = db.Database.SqlQuery<T_Retreat>("select * from T_Retreat where ID in (" + csvIds + ") order by ID desc");

                //List<T_Retreat> result = ret.ToList();
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                row1.CreateCell(0).SetCellValue("申请时间");
                row1.CreateCell(1).SetCellValue("旺旺号");
                row1.CreateCell(2).SetCellValue("退款原因");
                row1.CreateCell(3).SetCellValue("实际退款金额");
                row1.CreateCell(4).SetCellValue("店铺");
                row1.CreateCell(5).SetCellValue("出款方式");
                row1.CreateCell(6).SetCellValue("备注");
                row1.CreateCell(7).SetCellValue("审核备注");
                row1.CreateCell(8).SetCellValue("订单号");
                row1.CreateCell(9).SetCellValue("审核日期");
                row1.CreateCell(10).SetCellValue("退货物流单号");
                row1.CreateCell(11).SetCellValue("商品代码");
                row1.CreateCell(12).SetCellValue("商品名称");
                row1.CreateCell(13).SetCellValue("数量");
                row1.CreateCell(14).SetCellValue("物流公司");
                row1.CreateCell(15).SetCellValue("退货地址");
                row1.CreateCell(16).SetCellValue("订单金额");
                row1.CreateCell(17).SetCellValue("实付金额");

                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);

                    if (temID == queryData[i].ID)
                    {
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_date.ToString());
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].Retreat_wangwang);
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_Reason.ToString());

                        rowtemp.CreateCell(4).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].Retreat_PaymentAccounts);
                        rowtemp.CreateCell(6).SetCellValue(queryData[i].Retreat_Remarks);
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].shenhebeizhu);
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                        rowtemp.CreateCell(9).SetCellValue(queryData[i].shenheSJ.ToString());
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                        rowtemp.CreateCell(11).SetCellValue(queryData[i].item_code.ToString());
                        rowtemp.CreateCell(12).SetCellValue(queryData[i].item_name.ToString());
                        rowtemp.CreateCell(13).SetCellValue(queryData[i].qty.ToString());
                        rowtemp.CreateCell(14).SetCellValue(Com.GetExpressName(queryData[i].Retreat_expressName.ToString()));
                        rowtemp.CreateCell(15).SetCellValue(queryData[i].CollectAddress);
                    }
                    else
                    {
                        temID = queryData[i].ID;
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_date.ToString());
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].Retreat_wangwang);
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_Reason.ToString());
                        rowtemp.CreateCell(3).SetCellValue(double.Parse(queryData[i].Retreat_Actualjine.ToString()));
                        rowtemp.CreateCell(4).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].Retreat_PaymentAccounts);
                        rowtemp.CreateCell(6).SetCellValue(queryData[i].Retreat_Remarks);
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].shenhebeizhu);
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                        rowtemp.CreateCell(9).SetCellValue(queryData[i].shenheSJ.ToString());
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                        rowtemp.CreateCell(11).SetCellValue(queryData[i].item_code.ToString());
                        rowtemp.CreateCell(12).SetCellValue(queryData[i].item_name.ToString());
                        rowtemp.CreateCell(13).SetCellValue(queryData[i].qty.ToString());
                        rowtemp.CreateCell(14).SetCellValue(Com.GetExpressName(queryData[i].Retreat_expressName.ToString()));
                        rowtemp.CreateCell(15).SetCellValue(queryData[i].CollectAddress);
                        rowtemp.CreateCell(16).SetCellValue(queryData[i].OrderMoney.ToString());
                        rowtemp.CreateCell(17).SetCellValue(queryData[i].OrderpaymentMoney.ToString());
                    }
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
        }

        public partial class RetreatgetExcelManager
        {
            public int ID { get; set; }
            public Double OrderMoney { get; set; }
            public Double OrderpaymentMoney { get; set; }
            public DateTime Retreat_date { get; set; }
            public string Retreat_wangwang { get; set; }
            public string Retreat_Reason { get; set; }
            public Decimal Retreat_Actualjine { get; set; }
            public string Retreat_dianpName { get; set; }
            public string Retreat_PaymentAccounts { get; set; }
            public string Retreat_Remarks { get; set; }
            public string Retreat_OrderNumber { get; set; }
            public string Retreat_expressNumber { get; set; }
            public DateTime shenheSJ { get; set; }
            public string ApproveName { get; set; }
            public string shenhebeizhu { get; set; }
            public string item_code { get; set; }
            public string item_name { get; set; }
            public int qty { get; set; }
            public int TotalQty { get; set; }

            public Double price { get; set; }
            public Double amount { get; set; }

            public string Retreat_expressName { get; set; }
            public string CollectAddress { get; set; }
            public string OrderSatus { get; set; }
            public int IsRefund { get; set; }
            public int IsReturn { get; set; }

            public string Retreat_ApplyName { get; set; }


        }
        public FileResult ReportgetExcelManager(string queryStr, string statedate, string EndDate, string RetreatReason)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退货退款导出",
                OperateContent = string.Format("导出excel ReportgetExcelManager 条件 queryStr:{0},statedate:{1},EndDate:{2},RetreatReason:{3}", queryStr, statedate, EndDate, RetreatReason),
                Operater = Nickname,
                OperateTime = DateTime.Now,
                PID = -1
                //"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();
            List<RetreatgetExcelManager> queryData = null;
            int temID = 0;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
            string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //  string sql = "select a.ID   from T_Retreat a  join T_RetreatAppRove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";

            string sql = "select r.ID,isnull(r.OrderMoney,0) as OrderMoney,r.Retreat_expressName,price,amount,(case when totalqty>0 then  totalqty else qty end ) as TotalQty,r.CollectAddress,isnull(r.OrderpaymentMoney,0) as OrderpaymentMoney, item_code,item_name,qty,Retreat_date,Retreat_wangwang,Retreat_Reason,ISNULL(Retreat_Actualjine,0) as Retreat_Actualjine,ISNULL(Retreat_ApplyName,0) as Retreat_ApplyName,Retreat_dianpName,isnull(Retreat_PaymentAccounts,'无'),isnull(Retreat_Remarks,'无') as Retreat_Remarks,Retreat_OrderNumber,Retreat_expressNumber,(select top 1 ApproveTime from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenheSJ,(select top 1  ApproveName  from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as ApproveName,(select top 1  Memo  from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null and Oid=r.ID) as shenhebeizhu,isnull(OrderSatus,'') as OrderSatus,isnull(IsRefund,'') as IsRefund,isnull(IsReturn,'') as IsReturn from T_Retreat r inner join T_RetreatDetails rd on rd.Oid=r.ID  where r.ID in (select  Oid from T_RetreatAppRove  where  Status=1 and ApproveDName='财务' and ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and ApproveTime is not null)  order by r.ID  ";
            queryData = db.Database.SqlQuery<RetreatgetExcelManager>(sql).ToList();
            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason != null && a.Retreat_Reason.Contains(RetreatReason)).ToList();
            }
            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => a.Retreat_Reason == "").ToList();
            }
            //if (!string.IsNullOrEmpty(queryStr))
            //{
            //    queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Contains(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Contains(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Contains(queryStr)));
            //}
            //linq in 
            List<string> ids = new List<string>();
            foreach (var item in queryData)
            {
                ids.Add(item.ToString());
            }
            if (queryData.Count > 0)
            {

                //string csvIds = string.Join(",", ids.ToArray());
                //var ret = db.Database.SqlQuery<T_Retreat>("select * from T_Retreat where ID in (" + csvIds + ") order by ID desc");

                //List<T_Retreat> result = ret.ToList();
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                row1.CreateCell(0).SetCellValue("申请时间");
                row1.CreateCell(1).SetCellValue("财务审核时间");
                row1.CreateCell(2).SetCellValue("店铺");
                row1.CreateCell(3).SetCellValue("订单号");
                row1.CreateCell(4).SetCellValue("订单实付金额");
                row1.CreateCell(5).SetCellValue("旺旺号");
                row1.CreateCell(6).SetCellValue("商品代码");
                row1.CreateCell(7).SetCellValue("商品名称");
                row1.CreateCell(8).SetCellValue("数量");
                row1.CreateCell(9).SetCellValue("退款原因");
                row1.CreateCell(10).SetCellValue("货品退款总金额");
                row1.CreateCell(11).SetCellValue("货品退款均摊单价");
                row1.CreateCell(12).SetCellValue("出款方式");
                row1.CreateCell(13).SetCellValue("备注");
                row1.CreateCell(14).SetCellValue("审核备注");
                row1.CreateCell(15).SetCellValue("退货物流单号");
                row1.CreateCell(16).SetCellValue("物流公司");
                row1.CreateCell(17).SetCellValue("退货地址");
                row1.CreateCell(18).SetCellValue("审核财务");
                row1.CreateCell(19).SetCellValue("单品分摊总价");
                row1.CreateCell(20).SetCellValue("订单状态");
                row1.CreateCell(21).SetCellValue("需要退款");
                row1.CreateCell(22).SetCellValue("货物退回");
                for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);

                    if (temID == queryData[i].ID)
                    {
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_date.ToString());
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].shenheSJ.ToString());
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                        rowtemp.CreateCell(3).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                        rowtemp.CreateCell(4).SetCellValue(queryData[i].OrderMoney);
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].Retreat_wangwang);
                        rowtemp.CreateCell(6).SetCellValue(queryData[i].item_code.ToString());
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].item_name.ToString());
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].qty.ToString());
                        rowtemp.CreateCell(9).SetCellValue(queryData[i].Retreat_Reason.ToString());
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].Retreat_Actualjine.ToString());
                        //计算11的单价 货品退款均摊单价
                        decimal PriceZs = 0;
                        decimal totalPriceZ = 0;
                        if (queryData[i].TotalQty == 0)
                        {
                            queryData[i].TotalQty = queryData[i].qty;
                        }
                        decimal AmountDetail = (decimal)queryData.Where(a => a.ID == temID).Sum(a => (a.amount / a.TotalQty * a.qty));//price应为 amount
                        if (AmountDetail != 0 && queryData[i].TotalQty != 0)
                        {
                            decimal PriceZ = (decimal)queryData[i].amount / queryData[i].TotalQty / AmountDetail * queryData[i].Retreat_Actualjine;
                            PriceZs = PriceZ;
                            totalPriceZ = PriceZ * queryData[i].qty;
                        }
                        rowtemp.CreateCell(11).SetCellValue(PriceZs.ToString("0.00"));
                        if (queryData[i].Retreat_PaymentAccounts != null)
                        {
                            rowtemp.CreateCell(12).SetCellValue(queryData[i].Retreat_PaymentAccounts.ToString());
                        }
                        else
                        {
                            rowtemp.CreateCell(12).SetCellValue("");
                        }
                        //rowtemp.CreateCell(12).SetCellValue(queryData[i].Retreat_PaymentAccounts.ToString());
                        rowtemp.CreateCell(13).SetCellValue(queryData[i].Retreat_Remarks.ToString());
                        rowtemp.CreateCell(14).SetCellValue(queryData[i].shenhebeizhu.ToString());
                        rowtemp.CreateCell(15).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                        rowtemp.CreateCell(16).SetCellValue(Com.GetExpressName(queryData[i].Retreat_expressName.ToString()));
                        rowtemp.CreateCell(17).SetCellValue(queryData[i].CollectAddress.ToString());
                        rowtemp.CreateCell(18).SetCellValue(queryData[i].ApproveName.ToString());
                        rowtemp.CreateCell(19).SetCellValue(totalPriceZ.ToString("0.00"));
                        rowtemp.CreateCell(20).SetCellValue(queryData[i].OrderSatus.ToString());

                        rowtemp.CreateCell(21).SetCellValue(queryData[i].IsRefund.ToString() == "1" ? "是" : "否");
                        rowtemp.CreateCell(22).SetCellValue(queryData[i].IsReturn.ToString() == "1" ? "是" : "否");
                        rowtemp.CreateCell(23).SetCellValue(queryData[i].Retreat_ApplyName.ToString());
                    }
                    else
                    {
                        temID = queryData[i].ID;
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].Retreat_date.ToString());
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].shenheSJ.ToString());
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].Retreat_dianpName.ToString());
                        rowtemp.CreateCell(3).SetCellValue(queryData[i].Retreat_OrderNumber.ToString());
                        rowtemp.CreateCell(4).SetCellValue(queryData[i].OrderMoney);
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].Retreat_wangwang);
                        rowtemp.CreateCell(6).SetCellValue(queryData[i].item_code.ToString());
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].item_name.ToString());
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].qty.ToString());
                        rowtemp.CreateCell(9).SetCellValue(queryData[i].Retreat_Reason.ToString());
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].Retreat_Actualjine.ToString());

                        //计算11的单价
                        decimal PriceZs = 0;
                        decimal totalPriceZ = 0;
                        if (queryData[i].qty != 0)
                        {

                        }
                        if (queryData[i].TotalQty == 0)
                        {
                            queryData[i].TotalQty = queryData[i].qty;
                        }
                        decimal AmountDetail = Convert.ToDecimal(queryData.Where(a => a.ID == temID).Sum(a => (a.amount / (a.TotalQty) * a.qty)));
                        //price应为 amount
                        if (AmountDetail != 0 && queryData[i].TotalQty != 0)
                        {
                            // decimal PriceZ = decimal.Parse(queryData[i].price.ToString()) / decimal.Parse(queryData[i].OrderMoney.ToString()) * decimal.Parse(queryData[i].Retreat_Actualjine.ToString());
                            //decimal PriceZs = PriceZ / queryData[i].qty;
                            decimal PriceZ = (decimal)queryData[i].amount / queryData[i].TotalQty / AmountDetail * queryData[i].Retreat_Actualjine;
                            PriceZs = PriceZ;
                            totalPriceZ = PriceZ * queryData[i].qty;


                        }
                        rowtemp.CreateCell(11).SetCellValue(PriceZs.ToString("0.00"));
                        if (queryData[i].Retreat_PaymentAccounts != null)
                        {
                            rowtemp.CreateCell(12).SetCellValue(queryData[i].Retreat_PaymentAccounts.ToString());
                        }
                        else
                        {
                            rowtemp.CreateCell(12).SetCellValue("");
                        }
                        rowtemp.CreateCell(13).SetCellValue(queryData[i].Retreat_Remarks.ToString());
                        rowtemp.CreateCell(14).SetCellValue(queryData[i].shenhebeizhu.ToString());
                        rowtemp.CreateCell(15).SetCellValue(queryData[i].Retreat_expressNumber.ToString());
                        rowtemp.CreateCell(16).SetCellValue(Com.GetExpressName(queryData[i].Retreat_expressName.ToString()));
                        rowtemp.CreateCell(17).SetCellValue(queryData[i].CollectAddress.ToString());
                        rowtemp.CreateCell(18).SetCellValue(queryData[i].ApproveName.ToString());
                        rowtemp.CreateCell(19).SetCellValue(totalPriceZ.ToString("0.00"));
                        rowtemp.CreateCell(20).SetCellValue(queryData[i].OrderSatus.ToString());
                        rowtemp.CreateCell(21).SetCellValue(queryData[i].IsRefund.ToString() == "1" ? "是" : "否");
                        rowtemp.CreateCell(22).SetCellValue(queryData[i].IsReturn.ToString() == "1" ? "是" : "否");
                        rowtemp.CreateCell(23).SetCellValue(queryData[i].Retreat_ApplyName.ToString());
                    }
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
        }
        //退货退款已审核列表  
        [HttpPost]
        public ContentResult GetRetreatCheckenList(Lib.GridPager pager, string queryStr, string statedate, string EndDate, string RetreatReason)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_RetreatAppRove> ApproveMod = db.T_RetreatAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Retreat> queryData = from r in db.T_Retreat
                                              where Arry.Contains(r.ID) && r.Isdelete == "0"
                                              select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Equals(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Equals(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Equals(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Equals(queryStr)));
            }
            if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime start = DateTime.Parse(statedate + " 00:00:00");
                queryData = queryData.Where(s => s.Retreat_date >= start);
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime end = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.Retreat_date <= end);
            }
            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason == RetreatReason);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = new List<T_Retreat>();
            foreach (var item in queryData)
            {
                T_Retreat i = new T_Retreat();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                T_User x = db.T_User.FirstOrDefault(s => s.Name.Equals(i.Retreat_ApplyName) || s.Nickname.Equals(i.Retreat_ApplyName));
                if (x != null)
                {
                    i.Retreat_ApplyName = x.Nickname;
                }
                //
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //退货退款管理列表  
        [HttpPost]
        public ContentResult GetRetreatList(Lib.GridPager pager, string queryStr, string RetreatReason, string statedate, string EndDate, string selStatus, string RetreatWarehouseList)
        {
            //string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            //string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            //if (!string.IsNullOrEmpty(statedate))
            //{
            //    sdate = statedate + " 00:00:00";
            //}
            //if (!string.IsNullOrEmpty(EndDate))
            //{
            //    edate = EndDate + " 23:59:59";
            //}
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_Retreat> queryData = from r in db.T_Retreat
                                              where r.Isdelete == "0"
                                              select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber.Equals(queryStr)) || (a.Retreat_CustomerName.Equals(queryStr)) || (a.Retreat_wangwang.Equals(queryStr)) || (a.Retreat_OrderNumber.Equals(queryStr)));
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }

            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason != null && a.Retreat_Reason.Equals(RetreatReason));
            }
            if (!string.IsNullOrEmpty(selStatus) && selStatus != "-2")
            {
                int status = int.Parse(selStatus);
                queryData = queryData.Where(a => a.Status == status);
            }
            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => (a.Retreat_dianpName == "Tmall快乐老人医疗器械专营店") || (a.Retreat_dianpName == "Tb快乐老人经营产业有限公司") || (a.Retreat_dianpName.Contains("快乐老人医疗器械专营店")));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = queryData.ToList();
            foreach (var item in list)
            {
                // T_Retreat i = new T_Retreat();
                // i = item;
                item.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                item.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                T_User x = db.T_User.SingleOrDefault(s => s.Name.Equals(item.Retreat_ApplyName));
                if (x != null)
                {
                    item.Retreat_ApplyName = x.Nickname;
                }
                //

            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        //退货退款分拣列表  
        [HttpPost]
        public ContentResult GetRetreatsortingList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);



            IQueryable<T_Retreat> queryData = from r in db.T_Retreat

                                              where r.Isdelete == "0" && r.isSorting == 0 && r.SortingName == Nickname

                                              where r.Isdelete == "0" && r.isSorting == 0 && r.Status == 1

                                              select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = new List<T_Retreat>();
            foreach (var item in queryData)
            {
                T_Retreat i = new T_Retreat();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //退货退款库存列表  
        [HttpPost]
        public ContentResult GetRetreatWarehouseList(Lib.GridPager pager, string queryStr, string RetreatWarehouseList, string status)
        {

            IQueryable<T_RetreatSorting> queryData = db.T_RetreatSorting.Where(a => a.QualifiedNumber != 0 || a.UnqualifiedNumber != 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.WarehouseCode != null && a.WarehouseCode.Contains(queryStr)) || (a.ProductCode != null && a.ProductCode.Contains(queryStr)) || (a.ProductName != null && a.ProductName.Contains(queryStr)));
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.WarehouseCode == RetreatWarehouseList);
            }
            if (status == "1")
            {

                queryData = queryData.Where(a => a.QualifiedNumber > 0);
            }
            if (status == "2")
            {
                queryData = queryData.Where(a => a.UnqualifiedNumber > 0);
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_RetreatSorting> list = new List<T_RetreatSorting>();
            foreach (var item in queryData)
            {
                T_RetreatSorting i = new T_RetreatSorting();
                i = item;
                //i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.WarehouseCode = GetWarehouseString(item.WarehouseCode);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        //退货退款出库记录列表  
        [HttpPost]
        public ContentResult GetRetreatWarehouseRecordList(Lib.GridPager pager, string queryStr, string warehouse)
        {

            IQueryable<T_RetreatSortingDelivering> queryData = db.T_RetreatSortingDelivering.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.ProductCode != null && a.ProductCode.Contains(queryStr)) || (a.ProductName != null && a.ProductName.Contains(queryStr)));
            }
            if (!string.IsNullOrWhiteSpace(warehouse))
            {
                queryData = queryData.Where(a => a.WarehouseCode == warehouse);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_RetreatSortingDelivering> list = new List<T_RetreatSortingDelivering>();
            foreach (var item in queryData)
            {
                T_RetreatSortingDelivering i = new T_RetreatSortingDelivering();
                i = item;
                //i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.WarehouseCode = GetWarehouseString(item.WarehouseCode);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion
        #region Post提交逻辑判断
        //虚拟删除付款单记录 
        [HttpPost]
        [Description("退货退款删除")]
        public JsonResult DeleteRetreatFinance(int ID)
        {
            int i = 0;
            List<T_RetreatAppRove> approveList = db.T_RetreatAppRove.Where(s => s.Oid == ID && s.Status == 1 && s.ApproveDName == "财务").ToList();
            if (approveList.Count > 0)
            {
                i = 0;
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            T_Retreat model = db.T_Retreat.Find(ID);
            model.Isdelete = "1";
            db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;
            i = db.SaveChanges();
            //if (i > 0)
            //{

            //    List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_OrderNumber == model.Retreat_OrderNumber && a.Isdelete == "0" && a.Status != 3).ToList();
            //    if (RetreatList.Count == 0)
            //    {
            //        T_OrderList orderModel = db.T_OrderList.Find(model.OrderId);
            //        orderModel.Status_Retreat = 0;
            //        db.Entry<T_OrderList>(orderModel).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //    }
            //}

            //ModularByZP();

            return Json(i, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Description("退货退款作废")]
        public JsonResult VoidRetreat(int ID)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            int i = 0;
            List<T_RetreatAppRove> approveList = db.T_RetreatAppRove.Where(s => s.Oid == ID && s.Status == 1 && s.ApproveDName == "财务").ToList();
            if (approveList.Count > 0)
            {
                i = 0;
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            T_Retreat model = db.T_Retreat.Find(ID);
            model.Status = 3;
            db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;

            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退货退款作废",
                OperateContent = string.Format("VoidRetreat 条件 ID:{0},Retreat_OrderNumbe:{1}", model.ID, model.Retreat_OrderNumber),
                Operater = Nickname,
                OperateTime = DateTime.Now,
                PID = -1
                //"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();

            i = db.SaveChanges();



            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //新增保存
        [ValidateInput(false)]
        [HttpPost]
        [Description("退货退款新增保存")]
        public JsonResult RetreatAddSave(T_Retreat model, string jsonStr, string ischongfu, string Expense)
        {
            string code = model.Retreat_OrderNumber;
            model.repeat = "";
            string repeat = "";
            List<T_Retreat> modelLists = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0").ToList();
            if (modelLists.Count > 0)
            {
                repeat += "退货退款记录重复，";
            }
            List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 2).ToList();
            if (cash.Count > 0)
            {
                repeat += "有返现记录重复，";
            }
            List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (Reissue.Count > 0)
            {
                repeat += "有补发记录重复，";
            }
            List<T_ExchangeCenter> ExchangeCenter = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (ExchangeCenter.Count > 0)
            {
                repeat += "有换货记录重复，";
            }
            List<T_Intercept> Intercept = db.T_Intercept.Where(a => a.OrderNumber.Equals(code.Trim()) && a.IsDelete == 0 && a.Status != 2).ToList();
            if (Intercept.Count > 0)
            {
                repeat += "拦截模块有记录，";
            }

            int MainSaved = 0;
            int MainID = -1;
            List<T_RetreatDetails> details = App_Code.Com.Deserialize<T_RetreatDetails>(jsonStr);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string expressNumber = model.Retreat_expressNumber.Trim();
            string orderid = model.Retreat_OrderNumber;
            T_Retreat modelMain = db.T_Retreat.FirstOrDefault(a => a.Retreat_expressNumber == expressNumber && a.Retreat_OrderNumber == orderid && a.Isdelete == "0" && a.Status != 3);
            if (modelMain != null && modelMain.isNoheadparts == "1")
            {
                //如果已经存在，比对明细内容
                MainID = modelMain.ID;
                List<T_RetreatDetails> RetreatDetailsList = db.T_RetreatDetails.Where(a => a.Oid == MainID).ToList();
                if (RetreatDetailsList.Count != details.Count)
                {
                    return Json(new { State = "Fail", Message = "添加货物与仓库签收货物不相等" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    for (int z = 0; z < details.Count; z++)
                    {
                        if (details[z].qty == 0)
                        {
                            return Json(new { State = "Faile", Message = "产品数量不允许为空" }, JsonRequestBehavior.AllowGet);
                        }
                        List<T_RetreatDetails> ss = details.Where(a => a.item_code == RetreatDetailsList[z].item_code).ToList();
                        if (ss.Count == 0)
                        {
                            return Json(new { State = "Faile", Message = "产品代码：" + RetreatDetailsList[z].item_code + " 与仓库实收不一致，请找顾客确认" }, JsonRequestBehavior.AllowGet);

                        }

                    }
                }
                //对比完之后 清空明细表
                foreach (var item in RetreatDetailsList)
                {
                    db.T_RetreatDetails.Remove(item);
                }

                modelMain.Retreat_date = DateTime.Now;
                modelMain.Retreat_dianpName = model.Retreat_dianpName;
                modelMain.Retreat_wangwang = model.Retreat_wangwang;
                modelMain.Retreat_CustomerName = model.Retreat_CustomerName;
                modelMain.Retreat_Remarks = model.Retreat_Remarks;
                modelMain.Retreat_Shouldjine = model.Retreat_Shouldjine;
                modelMain.Retreat_OrderNumber = model.Retreat_OrderNumber;
                modelMain.Retreat_GoodsSituation = model.Retreat_GoodsSituation;
                modelMain.Retreat_Reason = model.Retreat_Reason;
                modelMain.OrderMoney = model.OrderMoney;
                modelMain.OrderpaymentMoney = model.OrderpaymentMoney;
                modelMain.Retreat_PaymentAccounts = model.Retreat_PaymentAccounts;
                modelMain.Retreat_ApplyName = name;
                modelMain.repeat = ischongfu;
                modelMain.isNoheadparts = "1";
                db.Entry<T_Retreat>(modelMain).State = System.Data.Entity.EntityState.Modified;
                foreach (var item in details)
                {
                    item.Oid = MainID;
                    db.T_RetreatDetails.Add(item);
                }
                try
                {
                    MainSaved = db.SaveChanges();
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Fail", Message = "modelMain失败," + ex.Message }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {

                model.repeat = repeat;
                model.Retreat_date = DateTime.Now;
                model.Retreat_ApplyName = name;
                model.Status = -1;
                model.Retreat_expressNumber = model.Retreat_expressNumber.Trim();
                model.Step = 0;
                model.Isdelete = "0";
                model.isSorting = 0;
                model.isNoheadparts = "0";
                //主表保存
                db.T_Retreat.Add(model);

                try
                {
                    MainSaved = db.SaveChanges();
                    MainID = model.ID;
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Fail", Message = "modelMain失败," + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in details)
                {
                    item.Oid = MainID;
                    db.T_RetreatDetails.Add(item);
                }
                //明细表保存
                try
                {
                    int detail = db.SaveChanges();
                }
                catch (Exception ex)
                {
                    db.T_Retreat.Remove(model);
                    db.SaveChanges();
                    return Json(new { State = "Fail", Message = "detail保存失败,主记录删除" + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            if (MainSaved > 0)
            {
                //需要新增审核流程表的记录
                T_RetreatAppRove RetreatAppRove = new T_RetreatAppRove();
                //得到退货退款原因
                string Reason = model.Retreat_Reason;
                //根据原因去查询原因类型
                T_RetreatReason ReasonModel = db.T_RetreatReason.SingleOrDefault(a => a.RetreatReason == Reason);
                //得到原因类型
                string Type = ReasonModel.Type;
                //去查询原因类型配置表
                T_RetreatConfig configModel = db.T_RetreatConfig.SingleOrDefault(a => a.Reason == Type && a.Step == 0);
                //判断原因类型配置表的审核人名称是否为空,如果不为空直接给审核人名称，如果为空证明是部门多人审核，就进去给部门名称
                RetreatAppRove.ApproveDName = configModel.Type;
                if (configModel.Name == null || configModel.Name == "")
                {
                    //如果是部门主管就去查询人员基础信息表，找到对应的部门主管，如果没有部门主管就默认成风审核
                    if (configModel.Type == "部门主管")
                    {
                        if (Expense == "部门主管")
                        {
                            T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name || a.Nickname == name);
                            string depart = userModel.DepartmentId;
                            T_User userDepartModel = db.T_User.SingleOrDefault(a => a.DepartmentId == depart && a.IsManagers == "1");
                            if (userDepartModel != null)
                            {
                                RetreatAppRove.ApproveName = userDepartModel.Nickname;
                            }
                            else
                            {
                                RetreatAppRove.ApproveName = "成风";
                            }
                        }
                        else
                        {
                            RetreatAppRove.ApproveName = Expense;
                        }
                    }
                    else
                    {
                        //如果不是部门主管以外的部门，都是由部门作为审核人，可以到退货退款人员配置表中查询详细有那些人员（T_RetreatGroup）
                        RetreatAppRove.ApproveName = configModel.Type;
                    }
                }
                else
                {
                    //如果审核人员不为空，就直接默认
                    RetreatAppRove.ApproveName = configModel.Name;
                }
                RetreatAppRove.Status = -1;
                RetreatAppRove.Type = Type;
                RetreatAppRove.Memo = "";
                RetreatAppRove.Oid = MainID;
                db.T_RetreatAppRove.Add(RetreatAppRove);
                try
                {
                    db.SaveChanges();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Fail", Message = "审核记录保存失败" + ex.Message }, JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                return Json(new { State = "Fail", Message = "主记录保存失败" }, JsonRequestBehavior.AllowGet);
            }

        }
        public void ModularByZP()
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

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_RetreatAppRove where Oid in (select ID from T_Retreat where  Isdelete='0'  and (Status = -1 or Status = 0 )   )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
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
            string RejectNumberSql = "select Retreat_ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Retreat where Status='2' and Isdelete=0 GROUP BY Retreat_ApplyName ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

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

        //新增保存
        [HttpPost]
        [Description("无头件新增保存")]
        public JsonResult RetreatNoheadpartsAddSave(T_Retreat model, string detailList)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                try
                {
                    string expressNumber = model.Retreat_expressNumber;
                    List<T_Retreat> RmodelList = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNumber).ToList();
                    if (RmodelList.Count > 0)
                    {
                        return Json(new { State = "Faile", Message = "快递单号已存在退款退款中，不允许添加无头件" }, JsonRequestBehavior.AllowGet);
                    }

                    List<T_RetreatDetails> details = App_Code.Com.Deserialize<T_RetreatDetails>(detailList);
                    //主表保存
                    model.Retreat_date = DateTime.Now;
                    model.repeat = "无";
                    model.Retreat_ApplyName = name;
                    model.Status = -1;
                    model.Step = 0;
                    model.Isdelete = "0";
                    model.isSorting = 0;
                    model.isNoheadparts = "1";
                    model.SortingName = Nickname;
                    db.T_Retreat.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        foreach (var item in details)
                        {
                            item.Oid = model.ID;
                            item.price = 0;
                            item.amount = 0;
                            if (item.Simplename == "" || item.Simplename == null)
                            {
                                item.Simplename = "无";
                            }
                            db.T_RetreatDetails.Add(item);
                        }
                        db.SaveChanges();

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


        //编辑保存
        [HttpPost]
        [Description("退货退款编辑保存")]
        public JsonResult ViewRetreatEditSave(T_Retreat model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    string name = Server.UrlDecode(Request.Cookies["Name"].Value);

                    T_OperaterLog log = new T_OperaterLog()
                    {
                        Module = "退货退款",
                        OperateContent = string.Format("ViewRetreatEditSave ID:{0},Retreat_OrderNumber:{1}", model.ID, model.Retreat_OrderNumber),
                        Operater = name,
                        OperateTime = DateTime.Now,
                        PID = -1

                    };
                    db.T_OperaterLog.Add(log);
                    db.SaveChanges();

                    List<T_RetreatDetails> details = App_Code.Com.Deserialize<T_RetreatDetails>(jsonStr);
                    //主表保存
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    string RetreatReason = model.Retreat_Reason;//记录原来的原因
                    T_Retreat PurMod = db.T_Retreat.Find(editID);

                    if (PurMod.Status != -1 && PurMod.Status != 2)
                    {
                        return Json(new { State = "Faile", Message = "该记录已经审核，不允许修改" }, JsonRequestBehavior.AllowGet);
                    }

                    PurMod.Retreat_Warehouse = model.Retreat_Warehouse;
                    PurMod.OrderpaymentMoney = model.OrderpaymentMoney;
                    PurMod.Retreat_Shouldjine = model.Retreat_Shouldjine;
                    PurMod.Retreat_Reason = model.Retreat_Reason;
                    PurMod.Retreat_GoodsSituation = model.Retreat_GoodsSituation;
                    PurMod.Retreat_expressName = model.Retreat_expressName;
                    PurMod.Retreat_expressNumber = model.Retreat_expressNumber;
                    PurMod.Retreat_Remarks = model.Retreat_Remarks;
                    PurMod.OrderSatus = model.OrderSatus;
                    PurMod.IsReturn = model.IsReturn;
                    PurMod.IsRefund = model.IsRefund;
                    PurMod.Step = 0;
                    PurMod.Status = -1;
                    db.Entry<T_Retreat>(PurMod).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        //修改审核  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                        T_RetreatAppRove ApproveMod = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (ApproveMod == null)
                        {
                            //不同意修改
                            T_RetreatAppRove Approvemodel = new T_RetreatAppRove();
                            //得到退货退款原因
                            string Reason = model.Retreat_Reason;
                            //根据原因去查询原因类型
                            T_RetreatReason ReasonModel = db.T_RetreatReason.SingleOrDefault(a => a.RetreatReason == Reason);
                            //得到原因类型
                            string Type = ReasonModel.Type;
                            //去查询原因类型配置表
                            T_RetreatConfig configModel = db.T_RetreatConfig.SingleOrDefault(a => a.Reason == Type && a.Step == 0);
                            Approvemodel.ApproveDName = configModel.Type;
                            //判断原因类型配置表的审核人名称是否为空,如果不为空直接给审核人名称，如果为空证明是部门多人审核，就进去给部门名称
                            if (configModel.Name == null || configModel.Name == "")
                            {
                                //如果是部门主管就去查询人员基础信息表，找到对应的部门主管，如果没有部门主管就默认成风审核
                                if (configModel.Type == "部门主管")
                                {
                                    T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name);
                                    string depart = userModel.DepartmentId;
                                    T_User userDepartModel = db.T_User.SingleOrDefault(a => a.DepartmentId == depart && a.IsManagers == "1");
                                    if (userDepartModel != null)
                                    {
                                        Approvemodel.ApproveName = userDepartModel.Nickname;
                                    }
                                    else
                                    {
                                        Approvemodel.ApproveName = "成风";
                                    }
                                }
                                else if (configModel.Type == "财务")
                                {
                                    T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == model.ID && a.ApproveDName == "财务" && a.Status == 1);
                                    if (appRoveModel != null)
                                    {
                                        T_RetreatConfig stepAppRoveMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == 1 && a.Reason == Type);
                                        if (stepAppRoveMod.Name != null && stepAppRoveMod.Name != "")
                                        {
                                            Approvemodel.ApproveName = stepAppRoveMod.Name;
                                            Approvemodel.ApproveDName = stepAppRoveMod.Name;
                                        }
                                        else
                                        {
                                            Approvemodel.ApproveName = stepAppRoveMod.Type;
                                            Approvemodel.ApproveDName = stepAppRoveMod.Type;
                                        }
                                    }
                                    else
                                    {
                                        Approvemodel.ApproveName = configModel.Type;
                                        Approvemodel.ApproveDName = configModel.Type;
                                    }
                                }
                                else
                                {
                                    //如果不是部门主管意外的部门，都是由部门作为审核人，可以到退货退款人员配置表中查询详细有那些人员（T_RetreatGroup）
                                    Approvemodel.ApproveName = configModel.Type;
                                }


                            }
                            else
                            {
                                //如果审核人员不为空，就直接默认
                                Approvemodel.ApproveName = configModel.Name;
                            }
                            Approvemodel.Type = Type;
                            Approvemodel.Status = -1;
                            Approvemodel.Memo = "";
                            Approvemodel.Oid = model.ID;
                            db.T_RetreatAppRove.Add(Approvemodel);
                            db.SaveChanges();
                        }
                        else
                        {
                            //新增未审批的修改
                            //得到退货退款原因
                            string Reason = model.Retreat_Reason;
                            //根据原因去查询原因类型
                            T_RetreatReason ReasonModel = db.T_RetreatReason.SingleOrDefault(a => a.RetreatReason == Reason);
                            //得到原因类型
                            string Type = ReasonModel.Type;
                            //去查询原因类型配置表
                            T_RetreatConfig configModel = db.T_RetreatConfig.SingleOrDefault(a => a.Reason == Type && a.Step == 0);
                            ApproveMod.ApproveDName = configModel.Type;
                            //判断原因类型配置表的审核人名称是否为空,如果不为空直接给审核人名称，如果为空证明是部门多人审核，就进去给部门名称
                            if (configModel.Name == null || configModel.Name == "")
                            {
                                //如果是部门主管就去查询人员基础信息表，找到对应的部门主管，如果没有部门主管就默认成风审核
                                if (configModel.Type == "部门主管")
                                {
                                    T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name);
                                    string depart = userModel.DepartmentId;
                                    T_User userDepartModel = db.T_User.SingleOrDefault(a => a.DepartmentId == depart && a.IsManagers == "1");
                                    if (userDepartModel != null)
                                    {
                                        ApproveMod.ApproveName = userDepartModel.Nickname;
                                    }
                                    else
                                    {
                                        ApproveMod.ApproveName = "成风";
                                    }
                                }
                                else if (configModel.Type == "财务")
                                {
                                    T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == model.ID && a.ApproveDName == "财务" && a.Status == 1);
                                    if (appRoveModel != null)
                                    {
                                        T_RetreatConfig stepAppRoveMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == 1 && a.Reason == Type);
                                        if (stepAppRoveMod.Name != null && stepAppRoveMod.Name != "")
                                        {
                                            ApproveMod.ApproveName = stepAppRoveMod.Name;
                                            ApproveMod.ApproveDName = stepAppRoveMod.Name;
                                        }
                                        else
                                        {
                                            ApproveMod.ApproveName = stepAppRoveMod.Type;
                                            ApproveMod.ApproveDName = stepAppRoveMod.Type;
                                        }
                                    }
                                    else
                                    {
                                        ApproveMod.ApproveName = configModel.Type;
                                        ApproveMod.ApproveDName = configModel.Type;
                                    }
                                }
                                else
                                {
                                    //如果不是部门主管意外的部门，都是由部门作为审核人，可以到退货退款人员配置表中查询详细有那些人员（T_RetreatGroup）
                                    ApproveMod.ApproveName = configModel.Type;
                                }
                            }
                            else
                            {
                                //如果审核人员不为空，就直接默认
                                ApproveMod.ApproveName = configModel.Name;
                            }
                            ApproveMod.Type = Type;
                            //   ApproveMod.ApproveName = GetUserNameById(model.ApproveFirst);
                            db.Entry<T_RetreatAppRove>(ApproveMod).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //删除oid==id原有的详情表记录
                        List<T_RetreatDetails> delMod = db.T_RetreatDetails.Where(a => a.Oid == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_RetreatDetails.Remove(item);
                        }
                        db.SaveChanges();

                        //添加新的详情
                        foreach (var item in details)
                        {
                            item.Oid = model.ID;
                            db.T_RetreatDetails.Add(item);
                        }
                        db.SaveChanges();
                        // ModularByZP();

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


        [HttpPost]
        [Description("退货退款分拣保存")]
        public JsonResult ViewRetreatSortingAddSave(T_Retreat model, string detailList)
        {
            //using (TransactionScope sc = new TransactionScope())
            //{
            //    try
            //    {
            //        string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //        string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            //        List<RetreatSorting> details = App_Code.Com.Deserialize<RetreatSorting>(detailList);
            //        string cpcode = "";
            //        //主表保存
            //        int editID = model.ID;//原记录的ID
            //        T_Retreat PurMod = db.T_Retreat.Find(editID);
            //        PurMod.isSorting = 1;
            //        PurMod.SortingName = Nickname;
            //        db.Entry<T_Retreat>(PurMod).State = System.Data.Entity.EntityState.Modified;
            //        int i = db.SaveChanges();
            //        if (i > 0)
            //        {
            //            string CkCode = PurMod.Retreat_Warehouse;
            //            for (int z = 0; z < details.Count; z++)
            //            {
            //                string CpCode = details[z].item_code;
            //                T_RetreatSorting SortingModel = db.T_RetreatSorting.SingleOrDefault(a => a.ProductCode == CpCode && a.WarehouseCode == CkCode);
            //                if (SortingModel != null)
            //                {
            //                    if (details[z].qualified != 0 || details[z].Unqualified != 0)
            //                    {

            //                        SortingModel.QualifiedNumber = SortingModel.QualifiedNumber + details[z].qualified;
            //                        SortingModel.UnqualifiedNumber = SortingModel.UnqualifiedNumber + details[z].Unqualified;
            //                        SortingModel.Simplename = details[z].Simplename;
            //                        db.Entry<T_RetreatSorting>(SortingModel).State = System.Data.Entity.EntityState.Modified;
            //                        db.SaveChanges();
            //                    }
            //                }
            //                else
            //                {
            //                    if (details[z].qualified != 0 || details[z].Unqualified != 0)
            //                    {
            //                        T_RetreatSorting Smodle = new T_RetreatSorting();
            //                        Smodle.ProductCode = details[z].item_code;
            //                        Smodle.ProductName = details[z].item_name;
            //                        Smodle.QualifiedNumber = details[z].qualified;
            //                        Smodle.UnqualifiedNumber = details[z].Unqualified;
            //                        Smodle.Simplename = details[z].Simplename;
            //                        Smodle.WarehouseCode = PurMod.Retreat_Warehouse;
            //                        db.T_RetreatSorting.Add(Smodle);
            //                        db.SaveChanges();
            //                    }
            //                }
            //                if (details[z].Notreceived != 0)
            //                {
            //                    T_ReceivedAfter AfterModel = new T_ReceivedAfter();
            //                    AfterModel.Type = "退货退款";
            //                    AfterModel.OrderNumber = PurMod.Retreat_OrderNumber;
            //                    AfterModel.ProductCode = details[z].item_code;
            //                    AfterModel.ProductName = details[z].item_name;
            //                    AfterModel.CollectExpressName = PurMod.Retreat_expressName;
            //                    AfterModel.CollectExpressNumber = PurMod.Retreat_expressNumber;
            //                    AfterModel.ShopName = PurMod.Retreat_dianpName;
            //                    AfterModel.CustomerName = PurMod.Retreat_CustomerName;
            //                    AfterModel.CustomerCode = PurMod.Retreat_wangwang;
            //                    AfterModel.ProductNumber = details[z].Notreceived;
            //                    AfterModel.CreatTime = DateTime.Now;
            //                    db.T_ReceivedAfter.Add(AfterModel);
            //                    db.SaveChanges();
            //                }
            //                int qty = details[z].qualified + details[z].Unqualified + details[z].Notreceived - details[z].qty;
            //                if (qty != 0)
            //                {
            //                    T_ReceivedAfter AfterModel = new T_ReceivedAfter();
            //                    AfterModel.Type = "退货退款";
            //                    AfterModel.OrderNumber = PurMod.Retreat_OrderNumber;
            //                    AfterModel.ProductCode = details[z].item_code;
            //                    AfterModel.ProductName = details[z].item_name;
            //                    AfterModel.CollectExpressName = PurMod.Retreat_expressName;
            //                    AfterModel.CollectExpressNumber = PurMod.Retreat_expressNumber;
            //                    AfterModel.ShopName = PurMod.Retreat_dianpName;
            //                    AfterModel.CustomerName = PurMod.Retreat_CustomerName;
            //                    AfterModel.CustomerCode = PurMod.Retreat_wangwang;
            //                    AfterModel.SurplusProductNumber = qty;
            //                    AfterModel.CreatTime = DateTime.Now;
            //                    db.T_ReceivedAfter.Add(AfterModel);
            //                    db.SaveChanges();
            //                }

            //                T_RetreatDetails DetailsMode = db.T_RetreatDetails.SingleOrDefault(a => a.Oid == editID && a.item_code == CpCode);
            //                if (DetailsMode == null)
            //                {
            //                    T_ReceivedAfter AfterModel = new T_ReceivedAfter();
            //                    AfterModel.Type = "退货退款";
            //                    AfterModel.OrderNumber = PurMod.Retreat_OrderNumber;
            //                    AfterModel.ProductCode = details[z].item_code;
            //                    AfterModel.ProductName = details[z].item_name;
            //                    AfterModel.CollectExpressName = PurMod.Retreat_expressName;
            //                    AfterModel.CollectExpressNumber = PurMod.Retreat_expressNumber;
            //                    AfterModel.ShopName = PurMod.Retreat_dianpName;
            //                    AfterModel.CustomerName = PurMod.Retreat_CustomerName;
            //                    AfterModel.CustomerCode = PurMod.Retreat_wangwang;
            //                    AfterModel.CreatTime = DateTime.Now;
            //                    AfterModel.SurplusProductNumber = details[z].qty;
            //                    db.T_ReceivedAfter.Add(AfterModel);
            //                    db.SaveChanges();
            //                }
            //                //   上传管易。调整单新增
            //                int qtyGY = details[z].qualified + details[z].Unqualified;
            //                if (z == details.Count - 1)
            //                {
            //                    cpcode += "{\"item_code\":\"" + details[z].item_code + "\",\"qty\":" + qtyGY + "}";
            //                }
            //                else
            //                {
            //                    cpcode += "{\"item_code\":\"" + details[z].item_code + "\",\"qty\":" + qtyGY + "},";
            //                }
            //            }
            //            string WarehouseCode = PurMod.Retreat_Warehouse;
            //            EBMS.App_Code.GY gy = new App_Code.GY();
            //            string cmd = "";
            //            cmd = "{" +
            //                        "\"appkey\":\"171736\"," +
            //                        "\"method\":\"gy.erp.stock.adjust.add\"," +
            //                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
            //                             "\"order_type\":\"001\"," +
            //                               "\"warehouse_code\":\"" + WarehouseCode + "\"," +
            //                          "\"detail_list\":[" + cpcode + "]" +
            //                        "}";
            //            string sign = gy.Sign(cmd);
            //            string comcode = "";
            //            comcode = "{" +
            //                     "\"appkey\":\"171736\"," +
            //                     "\"method\":\"gy.erp.stock.adjust.add\"," +
            //                     "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
            //                        "\"sign\":\"" + sign + "\"," +
            //                          "\"order_type\":\"001\"," +
            //                            "\"warehouse_code\":\"" + WarehouseCode + "\"," +
            //                       "\"detail_list\":[" + cpcode + "]" +
            //                     "}";
            //            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);

            //            sc.Complete();
            //            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            //        }

            //        return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            //    }
            //}
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public string Tz(T_Retreat model, List<T_RetreatDetails> detail)
        {
            string cpcode = "[";
            for (int z = 0; z < detail.Count; z++)
            {
                int Qty = int.Parse(detail[z].qty.ToString());
                if (z == detail.Count - 1)
                {
                    cpcode += "{\"spec_no\": \"" + detail[z].item_code + "\",\"price\": \"0\",\"num\": \"" + Qty + "\"}";
                }
                else
                {
                    cpcode += "{\"spec_no\": \"" + detail[z].item_code + "\",\"price\": \"0\",\"num\": \"" + Qty + "\"},";
                }
            }
            cpcode += "]";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string WarehouseCode = model.Retreat_Warehouse;
            string order_type = "2";
            string api_outer_no = model.Retreat_expressNumber;
            EBMS.App_Code.GY gy = new App_Code.GY();
            dic.Add("warehouse_no", WarehouseCode);
            dic.Add("order_type", order_type);
            dic.Add("api_outer_no", api_outer_no);
            dic.Add("goods_list", cpcode);
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());
            var aa = CreateParam(dic, true);

            string ret = Post("http://api.wangdian.cn/openapi2/wms_stockinout_order_push.php", aa);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string code = jsonData["code"].ToString();
            if (code == "0")
            {
                string stockout = jsonData["data"][0].ToString();

                return stockout;
            }

            ////   上传管易。调整单新增
            //int? qtyGY = detail.qty;
            //string cpcode = "{\"item_code\":\"" + detail.item_code + "\",\"qty\":" + qtyGY + "}";
            //EBMS.App_Code.GY gy = new App_Code.GY();
            //string cmd = "";
            //cmd = "{" +
            //            "\"appkey\":\"171736\"," +
            //            "\"method\":\"gy.erp.stock.adjust.add\"," +
            //            "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
            //                 "\"order_type\":\"001\"," +
            //                  "\"note\":\"平台单号:" + model.Retreat_OrderNumber + "来自退款已发货未出库\"," +
            //                   "\"warehouse_code\":\"" + model.Retreat_Warehouse + "\"," +
            //              "\"detail_list\":[" + cpcode + "]" +
            //            "}";
            //string sign = gy.Sign(cmd);
            //string comcode = "";
            //comcode = "{" +
            //         "\"appkey\":\"171736\"," +
            //         "\"method\":\"gy.erp.stock.adjust.add\"," +
            //         "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
            //            "\"sign\":\"" + sign + "\"," +
            //              "\"order_type\":\"001\"," +
            //              "\"note\":\"平台单号:" + model.Retreat_OrderNumber + "来自退款已发货未出库\"," +
            //                "\"warehouse_code\":\"" + model.Retreat_Warehouse + "\"," +
            //           "\"detail_list\":[" + cpcode + "]" +
            //         "}";
            //string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);

            //  return ret;



            return "0";
        }

        //审核操作
        [HttpPost]
        [Description("退货退款审核")]
        public JsonResult RetreatReportCheck(int id, int status, string memo, string Actualjine, string RetreatPayment, string RetreatexpressNumber)
        {



            string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

            List<T_RetreatGroup> RetreatGroupList = db.Database.SqlQuery<T_RetreatGroup>("select  * from T_RetreatGroup where Crew like '%" + Nickname + "%'").ToList();
            string GroupName = "";
            for (int i = 0; i < RetreatGroupList.Count; i++)
            {
                if (i == 0)
                {
                    GroupName += "'" + RetreatGroupList[i].GroupName + "'";
                }
                else
                {
                    GroupName += "," + "'" + RetreatGroupList[i].GroupName + "'";
                }
            }

            string sql = "select * from T_RetreatAppRove where Oid='" + id + "' and ApproveTime is null ";
            if (GroupName != "" && GroupName != null)
            {
                sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
            }
            else
            {
                sql += "    and ApproveName='" + Nickname + "'  ";
            }
            List<T_RetreatAppRove> AppRoveListModel = db.Database.SqlQuery<T_RetreatAppRove>(sql).ToList();
            if (AppRoveListModel.Count == 0)
            {
                return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
            }

            T_RetreatAppRove modelApprove = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);

            string result = "";
            //if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            using (TransactionScope sc = new TransactionScope())
            {

                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = memo;
                modelApprove.ApproveTime = DateTime.Now;
                if (status == 3)
                {
                    modelApprove.Status = 1;
                }
                else
                {
                    modelApprove.Status = status;
                }
                db.Entry<T_RetreatAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    T_Retreat model = db.T_Retreat.Find(id);
                    T_RetreatAppRove newApprove = new T_RetreatAppRove();
                    if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                    if (status == 1)
                    {
                        //同意
                        string type = modelApprove.Type;
                        int step = int.Parse(model.Step.ToString());
                        step++;
                        IQueryable<T_RetreatConfig> config = db.T_RetreatConfig.Where(a => a.Reason == type);
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {


                            //不是最后一步，主表状态为0 =>审核中

                            model.Status = 0;
                            T_RetreatConfig stepMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                            string nextName = stepMod.Name;
                            //下一步审核人不是null  审核记录插入一条新纪录
                            newApprove.Memo = "";
                            newApprove.Oid = id;
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
                            //下一级审核人
                            string shenheName = newApprove.ApproveDName;
                            if (shenheName == "财务")
                            {
                                T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveDName == "财务" && a.Status == 1);

                                if (appRoveModel != null)
                                {
                                    // int stepAppRove = step + 1;
                                    step++;
                                    T_RetreatConfig stepAppRoveMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                    if (stepAppRoveMod == null)
                                    {

                                        model.Status = status;
                                        step--;

                                        if (model.Retreat_Reason == "报损退款" || model.Retreat_Reason == "报损退款(新)")
                                        {
                                            int RetreatID = model.ID;

                                            List<T_RetreatDetails> DetailsMode = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();
                                            string name = model.Retreat_ApplyName;
                                            string DepartmentID = DepartmentQuer(name);
                                            List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();
                                            T_LossReport LossReport = new T_LossReport();
                                            LossReport.PostUser = model.Retreat_ApplyName;
                                            LossReport.Department = DepartmentID;
                                            LossReport.Shop = model.Retreat_dianpName;
                                            LossReport.Code = CodeQuery();
                                            LossReport.Status = -1;
                                            LossReport.Total = 0;
                                            LossReport.Step = 0;
                                            LossReport.IsAllowdelete = 1;
                                            if (ZgModel.Count > 0)
                                            {
                                                LossReport.ApproveFirst = ZgModel[0].DepartmentId;
                                            }
                                            else
                                            {
                                                string ApproveFirst = DepartmentQuer("成风");

                                                LossReport.ApproveFirst = ApproveFirst;
                                            }
                                            LossReport.IsDelete = 0;
                                            LossReport.PostTime = DateTime.Now;
                                            LossReport.IsPzStatus = 0;
                                            db.T_LossReport.Add(LossReport);
                                            int s = db.SaveChanges();
                                            if (s > 0)
                                            {
                                                T_LossReportApprove Approvemodel = new T_LossReportApprove();
                                                Approvemodel.Status = -1;

                                                if (ZgModel.Count > 0)
                                                {
                                                    Approvemodel.ApproveName = ZgModel[0].DepartmentId;
                                                }
                                                else
                                                {
                                                    //  string ApproveFirst = DepartmentQuer("成风");

                                                    Approvemodel.ApproveName = "汪紫炜";
                                                }
                                                Approvemodel.Memo = "";
                                                Approvemodel.Oid = LossReport.ID;
                                                db.T_LossReportApprove.Add(Approvemodel);
                                                db.SaveChanges();
                                                decimal price = 0;
                                                for (int z = 0; z < DetailsMode.Count; z++)
                                                {

                                                    T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                                                    LossReportDetailModel.Oid = LossReport.ID;

                                                    LossReportDetailModel.ProductCode = DetailsMode[z].item_code;
                                                    LossReportDetailModel.ProductName = DetailsMode[z].item_name;
                                                    LossReportDetailModel.WangWang = model.Retreat_wangwang;
                                                    LossReportDetailModel.OderNumber = model.Retreat_OrderNumber;
                                                    LossReportDetailModel.Reason = "售后问题处理报损";
                                                    LossReportDetailModel.Unit = "无";
                                                    LossReportDetailModel.UnitPrice = QueryUnitPrice(DetailsMode[z].item_code);
                                                    LossReportDetailModel.Qty = int.Parse(DetailsMode[z].qty.ToString());
                                                    LossReportDetailModel.Amount = int.Parse(DetailsMode[z].qty.ToString()) * decimal.Parse(QueryUnitPrice(DetailsMode[z].item_code).ToString());
                                                    price += LossReportDetailModel.Amount;
                                                    db.T_LossReportDetail.Add(LossReportDetailModel);
                                                    db.SaveChanges();
                                                }


                                                //总价
                                                T_LossReport modelLoss = db.T_LossReport.Find(LossReport.ID);
                                                modelLoss.Total = price;
                                                db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                                string LossReportApproveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                                                List<ModularQuery> LossReportApproveQuery = db.Database.SqlQuery<ModularQuery>(LossReportApproveSql).ToList();

                                                for (int e = 0; e < LossReportApproveQuery.Count; e++)
                                                {
                                                    string PendingAuditName = LossReportApproveQuery[e].PendingAuditName;

                                                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                                                    if (NotauditedModel != null)
                                                    {
                                                        NotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                                                    }
                                                    else
                                                    {
                                                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                        ModularNotauditedModel.ModularName = "报损";
                                                        ModularNotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                        ModularNotauditedModel.PendingAuditName = LossReportApproveQuery[e].PendingAuditName;
                                                        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                                    }
                                                    db.SaveChanges();
                                                }


                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (stepAppRoveMod.Name != null)
                                        {
                                            newApprove.ApproveName = stepAppRoveMod.Name;
                                        }
                                        else
                                        {
                                            newApprove.ApproveName = stepAppRoveMod.Type;
                                            newApprove.ApproveDName = stepAppRoveMod.Type;
                                        }
                                        db.T_RetreatAppRove.Add(newApprove);
                                        db.SaveChanges();
                                        if (newApprove.ApproveName == "仓库")
                                        {
                                            string exNumber = model.Retreat_expressNumber;

                                            //得到当前退款记录
                                            List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == exNumber && a.ID == id).ToList();


                                            T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.IsDelete == 0);

                                            if (Tostorage != null)
                                            {


                                                T_ReturnToStorage TostorageModel = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.isSorting == 1 && a.IsDelete == 0);
                                                if (TostorageModel != null)
                                                {
                                                    List<T_RetreatDetails> RetreatDetailsList = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();



                                                    for (int x = 0; x < RetreatDetailsList.Count; x++)
                                                    {
                                                        string itemCode = RetreatDetailsList[x].item_code;
                                                        int TostorageID = Tostorage.ID;
                                                        List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                                                        if (ReturnToStorageletList.Count == 0)
                                                        {
                                                            List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == exNumber).ToList();
                                                            if (ReceivedAfterList.Count == 0)
                                                            {
                                                                T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                                ReceivedAfterModel.Type = "退货退款";
                                                                ReceivedAfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
                                                                ReceivedAfterModel.ProductCode = RetreatDetailsList[x].item_code;
                                                                ReceivedAfterModel.ProductName = RetreatDetailsList[x].item_name;
                                                                ReceivedAfterModel.CollectExpressName = RetreatList[0].Retreat_expressName;
                                                                ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                                ReceivedAfterModel.ShopName = RetreatList[0].Retreat_dianpName;
                                                                ReceivedAfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
                                                                ReceivedAfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
                                                                ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].qty;
                                                                ReceivedAfterModel.CreatTime = DateTime.Now;
                                                                ReceivedAfterModel.IsHandle = 0;
                                                                db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                }



                                                //得到退款id
                                                int RetreatID = RetreatList[0].ID;
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
                                                        T_RetreatAppRove newApproves = new T_RetreatAppRove();

                                                        string types = AppRoveModel.Type;
                                                        //  int steps = int.Parse(Rmodel.Step.ToString());
                                                        step++;
                                                        IQueryable<T_RetreatConfig> configs = db.T_RetreatConfig.Where(a => a.Reason == types);
                                                        int stepLengths = configs.Count();//总共步骤
                                                        if (step < stepLengths)
                                                        {
                                                            //不是最后一步，主表状态为0 =>审核中
                                                            Rmodel.Status = 0;
                                                            T_RetreatConfig stepMods = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                                            string nextNames = stepMod.Name;
                                                            //下一步审核人不是null  审核记录插入一条新纪录
                                                            newApproves.Memo = "";
                                                            newApproves.Oid = RetreatID;
                                                            newApproves.Status = -1;
                                                            newApproves.Type = types;
                                                            newApproves.ApproveDName = stepMods.Type;
                                                            if (nextNames != null && nextNames != "")
                                                            {
                                                                newApproves.ApproveName = nextNames;
                                                            }
                                                            else
                                                            {
                                                                newApproves.ApproveName = stepMods.Type;
                                                            }
                                                            string shenheNames = newApproves.ApproveDName;
                                                            db.T_RetreatAppRove.Add(newApproves);
                                                            db.SaveChanges();


                                                        }
                                                        else
                                                        {
                                                            //最后一步，主表状态改为 1 => 同意
                                                            Rmodel.Status = 1;
                                                        }

                                                        Rmodel.Step = step;
                                                        db.Entry<T_Retreat>(Rmodel).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();



                                                        List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();

                                                    }
                                                }


                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    db.T_RetreatAppRove.Add(newApprove);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                db.T_RetreatAppRove.Add(newApprove);
                                db.SaveChanges();
                            }

                            if (shenheName == "仓库")
                            {
                                string exNumber = model.Retreat_expressNumber;

                                //得到当前退款记录
                                List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == exNumber && a.ID == id).ToList();


                                T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.IsDelete == 0);

                                if (Tostorage != null)
                                {
                                    int TostorageID = Tostorage.ID;
                                    List<T_ReturnToStoragelet> ReceiveList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID).ToList();
                                    T_ReturnToStorage TostorageModel = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.isSorting == 1 && a.IsDelete == 0);
                                    if (TostorageModel != null)
                                    {
                                        List<T_RetreatDetails> RetreatDetailsList = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();



                                        for (int x = 0; x < RetreatDetailsList.Count; x++)
                                        {
                                            string itemCode = RetreatDetailsList[x].item_code;

                                            List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                                            if (ReturnToStorageletList.Count == 0)
                                            {
                                                List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == exNumber).ToList();
                                                if (ReceivedAfterList.Count == 0)
                                                {
                                                    T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                    ReceivedAfterModel.Type = "退货退款";
                                                    ReceivedAfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
                                                    ReceivedAfterModel.ProductCode = RetreatDetailsList[x].item_code;
                                                    ReceivedAfterModel.ProductName = RetreatDetailsList[x].item_name;
                                                    ReceivedAfterModel.CollectExpressName = RetreatList[0].Retreat_expressName;
                                                    ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                    ReceivedAfterModel.ShopName = RetreatList[0].Retreat_dianpName;
                                                    ReceivedAfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
                                                    ReceivedAfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
                                                    ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].qty;
                                                    ReceivedAfterModel.CreatTime = DateTime.Now;
                                                    ReceivedAfterModel.IsHandle = 0;
                                                    db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }


                                    //得到退款id
                                    int RetreatID = RetreatList[0].ID;
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
                                            T_RetreatAppRove newApproves = new T_RetreatAppRove();

                                            string types = AppRoveModel.Type;
                                            //  int steps = int.Parse(Rmodel.Step.ToString());
                                            step++;
                                            IQueryable<T_RetreatConfig> configs = db.T_RetreatConfig.Where(a => a.Reason == types);
                                            int stepLengths = configs.Count();//总共步骤
                                            if (step < stepLengths)
                                            {
                                                //不是最后一步，主表状态为0 =>审核中
                                                Rmodel.Status = 0;
                                                T_RetreatConfig stepMods = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                                string nextNames = stepMod.Name;
                                                //下一步审核人不是null  审核记录插入一条新纪录
                                                newApproves.Memo = "";
                                                newApproves.Oid = RetreatID;
                                                newApproves.Status = -1;
                                                newApproves.Type = types;
                                                newApproves.ApproveDName = stepMods.Type;
                                                if (nextNames != null && nextNames != "")
                                                {
                                                    newApproves.ApproveName = nextNames;
                                                }
                                                else
                                                {
                                                    newApproves.ApproveName = stepMods.Type;
                                                }
                                                string shenheNames = newApproves.ApproveDName;
                                                db.T_RetreatAppRove.Add(newApproves);
                                                db.SaveChanges();


                                            }
                                            else
                                            {
                                                //最后一步，主表状态改为 1 => 同意
                                                Rmodel.Status = 1;
                                            }

                                            Rmodel.Step = step;
                                            db.Entry<T_Retreat>(Rmodel).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                            List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();

                                        }
                                    }


                                }

                            }

                            //当前审核人
                            string isCwuName = modelApprove.ApproveDName;
                            if (isCwuName == "财务")
                            {
                                model.Retreat_Actualjine = decimal.Parse(Actualjine);
                                model.Retreat_PaymentAccounts = RetreatPayment;

                                if (model.Retreat_Reason == "报损退款" || model.Retreat_Reason == "报损退款(新)")
                                {
                                    int RetreatID = model.ID;

                                    List<T_RetreatDetails> DetailsMode = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();
                                    string name = model.Retreat_ApplyName;
                                    string DepartmentID = DepartmentQuer(name);
                                    List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();
                                    T_LossReport LossReport = new T_LossReport();
                                    LossReport.PostUser = model.Retreat_ApplyName;
                                    LossReport.Department = DepartmentID;
                                    LossReport.Shop = model.Retreat_dianpName;
                                    LossReport.Code = CodeQuery();
                                    LossReport.Status = -1;
                                    LossReport.Total = 0;
                                    LossReport.Step = 0;
                                    LossReport.IsAllowdelete = 1;
                                    if (ZgModel.Count > 0)
                                    {
                                        LossReport.ApproveFirst = ZgModel[0].DepartmentId;
                                    }
                                    else
                                    {
                                        string ApproveFirst = DepartmentQuer("成风");

                                        LossReport.ApproveFirst = ApproveFirst;
                                    }
                                    LossReport.IsDelete = 0;
                                    LossReport.PostTime = DateTime.Now;
                                    LossReport.IsPzStatus = 0;
                                    db.T_LossReport.Add(LossReport);
                                    int s = db.SaveChanges();
                                    if (s > 0)
                                    {
                                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                                        Approvemodel.Status = -1;

                                        if (ZgModel.Count > 0)
                                        {
                                            Approvemodel.ApproveName = ZgModel[0].DepartmentId;
                                        }
                                        else
                                        {
                                            //  string ApproveFirst = DepartmentQuer("成风");

                                            Approvemodel.ApproveName = "汪紫炜";
                                        }
                                        Approvemodel.Memo = "";
                                        Approvemodel.Oid = LossReport.ID;
                                        db.T_LossReportApprove.Add(Approvemodel);
                                        db.SaveChanges();
                                        decimal price = 0;
                                        for (int z = 0; z < DetailsMode.Count; z++)
                                        {

                                            T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                                            LossReportDetailModel.Oid = LossReport.ID;

                                            LossReportDetailModel.ProductCode = DetailsMode[z].item_code;
                                            LossReportDetailModel.ProductName = DetailsMode[z].item_name;
                                            LossReportDetailModel.WangWang = model.Retreat_wangwang;
                                            LossReportDetailModel.OderNumber = model.Retreat_OrderNumber;
                                            LossReportDetailModel.Reason = "售后问题处理报损";
                                            LossReportDetailModel.Unit = "无";
                                            LossReportDetailModel.UnitPrice = QueryUnitPrice(DetailsMode[z].item_code);
                                            LossReportDetailModel.Qty = int.Parse(DetailsMode[z].qty.ToString());
                                            LossReportDetailModel.Amount = int.Parse(DetailsMode[z].qty.ToString()) * decimal.Parse(QueryUnitPrice(DetailsMode[z].item_code).ToString());
                                            price += LossReportDetailModel.Amount;
                                            db.T_LossReportDetail.Add(LossReportDetailModel);
                                            db.SaveChanges();
                                        }


                                        //总价
                                        T_LossReport modelLoss = db.T_LossReport.Find(LossReport.ID);
                                        modelLoss.Total = price;
                                        db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        string LossReportApproveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                                        List<ModularQuery> LossReportApproveQuery = db.Database.SqlQuery<ModularQuery>(LossReportApproveSql).ToList();

                                        for (int e = 0; e < LossReportApproveQuery.Count; e++)
                                        {
                                            string PendingAuditName = LossReportApproveQuery[e].PendingAuditName;

                                            T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                                            if (NotauditedModel != null)
                                            {
                                                NotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                                            }
                                            else
                                            {
                                                T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                ModularNotauditedModel.ModularName = "报损";
                                                ModularNotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                ModularNotauditedModel.PendingAuditName = LossReportApproveQuery[e].PendingAuditName;
                                                ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                            }
                                            db.SaveChanges();
                                        }


                                    }

                                    List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();

                                }


                            }
                            else if (isCwuName == "快递")
                            {
                                model.Retreat_expressNumber = RetreatexpressNumber;
                            }

                        }
                        else
                        {
                            string isCwuName = modelApprove.ApproveDName;
                            if (isCwuName == "财务")
                            {
                                model.Retreat_Actualjine = decimal.Parse(Actualjine);
                                model.Retreat_PaymentAccounts = RetreatPayment;
                                if (model.Retreat_Reason == "报损退款" || model.Retreat_Reason == "报损退款(新)")
                                {
                                    int RetreatID = model.ID;

                                    List<T_RetreatDetails> DetailsMode = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();
                                    string name = model.Retreat_ApplyName;
                                    string DepartmentID = DepartmentQuer(name);
                                    List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();
                                    T_LossReport LossReport = new T_LossReport();
                                    LossReport.PostUser = model.Retreat_ApplyName;
                                    LossReport.Department = DepartmentID;
                                    LossReport.Shop = model.Retreat_dianpName;
                                    LossReport.Code = CodeQuery();
                                    LossReport.Status = -1;
                                    LossReport.Total = 0;
                                    LossReport.Step = 0;
                                    LossReport.IsAllowdelete = 1;
                                    if (ZgModel.Count > 0)
                                    {
                                        LossReport.ApproveFirst = ZgModel[0].DepartmentId;
                                    }
                                    else
                                    {
                                        string ApproveFirst = DepartmentQuer("成风");

                                        LossReport.ApproveFirst = ApproveFirst;
                                    }
                                    LossReport.IsDelete = 0;
                                    LossReport.PostTime = DateTime.Now;
                                    LossReport.IsPzStatus = 0;
                                    db.T_LossReport.Add(LossReport);
                                    int s = db.SaveChanges();
                                    if (s > 0)
                                    {
                                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                                        Approvemodel.Status = -1;
                                        if (ZgModel.Count > 0)
                                        {
                                            Approvemodel.ApproveName = ZgModel[0].Name;
                                        }
                                        else
                                        {
                                            //  string ApproveFirst = DepartmentQuer("成风");

                                            Approvemodel.ApproveName = "汪紫炜";
                                        }
                                        Approvemodel.Memo = "";
                                        Approvemodel.Oid = LossReport.ID;
                                        db.T_LossReportApprove.Add(Approvemodel);
                                        db.SaveChanges();
                                        decimal price = 0;
                                        for (int z = 0; z < DetailsMode.Count; z++)
                                        {

                                            T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                                            LossReportDetailModel.Oid = LossReport.ID;

                                            LossReportDetailModel.ProductCode = DetailsMode[z].item_code;
                                            LossReportDetailModel.ProductName = DetailsMode[z].item_name;
                                            LossReportDetailModel.WangWang = model.Retreat_wangwang;
                                            LossReportDetailModel.OderNumber = model.Retreat_OrderNumber;
                                            LossReportDetailModel.Reason = "售后问题处理报损";
                                            LossReportDetailModel.Unit = "无";
                                            LossReportDetailModel.UnitPrice = QueryUnitPrice(DetailsMode[z].item_code);
                                            LossReportDetailModel.Qty = int.Parse(DetailsMode[z].qty.ToString());
                                            LossReportDetailModel.Amount = int.Parse(DetailsMode[z].qty.ToString()) * decimal.Parse(QueryUnitPrice(DetailsMode[z].item_code).ToString());
                                            price += LossReportDetailModel.Amount;
                                            db.T_LossReportDetail.Add(LossReportDetailModel);
                                            db.SaveChanges();
                                        }


                                        //总价
                                        T_LossReport modelLoss = db.T_LossReport.Find(LossReport.ID);
                                        modelLoss.Total = price;
                                        db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        string LossReportApproveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                                        List<ModularQuery> LossReportApproveQuery = db.Database.SqlQuery<ModularQuery>(LossReportApproveSql).ToList();

                                        for (int e = 0; e < LossReportApproveQuery.Count; e++)
                                        {
                                            string PendingAuditName = LossReportApproveQuery[e].PendingAuditName;

                                            T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                                            if (NotauditedModel != null)
                                            {
                                                NotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                                            }
                                            else
                                            {
                                                T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                ModularNotauditedModel.ModularName = "报损";
                                                ModularNotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                ModularNotauditedModel.PendingAuditName = LossReportApproveQuery[e].PendingAuditName;
                                                ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                            }
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }

                            List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();

                            model.SortingName = Nickname;
                            //最后一步，主表状态改为 1 => 同意
                            model.Status = status;
                        }
                        model.Step = step;
                        db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        if (j > 0)
                        {
                            result = "保存成功";
                        }
                        else
                        {
                            result = "保存失败";
                        }
                    }
                    else if (status == 3)
                    {
                        string Mtype = modelApprove.Type;
                        model.Status = 0;
                        T_RetreatConfig stepMod = db.T_RetreatConfig.SingleOrDefault(a => a.Reason == Mtype && a.Type == "快递");
                        string nextName = stepMod.Name;
                        //下一步审核人不是null  审核记录插入一条新纪录
                        newApprove.Memo = "";
                        newApprove.Oid = id;
                        newApprove.Status = -1;
                        newApprove.Type = Mtype;
                        newApprove.ApproveDName = stepMod.Type;
                        if (nextName != null && nextName != "")
                        {
                            newApprove.ApproveName = nextName;
                        }
                        else
                        {
                            newApprove.ApproveName = stepMod.Type;
                        }
                        db.T_RetreatAppRove.Add(newApprove);
                        db.SaveChanges();

                        model.Step = int.Parse(stepMod.Step.ToString());
                        model.Status = 0;
                        db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";

                    }
                    else
                    {
                        //不同意
                        model.Step = 0;
                        model.Status = 2;
                        db.Entry<T_Retreat>(model).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }

                //ModularByZP();
                sc.Complete();
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public partial class ModularQuery
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }

        public decimal QueryUnitPrice(string Code)
        {
            T_goodsGY model = db.T_goodsGY.SingleOrDefault(a => a.code == Code);
            string price = "0";
            if (model != null)
            {
                price = model.cost_price.ToString();
            }
            if (price == null || price == "")
            {
                price = "0";
            }

            return decimal.Parse(price);
        }

        //得到用户的部门
        public string DepartmentQuer(string name)
        {
            T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name || a.Nickname == name);

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
        [HttpPost]
        public JsonResult RetreatWarehouseEdit(List<ckshul> keyWordList, string ID, string type, string beizhu, string INcode)
        {


            //修改T_RetreatSorting数量
            string note = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_RetreatSorting> SortingList = db.Database.SqlQuery<T_RetreatSorting>("select * from  T_RetreatSorting where ID in (" + ID + ")").ToList();

            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string cpcode = "[";
                    int i = 0;


                    if (type == "1" || type == "3")
                    {
                        for (int x = 0; x < SortingList.Count; x++)
                        {
                            if (SortingList[x].WarehouseCode == INcode)
                            {
                                return Json(new { State = "Faile", miss = "货品目标仓库和原仓库不能相同,保存失败" }, JsonRequestBehavior.AllowGet);
                            }

                            for (int c = 0; c < keyWordList.Count; c++)
                            {
                                if (SortingList[x].ID == keyWordList[c].ID)
                                {
                                    if (x == SortingList.Count() - 1)
                                    {
                                        cpcode += "{\"num\":" + keyWordList[x].shul + ",\"spec_no\":\"" + SortingList[x].ProductCode + "\"}";
                                    }
                                    else
                                    {
                                        cpcode += "{\"num\":" + keyWordList[x].shul + ",\"spec_no\":\"" + SortingList[x].ProductCode + "\"},";
                                    }

                                    //新增T_RetreatSortingDelivering表记录，记录移库
                                    int QualifiedNumber = int.Parse(SortingList[x].QualifiedNumber.ToString()) - int.Parse(keyWordList[c].shul.ToString());
                                    SortingList[x].QualifiedNumber = QualifiedNumber;
                                    db.Entry<T_RetreatSorting>(SortingList[x]).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //新增T_RetreatSortingDelivering表记录，记录移库
                                    T_RetreatSortingDelivering Deliverng = new T_RetreatSortingDelivering();
                                    Deliverng.ProductCode = SortingList[x].ProductCode;
                                    Deliverng.ProductName = SortingList[x].ProductName;
                                    Deliverng.Number = int.Parse(keyWordList[c].shul.ToString());
                                    Deliverng.WarehouseCode = SortingList[x].WarehouseCode;
                                    Deliverng.type = int.Parse(type);
                                    Deliverng.WarehouseCodeB = INcode;
                                    Deliverng.AdjustmentDate = DateTime.Now;
                                    Deliverng.AdjustmentName = Nickname;
                                    db.T_RetreatSortingDelivering.Add(Deliverng);
                                    i = db.SaveChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int x = 0; x < SortingList.Count; x++)
                        {

                            for (int c = 0; c < keyWordList.Count; c++)
                            {
                                if (SortingList[x].ID == keyWordList[c].ID)
                                {
                                    if (x == SortingList.Count() - 1)
                                    {
                                        cpcode += "{\"num\":" + keyWordList[x].shul + ",\"spec_no\":\"" + SortingList[x].ProductCode + "\"}";
                                    }
                                    else
                                    {
                                        cpcode += "{\"num\":" + keyWordList[x].shul + ",\"spec_no\":\"" + SortingList[x].ProductCode + "\"},";
                                    }
                                    //修改T_RetreatSorting表数量
                                    int UnqualifiedNumber = int.Parse(SortingList[x].UnqualifiedNumber.ToString()) - int.Parse(keyWordList[c].shul.ToString());
                                    SortingList[x].UnqualifiedNumber = UnqualifiedNumber;
                                    db.Entry<T_RetreatSorting>(SortingList[x]).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //新增T_RetreatSortingDelivering表记录，记录出库
                                    T_RetreatSortingDelivering Deliverng = new T_RetreatSortingDelivering();
                                    Deliverng.ProductCode = SortingList[x].ProductCode;
                                    Deliverng.ProductName = SortingList[x].ProductName;
                                    Deliverng.Number = int.Parse(keyWordList[c].shul.ToString());
                                    Deliverng.WarehouseCode = SortingList[x].WarehouseCode;
                                    Deliverng.WarehouseCodeB = INcode;
                                    Deliverng.AdjustmentDate = DateTime.Now;
                                    Deliverng.AdjustmentName = Nickname;
                                    Deliverng.type = int.Parse(type);
                                    db.T_RetreatSortingDelivering.Add(Deliverng);
                                    i = db.SaveChanges();
                                }
                            }
                        }
                    }
                    cpcode += "]";
                    if (type == "1")
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        //  INcode = "7311002";

                        string GUID = System.Guid.NewGuid().ToString();
                        string transfer_info = "{\"transfer_type\":\"0\",\"outer_no\": \"" + GUID + "\",\"from_warehouse_no\": \"" + SortingList[0].WarehouseCode + "\",\"to_warehouse_no\": \"" + INcode + "\", \"skus\": " + cpcode + " }";
                        //   string transfer_info = "{\"transfer_type\":\"0\",\"outer_no\": \"" + GUID + "\",\"from_warehouse_no\": \"7311004\",\"to_warehouse_no\": \"" + INcode + "\", \"skus\": " + cpcode + " }";


                        dic.Add("transfer_info", transfer_info);

                        dic.Add("sid", "hhs2");
                        dic.Add("appkey", "hhs2-ot");

                        dic.Add("timestamp", GetTimeStamp());
                        string aa = CreateParam(dic, true);

                        string ret = Post("http://api.wangdian.cn/openapi2/stock_transfer_push.php", aa);
                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string code = jsonData["code"].ToString();
                        if (code == "0")
                        {


                            i = 1;
                        }
                        else
                        {
                            return Json(new { State = "Faile", miss = "旺店通提交失败" }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    if (i > 0)
                    {
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { State = "Faile", miss = "数据保存失败" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", miss = "异常导致失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            //新增出库移库表的记录T_RetreatSortingDelivering

            //做一条记录到管易

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
        public JsonResult ModifySave(T_Retreat model)
        {

            try
            {
                using (TransactionScope sc = new TransactionScope())
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_Retreat editModel = db.T_Retreat.Find(model.ID);
                    editModel.Retreat_PaymentAccounts = model.Retreat_PaymentAccounts;

                    T_OperaterLog log = new T_OperaterLog();
                    log.Module = "退货退款";
                    log.OperateContent = "修改出款账号";
                    log.Operater = name;
                    log.OperateTime = DateTime.Now;
                    log.PID = model.ID;
                    db.T_OperaterLog.Add(log);
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message = "保存成功" });
                }

            }
            catch (Exception e)
            {
                return Json(new { State = "Fail", Message = "保存失败" });
            }
        }
        #endregion


        //退货退款管理列表  
        [HttpPost]
        public ContentResult GetRetreatOutsourcingList(Lib.GridPager pager, string queryStr, string RetreatReason, string selStatus, string RetreatWarehouseList)
        {

            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_Retreat> queryData = from r in db.T_Retreat
                                              where r.Isdelete == "0" && r.OutsourcingSingle != null
                                              select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Contains(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Contains(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Contains(queryStr)) || (a.OutsourcingSingle != null && a.OutsourcingSingle.Contains(queryStr)));
            }
            if (!string.IsNullOrEmpty(RetreatWarehouseList))
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }

            if (!string.IsNullOrEmpty(RetreatReason))
            {
                queryData = queryData.Where(a => a.Retreat_Reason != null && a.Retreat_Reason.Contains(RetreatReason));
            }
            if (!string.IsNullOrEmpty(selStatus) && selStatus != "-2")
            {
                int status = int.Parse(selStatus);
                queryData = queryData.Where(a => a.Status == status);
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Retreat> list = new List<T_Retreat>();
            foreach (var item in queryData)
            {
                T_Retreat i = new T_Retreat();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                T_User x = db.T_User.FirstOrDefault(s => s.Name.Equals(i.Retreat_ApplyName));
                if (x != null)
                {
                    i.Retreat_ApplyName = x.Nickname;
                }
                // 
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        ///未审核页面导出excel
        public FileResult getExcelViewRetreatCheck(string queryStr, string ReasonType, string ExpressType, string RetreatWarehouseList, string queryStrS, string RetreatexpressNameList)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "退货退款导出",
                OperateContent = string.Format("导出excel getExcelViewRetreatCheck 条件 queryStr:{0},ReasonType:{1},ExpressType:{2},RetreatWarehouseList:{3},queryStrS:{4},RetreatexpressNameList:{5}", queryStr, ReasonType, ExpressType, RetreatWarehouseList, queryStrS, RetreatexpressNameList),
                Operater = Nickname,
                OperateTime = DateTime.Now,
                PID = -1
                //"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
            };
            db.T_OperaterLog.Add(log);
            db.SaveChanges();
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            List<T_RetreatGroup> GroupModel = db.T_RetreatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            //List<T_RetreatAppRove> ApproveMod = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
            //string arrID = "";
            //for (int i = 0; i < ApproveMod.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        arrID += ApproveMod[i].Oid.ToString();
            //    }
            //    else
            //    {
            //        arrID += "," + ApproveMod[i].Oid.ToString();
            //    }
            //}
            string sql = "select ID,Retreat_date,Retreat_dianpName,Retreat_wangwang,Retreat_CustomerName,Retreat_Remarks,Retreat_Shouldjine,Retreat_Actualjine,Retreat_OrderNumber,Retreat_expressName,Retreat_expressNumber,Retreat_GoodsSituation,Retreat_Warehouse,Retreat_ApplyName,Retreat_Reason,OrderMoney,OrderpaymentMoney,Retreat_PaymentAccounts,CollectName,CollectAddress,Isdelete,DeleteRemarks,OpenPieceName,OpenPieceDate,Status,Step,repeat,receivermobile,isNoheadparts,isnull((select top 1 TrackRecord_Situation from T_RetreatExpressRecord where Oid=r.ID order by T_RetreatExpressRecord.ID desc ),'') as KDJL from T_Retreat r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            //if (arrID != null && arrID != "")
            //{
            //    sql += " and ID in (" + arrID + ")";
            //}
            //else
            //{
            //    sql += " and 1=2";
            //}
            List<int?> Oids = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).Select(a => a.Oid).ToList();

            IQueryable<RetreatgetWshenhe> queryData = db.Database.SqlQuery<RetreatgetWshenhe>(sql).AsQueryable();
            queryData = queryData.Where(a => Oids.Contains(a.ID));
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Retreat_expressNumber != null && a.Retreat_expressNumber.Contains(queryStr)) || (a.Retreat_CustomerName != null && a.Retreat_CustomerName.Contains(queryStr)) || (a.Retreat_wangwang != null && a.Retreat_wangwang.Contains(queryStr)) || (a.Retreat_OrderNumber != null && a.Retreat_OrderNumber.Contains(queryStr)));
            }
            if (ReasonType != null && ReasonType != "")
            {
                queryData = queryData.Where(a => a.Retreat_Reason == ReasonType);
            }
            if (ExpressType != null && ExpressType != "")
            {
                if (ExpressType == "wcl")
                {
                    queryData = queryData.Where(a => a.KDJL == "");
                }
                else
                {
                    queryData = queryData.Where(a => a.KDJL == ExpressType);

                }

            }
            if (!string.IsNullOrEmpty(RetreatexpressNameList))
            {
                queryData = queryData.Where(a => a.Retreat_expressName == RetreatexpressNameList);
            }
            if (RetreatWarehouseList != null && RetreatWarehouseList != "")
            {
                queryData = queryData.Where(a => a.Retreat_Warehouse == RetreatWarehouseList);
            }
            if (queryStrS != null && queryStrS != "")
            {
                queryData = queryData.Where(a => a.Retreat_dianpName.Contains(queryStrS));
            }
            if (Nickname == "蔡侃")
            {
                queryData = queryData.Where(a => (a.Retreat_dianpName == "Tmall快乐老人医疗器械专营店") || (a.Retreat_dianpName == "Tb快乐老人经营产业有限公司") || (a.Retreat_dianpName.Contains("快乐老人医疗器械专营店")));
            }



            List<RetreatgetWshenhe> list = new List<RetreatgetWshenhe>();
            foreach (var item in queryData)
            {
                RetreatgetWshenhe i = new RetreatgetWshenhe();
                i = item;
                i.Retreat_expressName = GetexpressString(item.Retreat_expressName);
                i.Retreat_Warehouse = GetWarehouseString(item.Retreat_Warehouse);
                list.Add(i);
            }



            if (list.Count > 0)
            {

                //string csvIds = string.Join(",", ids.ToArray());
                //var ret = db.Database.SqlQuery<T_Retreat>("select * from T_Retreat where ID in (" + csvIds + ") order by ID desc");

                //List<T_Retreat> result = ret.ToList();
                //创建Excel文件的对象
                NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
                //给sheet1添加第一行的头部标题
                NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

                row1.CreateCell(0).SetCellValue("订单编号");
                row1.CreateCell(1).SetCellValue("物流单号");
                row1.CreateCell(2).SetCellValue("物流名称");
                row1.CreateCell(3).SetCellValue("退款金额");
                row1.CreateCell(4).SetCellValue("退款原因");
                row1.CreateCell(5).SetCellValue("店铺");

                for (int i = 0; i < list.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);


                    rowtemp.CreateCell(0).SetCellValue(list[i].Retreat_OrderNumber.ToString());
                    rowtemp.CreateCell(1).SetCellValue(list[i].Retreat_expressNumber.ToString());
                    rowtemp.CreateCell(2).SetCellValue(list[i].Retreat_expressName.ToString());
                    rowtemp.CreateCell(3).SetCellValue(list[i].Retreat_Shouldjine.ToString());
                    rowtemp.CreateCell(4).SetCellValue(list[i].Retreat_Reason.ToString());
                    rowtemp.CreateCell(5).SetCellValue(list[i].Retreat_dianpName);
                }

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "退货退款.xls");
            }
        }


        [Description("配置支付关系")]
        public ActionResult ViewRetreatPaymentList()
        {
            return View();
        }
        [Description("新增配置支付关系")]
        public ActionResult RetreatPaymentAdd()
        {
            ViewData["ShopName"] = GetWDTshop();
            //ViewData["PaymentAccountsName"] = GetPaymentAccountsName();

            return View();
        }
        public List<SelectListItem> GetWDTshop()
        {

            var list = db.T_WDTshop.AsQueryable();
            var selectList = new SelectList(list, "shop_name", "shop_name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> GetPaymentAccountsName()
        {
            T_RetreatGroup lists = db.T_RetreatGroup.SingleOrDefault(s => s.GroupName == "财务");
            string[] array = lists.Crew.Split(',');
            List<SelectListItem> items = new List<SelectListItem>();

            //foreach (var item in array)
            //{
            for (int i = 0; i < array.Length; i++)
            {
                string name = array[i];
                items.Add(new SelectListItem { Text = name, Value = name });
                //}

            }
            return items;
        }
        //  支付列表 
        [HttpPost]
        public ContentResult GetPaymentList(Lib.GridPager pager, string queryStr)
        {
            List<T_RetreatPayment> queryData = db.T_RetreatPayment.AsQueryable().ToList();

            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PaymentAccounts.Contains(queryStr) || a.ShopName.Contains(queryStr)).ToList();
            }
            //分页
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderBy(c => c.ID.ToString()).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //删除配置
        public JsonResult Del(int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                T_RetreatPayment MOD = db.T_RetreatPayment.Find(ID);
                if (MOD != null)
                {
                    db.T_RetreatPayment.Remove(MOD);
                }
                int i = db.SaveChanges();
                string result = "";
                if (i > 0)
                {

                    result = "删除成功";
                }
                else
                {
                    result = "删除失败";
                }

                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //支出账号保存
        [HttpPost]
        public JsonResult AddSave(T_RetreatPayment model)
        {
            string shopName = model.ShopName;
            string IsBlending = model.IsBlending;

            if (IsBlending == "1")
            {
                List<T_RetreatPayment> CashBackList = db.T_RetreatPayment.Where(a => a.ShopName == shopName && a.IsBlending == "1").ToList();
                if (CashBackList.Count > 0)
                {
                    return Json(new { State = "Faile", Message = "保存失败,该店铺已有主账号" }, JsonRequestBehavior.AllowGet);
                }

            }

            db.T_RetreatPayment.Add(model);
            int i = db.SaveChanges();
            if (i > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "保存失败,请联系相关人员" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetToken()
        {
            string url = "https://oapi.dingtalk.com/gettoken?corpid=ding2d039a809b22b5dc&corpsecret=ixiCpMGOiSCFzZ7pmCoKIq2r0QxIhY6eyuJ-0UKGx_WKtzE3UWDK6R9n7F_S3WtA";
            string ret = GY.HttpGet(url, "");
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            if (jsonData.Count == 4)
                return jsonData["access_token"].ToString();
            else
            {
                return "错误";
            }
        }


        public JsonResult Handle(int ID)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            if (Nickname != "半夏")
            {
                return Json("没有权限,请找半夏", JsonRequestBehavior.AllowGet);
            }

            using (TransactionScope sc = new TransactionScope())
            {
                T_Retreat MOD = db.T_Retreat.Find(ID);
                if (MOD == null)
                {
                    return Json("找不到该记录", JsonRequestBehavior.AllowGet);
                }
                MOD.Status = 2;
                MOD.Step = 0;
                MOD.Retreat_PaymentAccounts = null;
                db.Entry<T_Retreat>(MOD).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                //查询审核记录
                int maxId = Convert.ToInt32(db.T_RetreatAppRove.Where(a => a.Oid == ID).Max(a => a.ID));
                T_RetreatAppRove modelList = db.T_RetreatAppRove.SingleOrDefault(a => a.ID == maxId);//除了该订单本身是否还存在没有作废的订单，如果
                if (modelList == null)
                {
                    return Json("撤回失败", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    modelList.Status = 2;
                    modelList.ApproveTime = null;
                    db.Entry<T_RetreatAppRove>(modelList).State = System.Data.Entity.EntityState.Modified;
                    i = db.SaveChanges();
                }
                string result = "";
                if (i > 0)
                {

                    result = "撤回成功";
                    T_OperaterLog log = new T_OperaterLog();
                    log.Module = "退款";
                    log.OperateContent = "操作处理的驳回" + MOD.Retreat_OrderNumber;
                    log.Operater = Nickname;
                    log.OperateTime = DateTime.Now;
                    log.PID = MOD.ID;
                    db.T_OperaterLog.Add(log);

                    db.SaveChanges();
                }
                else
                {
                    result = "撤回失败";
                }
                ding(MOD.Retreat_OrderNumber, (decimal)MOD.Retreat_Actualjine);
                //ModularByZP();
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void ding(string order, decimal money)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

            //string tel = "13873155482";
            string number = "manager6456";//031365626621881757
            string cmd = "";

            string token = GetToken();
            string url = "https://oapi.dingtalk.com/message/send?access_token=" + token;//好护士
            string neir = "EBMS:" + Nickname + ":撤回退货退款已同意数据" + order + ",金额：" + money + "元,请知悉";
            cmd = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"text\",\"agentid\":\"97171864\"," +
                "\"text\":{ \"content\":\" " + neir + " \"} }";//好护士text
            string cmdss = "";
            string cmdoas = "";
            string ur = "http://ebms.hhs16.com/";
            string urr = "dingtalk://dingtalkclient/page/link?url=http%3a%2f%2febms.hhs16.com%3fpc_slide%3dfalse";

            cmdss = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"link\",\"agentid\":\"97171864\"," +
                "\"link\":{\"messageUrl\": \"http://ebms.hhs16.com\"," +
                                "\"picUrl\": \"@lALOACZwe2Rk\"," +
                                " \"title\": \"EBMS审批处理提醒\"," +
                                "\"text\": \"" + neir + "\"" +
                            "}}";//好护士link
            cmdoas = "{ \"touser\":\"" + number + "\"," +
                        "\"toparty\":\"\"," +
                        "\"msgtype\":\"oa\"," +
                        "\"agentid\":\"97171864\"," +
                        "\"oa\":{" +
                        "\"message_url\": \"" + ur + "\"," +
                        "\"pc_message_url\": \"" + urr + " \"," +
                            "\"head\": {" +
                            "\"bgcolor\": \"FFFF9900\"," +
                            "\"text\": \"EBMS审批处理提醒\"}," +
                        "\"body\": {" +
                        "\"title\": \"EBMS审批处理提醒\"," +
                        "\"content\": \"" + neir + "\"," +
                        "\"author\": \"ebms.admin\"}}}";//好护士oa
            string ret = GY.DoPosts(url, cmd, Encoding.UTF8, "json");//好护士
        }

        /// <summary>
        /// 数据导入的时候,满足以下全部要求可导入 
        //1.必须是有物流单号的
        //2.订单号在旺店通要可以找到
        //3.退款金额小于等于订单金额的
        //4.物流单号不重复
        //5.订单编号不重复
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadFiles()
        {
            string url = "";
            string result = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {

                    HttpPostedFileBase postFile = Request.Files[i];
                    string newFilePath = Server.MapPath("~/Upload/File/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName);
                    string dateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fullName = newFilePath + dateNow + fileName;

                    url = "/Upload/File/" + dateNow + fileName;
                    postFile.SaveAs(fullName);//save file 

                    AboutExcel AE = new AboutExcel();
                    DataTable RetreatData = AE.ImportExcelFile(fullName);
                    for (int j = 0; j < RetreatData.Rows.Count; i++)
                    {
                        string orderNO = RetreatData.Rows[j]["订单编号"].ToString();
                        string expressNO = RetreatData.Rows[j]["退货物流单号"].ToString();
                        string expressName = RetreatData.Rows[j]["退货物流公司"].ToString();
                        string retreatMoney = RetreatData.Rows[j]["退款金额"].ToString();
                        JsonResult x = QuyerRetreatDetailBYcode(orderNO);
                        if (RepeatMemo(orderNO) == "")
                        {
                            if (!string.IsNullOrEmpty(expressNO))
                            {
                                List<T_Retreat> RetreatModel = db.T_Retreat.Where(a => a.Retreat_expressNumber == expressNO && a.Isdelete == "0").ToList();
                                if (RetreatModel.Count == 0)
                                {
                                    Dictionary<string, string> dic = new Dictionary<string, string>();

                                    App_Code.GY gy = new App_Code.GY();
                                    string repeat = RepeatMemo(orderNO);
                                    //查询旺店通

                                    dic.Clear();
                                    dic.Add("src_tid", orderNO);
                                    //dic.Add("trade_no", code);
                                    dic.Add("sid", "hhs2");
                                    dic.Add("appkey", "hhs2-ot");
                                    dic.Add("timestamp", GetTimeStamp());
                                    string cmd = CreateParam(dic, true);
                                    string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/trade_query.php", cmd, Encoding.UTF8);
                                    JsonData jsonData = null;
                                    jsonData = JsonMapper.ToObject(ret);
                                    //T_Retreat model = new T_Retreat()
                                    //{
                                    //	CollectAddress="",
                                    //	CollectName="",
                                    //	DeleteRemarks="",
                                    //	DeliverStatus="",
                                    //};

                                }
                                else
                                {
                                    result += orderNO + ",快递单号重复\r\n";
                                }
                            }
                            else
                            {
                                result += orderNO + ",快递单号为空\r\n";
                            }

                        }
                        else
                        {
                            result += orderNO + ",订单号重复\r\n";
                        }


                    }
                }
                return Json(new { Message = "上传成功" });
            }
            catch (Exception ex)
            {
                return Json(new { Message = "上传失败" + ex.Message });
            }

        }
    }
}
