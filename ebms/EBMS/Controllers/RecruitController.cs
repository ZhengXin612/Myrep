using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Data.Entity.Validation;
using System.Transactions;
using Newtonsoft.Json;
using EBMS.App_Code;

namespace EBMS.Controllers
{
    public class RecruitController : BaseController
    {
        //
        // GET: /Recruit/
        EBMSEntities db = new EBMSEntities();
        #region  其他
        public class EmploymentRegistration
        {
            public T_PersonnelFile perInfo { get; set; }
            public List<T_PersonnelEduBackgroud> EduBackgroud { get; set; }
            public List<T_PersonnelWorkExperience> WorkExperience { get; set; }
            public List<T_PersonnelFamily> Family { get; set; }
        }
        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "面试官", Value = "面试官" },
            };
            return TypeList;
        }
        public List<SelectListItem> ConfigList(string type)
        {
            var list = db.T_EmployDemandConfig.Where(a => a.Type == type);
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> SexList()
        {
            List<SelectListItem> SexList = new List<SelectListItem> {
            new SelectListItem { Text = "男", Value = "男" },
            new SelectListItem { Text = "女", Value = "女" },
            };
            return SexList;
        }
        public List<SelectListItem> MarryStateList()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "已婚", Value = "已婚" },
            new SelectListItem { Text = "未婚", Value = "未婚" },
            };
            return MarryStateList;
        }
        public List<SelectListItem> getApproveName(int step)
        {
            List<SelectListItem> approveList = new List<SelectListItem>();
            approveList.Add(new SelectListItem { Text = "结束", Value = "结束" });
            try
            {
                string approveName = db.T_PersonnelInterviewConfig.FirstOrDefault(a => a.Step == step).Name;
                string[] approveArr = approveName.Split(',');


                foreach (string item in approveArr)
                {
                    SelectListItem selectItem = new SelectListItem();
                    selectItem.Text = item;
                    selectItem.Value = item;
                    approveList.Add(selectItem);
                }
            }
            catch(Exception e)
            {
            
            }

           
            return approveList;
        }
        public List<SelectListItem> InterviewerList(string type,int step)
        {
            var list = db.T_PersonnelInterviewConfig.Where(a => a.Type == type&&a.Step==step);
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "结束", Value = "结束" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> DateType()
        {
            List<SelectListItem> DateType = new List<SelectListItem> {
            new SelectListItem { Text = "==请选择阴历或阳历==", Value = "" },
            new SelectListItem { Text = "阴历", Value = "阴历" },
            new SelectListItem { Text = "阳历", Value = "阳历" },
            };
            return DateType;
        }

        public string getBool(string code)
        {
            if (code == "1")
            {
                return "是";
            }
            else
            {
                return "否";
            }
        }

        #endregion
        public ActionResult Index()
        {
            return View();
        }
        #region 视图
        [Description("应聘登记")]
        [AllowAnonymous]
        public ActionResult ViewEmploymentRegistration()
        {
            ViewData["JobList"] = ConfigList("岗位");
            ViewData["SexList"] = SexList();
            ViewData["BoolList"] = Com.BoolList;
            ViewData["MarryStateList"] = MarryStateList();
            ViewData["DateType"] = DateType();
            return View();
        }
         [Description("简历管理")]
        public ActionResult ViewResumeManageList()
        {
            return View();
        }
         [Description("待面试列表")]
         public ActionResult ViewWaitInterviewList()
         {
             return View();
         }
         public ActionResult ViewWaitInterviewCheck(int PID)
         {
             string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            
             T_PersonnelInterviewRecord model = db.T_PersonnelInterviewRecord.FirstOrDefault(a => a.PID == PID &&a.State==0);

             ViewData["NextInterviewer"] = getApproveName((int)model.Step + 1);
             return View(model);
         }
         public ActionResult ViewEmployeeDemandList(int ID)
         {
             ViewData["ID"] = ID;
             return View();
         }
          [Description("面试配置")]
         public ActionResult ViewInterviewConfig()
         {
             return View();
         }
          [Description("面试配置新增")]
         public ActionResult ViewInterviewConfigAdd()
         {
             int configCount = db.T_PersonnelInterviewConfig.Count();
              int maxStep =1;
             if (configCount > 0)
             {
                 maxStep = db.T_PersonnelInterviewConfig.Max(a => a.Step) + 1;
             }
             ViewData["boolList"] = Com.BoolList;
             ViewData["ListUser"] = Com.UserList();
             ViewData["TypeList"] = TypeList();
             ViewData["maxStep"] = maxStep;
             return View();
         }
          public ActionResult ViewInterviewConfigEdit(int ID)
          {
              ViewData["boolList"] = Com.BoolList;
              ViewData["ListUser"] = Com.UserList();
              T_PersonnelInterviewConfig model = db.T_PersonnelInterviewConfig.Find(ID);
              if (model != null)
              {
                  return View(model);
              }
              else
              {
                  return HttpNotFound();
              }
          }

          public ActionResult ViewInterviewCheckDetail(int DID)
          {
              ViewData["DID"] = DID;
              EmploymentRegistration model = new EmploymentRegistration();
              model.perInfo=db.T_PersonnelFile.Find(DID);
              model.perInfo.CanBusinessTravel = getBool(model.perInfo.CanBusinessTravel);
              model.EduBackgroud = db.T_PersonnelEduBackgroud.Where(a => a.PID == DID).ToList();
              model.Family = db.T_PersonnelFamily.Where(a => a.Pid == DID).ToList();
              model.WorkExperience = db.T_PersonnelWorkExperience.Where(a => a.PID == DID).ToList();
              if (model != null)
              {
                  return View(model);
              }
              else
              {
                  return View(model);
              }
          }

        #endregion
        #region  绑定数据
         public ContentResult GetInterviewList(Lib.GridPager pager, string queryStr,int isWait=0,int isManage=0)
         { 
             string name=Server.UrlDecode(Request.Cookies["NickName"].Value);
             IQueryable<T_PersonnelFile> queryData = db.T_PersonnelFile;
             if (isWait == 1)//待面试列表数据
             {
                 int[] step = (from r in db.T_PersonnelInterviewConfig
                               where r.isMultiple == "1" && r.Name.Contains(name)
                               select r.Step).ToArray();
                 queryData = queryData.Where(a => (a.InterviewState == -1 || a.InterviewState == 0) && (a.CurrentInterviewer == name));
                 
             }
             else if (isManage ==1)//简历管理数据
             {
                 queryData = queryData.Where(a => a.DemandID==null);
             }
             if(!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.TrueName != null && a.TrueName.Contains(queryStr));
             }
             if (queryData != null)
             {
                 List<T_PersonnelFile> list = queryData.OrderBy(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                 pager.totalRows = queryData.Count();
                 string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             else
             {
                 return Content("");
             }
         }
         public ContentResult GetEmployeeDemandList(Lib.GridPager pager, string queryStr)
         {
             string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
             IQueryable<T_EmployDemand> queryData = db.T_EmployDemand.Where(a => a.IsDelete == "0"&&a.State==1&&a.DistributionNum<a.PeopleNum);
             
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.Job != null && a.Job.Contains(queryStr));
             }
             if (queryData != null)
             {
                 List<T_EmployDemand> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                 pager.totalRows = queryData.Count();
                 string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             else
             {
                 return Content("");
             }
         }

         public ContentResult GetInterviewConfigList(Lib.GridPager pager, string queryStr)
         {
             IQueryable<T_PersonnelInterviewConfig> queryData = db.T_PersonnelInterviewConfig;
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.Name != null && a.Name.Contains(queryStr));
             }
             List<T_PersonnelInterviewConfig> list = queryData.OrderBy(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             pager.totalRows = queryData.Count();
             string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
             return Content(json);
         }

         public ContentResult GetInterviewCheckList(int DID)
         {
             List<T_PersonnelInterviewRecord> list = db.T_PersonnelInterviewRecord.Where(a => a.PID == DID).ToList();
             string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
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
             List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "招聘").ToList();
             if (ModularNotaudited.Count > 0)
             {
                 foreach (var item in ModularNotaudited)
                 {
                     db.T_ModularNotaudited.Remove(item);
                 }
                 db.SaveChanges();
             }
             string RetreatAppRoveSql = "select Interviewer as PendingAuditName,COUNT(*) as NotauditedNumber from T_PersonnelInterviewRecord where PID in (select ID from T_PersonnelFile where  Isdelete='0'  and (InterviewState = -1 or InterviewState = 0) )  and  State=0 and Date is null GROUP BY Interviewer";
             List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            // string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
             {
                 string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                 T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "招聘" && a.PendingAuditName == PendingAuditName);
                 if (NotauditedModel != null)
                 {
                     NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                     db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                 }
                 else
                 {
                     T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                     ModularNotauditedModel.ModularName = "招聘";
                     ModularNotauditedModel.RejectNumber = 0;
                     ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                     ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                     ModularNotauditedModel.ToupdateDate = DateTime.Now;
                     ModularNotauditedModel.ToupdateName = "admin";
                     db.T_ModularNotaudited.Add(ModularNotauditedModel);
                 }
                 db.SaveChanges();
             }
           //招聘部分没有
         }
         [AllowAnonymous]
         [HttpPost]
         public JsonResult ResumeAddSave(EmploymentRegistration model)
         {
             using(TransactionScope sc=new TransactionScope())
             {
                 try
                 {
                     T_PersonnelFile PerFile = model.perInfo;
                     PerFile.IsDelete = 0;
                     PerFile.ApplyDate = DateTime.Now;
                     PerFile.Code = "";
                     PerFile.NickName = "";
                     PerFile.OnJob = -1;
                     PerFile.online = "电子商务部";
                     PerFile.InterviewState = -1;
                     PerFile.InterviewStep = 1;
                     PerFile.isZhuanzheng = 0;
                     PerFile.CurrentInterviewer = db.T_PersonnelInterviewConfig.FirstOrDefault(a => a.Step == 1 && a.Type == "面试官").Name;
                     db.T_PersonnelFile.Add(PerFile);
                     db.SaveChanges();
                     T_PersonnelInterviewRecord InterviewRecord = new T_PersonnelInterviewRecord();
                     InterviewRecord.Interviewer = PerFile.CurrentInterviewer;
                     InterviewRecord.PID = PerFile.ID;
                     InterviewRecord.State = 0;
                     InterviewRecord.Memo = "";
                     InterviewRecord.Step = 1;
                     db.T_PersonnelInterviewRecord.Add(InterviewRecord);
                     db.SaveChanges();
                     foreach (var EduBackgroud in model.EduBackgroud)
                     {
                         if (!string.IsNullOrWhiteSpace(EduBackgroud.School))
                         {
                             EduBackgroud.PID = PerFile.ID;
                             db.T_PersonnelEduBackgroud.Add(EduBackgroud);
                         }
                     }
                     foreach (var WorkExperience in model.WorkExperience)
                     {
                         if (!string.IsNullOrWhiteSpace(WorkExperience.Job))
                         {
                             WorkExperience.PID = PerFile.ID;
                             db.T_PersonnelWorkExperience.Add(WorkExperience);
                         }
                     }
                     foreach (var Family in model.Family)
                     {
                         if (!string.IsNullOrWhiteSpace(Family.Name))
                         {
                             Family.Pid = PerFile.ID;
                             db.T_PersonnelFamily.Add(Family);
                         }
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
             
            
         }//简历新增
         public JsonResult InterviewEditSave(T_PersonnelInterviewRecord model,string NextInterviewer)
         {
             using (TransactionScope sc = new TransactionScope())
             {
                 try
                 {
                     string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                     T_PersonnelInterviewRecord editModel = db.T_PersonnelInterviewRecord.Find(model.ID);
                     editModel.State = model.State;
                     editModel.Memo = model.Memo;
                     editModel.Interviewer = name;
                     editModel.Date = DateTime.Now;
                     db.Entry<T_PersonnelInterviewRecord>(editModel).State = System.Data.Entity.EntityState.Modified;

                     T_PersonnelFile perModel = db.T_PersonnelFile.Find(editModel.PID);

                     if (model.State == 1)
                     {
                         if (NextInterviewer != "结束")//进入下一轮面试
                         {
                             T_PersonnelInterviewRecord newRecord = new T_PersonnelInterviewRecord();
                             newRecord.Interviewer = NextInterviewer;
                             newRecord.Memo = "";
                             newRecord.PID = editModel.PID;
                             newRecord.State = 0;
                             newRecord.Step = editModel.Step + 1;
                             db.T_PersonnelInterviewRecord.Add(newRecord);

                             perModel.CurrentInterviewer = NextInterviewer;
                             perModel.InterviewStep = newRecord.Step;
                             perModel.InterviewState = 0;
                         }
                         else//面试通过
                         {
                             perModel.CurrentInterviewer = "";
                             perModel.InterviewStep = 99;
                             perModel.InterviewState = 1;
                             //发送短信
                             //string[] msg = { "恭喜您通过面试" };
                             //Lib.SendSMS.Send(msg, "15608486578");
                         }
                     }
                     else//面试不通过
                     {
                         perModel.CurrentInterviewer = "";
                         perModel.InterviewStep = 99;
                         perModel.InterviewState = 2;
                        
                     }
                     db.Entry<T_PersonnelFile>(perModel).State = System.Data.Entity.EntityState.Modified;
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
         public JsonResult ResumeDistribute(int DID,int RID)//简历分配
         {
             using (TransactionScope sc = new TransactionScope())
             {
                 try
                 {
                     T_PersonnelFile pModel = db.T_PersonnelFile.Find(RID);
                     T_EmployDemand dModel = db.T_EmployDemand.Find(DID);
                     dModel.DistributionNum += 1;//已分配数量+1
                     pModel.DemandID = DID;//档案存入分配需求ID
                     
                     db.Entry<T_PersonnelFile>(pModel).State = System.Data.Entity.EntityState.Modified;
                     db.Entry<T_EmployDemand>(dModel).State = System.Data.Entity.EntityState.Modified;
                     db.SaveChanges();
                     sc.Complete();
                     return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                 }
                 catch (Exception e)
                 {
                     return Json(new { State = "Faile", Message =e.Message}, JsonRequestBehavior.AllowGet);
                 }
             }

         }
        
         public JsonResult InterviewConfigAddSave(T_PersonnelInterviewConfig model)//面试配置新增
         {
            // model.Type = "面试官";
             db.T_PersonnelInterviewConfig.Add(model);
             try
             {
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (DbEntityValidationException e)
             {
                 return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
             }
         }
         public JsonResult InterviewConfigEditSave(T_PersonnelInterviewConfig model)//面试配置编辑
         {
             T_PersonnelInterviewConfig editModel = db.T_PersonnelInterviewConfig.Find(model.ID);
             editModel.Step = model.Step;
             editModel.Name = model.Name;
             editModel.isMultiple = model.isMultiple;
             db.Entry<T_PersonnelInterviewConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
             try
             {
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (DbEntityValidationException e)
             {
                 return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
             }
         }
        //面试配置删除
         public JsonResult InterviewConfigDelete(int ID)
         {
             T_PersonnelInterviewConfig delModel = db.T_PersonnelInterviewConfig.Find(ID);
             if (delModel != null)
             {
                 db.T_PersonnelInterviewConfig.Remove(delModel);
                 int i=db.SaveChanges();
                 return Json(i);
             }
             else
             {
                 return Json(-1);
             }
         }
        #endregion
    }
}
