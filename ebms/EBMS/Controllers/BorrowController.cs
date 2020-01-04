using EBMS.App_Code;
using EBMS.Models;
using Lib;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Script.Serialization;

namespace EBMS.Controllers
{
    /// <summary>
    /// 借支
    /// </summary>
    public class BorrowController : BaseController
    {
        #region 公共属性/字段/方法

        EBMSEntities db = new EBMSEntities();

        public class ViewBorrowSetement
        {
            public int ID { get; set; }
            public string PostUser { get; set; }
            public string Code { get; set; }
            public string DepartMent { get; set; }
            public decimal BorrowCost { get; set; }
            public string BorrowReason { get; set; }
            public decimal SurplusCost { get; set; }
            public decimal SettementCost { get; set; }
            public Nullable<System.DateTime> BorrowDate { get; set; }
            public int BorrowStatus { get; set; }
            public Nullable<int> SettementStatus { get; set; }
        }

        public class Borrow
        {
            public string BorrowName { get; set; }
            public DateTime BorrowDate { get; set; }
            public string BorrowCode { get; set; }
            public decimal BorrowMoney { get; set; }
            public string BorrowReason { get; set; }
            public string BorrowBank { get; set; }
            public string BorrowAccountID { get; set; }
            public DateTime time { get; set; }
            public int status { get; set; }
            public int BorrowSettementState { get; set; }
        }

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

        /// <summary>
        /// 绑定下级审核人
        /// </summary>
        /// <param name="step"></param>
        private void BindApproveName(int step = 0)
        {
              string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
               T_BorrowApproveConfig approveusers;

               if (name == "罗瑶" && step==3)
               {
                   step = step + 1;
                   approveusers = db.T_BorrowApproveConfig.FirstOrDefault(a => a.Step == step);
               }
               else
               {
                   approveusers = db.T_BorrowApproveConfig.FirstOrDefault(a => a.Step == step);
               }
          
            if (approveusers != null)
            {
                //如果是动态获取当前部门主管
                if (approveusers.ApproveUser.Equals("部门主管"))
                {
                    List<SelectListItem> items = new List<SelectListItem>();
                    items.Add(new SelectListItem { Text = "请选择", Value = "9999" });
                    ViewData["NextList"] = items;

                }
                
                //如果还有其他的审核组或者动态绑定的数据 再增加else
                //如果是固定的某些人
                else
                {
                    string[] array = approveusers.ApproveUser.Split(',');
                    List<SelectListItem> items = new List<SelectListItem>();

                    foreach (var item in array)
                    {
                        T_User user = db.T_User.FirstOrDefault(a => a.Nickname.Equals(item));
                        if (user != null)
                            items.Add(new SelectListItem { Text = user.Nickname, Value = user.ID.ToString() });
                    }
                    ViewData["NextList"] = items;
                }
            }
            else
            {
                    ViewData["NextList"] = null;
            }
        }
        /// <summary>
        /// 绑定下级审核人
        /// </summary>
        /// <param name="step"></param>
        private void BindApproveBmName()
        {
            //如果是动态获取当前部门主管
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "请选择", Value = "9999" });
            ViewData["NextList"] = items;
        }
        //绑定凭证部门下拉框
        public List<SelectListItem> GetDepartment()
        {
            var list = db.T_PZ_DePartMent.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择", Value = "9999" });
            selecli.AddRange(selectList);
            return selecli;
        }
        //绑定凭证科目下拉框
        public List<SelectListItem> GetSubject()
        {
            var list = db.T_AccountantProject.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择", Value = "9999" });
            selecli.AddRange(selectList);
            return selecli;
        }
        #endregion

        #region 视图

        #region 借支

        [Description("申请借支")]
        public ActionResult ViewBorrowAdd()
        {
			ViewData["CompanyList"] = Com.DirectoryList("公司");

			ViewData["ReportDepartment"] = Com.DepartMent();

            string code = "JZ-DE-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_Borrow> lists = db.T_Borrow.Where(a => a.BorrowCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (lists.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = lists[0].BorrowCode.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            T_Borrow model = new T_Borrow();
            model.BorrowCode = code;
            model.BorrowName = UserModel.Name;
            model.BorrowerDep = UserModel.DepartmentId;
            return View(model);
        }

        [Description("借支管理")]
        public ActionResult ViewBorrowManager()
        {
            return View();
        }

        [Description("借支打印")]
        public ActionResult ViewBorrowPrint(int id)
        {
			ViewData["CompanyList"] = Com.DirectoryList("公司");
			var model = db.T_Borrow.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [Description("借支编辑")]
        public ActionResult ViewBorrowEdit(int id)
        {
            var model = db.T_Borrow.Find(id);
            if (model == null)
                return HttpNotFound();
            BindApproveBmName();
            model.BorrowerDep = db.T_Department.FirstOrDefault(s => s.Name.Equals(model.BorrowerDep)).ID.ToString();
            ViewData["ReportDepartment"] = Com.DepartMent(model.BorrowerDep);
			ViewData["CompanyList"] = Com.DirectoryList("公司");

			return View(model);
        }

        [Description("借支审核")]
        public ActionResult ViewBorrowApprove(int id)
        {
            var model = db.T_Borrow.Find(id);
            if (model == null)
                return HttpNotFound();
            var history = db.T_BorrowApprove.Where(a => a.Pid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";

            ViewData["MoneyFrom"] = Comm.BorrowForm;
            int Step = db.T_BorrowApproveConfig.ToList().Count;
            ViewData["Step"] = Step;
            ViewData["total"] = db.T_BorrowApproveConfig.Count();
            BindApproveName(model.BorrowStep + 1);
            //获取审核表中的 审核记录ID
            T_BorrowApprove approve = db.T_BorrowApprove.FirstOrDefault(a => !a.ApproveTime.HasValue && a.Pid == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
            {
                ViewData["approveid"] = 0;
            }
            ViewData["comPanyIn"] = Com.ExpenseCompany();
            return View(model);
        }

        [Description("借支详情")]
        public ActionResult ViewBorrowDetail(int id)
        {
            var model = db.T_Borrow.Find(id);
            if (model == null)
                return HttpNotFound();
            var history = db.T_BorrowApprove.Where(a => a.Pid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.ApproveStatus == -1) s = "<font color=blue>未审核</font>";
                if (item.ApproveStatus == 1) s = "<font color=green>已同意</font>";
                if (item.ApproveStatus == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View(model);
        }

        [Description("借支未审核")]
        public ActionResult ViewBorrowNotCheckList()
        {
            return View();
        }

        [Description("借支已审核")]
        public ActionResult ViewBorrowCheckedList()
        {
            return View();
        }

        [Description("我的借支")]
        public ActionResult ViewBorrowForMy()
        {
            return View();
        }

        [Description("获取已审核报销单号")]
        public ActionResult ViewExpenseCodeIscheck()
        {
            return View();
        }

        #endregion
        #region 结算借支

        [Description("结算借支列表")]
        public ActionResult ViewBorrowSetementList()
        {
            return View();
        }
        [Description("借支凭证列表")]
        public ActionResult VoucherTable()
        {
            return View();
        }

        [Description("借支凭证")]
        public ActionResult VoucherList()
        {
            ViewData["userName"] = UserModel.Nickname;
            return View();
        }
        [Description("结算")]
        public ActionResult ViewSetement(string code, int num)
        {
            var borrowmodel = db.T_Borrow.FirstOrDefault(s => s.BorrowCode.Equals(code));
            ViewData["SurplusCost"] = db.T_BorrowSettementRecord.FirstOrDefault(s => s.BorrowCode.Equals(code)) == null ? borrowmodel.BorrowMoney : borrowmodel.BorrowMoney - db.T_BorrowSettementRecord.Where(s => s.BorrowCode.Equals(code)).Sum(s => s.SettementCost);
            ViewData["num"] = num;
            return View(borrowmodel);
        }

        [Description("结算记录")]
        public ActionResult ViewSetementRecord(string code)
        {
            var model = db.T_Borrow.FirstOrDefault(s => s.BorrowCode.Equals(code));
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        #endregion
        #region  申请凭证
        public ActionResult ViewVoucher(int ID)
        {
            T_Borrow MOD = db.T_Borrow.Find(ID);
            ViewData["borrowId"] = ID;
            return View();
        }

        public ActionResult ViewBorrowPzApprove(int id)
        {
            if (id == 0)
                return HttpNotFound();
            var model = db.T_Expense.Find(id);
            ViewData["borrowId"] = id;
            return View(model);
        }

        #endregion

        #endregion

        #region Post提交

        /// <summary>
        /// 修改打印次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult UpdateprintCountByID(int id)
        {
            try
            {
                T_Borrow model = db.T_Borrow.Find(id);
                model.PrintCount += 1;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message });
            }
        }

        #region 借支

        /// <summary>
        /// 借支管理列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewBorrowManagerList(Lib.GridPager pager, string userName, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_Borrow> list = db.T_Borrow.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
                list = list.Where(s => s.BorrowName.Equals(userName));
            if (status != -2)
                list = list.Where(s => s.BorrowState == status);
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.BorrowDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.BorrowDate <= end);
            }
            pager.totalRows = list.Count();
            List<T_Borrow> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 我的借支管理
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewBorrowManagerForMyList(Lib.GridPager pager, string code, int status = -2)
        {
            IQueryable<T_Borrow> list = db.T_Borrow.Where(s => s.BorrowName.Equals(UserModel.Name) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.BorrowCode.Equals(code));
            if (status != -2)
                list = list.Where(s => s.BorrowState == status);
            pager.totalRows = list.Count();
            List<T_Borrow> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 借支未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewBorrowNotCheckList(Lib.GridPager pager, string query)
        {
            List<T_Borrow> borrowList = lits(0);
            if (!string.IsNullOrWhiteSpace(query))
                borrowList = borrowList.Where(s => s.BorrowName.Equals(query)).ToList();
            pager.totalRows = borrowList.Count();
            borrowList = borrowList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(borrowList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        public List<T_Borrow> lits(int status)
        {
            List<T_Borrow> listReissue = new List<T_Borrow>();
            IQueryable<T_BorrowApprove> listapprove = db.T_BorrowApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && !s.ApproveTime.HasValue);
            if (status == 1)//已审核
                listapprove = db.T_BorrowApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveTime.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in listapprove.Select(s => new { itemId = s.Pid }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Borrow model = db.T_Borrow.SingleOrDefault(s => s.ID == item && s.IsDelete == 0);
                if (model != null)
                    listReissue.Add(model);
            }
            return listReissue;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string query)
        {
            string sql = @"select BorrowName,BorrowDate,BorrowCode,BorrowMoney,BorrowReason,BorrowBank,BorrowAccountID,(select ApproveTime from T_BorrowApprove where ApproveName='" + UserModel.Nickname + "' and Pid=b.ID ) as time,(select ApproveStatus from T_BorrowApprove where ApproveName='" + UserModel.Nickname + "' and Pid=b.ID ) as status,BorrowSettementState from T_Borrow b where ID in(select Pid from T_BorrowApprove where  ApproveName='" + UserModel.Nickname + "' and ApproveTime is not null)";
            if (!string.IsNullOrWhiteSpace(query))
                sql += " AND BorrowName='" + query + "'";
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            List<Borrow> list = db.Database.SqlQuery<Borrow>(sql).ToList(); ;
            row1.CreateCell(0).SetCellValue("借支人");
            row1.CreateCell(1).SetCellValue("借支日期");
            row1.CreateCell(2).SetCellValue("借支批号");
            row1.CreateCell(3).SetCellValue("借支金额");
            row1.CreateCell(4).SetCellValue("借支事由");
            row1.CreateCell(5).SetCellValue("银行卡");
            row1.CreateCell(6).SetCellValue("开户行");
            row1.CreateCell(7).SetCellValue("审核日期");
            row1.CreateCell(8).SetCellValue("审核状态");
            row1.CreateCell(9).SetCellValue("是否结算");
            sheet1.SetColumnWidth(0, 25 * 256);
            sheet1.SetColumnWidth(1, 20 * 256);
            sheet1.SetColumnWidth(2, 20 * 256);
            sheet1.SetColumnWidth(3, 20 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 20 * 256);
            sheet1.SetColumnWidth(8, 20 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowName) ? "" : list[i].BorrowName);
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = true;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";

                rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowName) ? "" : list[i].BorrowName.ToString());
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowDate.ToString()) ? "" : list[i].BorrowDate.ToString());
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowCode) ? "" : list[i].BorrowCode.ToString());
                rowtemp.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(Convert.ToDecimal(list[i].BorrowMoney).ToString("0.00")) ? "0" : Convert.ToDecimal(list[i].BorrowMoney.ToString()).ToString("0.00"));
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowReason) ? "" : list[i].BorrowReason.ToString());
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowBank) ? "" : list[i].BorrowBank.ToString());
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(list[i].BorrowAccountID) ? "" : list[i].BorrowAccountID.ToString());
                rowtemp.CreateCell(7).SetCellValue(list[i].time.ToString());
                rowtemp.CreateCell(8).SetCellValue(list[i].status.ToString() == "1" ? "已同意" : "不同意");
                rowtemp.CreateCell(9).SetCellValue(list[i].BorrowSettementState.ToString() == "1" ? "已结算" : list[i].BorrowSettementState.ToString() == "-1" ? "未结算" : "部分结算");
            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(2);
            ICell heji2 = heji.CreateCell(3);
            heji1.SetCellValue("合计");
            heji2.SetCellValue(list.Sum(s => s.BorrowMoney).ToString());

            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "借支数据.xls");
        }

        /// <summary>
        /// 借支已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewBorrowCheckList(Lib.GridPager pager, string query)
        {
            List<T_Borrow> borrowList = lits(1);
            if (!string.IsNullOrWhiteSpace(query))
                borrowList = borrowList.Where(s => s.BorrowName.Equals(query)).ToList();
            pager.totalRows = borrowList.Count();
            borrowList = borrowList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(borrowList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        /// <summary>
        /// 借支保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewBorrowAddSave(T_Borrow model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    string borrowCode = "JZ-DE-";
                    string borrowDate = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_Borrow> lists = db.T_Borrow.Where(a => a.BorrowCode.Contains(borrowDate)).OrderByDescending(c => c.ID).ToList();
                    if (lists.Count == 0)
                    {
                        borrowCode += borrowDate + "-" + "0001";
                    }
                    else
                    {
                        string old = lists[0].BorrowCode.Substring(15);
                        int newcode = int.Parse(old) + 1;
                        borrowCode += borrowDate + "-" + newcode.ToString().PadLeft(4, '0');
                    }
                    model.BorrowCode = borrowCode;
                    model.BorrowState = -1;
                    model.BorrowSettementState = -1;
                    model.BorrowStep = 0;
                    model.BorrowDate = DateTime.Now;
                    model.IsDelete = 0;
                    model.IsVoucher = 0;
                    model.PrintCount = 0;
                    int deId = int.Parse(model.BorrowerDep);
                    model.BorrowerDep = db.T_Department.Find(deId).Name;
                    db.T_Borrow.Add(model);
                    db.SaveChanges();

                    T_BorrowApprove approve = new T_BorrowApprove
                    {
                        ApproveName = Com.GetNickName(model.BorrowNextApprove),
                        ApproveStatus = -1,
                        Pid = model.ID
                    };
                    db.T_BorrowApprove.Add(approve);


                    //应付
                    string codes = "KF-YF-";
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_AP> list = db.T_AP.Where(a => a.BillCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
                    if (list.Count == 0)
                    {
                        codes += date + "-" + "0001";
                    }
                    else
                    {
                        string old = list[0].BillCode.Substring(15);
                        int newcode = int.Parse(old) + 1;
                        codes += date + "-" + newcode.ToString().PadLeft(4, '0');
                    }
                    //应付
                    T_AP ap = new T_AP
                    {
                        BillCode = codes,
                        BillFromCode = model.BorrowCode,
                        BillMoney = Convert.ToDouble(model.BorrowMoney),
                        BillType = "借支申请",
                        CreateTime = DateTime.Now,
                        CreatUser = model.BorrowName,
                        PayMoney = Convert.ToDouble("-" + model.BorrowMoney)
                    };
                    db.T_AP.Add(ap);
                    db.SaveChanges();

                    List<T_ModularNotaudited> deleteItem = db.T_ModularNotaudited.Where(a => a.ModularName == "借支").ToList();
                    if (deleteItem.Count > 0)
                    {
                        foreach (var item in deleteItem)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
                        db.SaveChanges();
                    }

                    //ModularByZP();


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
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewBorrowEditSave(T_Borrow model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    var borrow = db.T_Borrow.Find(model.ID);
                    int deId = int.Parse(model.BorrowerDep);
                    borrow.BorrowerDep = db.T_Department.Find(deId).Name;
                    borrow.BorrowBank = model.BorrowBank;
                    borrow.BorrowAccountID = model.BorrowAccountID;
                    borrow.BorrowAccountName = model.BorrowAccountName;
                    borrow.BorrowNeedDate = model.BorrowNeedDate;
                    borrow.BorrowMoney = model.BorrowMoney;
                    borrow.BorrowNextApprove = model.BorrowNextApprove;
                    borrow.BorrowReason = model.BorrowReason;
					borrow.Company = model.Company;
                    db.SaveChanges();
                    if (model.BorrowStep == 0)
                    {
                        T_BorrowApprove approve = db.T_BorrowApprove.FirstOrDefault(a => a.Pid == model.ID && !a.ApproveTime.HasValue);
                        approve.ApproveName = Com.GetNickName(model.BorrowNextApprove);
                    }
                    else
                    {
                        T_BorrowApprove approve = new T_BorrowApprove();
                        approve.ApproveName = Com.GetNickName(model.BorrowNextApprove);
                        approve.ApproveStatus = -1;
                        borrow.BorrowStep = 0;
                        borrow.BorrowState = -1;
                        approve.Pid = model.ID;
                        db.T_BorrowApprove.Add(approve);
                    }
                    db.SaveChanges();
                    //ModularByZP();

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
        /// 作废
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ViewBorrowInvalid(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    T_Borrow model = db.T_Borrow.Find(id);
                    model.BorrowState = 3;
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

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewBorrowDelete(int id)
        {

            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    T_Borrow model = db.T_Borrow.Find(id);
                    model.IsDelete = 1;
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
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "借支").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_BorrowApprove where  Pid in ( select id from T_Borrow where IsDelete=0 and  (BorrowState=-1 or BorrowState=0 )) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "借支" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "借支";
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
            string RejectNumberSql = "select BorrowName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Borrow where BorrowState='2' and IsDelete=0  GROUP BY BorrowName";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "借支" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "借支";
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
        /// 审核
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <param name="nextapprove"></param>
        /// <param name="BorrowerFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Check(int approveID, int status, string memo, string nextapprove, string BorrowerFrom, string company, string number)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);



                    int TotalStep = db.T_BorrowApproveConfig.ToList().Count;
                    T_BorrowApprove approve = db.T_BorrowApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1 && (a.ApproveName == curName || a.ApproveName == Nickname));
                    if (approve==null)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" }, JsonRequestBehavior.AllowGet);
                    }
                    approve.ApproveStatus = status;
                    approve.ApproveTime = DateTime.Now;
                    approve.Memo = memo;
                    T_Borrow model = db.T_Borrow.Find(approve.Pid);


                    if (!string.IsNullOrEmpty(BorrowerFrom))
                        model.BorrowerFrom = BorrowerFrom;
                    int Step = model.BorrowStep;
                    Step++;
                    if (status == 2)
                    {

                        model.BorrowState = 2;
                        model.BorrowStep = Step;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (TotalStep == Step)
                        {
                            if (company == "==请选择==")
                                return Json(new { State = "Faile", Message = "请选择公司" }, JsonRequestBehavior.AllowGet);
                            model.BorrowState = status;
                            model.SpendingCompany = company;
                            model.SpendingNumber = number;
                            //应收
                            string codes = "KF-YS-";
                            string date = DateTime.Now.ToString("yyyyMMdd");
                            //查找当前已有的编号
                            List<T_AR> list = db.T_AR.Where(a => a.BillCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
                            if (list.Count == 0)
                            {
                                codes += date + "-" + "0001";
                            }
                            else
                            {
                                string old = list[0].BillCode.Substring(15);
                                int newcode = int.Parse(old) + 1;
                                codes += date + "-" + newcode.ToString().PadLeft(4, '0');
                            }

                            //应收
                            T_AR ar = new T_AR
                            {
                                BillCode = codes,
                                BillCompany = model.BorrowerFrom,
                                BillFromCode = model.BorrowCode,
                                BillMoney = Convert.ToDouble(model.BorrowMoney),
                                BillType = "借支申请",
                                CreateTime = DateTime.Now,
                                CreatUser = UserModel.Name,
                                ReceivedMony = Convert.ToDouble(model.BorrowMoney)
                            };

                            //实付
                            string codes1 = "KF-FK-";
                            string date1 = DateTime.Now.ToString("yyyyMMdd");
                            //查找当前已有的编号
                            List<T_PP> list1 = db.T_PP.Where(a => a.BillCode.Contains(date1)).OrderByDescending(c => c.ID).ToList();
                            if (list1.Count == 0)
                            {
                                codes1 += date1 + "-" + "0001";
                            }
                            else
                            {
                                string old = list1[0].BillCode.Substring(15);
                                int newcode = int.Parse(old) + 1;
                                codes1 += date1 + "-" + newcode.ToString().PadLeft(4, '0');
                            }

                            //实付
                            T_PP pp = new T_PP
                            {
                                BillCode = codes1,
                                BillCompany = model.BorrowerFrom,
                                BillFromCode = model.BorrowCode,
                                BillMoney = Convert.ToDouble(model.BorrowMoney),
                                BillType = "借支申请",
                                CreateTime = DateTime.Now,
                                CreatUser = model.BorrowName,
                                PayMoney = Convert.ToDouble("-" + model.BorrowMoney)
                            };
                            db.T_PP.Add(pp);
                            db.T_AR.Add(ar);
                            db.SaveChanges();
                        }
                        else
                        {
                             string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                            T_BorrowApprove newApprove = new T_BorrowApprove();
                            newApprove.ApproveStatus = -1;
                            if (name == "罗瑶" && model.BorrowStep==2)
                            {
                                 newApprove.ApproveName="三疯";
                                 model.Cashier = nextapprove;
                            }
                            else if (name == "三疯" && model.BorrowStep == 3)
                            {
                                newApprove.ApproveName =model.Cashier;
                                nextapprove = model.Cashier;
                            }
                            else
                            {
                                newApprove.ApproveName = nextapprove;
                            }
                       

                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_BorrowApprove.Add(newApprove);

                            T_User u = db.T_User.FirstOrDefault(a => a.Nickname.Equals(nextapprove));
                            model.BorrowNextApprove = u.ID;
                            model.BorrowState = 0;
                        }
                        model.BorrowStep = Step;
                        db.SaveChanges();
                    }
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


        #endregion

        #region 结算借支

        /// <summary>
        /// 结算借支列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="userName"></param>
        /// <param name="status"></param>
        /// <param name="setmentStatus"></param>
        /// <returns></returns>
        public ContentResult GetViewBorrowSetementList(Lib.GridPager pager, string userName, int setmentStatus = -2)
        {
            List<ViewBorrowSetement> settementList = new List<ViewBorrowSetement>();
            IQueryable<T_Borrow> list = db.T_Borrow.Where(s => s.BorrowState == 1 && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
                list = list.Where(s => s.BorrowName.Equals(userName));
            if (setmentStatus != -2)
                list = list.Where(s => s.BorrowSettementState == setmentStatus);
            pager.totalRows = list.Count();
            List<T_Borrow> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            foreach (var item in querData)
            {
                ViewBorrowSetement model = new ViewBorrowSetement
                {
                    ID = item.ID,
                    PostUser = item.BorrowName,
                    Code = item.BorrowCode,
                    BorrowCost = item.BorrowMoney,
                    BorrowDate = item.BorrowDate,
                    BorrowReason = item.BorrowReason,
                    BorrowStatus = item.BorrowState,
                    DepartMent = item.BorrowerDep,
                    SettementStatus = item.BorrowSettementState,
                    SettementCost = db.T_BorrowSettementRecord.FirstOrDefault(s => s.BorrowCode.Equals(item.BorrowCode)) == null ? 0 : db.T_BorrowSettementRecord.Where(s => s.BorrowCode.Equals(item.BorrowCode)).Sum(s => s.SettementCost),
                    SurplusCost = db.T_BorrowSettementRecord.FirstOrDefault(s => s.BorrowCode.Equals(item.BorrowCode)) == null ? item.BorrowMoney : item.BorrowMoney - db.T_BorrowSettementRecord.Where(s => s.BorrowCode.Equals(item.BorrowCode)).Sum(s => s.SettementCost)
                };
                settementList.Add(model);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(settementList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="code"></param>
        /// <param name="settementCost"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewSettementSave(string code, string settementCost, string SurplusCost, string remark, int num, string reunCode)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    var model = db.T_Borrow.FirstOrDefault(s => s.BorrowCode.Equals(code));
                    var recordlist = db.T_BorrowSettementRecord.Where(s => s.BorrowCode.Equals(model.BorrowCode));
                    if (recordlist.Count() == 0)
                    {
                        if (decimal.Parse(settementCost) > model.BorrowMoney)
                        {
                            return Json(new { State = "Faile", Message = "结算金额不能大于借支金额" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (decimal.Parse(settementCost) > decimal.Parse(SurplusCost))
                        {
                            return Json(new { State = "Faile", Message = "结算金额不能大于剩余金额" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    string type = "";
                    if (num == 1)
                        type = "现金结算";
                    else
                    {
                        type = "报销冲抵";
                        code = reunCode;
                        var expense = db.T_Expense.FirstOrDefault(s => s.Reun_Code.Equals(reunCode));
                        expense.IsExpenseMatch = true;
                        db.SaveChanges();
                    }
                    T_BorrowSettementRecord record = new T_BorrowSettementRecord
                    {
                        BorrowCode = model.BorrowCode,
                        SettementUser = UserModel.Name,
                        BorrowType = type,
                        ReunCode = reunCode,
                        BorrowCost = model.BorrowMoney,
                        SettementCost = decimal.Parse(settementCost),
                        SurplusCost = recordlist.Count() == 0 ? model.BorrowMoney - decimal.Parse(settementCost) : decimal.Parse(SurplusCost) - decimal.Parse(settementCost),
                        Remark = remark,
                        SettementDate = DateTime.Now
                    };
                    db.T_BorrowSettementRecord.Add(record);
                    db.SaveChanges();
                    //实收
                    string codes = "KF-SK-";
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_PR> list = db.T_PR.Where(a => a.BillCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
                    if (list.Count == 0)
                    {
                        codes += date + "-" + "0001";
                    }
                    else
                    {
                        string old = list[0].BillCode.Substring(15);
                        int newcode = int.Parse(old) + 1;
                        codes += date + "-" + newcode.ToString().PadLeft(4, '0');
                    }

                    //实收
                    T_PR pr = new T_PR
                    {
                        BillCode = codes,
                        BillCompany = model.BorrowerFrom,
                        BillFromCode = code,
                        BillMoney = Convert.ToDouble(model.BorrowMoney),
                        BillType = type,
                        CreateTime = DateTime.Now,
                        CreatUser = UserModel.Name,
                        ReceivedMony = Convert.ToDouble(record.SettementCost)
                    };
                    db.T_PR.Add(pr);
                    db.SaveChanges();
                    model.BorrowSettementState = 0;
                    if (recordlist.Sum(s => s.SettementCost) == model.BorrowMoney)
                    {
                        model.BorrowSettementState = 1;
                    }
                    db.SaveChanges();
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
        /// 结算记录
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ContentResult ViewSetementRecordList(Lib.GridPager pager, string code)
        {
            IQueryable<T_BorrowSettementRecord> list = db.T_BorrowSettementRecord.Where(s => s.BorrowCode.Equals(code)).AsQueryable();
            pager.totalRows = list.Count();
            List<T_BorrowSettementRecord> lists = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(lists, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获得已审核并且未报销冲抵的报销单
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewExpenseCodeIscheckList(Lib.GridPager pager, string code)
        {

            IQueryable<T_Expense> list = db.T_Expense.Where(s => s.Status == 1 && s.IsExpenseMatch == false).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Equals(code));
            pager.totalRows = list.Count();
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

        #region 凭证

        /// <summary>
        /// 获取凭证
        /// </summary>
        /// <param name="borrowId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetBorrowPzList(int borrowId)
        {
            IQueryable<T_Borrow> list = db.T_Borrow.Where(s => s.ID == borrowId).AsQueryable();
            List<T_Borrow> querData = list.OrderBy(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //凭证新增 
        public JsonResult ViewPZSave(string jsonStr, int borrowId)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "数据有误" });

                    List<T_Borrow> productList = Com.Deserialize<T_Borrow>(jsonStr);
                    T_Borrow model = db.T_Borrow.Find(borrowId);
                    foreach (var item in productList)
                    {
                        T_Borrow product = db.T_Borrow.Find(item.ID);
                        product.PZ_Department = item.PZ_Department;
                        product.PZ_Subject = item.PZ_Subject;
                        product.PZ_Direction = item.PZ_Direction;
                        db.SaveChanges();
                    }
                    model.IsVoucher = -1;
                    model.Pz_JZMemo = "";
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
                    T_Borrow borrow = db.T_Borrow.Find(id);
                    borrow.IsVoucher = status;
                    borrow.Pz_JZMemo = memo;
                    db.SaveChanges();
                    if (status == 1)
                    {
                        List<T_Borrow> borrorList = db.T_Borrow.Where(s => s.ID == id).ToList();
                        foreach (var item in borrorList)
                        {
                            T_PZ_JZ bx = new T_PZ_JZ
                            {
                                PZ_OrderNum = borrow.BorrowCode,
                                PZ_Summary = item.BorrowReason,
                                PZ_Department = item.PZ_Department,
                                PZ_Direction = item.PZ_Direction,
                                PZ_Money = borrow.BorrowMoney,
                                PZ_Subject = item.PZ_Subject,
                                PZ_Time = DateTime.Now
                            };
                            db.T_PZ_JZ.Add(bx);
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

        //可以生成凭证的列表
        [HttpPost]
        public ContentResult ViewVoucherList(Lib.GridPager pager, string userName, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_Borrow> list = db.T_Borrow.Where(s => s.IsVoucher != 1 && s.BorrowState == 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
                list = list.Where(s => s.BorrowName.Equals(userName));
            if (status != -2)
                list = list.Where(s => s.BorrowState == status);
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.BorrowDate >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.BorrowDate <= end);
            }
            pager.totalRows = list.Count();
            List<T_Borrow> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //凭证列表 [HttpPost]
        public ContentResult ViewVoucherTable(Lib.GridPager pager, string userName, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_PZ_JZ> list = db.T_PZ_JZ.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userName))
                list = list.Where(s => s.PZ_OrderNum.Equals(userName));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                list = list.Where(s => s.PZ_Time >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.PZ_Time <= end);
            }
            pager.totalRows = list.Count();
            List<T_PZ_JZ> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion

        #endregion

    }
}
