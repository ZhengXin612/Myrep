using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Data.Entity.Validation;
using Newtonsoft.Json;
using EBMS.App_Code;
namespace EBMS.Controllers
{
    public class EmployeeDemandController : BaseController
    {
        //
        // GET: /EmployeeDemand/
        //用人需求
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region  其他
        //获取数据作为下拉列表（审核专员  岗位...）
        public List<SelectListItem> ConfigList(string type)
        {
            var list = db.T_EmployDemandConfig.Where(a=>a.Type==type);
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        //获取部门数据作为下拉列表
        public List<SelectListItem> DepartMentList()
        {
            EBMSEntities db = new EBMSEntities();
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            string deptID =db.T_User.First(a => a.Nickname == name).DepartmentId;
            int dID = Convert.ToInt32(deptID);
            string deptName = db.T_Department.Find(dID).Name;
            var list = db.T_Department.Where(a=>a.parentId>0);
            var selectList = new SelectList(list, "Name", "Name", deptName);
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        /// <summary>
        /// 获得某步骤的审核人下拉列表
        /// </summary>
        ///  <param name="step"></param>
        /// <returns></returns>
        public List<SelectListItem> getApproveName(int step)
        {
            try
            {
                string approveName = db.T_EmployDemandConfig.FirstOrDefault(a => a.Step == step).Name;
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

        //配置类型下拉列表
        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
                 new SelectListItem { Text = "审核人", Value = "审核人" },
                 new SelectListItem { Text = "招聘专员", Value = "招聘专员" },
                 new SelectListItem { Text = "岗位", Value = "岗位" },
            };
            return TypeList;
        }
        #endregion
        #region  视图
        [Description("新增用人需求")]
        public ActionResult ViewEmployeeDemandAdd()
        {
            ViewData["JobList"] = ConfigList("岗位");
            ViewData["approveList"] = getApproveName(1);
            ViewData["deptList"] = DepartMentList();
            return View();
        }
        [Description("我的用人需求")]
        public ActionResult ViewEmployeeDemandMyList()
        {
            return View();
        }
        [Description("用人需求列表")]
        public ActionResult ViewEmployeeDemandList()
        {
            return View();
        }
        [Description("用人需求未审核")]
        public ActionResult ViewEmployeeDemandCheckList()
        {
            return View();
        }
         [Description("用人需求已审核")]
        public ActionResult ViewEmployeeDemandCheckedList()
        {
            return View();
        }
          [Description("用人需求编辑")]
         public ActionResult ViewEmployeeDemandEdit(int ID)
         {
             ViewData["JobList"] = ConfigList("岗位");
             T_EmployDemand model = db.T_EmployDemand.Find(ID);
             if (model != null)
             {
                 return View(model);
             }
             else
             {
                 return HttpNotFound();
             }
         }
         [Description("用人需求审核")]
          public ActionResult ViewEmployeeDemandCheck(int DID)
          {
              string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
              T_EmployDemandApprove model = db.T_EmployDemandApprove.SingleOrDefault(a => a.DemandID == DID && a.ApproveUser == name && a.ApproveState == 0);
              ViewData["recruitList"] = ConfigList("招聘专员");
              return View(model);
          }
        [Description("用人需求审核详情")]
         public ActionResult ViewEmployeeDemandCheckDetail(int DID)
         {
             ViewData["DID"] = DID;
             return View();
         }
         [Description("用人需求配置")]
        public ActionResult ViewEmployeeDemandConfig()
        {
            return View();
         }
         [Description("用人需求配置新增")]
         public ActionResult ViewEmployeeDemandConfigAdd()
         {
             ViewData["boolList"] = Com.BoolList;
             ViewData["TypeList"] = TypeList();
             int configCount = db.T_PersonnelTransferConfig.Count();
             int maxStep = 1;
             if (configCount > 0)
             {
                 maxStep = db.T_PersonnelTransferConfig.Max(a => a.Step) + 1;
             }
             ViewData["maxStep"] = maxStep;
             return View();
         }
         [Description("用人需求配置编辑")]
         public ActionResult ViewEmployeeDemandConfigEdit(int ID)
         {
             T_EmployDemandConfig model = db.T_EmployDemandConfig.Find(ID);
             ViewData["boolList"] = Com.BoolList;
             ViewData["TypeList"] = TypeList();
             if (model == null)
             {
                 return HttpNotFound();
             }
             else
             {
                 return View(model);
             }
         }
        #endregion
        #region  绑定数据
        //获取用人需求列表
        public ContentResult GetEmployeeDemandList(Lib.GridPager pager, string queryStr, int isMy = 0, int isSearch = 0, int isCheck=0)
         {
             string name=Server.UrlDecode(Request.Cookies["NickName"].Value);
             IQueryable<T_EmployDemand> queryData = db.T_EmployDemand.Where(a=>a.IsDelete=="0");
             if (isMy == 1)//我的用人需求
             {
                 queryData = queryData.Where(a => a.DemandUser == name);
             }
             else  if (isSearch == 1)//用人需求列表
             {
                 queryData = queryData.Where(a => a.State == 1);
                 string[] recruiters = (from DemandConfig in db.T_EmployDemandConfig where DemandConfig.Type == "招聘专员" select DemandConfig.Name).ToArray();
                 if (recruiters.Contains(name))
                 {
                     queryData = queryData.Where(a => a.Recruiter == name);//招聘专员看到指定的数据
                 }
             }
             else if (isCheck == 1)//未审核需求列表
             {
                 queryData = queryData.Where(a => a.CurrentApproveName == name);
             }
             else if (isCheck == 2)//已审核需求列表
             {
                 int step = db.T_EmployDemandConfig.FirstOrDefault(a => a.Name == name && a.Type == "审核人").Step;
                 queryData = queryData.Where(a => a.Step > step);
             }
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
        //获取审核详情  
        public ContentResult GetEmployeeDemandCheckList(int DID)
          {
              List<T_EmployDemandApprove> list = db.T_EmployDemandApprove.Where(a => a.DemandID == DID).ToList();
              string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
              return Content(json);
          }
        //获取用人需求配置列表
        public ContentResult GetEmployeeDemandConfigList(Lib.GridPager pager, string queryStr)
          {
              IQueryable<T_EmployDemandConfig> queryData = db.T_EmployDemandConfig;
              if (!string.IsNullOrWhiteSpace(queryStr))
              {
                  queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
              }
              if (queryData != null)
              {
                  pager.totalRows = queryData.Count();
                  List<T_EmployDemandConfig> list = queryData.OrderBy(a => a.ID).OrderBy(a => a.Type).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                  string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                  return Content(json);
              }
              else
              {
              return Content("");
              }
          }
        #endregion


        #region 增删改
        #region  用人需求及其审核

        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }

        public void ModularByZP()
        {

            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "用人需求").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = "select ApproveUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_EmployDemandApprove where DemandID in (select ID from T_EmployDemand where  Isdelete='0'  and (State = -1 or State = 0) )  and  ApproveState=0 and ApproveDate is null GROUP BY ApproveUser";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "用人需求" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "用人需求";
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
            string RejectNumberSql = "select DemandUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_EmployDemand where State='2' and IsDelete=0  GROUP BY DemandUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "用人需求" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "用人需求";
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




        public JsonResult EmployeeDemandAddSave(T_EmployDemand model)// 用人需求新增保存
         {
             try
             {
                 //model.CurrentApproveName = db.T_EmployDemandConfig.FirstOrDefault(a => a.Step == 1 && a.Type == "审核人").Name;
                 model.DemandUser = Server.UrlDecode(Request.Cookies["NickName"].Value);
                 model.DistributionNum = 0;
                 model.PerMemo = "";
                 model.State = -1;
                 model.Step = 1;
                 model.IsDelete = "0";
                 model.Date = DateTime.Now;
                 db.T_EmployDemand.Add(model);
                 db.SaveChanges();

                 T_EmployDemandApprove approve = new T_EmployDemandApprove();
                 approve.ApproveState = 0;
                 approve.ApproveUser = model.CurrentApproveName;
                 approve.DemandID = model.ID;
                 approve.Step = 1;
                 db.T_EmployDemandApprove.Add(approve);
                 db.SaveChanges();
                 //ModularByZP();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (DbEntityValidationException e)
             {
                 return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
             }
         }
         public JsonResult EmployeeDemandEditSave(T_EmployDemand model)//用人需求编辑保存
         {
             T_EmployDemand editModel = db.T_EmployDemand.Find(model.ID);
             if (editModel != null)
             {
                 editModel.Job = model.Job;
                 editModel.PeopleNum = model.PeopleNum;
                 editModel.RecommendSalary = model.RecommendSalary;
                 editModel.Memo = model.Memo;
                 editModel.CurrentApproveName = db.T_EmployDemandConfig.FirstOrDefault(a => a.Step == 1 && a.Type == "审核人").Name;
                 editModel.Step = 1;
                 editModel.Date = DateTime.Now;
                 if (editModel.State == 2)
                 {
                     editModel.State = 0;
                     T_EmployDemandApprove approve = new T_EmployDemandApprove();
                     approve.ApproveState = 0;
                     approve.ApproveUser = editModel.CurrentApproveName;
                     approve.DemandID = model.ID;
                     approve.Step = 1;
                     db.T_EmployDemandApprove.Add(approve);
                 }
                 db.Entry<T_EmployDemand>(editModel).State = System.Data.Entity.EntityState.Modified;
                 db.SaveChanges();
                // ModularByZP();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             else
             {
                 return Json(new { State = "Faile", Message ="无相关记录" }, JsonRequestBehavior.AllowGet);
             }
         }
         public JsonResult EmployeeDemandDelete(int ID)//用人需求删除保存
         { 
          T_EmployDemand editModel = db.T_EmployDemand.Find(ID);
          if (editModel != null)
          {
              editModel.IsDelete = "1";
              db.Entry<T_EmployDemand>(editModel).State = System.Data.Entity.EntityState.Modified;
              int i = db.SaveChanges();
              //ModularByZP();
              return Json(i);
          }
          else
          {
              return Json(-1);
          }
         }
         public JsonResult EmployeeDemandVoid(int ID)//作废
         {
             T_EmployDemand editModel = db.T_EmployDemand.Find(ID);
             if (editModel != null)
             {
                 editModel.State = 3;
                 editModel.CurrentApproveName = "";
                
                 db.Entry<T_EmployDemand>(editModel).State = System.Data.Entity.EntityState.Modified;
                 int i = db.SaveChanges();
                 //ModularByZP();
                 return Json(i);
             }
             else
             {
                 return Json(-1);
             }
         }
         public JsonResult EmployeeDemandCheckSave(T_EmployDemandApprove model, string recruit)//用人需求审核
         {
             try
             {
                 string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                 T_EmployDemandApprove editApprove = db.T_EmployDemandApprove.Find(model.ID);
                 T_EmployDemand editDemand = db.T_EmployDemand.Find(editApprove.DemandID);
                 editApprove.ApproveState = model.ApproveState;
                 editApprove.ApproveMemo = model.ApproveMemo;
                 editApprove.ApproveDate = DateTime.Now;
                 editApprove.ApproveUser = name;

                 if (editApprove.ApproveState == 1)//同意
                 {
                     int nextStep = editDemand.Step + 1;
                     List<T_EmployDemandConfig> ConfigList = db.T_EmployDemandConfig.Where(a => a.Step == nextStep).ToList();
                     if (ConfigList.Count > 0)//下一步
                     {
                         T_EmployDemandApprove newApprove = new T_EmployDemandApprove();
                         newApprove.ApproveState = 0;
                         newApprove.DemandID = editApprove.DemandID;
                         newApprove.Step = nextStep;
                         newApprove.ApproveUser =ConfigList.First().Name;
                         db.T_EmployDemandApprove.Add(newApprove);
                         if (editDemand.Step == 1)
                         {
                             editDemand.PerMemo = model.ApproveMemo;//人事备注
                         }
                         else if (editDemand.Step == 2)
                         {
                             editDemand.Recruiter = recruit;//指定招聘专员
                         }
                         editDemand.Step = nextStep;
                         editDemand.State = 0;//审核中
                         editDemand.CurrentApproveName = ConfigList.First().Name;
                     }
                     else//结束
                     {
                          if (editDemand.Step == 2)
                         {
                             editDemand.Recruiter = recruit;//指定招聘专员
                         }
                         editDemand.State = 1;
                         editDemand.Step = 99;
                         editDemand.CurrentApproveName = "";
                     }
                 }
                 else
                 {
                     editDemand.Step = 99;
                     editDemand.State = 2;//不同意
                     editDemand.CurrentApproveName = "";
                 }
                 db.Entry<T_EmployDemand>(editDemand).State = System.Data.Entity.EntityState.Modified;
                 db.Entry<T_EmployDemandApprove>(editApprove).State = System.Data.Entity.EntityState.Modified;
                 db.SaveChanges();
                 //ModularByZP();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception e)
             {
                 return Json(new { State = "Faile", Message = e.Message}, JsonRequestBehavior.AllowGet);
             }
         }
        #endregion
        #region  用人需求配置
         public JsonResult EmployeeDemandConfigAddSave(T_EmployDemandConfig model)
         {
             try
             {
                 //model.Step = -1;
                 db.T_EmployDemandConfig.Add(model);
                 db.SaveChanges();
                 return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
             }
             catch (DbEntityValidationException e)
             {
                 return Json(new { State = "Faile", Message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
             }
         }
         public JsonResult EmployeeDemandConfigEditSave(T_EmployDemandConfig model)
         {
             T_EmployDemandConfig editModel = db.T_EmployDemandConfig.Find(model.ID);
             if (editModel != null)
             {
                 editModel.Name = model.Name;
                 editModel.Step = model.Step;
                 editModel.isMultiple = model.isMultiple;
                 editModel.Type = model.Type;
                 db.Entry<T_EmployDemandConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
             else
             { 
                 return Json(new { State = "Faile", Message = "保存失败请重试" }, JsonRequestBehavior.AllowGet);
             }
         }
         public JsonResult EmployeeDemandConfigDelete(int ID)
         {
             T_EmployDemandConfig delModel = db.T_EmployDemandConfig.Find(ID);
             if (delModel == null)
             {
                 return Json(-1);
             }
             else
             {
                 db.T_EmployDemandConfig.Remove(delModel);
                int i= db.SaveChanges();
                return Json(i);
             }
         }
        #endregion
        #endregion
    }
}
