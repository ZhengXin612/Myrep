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
    /// <summary>
    ///员工入职控制器
    /// </summary>
    public class JoinedController : BaseController
    {
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
        public string GetCode() {
            string code = "RZ-SQ-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            List<T_Joined> listJoined = db.T_Joined.Where(a => a.Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (listJoined.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = listJoined[0].Code.Substring(15);
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
            var history = db.T_JoinedApprove.Where(a => a.Oid == id);
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

        //给目标部门的父级部门人员字段插入人员ID方法
        public void addEmployeesToDepartmentForEidt(int pid, string sId)
        {
            //pid是目标部门的Id, sId是人员ID
            if (pid != -1)
            {
                T_Department departmentEndModel = db.T_Department.Find(pid);//找到目标门数据
                string endEm = departmentEndModel.employees;//目标部门的人员字段
                int endDepartmentParentID = int.Parse(departmentEndModel.parentId.ToString());//目标部门父ID
                string str2 = "";
                if (endEm != null && endEm != "")
                {
                    str2 += endEm + "," + sId;
                }
                else
                {
                    str2 += sId;
                }
                departmentEndModel.employees = str2;
                db.Entry<T_Department>(departmentEndModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                addEmployeesToDepartmentForEidt(endDepartmentParentID, sId);
            }
        }

        //部门名称下拉列表
        public  List<SelectListItem> DepartMentName()
        {
          
            var list = db.T_Department.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        //岗位名称下拉列表
        public  List<SelectListItem> JobName()
        {
           
            var list = db.T_EmployDemandConfig.Where(a=>a.Type=="岗位").AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==选择岗位==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        public string WorkNum()//生成员工编号
        {
            T_PersonnelFile personnel = db.T_PersonnelFile.OrderByDescending(a => a.Code).FirstOrDefault();
              string codestr="010001";
            if (personnel == null)
            {
                return codestr;
            }
            else
            {
                int code = int.Parse(personnel.Code);
                int newcode = code + 1;
                codestr = "0" + newcode;
            }
            return codestr;
        }

        //检验花名是否重复
        public JsonResult CheckName(string nickName)
        {
            T_User user = db.T_User.FirstOrDefault(a => a.Nickname == nickName);
            if (user != null)
            {
                return Json(new { State = "Faile", Message = "注意：该花名已使用" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { State = "Success", Message = "" }, JsonRequestBehavior.AllowGet);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "员工入职").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_JoinedApprove where oid in (select ID from T_Joined where(Status = -1 or Status = 0) and IsDelete = 0) and Status = -1 and ApproveTime is null GROUP BY ApproveName";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "员工入职" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "员工入职";
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
            string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_Joined where Status = '2' and IsDelete = 0  GROUP BY PostUser  ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "员工入职" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "员工入职";
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
        #endregion
        #region 视图
        public ActionResult Index()
        {
            return View();
        }
        [Description("访问入职新增页面")]
        public ActionResult ViewJoinedAdd()
        {
            //审批批号绑定
            ViewData["Code"] = GetCode();
            //用户花名绑定
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["Nickname"] = Nickname;
            //用户部门绑定
            T_User UserModel = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
            int DepartmentId = int.Parse(UserModel.DepartmentId);
            T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == DepartmentId);
            ViewData["PostDepartment"] = dModel.Name;
            //性别下拉框
             List<SelectListItem> SexOptions = new List<SelectListItem>();
             SexOptions.Add(new SelectListItem { Text = "男", Value = "男" });
             SexOptions.Add(new SelectListItem { Text = "女", Value = "女" });
             ViewData["Sex"] = SexOptions;
            //一级审核人
             ViewData["FirstApprove"] = GetFisrtNameForApprove();
             if (dModel.supervisor != null) {
                 int supervisorID = int.Parse(dModel.supervisor.ToString());
                 T_User supervisorModel = db.T_User.Find(supervisorID);
                 ViewData["MyUser"] = supervisorModel.Name;
             }
             ViewData["DepartMentName"] = DepartMentName();
             ViewData["JobName"] = JobName();
            return View();
        }
        [Description("访问入职新增页面")]
        public ActionResult GetJoinedAdd()
        {
            //审批批号绑定
            ViewData["Code"] = GetCode();
            //用户花名绑定
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            ViewData["Nickname"] = Nickname;
            //用户部门绑定
            T_User UserModel = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
            int DepartmentId = int.Parse(UserModel.DepartmentId);
            T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == DepartmentId);
            ViewData["PostDepartment"] = dModel.Name;
            //性别下拉框
            List<SelectListItem> SexOptions = new List<SelectListItem>();
            SexOptions.Add(new SelectListItem { Text = "男", Value = "男" });
            SexOptions.Add(new SelectListItem { Text = "女", Value = "女" });
            ViewData["Sex"] = SexOptions;
            //一级审核人
            ViewData["FirstApprove"] = GetFisrtNameForApprove();
            if (dModel.supervisor != null)
            {
                int supervisorID = int.Parse(dModel.supervisor.ToString());
                T_User supervisorModel = db.T_User.Find(supervisorID);
                ViewData["MyUser"] = supervisorModel.Name;
            }
            ViewData["DepartMentName"] = DepartMentName();
            ViewData["JobName"] = JobName();
            return View();
        }
        [Description("访问入职审批管理页面")]
        public ActionResult ViewJoinedList()
        {
            return View();
        }
        [Description("访问入职记录详情页面")]
        public ActionResult ViewJoinedDetail(int ID)
        {
            T_Joined model = db.T_Joined.Find(ID);
            if (model == null)  
            {
                return HttpNotFound();
            }
            GetApproveHistory(ID);
            return View(model);
        }
        [Description("访问入职审核历史页面")]
        public ActionResult ViewJoinedHistory(int ID)
        {
            T_Joined model = db.T_Joined.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(ID);
            return View(model);
        }
        [Description("访问入职审批未审核页面")]
        public ActionResult ViewJoinedUnApprove()
        {
            return View();
        }
        [Description("访问入职审核页面")]
        public ActionResult ViewJoinedCheck(int ID)
        {
            GetApproveHistory(ID);
            ViewData["approveid"] = ID;
            return View();
        }
         [Description("访问入职审批已审核页面")]
        public ActionResult ViewJoinedApproveEd()
        {
            return View();
        }
         [Description("访问入职审批编辑页面")]
         public ActionResult ViewJoinedEdit(int ID)
         {
             T_Joined model = db.T_Joined.Find(ID);
             if (model == null)
             {
                 return HttpNotFound();
             }
             ViewData["Code"] = model.Code;
             //用户花名绑定
             string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
             ViewData["Nickname"] = Nickname;
             //用户部门绑定
             T_User UserModel = db.T_User.SingleOrDefault(a => a.Nickname == Nickname);
             int DepartmentId = int.Parse(UserModel.DepartmentId);
             T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == DepartmentId);
             ViewData["PostDepartment"] = dModel.Name;
             //性别下拉框
             List<SelectListItem> SexOptions = new List<SelectListItem>();
             SexOptions.Add(new SelectListItem { Text = "男", Value = "男" });
             SexOptions.Add(new SelectListItem { Text = "女", Value = "女" });
             ViewData["Sex"] = SexOptions;
             //一级审核人
             ViewData["FirstApprove"] = GetFisrtNameForApprove();
             if (dModel.supervisor != null)
             {
                 int supervisorID = int.Parse(dModel.supervisor.ToString());
                 T_User supervisorModel = db.T_User.Find(supervisorID);
                 ViewData["MyUser"] = supervisorModel.Name;
             }
             ViewData["modelSex"] = model.Sex;
             ViewData["modelFirstApprove"] = model.FirstApprove;
             return View(model);
         }
        [Description("面试成功未入职简历页面")]
         public ActionResult ViewInterviewList()
         {
             return View();
         }
        public ActionResult ViewMyList()
        {
            return View();
        }
        #endregion
        #region Post方法
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
                        string path = "~/Upload/JoinedData/" + title;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        file.SaveAs(path);
                        link = "/Upload/JoinedData/" + title;
                        filesName = "~/Upload/JoinedData/" + title;
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
                            string path = "~/Upload/JoinedData/" + title;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            file.SaveAs(path);
                            urllist[i] = path;
                            link = "/Upload/JoinedData/" + title;
                            filesName = "~/Upload/JoinedData/" + title;
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
        public void DeleteModelFile(string path,int ID)
        {
            string strPath = path;
            path = Server.MapPath(path);
            //获得文件对象
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            if (file.Exists)
            {
                file.Delete();//删除
            }
            T_JoinedOptions model = db.T_JoinedOptions.SingleOrDefault(a => a.Oid == ID && a.Path == strPath);
            if (model != null) {
                db.T_JoinedOptions.Remove(model);
                db.SaveChanges();
            }
            //file.Create();//文件创建方法
        }
        //入职申请新增保存
        [HttpPost]
        public JsonResult JoinedAddSave(T_Joined model, string jsonStr) 
        {
            using (TransactionScope sc = new TransactionScope()) 
            {
                try
                {
                    
                    //主表保存
                    model.SetGroup = model.Department;
                    model.IsDelete = 0;
                    model.Step = 0;
                    model.Status = -1;
                    model.PostTime = DateTime.Now;
                    db.T_Joined.Add(model);
                    int i = db.SaveChanges();
                    if (i > 0) 
                    {
                       //审核保存
                        T_JoinedApprove modelApprove = new T_JoinedApprove();
                        modelApprove.Status = -1;
                        modelApprove.Memo = "";                     
                        modelApprove.Oid = model.ID;
                        modelApprove.ApproveName = model.FirstApprove;
                        db.T_JoinedApprove.Add(modelApprove);
                        db.SaveChanges();
                        //附件保存
                        if (!string.IsNullOrEmpty(jsonStr))
                        {
                            List<T_JoinedOptions> details = Deserialize<T_JoinedOptions>(jsonStr);
                            foreach (var item in details)
                            {
                                item.Oid = model.ID;
                                db.T_JoinedOptions.Add(item);
                            }
                            db.SaveChanges();
                        }

                        string Name=model.FirstApprove;
                        T_User UserModel = db.T_User.SingleOrDefault(a => a.Name == Name);


                        string[] msg = new string[] { "你有一条入职未审批：请及时审核," };
                        string res = Lib.SendSMS.Send(msg, "162067", UserModel.Tel);
                        //ModularByZP();
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
        //编辑
        [HttpPost]
        public JsonResult JoinedEditSave(T_Joined model, string jsonStr, int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    //主表修改
                    int editStatus = model.Status;//原记录的状态
                   int editID = model.ID;//原记录的ID
                    T_Joined modelJoined = db.T_Joined.Find(ID);
                    modelJoined.Address = model.Address;
                    modelJoined.Code = model.Code;
                    modelJoined.DataDrivingLicense = model.DataDrivingLicense;
                    modelJoined.DataExaminationReport = model.DataExaminationReport;
                    modelJoined.DataIdcard = model.DataIdcard;
                    modelJoined.DataPhoto = model.DataPhoto;
                    modelJoined.DataPost = model.DataPost;
                    modelJoined.DataQualifications = model.DataQualifications;
                    modelJoined.DataResult = model.DataResult;
                    modelJoined.DataUserRegister = model.DataUserRegister;
                    modelJoined.Department = model.Department;
                    modelJoined.EmergencyContact = model.EmergencyContact;
                    modelJoined.EmergencyTel = model.EmergencyTel;
                    modelJoined.FirstApprove = model.FirstApprove;
                    modelJoined.IdcardAddress = model.IdcardAddress;
                    modelJoined.Information = model.Information;
                    modelJoined.Job = model.Job;
                    modelJoined.Memo = model.Memo;
                    modelJoined.Name = model.Name;
                    modelJoined.Nickname = model.Nickname;
                    modelJoined.SetGroup = model.SetGroup;
                    modelJoined.Sex = model.Sex;
                    modelJoined.Tel = model.Tel;
                    modelJoined.Status = -1;

                    db.Entry<T_Joined>(modelJoined).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //审核保存  不同意修改 新添加一条审核记录。未审核的则不添加而是修改
                        T_JoinedApprove Approvemodel = db.T_JoinedApprove.SingleOrDefault(a => a.Oid == editID && a.ApproveTime == null);
                        if (Approvemodel == null) 
                        {
                           //不同意 修改
                            T_JoinedApprove modelApprove = new T_JoinedApprove();
                            modelApprove.Status = -1;
                            modelApprove.Memo = "";
                            modelApprove.Oid = model.ID;
                            modelApprove.ApproveName = model.FirstApprove;
                            db.T_JoinedApprove.Add(modelApprove);
                            db.SaveChanges();
                        }
                        else 
                        {
                            //新增修改
                            Approvemodel.ApproveName = model.FirstApprove;
                            db.Entry<T_JoinedApprove>(Approvemodel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //附件保存 先删除原有的附件
                        List<T_JoinedOptions> delMod = db.T_JoinedOptions.Where(a => a.Oid == editID).ToList();
                        foreach (var item in delMod)
                        {
                            db.T_JoinedOptions.Remove(item);
                        }
                        db.SaveChanges();
                        if (!string.IsNullOrEmpty(jsonStr))
                        {
                            List<T_JoinedOptions> details = Deserialize<T_JoinedOptions>(jsonStr);
                            foreach (var item in details)
                            {
                                item.Oid = model.ID;
                                db.T_JoinedOptions.Add(item);
                            }
                            db.SaveChanges();
                        }
                       // ModularByZP();
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
        //入职申请列表  
        [HttpPost]
        public ContentResult ShowJoinedList(Lib.GridPager pager, string queryStr, int status,int isMy=0)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_Joined> queryData = db.T_Joined.Where(a =>  a.IsDelete == 0);
            if (isMy == 1)
            {
                queryData = queryData.Where(a => a.PostUser == name);
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Nickname != null && a.Nickname.Contains(queryStr)) || (a.Name != null && a.Name.Contains(queryStr)));
            }
            if (status != 9999)
            {
                queryData = queryData.Where(a => a.Status == status);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Joined> list = new List<T_Joined>();
            foreach (var item in queryData)
            {
                T_Joined i = new T_Joined();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //获取附件
        public JsonResult GetJoinedOptin(int ID)  
        {
            List<T_JoinedOptions> model = db.T_JoinedOptions.Where(a => a.Oid == ID).ToList();
            string options = "";
            if(model.Count>0){
             options+="[";
                 foreach (var item in model)
                 {
                     options += "{\"Name\":\"" + item.Name + "\",\"Url\":\"" + item.Url + "\",\"Size\":\"" + item.Size + "\",\"Path\":\"" + item.Path + "\"},";
                  }
             options = options.Substring(0, options.Length - 1);
             options += "]";
            }
            return Json(options,JsonRequestBehavior.AllowGet);
        }
        //虚拟删除入职审批 
        [HttpPost]
        [Description("删除入职审批")]
        public JsonResult DeleteJoined(int ID)
        {
            T_Joined model = db.T_Joined.Find(ID);
            model.IsDelete = 1;
            db.Entry<T_Joined>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            //ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //作废入职审批
        [HttpPost]
        [Description("作废入职审批")]
        public JsonResult VoidJoined(int ID)
        {
            T_Joined model = db.T_Joined.Find(ID);
            model.Status = 3;
            db.Entry<T_Joined>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            //ModularByZP();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //待审核数据列表
        [HttpPost]
        public ContentResult UnCheckJoinedList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_JoinedApprove> ApproveMod = db.T_JoinedApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Joined> queryData = from r in db.T_Joined
                                                 where Arry.Contains(r.ID) && r.IsDelete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                                 select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Code != null && a.Code.Contains(queryStr) || a.PostUser != null && a.PostUser.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_Joined> list = new List<T_Joined>();
            foreach (var item in queryData)
            {
                T_Joined i = new T_Joined();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list,Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //已审核
        [HttpPost]
        public ContentResult CheckedJoinedList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            List<T_JoinedApprove> ApproveMod = db.T_JoinedApprove.Where(a => a.ApproveName == name && a.ApproveTime != null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Joined> queryData = from r in db.T_Joined
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
            List<T_Joined> list = new List<T_Joined>();
            foreach (var item in queryData)
            {
                T_Joined i = new T_Joined();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list,Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
       //审核操作
        [HttpPost]
        [Description("报损审核")]
        public JsonResult JoinedCheck(int id, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_JoinedApprove modelApprove = db.T_JoinedApprove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);
                    string curName = Server.UrlDecode(Request.Cookies["Name"].Value);
                    string result = "";
                    if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                    modelApprove.Memo = memo;
                    modelApprove.ApproveTime = DateTime.Now;
                    modelApprove.Status = status;
                    db.Entry<T_JoinedApprove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        T_Joined model = db.T_Joined.Find(id);
                        T_JoinedApprove newApprove = new T_JoinedApprove();
                        if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                        if (status == 1)
                        {
                            //同意
                            int step = int.Parse(model.Step.ToString());
                            step++;
                            IQueryable<T_JoinedApproveConfig> config = db.T_JoinedApproveConfig.AsQueryable();
                            int stepLength = config.Count();//总共步骤
                            if (step < stepLength)
                            {
                                //不是最后一步，主表状态为0 =>审核中
                                model.Status = 0;
                                T_JoinedApproveConfig stepMod = db.T_JoinedApproveConfig.SingleOrDefault(a => a.Step == step);
                                string nextName = stepMod.Name;
                                //下一步审核人不是null  审核记录插入一条新纪录
                                newApprove.Memo = "";
                                newApprove.Oid = id;
                                newApprove.Status = -1;
                                if (nextName != null)
                                {
                                    newApprove.ApproveName = nextName;
                                }
                                db.T_JoinedApprove.Add(newApprove);

                                //T_JoinedApproveConfig stepMods = db.T_JoinedApproveConfig.SingleOrDefault(a => a.Step == step);
                                //string Name = stepMods.Name;
                                //发送短信给下一步审核人
                                T_User UserModel = db.T_User.SingleOrDefault(a => a.Name == nextName);
                                if (UserModel != null && UserModel.Tel != null)
                                {
                                    string[] msg = new string[] { "你有一条入职未审批：请及时审核," };
                                    string res = Lib.SendSMS.Send(msg, "162067", UserModel.Tel);
                                }

                               
                                db.SaveChanges();
                            }
                            else
                            {
                                //最后一步，主表状态改为 1 => 同意
                                model.Status = status;

                                T_Department deptModel = db.T_Department.FirstOrDefault(a => a.Name == model.Department);//找到部门
                                if (deptModel == null)//如果没有就放在好护士下
                                {
                                    deptModel = db.T_Department.Find(1);
                                }
                               T_User used = db.T_User.FirstOrDefault(a => a.Nickname == model.Nickname);//入职时该员工未建立账号的
                                T_User newUser = new T_User();//新建一个账号
                                int UserId = 0;
                                if (used == null)
                                {

                                    newUser.DepartmentId = deptModel.ID.ToString();
                                    newUser.IsManagers = "0";
                                    newUser.Name = model.Name;
                                    newUser.Nickname = model.Nickname;
                                    newUser.PassWord = "123456";
                                    newUser.Power = "";
                                    newUser.Tel = model.Tel;
                                    db.T_User.Add(newUser);
                                    db.SaveChanges();

                                    string uIDstr = "";//给部门添加员工ID
                                    uIDstr = "," + newUser.ID;
                                    deptModel.employees += uIDstr;
                                    db.Entry<T_Department>(deptModel).State = System.Data.Entity.EntityState.Modified;
                                    addEmployeesToDepartmentForEidt(deptModel.ID, newUser.ID.ToString());
                                    UserId = newUser.ID;
                                }
                                else
                                {
                                    UserId = used.ID;
                                }
                                if (model.PID > 0)//修改档案信息
                                {
                                    T_PersonnelFile pModel = db.T_PersonnelFile.Find(model.PID);
                                    pModel.Department = model.Department;
                                    pModel.Job = model.Job;
                                    pModel.OnJob = 0;
                                    pModel.oid = UserId;
                                    pModel.Hiredate = model.Hiredate;
                                    pModel.Probation = model.Probation;
                                    pModel.ProbationSalary = model.ProbationSalary;
                                    // pModel.Salary = model.Salary;
                                    pModel.ContractFirstDeadline = model.ContractFirstDeadline;
                                    pModel.ContractFirstStartTime = model.ContractFirstStartTime;
                                    pModel.Code= WorkNum();
                                    string DataStr = "";
                                    #region 给DataStr赋值(入职资料)
                                    if (model.DataUserRegister)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "员工登记";
                                        }
                                        else
                                        {
                                            DataStr += ",员工登记";
                                        }
                                    }
                                    if (model.DataPost)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "应聘申请";
                                        }
                                        else
                                        {
                                            DataStr += ",应聘申请";
                                        }
                                    }

                                    if (model.DataResult)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "简历";
                                        }
                                        else
                                        {
                                            DataStr += ",简历";
                                        }
                                    }
                                    if (model.DataIdcard)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "身份证复印";
                                        }
                                        else
                                        {
                                            DataStr += ",身份证复印";
                                        }
                                    }
                                    if (model.DataQualifications)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "学历证书";
                                        }
                                        else
                                        {
                                            DataStr += ",学历证书";
                                        }
                                    }
                                    if (model.DataPhoto)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "寸照";
                                        }
                                        else
                                        {
                                            DataStr += ",寸照";
                                        }
                                    }
                                    if (model.DataDrivingLicense)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "驾驶证";
                                        }
                                        else
                                        {
                                            DataStr += ",驾驶证";
                                        }
                                    }
                                    if (model.DataExaminationReport)
                                    {
                                        if (DataStr == "")
                                        {
                                            DataStr += "体检报告";
                                        }
                                        else
                                        {
                                            DataStr += ",体检报告";
                                        }
                                    }
                                    #endregion
                                    pModel.EntryData = DataStr;
                                    pModel.NickName = model.Nickname;
                                    db.Entry<T_PersonnelFile>(pModel).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                            model.Step = step;
                            db.Entry<T_Joined>(model).State = System.Data.Entity.EntityState.Modified;
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
                            db.Entry<T_Joined>(model).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //审核流程结束 申请人编辑后插入下一条记录 
                            result = "保存成功";
                        }
                    }
                    else
                    {
                        result = "保存失败";
                    }
                   // ModularByZP();
                    sc.Complete();
                    
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json("保存失败", JsonRequestBehavior.AllowGet);
                }
            }
           
        }

        public ContentResult GetInterviewList(Lib.GridPager pager, string queryStr)
        {
           
            IQueryable<T_PersonnelFile> queryData = db.T_PersonnelFile.Where(a=>a.InterviewState==1&&a.OnJob!=0);
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.TrueName != null && a.TrueName.Contains(queryStr));
            }
            List<T_PersonnelFile> list = queryData.OrderBy(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            pager.totalRows = queryData.Count();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion
    }
}
