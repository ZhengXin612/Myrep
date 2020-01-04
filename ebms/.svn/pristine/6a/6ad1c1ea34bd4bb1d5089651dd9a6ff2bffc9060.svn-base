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
    public class AppPurchaseController : Controller
    {
        //
        // GET: /APP/AppPurchase/
        //采购
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        class PurcasheItem{
               public string title { get; set; }//申请人
              public int uid { get; set; }//id
     
              public string subTitle { get; set; }//理由
             public string remark{ get; set; }//状态
             
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
           //仓库ID转换中文名
        public string GetWarehouseString(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {

                List<T_Warehouses> model = db.T_Warehouses.Where(a => a.code == code).ToList();
                if (model.Count > 0)
                {
                    return model[0].name;
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
        //采购列表     
            [HttpPost]
        public JsonResult GetList(string queryStr, string startSendTime, string endSendTime,string CurUser)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
           //真名
            string name = MOD_User.Name;
            List<T_PurchaseApprove> ApproveMod = db.T_PurchaseApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();

            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Purchase> queryData = from r in db.T_Purchase
                                               where Arry.Contains(r.ID) || (r.IsDelete == 0 && r.Status != -1)
                                               select r;
          
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.PurchaseOddNum != null && a.PurchaseOddNum.Contains(queryStr) || a.ApplyName != null && a.ApplyName.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(startSendTime))
            {
                DateTime startTime = DateTime.Parse(startSendTime);
                DateTime endTime = startTime.AddDays(5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            else if (!string.IsNullOrWhiteSpace(endSendTime))
            {
                DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                DateTime startTime = endTime.AddDays(-5);
                queryData = queryData.Where(s => s.ApplyDate >= startTime && s.ApplyDate <= endTime);
            }
            //pager.totalRows = queryData.Count();
            ////分页
            //queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<PurcasheItem> list = new List<PurcasheItem>();
            foreach (var item in queryData)
            {
                PurcasheItem i = new PurcasheItem();
                i.uid = item.ID;
                string str = item.ApplyReason;
                if (str == null) {
                    str = "";
                }
                if (str.Length >= 9) {
                   str = str.Substring(0,9) + "...";
                }
                i.subTitle = "采购理由：" + str;
                i.title = "申请人："+item.ApplyName;
                string remark = "";
                switch (item.Status)
                {
                    case -1:
                        remark = "未审核";
                        break;
                    case 0:
                       remark = "审核中";
                        break;
                    case 1:
                       remark = "已同意";
                        break;
                    case 2:
                       remark = "不同意";
                        break;
                    case 3:
                        remark = "已作废";
                        break;

                }

                i.remark = remark;
                list.Add(i);
            }

           // string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                string json =  JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            return Json(json,JsonRequestBehavior.AllowGet);
        }
    }
}
