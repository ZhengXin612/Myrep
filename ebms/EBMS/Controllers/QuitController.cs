using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EBMS.Controllers
{
    public class QuitController : BaseController
    {
        /// <summary>
        ///员工离职控制器
        /// </summary>
         #region 公共方法
         EBMSEntities db = new EBMSEntities();
         //一级主管
         public List<SelectListItem> GetFisrtNameForApprove()
         {
             var list = db.T_User.Where(a => a.IsManagers == "1").AsQueryable();
             var selectList = new SelectList(list, "Name", "Name");
             List<SelectListItem> selecli = new List<SelectListItem>();
             selecli.AddRange(selectList);
             return selecli;
         }
         //审批批号绑定
         public string GetCode()
         {
             string code = "LS-BG-";
             string date = DateTime.Now.ToString("yyyyMMdd");
             List<T_Quit> listQuit = db.T_Quit.Where(a => a.Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
             if (listQuit.Count == 0)
             {
                 code += date + "-" + "0001";
             }
             else
             {
                 string old = listQuit[0].Code.Substring(15);
                 int newcode = int.Parse(old) + 1;
                 code += date + "-" + newcode.ToString().PadLeft(4, '0');
             }
             return code;
         }
         //接收JSON 反序列化
         public static List<T> Deserialize<T>(string text)
         {
             try
             {
                 JavaScriptSerializer js = new JavaScriptSerializer();
                 List<T> list = (List<T>)js.Deserialize(text, typeof(List<T>));
                 return list;
             }
             catch (Exception e)
             {
                 return null;
             }
         }
         //获取审核详情记录
         private void GetApproveHistory(int id = 0)
         {
             var history = db.T_QuitApprove.Where(a => a.Oid == id);
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
         //我的固定资产
         public string GetMyAssets(string Name) 
         {
             string myAssets = "";
             List<T_Assets> modelAssets = db.T_Assets.Where(a => a.Owner == Name && a.isDelete == "0").ToList();
             if (modelAssets.Count != 0)
             {
                 myAssets += "[";
                 foreach (var item in modelAssets)
                 {
                     myAssets += "{\"Code\":\"" + item.Code + "\",\"Name\":\"" + item.Name + "\",\"Spec\":\"" + item.Spec + "\",\"Cost\":\"" + item.Cost + "\",\"isScrap\":\"" + item.isScrap + "\"},";
                 }
                 myAssets = myAssets.Substring(0, myAssets.Length - 1);
                 myAssets += "]";
             }
             return myAssets;
         }
        //我的借支
         public string GetMyBorrow(string Name) 
         {
             List<T_Borrow> borrowModel = db.T_Borrow.Where(a => a.BorrowName == Name && a.BorrowState == 1 && a.BorrowSettementState != 1).ToList();
             string myBorrow = "";
             if (borrowModel.Count != 0)
             {
                 myBorrow += "[";
                 foreach (var item in borrowModel)
                 {
                     myBorrow += "{\"Code\":\"" + item.BorrowCode + "\",\"Date\":\"" + item.BorrowDate + "\",\"Money\":\"" + item.BorrowMoney + "\",\"Reson\":\"" + item.BorrowReason + "\",\"State\":\"" + item.BorrowSettementState + "\"},";
                 }
                 myBorrow = myBorrow.Substring(0, myBorrow.Length - 1);
                 myBorrow += "]";
             }
             return myBorrow;
         }
         #endregion
         #region 视图
         public ActionResult Index()
         {
             return View();
         }
        [Description("访问我的离职页面")]
         public ActionResult ViewMyQuit()
         {
             return View();
         }
        [Description("访问离职审批管理页面")]
        public ActionResult ViewQuitList()
        {
            return View();
        }
        [Description("访问离职新增页面")]
         public ActionResult QuitAdd()
         {
             //审批批号绑定
             ViewData["Code"] = GetCode();
             //用户花名绑定
             string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
             ViewData["Nickname"] = Nickname;
             ViewData["Name"] = Name;
            //一个用户只有 未作废和未删除状态以外 的 离职记录 只能有一条;
             IQueryable<T_Quit> modelQuit = db.T_Quit.Where(a=>a.PostUser == Name && a.Status!=3 &&a.IsDelete != 1);
             List<T_Quit> listQuit = modelQuit.ToList();
             if (listQuit.Count > 0) 
             {
                 ViewData["QLength"] = 1;
                
             }
             //用户部门绑定
             T_User UserModel = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
             int DepartmentId = int.Parse(UserModel.DepartmentId);
             T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == DepartmentId);
             ViewData["PostDepartment"] = dModel.Name;
             //一级审核人
             ViewData["FirstApprove"] = GetFisrtNameForApprove();
             if (dModel.supervisor != null)
             {
                 int supervisorID = int.Parse(dModel.supervisor.ToString());
                 T_User supervisorModel = db.T_User.Find(supervisorID);
                 ViewData["MyUser"] = supervisorModel.Name;
             }
            //我的固定资产
             ViewData["Assets"] = GetMyAssets(Name);
            //我的借支
             ViewData["Borrow"] = GetMyBorrow(Nickname);
             return View();
         }
           [Description("访问离职编辑页面")]
        public ActionResult ViewQuitEdit(int ID)
         {
             T_Quit MOD = db.T_Quit.Find(ID);
             if (MOD == null)   
             {
                 return HttpNotFound();
             }
             //审批批号绑定
             ViewData["Code"] = MOD.Code;
             //用户花名绑定
             string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             string Name = Server.UrlDecode(Request.Cookies["Name"].Value);
             ViewData["Nickname"] = Nickname;
             ViewData["Name"] = Name;
             //用户部门绑定
             T_User UserModel = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
             int DepartmentId = int.Parse(UserModel.DepartmentId);
             T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == DepartmentId);
             ViewData["PostDepartment"] = dModel.Name;
             //一级审核人
             ViewData["FirstApprove"] = GetFisrtNameForApprove();
             ViewData["MyUser"] = MOD.FirstApprove;
             //离职日期
             ViewData["Time"] = MOD.QuitTime;
            //我的固定资产
             ViewData["Assets"] = GetMyAssets(Name);
            //我的借支
             ViewData["Borrow"] = GetMyBorrow(Nickname);
             return View(MOD);
         }
        
        [Description("访问离职详情页面")]
        public ActionResult ViewQuitDetail(int ID)
         {
             
             T_Quit model = db.T_Quit.Find(ID);
             if (model == null)
             {
               return  HttpNotFound();
             }
             string Name = model.PostUser;
             T_User userModel = db.T_User.SingleOrDefault(a=>a.Name==Name);
             string Nickname = userModel.Nickname;
             //我的固定资产
             ViewData["Assets"] = GetMyAssets(Name);
             //我的借支
             ViewData["Borrow"] = GetMyBorrow(Nickname);
             GetApproveHistory(ID);
             return View(model);
         }
        [Description("访问离职未审核页面")]
        public ActionResult ViewQuitUnApprove()
         {
             return View();
          }
        [Description("访问离职审核历史页面")]
        public ActionResult ViewQuitHistory(int ID)
        {
            T_Quit model = db.T_Quit.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(ID);
            return View(model);
        }
        [Description("访问离职审核页面")]
        public ActionResult ViewQuitCheck(int ID)
        {
            T_Quit model = db.T_Quit.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(ID);
            ViewData["approveid"] = ID;
            return View(model);
        }
        [Description("访问离职审批已审核页面")]
        public ActionResult ViewQuitApproveEd()
        {
            return View();
        }
        #endregion
        #region Post 方法
        //入职附件上传
        [HttpPost]
        public JsonResult Upload()
        {
            string link = "";
            string filesName = "";
            if (Request.Files.Count > 0)
            {
                if (Request.Files.Count == 1)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        string title = string.Empty;
                        title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                        string path = "~/Upload/QuitData/" + title;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        file.SaveAs(path);
                        link = "/Upload/QuitData/" + title;
                        filesName = "~/Upload/QuitData/" + title;
                        return Json(new { status = true, url = path, link = link, title = filesName });
                    }
                }
                else
                {
                    string[] urllist = new string[Request.Files.Count];

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file.ContentLength > 0)
                        {
                            string title = string.Empty;
                            title = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);
                            string path = "~/Upload/QuitData/" + title;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            file.SaveAs(path);
                            urllist[i] = path;
                            link = "/Upload/QuitData/" + title;
                            filesName = "~/Upload/QuitData/" + title;
                        }
                    }
                    return Json(new { status = true, url = urllist, link = link, title = filesName });
                }
            }
            else
            {
                return Json(new { status = false, url = "", msg = "没有文件" });
            }
            return Json(new { status = false, url = "", msg = "" });
        }
        //入职附件删除
        public void DeleteFile(string path)
        {
            path = Server.MapPath(path);
            //获得文件对象
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }

            //file.Create();//文件创建方法
        }
        //入职附件删除
        public void DeleteModelFile(string path, int ID)
        {
            string strPath = path;
            path = Server.MapPath(path);
            //获得文件对象
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }
            T_QuitOptions model = db.T_QuitOptions.SingleOrDefault(a => a.Oid == ID && a.Path == strPath);
            if (model != null)
            {
                db.T_QuitOptions.Remove(model);
                db.SaveChanges();
            }
            //file.Create();//文件创建方法
        }
        //离职新增保存
        [HttpPost]
        public JsonResult QuitAddSave(T_Quit model, string jsonStr)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    //主表保存
                    model.IsDelete = 0;
                    model.Step = 0;
                    model.Status = -1;
                    model.PostTime = DateTime.Now;
                    db.T_Quit.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //审核保存
                        T_QuitApprove modelApprove = new T_QuitApprove();
                        modelApprove.Status = -1;
                        modelApprove.Memo = "";
                        modelApprove.Oid = model.ID;
                        modelApprove.ApproveName = model.FirstApprove;
                        db.T_QuitApprove.Add(modelApprove);
                        db.SaveChanges();
                        //附件保存
                        if (!string.IsNullOrEmpty(jsonStr))
                        {
                            List<T_QuitOptions> details = Deserialize<T_QuitOptions>(jsonStr);
                            foreach (var item in details)
                            {
                                item.Oid = model.ID;
                                db.T_QuitOptions.Add(item);
                            }
                            db.SaveChanges();
                        }
                        ModularByZL();
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

        public void ModularByZL()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "离职管理").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }
            string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_QuitApprove where Oid in (select ID from T_Quit where  IsDelete='0'  and (Status = -1 or Status = 0 ) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "离职管理" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "离职管理";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_Quit where Status='2' and IsDelete=0 GROUP BY PostUser ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "离职管理" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "离职管理";
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
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //我的离职列表数据绑定  
        [HttpPost]
        public ContentResult ShowMyQuit(Lib.GridPager pager, string queryStr, int status)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            IQueryable<T_Quit> queryData = db.T_Quit.Where(a => a.PostUser == name && a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Quit> list = new List<T_Quit>();
            foreach (var item in queryData)
            {
                T_Quit i = new T_Quit();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //离职申请列表  
        [HttpPost]
        public ContentResult ShowQuitList(Lib.GridPager pager, string queryStr, int status)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            IQueryable<T_Quit> queryData = db.T_Quit.Where(a => a.IsDelete == 0);
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || (a.PostUser != null && a.PostUser.Contains(queryStr)));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Quit> list = new List<T_Quit>();
            foreach (var item in queryData)
            {
                T_Quit i = new T_Quit();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //获取附件
        public JsonResult GetQuitOptin(int ID)
        {
            List<T_QuitOptions> model = db.T_QuitOptions.Where(a => a.Oid == ID).ToList();
            string options = "";
            if (model.Count > 0)
            {
                options += "[";
                foreach (var item in model)
                {
                    options += "{\"Name\":\"" + item.Name + "\",\"Url\":\"" + item.Url + "\",\"Size\":\"" + item.Size + "\",\"Path\":\"" + item.Path + "\"},";
                }
                options = options.Substring(0, options.Length - 1);
                options += "]";
            }
            return Json(options, JsonRequestBehavior.AllowGet);
        }
        //虚拟删除入职审批 
        [HttpPost]
        [Description("删除离职审批")]
        public JsonResult DeleteQuit(int ID)
        {
            T_Quit model = db.T_Quit.Find(ID);
            model.IsDelete = 1;
            db.Entry<T_Quit>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            ModularByZL();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //作废入职审批
        [HttpPost]
        [Description("作废入职审批")]
        public JsonResult VoidQuit(int ID)
        {
            T_Quit model = db.T_Quit.Find(ID);
            model.Status = 3;
            db.Entry<T_Quit>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            ModularByZL();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //待审核数据列表
        [HttpPost]
        public ContentResult UnCheckQuitList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_QuitApprove> ApproveMod = db.T_QuitApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Quit> queryData = from r in db.T_Quit
                                             where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                             select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Quit> list = new List<T_Quit>();
            foreach (var item in queryData)
            {
                T_Quit i = new T_Quit();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //审核操作
        [HttpPost]
        [Description("报损审核")]
        public JsonResult QuitCheck(int id, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                T_QuitApprove modelApprove = db.T_QuitApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
                string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                 string nikeName = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                string result = "";
                if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                modelApprove.Memo = memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = status;
                db.Entry<T_QuitApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    T_Quit model = db.T_Quit.Find(id);
                    T_QuitApprove newApprove = new T_QuitApprove();
                    if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                    if (status == 1)
                    {
                        //同意
                        int step = int.Parse(model.Step.ToString());
                        step++;
                        IQueryable<T_QuitApproveConfig> config = db.T_QuitApproveConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            //不是最后一步，主表状态为0 =>审核中
                            model.Status = 0;
                            T_QuitApproveConfig stepMod = db.T_QuitApproveConfig.SingleOrDefault(a => a.Step == step);
                            string nextName = stepMod.Name;
                            //下一步审核人不是null  审核记录插入一条新纪录
                            newApprove.Memo = "";
                            newApprove.Oid = id;
                            newApprove.Status = -1;
                            if (nextName != null)
                            {
                                newApprove.ApproveName = nextName;
                            }
                            db.T_QuitApprove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            //最后一步，主表状态改为 1 => 同意
                            model.Status = status;
                            //人事档案改为离职 
                            T_PersonnelFile MOD_Person = db.T_PersonnelFile.FirstOrDefault(a => a.TrueName == model.PostUser);
                            if (MOD_Person == null) {
                                return Json("保存失败", JsonRequestBehavior.AllowGet);
                            }
                                MOD_Person.OnJob = 1;
                                MOD_Person.QuitDate = DateTime.Now.ToString();
                                db.Entry<T_PersonnelFile>(MOD_Person).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                        }
                        model.Step = step;
                        db.Entry<T_Quit>(model).State = System.Data.Entity.EntityState.Modified;
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
                        model.Step = 0;
                        model.Status = 2;
                        db.Entry<T_Quit>(model).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }
                ModularByZL();
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
           
        }
        //已审核
        [HttpPost]
        public ContentResult CheckedQuitList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_QuitApprove> ApproveMod = db.T_QuitApprove.Where(a => a.ApproveName == name && a.ApproveTime != null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Quit> queryData = from r in db.T_Quit
                                             where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != -1
                                             select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Quit> list = new List<T_Quit>();
            foreach (var item in queryData)
            {
                T_Quit i = new T_Quit();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //编辑
        [HttpPost]
        public JsonResult QuitEditSave(T_Quit model, string jsonStr, int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    //主表修改
                    int editStatus = model.Status;//原记录的状态
                    int editID = model.ID;//原记录的ID
                    T_Quit modelQuit = db.T_Quit.Find(ID);
                    modelQuit.Memo = model.Memo;
                    modelQuit.PostUser = model.PostUser;
                    modelQuit.QuitTime = model.QuitTime;
                    modelQuit.AssetsCode = model.AssetsCode;
                    modelQuit.BorrowCode = model.BorrowCode;
                    modelQuit.FirstApprove = model.FirstApprove;
                    modelQuit.QuitTime = model.QuitTime;
                    modelQuit.Reason = model.Reason;
                    modelQuit.WageAccount = model.WageAccount;
                    modelQuit.WageAccountName = model.WageAccountName;
                    modelQuit.WageBank = model.WageBank;
                    modelQuit.WorkAccount = model.WorkAccount;
                    db.Entry<T_Quit>(modelQuit).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //审核保存  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                       T_QuitApprove Approvemodel = db.T_QuitApprove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (Approvemodel == null)
                        {
                            //不同意 修改
                            T_QuitApprove modelApprove = new T_QuitApprove();
                            modelApprove.Status = -1;
                            modelApprove.Memo = "";
                            modelApprove.Oid = model.ID;
                            modelApprove.ApproveName = model.FirstApprove;
                            db.T_QuitApprove.Add(modelApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            //新增修改
                            Approvemodel.ApproveName = model.FirstApprove;
                            db.Entry<T_QuitApprove>(Approvemodel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //附件保存 先删除原有的附件
                        List<T_QuitOptions> delMod = db.T_QuitOptions.Where(a => a.Oid == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_QuitOptions.Remove(item);
                        }
                        db.SaveChanges();
                        if (!string.IsNullOrEmpty(jsonStr))
                        {
                            List<T_QuitOptions> details = Deserialize<T_QuitOptions>(jsonStr);
                            foreach (var item in details)
                            {
                                item.Oid = model.ID;
                                db.T_QuitOptions.Add(item);
                            }
                            db.SaveChanges();
                        }
                        ModularByZL();

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
        #endregion
    }
}
