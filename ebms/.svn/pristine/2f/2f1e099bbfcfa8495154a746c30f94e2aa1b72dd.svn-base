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

namespace EBMS.Controllers
{
    /// <summary>
    /// 财务中心
    /// </summary>
    public class FinanceController : BaseController
    {

        #region 公共属性/方法/类

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
        /// 凭证费用
        /// </summary>
        public class PZCost
        {
            public string PZ_Department { get; set; }
            public decimal PZ_Money { get; set; }
        }

        #endregion

        #region 视图

        #region 资金调拨


        [Description("资金调拨管理")]
        public ActionResult ViewFundAllotManager()
        {
            return View();
        }

        [Description("我的资金调拨")]
        public ActionResult ViewFundAllotForMy()
        {
            return View();
        }

        [Description("资金调拨审核")]
        public ActionResult ViewFundAllotAprove(int id = 0)
        {
            var model = db.T_FundAllot.Find(id);
            if (model == null)
                return HttpNotFound();
            else
            {
                List<T_FundApprove> approve = db.T_FundApprove.Where(a => a.ItemId == id).ToList();
                string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>审核备注</td></tr>";
                string tr = "";
                foreach (var item in approve)
                {
                    string s = "";
                    if (item.Status == -1) s = "<font color=blue>未审核</font>";
                    if (item.Status == 0) s = "<font color=blue>审核中</font>";
                    if (item.Status == 1) s = "<font color=green>已同意</font>";
                    if (item.Status == 2) s = "<font color=red>不同意</font>";
                    tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
                }
                ViewData["approID"] = db.T_FundApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.ItemId == id).ID;
                ViewData["history"] = table + tr + "</tbody></table>";
                ViewData["approveName"] = UserModel.Name;
                ViewData["companyOut"] = Com.CompanyOut();
                List<SelectListItem> listPayBank = Lib.Comm.PayBank;
                ViewData["payBank"] = listPayBank;
                ViewData["payNumber"] = Com.PayNumber();
                return View(model);
            }
        }


        [Description("资金调拨申请")]
        public ActionResult VieFundAllotAdd()
        {
            ViewData["Name"] = UserModel.Name;
            ViewData["comPanyIn"] = Com.Companyin();
            ViewData["theReceivingBank"] = Lib.Comm.ReceivingBank;
            ViewData["accountNumber"] = Com.AcountNumber();
            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "ID", "Name", UserModel.DepartmentId);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ViewData["ReportDepartment"] = selecli;
            selecli.AddRange(selectList);
            return View();
        }


        [Description("资金调拨编辑")]
        public ActionResult ViewFundAllotEdit(int id = 0)
        {
            var model = db.T_FundAllot.Find(id);
            if (model == null)
                return HttpNotFound();
            else
            {
                model.CompanyIn = db.T_FundAcount.FirstOrDefault(s => s.Name.Equals(model.CompanyIn)).ID.ToString();
                model.AccountNumber = db.T_FundAcount.FirstOrDefault(s => s.Number.Equals(model.AccountNumber)).ID.ToString();
                model.Department = db.T_Department.FirstOrDefault(s => s.Name.Equals(model.Department)).ID.ToString();
                var selectList = new SelectList(db.T_Department.AsQueryable(), "ID", "Name", model.Department);
                List<SelectListItem> selecli = new List<SelectListItem>();
                selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
                ViewData["ReportDepartment"] = selecli;
                selecli.AddRange(selectList);

                var comPanyInList = new SelectList(db.T_FundAcount.Where(s => s.Name != null).ToList(), "ID", "Name", model.CompanyIn);
                List<SelectListItem> companyInItem = new List<SelectListItem>();
                companyInItem.Add(new SelectListItem { Text = "==请选择==", Value = "" });
                ViewData["comPanyIns"] = companyInItem;
                companyInItem.AddRange(comPanyInList);

                var theReceivingBankList = new SelectList(Lib.Comm.ReceivingBank, "Value", "Text", model.TheReceivingBank);
                List<SelectListItem> theReceivingBankItem = new List<SelectListItem>();
                theReceivingBankItem.AddRange(theReceivingBankList);
                ViewData["theReceivingBanks"] = theReceivingBankItem;

                var accountNumberList = new SelectList(db.T_FundAcount.Where(s => s.Number != null), "ID", "number", model.AccountNumber);
                List<SelectListItem> accountNumberItem = new List<SelectListItem>();
                accountNumberItem.Add(new SelectListItem { Text = "==请选择==", Value = "" });
                ViewData["accountNumbers"] = accountNumberItem;
                accountNumberItem.AddRange(accountNumberList);
                return View(model);
            }
        }


        [Description("资金调拨未审核")]
        public ActionResult ViewFundAllotNotCheck()
        {
            return View();
        }


        [Description("资金调拨打印")]
        public ActionResult ViewFundAllotPrint(int id = 0)
        {
            var model = db.T_FundAllot.Find(id);
            if (model == null)
                return HttpNotFound();
            else
                return View(model);
        }

        [Description("资金调拨详情")]
        public ActionResult ViewFundAllotDetail(int id = 0)
        {
            var model = db.T_FundAllot.Find(id);
            if (model == null)
                return HttpNotFound();
            else
            {
                List<T_FundApprove> approve = db.T_FundApprove.Where(a => a.ItemId == id).ToList();
                string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>审核备注</td></tr>";
                string tr = "";
                foreach (var item in approve)
                {
                    string s = "";
                    if (item.Status == -1) s = "<font color=blue>未审核</font>";
                    if (item.Status == 0) s = "<font color=blue>审核中</font>";
                    if (item.Status == 1) s = "<font color=green>已同意</font>";
                    if (item.Status == 2) s = "<font color=red>不同意</font>";
                    tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
                }
                ViewData["history"] = table + tr + "</tbody></table>";
                return View(model);
            }
        }

        [Description("资金调拨凭证")]
        public ActionResult ViewFundAllotPz()
        {
            ViewData["userName"] = UserModel.Nickname;
            return View();
        }

        [Description("资金调拨凭证添加")]
        public ActionResult ViewFundAllotPzAdd(int id)
        {
            if (id == 0)
                return HttpNotFound();
            T_FundAllot model = db.T_FundAllot.Find(id);
            ViewData["Rection"] = Com.diRection();
            return View(model);
        }

        [Description("凭证部门")]
        public ActionResult ViewAllotPzDepart()
        {
            return View();
        }

        [Description("凭证科目")]
        public ActionResult ViewAlloptPzSubject()
        {
            return View();
        }

        [Description("凭证列表")]
        public ActionResult ViewFundAllotPzList()
        {
            ViewData["subject"] = Com.subject();
            ViewData["depart"] = Com.depart();
            return View();
        }

        public ActionResult ViewFundPzApprove(int id)
        {
            ViewData["fundId"] = id;
            return View();
        }

        #endregion

        #region 资金冻结


        [Description("资金冻结管理")]
        public ActionResult ViewFundFreezeManager()
        {
            return View();
        }


        [Description("我的资金冻结")]
        public ActionResult ViewFundFreezeForMy()
        {
            return View();
        }

        [Description("资金冻结申请")]
        public ActionResult ViewFundFreezeAdd()
        {
            ViewData["Name"] = UserModel.Nickname;
            var listcom = new SelectList(db.T_ShopFromGY.Where(s => s.name != null), "ID", "name");
            List<SelectListItem> listshopName = new List<SelectListItem>();
            listshopName.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ViewData["shopName"] = listshopName;
            listshopName.AddRange(listcom);
            var listnumber = new SelectList(db.T_ShopFromGY.Where(s => s.number != null), "ID", "number");
            List<SelectListItem> listAlipay = new List<SelectListItem>();
            listAlipay.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ViewData["aliNumber"] = listAlipay;
            listAlipay.AddRange(listnumber);
            return View();
        }

        [Description("资金冻结未审核")]
        public ActionResult ViewFundFreezeNotCheck()
        {
            return View();
        }

        /// <summary>
        /// 资金冻结编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Description("资金冻结编辑")]
        public ActionResult ViewFundFreezeEdit(int id = 0)
        {
            var model = db.T_Freeze.Find(id);
            if (model == null)
                return HttpNotFound();
            model.alipay = db.T_ShopFromGY.FirstOrDefault(s => s.number.Equals(model.alipay)).ID.ToString();
            model.shopName = db.T_ShopFromGY.FirstOrDefault(s => s.name.Equals(model.shopName)).ID.ToString();

            var shopList = new SelectList(db.T_ShopFromGY.Where(s => s.name != null), "ID", "name", model.shopName);
            List<SelectListItem> item = new List<SelectListItem>();
            item.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ViewData["shopNames"] = item;
            item.AddRange(shopList);

            var alipay = new SelectList(db.T_ShopFromGY.Where(s => s.number != null), "ID", "number", model.alipay);
            List<SelectListItem> item1 = new List<SelectListItem>();
            item1.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ViewData["aliPays"] = item1;
            item1.AddRange(alipay);

            return View(model);
        }

        [Description("资金冻结审核")]
        public ActionResult ViewFundFreezeApprove(int id = 0)
        {
            var model = db.T_Freeze.Find(id);
            if (model == null)
                return HttpNotFound();
            else
            {
                List<T_FreezeApprove> approve = db.T_FreezeApprove.Where(a => a.freezeID == id).ToList();
                string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>审核备注</td></tr>";
                string tr = "";
                foreach (var item in approve)
                {
                    string s = "";
                    if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                    if (item.ApproveStatus == 0) s = "<font color=blue>审核中</font>";
                    if (item.ApproveStatus == 3) s = "<font color=green>已同意</font>";
                    if (item.ApproveStatus == 4) s = "<font color=red>不同意</font>";
                    tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
                }
                ViewData["approID"] = db.T_FreezeApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.freezeID == id).ID;
                ViewData["history"] = table + tr + "</tbody></table>";
                ViewData["config"] = db.T_FreezeConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Step > model.Step) == null ? "" : "1";
                return View(model);
            }
        }

        /// <summary>
        /// 资金冻结详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Description("资金冻结详情")]
        public ActionResult ViewFundFreezeDetail(int id = 0)
        {
            var model = db.T_Freeze.Find(id);
            if (model == null)
                return HttpNotFound();
            else
            {
                List<T_FreezeApprove> approve = db.T_FreezeApprove.Where(a => a.freezeID == id).ToList();
                string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>审核备注</td></tr>";
                string tr = "";
                foreach (var item in approve)
                {
                    string s = "";
                    if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                    if (item.ApproveStatus == 0) s = "<font color=blue>审核中</font>";
                    if (item.ApproveStatus == 3) s = "<font color=green>已同意</font>";
                    if (item.ApproveStatus == 4) s = "<font color=red>不同意</font>";
                    tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
                }
                ViewData["history"] = table + tr + "</tbody></table>";
                return View(model);
            }
        }

        #endregion


        #endregion

        #region Post提交

        #region 资金调拨

        /// <summary>
        /// 资金调拨凭证
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetViewFundAllotPz(Lib.GridPager pager, string query)
        {
            IQueryable<T_FundAllot> list = db.T_FundAllot.Where(s => s.Status == 1 && s.IsPzStatus != 1);
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.PostUser.Equals(query));
            pager.totalRows = list.Count();
            List<T_FundAllot> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 凭证列表
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        public ContentResult GetPzList(Lib.GridPager pager, string code, string sub, string dep)
        {
            IQueryable<T_PZ_FundAllot> list = db.T_PZ_FundAllot.AsQueryable();
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
            List<T_PZ_FundAllot> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
            return Content(json);
        }

        /// <summary>
        /// 调拨凭证AddSave
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="subject"></param>
        /// <param name="diRection"></param>
        /// <returns></returns>
        public JsonResult FundAlotPzAddSave(int id, string depart, string subject, int diRection)
        {
            //using (TransactionScope sc = new TransactionScope())
            //{
            //    try
            //    {
            //        T_FundAllot model = db.T_FundAllot.Find(id);
            //        T_PZ_FundAllot bx = new T_PZ_FundAllot
            //            {
            //                PZ_OrderNum = model.FundAllotCode,
            //                PZ_Summary = model.UseOfProceeds,
            //                PZ_Department = depart,
            //                PZ_Direction = diRection,
            //                PZ_Money = model.theMoney,
            //                PZ_Subject = subject,
            //                PZ_Time = DateTime.Now
            //            };
            //        db.T_PZ_FundAllot.Add(bx);
            //        db.SaveChanges();
            //        model.IsPzStatus = 1;
            //        db.SaveChanges();
            //        sc.Complete();
            //        return Json(new { State = "Success" });
            //    }
            //    catch (Exception ex)
            //    {
            //        return Json(new { State = "Faile", Message = ex.Message });
            //    }
            //}
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_FundAllot model = db.T_FundAllot.Find(id);
                    model.PZ_Department = depart;
                    model.PZ_Subject = subject;
                    model.PZ_Direction = diRection;
                    model.IsPzStatus = -1;
                    model.Pz_FundMemo = "";
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);

                }

            }
        }


        /// <summary>
        /// 凭证审核列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="fundId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetFundPzList(int fundId)
        {
            IQueryable<T_FundAllot> list = db.T_FundAllot.Where(s => s.ID == fundId).AsQueryable();
            List<T_FundAllot> querData = list.OrderByDescending(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 资金调拨管理
        /// </summary>
        /// <param name="number"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewFundAllotManagerList(Lib.GridPager pager, string number, string startTime, string endTime, int status = -2)
        {
            IQueryable<T_FundAllot> list = db.T_FundAllot.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(number))
            {
                list = list.Where(a => a.AccountNumber.Contains(number) || a.PaymentNumber.Contains(number));
            }
            if (status != -2)
            {
                list = list.Where(a => a.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = DateTime.Parse(startTime + " 00:00:00");
                list = list.Where(s => s.PostTime >= start);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = DateTime.Parse(endTime + " 23:59:59");
                list = list.Where(s => s.PostTime <= end);
            }
            pager.totalRows = list.Count();
            List<T_FundAllot> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 我的资金调拨
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="number"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewFundAllotForMyList(Lib.GridPager pager, string number, string startTime, string endTime, int status = -2)
        {
            IQueryable<T_FundAllot> list = db.T_FundAllot.Where(a => a.PostUser == UserModel.Name && a.IsDelete == 0);
            if (UserModel.Name == "李明霞" || UserModel.Name == "段志红")
                list = list.Where(a => a.PostUser == "李明霞" || a.PostUser == "段志红");
            if (!string.IsNullOrEmpty(number))
                list = list.Where(a => a.AccountNumber.Contains(number) || a.PaymentNumber.Contains(number));
            if (status != -2)
                list = list.Where(a => a.Status == status);
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = DateTime.Parse(startTime + " 00:00:00");
                list = list.Where(s => s.PostTime >= start);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = DateTime.Parse(endTime + " 23:59:59");
                list = list.Where(s => s.PostTime <= end);
            }
            pager.totalRows = list.Count();
            List<T_FundAllot> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 资金调拨未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewFundAllotNotCheckList(Lib.GridPager pager, string query)
        {
            List<T_FundAllot> fundList = new List<T_FundAllot>();
            IQueryable<T_FundApprove> list = db.T_FundApprove.Where(s => s.ApproveName.Equals(UserModel.Name) && !s.ApproveTime.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.ItemId }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_FundAllot model = db.T_FundAllot.Find(item);
                if (model != null)
                    fundList.Add(model);
            }
            fundList = fundList.Where(a => a.IsDelete == 0).ToList();
          
            if (!string.IsNullOrWhiteSpace(query))
                fundList = fundList.Where(s => s.PostUser.Equals(query) || s.CompanyIn.Equals(query) || s.CompanyOut.Equals(query) ).ToList();
            pager.totalRows = fundList.Count();
            fundList = fundList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(fundList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="approveId"></param>
        /// <param name="status"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FundCheck(int approveId, int status, int step, string memo, string companyOut, string paymentBank, string paymentNumber)
        {

            var configNextModel = db.T_FundConfig.FirstOrDefault(s => s.Step > step);
            var Approve = db.T_FundApprove.Find(approveId);
            var fund = db.T_FundAllot.FirstOrDefault(s => s.ID == Approve.ItemId);
            Approve.ApproveTime = DateTime.Now;
            Approve.Memo = memo;
            try
            {
                //同意
                if (status == 1)
                {
                    Approve.Status = 1;
                    //是否存在下一级
                    if (configNextModel != null)
                    {
                        fund.Step = configNextModel.Step;
                        fund.Status = 0;
                        var approveModel = new T_FundApprove();
                        approveModel.ApproveName = configNextModel.ApproveUser;
                        approveModel.Status = -1;
                        approveModel.ItemId = fund.ID;
                        db.T_FundApprove.Add(approveModel);
                    }
                    else
                    {
                        fund.Status = 1;
                        fund.CompanyOut = companyOut;
                        fund.ThePaymentBank = paymentBank;
                        fund.PaymentNumber = paymentNumber;
                        var time = DateTime.Now;
                        //调入记录
                        T_FundRecord modelIn = new T_FundRecord
                        {
                            Type = "资金调入",
                            Number = fund.FundAllotCode,
                            ShopName = fund.CompanyIn,
                            Account = fund.AccountNumber,
                            Cost = Convert.ToDecimal(fund.theMoney),
                            Time = time
                        };
                        db.T_FundRecord.Add(modelIn);
                        //调出记录
                        T_FundRecord modelOut = new T_FundRecord
                        {
                            Type = "资金调出",
                            Number = fund.FundAllotCode,
                            ShopName = fund.CompanyOut,
                            Account = fund.PaymentNumber,
                            Cost = Convert.ToDecimal("-" + fund.theMoney),
                            Time = time
                        };
                        db.T_FundRecord.Add(modelOut);
                    }
                }
                //不同意
                else
                {
                    Approve.Status = 2;
                    fund.Status = 2;
                    fund.Step = db.T_FundConfig.OrderBy(s => s.Step).First().Step;
                }


                var i = db.SaveChanges();
                List<T_ModularNotaudited> deleteItem = db.T_ModularNotaudited.Where(a => a.ModularName == "资金调拨").ToList();
                if (deleteItem.Count > 0)
                {
                    foreach (var item in deleteItem)
                    {
                        db.T_ModularNotaudited.Remove(item);
                    }
                    db.SaveChanges();
                }

               // ModularByZP();


                if (i > 0)
                {
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { State = "Faile", Message = "操作失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
                    T_FundAllot fund = db.T_FundAllot.Find(id);
                    fund.IsPzStatus = status;
                    fund.Pz_FundMemo = memo;
                    db.SaveChanges();
                    if (status == 1)
                    {
                        T_PZ_FundAllot bx = new T_PZ_FundAllot
                            {
                                PZ_OrderNum = fund.FundAllotCode,
                                PZ_Summary = fund.UseOfProceeds,
                                PZ_Department = fund.PZ_Department,
                                PZ_Direction = fund.PZ_Direction,
                                PZ_Money = fund.theMoney,
                                PZ_Subject = fund.PZ_Subject,
                                PZ_Time = DateTime.Now
                            };
                        db.T_PZ_FundAllot.Add(bx);
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

        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        /// <summary>
        /// 申请保存
        /// </summary>
        /// <param name="fund"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult VieFundAllotAddSave(T_FundAllot fund)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string code = "ZJ-DB-";
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_FundAllot> list = db.T_FundAllot.Where(a => a.FundAllotCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
                    if (list.Count == 0)
                    {
                        code += date + "-" + "0001";
                    }
                    else
                    {
                        string old = list[0].FundAllotCode.Substring(15);
                        int newcode = int.Parse(old) + 1;
                        code += date + "-" + newcode.ToString().PadLeft(4, '0');
                    }
                    fund.Step = 0;
                    fund.FundAllotCode = code;
                    fund.PostTime = DateTime.Now;
                    fund.Status = -1;
                    fund.IsDelete = 0;
                    fund.IsPzStatus = 0;
                    var id = int.Parse(fund.CompanyIn);
                    var id1 = int.Parse(fund.AccountNumber);
                    fund.CompanyIn = db.T_FundAcount.FirstOrDefault(s => s.ID == id).Name;
                    fund.AccountNumber = db.T_FundAcount.FirstOrDefault(s => s.ID == id1).Number;
                    var departID = int.Parse(fund.Department);
                    fund.Department = db.T_Department.Find(departID).Name;
                    db.T_FundAllot.Add(fund);
                    db.SaveChanges();
                    T_FundApprove approve = new T_FundApprove
                    {
                        ApproveName = db.T_FundConfig.OrderBy(s => s.ID).FirstOrDefault(s => s.Step == fund.Step).ApproveUser,
                        ItemId = fund.ID,
                        Status = -1,
                        Memo = ""
                    };
                    db.T_FundApprove.Add(approve);
                    db.SaveChanges();
                   // ModularByZP();

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "资金调拨").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_FundApprove where  ItemId in ( select id from T_FundAllot where IsDelete=0 and  (status=-1 or status=0)) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金调拨" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资金调拨";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_FundAllot where Status='2'  and IsDelete=0 GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金调拨" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资金调拨";
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
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewFundAllotEditSave(T_FundAllot model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    var fund = db.T_FundAllot.Find(model.ID);
                    var id = int.Parse(model.CompanyIn);
                    var id1 = int.Parse(model.AccountNumber);
                    var department = int.Parse(model.Department);
                    fund.PostUser = model.PostUser;
                    fund.Department = db.T_Department.Find(department).Name;
                    fund.TheReceivingBank = model.TheReceivingBank;
                    fund.theMoney = model.theMoney;
                    fund.UseOfProceeds = model.UseOfProceeds;
                    fund.CompanyIn = db.T_FundAcount.Find(id).Name;
                    fund.AccountNumber = db.T_FundAcount.Find(id1).Number;
                    fund.Status = -1;
                    fund.Step = 0;
                    db.SaveChanges();
                    //不同意新增审核流程
                    if (model.Status == 2)
                    {
                        T_FundApprove Approvemodel = new T_FundApprove
                        {
                            ApproveName = db.T_FundConfig.FirstOrDefault(s => s.Step == fund.Step).ApproveUser,
                            Status = -1,
                            Memo = "",
                            ItemId = model.ID
                        };
                        db.T_FundApprove.Add(Approvemodel);
                        db.SaveChanges();
                    }
                    ModularByZP();
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
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewFundAllotDelete(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_FundAllot model = db.T_FundAllot.Find(id);
                    model.IsDelete = 1;
                    db.SaveChanges();
                //  ModularByZP();

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region 资金冻结


        /// <summary>
        /// 资金冻结申请
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult ViewFundFreezeAddSave(T_Freeze model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    model.Step = 0;
                    model.datetime = DateTime.Now;
                    model.state = -1;
                    model.userName = UserModel.Nickname;
                    var id = int.Parse(model.shopName);
                    var id1 = int.Parse(model.alipay);
                    model.shopName = db.T_ShopFromGY.FirstOrDefault(s => s.ID == id).name;
                    model.alipay = db.T_ShopFromGY.FirstOrDefault(s => s.ID == id1).number;
                    model.isDelete = 0;
                    db.T_Freeze.Add(model);
                    db.SaveChanges();
                    T_FreezeApprove approve = new T_FreezeApprove
                    {
                        ApproveName = db.T_FreezeConfig.OrderBy(s => s.ID).FirstOrDefault(s => s.Step == model.Step).ApproveUser,
                        freezeID = model.ID,
                        ApproveStatus = -1,
                        Memo = ""
                    };
                    db.T_FreezeApprove.Add(approve);
                    db.SaveChanges();
                   // ModularByZPDJ();

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public void ModularByZPDJ()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "资金冻结").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_FreezeApprove where  freezeID in ( select ID from T_Freeze where isDelete=0  and  (state=-1 or state=0) ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金冻结" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资金冻结";
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
            string RejectNumberSql = "select userName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Freeze where state='4' GROUP BY userName ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金冻结" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资金冻结";
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
        /// 资金冻结管理列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="postUser"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ContentResult ViewFundFreezeManagerList(Lib.GridPager pager, string postUser, string startTime, string endTime, int state = -2)
        {
            IQueryable<T_Freeze> list = db.T_Freeze.Where(s => s.isDelete == 0);
            if (!string.IsNullOrWhiteSpace(postUser))
                list = list.Where(s => s.userName.Equals(postUser));
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = DateTime.Parse(startTime + " 00:00:00");
                list = list.Where(s => s.datetime >= start);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = DateTime.Parse(endTime + " 23:59:59");
                list = list.Where(s => s.datetime <= end);
            }
            if (state != -2)
                list = list.Where(s => s.state == state);
            pager.totalRows = list.Count();
            List<T_Freeze> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 我的资金冻结列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ContentResult ViewFundFreezeForMyList(Lib.GridPager pager, string startTime, string endTime, int state = -2)
        {
            IQueryable<T_Freeze> list = db.T_Freeze.Where(s => s.isDelete == 0 && s.userName.Equals(UserModel.Nickname));
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = DateTime.Parse(startTime + " 00:00:00");
                list = list.Where(s => s.datetime >= start);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = DateTime.Parse(endTime + " 23:59:59");
                list = list.Where(s => s.datetime <= end);
            }
            if (state != -2)
                list = list.Where(s => s.state == state);
            pager.totalRows = list.Count();
            List<T_Freeze> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 资金冻结未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult ViewFundFreezeNotCheckList(Lib.GridPager pager, string startTime, string endTime, string query)
        {
            List<T_Freeze> fundList = new List<T_Freeze>();
            IQueryable<T_FreezeApprove> list = db.T_FreezeApprove.Where(s => s.ApproveName.Equals(UserModel.Name) && !s.ApproveTime.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.freezeID }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Freeze model = db.T_Freeze.Find(item);
                if (model != null)
                    fundList.Add(model);
            }
      
            if (!string.IsNullOrWhiteSpace(query))
                fundList = fundList.Where(s => s.userName.Equals(query)).ToList();
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                DateTime start = DateTime.Parse(startTime + " 00:00:00");
                fundList = fundList.Where(s => s.datetime >= start).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                DateTime end = DateTime.Parse(endTime + " 23:59:59");
                fundList = fundList.Where(s => s.datetime <= end).ToList();
            }
            pager.totalRows = fundList.Count();
            fundList = fundList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(fundList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 资金冻结审核
        /// </summary>
        /// <param name="approveId"></param>
        /// <param name="status"></param>
        /// <param name="step"></param>
        /// <param name="memo"></param>
        /// <param name="usedMoney"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FreezeCheck(int approveId, int status, int step, string memo, decimal usedMoney = 0)
        {
            var configNextModel = db.T_FreezeConfig.FirstOrDefault(s => s.Step > step);
            var Approve = db.T_FreezeApprove.Find(approveId);
            var model = db.T_Freeze.FirstOrDefault(s => s.ID == Approve.freezeID);
            Approve.ApproveTime = DateTime.Now;
            Approve.Memo = memo;
            try
            {
                //同意
                if (status == 3)
                {
                    Approve.ApproveStatus = 3;
                    //是否存在下一级
                    if (configNextModel != null)
                    {
                        model.Step = configNextModel.Step;
                        model.state = 1;
                        var approveModel = new T_FreezeApprove();
                        approveModel.ApproveName = configNextModel.ApproveUser;
                        approveModel.ApproveStatus = -1;
                        approveModel.freezeID = model.ID;
                        db.T_FreezeApprove.Add(approveModel);
                    }
                    else
                    {
                        if (usedMoney > model.freezeMoney)
                            return Json(new { State = "Faile", Message = "使用金额不能大于冻结金额" }, JsonRequestBehavior.AllowGet);
                        model.state = 2;
                        model.usedMoney = usedMoney;
                        model.surplusMoney = model.freezeMoney - model.usedMoney;
                    }
                }
                //不同意
                else
                {
                    Approve.ApproveStatus = 4;
                    model.state = 4;
                    model.Step = db.T_FreezeConfig.OrderBy(s => s.Step).First().Step;
                }
                var i = db.SaveChanges();


                if (i > 0)
                {
                    List<T_ModularNotaudited> deleteItem = db.T_ModularNotaudited.Where(a => a.ModularName == "资金冻结").ToList();
                    if (deleteItem.Count > 0)
                    {
                        foreach (var item in deleteItem)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
                        db.SaveChanges();
                    }
                   // ModularByZPDJ();
                   
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { State = "Faile", Message = "操作失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 编辑保存
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ViewFundFreezeEditSave(T_Freeze model)
        {
            try
            {
                var freeze = db.T_Freeze.Find(model.ID);
                var id = int.Parse(model.shopName);
                var id1 = int.Parse(model.alipay);
                freeze.userName = model.userName;
                freeze.freezeMoney = model.freezeMoney;
                freeze.freezeReason = model.freezeReason;
                freeze.shopName = db.T_ShopFromGY.Find(id).name;
                freeze.alipay = db.T_ShopFromGY.Find(id1).number;
                freeze.state = -1;
                freeze.Step = 0;
                freeze.remark = model.remark;
                db.SaveChanges();

               // ModularByZPDJ();

                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 虚拟删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ViewFreezeDelete(int id)
        {
            try
            {
                T_Freeze model = db.T_Freeze.Find(id);
                model.isDelete = 1;

                db.SaveChanges();
                //ModularByZPDJ();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #endregion

    }
}
