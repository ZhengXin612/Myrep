using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Converters;

namespace EBMS.Controllers
{
    public class WDTJDStockController : Controller
    {
        //
        // GET: /WDTJDQuery/
      //  EBMSEntitiesxx db=new EBMSEntitiesxx();

        public ActionResult WDTJDQuery()
        {
            return View();
        }

        class listResult
        {
            public string fnumber { set; get; }
            public string fname { set; get; }
            public string fmodel { set; get; }
            public string jlname { set; get; }
            public decimal fqty { set; get; }
            public string CKfname { set; get; }
            public string PTname { set; get; }

        }

        public class CashBackStatisticsTotal
        {
            public string jlname { get; set; }
            public decimal fqty { get; set; }
        }
        List<listResult> list = new List<listResult>();
        //查询  
        [HttpPost]
        public ContentResult GetList(Lib.GridPager pager, string queryStr, string Type)
        {
            list = new List<listResult>();
            UpToGY(queryStr);
            GetKingdee(queryStr);

            if(Type!=null&& Type!="")
            {
               list = list.Where(a => a.CKfname == Type).ToList();
            }


            //分页
            pager.totalRows = list.Count();
            List<listResult> queryData = list.OrderByDescending(c => c.fnumber).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

            //结果集合重新包装成需要的数据返回
            List<listResult> ResultMod = new List<listResult>();
            foreach (var item in queryData)
            {
                listResult ietmMOD = new listResult();
                ietmMOD.fnumber = item.fnumber;
                ietmMOD.fname = item.fname;
                ietmMOD.fmodel = item.fmodel;
                ietmMOD.jlname = item.jlname;
                ietmMOD.fqty = item.fqty;
                ietmMOD.CKfname = item.CKfname;
                ietmMOD.PTname = item.PTname;
                ResultMod.Add(ietmMOD);
            }
            List<CashBackStatisticsTotal> footerList = new List<CashBackStatisticsTotal>();
            CashBackStatisticsTotal footer = new CashBackStatisticsTotal();
            footer.jlname = "总计:";
            if (ResultMod.Count() > 0)
                footer.fqty = decimal.Parse(ResultMod.Sum(s => decimal.Parse(s.fqty.ToString())).ToString());
            else
                footer.fqty = 0;
            footerList.Add(footer);
           // string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(ResultMod, Lib.Comm.setTimeFormat()) + "}";
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(ResultMod, setTimeFormat()) + ",\"footer\":" + JsonConvert.SerializeObject(footerList, setTimeFormat()) + "}";
            return Content(json);
        }
        public static IsoDateTimeConverter setTimeFormat()
        {
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return timeFormat;
        }
        public void GetKingdee(string code)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                string sql = "select t_icitem.fnumber as fnumber,t_icitem.fname as fname,t_icitem.fmodel as fmodel,t_measureunit.fname as jlname,fqty as fqty,t_stock.fname as CKfname from icinventory,t_icitem,t_stock,t_measureunit where icinventory.fitemid = t_icitem.fitemid and icinventory.fstockid = t_stock.fitemid and t_measureunit.fmeasureunitid = t_icitem.funitid and  t_icitem.fnumber like '%"+code+"%' order by t_icitem.fnumber asc";
                conn.ConnectionString = "Data Source=222.240.26.98;Initial Catalog=AIS20170114104255;User ID=getstock;Password=getstock";
                conn.Open();
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    listResult ResultModel = new listResult();

                    ResultModel.fnumber= dt.Rows[i]["fnumber"].ToString();
                    ResultModel.fname = dt.Rows[i]["fname"].ToString();
                    ResultModel.fmodel = dt.Rows[i]["fmodel"].ToString();
                    ResultModel.jlname = dt.Rows[i]["jlname"].ToString();
                    ResultModel.fqty =decimal.Parse(dt.Rows[i]["fqty"].ToString());
                    ResultModel.CKfname = dt.Rows[i]["CKfname"].ToString();
                    ResultModel.PTname = "金碟平台数据";
                    list.Add(ResultModel);
                }

            }
            
        }
        public void UpToGY(string queryStr)
        {
            string code = "";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection())
            {
                queryStr = queryStr.PadLeft(5, '0');
                string sql = "select Code from dbo.T_Bas_Goods where Code like '%" + queryStr + "'";
                conn.ConnectionString = "Data Source=120.24.176.207;Initial Catalog=ebms3;User ID=erp_ggpt;Password=erp_ggpt123";
                conn.Open();
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(ds);
                
                 dt = ds.Tables[0];

            }
            for (int x = 0; x < dt.Rows.Count; x++)
            {

               code= dt.Rows[x]["Code"].ToString();
               
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("sid", "hhs2");
                dic.Add("appkey", "hhs2-ot");
                dic.Add("spec_no", code);
                dic.Add("timestamp", GetTimeStamp());
                var aa = CreateParam(dic, true);

                string ret = Post("http://api.wangdian.cn/openapi2/stock_query.php", aa);


                JsonData jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                string sd = jsonData[0].ToString();
                if (sd == "0")
                {
                    int count = int.Parse(jsonData["total_count"].ToString());

                    for (int i = 0; i < count; i++)
                    {
                        listResult ResultModel = new listResult();
                        JsonData stocks = jsonData["stocks"][i];
                        ResultModel.fname = stocks["goods_name"].ToString();
                        ResultModel.fnumber = stocks["goods_no"].ToString();
                        ResultModel.fmodel = stocks["spec_name"].ToString();
                        ResultModel.jlname = "";
                        ResultModel.fqty =decimal.Parse(stocks["stock_num"].ToString());
                        ResultModel.CKfname = stocks["warehouse_name"].ToString();
                        ResultModel.PTname = "旺店通数据";
                        list.Add(ResultModel);
                    }

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
