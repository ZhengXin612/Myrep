using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using static EBMS.Controllers.DataStatisticsController;

namespace EBMS.Controllers
{
    public class PaymentController : Controller
    {
        //
        // GET: /Payment/

        public ActionResult Index()
        {
            return View();
        }
        EBMSEntities db = new EBMSEntities();
        public T_User UserModel
        {

            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }
        /// <summary>
        /// 根据部门选择下级审核人
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetApproveByDeptID(int ID)
        {
            var dept = db.T_Department.Find(ID);
            if (dept != null)
            {
                Dictionary<int, string> list = new Dictionary<int, string>();
                if (dept.supervisor != null)
                {
                    T_User user = db.T_User.Find(dept.supervisor);
                    list.Add(user.ID, user.Nickname);
                }
                else
                    list.Add(18, "姜尚");
                return Json(list.ToArray());
            }
            return Json(null);
        }


        [Description("付款单未审核")]
        public ActionResult PaymentCheck()
        {
            return View();
        }
        /// <summary>
        /// 获取费用类型
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetExpenseCostType(Lib.GridPager pager, string type)
        {
            List<SelectListItem> list = Lib.Comm.ExpenseCostType;
            if (!string.IsNullOrWhiteSpace(type))
                list = list.Where(s => s.Text.Contains(type)).ToList();
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.Value).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            var json = new
            {
                total = pager.totalRows,
                rows = (from r in list
                        select new
                        {
                            Type = r.Text
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public class ExpenseCost
        {
            public string Car_Number { get; set; }
            public decimal Reun_Cost { get; set; }
        }
        /// <summary>
        /// 获取店铺
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetShopName(Lib.GridPager pager, string name)
        {
            List<T_ShopFromGY> shopList = db.T_ShopFromGY.ToList();
            if (!string.IsNullOrWhiteSpace(name))
                shopList = shopList.Where(s => s.name.Contains(name)).ToList();
            pager.totalRows = shopList.Count();
            shopList = shopList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(shopList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 选择店铺
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewShop(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        /// <summary>
        /// 选择费用类别
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ActionResult ViewPaymentCostType(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        [Description("审核")]
        public ActionResult PaymenReportCheck(int id)
        {

            if (id == 0)
                return HttpNotFound();
            var history = db.T_PaymentApprove.Where(a => a.Reunbursement_id == id);
            var model = db.T_Payment.Find(id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveDate, item.Remark);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            ViewData["expenseId"] = id;
            //获取审核表中的 审核记录ID
            T_PaymentApprove approve = db.T_PaymentApprove.FirstOrDefault(a => !a.ApproveDate.HasValue && a.Reunbursement_id == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
            {
                ViewData["approveid"] = 0;
            }
            BindApproveName(model.Step + 1);
            int Step = db.T_ExpenseApproveConfig.ToList().Count;
            ViewData["Step"] = Step;
            ViewData["comPanyIn"] = Com.ExpenseCompany();
            //  ViewData["ExpStatusList"] = Com.ExpenseStatus();
            return View(model);
        }
        /// <summary>
        /// 绑定下级审核人
        /// </summary>
        /// <param name="step"></param>
        private void BindApproveName(int step = 0)
        {
            var approveusers = db.T_PaymentApproveConfig.FirstOrDefault(a => a.Step == 4);


            if (approveusers != null)
            {
                //如果是动态获取当前部门主管
                //if (approveusers.ApproveUser.Equals("部门主管"))
                //{
                //    List<SelectListItem> items = new List<SelectListItem>();
                //    items.Add(new SelectListItem { Text = "请选择", Value = "" });
                //    ViewData["NextList"] = items;
                //}
                //如果还有其他的审核组或者动态绑定的数据 再增加else
                //如果是固定的某些人
                //else
                //{
                string[] array = approveusers.ApproveUser.Split(',');
                List<SelectListItem> items = new List<SelectListItem>();

                foreach (var item in array)
                {
                    T_User user = db.T_User.FirstOrDefault(a => a.Nickname.Equals(item) || a.Name.Equals(item));
                    if (user != null)
                        items.Add(new SelectListItem { Text = user.Nickname, Value = user.ID.ToString() });
                }
                ViewData["NextList"] = items;
                // }
            }
            else
            {
                ViewData["NextList"] = null;
            }
        }

        [Description("付款单打印")]
        public ActionResult ViewPaymentPrint(int id, int page = 1)
        {
			ViewData["CompanyList"] = Com.DirectoryList("公司");
			ViewData["Bid"] = id;
            T_Payment model = db.T_Payment.Find(id);
            ViewData["jine"] = model.Reun_Cost;
            ViewData["code"] = model.Reun_Code;
            ViewData["PrintCount"] = model.PrintCount;
            ViewData["borrowCode"] = string.IsNullOrWhiteSpace(model.MatchBorrowNumber) ? "" : "借支批号:" + model.MatchBorrowNumber;
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
            IQueryable<T_PaymentProduct> list = db.T_PaymentProduct.Where(a => a.ReunId == id);
            int totalRows = list.Count();
            int P = 0;
            if (totalRows % 5 != 0)
                P = totalRows / 5 + 1;
            else
            {
                P = totalRows / 5;
            }
            for (int i = 1; i <= P; i++)
            {
                href += "<a href=\"?page=" + i + "&id=" + id + "\">   " + i + "   </a>";
            }
            ViewData["pager"] = href;
            ViewData["total"] = P;
            ViewData["id"] = id;
            return View(model);
        }
        [Description("我的付款单")]
        public ActionResult PaymentListForMy()
        {
            ViewData["user"] = UserModel.Nickname;
            return View();
        }

        [Description("付款单明细")]
        public ActionResult PaymentProductDetail(int expenseId)
        {
            if (expenseId == 0)
                return HttpNotFound();
            ViewData["expenseId"] = expenseId;
            var history = db.T_PaymentApprove.Where(a => a.Reunbursement_id == expenseId);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveDate, item.Remark);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View();
        }
        //获取附件
        public JsonResult GetPaymentEnclosure(int id)
        {
            List<T_PaymentEnclosure> model = db.T_PaymentEnclosure.Where(a => a.ExpenseId == id).ToList();
            string options = "";
            if (model.Count > 0)
            {
                options += "[";
                foreach (var item in model)
                {
                    options += "{\"ScName\":\"" + item.ScName + "\",\"Url\":\"" + item.Url + "\",\"Size\":\"" + item.Size + "\",\"Path\":\"" + item.Path + "\"},";
                }
                options = options.Substring(0, options.Length - 1);
                options += "]";
            }
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [Description("付款单列表")]
        public ActionResult PaymentList()
        {
            return View();
        }
        /// <summary>
        /// 我的付款单
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="Code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult PaymentListForMyList(Lib.GridPager pager, string code, int status = -2)
        {
            IQueryable<T_Payment> list = db.T_Payment.Where(s => s.PostUser.Equals(UserModel.Name) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Equals(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            pager.totalRows = list.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = list.Sum(s => s.Reun_Cost);
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_Payment> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 修改打印次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult UpdateprintCountByID(int id)
        {
            try
            {
                T_Payment model = db.T_Payment.Find(id);
                model.PrintCount += 1;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        /// <summary>
        /// 绑定账号
        /// </summary>
        /// <param name="type"></param>
        public JsonResult GetPaymentAcount(int type)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            List<T_ExpenseAcount> acountList = db.T_ExpenseAcount.Where(s => s.type == type).ToList();
            foreach (var item in acountList)
            {
                list.Add(item.Number, item.Number);
            }
            return Json(list.ToArray());
        }
        /// <summary>
        /// 报销明细数据
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetPaymentProductList(int expenseId)
        {
            IQueryable<T_PaymentProduct> list = db.T_PaymentProduct.Where(s => s.ReunId == expenseId).AsQueryable();
            List<T_PaymentProduct> querData = list.OrderBy(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获取报销打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetPaymentPrint(int id)
        {
            T_Payment model = db.T_Payment.Find(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取报销明细打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetPaymentProductPrint(int expenseId, int page)
        {
            IQueryable<T_PaymentProduct> list = db.T_PaymentProduct.Where(a => a.ReunId == expenseId);
            int totalRows = list.Count();
            List<T_PaymentProduct> modelList = list.OrderBy(c => c.ID).Skip((page - 1) * 5).Take(5).ToList();
            return Json(modelList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 付款单列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ContentResult ViewPaymentList(Lib.GridPager pager, string code, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_Payment> list = db.T_Payment.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Contains(code) || s.PostUser.Contains(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.CrateDate >= start);
            }
            else if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CrateDate <= end);
            }
            pager.totalRows = list.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = list.Sum(s => s.Reun_Cost);
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_Payment> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("付款申请单")]
        public ActionResult PaymentAdd()
        {
			ViewData["Company"] = Com.DirectoryList("公司");
			//绑定部门
			ViewData["ReportDepartment"] = Com.DepartMent();
            string code = "FK-DE-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_Payment> lists = db.T_Payment.Where(a => a.Reun_Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (lists.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = lists[0].Reun_Code.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            T_Payment model = new T_Payment();
            model.Reun_Code = code;
            model.PostUser = UserModel.Name;
        
            model.Department = UserModel.DepartmentId;
            return View(model);
        }
        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ViewPaymentInvalid(int id)
        {
            try
            {
                T_Payment model = db.T_Payment.Find(id);
                model.Status = 3;
                db.SaveChanges();

                //List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "报销").ToList();
                //if (ModularNotaudited.Count > 0)
                //{
                //    foreach (var item in ModularNotaudited)
                //    {
                //        db.T_ModularNotaudited.Remove(item);
                //    }
                //    db.SaveChanges();
                //}
              
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Description("付款单已审核")]
        public ActionResult PaymentChecked()
        {
            return View();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewPaymentDelete(int id)
        {
            try
            {

                T_Payment model = db.T_Payment.Find(id);
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
        /// 未审核列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewPaymentCheckList(Lib.GridPager pager, string user)
        {
            List<T_Payment> expenseList = new List<T_Payment>();
            IQueryable<T_PaymentApprove> list = db.T_PaymentApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && !s.ApproveDate.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.Reunbursement_id }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Payment model = db.T_Payment.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
                if (model != null)
                    expenseList.Add(model);
            }
            if (!string.IsNullOrWhiteSpace(user))
                expenseList = expenseList.Where(s => s.PostUser.Contains(user) || s.Reun_Code.Contains(user)).ToList();


            pager.totalRows = expenseList.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = expenseList.Sum(s => s.Reun_Cost);
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_Payment> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 付款保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PaymentAddSave(T_Payment model, string jsonStr, string jsonStr1, string picUrls)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return Json(new { State = "Faile", Message = "请添加报销明细" });
            }
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_PaymentProduct> details = Com.Deserialize<T_PaymentProduct>(jsonStr);
                    string expensecode = "FK-DE-";
                    string expensedate = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_Payment> lists = db.T_Payment.Where(a => a.Reun_Code.Contains(expensedate)).OrderByDescending(c => c.ID).ToList();
                    if (lists.Count == 0)
                    {
                        expensecode += expensedate + "-" + "0001";
                    }
                    else
                    {
                        string old = lists[0].Reun_Code.Substring(15);
                        int newcode = int.Parse(old) + 1;
                        expensecode += expensedate + "-" + newcode.ToString().PadLeft(4, '0');
                    }
                    model.Reun_Code = expensecode;
                    //主表数据
                    model.CrateDate = DateTime.Now;
                    model.Status = -1;
                    //model.IsBlending = 0;
                    model.IsExpenseMatch = false;
                    model.IsDelete = 0;
                    int departId = int.Parse(model.Department);
                    model.Department = db.T_Department.Find(departId).Name;
                    model.Reun_Cost = details.Sum(s => s.Price * s.Num);
                    model.Step = 0;
                    model.Pz_BXStatus = 0;
                    model.PostUser = UserModel.Name;
                    model.PrintCount = 0;
                    if (!string.IsNullOrWhiteSpace(model.Shop))
                        model.Shop = db.T_ShopFromGY.FirstOrDefault(s => s.number.Equals(model.Shop)).name;
                    if (!string.IsNullOrWhiteSpace(picUrls))
                        model.IsExpenseEnclosure = 1;
                    else
                        model.IsExpenseEnclosure = 0;

                    db.T_Payment.Add(model);
                    db.SaveChanges();
                    //审核记录
                    T_PaymentApprove Approvemodel = new T_PaymentApprove
                    {
                        ApproveName = Com.GetNickName(model.ExpenseNextApprove),
                        ApproveStatus = -1,
                        Reunbursement_id = model.ID
                    };
                    db.T_PaymentApprove.Add(Approvemodel);
                    db.SaveChanges();
                    //添加详情
                    foreach (var item in details)
                    {
                        item.ReunId = model.ID;
                        db.T_PaymentProduct.Add(item);
                    }
                    db.SaveChanges();
                 
                    //附件保存
                    if (!string.IsNullOrWhiteSpace(picUrls))
                    {
                        string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        // List<T_ExpenseEnclosure> Enclosure = Com.Deserialize<T_ExpenseEnclosure>(picUrls);
                        foreach (string item in picArr)
                        {
                            T_PaymentEnclosure Enclosure = new T_PaymentEnclosure();
                            Enclosure.Scdate = DateTime.Now;
                            Enclosure.ExpenseId = model.ID;
                            Enclosure.Url = item;
                            Enclosure.Path = item;
                            string[] ss = item.Split('/');
                            Enclosure.ScName = ss[ss.Length - 1];
                            Enclosure.Size = "0";

                            db.T_PaymentEnclosure.Add(Enclosure);
                        }
                        db.SaveChanges();
                    }

               //     ModularByZP();
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
        /// 付款单已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewPaymentCheckedList(Lib.GridPager pager, string user, string createDate, string endDate)
        {
            List<T_Payment> expenseList = new List<T_Payment>();
            IQueryable<T_PaymentApprove> list = db.T_PaymentApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveDate.HasValue);
            if (!string.IsNullOrWhiteSpace(createDate))
            {
                DateTime creat = DateTime.Parse(createDate + " 00:00:00");
                list = list.Where(s => s.ApproveDate >= creat);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.ApproveDate <= end);
            }
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.Reunbursement_id }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Payment model = db.T_Payment.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
                if (model != null)
                    expenseList.Add(model);
            }
            if (!string.IsNullOrWhiteSpace(user))
                expenseList = expenseList.Where(s => s.PostUser.Contains(user)).ToList();
            pager.totalRows = list.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = expenseList.Sum(s => s.Reun_Cost);
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_Payment> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public class OutDetail
        {
            public DateTime CrateDate { get; set; }
            public string PostUser { get; set; }
            public string Reun_Reason { get; set; }
            public string StoreName { get; set; }
            public string Abstract { get; set; }
            public decimal Price { get; set; }
            public int Num { get; set; }
            public Nullable<DateTime> approveDate { get; set; }
            public string Reun_Code { get; set; }
            public string Remark { get; set; }
            public string MatchBorrowNumber { get; set; }
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string statedate, string EndDate)
        {


            string sql = string.Format(@"select (select MatchBorrowNumber from T_Payment where ID=p.ReunId) as  MatchBorrowNumber,(select Reun_Code from T_Payment where ID=p.ReunId) as  Reun_Code,(select CrateDate from T_Payment where ID=p.ReunId) as  CrateDate, 
                                        (select PostUser from T_Payment where ID=p.ReunId) as PostUser,
                                        (select Reun_Reason from T_Payment where ID=p.ReunId) as Reun_Reason,
                                            StoreName,Abstract,Price,Num,
                                    (select top 1 ApproveDate from T_PaymentApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as ApproveDate,(select top 1 Remark from T_PaymentApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as Remark from T_PaymentProduct p where ReunId in(select Reunbursement_id from T_PaymentApprove where ApproveDate>='" + statedate + "' and ApproveDate<='" + EndDate + " 23:59:59' AND  ApproveName='" + UserModel.Nickname + "')");

            List<OutDetail> list = db.Database.SqlQuery<OutDetail>(sql).ToList();
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("申请日期");
            row1.CreateCell(1).SetCellValue("报销人");
            row1.CreateCell(2).SetCellValue("报销原因");
            row1.CreateCell(3).SetCellValue("店铺名称");
            row1.CreateCell(4).SetCellValue("产品名称");
            row1.CreateCell(5).SetCellValue("单价");
            row1.CreateCell(6).SetCellValue("数量");
            row1.CreateCell(7).SetCellValue("应付金额");
            row1.CreateCell(8).SetCellValue("审核日期");
            row1.CreateCell(9).SetCellValue("报销单号");
            row1.CreateCell(10).SetCellValue("审核备注");
            row1.CreateCell(11).SetCellValue("冲抵借支批号");
            //Remark
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 30 * 256);
            sheet1.SetColumnWidth(2, 15 * 256);
            sheet1.SetColumnWidth(3, 15 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 20 * 256);
            sheet1.SetColumnWidth(8, 20 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            sheet1.SetColumnWidth(10, 20 * 256);
            sheet1.SetColumnWidth(11, 20 * 256);
            double cost = 0;
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(list[i].CrateDate.ToString());
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";

                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;

                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(list[i].PostUser) ? "" : list[i].PostUser);
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(list[i].Reun_Reason) ? "" : list[i].Reun_Reason);
                rowtemp.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(list[i].StoreName) ? "" : list[i].StoreName);
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(list[i].Abstract) ? "" : list[i].Abstract);
                rowtemp.CreateCell(5).SetCellValue(Convert.ToDouble(list[i].Price).ToString("0.00"));
                rowtemp.CreateCell(6).SetCellValue(list[i].Num.ToString());
                rowtemp.CreateCell(7).SetCellValue(Convert.ToDouble(Convert.ToDouble(list[i].Price) * Convert.ToDouble(list[i].Num)).ToString("0.00"));
                rowtemp.CreateCell(8).SetCellValue(list[i].approveDate.ToString());
                rowtemp.CreateCell(9).SetCellValue(list[i].Reun_Code);
                rowtemp.CreateCell(10).SetCellValue(list[i].Remark);
                rowtemp.CreateCell(11).SetCellValue(list[i].MatchBorrowNumber);
                cost += Convert.ToDouble(Convert.ToDouble(list[i].Price) * Convert.ToDouble(list[i].Num));
            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(7);
            heji1.SetCellValue(cost.ToString("0.00"));
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "报销数据.xls");
        }
        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="approveID"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <param name="nextapprove"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Check(int approveID, int status, string memo, string nextapprove, string company, string number)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    int TotalStep = db.T_ExpenseApproveConfig.ToList().Count;
                    T_PaymentApprove approve = db.T_PaymentApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1 && (a.ApproveName == curName || a.ApproveName == Nickname));
                    if (approve == null)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" }, JsonRequestBehavior.AllowGet);
                    }

                    approve.ApproveStatus = status;
                    approve.ApproveDate = DateTime.Now;
                    approve.Remark = memo;
                    T_Payment model = db.T_Payment.Find(approve.Reunbursement_id);
                    if (nextapprove != null && nextapprove != "")
                    {
                        model.Cashier = nextapprove;
                    }
                    //if (ExpStatus != null && ExpStatus != "")
                    //{
                    //    model.ExpStatus = ExpStatus;
                    //}
                    int Step = model.Step;
                    Step++;
                    if (status == 2)
                    {

                        model.Status = 2;
                        model.Step = 0;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (TotalStep == Step)
                        {
                            if (company == "==请选择==")
                                return Json(new { State = "Faile", Message = "请选择公司" }, JsonRequestBehavior.AllowGet);
                            model.Status = status;
                            model.SpendingCompany = company;
                            model.SpendingNumber = number;
                           
                        }
                        else
                        {
                            if (status != 2)
                            {
                                T_PaymentApproveConfig ModelConFig = db.T_PaymentApproveConfig.SingleOrDefault(a => a.Step == Step);


                                T_PaymentApprove newApprove = new T_PaymentApprove();
                                newApprove.ApproveStatus = -1;
                                if (Step == 4)//步奏是4就是三疯进来审核
                                {
                                    newApprove.ApproveName = model.Cashier;
                                    nextapprove = model.Cashier;
                                }
                                else
                                {
                                    newApprove.ApproveName = ModelConFig.ApproveUser;
                                    nextapprove = ModelConFig.ApproveUser;
                                }

                                newApprove.ApproveDate = null;
                                newApprove.Reunbursement_id = approve.Reunbursement_id;
                                db.T_PaymentApprove.Add(newApprove);

                                T_User u = db.T_User.FirstOrDefault(a => a.Nickname.Equals(nextapprove));
                                model.ExpenseNextApprove = u.ID;
                                model.Status = 0;
                            }
                        }
                        model.Step = Step;
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
    }
}
