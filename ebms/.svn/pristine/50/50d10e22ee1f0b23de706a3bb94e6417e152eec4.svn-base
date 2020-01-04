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
    public class AppLossReportController : Controller
    {
        //
        // GET: /APP/AppLossReport/
        //APP 报损控制器
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
           
            return View();
        }
        public ActionResult Detail(int id)
        {
            ViewData["ID"]=id;
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
        //列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = -1, int myList=0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_LossReportApprove> ApproveMod = new List<T_LossReportApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_LossReportApprove.Where(a => a.ApproveName == name).ToList();
            }
            else
            {
                ApproveMod = db.T_LossReportApprove.Where(a => a.ApproveName == name && a.Status == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_LossReport> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_LossReport
                                                     where Arry.Contains(r.ID) && r.IsDelete == 0 && r.Status != 3
                                                     select r;
            }
            else {
               queryData = from r in db.T_LossReport
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
                string str = item.Shop;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "报损店铺：" + str;
                i.title = "申请人：" + item.PostUser;
            
                i.remark = item.Status;
                list.Add(i);
            }


            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //报损主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string PostUser { get; set; }
            public string Department { get; set; }
            public string Shop { get; set; }
            public int Status { get; set; }
            public string Code { get; set; }
            public Nullable<decimal> Total { get; set; }
            public Nullable<System.DateTime> PostTime { get; set; }
            public Nullable<int> IsPzStatus { get; set; }
        }
        //详情页面数据加载
        public JsonResult GetDetail(int ID,string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_LossReport mod = db.T_LossReport.Find(ID);
            mainItem list = new mainItem();
            list.Code = mod.Code;
            list.Department = GetDaparementString(mod.Department);
            list.PostTime = mod.PostTime;
            list.ID = mod.ID;
            list.PostUser = mod.PostUser;
            list.Shop = mod.Shop;
            list.Total = mod.Total;
            list.Status = mod.Status;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            //详情
            IQueryable<T_LossReportDetail> mod_Detail = db.T_LossReportDetail.Where(a => a.Oid == ID);
            string modDetail = JsonConvert.SerializeObject(mod_Detail, Lib.Comm.setTimeFormat());
            //审核记录
            IQueryable<T_LossReportApprove> mod_Approve = db.T_LossReportApprove.Where(a => a.Oid == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());
            //查看是不是财务主管
             T_LossReportApproveConfig financeMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Type == "财务主管");//审核财务主管一步
            string financeAdmin = financeMod.Name;
            T_LossReport lossreport = db.T_LossReport.Find(ID);
            //用于判断是不是 财务主管 且 不是部门主管身份进来审核
            int nextMan = 0;
            if (name == financeAdmin && lossreport.Step > 0)
            {
                nextMan = 1;
            }
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_LossReportApprove MyApprove = db.T_LossReportApprove.FirstOrDefault(a => a.Oid == ID && a.ApproveName == name && a.ApproveTime == null);
            if (MyApprove!=null) {
                myCheck = 1;
            }
            result += "{\"Loss\":[" + modJson + "],\"Detail\":" + modDetail + ",\"Approve\":" + approve + ",\"nextMan\":" + nextMan + ",\"myCheck\":" + myCheck + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //审核
        [HttpPost]
        public JsonResult Check(int id, int status, string memo, string checkMan, string CurUser)
        {
            using (TransactionScope sc = new TransactionScope())
            { 
            T_LossReportApprove modelApprove = db.T_LossReportApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名      
            string curName = MOD_User.Name;
            T_LossReportApproveConfig financeMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Type == "财务主管");//审核财务主管一步
            string financeAdmin = financeMod.Name;
            string result = "";
            if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            modelApprove.Memo = memo;
            modelApprove.ApproveTime = DateTime.Now;
            modelApprove.Status = status;
            db.Entry<T_LossReportApprove>(modelApprove).State = System.Data.EntityState.Modified;
            int i = db.SaveChanges();
            if (i > 0)
            {
                T_LossReport model = db.T_LossReport.Find(id);
                T_LossReportApprove newApprove = new T_LossReportApprove();
                if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                if (status == 1)
                {
                    //同意
                    int step = int.Parse(model.Step.ToString());
                    step++;
                    IQueryable<T_LossReportApproveConfig> config = db.T_LossReportApproveConfig.AsQueryable();
                    int stepLength = config.Count();//总共步骤
                    if (step < stepLength)
                    {
                        //不是最后一步，主表状态为0 =>审核中
                        model.Status = 0;
                        T_LossReportApproveConfig stepMod = db.T_LossReportApproveConfig.SingleOrDefault(a => a.Step == step);
                        string nextName = stepMod.Name;
                        //下一步审核人不是null  审核记录插入一条新纪录
                        newApprove.Memo = "";
                        newApprove.Oid = id;
                        newApprove.Status = -1;
                        if (nextName != null)
                        {
                            newApprove.ApproveName = nextName;
                        }
                        if (curName == financeAdmin && model.Step > 0)  //如果是以财务主管来审核
                        {
                            newApprove.ApproveName = checkMan;
                        }
                        db.T_LossReportApprove.Add(newApprove);
                        db.SaveChanges();
                    }
                    else
                    {
                        //最后一步，主表状态改为 1 => 同意
                        model.Status = status;
                    }
                    model.Step = step;
                    db.Entry<T_LossReport>(model).State = System.Data.EntityState.Modified;
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
                    model.Step = 0;
                    model.Status = 2;
                    db.Entry<T_LossReport>(model).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    //审核流程结束 申请人编辑后插入下一条记录 
                    result = "保存成功";
                }
            }
            else
            {
                result = "保存失败";
            }
            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = CurUser;
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "报损";
                    ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
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
