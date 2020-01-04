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
    public class InvoiceApplyController : BaseController
    {
        /// <summary>
        /// 绑定店铺无默认值
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> ExpenseAcount()
        { 

            EBMSEntities db = new EBMSEntities();
            var list = db.T_ExpenseAcount.Where(a =>a.ComPany!=null).AsQueryable();
            var selectList = new SelectList(list, "ComPany", "ComPany");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        //
        // GET: /InvoiceApply/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewInvoiceApplyApp()
        {
            ViewData["ShopNameList"] = ExpenseAcount();
            return View();
        }
        public ActionResult ViewInvoiceApply()
        {
            return View();
        }
        public ActionResult ViewInvoiceApplyList()
        {
            return View();
        }
        public ActionResult ViewInvoiceApplyCheck()
        {
            return View();
        }
        public ActionResult ViewInvoiceApplyChecken()
        {
            return View();
        }
        public ActionResult ViewInvoiceApplyReprtCheck(int ID)
        {
            ViewData["ID"] = ID;
            T_InvoiceApply Model = db.T_InvoiceApply.SingleOrDefault(a => a.ID == ID);
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_InvoiceApplyAppRove.Where(a => a.Oid == ID);
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
        public ActionResult ViewInvoiceApplyDetail(int ID)
        {
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_InvoiceApplyAppRove.Where(a => a.Oid == ID);
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
        [Description("申领详情")]
        public ContentResult GetMajorInvoiceDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_InvoiceApply> queryData = db.T_InvoiceApply.Where(a => a.ID == ID).AsQueryable();

            pager.totalRows = queryData.Count();
            //分页
            List<T_InvoiceApply> list = queryData.ToList();//.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [HttpPost]
        [Description("申领删除")]
        public JsonResult DeleteInvoiceApplyFinance(int ID)
        {
            T_InvoiceApply model = db.T_InvoiceApply.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_InvoiceApply>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

            //ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        [Description("发票申请管理")]
        public ContentResult GetInvoiceApplyList(Lib.GridPager pager, string queryStr)
        {

            IQueryable<T_InvoiceApply> queryData = db.T_InvoiceApply.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_InvoiceApply> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
            [Description("发票申领")]
        public ContentResult GetInvoiceApply(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_InvoiceApply> queryData = db.T_InvoiceApply.Where(a => a.ApplyName == Nickname&&a.Isdelete==0).AsQueryable() ;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_InvoiceApply> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

            [Description("发票申领未审核")]
            public ContentResult GetInvoiceApplyCheck(Lib.GridPager pager, string queryStr)
            {
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                List<T_InvoiceApplyGroup> GroupModel = db.T_InvoiceApplyGroup.Where(a => a.GroupUser != null && (a.GroupUser.Contains(name) || a.GroupUser.Contains(Nickname))).ToList();
                string[] shenheName = new string[GroupModel.Count];
                for (int z = 0; z < GroupModel.Count; z++)
                {
                    shenheName[z] = GroupModel[z].GroupName;
                }
                List<T_InvoiceApplyAppRove> ApproveMod = db.T_InvoiceApplyAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
                string sql = "select * from T_InvoiceApply r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
                if (arrID != null && arrID != "")
                {
                    sql += " and ID in (" + arrID + ")";
                }
                else
                {
                    sql += " and 1=2";
                }
                IQueryable<T_InvoiceApply> queryData = db.Database.SqlQuery<T_InvoiceApply>(sql).AsQueryable();
                //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
                if (!string.IsNullOrEmpty(queryStr))
                {
                    queryData = queryData.Where(a => a.ShopName != null && a.ShopName.Contains(queryStr));
                }
                pager.totalRows = queryData.Count();
                //分页
                List<T_InvoiceApply> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            //发票申请已审核列表  
            [HttpPost]
            public ContentResult GetInvoiceApplyCheckenList(Lib.GridPager pager, string queryStr)
            {
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


                List<T_InvoiceApplyAppRove> ApproveMod = db.T_InvoiceApplyAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
                int[] Arry = new int[ApproveMod.Count];
                for (int i = 0; i < ApproveMod.Count; i++)
                {
                    Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
                }
                IQueryable<T_InvoiceApply> queryData = from r in db.T_InvoiceApply
                                                       where Arry.Contains(r.ID) && r.Isdelete == 0
                                                       select r;
                if (!string.IsNullOrEmpty(queryStr))
                {
                    queryData = queryData.Where(a => (a.ShopName != null && a.ShopName.Contains(queryStr)));
                }

                pager.totalRows = queryData.Count();
                //分页
                queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
                List<T_InvoiceApply> list = new List<T_InvoiceApply>();
                foreach (var item in queryData)
                {
                    T_InvoiceApply i = new T_InvoiceApply();
                    i = item;
                    list.Add(i);
                }
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
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
			List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "发票").ToList();
			if (ModularNotaudited.Count > 0)
			{
				foreach (var item in ModularNotaudited)
				{
					db.T_ModularNotaudited.Remove(item);
				}
				db.SaveChanges();
			}

			string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_InvoiceApplyAppRove where Oid in (select ID from T_InvoiceApply where  Isdelete='0'  and (Status = -1 or Status = 0 ) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
			List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
			{
				string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

				T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "发票" && a.PendingAuditName == PendingAuditName);
				if (NotauditedModel != null)
				{
					NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
					db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
				}
				else
				{
					T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
					ModularNotauditedModel.ModularName = "发票";
					ModularNotauditedModel.RejectNumber = 0;
					ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
					ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
					ModularNotauditedModel.ToupdateDate = DateTime.Now;
					ModularNotauditedModel.ToupdateName = Nickname;
					db.T_ModularNotaudited.Add(ModularNotauditedModel);
				}
				db.SaveChanges();
			}
		}

        [Description("新增保存")]
        public JsonResult InvoiceApplyApp(T_InvoiceApply model)
        {

            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.ApplyName = Nickname;
            model.ApplyDate = DateTime.Now;
            model.Status = -1;
            model.Step = 0;
            model.Isdelete = 0;
            db.T_InvoiceApply.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                T_InvoiceApplyConfig modelconfig = db.T_InvoiceApplyConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                T_InvoiceApplyAppRove AppRoveModel = new T_InvoiceApplyAppRove();
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
                db.T_InvoiceApplyAppRove.Add(AppRoveModel);
                db.SaveChanges();
               // ModularByZP();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }
          

        }
        //审核 
        public JsonResult InvoiceApplyCheck(T_InvoiceApply model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_InvoiceApply Invoicemodel = db.T_InvoiceApply.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_InvoiceApplyAppRove modelApprove = db.T_InvoiceApplyAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_InvoiceApplyAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_InvoiceApplyAppRove newApprove = new T_InvoiceApplyAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_InvoiceApplyConfig> config = db.T_InvoiceApplyConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_InvoiceApplyConfig stepMod = db.T_InvoiceApplyConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_InvoiceApplyAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_InvoiceApply>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
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
                        db.Entry<T_InvoiceApply>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }
                //ModularByZP();
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
