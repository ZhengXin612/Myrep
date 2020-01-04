using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
namespace EBMS.Areas.APP.Controllers
{
    public class AppExpenseController : Controller
    {
        //报销控制器
        // GET: /APP/AppExpense/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(int id)
        {
            ViewData["ID"] = id;
            return View();
        }
        
        class resultItem
        {
            public string title { get; set; }//申请人
            public int uid { get; set; }//id
            public string subTitle { get; set; }//理由
            public int remark { get; set; }//状态
        }
        //列表
        public JsonResult GetList(string CurUser, int page, int pageSize, string queryStr, int Status = -1, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_ExpenseApprove> ApproveMod = new List<T_ExpenseApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_ExpenseApprove.Where(a => a.ApproveName == CurUser).ToList();
            }
            else
            {
                ApproveMod = db.T_ExpenseApprove.Where(a => a.ApproveName == CurUser && a.ApproveStatus == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Reunbursement_id.ToString());
            }
            IQueryable<T_Expense> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_Expense
                            where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_Expense
                            where r.IsDelete == 0 && r.PostUser == CurUser && r.Status != 3
                            select r;
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser.Contains(queryStr) || a.Reun_Code.Contains(queryStr));
            }
           int ccc = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((page - 1) * pageSize).Take(pageSize);
            List<resultItem> list = new List<resultItem>();
            foreach (var item in queryData)
            {
                resultItem i = new resultItem();
                i.uid = item.ID;
                string str = item.Reun_Code;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 22)
                {
                    str = str.Substring(0, 22) + "...";
                }
                i.subTitle = "编码：" + str;
                i.title = "申请人：" + item.PostUser;

                i.remark = int.Parse(item.Status.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string Department { get; set; }
            public string Reun_Reason { get; set; }
            public int ExpenseNextApprove { get; set; }
            public decimal Reun_Cost { get; set; }
            public string PostUser { get; set; }
            public System.DateTime CrateDate { get; set; }
            public string Reun_Code { get; set; }
            public string AccountType { get; set; }
            public string Reun_Bank { get; set; }
            public string Shop { get; set; }
            public string Reun_Name { get; set; }
            public string Car_Number { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public bool IsExpenseMatch { get; set; }
            public int IsExpenseEnclosure { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public Nullable<int> Pz_BXStatus { get; set; }
            public string MatchBorrowNumber { get; set; }
        }
        public partial class Modular
        {
            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
      
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
        //详情页面数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_Expense mod = db.T_Expense.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.Department = mod.Department;
            list.Reun_Reason = mod.Reun_Reason;
            list.ExpenseNextApprove = mod.ExpenseNextApprove;
            list.Reun_Cost = mod.Reun_Cost;
            list.PostUser = mod.PostUser;
            list.CrateDate = mod.CrateDate;
            list.Reun_Code = mod.Reun_Code;
            list.AccountType = mod.AccountType;
            list.Reun_Bank = mod.Reun_Bank;
            list.Shop = mod.Shop;
            list.Reun_Name = mod.Reun_Name;
            list.Car_Number = mod.Car_Number;
            list.Status = mod.Status;
            list.Step = mod.Step;
            list.IsExpenseMatch = mod.IsExpenseMatch;
            list.IsExpenseEnclosure = mod.IsExpenseEnclosure;
            list.Pz_BXStatus = mod.Pz_BXStatus;
            list.MatchBorrowNumber = mod.MatchBorrowNumber;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            //详情
            IQueryable<T_ExpenseProduct> mod_Detail = db.T_ExpenseProduct.Where(a => a.ReunId == ID);
            string modDetail = JsonConvert.SerializeObject(mod_Detail, Lib.Comm.setTimeFormat());
            //审核记录
            IQueryable<T_ExpenseApprove> mod_Approve = db.T_ExpenseApprove.Where(a => a.Reunbursement_id == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());
           
            T_Expense lossreport = db.T_Expense.Find(ID);
            //用于判断是不是 财务主管 
            int nextMan = 0;
            int Step = db.T_ExpenseApproveConfig.ToList().Count;
            if (mod.Step != Step - 1) {
                nextMan = 1;
            }
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_ExpenseApprove MyApprove = db.T_ExpenseApprove.FirstOrDefault(a => a.Reunbursement_id == ID && a.ApproveName == UserName && a.ApproveDate == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            //审核人
            List<SelectListItem> getCheckMan = new List<SelectListItem>();
            var approveusers = db.T_ExpenseApproveConfig.FirstOrDefault(a => a.Step == 4);
            if (approveusers != null)
            {
                //如果是动态获取当前部门主管
                string[] array = approveusers.ApproveUser.Split(',');
                List<SelectListItem> items = new List<SelectListItem>();
                foreach (var item in array)
                {
                    T_User user = db.T_User.FirstOrDefault(a => a.Nickname.Equals(item) || a.Name.Equals(item));
                    if (user != null)
                        items.Add(new SelectListItem { Text = user.Nickname, Value = user.ID.ToString() });
                }
                getCheckMan = items;
            }
            else
            {
                getCheckMan = null;
            }
            string CheckManJson = JsonConvert.SerializeObject(getCheckMan);
            string ExpStatus = JsonConvert.SerializeObject(Com.ExpenseStatus());
            
            //未审核记录ID
            T_ExpenseApprove Approve = db.T_ExpenseApprove.FirstOrDefault(s => !s.ApproveDate.HasValue && s.Reunbursement_id == ID);
            if (Approve == null)
            {
                Approve = db.T_ExpenseApprove.FirstOrDefault(s => s.ApproveDate.HasValue && s.Reunbursement_id == ID);

            }

            result += "{\"Main\":[" + modJson + "],\"Detail\":" + modDetail + ",\"Approve\":" + approve + ",\"Step\":" + mod.Step + ",\"approveId\":" + Approve.ID + ",\"myCheck\":" + myCheck + ",\"ExpStatus\":" + ExpStatus + ",\"CheckList\":" + CheckManJson + ",\"Company\":" + JsonConvert.SerializeObject(Com.ExpenseCompany()) + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
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
        /// <summary>
     
        [HttpPost]
        public JsonResult Check(string UserName, int approveID, int status, string memo, string nextapprove, string company, string number, string ExpStatus)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    int TotalStep = db.T_ExpenseApproveConfig.ToList().Count;
                    T_ExpenseApprove approve = db.T_ExpenseApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1 && (a.ApproveName == curName || a.ApproveName == Nickname));
                    if (approve == null)
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
                    if (ExpStatus != null && ExpStatus != "")
                    {
                        model.ExpStatus = ExpStatus;
                    }
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
                                CreatUser = UserName,
                                PayMoney = Convert.ToDouble("-" + model.Reun_Cost)
                            };
                            db.T_PP.Add(pp);
                            db.SaveChanges();
                        }
                        else
                        {
                            if (status != 2)
                            {
                                T_ExpenseApproveConfig ModelConFig = db.T_ExpenseApproveConfig.SingleOrDefault(a => a.Step == Step);


                                T_ExpenseApprove newApprove = new T_ExpenseApprove();
                                newApprove.ApproveStatus = -1;
                                if (Step == 4)
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
                                db.T_ExpenseApprove.Add(newApprove);

                                T_User u = db.T_User.FirstOrDefault(a => a.Nickname.Equals(nextapprove));
                                model.ExpenseNextApprove = u.ID;
                                model.Status = 0;
                            }
                        }
                        model.Step = Step;
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
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
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
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
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
    }
}
