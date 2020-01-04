using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EBMS.Models.MOD
{
    /// <summary>
    /// 总计
    /// </summary>
    public class CostCount
    {
        public int ID { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 费用
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 旧费用
        /// </summary>
        public Nullable<decimal> Oldtotal { get; set; }
    }

    /// <summary>
    /// 中转费
    /// </summary>
    public class TransFerAdjusts
    {
        /// <summary>
        /// 运单编号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public string SettlementDate { get; set; }
        /// <summary>
        /// 始发城市
        /// </summary>
        public string StartCityName { get; set; }
        /// <summary>
        /// 目地网点
        /// </summary>
        public string DestinationBranchName { get; set; }
        /// <summary>
        /// 结算重量
        /// </summary>
        public string SettlementWeight { get; set; }
        /// <summary>
        /// 中转费
        /// </summary>
        public string TransferCosts { get; set; }
        /// <summary>
        /// 运输类别名称
        /// </summary>
        public string TransportTypeName { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 新费用发生日
        /// </summary>
        public string NewCostHappenDay { get; set; }
        /// <summary>
        /// 签收网点
        /// </summary>
        public string SignInBranchName { get; set; }
        /// <summary>
        /// 新结算重量
        /// </summary>
        public string NewSettlementWeight { get; set; }
        /// <summary>
        /// 新中转费
        /// </summary>
        public string NewTransferCost { get; set; }
        /// <summary>
        /// 新运输类别
        /// </summary>
        public string NewTransportWay { get; set; }
        /// <summary>
        /// 揽件人名称
        /// </summary>
        public string SendName { get; set; }
        /// <summary>
        /// 派件人名称
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// 是否退回件
        /// </summary>
        public string IsReturn { get; set; }
        /// <summary>
        /// 揽件重量
        /// </summary>
        public string DeliveryWeight { get; set; }
        /// <summary>
        /// 揽件目的地
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 揽件中转费
        /// </summary>
        public string DeliveryTransferCosts { get; set; }
        /// <summary>
        /// 小计(新-旧)
        /// </summary>
        public string TotaNewOrOld { get; set; }
        /// <summary>
        /// 重量差(新结算重量-揽件重量)
        /// </summary>
        public string WeightGap { get; set; }
        /// <summary>
        /// 费用差(新中转费-揽件中转费)
        /// </summary>
        public string CostGap { get; set; }
    }

    /// <summary>
    /// 代收货款
    /// </summary>
    public class CollectionQuidco
    {
        /// <summary>
        /// 运单号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 下单日期
        /// </summary>
        public string OverTime { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrdersFrom { get; set; }
        /// <summary>
        /// 代收金额
        /// </summary>
        public string CollectionMoney { get; set; }
        /// <summary>
        /// 返款扣款日期
        /// </summary>
        public string FinanceQuidcoDate { get; set; }
        /// <summary>
        /// 返款扣款状态
        /// </summary>
        public string FinanceQuidcoStatus { get; set; }
        /// <summary>
        /// 揽件公司
        /// </summary>
        public string ReceiveCompany { get; set; }
        /// <summary>
        /// 应收合计(揽件站点)
        /// </summary>
        public decimal? ReceivableCost { get; set; }
        /// <summary>
        /// 派件公司
        /// </summary>
        public string SendCompany { get; set; }
        /// <summary>
        /// 应收合计(派件站点)
        /// </summary>
        public string ReceivableCostBySend { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public string SettlementDate { get; set; }
    }

    /// <summary>
    /// 互带费
    /// </summary>
    public class MutuallyCostAndTubeEasySend
    {
        /// <summary>
        /// 运单号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public string SettlementDate { get; set; }
        /// <summary>
        /// 揽件公司名称
        /// </summary>
        public string ReceiveCompanyName { get; set; }
        /// <summary>
        /// 费用名称
        /// </summary>
        public string CostName { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public string Weight { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string Cost { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
    }

    /// <summary>
    /// 到付费
    /// </summary>
    public class CollectCosts
    {
        /// <summary>
        /// 运单号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public string SettlementDate { get; set; }
        /// <summary>
        /// 揽件公司名称
        /// </summary>
        public string ReceiveCompanyName { get; set; }
        /// <summary>
        /// 派件公司名称
        /// </summary>
        public string SendComPanyName { get; set; }
        /// <summary>
        /// 费用名称
        /// </summary>
        public string CostName { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string SettlementCost { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public string SignInDate { get; set; }
    }

    public class AreaSubsidyCosts
    {
        /// <summary>
        /// 结算日期
        /// </summary>
        public string SettlementDate { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 始发地名称
        /// </summary>
        public string StartAdressName { get; set; }
        /// <summary>
        /// 目的地名称
        /// </summary>
        public string DestinationName { get; set; }
        /// <summary>
        /// 费用名称
        /// </summary>
        public string CostName { get; set; }
        /// <summary>
        /// 结算费用
        /// </summary>
        public string SettlementCost { get; set; }
        /// <summary>
        /// 揽件业务员名称
        /// </summary>
        public string ConsigneeName { get; set; }
        /// <summary>
        /// 派送员名称
        /// </summary>
        public string SendCodeName { get; set; }
        /// <summary>
        /// 到件时间
        /// </summary>
        public string ArriveDate { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public string SignInDate { get; set; }
    }
}