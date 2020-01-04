using EBMS.App_Code;
using EBMS.Models;
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

namespace EBMS.Controllers
{
    public class LineExpenseController : Controller
    {
        //
        // GET: /LineExpense/

        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }
        public class ExpenseCost
        {
            public string Car_Number { get; set; }
            public decimal Reun_Cost { get; set; }
        }
        /// <summary>
        /// 选择费用类别
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ActionResult ViewExpenseCostType(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        public ActionResult ViewLineExpense()
        {
           
            return View();
        }
        public ActionResult ViewLineExpenseList()
        {

            return View();
        }
        public ActionResult ViewLineExpenseChecken()
        {

            return View();
        }
        [Description("报销打印")]
        public ActionResult ViewLineExpensePrint(int id, int page = 1)
        {
            ViewData["Bid"] = id;
            T_LineExpense model = db.T_LineExpense.Find(id);
            ViewData["jine"] = model.Reun_Cost;
            ViewData["code"] = model.Reun_Code;
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
            IQueryable<T_LineExpenseProduct> list = db.T_LineExpenseProduct.Where(a => a.ReunId == id);
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
            return View();
        }

        /// <summary>
        /// 获取报销打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetExpensePrint(int id)
        {
            T_LineExpense model = db.T_LineExpense.Find(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取报销明细打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetExpenseProductPrint(int expenseId, int page)
        {
            IQueryable<T_LineExpenseProduct> list = db.T_LineExpenseProduct.Where(a => a.ReunId == expenseId);
            int totalRows = list.Count();
            List<T_LineExpenseProduct> modelList = list.OrderBy(c => c.ID).Skip((page - 1) * 5).Take(5).ToList();
            return Json(modelList, JsonRequestBehavior.AllowGet);
        }   
        public ActionResult ViewLineExpenseReportcheck(int id)
        {
            if (id == 0)
                return HttpNotFound();
            var history = db.T_LineExpenseApprove.Where(a => a.Reunbursement_id == id);
            var model = db.T_Expense.Find(id);
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
            T_LineExpenseApprove APPModel=db.T_LineExpenseApprove.SingleOrDefault(a => a.Reunbursement_id == id&&a.ApproveDate==null);
            if(APPModel!=null)
            {
                ViewData["NameType"] = APPModel.ApproveName;
            }
            //获取审核表中的 审核记录ID
            T_LineExpenseApprove approve = db.T_LineExpenseApprove.FirstOrDefault(a => !a.ApproveDate.HasValue && a.Reunbursement_id == id);
            if (approve != null)
                ViewData["approveid"] = approve.ID;
            else
            {
                ViewData["approveid"] = 0;
            }
            //BindApproveName(model.Step + 1);
            //int Step = db.T_ExpenseApproveConfig.ToList().Count;
            //ViewData["Step"] = Step;
            ViewData["comPanyIn"] = Com.LineExpenseAcount();
            return View(model);
          
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


            string sql = string.Format(@"select (select MatchBorrowNumber from T_LineExpense where ID=p.ReunId) as  MatchBorrowNumber,(select Reun_Code from T_LineExpense where ID=p.ReunId) as  Reun_Code,(select CrateDate from T_LineExpense where ID=p.ReunId) as  CrateDate, 
                                        (select PostUser from T_LineExpense where ID=p.ReunId) as PostUser,
                                        (select Reun_Reason from T_LineExpense where ID=p.ReunId) as Reun_Reason,
                                            StoreName,Abstract,Price,Num,
                                    (select top 1 ApproveDate from T_LineExpenseApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as ApproveDate,(select top 1 Remark from T_LineExpenseApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as Remark from T_LineExpenseProduct p where ReunId in(select Reunbursement_id from T_LineExpenseApprove where ApproveDate>='" + statedate + "' and ApproveDate<='" + EndDate + " 23:59:59' AND  ApproveName='" + UserModel.Nickname + "')");

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
            row1.CreateCell(3).SetCellValue("产品名称");
            row1.CreateCell(4).SetCellValue("单价");
            row1.CreateCell(5).SetCellValue("数量");
            row1.CreateCell(6).SetCellValue("应付金额");
            row1.CreateCell(7).SetCellValue("审核日期");
            row1.CreateCell(8).SetCellValue("报销单号");
            row1.CreateCell(9).SetCellValue("审核备注");
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
                rowtemp.CreateCell(3).SetCellValue(string.IsNullOrWhiteSpace(list[i].Abstract) ? "" : list[i].Abstract);
                rowtemp.CreateCell(4).SetCellValue(Convert.ToDouble(list[i].Price).ToString("0.00"));
                rowtemp.CreateCell(5).SetCellValue(list[i].Num.ToString());
                rowtemp.CreateCell(6).SetCellValue(Convert.ToDouble(Convert.ToDouble(list[i].Price) * Convert.ToDouble(list[i].Num)).ToString("0.00"));
                rowtemp.CreateCell(7).SetCellValue(list[i].approveDate.ToString());
                rowtemp.CreateCell(8).SetCellValue(list[i].Reun_Code);
                rowtemp.CreateCell(9).SetCellValue(list[i].Remark);
               
                cost += Convert.ToDouble(Convert.ToDouble(list[i].Price) * Convert.ToDouble(list[i].Num));
            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(6);
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
        [Description("报销未审核")]
        public ActionResult ViewLineExpenseChecked()
        {
            return View();
        }
        public ActionResult ViewLineExpenseAdd()
        {
            //绑定部门
            ViewData["ReportDepartment"] = Com.LinDepartMent();
            string code = "KYBX";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_LineExpense> lists = db.T_LineExpense.Where(a => a.Reun_Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
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
            T_LineExpense model = new T_LineExpense();
            model.Reun_Code = code;
            model.PostUser = UserModel.Name;
            model.Department = UserModel.DepartmentId;
            return View(model);
        }
        /// <summary>
        /// 未审核列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewExpenseNocheckList(Lib.GridPager pager, string user)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_LineExpenseGroup> GroupModel = db.T_LineExpenseGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_LineExpenseApprove> ApproveMod = db.T_LineExpenseApprove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveDate == null).ToList();
            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                if (i == 0)
                {
                    arrID += ApproveMod[i].Reunbursement_id.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Reunbursement_id.ToString();
                }
            }
            string sql = "select * from T_LineExpense r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            List<T_LineExpense> expenseList = db.Database.SqlQuery<T_LineExpense>(sql).ToList();
          //  List<T_LineExpense> expenseList = new List<T_LineExpense>();
            //IQueryable<T_LineExpenseApprove> list = db.T_LineExpenseApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && !s.ApproveDate.HasValue);
            //List<int> itemIds = new List<int>();
            //foreach (var item in list.Select(s => new { itemId = s.Reunbursement_id }).GroupBy(s => s.itemId))
            //{
            //    itemIds.Add(int.Parse(item.Key.ToString()));
            //}

            //foreach (var item in itemIds)
            //{
            //    T_LineExpense model = db.T_LineExpense.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
            //    if (model != null)
            //        expenseList.Add(model);
            //}
            if (!string.IsNullOrWhiteSpace(user))
                expenseList = expenseList.Where(s => s.PostUser.Contains(user)).ToList();

            pager.totalRows = expenseList.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (expenseList.Count() > 0)
                footer.Reun_Cost =decimal.Parse(expenseList.Sum(s => s.Reun_Cost).ToString());
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_LineExpense> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("编辑报销")]
        public ActionResult ViewLineExpenseEdit(int id)
        {
            var model = db.T_LineExpense.Find(id);
            if (model == null)
                return HttpNotFound();
           // BindApproveName(model.Step);
            //if (!string.IsNullOrWhiteSpace(model.Shop))
            //    model.Shop = db.T_ShopFromGY.FirstOrDefault(s => s.name.Equals(model.Shop)).number;
      //      model.Department = db.T_Department.FirstOrDefault(s => s.Name.Equals(model.Department)).ID.ToString();
            //ViewData["ReportDepartment"] = Com.DepartMent(model.Department);
           // ViewData["BorrowCode"] = GetBorrow(model.MatchBorrowNumber);
            return View(model);
        }
     
        //获取附件
        public JsonResult GetExpenseEnclosure(int id)
        {
            List<T_LineExpenseEnclosure> model = db.T_LineExpenseEnclosure.Where(a => a.ExpenseId == id).ToList();
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
        //附件删除
        public void DeleteModelFile(string path, int id = 0)
        {
            string strPath = path;
            path = Server.MapPath(path);
            //获得文件对象
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }
            if (id != 0)
            {
                T_LineExpenseEnclosure model = db.T_LineExpenseEnclosure.FirstOrDefault(a => a.ExpenseId == id && a.Path == strPath);
                if (model != null)
                {
                    db.T_LineExpenseEnclosure.Remove(model);
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 报销已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewExpenseCheckedList(Lib.GridPager pager, string user, string createDate, string endDate)
        {
            List<T_LineExpense> expenseList = new List<T_LineExpense>();
            IQueryable<T_LineExpenseApprove> list = db.T_LineExpenseApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveDate.HasValue);
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
                itemIds.Add(int.Parse(item.Key.ToString()));
            }

            foreach (var item in itemIds)
            {
                T_LineExpense model = db.T_LineExpense.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
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
                footer.Reun_Cost = decimal.Parse(expenseList.Sum(s => s.Reun_Cost).ToString());
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_LineExpense> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 我的报销
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="Code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewExpenseListForMyList(Lib.GridPager pager, string code, int status = -2)
        {
            IQueryable<T_LineExpense> list = db.T_LineExpense.Where(s => s.PostUser.Equals(UserModel.Name) && s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Equals(code));
            if (status != -2)
                list = list.Where(s => s.Status == status);
            pager.totalRows = list.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = decimal.Parse(list.Sum(s => s.Reun_Cost).ToString());
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_LineExpense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 报销列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ContentResult ViewExpenseList(Lib.GridPager pager, string code, string startDate, string endDate, int status = -2)
        {
            IQueryable<T_LineExpense> list = db.T_LineExpense.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Equals(code));
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
                footer.Reun_Cost =decimal.Parse(list.Sum(s => s.Reun_Cost).ToString());
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_LineExpense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("报销明细")]
        public ActionResult ViewLineExpenseDetail(int expenseId)
        {
            if (expenseId == 0)
                return HttpNotFound();
            ViewData["expenseId"] = expenseId;
            var history = db.T_LineExpenseApprove.Where(a => a.Reunbursement_id == expenseId);
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
      
        /// <summary>
        /// 报销明细数据
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewExpenseProductList(int expenseId)
        {
            IQueryable<T_LineExpenseProduct> list = db.T_LineExpenseProduct.Where(s => s.ReunId == expenseId).AsQueryable();
            List<T_LineExpenseProduct> querData = list.OrderBy(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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

        
        /// <summary>
        /// 下线报销保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewExpenseAddSave(T_LineExpense model, string jsonStr, string jsonStr1, string picUrls)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return Json(new { State = "Faile", Message = "请添加报销明细" });
            }
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_LineExpenseProduct> details = Com.Deserialize<T_LineExpenseProduct>(jsonStr);
                    string expensecode = "KYBX";
                    string expensedate = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_LineExpense> lists = db.T_LineExpense.Where(a => a.Reun_Code.Contains(expensedate)).OrderByDescending(c => c.ID).ToList();
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
                   // int departId = int.Parse(model.Department);
                 //   model.Department = db.T_Department.Find(departId).Name;
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

                    db.T_LineExpense.Add(model);
                    db.SaveChanges();

                    T_LineExpenseApproveConfig Config = db.T_LineExpenseApproveConfig.SingleOrDefault(a=>a.Step==0 );
                   // T_LineExpenseGroup GroupModel = db.T_LineExpenseGroup.SingleOrDefault(a=>a.GroupName);
                    string APPName = "";
                    if (Config.ApproveUser != "" && Config.ApproveUser != null)
                    {
                        APPName = Config.ApproveUser;
                    }
                    else
                    {
                        APPName = Config.ApproveType;
                    }
                    //审核记录
                    T_LineExpenseApprove Approvemodel = new T_LineExpenseApprove
                    {

                        ApproveName = APPName,
                        Step="1",
                        ApproveStatus = -1,
                        ApproveDName = Config.ApproveType,
                        Reunbursement_id = model.ID
                    };
                    db.T_LineExpenseApprove.Add(Approvemodel);
                    db.SaveChanges();
                    //添加详情
                    foreach (var item in details)
                    {
                        item.ReunId = model.ID;
                        db.T_LineExpenseProduct.Add(item);
                    }
                    db.SaveChanges();
                    //应付
                    //string codes = "KF-YF-";
                    //string date = DateTime.Now.ToString("yyyyMMdd");
                    ////查找当前已有的编号
                    //List<T_AP> list = db.T_AP.Where(a => a.BillCode.Contains(date)).OrderByDescending(c => c.ID).ToList();
                    //if (list.Count == 0)
                    //{
                    //    codes += date + "-" + "0001";
                    //}
                    //else
                    //{
                    //    string old = list[0].BillCode.Substring(15);
                    //    int newcode = int.Parse(old) + 1;
                    //    codes += date + "-" + newcode.ToString().PadLeft(4, '0');
                    //}
                    ////应付
                    //T_AP ap = new T_AP
                    //{
                    //    BillCode = codes,
                    //    BillCompany = model.Reun_Bank,
                    //    BillFromCode = model.Reun_Code,
                    //    BillMoney = Convert.ToDouble(model.Reun_Cost),
                    //    BillType = "报销申请",
                    //    CreateTime = DateTime.Now,
                    //    CreatUser = model.PostUser,
                    //    PayMoney = Convert.ToDouble("-" + model.Reun_Cost)
                    //};
                    //db.T_AP.Add(ap);
                    //db.SaveChanges();
                    //附件保存
                    if (!string.IsNullOrWhiteSpace(picUrls))
                    {
                        string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        // List<T_ExpenseEnclosure> Enclosure = Com.Deserialize<T_ExpenseEnclosure>(picUrls);
                        foreach (string item in picArr)
                        {
                            T_LineExpenseEnclosure Enclosure = new T_LineExpenseEnclosure();
                            Enclosure.Scdate = DateTime.Now;
                            Enclosure.ExpenseId = model.ID;
                            Enclosure.Url = item;
                            Enclosure.Path = item;
                            string[] ss = item.Split('/');
                            Enclosure.ScName = ss[ss.Length - 1];
                            Enclosure.Size = "0";

                            db.T_LineExpenseEnclosure.Add(Enclosure);
                        }
                        db.SaveChanges();
                    }

                    //List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "报销").ToList();
                    //if (ModularNotaudited.Count > 0)
                    //{
                    //    foreach (var item in ModularNotaudited)
                    //    {
                    //        db.T_ModularNotaudited.Remove(item);
                    //    }
                    //    db.SaveChanges();
                    //}

                    //string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpenseApprove where Reunbursement_id in (select ID from T_Expense where IsDelete=0  and Status!=3) and  ApproveStatus=-1 and ApproveDate is null GROUP BY ApproveName";
                    //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                    //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                    //{
                    //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                    //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                    //    if (NotauditedModel != null)
                    //    {
                    //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                    //    }
                    //    else
                    //    {
                    //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    //        ModularNotauditedModel.ModularName = "报销";
                    //        ModularNotauditedModel.RejectNumber = 0;
                    //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    //        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    //        ModularNotauditedModel.ToupdateName = Nickname;
                    //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    //    }
                    //    db.SaveChanges();
                    //}
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
        ///报销编辑保存 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewExpenseEditSave(T_LineExpense model, string jsonStr, string jsonStr1)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return Json(new { State = "Faile", Message = "请添加报销明细" });
            }
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    List<T_LineExpenseProduct> details = Com.Deserialize<T_LineExpenseProduct>(jsonStr);
                    T_LineExpense expense = db.T_LineExpense.Find(model.ID);
                  //  int departId = int.Parse(model.Department);
                    if (model.Status != -1)
                    {
                        T_LineExpenseApproveConfig Config = db.T_LineExpenseApproveConfig.SingleOrDefault(a => a.Step == 0);
                        // T_LineExpenseGroup GroupModel = db.T_LineExpenseGroup.SingleOrDefault(a=>a.GroupName);
                        string APPName = "";
                        if (Config.ApproveUser != "" && Config.ApproveUser != null)
                        {
                            APPName = Config.ApproveUser;
                        }
                        else
                        {
                            APPName = Config.ApproveType;
                        }
                        T_LineExpenseApprove Approvemodel = new T_LineExpenseApprove
                        {
                            ApproveName = APPName,
                            ApproveDName=Config.ApproveType,
                            ApproveStatus = -1,
                            Reunbursement_id = model.ID
                        };
                        db.T_LineExpenseApprove.Add(Approvemodel);
                        db.SaveChanges();
                    }
                 
                    //if (model.Status == -1)
                    //{
                    //    T_LineExpenseApprove Approve = db.T_LineExpenseApprove.SingleOrDefault(s => s.Reunbursement_id == model.ID && !s.ApproveDate.HasValue);
                    //    //Approve.ApproveName = Com.GetNickName(model.ExpenseNextApprove);
                    //    db.SaveChanges();
                    //}

                    expense.Car_Number = model.Car_Number;
                    expense.Reun_Bank = model.Reun_Bank;
                    expense.Reun_Name = model.Reun_Name;
                    if (!string.IsNullOrWhiteSpace(jsonStr1))
                        expense.IsExpenseEnclosure = 1;
                    else
                        expense.IsExpenseEnclosure = 0;
                 //   expense.Department = db.T_Department.Find(departId).Name;
                    expense.Status = -1;
                    expense.IsBlending = model.IsBlending;
                    expense.Memo = model.Memo;
                    expense.AccountType = model.AccountType;
                    expense.Reun_Reason = model.Reun_Reason;
                   // expense.MatchBorrowNumber = model.MatchBorrowNumber;
                    expense.Reun_Cost = details.Sum(s => s.Num * s.Price);
                    //if (!string.IsNullOrWhiteSpace(model.Shop))
                    //    expense.Shop = db.T_ShopFromGY.FirstOrDefault(s => s.number.Equals(model.Shop)).name;
                    db.SaveChanges();

                    //foreach (var item in db.T_ExpenseProduct.Where(s => s.ReunId == model.ID))
                    //{
                    //    db.T_ExpenseProduct.Remove(item);
                    //}
                    //db.SaveChanges();
                    //删除
                    List<T_LineExpenseProduct> productList = db.T_LineExpenseProduct.Where(s => s.ReunId == model.ID).ToList();
                    List<int> lists = productList.Select(s => s.ID).Except(details.Select(s => s.ID)).ToList();
                    foreach (var item in lists)
                    {
                        if (item != 0)
                        {
                            T_LineExpenseProduct product = db.T_LineExpenseProduct.Find(item);
                            db.T_LineExpenseProduct.Remove(product);
                            db.SaveChanges();
                        }
                    }
                    foreach (var item in details)
                    {
                        if (item.ID == 0)
                        {
                            T_LineExpenseProduct pro = new T_LineExpenseProduct
                            {
                                ReunId = model.ID,
                                Abstract = item.Abstract,
                                Num = int.Parse(item.Num.ToString()),
                                Price =decimal.Parse(item.Price.ToString()),
                                Type = item.Type,
                                StoreName = item.StoreName
                            };
                            db.T_LineExpenseProduct.Add(pro);
                        }
                        else
                        {
                            T_LineExpenseProduct product = db.T_LineExpenseProduct.Find(item.ID);
                            product.Abstract = item.Abstract;
                            product.Num = int.Parse(item.Num.ToString());
                            product.Price =decimal.Parse(item.Price.ToString());
                            product.Type = item.Type;
                            product.StoreName = item.StoreName;
                        }
                        db.SaveChanges();
                    }

                    //附件保存 先删除原有的附件
                    List<T_LineExpenseEnclosure> delMod = db.T_LineExpenseEnclosure.Where(a => a.ExpenseId == model.ID).ToList();
                    foreach (var item in delMod)
                    {
                        db.T_LineExpenseEnclosure.Remove(item);
                    }
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(jsonStr1))
                    {
                        List<T_LineExpenseEnclosure> enclosure = Com.Deserialize<T_LineExpenseEnclosure>(jsonStr1);
                        foreach (var item in enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ExpenseId = model.ID;
                            db.T_LineExpenseEnclosure.Add(item);
                        }
                        db.SaveChanges();
                    }
                    //List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "报销").ToList();
                    //if (ModularNotaudited.Count > 0)
                    //{
                    //    foreach (var item in ModularNotaudited)
                    //    {
                    //        db.T_ModularNotaudited.Remove(item);
                    //    }
                    //    db.SaveChanges();
                    //}

                    //string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpenseApprove where Reunbursement_id in (select ID from T_Expense where IsDelete=0  and Status!=3) and  ApproveStatus=-1 and ApproveDate is null GROUP BY ApproveName";
                    //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                    //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                    //{
                    //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                    //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                    //    if (NotauditedModel != null)
                    //    {
                    //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                    //    }
                    //    else
                    //    {
                    //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    //        ModularNotauditedModel.ModularName = "报销";
                    //        ModularNotauditedModel.RejectNumber = 0;
                    //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    //        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    //        ModularNotauditedModel.ToupdateName = Nickname;
                    //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    //    }
                    //    db.SaveChanges();
                    //}

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
        public JsonResult ViewExpenseInvalid(int id)
        {
            try
            {

                T_LineExpense model = db.T_LineExpense.Find(id);
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
                //string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpenseApprove where Reunbursement_id in (select ID from T_Expense where IsDelete=0  and Status!=3) and  ApproveStatus=-1 and ApproveDate is null GROUP BY ApproveName";
                //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                //{
                //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                //    if (NotauditedModel != null)
                //    {
                //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                //    }
                //    else
                //    {
                //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                //        ModularNotauditedModel.ModularName = "报销";
                //        ModularNotauditedModel.RejectNumber = 0;
                //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                //        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                //        ModularNotauditedModel.ToupdateName = Nickname;
                //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
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
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewExpenseDelete(int id)
        {
            try
            {

                T_LineExpense model = db.T_LineExpense.Find(id);
                model.IsDelete = 1;
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
                //string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpenseApprove where Reunbursement_id in (select ID from T_Expense where IsDelete=0  and Status!=3) and  ApproveStatus=-1 and ApproveDate is null GROUP BY ApproveName";
                //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                //{
                //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                //    if (NotauditedModel != null)
                //    {
                //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                //    }
                //    else
                //    {
                //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                //        ModularNotauditedModel.ModularName = "报销";
                //        ModularNotauditedModel.RejectNumber = 0;
                //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                //        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                //        ModularNotauditedModel.ToupdateName = Nickname;
                //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
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
                  

                    string result = "";
                    int ID = approveID;
                   T_LineExpense LineExpense = db.T_LineExpense.SingleOrDefault(a => a.ID == ID && a.IsDelete == 0);
                   if (LineExpense == null)
                   {
                       return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                   }
                   T_LineExpenseApprove modelApprove = db.T_LineExpenseApprove.FirstOrDefault(a => a.Reunbursement_id == ID && a.ApproveDate == null);
                   string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                   modelApprove.ApproveName = Nickname;
                   modelApprove.Remark = memo;
                   modelApprove.ApproveDate = DateTime.Now;
                   modelApprove.ApproveStatus = int.Parse(status.ToString());
                   db.Entry<T_LineExpenseApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                   int i = db.SaveChanges();

                   if (company != "" && company != "==请选择==")
                   {
                       LineExpense.SpendingNumber = company;
                       LineExpense.SpendingCompany = company;
                   }
                   if (i > 0)
                   {
                       if (status == 1)
                       {
                           T_LineExpenseApprove newApprove = new T_LineExpenseApprove();
                           int step = int.Parse(LineExpense.Step.ToString());
                           step++;
                           IQueryable<T_LineExpenseApproveConfig> config = db.T_LineExpenseApproveConfig.AsQueryable();
                           int stepLength = config.Count();//总共步骤
                           if (step < stepLength)
                           {
                               LineExpense.Status = 0;
                               T_LineExpenseApproveConfig stepMod = db.T_LineExpenseApproveConfig.SingleOrDefault(a => a.Step == step);
                               string nextName = stepMod.ApproveUser;
                               newApprove.Remark = "";
                               newApprove.Reunbursement_id = ID;
                               newApprove.ApproveStatus = -1;
                               newApprove.Step = step.ToString();
                               if (nextName != null)
                               {
                                   newApprove.ApproveName = nextName;
                                   newApprove.ApproveDName = stepMod.ApproveType;
                               }
                               else
                               {
                                   newApprove.ApproveName = stepMod.ApproveType;
                                   newApprove.ApproveDName = stepMod.ApproveType;
                               }
                               db.T_LineExpenseApprove.Add(newApprove);
                               db.SaveChanges();

                           }
                           else
                           {
                               LineExpense.Status = int.Parse(status.ToString());
                           }
                           LineExpense.Step = step;
                           db.Entry<T_LineExpense>(LineExpense).State = System.Data.Entity.EntityState.Modified;
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
                           LineExpense.Step = 0;
                           LineExpense.Status = 2;
                           db.Entry<T_LineExpense>(LineExpense).State = System.Data.Entity.EntityState.Modified;
                           db.SaveChanges();
                           //审核流程结束 申请人编辑后插入下一条记录 
                           result = "保存成功";
                       }
                   }
                   else
                   {
                       result = "保存失败";
                   }

                    sc.Complete();
                    return Json(new { State = "Success", Message = result }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = "异常错误" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
