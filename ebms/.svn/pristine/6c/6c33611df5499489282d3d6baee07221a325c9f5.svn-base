using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class SecurityCodeController : BaseController
    {
        //
        // GET: /SecurityCode/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewSecurityCodeAdd()
        {

            return View();
        }
        public ActionResult ViewSecurityCode()
        {

            return View();
        }
        public ActionResult ViewSecurityCodeCheck()
        {

            return View();
        }
        public ActionResult ViewSecurityCodeChecken()
        {

            return View();
        }
        public ActionResult ViewSecurityCodeDetail(int ID)
        {
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_SecurityCodeAppRove.Where(a => a.Oid == ID);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=blue>未审核</font>";
                if (item.Status == 1) s = "<font color=green>已同意</font>";
                if (item.Status == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View();
        }
        public ActionResult ViewSecurityCodeList()
        {

            return View();
        }
        public ActionResult ViewSecurityCodeReportCheck(int ID)
        {
            ViewData["ID"] = ID;
            T_SecurityCode Model = db.T_SecurityCode.SingleOrDefault(a => a.ID == ID);
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_SecurityCodeAppRove.Where(a => a.Oid == ID);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=blue>未审核</font>";
                if (item.Status == 1) s = "<font color=green>已同意</font>";
                if (item.Status == 2) s = "<font color=red>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
            return View(Model);
        }
        [Description("我的防伪码")]
        public ContentResult GetSecurityCode(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_SecurityCode> queryData = db.T_SecurityCode.Where(a => a.ApplyName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_SecurityCode> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("防伪码未审核")]
        public ContentResult GetSecurityCodeCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_SecurityCodeGroup> GroupModel = db.T_SecurityCodeGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_SecurityCodeAppRove> ApproveMod = db.T_SecurityCodeAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
            string arrID = "";
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                if (i == 0)
                {
                    arrID += ApproveMod[i].Oid.ToString();
                }
                else
                {
                    arrID += "," + ApproveMod[i].Oid.ToString();
                }
            }
            string sql = "select * from T_SecurityCode r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_SecurityCode> queryData = db.Database.SqlQuery<T_SecurityCode>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_SecurityCode> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //防伪码已审核列表  
        [HttpPost]
        public ContentResult GetSecurityCodeCheckenList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_SecurityCodeAppRove> ApproveMod = db.T_SecurityCodeAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_SecurityCode> queryData = from r in db.T_SecurityCode
                                                   where Arry.Contains(r.ID) && r.Isdelete == 0
                                                   select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.ProductName != null && a.ProductName.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_SecurityCode> list = new List<T_SecurityCode>();
            foreach (var item in queryData)
            {
                T_SecurityCode i = new T_SecurityCode();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("防伪码管理")]
        public ContentResult GetSecurityCodeList(Lib.GridPager pager, string queryStr)
        {

            IQueryable<T_SecurityCode> queryData = db.T_SecurityCode.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ProductName != null && a.ProductName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_SecurityCode> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("防伪码详情")]
        public ContentResult GetSecurityCodeDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_SecurityCode> queryData = db.T_SecurityCode.Where(a => a.ID == ID).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<T_SecurityCode> list = queryData.ToList();//.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("新增保存")]
        public JsonResult SecurityCodeApp(T_SecurityCode model)
        {

            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.ApplyName = Nickname;
            model.ApplyDate = DateTime.Now;
            model.Status = -1;
            model.Step = 0;
            model.Isdelete = 0;
            db.T_SecurityCode.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                T_SecurityCodeConfig modelconfig = db.T_SecurityCodeConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                T_SecurityCodeAppRove AppRoveModel = new T_SecurityCodeAppRove();
                AppRoveModel.Status = -1;
                AppRoveModel.Step = "0";
                if (modelconfig.Name == null || modelconfig.Name == "")
                {
                    AppRoveModel.ApproveName = modelconfig.Type;
                }
                else
                {
                    AppRoveModel.ApproveName = modelconfig.Name;
                }
                AppRoveModel.ApproveDName = modelconfig.Type;
                AppRoveModel.Oid = model.ID;
                db.T_SecurityCodeAppRove.Add(AppRoveModel);
                db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        //审核 
        public JsonResult SecurityCodeCheck(T_SecurityCode model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_SecurityCode Invoicemodel = db.T_SecurityCode.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_SecurityCodeAppRove modelApprove = db.T_SecurityCodeAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_SecurityCodeAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_SecurityCodeAppRove newApprove = new T_SecurityCodeAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_SecurityCodeConfig> config = db.T_SecurityCodeConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_SecurityCodeConfig stepMod = db.T_SecurityCodeConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            newApprove.Memo = "";
                            newApprove.Oid = ID;
                            newApprove.Status = -1;
                            newApprove.Step = step.ToString();
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                                newApprove.ApproveDName = stepMod.Type;
                            }
                            else
                            {
                                newApprove.ApproveName = stepMod.Type;
                                newApprove.ApproveDName = stepMod.Type;
                            }
                            db.T_SecurityCodeAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_SecurityCode>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        if (j > 0)
                        {
                            result = "保存成功";
                        }
                        else
                        {
                            result = "保存失败";
                        }
                    }
                    else
                    {

                        //不同意
                        Invoicemodel.Step = 0;
                        Invoicemodel.Status = 2;
                        db.Entry<T_SecurityCode>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }

                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [Description("申领删除")]
        public JsonResult DeleteSecurityCodeFinance(int ID)
        {
            T_SecurityCode model = db.T_SecurityCode.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_SecurityCode>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
    }
}
