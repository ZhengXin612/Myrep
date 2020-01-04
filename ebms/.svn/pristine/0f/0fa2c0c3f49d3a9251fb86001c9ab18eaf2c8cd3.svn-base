using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class AccountantProjectController : BaseController
    {

        #region 公共/属性/字段/方法

        EBMSEntities db = new EBMSEntities();

        //会计科目 树的数据
        public string AccountProjectTree(int pid = -1)
        {
            string resultStr = string.Empty;

            List<T_AccountantProject> queryData = db.T_AccountantProject.Where(a => a.ParentID == pid).ToList();
            if (queryData.Count > 0)
            {
                resultStr += "[";
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\",\"text\":\"{1}\"", oid, name);
                    resultStr += getAccountantProjectStr(int.Parse(oid.ToString()));
                    resultStr += "}";
                }
                resultStr += string.Format("]");
            }
            return resultStr;
        }

        public string getAccountantProjectStr2(int id)
        {
            string resultStr = "";
            List<T_AccountantProject> queryData = db.T_AccountantProject.Where(a => a.ParentID == id).ToList();
            if (queryData.Count > 0)
            {
                resultStr += "[";
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\", \"text\": \"{1}\"", oid, name);
                    resultStr += getAccountantProjectStr(oid);
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

        public int getSonList(int id, int EndId)
        {
            int i = 0;
            List<T_AccountantProject> model = db.T_AccountantProject.Where(a => a.ParentID == id).ToList();
            if (model.Count > 0)
            {
                for (int k = 0; k < model.Count; k++)
                {
                    int theParentId = int.Parse(model[k].ParentID.ToString());
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

        public string GetDaparementString(int? id = 0)
        {
            if (id != 0)
            {
                List<T_AccountantProject> model = db.T_AccountantProject.Where(a => a.ID == id).ToList();
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

        //会计科目 树的子节点数据
        public string getAccountantProjectStr(int id)
        {
            string resultStr = "";
            List<T_AccountantProject> queryData = db.T_AccountantProject.Where(a => a.ParentID == id).ToList();
            if (queryData.Count > 0)
            {
                resultStr += string.Format(",children: [");
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\", \"text\": \"{1}\"", oid, name);
                    resultStr += getAccountantProjectStr(oid);
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

        #endregion

        #region 视图

        [Description("会计科目维护新增")]
        public ActionResult ViewAccountantProjectAdd(int Pid)
        {
            ViewData["Pid"] = Pid;
            return View();
        }

        [Description("会计科目维护编辑")]
        public ActionResult ViewAccountantProjectEdit(int pid)
        {
            T_AccountantProject model = db.T_AccountantProject.Find(pid);
            ViewData["ThisId"] = pid;
            ViewData["parentDepartmentName"] = GetDaparementString(model.ParentID);
            ViewData["parentDepartmentId"] = model.ParentID;
            string tree = AccountProjectTree(-1);
            ViewData["tree"] = tree;
            return View(model);
        }

        [Description("会计科目树")]
        public ActionResult ViewAccountantProject()
        {
            string treeData = AccountProjectTree(-1);
            ViewData["tree"] = treeData;
            return View();
        }

        #endregion

        #region Post提交

        /// <summary>
        /// 会计科目保存
        /// </summary>
        /// <param name="comName"></param>
        /// <param name="Pid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddAccountProjectSave(T_AccountantProject model, int Pid)
        {
            try
            {
                List<T_AccountantProject> list = db.T_AccountantProject.Where(s => s.Code.Equals(model.Code) || s.Name.Equals(model.Name)).ToList();
                if (list.Count() != 0)
                    return Json(new { State = "Faile", Message = "已存在该编码或名称" });
                model.ParentID = Pid;
                db.T_AccountantProject.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success", ID = model.ID, name = model.Name }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 检查
        /// </summary>
        /// <param name="StarId"></param>
        /// <param name="EndId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult check(int StarId, int EndId)
        {
            int i = 0;
            List<T_AccountantProject> model = db.T_AccountantProject.Where(a => a.ParentID == StarId).ToList();
            if (model.Count > 0)
            {
                for (int j = 0; j < model.Count; j++)
                {
                    int theParentId = int.Parse(model[j].ParentID.ToString());
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

        /// <summary>
        /// 编辑保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Pid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditAccountProjectSave(T_AccountantProject model, int id, int pid)
        {
            try
            {

                T_AccountantProject dModel = db.T_AccountantProject.Find(id);
                dModel.Name = model.Name;
                dModel.Code = model.Code;
                dModel.ParentID = pid;
                db.SaveChanges();
                string s = getAccountantProjectStr2(pid);
                return Json(new { State = "Success", theJson = s, ID = dModel.ID, fId = dModel.ParentID }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 删除科目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteAccountProject(int Pid)
        {
            try
            {
                T_AccountantProject model = db.T_AccountantProject.Find(Pid);
                db.T_AccountantProject.Remove(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 查询是否存在子科目
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FindChildForDelete(int pid)
        {
            IQueryable<T_AccountantProject> list = db.T_AccountantProject.Where(s => s.ParentID == pid);
            if (list.Count() > 0)
                return Json(new { State = "Faile" });
            else
                return Json(new { State = "Success" });
        }

        /// <summary>
        /// 查询所有子科目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetAccountChildProject(Lib.GridPager pager, int id)
        {
            List<T_AccountantProject> lists = new List<T_AccountantProject>();
            List<T_AccountantProject> list = db.T_AccountantProject.ToList();
            if (id == 1)
                list = list.Where(s => s.ID != 1).ToList();
            else
            {
                list = GetForeach(lists, id);
            }
            pager.totalRows = list.Count();
            list = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 循环
        /// </summary>
        /// <param name="lits"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<T_AccountantProject> GetForeach(List<T_AccountantProject> lits, int id)
        {
            List<T_AccountantProject> quer = db.T_AccountantProject.Where(s => s.ParentID == id).ToList();
            if (quer.Count() > 0)
            {
                foreach (var item in quer)
                {
                    T_AccountantProject l = new T_AccountantProject();
                    l = item;
                    lits.Add(l);
                    GetForeach(lits, item.ID);
                }
                return lits;
            }
            return lits;
        }


        #endregion


    }
}
