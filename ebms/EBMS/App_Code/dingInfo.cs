using EBMS.Models;
using Lib;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;


namespace EBMS
{
	

	public static class dingInfo
	{
		
		static EBMSEntities db = new EBMSEntities();
		public static string appid { get { return getKeyValue("appid"); } }
		public static string appsecret { get{ return getKeyValue("appsecret"); } }
		public static string redirect_uri { get { return getKeyValue("redirect_uri"); } }
		public static string corpid { get { return getKeyValue("corpid"); } }
		public static string corpsecret { get { return getKeyValue("corpsecret"); } }
		public static string agentid { get { return getKeyValue("agentid"); } }

		public static string getKeyValue(string key)
		{
			T_DingConfig dingConfig = db.T_DingConfig.FirstOrDefault(a => a.Key == key);
			string KeyValue = dingConfig.Value;
			return KeyValue;
		}
	}
	public class DingLogin
	{
		public string _appid = "dingoa5qlmsqgbagn2vyjy";
		public string _appsecret = "Cy6x9JM95Gjyzq6pTR49pLMIbXDb0eLvOuDFjeQFrQyv_Jz4LOAudTt9m2xsZt_S";


		public string tmp_auth_code { get; set; }
		public string state { get; set; }

		public string appid
		{
			get { return _appid; }
			set { _appid = value; }
		}
		public string appsecret
		{
			get { return _appsecret; }
			set { _appsecret = value; }
		}



		public JsonData login()
		{

			//DingDingEntities db = new DingDingEntities();
			string url_access_token = "https://oapi.dingtalk.com/sns/gettoken?appid=" + _appid + "&appsecret=" + _appsecret;
			string access_token = "";
			string ret = HttpHelper.HttpGet(url_access_token, "");
			JsonData jsonData = JsonMapper.ToObject(ret);
			if (jsonData.Count == 3)
			{
				access_token = jsonData["access_token"].ToString();
				if (state == "STATE")
				{
					string url_code = "https://oapi.dingtalk.com/sns/get_persistent_code?access_token=" + access_token;
					string persistent_code = "";
					string openid = "";
					string unionid = "";
					string cmd = "{\"tmp_auth_code\":\"" + tmp_auth_code + "\"}";
					string ret_code = HttpHelper.DoPost(url_code, cmd, Encoding.UTF8, "json");

					JsonData jsonData_code = JsonMapper.ToObject(ret_code);
					if (jsonData_code.Count == 5)
					{
						persistent_code = jsonData_code["persistent_code"].ToString();
						openid = jsonData_code["openid"].ToString();
						unionid = jsonData_code["unionid"].ToString();
						string url_SNS_token = "https://oapi.dingtalk.com/sns/get_sns_token?access_token=" + access_token;
						string SNS_token = "";
						string cmds = "{ " +
									"\"openid\": \"" + openid + "\"," +
									"\"persistent_code\": \"" + persistent_code + "\"" +
								   " }";
						string ret_SNS_token = HttpHelper.DoPost(url_SNS_token, cmds, Encoding.UTF8, "json");
						JsonData jsonData_SNS_token = JsonMapper.ToObject(ret_SNS_token);
						if (jsonData_SNS_token.Count == 4)
						{

							SNS_token = jsonData_SNS_token["sns_token"].ToString();
							string url_user = "https://oapi.dingtalk.com/sns/getuserinfo?sns_token=" + SNS_token;
							string ret_user = HttpHelper.HttpGet(url_user, "");
							try
							{
								JsonData jsonData_user = JsonMapper.ToObject(ret_user);

								if (jsonData_user.Count == 3)
								{
									JsonData user_info = jsonData_user["user_info"];
									if (user_info.Count == 4)
									{
										string dingid = user_info["dingId"].ToString();

										//string connectionStr = "server=120.24.176.207;database=DingDing;user=dingding;pwd=123456;";

										//string sql = "select *From T_Personnel where dingId='" + dingid + "'";
										//SqlHelper sqlhelp = new SqlHelper(connectionStr);
										//var user = sqlhelp.ExecuteDataRow(sql);

										////T_Personnel User = db.T_Personnel.SingleOrDefault(s => s.dingId.Equals(dingid));
										////核实员工信息
										//JsonData x = JsonMapper.ToObject(JsonConvert.SerializeObject(user));
										return user_info;


									}
								}
							}
							catch (Exception e)
							{
								throw e;
							}
						}


					}
				}
			}
			return null;
		}



	}
}