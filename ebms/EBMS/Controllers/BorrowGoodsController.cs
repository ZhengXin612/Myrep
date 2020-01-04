using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Transactions;
using EBMS.App_Code;
using Newtonsoft.Json;

namespace EBMS.Controllers
{
    public class BorrowGoodsController : BaseController
    {
        //
        // GET: /BorrowGoods/
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
                string approveName = db.T_BorrowGoodsConfig.FirstOrDefault(a => a.Step == step).Name;
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
        public ActionResult ViewApply()
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            ViewData["ApproveList"] = getApproveName(1);
            T_BorrowGoods model = new T_BorrowGoods();
            T_User user = db.T_User.FirstOrDefault(a => a.Nickname == name);
            model.PostUser = name;
            model.Dept = Com.GetDepartmentName(Convert.ToInt32(user.DepartmentId));
            model.ReturnDate = DateTime.Now;

            return View(model);
        }

        public ActionResult ViewList()
        {
            return View();
        }

        public ActionResult ViewDetail(int PID)
        {
            ViewData["PID"] = PID;
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
            T_BorrowGoodsApprove model = db.T_BorrowGoodsApprove.FirstOrDefault(a => a.PID == PID && a.Status == -1);
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
            ViewData["ID"] = ID;
            T_BorrowGoods model = db.T_BorrowGoods.Find(ID);
            ViewData["ApproveList"] = getApproveName(1);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }

        }

        public ActionResult ViewConfig()
        {
            return View();
        }
        public ActionResult ViewConfigEdit(int ID)
        {
            T_BorrowGoodsConfig model = db.T_BorrowGoodsConfig.Find(ID);
            ViewData["boolList"] = Com.BoolList;

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
            return View();
        }
        #endregion

        #region 绑定数据
        public ContentResult GetList(Lib.GridPager pager, string queryStr, int isUncheck = 0, int isMy = 0)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_BorrowGoods> queryData = db.T_BorrowGoods.Where(a => a.IsDelete == 0);
            if (isUncheck == 1)//未审核
            {
                int[] step = (from r in db.T_BorrowGoodsConfig
                              where r.isMultiple == 1 && r.Name.Contains(name)
                              select r.Step).ToArray();
                queryData = queryData.Where(a => (a.Status == 0 || a.Status == -1) && (a.CurrentApprove == name || step.Contains(a.Step)));
            }
            else if (isMy == 1)
            {
                queryData = queryData.Where(a => a.PostUser == name);
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr));
            }

            pager.totalRows = queryData.Count();
            //分页
            List<T_BorrowGoods> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetDetailList(Lib.GridPager pager, string queryStr, int PID = 0)
        {
            IQueryable<T_BorrowGoodsDetail> queryData = db.T_BorrowGoodsDetail.Where(a => a.Pid == PID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.GoodsCode != null && a.GoodsCode.Contains(queryStr)) || (a.GoodsName != null && a.GoodsName.Contains(queryStr)));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_BorrowGoodsDetail> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_BorrowGoodsConfig> queryData = db.T_BorrowGoodsConfig;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Name != null && a.Name.Contains(queryStr)) || (a.Type != null && a.Type.Contains(queryStr)));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_BorrowGoodsConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion
        #region 增删改
        public JsonResult ApplySave(T_BorrowGoods model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string nickName = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    List<T_BorrowGoodsDetail> details = Com.Deserialize<T_BorrowGoodsDetail>(jsonStr);
                    //主表保存
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 1;
                    model.IsDelete = 0;
                    // model.CurrentApprove = "游神";
                    model.PostUser = nickName;
                    db.T_BorrowGoods.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_BorrowGoodsApprove Approvemodel = new T_BorrowGoodsApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName = model.CurrentApprove;
                        Approvemodel.Memo = "";
                        Approvemodel.PID = model.ID;
                        Approvemodel.Step = 1;
                        db.T_BorrowGoodsApprove.Add(Approvemodel);
                        foreach (T_BorrowGoodsDetail item in details)
                        {
                            item.Pid = model.ID;
                            db.T_BorrowGoodsDetail.Add(item);
                        }

                        db.SaveChanges();

                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult Delete(int ID)
        {
            try
            {
                T_BorrowGoods delModel = db.T_BorrowGoods.Find(ID);
                delModel.IsDelete = 1;
                db.Entry<T_BorrowGoods>(delModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EditSave(T_BorrowGoods model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_BorrowGoods editModel = db.T_BorrowGoods.Find(model.ID);
                    editModel.CurrentApprove = model.CurrentApprove;
                    db.Entry<T_BorrowGoods>(editModel).State = System.Data.Entity.EntityState.Modified;
                    List<T_BorrowGoodsDetail> details = Com.Deserialize<T_BorrowGoodsDetail>(jsonStr);
                    // editModel.CurrentApprove = model.CurrentApprove;
                    foreach (T_BorrowGoodsDetail item in details)
                    {
                        T_BorrowGoodsDetail editDetail = db.T_BorrowGoodsDetail.Find(item.ID);
                     
                        editDetail.GoodsCode = item.GoodsCode;
                        editDetail.GoodsName = item.GoodsName;
                        editDetail.qty = item.qty;

                        db.Entry<T_BorrowGoodsDetail>(editDetail).State = System.Data.Entity.EntityState.Modified;
                    }


                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;

                        T_BorrowGoodsApprove Approvemodel = new T_BorrowGoodsApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName = editModel.CurrentApprove;
                        Approvemodel.Memo = "";
                        Approvemodel.PID = model.ID;
                        Approvemodel.Step = 1;
                        db.T_BorrowGoodsApprove.Add(Approvemodel);
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
        public JsonResult CheckSave(T_BorrowGoodsApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_BorrowGoodsApprove editModel = db.T_BorrowGoodsApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveTime = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_BorrowGoodsApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_BorrowGoods BorrowModel = db.T_BorrowGoods.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        BorrowModel.Step = nextStep;
                        List<T_BorrowGoodsConfig> ConfigList = db.T_BorrowGoodsConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            BorrowModel.Status = 0;
                            BorrowModel.Step = nextStep;
                            BorrowModel.CurrentApprove = ConfigList.First().Name;

                            T_BorrowGoodsApprove newApprove = new T_BorrowGoodsApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = -1;
                            newApprove.Step = nextStep;
                            db.T_BorrowGoodsApprove.Add(newApprove);
                        }
                        else//结束
                        {

                            BorrowModel.Status = 1;
                            BorrowModel.Step = 99;

                        }
                    }
                    else//不同意结束
                    {
                        BorrowModel.Status = 2;
                        BorrowModel.Step = 99;
                    }
                    db.Entry<T_BorrowGoods>(BorrowModel).State = System.Data.Entity.EntityState.Modified;
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

        #region 配置
        [HttpPost]
        public JsonResult ConfigAddSave(T_BorrowGoodsConfig model)
        {
            try
            {
                model.isMultiple = 0;
                model.Type = "审核人";
                db.T_BorrowGoodsConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ConfigEditSave(T_BorrowGoodsConfig model)
        {
            try
            {
                T_BorrowGoodsConfig editModel = db.T_BorrowGoodsConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;

                db.Entry<T_BorrowGoodsConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
