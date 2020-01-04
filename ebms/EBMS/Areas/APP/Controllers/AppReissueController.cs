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
    public class AppReissueController : Controller
    {
        //补发货管理
        // GET: /APP/AppReissue/
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
        /// <summary>
        /// 提交管易
        /// </summary>
        /// <returns></returns>
        public string PostGy(T_Reissue model)
        {
            string shangp = "";
            List<T_ReissueDetail> commodel = db.T_ReissueDetail.Where(a => a.ReissueId == model.ID).ToList();
            for (int i = 0; i < commodel.Count; i++)
            {
                if (i == commodel.Count - 1)
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].ProductCode + "\"}";
                }
                else
                {
                    shangp += "{\"qty\":" + commodel[i].Num + ",\"price\":0,\"note\":\"\",\"refund\":0,\"item_code\":\"" + commodel[i].ProductCode + "\"},";
                }
            }


            string datetime = DateTime.Parse(model.SingleTime.ToString()).ToString("yyyy-MM-dd hh:mm:ss");
            string sellerremarks = "";
            if (!string.IsNullOrWhiteSpace(model.SalesRemark))
            {
                sellerremarks = Regex.Replace(model.SalesRemark.ToUpper().Replace((char)32, ' ').Replace((char)13, ' ').Replace((char)10, ' '), "[ \\[ \\] \\^ \t \\-×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；;\"‘’“”-]", "☆").Replace("☆", "").Replace(" ", "");
            }
            else
            {
                sellerremarks = "";
            }
            string BuyersRemarks = "";
            if (!string.IsNullOrWhiteSpace(model.BuyRemark))
            {
                BuyersRemarks = Regex.Replace(model.BuyRemark.ToUpper().Replace((char)32, ' ').Replace((char)13, ' ').Replace((char)10, ' '), "[ \\[ \\] \\^ \t \\-×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；;\"‘’“”-]", "☆").Replace("☆", "").Replace(" ", "");
            }
            else
            {
                BuyersRemarks = "";
            }

            string[] address = model.AddressMessage.Split('-');
            string receiver_province = "";
            string receiver_city = "";
            string receiver_district = "";
            if (address.Length >= 1)
            {
                receiver_province = address[0];
            }
            if (address.Length >= 2)
            {
                receiver_city = address[1];
            }
            if (address.Length >= 3)
            {
                receiver_district = address[2];
            }
            DateTime dtshottime = DateTime.Now;
            DateTime shoptime = dtshottime.AddDays(-3);

            List<T_Reissue> ReissOrederModelList = db.T_Reissue.Where(a => a.OrderCode == model.OrderCode && a.CreatDate >= shoptime).ToList();
            string sellerRemarksList = "";
            if (ReissOrederModelList.Count >= 2)
            {
                sellerRemarksList = "三天内多次补发换货," + sellerremarks + "制单人：" + model.PostUser;

            }
            else
            {
                sellerRemarksList = sellerremarks + "制单人：" + model.PostUser;
            }


            GY gy = new GY();
            string cmd = "";
            cmd = "{" +
                        "\"appkey\":\"171736\"," +
                        "\"method\":\"gy.erp.trade.add\"," +
                        "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                        "\"platform_code\":\"" + model.NewOrderCode + "\"," +
                        "\"order_type_code\":\"" + model.OrderType + "\"," +
                        "\"shop_code\":\"" + model.StoreCode + "\"," +
                        "\"express_code\":\"" + model.ExpressName + "\"," +
                            "\"receiver_province\":\"" + receiver_province + "\"," +
                                "\"receiver_city\":\"" + receiver_city + "\"," +
                                    "\"receiver_district\":\"" + receiver_district + "\"," +
                        "\"warehouse_code\":\"" + model.WarhouseName + "\"," +
                        "\"vip_code\":\"" + model.VipCode + "\"," +
                        "\"vip_name\":\"" + model.VipName + "\"," +
                        "\"receiver_name\":\"" + model.ReceivingName + "\"," +
                        "\"receiver_address\":\"" + model.Address + "\"," +
                        "\"receiver_zip\":\"" + model.PostalCode + "\"," +
                        "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                        "\"receiver_phone\":\"" + model.Phone + "\"," +
                        "\"deal_datetime\":\"" + datetime + "\"," +
                        "\"buyer_memo\":\"" + BuyersRemarks + "\"," +
                        "\"seller_memo\":\"" + sellerRemarksList + "\"," +
                           "\"business_man_code\":\"" + model.BusinessName + "\"," +
                          "\"details\":[" + shangp + "]" +
                        "}";
            string sign = gy.Sign(cmd);
            string comcode = "{" +
                    "\"appkey\":\"171736\"," +
                    "\"method\":\"gy.erp.trade.add\"," +
                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                     "\"platform_code\":\"" + model.NewOrderCode + "\"," +
                    "\"order_type_code\":\"" + model.OrderType + "\"," +
                    "\"shop_code\":\"" + model.StoreCode + "\"," +
                    "\"express_code\":\"" + model.ExpressName + "\"," +
                    "\"receiver_province\":\"" + receiver_province + "\"," +
                    "\"receiver_city\":\"" + receiver_city + "\"," +
                    "\"receiver_district\":\"" + receiver_district + "\"," +
                    "\"warehouse_code\":\"" + model.WarhouseName + "\"," +
                    "\"vip_code\":\"" + model.VipCode + "\"," +
                    "\"vip_name\":\"" + model.VipName + "\"," +
                    "\"receiver_name\":\"" + model.ReceivingName + "\"," +
                    "\"receiver_address\":\"" + model.Address + "\"," +
                    "\"receiver_zip\":\"" + model.PostalCode + "\"," +
                    "\"receiver_mobile\":\"" + model.TelPhone + "\"," +
                    "\"receiver_phone\":\"" + model.Phone + "\"," +
                    "\"deal_datetime\":\"" + datetime + "\"," +
                    "\"buyer_memo\":\"" + BuyersRemarks + "\"," +
                    "\"sign\":\"" + sign + "\"," +
                    "\"seller_memo\":\"" + sellerRemarksList + "\"," +
                //   "\"receiver_province\":\"" + listmodel.address + "\"," +
                 "\"business_man_code\":\"" + model.BusinessName + "\"," +
                    "\"details\":[" + shangp + "]" +
                    "}";
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string sd = jsonData[0].ToString();
            return sd;
        }
        //列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = 9999, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_ReissueApprove> ApproveMod = new List<T_ReissueApprove>();
            string sql = "select * from T_ReissueApprove where (ApproveUser='" + CurUser + "' or ApproveName='" + CurUser + "' or ApproveName in (select GroupName  from T_ReissueGroup where GroupUser in ('" + CurUser + "') ))";
            if (Status == 9999)
            {
                // ApproveMod = db.T_ExchangeCenterApprove.Where(a => a.ApproveName == CurUser||a.ApproveUser==CurUser).ToList();
                ApproveMod = db.Database.SqlQuery<T_ReissueApprove>(sql).ToList();
            }
            else
            {
                // ApproveMod = db.T_ExchangeCenterApprove.Where(a => (a.ApproveName == CurUser || a.ApproveUser == CurUser) && a.ApproveStatus == Status).ToList();
                sql += " and ApproveStatus='" + Status + "'";
                ApproveMod = db.Database.SqlQuery<T_ReissueApprove>(sql).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Pid.ToString());
            }
            IQueryable<T_Reissue> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_Reissue
                            where Arry.Contains(r.ID) && r.IsDelete == 0
                            select r;
            }
            else
            {
                queryData = from r in db.T_Reissue
                            where r.IsDelete == 0 && r.PostUser == CurUser
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
                string str = item.StoreName;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 22)
                {
                    str = str.Substring(0, 22) + "...";
                }
                i.subTitle = "店铺名称：" + str;
                i.title = "申请人：" + item.PostUser;

                i.remark = int.Parse(item.Status.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string OrderCode { get; set; }
            public string NewOrderCode { get; set; }
            public string VipName { get; set; }
            public string StoreCode { get; set; }
            public string StoreName { get; set; }
            public string WarhouseName { get; set; }
            public string ExpressName { get; set; }
            public string OrderType { get; set; }
            public string SingleTime { get; set; }
            public string ReceivingName { get; set; }
            public string PostalCode { get; set; }
            public string Phone { get; set; }
            public string TelPhone { get; set; }
            public string VipCode { get; set; }
            public string Address { get; set; }
            public string SalesRemark { get; set; }
            public string BuyRemark { get; set; }
            public int Step { get; set; }
            public int Status { get; set; }
            public string BusinessName { get; set; }
            public string AddressMessage { get; set; }
            public System.DateTime CreatDate { get; set; }
            public string PostUser { get; set; }
            public string SystemRemark { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public string DraftName { get; set; }
            public decimal Cost { get; set; }
            public string ReissueReson { get; set; }
        }
        public partial class Modular
        {
            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        //详情页面数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_Reissue mod = db.T_Reissue.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.OrderCode = mod.OrderCode;
            list.NewOrderCode = mod.NewOrderCode;
            list.VipName = mod.VipName;
            list.StoreCode = mod.StoreCode;
            list.StoreName = mod.StoreName;
            list.WarhouseName = mod.WarhouseName;
            list.ExpressName = mod.ExpressName;
            list.OrderType = mod.OrderType;
            list.SingleTime = mod.SingleTime;
            list.ReceivingName = mod.ReceivingName;
            list.PostalCode = mod.PostalCode;
            list.Phone = mod.Phone;
            list.TelPhone = mod.TelPhone;
            list.VipCode = mod.VipCode;
            list.Address = mod.Address;
            list.SalesRemark = mod.SalesRemark;
            list.BuyRemark = mod.BuyRemark;
            list.Status = int.Parse(mod.Status.ToString());
            list.BusinessName = mod.BusinessName;
            list.AddressMessage = mod.AddressMessage;
            list.CreatDate = DateTime.Parse(mod.CreatDate.ToString());
            list.PostUser = mod.PostUser;
            list.SystemRemark = mod.SystemRemark;
            list.DraftName = mod.DraftName;
            list.Cost =decimal.Parse(mod.Cost.ToString());
            list.ReissueReson = mod.ReissueReson;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            //详情
            IQueryable<T_ReissueDetail> mod_Detail = db.T_ReissueDetail.Where(a => a.ReissueId == ID);
            string modDetail = JsonConvert.SerializeObject(mod_Detail, Lib.Comm.setTimeFormat());
            //审核记录
            IQueryable<T_ReissueApprove> mod_Approve = db.T_ReissueApprove.Where(a => a.Pid == ID);
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
            string sql = "select * from T_ReissueApprove where  Pid='" + ID + "' and ApproveTime is null  and   (ApproveUser='" + UserName + "' or ApproveName='" + UserName + "' or ApproveName in (select GroupName  from T_ReissueGroup where GroupUser in ('" + UserName + "')) )";
            List<T_ReissueApprove> MyApprove = db.Database.SqlQuery<T_ReissueApprove>(sql).ToList();
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
            T_ReissueApprove Approve = db.T_ReissueApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.Pid == ID);
            if (Approve == null)
            {
                Approve = db.T_ReissueApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.Pid == ID);
            }

            result += "{\"Main\":[" + modJson + "],\"Detail\":" + modDetail + ",\"Approve\":" + approve + ",\"Step\":" + nextMan + ",\"approveId\":" + Approve.ID + ",\"myCheck\":" + myCheck + ",\"CheckList\":" + CheckManJson + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        ///  审核
        /// </summary>
        /// <param name="approveID"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        //[HttpPost]
        //public JsonResult Check(string UserName, int approveID, int status, string memo)
        //{
        //    using (TransactionScope sc = new TransactionScope())
        //    {
        //        try
        //        {
        //            int TotalStep = db.T_ReissueConfig.ToList().Count;
        //            T_ReissueApprove approve = db.T_ReissueApprove.Find(approveID);
        //            string name = approve.ApproveName;
        //            T_Reissue model = db.T_Reissue.Find(approve.Pid);
        //            approve.ApproveName = UserName;
        //            approve.ApproveStatus = status;
        //            approve.ApproveTime = DateTime.Now;
        //            approve.Memo = memo;
        //            model.Status = status;
        //            db.SaveChanges();
        //            if (status == 2)//不同意
        //            {
        //                model.Step = model.Step + 1;
        //                db.SaveChanges();
        //            }
        //            else//同意
        //            {
        //                int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
        //                int LastStep = db.T_ReissueConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
        //                if (LastStep > model.Step)//判断是否存在下一级
        //                {
        //                    //获得下一级审核部门
        //                    string nextapproveType = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
        //                    T_ReissueApprove newApprove = new T_ReissueApprove();
        //                    newApprove.ApproveStatus = -1;
        //                    newApprove.ApproveName = nextapproveType;
        //                    newApprove.ApproveTime = null;
        //                    newApprove.Pid = approve.Pid;
        //                    db.T_ReissueApprove.Add(newApprove);
        //                    db.SaveChanges();
        //                    model.Status = 0;
        //                    model.Step = model.Step + 1;
        //                    db.SaveChanges();
        //                }
        //                if (name.Equals("售后主管"))//售后主管审核后直接加入补发货
        //                {
        //                    T_OrderList order = db.T_OrderList.SingleOrDefault(s => s.platform_code.Equals(model.OrderCode));

        //                    List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
        //                    //判断是否为第一次补发
        //                    if (reissue.Count() == 1)
        //                    {
        //                        order.ReissueStatus = 2;
        //                    }
        //                    List<T_ReissueDetail> reiDetails = db.T_ReissueDetail.Where(s => s.ReissueId == model.ID).ToList();
        //                    foreach (var item in reiDetails)
        //                    {
        //                        T_OrderDetail Orderdetail = db.T_OrderDetail.SingleOrDefault(s => s.oid.Equals(order.code) && s.item_code.Equals(item.ProductCode));
        //                        if (Orderdetail != null)
        //                        {
        //                            Orderdetail.ReissueStatus = 1;
        //                            Orderdetail.ReissueQty += item.Num;
        //                        }
        //                    }
        //                    db.SaveChanges();
        //                    #region 加入快递赔付

        //                    if (model.ReissueReson.Equals("快递破损"))
        //                    {

        //                        T_ExpressIndemnity Inde = new T_ExpressIndemnity
        //                        {
        //                            PostUserName = model.PostUser,
        //                            Date = DateTime.Now,
        //                            OrderNum = model.OrderCode,
        //                            wangwang = model.VipCode,
        //                            ShopName = model.StoreName,
        //                            RetreatExpressNum = order.mail_no,
        //                            State = "0",
        //                            OrderMoney = Convert.ToDouble(order.amount),
        //                            Type = "破损",
        //                            Second = "0",
        //                            CurrentApproveName = "快递组",
        //                            IsDelete = 0,
        //                            ExpressName = order.express_name,
        //                            IndemnityMoney = 0
        //                        };
        //                        db.T_ExpressIndemnity.Add(Inde);
        //                        db.SaveChanges();
        //                        List<T_ExchangePic> picList = db.T_ExchangePic.Where(s => s.ExchangeId == model.ID).ToList();
        //                        foreach (var item in picList)
        //                        {
        //                            T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
        //                            {
        //                                EID = Inde.ID,
        //                                PicURL = item.Url
        //                            };
        //                            db.T_ExpressIndemnityPic.Add(expressPic);
        //                        }
        //                        db.SaveChanges();
        //                    }
        //                    else if (model.ReissueReson.Equals("丢件"))
        //                    {

        //                        T_ExpressIndemnity Inde = new T_ExpressIndemnity
        //                        {
        //                            PostUserName = model.PostUser,
        //                            Date = DateTime.Now,
        //                            OrderNum = model.OrderCode,
        //                            wangwang = model.VipCode,
        //                            ShopName = model.StoreName,
        //                            RetreatExpressNum = order.mail_no,
        //                            State = "0",
        //                            OrderMoney = Convert.ToDouble(order.amount),
        //                            Type = "丢件",
        //                            Second = "0",
        //                            CurrentApproveName = "快递组",
        //                            IsDelete = 0,
        //                            ExpressName = order.express_name,
        //                            IndemnityMoney = 0
        //                        };
        //                        db.T_ExpressIndemnity.Add(Inde);
        //                        db.SaveChanges();
        //                    }

        //                    #endregion

        //                    if (PostGy(model) != "True")
        //                        return Json(new { State = "Faile", Message = "上传管易错误,请联系管理员" }, JsonRequestBehavior.AllowGet);
        //                }
        //            }

        //            db.SaveChanges();
        //            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "补发货未审核").ToList();
        //            if (ModularNotaudited.Count > 0)
        //            {
        //                foreach (var item in ModularNotaudited)
        //                {
        //                    db.T_ModularNotaudited.Remove(item);
        //                }
        //                db.SaveChanges();
        //            }
        //            string RetreatAppRoveSql = "select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ReissueApprove where  Pid in ( select ID from T_Reissue where IsDelete=0 ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser  ";
        //            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
        //            string Nickname = UserName;
        //            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
        //            {
        //                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

        //                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "补发货未审核" && a.PendingAuditName == PendingAuditName);
        //                if (NotauditedModel != null)
        //                {
        //                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
        //                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

        //                }
        //                else
        //                {
        //                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
        //                    ModularNotauditedModel.ModularName = "补发货未审核";
        //                    ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
        //                    ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
        //                    ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
        //                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
        //                }
        //                db.SaveChanges();
        //            }
        //            sc.Complete();
        //            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        [HttpPost]
        public JsonResult Check(string UserName,int approveID, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    int TotalStep = db.T_ReissueConfig.ToList().Count;
                    T_ReissueApprove approve = db.T_ReissueApprove.Find(approveID);
                    string name = approve.ApproveName;
                    T_Reissue model = db.T_Reissue.Find(approve.Pid);
                    approve.ApproveName = UserName;
                    approve.ApproveStatus = status;
                    approve.ApproveTime = DateTime.Now;
                    approve.Memo = memo;
                    model.Status = status;
                    db.SaveChanges();
                    if (status == 2)//不同意
                    {
                        model.Step = model.Step + 1;
                        db.SaveChanges();
                    }
                    else//同意
                    {
                        int type = db.T_ReissueReson.SingleOrDefault(s => s.Reson.Equals(model.ReissueReson)).Type;
                        int LastStep = db.T_ReissueConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
                        if (LastStep > model.Step)//判断是否存在下一级
                        {
                            //获得下一级审核部门
                            string nextapproveType = db.T_ReissueConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
                            T_ReissueApprove newApprove = new T_ReissueApprove();
                            newApprove.ApproveStatus = -1;
                            newApprove.ApproveName = nextapproveType;
                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_ReissueApprove.Add(newApprove);
                            db.SaveChanges();
                            model.Status = 0;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }
                        if (name.Equals("售后主管") || name.Equals("呼吸机主管"))//售后主管审核后直接加入补发货
                        {
                            //T_OrderList order = db.T_OrderList.Find(model.OrderId);

                            //List<T_Reissue> reissue = db.T_Reissue.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();
                            ////判断是否为第一次补发
                            //if (reissue.Count() == 1)
                            //{
                            //    order.ReissueStatus = 2;
                            //}
                            //List<T_ReissueDetail> reiDetails = db.T_ReissueDetail.Where(s => s.ReissueId == model.ID).ToList();
                            //foreach (var item in reiDetails)
                            //{
                            //    T_OrderDetail Orderdetail = db.T_OrderDetail.FirstOrDefault(s => s.oid.Equals(order.code) && s.item_code.Equals(item.ProductCode));
                            //    if (Orderdetail != null)
                            //    {
                            //        Orderdetail.ReissueStatus = 1;
                            //        Orderdetail.ReissueQty += item.Num;
                            //    }
                            //}
                            db.SaveChanges();
                            #region 加入快递赔付

                            if (model.ReissueReson.Equals("快递破损"))
                            {
                                GY gy = new GY();
                                string cmd = "";

                                cmd = "{" +
                                      "\"appkey\":\"171736\"," +
                                      "\"method\":\"gy.erp.trade.get\"," +
                                      "\"page_no\":1," +
                                      "\"page_size\":10," +
                                      "\"platform_code\":\"" + model.OrderCode + "\"," +
                                      "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                                      "}";

                                string sign = gy.Sign(cmd);
                                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                                string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                                JsonData jsonData = null;
                                jsonData = JsonMapper.ToObject(ret);

                                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                                {
                                    cmd = "{" +
                                    "\"appkey\":\"171736\"," +
                                    "\"method\":\"gy.erp.trade.history.get\"," +
                                    "\"page_no\":1," +
                                    "\"page_size\":10," +
                                    "\"platform_code\":\"" + model.OrderCode + "\"," +
                                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                                    "}";
                                    sign = gy.Sign(cmd);
                                    cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                                    ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                                    jsonData = null;
                                    jsonData = JsonMapper.ToObject(ret);
                                    if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                                    {
                                        return Json(new { State = "Faile", Message = "订单号不存在" });
                                    }
                                }
                                JsonData jsonOrders = jsonData["orders"][0];
                                var deliver = jsonOrders["deliverys"][0];
                                //快递单号
                                string mail_no = isNULL(deliver["mail_no"]).ToString();
                                //订单金额
                                string amount = isNULL(jsonOrders["amount"]).ToString();
                                //快递名称
                                string express_name = isNULL(jsonOrders["express_name"]).ToString();
                                T_ExpressIndemnity Inde = new T_ExpressIndemnity
                                {
                                    PostUserName = model.PostUser,
                                    Date = DateTime.Now,
                                    OrderNum = model.OrderCode,
                                    wangwang = model.VipCode,
                                    ShopName = model.StoreName,
                                    RetreatExpressNum = mail_no,
                                    State = "0",
                                    OrderMoney = Convert.ToDouble(amount),
                                    Type = "破损",
                                    Second = "0",
                                    CurrentApproveName = "快递组",
                                    IsDelete = 0,
                                    ExpressName = express_name,
                                    IndemnityMoney = 0
                                };
                                db.T_ExpressIndemnity.Add(Inde);
                                db.SaveChanges();
                                List<T_ReissuePic> picList = db.T_ReissuePic.Where(s => s.ReissueId == model.ID).ToList();
                                foreach (var item in picList)
                                {
                                    T_ExpressIndemnityPic expressPic = new T_ExpressIndemnityPic
                                    {
                                        EID = Inde.ID,
                                        PicURL = item.Url
                                    };
                                    db.T_ExpressIndemnityPic.Add(expressPic);
                                }
                                db.SaveChanges();
                            }
                            else if (model.ReissueReson.Equals("丢件"))
                            {
                                GY gy = new GY();
                                string cmd = "";

                                cmd = "{" +
                                      "\"appkey\":\"171736\"," +
                                      "\"method\":\"gy.erp.trade.get\"," +
                                      "\"page_no\":1," +
                                      "\"page_size\":10," +
                                      "\"platform_code\":\"" + model.OrderCode + "\"," +
                                      "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                                      "}";

                                string sign = gy.Sign(cmd);
                                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                                string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                                JsonData jsonData = null;
                                jsonData = JsonMapper.ToObject(ret);

                                if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                                {
                                    cmd = "{" +
                                    "\"appkey\":\"171736\"," +
                                    "\"method\":\"gy.erp.trade.history.get\"," +
                                    "\"page_no\":1," +
                                    "\"page_size\":10," +
                                    "\"platform_code\":\"" + model.OrderCode + "\"," +
                                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                                    "}";
                                    sign = gy.Sign(cmd);
                                    cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                                    ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                                    jsonData = null;
                                    jsonData = JsonMapper.ToObject(ret);
                                    if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
                                    {
                                        return Json(new { State = "Faile", Message = "订单号不存在" });
                                    }
                                }
                                JsonData jsonOrders = jsonData["orders"][0];
                                var deliver = jsonOrders["deliverys"][0];
                                //快递单号
                                string mail_no = isNULL(deliver["mail_no"]).ToString();
                                //订单金额
                                string amount = isNULL(jsonOrders["amount"]).ToString();
                                //快递名称
                                string express_name = isNULL(jsonOrders["express_name"]).ToString();
                                T_ExpressIndemnity Inde = new T_ExpressIndemnity
                                {
                                    PostUserName = model.PostUser,
                                    Date = DateTime.Now,
                                    OrderNum = model.OrderCode,
                                    wangwang = model.VipCode,
                                    ShopName = model.StoreName,
                                    RetreatExpressNum = mail_no,
                                    State = "0",
                                    OrderMoney = Convert.ToDouble(amount),
                                    Type = "丢件",
                                    Second = "0",
                                    CurrentApproveName = "快递组",
                                    IsDelete = 0,
                                    ExpressName = express_name,
                                    IndemnityMoney = 0
                                };
                                db.T_ExpressIndemnity.Add(Inde);
                                db.SaveChanges();
                            }

                            #endregion

                            if (PostGy(model) != "True")
                                return Json(new { State = "Faile", Message = "上传管易错误,请联系管理员" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    db.SaveChanges();
                    List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "补发货未审核").ToList();
                    if (ModularNotaudited.Count > 0)
                    {
                        foreach (var item in ModularNotaudited)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
                        db.SaveChanges();
                    }
                    string RetreatAppRoveSql = "select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ReissueApprove where  Pid in ( select ID from T_Reissue where IsDelete=0 ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser  ";
                    List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                    {
                        string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                        T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "补发货未审核" && a.PendingAuditName == PendingAuditName);
                        if (NotauditedModel != null)
                        {
                            NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                            db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                        }
                        else
                        {
                            T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                            ModularNotauditedModel.ModularName = "补发货未审核";
                            ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                            ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                            ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                            db.T_ModularNotaudited.Add(ModularNotauditedModel);
                        }
                        db.SaveChanges();
                    }
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
