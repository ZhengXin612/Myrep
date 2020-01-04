using EBMS.App_Code;
using EBMS.Models;
using LitJson;
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
    public class AppExchangeCenterController : Controller
    {
        //换货控制器
        // GET: /APP/AppExchangeCenter/
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
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        class resultItem
        {
            public string title { get; set; }//申请人
            public int uid { get; set; }//id
            public string subTitle { get; set; }//理由
            public int remark { get; set; }//状态
        }
        //列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = 9999, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_ExchangeCenterApprove> ApproveMod = new List<T_ExchangeCenterApprove>();
            string sql = "select * from T_ExchangeCenterApprove where (ApproveUser='" + CurUser + "' or ApproveName='" + CurUser + "' or ApproveName in (select GroupName  from T_ExchangeGroup where GroupUser in ('" + CurUser + "') ))";
            if (Status == 9999)
            {
               // ApproveMod = db.T_ExchangeCenterApprove.Where(a => a.ApproveName == CurUser||a.ApproveUser==CurUser).ToList();
                ApproveMod = db.Database.SqlQuery<T_ExchangeCenterApprove>(sql).ToList();
            }
            else
            {
              // ApproveMod = db.T_ExchangeCenterApprove.Where(a => (a.ApproveName == CurUser || a.ApproveUser == CurUser) && a.ApproveStatus == Status).ToList();
                sql += " and ApproveStatus='" + Status + "'";
                ApproveMod = db.Database.SqlQuery<T_ExchangeCenterApprove>(sql).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].Pid.ToString());
            }
            IQueryable<T_ExchangeCenter> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_ExchangeCenter
                            where Arry.Contains(r.ID) && r.IsDelete == 0
                            select r;
            }
            else
            {
                queryData = from r in db.T_ExchangeCenter
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
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string StoreCode { get; set; }
            public string StoreName { get; set; }
            public string VipName { get; set; }
            public string VipCode { get; set; }
            public string OrderCode { get; set; }
            public string NewOrderCode { get; set; }
            public string SingleTime { get; set; }
            public string NeedOrderType { get; set; }
            public string ReturnExpressName { get; set; }
            public string ReturnExpressCode { get; set; }
            public string ReturnWarhouse { get; set; }
            public string ExchangeReson { get; set; }
            public string ReceivingName { get; set; }
            public string ReceivingPhone { get; set; }
            public string ReceivingTelPhone { get; set; }
            public string ReceivingAddress { get; set; }
            public string PostUser { get; set; }
            public System.DateTime CreateDate { get; set; }
            public int Status { get; set; }
            public int Step { get; set; }
            public string NeedWarhouse { get; set; }
            public string NeedExpress { get; set; }
            public string NeedPostalCode { get; set; }
            public string SalesRemark { get; set; }
            public string BuyRemark { get; set; }
            public string AddressMessage { get; set; }
            public Nullable<int> WarhouseStatus { get; set; }
            public string SystemRemark { get; set; }
            public Nullable<int> IsDelete { get; set; }
            public string Pic { get; set; }
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
            T_ExchangeCenter mod = db.T_ExchangeCenter.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.StoreCode = mod.StoreCode;
            list.StoreName = mod.StoreName;
            list.VipName = mod.VipName;
            list.VipCode = mod.VipCode;
            list.PostUser = mod.PostUser;
            list.OrderCode = mod.OrderCode;
            list.NewOrderCode = mod.NewOrderCode;
            list.SingleTime = mod.SingleTime;
            list.NeedOrderType = mod.NeedOrderType;
            list.ReturnExpressName = mod.ReturnExpressName;
            list.ReturnExpressCode = mod.ReturnExpressCode;
            list.ReturnWarhouse = mod.ReturnWarhouse;
            list.Status = mod.Status;
            list.ExchangeReson = mod.ExchangeReson;
            list.ReceivingName = mod.ReceivingName;
            list.ReceivingPhone = mod.ReceivingPhone;
            list.ReceivingTelPhone = mod.ReceivingTelPhone;
            list.ReceivingAddress = mod.ReceivingAddress;
            list.CreateDate = mod.CreateDate;
            list.NeedWarhouse = mod.NeedWarhouse;
            list.NeedExpress = mod.NeedExpress;
            list.NeedPostalCode = mod.NeedPostalCode;
            list.SalesRemark = mod.SalesRemark;
            list.BuyRemark = mod.BuyRemark;
            list.AddressMessage = mod.AddressMessage;
            list.WarhouseStatus = mod.WarhouseStatus;        
            list.SystemRemark = mod.SystemRemark;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());
            //详情
            IQueryable<T_ExchangeDetail> mod_Detail = db.T_ExchangeDetail.Where(a => a.ExchangeCenterId == ID);
            string modDetail = JsonConvert.SerializeObject(mod_Detail, Lib.Comm.setTimeFormat());
            //审核记录
            IQueryable<T_ExchangeCenterApprove> mod_Approve = db.T_ExchangeCenterApprove.Where(a => a.Pid == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_ExchangeCenter lossreport = db.T_ExchangeCenter.Find(ID);
            //用于判断是不是 财务主管 
            int nextMan = 0;
            int Step = db.T_ExchangeCenterApprove.ToList().Count;
            if (mod.Step != Step - 1)
            {
                nextMan = 1;
            }
            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            //T_ExchangeCenterApprove MyApprove = db.T_ExchangeCenterApprove.FirstOrDefault(a => a.Pid == ID && (a.ApproveName == UserName || a.ApproveUser == UserName) && a.ApproveTime == null);
            string sql = "select * from T_ExchangeCenterApprove where  Pid='" + ID + "' and ApproveTime is null  and   (ApproveUser='" + UserName + "' or ApproveName='" + UserName + "' or ApproveName in (select GroupName  from T_ExchangeGroup where GroupUser in ('" + UserName + "')) )";
            List<T_ExchangeCenterApprove> MyApprove = db.Database.SqlQuery<T_ExchangeCenterApprove>(sql).ToList();
            if (MyApprove.Count>0 )
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
            T_ExchangeCenterApprove Approve = db.T_ExchangeCenterApprove.FirstOrDefault(s => !s.ApproveTime.HasValue && s.Pid == ID);
            if (Approve == null)
            {
                Approve = db.T_ExchangeCenterApprove.FirstOrDefault(s => s.ApproveTime.HasValue && s.Pid == ID);
            }

            result += "{\"Main\":[" + modJson + "],\"Detail\":" + modDetail + ",\"Approve\":" + approve + ",\"Step\":" + nextMan + ",\"approveId\":" + Approve.ID + ",\"myCheck\":" + myCheck + ",\"CheckList\":" + CheckManJson + "}";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="memo"></param>
        /// <param name="nextapprove"></param>
        /// <param name="BorrowerFrom"></param>
        /// string UserName
        /// <returns></returns>
        [HttpPost]
        public JsonResult Check(string UserName,int approveID, int status, string memo)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    int TotalStep = db.T_ExchangeCenterConfig.ToList().Count;
                    T_ExchangeCenterApprove approve = db.T_ExchangeCenterApprove.Find(approveID);
                    string name = approve.ApproveName;
                    T_ExchangeCenter model = db.T_ExchangeCenter.Find(approve.Pid);
                    approve.ApproveName = UserName;
                    approve.ApproveStatus = status;
                    approve.ApproveTime = DateTime.Now;
                    approve.Memo = memo;
                    db.SaveChanges();
                    if (status == 2)//不同意
                    {
                        model.Status = status;
                        model.Step = model.Step + 1;
                        db.SaveChanges();
                    }
                    else//同意
                    {
                        int type = db.T_ExchangeReson.SingleOrDefault(s => s.Reson.Equals(model.ExchangeReson)).Type;
                        int LastStep = db.T_ExchangeCenterConfig.OrderByDescending(s => s.Step).FirstOrDefault(s => s.Reson == type).Step;
                        if (LastStep > model.Step)//判断是否存在下一级
                        {
                            //获得下一级审核部门
                            string nextapproveType = db.T_ExchangeCenterConfig.OrderBy(s => s.Step).FirstOrDefault(s => s.Reson == type && s.Step > model.Step).ApproveType;
                            T_ExchangeCenterApprove newApprove = new T_ExchangeCenterApprove();
                            newApprove.ApproveStatus = -1;
                            newApprove.ApproveName = nextapproveType;
                            newApprove.ApproveTime = null;
                            newApprove.Pid = approve.Pid;
                            db.T_ExchangeCenterApprove.Add(newApprove);
                            db.SaveChanges();
                            model.Status = 0;
                            model.Step = model.Step + 1;
                            db.SaveChanges();
                        }
                        if (name.Equals("售后主管") || name.Equals("呼吸机主管"))//售后主管审核后直接加入补发货
                        {
                            //T_OrderList order = db.T_OrderList.Find(model.OrderId);
                            List<T_ExchangeDetail> exchangedetail = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == model.ID).ToList();

                            #region 加入快递赔付

                            if (model.ExchangeReson.Equals("快递破损"))
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
                                List<T_ExchangePic> picList = db.T_ExchangePic.Where(s => s.ExchangeId == model.ID).ToList();
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

                            #endregion

                            //#region 新增订单数据

                            //string newOrderCode = "";
                            //string newCode = "";
                            //string usedOrderCode = model.OrderCode;
                            //string userCode = order.code;
                            //List<T_OrderList> HEmodel = db.T_OrderList.Where(a => a.platform_code.Contains(usedOrderCode)).ToList();
                            //List<T_OrderList> orderList = db.T_OrderList.Where(a => a.code.Contains(userCode)).ToList();
                            //if (HEmodel.Count > 1)
                            //{
                            //    string HEmodelCount = HEmodel.Count.ToString();
                            //    newOrderCode = usedOrderCode + HEmodelCount;
                            //}
                            //else
                            //    newOrderCode = usedOrderCode + "1";
                            //if (orderList.Count > 1)
                            //{
                            //    string listCount = orderList.Count.ToString();
                            //    newCode = userCode + listCount;
                            //}
                            //else
                            //    newCode = userCode + "1";
                            //model.NewOrderCode = newOrderCode;
                            //db.SaveChanges();
                            //T_OrderList newOrder = new T_OrderList
                            //{
                            //    code = newCode,
                            //    order_type_name = "换货订单",
                            //    platform_code = newOrderCode,
                            //    createtime = DateTime.Now,
                            //    dealtime = model.SingleTime.ToString(),
                            //    cod = order.cod,
                            //    approve = order.approve,
                            //    delivery_state = order.delivery_state,
                            //    warehouse_code = model.ReturnWarhouse,
                            //    warehouse_name = db.T_Warehouses.SingleOrDefault(s => s.code.Equals(model.ReturnWarhouse)).name,
                            //    shop_code = model.StoreCode,
                            //    shop_name = model.StoreName,
                            //    express_code = model.ReturnExpressName,
                            //    express_name = db.T_Express.SingleOrDefault(s => s.Code.Equals(model.ReturnExpressName)).Name,
                            //    buyer_memo = model.BuyRemark,
                            //    seller_memo = model.SalesRemark,
                            //    vip_code = model.VipCode,
                            //    vip_name = model.VipName,
                            //    receiver_name = model.ReceivingName,
                            //    receiver_mobile = model.ReceivingTelPhone,
                            //    receiver_phone = model.ReceivingPhone,
                            //    receiver_zip = model.NeedPostalCode,
                            //    receiver_area = model.AddressMessage,
                            //    receiver_address = model.ReceivingAddress,
                            //    payCode = order.payCode,
                            //    vipIdCard = order.vipIdCard,
                            //    vipRealName = order.vipRealName,
                            //    vipEmail = order.vipEmail,
                            //    amount = 0,
                            //    payment_amount = 0,
                            //    post_fee = 0,
                            //    discount_fee = 0,
                            //    payment = 0,
                            //    qty = "1",
                            //    weight_origin = "0",
                            //    post_cost = 0,
                            //    mail_no = model.ReturnExpressCode,
                            //    platform_flag = "0",
                            //    IsGeneralize = 0,
                            //    Status_CashBack = 0,
                            //    Status_Retreat = 0,
                            //    Status_ExpressIndemnity = 0,
                            //    ExchangeStatus = 0,
                            //    ReissueStatus = 0
                            //};
                            //db.T_OrderList.Add(newOrder);
                            //db.SaveChanges();
                            //foreach (var item in exchangedetail)
                            //{
                            //    T_OrderDetail Orderdetail = db.T_OrderDetail.FirstOrDefault(s => s.oid.Equals(order.code) && s.item_code.Equals(item.SendProductCode));
                            //    if (Orderdetail != null)//修改原订单详情换货状态与换货数量
                            //    {
                            //        Orderdetail.ExchangeStatus = 1;
                            //        Orderdetail.ExchangeQty += item.SendProductNum;
                            //        db.SaveChanges();
                            //    }
                            //    T_OrderDetail t = new T_OrderDetail
                            //    {
                            //        oid = newCode,
                            //        refund = 0,
                            //        item_code = item.NeedProductCode,
                            //        item_name = item.NeedProductName,
                            //        item_simple_name = "",
                            //        sku_code = "",
                            //        sku_name = "",
                            //        qty = 0,
                            //        price = 0,
                            //        amount = 0,
                            //        discount_fee = 0,
                            //        amount_after = 0,
                            //        post_fee = 0,
                            //        platform_item_name = "",
                            //        platform_sku_name = "",
                            //        note = "",
                            //        ExchangeStatus = 0,
                            //        ExchangeQty = 0,
                            //        ReissueStatus = 0,
                            //        ReissueQty = 0,
                            //        RetreatQty = 0,
                            //        RetreatStatus = 0
                            //    };
                            //    db.T_OrderDetail.Add(t);
                            //    db.SaveChanges();
                            //}

                            //#endregion

                            #region 加入补发货

                            string remark = "";
                            T_Reissue re = db.T_Reissue.FirstOrDefault(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0);
                            if (re != null)
                            {
                                var date = Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd");
                                var modeldate = Convert.ToDateTime(re.CreatDate).ToString("yyyyMMdd");

                                if (re != null && int.Parse(date) - int.Parse(modeldate) <= 3)
                                    remark = model.SystemRemark + "3天内补发货重复";
                            }
                            //更改订单主表补发货状态
                            //else
                            //{
                            //    order.ReissueStatus = 1;
                            //    db.SaveChanges();
                            //}
                            T_Reissue reissue = new T_Reissue
                            {
                                OrderCode = model.OrderCode,
                                NewOrderCode = "8" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                                VipName = model.VipName,
                                StoreName = model.StoreName,
                                WarhouseName = model.NeedWarhouse,
                                ExpressName = model.NeedExpress,
                                OrderType = "Return",
                                SingleTime = model.SingleTime,
                                ReceivingName = model.ReceivingName,
                                PostalCode = model.NeedPostalCode,
                                Phone = model.ReceivingPhone,
                                TelPhone = model.ReceivingTelPhone,
                                VipCode = model.VipCode,
                                Address = model.ReceivingAddress,
                                AddressMessage = model.AddressMessage,
                                SalesRemark = model.SalesRemark,
                                BuyRemark = model.BuyRemark,
                                StoreCode = model.StoreCode,
                                Step = 0,
                                Status = -2,
                                BusinessName = Com.GetReissueName(model.StoreCode, model.ExchangeReson),
                                PostUser = model.PostUser,
                                DraftName = Com.GetReissueName(model.StoreCode, model.ExchangeReson),
                                CreatDate = DateTime.Now,
                                IsDelete = 0,
                                ReissueReson = model.ExchangeReson,
                                SystemRemark = remark
                            };
                            db.T_Reissue.Add(reissue);
                            db.SaveChanges();
                            IQueryable<T_ExchangeDetail> detail = db.T_ExchangeDetail.Where(s => s.ExchangeCenterId == model.ID);
                            foreach (var item in detail)
                            {
                                T_ReissueDetail items = new T_ReissueDetail
                                {
                                    ProductCode = item.NeedProductCode,
                                    ProductName = item.NeedProductName,
                                    Num = item.NeedProductNum,
                                    ReissueId = reissue.ID
                                };
                                db.T_ReissueDetail.Add(items);
                            }
                            db.SaveChanges();

                            #endregion

                            #region 判断仓库是否收货

                            T_ReturnToStorage storge = db.T_ReturnToStorage.SingleOrDefault(s => s.Retreat_expressNumber.Equals(model.ReturnExpressCode));
                            if (storge != null)
                            {
                                //List<T_ExchangeCenter> exchange = db.T_ExchangeCenter.Where(s => s.OrderCode.Equals(model.OrderCode) && s.IsDelete == 0).ToList();

                                model.WarhouseStatus = 1;
                                model.Status = 1;
                                T_ExchangeCenterApprove approve1 = db.T_ExchangeCenterApprove.SingleOrDefault(s => s.ApproveName.Equals("仓库") && !s.ApproveTime.HasValue && s.Pid == model.ID);
                                approve1.ApproveName = "仓库";
                                approve1.ApproveStatus = 1;
                                approve1.ApproveTime = DateTime.Now;
                                //判断是否第一次换货，如果是则修改订单状态为已收货
                                //if (exchange.Count() == 1)
                                //{
                                //    order.ExchangeStatus = 2;
                                //}
                            }


                            #endregion

                        }
                    }
                    List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "换货未审核").ToList();
                    if (ModularNotaudited.Count > 0)
                    {
                        foreach (var item in ModularNotaudited)
                        {
                            db.T_ModularNotaudited.Remove(item);
                        }
                        db.SaveChanges();
                    }
                    string RetreatAppRoveSql = "  select isnull(ApproveUser,ApproveName) as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExchangeCenterApprove where  Pid in ( select ID from T_ExchangeCenter where IsDelete=0 ) and  ApproveStatus=-1 and ApproveTime is null GROUP BY ApproveName,ApproveUser";
                    List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                    {
                        string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                        T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "换货未审核" && a.PendingAuditName == PendingAuditName);
                        if (NotauditedModel != null)
                        {
                            NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                            db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                        }
                        else
                        {
                            T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                            ModularNotauditedModel.ModularName = "换货未审核";
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
