using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Data.Entity.Validation;
using System.EnterpriseServices;
using Newtonsoft.Json;
using EBMS.App_Code;
using System.Transactions;

namespace EBMS.Controllers
{
    public class BusinessTravelController : BaseController
    {
        //
        // GET: /BusinessTravel/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        public List<SelectListItem> getApproveName(int step)
        {
            try
            {
                string approveName = db.T_BusinessTravelConfig.FirstOrDefault(a => a.Step == step).Name;
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

        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "审核人", Value = "审核人" },
            };
            return TypeList;
        }

        #endregion

        #region 视图
        [Description("出差申请")]
        public ActionResult ViewApply()
        {
            ViewData["approveList"] = getApproveName(1);
            return View();
        }
         [Description("出差未审核")]
        public ActionResult ViewUncheckList()
        {
            return View();
        }
         [Description("出差列表")]
        public ActionResult ViewList()
        {
            return View();
        }
         [Description("出差配置")]
         public ActionResult ViewConfig()
         {
             return View();
         }
         [Description("我的出差")]
         public ActionResult ViewMyList()
         {
             return View();
         }
        
         public ActionResult ViewCheckDetail(int PID)
         {
             ViewData["PID"] = PID;
             return View();
         }

         public ActionResult ViewCheck(int PID)
         {
             T_BusinessTravel Wmodel = db.T_BusinessTravel.Find(PID);

             T_BusinessTravelApprove model = db.T_BusinessTravelApprove.FirstOrDefault(a => a.PID == PID && a.Status == 0);
             if (model != null)
             {
                 return View(model);
             }
             else
             {
                 return HttpNotFound();
             }
         }

         public ActionResult ViewConfigAdd()
         {
             int configCount = db.T_BusinessTravelConfig.Count();
             int maxStep = 1;
             if (configCount > 0)
             {
                 maxStep = db.T_BusinessTravelConfig.Max(a => a.Step) + 1;
             }
             ViewData["boolList"] = Com.BoolList;
             ViewData["TypeList"] = TypeList();
             ViewData["maxStep"] = maxStep;
             return View();
         }

         public ActionResult ViewConfigEdit(int ID)
         {
             T_BusinessTravelConfig model = db.T_BusinessTravelConfig.Find(ID);
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
         public ActionResult ViewEdit(int ID)
         {
             T_BusinessTravel model = db.T_BusinessTravel.Find(ID);
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
        #endregion

        #region 绑定数据
         public ContentResult GetList(Lib.GridPager pager, string queryStr, int isUncheck=0,int isMy=0)
         {
             string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
             IQueryable<T_BusinessTravel> queryData = db.T_BusinessTravel.Where(a => a.isDelete == 0);
             if(isUncheck==1)
             {
                 int[] step = (from r in db.T_BusinessTravelConfig
                               where r.isMultiple == "1" && r.Name.Contains(name)
                               select r.Step).ToArray();
                 queryData = queryData.Where(a => (a.Status == 0 || a.Status == -1) && (a.CurrentApprove == name || step.Contains(a.Step)));
             }
             else if (isMy == 1)
             {
                 queryData = queryData.Where(a => a.PostName == name);
             }
             if (!string.IsNullOrEmpty(queryStr))
             {
                 queryData = queryData.Where(a => (a.PostName != null && a.PostName.Contains(queryStr)) || (a.Address != null && a.Address.Contains(queryStr)));
             }

             pager.totalRows = queryData.Count();
             //分页
             List<T_BusinessTravel> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
             return Content(json);
         }

         public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
         {
             IQueryable<T_BusinessTravelConfig> queryData = db.T_BusinessTravelConfig;

             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
             }
             List<T_BusinessTravelConfig> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             pager.totalRows = queryData.Count();
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
             return Content(json);
            
         }

         public ContentResult GetCheckDetailList(int PID)
         {
             List<T_BusinessTravelApprove> list = db.T_BusinessTravelApprove.Where(a => a.PID == PID).ToList();
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
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "出差").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from  T_BusinessTravelApprove where Pid in (select ID from T_BusinessTravel where(Status = -1 or Status = 0) and IsDelete = 0) and Status = 0 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "出差" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "出差";
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
            string RejectNumberSql = "select PostName as PendingAuditName,COUNT(*) as NotauditedNumber from T_BusinessTravel where Status='2' and IsDelete=0  GROUP BY PostName  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "出差" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "出差";
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

        public JsonResult ApplySave(T_BusinessTravel model)
        {
            try
            {

                model.PostName = Server.UrlDecode(Request.Cookies["NickName"].Value);
                model.Status = -1;
                model.Step = 1;
                model.isDelete = 0;
                model.PostTime = DateTime.Now;
                db.T_BusinessTravel.Add(model);
                db.SaveChanges();

                T_BusinessTravelApprove approve = new T_BusinessTravelApprove();
                approve.Status = 0;
                approve.ApproveName = model.CurrentApprove;
                approve.PID = model.ID;
                approve.Step = 1;
                db.T_BusinessTravelApprove.Add(approve);
                db.SaveChanges();
                //ModularByZP();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            
            catch (DbEntityValidationException e)
            {
                return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public JsonResult EditSave(T_BusinessTravel model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_BusinessTravel editModel = db.T_BusinessTravel.Find(model.ID);
                    editModel.Address = model.Address;
                    editModel.Reason = model.Reason;
                    editModel.Money = model.Money;
                    editModel.Transport = model.Transport;
                    editModel.CurrentApprove = model.CurrentApprove;
                   
                    editModel.EndTime = model.EndTime;
                    
                    
                    editModel.StartTime = model.StartTime;
                   
                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;

                        T_BusinessTravelApprove newApprove = new T_BusinessTravelApprove();
                        newApprove.ApproveName = model.CurrentApprove;
                        newApprove.PID = model.ID;
                        newApprove.Status = 0;
                        newApprove.Step = 1;
                        db.T_BusinessTravelApprove.Add(newApprove);
                    }


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
        public JsonResult Delete(int ID)
        {
            try
            {
                T_BusinessTravel editModel = db.T_BusinessTravel.Find(ID);
                editModel.isDelete = 1;
                
                db.SaveChanges();
                //ModularByZP();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }
        #region 配置
        [HttpPost]
        public JsonResult ConfigAddSave(T_BusinessTravelConfig model)
        {
            try
            {
                db.T_BusinessTravelConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult BusinessTravelCheckSave(T_BusinessTravelApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_BusinessTravelApprove editModel = db.T_BusinessTravelApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveTime = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_BusinessTravelApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_BusinessTravel BusinessModel = db.T_BusinessTravel.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_BusinessTravelConfig> ConfigList = db.T_BusinessTravelConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            BusinessModel.Status = 0;
                            BusinessModel.Step = nextStep;

                            BusinessModel.CurrentApprove = ConfigList.First().Name;
                            T_BusinessTravelApprove newApprove = new T_BusinessTravelApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = 0;
                            newApprove.Step = nextStep;
                            db.T_BusinessTravelApprove.Add(newApprove);
                        }
                        else//结束
                        {
                            BusinessModel.CurrentApprove = "";
                            BusinessModel.Status = 1;
                            BusinessModel.Step = 99;
                          
                        }
                    }
                    else//不同意结束
                    {
                        BusinessModel.CurrentApprove = "";
                        BusinessModel.Status = 2;
                        BusinessModel.Step = 99;
                    }
                    db.Entry<T_BusinessTravel>(BusinessModel).State = System.Data.Entity.EntityState.Modified;
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
        public JsonResult ConfigDelete(int ID)
        {
            try
            {
                T_BusinessTravelConfig delModel = db.T_BusinessTravelConfig.Find(ID);
                db.T_BusinessTravelConfig.Remove(delModel);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ConfigEditSave(T_BusinessTravelConfig model)
        {
            try
            {
                T_BusinessTravelConfig editModel = db.T_BusinessTravelConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;
                editModel.isMultiple = model.isMultiple;
                db.Entry<T_BusinessTravelConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
