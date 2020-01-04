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
    public class AppBorrowController : Controller
    {
        //
        // GET: /APP/AppBorrow/  借支管理
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
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = -1, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_BorrowApprove> ApproveMod = new List<T_BorrowApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_BorrowApprove.Where(a => a.ApproveName == CurUser).ToList();
            }
            else
            {
                ApproveMod = db.T_BorrowApprove.Where(a => a.ApproveName == CurUser && a.ApproveStatus == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Pid.ToString());
            }
            IQueryable<T_Borrow> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_Borrow
                            where Arry.Contains(r.ID) && r.IsDelete == 0 && r.BorrowState != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_Borrow
                            where r.IsDelete == 0 && r.BorrowName == CurUser && r.BorrowState != 3
                            select r;
            }
            //pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((page - 1) * pageSize).Take(pageSize);
            List<resultItem> list = new List<resultItem>();
            foreach (var item in queryData)
            {
                resultItem i = new resultItem();
                i.uid = item.ID;
                string str = item.BorrowerDep;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "借支部门：" + str;
                i.title = "申请人：" + item.BorrowName;

                i.remark = int.Parse(item.BorrowState.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string BorrowName { get; set; }
            public string BorrowerFrom { get; set; }
            public string BorrowerDep { get; set; }
            public string BorrowReason { get; set; }
            public decimal BorrowMoney { get; set; }
            public Nullable<System.DateTime> BorrowDate { get; set; }
            public int BorrowState { get; set; }
            public int BorrowSettementState { get; set; }
            public string BorrowCode { get; set; }
            public string BorrowAccountID { get; set; }
            public string BorrowBank { get; set; }
            public Nullable<System.DateTime> BorrowNeedDate { get; set; }
            public string BorrowAccountName { get; set; }
            public int BorrowNextApprove { get; set; }
            public int BorrowStep { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public Nullable<int> IsVoucher { get; set; }
        }
       
        //详情数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_Borrow mod = db.T_Borrow.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.BorrowName = mod.BorrowName;
            list.BorrowerFrom = mod.BorrowerFrom;
            list.BorrowerDep = mod.BorrowerDep;
            list.BorrowReason = mod.BorrowReason;
            list.BorrowMoney = mod.BorrowMoney;
            list.BorrowDate = mod.BorrowDate;
            list.BorrowState = mod.BorrowState;
            list.BorrowSettementState = mod.BorrowSettementState;
            list.BorrowCode = mod.BorrowCode;
            list.BorrowAccountID = mod.BorrowAccountID;
            list.BorrowBank = mod.BorrowBank;
            list.BorrowNeedDate = mod.BorrowNeedDate;
            list.BorrowAccountName = mod.BorrowAccountName;
            list.BorrowNextApprove = mod.BorrowNextApprove;
            list.BorrowStep = mod.BorrowStep;
            list.BorrowAccountName = mod.BorrowAccountName;
            list.IsVoucher = mod.IsVoucher;
          
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_BorrowApprove> mod_Approve = db.T_BorrowApprove.Where(a => a.Pid == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_BorrowApprove Approve = db.T_BorrowApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.Pid == ID);
            if (Approve == null)
            {
                Approve = db.T_BorrowApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.Pid == ID);
            }


            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_BorrowApprove MyApprove = db.T_BorrowApprove.FirstOrDefault(a => a.Pid == ID && a.ApproveName == UserName && a.ApproveTime == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            //财务主管？
            //int Cashier = 0;
            int curStep = int.Parse(mod.BorrowStep.ToString());
            //取最后一步
           
            int Step = db.T_BorrowApproveConfig.ToList().Count;
          
           //如果不是最后1 步，就显示选择下拉框
         
           

           List<SelectListItem> getCheckMan = new List<SelectListItem>();
           T_BorrowApproveConfig approveusers;
           mod.BorrowStep = mod.BorrowStep + 1;
           if (UserName == "子轩" && mod.BorrowStep == 3)
           {
               mod.BorrowStep = mod.BorrowStep + 1;
               approveusers = db.T_BorrowApproveConfig.FirstOrDefault(a => a.Step == mod.BorrowStep);
           }
           else
           {
               approveusers = db.T_BorrowApproveConfig.FirstOrDefault(a => a.Step == mod.BorrowStep);
           }
           //var approveusers = db.T_BorrowApproveConfig.FirstOrDefault(a => a.Step == mod.BorrowStep + 1);
           if (approveusers != null)
           {
               //如果是动态获取当前部门主管
               if (approveusers.ApproveUser.Equals("部门主管"))
               {
                   List<SelectListItem> items = new List<SelectListItem>();
                   items.Add(new SelectListItem { Text = "请选择", Value = "9999" });
                   getCheckMan = items;

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
                   getCheckMan = items;
               }
           }
           else
           {
               getCheckMan = null;
           }
         //支付
           List<SelectListItem> listBorrowForm = Lib.Comm.BorrowForm;
           string theBorrowForm = JsonConvert.SerializeObject(listBorrowForm, Lib.Comm.setTimeFormat());
            //审核人
           string CheckManJson = JsonConvert.SerializeObject(getCheckMan, Lib.Comm.setTimeFormat());
            //公司
           List<SelectListItem> listCompany = Com.ExpenseCompany();
           string theCompany = JsonConvert.SerializeObject(listCompany, Lib.Comm.setTimeFormat());




           result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":" + myCheck + ",\"Step\":" + Step + ",\"approveId\":" + Approve.ID + ",\"CheckList\":" + CheckManJson + ",\"theBorrowForm\":" + theBorrowForm + ",\"listCompany\":" + theCompany + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
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
        public JsonResult Check(string UserName, int approveID, int status, string memo, string nextapprove, string BorrowerFrom, string company, string number)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    int TotalStep = db.T_BorrowApproveConfig.ToList().Count;
                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    T_BorrowApprove approve = db.T_BorrowApprove.SingleOrDefault(a => a.ID == approveID && a.ApproveStatus == -1 && (a.ApproveName == curName || a.ApproveName == Nickname));
                    if (approve == null)
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
                                CreatUser = UserName,
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
                            T_BorrowApprove newApprove = new T_BorrowApprove();
                            newApprove.ApproveStatus = -1;
                            if (UserName == "子轩" && model.BorrowStep == 2)
                            {
                                newApprove.ApproveName = "三疯";
                                model.Cashier = nextapprove;
                            }
                            else if (UserName == "三疯" && model.BorrowStep == 3)
                            {
                                newApprove.ApproveName = model.Cashier;
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
                    List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "借支").ToList();
                    if (ModularNotaudited.Count > 0)
                    {
                        foreach (var item in ModularNotaudited)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
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
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
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
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
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
        //选择公司绑定账号
         [HttpPost]
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

    }
}
