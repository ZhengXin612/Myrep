using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using EBMS.App_Code;
using Newtonsoft.Json;

namespace EBMS.Controllers
{
    public class PhotoManageController : BaseController
    {
        //
        // GET: /PhotoManage/
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
                string approveName = db.T_PhotoConfig.FirstOrDefault(a => a.Step == step).Name;
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

        public class PhotoData
        {
            public int ID { get; set; }
            public System.DateTime PostTime { get; set; }
            public string PostName { get; set; }
            public int Step { get; set; }
            public int Status { get; set; }
            public string CurrentApprove { get; set; }
            public int IsDelete { get; set; }
            public int PID { get; set; }
            public string ShopName { get; set; }
            public string GoodsCode { get; set; }
            public string GoodsName { get; set; }
            public string Spec { get; set; }
            public string BasicRequire { get; set; }
            public string SpecialRequire { get; set; }
        }
        #endregion
        #region 视图
        public ActionResult ViewApply()
        {
            ViewData["ApproveList"] = getApproveName(1);
            return View();
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
        public ActionResult ViewDataList()
        {
            return View();
        }
        public ActionResult ViewEdit(int ID)
        {
            ViewData["ID"] = ID;
            T_Photograph model = db.T_Photograph.Find(ID);
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
        public ActionResult ViewCheck(int PID)
        {
            T_PhotoApprove model = db.T_PhotoApprove.FirstOrDefault(a => a.PID == PID && a.Status == -1);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
             
        }

        public ActionResult ViewConfigList()
        {
            return View();
        }
        public ActionResult ViewConfigEdit(int ID)
        {
            T_PhotoConfig model = db.T_PhotoConfig.Find(ID);
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

        public ActionResult ViewShop(int index)
        {
            ViewData["index"] = index;
            return View();
        }
        #endregion
        #region 绑定数据
        public ContentResult GetList(Lib.GridPager pager, string queryStr, int isUncheck = 0, int isMy = 0)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_Photograph> queryData = db.T_Photograph.Where(a=>a.IsDelete==0);
            if (isUncheck == 1)//未审核
            {
                int[] step = (from r in db.T_PhotoConfig
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
                queryData = queryData.Where(a => a.PostName != null && a.PostName.Contains(queryStr));
            }

            pager.totalRows = queryData.Count();
            //分页
            List<T_Photograph> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetDetailList(Lib.GridPager pager, string queryStr, int PID=0)
        {
            IQueryable<T_PhotoDetail> queryData = db.T_PhotoDetail.Where(a => a.PID == PID);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.GoodsCode != null && a.GoodsCode.Contains(queryStr)) || (a.GoodsName != null && a.GoodsName.Contains(queryStr)));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PhotoDetail> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetDataList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            IQueryable<T_Photograph> queryData = db.T_Photograph.Where(a => a.IsDelete == 0);
            IQueryable<T_PhotoDetail> detail = db.T_PhotoDetail;
            List<T_PhotoDetail> detailList = new List<T_PhotoDetail>();
            List<T_Photograph> modelList = new List<T_Photograph>();
            if (!string.IsNullOrEmpty(queryStr))
            {
                detailList = detail.Where(a => (a.GoodsCode != null && a.GoodsCode.Contains(queryStr)) || (a.GoodsName != null && a.GoodsName.Contains(queryStr))).ToList();
                int[] arr = detailList.Select(a => a.PID).ToArray();
                modelList = queryData.Where(a => arr.Contains(a.ID)).ToList();
            }
            else
            {
                detailList = detail.ToList();
                modelList = queryData.ToList();
            }
            var zz = from r in modelList
                     from d in detailList
                    where r.ID==d.PID
                     select new PhotoData
                     {
                         ID=r.ID,
                         BasicRequire=d.BasicRequire,
                         GoodsCode=d.GoodsCode,
                         GoodsName=d.GoodsName,
                         PostName=r.PostName,
                         PostTime=r.PostTime,
                         ShopName=d.ShopName,
                         Spec=d.Spec,
                         SpecialRequire=d.SpecialRequire,
                         Status=r.Status
                         
                     };
            pager.totalRows = zz.Count();
            //分页
            List<PhotoData> list = zz.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(zz, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_PhotoConfig> queryData = db.T_PhotoConfig;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Name != null && a.Name.Contains(queryStr)) || (a.Type != null && a.Type.Contains(queryStr)));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_PhotoConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        public ContentResult GetShop(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_ShopFromGY> queryData = db.T_ShopFromGY.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr) || a.code != null && a.code.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
           // queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ShopFromGY> list = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
           
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }
        #endregion
        #region 增删改
        public JsonResult ApplySave(T_Photograph model,string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string nickName = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    List<T_PhotoDetail> details = Com.Deserialize<T_PhotoDetail>(jsonStr);
                    //主表保存
                    model.PostTime = DateTime.Now;
                    model.Status = -1;
                    model.Step = 1;
                    model.IsDelete = 0;
                   // model.CurrentApprove = "游神";
                    model.PostName = nickName;
                    db.T_Photograph.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_PhotoApprove Approvemodel = new T_PhotoApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName =model.CurrentApprove;
                        Approvemodel.Memo = "";
                        Approvemodel.PID = model.ID;
                        Approvemodel.Step = 1;
                        db.T_PhotoApprove.Add(Approvemodel);
                        foreach (T_PhotoDetail item in details)
                        {
                            item.PID = model.ID;
                            db.T_PhotoDetail.Add(item);
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
                T_Photograph delModel = db.T_Photograph.Find(ID);
                delModel.IsDelete = 1;
                db.Entry<T_Photograph>(delModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EditSave(T_Photograph model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_Photograph editModel = db.T_Photograph.Find(model.ID);
                    editModel.CurrentApprove = model.CurrentApprove;
                    db.Entry<T_Photograph>(editModel).State = System.Data.Entity.EntityState.Modified;
                    List<T_PhotoDetail> details = Com.Deserialize<T_PhotoDetail>(jsonStr);
                   // editModel.CurrentApprove = model.CurrentApprove;
                    foreach (T_PhotoDetail item in details)
                    {
                        T_PhotoDetail editDetail = db.T_PhotoDetail.Find(item.ID);
                        editDetail.BasicRequire = item.BasicRequire;
                        editDetail.GoodsCode = item.GoodsCode;
                        editDetail.GoodsName = item.GoodsName;
                        editDetail.ShopName = item.ShopName;
                        editDetail.Spec = item.Spec;
                        editDetail.SpecialRequire = item.SpecialRequire;
                        db.Entry<T_PhotoDetail>(editDetail).State = System.Data.Entity.EntityState.Modified;
                    }


                    if (editModel.Status == 2)
                    {
                        editModel.Status = -1;
                        editModel.Step = 1;

                        T_PhotoApprove Approvemodel = new T_PhotoApprove();
                        Approvemodel.Status = -1;
                        Approvemodel.ApproveName = editModel.CurrentApprove;
                        Approvemodel.Memo = "";
                        Approvemodel.PID = model.ID;
                        Approvemodel.Step = 1;
                        db.T_PhotoApprove.Add(Approvemodel);
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
        public JsonResult CheckSave(T_PhotoApprove model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                    T_PhotoApprove editModel = db.T_PhotoApprove.Find(model.ID);
                    editModel.Status = model.Status;
                    editModel.ApproveTime = DateTime.Now;
                    editModel.ApproveName = name;
                    editModel.Memo = model.Memo;
                    db.Entry<T_PhotoApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                    T_Photograph PhotoModel = db.T_Photograph.Find(editModel.PID);
                    if (model.Status == 1)//同意
                    {
                        int nextStep = editModel.Step + 1;
                        List<T_PhotoConfig> ConfigList = db.T_PhotoConfig.Where(a => a.Step == nextStep).ToList();
                        if (ConfigList.Count > 0)//下一步
                        {
                            PhotoModel.Status = 0;
                            PhotoModel.Step = nextStep;
                            PhotoModel.CurrentApprove = ConfigList.First().Name;

                            T_PhotoApprove newApprove = new T_PhotoApprove();
                            newApprove.ApproveName = ConfigList.First().Name;
                            newApprove.PID = editModel.PID;
                            newApprove.Status = -1;
                            newApprove.Step = nextStep;
                            db.T_PhotoApprove.Add(newApprove);
                        }
                        else//结束
                        {

                            PhotoModel.Status = 1;
                            PhotoModel.Step = 99;

                        }
                    }
                    else//不同意结束
                    {
                        PhotoModel.Status = 2;
                        PhotoModel.Step = 99;
                    }
                    db.Entry<T_Photograph>(PhotoModel).State = System.Data.Entity.EntityState.Modified;
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
        public JsonResult ConfigAddSave(T_PhotoConfig model)
        {
            try
            {
                model.isMultiple = "1";
                model.Type = "审核人";
                db.T_PhotoConfig.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ConfigEditSave(T_PhotoConfig model)
        {
            try
            {
                T_PhotoConfig editModel = db.T_PhotoConfig.Find(model.ID);
                editModel.Name = model.Name;
                editModel.Step = model.Step;
                editModel.Type = model.Type;

                db.Entry<T_PhotoConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
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
