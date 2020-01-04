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
    public class PersonnelPayRaiseController : BaseController
    {
        //
        // GET: /PersonnelPayRaise/
        EBMSEntities db=new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }

        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }
        #region 其他
        //配置类型下拉列表
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
                string approveName = db.T_PersonnelPayRaiseConfig.FirstOrDefault(a => a.Step == step).Name;
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
        [Description("调薪申请")]
        public ActionResult ViewPayRaiseApply()
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            T_User uModel = db.T_User.FirstOrDefault(a => a.Nickname == name);
            ViewData["approveList"] = getApproveName(1);
            //string deptID=uModel.DepartmentId;
            //string ApproveName = db.T_User.FirstOrDefault(a => a.DepartmentId == deptID).Nickname;
            T_PersonnelFile pModel = db.T_PersonnelFile.FirstOrDefault(a => a.NickName == name && a.IsDelete == 0 && a.OnJob == 0);
            T_PersonnelPayRaise model = new T_PersonnelPayRaise();
            if (pModel != null)
            {
                model.PFID = pModel.ID;
                model.PostUser = name;
                //model.CurrentApproveName = ApproveName;
                return View(model);
            }
            else
            {
                model.Reason = "您没有档案信息,无法申请调薪,请联系人事";
                return View(model);
            }
          
        }

        public ActionResult ViewEdit(int ID)
        {
            T_PersonnelPayRaise model = db.T_PersonnelPayRaise.Find(ID);
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
            T_PersonnelPayRaiseApprove model = db.T_PersonnelPayRaiseApprove.FirstOrDefault(a => a.PID == PID && a.Status == 0);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Description("调薪配置")]
        public ActionResult ViewPayRaiseConfig()
        {
            return View();
        }

        [Description("调薪未审核")]
        public ActionResult ViewPayRaiseUncheckList()
        {
            return View();
        }

        [Description("调薪列表")]
        public ActionResult ViewPayRaiseList()
        {
            return View();
        }

        [Description("我的调薪")]
        public ActionResult ViewPayRaiseMyList()
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
            ViewData["maxStep"] = maxStep;
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            return View();
        }

        public ActionResult ViewConfigEdit(int ID)
        {
            T_PersonnelPayRaiseConfig model = db.T_PersonnelPayRaiseConfig.Find(ID);
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
        [Description("调薪已审核")]
        public ActionResult ViewPayRaiseChecked()
        {
            return View();
        }
        #endregion
        #region 绑定数据
        public ContentResult GetPayRaiseList(Lib.GridPager pager, string queryStr, int isMy = 0, int isUncheck=0)
        {
              string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_PersonnelPayRaise> queryData = db.T_PersonnelPayRaise.Where(a=>a.isDelete==0);
            if (isMy == 1)//我的调薪
            {
                queryData = queryData.Where(a => a.PostUser == name);
            }
            else if (isUncheck == 1)//调薪未审核
            {
                int[] step =( from r in db.T_PersonnelPayRaiseConfig
                              where r.isMultiple == "1" && r.Name.Contains(name)
                             select r.Step).ToArray();
                queryData = queryData.Where(a => (a.Status == -1||a.Status==0) && (a.CurrentApproveName == name||step.Contains(a.Step)));
            }
            else if (isUncheck == 2)//调薪已审核
            {
                int[] IDs = (from r in db.T_PersonnelPayRaiseApprove
                             where r.ApproveName == name && r.Status > 0
                             select r.PID).ToArray();
                queryData = queryData.Where(a => IDs.Contains(a.ID));
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelPayRaise> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public ContentResult GetPayRaiseConfigList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_PersonnelPayRaiseConfig> queryData = db.T_PersonnelPayRaiseConfig;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PersonnelPayRaiseConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetCheckDetailList(int ID)
        {
            List<T_PersonnelPayRaiseApprove> list = db.T_PersonnelPayRaiseApprove.Where(a => a.PID == ID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion
        #region 增删改
        public JsonResult PersonnelPayRaiseApplySave(T_PersonnelPayRaise model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string approveName = db.T_PersonnelPayRaiseConfig.First(a => a.Step == 1).Name;
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 1;
                    //model.CurrentApproveName = approveName;
                    model.PostUser = UserModel.Nickname;
                    model.isDelete = 0;
                    db.T_PersonnelPayRaise.Add(model);
                    db.SaveChanges();

                    T_PersonnelPayRaiseApprove newApprove = new T_PersonnelPayRaiseApprove();
                    newApprove.ApproveName =model.CurrentApproveName;
                    newApprove.PID = model.ID;
                    newApprove.Status = 0;
                    newApprove.Step = 1;
                    db.T_PersonnelPayRaiseApprove.Add(newApprove);
                    db.SaveChanges();
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
        public JsonResult PayRaiseEditSave(T_PersonnelPayRaise model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_PersonnelPayRaise editModel = db.T_PersonnelPayRaise.Find(model.ID);
                    editModel.PresentSalary = model.PresentSalary;
                    editModel.PayRaise = model.PayRaise;
                    editModel.Reason = model.Reason;
                   
                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;

                        T_PersonnelPayRaiseApprove newApprove = new T_PersonnelPayRaiseApprove();
                        newApprove.ApproveName = db.T_PersonnelPayRaiseApprove.FirstOrDefault(a => a.PID == editModel.ID && a.Step == 1).ApproveName; //db.T_PersonnelTransferConfig.First(a => a.Step == editModel.Step).Name;
                        newApprove.PID = model.ID;
                        newApprove.Status = 0;
                        newApprove.Step = 1;
                        db.T_PersonnelPayRaiseApprove.Add(newApprove);
                    }


                    db.SaveChanges();
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
        public JsonResult DeletePayRaise(int ID)
        {
            try
            {
                T_PersonnelPayRaise editModel = db.T_PersonnelPayRaise.Find(ID);
                editModel.isDelete = 1;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ConfigAddSave(T_PersonnelPayRaiseConfig model)
        {
            try
            {
                db.T_PersonnelPayRaiseConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ConfigEditSave(T_PersonnelPayRaiseConfig model)
        {
            try
            {
                T_PersonnelPayRaiseConfig editModel = db.T_PersonnelPayRaiseConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;
                editModel.isMultiple = model.isMultiple;
                db.Entry<T_PersonnelPayRaiseConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
                T_PersonnelPayRaiseConfig delModel = db.T_PersonnelPayRaiseConfig.Find(ID);
                db.T_PersonnelPayRaiseConfig.Remove(delModel);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult PayRaiseCheckSave(T_PersonnelPayRaiseApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_PersonnelPayRaiseApprove editModel = db.T_PersonnelPayRaiseApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveTime = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_PersonnelPayRaiseApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_PersonnelPayRaise PayRaiseModel = db.T_PersonnelPayRaise.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_PersonnelPayRaiseConfig> ConfigList = db.T_PersonnelPayRaiseConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            PayRaiseModel.Status = 0;
                            PayRaiseModel.Step = nextStep;
                            PayRaiseModel.CurrentApproveName = ConfigList.First().Name; 
                            T_PersonnelPayRaiseApprove newApprove = new T_PersonnelPayRaiseApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = 0;
                            newApprove.Step = nextStep;
                            db.T_PersonnelPayRaiseApprove.Add(newApprove);
                        }
                        else//结束
                        {
                            T_PersonnelFile person = db.T_PersonnelFile.Find(PayRaiseModel.PFID);
                            person.Salary = PayRaiseModel.PayRaise;
                            db.Entry<T_PersonnelFile>(person).State = System.Data.Entity.EntityState.Modified;   //修改档案表T_PersonnelFile薪资信息
                        
                            PayRaiseModel.CurrentApproveName = ""; 
                            PayRaiseModel.Status = 1;
                            PayRaiseModel.Step = 99;
                        }
                    }
                    else//不同意结束
                    {
                        PayRaiseModel.CurrentApproveName = ""; 
                        PayRaiseModel.Status = 2;
                        PayRaiseModel.Step = 99;
                    }
                    db.Entry<T_PersonnelPayRaise>(PayRaiseModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
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
