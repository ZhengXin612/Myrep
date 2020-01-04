/// 刘明

using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EBMS.Controllers
{
    /// <summary>
    /// 采购
    /// </summary>
    public class PurchaseController : BaseController
    {
        //
        // GET: /Purchase/

        #region 采购视图
        EBMSEntities db = new EBMSEntities();

        /// <summary>
        /// 产品采购凭证视图
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewProductPurchasePz()
        {
            ViewData["userName"] = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            return View();
        }

        /// <summary>
        /// 凭证费用
        /// </summary>
        public class PZCost
        {
            public string PZ_Department { get; set; }
            public Nullable<decimal> PZ_Money { get; set; }
        }

        /// <summary>
        /// 产品采购凭证列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewProductPurchasePzList()
        {
            ViewData["subject"] = Com.subject();
            ViewData["depart"] = Com.depart();
            return View();
        }

        /// <summary>
        /// 行政采购凭证
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewPurchaseExecutivePz()
        {
            ViewData["userName"] = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            return View();
        }


        /// <summary>
        /// 获取行政采购财务同意列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public ContentResult GetViewPurchaseExecutive(Lib.GridPager pager, string orderCode)
        {
            List<T_PurchaseExecutive> purchaseExecutiveList = new List<T_PurchaseExecutive>();
            IQueryable<T_PurchaseExecutiveApprove> Approvelist = db.T_PurchaseExecutiveApprove.Where(s => s.ApproveName.Contains("段志红") && s.Status == 1).AsQueryable();
            List<int?> itemIds = new List<int?>();
            foreach (var item in Approvelist.Select(s => new { itemId = s.Oid }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }
            foreach (var item in itemIds)
            {
                T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(item);
                if (model != null)
                    purchaseExecutiveList.Add(model);
            }
            purchaseExecutiveList = purchaseExecutiveList.Where(s => s.IsPzStatus != 1).ToList();
            if (!string.IsNullOrWhiteSpace(orderCode))
                purchaseExecutiveList = purchaseExecutiveList.Where(s => s.PurchaseOddNum.Equals(orderCode)).ToList();
            pager.totalRows = purchaseExecutiveList.Count();
            purchaseExecutiveList = purchaseExecutiveList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(purchaseExecutiveList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        ///获取行政采购详情列表
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public ActionResult ViewPuchaseExecutivePzDetailList(int oid)
        {
            ViewData["oid"] = oid;
            return View();
        }

        /// <summary>
        /// 凭证保存
        /// </summary>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewPzAddSave(string jsonStr, int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "数据有误" });

                    List<T_PurchaseExecutiveDetails> productList = Com.Deserialize<T_PurchaseExecutiveDetails>(jsonStr);
                    T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
                    foreach (var item in productList)
                    {
                        T_PurchaseExecutiveDetails product = db.T_PurchaseExecutiveDetails.Find(item.ID);
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

        /// <summary>
        /// 行政采购凭证列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewPurchaseExecutivePzList()
        {
            ViewData["subject"] = Com.subject();
            ViewData["depart"] = Com.depart();
            return View();
        }

        /// <summary>
        /// 获取产品采购凭证列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="sub"></param>
        /// <param name="dep"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public ContentResult GetPzList(string code, string sub, string dep, Lib.GridPager pager)
        {
            IQueryable<T_PZ_ProductPurchase> list = db.T_PZ_ProductPurchase.AsQueryable();
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
            List<T_PZ_ProductPurchase> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获取行政采购凭证列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="sub"></param>
        /// <param name="dep"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public ContentResult GetExecutivePzList(string code, string sub, string dep, Lib.GridPager pager)
        {
            IQueryable<T_PZ_PurchaseExecutive> list = db.T_PZ_PurchaseExecutive.AsQueryable();
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
            List<T_PZ_PurchaseExecutive> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
            return Content(json);
        }

        //一级主管
        public List<SelectListItem> GetFisrtNameForApprove()
        {
            //是否默认主管
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_User userManagers = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
            string DepartmentID = userManagers.DepartmentId;
            List<T_User> userList = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();

            var list = db.T_User.Where(a => a.IsManagers == "1").AsQueryable();
            int ID = 0;

            if (userList.Count > 0)
            {
                ID = userList[0].ID;
            }
            var selectList = new SelectList(list, "ID", "Name", ID);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.AddRange(selectList);
            return selecli;
        }

        [Description("访问我的采购页面")]
        public ActionResult ViewPurchase()
        {
            return View();
        }

        [Description("行政采购打印")]
        public ActionResult ViewPurchaseExcutivePrint(int id)
        {
            ViewData["Bid"] = id;
            T_PurchaseExecutive dmodel = db.T_PurchaseExecutive.SingleOrDefault(a => a.ID == id);
            ViewData["code"] = dmodel.PurchaseOddNum;
            return View();
        }

        [Description("产品管理")]
        public ActionResult ViewGoodsGY(int index)
        {
            ViewData["index"] = index;
            return View();
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
        [Description("访问采购收货页面")]
        public ActionResult ViewPurchaseWarehouse()
        {
            return View();
        }


        [Description("访问付款单审核页面")]
        public ActionResult ViewPurchasePaymentGoodsCheck()
        {
            string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
            ViewData["Name"] = Name;
            return View();
        }

        [Description("访问财务审核页面")]
        public ActionResult ViewFinanceTrialCheck(int ID)
        {
            T_PurchaseFinance Purchase = new T_PurchaseFinance();
            Purchase = db.T_PurchaseFinance.Single(a => a.ID == ID);
            string OddNum = Purchase.PurchaseOddNum;
            T_Purchase purmodel = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == OddNum);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>申请人</td><td>应付总金额</td><td>本次需支付金额</td><td>支付方式</td><td>申请备注</td></tr>";
            string tr = "";

            tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", Purchase.ApplyName, float.Parse(purmodel.CopelPay.ToString()), float.Parse(Purchase.needpay.ToString()), Purchase.paymenmode, Purchase.ApplyRemarks);

            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["approveid"] = ID;
            ViewData["paymentList"] = Lib.Comm.PaymenName;
            return View(Purchase);
        }

        [Description("付款单出纳审核页面页面")]
        public ActionResult ViewpaymentTrialCheck(int ID)
        {
            T_PurchaseFinance Purchase = new T_PurchaseFinance();
            Purchase = db.T_PurchaseFinance.Single(a => a.ID == ID);
            string OddNum = Purchase.PurchaseOddNum;
            T_Purchase purmodel = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == OddNum);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>申请人</td><td>应付总金额</td><td>已付款金额</td><td>本次申请支付金额</td><td>支付方式</td><td>申请备注</td><td>财务主管备注</td></tr>";
            string tr = "";

            tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>", Purchase.ApplyName, float.Parse(purmodel.CopelPay.ToString()), float.Parse(purmodel.ActualPay.ToString()), float.Parse(Purchase.needpay.ToString()), Purchase.paymenmode, Purchase.ApplyRemarks, Purchase.FinanceToexaminRemarks);

            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["approveid"] = ID;

            return View(Purchase);

        }
        [Description("访问采购未审核页面")]
        public ActionResult ViewPurchaseCheck()
        {
            string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
            T_PurchaseConfig Modelconfig = db.T_PurchaseConfig.SingleOrDefault(a => a.Name == Name);
            if (Modelconfig != null)
            {
                ViewData["ConfigType"] = Modelconfig.Type;
            }

            return View();
        }
        [Description("访问采购核价页面")]
        public ActionResult ViewPurchasePricing(int ID)
        {
            T_Purchase model = db.T_Purchase.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            //部门选择

            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", model.ApplyDepartment);
            List<SelectListItem> selecli = new List<SelectListItem>();
            ViewData["Code"] = model.Warehouse;
            ViewData["Warehouse"] = App_Code.Com.Warehouses(model.Warehouse);
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);

            ViewData["ID"] = ID;
            return View(model);
        }
        [Description("访问仓库收货确认页面")]
        public ActionResult ViewPurchaseWarehouseDetail(int ID)
        {
            T_Purchase model = db.T_Purchase.Find(ID);
            model.ApplyDepartment = GetDaparementString(model.ApplyDepartment);
            model.Warehouse = GetWarehouseString(model.Warehouse);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewData["ID"] = ID;
            return View(model);
        }
        [Description("访问采购已审核页面")]
        public ActionResult ViewPurchaseChecked()
        {
            return View();
        }


        [Description("访问付款单编辑")]
        public ActionResult ViewPurchasePaymentGoodsEdit(int ID)
        {

            T_PurchaseFinance FinanceModel = db.T_PurchaseFinance.SingleOrDefault(a => a.ID == ID);
            ViewData["paymenmodeList"] = Lib.Comm.Paymen;
            string OddNum = FinanceModel.PurchaseOddNum;
            T_Purchase purchase = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == OddNum);
            ViewData["CopelPay"] = purchase.CopelPay;
            ViewData["ActualPay"] = purchase.ActualPay;
            ViewData["OddNum"] = OddNum;
            ViewData["ID"] = ID;
            return View(FinanceModel);
        }


        [Description("访问采购审核页面")]
        public ActionResult ViewPurchaseReportCheck(int ID)
        {
            T_Purchase Purchase = new T_Purchase();
            Purchase = db.T_Purchase.Single(a => a.ID == ID);
            List<T_PurchaseApprove> approve = db.T_PurchaseApprove.Where(a => a.Oid == ID).ToList();
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
            return View(Purchase);

        }


        [Description("访问供应商页面")]
        public ActionResult ViewSupplier(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        [Description("采购人员申请付款页面")]
        public ActionResult ViewPurchasePaymentGoods()
        {
            return View();
        }
        [Description("访问新增付款单页面")]
        public ActionResult ViewPurchasePaymentGoodsAdd()
        {


            T_PurchaseFinance model = new T_PurchaseFinance();
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);

            model.ApplyName = name;
            ViewData["paymenmode"] = Lib.Comm.Paymen;
            return View(model);
        }
        [Description("访问供应商页面查询方法")]
        [HttpPost]
        public ContentResult GetSupplier(Lib.GridPager pager, string name)
        {
            IQueryable<T_Suppliers> queryData = db.T_Suppliers.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                queryData = queryData.Where(a => a.SuppliersName != null && a.SuppliersName.Contains(name));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Suppliers> list = new List<T_Suppliers>();
            foreach (var item in queryData)
            {
                T_Suppliers i = new T_Suppliers();

                i = item;

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }
        [Description("访问单位页面")]
        public ActionResult ViewCompany(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        [Description("访问采购查询")]
        public ActionResult ViewPurchaseQuery()
        {

            return View();
        }

        [Description("访问单位页面查询方法")]
        [HttpPost]
        public ContentResult GetCompany(Lib.GridPager pager, string name)
        {

            IQueryable<T_Company> queryData = db.T_Company.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                queryData = queryData.Where(a => a.CompanyName != null && a.CompanyName.Contains(name));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Company> list = new List<T_Company>();
            foreach (var item in queryData)
            {
                T_Company i = new T_Company();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }
        //[Description("访问我的采购审核历史页面")]
        //public ActionResult ViewPurchaseHistory(int tid)
        //{
        //    ViewData["ID"] = tid;
        //    T_Purchase model = db.T_Purchase.Find(tid);
        //    if (model == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View();
        //}
        [Description("访问采购编辑页面")]
        public ActionResult ViewPurchaseEdit(int ID)
        {
            T_Purchase model = db.T_Purchase.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            //部门选择

            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", model.ApplyDepartment);
            List<SelectListItem> selecli = new List<SelectListItem>();
            ViewData["Code"] = model.Warehouse;
            ViewData["Warehouse"] = App_Code.Com.Warehouses(model.Warehouse);
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);


            ViewData["adminId"] = model.ApproveFirst;
            ViewData["ApproveFirstList"] = GetFisrtNameForApprove();
            ViewData["ID"] = ID;
            return View(model);
        }

        [Description("产品采购审核页面")]
        public ActionResult ViewProductPurchasePzApprove(int id)
        {
            string number = db.T_PurchaseFinance.Find(id).PurchaseOddNum;
            ViewData["ID"] = db.T_Purchase.SingleOrDefault(s => s.PurchaseOddNum.Equals(number)).ID;
            ViewData["finaceID"] = id;
            return View();
        }

        [Description("访问我的采购详情页面")]
        public ActionResult ViewPurchaseListDetail(int tid)
        {
            ViewData["ID"] = tid;
            T_Purchase model = db.T_Purchase.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(tid);
            ViewData["LossReportCode"] = model.PurchaseOddNum;

            return View();
        }
        [Description("访问我的采购详情页面")]
        public ActionResult ViewPurchaseFinanceListDetail(string OddNum)
        {

            T_Purchase model = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == OddNum);
            ViewData["ID"] = model.ID;
            if (model == null)
            {
                return HttpNotFound();
            }
            //  GetApproveHistory(tid);
            ViewData["LossReportCode"] = OddNum;

            return View();
        }
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_PurchaseDetails> queryData = db.T_PurchaseDetails.Where(a => a.Purchase_ID == ID);
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
        //虚拟删除采购记录 
        [HttpPost]
        [Description("删除采购")]
        public JsonResult DeletePurchase(int del)
        {
            T_Purchase model = db.T_Purchase.Find(del);
            model.IsDelete = 1;
            db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

            //ModularByZP();

            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //作废采购记录
        [HttpPost]
        [Description("作废采购")]
        public JsonResult VoidPurchase(int ID)
        {
            T_Purchase model = db.T_Purchase.Find(ID);
            model.Status = 3;
            db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_PurchaseApprove.Where(a => a.Oid == id);
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
        [Description("访问采购新增页面")]
        public ActionResult ViewPurchaseAdd()
        {

            //选择审批人
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //自动生成批号
            string code = "CG-DS-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_Purchase> listLoss = db.T_Purchase.Where(a => a.PurchaseOddNum.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (listLoss.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = listLoss[0].PurchaseOddNum.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            ViewData["PurchaseOddNum"] = code;
            T_User modeuser = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
            //部门选择
            var list = db.T_Department.AsQueryable();
            string DepartmentId = "";
            if (modeuser == null)
            {
                DepartmentId = modeuser.DepartmentId;
            }
            var selectList = new SelectList(list, "Id", "Name", DepartmentId);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);

            ViewData["Warehouse"] = App_Code.Com.Warehouses();
            ViewData["ApproveFirst"] = GetFisrtNameForApprove();
            return View();
        }
        //我的采购列表  
        [HttpPost]
        public ContentResult ShowPurchaseList(Lib.GridPager pager, string queryStr, int status)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_Purchase> queryData = db.T_Purchase.Where(a => a.ApplyName == Nickname && a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Purchase> list = new List<T_Purchase>();
            foreach (var item in queryData)
            {
                T_Purchase i = new T_Purchase();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class PurchaseQuery
        {
            public int ID { get; set; }
            public string ApplyName { get; set; }
            public string ApplyDepartment { get; set; }
            public string ApplyReason { get; set; }
            public string PurchaseOddNum { get; set; }
            public System.DateTime ApplyDate { get; set; }
            public string Payment { get; set; }
            public string Warehouse { get; set; }
            public Nullable<decimal> CopelPay { get; set; }
            public string paymentMode { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public int IsDelete { get; set; }
            public Nullable<int> Ispayment { get; set; }
            public Nullable<int> IsReceived { get; set; }
            public Nullable<decimal> ActualPay { get; set; }
            public string ApproveFirst { get; set; }
            public decimal needpay { get; set; }
            public int cishu { get; set; }
        }
        //采购列表查询
        [HttpPost]
        public ContentResult ViewPurchaseQueryList(Lib.GridPager pager, string queryStr, int status)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<PurchaseQuery> queryData = db.Database.SqlQuery<PurchaseQuery>("select ID,ApplyName,ApplyDepartment,ApplyReason,PurchaseOddNum,ApplyDate,Payment,Warehouse,CopelPay,paymentMode,Status,Step,IsDelete,Ispayment,IsReceived,ActualPay,ApproveFirst,(select COUNT(*)  from T_PurchaseFinance where p.PurchaseOddNum=PurchaseOddNum ) as cishu,isnull((select SUM(needpay)  from T_PurchaseFinance where p.PurchaseOddNum=PurchaseOddNum and  ispayment='0' ),0) as  needpay from T_Purchase p where Step='2' and Ispayment!='2' and IsDelete='0'").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<PurchaseQuery> list = new List<PurchaseQuery>();
            foreach (var item in queryData)
            {
                PurchaseQuery i = new PurchaseQuery();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //采购详情列表 
        [HttpPost]
        public ContentResult GetPurchaseDetailList(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_PurchaseDetails> queryData = db.T_PurchaseDetails.Where(a => a.Purchase_ID == ID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseDetails> list = new List<T_PurchaseDetails>();
            foreach (var item in queryData)
            {
                T_PurchaseDetails i = new T_PurchaseDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //付款单查询采购详情列表 
        [HttpPost]
        public ContentResult GetPurchaseFinanceListDetail(Lib.GridPager pager, string queryStr, int ID)
        {


            IQueryable<T_PurchaseDetails> queryData = db.T_PurchaseDetails.Where(a => a.Purchase_ID == ID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseDetails> list = new List<T_PurchaseDetails>();
            foreach (var item in queryData)
            {
                T_PurchaseDetails i = new T_PurchaseDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //财务人员审核付款单查询      
        [HttpPost]
        public ContentResult GetPurchasePaymentGoodsCheck(Lib.GridPager pager, string queryStr)
        {

            string name = Server.UrlDecode(Request.Cookies["name"].Value);
            IQueryable<T_PurchaseFinance> queryData;
            if (name == "罗瑶")
            {
                queryData = db.T_PurchaseFinance.Where(a => a.isFinanceToexamine == 0 && a.IsDelte == 0 && a.ispayment == 0);
            }
            else
            {
                queryData = db.T_PurchaseFinance.Where(a => a.paymentName == name && a.isFinanceToexamine == 1 && a.IsDelte == 0 && a.ispayment == 0);
            }

            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseFinance> list = new List<T_PurchaseFinance>();
            foreach (var item in queryData)
            {
                T_PurchaseFinance i = new T_PurchaseFinance();
                i = item;

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 产品采购新增页面
        /// </summary>
        /// <param name="OddNum"></param>
        /// <returns></returns>
        public ActionResult ViewProcutPurchasePzDetailList(string OddNum, int finaId)
        {
            T_Purchase model = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == OddNum);
            ViewData["ID"] = model.ID;
            ViewData["finalId"] = finaId;
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewData["LossReportCode"] = OddNum;
            return View();
        }

        //采购人员申请付款查询      
        [HttpPost]
        public ContentResult CheckedPurchasePaymentGoods(Lib.GridPager pager, string queryStr, string pz)
        {

            string name = Server.UrlDecode(Request.Cookies["name"].Value);
            IQueryable<T_PurchaseFinance> queryData = db.T_PurchaseFinance.Where(a => a.ApplyName == name && a.IsDelte == 0);
            if (!string.IsNullOrWhiteSpace(pz))
                queryData = db.T_PurchaseFinance.Where(s => s.IsDelte == 0 && s.Is_PzStatus != 1 && s.ispayment == 1);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseFinance> list = new List<T_PurchaseFinance>();
            foreach (var item in queryData)
            {
                T_PurchaseFinance i = new T_PurchaseFinance();
                i = item;

                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 产品采购凭证保存
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewPurchaseExecutivePzAddSave(int ID, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "数据有误" });

                    List<T_PurchaseExecutiveDetails> productList = Com.Deserialize<T_PurchaseExecutiveDetails>(jsonStr);
                    T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
                    foreach (var item in productList)
                    {
                        T_PurchaseExecutiveDetails product = db.T_PurchaseExecutiveDetails.Find(item.ID);
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

        /// <summary>
        /// 行政采购审核页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ViewPurcharseExecutivePzApprove(int id)
        {
            ViewData["ID"] = id;
            return View();
        }

        //采购已审核      
        [HttpPost]
        public ContentResult CheckedPurchaseReportList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseApprove> ApproveMod = db.T_PurchaseApprove.Where(a => a.ApproveName == name && a.ApproveTime != null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Purchase> queryData = from r in db.T_Purchase
                                               where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != -1
                                               select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Purchase> list = new List<T_Purchase>();
            foreach (var item in queryData)
            {
                T_Purchase i = new T_Purchase();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public JsonResult PurchasePro(string id)
        {
            int ID = Convert.ToInt32(id);
            List<T_PurchaseExecutiveDetails> list = db.Database.SqlQuery<T_PurchaseExecutiveDetails>("select *,(select Warehouse from T_PurchaseExecutive where ID=d.Purchase_ID) as  diqucku from T_PurchaseExecutiveDetails d where d.Purchase_ID='" + ID + "'").ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult QueryPurchaseBYid(string id)
        {
            int ID = Convert.ToInt32(id);
            List<T_PurchaseExecutive> list = db.T_PurchaseExecutive.Where(a => a.ID == ID).ToList();
            string bumenname = "";
            string SRbeizhu = "";
            if (list.Count > 0)
            {

                int bumenid = int.Parse(list[0].ApplyDepartment);
                List<T_Department> bumen = db.T_Department.Where(a => a.ID == bumenid).ToList();
                bumenname = bumen[0].Name;
                List<T_PurchaseExecutiveApprove> beizhu = db.T_PurchaseExecutiveApprove.Where(a => a.ApproveName=="张思蓉" && a.Oid==ID).ToList();
                if (beizhu.Count > 0)
                {
                    SRbeizhu = beizhu[0].Memo;
                }
            }

            var json = new
            {
                rows = (from m in list
                        select new
                        {
                            Department = bumenname,
                            zhanghao = m.PurchaseOddNum,
                            thisdate = m.ApplyDate,
                            beizhu = SRbeizhu,
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //待审核数据列表
        [HttpPost]
        public ContentResult UnCheckPurchaseReportList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseApprove> ApproveMod = db.T_PurchaseApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Purchase> queryData = from r in db.T_Purchase
                                               where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                               select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Purchase> list = new List<T_Purchase>();
            foreach (var item in queryData)
            {
                T_Purchase i = new T_Purchase();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //仓库未审核数据列表
        [HttpPost]
        public ContentResult PurchaseWarehouseList(Lib.GridPager pager, string queryStr)
        {
            // string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseApprove> ApproveMod = db.T_PurchaseApprove.Where(a => a.ApproveName == "等待中" && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Purchase> queryData = from r in db.T_Purchase
                                               where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2) && (r.IsReceived == 0 || r.IsReceived == 1)
                                               select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Purchase> list = new List<T_Purchase>();
            foreach (var item in queryData)
            {
                T_Purchase i = new T_Purchase();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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




        #endregion

        #region Post 操作
        //接收JSON 反序列化
        public static List<T> Deserialize<T>(string text)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                List<T> list = (List<T>)js.Deserialize(text, typeof(List<T>));
                return list;

            }
            catch (Exception)
            {

                return null;
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
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //采购新增保存
        [HttpPost]
        [Description("采购新增保存")]
        public JsonResult PurchaseAddSave(T_Purchase model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_PurchaseDetails> details = Deserialize<T_PurchaseDetails>(jsonStr);
                    //主表保存
                    model.ApplyDate = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;

                    model.IsDelete = 0;
                    model.Ispayment = 0;
                    model.IsReceived = 0;
                    model.ActualPay = 0;
                    T_LossReport lossMod = new T_LossReport();
                    db.T_Purchase.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_PurchaseApprove PurchaseApprove = new T_PurchaseApprove();
                        PurchaseApprove.Status = -1;
                        PurchaseApprove.ApproveName = GetUserNameById(model.ApproveFirst);
                        PurchaseApprove.Memo = "";
                        PurchaseApprove.Oid = model.ID;
                        db.T_PurchaseApprove.Add(PurchaseApprove);
                        db.SaveChanges();

                        foreach (var item in details)
                        {
                            item.Purchase_ID = model.ID;
                            item.ActualNum = 0;
                            db.T_PurchaseDetails.Add(item);
                        }
                        db.SaveChanges();
                       //ModularByZP();


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
        [Description("采购编辑保存")]
        public JsonResult ViewPurchaseEditSave(T_Purchase model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_PurchaseDetails> details = Deserialize<T_PurchaseDetails>(jsonStr);
                    //主表保存
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    T_Purchase PurMod = db.T_Purchase.Find(editID);
                    PurMod.Warehouse = model.Warehouse;
                    PurMod.ApproveFirst = model.ApproveFirst;
                    PurMod.ApplyDepartment = model.ApplyDepartment;
                    PurMod.ApplyReason = model.ApplyReason;
                    db.Entry<T_Purchase>(PurMod).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        //修改审核  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                        T_PurchaseApprove ApproveMod = db.T_PurchaseApprove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (ApproveMod == null)
                        {
                            //不同意修改
                            T_PurchaseApprove Approvemodel = new T_PurchaseApprove();
                            Approvemodel.Status = -1;
                            Approvemodel.ApproveName = GetUserNameById(model.ApproveFirst);
                            Approvemodel.Memo = "";
                            Approvemodel.Oid = model.ID;
                            db.T_PurchaseApprove.Add(Approvemodel);
                            db.SaveChanges();
                        }
                        else
                        {
                            //新增未审批的修改
                            ApproveMod.ApproveName = GetUserNameById(model.ApproveFirst);
                            db.Entry<T_PurchaseApprove>(ApproveMod).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //删除oid==id原有的详情表记录
                        List<T_PurchaseDetails> delMod = db.T_PurchaseDetails.Where(a => a.Purchase_ID == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_PurchaseDetails.Remove(item);
                        }
                        db.SaveChanges();

                        //添加新的详情
                        foreach (var item in details)
                        {
                            item.Purchase_ID = model.ID;
                            db.T_PurchaseDetails.Add(item);
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

        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "产品采购").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_PurchaseApprove where Oid in (select ID from T_Purchase where (Status=-1 or Status=0) and IsDelete=0) and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "产品采购" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "产品采购";
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
            string RejectNumberSql = "select ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Purchase where Status='2' and IsDelete=0  GROUP BY ApplyName  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "产品采购" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "产品采购";
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
        public void ModularByZPK()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "行政采购").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from  T_PurchaseExecutiveApprove where Oid in (select ID from T_PurchaseExecutive where(Status = -1 or Status = 0) and IsDelete = 0) and Status = -1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "行政采购" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "行政采购";
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
            string RejectNumberSql = "select ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_PurchaseExecutive where Status='2' and IsDelete=0  GROUP BY ApplyName  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "行政采购" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "行政采购";
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
        /// 产品采购审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ProductPzCheck(int id, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_PurchaseFinance finance = db.T_PurchaseFinance.Find(id);
                    finance.Is_PzStatus = status;
                    finance.Pz_Memo = memo;
                    db.SaveChanges();
                    T_Purchase purchase = db.T_Purchase.SingleOrDefault(s => s.PurchaseOddNum.Equals(finance.PurchaseOddNum));
                    if (status == 1)
                    {
                        List<T_PurchaseDetails> productList = db.T_PurchaseDetails.Where(s => s.Purchase_ID == purchase.ID).ToList();
                        foreach (var item in productList)
                        {
                            T_PZ_ProductPurchase purchasePz = new T_PZ_ProductPurchase
                            {
                                PZ_OrderNum = purchase.PurchaseOddNum,
                                PZ_Summary = item.Supplier,
                                PZ_Department = item.PZ_Department,
                                PZ_Direction = item.PZ_Direction,
                                PZ_Money = item.UnitPrice * Convert.ToInt32(item.ActualNum),
                                PZ_Subject = item.PZ_Subject,
                                PZ_Time = DateTime.Now
                            };
                            db.T_PZ_ProductPurchase.Add(purchasePz);
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

        /// <summary>
        /// 行政采购审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ExecutivePzCheck(int id, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_PurchaseExecutive executive = db.T_PurchaseExecutive.Find(id);
                    executive.IsPzStatus = status;
                    executive.PzMemo = memo;
                    db.SaveChanges();
                    if (status == 1)
                    {
                        List<T_PurchaseExecutiveDetails> productList = db.T_PurchaseExecutiveDetails.Where(s => s.Purchase_ID == id).ToList();
                        foreach (var item in productList)
                        {
                            T_PZ_ProductPurchase purchasePz = new T_PZ_ProductPurchase
                            {
                                PZ_OrderNum = executive.PurchaseOddNum,
                                PZ_Summary = item.Supplier,
                                PZ_Department = item.PZ_Department,
                                PZ_Direction = item.PZ_Direction,
                                PZ_Money = item.UnitPrice * Convert.ToInt32(item.ActualNum),
                                PZ_Subject = item.PZ_Subject,
                                PZ_Time = DateTime.Now
                            };
                            db.T_PZ_ProductPurchase.Add(purchasePz);
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

        //审核操作
        [HttpPost]
        [Description("采购审核")]
        public JsonResult PurchaseReportCheck(int id, int status, string memo, string checkMan)
        {
            T_PurchaseApprove modelApprove = db.T_PurchaseApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
            string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
            string result = "";
            if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            modelApprove.Memo = memo;
            modelApprove.ApproveTime = DateTime.Now;
            modelApprove.Status = status;
            db.Entry<T_PurchaseApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                T_Purchase model = db.T_Purchase.Find(id);
                T_PurchaseApprove newApprove = new T_PurchaseApprove();
                if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                if (status == 1)
                {
                    //同意
                    int step = int.Parse(model.Step.ToString());
                    step++;
                    IQueryable<T_PurchaseConfig> config = db.T_PurchaseConfig.AsQueryable();
                    int stepLength = config.Count();//总共步骤
                    if (step < stepLength)
                    {
                        //不是最后一步，主表状态为0 =>审核中
                        model.Status = 0;
                        T_PurchaseConfig stepMod = db.T_PurchaseConfig.SingleOrDefault(a => a.Step == step);
                        string nextName = stepMod.Name;
                        //下一步审核人不是null  审核记录插入一条新纪录
                        newApprove.Memo = "";
                        newApprove.Oid = id;
                        newApprove.Status = -1;
                        if (nextName != null)
                        {
                            newApprove.ApproveName = nextName;
                        }

                        db.T_PurchaseApprove.Add(newApprove);
                        db.SaveChanges();
                        //给采购发信息
                        if (stepMod.Type == "采购")
                        {
                            string[] msg = new string[] { "采购未审核："};
                            //13807317408
                            string res = Lib.SendSMS.Send(msg, "162067", "13807317408");
                        }

                    }
                    else
                    {
                        //最后一步，主表状态改为 1 => 同意
                        model.Status = status;
                    }
                    model.Step = step;
                    db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
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
                    db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
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

           

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        [Description("财务审核")]
        public JsonResult PaymentTrialCheck(int id, int status, string memo, string payment)
        {
            T_PurchaseFinance Finance = db.T_PurchaseFinance.SingleOrDefault(a => a.ID == id);
            Finance.isFinanceToexamine = status;
            Finance.FinanceToexaminedate = DateTime.Now;
            Finance.FinanceToexaminRemarks = memo;
            Finance.paymentName = payment;
            db.Entry<T_PurchaseFinance>(Finance).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            string result = "";
            if (i > 0)
            {
                result = "保存成功";
            }
            else
            {
                result = "保存失败";
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Description("出纳审核")]
        public JsonResult PaymentCheck(int id, int status, string memo, string payment)
        {
            T_PurchaseFinance Finance = db.T_PurchaseFinance.SingleOrDefault(a => a.ID == id);
            Finance.ispayment = status;
            Finance.payment = decimal.Parse(payment);
            Finance.paymendate = DateTime.Now;
            Finance.paymenRemarks = memo;
            db.Entry<T_PurchaseFinance>(Finance).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

            string result = "";
            if (i > 0)
            {
                string oddNum = Finance.PurchaseOddNum;
                T_Purchase Pmodel = db.T_Purchase.SingleOrDefault(a => a.PurchaseOddNum == oddNum);
                decimal ActualPay = decimal.Parse(Pmodel.ActualPay.ToString()) + decimal.Parse(payment);


                if (ActualPay >= Pmodel.CopelPay)
                {
                    Pmodel.Ispayment = 2;
                    //改变表

                    //如果收货情况也是2（收货已完成）的话就改变审核数据
                    if (Pmodel.IsReceived == 2)
                    {
                        T_PurchaseApprove modelApprove = db.T_PurchaseApprove.SingleOrDefault(a => a.Oid == Pmodel.ID && a.ApproveTime == null);
                        modelApprove.ApproveTime = DateTime.Now;
                        modelApprove.Status = 1;
                        db.Entry<T_PurchaseApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //并且改变主记录的，让主记录的步奏和状态都改成已完成。步奏（3）代表已完成。状态1代表结束
                        Pmodel.Status = 1;
                        Pmodel.Step = 3;
                    }

                }
                else
                {
                    Pmodel.Ispayment = 1;
                }
                Pmodel.ActualPay = ActualPay;
                db.Entry<T_Purchase>(Pmodel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                result = "保存成功";
            }
            else
            {
                result = "保存失败";
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public partial class SumPurchaseFinance
        {

            public decimal payment { get; set; }

        }
        //编辑保存
        [HttpPost]
        [Description("采购核价保存")]
        public JsonResult PurchasePricingSave(T_Purchase Purchasemodel, string jsonStr, int fkstatus, string memo)
        {
            string result = "";
            using (TransactionScope sc = new TransactionScope())
            {
                int id = Purchasemodel.ID;
                T_PurchaseApprove modelApprove = db.T_PurchaseApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                modelApprove.Memo = memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = fkstatus;
                db.Entry<T_PurchaseApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    T_Purchase model = db.T_Purchase.Find(id);
                    if (fkstatus == 1)
                    {
                        try
                        {
                            List<T_PurchaseDetails> details = Deserialize<T_PurchaseDetails>(jsonStr);
                            //主表保存
                            int editStatus = Purchasemodel.Status;//原记录的状态
                            int editID = Purchasemodel.ID;//原记录的ID
                            T_Purchase PurMod = db.T_Purchase.Find(editID);
                            PurMod.Warehouse = Purchasemodel.Warehouse;
                            PurMod.CopelPay = Purchasemodel.CopelPay;
                            PurMod.ApplyReason = Purchasemodel.ApplyReason;
                            db.Entry<T_Purchase>(PurMod).State = System.Data.Entity.EntityState.Modified;
                            int x = db.SaveChanges();
                            if (x > 0)
                            {
                                //删除oid==id原有的详情表记录
                                List<T_PurchaseDetails> delMod = db.T_PurchaseDetails.Where(a => a.Purchase_ID == editID).ToList();
                                foreach (var item in delMod)
                                {
                                    db.T_PurchaseDetails.Remove(item);
                                }
                                db.SaveChanges();

                                //添加新的详情
                                foreach (var item in details)
                                {
                                    item.Purchase_ID = Purchasemodel.ID;
                                    db.T_PurchaseDetails.Add(item);
                                }
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                        }




                        T_PurchaseApprove newApprove = new T_PurchaseApprove();
                        if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }

                        //同意
                        int step = int.Parse(model.Step.ToString());
                        step++;
                        IQueryable<T_PurchaseConfig> config = db.T_PurchaseConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            //不是最后一步，主表状态为0 =>审核中
                            model.Status = 0;
                            T_PurchaseConfig stepMod = db.T_PurchaseConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            //下一步审核人不是null  审核记录插入一条新纪录
                            newApprove.Memo = "";
                            newApprove.Oid = id;
                            newApprove.Status = -1;
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                            }

                            db.T_PurchaseApprove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            //最后一步，主表状态改为 1 => 同意
                            model.Status = fkstatus;
                        }
                        model.Step = step;
                        db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
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
                        db.Entry<T_Purchase>(model).State = System.Data.Entity.EntityState.Modified;
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

        public partial class QueryPurchaseDetails
        {
            public int ID { get; set; }
            public double Num { get; set; }

        }

        //收货确认保存
        [HttpPost]
        [Description("收货确认保存")]
        public JsonResult PurchaseWarehouseDetailSave(T_Purchase Purchasemodel, string detailList, int fkstatus, string memo)
        {
            string result = "";
            using (TransactionScope sc = new TransactionScope())
            {
                //得到采购主表ID
                int PID = Purchasemodel.ID;
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                List<QueryPurchaseDetails> details = Deserialize<QueryPurchaseDetails>(detailList);
                //循环前台提交的详情
                for (int x = 0; x < details.Count; x++)
                {
                    //给采购详情表修改收货数量和最后收货时间
                    int DetID = details[x].ID;
                    T_PurchaseDetails DetailsModel = db.T_PurchaseDetails.SingleOrDefault(a => a.ID == DetID);
                    double ActualNum = 0;
                    if (DetailsModel.ActualNum != null)
                    {
                        ActualNum = double.Parse(DetailsModel.ActualNum.ToString());
                    }
                    DetailsModel.ActualNum = details[x].Num + ActualNum;
                    DetailsModel.ActualDate = DateTime.Now;
                    db.Entry<T_PurchaseDetails>(DetailsModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //采购收货记录表添加当前收货记录
                    T_PurchaseCollect CollectModel = new T_PurchaseCollect();
                    CollectModel.PurchaseID = PID;
                    CollectModel.PurchaseDetailsID = DetID;
                    CollectModel.ThisNum = details[x].Num;
                    CollectModel.CollectName = curName;
                    CollectModel.CollectDate = DateTime.Now;
                    db.T_PurchaseCollect.Add(CollectModel);
                    db.SaveChanges();
                }
                //通过采购主表ID查询采购主记录
                T_Purchase pmodel = db.T_Purchase.SingleOrDefault(a => a.ID == PID);
                //通过采购主表ID查询采购详情记录
                List<T_PurchaseDetails> Dmodel = db.T_PurchaseDetails.Where(a => a.Purchase_ID == PID).ToList();
                //临时变量储存收货数量和采购数量相等的
                int xx = 0;
                //循环判断采购详情表，收货和采购数量是否相等
                for (int z = 0; z < Dmodel.Count; z++)
                {

                    if (Dmodel[z].PlanNum <= Dmodel[z].ActualNum)
                    {
                        xx++;
                    }
                }
                //如果采购数量和收货数量相等，那么把主表的是否收货完成字段改成已完成（2）
                if (Dmodel.Count == xx)
                {
                    pmodel.IsReceived = 2;
                    //如果支付情况也是2（支付已完成）的话就改变审核数据
                    if (pmodel.Ispayment == 2)
                    {
                        T_PurchaseApprove modelApprove = db.T_PurchaseApprove.SingleOrDefault(a => a.Oid == PID && a.ApproveTime == null);
                        modelApprove.ApproveTime = DateTime.Now;
                        modelApprove.Status = 1;
                        db.Entry<T_PurchaseApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //并且改变主记录的，让主记录的步奏和状态都改成已完成。步奏（3）代表已完成。状态1代表结束
                        pmodel.Status = 1;
                        pmodel.Step = 3;
                    }
                }
                else
                {
                    //如果采购数量和收货数不一致的情况下，只改变收货状态（1）已部分收货
                    pmodel.IsReceived = 1;
                }
                db.Entry<T_Purchase>(pmodel).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {

                    result = "保存成功";
                }
                else
                {
                    result = "保存失败";
                }
                sc.Complete();
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        [Description("付款单申请保存")]
        public JsonResult PurchaseFinanceAdd(T_PurchaseFinance PurchaseFinance, string Purchase)
        {
            string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
            PurchaseFinance.PurchaseOddNum = Purchase;
            PurchaseFinance.ApplyName = Name;
            PurchaseFinance.ApplyDate = DateTime.Now;
            PurchaseFinance.FinanceToexamineName = "罗瑶";
            PurchaseFinance.isFinanceToexamine = 0;
            PurchaseFinance.ispayment = 0;
            PurchaseFinance.IsDelte = 0;
            PurchaseFinance.Is_PzStatus = 0;
            db.T_PurchaseFinance.Add(PurchaseFinance);
            int i = db.SaveChanges();
            string result = "";
            if (i > 0)
            {
                result = "保存成功";
            }
            else
            {
                result = "保存失败";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Description("付款单编辑保存")]
        public JsonResult PurchaseFinanceEdit(T_PurchaseFinance PurchaseFinance, int ID)
        {
            T_PurchaseFinance PurchaseFinanceEdit = db.T_PurchaseFinance.SingleOrDefault(a => a.ID == ID);
            PurchaseFinanceEdit.needpay = PurchaseFinance.needpay;
            PurchaseFinanceEdit.paymentaccounts = PurchaseFinance.paymentaccounts;
            PurchaseFinanceEdit.paymenmode = PurchaseFinance.paymenmode;
            PurchaseFinanceEdit.ApplyRemarks = PurchaseFinance.ApplyRemarks;
            PurchaseFinanceEdit.isFinanceToexamine = 0;
            db.Entry<T_PurchaseFinance>(PurchaseFinanceEdit).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            string result = "";
            if (i > 0)
            {
                result = "保存成功";
            }
            else
            {
                result = "保存失败";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //虚拟删除付款单记录 
        [HttpPost]
        [Description("删除付款单")]
        public JsonResult DeletePurchaseFinance(int del)
        {
            T_PurchaseFinance model = db.T_PurchaseFinance.Find(del);
            model.IsDelte = 1;
            db.Entry<T_PurchaseFinance>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }


        #endregion
        #region 行政采购视图

        [Description("访问行政采购编辑页面")]
        public ActionResult ViewPurchaseExecutiveEdit(int ID)
        {
            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            //部门选择

            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", model.ApplyDepartment);
            List<SelectListItem> selecli = new List<SelectListItem>();
            ViewData["Code"] = model.Warehouse;
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);


            ViewData["adminId"] = model.ApproveFirst;
            ViewData["ApproveFirstList"] = GetFisrtNameForApprove();
            ViewData["ID"] = ID;
            return View(model);
        }

        [Description("访问我的行政采购页面")]
        public ActionResult ViewPurchaseExecutive()
        {
            return View();
        }

        [Description("访问行政采购核价页面")]
        public ActionResult ViewPurchaseExecutivePricing(int ID)
        {
            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            //部门选择

            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", model.ApplyDepartment);
            List<SelectListItem> selecli = new List<SelectListItem>();
            ViewData["Code"] = model.Warehouse;

            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);

            ViewData["ID"] = ID;
            return View(model);
        }
        [Description("访问行政采购审核页面")]
        public ActionResult ViewPurchaseExecutiveReportCheck(int ID)
        {
            T_PurchaseExecutive Purchase = new T_PurchaseExecutive();
            Purchase = db.T_PurchaseExecutive.Single(a => a.ID == ID);
            List<T_PurchaseExecutiveApprove> approve = db.T_PurchaseExecutiveApprove.Where(a => a.Oid == ID).ToList();
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
            return View(Purchase);

        }
        [Description("访问行政采购未审核页面")]
        public ActionResult ViewPurchaseExecutiveCheck()
        {
            string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
            T_PurchaseExecutiveConfig Modelconfig = db.T_PurchaseExecutiveConfig.SingleOrDefault(a => a.Name.Contains(Name));
            if (Modelconfig != null)
            {
                ViewData["ConfigType"] = Modelconfig.Type;
            }

            return View();

        }
        [Description("访问行政采购仓库收货详情")]
        public ActionResult ViewPurchaseExecutiveWarehouseDetail(int ID)
        {

            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
            model.ApplyDepartment = GetDaparementString(model.ApplyDepartment);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewData["ID"] = ID;
            return View(model);

        }

        [Description("访问行政采购已审核")]
        public ActionResult ViewPurchaseExecutiveChecked()
        {


            return View();
        }
        [Description("访问行政采购仓库收货")]
        public ActionResult ViewPurchaseExecutiveWarehouse()
        {


            return View();
        }
        [Description("访问行政采购详情页面")]
        public ActionResult ViewPurchaseExecutiveDetails(int tid)
        {
            ViewData["ID"] = tid;
            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveExecutiveHistory(tid);
            ViewData["LossReportCode"] = model.PurchaseOddNum;

            return View();
        }
        [Description("访问行政采购新增页面")]
        public ActionResult ViewPurchaseExecutiveAdd()
        {
            //选择审批人
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //自动生成批号
            string code = "CG-DS-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_PurchaseExecutive> listLoss = db.T_PurchaseExecutive.Where(a => a.PurchaseOddNum.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (listLoss.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = listLoss[0].PurchaseOddNum.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            ViewData["PurchaseOddNum"] = code;
            T_User modeuser = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
            //部门选择
            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Id", "Name", modeuser.DepartmentId);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择部门", Value = "9999" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);


            ViewData["ApproveFirst"] = GetFisrtNameForApprove();
            return View();

        }

        //获取审核详情记录
        private void GetApproveExecutiveHistory(int id = 0)
        {
            var history = db.T_PurchaseExecutiveApprove.Where(a => a.Oid == id);
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
        //编辑获取详情列表  
        public JsonResult EditExecutiveEditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_PurchaseExecutiveDetails> queryData = db.T_PurchaseExecutiveDetails.Where(a => a.Purchase_ID == ID);
            pager.totalRows = queryData.Count();

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData.ToList(), Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Description("删除采购")]
        public JsonResult DeletePurchaseExecutive(int del)
        {
            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(del);
            model.IsDelete = 1;
            db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            //ModularByZPK();
            return Json(i, JsonRequestBehavior.AllowGet);

        }
        //行政采购新增保存
        [HttpPost]
        [Description("行政采购新增保存")]
        public JsonResult PurchaseExecutiveAddSave(T_PurchaseExecutive model, string detailList)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_PurchaseExecutiveDetails> details = Deserialize<T_PurchaseExecutiveDetails>(detailList);
                    //主表保存
                    model.ApplyDate = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.IsDelete = 0;
                    model.ActualPay = 0;
                    model.IsPzStatus = 0;
                    db.T_PurchaseExecutive.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_PurchaseExecutiveApprove PurchaseApprove = new T_PurchaseExecutiveApprove();
                        PurchaseApprove.Status = -1;
                        PurchaseApprove.ApproveName = GetUserNameById(model.ApproveFirst);
                        PurchaseApprove.Memo = "";
                        PurchaseApprove.Oid = model.ID;
                        db.T_PurchaseExecutiveApprove.Add(PurchaseApprove);
                        db.SaveChanges();
                        foreach (var item in details)
                        {
                            item.Purchase_ID = model.ID;
                            item.ActualNum = 0;
                            db.T_PurchaseExecutiveDetails.Add(item);
                        }
                        db.SaveChanges();
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    //ModularByZPK();
                    return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //行政采购待审核数据列表
        [HttpPost]
        public ContentResult UnCheckPurchaseExecutiveReportList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseExecutiveApprove> ApproveMod = db.T_PurchaseExecutiveApprove.Where(a => a.ApproveName.Contains(name) && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_PurchaseExecutive> queryData = from r in db.T_PurchaseExecutive
                                                        where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                                        select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseExecutive> list = new List<T_PurchaseExecutive>();
            foreach (var item in queryData)
            {
                T_PurchaseExecutive i = new T_PurchaseExecutive();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //我的行政采购列表  
        [HttpPost]
        public ContentResult ShowPurchaseExecutiveList(Lib.GridPager pager, string queryStr, int status)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_PurchaseExecutive> queryData = db.T_PurchaseExecutive.Where(a => a.ApplyName == Nickname && a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseExecutive> list = new List<T_PurchaseExecutive>();
            foreach (var item in queryData)
            {
                T_PurchaseExecutive i = new T_PurchaseExecutive();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                i.Warehouse = GetWarehouseString(item.Warehouse);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //行政采购详情列表 
        [HttpPost]
        public ContentResult GetPurchaseExecutiveDetails(Lib.GridPager pager, string queryStr, int ID)
        {
            IQueryable<T_PurchaseExecutiveDetails> queryData = db.T_PurchaseExecutiveDetails.Where(a => a.Purchase_ID == ID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseExecutiveDetails> list = new List<T_PurchaseExecutiveDetails>();
            foreach (var item in queryData)
            {
                T_PurchaseExecutiveDetails i = new T_PurchaseExecutiveDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        //作废采购记录
        [HttpPost]
        [Description("作废采购")]
        public JsonResult VoidPurchaseExecutive(int ID)
        {
            T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(ID);
            model.Status = 3;
            db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

            //ModularByZPK();

            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //行政采购编辑保存
        [HttpPost]
        [Description("采购编辑保存")]
        public JsonResult ViewPurchaseExecutiveEditSave(T_PurchaseExecutive model, string detailList)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_PurchaseExecutiveDetails> details = Deserialize<T_PurchaseExecutiveDetails>(detailList);
                    //主表保存
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    T_PurchaseExecutive PurMod = db.T_PurchaseExecutive.Find(editID);
                    PurMod.Warehouse = model.Warehouse;
                    PurMod.ApproveFirst = model.ApproveFirst;
                    PurMod.ApplyDepartment = model.ApplyDepartment;
                    PurMod.ApplyReason = model.ApplyReason;
                    PurMod.Status = -1;
                    db.Entry<T_PurchaseExecutive>(PurMod).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        //修改审核  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                        T_PurchaseExecutiveApprove ApproveMod = db.T_PurchaseExecutiveApprove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (ApproveMod == null)
                        {
                            //不同意修改
                            T_PurchaseExecutiveApprove Approvemodel = new T_PurchaseExecutiveApprove();
                            Approvemodel.Status = -1;
                            Approvemodel.ApproveName = GetUserNameById(model.ApproveFirst);
                            Approvemodel.Memo = "";
                            Approvemodel.Oid = model.ID;
                            
                            db.T_PurchaseExecutiveApprove.Add(Approvemodel);
                            db.SaveChanges();
                        }
                        else
                        {
                            //新增未审批的修改
                            ApproveMod.ApproveName = GetUserNameById(model.ApproveFirst);
                            db.Entry<T_PurchaseExecutiveApprove>(ApproveMod).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //删除oid==id原有的详情表记录
                        List<T_PurchaseExecutiveDetails> delMod = db.T_PurchaseExecutiveDetails.Where(a => a.Purchase_ID == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_PurchaseExecutiveDetails.Remove(item);
                        }
                        db.SaveChanges();

                        //添加新的详情
                        foreach (var item in details)
                        {
                            item.Purchase_ID = model.ID;
                            db.T_PurchaseExecutiveDetails.Add(item);
                        }
                        db.SaveChanges();
                        //ModularByZPK();
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

        //审核操作
        [HttpPost]
        [Description("采购审核")]
        public JsonResult PurchaseExecutiveReportCheck(int id, int status, string memo, string checkMan)
        {
            T_PurchaseExecutiveApprove modelApprove = db.T_PurchaseExecutiveApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
            string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
            string result = "";
            if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }


            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            modelApprove.ApproveName = name;
            modelApprove.Memo = memo;
            modelApprove.ApproveTime = DateTime.Now;
            modelApprove.Status = status;
            db.Entry<T_PurchaseExecutiveApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(id);
                T_PurchaseExecutiveApprove newApprove = new T_PurchaseExecutiveApprove();
                if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                if (status == 1)
                {
                    //同意
                    int step = int.Parse(model.Step.ToString());
                    step++;
                    IQueryable<T_PurchaseExecutiveConfig> config = db.T_PurchaseExecutiveConfig.AsQueryable();
                    int stepLength = config.Count();//总共步骤
                    if (step < stepLength)
                    {
                        //不是最后一步，主表状态为0 =>审核中
                        model.Status = 0;
                        T_PurchaseExecutiveConfig stepMod = db.T_PurchaseExecutiveConfig.SingleOrDefault(a => a.Step == step);
                        string nextName = stepMod.Name;
                        //下一步审核人不是null  审核记录插入一条新纪录
                        newApprove.Memo = "";
                        newApprove.Oid = id;
                        newApprove.Status = -1;
                        if (nextName != null)
                        {
                            newApprove.ApproveName = nextName;
                        }

                        db.T_PurchaseExecutiveApprove.Add(newApprove);
                        db.SaveChanges();
                    }
                    else
                    {
                        //最后一步，主表状态改为 1 => 同意
                        model.Status = status;
                    }
                    model.Step = step;
                    db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
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
                    db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //审核流程结束 申请人编辑后插入下一条记录 
                    result = "保存成功";
                }
            }
            else
            {
                result = "保存失败";
            }
            //ModularByZPK();
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        //采购已审核      
        [HttpPost]
        public ContentResult CheckedPurchaseExecutiveReportList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseExecutiveApprove> ApproveMod = db.T_PurchaseExecutiveApprove.Where(a => a.ApproveName.Contains(name) && a.ApproveTime != null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_PurchaseExecutive> queryData = from r in db.T_PurchaseExecutive
                                                        where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != -1
                                                        select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseExecutive> list = new List<T_PurchaseExecutive>();
            foreach (var item in queryData)
            {
                T_PurchaseExecutive i = new T_PurchaseExecutive();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //编辑保存
        [HttpPost]
        [Description("采购核价保存")]
        public JsonResult PurchaseExecutivePricingSave(T_PurchaseExecutive Purchasemodel, string detailList, int fkstatus, string memo)
        {
            int id = Purchasemodel.ID;
            T_PurchaseExecutiveApprove modelApprove = db.T_PurchaseExecutiveApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
            string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
            string result = "";
            if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            modelApprove.Memo = memo;
            modelApprove.ApproveTime = DateTime.Now;
            modelApprove.Status = fkstatus;
            db.Entry<T_PurchaseExecutiveApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                using (TransactionScope sc = new TransactionScope())
                {
                    if (fkstatus==1) { 
                    try
                    {
                        List<T_PurchaseExecutiveDetails> details = Deserialize<T_PurchaseExecutiveDetails>(detailList);
                        //主表保存
                        int editStatus = Purchasemodel.Status;//原记录的状态
                        int editID = Purchasemodel.ID;//原记录的ID
                        T_PurchaseExecutive PurMod = db.T_PurchaseExecutive.Find(editID);
                        PurMod.Warehouse = Purchasemodel.Warehouse;
                        PurMod.CopelPay = Purchasemodel.CopelPay;
                        PurMod.ApplyReason = Purchasemodel.ApplyReason;
                        db.Entry<T_PurchaseExecutive>(PurMod).State = System.Data.Entity.EntityState.Modified;
                        int x = db.SaveChanges();

                        if (x > 0)
                        {
                            //删除oid==id原有的详情表记录
                            List<T_PurchaseExecutiveDetails> delMod = db.T_PurchaseExecutiveDetails.Where(a => a.Purchase_ID == editID).ToList();
                            foreach (var item in delMod)
                            {
                                db.T_PurchaseExecutiveDetails.Remove(item);
                            }
                            db.SaveChanges();

                            //添加新的详情
                            foreach (var item in details)
                            {
                                item.Purchase_ID = Purchasemodel.ID;
                                db.T_PurchaseExecutiveDetails.Add(item);
                            }
                            db.SaveChanges();


                        }

                    }
                    catch (Exception ex)
                    {
                        return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                    T_PurchaseExecutive model = db.T_PurchaseExecutive.Find(id);
                    T_PurchaseExecutiveApprove newApprove = new T_PurchaseExecutiveApprove();
                    if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                    if (fkstatus == 1)
                    {
                        //同意
                        int step = int.Parse(model.Step.ToString());
                        step++;
                        IQueryable<T_PurchaseExecutiveConfig> config = db.T_PurchaseExecutiveConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            //不是最后一步，主表状态为0 =>审核中
                            model.Status = 0;
                            T_PurchaseExecutiveConfig stepMod = db.T_PurchaseExecutiveConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            //下一步审核人不是null  审核记录插入一条新纪录
                            newApprove.Memo = "";
                            newApprove.Oid = id;
                            newApprove.Status = -1;
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                            }

                            db.T_PurchaseExecutiveApprove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            //最后一步，主表状态改为 1 => 同意
                            model.Status = fkstatus;
                        }
                        model.Step = step;
                        db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
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
                        db.Entry<T_PurchaseExecutive>(model).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                    //ModularByZPK();
                    sc.Complete();
                }
            }
            else
            {
                result = "保存失败";
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        //仓库未审核数据列表
        [HttpPost]
        public ContentResult PurchaseExecutiveWarehouseList(Lib.GridPager pager, string queryStr)
        {
            // string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_PurchaseExecutiveApprove> ApproveMod = db.T_PurchaseExecutiveApprove.Where(a => a.ApproveName == "等待中" && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_PurchaseExecutive> queryData = from r in db.T_PurchaseExecutive
                                                        where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                                        select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_PurchaseExecutive> list = new List<T_PurchaseExecutive>();
            foreach (var item in queryData)
            {
                T_PurchaseExecutive i = new T_PurchaseExecutive();
                i = item;
                i.ApplyDepartment = GetDaparementString(item.ApplyDepartment);
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        //收货确认保存
        [HttpPost]
        [Description("行政收货确认保存")]
        public JsonResult PurchaseExecutiveWarehouseDetailSave(T_PurchaseExecutive Purchasemodel, string detailList, int fkstatus, string memo)
        {
            string result = "";
            using (TransactionScope sc = new TransactionScope())
            {
                int i = 0;
                //得到采购主表ID
                int PID = Purchasemodel.ID;
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                List<QueryPurchaseDetails> details = Deserialize<QueryPurchaseDetails>(detailList);
                //循环前台提交的详情
                for (int x = 0; x < details.Count; x++)
                {
                    //给采购详情表修改收货数量和最后收货时间
                    int DetID = details[x].ID;
                    T_PurchaseExecutiveDetails DetailsModel = db.T_PurchaseExecutiveDetails.SingleOrDefault(a => a.ID == DetID);
                    double ActualNum = 0;

                    if (DetailsModel.ActualNum != null)
                    {
                        ActualNum = double.Parse(DetailsModel.ActualNum.ToString());
                    }
                    DetailsModel.ActualNum = details[x].Num + ActualNum;
                    DetailsModel.ActualDate = DateTime.Now;
                    db.Entry<T_PurchaseExecutiveDetails>(DetailsModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //采购收货记录表添加当前收货记录
                    T_PurchaseExecutiveCollect CollectModel = new T_PurchaseExecutiveCollect();
                    CollectModel.PurchaseID = PID;
                    CollectModel.PurchaseDetailsID = DetID;
                    CollectModel.ThisNum = details[x].Num;
                    CollectModel.CollectName = curName;
                    CollectModel.CollectDate = DateTime.Now;
                    db.T_PurchaseExecutiveCollect.Add(CollectModel);
                    i = db.SaveChanges();
                }
                //通过采购主表ID查询采购主记录
                T_PurchaseExecutive pmodel = db.T_PurchaseExecutive.SingleOrDefault(a => a.ID == PID);
                //通过采购主表ID查询采购详情记录
                List<T_PurchaseExecutiveDetails> Dmodel = db.T_PurchaseExecutiveDetails.Where(a => a.Purchase_ID == PID).ToList();
                //临时变量储存收货数量和采购数量相等的
                int xx = 0;
                //循环判断采购详情表，收货和采购数量是否相等
                for (int z = 0; z < Dmodel.Count; z++)
                {

                    if (Dmodel[z].PlanNum <= Dmodel[z].ActualNum)
                    {
                        xx++;
                    }
                }
                //如果采购数量和收货数量相等，那么把主表的是否收货完成字段改成已完成（2）
                if (Dmodel.Count == xx)
                {
                    T_PurchaseExecutiveApprove modelApprove = db.T_PurchaseExecutiveApprove.SingleOrDefault(a => a.Oid == PID && a.ApproveTime == null);
                    modelApprove.ApproveTime = DateTime.Now;
                    modelApprove.Status = 1;
                    modelApprove.ApproveName = curName;
                    db.Entry<T_PurchaseExecutiveApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //并且改变主记录的，让主记录的步奏和状态都改成已完成。步奏（3）代表已完成。状态1代表结束
                    pmodel.Status = 1;
                    pmodel.Step = 3;
                    db.Entry<T_PurchaseExecutive>(pmodel).State = System.Data.Entity.EntityState.Modified;
                }
                if (i > 0)
                {

                    result = "保存成功";
                }
                else
                {
                    result = "保存失败";
                }
                sc.Complete();
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }
}
