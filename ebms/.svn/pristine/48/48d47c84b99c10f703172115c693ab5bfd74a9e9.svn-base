using System;
using System.Collections.Generic;
using System.EnterpriseServices;
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
    public class ToRegularWorkerController : BaseController
    {
        //
        // GET: /ToRegularWorker/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        //岗位名称下拉列表
        public List<SelectListItem> JobName()
        {

            var list = db.T_EmployDemandConfig.Where(a => a.Type == "岗位").AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==选择岗位==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "审核人", Value = "审核人" },
            };
            return TypeList;
        }

        public List<SelectListItem> getApproveName(int step)
        {
            try
            {
                string approveName = db.T_ToRegularConfig.FirstOrDefault(a => a.Step == step).Name;
                string[] approveArr = approveName.Split(',');
                List<SelectListItem> approveList = new List<SelectListItem>();
                SelectListItem defaultItem = new SelectListItem { Text = "==请选择==", Value = "" };
                approveList.Add(defaultItem);
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
         [Description("转正申请")]
        public ActionResult ViewToRegularApply()
        {
            ViewData["JobName"] = JobName();
            ViewData["approveList"] = getApproveName(1);
            return View();
        }
         public ActionResult ViewEdit(int ID)
         {
             T_ToRegularWorker model = db.T_ToRegularWorker.Find(ID);
             ViewData["JobName"] = JobName();
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
         [Description("转正未审核")]
         public ActionResult ViewToRegularUncheckList()
        {
        return View();
        }

         [Description("转正列表")]
         public ActionResult ViewToRegularList()
        {
        return View();
        }
         public ActionResult ViewMyList()
         {
             return View();
         }
         [Description("转正配置")]
         public ActionResult ViewToRegularConfig()
        {
            return View();
        }

         [Description("审核详情")]
         public ActionResult ViewCheckDetail(int PID)
        {
            ViewData["PID"] = PID;
            return View();
        }

         public ActionResult ViewCheck(int PID)
         {
             T_ToRegularWorker Wmodel = db.T_ToRegularWorker.Find(PID);
             ViewData["Salary"] = Wmodel.Salary;
            ViewData["ApplyContent"] = Wmodel.ApplyContent;
             T_ToRegularApprove model = db.T_ToRegularApprove.FirstOrDefault(a => a.PID == PID && a.Status == 0);
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
             int configCount = db.T_ToRegularConfig.Count();
             int maxStep = 1;
             if (configCount > 0)
             {
                 maxStep = db.T_ToRegularConfig.Max(a => a.Step) + 1;
             }
             ViewData["boolList"] = Com.BoolList;
             ViewData["TypeList"] = TypeList();
             ViewData["maxStep"] = maxStep;
             return View();
         }

         public ActionResult ViewConfigEdit(int ID)
         {
             T_ToRegularConfig model = db.T_ToRegularConfig.Find(ID);
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

        #region 绑定数据
         public ContentResult GetList(Lib.GridPager pager, string queryStr,int isUncheck=0,int isMy=0)
         {
             string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
             IQueryable<T_ToRegularWorker> queryData = db.T_ToRegularWorker.Where(a => a.isDelete == 0);
             if (isMy == 1)//我的转正申请
             {
                 queryData = queryData.Where(a => a.PostName == name);
             }

             else if (isUncheck == 1)//转正未审核
             {
                 int[] step = (from r in db.T_ToRegularConfig
                               where r.isMultiple == "1" && r.Name.Contains(name)
                               select r.Step).ToArray();
                 queryData = queryData.Where(a => a.CurrentApprove == name || step.Contains(a.Step));
             }
            
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => (a.Job != null && a.Job.Contains(queryStr))||(a.PostName.Contains(queryStr)));
             }
             if (queryData != null)
             {
                 List<T_ToRegularWorker> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                 pager.totalRows = queryData.Count();
                 string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             else
             {
                 return Content("");
             }
         }

         public ContentResult GetCheckDetailList(int PID)
         {
             List<T_ToRegularApprove> list = db.T_ToRegularApprove.Where(a => a.PID == PID).ToList();
             string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
             return Content(json);
         }

         public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
         {

             IQueryable<T_ToRegularConfig> queryData = db.T_ToRegularConfig;
            
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
             }
             List<T_ToRegularConfig> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             pager.totalRows = queryData.Count();
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
             return Content(json);
            
            
         }
        #endregion

        #region 增删改
         public partial class Modular
         {

             public string ModularName { get; set; }
             public int NotauditedNumber { get; set; }
             public string PendingAuditName { get; set; }
         }
         public void ModularByZP()
         {

             List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "转正").ToList();
             if (ModularNotaudited.Count > 0)
             {
                 foreach (var item in ModularNotaudited)
                 {
                     db.T_ModularNotaudited.Remove(item);
                 }
                 db.SaveChanges();
             }
             string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ToRegularApprove where PID in (select ID from T_ToRegularWorker where  Isdelete='0'  and (status = -1 or status = 0) )  and  Status=0 and ApproveTime is null GROUP BY ApproveName";
             List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
             string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
             {
                 string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                 T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "转正" && a.PendingAuditName == PendingAuditName);
                 if (NotauditedModel != null)
                 {
                     NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                     db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                 }
                 else
                 {
                     T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                     ModularNotauditedModel.ModularName = "转正";
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
             string RejectNumberSql = "select PostName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ToRegularWorker where Status='2' and isDelete=0 GROUP BY PostName";
             List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

             for (int e = 0; e < RejectNumberQuery.Count; e++)
             {
                 string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                 T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "转正" && a.PendingAuditName == PendingAuditName);
                 if (NotauditedModel != null)
                 {
                     NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                     db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                 }
                 else
                 {
                     T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                     ModularNotauditedModel.ModularName = "转正";
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


         public JsonResult ToRegularWorkerApplySave(T_ToRegularWorker model)
         {
             try
             {
                 
                 model.PostName = Server.UrlDecode(Request.Cookies["NickName"].Value);
                 model.Status = -1;
                 model.Step = 1;
                 model.isDelete = 0;
                 model.PostTime = DateTime.Now;
                 db.T_ToRegularWorker.Add(model);
                 db.SaveChanges();

                 T_ToRegularApprove approve = new T_ToRegularApprove();
                 approve.Status = 0;
                 approve.ApproveName = model.CurrentApprove;
                 approve.PID = model.ID;
                 approve.Step = 1;
                 db.T_ToRegularApprove.Add(approve);
                 db.SaveChanges();
                 //ModularByZP();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (DbEntityValidationException e)
             {
                 return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
             }
         }

         public JsonResult EditSave(T_ToRegularWorker model)
         {
             using (TransactionScope sc = new TransactionScope())
             {
                 try
                 {
                     T_ToRegularWorker editModel = db.T_ToRegularWorker.Find(model.ID);
                     editModel.ApplyContent = model.ApplyContent;
                     editModel.EndProbationTime = model.EndProbationTime;
                     editModel.Job = model.Job;
                     editModel.ProbationJob = model.ProbationJob;
                     editModel.StartProbationTime = model.StartProbationTime;
                     editModel.EndProbationTime = model.EndProbationTime;
                     editModel.CurrentApprove = model.CurrentApprove;

                    

                     if (editModel.Status == 2)
                     {
                         editModel.Status = -1;
                         editModel.Step = 1;

                         T_ToRegularApprove newApprove = new T_ToRegularApprove();
                         newApprove.ApproveName = model.CurrentApprove;
                         newApprove.PID = model.ID;
                         newApprove.Status = 0;
                         newApprove.Step = 1;
                         db.T_ToRegularApprove.Add(newApprove);
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

         public JsonResult Delete(int ID)
         {
             try
             {
                 T_ToRegularWorker delModel = db.T_ToRegularWorker.Find(ID);
                 delModel.isDelete = 1;
                 db.Entry<T_ToRegularWorker>(delModel).State = System.Data.Entity.EntityState.Modified;
                 db.SaveChanges();
                 //ModularByZP();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
                 return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
             }
         }
         [HttpPost]
         public JsonResult ToRegularCheckSave(T_ToRegularApprove model, int Salary)
         {
             using (TransactionScope sc = new TransactionScope())
             {
                 try
                 {
                     string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                     T_ToRegularApprove editModel = db.T_ToRegularApprove.Find(model.ID);
                     editModel.Status = model.Status;
                     editModel.ApproveTime = DateTime.Now;
                     editModel.ApproveName = name;
                     editModel.Memo = model.Memo;
                     db.Entry<T_ToRegularApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                     T_ToRegularWorker ToRegularModel = db.T_ToRegularWorker.Find(editModel.PID);
                     if (model.Status == 1)//同意
                     {
                         int nextStep = editModel.Step + 1;
                         List<T_ToRegularConfig> ConfigList = db.T_ToRegularConfig.Where(a => a.Step == nextStep).ToList();
                         if (ConfigList.Count > 0)//下一步
                         {
                             ToRegularModel.Status = 0;
                             ToRegularModel.Step = nextStep;
                             ToRegularModel.Salary = Salary;
                             ToRegularModel.CurrentApprove = ConfigList.First().Name;
                             T_ToRegularApprove newApprove = new T_ToRegularApprove();
                             newApprove.ApproveName = ConfigList.First().Name;
                             newApprove.PID = editModel.PID;
                             newApprove.Status = 0;
                             newApprove.Step = nextStep;
                             db.T_ToRegularApprove.Add(newApprove);
                         }
                         else//结束
                         {
                              T_PersonnelFile person =null;
                              try
                              {
                                  person = db.T_PersonnelFile.FirstOrDefault(a => a.NickName == ToRegularModel.PostName && a.IsDelete == 0 && a.OnJob == 0);
                                  person.Salary = Salary;
                                  person.Job = ToRegularModel.Job;
                                  person.isZhuanzheng = 1;
                                  db.Entry<T_PersonnelFile>(person).State = System.Data.Entity.EntityState.Modified;   //修改档案表T_PersonnelFile薪资信息
                              }
                              catch (Exception e)
                              {
                                  return Json(new { State = "Faile", Message ="请给该用户添加人事档案,或者给该用户人事档案完善花名信息"}, JsonRequestBehavior.AllowGet);
                              }

                             ToRegularModel.CurrentApprove = "";
                             ToRegularModel.Status = 1;
                             ToRegularModel.Step = 99;
                             ToRegularModel.Salary = Salary;
                         }
                     }
                     else//不同意结束
                     {
                         ToRegularModel.CurrentApprove = "";
                         ToRegularModel.Status = 2;
                         ToRegularModel.Step = 99;
                     }
                     db.Entry<T_ToRegularWorker>(ToRegularModel).State = System.Data.Entity.EntityState.Modified;
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

        #region 配置
         [HttpPost]
         public JsonResult ConfigAddSave(T_ToRegularConfig model)
         {
             try
             {
                 db.T_ToRegularConfig.Add(model);
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
                 T_ToRegularConfig delModel = db.T_ToRegularConfig.Find(ID);
                 db.T_ToRegularConfig.Remove(delModel);
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
                 return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
             }
         }
         [HttpPost]
         public JsonResult ConfigEditSave(T_ToRegularConfig model)
         {
             try
             {
                 T_ToRegularConfig editModel = db.T_ToRegularConfig.Find(model.ID);
                 editModel.Name = model.Name;
                 editModel.Step = model.Step;
                 editModel.Type = model.Type;
                 editModel.isMultiple = model.isMultiple;
                 db.Entry<T_ToRegularConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
