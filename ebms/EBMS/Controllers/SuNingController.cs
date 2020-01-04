using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class SuNingController : Controller
    {
        //
        // GET: /SuNing/
        EBMSEntities db = new EBMSEntities();
        [Description("苏宁订单列表")]
        public ActionResult SuNingGrid()
        {
            return View();
        }

        [Description("Lazada订单详情列表")]
        public ActionResult ViewSuNingItemsList(string orderCode)
        {
            ViewData["OrderCode"] = orderCode;
            return View();
        }
        //获取苏宁订单数据
        public ContentResult GetSuningList(Lib.GridPager pager, string queryStr,string sel)
        {
            IQueryable<T_HtSuNing> queryData = db.T_HtSuNing.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
            { queryData = queryData.Where(a => a.OrderNumber.Contains(queryStr)); }

            if (!string.IsNullOrWhiteSpace(sel))
            {
                int s = Convert.ToInt32(sel);
                queryData = queryData.Where(a => a.Istijiao.Equals(s));
            }
            else {
                queryData = queryData.Where(a => a.Istijiao.Equals(0));
            }
            if (queryData != null)
            {
                List<T_HtSuNing> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
             
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        //获取苏宁订单明细数据
        public ContentResult GetSuNingItemList(Lib.GridPager pager, string OrderCode)
        {

            IQueryable<T_HtSuNingItem> queryData = db.T_HtSuNingItem.Where(a => a.PorderID == OrderCode);
            if (queryData != null)
            {
                List<T_HtSuNingItem> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }
        [HttpPost]
        public JsonResult SuNingDelete(string ids)//删除lazada订单
        {
            try
            {
                string[] Ids = ids.Split(',');
                for (int j = 0; j < Ids.Length; j++)
                {
                    int id = Convert.ToInt32(Ids[j]);
                    T_HtSuNing delModel = db.T_HtSuNing.Find(id);
                    T_HtSuNingItem item = db.T_HtSuNingItem.FirstOrDefault(s => s.PorderID.Equals(delModel.OrderNumber));
                    if (item!=null)
                    {
                        db.T_HtSuNingItem.Remove(item);
                    }
                    db.T_HtSuNing.Remove(delModel);
                }
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult UpToGY(string ids)//上传到旺店通
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string shibai = "0";
                try
                {
                    var aa = "";

                    string[] Ids = ids.Split(',');
                    for (int j = 0; j < Ids.Length; j++)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        int id = Convert.ToInt32(Ids[j]);
                        T_HtSuNing ordermodel = db.T_HtSuNing.SingleOrDefault(a => a.ID == id);
                        if (ordermodel.Istijiao == 1)
                            return Json(new { State = "Faile", Message = "" + ordermodel.OrderNumber + "已提交到旺店通，不能重复提交" }, JsonRequestBehavior.AllowGet);


                        string ShopName = "";
                      
                            ShopName = "3011";

                        string tid = ordermodel.OrderNumber;
                        //int trade_status = 10;
                        //int pay_status = 2;
                        //int delivery_term = 1;
                        DateTime trade_time = DateTime.Parse(ordermodel.CreateTime.ToString());
                        DateTime pay_time = DateTime.Parse(ordermodel.CreateTime.ToString());
                        string buyer_nick = ordermodel.CustomerName;
                        string receiver_name = ordermodel.CustomerName;
                     
                        string receiver_province = ordermodel.Province;
                        string receiver_city = ordermodel.City;
                        string receiver_district = ordermodel.Area;
                        string receiver_address = ordermodel.Address;
                        string receiver_mobile = ordermodel.Telephone;

                        // int logistics_type = -1;
                        string seller_memo = ordermodel.CustomerRemarks;
                        decimal paid = 0.00m ;

                        List<T_HtSuNingItem> orderItems = db.T_HtSuNingItem.Where(a => a.PorderID == tid).ToList();
                        string order_list = "";

                        for (int i = 0; i < orderItems.Count; i++)
                        {
                            decimal num = decimal.Parse(orderItems[i].num.ToString());
                            decimal UnitPrice = decimal.Parse(orderItems[i].UnitPrice.ToString());
                            paid += num * UnitPrice;

                            string goods_no = orderItems[i].ProductCode;
                            string spec_no = orderItems[i].ProductCode;
                            //    string Guid = System.Guid.NewGuid().ToString();
                            T_WDTGoods cofig = db.T_WDTGoods.SingleOrDefault(s => s.goods_no == goods_no);
                            string goods_name = "";
                            if (cofig != null)
                            {
                                goods_name = cofig.goods_name;
                            }
                            string oid = Guid.NewGuid().ToString();
                            if (order_list == "")
                            {
                                order_list += "{" +
                                   "\"oid\": \"" + oid + "\"," +
                                   "\"num\": \"" + num + "\"," +
                                   "\"price\": \"" + UnitPrice + "\"," +
                                   "\"status\": \"40\"," +
                                   "\"refund_status\": \"0\"," +
                                      "\"adjust_amount\": \"0\"," +
                                          "\"discount\": \"0\"," +
                                                "\"share_discount\": \"0\"," +
                                   "\"goods_id\": \"" + goods_no + "\"," +
                                   "\"goods_no\": \"" + goods_no + "\"," +
                                   "\"spec_no\": \"" + goods_no + "\"," +
                                   "\"goods_name\": \"" + goods_name + "\"," +
                                   "\"cid\": \"\"" +
                               "}";
                            }
                            else
                            {
                                order_list += ",{" +
                                     "\"oid\": \"" + oid + "\"," +
                                     "\"num\": \"" + num + "\"," +
                                     "\"price\": \"" + UnitPrice + "\"," +
                                     "\"status\": \"40\"," +
                                     "\"refund_status\": \"0\"," +
                                         "\"adjust_amount\": \"0\"," +
                                           "\"discount\": \"0\"," +
                                              "\"share_discount\": \"0\"," +
                                      "\"goods_id\": \"" + goods_no + "\"," +
                                     "\"goods_no\": \"" + goods_no + "\"," +
                                     "\"spec_no\": \"" + goods_no + "\"," +
                                     "\"goods_name\": \"" + goods_name + "\"," +
                                     "\"cid\": \"\"" +
                                 "}";
                            }
                        }

                        //旺店通
                        dic.Remove("shop_no");
                        dic.Add("shop_no", ShopName);

                        string cmd = "[{" +
                            "\"tid\": \"" + tid + "\"," +
                    "\"trade_status\": \"30\"," +
                    "\"pay_status\": \"2\"," +
                    "\"delivery_term\": \"1\"," +
                    "\"trade_time\": \"" + trade_time + "\"," +
                    "\"pay_time\": \"" + pay_time + "\"," +
                    "\"buyer_nick\": \"" + buyer_nick + "\"," +
                    "\"buyer_email\": \"\"," +
                    "\"receiver_name\": \"" + receiver_name + "\"," +
                    "\"receiver_province\": \"" + receiver_province + "\"," +
                    "\"receiver_city\": \"" + receiver_city + "\"," +
                    "\"receiver_district\": \"" + receiver_district + "\"," +
                    "\"receiver_address\": \"" + receiver_address + "\"," +
                    "\"receiver_mobile\": \"" + receiver_mobile + "\"," +
                    "\"receiver_zip\": \"\"," +
                    "\"logistics_type\": \"-1\"," +
                    "\"buyer_message\": \"\"," +
                    "\"seller_memo\": \"" + seller_memo + "\"," +
                    "\"post_amount\": \"0\"," +
                    "\"cod_amount\": \"0\"," +
                    "\"ext_cod_fee\": \"0\"," +
                    "\"paid\": \"" + paid + "\"," +
                    "\"order_list\": [" + order_list + "]}]";
                        dic.Remove("trade_list");
                        dic.Remove("sid");
                        dic.Remove("appkey");
                        dic.Remove("timestamp");
                        dic.Add("trade_list", cmd);
                        dic.Add("sid", "hhs2");
                        dic.Add("appkey", "hhs2-ot");
                        dic.Add("timestamp", GetTimeStamp());

                        aa = CreateParam(dic, true);


                        string ret = Post("http://api.wangdian.cn/openapi2/trade_push.php", aa);


                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string sd = jsonData[0].ToString();
                        if (sd == "0")
                        {
                            int sdz = int.Parse(jsonData[2].ToString());
                            if (sdz > 0)
                            {
                                T_HtSuNing model = db.T_HtSuNing.Single(a => a.ID == id);
                                model.Istijiao = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                shibai += tid + ",";
                            }
                        }
                        else
                            shibai += tid + ",";
                    }
                    sc.Complete();
                    if (shibai != "0")
                    {
                        return Json(new { State = "Faile", Message = "" + shibai + "提交旺店通失败，请与技术人员联系" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
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


        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strText));
            string strMd5 = BitConverter.ToString(result);
            strMd5 = strMd5.Replace("-", "");
            return strMd5;// System.Text.Encoding.Default.GetString(result);
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
        public string Post(string url, string postData)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream serviceRequestBodyStream = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bodyBytes = encoding.GetBytes(postData);
                request.ContentLength = bodyBytes.Length;
                using (serviceRequestBodyStream = request.GetRequestStream())
                {
                    serviceRequestBodyStream.Write(bodyBytes, 0, bodyBytes.Length);
                    serviceRequestBodyStream.Close();
                    using (response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string result = reader.ReadToEnd();
                            reader.Close();
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }

        }

    }
}
