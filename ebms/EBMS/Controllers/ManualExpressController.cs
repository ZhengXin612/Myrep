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
    public class ManualExpressController : BaseController
    {
        EBMSEntities db = new EBMSEntities();
        //
        /// <summary>
        /// 根据部门选择下级审核人
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetApproveByDeptID()
        {

            T_ManualExpressGroup list = db.T_ManualExpressGroup.SingleOrDefault(s => s.GroupName == "部门主管");
            string[] listCrew = list.GroupUser.Split(',');
            Dictionary<string, string> listCrewModel = new Dictionary<string, string>();
            for (int i = 0; i < listCrew.Length; i++)
            {
                string name = listCrew[i];

                listCrewModel.Add(name, name);
            }

            return Json(listCrewModel.ToArray());

        }
        // GET: /ManualExpress/
        public static List<SelectListItem> ManualExpressType(int type)
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_ManualExpressType.Where(a=>a.type==type).AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        public ActionResult ViewManualExpress()
        {
            ViewData["EntryType"] = ManualExpressType(1);
            return View();
        }
        public ActionResult ViewManualExpressCheck()
        {
            return View();
        }
        public ActionResult ViewManualExpressList()
        {
            return View();
        }
        public ActionResult ViewManualExpressChecken()
        {
            return View();
        }
        public ActionResult ViewManualExpressDetail(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_ManualExpress model = db.T_ManualExpress.SingleOrDefault(a => a.ID == ID);
            return View(model);
        }
        public ActionResult ViewManualExpressReportCheck(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_ManualExpress model = db.T_ManualExpress.SingleOrDefault(a => a.ID == ID);
            return View(model);
        }
        
        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_ManualExpressAppRove.Where(a => a.Oid == id);
            string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
            string tr = "";
            foreach (var item in history)
            {
                string s = "";
                if (item.Status == -1) s = "<font color=#d02e2e>未审核</font>";
                if (item.Status == 1) s = "<font color=#1fc73a>已同意</font>";
                if (item.Status == 2) s = "<font color=#d02e2e>不同意</font>";
                tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Memo);
            }
            ViewData["history"] = table + tr + "</tbody></table>";
        }
        public ActionResult ViewManualExpressAdd()
        {
            ViewData["ApplyShopList"] = App_Code.Com.Shop();
            ViewData["ApplyCompanyList"] = ManualExpressType(2);
            ViewData["EntryTypeList"] = ManualExpressType(1);

            return View();
        }
        [Description("我的快递手工录费")]
        public ContentResult GetManualExpress(Lib.GridPager pager, string queryStr, string EntryType)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_ManualExpress> queryData = db.T_ManualExpress.Where(a => a.ApplyName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Reason != null && a.Reason.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(EntryType))
            {
                queryData = queryData.Where(a => a.EntryType.Contains(EntryType));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualExpress> list = new List<T_ManualExpress>();
            foreach (var item in queryData)
            {
                T_ManualExpress i = new T_ManualExpress();
            
                i = item;
                list.Add(i);
            }
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //快递录费详情列表 
        [HttpPost]
        public ContentResult GetManualExpressDetailList(Lib.GridPager pager, int ID)
        {
            IQueryable<T_ManualExpressDetail> queryData = db.T_ManualExpressDetail.Where(a => a.Oid == ID);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualExpressDetail> list = new List<T_ManualExpressDetail>();
            foreach (var item in queryData)
            {
                T_ManualExpressDetail i = new T_ManualExpressDetail();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }

        [Description("手工录费管理")]
        public ContentResult GetManualExpressList(Lib.GridPager pager, string queryStr, string DetailsQuery)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_ManualExpress> queryData = db.T_ManualExpress.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(DetailsQuery))
            {
                queryData = db.Database.SqlQuery<T_ManualExpress>("select * from T_ManualExpress where ID in (select oid from T_ManualExpressDetail where express like '%" + DetailsQuery + "%'or Remarks  like  '%" + DetailsQuery + "%')and IsDelete = '0'").AsQueryable();
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Reason != null && a.Reason.Contains(queryStr)|| a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualExpress> list = new List<T_ManualExpress>();
            foreach (var item in queryData)
            {
                T_ManualExpress i = new T_ManualExpress();
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //手工订单已审核列表  
        [HttpPost]
        public ContentResult GetManualExpressChecken(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_ManualExpressAppRove> ApproveMod = db.T_ManualExpressAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_ManualExpress> queryData = from r in db.T_ManualExpress
                                                    where Arry.Contains(r.ID) && r.Isdelete == 0
                                                    select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Reason != null && a.Reason.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualExpress> list = new List<T_ManualExpress>();
            foreach (var item in queryData)
            {
                T_ManualExpress i = new T_ManualExpress();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("快递手工录费审核查询")]
        public ContentResult GetManualExpressCheck(Lib.GridPager pager, string queryStr, string DetailsQuery)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_ManualExpressGroup> GroupModel = db.T_ManualExpressGroup.Where(a => a.GroupUser != null && (a.GroupUser.Contains(name) || a.GroupUser.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_ManualExpressAppRove> ApproveMod = db.T_ManualExpressAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_ManualExpress r  where isdelete=0  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_ManualExpress> queryData = db.Database.SqlQuery<T_ManualExpress>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(DetailsQuery))
            {
                queryData = db.Database.SqlQuery<T_ManualExpress>("select * from T_ManualExpress where ID in (select oid from T_ManualExpressDetail where express like '%" + DetailsQuery + "%'or Remarks  like  '%" + DetailsQuery + "%')and IsDelete = '0'").AsQueryable();
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Reason != null && a.Reason.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ManualExpress> list = new List<T_ManualExpress>();
            foreach (var item in queryData)
            {
                T_ManualExpress i = new T_ManualExpress();
              
                i = item;
                list.Add(i);
            }
            //分页
            //  List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_ManualExpress model = db.T_ManualExpress.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_ManualExpress>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
           // ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }

        public void ModularByZP()
        {

            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "快递手工费").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ManualExpressAppRove where Oid in (select ID from T_ManualExpress where  Isdelete='0'  and (Status = -1 or Status = 0 ) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "快递手工费" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "快递手工费";
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
            string RejectNumberSql = "select ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ManualExpress where Status='2' and Isdelete=0 GROUP BY ApplyName";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "快递手工费" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "快递手工费";
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

        [Description("新增保存")]
        public JsonResult ManualExpressAdd(T_ManualExpress model, string jsonStr, string p_c_a, string Expense)
        {
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    model.ApplyName = Nickname;
                    model.ApplyDate = DateTime.Now;
                    model.Status = -1;
                    model.Step = 0;
                    model.Isdelete = 0;
                    model.DetailedAddress = p_c_a;
                    db.T_ManualExpress.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        List<T_ManualExpressDetail> details = App_Code.Com.Deserialize<T_ManualExpressDetail>(jsonStr);


                        

                        foreach (var item in details)
                        {

                            string express=item.express;
                           // List<T_ManualExpress> Dmodel = db.T_ManualExpress.Where(a => a.Isdelete !=1).ToList();
                            List<T_ManualExpressDetail> Dmodel = db.T_ManualExpressDetail.Where(a=>a.express==express).ToList();
                            if (Dmodel.Count > 0)
                            {
                                int[] PIDs = Dmodel.Select(a => a.Oid).ToArray();
                                List<T_ManualExpress> list= db.T_ManualExpress.Where(a => PIDs.Contains(a.ID) && a.Isdelete == 0).ToList();
                                if(list.Count>0)
                                {
                                    return Json(new { State = "cuowu" }, JsonRequestBehavior.AllowGet);
                                }
                                
                            }
                            item.Oid = model.ID;
                            db.T_ManualExpressDetail.Add(item);
                        }
                        db.SaveChanges();
                        T_ManualExpressConfig modelconfig = db.T_ManualExpressConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                        T_ManualExpressAppRove AppRoveModel = new T_ManualExpressAppRove();
                        AppRoveModel.Status = -1;
                        AppRoveModel.Step = "0";
                        AppRoveModel.ApproveName = Expense;
                        AppRoveModel.ApproveDName = modelconfig.Type;
                        AppRoveModel.Oid = model.ID;
                        db.T_ManualExpressAppRove.Add(AppRoveModel);
                        db.SaveChanges();

                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }
                    //ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);


                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //审核 
        public JsonResult ManualExpressReportCheck(T_ManualExpress model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_ManualExpress Invoicemodel = db.T_ManualExpress.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_ManualExpressAppRove modelApprove = db.T_ManualExpressAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_ManualExpressAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_ManualExpressAppRove newApprove = new T_ManualExpressAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_ManualExpressConfig> config = db.T_ManualExpressConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_ManualExpressConfig stepMod = db.T_ManualExpressConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_ManualExpressAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_ManualExpress>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        ModularByZP();
                        sc.Complete();
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
                        db.Entry<T_ManualExpress>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        sc.Complete();
                        if (j > 0)
                        {
                            result = "保存成功";
                        }
                        else
                        {
                            result = "保存失败";
                        }

                    }
                }
                else
                {
                    result = "保存失败";
                }


                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
