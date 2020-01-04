using Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace EBMS.App_Code
{
    public class Lazada
    {
        public static class LZD
        {

            public static string HttpGet(string Url, string postDataStr)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                return retString;
            }
            public static string getResult(DateTime statedate, string flag)
            {
                string url = "";
                string password = "";
                if (flag == "1")
                {
                    url = "https://api.sellercenter.lazada.sg/";
                    password = "2b432b0a7dead3fd3894307313ea00b5c84c5035";
                }
                else if (flag == "2")
                {
                    url = "https://api.sellercenter.lazada.co.th/";
                    password = "6e249ea65519a8d2a18721012d75766745dc994b";
                }
                else if (flag == "3")
                {
                    url = "https://api.sellercenter.lazada.co.id/";
                    password = "680945ce5d9d68073aa5bad81fb7f79c050ae03e";
                }
                else if (flag == "4")
                {
                    url = "https://api.sellercenter.lazada.com.ph/";
                    password = "b2f30268e7585fbdff45b5a67d5b93cb14268e89";
                }
                else
                {
                    url = "https://api.sellercenter.lazada.com.my/";
                    password = "b5637168bd5883034192d2f911ca8c63ba83c801";
                }
                string userId = "shelly@hnkyyl.com";
                //string password = "b5637168bd5883034192d2f911ca8c63ba83c801";//b5637168bd5883034192d2f911ca8c63ba83c801   123123Lfm
                string version = "1.0";
                string action = "GetOrders";
                //string url = "https://sellercenter-api.lazada.com.my";
                string result = "";
                //result = generateRequest(url, userId, password, version, action);
                string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss-0000");
                string stringToHash = "Action=" + action +
                "&CreatedAfter=" + System.Web.HttpContext.Current.Server.UrlEncode(statedate.ToString("yyyy-MM-ddTHH:mm:ss-0000")).ToUpper() +
                "&Format=JSON" +
                "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
                "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
                "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                string hash = HashString(stringToHash, password);
                string request = "Action=" + action +
                "&CreatedAfter=" + System.Web.HttpContext.Current.Server.UrlEncode(statedate.ToString("yyyy-MM-ddTHH:mm:ss-0000")).ToUpper() +
                "&Format=JSON" +
                "&Signature=" + System.Web.HttpContext.Current.Server.UrlEncode(hash) +
                "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
                "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
                "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                result = url + "?" + request;
                Console.WriteLine(result);
                return HttpGet(result, "");

            }
            public static string getResultOrderItems(string code, string flag)
            {
                string url = "";
                string password = "";
                if (flag == "1")
                {
                    url = "https://api.sellercenter.lazada.sg/";
                    password = "2b432b0a7dead3fd3894307313ea00b5c84c5035";
                }
                else if (flag == "2")
                {
                    url = "https://api.sellercenter.lazada.co.th/";
                    password = "6e249ea65519a8d2a18721012d75766745dc994b";
                }
                else if (flag == "3")
                {
                    url = "https://api.sellercenter.lazada.co.id/";
                    password = "680945ce5d9d68073aa5bad81fb7f79c050ae03e";
                }
                else if (flag == "4")
                {
                    url = "https://api.sellercenter.lazada.com.ph/";
                    password = "b2f30268e7585fbdff45b5a67d5b93cb14268e89";
                }
                else
                {
                    url = "https://api.sellercenter.lazada.com.my/";
                    password = "b5637168bd5883034192d2f911ca8c63ba83c801";
                }
                string userId = "shelly@hnkyyl.com";
                // string password = "b5637168bd5883034192d2f911ca8c63ba83c801";//b5637168bd5883034192d2f911ca8c63ba83c801   123123Lfm
                string version = "1.0";
                string action = "GetOrderItems";
                // string url = "https://sellercenter-api.lazada.com.my";
                string result = "";
                //result = generateRequest(url, userId, password, version, action);
                string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss-0000");
                string stringToHash = "Action=" + action +
               "&Format=JSON" +
                  "&OrderId=" + code +
              "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
              "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
              "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                string hash = HashString(stringToHash, password);
                string request = "Action=" + action +
               "&Format=JSON" +
                  "&OrderId=" + code +
              "&Signature=" + System.Web.HttpContext.Current.Server.UrlEncode(hash) +
              "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
              "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
              "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                result = url + "?" + request;
                Console.WriteLine(result);
                return HttpGet(result, "");

            }
            public static string getResultProducts(string SKU, string flag)
            {
                string url = "";
                string password = "";
                if (flag == "1")
                {
                    url = "https://api.sellercenter.lazada.sg/";
                    password = "2b432b0a7dead3fd3894307313ea00b5c84c5035";
                }
                else if (flag == "2")
                {
                    url = "https://api.sellercenter.lazada.co.th/";
                    password = "6e249ea65519a8d2a18721012d75766745dc994b";
                }
                else if (flag == "3")
                {
                    url = "https://api.sellercenter.lazada.co.id/";
                    password = "680945ce5d9d68073aa5bad81fb7f79c050ae03e";
                }
                else if (flag == "4")
                {
                    url = "https://api.sellercenter.lazada.com.ph/";
                    password = "b2f30268e7585fbdff45b5a67d5b93cb14268e89";
                }
                else
                {
                    url = "https://api.sellercenter.lazada.com.my/";
                    password = "b5637168bd5883034192d2f911ca8c63ba83c801";
                }
                string userId = "shelly@hnkyyl.com";
                // string password = "b5637168bd5883034192d2f911ca8c63ba83c801";//b5637168bd5883034192d2f911ca8c63ba83c801   123123Lfm
                string version = "1.0";
                string action = "GetProducts";
                // string url = "https://sellercenter-api.lazada.com.my";
                string result = "";
                //result = generateRequest(url, userId, password, version, action);
                string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss-0000");
                string stringToHash = "Action=" + action +
                 
               "&Format=JSON" +

               "&Search=" +SKU +
              "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
              "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
              "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                string hash = HashString(stringToHash, password);
                string request = "Action=" + action +
                 
               "&Format=JSON" +
               "&Search=" + SKU +
              "&Signature=" + System.Web.HttpContext.Current.Server.UrlEncode(hash) +
              "&Timestamp=" + System.Web.HttpContext.Current.Server.UrlEncode(timeStamp).ToUpper() +
              "&UserID=" + System.Web.HttpContext.Current.Server.UrlEncode(userId) +
              "&Version=" + System.Web.HttpContext.Current.Server.UrlEncode(version);
                result = url + "?" + request;
                Console.WriteLine(result);
                return HttpGet(result, "");

            }
           
            public static string HashString(string StringToHash, string HachKey)
            {

                byte[] Key = System.Text.Encoding.UTF8.GetBytes(HachKey);
                byte[] Text = System.Text.Encoding.UTF8.GetBytes(StringToHash);
                System.Security.Cryptography.HMACSHA256 myHMACSHA256 = new System.Security.Cryptography.HMACSHA256(Key);
                byte[] HashCode = myHMACSHA256.ComputeHash(Text);
                string hash = BitConverter.ToString(HashCode).Replace("-", "");
                return hash.ToLower();
            }
        }
    }
}