using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
namespace EBMS.Areas.APP.Controllers
{
    public class AppCashBackController : Controller
    {
        //返现控制器
        // GET: /APP/AppCashBack/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(int id)
        {
            ViewData["ID"] = id;
            return View();
        }
        class resultItem
        {
            public string title { get; set; }//申请人
            public int uid { get; set; }//id
            public string subTitle { get; set; }//理由
            public int remark { get; set; }//状态

        }
        //部门 ID转换中文名
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
        //返现列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = -1, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_CashBackApprove> ApproveMod = new List<T_CashBackApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_CashBackApprove.Where(a => a.ApproveName == CurUser).ToList();
            }
            else
            {
                ApproveMod = db.T_CashBackApprove.Where(a => a.ApproveName == CurUser && a.Status == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_CashBack> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_CashBack
                            where Arry.Contains(r.ID) && r.For_Delete == 0 && r.Status != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_CashBack
                            where r.For_Delete == 0 && r.PostUser == CurUser && r.Status != 3
                            select r;
            }
            //pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((page - 1) * pageSize).Take(pageSize);
            List<resultItem> list = new List<resultItem>();
            foreach (var item in queryData)
            {
                resultItem i = new resultItem();
                i.uid = item.ID;
                string str = item.ShopName;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "店铺名称：" + str;
                i.title = "申请人：" + item.PostUser;

                i.remark = int.Parse(item.Status.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //返现主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string PostUser { get; set; }
            public string OrderNum { get; set; }
            public string VipName { get; set; }
            public string ShopName { get; set; }
            public string WangWang { get; set; }
            public string Reason { get; set; }
            public decimal BackMoney { get; set; }
            public string ApproveName { get; set; }
            public Nullable<decimal> OrderMoney { get; set; }
            public string AlipayName { get; set; }
            public string AlipayAccount { get; set; }
            public string Note { get; set; }
            public Nullable<System.DateTime> PostTime { get; set; }    
            public Nullable<int> Status { get; set; }    
            public string Repeat { get; set; }
            public int Method { get; set; }
            public string BackFrom { get; set; }
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //详情数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_CashBack mod = db.T_CashBack.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.PostUser = mod.PostUser;
            list.OrderNum = mod.OrderNum;
            list.VipName = mod.VipName;
            list.ShopName = mod.ShopName;
            list.WangWang = mod.WangWang;
            list.Reason = mod.Reason;
            list.BackMoney = mod.BackMoney;
            list.ApproveName = mod.ApproveName;
            list.OrderMoney = mod.OrderMoney;
            list.AlipayName = mod.AlipayName;
            list.AlipayAccount = mod.AlipayAccount;
            list.Note = mod.Note;
            list.PostTime = mod.PostTime;
            list.Status = mod.Status;
            list.Repeat = mod.Repeat;
            list.Method = mod.Method;
            list.BackFrom = mod.BackFrom;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_CashBackApprove> mod_Approve = db.T_CashBackApprove.Where(a => a.Oid == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_CashBackApprove MyApprove = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == ID && a.ApproveName == UserName && a.ApproveTime == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            //财务出纳？
            int Cashier = 0;
            int curStep = int.Parse(mod.Step.ToString());
            //支付帐号
            IQueryable<T_CashBackFrom> BackFrom = db.T_CashBackFrom.AsQueryable();
            T_CashbackMethod MOD_method = db.T_CashbackMethod.FirstOrDefault(a => a.Method == mod.Method && a.Step == curStep);
            if (MOD_method.Cashier == 1)
            {
                using (TransactionScope sc = new TransactionScope())
                {
                    if (UserName == MOD_method.Name)
                    {
                        //string order = mod.OrderNum;
                        //T_OrderList MOD_Order = db.T_OrderList.FirstOrDefault(a => a.platform_code == order);
                        //int cash = int.Parse(MOD_Order.Status_CashBack.ToString());
                        //int retreat = int.Parse(MOD_Order.Status_Retreat.ToString());
                        //string repeatString = "返现状态：";
                        //switch (cash)
                        //{
                        //    case 0:
                        //        repeatString += "未申请";
                        //        break;
                        //    case 1:
                        //        repeatString += "申请中";
                        //        break;
                        //    case 2:
                        //        repeatString += "已返现";
                        //        break;
                        //}
                        //repeatString += ",退货退款：";
                        //switch (retreat)
                        //{
                        //    case 0:
                        //        repeatString += "未退款";
                        //        break;
                        //    case 1:
                        //        repeatString += "未退款";
                        //        break;
                        //    case 2:
                        //        repeatString += "已退款";
                        //        break;
                        //}
                        Cashier = 1; //是财务出纳
                        //mod.Repeat = repeatString;
                        //db.Entry<T_CashBack>(mod).State = System.Data.EntityState.Modified;
                        //db.SaveChanges();                      
                    }
                    sc.Complete();
                }
                //是财务出纳 刷新 是否返现/退款 字段

            }
          string sBackFrom =  JsonConvert.SerializeObject(BackFrom, Lib.Comm.setTimeFormat());
          result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":" + myCheck + ",\"Cashier\":" + Cashier + ",\"BackFrom\":" + sBackFrom + "}";
                return Json(result, JsonRequestBehavior.AllowGet);
           
        }
        //审核 
        public JsonResult Check(int id, int status, string memo, string method, int cashier, string backfrom, string CurUser)
        {
         
            using (TransactionScope sc = new TransactionScope())
            {
                //当前用户
                string curUser = CurUser;
                //审核主记录
                T_CashBack MOD_Cash = db.T_CashBack.Find(id);
                if (MOD_Cash == null)
                {
                    return Json("找不到该记录", JsonRequestBehavior.AllowGet);
                }
                int _Method = int.Parse(MOD_Cash.Method.ToString());       //当前审核方法
                int _Step = int.Parse(MOD_Cash.Step.ToString());           //当前审核步骤
                T_CashBackApprove cur_Approve = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
                if (cur_Approve == null)
                    return Json("该记录已审核", JsonRequestBehavior.AllowGet);
                if (cur_Approve.ApproveName != curUser)
                    return Json("当前不是你审核，或者你的帐号在别处登录了", JsonRequestBehavior.AllowGet);
                List<T_CashbackMethod> MOD_Method = db.T_CashbackMethod.Where(a => a.Method == _Method).ToList();
                int methodLength = MOD_Method.Count();  //该方法总步骤数
                //修改审核记录
                T_CashBackApprove MOD_Approve = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
                MOD_Approve.Status = status;
                MOD_Approve.Note = memo;
                MOD_Approve.ApproveTime = DateTime.Now;
                db.Entry<T_CashBackApprove>(MOD_Approve).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                //同意
                if (status == 1)
                {
                    //不是最后一步
                    if (_Step < methodLength - 1)
                    {
                        _Step++;  //步骤加1
                        T_CashbackMethod MOD_Nextapprove = db.T_CashbackMethod.FirstOrDefault(a => a.Method == _Method && a.Step == _Step);//当前流程的步骤
                        //审核同意而且,插入新的审核记录 
                        T_CashBackApprove CashApprove = new T_CashBackApprove();
                        CashApprove.Oid = id;
                        CashApprove.Note = "";
                        CashApprove.Status = -1;
                        CashApprove.ApproveName = MOD_Nextapprove.Name;
                        db.T_CashBackApprove.Add(CashApprove);
                        db.SaveChanges();
                        //主表状态为0 =>审核中
                        MOD_Cash.Status = 0;
                    }
                    else
                    {
                        //最后一步 修改主表的状态
                        MOD_Cash.Status = status;
                    }
                    //判断是否是出纳，出纳返现资金 
                    if (cashier == 1)
                    {
                        //主表支出帐号
                        MOD_Cash.BackFrom = backfrom;
                        //修改订单
                        //查询是否还有该订单返现数据
                        //修改订单字段 
                        //T_OrderList MOD_Order = db.T_OrderList.Find(MOD_Cash.OrderId);
                        //if (MOD_Order.Status_CashBack != 2)
                        //{
                        //    MOD_Order.Status_CashBack = 2;
                        //    db.Entry<T_OrderList>(MOD_Order).State = System.Data.EntityState.Modified;
                        //    int o = db.SaveChanges();
                        //    if (o == 0)
                        //    {
                        //        return Json("审核失败", JsonRequestBehavior.AllowGet);
                        //    }
                        //}
                    }
                }
                if (status == 2)
                {
                    //不同意
                    MOD_Cash.Status = status;
                    _Step = 0;
                    //审核流程结束 申请人编辑后插入下一条记录 
                    //驳回 发短信
                    T_User user = db.T_User.FirstOrDefault(a => a.Nickname == MOD_Cash.PostUser);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(user.Tel))
                        {
                            string[] msg = new string[] { "返现", "不同意" };
                            string res = Lib.SendSMS.Send(msg, user.Tel);
                        }
                    }
                }

                MOD_Cash.Step = _Step;
                db.Entry<T_CashBack>(MOD_Cash).State = System.Data.EntityState.Modified;   //修改主表
                int i = db.SaveChanges();
                string result = "审核失败";
                if (i > 0)
                {
                    result = "审核成功";
                }
                List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "返现").ToList();
                if (ModularNotaudited.Count > 0)
                {
                    foreach (var item in ModularNotaudited)
                    {
                        db.T_ModularNotaudited.Remove(item);
                    }
                    db.SaveChanges();
                }
                string RetreatAppRoveSql = "  select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_CashBackApprove where  Oid in ( select ID from T_CashBack where For_Delete=0 and Status!=3 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName  ";
                List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                {
                    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "返现" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "返现";
                        ModularNotauditedModel.RejectNumber = 0;
                        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }

                //增加驳回数据
                string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_CashBack where Status='2' GROUP BY PostUser";
                List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

                for (int e = 0; e < RejectNumberQuery.Count; e++)
                {
                    //   string Name = RejectNumberQuery[e].PendingAuditName;
                    string PendingAuditName = RejectNumberQuery[e].PendingAuditName;


                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "返现" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "返现";
                        ModularNotauditedModel.NotauditedNumber = 0;
                        ModularNotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                        // string Name = RejectNumberQuery[e].PendingAuditName;
                        ModularNotauditedModel.PendingAuditName = PendingAuditName;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                        ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }


                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
