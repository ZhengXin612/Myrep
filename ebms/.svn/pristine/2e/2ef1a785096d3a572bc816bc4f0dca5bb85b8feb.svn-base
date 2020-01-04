using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
namespace EBMS.Controllers
{
    /// <summary>
    ///报损控制器
    /// </summary>
    public class LossReportController : BaseController
    {
        //接收JSON 反序列化
        public static List<T> Deserialize<T>(string text)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                List<T> list = (List<T>)js.Deserialize(text, typeof(List<T>));
                return list;

            }
            catch (Exception e)
            {

                return null;
            }
        }

        public class PzProduct
        {
            public int ID { get; set; }
            public int Qty { get; set; }
            public double UnitPrice { get; set; }
            public string PZ_Department { get; set; }
            public string PZ_Subject { get; set; }
            public string Reason { get; set; }
            public int PZ_Direction { get; set; }
        }

        public string GetCode()
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

        /// <summary>
        /// 凭证费用
        /// </summary>
        public class PZCost
        {
            public string PZ_Department { get; set; }
            public Nullable<decimal> PZ_Money { get; set; }
        }

        //部门转中文
        //部门ID转换中文名
        public string GetDaparementString(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int cid = int.Parse(id);
                List<T_Department> model = db.T_Department.Where(a => a.ID == cid).ToList();
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
        //根据用户ID转用户名
        public string GetUserNameById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int cid = int.Parse(id);
                T_User model = db.T_User.Find(cid);
                return model.Name;
            }
            else
            {
                return "";
            }
        }
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
        {
            //IQueryable<T_goodsGY> queryData = db.T_goodsGY.Where(a => a.combine == "False").AsQueryable();
            //if (!string.IsNullOrEmpty(queryStr))
            //{
            //    queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr) || a.code != null && a.code.Contains(queryStr));
            //}
            //pager.totalRows = queryData.Count();
            ////分页
            //queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            //List<T_goodsGY> list = new List<T_goodsGY>();
            //foreach (var item in queryData)
            //{
            //    T_goodsGY i = new T_goodsGY();
            //    i = item;
            //    list.Add(i);
            //}
            //string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            //return Content(json);
            IQueryable<T_WDTGoods> modellist = db.T_WDTGoods.Where(s => ((s.goods_no != null && s.goods_no.Contains(queryStr)) || (s.goods_name != null && s.goods_name.Contains(queryStr)) || (s.spec_name != null && s.spec_name.Contains(queryStr))) );
            pager.totalRows = modellist.Count();
            List<T_WDTGoods> querData = modellist.OrderBy(s => s.goods_no).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_LossReportApprove.Where(a => a.Oid == id);
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
        // GET: /LossReport/
        #region 视图

        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 报损凭证
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewLossReportPz()
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["userName"] = name;
            return View();
        }

        /// <summary>
        /// 凭证审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewLossReportPzApprove(int id)
        {
            if (id == 0)
                return HttpNotFound();
            var model = db.T_LossReport.Find(id);
            ViewData["lossId"] = id;
            return View(model);
        }


        /// 报损明细数据
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewLossReportidList(int LossReportid)
        {
            IQueryable<T_LossReportDetail> list = db.T_LossReportDetail.Where(s => s.Oid == LossReportid).AsQueryable();


            List<T_LossReportDetail> querData = list.OrderBy(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }



        /// <summary>
        /// 报损凭证列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewLossReportPzList()
        {
            ViewData["subject"] = Com.subject();
            ViewData["depart"] = Com.depart();
            return View();
        }

        //角色根权限下拉框
        public List<SelectListItem> GetShop()
        {
            var list = db.T_ShopFromGY.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择店铺", Value = "9999" });
            selecli.AddRange(selectList);
            return selecli;
        }

        //一级主管
        public List<SelectListItem> GetFisrtNameForApprove()
        {
            var list = db.T_User.Where(a => a.IsManagers == "1").AsQueryable();
            var selectList = new SelectList(list, "ID", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.AddRange(selectList);
            return selecli;
        }

        //报损新增
        [Description("访问报损新增页面")]
        public ActionResult ViewLossReportAdd()
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_User modeuser = db.T_User.SingleOrDefault(a => a.Nickname == name);
            //部门选择
            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", modeuser.DepartmentId);
            ViewData["trueName"] = modeuser.Name;
			ViewData["WarehouseList"]=Com.Warehouse();

			List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);
            ViewData["Shop"] = GetShop();
            //选择审批人
            List<T_User> modelistuser = db.T_User.Where(a => a.IsManagers == "1" && a.DepartmentId == modeuser.DepartmentId).ToList();
            int adminID = 9999;
            if (modelistuser.Count > 0)
            {
                adminID = modelistuser[0].ID;
            }
            ViewData["adminId"] = adminID;
            ViewData["ApproveFirst"] = GetFisrtNameForApprove();

            ViewData["Code"] = GetCode();
            return View();
        }
        [Description("访问我的报损页面")]
        public ActionResult ViewLossReportList()
        {
            return View();
        }

        [Description("凭证详情")]
        public ActionResult ViewLossReportListDetailPz(int tid)
        {
            ViewData["id"] = tid;
            return View();
        }

        [Description("访问我的报损详情页面")]
        public ActionResult ViewLossReportListDetail(int tid)
        {
            ViewData["ID"] = tid;
            T_LossReport model = db.T_LossReport.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewData["LossReportCode"] = model.Code;
            ViewData["LossReportTotal"] = model.Total;
            GetApproveHistory(tid);
            return View();
        }
        [Description("访问我的报损审核历史页面")]
        public ActionResult ViewLossReportHistory(int tid)
        {
            ViewData["ID"] = tid;
            T_LossReport model = db.T_LossReport.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(tid);
            return View();
        }
        [Description("访问报损未审核页面")]
        public ActionResult ViewLossReportUnCheck()
        {
            return View();
        }
        [Description("访问报损审核操作页面")]
        public ActionResult ViewLossReportCheck(int ID)
        {
            T_LossReport lossreport = new T_LossReport();
            lossreport = db.T_LossReport.Single(a => a.ID == ID);
            List<T_LossReportApprove> approve = db.T_LossReportApprove.Where(a => a.Oid == ID).ToList();
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
            string userName = Server.UrlDecode(Request.Cookies["Name"].Value);//当前帐号的真名
            T_LossReportApproveConfig financeMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Type == "财务主管");//审核财务主管一步
            string financeAdmin = financeMod.Name;
            string ss = "";
            if (userName == financeAdmin && lossreport.Step > 0) //财务主管   不是部门主管身份进来审核
            {
                ss += "<tr><td>请选择下级审核人：</td><td colspan='3'><select id=\"checkSelect\" name=\"checkSelect\" ><option value=\"李明霞\">向日葵</option><option value=\"曹朝霞\">曹朝霞</option></select></td></tr>";
            }
            ViewData["histr"] = ss;
            ViewData["history"] = table+ tr + "</tbody></table>";
            ViewData["approveid"] = ID;
            return View(lossreport);
        }
        [Description("访问报损编辑页面")]
        public ActionResult ViewLossReportEdit(int ID)
        {
            T_LossReport model = db.T_LossReport.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            //部门选择
            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", model.Department);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);
            ViewData["Shop"] = GetShop();
            //选择审批人
            ViewData["adminId"] = model.ApproveFirst;
            ViewData["ApproveFirst"] = GetFisrtNameForApprove();
            ViewData["ID"] = ID;
            return View(model);
        }
        //报损已审核
        [Description("报损已审核")]
        public ActionResult ViewLossReportChecked()
        {
            return View();
        }
        //报损统计
        [Description("报损统计")]
        public ActionResult ViewLossReportStatistics()
        {
            return View();
        }
        [Description("报损详情统计")]
        public ActionResult ViewLossReportCountList()
        {
            ViewData["RetreatShop"] = App_Code.Com.Shop();
			ViewData["WarehouseList"] = App_Code.Com.Warehouse();
			return View();
        }

        #endregion
        #region Post 操作
        //保损查询是否订单返现和退货
        public JsonResult CheckOrder(string jsonStr)
        {
            string Result = "";
            List<T_LossReportDetail> details = Deserialize<T_LossReportDetail>(jsonStr);
            foreach (var item in details)
            {
                string orderNumber = item.OderNumber;
                T_OrderList MOD_Order = db.T_OrderList.FirstOrDefault(a => a.platform_code == orderNumber);
                if (MOD_Order != null)
                {
                    if (MOD_Order.Status_CashBack != 0 && MOD_Order.Status_CashBack != null)
                    {
                        Result += orderNumber + "涉及返现!";
                    }
                    if (MOD_Order.Status_Retreat != 0 && MOD_Order.Status_Retreat != null)
                    {
                        Result += orderNumber + "涉及退货退款!";
                    }
                }
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //报损新增保存
        [HttpPost]
        [Description("报损新增保存")]
        public JsonResult LossReportAddSave(T_LossReport model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_LossReportDetail> details = Deserialize<T_LossReportDetail>(jsonStr);
                    //主表保存
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.IsPzStatus = 0;
                    model.IsDelete = 0;
                    model.Code = GetCode();
                    T_LossReport lossMod = new T_LossReport();
                    db.T_LossReport.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName = GetUserNameById(model.ApproveFirst);
                        Approvemodel.Memo = "";
                        Approvemodel.Oid = model.ID;
                        db.T_LossReportApprove.Add(Approvemodel);
                        db.SaveChanges();
                        decimal price = 0;
                        
                        foreach (var item in details)
                        {
                           
                            List<T_LossReportDetail> listDetail = db.Database.SqlQuery<T_LossReportDetail>("select * from T_LossReportDetail where  oid in (select ID from T_LossReport where IsDelete=0 and Status!=3) and OderNumber='" + item.OderNumber.Trim() + "' and  ProductCode='" + item.ProductCode.Trim() + "'").ToList();
                            //  db.T_LossReportDetail.Where(s => s.OderNumber.Equals(item.OderNumber) && s.ProductCode.Equals(item.ProductCode)).ToList();
                            if (listDetail.Count() > 0)
                                return Json(new { State = "Faile", Message = "保存失败订单号:" + item.OderNumber + "与商品编码:" + item.ProductCode + "已报损" }, JsonRequestBehavior.AllowGet);
                            item.Oid = model.ID;
                            item.UnitPrice = 0;
                            //   item.Amount = item.Qty * decimal.Parse(item.UnitPrice.ToString());
                            //    price += item.Qty * decimal.Parse(item.UnitPrice.ToString());
                            db.T_LossReportDetail.Add(item);
                        }
                        db.SaveChanges();
                        //总价
                        T_LossReport modelLoss = db.T_LossReport.Find(model.ID);
                       modelLoss.Total = 0;
                        db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
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
        public void ModularByZP()
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
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
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
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

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
        /// <summary>
        /// 获得凭证列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetPzList(Lib.GridPager pager, string code, string sub, string dep)
        {
            IQueryable<T_PZ_BS> list = db.T_PZ_BS.AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.PZ_OrderNum.Equals(code));
            if (!string.IsNullOrWhiteSpace(sub))
                list = list.Where(s => s.PZ_Subject.Equals(sub));
            if (!string.IsNullOrWhiteSpace(dep))
                list = list.Where(s => s.PZ_Department.Equals(dep));
            pager.totalRows = list.Count();
            List<PZCost> costList = new List<PZCost>();
            PZCost cost = new PZCost
            {
                PZ_Department = "总计：",
                PZ_Money = list.Count() > 0 ? list.Sum(s => s.PZ_Money) : 0
            };
            costList.Add(cost);
            List<T_PZ_BS> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
            return Content(json);
        }

        /// <summary>
        /// 凭证
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public ContentResult GetLossReportListPz(Lib.GridPager pager, string query)
        {

            IQueryable<T_LossReport> queryData = db.T_LossReport.Where(s => s.IsPzStatus != 1 && s.Status == 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                queryData = queryData.Where(s => s.Code.Equals(query));
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReport> list = new List<T_LossReport>();
            foreach (var item in queryData)
            {
                T_LossReport i = new T_LossReport();
                i = item;
                i.Department = GetDaparementString(item.Department);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 凭证保存
        /// </summary>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewPzAddSave(string jsonStr, int lossReportId)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "数据有误" });

                    List<PzProduct> productList = Com.Deserialize<PzProduct>(jsonStr);
                    T_LossReport model = db.T_LossReport.Find(lossReportId);
                    foreach (var item in productList)
                    {
                        T_LossReportDetail product = db.T_LossReportDetail.Find(item.ID);
                        product.PZ_Department = item.PZ_Department;
                        product.PZ_Subject = item.PZ_Subject;
                        product.PZ_Direction = item.PZ_Direction;
                        db.SaveChanges();
                    }
                    model.IsPzStatus = -1;
                    model.PzMemo = "";
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }


        //我的报损列表  
        [HttpPost]
        public ContentResult ShowLossReportList(Lib.GridPager pager, string queryStr, int status)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            IQueryable<T_LossReport> queryData = db.T_LossReport.Where(a => a.PostUser == name && a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReport> list = new List<T_LossReport>();
            foreach (var item in queryData)
            {
                T_LossReport i = new T_LossReport();
                i = item;
                i.Department = GetDaparementString(item.Department);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //报损详情列表 
        [HttpPost]
        public ContentResult GetLossReportDetailList(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_LossReportDetail> queryData = db.T_LossReportDetail.Where(a => a.Oid == ID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr) || a.OderNumber != null && a.OderNumber.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReportDetail> list = new List<T_LossReportDetail>();
            foreach (var item in queryData)
            {
                T_LossReportDetail i = new T_LossReportDetail();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }

        /// <summary>
        /// 凭证审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PzCheck(int id, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_LossReport lossReport = db.T_LossReport.Find(id);
                    lossReport.IsPzStatus = status;
                    lossReport.PzMemo = memo;
                    db.SaveChanges();
                    if (status == 1)
                    {
                        List<T_LossReportDetail> productList = db.T_LossReportDetail.Where(s => s.Oid == id).ToList();
                        foreach (var item in productList)
                        {
                            T_PZ_BS bs = new T_PZ_BS
                            {
                                PZ_OrderNum = lossReport.Code,
                                PZ_Summary = item.Reason,
                                PZ_Department = item.PZ_Department,
                                PZ_Direction = item.PZ_Direction,
                                PZ_Money = item.Qty * item.UnitPrice,
                                PZ_Subject = item.PZ_Subject,
                                PZ_Time = DateTime.Now
                            };
                            db.T_PZ_BS.Add(bs);
                        }
                        db.SaveChanges();
                    }
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }

        //虚拟删除报损记录 
        [HttpPost]
        [Description("删除报损")]
        public JsonResult DeleteLossReport(int del)
        {
            T_LossReport model = db.T_LossReport.Find(del);
            if (model.IsAllowdelete == 1)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            model.IsDelete = 1;
            db.Entry<T_LossReport>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

           // ModularByZP();




            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //作废报损记录
        [HttpPost]
        [Description("作废报损")]
        public JsonResult VoidLossReport(int ID)
        {
            T_LossReport model = db.T_LossReport.Find(ID);
            if (model.IsAllowdelete == 1)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            model.Status = 3;
            db.Entry<T_LossReport>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
           // ModularByZP();


            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //待审核数据列表
        [HttpPost]
        public ContentResult UnCheckLossReportList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_LossReportApprove> ApproveMod = db.T_LossReportApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            } 
            IQueryable<T_LossReport> queryData = from r in db.T_LossReport
                                                 where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0)
                                                 select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReport> list = new List<T_LossReport>();
            foreach (var item in queryData)
            {
                T_LossReport i = new T_LossReport();
                i = item;
                i.Department = GetDaparementString(item.Department);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public class PzJsonStr
        {
            public string ProductCode { get; set; }
            public string UnitPrice { get; set; }
        }


            //审核操作
            [HttpPost]
        [Description("报损审核")]
        public JsonResult LossReportCheck(int id, int status, string memo, string checkMan,string jsonStr)
        {

                List<PzJsonStr> details = Deserialize<PzJsonStr>(jsonStr);
         
            T_LossReportApprove modelApprove = db.T_LossReportApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
            string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
            if (modelApprove.ApproveName != curName)
            {
                return Json("当前不是你审核，或者你的帐号在别处登录了", JsonRequestBehavior.AllowGet);
            }
            T_LossReportApproveConfig financeMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Type == "财务主管");//审核财务主管一步
            string financeAdmin = financeMod.Name;
            string result = "";
            if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            modelApprove.Memo = memo;
            modelApprove.ApproveTime = DateTime.Now;
            modelApprove.Status = status;
            db.Entry<T_LossReportApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                T_LossReport model = db.T_LossReport.Find(id);
                T_LossReportApprove newApprove = new T_LossReportApprove();
                if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                if (status == 1)
                {
                    //同意
                    int step = int.Parse(model.Step.ToString());
                    step++;
                    IQueryable<T_LossReportApproveConfig> config = db.T_LossReportApproveConfig.AsQueryable();
                    int stepLength = config.Count();//总共步骤
                    if (step < stepLength)
                    {
                        //不是最后一步，主表状态为0 =>审核中
                        model.Status = 0;
                        T_LossReportApproveConfig stepMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Step == step);
                        string nextName = stepMod.Name;
                        //下一步审核人不是null  审核记录插入一条新纪录
                        newApprove.Memo = "";
                        newApprove.Oid = id;
                        newApprove.Status = -1;
                        if (nextName != null)
                        {
                            newApprove.ApproveName = nextName;
                        }
                        if (curName == financeAdmin && model.Step > 0)  //如果是以财务主管来审核
                        {
                            newApprove.ApproveName = checkMan;
                        }
                        db.T_LossReportApprove.Add(newApprove);
                        db.SaveChanges();


                       
                    }
                    else
                    {
                        //最后一步，主表状态改为 1 => 同意
                        model.Status = status;
                    }
                     
                    if (curName == "向日葵"|| curName == "李明霞" || curName == "曹朝霞" || curName == "曹朝霞")
                    {
                        foreach (var item in details)
                        {
                            string Code=item.ProductCode;
                            T_LossReportDetail ReportDetail = db.T_LossReportDetail.SingleOrDefault(a => a.Oid == id && a.ProductCode == Code);
                            ReportDetail.UnitPrice = decimal.Parse(item.UnitPrice);
                            ReportDetail.Amount = decimal.Parse(item.UnitPrice)* ReportDetail.Qty;
                            db.Entry<T_LossReportDetail>(ReportDetail).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }


                    }
                    model.Step = step;
                    db.Entry<T_LossReport>(model).State = System.Data.Entity.EntityState.Modified;
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
                else
                {
                    //不同意
                    model.Step = 0;
                    model.Status = 2;
                    db.Entry<T_LossReport>(model).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //审核流程结束 申请人编辑后插入下一条记录 
                    result = "保存成功";
                }
            }
            else
            {
                result = "保存失败";
            }
          //  ModularByZP();
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_LossReportDetail> queryData = db.T_LossReportDetail.Where(a => a.Oid == ID);
            pager.totalRows = queryData.Count();
            List<T_LossReportDetail> list = new List<T_LossReportDetail>();
            foreach (var item in queryData)
            {
                T_LossReportDetail i = new T_LossReportDetail();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //编辑保存
        [HttpPost]
        [Description("报损编辑保存")]
        public JsonResult LossReportEditSave(T_LossReport model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_LossReportDetail> details = Deserialize<T_LossReportDetail>(jsonStr);
                    //主表保存
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    T_LossReport lossMod = db.T_LossReport.Find(editID);
                    lossMod.Shop = model.Shop;
                    lossMod.ApproveFirst = model.ApproveFirst;
                    lossMod.Department = model.Department;
                    lossMod.Status = -1;
                    db.Entry<T_LossReport>(lossMod).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //修改审核  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                        T_LossReportApprove ApproveMod = db.T_LossReportApprove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (ApproveMod == null)
                        {
                            //不同意修改
                            T_LossReportApprove Approvemodel = new T_LossReportApprove();
                            Approvemodel.Status = -1;
                            Approvemodel.ApproveName = GetUserNameById(model.ApproveFirst);
                            Approvemodel.Memo = "";
                            Approvemodel.Oid = model.ID;
                            db.T_LossReportApprove.Add(Approvemodel);
                            db.SaveChanges();
                        }
                        else
                        {
                            //新增未审批的修改
                            ApproveMod.ApproveName = GetUserNameById(model.ApproveFirst);
                            db.Entry<T_LossReportApprove>(ApproveMod).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //删除oid==id原有的详情表记录
                        List<T_LossReportDetail> delMod = db.T_LossReportDetail.Where(a => a.Oid == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_LossReportDetail.Remove(item);
                        }
                        db.SaveChanges();
                        decimal price = 0;
                        //添加新的详情
                        foreach (var item in details)
                        {
                            List<T_LossReportDetail> listDetail = db.Database.SqlQuery<T_LossReportDetail>("select * from T_LossReportDetail where  oid in (select ID from T_LossReport where IsDelete=0 and Status!=3) and OderNumber='" + item.OderNumber + "' and  ProductCode='" + item.ProductCode + "'").ToList();
                            if (listDetail.Count() > 0)
                                return Json(new { State = "Faile", Message = "编辑失败订单号:" + item.OderNumber + "与商品编码:" + item.ProductCode + "已报损" }, JsonRequestBehavior.AllowGet);
                            item.Oid = model.ID;
                            item.Amount = item.Qty * decimal.Parse(item.UnitPrice.ToString());
                            price += item.Qty * decimal.Parse(item.UnitPrice.ToString());
                            db.T_LossReportDetail.Add(item);
                        }
                        db.SaveChanges();
                        //总价
                        T_LossReport modelLoss = db.T_LossReport.Find(model.ID);
                        modelLoss.Total = price;
                        db.Entry<T_LossReport>(modelLoss).State = System.Data.Entity.EntityState.Modified;
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
        //报损已审核      
        [HttpPost]
        public ContentResult CheckedLossReportList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_LossReportApprove> ApproveMod = db.T_LossReportApprove.Where(a => a.ApproveName == name && a.ApproveTime != null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_LossReport> queryData = from r in db.T_LossReport
                                                 where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != -1
                                                 select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {

                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.PostTime >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.PostTime <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReport> list = new List<T_LossReport>();
            foreach (var item in queryData)
            {
                T_LossReport i = new T_LossReport();
                i = item;
                i.Department = GetDaparementString(item.Department);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //导出EXCEL
        public partial class QueryLossReport
        {
            public int id { get; set; }
            public Nullable<System.DateTime> PostTime { get; set; }//申请时间
            public string ProductCode { get; set; } //产品代码
            public string ProductName { get; set; } //产品名称
            public string WangWang { get; set; }//旺旺
            public string OderNumber { get; set; }//订单号
            public string Reason { get; set; }//报损原因
            public string Unit { get; set; }//单位
            public Nullable<decimal> UnitPrice { get; set; }//单价
            public int Qty { get; set; }//数量
            public Nullable<decimal> Amount { get; set; }//总价
            public string Note { get; set; }//备注
            public string PostUser { get; set; }//报损申请人
            public string Shop { get; set; }//报损店铺
            public Nullable<System.DateTime> ApproveTime { get; set; }//审核时间
            public string  Code { get; set; }
        }
        [Description("导出EXCEL")]
        public FileResult getExcel(string statedate, string EndDate)
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据
            IQueryable<QueryLossReport> queryData = null;
            // string checkMan = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            DateTime sdate = new DateTime();
            DateTime Edate = new DateTime();
            if (statedate != "" && statedate != null && EndDate != "" && EndDate != null)
            {
                sdate = DateTime.Parse(statedate);
                Edate = DateTime.Parse(EndDate).AddDays(1);
                queryData = db.Database.SqlQuery<QueryLossReport>("select *,(select top 1 ApproveTime from  T_LossReportApprove where Oid=ls.Oid  order by ApproveTime desc) ApproveTime,(select PostTime from T_LossReport where ID=ls.oid) as  PostTime,(select PostUser from T_LossReport where ID=ls.Oid) as  PostUser,(select Shop from T_LossReport where ID=ls.Oid) as  Shop,(select Code from T_LossReport where ID=ls.Oid) as Code from T_LossReportDetail ls where ls.Oid in (select l.ID from T_LossReport l inner join T_LossReportApprove la on la.Oid=l.ID where l.Status=1 and IsDelete='0' and la.ApproveTime>='" + sdate + "' and la.ApproveTime<='" + Edate + "')").AsQueryable();

            }
            else if (statedate != "" && statedate != null)
            {
                sdate = DateTime.Parse(statedate);
                queryData = db.Database.SqlQuery<QueryLossReport>("select *,(select top 1 ApproveTime from  T_LossReportApprove where Oid=ls.Oid  order by ApproveTime desc) ApproveTime,(select PostTime from T_LossReport where ID=ls.Oid) as  PostTime,(select PostUser from T_LossReport where ID=ls.Oid) as  PostUser,(select Shop from T_LossReport where ID=ls.Oid) as  Shop,(select Code from T_LossReport where ID=Oid) as Code from T_LossReportDetail ls where ls.Oid in (select l.ID from T_LossReport l inner join T_LossReportApprove la on la.Oid=l.ID where l.Status=1 and IsDelete='0' and la.ApproveTime>='" + sdate + "'").AsQueryable();
            }
            else if (EndDate != "" && EndDate != null)
            {
                Edate = DateTime.Parse(EndDate);
                queryData = db.Database.SqlQuery<QueryLossReport>("select *,(select top 1 ApproveTime from  T_LossReportApprove where Oid=ls.Oid   order by ApproveTime desc) ApproveTime,(select PostTime from T_LossReport where ID=ls.Oid) as  PostTime,(select PostUser from T_LossReport where ID=ls.Oid) as  PostUser,(select Shop from T_LossReport where ID=Oid) as  Shop,(select Code from T_LossReport where ID=ls.Oid) as Code from T_LossReportDetail ls where ls.Oid in (select l.ID from T_LossReport l inner join T_LossReportApprove la on la.Oid=l.ID where l.Status=1 and IsDelete='0'  and la.ApproveTime<='" + Edate + "')").AsQueryable();
            }
            else
            {
                queryData = db.Database.SqlQuery<QueryLossReport>("select *,(select top 1 ApproveTime from  T_LossReportApprove where Oid=ls.Oid order by ApproveTime desc) ApproveTime,(select PostTime from T_LossReport where ID=ls.Oid) as  PostTime,(select PostUser from T_LossReport where ID=ls.Oid) as  PostUser,(select Shop from T_LossReport where ID=ls.Oid) as  Shop,(select Code from T_LossReport where ID=ls.Oid) as Code from T_LossReportDetail ls where ls.Oid in (select ID from	T_LossReport where Status='1' and IsDelete='0')").AsQueryable();

            }
            List<QueryLossReport> ListInfo = queryData.ToList();
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            row1.CreateCell(0).SetCellValue("日期");
            row1.CreateCell(1).SetCellValue("产品代码");
            row1.CreateCell(2).SetCellValue("产品名称");
            row1.CreateCell(3).SetCellValue("旺旺号");
            row1.CreateCell(4).SetCellValue("订单号");
            row1.CreateCell(5).SetCellValue("报损原因");
            row1.CreateCell(6).SetCellValue("单位");
            row1.CreateCell(7).SetCellValue("单价");
            row1.CreateCell(8).SetCellValue("数量");
            row1.CreateCell(9).SetCellValue("总价");
            row1.CreateCell(10).SetCellValue("备注");
            row1.CreateCell(11).SetCellValue("报损申请人");
            row1.CreateCell(12).SetCellValue("报损店铺");
            row1.CreateCell(13).SetCellValue("审批时间");
            row1.CreateCell(14).SetCellValue("报损编码");
            for (int i = 0; i < ListInfo.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                string Rdate = "";
                if (ListInfo[i].PostTime != null)
                {
                    Rdate = ListInfo[i].PostTime.ToString();
                }
                string checkTime = "";
                if (ListInfo[i].ApproveTime != null)
                {
                    checkTime = ListInfo[i].ApproveTime.ToString();
                }
                rowtemp.CreateCell(0).SetCellValue(Rdate);
                rowtemp.CreateCell(1).SetCellValue(ListInfo[i].ProductCode);
                rowtemp.CreateCell(2).SetCellValue(ListInfo[i].ProductName);
                rowtemp.CreateCell(3).SetCellValue(ListInfo[i].WangWang);
                rowtemp.CreateCell(4).SetCellValue(ListInfo[i].OderNumber);
                rowtemp.CreateCell(5).SetCellValue(ListInfo[i].Reason);
                rowtemp.CreateCell(6).SetCellValue(ListInfo[i].Unit);
                rowtemp.CreateCell(7).SetCellValue(double.Parse(ListInfo[i].UnitPrice.ToString()));
                rowtemp.CreateCell(8).SetCellValue(ListInfo[i].Qty);
                rowtemp.CreateCell(9).SetCellValue(double.Parse(ListInfo[i].Amount.ToString()));
                rowtemp.CreateCell(10).SetCellValue(ListInfo[i].Note);
                rowtemp.CreateCell(11).SetCellValue(ListInfo[i].PostUser);
                rowtemp.CreateCell(12).SetCellValue(ListInfo[i].Shop);
                rowtemp.CreateCell(13).SetCellValue(checkTime);
                rowtemp.CreateCell(14).SetCellValue(ListInfo[i].Code);
            }
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "报损报表.xls");
        }

        public partial class LossReportProduct
        {
            public int Oid { get; set; }
            public string Shop { get; set; }
            public string PostUser { get; set; }
            public string Reason { get; set; }
            public string Code { get; set; }
            public string ProductCode { get; set; }
            public string WangWang { get; set; }
            public int Qty { get; set; }
            public decimal UnitPrice { get; set; }
            public string OderNumber { get; set; }
            public string ProductName { get; set; }
            public DateTime PostTime { get; set; }
            public decimal Amount { get; set; }
        }
        /// <summary>
        /// 报损详情统计
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="Shop"></param>
        /// <param name="name"></param>
        /// <param name="statedate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetViewLossReportCountList(Lib.GridPager pager, string DetailsQuery, string name, string RetreatShop, string startDate, string endDate,string Warehouse)
        {

            IQueryable<LossReportProduct> queryData = null;
            string sql = "select  Warehouse, Oid,PostTime,PostUser,Shop,Code,ProductCode,ProductName,Qty,UnitPrice,Amount,WangWang,OderNumber,Reason From T_LossReport t inner join  T_LossReportDetail r on t.ID = r.Oid  where t.IsDelete = 0 ";
            if (!string.IsNullOrWhiteSpace(DetailsQuery))
            {
                sql += "  and   OderNumber like '" + DetailsQuery + "' or   WangWang like '%" + DetailsQuery + "%'";
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " and  (  PostUser like '" + name + "' or   Reason like '%" + name + "%')";
            }
            if (RetreatShop != null && RetreatShop != "")
            {
                sql += "  and  Shop='" + RetreatShop + "' ";

            }
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime sdate = DateTime.Parse(startDate + " 00:00:00");
                sql += " and  PostTime>='" + sdate + "'";
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime Edate = DateTime.Parse(endDate + " 23:59:59");
                sql += " and  PostTime<='" + Edate + "'";
            }
			if (!string.IsNullOrWhiteSpace(Warehouse))
			{
				sql += " and Warehouse='" + Warehouse+ "'";
			}
            queryData = db.Database.SqlQuery<LossReportProduct>(sql).AsQueryable();
            //构造新的list
            List<LossReportProduct> list = new List<LossReportProduct>();
            //构造一个单独的list用于查询总数。此总数不分页。
            List<LossReportProduct> zongjinelist = db.Database.SqlQuery<LossReportProduct>(sql).ToList();
            decimal sum = 0;
            foreach (var item in zongjinelist)
            {
                if (item.Qty > 0)
                {
                    if (item.UnitPrice != 0 && item.Qty != 0)
                    {
                        sum += (decimal)(item.UnitPrice * item.Qty);
                    }
                }
            }
            //总数据量
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                //分页
                queryData = queryData.OrderByDescending(c => c.Oid).Skip((pager.page - 1) * pager.rows).Take(pager.rows);

                foreach (var item in queryData)
                {
                    LossReportProduct i = new LossReportProduct();
                    i.Oid = item.Oid;
                    i.Shop = item.Shop;
                    i.UnitPrice = item.UnitPrice;
                    i.OderNumber = item.OderNumber;
                    i.PostUser = item.PostUser;
                    i.Qty = item.Qty;
                    i.ProductName = item.ProductName;
                    i.Amount = item.Amount;
                    i.WangWang = item.WangWang;
                    i.PostTime = item.PostTime;
                    i.Reason = item.Reason;
                    i.Code = item.Code;
                    i.ProductCode = item.ProductCode;
                    list.Add(i);
    }
            }
            var json = new
            {
                total = pager.totalRows,
                rows = (from r in list
                        select new LossReportProduct
                        {
                            Oid = r.Oid,
                            Reason = r.Reason,
                            Code = r.Code,
                            ProductCode = r.ProductCode,
                            PostUser = r.PostUser,
                            WangWang = r.WangWang,
                            Qty = r.Qty,
                            UnitPrice = r.UnitPrice,
                            Shop = r.Shop,
                            OderNumber = r.OderNumber,
                            ProductName = r.ProductName,
                            PostTime = r.PostTime,
                            Amount = r.Amount,
    }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //报损统计 
        [HttpPost]
        public ContentResult AllLossReportList(Lib.GridPager pager, string queryStr, int status)
        {

            IQueryable<T_LossReport> queryData = db.T_LossReport.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_LossReport> list = new List<T_LossReport>();
            foreach (var item in queryData)
            {
                T_LossReport i = new T_LossReport();
                i = item;
                i.Department = GetDaparementString(item.Department);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //打印
        public ActionResult ViewPrint(int id, int page = 1)
        {
            ViewData["id"] = id;
            T_LossReport model = db.T_LossReport.SingleOrDefault(a => a.ID == id);
            string pihao = model.Code;
            string Department = GetDaparementString(model.Department);
            ViewData["tTotal"] = model.Total;
            ViewData["Code"] = pihao;
            ViewData["Time"] = model.PostTime;
            ViewData["Shop"] = model.Shop;
            ViewData["Department"] = Department;
            if (!string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                try
                {
                    page = Convert.ToInt32(Request.QueryString["page"]);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            ViewData["page"] = page;
            string href = "";
            IQueryable<T_LossReportDetail> list = db.T_LossReportDetail.Where(a => a.Oid == id);
            int totalRows = list.Count();
            int P = 0;
            if (totalRows % 3 != 0)
                P = totalRows / 3 + 1;
            else
            {
                P = totalRows / 3;
            }
            for (int i = 1; i <= P; i++)
            {
                href += "<a href=\"?page=" + i + "&id=" + id + "\">   " + i + "   </a>";
            }
            ViewData["pager"] = href;
            ViewData["total"] = P;
            return View();
        }
        //打印数据
        public JsonResult getPrintData(int id, int page)
        {
            List<T_LossReportDetail> list = db.T_LossReportDetail.Where(a => a.Oid == id).ToList();
            List<T_LossReportDetail> modelList = null;
            T_LossReport model = db.T_LossReport.FirstOrDefault(s => s.ID == id);
            modelList = list.OrderByDescending(c => c.ID).Skip((page - 1) * 3).Take(3).ToList();
            var json = new
            {
                rows = (from m in modelList
                        select new
                        {
                            ProductCode = m.ProductCode,
                            ProductName = m.ProductName,
                            WangWang = m.WangWang,
                            OrderNumber = m.OderNumber,
                            // RetreatNumber = string.IsNullOrWhiteSpace(m.RetreatNumber) ? "" : "退款账号：" + m.RetreatNumber,
                            Reason = m.Reason,
                            Qty = m.Qty,
                            UnitPrice = m.UnitPrice,
                            Amount = m.Amount,
                            Note = string.IsNullOrWhiteSpace(m.Note) ? "" : "备注：" + m.Note
                        }).ToArray(),
                //OpenBank = "开户行：" + model.OpenBank,
                //Name = "姓名：" + model.Name
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        #endregion



        public partial class getExcels
        {
            public string PostUser { get; set; }
            public string Department { get; set; }
            public string Shop { get; set; }
            public string Code { get; set; }
            public DateTime PostTime { get; set; }
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string WangWang { get; set; }
            public string OderNumber { get; set; }
            public string Reason { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
            public string Note { get; set; }
            public int qty { get; set; }

			public string Warehouse { get; set; }




		}
        public FileResult getExcelManager(string queryStr, string statedate, string EndDate, string RetreatShop, string DetailsQuery,string Warehouse)
        {
            List<getExcels> queryData = null;
            int temID = 0;
            //显示当前用户的数据
            string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            if (!string.IsNullOrEmpty(statedate))
            {
                sdate = statedate + " 00:00:00";
            }
            if (!string.IsNullOrEmpty(EndDate))
            {
                edate = EndDate + " 23:59:59";
            }
            string sql = "select Warehouse, PostUser,(select Name From T_Department where  id=a.Department) as  Department,Shop,Code,PostTime,ProductCode,ProductName,WangWang,OderNumber,Reason,UnitPrice,Amount,Note,qty " +
                " From T_LossReport a left join T_LossReportDetail  b on a.ID = b.Oid where PostTime>='" + sdate + "' and PostTime<='" + edate + "'   and IsDelete=0  ";

           
            if (!string.IsNullOrWhiteSpace(DetailsQuery))
            {
                sql += "  and   OderNumber like '" + DetailsQuery + "' or   WangWang like '%" + DetailsQuery + "%'";
            }
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                sql += " and  (  PostUser like '" + queryStr + "' or   Reason like '%" + queryStr + "%')";
            }
            if (RetreatShop != null && RetreatShop != "")
            {
                sql += "  and  Shop='" + RetreatShop + "' ";

            }
			if (!string.IsNullOrWhiteSpace(Warehouse))
			{
				sql += " and Warehouse='" + Warehouse + "'";
			}
			queryData = db.Database.SqlQuery<getExcels>(sql).ToList();
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

                row1.CreateCell(0).SetCellValue("申请人");
                row1.CreateCell(1).SetCellValue("申请部门");
                row1.CreateCell(2).SetCellValue("店铺");
                row1.CreateCell(3).SetCellValue("报损编码");
                row1.CreateCell(4).SetCellValue("申请时间");
                row1.CreateCell(5).SetCellValue("产品代码");
                row1.CreateCell(6).SetCellValue("产品名称");
                row1.CreateCell(7).SetCellValue("旺旺号");
                row1.CreateCell(8).SetCellValue("订单编号");
                row1.CreateCell(9).SetCellValue("报损原因");
                row1.CreateCell(10).SetCellValue("单价");
                row1.CreateCell(11).SetCellValue("金额");
                row1.CreateCell(12).SetCellValue("备注");
                row1.CreateCell(13).SetCellValue("数量");
				row1.CreateCell(14).SetCellValue("仓库");
				for (int i = 0; i < queryData.Count; i++)
                {
                    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                        rowtemp.CreateCell(0).SetCellValue(queryData[i].PostUser.ToString());
                        rowtemp.CreateCell(1).SetCellValue(queryData[i].Department.ToString());
                        rowtemp.CreateCell(2).SetCellValue(queryData[i].Shop.ToString());
                        rowtemp.CreateCell(3).SetCellValue(queryData[i].Code.ToString());
                        rowtemp.CreateCell(4).SetCellValue(queryData[i].PostTime.ToString());
                        rowtemp.CreateCell(5).SetCellValue(queryData[i].ProductCode);
                        rowtemp.CreateCell(6).SetCellValue(queryData[i].ProductName);
                        rowtemp.CreateCell(7).SetCellValue(queryData[i].WangWang.ToString());
                        rowtemp.CreateCell(8).SetCellValue(queryData[i].OderNumber.ToString());
                        rowtemp.CreateCell(9).SetCellValue(queryData[i].Reason.ToString());
                        rowtemp.CreateCell(10).SetCellValue(queryData[i].UnitPrice.ToString());
                        rowtemp.CreateCell(11).SetCellValue((double)queryData[i].Amount);
                        rowtemp.CreateCell(12).SetCellValue(queryData[i].Note);
                        rowtemp.CreateCell(13).SetCellValue(queryData[i].qty);
					rowtemp.CreateCell(14).SetCellValue(queryData[i].Warehouse);

				}
                T_OperaterLog log = new T_OperaterLog()
                {
                    Module = "导出",
                    OperateContent = "导出报损Excel" + queryStr + statedate + EndDate +  RetreatShop+ DetailsQuery ,
                    Operater = Server.UrlDecode(Request.Cookies["Nickname"].Value),
                    OperateTime = DateTime.Now,
                    PID = 1
                };
                db.T_OperaterLog.Add(log);
                db.SaveChanges();

                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                book.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "报损列表导出.xls");
            }
            else
            {
                Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
                // 写入到客户端 
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Position = 0;
                return File(ms, "application/vnd.ms-excel", "报损列表导出.xls");
            }
        }
    }
}
