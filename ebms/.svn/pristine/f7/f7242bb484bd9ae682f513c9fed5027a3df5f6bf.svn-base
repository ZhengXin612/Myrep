using EBMS.App_Code;
using EBMS.Models;
using EBMS.Models.MOD;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class ExpressCostCountController : BaseController
    {

        #region 公共字段/方法/属性

        EBMSEntities db = new EBMSEntities();

        public class CostSum
        {
            public decimal Total { get; set; }
        }

        public class FinancialCost
        {
            /// <summary>
            /// 总计
            /// </summary>
            public string DepartName { get; set; }
            /// <summary>
            /// 金额
            /// </summary>
            public decimal Cost { get; set; }
        }

        #endregion


        #region 视图

        /// <summary>
        /// 支付宝账单
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewAliCost()
        {

            ViewData["storeName"] = Com.Shop();
            return View();
        }

        /// <summary>
        /// 费用统计
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewCostCountList()
        {
            ViewData["storeName"] = Com.Shop();
            return View();
        }

        /// <summary>
        /// 店铺快递总费用
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewExpressCostCountByShop()
        {
            ViewData["storeName"] = Com.Shop();
            return View();
        }

        /// <summary>
        /// 日期快递费用
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewExpressCostCountBydate()
        {
            return View();
        }

        /// <summary>
        /// 非韵达
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewNotYUNDA()
        {
            List<SelectListItem> item = new List<SelectListItem> 
            {
                 new SelectListItem { Text = "==请选择==", Value = ""},
                new SelectListItem { Text = "黎托韵达", Value = "黎托韵达"},
                new SelectListItem { Text = "德邦", Value = "德邦" },
                new SelectListItem { Text = "汇通", Value = "汇通" },
                new SelectListItem { Text = "圆通", Value = "圆通" },
               new SelectListItem { Text = "顺丰", Value = "顺丰" },
               new SelectListItem { Text = "杭州申通", Value = "杭州申通" },
               new SelectListItem { Text = "长沙申通", Value = "长沙申通" },
               new SelectListItem { Text = "杭州邮政", Value = "杭州邮政" },
               new SelectListItem { Text = "榔梨快递包裹", Value = "榔梨快递包裹" },
               new SelectListItem { Text = "榔梨邮政平邮", Value = "榔梨邮政平邮" },
               new SelectListItem { Text = "高桥邮政", Value = "高桥邮政" }
            };
            ViewData["expressName"] = item;
            ViewData["date"] = Convert.ToDateTime(DateTime.Now.AddDays(-3)).ToString("yyyy-MM-dd");
            return View();
        }

        /// <summary>
        /// 中转费
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewTransFerAdjustCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            return View();
        }

        /// <summary>
        /// 代收货款
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewCollectionCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            return View();
        }

        /// <summary>
        /// 区域补贴
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewAreaSubsidyCost()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            return View();
        }

        /// <summary>
        /// 平衡派送费
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewSendBalanceCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            var balanceName = db.T_BalanceCost.Select(s => new { s.SettlementName }).GroupBy(s => s.SettlementName).ToList();
            var listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            foreach (var item in balanceName)
            {
                listItems.Add(new SelectListItem { Text = item.Key.ToString(), Value = item.Key.ToString() });
            }
            ViewData["balanceName"] = listItems;
            return View();
        }

        /// <summary>
        /// 有偿派送费
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewSendCompensationCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            var compensationName = db.T_CompensationCost.Select(s => new { s.SettlementName }).GroupBy(s => s.SettlementName);
            var listItems1 = new List<SelectListItem>();
            listItems1.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            foreach (var item in compensationName)
            {
                listItems1.Add(new SelectListItem { Text = item.Key.ToString(), Value = item.Key.ToString() });
            }
            ViewData["compensationName"] = listItems1;
            return View();
        }

        /// <summary>
        /// 互带费
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewMutuallyCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            return View();
        }


        /// <summary>
        /// 到付费
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewCollectCostList()
        {
            ViewData["date"] = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            return View();
        }

        #endregion

        #region POST提交JSON对象

        #region 支付宝账单


        /// <summary>
        /// 支付宝账单
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="orderNumber"></param>
        /// <param name="shopName"></param>
        /// <returns></returns>
        public ContentResult GetaLicostList(Lib.GridPager pager, string orderNumber, string shopName)
        {
            IQueryable<T_ZD_Cost> list = db.T_ZD_Cost.AsQueryable();
            if (!string.IsNullOrWhiteSpace(orderNumber))
                list = list.Where(s => s.Number.Equals(orderNumber));
            if (!string.IsNullOrWhiteSpace(shopName))
                list = list.Where(s => s.DepartName.Equals(shopName));
            pager.totalRows = list.Count();
            //List<FinancialCost> footList = new List<FinancialCost>();
            //FinancialCost foot = new FinancialCost
            //{
            //    DepartName = "总计:",
            //    Cost = list.Count() > 0 ? list.Sum(s => s.Cost) : "0"
            //};
            //footList.Add(foot);
            List<T_ZD_Cost> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

        /// <summary>
        /// 时间费用
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ContentResult GetExpressCostCountByDate(string startDate)
        {
            string sql = string.Format(@"select SUM(Total) as Total,CONVERT(varchar(100), CreateDate, 23)  from T_ExpressMoney_Query where CONVERT(varchar(100), CreateDate, 23)='" + startDate + "' group by CONVERT(varchar(100), CreateDate, 23) ");
            IQueryable<CostCount> list = db.Database.SqlQuery<CostCount>(sql).AsQueryable();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 根据店铺统计
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ContentResult GetExpressCostCountByShop(string shopName)
        {

            List<T_ExpressMoney_Query> list = new List<T_ExpressMoney_Query>();
            List<T_OrderList> orderlist = db.T_OrderList.Where(s => s.shop_name.Equals(shopName)).ToList();
            decimal cost = 0;
            foreach (var item in orderlist)
            {
                List<T_ExpressMoney_Query> querList = db.T_ExpressMoney_Query.Where(s => s.Number.Equals(item.mail_no)).ToList();
                cost += querList.Sum(s => s.Total);
            }
            T_ExpressMoney_Query model = new T_ExpressMoney_Query();
            model.Total = cost;
            list.Add(model);
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 非韵达
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="number"></param>
        /// <param name="startDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public JsonResult GetNotYUNDA(Lib.GridPager pager, string number, string startDate, string EndDate, string expressName)
        {
            IQueryable<T_ExpressCost> list = null;
            string sql = string.Format("select * from Express WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND Number='" + number + "'";
            }
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                startDate = Convert.ToDateTime(startDate).ToString("yyyyMMdd");
                sql += " AND Date>='" + startDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(EndDate))
            {
                EndDate = Convert.ToDateTime(EndDate).ToString("yyyyMMdd");
                sql += " AND Date<='" + EndDate + "'";
            }
            if (!string.IsNullOrEmpty(expressName))
            {
                sql += " AND ExpressName='" + expressName + "'";
            }
            list = db.Database.SqlQuery<T_ExpressCost>(sql).AsQueryable();
            pager.totalRows = list.Count();
            var querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            var json = new
            {
                total = pager.totalRows,
                rows = (from r in querData
                        select new T_ExpressCost
                        {
                            ID = r.ID,
                            Date = r.Date,
                            ExpressName = r.ExpressName,
                            Number = r.Number,
                            Cost = r.Cost,
                            Destination = r.Destination
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 费用统计
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetCostCountList(Lib.GridPager pager, string number, string storeName)
        {
            IQueryable<T_ExpressMoney_Query> list = db.T_ExpressMoney_Query.AsQueryable();
            if (!string.IsNullOrWhiteSpace(number))
                list = list.Where(s => s.Number.Contains(number));
            pager.totalRows = list.Count();
            List<CostSum> footList = new List<CostSum>();
            CostSum cost = new CostSum
            {
                Total = list.Count() > 0 ? list.Sum(s => s.Total) : 0
            };
            footList.Add(cost);
            List<T_ExpressMoney_Query> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footList, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }


        /// <summary>
        /// 中转费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTransFerAdjustCostList(Lib.GridPager pager, string number, string startDate, string endDate, string shopName, string newCostHappenDayStart, string newCostHappenDayEnd)
        {
            IQueryable<TransFerAdjusts> queryData = null;
            string sql = string.Format(@"select t.Number,t.SettlementDate,t.StartCityName,t.DestinationBranchName,
                                                                t.SettlementWeight,t.TransferCosts,t.TransportTypeName,c.shop_name as ShopName,ta.NewCostHappenDay,ta.SignInBranchName,ta.NewSettlementWeight,ta.NewTransferCost,
                                                                ta.NewTransportWay,ta.SendName,ta.DeliveryName,d.DeliveryWeight,d.Destination,ta.TotaNewOrOld
                                                                from T_TransFerCost(NOLOCK) t 
                                                                        left join T_OrderList(NOLOCK) c on c.mail_no=t.Number
                                                                        left join T_TransFerAdjustCost(NOLOCK) ta on ta.Number=t.Number 
                                                                        left join T_DeliveryCountDetail(NOLOCK) d on d.Number=t.Number Where 1=1");
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                startDate = Convert.ToDateTime(startDate).ToString("yyyyMMdd");
                sql += " AND t.SettlementDate>='" + startDate + "'";
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                endDate = Convert.ToDateTime(endDate).ToString("yyyyMMdd");
                sql += " AND t.SettlementDate<='" + endDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(newCostHappenDayStart))
            {
                newCostHappenDayStart = Convert.ToDateTime(newCostHappenDayStart).ToString("yyyyMMdd");
                sql += " AND ta.NewCostHappenDay>='" + newCostHappenDayStart + "'";
            }
            if (!string.IsNullOrWhiteSpace(newCostHappenDayEnd))
            {
                newCostHappenDayEnd = Convert.ToDateTime(newCostHappenDayEnd).ToString("yyyyMMdd");
                sql += " AND ta.NewCostHappenDay<='" + newCostHappenDayEnd + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND t.Number LIKE'%" + number + "%'";
            }
            if (!string.IsNullOrWhiteSpace(shopName))
            {
                sql += " AND c.shop_name LIKE'%" + shopName + "%'";
            }
            queryData = db.Database.SqlQuery<TransFerAdjusts>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new TransFerAdjusts
                        {
                            Number = c.Number,
                            SettlementDate = c.SettlementDate,
                            StartCityName = c.StartCityName,
                            DestinationBranchName = c.DestinationBranchName,
                            SettlementWeight = c.SettlementWeight,
                            TransferCosts = c.TransferCosts,
                            TransportTypeName = c.TransportTypeName,
                            ShopName = c.ShopName,
                            NewCostHappenDay = c.NewCostHappenDay,
                            SignInBranchName = c.SignInBranchName,
                            NewSettlementWeight = c.NewSettlementWeight,
                            NewTransferCost = c.NewTransferCost,
                            TotaNewOrOld = !string.IsNullOrWhiteSpace(c.NewTransferCost.ToString()) ? Convert.ToDecimal(Convert.ToDecimal(c.NewTransferCost) - Convert.ToDecimal(c.TransferCosts)).ToString() : "0",
                            NewTransportWay = c.NewTransportWay,
                            SendName = c.SendName,
                            DeliveryName = c.DeliveryName,
                            IsReturn = c.Destination == "湖南长沙江背镇公司" ? "是" : "否",
                            DeliveryWeight = c.DeliveryWeight,
                            Destination = c.Destination,
                            WeightGap = (Convert.ToDecimal(c.NewSettlementWeight) - Convert.ToDecimal(c.DeliveryWeight)).ToString("0.00"),
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 代收货款
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCollectionCostList(Lib.GridPager pager, string number, string overStartTime, string overEndTime, string settementStartTime, string settementEndTime, string ordersFrom)
        {
            IQueryable<CollectionQuidco> queryData = null;

            string sql = string.Format(@"select c.Number,c.OverTime,
                                                                            c.OrdersFrom,c.CollectionMoney,c.FinanceQuidcoDate,c.FinanceQuidcoStatus,
                                                                            cs.ReceiveCompany,cs.ReceivableCost,cs.SendCompany,cs.ReceivableCostBySend,cs.SettlementDate
                                                                            from T_CollectionQuidcoStatus(NOLOCK) c 
                                                                                inner join T_CollectionServiceCost(NOLOCK) cs on cs.Number=c.Number where 1=1");
            if (!string.IsNullOrWhiteSpace(overStartTime))
            {
                sql += "  AND c.OverTime>='" + overStartTime + " 00:00:00" + "'";
            }
            if (!string.IsNullOrWhiteSpace(overEndTime))
            {
                sql += "  AND c.OverTime<='" + overEndTime + " 23:59:59" + "'";
            }
            if (!string.IsNullOrEmpty(settementStartTime))
            {
                sql += " AND cs.SettlementDate>='" + settementStartTime + "'";
            }
            if (!string.IsNullOrWhiteSpace(settementEndTime))
            {
                sql += " AND cs.SettlementDate<='" + settementStartTime + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND c.Number LIKE '%" + number + "%'";
            }
            if (!string.IsNullOrWhiteSpace(ordersFrom))
            {
                sql += " AND c.OrdersFrom LIKE '%" + ordersFrom + "%'";
            }
            queryData = db.Database.SqlQuery<CollectionQuidco>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.OverTime).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new CollectionQuidco
                        {
                            Number = c.Number,
                            OverTime = c.OverTime,
                            OrdersFrom = c.OrdersFrom,
                            CollectionMoney = c.CollectionMoney,
                            FinanceQuidcoDate = c.FinanceQuidcoDate,
                            FinanceQuidcoStatus = c.FinanceQuidcoStatus,
                            ReceiveCompany = c.ReceiveCompany,
                            ReceivableCost = c.ReceivableCost,
                            SendCompany = c.SendCompany,
                            ReceivableCostBySend = c.ReceivableCostBySend,
                            SettlementDate = c.SettlementDate
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 区域补贴
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAreaSubsidyCostList(Lib.GridPager pager, string number, string settlementStartDate, string settlementEndDate)
        {
            IQueryable<T_AreaSubsidyCost> queryData = null;
            string sql = string.Format(@"select * from T_AreaSubsidyCost(NOLOCK) Where 1=1");
            if (!string.IsNullOrWhiteSpace(settlementStartDate))
            {
                settlementStartDate = Convert.ToDateTime(settlementStartDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate>='" + settlementStartDate + "'";
            }
            if (!string.IsNullOrEmpty(settlementEndDate))
            {
                settlementEndDate = Convert.ToDateTime(settlementEndDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate<='" + settlementEndDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND Number LIKE'%" + number + "%'";
            }
            queryData = db.Database.SqlQuery<T_AreaSubsidyCost>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new AreaSubsidyCosts
                        {
                            Number = c.Number,
                            SettlementDate = c.SettlementDate,
                            StartAdressName = c.StartAdressName,
                            DestinationName = c.DestinationName,
                            CostName = c.CostName,
                            SettlementCost = c.SettlementCost,
                            ConsigneeName = c.ConsigneeName,
                            SendCodeName = c.SendCodeName,
                            ArriveDate = c.ArriveDate,
                            SignInDate = c.SignInDate
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 平衡派送费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBalenceSendCostList(Lib.GridPager pager, string number, string settlementStartDate, string settlementEndDate, string balanceName)
        {
            IQueryable<T_BalanceCost> queryData = null;

            string sql = string.Format(@"select * from  T_BalanceCost(NOLOCK) b  where 1=1");
            if (!string.IsNullOrEmpty(settlementStartDate))
            {
                settlementStartDate = Convert.ToDateTime(settlementStartDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate>='" + settlementStartDate + "'";
            }
            if (!string.IsNullOrEmpty(settlementEndDate))
            {
                settlementEndDate = Convert.ToDateTime(settlementEndDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate<='" + settlementEndDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND Number LIKE '%" + number + "%'";
            }
            if (!string.IsNullOrWhiteSpace(balanceName))
            {
                sql += " AND SettlementName='" + balanceName + "'";
            }
            queryData = db.Database.SqlQuery<T_BalanceCost>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new T_BalanceCost
                        {
                            Number = c.Number,
                            SettlementDate = c.SettlementDate,
                            CheckComPanyName = c.CheckChildComPanyName,
                            ConsigneeName = c.ConsigneeName,
                            SettlementName = c.SettlementName,
                            SettlementCost = c.SettlementCost,
                            SendName = c.SendName,
                            ArriveDate = c.ArriveDate,
                            SendDate = c.SendDate,
                            ActualDeliveryTime = c.ActualDeliveryTime,
                            SignInDate = c.SignInDate,
                            ProvisionDate = c.ProvisionDate,
                            SettlementDateOld = c.SettlementDateOld,
                            StartAdressName = c.StartAdressName,
                            DestinationName = c.DestinationName,
                            SettlementWeight = c.SettlementWeight,
                            TransportWay = c.TransportWay,
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 有偿派送费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCompensationSendCostList(Lib.GridPager pager, string number, string settlementStartDate, string settlementEndDate, string compensationName)
        {
            IQueryable<T_CompensationCost> queryData = null;

            string sql = string.Format(@"select * from T_CompensationCost(NOLOCK)  where 1=1");
            if (!string.IsNullOrEmpty(settlementStartDate))
            {
                settlementStartDate = Convert.ToDateTime(settlementStartDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate>='" + settlementStartDate + "'";
            }
            if (!string.IsNullOrEmpty(settlementEndDate))
            {
                settlementEndDate = Convert.ToDateTime(settlementEndDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate<='" + settlementEndDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND Number LIKE '%" + number + "%'";
            }
            if (!string.IsNullOrWhiteSpace(compensationName))
            {
                sql += " AND cs.SettlementName='" + compensationName + "'";
            }
            queryData = db.Database.SqlQuery<T_CompensationCost>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new T_CompensationCost
                        {
                            SettlementDate = c.SettlementDate,
                            SettlementDateOld = c.SettlementDateOld,
                            Number = c.Number,
                            CheckChildComPanyName = c.CheckChildComPanyName,
                            StartAdressName = c.StartAdressName,
                            DestinationName = c.DestinationName,
                            SettlementName = c.SettlementName,
                            SettlementCost = c.SettlementCost,
                            ConsigneeName = c.ConsigneeName,
                            SendName = c.SendName,
                            ArriveDate = c.ArriveDate,
                            SendDate = c.SendDate,
                            SignInDate = c.SignInDate,
                            ActualDeliveryTime = c.ActualDeliveryTime,
                            ProvisionDate = c.ProvisionDate,
                            SettlementWeight = c.SettlementWeight,
                            TransportWay = c.TransportWay
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 互带费
        /// </summary>
        /// <param name="number"></param>
        /// <param name="settlementDate"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetMutuallyCostList(Lib.GridPager pager, string number, string settlementStartDate, string settlementEndDate)
        {
            IQueryable<MutuallyCostAndTubeEasySend> queryData = null;

            string sql = string.Format(@"select m.Number,m.SettlementDate,m.ReceiveCompanyName,m.CostName,
                                        m.Weight,m.Cost,c.shop_name as ShopName 
                                            from T_MutuallyCost(NOLOCK) m 
                                            left join T_OrderList(NOLOCK) c on c.mail_no=m.Number where 1=1");
            if (!string.IsNullOrEmpty(settlementStartDate))
            {
                settlementStartDate = Convert.ToDateTime(settlementStartDate).ToString("yyyyMMdd");
                sql += " AND m.SettlementDate>='" + settlementStartDate + "'";
            }
            if (!string.IsNullOrEmpty(settlementEndDate))
            {
                settlementEndDate = Convert.ToDateTime(settlementEndDate).ToString("yyyyMMdd");
                sql += " AND m.SettlementDate<='" + settlementEndDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND m.Number LIKE '%" + number + "%'";
            }
            queryData = db.Database.SqlQuery<MutuallyCostAndTubeEasySend>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new MutuallyCostAndTubeEasySend
                        {
                            Number = c.Number,
                            SettlementDate = c.SettlementDate,
                            ReceiveCompanyName = c.ReceiveCompanyName,
                            CostName = c.CostName,
                            Weight = c.Weight,
                            Cost = c.Cost,
                            ShopName = c.ShopName
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 到付费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCollectCostList(Lib.GridPager pager, string number, string settlementStartDate, string settlementEndDate)
        {
            IQueryable<CollectCosts> queryData = null;

            string sql = string.Format(@"select Number,SettlementDate,ReceiveCompanyName,
                                        SendComPanyName,CostName,SettlementCost,SignInDate from T_CollectCost(NOLOCK) where 1=1");
            if (!string.IsNullOrEmpty(settlementStartDate))
            {
                settlementStartDate = Convert.ToDateTime(settlementStartDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate>='" + settlementStartDate + "'";
            }
            if (!string.IsNullOrEmpty(settlementEndDate))
            {
                settlementEndDate = Convert.ToDateTime(settlementEndDate).ToString("yyyyMMdd");
                sql += " AND SettlementDate<='" + settlementEndDate + "'";
            }
            if (!string.IsNullOrWhiteSpace(number))
            {
                sql += " AND Number LIKE '%" + number + "%'";
            }
            queryData = db.Database.SqlQuery<CollectCosts>(sql).AsQueryable();
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(s => s.SettlementDate).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            var json = new
            {
                total = pager.totalRows,
                rows = (from c in queryData
                        select new CollectCosts
                        {
                            Number = c.Number,
                            SettlementDate = c.SettlementDate,
                            ReceiveCompanyName = c.ReceiveCompanyName,
                            SendComPanyName = c.SendComPanyName,
                            CostName = c.CostName,
                            SettlementCost = c.SettlementCost,
                            SignInDate = c.SignInDate
                        }).ToArray()
            };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
