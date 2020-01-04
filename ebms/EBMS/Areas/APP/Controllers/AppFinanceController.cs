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
    public class AppFinanceController : Controller
    {
        //
        // GET: /APP/AppFinance/资金调拨 app 控制器
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
        //资金调拨列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = 9999, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_FundApprove> ApproveMod = new List<T_FundApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_FundApprove.Where(a => a.ApproveName == name).ToList();
            }
            else
            {
                ApproveMod = db.T_FundApprove.Where(a => a.ApproveName == name && a.Status == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].ItemId.ToString());
            }
            IQueryable<T_FundAllot> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_FundAllot
                            where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_FundAllot
                            where r.IsDelete == 0 && r.PostUser == name && r.Status != 3
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
                string str = item.Department;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "部门：" + str;
                i.title = "申请人：" + item.PostUser;

                i.remark = int.Parse(item.Status.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string PostUser { get; set; }
            public string Department { get; set; }
            public string CompanyIn { get; set; }
            public string TheReceivingBank { get; set; }
            public string AccountNumber { get; set; }
            public string CompanyOut { get; set; }
            public string ThePaymentBank { get; set; }
            public string PaymentNumber { get; set; }
            public int Status { get; set; }
            public string UseOfProceeds { get; set; }
            public System.DateTime PostTime { get; set; }
            public int Step { get; set; }
            public double theMoney { get; set; }
            public string FundAllotCode { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public Nullable<int> IsPzStatus { get; set; }
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
            T_FundAllot mod = db.T_FundAllot.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.PostUser = mod.PostUser;
            list.Department = mod.Department;
            list.TheReceivingBank = mod.TheReceivingBank;
            list.CompanyIn = mod.CompanyIn;
            list.AccountNumber = mod.AccountNumber;
            list.CompanyOut = mod.CompanyOut;
            list.ThePaymentBank = mod.ThePaymentBank;
            list.PaymentNumber = mod.PaymentNumber;
            list.Status = mod.Status;
            list.UseOfProceeds = mod.UseOfProceeds;
            list.PostTime = mod.PostTime;
            list.Step = mod.Step;
            list.theMoney = Convert.ToDouble(mod.theMoney);
            list.PostTime = mod.PostTime;
            list.Status = mod.Status;
            list.FundAllotCode = mod.FundAllotCode;
            list.IsPzStatus = mod.IsPzStatus;

            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_FundApprove> mod_Approve = db.T_FundApprove.Where(a => a.ItemId == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_FundApprove Approve = db.T_FundApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.ItemId == ID);
            if (Approve == null)
            {
                Approve = db.T_FundApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.ItemId == ID);
            }


            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_FundApprove MyApprove = db.T_FundApprove.FirstOrDefault(a => a.ItemId == ID && a.ApproveName == name && a.ApproveTime == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            //财务主管？
            int Cashier = 0;
            int curStep = int.Parse(mod.Step.ToString());
            //支付帐号
            if (name == "殷治云")
            {
                Cashier = 1;
            }

            //付款账号
            string PayNumber = JsonConvert.SerializeObject(Com.PayNumber(), Lib.Comm.setTimeFormat());
            //资金调拨调出单位
            string CompanyOut = JsonConvert.SerializeObject(Com.CompanyOut(), Lib.Comm.setTimeFormat());
            //付款银行
            List<SelectListItem> listPayBank = Lib.Comm.PayBank;
            string ThePaymentBank = JsonConvert.SerializeObject(listPayBank, Lib.Comm.setTimeFormat());
            result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":" + myCheck + ",\"Cashier\":" + Cashier + ",\"CompanyOut\":" + CompanyOut + ",\"PayNumber\":" + PayNumber + ",\"ThePaymentBank\":" + ThePaymentBank + ",\"approveId\":" + Approve.ID + "}";
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        //审核Check
        [HttpPost]
        public JsonResult Check(int id, int approveId, int status, string memo, string companyOut, string paymentBank, string paymentNumber, string CurUser)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            var Approve = db.T_FundApprove.Find(approveId);
            var fund = db.T_FundAllot.FirstOrDefault(s => s.ID == Approve.ItemId);
            int step = fund.Step;

            var configNextModel = db.T_FundConfig.FirstOrDefault(s => s.Step > step);


            Approve.ApproveTime = DateTime.Now;
            Approve.Memo = memo;
            try
            {
                //同意
                if (status == 1)
                {
                    Approve.Status = 1;
                    //是否存在下一级
                    if (configNextModel != null)
                    {
                        fund.Step = configNextModel.Step;
                        fund.Status = 0;
                        var approveModel = new T_FundApprove();
                        approveModel.ApproveName = configNextModel.ApproveUser;
                        approveModel.Status = -1;
                        approveModel.ItemId = fund.ID;
                        db.T_FundApprove.Add(approveModel);
                    }
                    else
                    {
                        fund.Status = 1;
                        fund.CompanyOut = companyOut;
                        fund.ThePaymentBank = paymentBank;
                        fund.PaymentNumber = paymentNumber;
                        var time = DateTime.Now;
                        //调入记录
                        T_FundRecord modelIn = new T_FundRecord
                        {
                            Type = "资金调入",
                            Number = fund.FundAllotCode,
                            ShopName = fund.CompanyIn,
                            Account = fund.AccountNumber,
                            Cost = Convert.ToDecimal(fund.theMoney),
                            Time = time
                        };
                        db.T_FundRecord.Add(modelIn);
                        //调出记录
                        T_FundRecord modelOut = new T_FundRecord
                        {
                            Type = "资金调出",
                            Number = fund.FundAllotCode,
                            ShopName = fund.CompanyOut,
                            Account = fund.PaymentNumber,
                            Cost = Convert.ToDecimal("-" + fund.theMoney),
                            Time = time
                        };
                        db.T_FundRecord.Add(modelOut);
                    }
                }
                //不同意
                else
                {
                    Approve.Status = 2;
                    fund.Status = 2;
                    fund.Step = db.T_FundConfig.OrderBy(s => s.Step).First().Step;
                }


                var i = db.SaveChanges();


                string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_FundApprove where  ItemId in ( select id from T_FundAllot where IsDelete=0) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                string Nickname = CurUser;
                for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                {
                    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资金调拨未审核" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "资金调拨未审核";
                        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }

                if (i > 0)
                {
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
