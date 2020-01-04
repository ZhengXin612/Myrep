using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EBMS
{
  public class HttpHelper
	{
		public static string DoPost(string postUrl, string paramData, Encoding dataEncode, string type = "")
		{
			string ret = string.Empty;
			try
			{
				//byte[] byteArray = dataEncode.GetBytes(paramData); //转化
				byte[] byteArray = Encoding.UTF8.GetBytes(paramData);
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
				webReq.Method = "POST";
				if (type == "")
					webReq.ContentType = "application/x-www-form-urlencoded";
				else
				{
					webReq.ContentType = "application/json";
				}
				webReq.Proxy = null;
				webReq.ContentLength = byteArray.Length;
				Stream newStream = webReq.GetRequestStream();
				newStream.Write(byteArray, 0, byteArray.Length);//写入参数
				newStream.Close();
				HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
				StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				ret = sr.ReadToEnd();
				sr.Close();
				response.Close();
				newStream.Close();
			}
			catch (Exception ex)
			{
				throw;
			}
			return ret;
		}
		public static string HttpGet(string Url, string postDataStr)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
			request.Method = "GET";
			request.ContentType = "text/html;charset=UTF-8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream myResponseStream = response.GetResponseStream();
			StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
			string retString = myStreamReader.ReadToEnd();
			myStreamReader.Close();
			myResponseStream.Close();

			return retString;
		}
	}
}
