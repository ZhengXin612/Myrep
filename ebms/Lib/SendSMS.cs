using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CCPRestSDK;
namespace Lib
{
    public class SendSMS
    {
        public static string Send(string[] msg,string number="15802600278")
        {
            string ret = null;
          
            CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
            //ip格式如下，不带https://
            bool isInit = api.init("app.cloopen.com", "8883"); //  生产) Rest URL：https://app.cloopen.com:8883   测试sandboxapp.cloopen.com
            api.setAccount("aaf98f894e2360b4014e32c97baf0d57","0740a208144f4fa3a5b38666da37acb9");
            api.setAppId("8a48b55153cb69470153e5b9ee1e255b");

            try
            {
                if (isInit)
                {
                    Dictionary<string, object> retData = api.SendTemplateSMS(number, "78674", msg);
                    ret = getDictionaryData(retData);
                }
                else
                {
                    ret = "初始化失败";
                }
            }
            catch (Exception exc)
            {
                ret = exc.Message;
            }
             
            return ret;
        }
        public static string SendGZUrl(string[] msg,  string number = "15802600278")
        {
            string ret = null;

            CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
            //ip格式如下，不带https://
            bool isInit = api.init("app.cloopen.com", "8883"); //  生产) Rest URL：https://app.cloopen.com:8883   测试sandboxapp.cloopen.com
            api.setAccount("aaf98f894e2360b4014e32c97baf0d57", "0740a208144f4fa3a5b38666da37acb9");
            api.setAppId("8a48b55153cb69470153e5b9ee1e255b");

            try
            {
                if (isInit)
                {
                    Dictionary<string, object> retData = api.SendTemplateSMS(number, "92121", msg);
                    ret = getDictionaryData(retData);
                }
                else
                {
                    ret = "初始化失败";
                }
            }
            catch (Exception exc)
            {
                ret = exc.Message;
            }

            return ret;
        }
        public static string Send(string[] msg,string templete, string number = "15802600278")
        {
            string ret = null;

            CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
            //ip格式如下，不带https://
            bool isInit = api.init("app.cloopen.com", "8883"); //  生产) Rest URL：https://app.cloopen.com:8883   测试sandboxapp.cloopen.com
            api.setAccount("aaf98f894e2360b4014e32c97baf0d57", "0740a208144f4fa3a5b38666da37acb9");
            api.setAppId("8a48b55153cb69470153e5b9ee1e255b");

            try
            {
                if (isInit)
                {
                    Dictionary<string, object> retData = api.SendTemplateSMS(number, templete, msg);
                    ret = getDictionaryData(retData);
                }
                else
                {
                    ret = "初始化失败";
                }
            }
            catch (Exception exc)
            {
                ret = exc.Message;
            }

            return ret;
        }
        private static string getDictionaryData(Dictionary<string, object> data)
        {
            string ret = null;
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ret += item.Key.ToString() + "={";
                    ret += getDictionaryData((Dictionary<string, object>)item.Value);
                    ret += "};";
                }
                else
                {
                    ret += item.Key.ToString() + "=" + (item.Value == null ? "null" : item.Value.ToString()) + ";";
                }
            }
            return ret;
        }
    }
}
