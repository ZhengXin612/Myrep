using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EBMS.Areas.APP.Controllers
{
    public class AppHomeController : Controller
    {
        EBMSEntities db = new EBMSEntities();
        //
        // GET: /APP/Home/
       
        public ActionResult Index(string CurUser)
        {
            //ViewData["mapDom"] = JsonConvert.SerializeObject(getMapItem(CurUser));
            ViewData["mapDom"] = JsonConvert.SerializeObject(getMapItem(CurUser));
            return View();
        }
        public ActionResult Login()
        {

            return View();
        }
        //登录
        [HttpPost]
        public JsonResult Login(string UserName, string Password, string remenberMe)
        {

            T_User curUser = db.T_User.FirstOrDefault(a => (a.Nickname == UserName || a.Tel == UserName) && a.PassWord == Password);
            if (curUser == null)
            {
                return Json(new { message = "用户名或密码错误" }, JsonRequestBehavior.AllowGet);
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
                Response.Cookies["zh"].Value = "";
                Response.Cookies["zh"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["mm"].Value = "";
                Response.Cookies["mm"].Expires = DateTime.Now.AddDays(-1);

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
                string curZh = "";
                string curMm = "";
                if (Request.Cookies["zh"] != null && Request.Cookies["mm"] != null)
                {
                    curZh = Server.UrlDecode(Request.Cookies["zh"].Value);
                    curMm = Server.UrlDecode(Request.Cookies["mm"].Value);
                }

                return Json(new { message = "1", myAccount = curZh, myPassWord = curMm, CurUser = curUser.Nickname }, JsonRequestBehavior.AllowGet);
            }
        }
        //桌面实体类
        public class MapItem
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public int Qty { get; set; }
            public string AppIcon { get; set; }
            public string AppUrl { get; set; }
            public string AppName { get; set; }

            public string RejectNumberName { get; set; }
            public string NotauditedNumberUrl { get; set; }
            public string RejectNumberUrl { get; set; }
            public int RejectNumberQty { get; set; }
            public int NotauditedNumberQty { get; set; }
            public string ModName { get; set; }
          
         
      
        }
        public JsonResult getMapItem(string CurUser)
        { 
            //花名
            string nickName = CurUser;
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == nickName);
            //真名
            string trueName = MOD_User.Name;
            List<MapItem> MapListModel = new List<MapItem>();
            List<MapItem> MapList = null;
            //报损模块
            string lossReportSQL = "SELECT A.AppName AS Name,A.AppIcon AS AppIcon,A.AppUrl AS AppUrl,count(B.ID) AS QTY From T_SysModule AS A,[T_LossReportApprove] AS B ,T_LossReport AS C WHERE  A.AppName = '报损管理'  and B.ApproveName = '" + trueName + "' and B.ApproveTime is null AND B.Oid = C.ID AND  C.IsDelete = 0 and C.Status != 3 GROUP BY A.AppName,A.AppIcon,A.AppUrl";
            MapList = db.Database.SqlQuery<MapItem>(lossReportSQL).ToList();
            if (MapList.Count > 0 && MapList[0].Qty > 0)
            {
                MapListModel.Add(MapList[0]);
            }
            //返现模块
            string cashBackSQL = "SELECT A.AppName AS Name,A.AppIcon AS AppIcon,A.AppUrl AS AppUrl,count(B.ID) AS Qty From T_SysModule AS A,[T_CashBackApprove] AS B ,T_CashBack AS C  WHERE  A.AppName = '返现管理'  and B.ApproveName = '" + nickName + "' and B.ApproveTime is null AND B.Oid = C.ID AND  C.For_Delete = 0 and C.Status != 3 GROUP BY A.AppName,A.AppIcon,A.AppUrl";
            MapList = db.Database.SqlQuery<MapItem>(cashBackSQL).ToList();
            if (MapList.Count > 0 && MapList[0].Qty > 0)
            {
                MapListModel.Add(MapList[0]);
            }
            //退货退款
            var depart = db.T_RetreatGroup.AsQueryable();
            string[] roleArray = {};
            foreach (var item in depart)
            {
                string[] arrayItem = item.Crew.Split(',');
                int theFlag = Array.IndexOf(arrayItem, nickName);
                if (theFlag != -1)
                {
                    List<string> roleList = roleArray.ToList();
                    roleList.Add(item.GroupName);
                    roleArray = roleList.ToArray();
                }
            }
            string strRetreatRole ="";
            if (roleArray.Length > 0)
            {
                foreach (var item in roleArray)
                {
                    strRetreatRole += "'" + item + "',";
                }
                strRetreatRole = strRetreatRole.Substring(0, strRetreatRole.Length - 1);
            }
            else
            {
                strRetreatRole = "''";
            }
            string retreatSQL = "SELECT A.AppName AS Name,A.AppIcon AS AppIcon,A.AppUrl AS AppUrl,B.Qty FROM T_SysModule as A,(SELECT COUNT(ID) AS Qty FROM T_Retreat WHERE Isdelete = 0 AND Status != 3 AND ID IN (SELECT Oid FROM T_RetreatAppRove WHERE ApproveTime IS NULL AND (ApproveName = '" + nickName + "' OR ApproveName IN (" + strRetreatRole + ")))) AS B WHERE AppName = '退货退款管理' ";
            MapList = db.Database.SqlQuery<MapItem>(retreatSQL).ToList();
            if (MapList.Count > 0 &&MapList[0].Qty > 0)
            {
                    MapListModel.Add(MapList[0]);
            }
            //借支管理
            string borrowSQL = "SELECT A.AppName AS Name,A.AppIcon AS AppIcon,A.AppUrl AS AppUrl,count(B.ID) AS QTY From T_SysModule AS A,[T_BorrowApprove] AS B ,T_Borrow AS C WHERE  A.AppName = '借支管理' and B.ApproveName = '" + nickName + "' and B.ApproveTime is null AND B.Pid = C.ID AND  C.IsDelete = 0 and C.BorrowState != 3 GROUP BY A.AppName,A.AppIcon,A.AppUrl";
            MapList = db.Database.SqlQuery<MapItem>(borrowSQL).ToList();
            if (MapList.Count > 0 && MapList[0].Qty > 0)
            {
                MapListModel.Add(MapList[0]);
            }
            //报销管理
            string expenseSQL = "SELECT A.AppName AS Name,A.AppIcon AS AppIcon,A.AppUrl AS AppUrl,COUNT(B.ID) AS Qty FROM T_SysModule AS A,(SELECT ID FROM T_Expense WHERE IsDelete = 0 AND Status ! = 3 AND ID IN  (SELECT Reunbursement_id FROM T_ExpenseApprove WHERE ApproveName = '" + nickName + "' and ApproveDate IS  NULL)) AS  B WHERE AppName = '报销管理' GROUP BY A.AppName,A.AppIcon,A.AppUrl";
            MapList = db.Database.SqlQuery<MapItem>(expenseSQL).ToList();
            if (MapList.Count > 0 && MapList[0].Qty > 0)
            {
                MapListModel.Add(MapList[0]);
            }
            return Json(MapListModel);
        }

        #region 桌面数据
        //桌面数据 
        public JsonResult getMapItem2(string CurUser)
        {
          
            //花名
            string name = CurUser;
            //真名
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == name);
            string Zname = MOD_User.Name;
            //根据花名去对应T_Role的ID
            string power = db.T_User.FirstOrDefault(s => s.Nickname == name).Power;
            int pow = Convert.ToInt32(power);
            //T_Role表的ID得到具体T_SysModule的ID
            string access = db.T_Role.FirstOrDefault(a => a.ID == pow).Access;
            string sql = "select ModularName as Name,'' as RejectNumberName,isnull(SUM(NotauditedNumber),0) as NotauditedNumberQty,isnull(SUM(RejectNumber),0) as RejectNumberQty,'' as  NotauditedNumberUrl,'' as RejectNumberUrl  from T_ModularNotaudited  where PendingAuditName='" + name + "' or PendingAuditName='" + Zname + "'";
            if (name == "向日葵" || name == "曹朝霞")
            {
                sql += " or  PendingAuditName='财务'";
            }
            else if (name == "半夏" || name == "君君" || name == "小雨" || name == "苗苗")
            {
                sql += " or  PendingAuditName='快递'";
            }
            else if (name == "半夏")
            {
                sql += " or  PendingAuditName='审单'";
            }
            else if (name == "栀子" || name == "小小" || name == "青莲" || name == "蔚蓝" || name == "超女" || name == "蒲公英" || name == "梦理" || name == "糖小回" || name == "小克" || name == "陈陈")
            {
                sql += " or  PendingAuditName='长沙制单'";
            }
            else if (name == "离陌" || name == "月神" || name == "栀子" || name == "南烟" || name == "小五" || name == "知秋")
            {
                sql += " or  PendingAuditName='仓库'";
            }
            else if (name == "部门主管" || name == "小姝" || name == "成风" || name == "武装色" || name == "金柚")
            {
                sql += " or  PendingAuditName='部门主管'";
            }
            
            sql += " GROUP BY ModularName";
            List<MapItem> MapList = db.Database.SqlQuery<MapItem>(sql).ToList();
            List<MapItem> MapListModel = new List<MapItem>();
            foreach (MapItem item in MapList)
            {
                if (item.Name == "退货退款")
                {
                    MapItem MapItemZmodel = new MapItem();
                   
                    MapItemZmodel.Name = item.Name;
                    MapItemZmodel.AppIcon = item.AppIcon;
                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "退货退款未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的退货退款").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "退货退款未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                 
                    
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资金调拨")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资金调拨未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资金调拨").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资金调拨未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                  
                  
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资金冻结")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资金冻结未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资金冻结").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资金冻结未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                 
                
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "报损")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "报损未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的报损").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "报损未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "借支")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "借支未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的借支").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "借支未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                   
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "资产变更")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "资产变更待审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的资产变更").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "资产变更待审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
               
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "报销")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "报销未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的报销").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "报销未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                 
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "产品采购")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "产品采购未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "产品我的采购").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "产品采购未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                    
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "换货")
                {
                    MapItem MapItemZmodel = new MapItem();

                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "换货未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的换货").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "换货未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "补发货")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "补发货未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的补发货").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "补发货未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
              
                    MapListModel.Add(MapItemZmodel);
                }
                if (item.Name == "返现")
                {
                    MapItem MapItemZmodel = new MapItem();
                    MapItemZmodel.Name = item.Name;

                    MapItemZmodel.Qty = item.NotauditedNumberQty;
                    List<T_SysModule> NotauditedNumberSysmodel = db.T_SysModule.Where(a => a.Name == "返现未审核").ToList();
                    List<T_SysModule> RejectNumberSysmodel = db.T_SysModule.Where(a => a.Name == "我的返现").ToList();
                    if (NotauditedNumberSysmodel.Count > 0)
                    {
                        MapItemZmodel.Name = "返现未审核";
                        MapItemZmodel.Url = NotauditedNumberSysmodel[0].Url;
                        MapItemZmodel.AppUrl = NotauditedNumberSysmodel[0].AppUrl;
                        MapItemZmodel.AppIcon = NotauditedNumberSysmodel[0].AppIcon;
                        MapItemZmodel.AppName = NotauditedNumberSysmodel[0].AppName;
                    }
                   
                    MapListModel.Add(MapItemZmodel);
                }
            }

          
            return Json(MapListModel);
        }

        #endregion
    }
}
