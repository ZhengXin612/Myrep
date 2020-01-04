using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EBMS
{
	public class wdtHelper
	{
		static string WdtConnectionStr=System.Configuration.ConfigurationManager.AppSettings["wdtConnection"];
		static MySqlHelper sqlhelper_mysql = new MySqlHelper(WdtConnectionStr);

		public static string getOrder(string OrderCode= "575611756379512818")
		{
			//sales_trade_order 销售 产品  详情
			//string sql = "select *from sales_trade_order where src_tid='" + OrderCode+"'";
			//var x= sqlhelper_mysql.ExecuteDataTable(sql);
			//JsonData result = JsonMapper.ToObject(JsonConvert.SerializeObject(x));

			string sqlOrder = "select src_tids,receiver_name,receiver_address,receiver_area from sales_trade where src_tids='" + OrderCode + "'  LIMIT 1";
			var y = sqlhelper_mysql.ExecuteDataTable(sqlOrder);
			return JsonConvert.SerializeObject(y);

		}

	}
}