
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EBMS.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    public class HomeController :Controller
    {
        //
        // GET: /Home/
        #region 视图
        EBMSEntities db = new EBMSEntities();
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [Description("访问登录页面")]
        [AllowAnonymous]
        public ActionResult LoginOld()
        {
			
			return View();
        }

		[AllowAnonymous]
		public ActionResult Login()
		{
			ViewData["appid"] = dingInfo.appid;
			ViewData["redirect_uri"] = dingInfo.redirect_uri;
			return View();
		}
		[Description("访问修改密码页面")]
        public ActionResult EditPwd()
        {
            var name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_User model = db.T_User.SingleOrDefault(a => a.Nickname == name);
            return View(model);
        }
        [Description("桌面")]
        public ActionResult WorkMap()
        {
            return View();
        }
        #endregion
        #region post方法
        //登录
        [Description("提交登录")]
        [HttpPost]
        public JsonResult Login(string UserName, string Password, string remenberMe)
        {

            T_User curUser = db.T_User.SingleOrDefault(a => (a.Nickname == UserName || a.Tel == UserName) && a.PassWord == Password);
            if (curUser == null)
            {
                return Json(new { message = "用户名或密码错误" }, JsonRequestBehavior.AllowGet);
            }
            T_PersonnelFile person = db.T_PersonnelFile.SingleOrDefault(s => s.NickName.Equals(UserName));
            if (person != null && person.OnJob == 1)
            {
                return Json(new { message = "该用户已离职" }, JsonRequestBehavior.AllowGet);
            }
            string sUserId = curUser.ID.ToString();
            Response.Cookies["Nickname"].Value = Server.UrlEncode(curUser.Nickname);
            Response.Cookies["Name"].Value = Server.UrlEncode(curUser.Name);
            Response.Cookies["UserId"].Value = Server.UrlEncode(sUserId);
            Response.Cookies["pasword"].Value = Server.UrlEncode(curUser.PassWord);
            if (remenberMe == "1")
            {
                Response.Cookies["zh"].Value = Server.UrlEncode(curUser.Nickname);
                Response.Cookies["mm"].Value = Server.UrlEncode(curUser.PassWord);
                Response.Cookies["zh"].Expires = DateTime.MaxValue;
                Response.Cookies["mm"].Expires = DateTime.MaxValue;
            }
            if (remenberMe == "0")
            {
                if (Response.Cookies["zh"] != null)
                {
                    Response.Cookies["zh"].Expires = DateTime.Now.AddDays(-1);
                    //Response.Cookies["zh"].Expires.AddDays(-1);
                };
                if (Response.Cookies["mm"] != null)
                {
                    Response.Cookies["mm"].Expires = DateTime.Now.AddDays(-1);
                }
            }
            Response.Cookies["ID"].Value = Server.UrlEncode(curUser.ID.ToString());
            if (curUser.Power == null || curUser.Power == "0")
            {
                Response.Cookies["UserPower"].Value = "0";
                return Json(new { message = "您没有权限使用本系统" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                Response.Cookies["UserPower"].Value = curUser.Power;
                FormsAuthentication.SetAuthCookie(Server.UrlEncode(curUser.Nickname), true);
                string ip = Request.UserHostAddress;
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                T_Syslog model = new T_Syslog
                {
                    Operator = curUser.Nickname,
                    CreateTime = time,
                    IP = ip,
                    Contentlog = string.Format("操作人:{0},操作时间:{1},IP地址:{2}", curUser.Nickname, time, ip),
                    Type = "登录日志"
                };
                db.T_Syslog.Add(model);
                db.SaveChanges();
                return Json(new { message = "1" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Logout()
        {
            Response.Cookies.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        [Description("访问功能权限管理页面")]
        //顶级导航
        public JsonResult getTopNav()
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //根据登录名称得到角色
            T_User UserMod = db.T_User.SingleOrDefault(a => a.Nickname == name);
            int roleId = int.Parse(UserMod.Power);
            T_Role rolemodel = db.T_Role.Find(roleId);
            if (rolemodel == null) return Json("");
            string[] ids = rolemodel.Access.Split(',');
            int[] ints = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ints[i] = Convert.ToInt32(ids[i]);
            }
            IQueryable<T_SysModule> modelList = from r in db.T_SysModule
                                                where ints.Contains(r.ID) && (r.ParentId == "-1")
                                                orderby r.Sort ascending
                                                select r;
            var json = new
            {

                rows = (from r in modelList.ToList()
                        select new T_SysModule
                        {
                            ID = r.ID,
                            IdName = r.IdName,
                            EnglishName = r.EnglishName,
                            Url = r.Url,
                            Name = r.Name
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //左侧导航树的JSON
        public JsonResult getTreeData(string pId)
        {
            string strData = string.Empty;
            if (!String.IsNullOrEmpty(pId))
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                //根据登录名称得到角色
                T_User UserMod = db.T_User.SingleOrDefault(a => a.Nickname == name);
                int roleId = int.Parse(UserMod.Power);
                T_Role rolemodel = db.T_Role.Find(roleId);
                if (rolemodel == null) return Json("");
                string[] ids = rolemodel.Access.Split(',');
                int[] ints = new int[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    ints[i] = Convert.ToInt32(ids[i]);
                }
                IQueryable<T_SysModule> modelList = from r in db.T_SysModule
                                                    where ints.Contains(r.ID) && (r.ParentId == pId)
                                                    orderby r.Sort ascending
                                                    select r;
                List<T_SysModule> model = modelList.ToList();
                strData += "[";
                if (model.Count > 0)
                {
                    for (int i = 0; i < model.Count; i++)
                    {
                        int ID = model[i].ID;
                        string Name = model[i].Name;
                        string Url = model[i].Url;
                        string Icon = model[i].Iconic;
                        string parentId = model[i].IdName;
                        strData += "{";
                        strData += string.Format("\"id\":\"{0}\",\"text\":\"{1}\",\"href\":\"{2}\",\"iconCls\":\"{3}\"", ID, Name, Url, Icon);
                        strData += getTreeChildren(parentId);
                        if (model.Count - 1 == i)
                        {
                            strData += "}";
                        }
                        else
                        {
                            strData += "},";
                        }
                    }
                }
                strData += "]";
            }
            return Json(strData, JsonRequestBehavior.AllowGet);
        }
        public string getTreeChildren(string pId)
        {
            string strData = "";
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            //根据登录名称得到角色
            T_User UserMod = db.T_User.SingleOrDefault(a => a.Nickname == name);
            int roleId = int.Parse(UserMod.Power);
            T_Role rolemodel = db.T_Role.Find(roleId);
            if (rolemodel == null) return "";
            string[] ids = rolemodel.Access.Split(',');
            int[] ints = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ints[i] = Convert.ToInt32(ids[i]);
            }
            IQueryable<T_SysModule> modelList = from r in db.T_SysModule
                                                where ints.Contains(r.ID) && (r.ParentId == pId)
                                                orderby r.Sort ascending
                                                select r;
            List<T_SysModule> model = modelList.ToList();

            if (model.Count > 0)
            {
                strData += string.Format(",children:[");
                for (int i = 0; i < model.Count; i++)
                {
                    int ID = model[i].ID;
                    string Name = model[i].Name;
                    string Url = model[i].Url;
                    string Icon = model[i].Iconic;
                    string parentId = model[i].IdName;
                    strData += "{";
                    strData += string.Format("\"id\":\"{0}\",\"text\":\"{1}\",\"href\":\"{2}\",\"iconCls\":\"{3}\"", ID, Name, Url, Icon);
                    strData += getTreeChildren(parentId);
                    strData += "}";
                    if (model.Count - 1 == i)
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
        //修改密码
        [HttpPost]
        public JsonResult EditPwd(T_User model, string oldPwd)
        {
            var name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_User entity = db.T_User.SingleOrDefault(a => a.Nickname == name);
            if (oldPwd != entity.PassWord)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                entity.PassWord = model.PassWord;
                db.Entry<T_User>(entity).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        public class MapItem
        {
            public string Name { get; set; }
            public string RejectNumberName { get; set; }
            public string NotauditedNumberUrl { get; set; }
            public string RejectNumberUrl { get; set; }
            public int RejectNumberQty { get; set; }
            public int NotauditedNumberQty { get; set; }
            public string ModName { get; set; }
        }
        //桌面数据
        public JsonResult getMapItem()
        {
            //真名
            string Zname = Server.UrlDecode(Request.Cookies["Name"].Value);
            //花名
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
            //根据花名去对应T_Role的ID
            string power = db.T_User.FirstOrDefault(s => s.Nickname == name).Power;
            int pow = Convert.ToInt32(power);
            //T_Role表的ID得到具体T_SysModule的ID 
            string access = db.T_Role.FirstOrDefault(a => a.ID == pow).Access;

            List<MapItem> MapList = db.Database.SqlQuery<MapItem>("select ModularName as Name,'' as RejectNumberName,isnull(SUM(NotauditedNumber),0) as NotauditedNumberQty,isnull(SUM(RejectNumber),0) as RejectNumberQty,'' as  NotauditedNumberUrl,'' as RejectNumberUrl  from T_ModularNotaudited  where PendingAuditName='" + name + "' or PendingAuditName='" + Zname + "' GROUP BY ModularName").ToList();
            List<MapItem> MapListModel = new List<MapItem>();
            foreach (MapItem item in MapList)
            {
                if (item.Name == "退货退款")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "退货退款未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的退货退款").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "退货退款未审核";
                      MapItemZmodel.NotauditedNumberUrl=NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的退货退款";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "退货退款模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资金调拨")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资金调拨未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资金调拨").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资金调拨未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的资金调拨";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "资金调拨模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资金冻结")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资金冻结未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资金冻结").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资金冻结未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的资金冻结";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "资金冻结模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "请假调休")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的申请").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的申请";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "请假调休";
                    MapListModel.Add(MapItemZmodel);
                } 
                if (item.Name == "专票")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "专票未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的专票").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "专票未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的专票";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "专票模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "离职管理")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "离职审批未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的离职申请").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "离职审批未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的离职申请";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "离职模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "报损")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "报损未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的报损").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "报损未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的报损";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "报损模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "借支")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "借支未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的借支").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "借支未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的借支";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "借支模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资产变更")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资产变更待审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资产变更").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资产变更待审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的资产变更";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "资产变更模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "报销")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "报销未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的报销").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "报销未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的报销";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "报销模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "产品采购")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "产品采购未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "产品我的采购").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "产品采购未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "产品我的采购";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "产品采购模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "行政采购")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "行政采购未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的行政采购").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "行政采购未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的行政采购";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "行政采购模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "换货")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "换货未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的换货").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "换货未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的换货";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "换货模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "补发货")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "补发货未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的补发货").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "补发货未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的补发货";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "补发货模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "返现")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "返现未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的返现").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "返现未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的返现";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "返现模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "拦截")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "拦截未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的未发货拦截").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "拦截未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的未发货拦截";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "拦截模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "专票")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "专票未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的专票").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "专票未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的专票";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "专票模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "发票")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "发票申领未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的发票申领").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "发票申领未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "发票申领";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "发票申领模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "快递手工费")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "快递手工费未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的快递手工费").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "快递手工费未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的快递手工费";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "快递手工费模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "用人需求")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "用人需求未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的用人需求").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "用人需求未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的用人需求";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "用人需求模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "招聘")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "待面试列表").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "简历管理").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "待面试列表";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "简历管理";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "招聘模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "转正")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "转正未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的转正").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "转正未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的转正";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "转正模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "转岗")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "转岗未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的转岗").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "转岗未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的转岗";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "转岗模块";
                    MapListModel.Add(MapItemZmodel);
                }
                 if (item.Name == "出差")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "出差未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的出差").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "出差未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的出差";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "出差模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "部门活动")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "活动未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的活动").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "活动未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的活动";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "部门活动模块";
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "员工入职")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.NotauditedNumberQty = item.NotauditedNumberQty;
                    MapItemZmodel.RejectNumberQty = item.RejectNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "入职审批未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的入职").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "入职审批未审核";
                        MapItemZmodel.NotauditedNumberUrl = NotauditedNumberSysmodel[0].Url;
                    }
                    if (RejectNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.RejectNumberName = "我的入职";
                        MapItemZmodel.RejectNumberUrl = RejectNumberSysmodel[0].Url;
                    }
                    MapItemZmodel.ModName = "员工入职";
                    MapListModel.Add(MapItemZmodel);
                }
            }

            //List<MapItem> MapList = db.Database.SqlQuery<MapItem>("select Name ,Url, Qty=0 from T_SysModule where ID in (" + access + ")").ToList();
            //List<MapItem> MapListModel = new List<MapItem>();
            //foreach (MapItem item in MapList)
            //{
            //    if (item.Name == "退货退款未审核")
            //    {
            //        List<T_RetreatGroup> RetreatGroupModel = db.Database.SqlQuery<T_RetreatGroup>("select * from T_RetreatGroup where Crew like '%" + name + "%'").ToList();
            //        int shul = 0;
            //        for (int z = 0; z < RetreatGroupModel.Count; z++)
            //        {
            //            string shenheName = RetreatGroupModel[z].GroupName;
            //            T_ModularNotaudited ModularNotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && a.PendingAuditName == shenheName);
            //            if (ModularNotauditedModel != null)
            //            {
            //                shul = shul + int.Parse(ModularNotauditedModel.NotauditedNumber.ToString());
            //            }
            //        }
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "换货未审核")
            //    {
            //        List<T_ExchangeGroup> RetreatGroupModel = db.Database.SqlQuery<T_ExchangeGroup>("select * from T_ExchangeGroup where GroupUser like '%" + name + "%'").ToList();
            //        int shul = 0;
            //        string shenheName = "";
            //        for (int z = 0; z < RetreatGroupModel.Count; z++)
            //        {
            //            shenheName = RetreatGroupModel[z].GroupName;
            //            T_ModularNotaudited ModularNotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货未审核" && a.PendingAuditName == shenheName);
            //            if (ModularNotauditedModel != null)
            //            {
            //                shul = shul + int.Parse(ModularNotauditedModel.NotauditedNumber.ToString());
            //            }
            //        }
            //        List<T_ModularNotaudited> ModularQueryByName = new List<T_ModularNotaudited>();
            //        ModularQueryByName = db.T_ModularNotaudited.Where(a => a.ModularName == "换货未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname)).ToList();
            //        for (int x = 0; x < ModularQueryByName.Count; x++)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName[x].NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "补发货未审核")
            //    {

            //        List<T_ReissueGroup> RetreatGroupModel = db.Database.SqlQuery<T_ReissueGroup>("select * from T_ReissueGroup where GroupUser like '%" + name + "%'").ToList();
            //        int shul = 0;
            //        string shenheName = "";
            //        for (int z = 0; z < RetreatGroupModel.Count; z++)
            //        {
            //            shenheName = RetreatGroupModel[z].GroupName;
            //            T_ModularNotaudited ModularNotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "补发货未审核" && a.PendingAuditName == shenheName);
            //            if (ModularNotauditedModel != null)
            //            {
            //                shul = shul + int.Parse(ModularNotauditedModel.NotauditedNumber.ToString());
            //            }
            //        }

            //        List<T_ModularNotaudited> ModularQueryByName = new List<T_ModularNotaudited>();
            //        if (shenheName != "")
            //        {
            //            ModularQueryByName = db.T_ModularNotaudited.Where(a => a.ModularName == "补发货未审核" && a.PendingAuditName != shenheName && (a.PendingAuditName == name || a.PendingAuditName == Zname)).ToList();
            //        }
            //        else
            //        {
            //            ModularQueryByName = db.T_ModularNotaudited.Where(a => a.ModularName == "补发货未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname)).ToList();
            //        }
            //        for (int x = 0; x < ModularQueryByName.Count; x++)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName[x].NotauditedNumber.ToString());
            //        }
            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "报销未审核")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报销未审核" && a.PendingAuditName == name);
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "产品采购未审核")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "采购未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "借支未审核")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "借支未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "资金调拨")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金调拨未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "报损未审核")
            //    {
            //        int shul = 0;
            //        List<T_ModularNotaudited> ModularQueryByName = db.T_ModularNotaudited.Where(a => a.ModularName == "报损" && (a.PendingAuditName == name || a.PendingAuditName == Zname)).ToList();
            //        if (ModularQueryByName.Count>0 )
            //        {
            //            shul = shul + int.Parse(ModularQueryByName[0].NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "资金冻结")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金冻结" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "资产变更待审核")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资产变更待审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "未审核推广列表")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "未审核推广列表" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }
            //    if (item.Name == "返现未审核")
            //    {
            //        int shul = 0;
            //        T_ModularNotaudited ModularQueryByName = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "返现未审核" && (a.PendingAuditName == name || a.PendingAuditName == Zname));
            //        if (ModularQueryByName != null)
            //        {
            //            shul = shul + int.Parse(ModularQueryByName.NotauditedNumber.ToString());
            //        }

            //        item.Qty = shul;
            //        MapItem MapItemZmodel = new MapItem();
            //        MapItemZmodel.Name = item.Name;
            //        MapItemZmodel.Qty = item.Qty;
            //        MapItemZmodel.Url = item.Url;
            //        MapListModel.Add(MapItemZmodel);
            //    }

            //}
            return Json(MapListModel);
        }
        //桌面左侧默认界面  员工照片信息
        public JsonResult GetWorkPannel()
        {
            var NickName = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            T_PersonnelFile MOD_Personel = db.T_PersonnelFile.FirstOrDefault(a => a.NickName == NickName);
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == NickName);
            int ID = int.Parse(MOD_User.Power.ToString());
            T_Role MOD_Role = db.T_Role.Find(ID);

            // string result = "";
            if (MOD_Personel == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            List<T_PersonnelFile> list = new List<T_PersonnelFile>();
            list.Add(MOD_Personel);
            return Json(new { userPower = MOD_Role.Name, jsonString = list }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getNoticeList() 
        {
            string rule = "";
            string notice = "";
            List<T_RulesNotice> ruleMod = db.T_RulesNotice.Where(a => a.IsDelete == "0"&&a.Type=="1").OrderByDescending(a=>a.ID).Take(6).ToList();
            if (ruleMod != null)
            {
                 rule = JsonConvert.SerializeObject(ruleMod, Lib.Comm.setTimeFormat());
            }
            List<T_RulesNotice> noticeMod = db.T_RulesNotice.Where(a => a.IsDelete == "0" && a.Type == "2").OrderByDescending(a => a.ID).Take(6).ToList();
            if (noticeMod != null)
            {
                notice = JsonConvert.SerializeObject(noticeMod, Lib.Comm.setTimeFormat());
            }
            string result = "{\"Rule\":" + rule + ",\"Notice\":" + notice + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }

		[AllowAnonymous]

		public void ConfirmLoginInfo(string code, string state)
		{
			//钉钉登陆，实例化
			DingLogin loginDemo = new DingLogin()
			{
				appid = dingInfo.appid,
				appsecret = dingInfo.appsecret,
				tmp_auth_code = code,
				state = state
			};
			//login方法 返回人员信息
			JsonData Result = loginDemo.login();
			//JsonData userInfo = Result["Table"][0];
			if (Result != null)
			{
				string dingId = Result["dingId"].ToString();

				string ip = Request.UserHostAddress;
				string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				var User = db.T_User.SingleOrDefault(s => s.dingID == dingId);
				if (User != null)
				{

					Response.Cookies["Nickname"].Value = Server.UrlEncode(User.Nickname);
					Response.Cookies["Name"].Value = Server.UrlEncode(User.Name);
					Response.Cookies["UserId"].Value = Server.UrlEncode(User.ID.ToString());
					Response.Cookies["dingID"].Value = Server.UrlEncode(User.dingID);

					//Response.Cookies["Account"].Value = Server.UrlEncode(User.Name);
					//Response.Cookies["Account"].Expires = DateTime.Now.AddDays(1);
					
					//Response.Cookies["dingID"].Expires = DateTime.Now.AddDays(1);
					//Response.Cookies["UserPower"].Value = User.Power.ToString();
					Response.Cookies["dingID"].Expires = DateTime.Now.AddDays(1);
					FormsAuthentication.SetAuthCookie(Server.UrlEncode(User.dingID), true);


					T_Syslog model = new T_Syslog
					{
						Operator = dingId,
						CreateTime = time,
						IP = ip,
						Contentlog = string.Format("操作人:{0},操作时间:{1},IP地址:{2}", dingId, time, ip),
						Type = "登录成功日志"
					};
					db.T_Syslog.Add(model);
					db.SaveChanges();
					//FormsAuthentication.RedirectFromLoginPage(Server.UrlEncode(User.userid), true);

					//得到请求的url
					string requestUrl = FormsAuthentication.GetRedirectUrl(FormsAuthentication.FormsCookieName, false);

					//重新定向到请求的url
					Response.Redirect("/Home/Index");


				}
				else
				{
					T_Syslog model = new T_Syslog
					{
						Operator = dingId,
						CreateTime = time,
						IP = ip,
						Contentlog = string.Format("操作人:{0},操作时间:{1},IP地址:{2}", dingId, time, ip),
						Type = "登录失败日志"
					};
					db.T_Syslog.Add(model);
					db.SaveChanges();
					Response.Redirect("/Home/Login");
				}



			}


		}

		public ActionResult getOrder()
		{
		  return Json(wdtHelper.getOrder(),JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}
