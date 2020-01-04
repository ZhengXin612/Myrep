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
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
namespace EBMS.Areas.APP.Controllers
{
    public class AppRetreatController : Controller
    {
        //
        // GET: /APP/AppRetreat/
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
        //列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = -1, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_RetreatGroup> GroupModel = db.T_RetreatGroup.Where(a => a.Crew != null && (a.Crew.Contains(name) || a.Crew.Contains(CurUser))).ToList();
            string[] shenheName = new string[GroupModel.Count];
            for (int z = 0; z < GroupModel.Count; z++)
            {
                shenheName[z] = GroupModel[z].GroupName;
            }
            List<T_RetreatAppRove> ApproveMod = new List<T_RetreatAppRove>();
            if (Status == 9999)
            {
               ApproveMod = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == CurUser) && a.ApproveTime == null).ToList();
            }
            else {
              ApproveMod = db.T_RetreatAppRove.Where(a => (shenheName.Contains(a.ApproveName) || a.ApproveName == name || a.ApproveName == CurUser) && a.Status == Status).ToList();  
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
            }
            IQueryable<T_Retreat> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_Retreat
                            where Arry.Contains(r.ID) && r.Isdelete == "0" &&r.Status!=3
                            select r;
            }
            else
            {
                queryData = from r in db.T_Retreat
                            where r.Isdelete == "0" && r.Retreat_ApplyName == name && r.Status != 3
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
                string str = item.Retreat_dianpName;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 22)
                {
                    str = str.Substring(0, 22) + "...";
                }
                i.subTitle = "店铺名称：" + str;
                i.title = "申请人：" + item.Retreat_ApplyName;

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
            public Nullable<System.DateTime> Retreat_date { get; set; }
            public string Retreat_dianpName { get; set; }
            public string Retreat_wangwang { get; set; }
            public string Retreat_CustomerName { get; set; }
            public string Retreat_Remarks { get; set; }
            public Nullable<decimal> Retreat_Shouldjine { get; set; }
            public Nullable<decimal> Retreat_Actualjine { get; set; }
            public string Retreat_OrderNumber { get; set; }
            public string Retreat_expressName { get; set; }
            public string Retreat_expressNumber { get; set; }
            public string Retreat_GoodsSituation { get; set; }
            public string Retreat_Warehouse { get; set; }
            public string Retreat_ApplyName { get; set; }
            public string Retreat_Reason { get; set; }
            public Nullable<double> OrderMoney { get; set; }
            public Nullable<double> OrderpaymentMoney { get; set; }
            public string Retreat_PaymentAccounts { get; set; }
            public string CollectName { get; set; }
            public string CollectAddress { get; set; }
            public string Isdelete { get; set; }
            public string DeleteRemarks { get; set; }
            public string OpenPieceName { get; set; }
            public Nullable<System.DateTime> OpenPieceDate { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public string repeat { get; set; }
            public Nullable<int> isSorting { get; set; }
            public string SortingName { get; set; }
            public string receivermobile { get; set; }
            public string isNoheadparts { get; set; }
        }
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        public partial class ModularQuery
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //绑定支付帐号xp
        public List<SelectListItem> GetRetreatPaymentList(string UserName)
        {

            string Nickname = UserName;
            var list = db.T_RetreatPayment.Where(a => a.PaymentAccountsName == Nickname);
            var selectList = new SelectList(list, "PaymentAccounts", "PaymentAccounts");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        public decimal QueryUnitPrice(string Code)
        {
            T_goodsGY model = db.T_goodsGY.SingleOrDefault(a => a.code == Code);
            string price = "0";
            if (model != null)
            {
                price = model.cost_price.ToString();
            }
            if (price == null || price == "")
            {
                price = "0";
            }

            return decimal.Parse(price);
        }
     
        //详情页面数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_Retreat mod = db.T_Retreat.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.Retreat_date = mod.Retreat_date;
            list.Retreat_dianpName = mod.Retreat_dianpName;
            list.Retreat_wangwang = mod.Retreat_wangwang;
            list.Retreat_CustomerName = mod.Retreat_CustomerName;
            list.Retreat_Remarks = mod.Retreat_Remarks;
            list.Retreat_Shouldjine = mod.Retreat_Shouldjine;
            list.Retreat_Actualjine = mod.Retreat_Actualjine;
            list.Retreat_OrderNumber = mod.Retreat_OrderNumber;
            list.Retreat_expressName = mod.Retreat_expressName;
            list.Retreat_expressNumber = mod.Retreat_expressNumber;
            list.Retreat_GoodsSituation = mod.Retreat_GoodsSituation;
            list.Retreat_Warehouse = mod.Retreat_Warehouse;
            list.Retreat_ApplyName = mod.Retreat_ApplyName;
            list.Retreat_Reason = mod.Retreat_Reason;
            list.OrderMoney = mod.OrderMoney;
            list.OrderpaymentMoney = mod.OrderpaymentMoney;
            list.Retreat_PaymentAccounts = mod.Retreat_PaymentAccounts;
            list.CollectName = mod.CollectName;
            list.CollectAddress = mod.CollectAddress;
            list.DeleteRemarks = mod.DeleteRemarks;
            list.OpenPieceName = mod.OpenPieceName;
            list.OpenPieceDate = mod.OpenPieceDate;
            list.Status = mod.Status;
            list.Step = mod.Step;
            list.repeat = mod.repeat;
            list.isSorting = mod.isSorting;
            list.SortingName = mod.SortingName;
            list.receivermobile = mod.receivermobile;
            list.isNoheadparts = mod.isNoheadparts;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            //详情
            IQueryable<T_RetreatDetails> mod_Detail = db.T_RetreatDetails.Where(a => a.Oid == ID);
            string modDetail = JsonConvert.SerializeObject(mod_Detail, Lib.Comm.setTimeFormat());
            //审核记录
            IQueryable<T_RetreatAppRove> mod_Approve = db.T_RetreatAppRove.Where(a => a.Oid == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());
            T_Reissue lossreport = db.T_Reissue.Find(ID);
            //用于判断是不是 财务主管 
            int nextMan = 0;
            int Step = db.T_ReissueApprove.ToList().Count;
            if (mod.Step != Step - 1)
            {
                nextMan = 1;
            }

           
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            //  T_ExchangeCenterApprove MyApprove = db.T_ExchangeCenterApprove.FirstOrDefault(a => a.Pid == ID && (a.ApproveName == UserName || a.ApproveUser == UserName) && a.ApproveTime == null);
           // string sql = "select * from T_ReissueApprove where  Pid='" + ID + "' and ApproveTime is null  and   (ApproveUser='" + UserName + "' or ApproveName='" + UserName + "' or ApproveName in (select GroupName  from T_ReissueGroup where GroupUser in ('" + UserName + "')) )";
            string sql = "SELECT * FROM T_RetreatAppRove WHERE Oid='" + ID + "' and   ( ApproveName='" + UserName + "' or ApproveName=(select GroupName from T_RetreatGroup where Crew like '%" + UserName + "%')) and ApproveTime is  null";
            List<T_RetreatAppRove> MyApprove = db.Database.SqlQuery<T_RetreatAppRove>(sql).ToList();
            if (MyApprove.Count > 0)
            {
                myCheck = 1;
            }
            //审核人
            List<SelectListItem> getCheckMan = new List<SelectListItem>();
            var approveusers = db.T_ExpenseApproveConfig.FirstOrDefault(a => a.Step == mod.Step + 1);
            if (approveusers != null)
            {
                //如果是动态获取当前部门主管
                if (approveusers.ApproveUser.Equals("部门主管"))
                {
                    List<SelectListItem> items = new List<SelectListItem>();
                    items.Add(new SelectListItem { Text = "请选择", Value = "9999" });
                    getCheckMan = items;

                }
                //如果还有其他的审核组或者动态绑定的数据 再增加else
                //如果是固定的某些人
                else
                {
                    string[] array = approveusers.ApproveUser.Split(',');
                    List<SelectListItem> items = new List<SelectListItem>();

                    foreach (var item in array)
                    {
                        T_User user = db.T_User.FirstOrDefault(a => a.Nickname.Equals(item));
                        if (user != null)
                            items.Add(new SelectListItem { Text = user.Nickname, Value = user.ID.ToString() });
                    }
                    getCheckMan = items;
                }
            }
            else
            {
                getCheckMan = null;
            }
            string CheckManJson = JsonConvert.SerializeObject(getCheckMan, Lib.Comm.setTimeFormat());
           //未审核记录ID
            //T_ReissueApprove Approve = db.T_ReissueApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.Pid == ID);
            //if (Approve == null)
            //{
            //    Approve = db.T_ReissueApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.Pid == ID);
            //}
            //查询当前审核人是否是财务 T_RetreatAppRove
            int Finance = 0;
           
            T_RetreatAppRove appRoveMod = db.T_RetreatAppRove.FirstOrDefault(a => a.ApproveTime == null && a.ApproveName == "财务" && a.Oid == ID);
            if (appRoveMod != null) {
                T_RetreatGroup retreatGroupMod = db.T_RetreatGroup.FirstOrDefault(a => a.GroupName == "财务");
                if (retreatGroupMod != null) {
                    string [] crewList = retreatGroupMod.Crew.Split(',');
                    if (crewList.Contains(UserName)) {
                        Finance = 1;
                    }
                }
            }
            decimal Actualjine = 0;
            if (mod.Retreat_Actualjine != null) {
                Actualjine = decimal.Parse(mod.Retreat_Actualjine.ToString());
            }
          
            //支付宝下垃
            List<SelectListItem> listPayBank = GetRetreatPaymentList(UserName);
            string ThePaymentBank = JsonConvert.SerializeObject(listPayBank, Lib.Comm.setTimeFormat());
            result += "{\"Main\":[" + modJson + "],\"Detail\":" + modDetail + ",\"Approve\":" + approve + ",\"Step\":" + nextMan + ",\"myCheck\":" + myCheck + ",\"listPayBank\":" + ThePaymentBank + ",\"Actualjine\":" + Actualjine + ",\"Finance\":" + Finance + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //得到用户的部门
        public string DepartmentQuer(string name)
        {
            T_User userModel = db.T_User.SingleOrDefault(a => a.Name == name || a.Nickname == name);

            return userModel.DepartmentId;
        }
        //生成报损Code
        public string CodeQuery()
        {
            //自动生成批号
            string code = "BS-DS-";
            string date = DateTime.Now.ToString("yyyyMMdd");
            //查找当前已有的编号
            List<T_LossReport> listLoss = db.T_LossReport.Where(a => a.Code.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (listLoss.Count == 0)
            {
                code += date + "-" + "0001";
            }
            else
            {
                string old = listLoss[0].Code.Substring(15);
                int newcode = int.Parse(old) + 1;
                code += date + "-" + newcode.ToString().PadLeft(4, '0');
            }
            return code;
        }
        public string Tz(T_Retreat model, T_RetreatDetails detail)
        {
            //   上传管易。调整单新增
            int? qtyGY = detail.qty;
            string cpcode = "{\"item_code\":\"" + detail.item_code + "\",\"qty\":" + qtyGY + "}";
            EBMS.App_Code.GY gy = new App_Code.GY();
            string cmd = "";
            cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.stock.adjust.add\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                             "\"order_type\":\"001\"," +
                              "\"note\":\"平台单号:" + model.Retreat_OrderNumber + "来自退款已发货未出库\"," +
                               "\"warehouse_code\":\"" + model.Retreat_Warehouse + "\"," +
                          "\"detail_list\":[" + cpcode + "]" +
                        "}";
            string sign = gy.Sign(cmd);
            string comcode = "";
            comcode = "{" +
                     "\"appkey\":\"171736\"," +
                     "\"method\":\"gy.erp.stock.adjust.add\"," +
                     "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                        "\"sign\":\"" + sign + "\"," +
                          "\"order_type\":\"001\"," +
                          "\"note\":\"平台单号:" + model.Retreat_OrderNumber + "来自退款已发货未出库\"," +
                            "\"warehouse_code\":\"" + model.Retreat_Warehouse + "\"," +
                       "\"detail_list\":[" + cpcode + "]" +
                     "}";
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);

            return ret;
        }
        //审核操作
        public JsonResult Check(string UserName,int id, int status, string memo, string Actualjine, string RetreatPayment)
        {
          
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string Nickname = UserName;
            //花名
            string curName = MOD_User.Name;
            List<T_RetreatGroup> RetreatGroupList = db.Database.SqlQuery<T_RetreatGroup>("select  * from T_RetreatGroup where Crew like '%" + Nickname + "%'").ToList();
            string GroupName = "";
            for (int i = 0; i < RetreatGroupList.Count; i++)
            {
                if (i == 0)
                {
                    GroupName += "'" + RetreatGroupList[i].GroupName + "'";
                }
                else
                {
                    GroupName += "," + "'" + RetreatGroupList[i].GroupName + "'";
                }
            }

            string sql = "select * from T_RetreatAppRove where Oid='" + id + "' and ApproveTime is null ";
            if (GroupName != "" && GroupName != null)
            {
                sql += "  and (ApproveName='" + Nickname + "' or ApproveName  in (" + GroupName + ")) ";
            }
            else
            {
                sql += "    and ApproveName='" + Nickname + "'  ";
            }
            List<T_RetreatAppRove> AppRoveListModel = db.Database.SqlQuery<T_RetreatAppRove>(sql).ToList();
            if (AppRoveListModel.Count == 0)
            {
                return Json("该数据已审核，请勿重复审核", JsonRequestBehavior.AllowGet);
            }

            T_RetreatAppRove modelApprove = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveTime == null);

            string result = "";
            //if (modelApprove == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
            using (TransactionScope sc = new TransactionScope())
            {

                modelApprove.ApproveName = Nickname;
                modelApprove.Memo = memo;
                modelApprove.ApproveTime = DateTime.Now;
                modelApprove.Status = status;
                db.Entry<T_RetreatAppRove>(modelApprove).State = System.Data.EntityState.Modified;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    T_Retreat model = db.T_Retreat.Find(id);
                    T_RetreatAppRove newApprove = new T_RetreatAppRove();
                    if (model == null) { return Json("数据可能被删除", JsonRequestBehavior.AllowGet); }
                    if (status == 1)
                    {

                        if (model.Retreat_Reason == "已发货未出库" && modelApprove.ApproveDName == "审单")//调整单新增
                        {
                            foreach (var item in db.T_RetreatDetails.Where(s => s.Oid == model.ID).ToList())
                            {
                                Tz(model, item);
                            }
                        }
                        //同意
                        string type = modelApprove.Type;
                        int step = int.Parse(model.Step.ToString());
                        step++;
                        IQueryable<T_RetreatConfig> config = db.T_RetreatConfig.Where(a => a.Reason == type);
                        int stepLength = config.Count();//总共步骤
                        if (step < stepLength)
                        {


                            //不是最后一步，主表状态为0 =>审核中

                            model.Status = 0;
                            T_RetreatConfig stepMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                            string nextName = stepMod.Name;
                            //下一步审核人不是null  审核记录插入一条新纪录
                            newApprove.Memo = "";
                            newApprove.Oid = id;
                            newApprove.Status = -1;
                            newApprove.Type = type;
                            newApprove.ApproveDName = stepMod.Type;
                            if (nextName != null && nextName != "")
                            {
                                newApprove.ApproveName = nextName;
                            }
                            else
                            {
                                newApprove.ApproveName = stepMod.Type;
                            }
                            //下一级审核人
                            string shenheName = newApprove.ApproveDName;
                            if (shenheName == "财务")
                            {
                                T_RetreatAppRove appRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == id && a.ApproveDName == "财务" && a.Status == 1);

                                if (appRoveModel != null)
                                {
                                    // int stepAppRove = step + 1;
                                    step++;
                                    T_RetreatConfig stepAppRoveMod = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                    if (stepAppRoveMod.Name != null)
                                    {
                                        newApprove.ApproveName = stepAppRoveMod.Name;
                                    }
                                    else
                                    {
                                        newApprove.ApproveName = stepAppRoveMod.Type;
                                        newApprove.ApproveDName = stepAppRoveMod.Type;
                                    }
                                }
                            }
                            db.T_RetreatAppRove.Add(newApprove);
                            db.SaveChanges();
                            if (shenheName == "仓库")
                            {
                                string exNumber = model.Retreat_expressNumber;

                                //得到当前退款记录
                                List<T_Retreat> RetreatList = db.T_Retreat.Where(a => a.Retreat_expressNumber == exNumber && a.ID == id).ToList();


                                T_ReturnToStorage Tostorage = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.IsDelete == 0);

                                if (Tostorage != null)
                                {


                                    T_ReturnToStorage TostorageModel = db.T_ReturnToStorage.SingleOrDefault(a => a.Retreat_expressNumber == exNumber && a.isSorting == 1 && a.IsDelete == 0);
                                    if (TostorageModel != null)
                                    {
                                        List<T_RetreatDetails> RetreatDetailsList = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();



                                        for (int x = 0; x < RetreatDetailsList.Count; x++)
                                        {
                                            string itemCode = RetreatDetailsList[x].item_code;
                                            int TostorageID = Tostorage.ID;
                                            List<T_ReturnToStoragelet> ReturnToStorageletList = db.T_ReturnToStoragelet.Where(a => a.Pid == TostorageID && a.item_code == itemCode).ToList();
                                            if (ReturnToStorageletList.Count == 0)
                                            {
                                                List<T_ReceivedAfter> ReceivedAfterList = db.T_ReceivedAfter.Where(a => a.ProductCode == itemCode && a.CollectExpressNumber == exNumber).ToList();
                                                if (ReceivedAfterList.Count == 0)
                                                {
                                                    T_ReceivedAfter ReceivedAfterModel = new T_ReceivedAfter();

                                                    ReceivedAfterModel.Type = "退货退款";
                                                    ReceivedAfterModel.OrderNumber = RetreatList[0].Retreat_OrderNumber;
                                                    ReceivedAfterModel.ProductCode = RetreatDetailsList[x].item_code;
                                                    ReceivedAfterModel.ProductName = RetreatDetailsList[x].item_name;
                                                    ReceivedAfterModel.CollectExpressName = RetreatList[0].Retreat_expressName;
                                                    ReceivedAfterModel.CollectExpressNumber = exNumber;
                                                    ReceivedAfterModel.ShopName = RetreatList[0].Retreat_dianpName;
                                                    ReceivedAfterModel.CustomerCode = RetreatList[0].Retreat_wangwang;
                                                    ReceivedAfterModel.CustomerName = RetreatList[0].Retreat_CustomerName;
                                                    ReceivedAfterModel.ProductNumber = RetreatDetailsList[x].qty;
                                                    ReceivedAfterModel.IsHandle = 0;
                                                    db.T_ReceivedAfter.Add(ReceivedAfterModel);
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }


                                    //得到退款id
                                    int RetreatID = RetreatList[0].ID;
                                    //看该退款记录走到那里如果是仓库就审核并且下一步
                                    T_RetreatAppRove AppRoveModel = db.T_RetreatAppRove.SingleOrDefault(a => a.Oid == RetreatID && a.ApproveTime == null);
                                    if (AppRoveModel != null && AppRoveModel.ApproveName == "仓库")
                                    {

                                        AppRoveModel.ApproveTime = DateTime.Now;
                                        AppRoveModel.Status = 1;
                                        db.Entry<T_RetreatAppRove>(AppRoveModel).State = System.Data.EntityState.Modified;
                                        int w = db.SaveChanges();

                                        if (w > 0)
                                        {
                                            T_Retreat Rmodel = db.T_Retreat.Find(RetreatID);
                                            T_RetreatAppRove newApproves = new T_RetreatAppRove();

                                            string types = AppRoveModel.Type;
                                            //  int steps = int.Parse(Rmodel.Step.ToString());
                                            step++;
                                            IQueryable<T_RetreatConfig> configs = db.T_RetreatConfig.Where(a => a.Reason == types);
                                            int stepLengths = configs.Count();//总共步骤
                                            if (step < stepLengths)
                                            {
                                                //不是最后一步，主表状态为0 =>审核中
                                                Rmodel.Status = 0;
                                                T_RetreatConfig stepMods = db.T_RetreatConfig.SingleOrDefault(a => a.Step == step && a.Reason == type);
                                                string nextNames = stepMod.Name;
                                                //下一步审核人不是null  审核记录插入一条新纪录
                                                newApproves.Memo = "";
                                                newApproves.Oid = RetreatID;
                                                newApproves.Status = -1;
                                                newApproves.Type = types;
                                                newApproves.ApproveDName = stepMods.Type;
                                                if (nextNames != null && nextNames != "")
                                                {
                                                    newApproves.ApproveName = nextNames;
                                                }
                                                else
                                                {
                                                    newApproves.ApproveName = stepMods.Type;
                                                }
                                                string shenheNames = newApproves.ApproveDName;
                                                db.T_RetreatAppRove.Add(newApproves);
                                                db.SaveChanges();


                                            }
                                            else
                                            {
                                                //最后一步，主表状态改为 1 => 同意
                                                Rmodel.Status = 1;
                                            }

                                            Rmodel.Step = step;
                                            db.Entry<T_Retreat>(Rmodel).State = System.Data.EntityState.Modified;
                                            db.SaveChanges();


                                            //T_OrderList OrderModel = db.T_OrderList.Find(model.OrderId);
                                            //if (OrderModel.Status_Retreat != 2)
                                            //{
                                            //    OrderModel.Status_Retreat = 2;
                                            //    db.Entry<T_OrderList>(OrderModel).State = System.Data.EntityState.Modified;
                                            //    db.SaveChanges();
                                            //}
                                            List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();
                                            //  string code = OrderModel.code;
                                            //foreach (var item in details)
                                            //{
                                            //    string itemcode = item.item_code;
                                            //   List<T_OrderDetail> OrderDetailModel = db.T_OrderDetail.Where(a => a.oid == code && a.item_code == itemcode).ToList();
                                            //    int qty = 0;
                                            //    if (OrderDetailModel.Count>0)
                                            //    {
                                            //        if (OrderDetailModel[0].RetreatQty == null)
                                            //        {
                                            //            OrderDetailModel[0].RetreatQty = 0;
                                            //        }
                                            //        else
                                            //        {
                                            //            qty = int.Parse(OrderDetailModel[0].RetreatQty.ToString());
                                            //        }
                                            //        OrderDetailModel[0].RetreatQty = item.qty + qty;
                                            //        OrderDetailModel[0].RetreatStatus = 1;
                                            //        db.Entry<T_OrderDetail>(OrderDetailModel[0]).State = System.Data.EntityState.Modified;
                                            //        db.SaveChanges();
                                            //    }
                                            //}
                                        }
                                    }

                                    //if (model.isNoheadparts == "1")
                                    //{
                                    //    model.SortingName = Nickname;
                                    //    model.Status = status;
                                    //    newApprove.Status = 1;
                                    //    newApprove.ApproveTime = DateTime.Now;
                                    //}
                                    //else
                                    //{
                                    //    model.SortingName = Nickname;
                                    //}
                                }

                            }

                            //当前审核人
                            string isCwuName = modelApprove.ApproveDName;
                            if (isCwuName == "财务")
                            {
                                model.Retreat_Actualjine = decimal.Parse(Actualjine);
                                model.Retreat_PaymentAccounts = RetreatPayment;

                                if (model.Retreat_Reason == "报损退款")
                                {
                                    int RetreatID = model.ID;

                                    List<T_RetreatDetails> DetailsMode = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();
                                    string name = model.Retreat_ApplyName;
                                    string DepartmentID = DepartmentQuer(name);
                                    List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();
                                    T_LossReport LossReport = new T_LossReport();
                                    LossReport.PostUser = model.Retreat_ApplyName;
                                    LossReport.Department = DepartmentID;
                                    LossReport.Shop = model.Retreat_dianpName;
                                    LossReport.Code = CodeQuery();
                                    LossReport.Status = -1;
                                    LossReport.Total = 0;
                                    LossReport.Step = 0;
                                    LossReport.IsAllowdelete = 1;
                                    if (ZgModel.Count > 0)
                                    {
                                        LossReport.ApproveFirst = ZgModel[0].DepartmentId;
                                    }
                                    else
                                    {
                                        string ApproveFirst = DepartmentQuer("成风");

                                        LossReport.ApproveFirst = ApproveFirst;
                                    }
                                    LossReport.IsDelete = 0;
                                    LossReport.PostTime = DateTime.Now;
                                    LossReport.IsPzStatus = 0;
                                    db.T_LossReport.Add(LossReport);
                                    int s = db.SaveChanges();
                                    if (s > 0)
                                    {
                                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                                        Approvemodel.Status = -1;

                                        if (ZgModel.Count > 0)
                                        {
                                            Approvemodel.ApproveName = ZgModel[0].DepartmentId;
                                        }
                                        else
                                        {
                                            //  string ApproveFirst = DepartmentQuer("成风");

                                            Approvemodel.ApproveName = "汪紫炜";
                                        }
                                        Approvemodel.Memo = "";
                                        Approvemodel.Oid = LossReport.ID;
                                        db.T_LossReportApprove.Add(Approvemodel);
                                        db.SaveChanges();
                                        decimal price = 0;
                                        for (int z = 0; z < DetailsMode.Count; z++)
                                        {

                                            T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                                            LossReportDetailModel.Oid = LossReport.ID;

                                            LossReportDetailModel.ProductCode = DetailsMode[z].item_code;
                                            LossReportDetailModel.ProductName = DetailsMode[z].item_name;
                                            LossReportDetailModel.WangWang = model.Retreat_wangwang;
                                            LossReportDetailModel.OderNumber = model.Retreat_OrderNumber;
                                            LossReportDetailModel.Reason = "售后问题处理报损";
                                            LossReportDetailModel.Unit = "无";
                                            LossReportDetailModel.UnitPrice = QueryUnitPrice(DetailsMode[z].item_code);
                                            LossReportDetailModel.Qty = int.Parse(DetailsMode[z].qty.ToString());
                                            LossReportDetailModel.Amount = int.Parse(DetailsMode[z].qty.ToString()) * decimal.Parse(QueryUnitPrice(DetailsMode[z].item_code).ToString());
                                            price += LossReportDetailModel.Amount;
                                            db.T_LossReportDetail.Add(LossReportDetailModel);
                                            db.SaveChanges();
                                        }


                                        //总价
                                        T_LossReport modelLoss = db.T_LossReport.Find(model.ID);
                                        modelLoss.Total = price;
                                        db.Entry<T_LossReport>(modelLoss).State = System.Data.EntityState.Modified;
                                        db.SaveChanges();

                                        string LossReportApproveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                                        List<ModularQuery> LossReportApproveQuery = db.Database.SqlQuery<ModularQuery>(LossReportApproveSql).ToList();

                                        for (int e = 0; e < LossReportApproveQuery.Count; e++)
                                        {
                                            string PendingAuditName = LossReportApproveQuery[e].PendingAuditName;

                                            T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                                            if (NotauditedModel != null)
                                            {
                                                NotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                                            }
                                            else
                                            {
                                                T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                ModularNotauditedModel.ModularName = "报损";
                                                ModularNotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                ModularNotauditedModel.PendingAuditName = LossReportApproveQuery[e].PendingAuditName;
                                                ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                            }
                                            db.SaveChanges();
                                        }


                                    }
                                    //T_OrderList OrderModel = db.T_OrderList.Find(model.OrderId);
                                    //if (OrderModel.Status_Retreat != 2)
                                    //{
                                    //    OrderModel.Status_Retreat = 2;
                                    //    db.Entry<T_OrderList>(OrderModel).State = System.Data.EntityState.Modified;
                                    //    db.SaveChanges();
                                    //}
                                    List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();
                                    //string code = OrderModel.code;
                                    //foreach (var item in details)
                                    //{
                                    //    string itemcode = item.item_code;
                                    //    T_OrderDetail OrderDetailModel = db.T_OrderDetail.SingleOrDefault(a => a.oid == code && a.item_code == itemcode);
                                    //    int qty = 0;
                                    //    if (OrderDetailModel != null)
                                    //    {
                                    //        if (OrderDetailModel.RetreatQty == null)
                                    //        {
                                    //            OrderDetailModel.RetreatQty = 0;
                                    //        }
                                    //        else
                                    //        {
                                    //            qty = int.Parse(OrderDetailModel.RetreatQty.ToString());
                                    //        }
                                    //        OrderDetailModel.RetreatQty = item.qty + qty;
                                    //        OrderDetailModel.RetreatStatus = 1;
                                    //        db.Entry<T_OrderDetail>(OrderDetailModel).State = System.Data.EntityState.Modified;
                                    //        db.SaveChanges();
                                    //    }
                                    //}
                                }


                            }

                        }
                        else
                        {
                            string isCwuName = modelApprove.ApproveDName;
                            if (isCwuName == "财务")
                            {
                                model.Retreat_Actualjine = decimal.Parse(Actualjine);
                                model.Retreat_PaymentAccounts = RetreatPayment;
                                if (model.Retreat_Reason == "报损退款")
                                {
                                    int RetreatID = model.ID;

                                    List<T_RetreatDetails> DetailsMode = db.T_RetreatDetails.Where(a => a.Oid == RetreatID).ToList();
                                    string name = model.Retreat_ApplyName;
                                    string DepartmentID = DepartmentQuer(name);
                                    List<T_User> ZgModel = db.T_User.Where(a => a.DepartmentId == DepartmentID && a.IsManagers == "1").ToList();
                                    T_LossReport LossReport = new T_LossReport();
                                    LossReport.PostUser = model.Retreat_ApplyName;
                                    LossReport.Department = DepartmentID;
                                    LossReport.Shop = model.Retreat_dianpName;
                                    LossReport.Code = CodeQuery();
                                    LossReport.Status = -1;
                                    LossReport.Total = 0;
                                    LossReport.Step = 0;
                                    LossReport.IsAllowdelete = 1;
                                    if (ZgModel.Count > 0)
                                    {
                                        LossReport.ApproveFirst = ZgModel[0].DepartmentId;
                                    }
                                    else
                                    {
                                        string ApproveFirst = DepartmentQuer("成风");

                                        LossReport.ApproveFirst = ApproveFirst;
                                    }
                                    LossReport.IsDelete = 0;
                                    LossReport.PostTime = DateTime.Now;
                                    LossReport.IsPzStatus = 0;
                                    db.T_LossReport.Add(LossReport);
                                    int s = db.SaveChanges();
                                    if (s > 0)
                                    {
                                        T_LossReportApprove Approvemodel = new T_LossReportApprove();
                                        Approvemodel.Status = -1;
                                        if (ZgModel.Count > 0)
                                        {
                                            Approvemodel.ApproveName = ZgModel[0].Name;
                                        }
                                        else
                                        {
                                            //  string ApproveFirst = DepartmentQuer("成风");

                                            Approvemodel.ApproveName = "汪紫炜";
                                        }
                                        Approvemodel.Memo = "";
                                        Approvemodel.Oid = LossReport.ID;
                                        db.T_LossReportApprove.Add(Approvemodel);
                                        db.SaveChanges();
                                        decimal price = 0;
                                        for (int z = 0; z < DetailsMode.Count; z++)
                                        {

                                            T_LossReportDetail LossReportDetailModel = new T_LossReportDetail();
                                            LossReportDetailModel.Oid = LossReport.ID;

                                            LossReportDetailModel.ProductCode = DetailsMode[z].item_code;
                                            LossReportDetailModel.ProductName = DetailsMode[z].item_name;
                                            LossReportDetailModel.WangWang = model.Retreat_wangwang;
                                            LossReportDetailModel.OderNumber = model.Retreat_OrderNumber;
                                            LossReportDetailModel.Reason = "售后问题处理报损";
                                            LossReportDetailModel.Unit = "无";
                                            LossReportDetailModel.UnitPrice = QueryUnitPrice(DetailsMode[z].item_code);
                                            LossReportDetailModel.Qty = int.Parse(DetailsMode[z].qty.ToString());
                                            LossReportDetailModel.Amount = int.Parse(DetailsMode[z].qty.ToString()) * decimal.Parse(QueryUnitPrice(DetailsMode[z].item_code).ToString());
                                            price += LossReportDetailModel.Amount;
                                            db.T_LossReportDetail.Add(LossReportDetailModel);
                                            db.SaveChanges();
                                        }


                                        //总价
                                        T_LossReport modelLoss = db.T_LossReport.Find(LossReport.ID);
                                        modelLoss.Total = price;
                                        db.Entry<T_LossReport>(modelLoss).State = System.Data.EntityState.Modified;
                                        db.SaveChanges();

                                        string LossReportApproveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_LossReportApprove where  Oid in ( select ID from T_LossReport where Status!=3 and IsDelete=0 ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName ";
                                        List<ModularQuery> LossReportApproveQuery = db.Database.SqlQuery<ModularQuery>(LossReportApproveSql).ToList();

                                        for (int e = 0; e < LossReportApproveQuery.Count; e++)
                                        {
                                            string PendingAuditName = LossReportApproveQuery[e].PendingAuditName;

                                            T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "报损" && a.PendingAuditName == PendingAuditName);
                                            if (NotauditedModel != null)
                                            {
                                                NotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                                            }
                                            else
                                            {
                                                T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                                                ModularNotauditedModel.ModularName = "报损";
                                                ModularNotauditedModel.NotauditedNumber = LossReportApproveQuery[e].NotauditedNumber;
                                                ModularNotauditedModel.PendingAuditName = LossReportApproveQuery[e].PendingAuditName;
                                                ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                                                db.T_ModularNotaudited.Add(ModularNotauditedModel);
                                            }
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            //List<T_OrderList> OrderModel = db.T_OrderList.Where(a => a.ID == model.OrderId || a.platform_code == model.Retreat_OrderNumber).ToList();
                            //if (OrderModel[0].Status_Retreat != 2)
                            //{
                            //    OrderModel[0].Status_Retreat = 2;
                            //    db.Entry<T_OrderList>(OrderModel[0]).State = System.Data.EntityState.Modified;
                            //    db.SaveChanges();
                            //}
                            List<T_RetreatDetails> details = db.T_RetreatDetails.Where(a => a.Oid == id).ToList();
                            //string code = OrderModel[0].code;
                            //foreach (var item in details)
                            //{
                            //    string itemcode = item.item_code;
                            //   List<T_OrderDetail> OrderDetailModel = db.T_OrderDetail.Where(a => a.oid == code && a.item_code == itemcode).ToList();
                            //    int qty = 0;
                            //    if (OrderDetailModel.Count >0)
                            //    {
                            //        if (OrderDetailModel[0].RetreatQty == null)
                            //        {
                            //            OrderDetailModel[0].RetreatQty = 0;
                            //        }
                            //        else
                            //        {
                            //            qty = int.Parse(OrderDetailModel[0].RetreatQty.ToString());
                            //        }
                            //        OrderDetailModel[0].RetreatQty = item.qty + qty;
                            //        OrderDetailModel[0].RetreatStatus = 1;
                            //        db.Entry<T_OrderDetail>(OrderDetailModel[0]).State = System.Data.EntityState.Modified;
                            //        db.SaveChanges();
                            //    }
                            //}
                            model.SortingName = Nickname;
                            //最后一步，主表状态改为 1 => 同意
                            model.Status = status;
                        }
                        model.Step = step;
                        db.Entry<T_Retreat>(model).State = System.Data.EntityState.Modified;
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
                        db.Entry<T_Retreat>(model).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                        //审核流程结束 申请人编辑后插入下一条记录 
                        result = "保存成功";
                    }
                }
                else
                {
                    result = "保存失败";
                }

                List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "退货退款").ToList();
                if (ModularNotaudited.Count > 0)
                {
                    foreach (var item in ModularNotaudited)
                    {
                        db.T_ModularNotaudited.Remove(item);
                    }
                    db.SaveChanges();
                }

                //查询审核详情未审核数据并且根据名字分组得到名字和数量
                string RetreatAppRoveSql = "select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_RetreatAppRove where Oid in (select ID from T_Retreat where  Isdelete='0'  and (Status = -1 or Status = 0 or Status = 2) )  and  Status=-1 and ApproveTime is null GROUP BY ApproveName";
                List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                //循环未审核的数据条数
                for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                {
                    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    //查询该模块是否有该名字存在
                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && a.PendingAuditName == PendingAuditName);
                    //如果有就修改记录，没有就新增记录
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "退货退款";
                        ModularNotauditedModel.RejectNumber = 0;
                        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                        ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }
                //增加驳回数据
                string RejectNumberSql = "select Retreat_ApplyName as PendingAuditName,COUNT(*) as NotauditedNumber from T_Retreat  where Status='2' GROUP BY Retreat_ApplyName";
                List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

                for (int e = 0; e < RejectNumberQuery.Count; e++)
                {
                    // string PendingAuditName = RejectNumberQuery[e].PendingAuditName;
                    string Retreat_ApplyName = RejectNumberQuery[e].PendingAuditName;
                    string PendingAuditName = db.T_User.SingleOrDefault(a => a.Name.Equals(Retreat_ApplyName) || a.Nickname.Equals(Retreat_ApplyName)).Nickname;
                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "退货退款" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "退货退款";
                        ModularNotauditedModel.NotauditedNumber = 0;
                        ModularNotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                        //    string Retreat_ApplyName = RejectNumberQuery[e].PendingAuditName;
                        ModularNotauditedModel.PendingAuditName = db.T_User.SingleOrDefault(s => s.Name.Equals(Retreat_ApplyName) || s.Nickname.Equals(Retreat_ApplyName)).Nickname;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now;
                        ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }
                sc.Complete();
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
}
