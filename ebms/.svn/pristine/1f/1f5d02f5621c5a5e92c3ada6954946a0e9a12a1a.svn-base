using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Transactions;
using Newtonsoft.Json;
using System.EnterpriseServices;
using EBMS.App_Code;
namespace EBMS.Controllers
{
    public class PersonnelTransferController : BaseController
    {
        //
        // GET: /PersonnelTransfer/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        public List<SelectListItem> TransferType()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "线上调岗", Value = "线上调岗" },
            new SelectListItem { Text = "调线下", Value = "调线下" },
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
                string approveName = db.T_PersonnelTransferConfig.FirstOrDefault(a => a.Step == step).Name;
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
                List<SelectListItem> approveList = new List<SelectListItem>();
                if (step > 1)
                {
                    SelectListItem select = new SelectListItem { Text = "结束", Value = "" };
                    approveList.Add(select);
                }
                else
                {
                    SelectListItem select = new SelectListItem { Text = "请联系人事配置审核流程", Value = "" };
                    approveList.Add(select);
                }
              
                return approveList;
            }
           
        }
        #endregion
        #region 视图
        [Description("转岗申请")]
        public ActionResult ViewPersonnelTransferApply()
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            T_PersonnelFile pModel = db.T_PersonnelFile.FirstOrDefault(a => a.NickName == name && a.IsDelete == 0 && a.OnJob == 0);
            T_PersonnelTransfer model = new T_PersonnelTransfer();
            ViewData["approveList"] = getApproveName(1);
            ViewData["TransferType"] = TransferType();
            ViewData["transferJob"] = ConfigList("岗位");
            if (pModel != null)
            {
               
                model.Department = pModel.Department;
                model.Job = pModel.Job;
                model.Pid = pModel.ID;
                model.PostUser = name;
                return View(model);
            }
            else 
            {
                model.Department = "您没有档案信息,无法申请转岗,请联系人事";
                return View(model);
            }
           
        }
        [Description("转岗未审核")]
        public ActionResult ViewPersonnelTransferUncheck()
        {
            return View();
        }

        public ActionResult ViewCheck(int PTID)
        {
            T_PersonnelTransferApprove model = db.T_PersonnelTransferApprove.FirstOrDefault(a => a.PTID == PTID && a.Status == 0);
           
            if (model != null)
            {
                ViewData["approveList"] = getApproveName(model.Step + 1);
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
         [Description("转岗列表")]
        public ActionResult ViewPersonnelTransferList()
        {
            return View();
        }
        [Description("我的转岗")]
         public ActionResult ViewPersonnelTransferMyList()
         {
             return View();
         }
        public ActionResult ViewEdit(int ID)
        {
            T_PersonnelTransfer model = db.T_PersonnelTransfer.Find(ID);
            ViewData["transferJob"] = ConfigList("岗位");
            ViewData["approveList"] = getApproveName(1);
            ViewData["TransferType"] = TransferType();
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("转岗配置")]
        public ActionResult ViewPersonnelTransferConfig()
        {
            return View();
        }

        public ActionResult ViewConfigAdd()
        {
            int configCount = db.T_PersonnelTransferConfig.Count();
            int maxStep = 1;
            if (configCount > 0)
            {
                maxStep = db.T_PersonnelTransferConfig.Max(a => a.Step) + 1;
            }
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            ViewData["maxStep"] = maxStep;
            return View();
        }

        public ActionResult ViewConfigEdit(int ID)
        {
            T_PersonnelTransferConfig model = db.T_PersonnelTransferConfig.Find(ID);
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

        public ActionResult ViewCheckDetail(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("转岗已审核")]
        public ActionResult ViewPersonnelTransferChecked()
        {
            return View();
        }
        #endregion

        #region 绑定数据
        public ContentResult GetUncheckList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
          
            IQueryable<T_PersonnelTransfer> queryData = db.T_PersonnelTransfer.Where(a => a.IsDelete == 0 && (a.Status == -1 || a.Status == 0));
            int[] step = (from r in db.T_PersonnelTransferConfig
                          where r.isMultiple == "1" && r.Name.Contains(name)
                          select r.Step).ToArray();
            queryData = queryData.Where(a => a.CurrentApprove == name || step.Contains(a.Step));
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a=>a.PostUser!=null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
                //分页
            List<T_PersonnelTransfer> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetTransferList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_PersonnelTransfer> queryData = db.T_PersonnelTransfer.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a=>a.PostUser!=null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelTransfer> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetTransferMyList(Lib.GridPager pager, string queryStr)
        {
            string name=Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_PersonnelTransfer> queryData = db.T_PersonnelTransfer.Where(a => a.IsDelete == 0 && a.PostUser == name);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelTransfer> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetTransferConfigList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_PersonnelTransferConfig> queryData = db.T_PersonnelTransferConfig;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelTransferConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetCheckedList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            int[] IDs = (from r in db.T_PersonnelTransferApprove
                          where r.ApproveName==name && r.Status>0
                          select r.PTID).ToArray();
            IQueryable<T_PersonnelTransfer> queryData = db.T_PersonnelTransfer.Where(a => IDs.Contains(a.ID));
           
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelTransfer> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetCheckDetailList(int ID)
        {
            List<T_PersonnelTransferApprove> list = db.T_PersonnelTransferApprove.Where(a => a.PTID == ID).ToList();
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


            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "转岗").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_PersonnelTransferApprove where PTID in (select ID from T_PersonnelTransfer where  Isdelete='0'  and (Status = -1 or Status = 0) )  and  Status=0 and ApproveDate is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "转岗" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "转岗";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_PersonnelTransfer where Status='2' and IsDelete=0 GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "转岗" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "转岗";
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

        [HttpPost]
        public JsonResult PersonnelTransferApplySave(T_PersonnelTransfer model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    //string approveName = db.T_PersonnelTransferConfig.First(a => a.Step == 1).Name;
                    model.IsDelete = 0;
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 1;
                    //model.CurrentApprove = approveName;
                    db.T_PersonnelTransfer.Add(model);
                    db.SaveChanges();

                    T_PersonnelTransferApprove newApprove = new T_PersonnelTransferApprove();
                    newApprove.ApproveName = model.CurrentApprove;
                    newApprove.PTID = model.ID;
                    newApprove.Status = 0;
                    newApprove.Step = 1;
                    db.T_PersonnelTransferApprove.Add(newApprove);
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
        public JsonResult TransferCheckSave(T_PersonnelTransferApprove model, string nextApprove)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_PersonnelTransferApprove editModel = db.T_PersonnelTransferApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveDate = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_PersonnelTransferApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_PersonnelTransfer TransferModel = db.T_PersonnelTransfer.Find(editModel.PTID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_PersonnelTransferConfig> ConfigList = db.T_PersonnelTransferConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            TransferModel.Status = 0;
                            TransferModel.Step = nextStep;
                            TransferModel.CurrentApprove = nextApprove;

                            T_PersonnelTransferApprove newApprove = new T_PersonnelTransferApprove();
                            if (nextApprove != "")
                            {
                                newApprove.ApproveName = nextApprove;
                            }
                            else
                            {
                                newApprove.ApproveName = ConfigList.First().Name;
                            }
                            
                            newApprove.PTID = editModel.PTID;
                            newApprove.Status = 0;
                            newApprove.Step = nextStep;
                            db.T_PersonnelTransferApprove.Add(newApprove);
                        }
                        else//结束
                        {
                            T_PersonnelFile person = db.T_PersonnelFile.Find(TransferModel.Pid);
                            person.Job = TransferModel.TransJob;
                            person.Department = TransferModel.TransDepartment;
                            //还需要修改user的departmentID信息
                            TransferModel.Status = 1;
                            TransferModel.Step = 99;
                            TransferModel.TransferDate = DateTime.Now;
                        }
                    }
                    else//不同意结束
                    {
                        TransferModel.Status = 2;
                        TransferModel.Step = 99;
                    }
                    db.Entry<T_PersonnelTransfer>(TransferModel).State = System.Data.Entity.EntityState.Modified;
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
        public JsonResult PersonnelTransferEditSave(T_PersonnelTransfer model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_PersonnelTransfer editModel = db.T_PersonnelTransfer.Find(model.ID);
                    editModel.TransDepartment = model.TransDepartment;
                    editModel.TransferReason = model.TransferReason;
                    editModel.TransJob = model.TransJob;
                    editModel.Type = model.Type;
                    editModel.CurrentApprove = model.CurrentApprove;
                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;
                       
                        T_PersonnelTransferApprove newApprove = new T_PersonnelTransferApprove();
                        newApprove.ApproveName = db.T_PersonnelTransferApprove.FirstOrDefault(a => a.PTID == editModel.ID && a.Step == 1).ApproveName; //db.T_PersonnelTransferConfig.First(a => a.Step == editModel.Step).Name;
                        newApprove.PTID = model.ID;
                        newApprove.Status = 0;
                        newApprove.Step = 1;
                        db.T_PersonnelTransferApprove.Add(newApprove);
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
        public JsonResult DeleteTransfer(int ID)
        {
            try
            {
                T_PersonnelTransfer editModel = db.T_PersonnelTransfer.Find(ID);
                editModel.IsDelete = 1;
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
        public JsonResult ConfigAddSave(T_PersonnelTransferConfig model)
        {
            try
            {
                db.T_PersonnelTransferConfig.Add(model);
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
                T_PersonnelTransferConfig delModel = db.T_PersonnelTransferConfig.Find(ID);
                db.T_PersonnelTransferConfig.Remove(delModel);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ConfigEditSave(T_PersonnelTransferConfig model)
        {
            try
            {
                T_PersonnelTransferConfig editModel = db.T_PersonnelTransferConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;
                editModel.isMultiple = model.isMultiple;
                db.Entry<T_PersonnelTransferConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
