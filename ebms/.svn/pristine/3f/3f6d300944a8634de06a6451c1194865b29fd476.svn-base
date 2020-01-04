using EBMS.App_Code;
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
    public class HandwrittenExpressController : BaseController
    {
        //
        // GET: /HandwrittenExpress/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewHandwrittenExpressAdd()
        {
            ViewData["Warehouse"] = Com.Warehouses();
            return View();
        }
        public ActionResult ViewHandwrittenExpress()
        {
            return View();
        }
        public ActionResult ViewHandwrittenExpressList()
        {
            return View();
        }
        public ActionResult ViewHandwrittenExpressCheck()
        {
            return View();
        }
        public ActionResult ViewHandwrittenExpressChecken()
        {
            return View();
        }
        public ActionResult ViewHandwrittenExpressCopy()
        {
            return View();
        }
        
        public ActionResult ViewHandwrittenExpressDetail(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_HandwrittenExpress model = db.T_HandwrittenExpress.SingleOrDefault(a => a.ID == ID);
            model.Warehouse = Com.GetWarehouseName(model.Warehouse);
            return View(model);
        }
        public ActionResult ViewUserGY()
        {
            return View();
        }
      

        public ActionResult ViewHandwrittenExpressReportCheck(int ID)
        {
            GetApproveHistory(ID);
            ViewData["ID"] = ID;
            T_HandwrittenExpress model = db.T_HandwrittenExpress.SingleOrDefault(a => a.ID == ID);
            return View(model);
        }
        //获取审核详情记录
        private void GetApproveHistory(int id = 0)
        {
            var history = db.T_HandwrittenExpressAppRove.Where(a => a.Oid == id);
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

        
        [Description("我的手写快递单")]
        public ContentResult GetHandwrittenExpress(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_HandwrittenExpress> queryData = db.T_HandwrittenExpress.Where(a => a.ApplyName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Telephone != null && a.Telephone.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_HandwrittenExpress> list = new List<T_HandwrittenExpress>();
            foreach (var item in queryData)
            {
                T_HandwrittenExpress i = new T_HandwrittenExpress();
                item.Warehouse = Com.GetWarehouseName(item.Warehouse);
                i = item;
                list.Add(i);
            }
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("手写快递审核查询")]
        public ContentResult GetHandwrittenExpressCheck(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_HandwrittenExpressGroup> GroupModel = db.T_HandwrittenExpressGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_HandwrittenExpressAppRove> ApproveMod = db.T_HandwrittenExpressAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_HandwrittenExpress r  where isdelete=0  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_HandwrittenExpress> queryData = db.Database.SqlQuery<T_HandwrittenExpress>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Telephone != null && a.Telephone.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_HandwrittenExpress> list = new List<T_HandwrittenExpress>();
            foreach (var item in queryData)
            {
                T_HandwrittenExpress i = new T_HandwrittenExpress();
                item.Warehouse = Com.GetWarehouseName(item.Warehouse);
                i = item;
                list.Add(i);
            }
            //分页
            //  List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //手写快递已审核列表  
        [HttpPost]
        public ContentResult GetHandwrittenExpressChecken(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_HandwrittenExpressAppRove> ApproveMod = db.T_HandwrittenExpressAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_HandwrittenExpress> queryData = from r in db.T_HandwrittenExpress
                                                    where Arry.Contains(r.ID) && r.Isdelete == 0
                                                    select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Telephone != null && a.Telephone.Contains(queryStr)));
            }

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_HandwrittenExpress> list = new List<T_HandwrittenExpress>();
            foreach (var item in queryData)
            {
                T_HandwrittenExpress i = new T_HandwrittenExpress();
                item.Warehouse = Com.GetWarehouseName(item.Warehouse);
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public partial class HandwrittenExpress
        {

            public int ID { get; set; }
            public string ApplyName { get; set; }
            public Nullable<System.DateTime> ApplyDate { get; set; }
            public string ApplyPurpose { get; set; }
            public Nullable<int> ApplyNumber { get; set; }
            public string Address { get; set; }
            public string Telephone { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public Nullable<int> Isdelete { get; set; }
            public string AddressName { get; set; }
            public string Zip { get; set; }
            public string ccname { get; set; }
            public string Warehouse { get; set; }
        }
        [Description("手写快快递抄送")]
        public ContentResult GetHandwrittenExpressCopy(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_HandwrittenExpressCopy> ApproveMod = db.T_HandwrittenExpressCopy.Where(a => a.ReceiveName == Nickname).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                ApproveMod = ApproveMod.Where(a => a.CCName != null && a.CCName.Contains(queryStr));
            }
            int[] Arry = new int[ApproveMod.ToList().Count];
            string Arryid = "";
            for (int i = 0; i < ApproveMod.ToList().Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod.ToList()[i].Oid.ToString());
                if (i == 0)
                {
                    Arryid += ApproveMod.ToList()[i].Oid.ToString();
                }
                else
                {
                    Arryid +=","+ ApproveMod.ToList()[i].Oid.ToString();
                }
            }
            string sql = "";
            if (Arryid != "")
            {
                 sql = "select ID,ApplyName,ApplyDate,ApplyPurpose,ApplyNumber,Address,Telephone,Status,Step,Isdelete,AddressName,Zip,(select  top 1 CCName  from T_HandwrittenExpressCopy where h.ID=Oid) as ccname  from T_HandwrittenExpress h where ID in (" + Arryid + ")";
            }
            else
            {
                sql = "select ID,ApplyName,ApplyDate,ApplyPurpose,ApplyNumber,Address,Telephone,Status,Step,Isdelete,AddressName,Zip,(select  top 1 CCName  from T_HandwrittenExpressCopy where h.ID=Oid) as ccname  from T_HandwrittenExpress h where 1=2";
            }
            IQueryable<HandwrittenExpress> queryData = db.Database.SqlQuery<HandwrittenExpress>(sql).AsQueryable();
           
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<HandwrittenExpress> list = new List<HandwrittenExpress>();
            foreach (var item in queryData)
            {
                HandwrittenExpress i = new HandwrittenExpress();
                item.Warehouse = Com.GetWarehouseName(item.Warehouse);
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("手写快递单管理")]
        public ContentResult GetHandwrittenExpressList(Lib.GridPager pager, string queryStr)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_HandwrittenExpress> queryData = db.T_HandwrittenExpress.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Telephone != null && a.Telephone.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_HandwrittenExpress> list = new List<T_HandwrittenExpress>();
            foreach (var item in queryData)
            {
                T_HandwrittenExpress i = new T_HandwrittenExpress();
                item.Warehouse = Com.GetWarehouseName(item.Warehouse);
                i = item;
                list.Add(i);
            }
            //分页
            // List<T_ManualBilling> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [Description("新增保存")]
        public JsonResult HandwrittenExpressAdd(T_HandwrittenExpress model, string jsonStr)
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

                    db.T_HandwrittenExpress.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_HandwrittenExpressConfig modelconfig = db.T_HandwrittenExpressConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                        T_HandwrittenExpressAppRove AppRoveModel = new T_HandwrittenExpressAppRove();
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
                        db.T_HandwrittenExpressAppRove.Add(AppRoveModel);
                        db.SaveChanges();
                    }
                    else
                    {
                        return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
                    }

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);


                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //产品列表 
        [HttpPost]
        public ContentResult GetUserGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_User> queryData = db.T_User.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Nickname != null && a.Nickname.Contains(queryStr) || a.Name != null && a.Name.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_User> list = new List<T_User>();
            foreach (var item in queryData)
            {
                T_User i = new T_User();
                item.DepartmentId = GetDepartmentId(item.DepartmentId);
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        public string GetDepartmentId(string DepartmentId)
        {
            int Did=int.Parse(DepartmentId);
            List<T_Department> DepartmentModel = db.T_Department.Where(a => a.ID == Did).ToList();
            if (DepartmentModel.Count > 0)
            {
                return DepartmentModel[0].Name;
            }
            return "";
        }
        [HttpPost]
        [Description("手工单删除")]
        public JsonResult DeleteInvoiceFinance(int ID)
        {
            T_HandwrittenExpress model = db.T_HandwrittenExpress.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_HandwrittenExpress>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //审核 
        public JsonResult HandwrittenExpressCheckCheck(T_HandwrittenExpress model, string status, string Memo, string CSG)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                int ID = model.ID;
                T_HandwrittenExpress Invoicemodel = db.T_HandwrittenExpress.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                T_HandwrittenExpressAppRove modelApprove = db.T_HandwrittenExpressAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_HandwrittenExpressAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (CSG != null && CSG != "")
                    {
                        string[] CSGname = CSG.Split(',');
                        for (int f = 0; f < CSGname.Length; f++)
                        {
                            T_HandwrittenExpressCopy Copy = new T_HandwrittenExpressCopy();
                            Copy.Oid = ID;
                            Copy.CCName = Nickname;
                            Copy.ReceiveName = CSGname[f]; 
                            db.T_HandwrittenExpressCopy.Add(Copy);
                            db.SaveChanges();
                        }
                    }

                    if (status == "1")
                    {


                        T_HandwrittenExpressAppRove newApprove = new T_HandwrittenExpressAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_HandwrittenExpressConfig> config = db.T_HandwrittenExpressConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_HandwrittenExpressConfig stepMod = db.T_HandwrittenExpressConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_HandwrittenExpressAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_HandwrittenExpress>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
                        int j = db.SaveChanges();
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
                        db.Entry<T_HandwrittenExpress>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
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
