using EBMS.App_Code;
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
    public class AppFundFreezeController : Controller
    {
        //资金冻结
        // GET: /APP/AppFundFreeze/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
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
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = 9999, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_FreezeApprove> ApproveMod = new List<T_FreezeApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_FreezeApprove.Where(a => a.ApproveName == name).ToList();
            }
            else
            {
                ApproveMod = db.T_FreezeApprove.Where(a => a.ApproveName == name && a.ApproveStatus == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].freezeID.ToString());
            }
            IQueryable<T_Freeze> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_Freeze
                            where Arry.Contains(r.ID) && r.isDelete == 0 && r.state != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_Freeze
                            where r.isDelete == 0 && r.userName == CurUser && r.state != 3
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
                string str = item.shopName;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "店铺名称：" + str;
                i.title = "申请人：" + item.userName;

                i.remark = int.Parse(item.state.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string shopName { get; set; }
            public string alipay { get; set; }
            public decimal freezeMoney { get; set; }
            public string freezeReason { get; set; }
            public string remark { get; set; }
            public string userName { get; set; }
            public System.DateTime datetime { get; set; }
            public int state { get; set; }
            public int isDelete { get; set; }
            public Nullable<decimal> usedMoney { get; set; }
            public Nullable<decimal> surplusMoney { get; set; }
            public int Step { get; set; }
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
            T_Freeze mod = db.T_Freeze.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.shopName = mod.shopName;
            list.alipay = mod.alipay;
            list.freezeMoney = mod.freezeMoney;
            list.freezeReason = mod.freezeReason;
            list.remark = mod.remark;
            list.userName = mod.userName;
            list.datetime = mod.datetime;
            list.state = mod.state;
            list.usedMoney = mod.usedMoney;
            list.surplusMoney = mod.surplusMoney;
          

            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_FreezeApprove> mod_Approve = db.T_FreezeApprove.Where(a => a.freezeID == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_FreezeApprove Approve = db.T_FreezeApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.freezeID == ID);
            if (Approve == null)
            {
                Approve = db.T_FreezeApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.freezeID == ID);
            }


            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_FreezeApprove MyApprove = db.T_FreezeApprove.FirstOrDefault(a => a.freezeID == ID && a.ApproveName == name && a.ApproveTime == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            //财务主管？
            int Cashier = 0;
            int curStep = int.Parse(mod.Step.ToString());
            //取最后一步
            int lastStep = db.T_FreezeConfig.OrderByDescending(s => s.Step).First().Step;
          
            if (lastStep == curStep)
            {
                Cashier = 1;
            }
         
            //付款银行
            List<SelectListItem> listPayBank = Lib.Comm.PayBank;
            string ThePaymentBank = JsonConvert.SerializeObject(listPayBank, Lib.Comm.setTimeFormat());
            result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":" + myCheck + ",\"Cashier\":" + Cashier + ",\"approveId\":" + Approve.ID + "}";
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        //审核
        [HttpPost]
        public JsonResult Check(string CurUser,int approveId, int status,  string memo, decimal usedMoney = 0)
        {           
            var Approve = db.T_FreezeApprove.Find(approveId);
            var model = db.T_Freeze.FirstOrDefault(s => s.ID == Approve.freezeID);
            int step = model.Step;
            var configNextModel = db.T_FreezeConfig.FirstOrDefault(s => s.Step > step);
            Approve.ApproveTime = DateTime.Now;
            Approve.Memo = memo;
            try
            {
                //同意
                if (status == 3)
                {
                    Approve.ApproveStatus = 3;
                    //是否存在下一级
                    if (configNextModel != null)
                    {
                        model.Step = configNextModel.Step;
                        model.state = 1;
                        var approveModel = new T_FreezeApprove();
                        approveModel.ApproveName = configNextModel.ApproveUser;
                        approveModel.ApproveStatus = -1;
                        approveModel.freezeID = model.ID;
                        db.T_FreezeApprove.Add(approveModel);
                    }
                    else
                    {
                        if (usedMoney > model.freezeMoney)
                            return Json(new { State = "Faile", Message = "使用金额不能大于冻结金额" }, JsonRequestBehavior.AllowGet);
                        model.state = 2;
                        model.usedMoney = usedMoney;
                        model.surplusMoney = model.freezeMoney - model.usedMoney;
                    }
                }
                //不同意
                else
                {
                    Approve.ApproveStatus = 4;
                    model.state = 4;
                    model.Step = db.T_FreezeConfig.OrderBy(s => s.Step).First().Step;
                }
                var i = db.SaveChanges();


                if (i > 0)
                {

                    string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_FreezeApprove where  freezeID in ( select ID from T_Freeze where isDelete=0 ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName ";
                    List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                    string Nickname = CurUser;
                    for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                    {
                        string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                        T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金冻结未审核" && a.PendingAuditName == PendingAuditName);
                        if (NotauditedModel != null)
                        {
                            NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                            db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                        }
                        else
                        {
                            T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                            ModularNotauditedModel.ModularName = "资金冻结未审核";
                            ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                            ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                            ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                            db.T_ModularNotaudited.Add(ModularNotauditedModel);
                        }
                        db.SaveChanges();
                    }

                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { State = "Faile", Message = "操作失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
