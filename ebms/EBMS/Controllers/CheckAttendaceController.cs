using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Data.Entity.Validation;
using Newtonsoft.Json;
using System.Transactions;
using EBMS.App_Code;
namespace EBMS.Controllers
{
    public class CheckAttendaceController : BaseController
    {
        //
        // GET: /CheckAttendace/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        public string getType(string type)
        { 
            switch (type)
            {
                case "1":
                    return "异常";
                case "2":
                    return "请假";
                case "3":
                    return "销假";
                case "4":
                    return "调休";
                default:
                    return "";

            }
        
        }

        public List<SelectListItem> getMode(int step)
        {
            List<SelectListItem> ModeList = new List<SelectListItem>();
            List<T_CheckAttendaceConfig> configList = db.T_CheckAttendaceConfig.Where(a => a.Step == step).ToList();
            SelectListItem defaultItem = new SelectListItem { Text = "==请选择==", Value = "" };
            ModeList.Add(defaultItem);
            foreach (T_CheckAttendaceConfig configItem in configList)
            {

                string approveName = configItem.Name;
                string[] approveArr = approveName.Split(',');
                foreach (string item in approveArr)
                {
                    SelectListItem selectItem = new SelectListItem();
                    selectItem.Text = item;
                     selectItem.Value = configItem.Mode.ToString();
                    //selectItem.Value = item;

                    ModeList.Add(selectItem);
                }
            }
            return ModeList;
        }
        public List<SelectListItem> getApproveName(int step)
        {
            try
            {
                List<SelectListItem> approveList = new List<SelectListItem>();
                List<T_CheckAttendaceConfig> configList = db.T_CheckAttendaceConfig.Where(a => a.Step == step).ToList();
                SelectListItem defaultItem = new SelectListItem { Text = "==请选择==", Value = "" };
                approveList.Add(defaultItem);
                foreach (T_CheckAttendaceConfig configItem in configList)
                {
                    
                    string approveName = configItem.Name;
                    string[] approveArr = approveName.Split(',');
                    foreach (string item in approveArr)
                    {
                        SelectListItem selectItem = new SelectListItem();
                        selectItem.Text = item;
                       // selectItem.Value = configItem.Mode.ToString();
                        selectItem.Value = item;
                        //if (CurrentApprove == item)
                        //{
                        //    selectItem.Selected = true;
                        //}
                        approveList.Add(selectItem);
                    }
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
        public List<SelectListItem> getLeaveType()
        {
            var list = db.T_CheckAttendaceConfig.Where(a => a.Type == "请假类型").AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==选择请假类型==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "审核人", Value = "审核人" },
            new SelectListItem { Text = "请假类型", Value = "请假类型" },
            };
            return TypeList;
        }

       
        #endregion
        #region 视图
        public ActionResult ViewAttendaceApply(string flag)
        {
            T_CheckAttendace model = new T_CheckAttendace();
            ViewData["flag"] = flag;
            ViewData["approveList"] = getApproveName(1);
            ViewData["LeaveTypeList"] = getLeaveType();
            ViewData["ModeList"] = getMode(1);
            model.Type = getType(flag);
            return View(model);
        }
        public ActionResult ViewAttendaceEdit(int ID)
        {
            T_CheckAttendace model = db.T_CheckAttendace.Find(ID);
           
            ViewData["approveList"] = getApproveName(1);
            ViewData["LeaveTypeList"] = getLeaveType();
            ViewData["ModeList"] = getMode(1);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        public ActionResult ViewList()
        {
            return View();
        }
        public ActionResult ViewMyList()
        {
            return View();
        }
        
        public ActionResult ViewUncheckList()
        {
            return View();
        }
        public ActionResult ViewCheck(int PID)
        {
            T_CheckAttendanceApprove model = db.T_CheckAttendanceApprove.FirstOrDefault(a => a.Status == -1 && a.PID == PID);
            return View(model);
        }
        public ActionResult ViewCheckDetail(int ID=0)
        {
            ViewData["PID"] = ID;
            return View();
        }

        public ActionResult ViewAttendaceConfigList()
        {
            return View();
        }
        public ActionResult ViewConfigAdd()
        {
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            return View();
        }

        public ActionResult ViewConfigEdit(int ID)
        {
            T_CheckAttendaceConfig model = db.T_CheckAttendaceConfig.Find(ID);
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

       
        #endregion
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        #region 绑定数据
        public ContentResult GetList(Lib.GridPager pager, string queryStr, int isUncheck = 0, int isMy = 0)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_CheckAttendace> queryData = db.T_CheckAttendace.Where(a => a.IsDelete == 0);
            if (isMy == 1)//我的申请
            {
                queryData = queryData.Where(a => a.PostName == name);
            }

            else if (isUncheck == 1)//未审核
            {
                int[] step = (from r in db.T_CheckAttendaceConfig
                              where (r.isMultiple == "1" && r.Name.Contains(name)) //|| (r.isMultiple == "0" && r.Name==name)
                              select r.Step).ToArray();
                queryData = queryData.Where(a => a.CurrentApprove == name || step.Contains(a.Step));
            }

            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.PostName != null && a.PostName.Contains(queryStr));
            }
            List<T_CheckAttendace> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            pager.totalRows = queryData.Count();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
           
        }

        public ContentResult GetCheckDetailList(int PID)
        {
            List<T_CheckAttendanceApprove> list = db.T_CheckAttendanceApprove.Where(a => a.PID == PID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
        {

            IQueryable<T_CheckAttendaceConfig> queryData = db.T_CheckAttendaceConfig;

            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
            }
            List<T_CheckAttendaceConfig> list = queryData.OrderByDescending(a => a.Mode).ThenBy(a=>a.Step).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            pager.totalRows = queryData.Count();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);


        }
        #endregion
        #region 增删改
        public JsonResult ApplySave(T_CheckAttendace model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    model.PostName = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    model.Status = -1;
                    model.Step = 1;
                    model.IsDelete = 0;
                    if (model.LeaveType == null || model.LeaveType == "")
                    {
                        model.LeaveType = model.Type;
                    }
                    model.PostTime = DateTime.Now;
                    db.T_CheckAttendace.Add(model);
                    db.SaveChanges();

                    T_CheckAttendanceApprove approve = new T_CheckAttendanceApprove();
                    approve.Status = -1;
                    approve.ApproveName = model.CurrentApprove;
                    approve.PID = model.ID;
                    approve.Step = 1;
                    db.T_CheckAttendanceApprove.Add(approve);
                    db.SaveChanges();

                    //ModularByZP();

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            
        }
        public JsonResult EditSave(T_CheckAttendace model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_CheckAttendace editModel = db.T_CheckAttendace.Find(model.ID);

                    editModel.CurrentApprove = model.CurrentApprove;
                    editModel.Days = model.Days;
                    editModel.EndTime = model.EndTime;
                    editModel.Mode = model.Mode;
                    editModel.Reason = model.Reason;
                    editModel.StartTime = model.StartTime;
                    editModel.WorkEndTime = model.WorkEndTime;
                    editModel.WorkStartTime = model.WorkStartTime;
                    editModel.IsDelete = 0;
                    if (model.LeaveType == null || model.LeaveType == "")
                    {
                        model.LeaveType = model.Type;
                    }
                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;
                        T_CheckAttendanceApprove approve = new T_CheckAttendanceApprove();
                        approve.Status = -1;
                        approve.ApproveName = model.CurrentApprove;
                        approve.PID = model.ID;
                        approve.Step = 1;
                        db.T_CheckAttendanceApprove.Add(approve);
                        db.SaveChanges();
                    }
                    db.Entry<T_CheckAttendace>(editModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (DbEntityValidationException e)
                {
                    return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            
        }
        public JsonResult Delete(int ID)
        {
            try
            {
                T_CheckAttendace delModel = db.T_CheckAttendace.Find(ID);
                delModel.IsDelete = 1;
                db.Entry<T_CheckAttendace>(delModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                //ModularByZP();

                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CheckSave(T_CheckAttendanceApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_CheckAttendanceApprove editModel = db.T_CheckAttendanceApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveDate = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_CheckAttendanceApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_CheckAttendace CModel = db.T_CheckAttendace.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_CheckAttendaceConfig> ConfigList = db.T_CheckAttendaceConfig.Where(a => a.Step == nextStep&&a.Mode==CModel.Mode).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            CModel.Status = 0;
                            CModel.Step = nextStep;
                            CModel.CurrentApprove = ConfigList.First().Name;
                            T_CheckAttendanceApprove newApprove = new T_CheckAttendanceApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = -1;
                            newApprove.Step = nextStep;
                            db.T_CheckAttendanceApprove.Add(newApprove);
                        }
                        else//结束
                        {
                            CModel.CurrentApprove = "";
                            CModel.Status = 1;
                            CModel.Step = 99;
                           
                        }
                    }
                    else//不同意结束
                    {
                        CModel.CurrentApprove = "";
                        CModel.Status = 2;
                        CModel.Step = 99;
                    }
                    db.Entry<T_CheckAttendace>(CModel).State = System.Data.Entity.EntityState.Modified;
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


        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "请假调休").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = "select CurrentApprove as PendingAuditName,COUNT(*) as NotauditedNumber from T_CheckAttendace  where  (Status = -1 or Status = 0 ) and IsDelete='0' GROUP BY CurrentApprove ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "请假调休" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "请假调休";
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
            string RejectNumberSql = "select PostName as PendingAuditName,COUNT(*) as NotauditedNumber from T_CheckAttendace where Status='2' and IsDelete=0  GROUP BY PostName ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "请假调休" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "请假调休";
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
        #region 配置增删改
         [HttpPost]
        public JsonResult ConfigAddSave(T_CheckAttendaceConfig model)
        {
            try
            {
                db.T_CheckAttendaceConfig.Add(model);
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
                 T_CheckAttendaceConfig delModel = db.T_CheckAttendaceConfig.Find(ID);
                 db.T_CheckAttendaceConfig.Remove(delModel);
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
                 return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
             }
         }
         [HttpPost]
         public JsonResult ConfigEditSave(T_CheckAttendaceConfig model)
         {
             try
             {
                 T_CheckAttendaceConfig editModel = db.T_CheckAttendaceConfig.Find(model.ID);
                 editModel.Name = model.Name;
                 editModel.Step = model.Step;
                 editModel.Type = model.Type;
                 editModel.isMultiple = model.isMultiple;
                 editModel.Mode = model.Mode;
                 db.Entry<T_CheckAttendaceConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
                 return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
             }
         }
        #endregion
        #endregion
    }
}
