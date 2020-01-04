
using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    /// <summary>
    /// 订单
    /// </summary>
    public class OrdersController : BaseController
    {


        #region 属性/字段/公共方法

        EBMSEntities db = new EBMSEntities();

        public T_User UserModel
        {
            get
            {
                string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                return db.T_User.SingleOrDefault(a => a.Nickname == name);
            }
        }


        //获取快递费用
        //public Decimal GetExpressCostByNumber(string mail_no, string exCOde)
        //{
        //   string

        //    if (exCOde == "YUNDA")
        //        expressCost = db.T_ExpressMoney_Query.FirstOrDefault(s => s.Number.Equals(mail_no)) == null ? expressCost : db.T_ExpressMoney_Query.FirstOrDefault(s => s.Number.Equals(mail_no)).Total;
        //    else
        //        expressCost = db.T_ExpressCost.FirstOrDefault(s => s.Number.Equals(mail_no)) == null ? expressCost : db.T_ExpressCost.FirstOrDefault(s => s.Number.Equals(mail_no)).Cost;

        //    var orderCount = db.T_OrderList.Count(s => s.mail_no.Equals(mail_no));

        //    decimal cost = 0;
        //    if (orderCount > 0)
        //        cost = expressCost / orderCount;
        //    return cost;
        //}

        //获取成本费用
        public Decimal? GetExpressAmountByCode(string code)
        {
            var number = db.T_OrderDetail.Where(s => s.oid.Equals(code));
            Nullable<decimal> cost = 0;
            foreach (var item in number)
            {
                cost += db.T_goodsGY.FirstOrDefault(s => s.code.Equals(item.item_code)) == null ? 0 : db.T_goodsGY.FirstOrDefault(s => s.code.Equals(item.item_code)).cost_price * item.qty;
            }
            Nullable<decimal> cost1 = 0;
            if (cost > 0)
                cost1 = cost;
            return cost1;
        }


        public class Orders
        {
            public int ID { get; set; }
            public string Number { get; set; }
            public Nullable<decimal> Cost { get; set; }
            public string Reson { get; set; }
        }

        public class OrdersProduct
        {
            /// <summary>
            /// 编码
            /// </summary>
            public string Code { get; set; }
            /// <summary>
            /// 订单时间
            /// </summary>
            public string OdersTime { get; set; }
            /// <summary>
            /// 商品名称 
            /// </summary>
            public string ProductName { get; set; }
            /// <summary>
            /// 销售数量
            /// </summary>
            public int Num { get; set; }
            /// <summary>
            /// 销售金额
            /// </summary>
            public decimal Amount { get; set; }
            /// <summary>
            /// 成本
            /// </summary>
            public decimal Cost { get; set; }
            /// <summary>
            /// 快递费用
            /// </summary>
            public decimal ExpressCost { get; set; }
            /// <summary>
            /// 利润
            /// </summary>
            public decimal Profit { get; set; }
        }
        public class OrdersProduct1
        {
            /// <summary>
            /// 商品名称 
            /// </summary>
            public string ProductName { get; set; }
            /// <summary>
            /// 销售金额
            /// </summary>
            public decimal Amount { get; set; }
            /// <summary>
            /// 成本
            /// </summary>
            public decimal Cost { get; set; }
            /// <summary>
            /// 快递费用
            /// </summary>
            public decimal ExpressCost { get; set; }
            /// <summary>
            /// 利润
            /// </summary>
            public decimal Profit { get; set; }
        }

        #endregion


        #region 视图

        [Description("订单列表")]
        public ActionResult ViewOrdersList()
        {
            return View();
        }

        [Description("订单详情")]
        public ActionResult ViewExpressDetail(string oid)
        {
            ViewData["oid"] = oid;
            return View();
        }

        [Description("商品列表")]
        public ActionResult ViewOrdersProductList()
        {
            return View();
        }

        [Description("卷皮网订单")]
        public ActionResult ViewListJPW()
        {
            return View();
        }



        [Description("卷皮网订单详情")]
        public ActionResult ViewDetailListJPW(string code)
        {
            ViewData["code"] = code;
            return View();
        }

        [Description("订单状态数量")]
        public ActionResult ViewOrderStatusCount(string code, int status)
        {
            ViewData["code"] = code;
            ViewData["status"] = status;
            return View();
        }


        #endregion



        #region Post提交


        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="number"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewOrdersList(Lib.GridPager pager, string number, string startDate, string endDate)
        {
            IQueryable<T_OrderList> orderList = db.T_OrderList.AsQueryable();
            if (!string.IsNullOrWhiteSpace(number))
                orderList = orderList.Where(s => s.platform_code.Contains(number));
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate + " 00:00:00");
                orderList = orderList.Where(s => s.createtime >= start);
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate + " 23:59:59");
                orderList = orderList.Where(s => s.createtime <= end);
            }
            pager.totalRows = orderList.Count();

            //foreach (var item in orderList)
            //{
            //    Orders model = new Orders
            //    {
            //        ID = item.ID,
            //        order_type_name = item.order_type_name,
            //        platform_code = item.platform_code,
            //        code = item.code,
            //        warehouse_name = item.warehouse_name,
            //        shop_name = item.shop_name,
            //        createtime = item.createtime,
            //        express_name = item.express_name,
            //        amount = item.amount,
            //        post_fee = GetExpressCostByNumber(item.mail_no, item.express_code),
            //        post_cost = GetExpressAmountByCode(item.code),
            //        Status_CashBack = item.Status_CashBack,
            //        Status_Retreat = item.Status_Retreat,
            //        Status_ExpressIndemnity = item.Status_ExpressIndemnity,
            //        ExchangeStatus = item.ExchangeStatus,
            //        ReissueStatus = item.ReissueStatus
            //    };
            //    list.Add(model);
            //}
            List<T_OrderList> querData = orderList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewDetailsList(Lib.GridPager pager, string oid)
        {
            IQueryable<T_OrderDetail> detailList = db.T_OrderDetail.Where(s => s.oid.Equals(oid));
            pager.totalRows = detailList.Count();
            List<T_OrderDetail> querData = detailList.OrderByDescending(s => s.id).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 统计订单状态数量
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetVieOrderCountList(Lib.GridPager pager, string code, int status)
        {
            List<Orders> ordersList = new List<Orders>();
            //返现
            if (status == 1)
            {
                List<T_CashBack> cashList = db.T_CashBack.Where(s => s.OrderNum.Equals(code)).ToList();
                foreach (var item in cashList)
                {
                    Orders model = new Orders
                    {
                        ID = item.ID,
                        Number = item.OrderNum,
                        Cost = item.BackMoney,
                        Reson = item.Reason
                    };
                    ordersList.Add(model);
                }
            }
            else if (status == 2)//退货退款
            {
                List<T_Retreat> retreatList = db.T_Retreat.Where(s => s.Retreat_OrderNumber.Equals(code)).ToList();
                foreach (var item in retreatList)
                {
                    Orders model = new Orders
                    {
                        ID = item.ID,
                        Number = item.Retreat_OrderNumber,
                        Cost = item.Retreat_Actualjine,
                        Reson = item.Retreat_Reason
                    };
                    ordersList.Add(model);
                }
            }
            else if (status == 3)//快递赔付
            {
                List<T_ExpressIndemnity> expressIndeList = db.T_ExpressIndemnity.Where(s => s.OrderNum.Equals(code)).ToList();
                foreach (var item in expressIndeList)
                {
                    Orders model = new Orders
                    {
                        ID = item.ID,
                        Number = item.OrderNum,
                        Cost = Convert.ToDecimal(item.OrderMoney),
                        Reson = item.RetreatReason
                    };
                    ordersList.Add(model);
                }
            }
            else if (status == 4)//换货
            {
                List<T_ExchangeCenter> exchangeList = db.T_ExchangeCenter.Where(s => s.OrderCode.Equals(code)).ToList();
                foreach (var item in exchangeList)
                {
                    Orders model = new Orders
                    {
                        ID = item.ID,
                        Number = item.OrderCode,
                        Cost = 0,
                        Reson = item.ExchangeReson
                    };
                    ordersList.Add(model);
                }
            }
            else if (status == 5)//补发
            {
                List<T_Reissue> reissueList = db.T_Reissue.Where(s => s.OrderCode.Equals(code)).ToList();
                foreach (var item in reissueList)
                {
                    Orders model = new Orders
                    {
                        ID = item.ID,
                        Number = item.OrderCode,
                        Cost = item.Cost,
                        Reson = item.ReissueReson
                    };
                    ordersList.Add(model);
                }
            }
            pager.totalRows = ordersList.Count();
            List<Orders> querData = ordersList.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="productCode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetOrderProductList(Lib.GridPager pager, string productName, string startDate, string endDate)
        {
            string sql = string.Format(@"select  cd.item_code as Code,convert(nvarchar(10),c.createtime,120) as OdersTime,
                                                cd.item_name as ProductName,ISNULL(SUM(cd.qty),0) as Num,ISNULL(SUM(cd.amount_after),0) as Amount,
                                                ISNULL(SUM(cb.cost_price*cd.qty),0) as Cost,ISNULL(SUM(eq.Total),0) as ExpressCost,
                                                (ISNULL(SUM(cd.amount_after),0)-ISNULL(SUM(cb.cost_price*cd.qty),0))-ISNULL(SUM(eq.Total),0) Profit
                                    from T_OrderDetail cd 
                                            inner join T_OrderList c   on cd.oid=c.code
                                            left join T_goodsGY cb on cb.code=cd.item_code
                                            left join T_ExpressMoney_Query eq on eq.Number=c.mail_no
                                            WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(productName))
                sql += " AND cd.item_name like'%" + productName + "%' OR cd.item_code like'%" + productName + "%'";
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                sql += " AND convert(nvarchar(10),c.createtime,120)>='" + startDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                sql += " AND convert(nvarchar(10),c.createtime,120)<='" + endDate + "'";
            }
            sql += " group by convert(nvarchar(10),c.createtime,120),cd.item_name,cd.item_code order by convert(nvarchar(10),c.createtime,120)  desc";
            IQueryable<OrdersProduct> list = db.Database.SqlQuery<OrdersProduct>(sql).AsQueryable();
            pager.totalRows = list.Count();
            List<OrdersProduct1> listSum = new List<OrdersProduct1>();
            OrdersProduct1 model = new OrdersProduct1();
            model.ProductName = "总费用:";
            model.Amount = list.Sum(s => s.Amount);
            model.Cost = list.Sum(s => s.Cost);
            model.ExpressCost = list.Sum(s => s.ExpressCost);
            model.Profit = list.Sum(s => s.Profit);
            listSum.Add(model);
            List<OrdersProduct> querData = list.Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in querData
                        select new OrdersProduct
                        {
                            Code = c.Code,
                            OdersTime = c.OdersTime,
                            ProductName = c.ProductName,
                            Num = c.Num,
                            Amount = c.Amount,
                            Cost = c.Cost,
                            ExpressCost = c.ExpressCost,
                            Profit = c.Profit
                        }).ToArray(),
                footer = listSum
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获得卷皮网订单
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="queryStr"></param>
        /// <param name="status"></param>
        /// <param name="shopcode"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetJPWOrdersList(Lib.GridPager pager, string queryStr, int status = -1)
        {
            IQueryable<T_ManualOrder> queryData = db.T_ManualOrder.AsQueryable();
            if (status != -1)
                queryData = db.T_ManualOrder.Where(s => s.orderstatus == status);
            if (!string.IsNullOrEmpty(queryStr))
                queryData = queryData.Where(s => s.platform_code.Equals(queryStr));
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 卷皮网订单详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetJPWOrdersDetailList(Lib.GridPager pager, string code)
        {
            IQueryable<T_ManualorderDetail> list = db.T_ManualorderDetail.Where(s => s.OrderCode.Equals(code));
            pager.totalRows = list.Count();
            List<T_ManualorderDetail> queryData = list.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData) + "}";
            return Content(json);
        }


        /// <summary>
        /// 删除卷皮网订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewListJPWDelete(int id = 0)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_ManualOrder model = db.T_ManualOrder.Find(id);
                    db.T_ManualOrder.Remove(model);
                    db.SaveChanges();
                    List<T_ManualorderDetail> list = db.T_ManualorderDetail.Where(a => a.OrderCode.Equals(model.platform_code)).ToList();
                    foreach (var deleteItem in list)
                    {
                        db.T_ManualorderDetail.Remove(deleteItem);
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


        /// <summary>
        /// 写入管易
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Approve(int id)
        {
            T_ManualOrder order = new T_ManualOrder();
            order = db.T_ManualOrder.Find(id);
            if (order != null)
            {
                try
                {
                    using (TransactionScope sc = new TransactionScope())
                    {
                        order.orderstatus = 1;
                        db.SaveChanges();
                        //详情
                        IQueryable<T_ManualorderDetail> details = null;
                        details = db.T_ManualorderDetail.Where(a => a.OrderCode.Equals(order.platform_code));
                        //构造商品明细
                        string detail = "";
                        foreach (var item in details)
                        {
                            detail += "{\"qty\":" + item.qty + ",\"price\":\"" + item.price + "\",\"note\":\"\",\"refund\":0,\"item_code\":\"" + System.Web.HttpContext.Current.Server.UrlEncode(item.item_code) + "\"}";
                        }
                        //构造付款信息  必须要付款信息 否则全让利了 应收成0
                        string payment = "";
                        //discount_fee 让利直接给出 无效 此字段改成实收金额
                        payment += "{\"pay_type_code\":\"wangyin\",\"payment\":\"" + order.payment + "\"}";
                        GY gy = new GY();
                        string cmd = "";
                        cmd = "{" +
                                    "\"appkey\":\"171736\"," +
                                    "\"method\":\"gy.erp.trade.add\"," +
                                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                    "\"order_type_code\":\"" + order.order_type_code + "\"," +
                                     "\"platform_code\":\"" + order.platform_code + "\"," +
                                    "\"shop_code\":\"" + order.shop_code + "\"," +
                                    "\"express_code\":\"" + order.express_code + "\"," +
                                        "\"receiver_province\":\"" + order.receiver_province + "\"," +
                                            "\"receiver_city\":\"" + order.receiver_city + "\"," +
                                                "\"receiver_district\":\"" + order.receiver_district + "\"," +
                                    "\"warehouse_code\":\"" + order.warehouse_code + "\"," +
                                    "\"vip_code\":\"" + order.vip_code + "\"," +
                                    "\"vip_name\":\"" + order.vip_name + "\"," +
                                    "\"receiver_name\":\"" + order.receiver_name + "\"," +
                                    "\"receiver_address\":\"" + order.receiver_address + "\"," +
                                    "\"receiver_mobile\":\"" + order.receiver_mobile + "\"," +
                                    "\"receiver_phone\":\"" + order.receiver_phone + "\"," +
                                    "\"deal_datetime\":\"" + order.deal_datetime.ToString() + "\"," +
                                     "\"pay_datetime\":\"" + order.deal_datetime.ToString() + "\"," +
                                    "\"buyer_memo\":\"" + order.buyer_memo + "\"," +
                                    "\"seller_memo\":\"" + order.seller_memo + "\"," +
                                       "\"business_man_code\":\"" + order.business_man_code + "\"," +
                                       "\"payments\":[" + payment + "]," +  //payment 支付金额 设置了也无效 应该是根据明细里面的单价 自动算出来的
                                      "\"details\":[" + detail + "]" +
                                    "}";
                        string sign = gy.Sign(cmd);
                        string comcode = "{" +
                               "\"appkey\":\"171736\"," +
                                    "\"method\":\"gy.erp.trade.add\"," +
                                    "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                    "\"order_type_code\":\"" + order.order_type_code + "\"," +
                                     "\"platform_code\":\"" + order.platform_code + "\"," +
                                    "\"shop_code\":\"" + order.shop_code + "\"," +
                                    "\"express_code\":\"" + order.express_code + "\"," +
                                        "\"receiver_province\":\"" + order.receiver_province + "\"," +
                                            "\"receiver_city\":\"" + order.receiver_city + "\"," +
                                                "\"receiver_district\":\"" + order.receiver_district + "\"," +
                                    "\"warehouse_code\":\"" + order.warehouse_code + "\"," +
                                    "\"vip_code\":\"" + order.vip_code + "\"," +
                                    "\"vip_name\":\"" + order.vip_name + "\"," +
                                    "\"receiver_name\":\"" + order.receiver_name + "\"," +
                                    "\"receiver_address\":\"" + order.receiver_address + "\"," +
                                    "\"receiver_mobile\":\"" + order.receiver_mobile + "\"," +
                                    "\"receiver_phone\":\"" + order.receiver_phone + "\"," +
                                    "\"deal_datetime\":\"" + order.deal_datetime.ToString() + "\"," +
                                     "\"pay_datetime\":\"" + order.deal_datetime.ToString() + "\"," +
                                     "\"sign\":\"" + sign + "\"," +
                                    "\"buyer_memo\":\"" + order.buyer_memo + "\"," +
                                    "\"seller_memo\":\"" + order.seller_memo + "\"," +
                                       "\"business_man_code\":\"" + order.business_man_code + "\"," +
                                        "\"payments\":[" + payment + "]," +
                                      "\"details\":[" + detail + "]" +
                                "}";
                        string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string result = jsonData[0].ToString();
                        if (result == "True")
                        {
                            sc.Complete();
                            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { State = "Faile", Message = "数据有误,请联系管理员" }, JsonRequestBehavior.AllowGet);
                        }
                    }


                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new { State = "Faile", Message = "数据出错，请联系技术人员" }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion
    }
}
