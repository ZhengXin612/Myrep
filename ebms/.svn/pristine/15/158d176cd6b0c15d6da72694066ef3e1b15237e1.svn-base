using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class WeChatController : Controller
    {
        //
        // GET: /WeChat/
        EBMSEntities db = new EBMSEntities();
        public ActionResult ViewWeChatAdd()
        {

            ViewData["ExpenditureList"] = ManualExpressType();
            return View();
        }
        // GET: /ManualExpress/
        public static List<SelectListItem> ManualExpressType()
        {
            EBMSEntities db = new EBMSEntities();
            var list = db.T_WeChatAccounts.AsQueryable();
            var selectList = new SelectList(list, "type", "type");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public ActionResult ViewWeChatMy()
        {
            ViewData["ExpenditureList"] = ManualExpressType();
            ViewData["shop"] = Com.Shop();
            return View();
        }
         public ActionResult ViewWeChatAbnormalAdd()
        {
            ViewData["ShopNameList"] = Com.Shop();
            ViewData["ExpenditureList"] = ManualExpressType();
            return View();
         
         }
         public ActionResult ViewGoodsGY(int index)
         {
             ViewData["index"] = index;
             return View();
         }
        public ActionResult ViewWeChatList()
        {
            ViewData["ExpenditureList"] = ManualExpressType();
            ViewData["shop"] = Com.Shop();
            return View();
        }
        public ActionResult ViewWeChatDetail(int tid)
        {
            ViewData["ID"] = tid;
            T_WeChat model = db.T_WeChat.Find(tid);
            if (model == null)
            {
                return HttpNotFound();
            }
            GetApproveHistory(tid);
            return View(model);
        }
        //产品列表 
        [HttpPost]
        public ContentResult GetRetreatgoodsGY(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_goodsGY> queryData = db.T_goodsGY.Where(a => a.combine == "False").AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr) || a.code != null && a.code.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderBy(c => c.code).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_goodsGY> list = new List<T_goodsGY>();
            foreach (var item in queryData)
            {
                T_goodsGY i = new T_goodsGY();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
          //获取审核详情记录
          private void GetApproveHistory(int id = 0)
          {
              var history = db.T_WeChatAppRove.Where(a => a.Oid == id);
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
        public ActionResult ViewWeChatCheck()
        {

            ViewData["ExpenditureList"] = ManualExpressType();
            ViewData["shop"] = Com.Shop();
            return View();
        }
       
        public ActionResult ViewWeChatChecken()
        {
            ViewData["ExpenditureList"] = ManualExpressType();
            ViewData["shop"] = Com.Shop();
            return View();
        }
        public ActionResult ViewWeChatEdit(int ID)
        {
            var model = db.T_WeChat.Find(ID);
            ViewData["ExpenditureList"] = ManualExpressType();
            ViewData["ID"] = ID;

            return View(model);
        }
        [HttpPost]
        [Description("微信账目删除")]
        public JsonResult DeleteInvoiceApplyFinance(int ID)
        {
            T_WeChat model = db.T_WeChat.Find(ID);
            model.Isdelete = 1;
            db.Entry<T_WeChat>(model).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewWeChatReportCheck(int ID)
        {
            ViewData["ID"] = ID;
            T_WeChat Model = db.T_WeChat.SingleOrDefault(a => a.ID == ID);
            if (ID == 0)
                return HttpNotFound();
            ViewData["ID"] = ID;
            var history = db.T_WeChatAppRove.Where(a => a.Oid == ID);
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
        //编辑获取详情列表  
        public JsonResult EditGetDetail(Lib.GridPager pager, int ID)
        {
            IQueryable<T_WeChatDetail> queryData = db.T_WeChatDetail.Where(a => a.Pid == ID);
            pager.totalRows = queryData.Count();
          
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData.ToList(), Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //微信账单详情数据
        [HttpPost]
        public ContentResult GetWeChatDetailList(Lib.GridPager pager, int ID)
        {
            IQueryable<T_WeChatDetail> queryData = db.T_WeChatDetail.Where(a => a.Pid == ID);
         
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WeChatDetail> list = new List<T_WeChatDetail>();
            foreach (var item in queryData)
            {
                T_WeChatDetail i = new T_WeChatDetail();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);

        }
        [Description("微信账单管理")]
        public ContentResult GetWeChatList(Lib.GridPager pager, string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus, string CheckStatus)
        {

            IQueryable<T_WeChat> queryData = db.T_WeChat.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber== queryStr || a.WechatNumber==queryStr);
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName== Bzhu || a.Remarks != null && a.Remarks==Bzhu);

            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store));
            }
            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure == Expenditure);
            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc == selstatus);
            }
            if (!string.IsNullOrEmpty(CheckStatus))
            {
                int Cstatus = int.Parse(CheckStatus);
                queryData = queryData.Where(a => a.Status == Cstatus);
            }
            if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.EstablishDate >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.EstablishDate <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            List<T_WeChat> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            List<WeChatCost> footerList = new List<WeChatCost>();
            WeChatCost footer = new WeChatCost();
            footer.Wangwang = "总计:";
            if (list.Count() > 0)
                footer.Commission = decimal.Parse(list.Sum(s => s.Commission).ToString());
            else
                footer.Commission = 0;
            footerList.Add(footer);

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("我的微信账单")]
        public ContentResult GetIWeChat(Lib.GridPager pager, string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            IQueryable<T_WeChat> queryData = db.T_WeChat.Where(a => a.EstablishName == Nickname && a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber==queryStr || a.WechatNumber==queryStr);
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName==Bzhu || a.Remarks != null && a.Remarks==Bzhu);

            }

            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure == Expenditure);
            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store));
            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc == selstatus);
            }
            if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.EstablishDate >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.EstablishDate <= endTime);
            }

            pager.totalRows = queryData.Count();
            //分页
            List<T_WeChat> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            List<WeChatCost> footerList = new List<WeChatCost>();
            WeChatCost footer = new WeChatCost();
            footer.Wangwang = "总计:";
            if (list.Count() > 0)
                footer.Commission = decimal.Parse(list.Sum(s => s.Commission).ToString());
            else
                footer.Commission = 0;
            footerList.Add(footer);

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public class WeChatCost
        {
            public string Wangwang { get; set; }
            public decimal Commission { get; set; }
        }
        [Description("微信账单未审核")]
        public ContentResult GetWeChatCheck(Lib.GridPager pager, string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_WeChatGroup> GroupModel = db.T_WeChatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(Nickname))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_WeChatAppRove> ApproveMod = db.T_WeChatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == Nickname) && a.ApproveTime == null).ToList();
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
            string sql = "select * from T_WeChat r  where Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) ";
            if (arrID != null && arrID != "")
            {
                sql += " and ID in (" + arrID + ")";
            }
            else
            {
                sql += " and 1=2";
            }
            IQueryable<T_WeChat> queryData = db.Database.SqlQuery<T_WeChat>(sql).AsQueryable();
            //  IQueryable<T_MajorInvoice> queryData = db.T_MajorInvoice.Where(a=>a.).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber==queryStr || a.WechatNumber==queryStr);
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName==Bzhu || a.Remarks != null && a.Remarks==Bzhu);

            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc == selstatus);
            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store));
            }
            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure==Expenditure);
            }
            if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.EstablishDate >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.EstablishDate <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
          

            List<T_WeChat> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             List<WeChatCost> footerList = new List<WeChatCost>();
            WeChatCost footer = new WeChatCost();
            footer.Wangwang = "总计:";
            if (list.Count() > 0)
                footer.Commission = decimal.Parse(list.Sum(s => s.Commission).ToString());
            else
                footer.Commission = 0;
            footerList.Add(footer);

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        [Description("异常新增保存")]
        public JsonResult WeChatAbnormalAdd(T_WeChat model, string jsonStr)
        {
            decimal jiage = 0;
            List<T_WeChatDetail> details = App_Code.Com.Deserialize<T_WeChatDetail>(jsonStr);
            foreach (var item in details)
            {
                jiage += decimal.Parse(item.Price.ToString());
            }
            string Order = model.OrderNumber;
            List<T_WeChat> QueryOrder = db.T_WeChat.Where(a => a.OrderNumber == Order && a.Isdelete == 0).ToList();
            string Number = model.WechatNumber;
            List<T_WeChat> QueryNumber = db.T_WeChat.Where(a => a.WechatNumber == Number && a.Isdelete == 0).ToList();

            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.Commission = jiage;
            model.EstablishName = Nickname;
            model.EstablishDate = DateTime.Now;
            model.Status = -1;
            model.Step = 0;
            model.iszhengc = "异常订单";
            model.Isdelete = 0;
            if (QueryOrder.Count > 0)
            {
                model.SystemRemarks = "重复订单数据";
            } 
            if (QueryNumber.Count > 0)
            {
                model.SystemRemarks = "微信号重复";
            }
            if (QueryNumber.Count > 0&& QueryOrder.Count > 0)
            {
                model.SystemRemarks = "微信号和订单编号重复";
            }
            db.T_WeChat.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                foreach (var item in details)
                {
                    decimal s = decimal.Parse(item.Price.ToString());
                    item.Pid = model.ID;
                    db.T_WeChatDetail.Add(item);
                }
                db.SaveChanges();


                T_WeChatConfig modelconfig = db.T_WeChatConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                T_WeChatAppRove AppRoveModel = new T_WeChatAppRove();
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
                db.T_WeChatAppRove.Add(AppRoveModel);
                db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        [Description("新增保存")]
        public JsonResult WeChatAdd(T_WeChat model, string jsonStr)
        {
            decimal jiage = 0;
            List<T_WeChatDetail> details = App_Code.Com.Deserialize<T_WeChatDetail>(jsonStr);
            foreach (var item in details)
            {
                jiage+= decimal.Parse(item.Price.ToString());
            }
            string Order=model.OrderNumber;
            List<T_WeChat> QueryOrder = db.T_WeChat.Where(a => a.OrderNumber == Order&&a.Isdelete==0).ToList();
            if (QueryOrder.Count > 0)
            {
                model.SystemRemarks = "重复订单数据";
            }
            string Number = model.WechatNumber;
            List<T_WeChat> QueryNumber = db.T_WeChat.Where(a => a.WechatNumber == Number && a.Isdelete == 0).ToList();
            if (QueryNumber.Count > 0)
            {
                model.SystemRemarks = "微信号重复";
            }
            if (QueryNumber.Count > 0 && QueryOrder.Count > 0)
            {
                model.SystemRemarks = "微信号和订单编号重复";
            }
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            model.Commission = jiage;
            model.EstablishName = Nickname;
            model.EstablishDate = DateTime.Now;
            model.Status = -1;
            model.Step = 0;
            model.iszhengc = "正常订单";
            model.Isdelete = 0;
           
            db.T_WeChat.Add(model);

            int i = db.SaveChanges();
            if (i > 0)
            {
                foreach (var item in details)
                {
                    decimal s = decimal.Parse(item.Price.ToString());
                    item.Pid = model.ID;
                    db.T_WeChatDetail.Add(item);
                }
                db.SaveChanges();


                T_WeChatConfig modelconfig = db.T_WeChatConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                T_WeChatAppRove AppRoveModel = new T_WeChatAppRove();
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
                db.T_WeChatAppRove.Add(AppRoveModel);
                db.SaveChanges();
                return Json(i, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(i, JsonRequestBehavior.AllowGet);
            }
        }
        [Description("访问微信号查询方法")]
        public JsonResult WebNumberRepeat(string Number = "")
        {
            List<T_WeChat> QueryNumber = db.T_WeChat.Where(a => a.WechatNumber == Number && a.Isdelete == 0).ToList();
            if (QueryNumber.Count > 0)
            {
                return Json("该微信号重复，请核实是否正确后提交", JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);

        }

        //旺店通接口
        public static string GetTimeStamp()
        {
            return (GetTimeStamp(System.DateTime.Now));
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time, int length = 10)
        {
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString().Substring(0, length);
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为10位      
            return t;
        }

        public string CreateParam(Dictionary<string, string> dicReq, bool isLower = false)
        {
            //排序
            dicReq = dicReq.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in dicReq)
            {
                if (item.Key == "sign")
                    continue;
                if (i > 0)
                {
                    sb.Append(";");
                }
                i++;
                sb.Append(item.Key.Length.ToString("00"));
                sb.Append("-");
                sb.Append(item.Key);
                sb.Append(":");

                sb.Append(item.Value.Length.ToString("0000"));
                sb.Append("-");
                sb.Append(item.Value);
            }
            if (isLower)
                dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401").ToLower());
            else
            {
                dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401"));
            }
            sb = new StringBuilder();
            i = 0;
            foreach (var item in dicReq)
            {
                if (i == 0)
                {

                    sb.Append(string.Format("{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
                }
                else
                {
                    sb.Append(string.Format("&{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
                }
                i++;
            }
            // HttpUtility.UrlEncode(
            return sb.ToString();
        }
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
            string strMd5 = BitConverter.ToString(result);
            strMd5 = strMd5.Replace("-", "");
            return strMd5;// System.Text.Encoding.Default.GetString(result);
        }
        Dictionary<string, string> dic = new Dictionary<string, string>();
        [Description("得到管易的订单详情")]
        public JsonResult QuyerRetreatDetailBYcode(string code = "")
        {


            App_Code.GY gy = new App_Code.GY();

       
         
                dic.Clear();

                dic.Add("src_tid", code);
                //dic.Add("trade_no", code);

                dic.Add("sid", "hhs2");
                dic.Add("appkey", "hhs2-ot");
                dic.Add("timestamp", GetTimeStamp());
                string cmd = CreateParam(dic, true);

                string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/trade_query.php", cmd, Encoding.UTF8);
                string ssx = Regex.Unescape(ret);
                JsonData jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                string iscode = jsonData["total_count"].ToString();
                //total_count
                if (iscode != "0")
                {
                    JsonData jsontrades = jsonData["trades"];

                    if (jsontrades.Count != 0)
                    {
                        JsonData trades = jsontrades[0];
                        //店铺名称
                        string shop_name1 = trades["shop_name"].ToString();
                        //仓库编码
                        string warehouse_no = trades["warehouse_no"].ToString();
                        //原始订单编号
                        string src_tids = trades["src_tids"].ToString();
                        //下单时间
                        string trade_time = trades["trade_time"].ToString();
                        //付款时间
                        string pay_time = trades["pay_time"].ToString();
                        //旺旺号
                        string customer_name = trades["buyer_nick"].ToString();
                        //收件人姓名
                        string receiver_name1 = trades["receiver_name"].ToString();
                        //省
                        string receiver_province = trades["receiver_province"].ToString();
                        //市
                        string receiver_city = trades["receiver_city"].ToString();
                        //区
                        string receiver_district = trades["receiver_district"].ToString();
                        //详细地址
                        string receiver_address1 = trades["receiver_address"].ToString();
                        //电话号码
                        string receiver_mobile1 = trades["receiver_mobile"].ToString();
                        //邮政编码
                        string receiver_zip1 = trades["receiver_zip"].ToString();
                        //省市县
                        string receiver_area1 = trades["receiver_area"].ToString();
                        //快递公司编号
                        string logistics_code = trades["logistics_code"].ToString();
                        //快递单号
                        string logistics_no = trades["logistics_no"].ToString();
                        //买家留言
                        string buyer_message = trades["buyer_message"].ToString();
                        //客服备注
                        string cs_remark = trades["cs_remark"].ToString();
                        //实付金额
                        string paid = trades["paid"].ToString();
                        //商品详情
                        JsonData goods_list = trades["goods_list"];
                        //查询3次。对应到具体的省市区
                        //if (receiver_province != null)
                        //{

                        //    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_province);
                        //    if (commonarea != null)
                        //    {
                        //        receiver_province = commonarea.REGION_NAME;
                        //    }
                        //}
                        //if (receiver_city != null)
                        //{

                        //    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_city);
                        //    if (commonarea != null)
                        //    {
                        //        receiver_city = commonarea.REGION_NAME;
                        //    }
                        //    if (receiver_city == "市辖区")
                        //    {
                        //        receiver_city = receiver_province;
                        //        receiver_province = receiver_province.Substring(0, receiver_province.Length - 1);


                        //    }
                        //}
                        //if (receiver_district != null)
                        //{

                        //    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_district);
                        //    if (commonarea != null)
                        //    {
                        //        receiver_district = commonarea.REGION_NAME;
                        //    }
                        //}
                        //string ssq = receiver_province + "-" + receiver_city + "-" + receiver_district;
                        //查询一次..
                        //string shop_Code = "";

                        //if (shop_name1 != null)
                        //{
                        //    T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name1);
                        //    shop_Code = commonarea.shop_no;
                        //    //shop_Code = "tm004";
                        //}
                        
                        T_WeChat webchat = new T_WeChat
                        {
                        OrderNumber = code,
                        ShopName = shop_name1,
                        Wangwang = customer_name
                        };
                        List<T_WeChat> QueryOrder1 = db.T_WeChat.Where(a => a.OrderNumber == code && a.Isdelete == 0).ToList();
                        if (QueryOrder1.Count > 0)
                        {
                            webchat.SystemRemarks = "重复订单数据";
                        }
                        List<T_WeChatDetail> DetailsList1 = new List<T_WeChatDetail>();


                        for (int i = 0; i < goods_list.Count; i++)
                        {
                            T_WeChatDetail DetailsModel = new T_WeChatDetail();
                            DetailsModel.GoodsCode = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
                            DetailsModel.GoodsName = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
                            decimal qyt = decimal.Parse(goods_list[i]["actual_num"].ToString());
                            DetailsModel.qty = int.Parse(Math.Round(qyt).ToString());
                            DetailsList1.Add(DetailsModel);
                        }
                        var json1 = new
                        {
                            rows = (from r in DetailsList1
                                    select new T_MajorInvoiceDetails
                                    {
                                        Code = r.GoodsCode,
                                        Name = r.GoodsName,
                                        qty = int.Parse(r.qty.ToString()),
                                    }).ToArray()

                            //rows = (from r in DetailsList1
                            //        select new T_WeChatDetail
                            //        {
                            //            GoodsCode = r.GoodsCode,
                            //            GoodsName = r.GoodsName,
                            //            qty = r.qty
                            //        }).ToArray()
                        };


                        //return Json("-2", JsonRequestBehavior.AllowGet);
                        return Json(new { State = "Success", ModelList = webchat, Json = json1}, JsonRequestBehavior.AllowGet);

                    }

                }





             

         
            return Json("", JsonRequestBehavior.AllowGet);
         

        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        /// <summary>
        ///编辑保存 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewWeChatEditSave(T_WeChat model, string jsonStr)
        {
         
            using (TransactionScope sc = new TransactionScope())
            {

                try
                {
                    decimal jiage = 0;
                    List<T_WeChatDetail> details = App_Code.Com.Deserialize<T_WeChatDetail>(jsonStr);
                    T_WeChat expense = db.T_WeChat.Find(model.ID);
                    string Statusz = expense.Status.ToString();

                    if (Statusz != "-1" && Statusz != "2")
                    {
                        return Json(new { State = "-1", Message = "该数据已审核，不允许修改"}, JsonRequestBehavior.AllowGet);
                    }

                    T_WeChatConfig modelconfig = db.T_WeChatConfig.SingleOrDefault(a => a.Step == 0 && a.Reason == "1");
                    T_WeChatAppRove AppRoveModel = new T_WeChatAppRove();
                    int ID = model.ID;
                    string shenheName = "";
                    if (modelconfig.Name == null || modelconfig.Name == "")
                    {
                        shenheName = modelconfig.Type;
                    }
                    else
                    {
                        shenheName = modelconfig.Name;
                    }
                    if (expense.Status != -1)
                    {
                        
                        T_WeChatAppRove Approvemodel = new T_WeChatAppRove
                        {
                            ApproveName = shenheName,
                            Status = -1,
                            Oid = model.ID
                        };
                        db.T_WeChatAppRove.Add(Approvemodel);
                        db.SaveChanges();
                        expense.Step = 0;
                        expense.Status = -1;
                    }
                    //if (expense.Status == -1)
                    //{
                    //    T_ExpenseApprove Approve = db.T_ExpenseApprove.SingleOrDefault(s => s.Reunbursement_id == model.ID && !s.ApproveDate.HasValue);
                    //    Approve.ApproveName = shenheName;
                    //    db.SaveChanges();
                    //}
                    foreach (var item in details)
                    {
                        jiage += int.Parse(item.Price.ToString());
                    }
                    expense.Commission = jiage;
                    expense.Remarks = model.Remarks;
                    expense.Expenditure = model.Expenditure;
                    expense.WechatNumber = model.WechatNumber;
                     db.SaveChanges();
                
                       List<T_WeChatDetail> delMod = db.T_WeChatDetail.Where(a => a.Pid == ID).ToList();
                       foreach (var item in delMod)
                       {
                           db.T_WeChatDetail.Remove(item);
                       }
                   
                       foreach (var item in details)
                       {
                           item.Pid = model.ID;
                           db.T_WeChatDetail.Add(item);
                       }
                       db.SaveChanges();
                       sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //发票申请已审核列表  
        [HttpPost]
        public ContentResult GetWeChatCheckenList(Lib.GridPager pager, string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);


            List<T_WeChatAppRove> ApproveMod = db.T_WeChatAppRove.Where(a => (a.ApproveName == name || a.ApproveName == Nickname) && (a.Status == 1 || a.Status == 2)).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_WeChat> queryData = from r in db.T_WeChat
                                                   where Arry.Contains(r.ID) && r.Isdelete == 0
                                                   select r;
          
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber==queryStr || a.WechatNumber==queryStr);
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName==Bzhu || a.Remarks != null && a.Remarks==Bzhu);

            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc==selstatus);
            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store));
            }

            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure == Expenditure);
            }

            if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.EstablishDate >= startTime);
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.EstablishDate <= endTime);
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_WeChat> list = new List<T_WeChat>();
            foreach (var item in queryData)
            {
                T_WeChat i = new T_WeChat();
                i = item;
                list.Add(i);
            }
            List<WeChatCost> footerList = new List<WeChatCost>();
            WeChatCost footer = new WeChatCost();
            footer.Wangwang = "总计:";
            if (list.Count() > 0)
                footer.Commission = decimal.Parse(list.Sum(s => s.Commission).ToString());
            else
                footer.Commission = 0;
            footerList.Add(footer);

            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //审核 
        public JsonResult WeChatCheck(T_WeChat model, string status, string Memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

                int ID = model.ID;
                T_WeChat Invoicemodel = db.T_WeChat.SingleOrDefault(a => a.ID == ID && a.Isdelete == 0);
                if (Invoicemodel == null)
                {
                    return Json("数据可能被删除", JsonRequestBehavior.AllowGet);
                }

                List<T_WeChatGroup> WeChatGroupList = db.Database.SqlQuery<T_WeChatGroup>("select  * from T_WeChatGroup where Crew like '%" + Nickname + "%'").ToList();
                string GroupName = "";
                for (int z = 0; z < WeChatGroupList.Count; z++)
                {
                    if (z == 0)
                    {
                        GroupName += "'" + WeChatGroupList[z].GroupName + "'";
                    }
                    else
                    {
                        GroupName += "," + "'" + WeChatGroupList[z].GroupName + "'";
                    }
                }
                string sql = "select * from T_WeChatAppRove where Oid='" + ID + "' and ApproveTime is null ";
                if (GroupName != "" && GroupName != null)
                {
                    sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
                }
                else
                {
                    sql += "    and ApproveName='" + Nickname + "'  ";
                }
                List<T_WeChatAppRove> AppRoveListModel = db.Database.SqlQuery<T_WeChatAppRove>(sql).ToList();
                if (AppRoveListModel.Count == 0)
                {
                    return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
                }

                T_WeChatAppRove modelApprove = db.T_WeChatAppRove.FirstOrDefault(a => a.Oid == ID && a.ApproveTime == null);
              

                string result = "";
                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = Memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = int.Parse(status);
                db.Entry<T_WeChatAppRove>(modelApprove).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (status == "1")
                    {

                        T_WeChatAppRove newApprove = new T_WeChatAppRove();
                        int step = int.Parse(Invoicemodel.Step.ToString());
                        step++;
                        IQueryable<T_WeChatConfig> config = db.T_WeChatConfig.AsQueryable();
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {
                            Invoicemodel.Status = 0;
                            T_WeChatConfig stepMod = db.T_WeChatConfig.SingleOrDefault(a => a.Step == step);
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
                            db.T_WeChatAppRove.Add(newApprove);
                            db.SaveChanges();
                        }
                        else
                        {
                            Invoicemodel.Status = int.Parse(status);
                        }
                        Invoicemodel.Step = step;
                        db.Entry<T_WeChat>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
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
                        db.Entry<T_WeChat>(Invoicemodel).State = System.Data.Entity.EntityState.Modified;
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
        public partial class WeChatgetExcel
        {
            public int ID { get; set; }
            public string ShopName { get; set; }
            public string Wangwang { get; set; }
            public string OrderNumber { get; set; }
            public Nullable<decimal> Commission { get; set; }
            public string EstablishName { get; set; }
            public Nullable<System.DateTime> EstablishDate { get; set; }
            public string WechatNumber { get; set; }
            public string Remarks { get; set; }
            public Nullable<int> Status { get; set; }
            public Nullable<int> Step { get; set; }
            public Nullable<int> Isdelete { get; set; }
            public string Expenditure { get; set; }
            public string SystemRemarks { get; set; }
            public string iszhengc { get; set; }
            public string shenhebeizhu { get; set; }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus, string CheckStatus)
        {
            List<WeChatgetExcel> queryData = null;
            string sql = " select  t.ID,ISNULL(ShopName,'') as ShopName,wangwang,EstablishDate,OrderNumber,Commission,EstablishName,WechatNumber,Remarks,t.Status, Expenditure,SystemRemarks,iszhengc, (select top 1  Memo from T_WeChatAppRove where ApproveDName = '财务' and Oid = t.ID order by ID desc ) as shenhebeizhu from T_WeChat t where t.Isdelete = 0";

            //string sql = " select  t.ID,ISNULL(ShopName,'') as ShopName,wangwang,EstablishDate,OrderNumber,Commission,EstablishName,WechatNumber,Remarks,t.Status, Expenditure,SystemRemarks,iszhengc, (select top 1  Memo from T_WeChatAppRove where ApproveDName = '财务' and Oid = t.ID ) as shenhebeizhu From T_WeChat t inner join T_WeChatAppRove r on r.Oid = t.ID  where t.ID = r.Oid  and r.ApproveDName = '财务'";
            queryData = db.Database.SqlQuery<WeChatgetExcel>(sql).ToList();

            //IQueryable<T_WeChat> queryData = db.T_WeChat.Where(a => a.Isdelete == 0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber.Contains(queryStr) || a.WechatNumber.Contains(queryStr)).ToList();
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName.Contains(Bzhu) || a.Remarks != null && a.Remarks.Contains(Bzhu)).ToList();

            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure == Expenditure).ToList();
            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc == selstatus).ToList();
            }
            if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(statedate))
            {
                DateTime startTime = DateTime.Parse(statedate);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.EstablishDate >= startTime).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(EndDate))
            {
                DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.EstablishDate <= endTime).ToList();
            }
            if (!string.IsNullOrEmpty(CheckStatus))
            {
                int Cstatus = int.Parse(CheckStatus);
                queryData = queryData.Where(a => a.Status == Cstatus).ToList();
            }

            List<WeChatgetExcel> list = queryData.ToList();
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("店铺名称");
            row1.CreateCell(1).SetCellValue("订单号");
            row1.CreateCell(2).SetCellValue("旺旺号");
            row1.CreateCell(3).SetCellValue("总佣金");
            row1.CreateCell(4).SetCellValue("支出帐号");
            row1.CreateCell(5).SetCellValue("订单状态");
            row1.CreateCell(6).SetCellValue("系统备注");
            row1.CreateCell(7).SetCellValue("微信帐号");
            row1.CreateCell(8).SetCellValue("备注");
            row1.CreateCell(9).SetCellValue("操作人");
            row1.CreateCell(10).SetCellValue("操作时间");
            row1.CreateCell(11).SetCellValue("审核状态");
            row1.CreateCell(12).SetCellValue("审核备注");
            //Remark
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 30 * 256);
            sheet1.SetColumnWidth(2, 15 * 256);
            sheet1.SetColumnWidth(3, 15 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 20 * 256);
            sheet1.SetColumnWidth(8, 20 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            sheet1.SetColumnWidth(10, 20 * 256);
            sheet1.SetColumnWidth(11, 20 * 256);
            sheet1.SetColumnWidth(12, 20 * 256);
            double cost = 0;
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].ShopName) ? "" : list[i].ShopName);
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(list[i].OrderNumber) ? "" : list[i].OrderNumber);
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(list[i].Wangwang) ? "" : list[i].Wangwang);
                rowtemp.CreateCell(3).SetCellValue(Convert.ToDouble(list[i].Commission).ToString("0.00"));
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(list[i].Expenditure) ? "" : list[i].Expenditure);
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(list[i].iszhengc) ? "" : list[i].iszhengc);
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(list[i].SystemRemarks) ? "" : list[i].SystemRemarks);
                rowtemp.CreateCell(7).SetCellValue(string.IsNullOrWhiteSpace(list[i].WechatNumber) ? "" : list[i].WechatNumber);
                rowtemp.CreateCell(8).SetCellValue(string.IsNullOrWhiteSpace(list[i].Remarks) ? "" : list[i].Remarks);
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(list[i].EstablishName) ? "" : list[i].EstablishName);
                rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(list[i].EstablishDate.ToString()) ? "" : list[i].EstablishDate.ToString());
                string Cstatus = "";
                if (list[i].Status.ToString() == "-1")
                {
                    Cstatus = "未审核";
                }
                else if (list[i].Status.ToString() == "0")
                {
                    Cstatus = "审核中";
                }
                else if (list[i].Status.ToString() == "1")
                {
                    Cstatus = "已同意";
                }
                else if (list[i].Status.ToString() == "2")
                {
                    Cstatus = "不同意";
                }
                else if (list[i].Status.ToString() == "3")
                {
                    Cstatus = "已作废";
                }
                rowtemp.CreateCell(11).SetCellValue(Cstatus);
                rowtemp.CreateCell(12).SetCellValue(string.IsNullOrWhiteSpace(list[i].shenhebeizhu) ? "" : list[i].shenhebeizhu);

            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(7);
          
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "微信账单数据.xls");
        }
        /// <summary>
        /// 已审核导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutYishenheExcel(string queryStr, string statedate, string EndDate, string store, string Bzhu, string Expenditure, string selstatus)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            DateTime startTime = DateTime.Parse(statedate);
            DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
            string sql = "select * from T_WeChat where ID in ( select Oid from T_WeChatAppRove where ApproveTime>='" + startTime + "' and ApproveTime<='" + endTime + "' and ApproveName='" + Nickname + "' and Status='1') and Isdelete='0'";
            IQueryable<T_WeChat> queryData = db.Database.SqlQuery<T_WeChat>(sql).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.OrderNumber.Contains(queryStr) || a.WechatNumber.Contains(queryStr));
            }
            if (!string.IsNullOrEmpty(Bzhu))
            {
                queryData = queryData.Where(a => a.EstablishName != null && a.EstablishName.Contains(Bzhu) || a.Remarks != null && a.Remarks.Contains(Bzhu));

            }
            if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.ShopName.Equals(store));
            }
            if (!string.IsNullOrWhiteSpace(Expenditure) && !Expenditure.Equals("==请选择=="))
            {
                queryData = queryData.Where(a => a.Expenditure == Expenditure);
            }
            if (!string.IsNullOrEmpty(selstatus))
            {
                queryData = queryData.Where(a => a.iszhengc == selstatus);
            }
            //if (!string.IsNullOrWhiteSpace(statedate) && !string.IsNullOrWhiteSpace(statedate))
            //{
            //    DateTime startTime = DateTime.Parse(statedate);
            //    DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
            //    queryData = queryData.Where(s => s.EstablishDate >= startTime && s.EstablishDate <= endTime);
            //}
            //else if (!string.IsNullOrWhiteSpace(statedate))
            //{
            //    DateTime startTime = DateTime.Parse(statedate);
            //    DateTime endTime = startTime.AddDays(5);
            //    queryData = queryData.Where(s => s.EstablishDate >= startTime);
            //}
            //else if (!string.IsNullOrWhiteSpace(EndDate))
            //{
            //    DateTime endTime = DateTime.Parse(EndDate + " 23:59:59");
            //    DateTime startTime = endTime.AddDays(-5);
            //    queryData = queryData.Where(s => s.EstablishDate <= endTime);
            //}



            List<T_WeChat> list = queryData.ToList();
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("店铺名称");
            row1.CreateCell(1).SetCellValue("订单号");
            row1.CreateCell(2).SetCellValue("旺旺号");
            row1.CreateCell(3).SetCellValue("总佣金");
            row1.CreateCell(4).SetCellValue("支出帐号");
            row1.CreateCell(5).SetCellValue("订单状态");
            row1.CreateCell(6).SetCellValue("系统备注");
            row1.CreateCell(7).SetCellValue("微信帐号");
            row1.CreateCell(8).SetCellValue("备注");
            row1.CreateCell(9).SetCellValue("操作人");
            row1.CreateCell(10).SetCellValue("操作时间");
            //Remark
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 30 * 256);
            sheet1.SetColumnWidth(2, 15 * 256);
            sheet1.SetColumnWidth(3, 15 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            sheet1.SetColumnWidth(5, 20 * 256);
            sheet1.SetColumnWidth(6, 20 * 256);
            sheet1.SetColumnWidth(7, 20 * 256);
            sheet1.SetColumnWidth(8, 20 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            sheet1.SetColumnWidth(10, 20 * 256);

            double cost = 0;
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.Height = 3 * 265;
                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].ShopName) ? "" : list[i].ShopName);
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(list[i].OrderNumber) ? "" : list[i].OrderNumber);
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(list[i].Wangwang) ? "" : list[i].Wangwang);
                rowtemp.CreateCell(3).SetCellValue(Convert.ToDouble(list[i].Commission).ToString("0.00"));
                rowtemp.CreateCell(4).SetCellValue(string.IsNullOrWhiteSpace(list[i].Expenditure) ? "" : list[i].Expenditure);
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(list[i].iszhengc) ? "" : list[i].iszhengc);
                rowtemp.CreateCell(6).SetCellValue(string.IsNullOrWhiteSpace(list[i].SystemRemarks) ? "" : list[i].SystemRemarks);
                rowtemp.CreateCell(7).SetCellValue(string.IsNullOrWhiteSpace(list[i].WechatNumber) ? "" : list[i].WechatNumber);
                rowtemp.CreateCell(8).SetCellValue(string.IsNullOrWhiteSpace(list[i].Remarks) ? "" : list[i].Remarks);
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(list[i].EstablishName) ? "" : list[i].EstablishName);
                rowtemp.CreateCell(10).SetCellValue(string.IsNullOrWhiteSpace(list[i].EstablishDate.ToString()) ? "" : list[i].EstablishDate.ToString());


            }
            IRow heji = sheet1.CreateRow(list.Count() + 1);
            ICell heji1 = heji.CreateCell(7);

            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "微信账单数据.xls");
        }


        private string GetToken()
        {
            string url = "https://oapi.dingtalk.com/gettoken?corpid=ding2d039a809b22b5dc&corpsecret=ixiCpMGOiSCFzZ7pmCoKIq2r0QxIhY6eyuJ-0UKGx_WKtzE3UWDK6R9n7F_S3WtA";
            string ret = GY.HttpGet(url, "");
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            if (jsonData.Count == 4)
                return jsonData["access_token"].ToString();
            else
            {
                return "错误";
            }
        }


        public JsonResult Handle(int ID)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            if (Nickname != "半夏")
            {
                return Json("没有权限,请找半夏", JsonRequestBehavior.AllowGet);
            }
            //T_WeChatGroup ApproveMod = db.T_WeChatGroup.SingleOrDefault(a => a.GroupName == "财务");
            //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

            //string[] nick = ApproveMod.Crew.Split(',');
            //string nickname = "";
            //int os = 0;
            //for (int i = 0; i < nick.Length; i++)
            //{
            //    nickname = nick[i];
            //    if (nickname == Nickname)
            //    {
            //        os = 1;
            //    }
            //}
            //if (os == 0)
            //{
            //    return Json("您没有操作权限,请核实", JsonRequestBehavior.AllowGet);
            //}
            using (TransactionScope sc = new TransactionScope())
            {
                T_WeChat MOD = db.T_WeChat.Find(ID);
                if (MOD == null)
                {
                    return Json("找不到该记录", JsonRequestBehavior.AllowGet);
                }
                MOD.Status = 2;
                MOD.Step = 0;
              
                db.Entry<T_WeChat>(MOD).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                //查询审核记录
                int maxId = Convert.ToInt32(db.T_WeChatAppRove.Where(a => a.Oid == ID).Max(a => a.ID));
                T_WeChatAppRove modelList = db.T_WeChatAppRove.SingleOrDefault(a => a.ID == maxId);//除了该订单本身是否还存在没有作废的订单，如果
                if (modelList == null)
                {
                    return Json("撤回失败", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    modelList.Status = 2;
                    modelList.ApproveTime = null;
                    db.Entry<T_WeChatAppRove>(modelList).State = System.Data.Entity.EntityState.Modified;
                    i = db.SaveChanges();
                }
                string result = "";
                if (i > 0)
                {

                    result = "撤回成功";
                    T_OperaterLog log = new T_OperaterLog();
                    log.Module = "微信账目";
                    log.OperateContent = "操作处理的驳回" + MOD.OrderNumber;
                    log.Operater = Nickname;
                    log.OperateTime = DateTime.Now;
                    log.PID = MOD.ID;
                    db.T_OperaterLog.Add(log);

                    db.SaveChanges();
                }
                else
                {
                    result = "撤回失败";
                }
                ding(MOD.OrderNumber, (decimal)MOD.Commission);
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void ding(string order, decimal money)
        {
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

            
            //string tel = "13873155482";
            string number = "manager6456";//031365626621881757
            string cmd = "";

            string token = GetToken();
            string url = "https://oapi.dingtalk.com/message/send?access_token=" + token;//好护士
            string neir = "EBMS:" + Nickname + ":撤回微信账目已同意数据" + order + ",金额：" + money + "元,请知悉";
            cmd = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"text\",\"agentid\":\"97171864\"," +
                "\"text\":{ \"content\":\" " + neir + " \"} }";//好护士text
            string cmdss = "";
            string cmdoas = "";
            string ur = "http://ebms.hhs16.com/";
            string urr = "dingtalk://dingtalkclient/page/link?url=http%3a%2f%2febms.hhs16.com%3fpc_slide%3dfalse";

            cmdss = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"link\",\"agentid\":\"97171864\"," +
                "\"link\":{\"messageUrl\": \"http://ebms.hhs16.com\"," +
                                "\"picUrl\": \"@lALOACZwe2Rk\"," +
                                " \"title\": \"EBMS审批处理提醒\"," +
                                "\"text\": \"" + neir + "\"" +
                            "}}";//好护士link
            cmdoas = "{ \"touser\":\"" + number + "\"," +
                        "\"toparty\":\"\"," +
                        "\"msgtype\":\"oa\"," +
                        "\"agentid\":\"97171864\"," +
                        "\"oa\":{" +
                        "\"message_url\": \"" + ur + "\"," +
                        "\"pc_message_url\": \"" + urr + " \"," +
                            "\"head\": {" +
                            "\"bgcolor\": \"FFFF9900\"," +
                            "\"text\": \"EBMS审批处理提醒\"}," +
                        "\"body\": {" +
                        "\"title\": \"EBMS审批处理提醒\"," +
                        "\"content\": \"" + neir + "\"," +
                        "\"author\": \"ebms.admin\"}}}";//好护士oa
            string ret = GY.DoPosts(url, cmd, Encoding.UTF8, "json");//好护士
        }

    }
}
