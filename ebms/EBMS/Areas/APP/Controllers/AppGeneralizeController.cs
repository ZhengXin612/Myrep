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
    public class AppGeneralizeController : Controller
    {
        //
        // GET: /APP/AppGeneralize/
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
            IQueryable<T_Generalize> queryData = db.T_Generalize.Where(s => s.IsDelete != 1 && s.Status == 2).AsQueryable();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((page - 1) * pageSize).Take(pageSize);
            List<resultItem> list = new List<resultItem>();
            foreach (var item in queryData)
            {
                resultItem i = new resultItem();
                i.uid = item.ID;
                string str = item.StoreName;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "店铺名称：" + str;
                i.title = "申请人：" + item.UploadName;

                i.remark = int.Parse(item.IsDispose.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
          
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string Guid { get; set; }
            public Nullable<System.DateTime> CreateDate { get; set; }
            public string PlatformCode { get; set; }
            public string PostUser { get; set; }
            public string StoreName { get; set; }
            public string OrderNumber { get; set; }
            public string ProductName { get; set; }
            public Nullable<decimal> Cost { get; set; }
            public Nullable<decimal> CommissionCost { get; set; }
            public string DKUserMessage { get; set; }
            public string AliNumber { get; set; }
            public string BankNumber { get; set; }
            public string TenPay { get; set; }
            public string IsCheck { get; set; }
            public string OrderType { get; set; }
            public string PayCommissionNumber { get; set; }
            public string Memo { get; set; }
            public string BorrowCode { get; set; }
            public string ResponsibleName { get; set; }
            public string IsSend { get; set; }
            public string IsCancel { get; set; }
            public string PlatformFlag { get; set; }
            public string WarhouseName { get; set; }
            public string UploadName { get; set; }
            public Nullable<int> Status { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public Nullable<int> IsDispose { get; set; }
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
            T_Generalize mod = db.T_Generalize.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.Guid = mod.Guid;
            list.CreateDate = mod.CreateDate;
            list.PlatformCode = mod.PlatformCode;
            list.PostUser = mod.PostUser;
            list.StoreName = mod.StoreName;
            list.OrderNumber = mod.OrderNumber;
            list.ProductName = mod.ProductName;
            list.Cost = mod.Cost;
            list.CommissionCost = mod.CommissionCost;
            list.DKUserMessage = mod.DKUserMessage;
            list.AliNumber = mod.AliNumber;
            list.BankNumber = mod.BankNumber;
            list.TenPay = mod.TenPay;
            list.IsCheck = mod.IsCheck;
            list.OrderType = mod.OrderType;
            list.PayCommissionNumber = mod.PayCommissionNumber;
            list.Memo = mod.Memo;
            list.BorrowCode = mod.BorrowCode;
            list.ResponsibleName = mod.ResponsibleName;
            list.IsSend = mod.IsSend;
            list.IsCancel = mod.IsCancel;
            list.PlatformFlag = mod.PlatformFlag;
            list.WarhouseName = mod.WarhouseName;
            list.UploadName = mod.UploadName;
            list.Status = mod.Status;
            list.IsDelete = mod.IsDelete;
            list.IsDispose = mod.IsDispose;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_GeneralizeApprove> mod_Approve = db.T_GeneralizeApprove.Where(a => a.Pid == mod.Guid);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_GeneralizeApprove Approve = db.T_GeneralizeApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.Pid == mod.Guid);
            if (Approve == null)
            {
                Approve = db.T_GeneralizeApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.Pid == mod.Guid);
            }
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            string MyApprove = db.T_GeneralizeApproveConfig.OrderByDescending(s => s.ID).First().ApproveUser == UserName ? "true" : "false";
          // T_FreezeApprove MyApprove = db.T_FreezeApprove.FirstOrDefault(a => a.freezeID == ID && a.ApproveName == name && a.ApproveTime == null);
            if (MyApprove == "true" && mod.IsDispose!=1)
            {
                myCheck = 1;
            }
            //付款银行
            List<SelectListItem> listPayBank = Lib.Comm.PayBank;
            string ThePaymentBank = JsonConvert.SerializeObject(listPayBank, Lib.Comm.setTimeFormat());
            result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":\"" + myCheck + "\",\"Guid\":\"" + Approve.Pid + "\"}";
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public JsonResult Check(string CurUser, string Guid, int status, string memo)
        {
           
               
                T_Generalize model = db.T_Generalize.SingleOrDefault(s => s.Guid.Equals(Guid));
                if (model.IsDispose == 1)
                    return Json(new { State = "Faile", Message = "该数据已处理" });
                model.IsDispose = 1;
                db.SaveChanges();
                T_GeneralizeApprove approve = new T_GeneralizeApprove
                {
                    ApproveName = CurUser,
                    ApproveTime = DateTime.Now,
                    ApproveStatus = status,
                    Memo = memo,
                    Pid = Guid
                };
                db.T_GeneralizeApprove.Add(approve);
                db.SaveChanges();
            
            return Json(new { State = "Success" });
        }
    }
}
