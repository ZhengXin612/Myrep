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
using System.Web.Script.Serialization;

namespace EBMS.Controllers
{
    /// <summary>
    /// 报销
    /// </summary>
    public class ExpenseController : BaseController
    {


        #region 公共属性/字段/方法

        EBMSEntities db = new EBMSEntities();
         
        public T_User UserModel
        {
           
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }

        public object PrintCount { get; private set; }

        public class ExpenseCost
        {
            public string Car_Number { get; set; }
            public decimal Reun_Cost { get; set; }
        }

        /// <summary>
        /// 动态获取报销编码
        /// </summary>
        /// <returns></returns>
        public string GetExpenseCode()
        {
            string code = "BX-DE-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_Expense> lists = db.T_Expense.Where(a => a.Reun_Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
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
            return code;
        }

        public string GetNoticeCode()
        {
            string code = "WP-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_NoTicketExpense> lists = db.T_NoTicketExpense.Where(a => a.Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (lists.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = lists[0].Code.Substring(12);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            return code;
        }

        #region 借支批号

        /// <summary>
        /// 无默认值
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetBorrow()
        {
            List<T_Borrow> list = db.T_Borrow.Where(s => s.BorrowState == 1 && (s.BorrowSettementState == -1 || s.BorrowSettementState == 0) && s.BorrowName.Equals(UserModel.Name)).ToList();
            SelectList selectList = new SelectList(list, "BorrowCode", "BorrowCode");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }


        /// <summary>
        /// 有默认值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<SelectListItem> GetBorrow(SelectListItem item)
        {
            List<T_Borrow> list = db.T_Borrow.Where(s => s.BorrowState == 1 && s.BorrowSettementState == -1 && s.BorrowName.Equals(UserModel.Name)).ToList();
            SelectList selectList = new SelectList(list, "BorrowCode", "BorrowCode");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(item);
            selecli.AddRange(selectList);
            return selecli;
        }

        #endregion

        /// <summary>
        /// 凭证费用
        /// </summary>
        public class PZCost
        {
            public string PZ_Department { get; set; }
            public decimal PZ_Money { get; set; }
        }


        public partial class ExpenseProduct
        {
            public int ID { get; set; }
            public int ReunId { get; set; }
            public string StoreName { get; set; }
            public string Abstract { get; set; }
            public int Num { get; set; }
            public decimal Price { get; set; }
            public string Type { get; set; }
            public string PostUser { get; set; }
            public DateTime CrateDate { get; set; }
            public decimal Cost { get; set; }
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


        public class PzProduct
        {
            public int ID { get; set; }
            public int Num { get; set; }
            public double Price { get; set; }
            public string PZ_Department { get; set; }
            public string PZ_Subject { get; set; }
            public string Abstract { get; set; }
            public int PZ_Direction { get; set; }
            public string PZ_Department1 { get; set; }
            public string PZ_Subject1 { get; set; }
            public int PZ_Direction1 { get; set; }
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
        /// 绑定账号
        /// </summary>
        /// <param name="type"></param>
        public JsonResult GetExpenseAcount(int type)
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
        /// 绑定下级审核人
        /// </summary>
        /// <param name="step"></param>
        private void BindApproveName(int step = 0)
        {
            var approveusers = db.T_ExpenseApproveConfig.FirstOrDefault(a => a.Step == 4);


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



        #endregion

        #region 视图

        [Description("申请报销")]
        public ActionResult ViewExpenceAdd()
        {
			ViewData["Company"] = Com.DirectoryList("公司");
			//绑定部门
			ViewData["ReportDepartment"] = Com.DepartMent();
            string code = "BX-DE-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_Expense> lists = db.T_Expense.Where(a => a.Reun_Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
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
            T_Expense model = new T_Expense();
            model.Reun_Code = code;
            model.PostUser = UserModel.Name;
            ViewData["BorrowCode"] = GetBorrow();
            model.Department = UserModel.DepartmentId;
            return View(model);
        }

         [Description("罗瑶查询老板已审核")]
        public ActionResult ViewExpenseListCC()
        {
       
            return View();
        }
        
        [Description("编辑报销")]
        public ActionResult ViewExpenseEdit(int id)
        {
            var model = db.T_Expense.Find(id);
            if (model == null)
                return HttpNotFound();
            BindApproveName(model.Step);
            if (!string.IsNullOrWhiteSpace(model.Shop))
                model.Shop = db.T_ShopFromGY.FirstOrDefault(s => s.name.Equals(model.Shop)).number;
            model.Department = db.T_Department.FirstOrDefault(s => s.Name.Equals(model.Department)).ID.ToString();
            SelectListItem item = new SelectListItem() { Text = model.MatchBorrowNumber, Value = model.MatchBorrowNumber, Selected = true };
            ViewData["ReportDepartment"] = Com.DepartMent(model.Department);
            ViewData["BorrowCode"] = GetBorrow(item);
			ViewData["CompanyList"] = Com.DirectoryList("公司");
			return View(model);
        }

        [Description("我的报销")]
        public ActionResult ViewExpenseListForMy()
        {
            ViewData["user"] = UserModel.Nickname;
            return View();
        }

        [Description("报销列表")]
        public ActionResult ViewExpenseListManage()
        {
            return View();
        }
        [Description("报销发票勾兑")]
        public ActionResult ViewExpenceBlending()
        {
            return View();
        }

        [Description("报销未审核")]
        public ActionResult ViewExpenseListNotcheck()
        {
            return View();
        }

        [Description("报销已审核")]
        public ActionResult ViewExpenseListChecked()
        {
            return View();
        }

        [Description("报销数据统计")]
        public ActionResult ViewExpenseCountList()
        {
            return View();
        }

        [Description("报销打印")]
        public ActionResult ViewExpensePrint(int id, int page = 1)
        {
			ViewData["CompanyList"] = Com.DirectoryList("公司");
			ViewData["Bid"] = id;
            T_Expense model = db.T_Expense.Find(id);
            ViewData["jine"] = model.Reun_Cost;
            ViewData["code"] = model.Reun_Code;
            ViewData["PrintCount"] =model.PrintCount;
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
            IQueryable<T_ExpenseProduct> list = db.T_ExpenseProduct.Where(a => a.ReunId == id);
            int totalRows = list.Count();
            int P = 0;
			P = (totalRows ) / 35;
			if ((totalRows ) % 35 != 0)
			{
				P = P + 1;

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

        [Description("报销明细")]
        public ActionResult ViewExpenseProductDetail(int expenseId)
        {
            if (expenseId == 0)
                return HttpNotFound();
            ViewData["expenseId"] = expenseId;
            var history = db.T_ExpenseApprove.Where(a => a.Reunbursement_id == expenseId);
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

        [Description("审核")]
        public ActionResult ViewExpenseApprove(int id)
        {

            if (id == 0)
                return HttpNotFound();
            var history = db.T_ExpenseApprove.Where(a => a.Reunbursement_id == id);
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
            //获取审核表中的 审核记录ID
            T_ExpenseApprove approve = db.T_ExpenseApprove.FirstOrDefault(a => !a.ApproveDate.HasValue && a.Reunbursement_id == id);
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

        public ActionResult ViewExpensePzApprove(int id)
        {
            if (id == 0)
                return HttpNotFound();
            var model = db.T_Expense.Find(id);
            ViewData["expenseId"] = id;
            return View(model);
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
        public ActionResult ViewExpenseCostType(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("财务已审核")]
        public ActionResult ViewExpenseFinanceChecked()
        {
            ViewData["userName"] = UserModel.Nickname;
            return View();
        }

        [Description("报销凭证列表")]
        public ActionResult ViewExpensePzList()
        {
            ViewData["subject"] = Com.subject();
            ViewData["depart"] = Com.depart();
            return View();
        }

        [Description("报销凭证详情")]
        public ActionResult ViewExpensePzDetailList(int expenseId)
        {
            ViewData["expenseid"] = expenseId;
            return View();
        }

        [Description("报销凭证部门")]
        public ActionResult ViewPzDepart(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("报销凭证部门1")]
        public ActionResult ViewPzDepart1(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("报销凭证科目")]
        public ActionResult ViewPzSubJect(int index)
        {
            ViewData["index"] = index;
            return View();
        }

        [Description("报销凭证科目1")]
        public ActionResult ViewPzSubJect1(int index)
        {
            ViewData["index"] = index;
            return View();
        }
         [Description("无票报销新增")]
        public ActionResult NoTicketExpenseAdd()
        {
            ViewData["viewUser"] = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["Code"] = GetNoticeCode();
            return View();
        }
         [Description("无票报销列表")]
         public ActionResult NoTicketExpenseList()
         {
             ViewData["viewUser"] = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             return View();
         }
         [Description("无票报销列表")]
         public ActionResult NoTicketExpenseDetail(int ID)
         {
             T_NoTicketExpense mod = db.T_NoTicketExpense.Find(ID);
             if (mod == null) 
             {
                 return HttpNotFound();
             }
             return View(mod);
         }
        [Description("无票报销审核")]
         public ActionResult NoTicketExpenseCheck(int ID)
         {
             T_NoTicketExpense mod = db.T_NoTicketExpense.Find(ID);
             if (mod == null) 
             {
                 return HttpNotFound();
             }
             return View(mod);
         }
         [Description("无票报销审核账号管理")]
         public ActionResult NoTicketExpenseAccount()
         {
            
             return View();
          }
         [Description("无票报销审核账号编辑")]
         public ActionResult NoTicketExpenseAccountEdit(int ID)
         {
             ViewData["ID"] = ID;
             T_NoTicketExpenseAccount mod = db.T_NoTicketExpenseAccount.Find(ID);
             if (mod == null)
             {
                 return HttpNotFound();
             }
             ViewData["Type"] = mod.Type;
             return View(mod);
          }
        
        #endregion

        #region Post提交

        public class jinequery
        {
            public string Shopname { get; set; }
            public decimal Cost { get; set; }
        }

        public class tuihuojinequery
        {
            public string Shopname { get; set; }
            public decimal jien { get; set; }
        }

        /// <summary>
        /// 生成返现或退款数据
        /// </summary>
        /// <param name="statedate"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddExpenseByCashBackOrRetreatSave(string statedate, int type)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {

                    DateTime date = DateTime.Parse(statedate);
                    if (type == 1)//返现
                    {
                        string store = date.Year + "年" + date.Month + "月自动生成返现";
                        List<T_Expense> list = db.T_Expense.Where(s => s.PostUser.Equals(UserModel.Name) && s.Shop.Equals(store) && s.IsDelete == 0).ToList();
                        if (list.Count > 0)
                            return Json(new { State = "Faile", Message = "该返现年月份数据已经存在，如需重新生成请先删除原先数据" });
                        //后台申请
                        string start = "";
                        string end = "";
                        ReturnDateFormat(date.Month, out start, out  end);
                        end += " 23:59:59";
                        if (UserModel.Nickname.Equals("向日葵"))
                        {


                            #region 后台申请

                            //List<jinequery> zongjine = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='后台申请' and Reason!='村淘返现'").ToList();
                            //decimal jinezi = 0;

                            //if (zongjine.Count > 0)
                            //    jinezi = zongjine[0].Cost;
                            //T_Expense expense = new T_Expense
                            //{
                            //    Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                            //    Reun_Reason = date.Year + "年" + date.Month + "月份电商返现店铺后台出",
                            //    ExpenseNextApprove = 30,
                            //    Reun_Cost = jinezi,
                            //    PostUser = UserModel.Name,
                            //    CrateDate = DateTime.Now,
                            //    Reun_Code = GetExpenseCode(),
                            //    AccountType = "银行卡",
                            //    Reun_Bank = "后台申请",
                            //    Reun_Name = "后台申请",
                            //    Car_Number = "后台申请",
                            //    Shop = store,
                            //    Status = -1,
                            //    Step = 0,
                            //    IsExpenseMatch = false,
                            //    IsExpenseEnclosure = 0,
                            //    IsDelete = 0,
                            //    Pz_BXStatus = 0,
                            //    IsBlending = 0,
                            //    PrintCount = 0,
                            //};
                            //db.T_Expense.Add(expense);
                            //db.SaveChanges();
                            ////审核记录
                            //T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                            //{
                            //    ApproveName = Com.GetNickName(expense.ExpenseNextApprove),
                            //    ApproveStatus = -1,
                            //    Reunbursement_id = expense.ID
                            //};
                            //db.T_ExpenseApprove.Add(Approvemodel);
                            //db.SaveChanges();
                            //List<jinequery> jineBYshopname = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='后台申请' and Reason!='村淘返现'  group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            //foreach (var item in jineBYshopname)
                            //{
                            //    T_ExpenseProduct product = new T_ExpenseProduct
                            //    {
                            //        ReunId = expense.ID,
                            //        Abstract = date.Year + "年" + date.Month + "月份后台促销推广费",
                            //        Num = 1,
                            //        Price = item.Cost,
                            //        Type = "返利费用",
                            //        StoreName = item.Shopname
                            //    };
                            //    db.T_ExpenseProduct.Add(product);
                            //}
                            //db.SaveChanges();

                            #endregion

                            #region haohushi_tm@163.com


                            List<jinequery> zonghaohushijine = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='haohushi_tm@163.com' and Reason!='村淘返现'").ToList();
                            decimal jinezhifubzi = 0;

                            if (zonghaohushijine.Count > 0)
                                jinezhifubzi = zonghaohushijine[0].Cost;
                            T_Expense expensezhifubbao = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商返现财务支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezhifubzi,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "湖南好护士医疗器械连锁经营有限公司",
                                Car_Number = "haohushi_tm@163.com",
                                Status = -1,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                Shop = store,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensezhifubbao);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodelzhifubao = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensezhifubbao.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensezhifubbao.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodelzhifubao);
                            db.SaveChanges();
                            List<jinequery> jineBYshopnamezhifubao = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='haohushi_tm@163.com' and Reason!='村淘返现'  group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            foreach (var item in jineBYshopnamezhifubao)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensezhifubbao.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份支付宝tm促销推广费",
                                    Num = 1,
                                    Price = item.Cost,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                            #region haohushi158@163.com

                            List<jinequery> hhs158List = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='haohushi158@163.com'   and Reason!='村淘返现'").ToList();
                            decimal hhsjine = 0;

                            if (hhs158List.Count > 0)
                                hhsjine = hhs158List[0].Cost;
                            T_Expense expensehhs = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商返现财务支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = hhsjine,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "湖南好护士医疗器械连锁经营有限公司",
                                Car_Number = "haohushi158@163.com",
                                Status = -1,
                                Shop = store,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensehhs);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodelhhs = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensehhs.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensehhs.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodelhhs);
                            db.SaveChanges();
                            List<jinequery> jineBYshopnamehhs = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='haohushi158@163.com' and Reason!='村淘返现'  group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            foreach (var item in jineBYshopnamehhs)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensehhs.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份支付宝158促销推广费",
                                    Num = 1,
                                    Price = item.Cost,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                        }
                        else
                        {
                            #region 店铺后台药健康

                            //List<jinequery> zongjine = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='后台申请' and Reason!='村淘返现' and ShopName='Tmall药健康大药房旗舰店'").ToList();
                            //decimal jinezi = 0;

                            //if (zongjine.Count > 0)
                            //    jinezi = zongjine[0].Cost;
                            //T_Expense expense = new T_Expense
                            //{
                            //    Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                            //    Reun_Reason = date.Year + "年" + date.Month + "月份药健康返现店铺后台出",
                            //    ExpenseNextApprove = 30,
                            //    Reun_Cost = jinezi,
                            //    PostUser = UserModel.Name,
                            //    CrateDate = DateTime.Now,
                            //    Reun_Code = GetExpenseCode(),
                            //    AccountType = "银行卡",
                            //    Reun_Bank = "后台申请",
                            //    Reun_Name = "后台申请",
                            //    Car_Number = "后台申请",
                            //    Status = -1,
                            //    Step = 0,
                            //    Shop = store,
                            //    IsExpenseMatch = false,
                            //    IsExpenseEnclosure = 0,
                            //    IsDelete = 0,
                            //    Pz_BXStatus = 0,
                            //    IsBlending = 0,
                            //    PrintCount = 0,
                            //};
                            //db.T_Expense.Add(expense);
                            //db.SaveChanges();
                            ////审核记录
                            //T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                            //{
                            //    ApproveName = Com.GetNickName(expense.ExpenseNextApprove),
                            //    ApproveStatus = -1,
                            //    Reunbursement_id = expense.ID
                            //};
                            //db.T_ExpenseApprove.Add(Approvemodel);
                            //db.SaveChanges();
                            //List<jinequery> jineBYshopname = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='后台申请' and Reason!='村淘返现' and ShopName='Tmall药健康大药房旗舰店'  group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            //foreach (var item in jineBYshopname)
                            //{
                            //    T_ExpenseProduct product = new T_ExpenseProduct
                            //    {
                            //        ReunId = expense.ID,
                            //        Abstract = date.Year + "年" + date.Month + "月份促销推广费",
                            //        Num = 1,
                            //        Price = item.Cost,
                            //        Type = "返利费用",
                            //        StoreName = item.Shopname
                            //    };
                            //    db.T_ExpenseProduct.Add(product);
                            //}
                            //db.SaveChanges();

                            #endregion

                            #region 支付宝

                            List<jinequery> zongjinezhifubao = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='yjkdyf888@163.com' and Reason!='村淘返现'  and ShopName='Tmall药健康大药房旗舰店'").ToList();
                            decimal jinezizhifubao = 0;

                            if (zongjinezhifubao.Count > 0)
                                jinezizhifubao = zongjinezhifubao[0].Cost;
                            T_Expense expensezhifubao = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商返现财务支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezizhifubao,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "桐庐好邻居大药房连锁有限公司",
                                Car_Number = "yjkdyf888@163.com",
                                Status = -1,
                                Shop = store,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensezhifubao);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodelzhifubao = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensezhifubao.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensezhifubao.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodelzhifubao);
                            db.SaveChanges();
                            List<jinequery> jineBYshopnamezhifubao = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='" + UserModel.Nickname + "') and BackFrom='yjkdyf888@163.com' and Reason!='村淘返现' and ShopName='Tmall药健康大药房旗舰店'  group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            foreach (var item in jineBYshopnamezhifubao)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensezhifubao.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份促销推广费",
                                    Num = 1,
                                    Price = item.Cost,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                            #region 村淘

                            List<jinequery> zongjinecuntao = db.Database.SqlQuery<jinequery>("select isnull(SUM(BackMoney),0) as Cost from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and ApproveName='曹朝霞') and BackFrom='haohushi8888@163.com' and Reason='村淘返现' ").ToList();
                            decimal jinezizhicuntao = 0;

                            if (zongjinecuntao.Count > 0)
                                jinezizhicuntao = zongjinecuntao[0].Cost;
                            T_Expense expensecuntou = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商返现支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezizhicuntao,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "湖南好护士医疗器械连锁经营有限公司",
                                Car_Number = "haohushi8888@163.com",
                                Status = -1,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                Shop = store,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensecuntou);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodelcuntou = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensecuntou.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensecuntou.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodelcuntou);
                            db.SaveChanges();
                            List<jinequery> jineBYshopnamecuntou = db.Database.SqlQuery<jinequery>("select ShopName,isnull(SUM(BackMoney),0) as Cost   from T_CashBack where ID in (select  Oid from  T_CashBackApprove where ApproveTime>='" + start + "' and ApproveTime<='" + end + "'  and Status='1' and  ApproveName='曹朝霞') and BackFrom='haohushi8888@163.com' and Reason='村淘返现'   group by ShopName  order by isnull(SUM(BackMoney),0) desc").ToList();

                            foreach (var item in jineBYshopnamecuntou)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensecuntou.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份促销推广费",
                                    Num = 1,
                                    Price = item.Cost,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                        }
                    }
                    else//退款
                    {
                        string store = date.Year + "年" + date.Month + "月自动生成退款";
                        List<T_Expense> list = db.T_Expense.Where(s => s.PostUser.Equals(UserModel.Name) && s.Shop.Equals(store) && s.IsDelete == 0).ToList();
                        if (list.Count > 0)
                            return Json(new { State = "Faile", Message = "该退款年月份数据已经存在，如需重新生成请先删除原先数据" });
                        string start = "";
                        string end = "";
                        ReturnDateFormat(date.Month, out start, out  end);
                        end += " 23:59:59";
                        if (UserModel.Nickname.Equals("向日葵"))
                        {

                            #region 店铺后台

                            //List<tuihuojinequery> houtai = db.Database.SqlQuery<tuihuojinequery>("select  isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where  Retreat_dianpName!='Tmall药健康大药房旗舰店'  and Retreat_PaymentAccounts='店铺出' and ID in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "')  and Status=1)  ").ToList();
                            //decimal houtaijine = 0;

                            //if (houtai.Count > 0)
                            //    houtaijine = houtai[0].jien;
                            //T_Expense expensehoutai = new T_Expense
                            //{
                            //    Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                            //    Reun_Reason = date.Year + "年" + date.Month + "月份电商退款财务店铺出",
                            //    ExpenseNextApprove = 30,
                            //    Reun_Cost = houtaijine,
                            //    PostUser = UserModel.Name,
                            //    CrateDate = DateTime.Now,
                            //    Reun_Code = GetExpenseCode(),
                            //    AccountType = "银行卡",
                            //    Reun_Bank = "店铺出",
                            //    Reun_Name = "店铺出",
                            //    Car_Number = "店铺出",
                            //    Status = -1,
                            //    Shop = store,
                            //    Step = 0,
                            //    IsExpenseMatch = false,
                            //    IsExpenseEnclosure = 0,
                            //    IsDelete = 0,
                            //    Pz_BXStatus = 0,
                            //    IsBlending = 0,
                            //    PrintCount = 0,
                            //};
                            //db.T_Expense.Add(expensehoutai);
                            //db.SaveChanges();
                            ////审核记录
                            //T_ExpenseApprove Approvemodelhoutai = new T_ExpenseApprove
                            //{
                            //    ApproveName = Com.GetNickName(expensehoutai.ExpenseNextApprove),
                            //    ApproveStatus = -1,
                            //    Reunbursement_id = expensehoutai.ID
                            //};
                            //db.T_ExpenseApprove.Add(Approvemodelhoutai);
                            //db.SaveChanges();
                            //List<tuihuojinequery> jineBYshopnamehoutai = db.Database.SqlQuery<tuihuojinequery>("select   Retreat_dianpName as ShopName,isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where    Retreat_dianpName!='Tmall药健康大药房旗舰店'  and Retreat_PaymentAccounts='店铺出'  and id in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "' and Status=1) ) group by Retreat_dianpName order by isnull(SUM(Retreat_Actualjine),0) desc ").ToList();

                            //foreach (var item in jineBYshopnamehoutai)
                            //{
                            //    T_ExpenseProduct product = new T_ExpenseProduct
                            //    {
                            //        ReunId = expensehoutai.ID,
                            //        Abstract = date.Year + "年" + date.Month + "月份后台退款费",
                            //        Num = 1,
                            //        Price = item.jien,
                            //        Type = "返利费用",
                            //        StoreName = item.Shopname
                            //    };
                            //    db.T_ExpenseProduct.Add(product);
                            //}
                            //db.SaveChanges();

                            #endregion

                            #region tm

                            List<tuihuojinequery> zhifubaozongjine = db.Database.SqlQuery<tuihuojinequery>("select  isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where   Retreat_PaymentAccounts='haohushi_tm@163.com' and ID in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "')  and Status=1)  ").ToList();//Retreat_dianpName!='Tmall药健康大药房旗舰店'  and
                            decimal jinezizhifubao = 0;

                            if (zhifubaozongjine.Count > 0)
                                jinezizhifubao = zhifubaozongjine[0].jien;
                            T_Expense expensezhifubao = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商退款财务tm支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezizhifubao,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "湖南好护士医疗器械连锁经营有限公司",
                                Car_Number = "haohushi_tm@163.com",
                                Status = -1,
                                Shop = store,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensezhifubao);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensezhifubao.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensezhifubao.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodel);
                            db.SaveChanges();
                            List<tuihuojinequery> jineBYshopname = db.Database.SqlQuery<tuihuojinequery>("select   Retreat_dianpName as ShopName,isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where     Retreat_PaymentAccounts='haohushi_tm@163.com'  and id in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "' and Status=1) ) group by Retreat_dianpName order by isnull(SUM(Retreat_Actualjine),0) desc ").ToList();//Retreat_dianpName!='Tmall药健康大药房旗舰店'  and

                            foreach (var item in jineBYshopname)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensezhifubao.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份支付宝tm退款费",
                                    Num = 1,
                                    Price = item.jien,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                            #region 158

                            List<tuihuojinequery> zhifubaozongjine158 = db.Database.SqlQuery<tuihuojinequery>("select  isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where   Retreat_PaymentAccounts='haohushi158@163.com' and ID in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "')  and Status=1)  ").ToList();//Retreat_dianpName!='Tmall药健康大药房旗舰店'  and
                            decimal jinezizhifubao158 = 0;

                            if (zhifubaozongjine158.Count > 0)
                                jinezizhifubao158 = zhifubaozongjine158[0].jien;
                            T_Expense expensezhifubao158 = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份电商退款财务支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezizhifubao158,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "湖南好护士医疗器械连锁经营有限公司",
                                Car_Number = "haohushi158@163.com",
                                Status = -1,
                                Step = 0,
                                Shop = store,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensezhifubao158);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodel158 = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensezhifubao158.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensezhifubao158.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodel158);
                            db.SaveChanges();
                            List<tuihuojinequery> jineBYshopname158 = db.Database.SqlQuery<tuihuojinequery>("select   Retreat_dianpName as ShopName,isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where     Retreat_PaymentAccounts='haohushi158@163.com'  and id in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and ApproveName='" + UserModel.Nickname + "' and Status=1 ) group by Retreat_dianpName order by isnull(SUM(Retreat_Actualjine),0) desc ").ToList();//Retreat_dianpName!='Tmall药健康大药房旗舰店'  and

                            foreach (var item in jineBYshopname158)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensezhifubao158.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份支付宝158退款费",
                                    Num = 1,
                                    Price = item.jien,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                        }
                        else
                        {

                            #region yjkdyf888@163.com

                            List<tuihuojinequery> zhifubaozongjine = db.Database.SqlQuery<tuihuojinequery>("select  isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where  Retreat_dianpName='Tmall药健康大药房旗舰店'  and Retreat_PaymentAccounts='yjkdyf888@163.com' and ID in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and (ApproveName='" + UserModel.Nickname + "')  and Status=1)  ").ToList();
                            decimal jinezizhifubao = 0;

                            if (zhifubaozongjine.Count > 0)
                                jinezizhifubao = zhifubaozongjine[0].jien;
                            T_Expense expensezhifubao = new T_Expense
                            {
                                Department = Com.GetDepartmentName(int.Parse(UserModel.DepartmentId)),
                                Reun_Reason = date.Year + "年" + date.Month + "月份药健康退款财务支付宝出",
                                ExpenseNextApprove = 30,
                                Reun_Cost = jinezizhifubao,
                                PostUser = UserModel.Name,
                                CrateDate = DateTime.Now,
                                Reun_Code = GetExpenseCode(),
                                AccountType = "银行卡",
                                Reun_Bank = "支付宝",
                                Reun_Name = "桐庐好邻居大药房连锁有限公司",
                                Car_Number = "yjkdyf888@163.com",
                                Status = -1,
                                Step = 0,
                                IsExpenseMatch = false,
                                IsExpenseEnclosure = 0,
                                IsDelete = 0,
                                Shop = store,
                                Pz_BXStatus = 0,
                                IsBlending = 0,
                                PrintCount = 0,
                            };
                            db.T_Expense.Add(expensezhifubao);
                            db.SaveChanges();
                            //审核记录
                            T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                            {
                                ApproveName = Com.GetNickName(expensezhifubao.ExpenseNextApprove),
                                ApproveStatus = -1,
                                Reunbursement_id = expensezhifubao.ID
                            };
                            db.T_ExpenseApprove.Add(Approvemodel);
                            db.SaveChanges();
                            List<tuihuojinequery> jineBYshopname = db.Database.SqlQuery<tuihuojinequery>("select   Retreat_dianpName as ShopName,isnull(SUM(Retreat_Actualjine),0) as jien from T_Retreat where    Retreat_dianpName='Tmall药健康大药房旗舰店'  and Retreat_PaymentAccounts='yjkdyf888@163.com'  and id in (select Distinct Oid from T_RetreatAppRove  where ApproveTime>='" + start + "'  and   ApproveTime<='" + end + "'  and ApproveName='" + UserModel.Nickname + "' and Status=1 ) group by Retreat_dianpName order by isnull(SUM(Retreat_Actualjine),0) desc ").ToList();

                            foreach (var item in jineBYshopname)
                            {
                                T_ExpenseProduct product = new T_ExpenseProduct
                                {
                                    ReunId = expensezhifubao.ID,
                                    Abstract = date.Year + "年" + date.Month + "月份退款费",
                                    Num = 1,
                                    Price = item.jien,
                                    Type = "返利费用",
                                    StoreName = item.Shopname
                                };
                                db.T_ExpenseProduct.Add(product);
                            }
                            db.SaveChanges();

                            #endregion

                        }
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

        public static void ReturnDateFormat(int month, out string firstDay, out string lastDay)
        {

            int year = DateTime.Now.Year + month / 12;
            if (month != 12)
            {
                month = month % 12;
            }
            switch (month)
            {
                case 1:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 2:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    if (DateTime.IsLeapYear(DateTime.Now.Year))
                        lastDay = DateTime.Now.ToString(year + "-0" + month + "-29");
                    else
                        lastDay = DateTime.Now.ToString(year + "-0" + month + "-28");
                    break;
                case 3:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString("yyyy-0" + month + "-31");
                    break;
                case 4:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 5:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 6:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 7:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 8:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 9:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 10:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-31");
                    break;
                case 11:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-30");
                    break;
                default:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-31");
                    break;
            }
        }

        /// <summary>
        /// 凭证保存
        /// </summary>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewPzAddSave(string jsonStr, int expenseId)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(jsonStr))
                        return Json(new { State = "Faile", Message = "数据有误" });

                    List<PzProduct> productList = Com.Deserialize<PzProduct>(jsonStr);
                    T_Expense model = db.T_Expense.Find(expenseId);
                    foreach (var item in productList)
                    {
                        T_ExpenseProduct product = db.T_ExpenseProduct.Find(item.ID);
                        product.PZ_Department = item.PZ_Department;
                        product.PZ_Subject = item.PZ_Subject;
                        product.PZ_Direction = item.PZ_Direction;
                        product.PZ_Department1 = item.PZ_Department1;
                        product.PZ_Subject1 = item.PZ_Subject1;
                        product.PZ_Direction1 = item.PZ_Direction1;
                        db.SaveChanges();
                    }
                    model.Pz_BXStatus = -1;
                    model.Pz_BxMemo = "";
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
                    T_Expense expense = db.T_Expense.Find(id);
                    expense.Pz_BXStatus = status;
                    expense.Pz_BxMemo = memo;
                    db.SaveChanges();
                    if (status == 1)
                    {
                        List<T_ExpenseProduct> productList = db.T_ExpenseProduct.Where(s => s.ReunId == id).ToList();
                        foreach (var item in productList)
                        {
                            T_PZ_BX bx = new T_PZ_BX
                            {
                                PZ_OrderNum = expense.Reun_Code,
                                PZ_Summary = item.Abstract,
                                PZ_Department = item.PZ_Department,
                                PZ_Direction = item.PZ_Direction,
                                PZ_Money = item.Price * item.Num,
                                PZ_Subject = item.PZ_Subject,
                                PZ_Department1 = item.PZ_Department1,
                                PZ_Subject1 = item.PZ_Subject1,
                                PZ_Direction1 = item.PZ_Direction1,
                                PZ_Time = DateTime.Now
                            };
                            db.T_PZ_BX.Add(bx);
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
        /// 获得凭证部门
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetDepart(Lib.GridPager pager, string name)
        {
            IQueryable<T_PZ_DePartMent> list = db.T_PZ_DePartMent.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                list = list.Where(s => s.Name.Contains(name));
            pager.totalRows = list.Count();
            List<T_PZ_DePartMent> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获得凭证科目
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetPzSubject(Lib.GridPager pager, string name)
        {
            IQueryable<T_AccountantProject> list = db.T_AccountantProject.Where(s => s.ID != 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                list = list.Where(s => s.Name.Contains(name));
            pager.totalRows = list.Count();
            List<T_AccountantProject> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
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
            IQueryable<T_Expense> list = db.T_Expense.Where(s => s.PostUser.Equals(UserModel.Name) && s.IsDelete == 0).AsQueryable();
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
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 抄送到罗瑶的报销查询
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="Code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult ViewExpenseCCList(Lib.GridPager pager, string code)
        {
            string sql = "select * from  T_Expense where Status=0 and ID in  (select Reunbursement_id from T_ExpenseApprove where ApproveName='三疯' and ApproveDate is  not null)";
            if (!string.IsNullOrWhiteSpace(code))
            {
                sql += " and PostUser='"+code+"'";
            }
            List<T_Expense> list = db.Database.SqlQuery<T_Expense>(sql).ToList();
       
            pager.totalRows = list.Count();
            List<ExpenseCost> footerList = new List<ExpenseCost>();
            ExpenseCost footer = new ExpenseCost();
            footer.Car_Number = "总计:";
            if (list.Count() > 0)
                footer.Reun_Cost = list.Sum(s => s.Reun_Cost);
            else
                footer.Reun_Cost = 0;
            footerList.Add(footer);
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 报销发票勾兑
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ContentResult ViewViewExpenceBlendingList(Lib.GridPager pager, string code, string startDate, string endDate,string txtjine,string txtName, int status = 1)
        {
            IQueryable<T_Expense> list = db.T_Expense.Where(s => s.IsDelete == 0 && s.Status == 1 && s.IsBlending == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Equals(code));

            if (!string.IsNullOrWhiteSpace(txtjine))
            {
                decimal jine =decimal.Parse(txtjine);
                list = list.Where(s => s.Reun_Cost == jine);
            }
            if (!string.IsNullOrWhiteSpace(txtName))
            {
                list = list.Where(s => s.PostUser == txtName);
            }

            if (!string.IsNullOrWhiteSpace(startDate) && !string.IsNullOrWhiteSpace(endDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                list = list.Where(s => s.CrateDate >= start && s.CrateDate <= end);
            }
            else if (!string.IsNullOrWhiteSpace(startDate))
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
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
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
            IQueryable<T_Expense> list = db.T_Expense.Where(s => s.IsDelete == 0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(code))
                list = list.Where(s => s.Reun_Code.Contains(code)||s.PostUser.Contains(code));
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
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 获取报销打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetExpensePrint(int id)
        {
            T_Expense model = db.T_Expense.Find(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取报销明细打印数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetExpenseProductPrint(int expenseId, int page)
        {
            IQueryable<T_ExpenseProduct> list = db.T_ExpenseProduct.Where(a => a.ReunId == expenseId);
            int totalRows = list.Count();
		//	List<T_ExpenseProduct> modelList = list.OrderBy(c => c.ID).ToList();
		   List <T_ExpenseProduct> modelList = list.OrderBy(c => c.ID).Skip((page - 1) * 35).Take(35).ToList();
            return Json(modelList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 报销明细数据
        /// </summary>
        /// <param name="pager"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewExpenseProductList(int expenseId)
        {
            IQueryable<T_ExpenseProduct> list = db.T_ExpenseProduct.Where(s => s.ReunId == expenseId).AsQueryable();
            List<T_ExpenseProduct> querData = list.OrderBy(s => s.ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
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
                T_Expense model = db.T_Expense.Find(id);
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
        /// 报销保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewExpenseAddSave(T_Expense model, string jsonStr, string jsonStr1, string picUrls)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return Json(new { State = "Faile", Message = "请添加报销明细" });
            }
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ExpenseProduct> details = Com.Deserialize<T_ExpenseProduct>(jsonStr);
                    string expensecode = "BX-DE-";
                    string expensedate = DateTime.Now.ToString("yyyyMMdd");
                    //查找当前已有的编号
                    List<T_Expense> lists = db.T_Expense.Where(a => a.Reun_Code.Contains(expensedate)).OrderByDescending(c => c.ID).ToList();
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

                    db.T_Expense.Add(model);
                    db.SaveChanges();
                    //审核记录
                    T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                    {
                        ApproveName = Com.GetNickName(model.ExpenseNextApprove),
                        ApproveStatus = -1,
                        Reunbursement_id = model.ID
                    };
                    db.T_ExpenseApprove.Add(Approvemodel);
                    db.SaveChanges();
                    //添加详情
                    foreach (var item in details)
                    {
                        item.ReunId = model.ID;
                        db.T_ExpenseProduct.Add(item);
                    }
                    db.SaveChanges();
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
                        BillCompany = model.Reun_Bank,
                        BillFromCode = model.Reun_Code,
                        BillMoney = Convert.ToDouble(model.Reun_Cost),
                        BillType = "报销申请",
                        CreateTime = DateTime.Now,
                        CreatUser = model.PostUser,
                        PayMoney = Convert.ToDouble("-" + model.Reun_Cost)
                    };
                    db.T_AP.Add(ap);
                    db.SaveChanges();
                    //附件保存
                    if (!string.IsNullOrWhiteSpace(picUrls))
                    {
                        string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                       // List<T_ExpenseEnclosure> Enclosure = Com.Deserialize<T_ExpenseEnclosure>(picUrls);
                        foreach (string item in picArr)
                        {
                            T_ExpenseEnclosure Enclosure = new T_ExpenseEnclosure();
                            Enclosure.Scdate = DateTime.Now;
                            Enclosure.ExpenseId = model.ID;
                            Enclosure.Url = item;
                            Enclosure.Path = item;
                            string [] ss=item.Split('/');
                            Enclosure.ScName = ss[ss.Length-1];
                            Enclosure.Size = "0";

                            db.T_ExpenseEnclosure.Add(Enclosure);
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
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "报销").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpenseApprove where Reunbursement_id in (select ID from T_Expense where IsDelete=0  and (Status=-1 or status=0)) and  ApproveStatus=-1 and ApproveDate is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "报销";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_Expense where Status='2' and IsDelete=0 GROUP BY PostUser  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "报销";
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
        //虚拟删除付款单记录 
        [HttpPost]
        [Description("报销发票勾兑")]
        public JsonResult ExpenceBlendingFinance(int del)
        {
            T_Expense model = db.T_Expense.Find(del);
            model.IsBlending = 1;
            db.Entry<T_Expense>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        ///报销编辑保存 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewExpenseEditSave(T_Expense model, string jsonStr, string jsonStr1)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return Json(new { State = "Faile", Message = "请添加报销明细" });
            }
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    List<T_ExpenseProduct> details = Com.Deserialize<T_ExpenseProduct>(jsonStr);
                    T_Expense expense = db.T_Expense.Find(model.ID);
                    int departId = int.Parse(model.Department);
                    if (model.Status != -1)
                    {
                        T_ExpenseApprove Approvemodel = new T_ExpenseApprove
                        {
                            ApproveName = Com.GetNickName(model.ExpenseNextApprove),
                            ApproveStatus = -1,
                            Reunbursement_id = model.ID
                        };
                        db.T_ExpenseApprove.Add(Approvemodel);
                        db.SaveChanges();
                    }
                    if (model.Status == -1)
                    {
                        T_ExpenseApprove Approve = db.T_ExpenseApprove.SingleOrDefault(s => s.Reunbursement_id == model.ID && !s.ApproveDate.HasValue);
                        Approve.ApproveName = Com.GetNickName(model.ExpenseNextApprove);
                        db.SaveChanges();
                    }

                    expense.Car_Number = model.Car_Number;
                    expense.Reun_Bank = model.Reun_Bank;
                    expense.Reun_Name = model.Reun_Name;
                    if (!string.IsNullOrWhiteSpace(jsonStr1))
                        expense.IsExpenseEnclosure = 1;
                    else
                        expense.IsExpenseEnclosure = 0;
                    expense.Department = db.T_Department.Find(departId).Name;
                    expense.Status = -1;
                    expense.IsBlending = model.IsBlending;
                    expense.Memo = model.Memo;
                    expense.AccountType = model.AccountType;
                    expense.Reun_Reason = model.Reun_Reason;
                    expense.MatchBorrowNumber = model.MatchBorrowNumber;
                    expense.Reun_Cost = details.Sum(s => s.Num * s.Price);
					expense.Company = model.Company;
                    if (!string.IsNullOrWhiteSpace(model.Shop))
                        expense.Shop = db.T_ShopFromGY.FirstOrDefault(s => s.number.Equals(model.Shop)).name;
                    db.SaveChanges();

                    //foreach (var item in db.T_ExpenseProduct.Where(s => s.ReunId == model.ID))
                    //{
                    //    db.T_ExpenseProduct.Remove(item);
                    //}
                    //db.SaveChanges();
                    //删除
                    List<T_ExpenseProduct> productList = db.T_ExpenseProduct.Where(s => s.ReunId == model.ID).ToList();
                    List<int> lists = productList.Select(s => s.ID).Except(details.Select(s => s.ID)).ToList();
                    foreach (var item in lists)
                    {
                        if (item != 0)
                        {
                            T_ExpenseProduct product = db.T_ExpenseProduct.Find(item);
                            db.T_ExpenseProduct.Remove(product);
                            db.SaveChanges();
                        }
                    }
                    foreach (var item in details)
                    {
                        if (item.ID == 0)
                        {
                            T_ExpenseProduct pro = new T_ExpenseProduct
                            {
                                ReunId = model.ID,
                                Abstract = item.Abstract,
                                Num = item.Num,
                                Price = item.Price,
                                Type = item.Type,
                                StoreName = item.StoreName
                            };
                            db.T_ExpenseProduct.Add(pro);
                        }
                        else
                        {
                            T_ExpenseProduct product = db.T_ExpenseProduct.Find(item.ID);
                            product.Abstract = item.Abstract;
                            product.Num = item.Num;
                            product.Price = item.Price;
                            product.Type = item.Type;
                            product.StoreName = item.StoreName;
                        }
                        db.SaveChanges();
                    }

                    //附件保存 先删除原有的附件
                    List<T_ExpenseEnclosure> delMod = db.T_ExpenseEnclosure.Where(a => a.ExpenseId == model.ID).ToList();
                    foreach (var item in delMod)
                    {
                        db.T_ExpenseEnclosure.Remove(item);
                    }
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(jsonStr1))
                    {
                        List<T_ExpenseEnclosure> enclosure = Com.Deserialize<T_ExpenseEnclosure>(jsonStr1);
                        foreach (var item in enclosure)
                        {
                            item.Scdate = DateTime.Now;
                            item.ExpenseId = model.ID;
                            db.T_ExpenseEnclosure.Add(item);
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
        /// 作废
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ViewExpenseInvalid(int id)
        {
            try
            {

                T_Expense model = db.T_Expense.Find(id);
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
               // ModularByZP();
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

                T_Expense model = db.T_Expense.Find(id);
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
                //ModularByZP();

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
        public ContentResult ViewExpenseNocheckList(Lib.GridPager pager, string user)
        {
            List<T_Expense> expenseList = new List<T_Expense>();
            IQueryable<T_ExpenseApprove> list = db.T_ExpenseApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && !s.ApproveDate.HasValue);
            List<int> itemIds = new List<int>();
            foreach (var item in list.Select(s => new { itemId = s.Reunbursement_id }).GroupBy(s => s.itemId))
            {
                itemIds.Add(item.Key);
            }

            foreach (var item in itemIds)
            {
                T_Expense model = db.T_Expense.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
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
            List<T_Expense> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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
            List<T_Expense> expenseList = new List<T_Expense>();
            IQueryable<T_ExpenseApprove> list = db.T_ExpenseApprove.Where(s => s.ApproveName.Equals(UserModel.Nickname) && s.ApproveDate.HasValue);
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
                T_Expense model = db.T_Expense.SingleOrDefault(s => s.ID == item && s.IsDelete == 0 && s.Status != 3);
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
            List<T_Expense> querData = expenseList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 财务已审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ContentResult GetExpenseFinanceCheckedList(Lib.GridPager pager, string user)
        {
            IQueryable<T_Expense> list = db.T_Expense.Where(s => s.Status == 1 && s.Pz_BXStatus != 1);
            if (!string.IsNullOrWhiteSpace(user))
                list = list.Where(s => s.PostUser.Equals(user));
            pager.totalRows = list.Count();
            List<T_Expense> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
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
                    T_ExpenseApprove approve = db.T_ExpenseApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1 && (a.ApproveName == curName || a.ApproveName == Nickname));
                    if (approve==null)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" }, JsonRequestBehavior.AllowGet);
                    }
                      
                    approve.ApproveStatus = status;
                    approve.ApproveDate = DateTime.Now;
                    approve.Remark = memo;
                    T_Expense model = db.T_Expense.Find(approve.Reunbursement_id);
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
                                BillCompany = model.Reun_Bank,
                                BillFromCode = model.Reun_Code,
                                BillMoney = Convert.ToDouble(model.Reun_Cost),
                                BillType = "报销申请",
                                CreateTime = DateTime.Now,
                                CreatUser = UserModel.Name,
                                PayMoney = Convert.ToDouble("-" + model.Reun_Cost)
                            };
                            db.T_PP.Add(pp);
                            db.SaveChanges();
                        }
                        else
                        {
                            if (status != 2)
                            {
                                T_ExpenseApproveConfig ModelConFig=db.T_ExpenseApproveConfig.SingleOrDefault(a=>a.Step==Step);
                               

                                T_ExpenseApprove newApprove = new T_ExpenseApprove();
                                newApprove.ApproveStatus = -1;
                                if (Step == 4)//步奏是4就是三疯进来审核
                                {
                                    newApprove.ApproveName =model.Cashier;
                                    nextapprove = model.Cashier;
                                }
                                else
                                {
                                    newApprove.ApproveName = ModelConFig.ApproveUser;
                                    nextapprove = ModelConFig.ApproveUser;
                                }

                                newApprove.ApproveDate = null;
                                newApprove.Reunbursement_id = approve.Reunbursement_id;
                                db.T_ExpenseApprove.Add(newApprove);

                                T_User u = db.T_User.FirstOrDefault(a => a.Nickname.Equals(nextapprove));
                                model.ExpenseNextApprove = u.ID;
                                model.Status = 0;
                            }
                        }
                        model.Step = Step;
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

        /// <summary>
        /// 报销数据统计
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="store"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="statedate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetViewExpenseCountList(Lib.GridPager pager, string store, string type, string name, string startDate, string endDate)
        {

            IQueryable<ExpenseProduct> queryData = null;
            string sql = "select ReunId,Abstract,Num,Price,[Type],StoreName,CrateDate,(ISNULL(Num,0)*ISNULL(Price,0)) as Cost,PostUser from T_ExpenseProduct p inner join T_Expense t on t.ID=p.ReunId where t.IsDelete=0 ";
            if (!string.IsNullOrWhiteSpace(store))
            {
                sql += "  and  StoreName='" + store + "' ";

            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                sql += "  and  Type='" + type + "' ";
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += "  and   PostUser like '" + name + "' or   Abstract like '%" + name + "%'";
            }
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime sdate = DateTime.Parse(startDate + " 00:00:00");
                sql += " and  CrateDate>='" + sdate + "'";
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime Edate = DateTime.Parse(endDate + " 23:59:59");
                sql += " and  CrateDate<='" + Edate + "'";
            }
            queryData = db.Database.SqlQuery<ExpenseProduct>(sql).AsQueryable();
            //构造新的list
            List<ExpenseProduct> list = new List<ExpenseProduct>();
            //构造一个单独的list用于查询总数。此总数不分页。
            List<ExpenseProduct> zongjinelist = db.Database.SqlQuery<ExpenseProduct>(sql).ToList();
            decimal sum = 0;
            foreach (var item in zongjinelist)
            {
                if (item.Num > 0)
                {
                    if (item.Price != 0 && item.Num != 0)
                    {
                        sum += (decimal)(item.Price * item.Num);
                    }
                }
            }

            //总数据量
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                //分页
                queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);

                foreach (var item in queryData)
                {
                    ExpenseProduct i = new ExpenseProduct();
                    i.ID = item.ID;
                    i.Abstract = item.Abstract;
                    i.Price = item.Price;
                    i.StoreName = item.StoreName;
                    i.PostUser = item.PostUser;
                    i.Num = item.Num;
                    i.Type = item.Type;
                    i.Cost = item.Cost;
                    i.ReunId = item.ReunId;
                    i.CrateDate = item.CrateDate;
                    list.Add(i);

                }
            }



            var json = new
            {
                total = pager.totalRows,
                rows = (from r in list
                        select new ExpenseProduct
                        {
                            ID = r.ID,
                            Abstract = r.Abstract,
                            StoreName = r.StoreName,
                            Price = r.Price,
                            PostUser = r.PostUser,
                            Num = r.Num,
                            Type = r.Type,
                            Cost = r.Cost,
                            ReunId = r.ReunId,
                            CrateDate = r.CrateDate,
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
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
        /// 获得凭证列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetPzList(Lib.GridPager pager, string code, string sub, string dep)
        {
            IQueryable<T_PZ_BX> list = db.T_PZ_BX.AsQueryable();
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
            List<T_PZ_BX> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
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
        /// 附件上传
        /// </summary>
        /// <returns></returns>
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
                        string path = "~/Upload/ExpenseData/" + title;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        file.SaveAs(path);
                        link = "/Upload/ExpenseData/" + title;
                        filesName = "~/Upload/ExpenseData/" + title;
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
                            string path = "~/Upload/ExpenseData/" + title;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            file.SaveAs(path);
                            urllist[i] = path;
                            link = "/Upload/ExpenseData/" + title;
                            filesName = "~/Upload/ExpenseData/" + title;
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
                T_ExpenseEnclosure model = db.T_ExpenseEnclosure.FirstOrDefault(a => a.ExpenseId == id && a.Path == strPath);
                if (model != null)
                {
                    db.T_ExpenseEnclosure.Remove(model);
                    db.SaveChanges();
                }
            }
        }

        //获取附件
        public JsonResult GetExpenseEnclosure(int id)
        {
            List<T_ExpenseEnclosure> model = db.T_ExpenseEnclosure.Where(a => a.ExpenseId == id).ToList();
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

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string statedate, string EndDate)
        {


            string sql = string.Format(@"select (select MatchBorrowNumber from T_Expense where ID=p.ReunId) as  MatchBorrowNumber,(select Reun_Code from T_Expense where ID=p.ReunId) as  Reun_Code,(select CrateDate from T_Expense where ID=p.ReunId) as  CrateDate, 
                                        (select PostUser from T_Expense where ID=p.ReunId) as PostUser,
                                        (select Reun_Reason from T_Expense where ID=p.ReunId) as Reun_Reason,
                                            StoreName,Abstract,Price,Num,
                                    (select top 1 ApproveDate from T_ExpenseApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as ApproveDate,(select top 1 Remark from T_ExpenseApprove where Reunbursement_id =p.ReunId and ApproveName='" + UserModel.Nickname + "' and ApproveDate<>'' order by ID desc) as Remark from T_ExpenseProduct p where ReunId in(select Reunbursement_id from T_ExpenseApprove where ApproveDate>='" + statedate + "' and ApproveDate<='" + EndDate + " 23:59:59' AND  ApproveName='" + UserModel.Nickname + "')");

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
        #endregion
        #region 无票报销  wwt 2017 03 20 
        [HttpPost]
        public JsonResult NoTicketExpenseAddSave(T_NoTicketExpense model)
        {

            using (TransactionScope sc = new TransactionScope())
            {
                string State = "";
                string Message = "";
                try
                {
                    T_NoTicketExpense mod = new T_NoTicketExpense();
                    mod.Note = model.Note;
                    mod.PostUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    mod.TheContent = model.TheContent;
                    mod.Total = model.Total;
                    mod.Status = 0;
                    mod.PostAccountInfo = model.PostAccountInfo;
                    mod.PostTime = DateTime.Now;
                    mod.Code = model.Code;
                    mod.Del = 0;
                    db.T_NoTicketExpense.Add(mod);
                    db.SaveChanges();
                    State = "Success";
                    Message = "保存成功";
                }  
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public class NoTicketExpense
        {
            public string TheContent { get; set; }
            public Nullable<decimal> Total { get; set; }
        }
        public ContentResult NoTicketExpenseGetList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime, string theStatus)
        {
            //所有记录
            var quaryData = db.T_NoTicketExpense.Where(a=>a.Del!=1).AsQueryable();
            var ViewName = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            if (ViewName.Equals("姜尚"))
            {
                if (!string.IsNullOrWhiteSpace(queryStr))
                {
                    quaryData = quaryData.Where(a => a.PostUser == queryStr || a.Code == queryStr);
                }

            }
            else {
                quaryData = quaryData.Where(a => a.PostUser == ViewName);
            }
            //根据日期查询
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {

                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                quaryData = quaryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                quaryData = quaryData.Where(s => s.PostTime >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                quaryData = quaryData.Where(s => s.PostTime <= endTime);
            }
            //根据审核查询
            if (!string.IsNullOrWhiteSpace(theStatus))
            {
                int _theStatus = int.Parse(theStatus);
                quaryData = quaryData.Where(a => a.Status == _theStatus);
            }
            //分页
            pager.totalRows = quaryData.Count();

            List<NoTicketExpense> costList = new List<NoTicketExpense>();
            NoTicketExpense cost = new NoTicketExpense
            {
                TheContent = "总计：",
                Total = quaryData.Count() > 0 ? quaryData.Sum(s => s.Total) : 0
            };
            costList.Add(cost);

            var queryData = quaryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(costList) + "}";
            return Content(json);
        }
        //无票审核保存
        [HttpPost]
        public JsonResult NoTicketExpenseCheckSave(int ID, string PayAccount, int type)
        {
            using(TransactionScope sc = new TransactionScope())
            {
                //type = 1：同意 2：不同意
                string State = "";
                string Message = "";
                try {
                    T_NoTicketExpense MOD = db.T_NoTicketExpense.Find(ID);
                    MOD.Status = type;
                    if (type == 1) 
                    {
                        MOD.PayAccount = PayAccount;
                    }
                    MOD.ApproveTime = DateTime.Now;
                    db.Entry<T_NoTicketExpense>(MOD).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    State = "Success";
                    Message = "保存成功";
                }
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
         
        }
        //删除无票报销记录
        public JsonResult NoTicketExpenseListDel(int ID) { 
            using(TransactionScope sc = new TransactionScope())
            {
               var curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                string State = "";
                string Message = "";
                try {
                    T_NoTicketExpense MOD = db.T_NoTicketExpense.Find(ID);
                    if (MOD.PostUser != curUser)
                    {
                        return Json(new { State = "Faile", Message = "不能删除其他人的记录！" }, JsonRequestBehavior.AllowGet);
                    }
                    MOD.Del = 1;
                    db.SaveChanges();
                    State = "Success";
                    Message = "保存成功";
                }
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
         
        }
        [HttpPost]
        public JsonResult getPayAccountForNoTicketExpense() {
            var modList = db.T_NoTicketExpenseAccount.Where(a => a.Del == 0).AsQueryable();
            return Json(modList);
        }
        //账号列表查询
        [HttpPost]
        public JsonResult NoTicketExpenseAccountList(Lib.GridPager pager, string queryStr,string queryType)
        {
            var modList = db.T_NoTicketExpenseAccount.Where(a=>a.Del==0).AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                modList = modList.Where(a => a.Code == queryStr);
            }
            if (!string.IsNullOrWhiteSpace(queryType))
            {
                modList = modList.Where(a => a.Type == queryType);
            }
            return Json(modList);
        }
        //新增账号保存
        [HttpPost]
        public JsonResult NoTicketExpenseAccountAddSave(T_NoTicketExpenseAccount model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string State = "";
                string Message = "";
                try {
                    T_NoTicketExpenseAccount MOD = new T_NoTicketExpenseAccount();
                    MOD.Code = model.Code;
                    MOD.Type = model.Type;
                    MOD.Del = 0;
                    db.T_NoTicketExpenseAccount.Add(MOD);
                    db.SaveChanges();
                    State = "Success";
                    Message = "保存成功";
                }
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //无票报销账号删除
        public JsonResult NoTicketExpenseAccountDel(int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string State = "";
                string Message = "";
                try
                {
                    T_NoTicketExpenseAccount MOD = db.T_NoTicketExpenseAccount.Find(ID);
                    MOD.Del = 1;
                    db.SaveChanges();
                    State = "Success";
                    Message = "删除成功";
                }
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //无票报销账号编辑
        public JsonResult NoTicketExpenseAccountEditSave(T_NoTicketExpenseAccount model,int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string State = "";
                string Message = "";
                try
                {
                    T_NoTicketExpenseAccount MOD = db.T_NoTicketExpenseAccount.Find(ID);
                    MOD.Code = model.Code;
                    MOD.Type = model.Type;
                    db.SaveChanges();
                    State = "Success";
                    Message = "修改成功";
                }
                catch (Exception e)
                {
                    State = "Faile";
                    Message = e.Message;
                }
                sc.Complete();
                return Json(new { State = State, Message = Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        #endregion
    }
}
