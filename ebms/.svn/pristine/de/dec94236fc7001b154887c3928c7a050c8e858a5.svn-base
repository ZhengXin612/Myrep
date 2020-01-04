using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace EBMS.Controllers
{
    /// <summary>
    /// 系统设置 参数
    /// </summary>
    public class SystemController : BaseController
    {
        //
        // GET: /System/
        #region 视图
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        //部门ID转换中文名
        public string GetDaparementString(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int cid = int.Parse(id);
                List<T_Department> model = db.T_Department.Where(a => a.ID == cid).ToList();
                if (model.Count > 0)
                {
                    return model[0].Name;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
         //角色ID转换中文名
        public string GetPowerString(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int cid = int.Parse(id);
                List<T_Role> model = db.T_Role.Where(a => a.ID == cid).ToList();
                if (model.Count > 0)
                {
                    return model[0].Name;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        [Description("访问用户管理页面")]
        public ActionResult ViewUserList()
        {
            string treeData = DepartmentTree(-1);
            ViewData["tree"] = treeData;
            return View();
        }
        [Description("访问角色管理页面")]
        public ActionResult ViewRoleList()
        {
            return View();
        }
        //角色根权限下拉框
        public List<SelectListItem> GetUserPower()
        {
            var list = db.T_SysModule.Where(a => a.ParentId == "-1");
            var selectList = new SelectList(list, "IdName", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.AddRange(selectList);
            return selecli;
        }
        [Description("访问角色新增页面")]
        public ActionResult ViewRoleAdd()
        {
            ViewData["Power"] = GetUserPower();
            return View();
        }
        //角色编辑视图
        [Description("访问角色编辑页面")]
        public ActionResult ViewRoleEdit(int id)
        {
            T_Role model = db.T_Role.Find(id);
            ViewData["Power"] = GetUserPower();
            ViewData["ID"] = id;
            string arry = model.Access;
            if (arry != null && arry != "")
            {
                string[] AccessArry = model.Access.Split(',');
                string Access = "{";
                foreach (var item in AccessArry)
                {
                    int _id = int.Parse(item);
                    T_SysModule sysMod = db.T_SysModule.Find(_id);
                    if (sysMod != null) {
                        List<T_SysModule> accessMod = db.T_SysModule.Where(a => a.ParentId == sysMod.IdName).ToList();
                        if (accessMod.Count == 0)
                        {
                            Access += "\"" + sysMod.ID + "\":\"" + sysMod.RootName + "\",";
                        }
                    }
                }
                Access = Access.Substring(0, Access.Length - 1);
                Access += "}";
                ViewData["AccessArry"] = Access;
            }
            else
            {
                ViewData["AccessArry"] = "";
            }
            return View(model);
        }
        //部门新增
        [Description("访问部门新增页面")]
        public ActionResult ViewCompanyAdd(int Pid)
        {
            ViewData["Pid"] = Pid;
            return View();
        }
        //部门编辑
         [Description("访问部门编辑页面")]
        public ActionResult ViewDepartmentEdit(int Pid)
        {
            T_Department model = db.T_Department.SingleOrDefault(a => a.ID == Pid);
            ViewData["DepartmentName"] = model.Name;
            ViewData["ThisId"] = Pid;
            string parentId = model.parentId.ToString();
            ViewData["parentDepartmentName"] = GetDaparementString(parentId);
            ViewData["parentDepartmentId"] = model.parentId;
            string tree = DepartmentTree(-1);
            ViewData["tree"] = tree;
            return View(model);
        }
         //人员新增 
         [Description("访问人员新增页面")]
         public ActionResult ViewEmployeesAdd(int Pid)
         {
             ViewData["Pid"] = Pid;
             var list = db.T_Role.AsEnumerable();
             var selectList = new SelectList(list, "Id", "Name");
             List<SelectListItem> selecli = new List<SelectListItem>();
             selecli.Add(new SelectListItem { Text = "请选择角色", Value = "9999" });
             ViewData["Power"] = selecli;
             selecli.AddRange(selectList);
             List<SelectListItem> options = new List<SelectListItem>();
             options.Add(new SelectListItem { Text = "否", Value = "0" });
             options.Add(new SelectListItem { Text = "是", Value = "1" });
             ViewData["options"] = options;
             return View();
         }
         //人员编辑 
         [Description("访问人员编辑页面")]
         public ActionResult ViewEmployeesEdit(int id)
        {
            T_User model = db.T_User.Find(id);
            if (model == null) {
                return HttpNotFound();
            }
            ViewData["isBoss"] = model.IsManagers;
            ViewData["sDepartmenID"] = GetDaparementString(model.DepartmentId);
            ViewData["DepartmenID"] = model.DepartmentId;
            var list = db.T_Role.AsEnumerable();
            var selectList = new SelectList(list, "Id", "Name", model.Power);

            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "请选择角色", Value = "9999" });
            ViewData["Power"] = selecli;
            selecli.AddRange(selectList);

            List<SelectListItem> options = new List<SelectListItem>();
            options.Add(new SelectListItem { Text = "否", Value = "0" });
            options.Add(new SelectListItem { Text = "是", Value = "1" });
            ViewData["options"] = options;

            string Tree = DepartmentTree(-1);
            ViewData["tree"] = Tree;
            ViewData["oId"] = id;
            return View(model);
        }
       
        [Description("公告通知新增")]
         public ActionResult ViewNoticeAdd()
         {
             List<SelectListItem> power = new List<SelectListItem> {
                new SelectListItem { Text = "全部", Value = "1",Selected = true},
                new SelectListItem { Text = "药健康", Value = "2" },
                new SelectListItem { Text = "好护士", Value = "3" },
              
             };
             List<SelectListItem> viewtype = new List<SelectListItem> {
               
                new SelectListItem { Text = "规章制度", Value = "1",Selected = true },
                new SelectListItem { Text = "通知", Value = "2" },
              
             };
             ViewData["power"] = power;
             ViewData["type"] = viewtype;
             return View();
         }
         [Description("公告通知列表")]
         public ActionResult ViewNoticeList(string Viewtype="0")
         {
             ViewData["Viewtype"] = Viewtype;
             return View();
         }
         public ActionResult ViewNoticeDetails(int id)
         {
             T_RulesNotice ruleModel = db.T_RulesNotice.Find(id);
             if (ruleModel == null)
             {
                 return HttpNotFound();
             }
             else
             {
                 string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                 T_RuleReader readModel = db.T_RuleReader.SingleOrDefault(a => a.RID == id && a.Reader == Nickname);
                 if (readModel == null)
                 {
                     T_RuleReader readmodel = new T_RuleReader();
                     readmodel.RID = id;
                     readmodel.ReadTime = DateTime.Now;
                     readmodel.Reader = Nickname;
                     db.T_RuleReader.Add(readmodel);
                     db.SaveChanges();
                 }
                 List<T_RuleReader> readList = db.T_RuleReader.Where(a => a.RID == id).ToList();
                 ViewData["Times"] = readList.Count;
                 //ViewBag.rule = ruleModel;
                 return View(ruleModel);
             }
         }
         public string getDept(string id)
         {
             int ID = Convert.ToInt32(id);
             T_Department dept = db.T_Department.Find(ID);
             string deptName = dept.Name;
             return deptName;
         }
         public ActionResult ViewNoticeReader(int id)
         {
             List<T_RuleReader> readList = db.T_RuleReader.Where(a => a.RID == id).ToList();
             if (readList.Count > 0)
             {
                 ViewBag.reader = readList;
             }
             else
             {
                 ViewBag.reader = null;
             }
             List<T_User> wei = db.Database.SqlQuery<T_User>("select*  from T_User where NickName not in (select Reader from T_RuleReader where RID =" + id + ")").ToList();
             for (int w = 0; w < wei.Count; w++)
             {
                 if (wei[w].DepartmentId != null)
                 {
                     wei[w].DepartmentId = getDept(wei[w].DepartmentId);
                 }

             }
             if (wei.Count > 0)
             {
                 ViewBag.unreader = wei;
             }
             else
             {
                 ViewBag.unreader = null;
             }
             return View();
         }
         public ActionResult ViewNoticeManagement()
         {
             return View();
         }
         public ActionResult ViewNoticeEdit(int id)
         {
             T_RulesNotice model = db.T_RulesNotice.Find(id);
             if (model == null)
             {
                 return HttpNotFound();
             }
             else
             {
                 List<SelectListItem> power = new List<SelectListItem> {
                new SelectListItem { Text = "全部", Value = "1",Selected = true},
                new SelectListItem { Text = "药健康", Value = "2" },
                new SelectListItem { Text = "好护士", Value = "3" },
              
             };
                 List<SelectListItem> viewtype = new List<SelectListItem> {
               
                new SelectListItem { Text = "规章制度", Value = "1",Selected = true },
                new SelectListItem { Text = "通知", Value = "2" },
              
             };
                 ViewData["power"] = power;
                 ViewData["type"] = viewtype;
                 return View(model);
             }

         }
        #endregion
        #region 角色 操作方法
        //角色ID组转中文
        public string GetRoleAccess(string ids)
        {
            if (ids != "" && ids != null)
            {
                string[] list = ids.Split(',');
                string name = "";
                for (int i = 0; i < list.Length; i++)
                {
                    int ID = int.Parse(list[i]);
                    var model = db.T_SysModule.SingleOrDefault(a => a.ID == ID);
                    if (model != null)
                        name += model.Name + ",";
                }
                if (string.IsNullOrWhiteSpace(name))
                    return "";
                else
                    return name.Substring(0, name.Length - 1);
            }
            else
            {
                return "";
            }
        }
        class Role {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Access { get; set; }
        }

        //角色列表
        [HttpPost]
        public ContentResult ShowRoleList(Lib.GridPager pager, string queryStr, string queryStr2)
        {
            string json = null;
         
            IQueryable<T_Role> queryData = null;
      
            queryData = db.T_Role;

            if (queryData != null )
            {
                if (!string.IsNullOrEmpty(queryStr))
                {
                    queryData = queryData.Where(a => a.Name != null && a.Name.Contains(queryStr));
                }
                if (!string.IsNullOrEmpty(queryStr2))
                {
                    List<Role> resultMod = new List<Role>();
                   T_SysModule modSys = db.T_SysModule.FirstOrDefault(a => a.Name==queryStr2);
                   if (modSys != null)
                   {
                       string strID = modSys.ID.ToString();
                       queryData = queryData.Where(a => a.Access.Contains(strID));
                   
                       foreach (var item in queryData)
                       {
                           Role mod = new Role();
                           ArrayList _Access = new ArrayList(item.Access.Split(','));
                           if (_Access.Contains(strID))
                           {
                               mod.ID = item.ID;
                               mod.Name = item.Name;
                               mod.Access = item.Access;
                               resultMod.Add(mod);
                           }
                       }
                   }
                   else {
                       queryData = queryData.Where(a => a.Name != null && a.Name.Contains(queryStr));
                   }
                      //fy
                   pager.totalRows = resultMod.Count();
                   resultMod = resultMod.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                      json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(resultMod) + "}";
                      return Content(json);
                 

                }
                pager.totalRows = queryData.Count();
                //分页
                queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);


                json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData) + "}";
                
            }
            else
            {
                json = "";
            }
            return Content(json);
        }
        //左侧导航树的JSON
        public JsonResult getTreeData(string pId)
        {
            string strData = string.Empty;
            if (!String.IsNullOrEmpty(pId))
            {
                List<T_SysModule> modelList = db.T_SysModule.Where(a => a.ParentId == pId).OrderBy(a=>a.Sort).ToList();
                T_SysModule root = db.T_SysModule.SingleOrDefault(a => a.IdName == pId);
                strData += "[{\"id\":" + root.ID + ",\"text\":\"" + root.Name + "\",\"checked\":" + "false" + ",children:";
                strData += "[";
                if (modelList.Count > 0)
                {
                    for (int i = 0; i < modelList.Count; i++)
                    {
                        int ID = modelList[i].ID;
                        string Name = modelList[i].Name;
                        string Url = modelList[i].Url;
                        string Icon = modelList[i].Iconic;
                        string parentId = modelList[i].IdName;
                        strData += "{";
                        strData += string.Format("\"id\":\"{0}\",\"text\":\"{1}\",\"checked\":{2},\"href\":\"{3}\",\"iconCls\":\"{4}\"", ID, Name, "false", Url, Icon);
                        strData += getTreeChildren(parentId);
                        if (modelList.Count - 1 == i)
                        {
                            strData += "}";
                        }
                        else
                        {
                            strData += "},";
                        }
                    }
                }
                strData += "]}]";
            }
            return Json(strData, JsonRequestBehavior.AllowGet);
        }
        //左侧导航树的递归
        public string getTreeChildren(string pId)
        {
            string strData = "";
            List<T_SysModule> modelList = db.T_SysModule.Where(a => a.ParentId == pId).OrderBy(a => a.Sort).ToList();
            if (modelList.Count > 0)
            {
                strData += string.Format(",children:[");
                for (int i = 0; i < modelList.Count; i++)
                {
                    int ID = modelList[i].ID;
                    string Name = modelList[i].Name;
                    string Url = modelList[i].Url;
                    string Icon = modelList[i].Iconic;
                    string parentId = modelList[i].IdName;
                    strData += "{";
                    strData += string.Format("\"id\":\"{0}\",\"text\":\"{1}\",\"checked\":{2},\"href\":\"{3}\",\"iconCls\":\"{4}\"", ID, Name, "false", Url, Icon);
                    strData += getTreeChildren(parentId);
                    strData += "}";
                    if (modelList.Count - 1 == i)
                    {
                        strData += string.Format("]");
                    }
                    else
                    {
                        strData += ",";
                    }
                }
            }
            return strData;
        }
        //角色新增
       [HttpPost]
       [Description("角色新增保存")]
        public ActionResult RoleSave(T_Role model, string type, string id, string ids)
        {
            if (type == "Edit")
            {
                int ID = int.Parse(id);
                T_Role modelEdit = db.T_Role.SingleOrDefault(a => a.ID == ID);
                modelEdit.Access = ids;
                modelEdit.Name = model.Name;
                db.Entry<T_Role>(modelEdit).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                model.Access = ids;
                db.T_Role.Add(model);
                int i = db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        //角色删除
        [Description("角色删除")]
        public void RoleDel(int del)
       {
           T_Role deleteItem = db.T_Role.Find(del);
           if (deleteItem != null)
           {
               db.T_Role.Remove(deleteItem);
           }
           db.SaveChanges();
       }
        #endregion
        #region 部门 操作方法
        //部门结构 树的数据
        public string DepartmentTree(int pid = -1)
        {
            string resultStr = string.Empty;
            
                List<T_Department> queryData = db.T_Department.Where(a => a.parentId == pid).ToList();
                if (queryData.Count > 0)
                {
                    resultStr += "[";
                    for (int i = 0; i < queryData.Count; i++)
                    {
                        int oid = queryData[i].ID;
                        string name = queryData[i].Name;
                        resultStr += "{";
                        resultStr += string.Format("\"id\": \"{0}\",\"text\":\"{1}\"", oid, name);
                        resultStr += getDepartmentStr(int.Parse(oid.ToString()));
                        resultStr += "}";
                    }
                resultStr += string.Format("]");
            }
            return resultStr;
        }
        //部门结构 树的子节点数据
        public string getDepartmentStr(int id)
        {
            string resultStr = "";
            List<T_Department> queryData = db.T_Department.Where(a=>a.parentId==id).ToList();
            if (queryData.Count > 0)
            {
                resultStr += string.Format(",children: [");
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\", \"text\": \"{1}\"", oid, name);
                    resultStr += getDepartmentStr(oid);
                    resultStr += "}";
                    if (queryData.Count - 1 == i)
                    {
                        resultStr += string.Format("]");
                    }
                    else
                    {
                        resultStr += ",";
                    }
                }
            }
            return resultStr;
        }
        //新增部门保存  
        [Description("部门新增保存")]
        public ActionResult AddCompanySave(string comName, int Pid)
        {
            T_Department model = new T_Department();
            model.Name = comName;
            model.parentId = Pid;
            db.T_Department.Add(model);
            int i = db.SaveChanges();
            string ID = Pid.ToString();
            return Json(new { i = i, ID = model.ID, name = model.Name }, JsonRequestBehavior.AllowGet);
        }
        //部门修改检测
        public JsonResult editCompanyCheck(int StarId, int EndId)
        {
            int i = 0;
            List<T_Department> model = db.T_Department.Where(a => a.parentId == StarId).ToList();
            if (model.Count > 0)
            {
                for (int j = 0; j < model.Count; j++)
                {
                    int theParentId = int.Parse(model[j].parentId.ToString());
                    int theId = int.Parse(model[j].ID.ToString());
                    if (theId == EndId)
                    {
                        i = 0;
                        return Json(i, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        i = getSonList(theId, EndId);
                    }
                }
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                i = 1;
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        public int getSonList(int id, int EndId)
        {
            int i = 0;
            List<T_Department> model = db.T_Department.Where(a => a.parentId == id).ToList();
            if (model.Count > 0)
            {
                for (int k = 0; k < model.Count; k++)
                {
                    int theParentId = int.Parse(model[k].parentId.ToString());
                    int theId = int.Parse(model[k].ID.ToString());
                    if (theId == EndId)
                    {
                        i = 0;
                        break;
                    }
                    else
                    {
                        i = getSonList(theId, EndId);
                    }
                }
            }
            else
            {
                i = 1;
            }
            return i;
        }
        //编辑部门保存  
        public JsonResult EditCompanySave(int id, int Pid, string comName)
        {
            T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == id);
            string theEmployees = dModel.employees;
            if (theEmployees != null && theEmployees != "")
            {
                string[] employeesArry = theEmployees.Split(',');
                for (int j = 0; j < employeesArry.Length; j++)
                {
                    deleteParentEmployees(employeesArry[j], id, Pid);
                }
            }
            dModel.parentId = Pid;
            dModel.Name = comName;
            db.Entry<T_Department>(dModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            string s = getDepartmentStr2(Pid);
            return Json(new { i = i, theJson = s, ID = dModel.ID, fId = dModel.parentId, name = dModel.Name }, JsonRequestBehavior.AllowGet);
        }
        //移动部门时候删除人员
        public void deleteParentEmployees(string eId, int id, int spid)
        {
            //eId 传过来的人员ID, id原iD , spid目标id
            deleteEmployeesWhenDepartmentEdit(id, eId);
            if (spid != 1 && spid != -1)
            {
                List<T_Department> departmentModel = new List<T_Department>();
               //departmentModel = db.Database.SqlQuery<T_Department>("select * from T_Department where Id=" + spid + " and employees  like  '%" + eId + "%'").ToList();
                departmentModel = db.T_Department.Where(a => a.ID == spid && a.employees.Contains(eId)).ToList();
                if (departmentModel.Count == 0)
                {
                    T_Department departmentModelss = db.T_Department.SingleOrDefault(a => a.ID == spid);
                    if (departmentModelss.employees != "" && departmentModelss.employees != null)
                    {
                        departmentModelss.employees += "," + eId;
                    }
                    else
                    {
                        departmentModelss.employees += eId;
                    }
                    int parentIntId = int.Parse(departmentModelss.parentId.ToString());
                    if (parentIntId != -1)
                    {
                        getParentT(parentIntId, eId);
                    }
                    db.Entry<T_Department>(departmentModelss).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

        }
        public void deleteEmployeesWhenDepartmentEdit(int id, string eId)
        {
            //eId 传过来的人员ID, id原iD , spid目标id
            T_Department Model = db.T_Department.SingleOrDefault(a => a.ID == id);
            int pId = int.Parse(Model.parentId.ToString());
            if (pId != 1 && pId != -1)
            {
                string str = "";
                string lastKey = null;
                T_Department departmentModel = db.T_Department.SingleOrDefault(a => a.ID == pId);
                string t = departmentModel.employees;
                string[] DepartmentEmployeesArry = t.Split(',');
                for (int i = 0; i < DepartmentEmployeesArry.Length; i++)
                {
                    if (DepartmentEmployeesArry[i] != eId)
                    {
                        if (DepartmentEmployeesArry.Length - 1 != i)
                        {
                            str += DepartmentEmployeesArry[i] + ",";

                        }
                        else
                        {
                            str += DepartmentEmployeesArry[i];
                        }
                    }
                }
                if (str != null && str != "")
                {
                    lastKey = str.Substring(str.Length - 1, 1);
                    if (lastKey == ",")
                    {
                        str = str.Remove(str.Length - 1, 1);
                    }
                }
                departmentModel.employees = str;
                db.Entry<T_Department>(departmentModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                deleteEmployeesWhenDepartmentEdit(pId, eId);
            }
        }
        public void getParentT(int Pid, string eId)
        {
            if (Pid != 1 && Pid != -1)
            {
                T_Department departmentModelss = db.T_Department.SingleOrDefault(a => a.ID == Pid);
                if (departmentModelss.employees != "" && departmentModelss.employees != null)
                {
                    departmentModelss.employees += "," + eId;
                }
                else
                {
                    departmentModelss.employees += eId;
                }
                int parentIntId = int.Parse(departmentModelss.parentId.ToString());

                getParentT(parentIntId, eId);

                db.Entry<T_Department>(departmentModelss).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
        public string getDepartmentStr2(int id)
        {
            string resultStr = "";
            List<T_Department> queryData = db.T_Department.Where(a=>a.parentId==id).ToList();
            if (queryData.Count > 0)
            {
                resultStr += "[";
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\", \"text\": \"{1}\"", oid, name);
                    resultStr += getDepartmentStr(oid);
                    resultStr += "}";
                    if (queryData.Count - 1 == i)
                    {
                        resultStr += string.Format("]");
                    }
                    else
                    {
                        resultStr += ",";
                    }
                }
            }
            return resultStr;
        }
        //删除部门
        //查看是否存在子部门或者员工
        public JsonResult findChildForDelete(int Pid)
        {
            int i = 0;
            List<T_Department> queryData = db.T_Department.Where(a=>a.parentId==Pid).ToList();
            int flag = queryData.Count;
            if (flag > 0)
            {
                i = 1;
            }
            else
            {
                List<T_Department> departmentModel = db.T_Department.Where(a => a.ID == Pid).ToList();
                if (departmentModel[0].employees != null && departmentModel[0].employees != "")
                {
                    i = 1;
                }
            }
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //删除部门
        public JsonResult departmentDelete(int Pid)
        {
            T_Department departmentModel = db.T_Department.SingleOrDefault(a => a.ID == Pid);
            db.T_Department.Remove(departmentModel);
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region 人员 操作方法
        //用户列表
        [HttpPost]
        public ContentResult ShowUserList(Lib.GridPager pager, string dId, string queryStr, string queryStr2)
        {
            IQueryable<T_User> queryData = null;
            queryData = db.T_User;
            if (queryData != null)
            {
                //部门成员
                int intId = int.Parse(dId);
                T_Department departmentModel = db.T_Department.Find(intId);
                if (departmentModel == null)
                {
                    
                    return Content("{\"total\":\"0 \",\"rows\":\"0\"");
                }
                if (departmentModel.employees != null && departmentModel.employees != "")
                {
					if (dId != "1")
					{
						string[] Employees = departmentModel.employees.Split(',');
						int[] EmployeesArry = new int[Employees.Length];
						for (int i = 0; i < Employees.Length; i++)
						{
							EmployeesArry[i] = Convert.ToInt32(Employees[i]);
						}
						queryData = from r in db.T_User
									where EmployeesArry.Contains(r.ID)
									select r;
					}
                   
                }
                else {
                    return Content("{\"total\":\"0 \",\"rows\":\"0\"");
                }
                //花名和真名查询人员
                if (!string.IsNullOrEmpty(queryStr))
                {
                    queryData = queryData.Where(a => a.Name != null && a.Name.Contains(queryStr) || a.Nickname != null && a.Nickname.Contains(queryStr));
                }
                if (!string.IsNullOrEmpty(queryStr2)) {

                    queryData = queryData.Where(a => a.Power == queryStr2);
                    //T_Role MOD_User = db.T_Role.FirstOrDefault(a => a.Name == queryStr2);
                    //if (MOD_User != null) {
                    //    string _Sid = MOD_User.ID.ToString();
                    //    queryData = queryData.Where(a => a.Power == _Sid);

                    //}
                }

                pager.totalRows = queryData.Count();
                //分页
                queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
                List<T_User> list = new List<T_User>();
                foreach (var item in queryData)
                {
                    T_User i = new T_User();
                    i = item;
                    i.DepartmentId = GetDaparementString(item.DepartmentId);
                    i.Power = GetPowerString(item.Power);
                    list.Add(i);
                }
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
                return Content(json);
            }
            else
            {
                return Content("{\"total\":\"0 \",\"rows\":\"0\"");
            }
        }
        //新增查看帐号是否存在
        public JsonResult checkUserName(T_User model)
        {
            int i = 0;
            T_User modelList = db.T_User.SingleOrDefault(a => a.Nickname == model.Nickname);
            if (modelList != null)
            {
                i = 1;
            }
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //查看部门是否有主管
        public JsonResult checkSupervisor(int Pid)
        {
            int i = 0;
            T_Department departmentModel = db.T_Department.Find(Pid);

            if (departmentModel.supervisor == null)
            {
                i = 1;
            }
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //员工新增保存
        [HttpPost]
        public ActionResult EmployeesAddSave(T_User model, string id, int Pid)
        {
            string theId = Pid.ToString();
            //人员表保存
            model.DepartmentId = theId;
            db.T_User.Add(model);
            db.SaveChanges();
            //插入部门表的 人员字段
            T_Department departmentModel = db.T_Department.SingleOrDefault(a => a.ID == Pid);
            string Eid = model.ID.ToString();
            if (departmentModel.employees == "" || departmentModel.employees == null)
            {
                departmentModel.employees += Eid;
            }
            else
            {
                departmentModel.employees += "," + Eid;
            }
            //部门主管设置
            if (model.IsManagers == "1")
            {
                departmentModel.supervisor = model.ID;
            }
            //遍历父级部门
            int parentId = int.Parse(departmentModel.parentId.ToString());
            getParent(parentId, Eid);
            db.Entry<T_Department>(departmentModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        public void getParent(int parentId, string id)
        {
            if (parentId != -1)
            {
                T_Department departmentModel = db.T_Department.SingleOrDefault(a => a.ID == parentId);
                if (departmentModel.employees == "" || departmentModel.employees == null)
                {
                    departmentModel.employees += id;
                }
                else
                {
                    departmentModel.employees += "," + id;
                }
                db.Entry<T_Department>(departmentModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                int ssid = int.Parse(departmentModel.parentId.ToString());
                getParent(ssid, id);
            }
        }
        //删除人员
        public JsonResult employeesDelete(int id)
        {
            T_User userModel = db.T_User.SingleOrDefault(a => a.ID == id);
            db.T_User.Remove(userModel);
            int i = db.SaveChanges();
            string sId = id.ToString();
            //删除人员字段该id
            List<T_Department> departmentModel = null;
           // departmentModel = db.Database.SqlQuery<T_Department>("select * from T_Department where employees like '%" + id + "%'").ToList();
            departmentModel = db.T_Department.Where(a => a.employees.Contains(sId)).ToList();
            for (int j = 0; j < departmentModel.Count; j++)
            {
                string[] employeesArry = departmentModel[j].employees.Split(',');
                string employeesStr = "";
                //  int length = employeesArry.Count;
                for (int k = 0; k < employeesArry.Length; k++)
                {
                    if (employeesArry[k] != sId)
                    {
                        if (employeesArry.Length - 1 != k)
                        {
                            employeesStr += employeesArry[k] + ",";
                        }
                        else
                        {
                            employeesStr += employeesArry[k];
                        }
                    }
                    if (employeesArry[k] == sId && employeesArry.Length - 1 == k)
                    {
                        if (employeesStr.Length > 0)
                        {
                            employeesStr = employeesStr.Substring(0, employeesStr.Length - 1);
                        }
                    }
                    if (employeesArry.Length - 1 == k)
                    {
                        int ddid = departmentModel[j].ID;
                        T_Department dModel = db.T_Department.SingleOrDefault(a => a.ID == ddid);
                        dModel.employees = employeesStr;
                        db.Entry<T_Department>(dModel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            //删除主管字段该人员Id
            List<T_Department> sModel = null;
           // sModel = db.Database.SqlQuery<T_Department>("select * from T_Department where supervisor like '%" + id + "%'").ToList();
            sModel = db.T_Department.Where(a => a.supervisor == id).ToList();
            if (sModel.Count > 0)
            {
                for (int l = 0; l < sModel.Count; l++)
                {
                    int sModelId = sModel[l].ID;
                    T_Department lModel = db.T_Department.SingleOrDefault(a => a.ID == sModelId);
                    lModel.supervisor = null;
                    db.Entry<T_Department>(lModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //编辑查看花名是否存在
        public JsonResult checkUserNameForEdit(T_User model, int oid)
        {
            int i = 0;
           T_User modelUs = db.T_User.Find(oid);
            if (modelUs.Nickname != model.Nickname)
            {
                List<T_User> modelList = db.T_User.Where(a => a.Nickname == model.Nickname).ToList();
                if (modelList.Count > 0)
                {
                    i = 1;
                }

            }
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //人员修改保存 没有改变人员的部门
        public JsonResult editEmployeesDepartment(T_User model, string departmentName, int id)
        {
            int dId = int.Parse(departmentName);
            T_Department modelDepartment = db.T_Department.Find(dId);
            if (modelDepartment == null) {
                return Json("没有找到该页面", JsonRequestBehavior.AllowGet);
            }
            if (model.IsManagers == "0" && modelDepartment.supervisor == id)
            {
                //取消主管
                modelDepartment.supervisor = null;
                db.Entry<T_Department>(modelDepartment).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                if (modelDepartment.supervisor == null)
                {
                    modelDepartment.supervisor = id;
                    db.Entry<T_Department>(modelDepartment).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //保存人原数据
            T_User userModel = db.T_User.SingleOrDefault(a => a.ID == id);
            userModel.Tel = model.Tel;
            userModel.DepartmentId = departmentName;
            userModel.Nickname = model.Nickname;
            userModel.IsManagers = model.IsManagers;
            userModel.Name = model.Name;
            userModel.Power = model.Power;
            userModel.PassWord = model.PassWord;
            db.Entry<T_User>(userModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //人员修改保存 改变了人员的部门
        public JsonResult editEmployeesDepartmentChange(T_User model, string departmentName, int id, int starId)
        {
            //  departmentName是目标部门的ID   starId是原部门的ID     id 是人员的ID
            //人员调换  部门表删除人员字段
            T_Department departmentModel = db.T_Department.Find(starId);//找到原部门数据
            string em = departmentModel.employees;//原部门人员
            string sId = id.ToString();
            string[] DepartmentEmployeesArry = em.Split(',');//原部门人员数组
            string str = "";
            string lastKey = null;
            //删除该人员 在原部门字段的记录
            if (starId != 1)
            {
                for (int j = 0; j < DepartmentEmployeesArry.Length; j++)
                {
                    if (DepartmentEmployeesArry[j] != sId)
                    {
                        if (DepartmentEmployeesArry.Length != j)
                        {
                            str += DepartmentEmployeesArry[j] + ",";
                        }
                        else
                        {
                            str += DepartmentEmployeesArry[j];
                        }
                    }
                }
                if (str != null && str != "")
                {
                    lastKey = str.Substring(str.Length - 1, 1);
                    if (lastKey == ",")
                    {
                        str = str.Remove(str.Length - 1, 1);
                    }
                }
                departmentModel.employees = str;
                if (departmentModel.supervisor == id)
                {
                    departmentModel.supervisor = null;// 清空原部门主管id
                }
                db.Entry<T_Department>(departmentModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                int parentID = int.Parse(departmentModel.parentId.ToString());

                //调用删除父部门所有记录方法

            }
            int sDepartmentName = int.Parse(departmentName);
            string stringId = id.ToString();
            deleteParentEmployeesForEdit(stringId, starId);
            //调用目标部门 的人员字段加入该人员id
            T_Department departmentEndModel = db.T_Department.SingleOrDefault(a => a.ID == sDepartmentName);//找到目标门数据
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
            //调用给目标部门的父级部门人员字段插入人员ID方法
            addEmployeesToDepartmentForEidt(endDepartmentParentID, sId);
            //保存user数据
            T_User userModel = db.T_User.Find(id);
            userModel.Tel = model.Tel;
            userModel.DepartmentId = departmentName;
            userModel.Nickname = model.Nickname;
            userModel.IsManagers = "0";
            userModel.Name = model.Name;
            userModel.Power = model.Power;
            userModel.PassWord = model.PassWord;
            db.Entry<T_User>(userModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //编辑人员修改部门后  删除改人员在原部门的所有记录
        public void deleteParentEmployeesForEdit(string eId, int id)
        {
            //eId是人的ID id是原部门id   spid 是目标ID
            T_Department Model = db.T_Department.Find(id);
            int pId = int.Parse(Model.parentId.ToString());
            if (id != 1)
            {
                string str = "";
                string lastKey = null;
                T_Department departmentModel = db.T_Department.Find(pId);
                string t = departmentModel.employees;
                string[] DepartmentEmployeesArry = t.Split(',');
                for (int i = 0; i < DepartmentEmployeesArry.Length; i++)
                {
                    if (DepartmentEmployeesArry[i] != eId)
                    {
                        if (DepartmentEmployeesArry.Length - 1 != i)
                        {
                            str += DepartmentEmployeesArry[i] + ",";
                        }
                        else
                        {
                            str += DepartmentEmployeesArry[i];
                        }
                    }
                }
                if (str != null && str != "")
                {
                    lastKey = str.Substring(str.Length - 1, 1);
                    if (lastKey == ",")
                    {
                        str = str.Remove(str.Length - 1, 1);
                    }
                }
                departmentModel.employees = str;
                db.Entry<T_Department>(departmentModel).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                deleteParentEmployeesForEdit(eId, pId);
            }
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
        //人员列表输入框获取权限列表 
        class powerMod{
            public int ID { get; set; }
            public string Name { get; set; }
        }
        public JsonResult getPowerList() {
            IQueryable<T_Role> mod = db.T_Role.AsQueryable();
            List<powerMod> powerList = new List<powerMod>();
            foreach (var item in mod)
            {
                powerMod power = new powerMod();
                power.ID = item.ID;
                power.Name = item.Name;
                powerList.Add(power);
            }
            string json = JsonConvert.SerializeObject(powerList);
            return Json(json);
        }

        #endregion
        #region 公告通知
        [HttpPost]
        public JsonResult ViewNoticeAddSave(T_RulesNotice model)//新增公告通知保存
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.Contents = Server.UrlDecode(model.Contents);
            model.CreateName = Nickname;
            model.CreateDate = DateTime.Now;
            model.IsDelete = "0";
            db.T_RulesNotice.Add(model);
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);

        }
        public JsonResult ViewNoticeEditSave(T_RulesNotice model)//编辑公告保存
        {
            T_RulesNotice editModel = db.T_RulesNotice.Find(model.ID);
            editModel.Power = model.Power;
            editModel.Summary = model.Summary;
            editModel.Title = model.Title;
            editModel.Type = model.Type;
            editModel.Contents = Server.UrlDecode(model.Contents);
            db.Entry<T_RulesNotice>(editModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i);
        }
        public JsonResult NoticeDelete(int id)
        {
            T_RulesNotice delModel = db.T_RulesNotice.Find(id);
            delModel.IsDelete = "1";
            db.Entry<T_RulesNotice>(delModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i);
        }
        public ContentResult GetRuleList(Lib.GridPager pager, string queryStr, string viewtype)//公告列表数据
        {
            IQueryable<T_RulesNotice> queryData = db.T_RulesNotice.Where(a=>a.IsDelete=="0");
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.Title.Contains(queryStr) || a.Summary.Contains(queryStr));
            }
            if (viewtype != "0" && viewtype!=null)
            {
                queryData = queryData.Where(a => a.Type == viewtype);
            }
            pager.totalRows = queryData.Count();
            
            List<T_RulesNotice> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
          
        }
        #endregion
    }
}
