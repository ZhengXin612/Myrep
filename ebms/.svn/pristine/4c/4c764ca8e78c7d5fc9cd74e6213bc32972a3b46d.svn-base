using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Data;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
namespace EBMS.Controllers
{
	/// <summary>
	/// 返现
	/// </summary>
	public class CashBackController : BaseController
	{
		//返现

		// GET: /CashBack/
		#region 公共方法
		EBMSEntities db = new EBMSEntities();
		//数据表店铺配置审核流程方法
		public JsonResult getApproveMethod()
		{
			using (TransactionScope sc = new TransactionScope())
			{
				try
				{
					int reasult = 0;
					T_CashbackApproveConfig model = new T_CashbackApproveConfig();
					//得到所有的店铺
					IQueryable<T_ShopFromGY> shopMod = db.T_ShopFromGY.AsQueryable();
					List<string> shopList = new List<string>();
					foreach (var shopItem in shopMod)
					{
						shopList.Add(shopItem.name);
					}
					string[] shopArry = shopList.ToArray(); //存储了从管易获取的所有店铺名字
															//所有返现的理由
					IQueryable<T_CashBackReason> reasonMod = db.T_CashBackReason.AsQueryable();
					List<string> reasonList = new List<string>();
					foreach (var reasonItem in reasonMod)
					{
						reasonList.Add(reasonItem.Name);
					}
					string[] reasonArry = reasonList.ToArray(); //存储了所有返现的理由
					string[] moneyArry = new string[] { "0", "1" };//金钱 0 小于100  1大于等于100
					string[] rolesArry = new string[] { "售前", "售后", "其他" };
					for (int i = 0; i < shopArry.Length; i++)
					{
						for (int j = 0; j < reasonArry.Length; j++)
						{
							for (int k = 0; k < moneyArry.Length; k++)
							{
								for (int l = 0; l < rolesArry.Length; l++)
								{
									string _reason = reasonArry[j];
									string _shop = shopArry[i];
									int _money = int.Parse(moneyArry[k]);
									string _roles = rolesArry[l];
									List<T_CashbackApproveConfig> findMod = db.T_CashbackApproveConfig.Where(a => a.Reason == _reason && a.Shop == _shop && a.Money == _money && a.Roles == _roles).ToList();
									if (findMod.Count > 0)
									{
										findMod[0].Shop = _shop;
										findMod[0].Reason = _reason;
										findMod[0].Money = _money;
										findMod[0].Roles = _roles;
										db.Entry<T_CashbackApproveConfig>(findMod[0]).State = System.Data.Entity.EntityState.Modified;
									}
									else
									{
										model.Shop = _shop;
										model.Reason = _reason;
										model.Money = _money;
										model.Roles = _roles;
										db.T_CashbackApproveConfig.Add(model);
									}
									reasult = db.SaveChanges();
								}
							}
						}
					}
					if (reasult > 0)
					{
						sc.Complete();
					}
					return Json(reasult, JsonRequestBehavior.AllowGet);

				}
				catch (Exception ex)
				{
					return Json(ex.Message, JsonRequestBehavior.AllowGet);
				}

			}

		}
		//接收JSON 反序列化
		public static List<T> Deserialize<T>(string text)
		{
			try
			{
				JavaScriptSerializer js = new JavaScriptSerializer();
				List<T> list = (List<T>)js.Deserialize(text, typeof(List<T>));
				return list;

			}
			catch (Exception e)
			{

				return null;
			}
		}
		//返现理由下拉框
		public List<SelectListItem> GetReason()
		{

			var list = db.T_CashBackReason.AsQueryable();
			var selectList = new SelectList(list, "Name", "Name");
			List<SelectListItem> selecli = new List<SelectListItem>();
			selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
			selecli.AddRange(selectList);
			return selecli;
		}
		//返现支出帐号下拉框
		public List<SelectListItem> GetCashBackFrom(string ShopName)
		{


			var list = db.Database.SqlQuery<T_CashBackFrom>(" select * from T_CashBackFrom  where ShopName='" + ShopName + "' order by  IsBlending desc ").AsQueryable();
			var selectList = new SelectList(list, "Name", "Name");
			List<SelectListItem> selecli = new List<SelectListItem>();
			selecli.AddRange(selectList);
			return selecli;
		}
		//返现支出帐号下拉框
		public List<SelectListItem> GetCashBackFroms()
		{
			var list = db.T_CashBackFrom.AsQueryable();
			var selectList = new SelectList(list, "Name", "Name");
			List<SelectListItem> selecli = new List<SelectListItem>();
			selecli.AddRange(selectList);
			return selecli;
		}
		//获取审核详情记录
		private void GetApproveHistory(int id = 0)
		{
			var history = db.T_CashBackApprove.Where(a => a.Oid == id);
			string table = "<table class=\"fromEditTable setTextWidth300\"> <tbody><tr><td>审核人</td><td>审核结果</td><td>审核时间</td><td>备注</td></tr>";
			string tr = "";
			foreach (var item in history)
			{
				string s = "";
				if (item.Status == -1) s = "<font color=#d02e2e>未审核</font>";
				if (item.Status == 0) s = "<font color=#2299ee>审核中</font>";
				if (item.Status == 1) s = "<font color=#1fc73a>已同意</font>";
				if (item.Status == 2) s = "<font color=#d02e2e>不同意</font>";
				tr += string.Format("<tr><td><label>{0}</label></td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", item.ApproveName, s, item.ApproveTime, item.Note);
			}
			ViewData["history"] = table + tr + "</tbody></table>";
		}

		private string isNULL(object data)
		{
			if (data == null) return "";
			else return data.ToString();
		}
		//旺店通接口
		public static string GetTimeStamp()
		{
			return (GetTimeStamp(System.DateTime.Now));
		}
		/// <summary>
		/// 获取时间戳
		/// </summary>
		/// <returns></returns>
		public static string GetTimeStamp(System.DateTime time, int length = 10)
		{
			long ts = ConvertDateTimeToInt(time);
			return ts.ToString().Substring(0, length);
		}
		/// <summary>  
		/// 将c# DateTime时间格式转换为Unix时间戳格式  
		/// </summary>  
		/// <param name="time">时间</param>  
		/// <returns>long</returns>  
		public static long ConvertDateTimeToInt(System.DateTime time)
		{
			System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
			long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为10位      
			return t;
		}

		public string CreateParam(Dictionary<string, string> dicReq, bool isLower = false)
		{
			//排序
			dicReq = dicReq.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);

			StringBuilder sb = new StringBuilder();
			int i = 0;
			foreach (var item in dicReq)
			{
				if (item.Key == "sign")
					continue;
				if (i > 0)
				{
					sb.Append(";");
				}
				i++;
				sb.Append(item.Key.Length.ToString("00"));
				sb.Append("-");
				sb.Append(item.Key);
				sb.Append(":");

				sb.Append(item.Value.Length.ToString("0000"));
				sb.Append("-");
				sb.Append(item.Value);
			}
			if (isLower)
				dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401").ToLower());
			else
			{
				dicReq.Add("sign", MD5Encrypt(sb + "b978cefc1322fd0ed90aa5396989d401"));
			}
			sb = new StringBuilder();
			i = 0;
			foreach (var item in dicReq)
			{
				if (i == 0)
				{

					sb.Append(string.Format("{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
				}
				else
				{
					sb.Append(string.Format("&{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8)));
				}
				i++;
			}
			// HttpUtility.UrlEncode(
			return sb.ToString();
		}
		public static string MD5Encrypt(string strText)
		{
			MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
			string strMd5 = BitConverter.ToString(result);
			strMd5 = strMd5.Replace("-", "");
			return strMd5;// System.Text.Encoding.Default.GetString(result);
		}
		Dictionary<string, string> dic = new Dictionary<string, string>();


		[Description("得到管易的订单数据")]
		public JsonResult GetCashBackByGy(string code)
		{
			string repeat = "";
			//去重

			string scode = code.Substring(0, 1);
			App_Code.GY gy = new App_Code.GY();


			if (scode == "s" || scode == "S")
			{



				string cmd = "v=1.0&sign=&message={\"ordercodes\": ['" + code + "']}";
				string ret = gy.DoPostnew("http://114.55.15.162:30001/PubService.svc/QuerySaleOrder", cmd, Encoding.UTF8);
				JsonData jsonData = null;
				jsonData = JsonMapper.ToObject(ret);

				JsonData jsonMessage = jsonData["message"];
				int RecordCount = int.Parse(jsonMessage["RecordCount"].ToString());
				if (RecordCount > 0)
				{
					JsonData jsonMessageData = jsonMessage["Data"][0];
					JsonData jsonproductdetails = jsonMessageData["productdetails"];
					JsonData jsonpaymentdetails = jsonMessageData["paymentdetails"];
					code = jsonMessageData["tradeid"].ToString(); ;
					List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0" && a.Status != 3).ToList();
					if (modelList.Count > 0)
					{

						repeat += "_存在退货退款记录重复，";
					}
					//查是否有返现记录

					List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 3).ToList();
					if (cash.Count > 0)
					{
						repeat += "_存在返现记录重复，";
					}
					List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
					if (Reissue.Count > 0)
					{
						repeat += "_存在补发记录重复，";
					}
					List<T_ExchangeCenter> exchangeList = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
					if (exchangeList.Count > 0)
					{
						repeat += "_存在换货记录重复，";
					}
					//店铺名称
					string shop_name = jsonMessageData["shopname"].ToString();
					//店铺代码
					string shop_Code = jsonMessageData["shopcode"].ToString();
					string vip_code = isNULL(jsonMessageData["membername"]).ToString();
					//客户姓名
					string vip_name = isNULL(jsonMessageData["buyernick"]).ToString();
					//原收货人
					string receiver_name = isNULL(jsonMessageData["consignee"]).ToString();
					//原收货地址
					string shouhuodizhi = isNULL(jsonMessageData["address"]).ToString();
					string amount = isNULL(jsonpaymentdetails[0]["amount"]).ToString();

					string payment = isNULL(jsonpaymentdetails[0]["payableamount"]).ToString();
					T_CashBack model = new T_CashBack
					{
						OrderNum = jsonMessageData["tradeid"].ToString(),
						VipName = vip_name,
						ShopName = shop_name,
						WangWang = vip_code,
						OrderMoney = decimal.Parse(payment),
						Repeat = repeat
					};
					return Json(new { State = "Success", ModelList = model }, JsonRequestBehavior.AllowGet);
				}
			}
			else
			{

				List<T_Retreat> modelList = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(code.Trim()) && a.Isdelete == "0" && a.Status != 3).ToList();
				if (modelList.Count > 0)
				{

					repeat += "_存在退货退款记录重复，";
				}
				//查是否有返现记录

				List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(code.Trim()) && a.For_Delete == 0 && a.Status != 3).ToList();
				if (cash.Count > 0)
				{
					repeat += "_存在返现记录重复，";
				}
				List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
				if (Reissue.Count > 0)
				{
					repeat += "_存在补发记录重复，";
				}
				List<T_ExchangeCenter> exchangeList = db.T_ExchangeCenter.Where(a => a.OrderCode.Equals(code.Trim()) && a.IsDelete == 0).ToList();
				if (exchangeList.Count > 0)
				{
					repeat += "_存在换货记录重复，";
				}

				dic.Clear();

				dic.Add("src_tid", code);
				//dic.Add("trade_no", code);

				dic.Add("sid", "hhs2");
				dic.Add("appkey", "hhs2-ot");
				dic.Add("timestamp", GetTimeStamp());
				string cmd = CreateParam(dic, true);

				string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/trade_query.php", cmd, Encoding.UTF8);
				string ssx = Regex.Unescape(ret);
				JsonData jsonData = null;
				jsonData = JsonMapper.ToObject(ret);
				string iscode = jsonData["total_count"].ToString();
				if (iscode != "0")
				{
					JsonData jsontrades = jsonData["trades"];

					if (jsontrades.Count != 0)
					{
						JsonData trades = jsontrades[0];
						//店铺名称
						string shop_name = trades["shop_name"].ToString();
						//仓库编码
						string warehouse_no = trades["warehouse_no"].ToString();
						//原始订单编号
						string src_tids = trades["src_tids"].ToString();
						//下单时间
						string trade_time = trades["trade_time"].ToString();
						//付款时间
						string pay_time = trades["pay_time"].ToString();
						//旺旺号
						string customer_name = trades["buyer_nick"].ToString();
						//收件人姓名
						string receiver_name = trades["receiver_name"].ToString();
						//省
						string receiver_province = trades["receiver_province"].ToString();
						//市
						string receiver_city = trades["receiver_city"].ToString();
						//区
						string receiver_district = trades["receiver_district"].ToString();
						//详细地址
						string receiver_address1 = trades["receiver_address"].ToString();
						//电话号码
						string receiver_mobile = trades["receiver_mobile"].ToString();
						//邮政编码
						string receiver_zip = trades["receiver_zip"].ToString();
						//省市县
						string receiver_area = trades["receiver_area"].ToString();
						//快递公司编号
						string logistics_code = trades["logistics_code"].ToString();
						//快递单号
						string logistics_no = trades["logistics_no"].ToString();
						//买家留言
						string buyer_message = trades["buyer_message"].ToString();
						//客服备注
						string cs_remark = trades["cs_remark"].ToString();
						//实付金额
						//    string paid = trades["paid"].ToString();
						//double paid = 0.00;
						//for (int c = 0; c < jsontrades.Count; c++)
						//{
						//    paid += double.Parse(jsontrades[c]["paid"].ToString());
						//}
						List<T_CashBackDetail> DetailsList = new List<T_CashBackDetail>();
						double paid = 0.00;
						for (int c = 0; c < jsontrades.Count; c++)
						{
							paid += double.Parse(jsontrades[c]["paid"].ToString());
							JsonData goods_list = jsontrades[c]["goods_list"];
							for (int i = 0; i < goods_list.Count; i++)
							{
								T_CashBackDetail DetailsModel = new T_CashBackDetail();
								string ss = goods_list[i]["goods_no"] == null ? "" : goods_list[i]["goods_no"].ToString();
								DetailsModel.goods_no = ss;
								DetailsModel.goods_name = goods_list[i]["goods_name"] == null ? "" : goods_list[i]["goods_name"].ToString();
								DetailsModel.spec_name = goods_list[i]["spec_name"] == null ? "" : goods_list[i]["spec_name"].ToString();
								T_WDTGoods goods = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == ss);
								if (goods != null)
								{
									DetailsModel.unit = goods.unit_name;
								}
								else
								{

									DetailsModel.unit = "";
								}
								//   double ssds=double.Parse(goods_list[i]["paid"].ToString()) / double.Parse(goods_list[i]["actual_num"].ToString());

								decimal dec = Convert.ToDecimal(Math.Round(double.Parse(goods_list[i]["share_amount"].ToString()), 2));
								//DetailsModel.BackMoney = dec;//分摊邮费 


								int qyt = Convert.ToInt32(Convert.ToDecimal(goods_list[i]["actual_num"].ToString()));
								if (qyt != 0)
								{
									DetailsModel.qty = qyt;
									DetailsModel.price = dec / DetailsModel.qty;
								}
								else
								{
									DetailsModel.qty = 0;
									DetailsModel.price = dec;
								}
								//if (qyt > 0)
								//{
								DetailsList.Add(DetailsModel);
								//}

							}
						}
						#region 
						//商品详情
						//JsonData goods_list = trades["goods_list"];
						//查询3次。对应到具体的省市区
						//if (receiver_province != null)
						//{

						//    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_province);
						//    if (commonarea != null)
						//    {
						//        receiver_province = commonarea.REGION_NAME;
						//    }
						//}
						//if (receiver_city != null)
						//{

						//    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_city);
						//    if (commonarea != null)
						//    {

						//        receiver_city = commonarea.REGION_NAME;
						//    }
						//    if (receiver_city == "市辖区")
						//    {
						//        receiver_city = receiver_province;
						//        receiver_province = receiver_province.Substring(0, receiver_province.Length - 1);


						//    }
						//}
						//if (receiver_district != null)
						//{

						//    DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_district);
						//    if (commonarea != null)
						//    {
						//        receiver_district = commonarea.REGION_NAME;
						//    }
						//}
						//string ssq = receiver_province + "-" + receiver_city + "-" + receiver_district;
						//查询一次..
						#endregion
						string shop_Code = "";

						if (shop_name != null)
						{
							T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
							shop_Code = commonarea.shop_no;
							//shop_Code = "tm004";
						}
						T_CashBack model = new T_CashBack
						{
							OrderNum = code,
							VipName = receiver_name,
							ShopName = shop_name,
							WangWang = customer_name,
							OrderMoney = decimal.Parse(paid.ToString()),
							Repeat = repeat
						};
						var json = new
						{

							rows = (from r in DetailsList
									select new T_CashBackDetail
									{
										goods_no = r.goods_no,
										goods_name = r.goods_name,
										price = r.price,
										qty = r.qty,
										spec_name = r.spec_name,
										unit = r.unit,
									}).ToArray()
						};
						return Json(new { State = "Success", ModelList = model, Json = json }, JsonRequestBehavior.AllowGet);

						//return Json(new { State = "Success", ModelList = model }, JsonRequestBehavior.AllowGet);
					}

				}


				cmd = "";

				cmd = "{" +
					  "\"appkey\":\"171736\"," +
					  "\"method\":\"gy.erp.trade.get\"," +
					  "\"page_no\":1," +
					  "\"page_size\":10," +
					  "\"platform_code\":\"" + code + "\"," +
					  "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
					  "}";

				string sign = gy.Sign(cmd);
				cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
				ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
				jsonData = null;
				jsonData = JsonMapper.ToObject(ret);

				if (jsonData.Count == 6 || jsonData["orders"].Count == 0)
				{
					cmd = "{" +
					"\"appkey\":\"171736\"," +
					"\"method\":\"gy.erp.trade.history.get\"," +
					"\"page_no\":1," +
					"\"page_size\":10," +
					"\"platform_code\":\"" + code + "\"," +
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
				//订单信息 
				string platform_code = isNULL(jsonOrders["platform_code"]).ToString();
				//物流单号


				//物流公司名称
				string wuliugongsi = isNULL(jsonOrders["express_name"]).ToString();
				// string order_type_name = isNULL(jsonOrders["order_type_name"]).ToString();
				//店铺名称
				string shop_name1 = isNULL(jsonOrders["shop_name"]).ToString();
				//旺旺帐号 
				string vip_code = isNULL(jsonOrders["vip_code"]).ToString();
				//客户姓名
				string vip_name = isNULL(jsonOrders["vip_name"]).ToString();
				string receiver_phone = isNULL(jsonOrders["receiver_phone"]).ToString();
				string receiver_address = isNULL(jsonOrders["receiver_address"]).ToString();
				//string payCode = isNULL(jsonOrders["payCode"]).ToString();
				string vipIdCard = isNULL(jsonOrders["vipIdCard"]).ToString();
				string vipRealName = isNULL(jsonOrders["vipRealName"]).ToString();
				string vipEmail = isNULL(jsonOrders["vipEmail"]).ToString();
				JsonData details = jsonOrders["details"];
				string shouhuoreName = isNULL(jsonOrders["receiver_name"]).ToString();
				string shouhuodizhi = isNULL(jsonOrders["receiver_address"]).ToString();
				string amount = isNULL(jsonOrders["amount"]).ToString();
				string payment = isNULL(jsonOrders["payment"]).ToString();
				T_CashBack model1 = new T_CashBack
				{
					OrderNum = code,
					VipName = vip_name,
					ShopName = shop_name1,
					WangWang = vip_code,
					OrderMoney = decimal.Parse(payment),
					Repeat = repeat
				};
				return Json(new { State = "Success", ModelList = model1 }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { State = "" }, JsonRequestBehavior.AllowGet);
		}

		public partial class getExcels
		{
			public int ID { get; set; }
			public decimal? BackMoney { get; set; }
			public decimal? OrderMoney { get; set; }
			public decimal? price { get; set; }

			public DateTime? Retreat_date { get; set; }
			public string unit { get; set; }

			public string PostUser { get; set; }
			public string OrderNum { get; set; }
			public string VipName { get; set; }
			public string ShopName { get; set; }
			public string WangWang { get; set; }
			public string Reason { get; set; }
			public string ApproveName { get; set; }
			public string AlipayName { get; set; }
			public DateTime? PostTime { get; set; }
			public string AlipayAccount { get; set; }
			public string Note { get; set; }
			public string goods_no { get; set; }
			public string goods_name { get; set; }
			//public decimal? qty { get; set; }
			public Nullable<int> qty { get; set; }

			public string spec_name { get; set; }

			public string Repeat { get; set; }


		}

		//导出excel
		public FileResult getExcelManager(string queryStr, string statedate, string EndDate, string RetreatReason, string store)
		{
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "返现导出",
				OperateContent = string.Format("导出excel getExcelManager 条件 startDate:{0},endDate:{1},queryStr:{2},RetreatReason:{3},store:{4}", statedate, EndDate, queryStr, RetreatReason, store),
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();

			List<getExcels> queryData = null;
			int temID = 0;
			//显示当前用户的数据
			DateTime sdate = DateTime.Now.AddDays(-7);
			DateTime edate = DateTime.Now.AddDays(1);
			string name = Server.UrlDecode(Request.Cookies["Name"].Value);
			
			if (!string.IsNullOrEmpty(statedate))
			{
				sdate = Convert.ToDateTime(statedate);
			}
			if (!string.IsNullOrEmpty(EndDate))
			{
				edate = Convert.ToDateTime(EndDate).AddDays(1);
			}
			string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			//string sql = "select a.ID   from T_Retreat a  join T_RetreatAppRove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";
			#region 原导出
			//string sql = "select ISnull(Repeat,'') Repeat,a.ID,PostUser,OrderNum,VipName,ShopName,WangWang,Reason,BackMoney,ApproveName,OrderMoney,AlipayName,AlipayAccount,Note,PostTime,goods_no,goods_name,spec_name,unit,qty,price From  T_CashBack a left join  T_CashBackdetail b on  a.id =b.oid where status=1 and PostTime>='" + sdate + "' and PostTime<='" + edate + "'    ";

			//queryData = db.Database.SqlQuery<getExcels>(sql).ToList();
			//if (!string.IsNullOrEmpty(queryStr))
			//{
			//	queryData = queryData.Where(a => a.OrderNum.Contains(queryStr) || a.PostUser.Contains(queryStr) || a.VipName.Contains(queryStr) || a.AlipayAccount.Contains(queryStr)).ToList(); ;
			//}
			//if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
			//{
			//	queryData = queryData.Where(a => a.ShopName.Equals(store)).ToList();
			//}

			//if (!string.IsNullOrEmpty(RetreatReason))
			//{
			//	queryData = queryData.Where(a => a.Reason != null && a.Reason.Equals(RetreatReason)).ToList();
			//}


			//if (queryData.Count > 0)
			//{


			//	//创建Excel文件的对象
			//	NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
			//	//添加一个sheet
			//	NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
			//	//给sheet1添加第一行的头部标题
			//	NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

			//	row1.CreateCell(0).SetCellValue("申请人");
			//	row1.CreateCell(1).SetCellValue("订单编号");
			//	row1.CreateCell(2).SetCellValue("会员账号");
			//	row1.CreateCell(3).SetCellValue("店铺");
			//	row1.CreateCell(4).SetCellValue("旺旺号");
			//	row1.CreateCell(5).SetCellValue("原因");
			//	row1.CreateCell(6).SetCellValue("返现金额");
			//	row1.CreateCell(7).SetCellValue("同意人");
			//	row1.CreateCell(8).SetCellValue("订单金额");
			//	row1.CreateCell(9).SetCellValue("支付名称");
			//	row1.CreateCell(10).SetCellValue("支付账号");
			//	row1.CreateCell(11).SetCellValue("备注");
			//	row1.CreateCell(12).SetCellValue("申请时间");
			//	row1.CreateCell(13).SetCellValue("产品代码");
			//	row1.CreateCell(14).SetCellValue("产品名称");
			//	row1.CreateCell(15).SetCellValue("规格名称");
			//	row1.CreateCell(16).SetCellValue("单位");
			//	row1.CreateCell(17).SetCellValue("数量");
			//	row1.CreateCell(18).SetCellValue("单价");
			//	row1.CreateCell(19).SetCellValue("系统备注");


			//	for (int i = 0; i < queryData.Count; i++)
			//	{
			//		NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);

			//		if (temID == queryData[i].ID)
			//		{

			//			rowtemp.CreateCell(0).SetCellValue(queryData[i].PostUser.ToString());
			//			rowtemp.CreateCell(1).SetCellValue(queryData[i].OrderNum.ToString());
			//			rowtemp.CreateCell(2).SetCellValue(queryData[i].VipName.ToString());
			//			rowtemp.CreateCell(3).SetCellValue(queryData[i].ShopName.ToString());
			//			rowtemp.CreateCell(4).SetCellValue(queryData[i].WangWang.ToString());
			//			rowtemp.CreateCell(5).SetCellValue(queryData[i].Reason);
			//			// rowtemp.CreateCell(6).SetCellValue(queryData[i].BackMoney);
			//			rowtemp.CreateCell(7).SetCellValue(queryData[i].ApproveName.ToString());
			//			rowtemp.CreateCell(8).SetCellValue(queryData[i].OrderMoney.ToString());
			//			rowtemp.CreateCell(9).SetCellValue(queryData[i].AlipayName.ToString());
			//			rowtemp.CreateCell(10).SetCellValue(queryData[i].AlipayAccount.ToString());
			//			rowtemp.CreateCell(11).SetCellValue(queryData[i].Note);
			//			rowtemp.CreateCell(12).SetCellValue(queryData[i].PostTime.ToString());
			//			rowtemp.CreateCell(13).SetCellValue(queryData[i].goods_no);
			//			rowtemp.CreateCell(14).SetCellValue(queryData[i].goods_name);
			//			rowtemp.CreateCell(15).SetCellValue(queryData[i].spec_name);
			//			rowtemp.CreateCell(16).SetCellValue(queryData[i].unit);
			//			if (queryData[i].qty != null)
			//			{
			//				rowtemp.CreateCell(17).SetCellValue(queryData[i].qty.ToString());
			//			}
			//			if (queryData[i].price != null)
			//			{
			//				rowtemp.CreateCell(18).SetCellValue(queryData[i].price.ToString());
			//			}
			//			rowtemp.CreateCell(19).SetCellValue(queryData[i].Repeat);
			//		}
			//		else
			//		{
			//			temID = queryData[i].ID;
			//			rowtemp.CreateCell(0).SetCellValue(queryData[i].PostUser.ToString());
			//			rowtemp.CreateCell(1).SetCellValue(queryData[i].OrderNum.ToString());
			//			rowtemp.CreateCell(2).SetCellValue(queryData[i].VipName.ToString());
			//			rowtemp.CreateCell(3).SetCellValue(queryData[i].ShopName.ToString());
			//			rowtemp.CreateCell(4).SetCellValue(queryData[i].WangWang.ToString());
			//			rowtemp.CreateCell(5).SetCellValue(queryData[i].Reason);
			//			rowtemp.CreateCell(6).SetCellValue(queryData[i].BackMoney.ToString());
			//			rowtemp.CreateCell(7).SetCellValue(queryData[i].ApproveName.ToString());
			//			rowtemp.CreateCell(8).SetCellValue(queryData[i].OrderMoney.ToString());
			//			rowtemp.CreateCell(9).SetCellValue(queryData[i].AlipayName.ToString());
			//			rowtemp.CreateCell(10).SetCellValue(queryData[i].AlipayAccount.ToString());
			//			rowtemp.CreateCell(11).SetCellValue(queryData[i].Note);
			//			rowtemp.CreateCell(12).SetCellValue(queryData[i].PostTime.ToString());
			//			rowtemp.CreateCell(13).SetCellValue(queryData[i].goods_no);
			//			rowtemp.CreateCell(14).SetCellValue(queryData[i].goods_name);
			//			rowtemp.CreateCell(15).SetCellValue(queryData[i].spec_name);
			//			rowtemp.CreateCell(16).SetCellValue(queryData[i].unit);
			//			if (queryData[i].qty != null)
			//			{
			//				rowtemp.CreateCell(17).SetCellValue(queryData[i].qty.ToString());
			//			}
			//			if (queryData[i].price != null)
			//			{
			//				rowtemp.CreateCell(18).SetCellValue(queryData[i].price.ToString());
			//			}
			//			rowtemp.CreateCell(19).SetCellValue(queryData[i].Repeat);

			//		}
			//	}
			#endregion
			IQueryable<T_CashBack> cashBackData = db.T_CashBack.Where(a => a.PostTime > sdate && a.PostTime < edate);
			if (!string.IsNullOrEmpty(queryStr))
			{
				cashBackData = cashBackData.Where(a => a.OrderNum.Contains(queryStr) || a.PostUser.Contains(queryStr) || a.VipName.Contains(queryStr) || a.AlipayAccount.Contains(queryStr));
			}
			if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
			{
				cashBackData = cashBackData.Where(a => a.ShopName.Equals(store));
			}

			if (!string.IsNullOrEmpty(RetreatReason))
			{
				cashBackData = cashBackData.Where(a => a.Reason != null && a.Reason.Equals(RetreatReason));
			}
			List<T_CashBack> cashbackList = cashBackData.ToList();
			List<int> IDs = cashbackList.Select(a => a.ID).ToList();
			List<T_CashBackDetail> cashDetail = db.T_CashBackDetail.Where(a => IDs.Contains(a.Oid)).ToList();

			List<getExcels> list = cashbackList.Join(cashDetail, a => a.ID, b => b.Oid, (a, b) => new getExcels
			{
				AlipayAccount = a.AlipayAccount,
				ApproveName = a.ApproveName,
				AlipayName = a.AlipayName,
				BackMoney = a.BackMoney,
				goods_name = b.goods_name,
				goods_no = b.goods_no,
				price = b.price,
				qty = b.qty,
				spec_name = b.spec_name,
				unit = b.unit,
				ID = a.ID,
				Note = a.Note,
				OrderMoney = a.OrderMoney,
				OrderNum = a.OrderNum,
				PostTime = a.PostTime,
				PostUser = a.PostUser,
				
				Reason = a.Reason,
				Repeat = a.Repeat,
				Retreat_date = a.PostTime,
				ShopName = a.ShopName,
				
				VipName = a.VipName,
				WangWang = a.WangWang


			}).ToList();

			if (list.Count > 0)
			{
				//创建Excel文件的对象
				NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
				//添加一个sheet
				NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
				//给sheet1添加第一行的头部标题
				NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

				row1.CreateCell(0).SetCellValue("申请人");
				row1.CreateCell(1).SetCellValue("订单编号");
				row1.CreateCell(2).SetCellValue("会员账号");
				row1.CreateCell(3).SetCellValue("店铺");
				row1.CreateCell(4).SetCellValue("旺旺号");
				row1.CreateCell(5).SetCellValue("原因");
				row1.CreateCell(6).SetCellValue("返现金额");
				row1.CreateCell(7).SetCellValue("同意人");
				row1.CreateCell(8).SetCellValue("订单金额");
				row1.CreateCell(9).SetCellValue("支付名称");
				row1.CreateCell(10).SetCellValue("支付账号");
				row1.CreateCell(11).SetCellValue("备注");
				row1.CreateCell(12).SetCellValue("申请时间");
				row1.CreateCell(13).SetCellValue("产品代码");
				row1.CreateCell(14).SetCellValue("产品名称");
				row1.CreateCell(15).SetCellValue("规格名称");
				row1.CreateCell(16).SetCellValue("单位");
				row1.CreateCell(17).SetCellValue("数量");
				row1.CreateCell(18).SetCellValue("单价");
				row1.CreateCell(19).SetCellValue("系统备注");

				for (int i = 0; i < list.Count; i++)
				{
					NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
					if (temID == list[i].ID)
					{

						rowtemp.CreateCell(0).SetCellValue(list[i].PostUser.ToString());
						rowtemp.CreateCell(1).SetCellValue(list[i].OrderNum.ToString());
						rowtemp.CreateCell(2).SetCellValue(list[i].VipName.ToString());
						rowtemp.CreateCell(3).SetCellValue(list[i].ShopName.ToString());
						rowtemp.CreateCell(4).SetCellValue(list[i].WangWang.ToString());
						rowtemp.CreateCell(5).SetCellValue(list[i].Reason);
						// rowtemp.CreateCell(6).SetCellValue(queryData[i].BackMoney);
						rowtemp.CreateCell(7).SetCellValue(list[i].ApproveName.ToString());
						rowtemp.CreateCell(8).SetCellValue(list[i].OrderMoney.ToString());
						rowtemp.CreateCell(9).SetCellValue(list[i].AlipayName.ToString());
						rowtemp.CreateCell(10).SetCellValue(list[i].AlipayAccount.ToString());
						rowtemp.CreateCell(11).SetCellValue(list[i].Note);
						rowtemp.CreateCell(12).SetCellValue(list[i].PostTime.ToString());
						rowtemp.CreateCell(13).SetCellValue(list[i].goods_no);
						rowtemp.CreateCell(14).SetCellValue(list[i].goods_name);
						rowtemp.CreateCell(15).SetCellValue(list[i].spec_name);
						rowtemp.CreateCell(16).SetCellValue(list[i].unit);
						if (list[i].qty != null)
						{
							rowtemp.CreateCell(17).SetCellValue(list[i].qty.ToString());
						}
						if (list[i].price != null)
						{
							rowtemp.CreateCell(18).SetCellValue(list[i].price.ToString());
						}
						rowtemp.CreateCell(19).SetCellValue(list[i].Repeat);
					}
					else
					{
						temID = list[i].ID;
						rowtemp.CreateCell(0).SetCellValue(list[i].PostUser.ToString());
						rowtemp.CreateCell(1).SetCellValue(list[i].OrderNum.ToString());
						rowtemp.CreateCell(2).SetCellValue(list[i].VipName.ToString());
						rowtemp.CreateCell(3).SetCellValue(list[i].ShopName.ToString());
						rowtemp.CreateCell(4).SetCellValue(list[i].WangWang.ToString());
						rowtemp.CreateCell(5).SetCellValue(list[i].Reason);
						rowtemp.CreateCell(6).SetCellValue(list[i].BackMoney.ToString());
						rowtemp.CreateCell(7).SetCellValue(list[i].ApproveName.ToString());
						rowtemp.CreateCell(8).SetCellValue(list[i].OrderMoney.ToString());
						rowtemp.CreateCell(9).SetCellValue(list[i].AlipayName.ToString());
						rowtemp.CreateCell(10).SetCellValue(list[i].AlipayAccount.ToString());
						rowtemp.CreateCell(11).SetCellValue(list[i].Note);
						rowtemp.CreateCell(12).SetCellValue(list[i].PostTime.ToString());
						rowtemp.CreateCell(13).SetCellValue(list[i].goods_no);
						rowtemp.CreateCell(14).SetCellValue(list[i].goods_name);
						rowtemp.CreateCell(15).SetCellValue(list[i].spec_name);
						rowtemp.CreateCell(16).SetCellValue(list[i].unit);
						if (list[i].qty != null)
						{
							rowtemp.CreateCell(17).SetCellValue(list[i].qty.ToString());
						}
						if (list[i].price != null)
						{
							rowtemp.CreateCell(18).SetCellValue(list[i].price.ToString());
						}
						rowtemp.CreateCell(19).SetCellValue(list[i].Repeat);

					}
				}
				

				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				book.Write(ms);
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "返现列表导出.xls");
			}


			else
			{
				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "返现列表导出.xls");
			}
		}
		#endregion
		#region 视图
		[Description("返现首页")]
		public ActionResult Index()
		{
			return View();
		}
		[Description("新增返现申请页面")]
		public ActionResult ViewCashBackAdd()
		{
			//T_OrderList order = db.T_OrderList.Find(ID);
			//T_CashBack model = new T_CashBack
			//{
			//    OrderNum = order.platform_code,
			//    VipName = order.vip_name,
			//    ShopName = order.shop_name,
			//    WangWang = order.vip_code,
			//    OrderMoney = order.payment,
			//    OrderId = ID
			//};
			//string repeat = "";
			//T_CashBack cashBack = db.T_CashBack.FirstOrDefault(s => s.OrderNum.Equals(model.OrderNum) && s.For_Delete == 0 && s.Status != 3);
			//T_Retreat reRetreat = db.T_Retreat.FirstOrDefault(s => s.Retreat_OrderNumber.Equals(model.OrderNum) && s.Isdelete == "0" && s.Status != 3);
			//if (cashBack != null && reRetreat != null)
			//    repeat = "_订单号重复_有退款记录";
			//else if (cashBack != null)
			//    repeat = "_订单号重复";
			//else if (reRetreat != null)
			//    repeat = "_有退款记录";
			//model.Repeat = repeat;
			//if (order.Status_Retreat != 0 || order.Status_CashBack != 0)
			//    ViewData["red"] = 1;
			//else
			//    ViewData["red"] = 0;
			ViewData["Reason"] = GetReason();
			return View();
		}
		[Description("返现编辑核页面")]
		public ActionResult Edit(int ID)
		{
			T_CashBack model = db.T_CashBack.Find(ID);
			if (model == null)
			{
				return HttpNotFound();
			}
			ViewData["Reason"] = GetReason();
			ViewData["CurReason"] = model.Reason;
			return View(model);
		}
		[Description("更新返现审核配置页面")]
		public ActionResult getMethod()
		{
			return View();
		}
		[Description("订单查询页面")]
		public ActionResult ViewOrderList()
		{
			return View();
		}


		[Description("订单详情页")]
		public ActionResult ViewOrderDetail(int tid)
		{
			ViewData["ID"] = tid;
			return View();
		}
		[Description("返现列表页")]
		public ActionResult CashbackList()
		{
			ViewData["shop"] = Com.Shop();
			ViewData["resons"] = GetReason();
			return View();
		}
		[Description("返现管理页")]
		public ActionResult CashBackAdmin()
		{
			return View();
		}

		[Description("我的返现列表页")]
		public ActionResult MyCashBack()
		{
			return View();
		}
		[Description("我的返现详情页")]
		public ActionResult Detail(int ID)
		{
			T_CashBack model = db.T_CashBack.Find(ID);
			if (model == null)
			{
				return HttpNotFound();
			}
			GetApproveHistory(ID);
			return View(model);
		}
		[Description("返现未审核页")]
		public ActionResult UnCashBack()
		{
			ViewData["RetreatShop"] = App_Code.Com.Shop();
			ViewData["RetreatBackFrom"] = App_Code.Com.BackFrom();
			return View();
		}
		[Description("返现审核页面")]
		public ActionResult Check(int ID)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				T_CashBack model = db.T_CashBack.Find(ID);
				string curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);
				if (model == null)
				{
					return HttpNotFound();
				}
				int curStep = int.Parse(model.Step.ToString());
				int CashierFlag = 0;//不是财务出纳
									//获取审核列表
				GetApproveHistory(ID);
				ViewData["ID"] = ID;
				ViewData["Method"] = model.Method;
				//查询此人是否是财务出纳
				T_CashbackMethod MOD_method = db.T_CashbackMethod.FirstOrDefault(a => a.Method == model.Method && a.Step == curStep);
				if (MOD_method.Cashier == 1)
				{

					List<T_CashbackMethod> MOD_methods = db.T_CashbackMethod.Where(a => a.Name == curUser && a.Cashier == 1).ToList();
					if (MOD_methods.Count > 0)
					{
						//是财务出纳 刷新 是否返现/退款 字段
						CashierFlag = 1; //是财务出纳
						string ShopName = model.ShopName;
						ViewData["BackFrom"] = GetCashBackFrom(ShopName);
					}
				}
				ViewData["step"] = model.Step;
				ViewData["Cashier"] = CashierFlag;
				sc.Complete();
				T_CashBack MOD = db.T_CashBack.Find(ID);
				return View(MOD);
			}
		}
		[Description("返现已审核页")]
		public ActionResult CashBackED()
		{
			var list = db.T_CashBackReason.AsQueryable();
			var selectList = new SelectList(list, "Name", "Name");
			List<SelectListItem> selecli = new List<SelectListItem>();
			selecli.Add(new SelectListItem { Text = "==全部==", Value = "" });
			selecli.AddRange(selectList);
			ViewData["Reason"] = selecli;
			return View();
		}

		public ActionResult CashbackModify()
		{
			return View();
		}

		public ActionResult ViewModify(int ID = 0)
		{
			T_CashBack EditModel = db.T_CashBack.Find(ID);
			//T_CashBack aa = db.T_CashBack.wh(a => a.ID == ID);
			ViewData["Back"] = Com.BackFrom();
			if (EditModel != null)
			{
				return View(EditModel);
			}
			else
			{
				return HttpNotFound();
			}
		}
		#endregion
		#region post方法
		public JsonResult ModifySave(T_CashBack model)
		{
			try
			{
				using (TransactionScope sc = new TransactionScope())
				{
					string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
					T_CashBack editModel = db.T_CashBack.Find(model.ID);
					editModel.BackFrom = model.BackFrom;

					T_OperaterLog log = new T_OperaterLog();
					log.Module = "返现";
					log.OperateContent = "修改出款账号";
					log.Operater = name;
					log.OperateTime = DateTime.Now;
					log.PID = model.ID;
					db.T_OperaterLog.Add(log);

					db.SaveChanges();
					sc.Complete();
					return Json(new { State = "Success", Message = "保存成功" });
				}

			}
			catch (Exception e)
			{
				return Json(new { State = "Fail", Message = "保存失败" });
			}
		}
		//获取查询订单列表
		[HttpPost]
		public ContentResult GetOrder(Lib.GridPager pager, string queryStr)
		{

			IQueryable<T_OrderList> queryData = db.T_OrderList.Where(a => a.platform_code.Contains(queryStr));
			pager.totalRows = queryData.Count();
			//分页
			queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_OrderList> list = new List<T_OrderList>();
			foreach (var item in queryData)
			{
				T_OrderList i = new T_OrderList();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//查看订单详情列表  
		[HttpPost]
		public ContentResult GetDetail(Lib.GridPager pager, int id, string queryStr)
		{
			T_OrderList mod = db.T_OrderList.Find(id);
			string sCode = mod.code;
			IQueryable<T_OrderDetail> queryData = db.T_OrderDetail.Where(a => a.oid == sCode);
			pager.totalRows = queryData.Count();
			//分页
			queryData = queryData.OrderByDescending(c => c.id).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_OrderDetail> list = new List<T_OrderDetail>();
			foreach (var item in queryData)
			{
				T_OrderDetail i = new T_OrderDetail();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//返现新增保存
		[HttpPost]
		public JsonResult AddSave(T_CashBack model, string jsonStr)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				//主表保存
				model.AlipayAccount = model.AlipayAccount.Trim();
				model.AlipayName = model.AlipayName.Trim();
				model.For_Delete = 0;
				model.PostTime = DateTime.Now;
				model.Step = 0;
				model.Status = -1;//初始化审核状态      未审核:-1  审核中：0  同意：1  不同意：2   作废：3   
				string _Reason = model.Reason;//返现的理由
				string _Shop = model.ShopName;//返现店铺
				int _Money = 0; //返现金额  大于等于100：1  小于100: 0
				string departmentName = "";//部门
										   //查看提交者是否为售后部门
				T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == model.PostUser);
				if (MOD_User != null)
				{
					string departId = MOD_User.DepartmentId;
					int dID = int.Parse(departId);
					T_Department MOD_Department = db.T_Department.Find(dID);

				}
				else
				{
					return Json(new { State = "Faile", Message = "保存失败,该部门可能不存在" }, JsonRequestBehavior.AllowGet);
				}
				//判断金额大小
				if (model.BackMoney >= 100)
				{
					_Money = 1;
				}
				else if (model.BackMoney > 10)
				{
					_Money = 2;
				}
				else
				{
					_Money = 0;
				}
				string repeat = "";
				List<T_CashBack> cash = db.T_CashBack.Where(a => a.OrderNum.Equals(model.OrderNum) && a.For_Delete == 0).ToList();
				if (cash.Count > 0)
				{
					repeat += "_存在返现记录重复，";
				}
				List<T_Reissue> Reissue = db.T_Reissue.Where(a => a.OrderCode.Equals(model.OrderNum) && a.IsDelete == 0).ToList();
				if (Reissue.Count > 0)
				{
					repeat += "_存在补发记录重复，";
				}
				List<T_Retreat> retreat = db.T_Retreat.Where(a => a.Retreat_OrderNumber.Equals(model.OrderNum) && a.Isdelete == "0").ToList();
				if (retreat.Count > 0)
				{
					repeat += "_存在退货退款记录重复，";
				}
				model.BackMoney = Math.Round(model.BackMoney, 2, MidpointRounding.AwayFromZero);
				model.Repeat = repeat;
				db.T_CashBack.Add(model);
				int i = db.SaveChanges();
				if (i > 0)
				{
					List<T_CashBackDetail> details = App_Code.Com.Deserialize<T_CashBackDetail>(jsonStr);
					if (details != null && details.Count > 0)
					{
						foreach (var item in details)
						{
							item.Oid = model.ID;
							db.T_CashBackDetail.Add(item);
							db.SaveChanges();
						}
					}



					//主表保存成功后 插入审核表
					string CurUserId = MOD_User.ID.ToString();
					T_Department Sale_Department = db.T_Department.FirstOrDefault(a => a.Name == "售前部门");
					T_Department Department_Sale = db.T_Department.FirstOrDefault(a => a.Name == "售后部门");
					//售前部门的ID数组
					string[] ids1 = Sale_Department.employees.Split(',');

					//售后部门的人员ID数组
					string[] ids2 = Department_Sale.employees.Split(',');

					//判断
					if (ids1.Contains(CurUserId))
					{
						departmentName = "售前";
					}
					else if (ids2.Contains(CurUserId))
					{
						departmentName = "售后";
					}
					else
					{
						departmentName = "其他";
					}
					//寻找审批方法
					T_CashbackApproveConfig MOD_Config = db.T_CashbackApproveConfig.FirstOrDefault(a => a.Reason == _Reason && a.Money == _Money && a.Shop == _Shop && a.Roles == departmentName);
					if (MOD_Config != null)
					{
						if (MOD_Config.Method == null)
						{
							return Json(new { State = "Faile", Message = "保存失败,请联系店铺负责人添加审核流程" }, JsonRequestBehavior.AllowGet);
						}
						int _Method = int.Parse(MOD_Config.Method.ToString());
						//查找第一个审核人
						T_CashbackMethod MOD_Method = db.T_CashbackMethod.FirstOrDefault(a => a.Method == _Method && a.Step == 0);
						if (MOD_Method != null)
						{
							string firstApprove = MOD_Method.Name;
							//审核记录表添加一条新的审核数据
							T_CashBackApprove MOD_Approve = new T_CashBackApprove();
							MOD_Approve.ApproveName = firstApprove;
							MOD_Approve.Status = -1;
							MOD_Approve.Note = "";
							MOD_Approve.Oid = model.ID;
							db.T_CashBackApprove.Add(MOD_Approve);
							db.SaveChanges();
							//主记录插入一个方法的值
							T_CashBack MOD_Cashback = db.T_CashBack.Find(model.ID);
							MOD_Cashback.Method = int.Parse(MOD_Method.Method.ToString());
							db.Entry<T_CashBack>(MOD_Cashback).State = System.Data.Entity.EntityState.Modified;
							db.SaveChanges();
						}
						else
						{
							return Json(new { State = "Faile", Message = "保存失败,未找到审核人" }, JsonRequestBehavior.AllowGet);
						}
					}
					else
					{
						return Json(new { State = "Faile", Message = "保存失败,请联系店铺负责人添加审核流程" }, JsonRequestBehavior.AllowGet);
					}
				}

				//ModularByZP();

				sc.Complete();
				return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
			}
		}
		public partial class Modular
		{

			public string ModularName { get; set; }
			public int NotauditedNumber { get; set; }
			public string PendingAuditName { get; set; }
		}
		//返现列表查询
		[HttpPost]
		public ContentResult GetCahsbackList(Lib.GridPager pager, string queryStr, int status, string startSendTime, string endSendTime, string store, string reson)
		{

			//查询出所有未删除的
			IQueryable<T_CashBack> queryData = db.T_CashBack.Where(a => a.For_Delete == 0);
			//根据订单号，提交者姓名，会员号码查询
			if (!string.IsNullOrEmpty(queryStr))
			{
				queryData = queryData.Where(a => a.OrderNum.Equals(queryStr) || a.PostUser.Equals(queryStr) || a.VipName.Equals(queryStr) || a.AlipayAccount.Equals(queryStr));
			}
			//根据状态查询
			if (status != 9999)
			{
				queryData = queryData.Where(a => a.Status == status);
			}
			if (!string.IsNullOrWhiteSpace(store) && !store.Equals("==请选择=="))
			{
				queryData = queryData.Where(a => a.ShopName.Equals(store));
			}
			if (!string.IsNullOrWhiteSpace(reson))
			{
				queryData = queryData.Where(a => a.Reason.Equals(reson));
			}
			//根据日期查询
			if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
			{

				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
			}
			else if (!string.IsNullOrWhiteSpace(startSendTime))
			{
				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = startTime.AddDays(5);
				queryData = queryData.Where(s => s.PostTime >= startTime);
			}
			else if (!string.IsNullOrWhiteSpace(endSendTime))
			{
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				DateTime startTime = endTime.AddDays(-5);
				queryData = queryData.Where(s => s.PostTime <= endTime);
			}
			//分页
			pager.totalRows = queryData.Count();
			queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_CashBack> list = new List<T_CashBack>();
			foreach (var item in queryData)
			{
				T_CashBack i = new T_CashBack();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//我的返现列表
		[HttpPost]
		public ContentResult GetMyCahsbackList(Lib.GridPager pager, string queryStr, int status, string startSendTime, string endSendTime)
		{
			//当前用户
			string curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			//查询出所有未删除的
			IQueryable<T_CashBack> queryData = db.T_CashBack.Where(a => a.For_Delete == 0 && a.PostUser == curUser);
			//根据订单号，提交者姓名，会员号码查询
			if (!string.IsNullOrEmpty(queryStr))
			{
				queryData = queryData.Where(a => a.OrderNum.Equals(queryStr) || a.VipName.Equals(queryStr));
			}
			//根据状态查询
			if (status != 9999)
			{
				queryData = queryData.Where(a => a.Status == status);
			}
			//根据日期查询
			if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
			{

				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
			}
			else if (!string.IsNullOrWhiteSpace(startSendTime))
			{
				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = startTime.AddDays(5);
				queryData = queryData.Where(s => s.PostTime >= startTime);
			}
			else if (!string.IsNullOrWhiteSpace(endSendTime))
			{
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				DateTime startTime = endTime.AddDays(-5);
				queryData = queryData.Where(s => s.PostTime <= endTime);
			}
			//分页
			pager.totalRows = queryData.Count();
			queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_CashBack> list = new List<T_CashBack>();
			foreach (var item in queryData)
			{
				T_CashBack i = new T_CashBack();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//未审核返现列表
		[HttpPost]
		public ContentResult GetUnCheckList(Lib.GridPager pager, string queryStr, string RetreatShop, string startSendTime, string endSendTime, string RetreatBackFrom, string FiveDollars)
		{
			//当前用户
			string curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);

			List<T_CashbackMethod> MethodModel = db.T_CashbackMethod.Where(a => a.Name == curUser && a.Cashier == 1).ToList();
			List<T_CashBackApprove> ApproveMod = new List<T_CashBackApprove>();
			if (MethodModel.Count > 0)
			{
				List<T_CashbackMethod> CashbackMethod = db.T_CashbackMethod.Where(a => a.Cashier == 1).ToList();

				string Name = "";
				for (int i = 0; i < CashbackMethod.Count; i++)
				{
					if (i == 0)
					{
						Name += "'" + CashbackMethod[i].Name + "'";
					}
					else
					{
						Name += ",'" + CashbackMethod[i].Name + "'";
					}
				}

				ApproveMod = db.Database.SqlQuery<T_CashBackApprove>("select * from  T_CashBackApprove where ApproveTime is null and Status='-1' and ApproveName in (" + Name + ") ").ToList();
			}
			else
			{

				ApproveMod = db.T_CashBackApprove.Where(a => a.ApproveName == curUser && a.ApproveTime == null && a.Status == -1).ToList();

			}

			//当前用户未审核的数据
			int[] Arry = new int[ApproveMod.Count];
			for (int i = 0; i < ApproveMod.Count; i++)
			{
				Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
			}
			IQueryable<T_CashBack> queryData = from r in db.T_CashBack
											   where Arry.Contains(r.ID) && r.For_Delete == 0 && (r.Status == -1 || r.Status == 0 || r.Status == 2)
											   select r;
			//根据订单号，提交者姓名，会员号码查询
			if (!string.IsNullOrEmpty(queryStr))
			{
				queryData = queryData.Where(a => a.OrderNum.Equals(queryStr) || a.PostUser.Equals(queryStr) || a.VipName.Equals(queryStr));
			}
			if (RetreatShop != null && RetreatShop != "")
			{
				queryData = queryData.Where(a => a.ShopName == RetreatShop);
			}
			if (RetreatBackFrom != null && RetreatBackFrom != "")
			{
				List<T_CashBackFrom> CashBackFromList = db.T_CashBackFrom.Where(a => a.Name == RetreatBackFrom && a.IsBlending == "1").ToList();

				string dianp = "";
				for (int i = 0; i < CashBackFromList.Count; i++)
				{
					if (i == 0)
					{
						dianp += CashBackFromList[i].ShopName;
					}
					else
					{
						dianp += ",'" + CashBackFromList[i].ShopName + "'";
					}

				}
				queryData = queryData.Where(a => dianp.Contains(a.ShopName));


			}
			//根据日期查询
			if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
			{

				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = DateTime.Parse(endSendTime);
				queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
			}
			else if (!string.IsNullOrWhiteSpace(startSendTime))
			{
				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = startTime.AddDays(5);
				queryData = queryData.Where(s => s.PostTime >= startTime);
			}
			else if (!string.IsNullOrWhiteSpace(endSendTime))
			{
				DateTime endTime = DateTime.Parse(endSendTime);
				DateTime startTime = endTime.AddDays(-5);
				queryData = queryData.Where(s => s.PostTime <= endTime);
			}
			if (FiveDollars != "" && FiveDollars != null)
			{
				if (FiveDollars == "大5")
				{
					queryData = queryData.Where(s => s.BackMoney >= 5);
				}
				else
				{
					queryData = queryData.Where(s => s.BackMoney <= 5);
				}
			}
			//分页
			pager.totalRows = queryData.Count();
			queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_CashBack> list = new List<T_CashBack>();
			foreach (var item in queryData)
			{
				T_CashBack i = new T_CashBack();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//返现已审核
		[HttpPost]
		public ContentResult GetCheckEDList(Lib.GridPager pager, string queryStr, string startSendTime, string endSendTime, string Repeat, string Reason)
		{
			//当前用户
			string curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);


			List<T_CashbackMethod> MethodModel = db.T_CashbackMethod.Where(a => a.Name == curUser && a.Cashier == 1).ToList();
			List<T_CashBackApprove> ApproveMod = new List<T_CashBackApprove>();
			if (MethodModel.Count > 0)
			{
				List<T_CashbackMethod> CashbackMethod = db.T_CashbackMethod.Where(a => a.Cashier == 1).ToList();

				string Name = "";
				for (int i = 0; i < CashbackMethod.Count; i++)
				{
					if (i == 0)
					{
						Name += "'" + CashbackMethod[i].Name + "'";
					}
					else
					{
						Name += ",'" + CashbackMethod[i].Name + "'";
					}
				}

				ApproveMod = db.Database.SqlQuery<T_CashBackApprove>("select * from  T_CashBackApprove where ApproveTime is null and Status='-1' and ApproveName in (" + Name + ") ").ToList();
			}
			else
			{

				ApproveMod = db.T_CashBackApprove.Where(a => a.ApproveName == curUser && a.ApproveTime != null).ToList();

			}
			//当前用户未审核的数据

			int[] Arry = new int[ApproveMod.Count];
			for (int i = 0; i < ApproveMod.Count; i++)
			{
				Arry[i] = int.Parse(ApproveMod[i].Oid.ToString());
			}
			IQueryable<T_CashBack> queryData = from r in db.T_CashBack
											   where Arry.Contains(r.ID) && r.For_Delete == 0 && r.Status != -1
											   select r;
			//根据订单号，提交者姓名，会员号码查询
			if (!string.IsNullOrEmpty(queryStr))
			{
				queryData = queryData.Where(a => a.OrderNum.Contains(queryStr) || a.PostUser.Contains(queryStr) || a.VipName.Contains(queryStr));
			}
			if (!string.IsNullOrWhiteSpace(startSendTime))
			{
				DateTime start = DateTime.Parse(startSendTime + " 00:00:00");
				queryData = queryData.Where(s => s.PostTime >= start);
			}
			if (!string.IsNullOrWhiteSpace(endSendTime))
			{
				DateTime end = DateTime.Parse(endSendTime + " 23:59:59");
				queryData = queryData.Where(s => s.PostTime <= end);
			}
			if (!string.IsNullOrEmpty(Repeat))
			{
				queryData = queryData.Where(a => a.Repeat == Repeat);
			}
			if (!string.IsNullOrEmpty(Reason))
			{
				queryData = queryData.Where(a => a.Reason == Reason);
			}
			//根据日期查询
			if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
			{

				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				queryData = queryData.Where(s => s.PostTime >= startTime && s.PostTime <= endTime);
			}
			else if (!string.IsNullOrWhiteSpace(startSendTime))
			{
				DateTime startTime = DateTime.Parse(startSendTime);
				DateTime endTime = startTime.AddDays(5);
				queryData = queryData.Where(s => s.PostTime >= startTime);
			}
			else if (!string.IsNullOrWhiteSpace(endSendTime))
			{
				DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
				DateTime startTime = endTime.AddDays(-5);
				queryData = queryData.Where(s => s.PostTime <= endTime);
			}
			//分页
			pager.totalRows = queryData.Count();
			queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
			List<T_CashBack> list = new List<T_CashBack>();
			foreach (var item in queryData)
			{
				T_CashBack i = new T_CashBack();
				i = item;
				list.Add(i);
			}
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
			return Content(json);
		}
		//删除返现
		public JsonResult Del(int ID)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				T_CashBack MOD = db.T_CashBack.Find(ID);
				if (MOD == null)
				{
					return Json("找不到该记录", JsonRequestBehavior.AllowGet);
				}
				MOD.For_Delete = 1;
				db.Entry<T_CashBack>(MOD).State = System.Data.Entity.EntityState.Modified;
				int i = db.SaveChanges();
				////查询是否还有该订单返现数据
				//List<T_CashBack> modelList = db.T_CashBack.Where(a => a.OrderNum == MOD.OrderNum && a.Status != 3 && a.For_Delete == 0).ToList();//除了该订单本身是否还存在没有作废的订单，如果
				//if (modelList.Count == 0)
				//{
				//    //修改订单字段 
				//    T_OrderList MOD_Order = db.T_OrderList.Find(MOD.OrderId);
				//    MOD_Order.Status_CashBack = 0;
				//    db.Entry<T_OrderList>(MOD_Order).State = System.Data.Entity.EntityState.Modified;
				//    int o = db.SaveChanges();
				//    if (o == 0)
				//    {
				//        return Json("删除失败", JsonRequestBehavior.AllowGet);
				//    }
				//}
				string result = "";
				if (i > 0)
				{

					result = "删除成功";
				}
				else
				{
					result = "删除失败";
				}

				// ModularByZP();

				sc.Complete();
				return Json(result, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult Handle(int ID)
		{
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			if (Nickname != "半夏")
			{
				return Json("没有权限,请找半夏", JsonRequestBehavior.AllowGet);
			}
			using (TransactionScope sc = new TransactionScope())
			{
				try
				{
					T_CashBack MOD = db.T_CashBack.Find(ID);
					if (MOD == null)
					{
						return Json("找不到该记录", JsonRequestBehavior.AllowGet);
					}
					MOD.Status = 2;
					MOD.Step = 0;
					MOD.BackFrom = null;
					db.Entry<T_CashBack>(MOD).State = System.Data.Entity.EntityState.Modified;
					int i = db.SaveChanges();
					//查询审核记录
					int maxId = Convert.ToInt32(db.T_CashBackApprove.Where(a => a.Oid == ID).Max(a => a.ID));
					T_CashBackApprove modelList = db.T_CashBackApprove.SingleOrDefault(a => a.ID == maxId);//除了该订单本身是否还存在没有作废的订单，如果
					if (modelList == null)
					{
						return Json("撤回失败", JsonRequestBehavior.AllowGet);
					}
					else
					{
						modelList.Status = 2;
						modelList.ApproveTime = null;
						db.Entry<T_CashBackApprove>(modelList).State = System.Data.Entity.EntityState.Modified;
						i = db.SaveChanges();
					}
					string result = "";
					if (i > 0)
					{

						result = "撤回成功";
						T_OperaterLog log = new T_OperaterLog();
						log.Module = "返现";
						log.OperateContent = "操作处理的驳回" + MOD.OrderNum;
						log.Operater = Nickname;
						log.OperateTime = DateTime.Now;
						log.PID = MOD.ID;
						db.T_OperaterLog.Add(log);
						db.SaveChanges();
					}
					else
					{
						result = "撤回失败";
					}
					//ModularByZP();
					ding(MOD.OrderNum, MOD.BackMoney);
					sc.Complete();
					return Json(result, JsonRequestBehavior.AllowGet);
				}
				catch (Exception ex)
				{
					return Json(ex, JsonRequestBehavior.AllowGet);
				}


			}
		}
		//获取好护士token
		private string GetToken()
		{
			string url = "https://oapi.dingtalk.com/gettoken?corpid=ding2d039a809b22b5dc&corpsecret=ixiCpMGOiSCFzZ7pmCoKIq2r0QxIhY6eyuJ-0UKGx_WKtzE3UWDK6R9n7F_S3WtA";
			string ret = GY.HttpGet(url, "");
			JsonData jsonData = null;
			jsonData = JsonMapper.ToObject(ret);
			if (jsonData.Count == 4)
				return jsonData["access_token"].ToString();
			else
			{
				return "错误";
			}
		}
		public void ding(string order, decimal money)
		{
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);

			//string tel = "13873155482";
			string number = "manager6456";//031365626621881757
			string cmd = "";

			string token = GetToken();
			string url = "https://oapi.dingtalk.com/message/send?access_token=" + token;//好护士
			string neir = "EBMS:" + Nickname + ":撤回返现已同意数据" + order + ",金额：" + money + "元,请知悉";
			cmd = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"text\",\"agentid\":\"97171864\"," +
				"\"text\":{ \"content\":\" " + neir + " \"} }";//好护士text
			string cmdss = "";
			string cmdoas = "";
			string ur = "http://ebms.hhs16.com/";
			string urr = "dingtalk://dingtalkclient/page/link?url=http%3a%2f%2febms.hhs16.com%3fpc_slide%3dfalse";

			cmdss = "{ \"touser\":\"" + number + "\",\"toparty\":\"\",\"msgtype\":\"link\",\"agentid\":\"97171864\"," +
				"\"link\":{\"messageUrl\": \"http://ebms.hhs16.com\"," +
								"\"picUrl\": \"@lALOACZwe2Rk\"," +
								" \"title\": \"EBMS审批处理提醒\"," +
								"\"text\": \"" + neir + "\"" +
							"}}";//好护士link
			cmdoas = "{ \"touser\":\"" + number + "\"," +
						"\"toparty\":\"\"," +
						"\"msgtype\":\"oa\"," +
						"\"agentid\":\"97171864\"," +
						"\"oa\":{" +
						"\"message_url\": \"" + ur + "\"," +
						"\"pc_message_url\": \"" + urr + " \"," +
							"\"head\": {" +
							"\"bgcolor\": \"FFFF9900\"," +
							"\"text\": \"EBMS审批处理提醒\"}," +
						"\"body\": {" +
						"\"title\": \"EBMS审批处理提醒\"," +
						"\"content\": \"" + neir + "\"," +
						"\"author\": \"ebms.admin\"}}}";//好护士oa
			string ret = GY.DoPosts(url, cmd, Encoding.UTF8, "json");//好护士
		}
		public void ModularByZP()
		{
			List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "返现").ToList();
			if (ModularNotaudited.Count > 0)
			{
				foreach (var item in ModularNotaudited)
				{
					db.T_ModularNotaudited.Remove(item);
				}
				db.SaveChanges();
			}

			string RetreatAppRoveSql = "  select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_CashBackApprove where  Oid in ( select ID from T_CashBack where For_Delete=0 and ( Status=0 or Status=-1) ) and  Status=-1 and ApproveTime is null GROUP BY ApproveName  ";
			List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
			{
				string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

				T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "返现" && a.PendingAuditName == PendingAuditName);
				if (NotauditedModel != null)
				{
					NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
					db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
				}
				else
				{
					T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
					ModularNotauditedModel.ModularName = "返现";
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
			string RejectNumberSql = "select PostUser as PendingAuditName,COUNT(*) as NotauditedNumber from T_CashBack where Status='2' and  For_Delete='0' GROUP BY PostUser ";
			List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

			for (int e = 0; e < RejectNumberQuery.Count; e++)
			{
				string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

				T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "返现" && a.PendingAuditName == PendingAuditName);
				if (NotauditedModel != null)
				{
					NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
					db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
				}
				else
				{
					T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
					ModularNotauditedModel.ModularName = "返现";
					ModularNotauditedModel.NotauditedNumber = 0;
					ModularNotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
					ModularNotauditedModel.PendingAuditName = RejectNumberQuery[e].PendingAuditName;
					ModularNotauditedModel.ToupdateDate = DateTime.Now;
					ModularNotauditedModel.ToupdateName = Nickname;
					db.T_ModularNotaudited.Add(ModularNotauditedModel);
				}
				db.SaveChanges();
			}
		}
		//返现作废
		public JsonResult Void(int ID)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				T_CashBack MOD = db.T_CashBack.Find(ID);
				if (MOD == null)
				{
					return Json("找不到该记录", JsonRequestBehavior.AllowGet);
				}
				MOD.Status = 3;
				db.Entry<T_CashBack>(MOD).State = System.Data.Entity.EntityState.Modified;
				int i = db.SaveChanges();
				////查询是否还有该订单返现数据
				//List<T_CashBack> modelList = db.T_CashBack.Where(a => a.OrderNum == MOD.OrderNum && a.Status != 3 && a.For_Delete == 0).ToList();
				//if (modelList.Count == 0)
				//{
				//    //修改订单字段 
				//    T_OrderList MOD_Order = db.T_OrderList.Find(MOD.OrderId);
				//    MOD_Order.Status_CashBack = 0;
				//    db.Entry<T_OrderList>(MOD_Order).State = System.Data.Entity.EntityState.Modified;
				//    int o = db.SaveChanges();
				//    if (o == 0)
				//    {
				//        return Json("作废失败", JsonRequestBehavior.AllowGet);
				//    }
				//}
				string result = "";
				if (i > 0)
				{
					result = "作废成功";
				}
				else
				{
					result = "作废失败";
				}
				//ModularByZP();
				sc.Complete();
				return Json(result, JsonRequestBehavior.AllowGet);
			}
		}
		//审核 
		public JsonResult CashBackCheck(int id, int status, string memo, string method, int cashier, string backfrom, int step)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				//当前用户
				string curUser = Server.UrlDecode(Request.Cookies["Nickname"].Value);
				//审核主记录
				T_CashBack MOD_Cash = db.T_CashBack.Find(id);
				if (MOD_Cash.Step != step)
				{
					return Json("流程已更改请刷新界面", JsonRequestBehavior.AllowGet);
				}
				if (MOD_Cash.Method != Convert.ToInt32(method))
				{
					return Json("审核方法已更改请刷新界面", JsonRequestBehavior.AllowGet);
				}
				if (MOD_Cash == null)
				{
					return Json("找不到该记录", JsonRequestBehavior.AllowGet);
				}
				T_CashBackApprove cur_Approve = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
				if (cashier == 1)
				{
					if ((backfrom == "" || backfrom == null) && status == 1)
					{
						return Json("出纳必须填写支付帐号,如未配置请联系管理员", JsonRequestBehavior.AllowGet);
					}
				}
				else
				{
					if (cur_Approve.ApproveName != curUser)
						return Json("当前不是你审核，或者你的帐号在别处登录了", JsonRequestBehavior.AllowGet);
				}
				int _Method = int.Parse(MOD_Cash.Method.ToString());       //当前审核方法
				int _Step = int.Parse(MOD_Cash.Step.ToString());           //当前审核步骤

				if (cur_Approve == null)
					return Json("该记录已审核", JsonRequestBehavior.AllowGet);

				List<T_CashbackMethod> MOD_Method = db.T_CashbackMethod.Where(a => a.Method == _Method).ToList();
				int methodLength = MOD_Method.Count();  //该方法总步骤数
														//修改审核记录
				T_CashBackApprove MOD_Approve = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == id && a.ApproveTime == null);
				MOD_Approve.Status = status;
				MOD_Approve.Note = memo;
				MOD_Approve.ApproveTime = DateTime.Now;
				MOD_Approve.ApproveName = curUser;
				db.Entry<T_CashBackApprove>(MOD_Approve).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();
				//同意
				if (status == 1)
				{
					//不是最后一步
					if (_Step < methodLength - 1)
					{
						_Step++;  //步骤加1
						T_CashbackMethod MOD_Nextapprove = db.T_CashbackMethod.FirstOrDefault(a => a.Method == _Method && a.Step == _Step);//当前流程的步骤
																																		   //审核同意而且,插入新的审核记录 
						T_CashBackApprove CashApprove = new T_CashBackApprove();
						CashApprove.Oid = id;
						CashApprove.Note = "";
						CashApprove.Status = -1;
						CashApprove.ApproveName = MOD_Nextapprove.Name;
						db.T_CashBackApprove.Add(CashApprove);
						db.SaveChanges();
						//主表状态为0 =>审核中
						MOD_Cash.Status = 0;
					}
					else
					{
						//最后一步 修改主表的状态
						MOD_Cash.Status = status;
					}
					//判断是否是出纳，出纳返现资金 
					if (cashier == 1)
					{
						//主表支出帐号
						MOD_Cash.BackFrom = backfrom;
						//修改订单
						//查询是否还有该订单返现数据
						//修改订单字段 
						//T_OrderList MOD_Order = db.T_OrderList.Find(MOD_Cash.OrderId);
						//if (MOD_Order.Status_CashBack != 2)
						//{
						//    MOD_Order.Status_CashBack = 2;
						//    db.Entry<T_OrderList>(MOD_Order).State = System.Data.Entity.EntityState.Modified;
						//    int o = db.SaveChanges();
						//    if (o == 0)
						//    {
						//        return Json("审核失败", JsonRequestBehavior.AllowGet);
						//    }
						//}
					}
				}
				if (status == 2)
				{
					//不同意
					MOD_Cash.Status = status;
					_Step = 0;
					//审核流程结束 申请人编辑后插入下一条记录 
					//驳回 发短信
					T_User user = db.T_User.FirstOrDefault(a => a.Nickname == MOD_Cash.PostUser);
					if (user != null)
					{
						if (!string.IsNullOrEmpty(user.Tel))
						{
							string[] msg = new string[] { "返现", "不同意" };
							string res = Lib.SendSMS.Send(msg, user.Tel);
						}
					}
				}

				MOD_Cash.Step = _Step;
				db.Entry<T_CashBack>(MOD_Cash).State = System.Data.Entity.EntityState.Modified;   //修改主表
				int i = db.SaveChanges();
				string result = "审核失败";
				if (i > 0)
				{
					result = "审核成功";
				}
				// ModularByZP();
				sc.Complete();
				return Json(result, JsonRequestBehavior.AllowGet);
			}
		}
		//编辑保存
		[HttpPost]
		public JsonResult EditSave(T_CashBack model)
		{
			using (TransactionScope sc = new TransactionScope())
			{
				//保存主表
				int _Money = 0;
				string _Shop = model.ShopName;
				string _Reason = model.Reason;
				string departmentName = "";//部门
										   //查看提交者是否为售后部门
				T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == model.PostUser);
				if (MOD_User != null)
				{
					string departId = MOD_User.DepartmentId;
					int dID = int.Parse(departId);
					T_Department MOD_Department = db.T_Department.Find(dID);

				}
				else
				{
					return Json("保存失败,该部门可能不存在", JsonRequestBehavior.AllowGet);
				}
				//判断金额大小
				if (model.BackMoney >= 100)
				{
					_Money = 1;
				}
				else if (model.BackMoney > 10)
				{
					_Money = 2;
				}
				else
				{
					_Money = 0;
				}
				//主表保存成功后 插入审核表
				string CurUserId = MOD_User.ID.ToString();
				T_Department Sale_Department = db.T_Department.FirstOrDefault(a => a.Name == "售前部门");
				T_Department Department_Sale = db.T_Department.FirstOrDefault(a => a.Name == "售后部门");
				//售前部门的ID数组
				string[] ids1 = Sale_Department.employees.Split(',');

				//售后部门的人员ID数组
				string[] ids2 = Department_Sale.employees.Split(',');

				//判断
				if (ids1.Contains(CurUserId))
				{
					departmentName = "售前";
				}
				else if (ids2.Contains(CurUserId))
				{
					departmentName = "售后";
				}
				else
				{
					departmentName = "其他";
				}
				//寻找审批方法
				T_CashbackApproveConfig MOD_Config = db.T_CashbackApproveConfig.FirstOrDefault(a => a.Reason == _Reason && a.Money == _Money && a.Shop == _Shop && a.Roles == departmentName);
				string firstName = "";
				if (MOD_Config != null)
				{
					if (MOD_Config.Method == null)
					{
						return Json("保存失败,请联系店铺负责人添加审核流程", JsonRequestBehavior.AllowGet);
					}
					int _Method = int.Parse(MOD_Config.Method.ToString());
					model.Method = _Method;//主表的方法修改
										   //查找第一个审核人
					T_CashbackMethod MOD_Method = db.T_CashbackMethod.FirstOrDefault(a => a.Method == _Method && a.Step == 0);
					if (MOD_Method != null)
					{
						firstName = MOD_Method.Name;

					}
					else
					{
						return Json("保存失败", JsonRequestBehavior.AllowGet);
					}
				}
				else
				{
					return Json("保存失败,请联系店铺负责人添加审核流程", JsonRequestBehavior.AllowGet);
				}
				if (model.Status == -1)
				{
					//新增修改的
					//审核记录修改
					T_CashBackApprove MOD_Approve = db.T_CashBackApprove.FirstOrDefault(a => a.Oid == model.ID && a.ApproveTime == null);
					if (MOD_Approve == null)
					{
						return Json("保存失败", JsonRequestBehavior.AllowGet);
					}
					MOD_Approve.ApproveName = firstName;
					db.Entry<T_CashBackApprove>(MOD_Approve).State = System.Data.Entity.EntityState.Modified;
					db.SaveChanges();
				}
				else if (model.Status == 2)
				{
					//不同意修改的
					//新增一条审核记录
					T_CashBackApprove MOD_ApproveNew = new T_CashBackApprove();
					MOD_ApproveNew.Oid = model.ID;
					MOD_ApproveNew.Note = "";
					MOD_ApproveNew.Status = -1;
					MOD_ApproveNew.ApproveName = firstName;
					db.T_CashBackApprove.Add(MOD_ApproveNew);
					db.SaveChanges();
					// 修改后主表审核状态改为审核中
					model.Status = 0;
				}
				else
				{
					return Json("当前状态不能编辑,请刷新页面", JsonRequestBehavior.AllowGet);
				}
				db.Entry<T_CashBack>(model).State = System.Data.Entity.EntityState.Modified;
				db.SaveChanges();

				// ModularByZP();

				sc.Complete();
				return Json("保存成功", JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// 返现已审核导出
		/// </summary>
		/// <param name="queryStr"></param>
		/// <param name="statedate"></param>
		/// <param name="EndDate"></param>
		/// <returns></returns>
		public FileResult getExcel(string queryStr, string statedate, string EndDate, string reason)
		{
			string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			T_OperaterLog log = new T_OperaterLog()
			{
				Module = "返现导出",
				OperateContent = string.Format("导出excel getExcel 条件 startDate:{0},endDate:{1},queryStr:{2},reason:{3}", statedate, EndDate, queryStr, reason),
				Operater = Nickname,
				OperateTime = DateTime.Now,
				PID = -1
				//"导出excel：query:" + query+ "orderType:" + orderType+ my+ startDate+ endDate+ RetreatReason
			};
			db.T_OperaterLog.Add(log);
			db.SaveChanges();
			IQueryable<int> queryData = null;
			//显示当前用户的数据
			string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
			string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
			if (!string.IsNullOrEmpty(statedate))
			{
				sdate = statedate + " 00:00:00";
			}
			if (!string.IsNullOrEmpty(EndDate))
			{
				edate = EndDate + " 23:59:59";
			}
			string user = Server.UrlDecode(Request.Cookies["Nickname"].Value);
			//  string sql = "select a.Id   from T_CashBack a  join T_CashBackApprove b on b.ApproveTime>='" + sdate + "' and b.ApproveTime<='" + edate + "' and a.ID=b.Oid and b.[Status]=1 and b.ApproveName='" + user + "'  and b.ApproveTime is not null";

			string sql = "";
			string Name = "";
			List<T_CashbackMethod> MethodModel = db.T_CashbackMethod.Where(a => a.Name == user && a.Cashier == 1).ToList();
			List<T_CashBackApprove> ApproveMod = new List<T_CashBackApprove>();
			if (MethodModel.Count > 0)
			{
				List<T_CashbackMethod> CashbackMethod = db.T_CashbackMethod.Where(a => a.Cashier == 1).ToList();


				for (int i = 0; i < CashbackMethod.Count; i++)
				{
					if (i == 0)
					{
						Name += "'" + CashbackMethod[i].Name + "'";
					}
					else
					{
						Name += ",'" + CashbackMethod[i].Name + "'";
					}
				}
				sql = "select ID from T_CashBack where ID in(select Oid from T_CashBackApprove where ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and [Status]=1 and ApproveName in (" + Name + ")  and ApproveTime is not null )";
			}
			else
			{
				// ApproveMod = db.T_CashBackApprove.Where(a => a.ApproveName == user && a.ApproveTime != null).ToList();
				sql = "select ID from T_CashBack where ID in(select Oid from T_CashBackApprove where ApproveTime>='" + sdate + "' and ApproveTime<='" + edate + "' and [Status]=1 and ApproveName='" + user + "'  and ApproveTime is not null )";

			}
			if (!string.IsNullOrWhiteSpace(queryStr))
				sql += "  and PostUser like'%" + queryStr + "%'";
			if (!string.IsNullOrWhiteSpace(reason))
				sql += " and Reason ='" + reason + "'";
			queryData = db.Database.SqlQuery<int>(sql).AsQueryable();
			//linq in 
			List<string> ids = new List<string>();
			foreach (var item in queryData)
			{
				ids.Add(item.ToString());
			}
			if (queryData.Count() > 0)
			{
				string csvIds = string.Join(",", ids.ToArray());

				var ret = db.Database.SqlQuery<T_CashBack>("  select b.ID, b.PostUser, b.OrderNum, b.VipName, b.ShopName,CONVERT(VARCHAR(24),a.ApproveTime,121) as WangWang, b.Reason, b.BackMoney, a.ApproveName, b.OrderMoney, b.AlipayName, b.AlipayAccount, b.Note, b.PostTime, " +
					"b.For_Delete,b.Status,b.Step,b.Repeat,b.Method,b.BackFrom,b.OrderId From  T_CashBackApprove a  join T_CashBack b " +
					"on a.oid = b.ID     where  a.ApproveName  in (" + Name + ") and a.status=1 and b.id in (" + csvIds + ") order by b.id desc");

				//var ret = db.Database.SqlQuery<T_CashBack>("select *,(select ) from T_CashBack where ID in (" + csvIds + ") order by id desc");

				List<T_CashBack> result = ret.ToList();
				//创建Excel文件的对象
				NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
				//添加一个sheet
				NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
				//给sheet1添加第一行的头部标题
				NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);

				row1.CreateCell(0).SetCellValue("申请人");
				row1.CreateCell(1).SetCellValue("申请时间");
				row1.CreateCell(2).SetCellValue("店铺名称");
				row1.CreateCell(3).SetCellValue("订单号");
				row1.CreateCell(4).SetCellValue("客户姓名");
				row1.CreateCell(5).SetCellValue("返现原因");
				row1.CreateCell(6).SetCellValue("返现金额");
				row1.CreateCell(7).SetCellValue("支付宝姓名");
				row1.CreateCell(8).SetCellValue("支付宝账号");
				row1.CreateCell(9).SetCellValue("订单金额");
				row1.CreateCell(10).SetCellValue("支出账号");
				row1.CreateCell(11).SetCellValue("备注");
				row1.CreateCell(12).SetCellValue("系统备注");
				row1.CreateCell(13).SetCellValue("出纳");
				row1.CreateCell(14).SetCellValue("返现时间");
				for (int i = 0; i < result.Count; i++)
				{
					NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);

					rowtemp.CreateCell(0).SetCellValue(result[i].PostUser.ToString());
					rowtemp.CreateCell(1).SetCellValue(result[i].PostTime.ToString());
					rowtemp.CreateCell(2).SetCellValue(result[i].ShopName);
					rowtemp.CreateCell(3).SetCellValue(result[i].OrderNum);
					rowtemp.CreateCell(4).SetCellValue(result[i].VipName);
					rowtemp.CreateCell(5).SetCellValue(result[i].Reason);

					rowtemp.CreateCell(7).SetCellValue(result[i].AlipayName);
					rowtemp.CreateCell(8).SetCellValue(result[i].AlipayAccount);
					rowtemp.CreateCell(9).SetCellValue(double.Parse(result[i].OrderMoney.ToString()));
					rowtemp.CreateCell(10).SetCellValue(result[i].BackFrom);
					if (!string.IsNullOrEmpty(result[i].Note))
						rowtemp.CreateCell(11).SetCellValue(result[i].Note);
					else
						rowtemp.CreateCell(11).SetCellValue("");
					if (!string.IsNullOrWhiteSpace(result[i].Repeat))
						rowtemp.CreateCell(12).SetCellValue(result[i].Repeat);
					else
						rowtemp.CreateCell(12).SetCellValue("");
					double money = 0;
					if (result[i].BackMoney.ToString() != "")
					{
						money = (double)result[i].BackMoney;
						money = -money;
						rowtemp.CreateCell(6).SetCellValue(money);
					}
					else { rowtemp.CreateCell(6).SetCellValue("0"); }
					rowtemp.CreateCell(13).SetCellValue(result[i].ApproveName.ToString());
					rowtemp.CreateCell(14).SetCellValue(result[i].WangWang.ToString());
				}

				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				book.Write(ms);
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "CashBack.xls");
			}
			else
			{
				Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
				// 写入到客户端 
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				ms.Seek(0, SeekOrigin.Begin);
				ms.Flush();
				ms.Position = 0;
				return File(ms, "application/vnd.ms-excel", "CashBack.xls");
			}


		}
		#endregion
		#region
		public List<SelectListItem> GetModuleList()
		{
			List<SelectListItem> State = new List<SelectListItem>
			{
				 new SelectListItem { Text = "==可选择数据来源==", Value = "" },
				 new SelectListItem { Text = "拦截", Value = "拦截"},
				 new SelectListItem { Text = "返现", Value = "返现"},
				 new SelectListItem { Text = "退款", Value = "退款"},
				 new SelectListItem { Text = "快递赔付", Value = "快递赔付"},
				 new SelectListItem { Text = "换货", Value = "换货"},
				 new SelectListItem { Text = "补发", Value = "补发"},
				 new SelectListItem { Text="专票", Value="专票" },
				 new SelectListItem { Text="微信", Value="微信" },
			};
			return State;
		}
		public List<SelectListItem> GetTypeList()
		{
			List<SelectListItem> State = new List<SelectListItem>
			{
				 new SelectListItem { Text = "==可选择类型==", Value = "" },
				 new SelectListItem { Text = "申请", Value = "申请"},
				 new SelectListItem { Text = "审核", Value = "审核"},
				 new SelectListItem { Text = "同意", Value = "同意"},
				 new SelectListItem { Text = "驳回", Value = "驳回"},
				  new SelectListItem { Text = "不同意", Value = "不同意"},
				   new SelectListItem { Text = "未处理", Value = "未处理"},
					new SelectListItem { Text = "删除", Value = "删除"},

			};
			return State;
		}
		[Description("统计数据")]
		public ActionResult ViewCashBackStatistics()
		{
			ViewData["Module"] = GetModuleList();
			ViewData["Type"] = GetTypeList();
			return View();
		}
		public class CashBackStatistics
		{
			public string PostUser { get; set; }
			public int qty { get; set; }
			public string Module { get; set; }
			public string Type { get; set; }
		}
		//获取统计数据
		public ContentResult GetStatisticsList(Lib.GridPager pager, string queryStr, string startDate, string endDate, string Module, string Type)
		{
			List<CashBackStatistics> queryData = null;
			string sdate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
			string edate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
			if (!string.IsNullOrEmpty(startDate))
			{
				sdate = startDate + " 00:00:00";
			}
			if (!string.IsNullOrEmpty(endDate))
			{
				edate = endDate + " 23:59:59";
			}
			string sql = "select PostUser, count(*) as qty, '返现' as Module, '申请' as Type From T_CashBack where For_Delete = 0 and  Status = 1 and  PostTime >= '" + sdate + "' and PostTime <= '" + edate + "' group by PostUser union  select PostUser ,count(*) as qty,'返现' as module,'不同意' as type From T_CashBack where For_Delete=0 and  Status=2 and  PostTime >= '" + sdate + "' and PostTime <= '" + edate + "'  group by PostUser  union select ApproveName, count(*) as qty, '返现' as Module, '审核' as Type From T_CashBackApprove where   ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "' group by ApproveName union select ApproveName, count(*) as qty, '返现' as Module, '同意' as Type From T_CashBackApprove  where Status = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName union select ApproveName, count(*) as qty, '返现' as Module, '驳回' as Type From T_CashBackApprove  where Status = 2 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName  " +
				"union  select PostUser, count(*) as qty, '拦截' as Module, '申请' as Type From T_Intercept where Status = 1 and IsDelete = 0 and  CreateDate >= '" + sdate + "' and CreateDate <= '" + edate + "'   group by PostUser   union   select PostUser ,count(*) as qty,'拦截' as module, '不同意' as type From T_Intercept where Status=2 and IsDelete=0 and  CreateDate>= '" + sdate + "'  and CreateDate<= '" + edate + "'    group by PostUser union select ApproveUser, count(*) as qty, '拦截' as Module, '审核' as Type From T_InterceptApprove where ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveUser union select ApproveUser, count(*) as qty, '拦截' as Module, '同意' as Type From T_InterceptApprove where ApproveStatus = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveUser union select ApproveUser, count(*) as qty, '拦截' as Module, '驳回' as Type From T_InterceptApprove where ApproveStatus = 2  and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveUser  " +
				"union    select Retreat_ApplyName, count(*) as qty, '退款' as Module, '申请' as Type From T_Retreat where Status = 1 and IsDelete = 0 and  Retreat_date >= '" + sdate + "' and Retreat_date <= '" + edate + "'   group by Retreat_ApplyName  union       select Retreat_ApplyName, count(*) as qty, '退款' as module, '不同意' as type From T_Retreat where Status = 2 and IsDelete = 0 and  Retreat_date >= '" + sdate + "' and Retreat_date <= '" + edate + "'    group by Retreat_ApplyName union select ApproveName, count(*) as qty, '退款' as Module, '审核' as Type From T_RetreatAppRove where ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName union select ApproveName, count(*) as qty, '退款' as Module, '同意' as Type From T_RetreatAppRove where Status = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName union select ApproveName, count(*) as qty, '退款' as Module, '驳回' as Type From T_RetreatAppRove where Status = 2 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName " +
				"union   select PostUserName, count(*) as qty, '快递赔付' as Module, '申请' as Type From T_ExpressIndemnity where  IsDelete = 0  and  Date >= '" + sdate + "' and Date <= '" + edate + "'  group by PostUserName  union  select PostUserName, count(*) as qty, '快递赔付' as module, '未处理' as type From T_ExpressIndemnity where  IsDelete = 0  and State =0 and Date >= '" + sdate + "' and Date <= '" + edate + "'  group by PostUserName union select ApproveName, count(*) as qty, '快递赔付' as Module, '审核' as Type From T_ExpressIndemnityApprove where   ApproveData >= '" + sdate + "' and ApproveData <= '" + edate + "' group by ApproveName   union     select PostUser, count(*) as qty, '换货' as Module, '申请' as Type From T_ExchangeCenter where Status = 1 and IsDelete = 0  and  CreateDate >= '" + sdate + "' and CreateDate <= '" + edate + "'  group by PostUser    union            select PostUser, count(*) as qty, '换货' as module, '不同意' as type From T_ExchangeCenter where Status = 2 and IsDelete = 0  and  CreateDate >= '" + sdate + "'  and CreateDate <= '" + edate + "'  group by PostUser  " +
				"union select ApproveName, count(*) as qty, '换货' as Module, '审核' as Type From T_ExchangeCenterApprove where  ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName union select ApproveName, count(*) as qty, '换货' as Module, '同意' as Type From T_ExchangeCenterApprove where ApproveStatus = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName  union select ApproveName, count(*) as qty, '换货' as Module, '驳回' as Type From T_ExchangeCenterApprove where ApproveStatus = 2 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'   group by ApproveName union   select PostUser, count(*) as qty, '补发' as Module, '申请' as Type From T_Reissue where Status = 1 and IsDelete = 0  and  CreatDate >= '" + sdate + "' and CreatDate <= '" + edate + "'  group by PostUser union       select PostUser, count(*) as qty, '补发' as module, '不同意' as type From T_Reissue where Status = 2 and IsDelete = 0  and  CreatDate >= '" + sdate + "'  and CreatDate <= '" + edate + "' group by PostUser  " +
				"union select ApproveUser, count(*) as qty, '补发' as Module, '审核' as Type From T_ReissueApprove where  ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "' group by ApproveUser union select ApproveUser, count(*) as qty, '补发' as Module, '同意' as Type From T_ReissueApprove where ApproveStatus = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveUser union select ApproveUser, count(*) as qty, '补发' as Module, '驳回' as Type From T_ReissueApprove where ApproveStatus = 2 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveUser union select PostUser, count(*) as qty, '返现' as Module, '删除' as Type From T_CashBack where For_Delete = 1 and PostTime >= '" + sdate + "' and PostTime <= '" + edate + "' group by PostUser union  select PostUser, count(*) as qty, '拦截' as Module, '删除' as Type From T_Intercept where  IsDelete = 1 and  CreateDate >= '" + sdate + "' and CreateDate <= '" + edate + "'   group by PostUser union    select Retreat_ApplyName, count(*) as qty, '退款' as Module, '删除' as Type From T_Retreat where  IsDelete = 1 and  Retreat_date >= '" + sdate + "' and Retreat_date <= '" + edate + "'   group by Retreat_ApplyName " +
				"union   select PostUserName, count(*) as qty, '快递赔付' as Module, '删除' as Type From T_ExpressIndemnity where  IsDelete = 1 and  Date >= '" + sdate + "' and Date <= '" + edate + "'  group by PostUserName " +
				"union     select PostUser, count(*) as qty, '换货' as Module, '删除' as Type From T_ExchangeCenter where   IsDelete = 1 and  CreateDate >= '" + sdate + "' and CreateDate <= '" + edate + "'  group by PostUser " +
				"union   select PostUser, count(*) as qty, '补发' as Module, '申请' as Type From T_Reissue where   IsDelete = 1 and  CreatDate >= '" + sdate + "' and CreatDate <= '" + edate + "'  group by PostUser " +
				"union  select PostUser, count(*) as qty, '专票' as module, '申请' as type From T_MajorInvoice where Status = 1 and IsDelete = 0  group by PostUser   union   select PostUser, count(*) as qty, '专票' as module, '删除' as type From T_MajorInvoice where   IsDelete = 1  group by PostUser    union select ApproveName, count(*) as qty, '专票' as module, '审核' as type From T_MajorInvoiceAppRove group by ApproveName union select ApproveName, count(*) as qty, '专票' as module, '同意' as type From T_MajorInvoiceAppRove where Status = 1 group by ApproveName union select ApproveName, count(*) as qty, '专票' as module, '驳回' as type From T_MajorInvoiceAppRove where Status = 2  group by ApproveName union   select PostUser, count(*) as qty, '专票' as module, '不同意' as type From T_MajorInvoice where   Status = 2 and  Isdelete = 0 group by PostUser " +
				"union select PostUser, count(*) as qty, '补发' as Module, '删除' as Type From T_Reissue where   IsDelete = 1  and CreatDate >= '" + sdate + "' and CreatDate <= '" + edate + "'  group by PostUser " +
				"union   select EstablishName, count(*) as qty, '微信' as Module, '申请' as Type From T_WeChat where Isdelete = 0 and Status = 1 and EstablishDate >= '" + sdate + "' and EstablishDate <= '" + edate + "' group by EstablishName " +
				"union  select EstablishName, count(*) as qty, '微信' as module, '不同意' as type From T_WeChat where Isdelete = 0 and  Status = 2 and  EstablishDate >= '2018-03-03' and EstablishDate <= '" + edate + "'  group by EstablishName " +
				"union select ApproveName, count(*) as qty, '微信' as Module, '审核' as Type From T_WeChatApprove where   ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "' group by ApproveName " +
				"union select ApproveName, count(*) as qty, '微信' as Module, '同意' as Type From T_WeChatApprove where Status = 1 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName " +
				"union select ApproveName, count(*) as qty, '微信' as Module, '驳回' as Type From T_WeChatApprove where Status = 2 and ApproveTime >= '" + sdate + "' and ApproveTime <= '" + edate + "'  group by ApproveName " +
				"union select EstablishName, count(*) as qty, '微信' as Module, '删除' as Type From T_WeChat where Isdelete = 1 and EstablishDate >= '" + sdate + "' and EstablishDate <= '" + edate + "' group by EstablishName ";   //统计
			queryData = db.Database.SqlQuery<CashBackStatistics>(sql).ToList();

			if (!string.IsNullOrWhiteSpace(Module))
			{
				queryData = queryData.Where(a => a.Module == Module).ToList();
			}
			if (!string.IsNullOrWhiteSpace(Type))
			{
				queryData = queryData.Where(a => a.Type == Type).ToList();
			}
			if (!string.IsNullOrEmpty(queryStr))
			{
				queryData = queryData.Where(a => a.PostUser != null && a.PostUser.Contains(queryStr)).ToList();
			}
			// IQueryable<CashBackStatistics> queryData = db.Database.SqlQuery<CashBackStatistics>(sql).AsQueryable();
			pager.totalRows = queryData.Count();
			//分页
			List<CashBackStatistics> list = queryData.OrderByDescending(c => c.qty).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
			List<CashBackStatisticsTotal> footerList = new List<CashBackStatisticsTotal>();
			CashBackStatisticsTotal footer = new CashBackStatisticsTotal();
			footer.Type = "总计:";
			if (list.Count() > 0)
				footer.qty = decimal.Parse(list.Sum(s => s.qty).ToString());
			else
				footer.qty = 0;
			footerList.Add(footer);
			string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, setTimeFormat()) + "}";
			// string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
			return Content(json);
		}
		public static IsoDateTimeConverter setTimeFormat()
		{
			IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
			timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
			return timeFormat;
		}
		public class CashBackStatisticsTotal
		{
			public string Type { get; set; }
			public decimal qty { get; set; }
		}
		#endregion
	}
}
