using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Transactions;
using EBMS.App_Code;
using Newtonsoft.Json;
using System.Collections;
using System.EnterpriseServices;

namespace EBMS.Controllers
{
    public class DepartmentActivityController : BaseController
    {
        //
        // GET: /DepartmentActivity/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
       
        public List<SelectListItem> getApproveName (int step) 
        {
            try
            {
                string approveName = db.T_DepartmentActivityConfig.FirstOrDefault(a => a.Step == step).Name;
                string[] approveArr = approveName.Split(',');
                List<SelectListItem> approveList = new List<SelectListItem>();
                foreach (string item in approveArr)
                {
                    SelectListItem selectItem = new SelectListItem();
                    selectItem.Text = item;
                    selectItem.Value = item;
                    approveList.Add(selectItem);
                }
                return approveList;
            }
            catch (Exception)
            {
                List<SelectListItem> approveList = new List<SelectListItem> { 
                new SelectListItem { Text="请联系人事配置审核流程",Value=""}
                };
                return approveList;
            }
        }
        #endregion
        #region 视图
        [Description("活动申请")]
        public ActionResult ViewDepartmentActivityApply()
        {
            T_DepartmentActivity model = new T_DepartmentActivity();
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);

            ViewData["approveList"] = getApproveName(1);
            T_User user=db.T_User.FirstOrDefault(a=>a.Nickname==name);
            model.PostUser = name;
            //model.CurrentApprove = approveName;
            model.ChargePerson = name;
            model.Tel = user.Tel;
            model.Dept =Com.GetDepartmentName(Convert.ToInt32(user.DepartmentId));
            return View(model);
        }

        [Description("活动列表")]
        public ActionResult ViewDepartmentActivityList()
        {
            return View();
        }

        [Description("我的活动")]
        public ActionResult ViewDepartmentActivityMyList()
        {
            return View();
        }

        [Description("活动未审核")]
        public ActionResult ViewDepartmentActivityUncheckList()
        {
            return View();
        }

        public ActionResult ViewEdit(int ID)
        {
            T_DepartmentActivity model = db.T_DepartmentActivity.Find(ID);
            ViewData["approveList"] = getApproveName(1);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult ViewCheck(int PID)
        {
            T_DepartmentActivityApprove model = db.T_DepartmentActivityApprove.FirstOrDefault(a => a.PID == PID && a.ApproveTime == null);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Description("活动配置")]
        public ActionResult ViewDepartmentActivityConfig()
        {
            return View();
        }

        public ActionResult ViewConfigAdd()
        {
            ViewData["boolList"] = Com.BoolList;
            ViewData["ListUser"] = Com.UserList();
            return View();
        }

        public ActionResult ViewConfigEdit(int ID)
        {
            T_DepartmentActivityConfig model = db.T_DepartmentActivityConfig.Find(ID);
            ViewData["boolList"] = Com.BoolList;
            ViewData["ListUser"] = Com.UserList();
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Description("审核详情")]
        public ActionResult ViewCheckDetail(int DID)
        {
            ViewData["DID"] = DID;
            return View();
        }

        #endregion

        #region 绑定数据
        public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_DepartmentActivityConfig> queryData = db.T_DepartmentActivityConfig;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_DepartmentActivityConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetDepartmentActivityList(Lib.GridPager pager, string queryStr, int isMy = 0, int isUncheck=0)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_DepartmentActivity> queryData = db.T_DepartmentActivity.Where(a=>a.isDelete==0);
            if (isMy == 1)
            {
                queryData = queryData.Where(a => a.PostUser == name);
            }
            else if (isUncheck == 1)
            {
                int[] step = (from r in db.T_DepartmentActivityConfig
                              where r.isMultiple == "1" && r.Name.Contains(name)
                              select r.Step).ToArray();
                queryData = queryData.Where(a=>(a.Status == -1 || a.Status == 0) && (a.CurrentApprove == name || step.Contains(a.Step)));
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_DepartmentActivity> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //获取审核详情  
        public ContentResult GetCheckList(int DID)
        {
            List<T_DepartmentActivityApprove> list = db.T_DepartmentActivityApprove.Where(a => a.PID == DID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "部门活动").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from  T_DepartmentActivityApprove where Pid in (select ID from T_DepartmentActivity where(Status = -1 or Status = 0) and IsDelete = 0) and Status = 0 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "部门活动" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "部门活动";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_DepartmentActivity where Status='2' and IsDelete=0  GROUP BY PostUser  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "部门活动" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "部门活动";
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
        #endregion

        #region 增删改
        public JsonResult DepartmentActivityApplySave(T_DepartmentActivity model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 1;
                    db.T_DepartmentActivity.Add(model);
                    db.SaveChanges();

                    T_DepartmentActivityApprove newApprove = new T_DepartmentActivityApprove();
                    newApprove.ApproveName = model.CurrentApprove;
                    newApprove.PID = model.ID;
                    newApprove.Status = 0;
                    newApprove.Step = 1;
                    db.T_DepartmentActivityApprove.Add(newApprove);
                    db.SaveChanges();
                    //ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
           
        }
        
        [HttpPost]
        public JsonResult DeleteDepartmentActivity(int ID)
        {
            try
            {
                T_DepartmentActivity editModel = db.T_DepartmentActivity.Find(ID);
                editModel.isDelete = 1;
                db.SaveChanges();
               // ModularByZP();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ConfigAddSave(T_DepartmentActivityConfig model)
        {
            try
            {
                db.T_DepartmentActivityConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ConfigEditSave(T_DepartmentActivityConfig model)
        {
            try
            {
                T_DepartmentActivityConfig editModel = db.T_DepartmentActivityConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;
                editModel.isMultiple = model.isMultiple;
                db.Entry<T_DepartmentActivityConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ConfigDelete(int ID)
        {
            try
            {
                T_DepartmentActivityConfig delModel = db.T_DepartmentActivityConfig.Find(ID);
                db.T_DepartmentActivityConfig.Remove(delModel);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DepartmentActivityEditSave(T_DepartmentActivity model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_DepartmentActivity editModel = db.T_DepartmentActivity.Find(model.ID);
                    editModel.Address = model.Address;
                    editModel.ChargePerson = model.ChargePerson;
                    editModel.Contents = model.Contents;
                    editModel.CurrentApprove = model.CurrentApprove;
                    editModel.Dept = model.Dept;
                    editModel.EndTime = model.EndTime;
                    editModel.Expense = model.Expense;
                    editModel.PeopleNum = model.PeopleNum;
                    editModel.Persons = model.Persons;
                    editModel.PostUser = model.PostUser;
                    editModel.Reason = model.Reason;
                    editModel.StartTime = model.StartTime;
                    editModel.Tel = model.Tel;
                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;

                        T_DepartmentActivityApprove newApprove = new T_DepartmentActivityApprove();
                        newApprove.ApproveName = model.CurrentApprove;
                        newApprove.PID = model.ID;
                        newApprove.Status = 0;
                        newApprove.Step = 1;
                        db.T_DepartmentActivityApprove.Add(newApprove);
                    }


                    db.SaveChanges();
                   // ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult DepartmentActivityCheckSave(T_DepartmentActivityApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_DepartmentActivityApprove editModel = db.T_DepartmentActivityApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveTime = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_DepartmentActivityApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_DepartmentActivity DepartmentActivityModel = db.T_DepartmentActivity.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_DepartmentActivityConfig> ConfigList = db.T_DepartmentActivityConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            DepartmentActivityModel.Status = 0;
                            DepartmentActivityModel.Step = nextStep;
                            DepartmentActivityModel.CurrentApprove = ConfigList.First().Name;
                            T_DepartmentActivityApprove newApprove = new T_DepartmentActivityApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = 0;
                            newApprove.Step = nextStep;
                            db.T_DepartmentActivityApprove.Add(newApprove);
                        }
                        else//结束
                        {
                            DepartmentActivityModel.Status = 1;
                            DepartmentActivityModel.Step = 99;
                        }
                    }
                    else//不同意结束
                    {
                        DepartmentActivityModel.Status = 2;
                        DepartmentActivityModel.Step = 99;
                    }
                    db.Entry<T_DepartmentActivity>(DepartmentActivityModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                   // ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion
    }
}
