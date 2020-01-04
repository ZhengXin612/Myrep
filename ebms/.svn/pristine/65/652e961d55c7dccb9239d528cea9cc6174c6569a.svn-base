using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.EnterpriseServices;
using EBMS.App_Code;
using LitJson;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Collections.Specialized;

namespace EBMS.Controllers
{

    public class BasicInformationController : BaseController
    {
        //
        // GET: /BasicInformation/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 视图
        [Description("供应商管理")]
        public ActionResult ViewSuppliersInfoManagement()
        {
            return View();
        }
        [Description("单位管理")]
        public ActionResult ViewUnitInfoManagement()
        {
            return View();
        }
        [Description("快递管理")]
        public ActionResult ViewExpressInfoManagement()
        {
            return View();
        }
        [Description("店铺管理")]
        public ActionResult ViewShopInfoManagement()
        {
            return View();
        }
        [Description("供应商新增")]
        public ActionResult ViewSupplierAdd()
        {
            return View();
        }
        [Description("供应商编辑")]
        public ActionResult ViewSupplierEdit(int ID)
        {
            T_Suppliers model = db.T_Suppliers.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("单位新增")]
        public ActionResult ViewUnitAdd()
        {
            return View();
        }
        [Description("单位编辑")]
        public ActionResult ViewUnitEdit(int ID)
        {
            T_Company model = db.T_Company.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("店铺新增")]
        public ActionResult ViewShopAdd()
        {
            return View();
        }
        [Description("店铺编辑")]
        public ActionResult ViewShopEdit(int ID)
        {
            T_ShopFromGY model = db.T_ShopFromGY.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [Description("快递新增")]
        public ActionResult ViewExpressAdd()
        {
            return View();
        }
        [Description("快递编辑")]
        public ActionResult ViewExpressEdit(int ID)
        {
            T_Express model = db.T_Express.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Description("商品管理")]
        public ActionResult ViewGoodsList()
        {
            return View();
        }

        [Description("仓库管理")]
        public ActionResult ViewWarehouseList()
        {
            return View();
        }

        [Description("借支报销支出列表")]
        public ActionResult ViewExpenseAcountList()
        {
            return View();
        }

        [Description("借支报销支出保存")]
        public ActionResult ViewExpenseAcountAdd(string comPany)
        {
            ViewData["comPany"] = comPany;
            return View();
        }
        [Description("店铺编辑")]
        public ActionResult ViewExpenseAcountEdit(int ID)
        {
            T_ExpenseAcount model = db.T_ExpenseAcount.Find(ID);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        #endregion

        #region 绑定数据

        /// <summary>
        /// 绑定商品数据
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodsList(Lib.GridPager pager, string query,string sel)
        {
            IQueryable<T_WDTGoods> list = db.T_WDTGoods.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            { list = list.Where(s => s.spec_code.Contains(query) || s.spec_name.Contains(query)); }
            if (sel=="1")
            { list = list.Where(s => s.spec_aux_unit_name.Equals("1") ); }
            pager.totalRows = list.Count();
            List<T_WDTGoods> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //获取供应商数据
        public ContentResult GetSupplierList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_Suppliers> queryData = db.T_Suppliers.Where(a => a.Isdelete == "0");
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.SuppliersName != null && a.SuppliersName.Contains(queryStr));
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                List<T_Suppliers> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        //获取单位数据
        public ContentResult GetUnitList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_Company> queryData = db.T_Company.Where(a => a.Isdelete == "0");
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.CompanyName != null && a.CompanyName.Contains(queryStr));
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                List<T_Company> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        //获取店铺数据
        public ContentResult GetShopList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_ShopFromGY> queryData = db.T_ShopFromGY.Where(a => a.Isdelete == "0");
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr));
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                List<T_ShopFromGY> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        //获取快递数据
        public ContentResult GetExpressList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_Express> queryData = db.T_Express.Where(a => a.Isdelete == "0");
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => (a.Name != null && a.Name.Contains(queryStr)) || (a.Code != null && a.Code.Contains(queryStr)));
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                List<T_Express> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        //获取仓库数据
        public ContentResult GetWarehouseList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_Warehouses> queryData = db.T_Warehouses;
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.name != null && a.name.Contains(queryStr));
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                List<T_Warehouses> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }

        /// <summary>
        /// 绑定借支报销支出公司
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="queryStr"></param>
        /// <returns></returns>
        public ContentResult GetExpenseAcountList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_ExpenseAcount> queryData = db.T_ExpenseAcount.AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryStr))
                queryData = queryData.Where(a => a.ComPany.Contains(queryStr));
            pager.totalRows = queryData.Count();
            List<T_ExpenseAcount> list = queryData.OrderBy(a => a.type).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        #endregion

        #region 增删改
        #region 供应商
        public JsonResult SupplierAddSave(T_Suppliers model)
        {

            model.Isdelete = "0";
            db.T_Suppliers.Add(model);
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SupplierEditSave(T_Suppliers model)
        {
            T_Suppliers editModel = db.T_Suppliers.Find(model.ID);
            editModel.SuppliersName = model.SuppliersName;
            editModel.ContactName = model.ContactName;
            editModel.ContactiTelephone = model.ContactiTelephone;
            db.Entry<T_Suppliers>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SupplierDelete(int ID)
        {
            T_Suppliers editModel = db.T_Suppliers.Find(ID);
            editModel.Isdelete = "1";
            db.Entry<T_Suppliers>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region 单位
        public JsonResult UnitAddSave(T_Company model)
        {
            model.Isdelete = "0";
            db.T_Company.Add(model);
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult UnitEditSave(T_Company model)
        {
            T_Company editModel = db.T_Company.Find(model.ID);
            editModel.CompanyName = model.CompanyName;
            editModel.Remarks = model.Remarks;
            db.Entry<T_Company>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult UnitDelete(int ID)
        {
            T_Company editModel = db.T_Company.Find(ID);
            editModel.Isdelete = "1";
            db.Entry<T_Company>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region 店铺
        public JsonResult ShopAddSave(T_ShopFromGY model)
        {
            model.Isdelete = "0";
            db.T_ShopFromGY.Add(model);
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShopEditSave(T_ShopFromGY model)
        {
            T_ShopFromGY editModel = db.T_ShopFromGY.Find(model.ID);
            editModel.code = model.code;
            editModel.company_Name = model.company_Name;
            editModel.DutyFinance = model.DutyFinance;
            editModel.name = model.name;
            editModel.nick = model.nick;
            editModel.number = model.number;
            editModel.type_name = model.type_name;

            db.Entry<T_ShopFromGY>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ShopDelete(int ID)
        {
            T_ShopFromGY editModel = db.T_ShopFromGY.Find(ID);
            editModel.Isdelete = "1";
            db.Entry<T_ShopFromGY>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        Dictionary<string, string> dic = new Dictionary<string, string>();
        private string SetParam()
        {
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());

            return CreateParam(dic, true);
        }

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

        public string StringToUnicode(string str)
        {
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] < 19968 || str[i] > 40869)
                    {
                        outStr += str[i];
                    }
                    else
                    {
                        //将中文字符转为10进制整数，然后转为16进制unicode字符
                        outStr += "\\u" + ((int)str[i]).ToString("x");
                    }

                }
            }
            return outStr;
        }
        public JsonResult ShopTb()
        {
            int page_no = 0;
            int c = 0;
            int s = 0;
            App_Code.GY gy = new App_Code.GY();
            //List<T_WDTshop> delMod = db.T_WDTshop.AsQueryable().ToList();
            //foreach (var item in delMod)
            //{
            //    db.T_WDTshop.Remove(item);
            //}
            //db.SaveChanges();
            do
            {
                
            dic.Clear();
            dic.Add("mine", "0");
            dic.Add("sid", "hhs2");
            dic.Add("appkey", "hhs2-ot");
            dic.Add("timestamp", GetTimeStamp());
            dic.Add("page_no", page_no.ToString());//页号
            var cmd = CreateParam(dic, true);
            string ret = gy.DoPostnew("http://api.wangdian.cn/openapi2/shop.php", cmd, Encoding.UTF8);
            string ssx = Regex.Unescape(ret);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            string iscode = jsonData["code"].ToString();
            if (iscode != "0")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            JsonData jsontrades = jsonData["shoplist"];
                c = jsontrades.Count;
           
            
            for (int i = 0; i < jsontrades.Count; i++)
            {
				string shopno= jsontrades[i]["shop_no"].ToString();
					T_WDTshop model = db.T_WDTshop.FirstOrDefault(a => a.shop_no == shopno);
					if (model == null)
					{
						T_WDTshop WDTshop = new T_WDTshop();
						WDTshop.platform_id = jsontrades[i]["platform_id"].ToString();
						WDTshop.sub_platform_id = jsontrades[i]["sub_platform_id"].ToString();
						WDTshop.shop_id = jsontrades[i]["shop_id"].ToString();
						WDTshop.shop_no = jsontrades[i]["shop_no"].ToString();
						WDTshop.shop_name = jsontrades[i]["shop_name"].ToString();
						WDTshop.account_id = jsontrades[i]["account_id"].ToString();
						WDTshop.account_nick = jsontrades[i]["account_nick"].ToString();
						WDTshop.province = jsontrades[i]["province"].ToString();
						WDTshop.city = jsontrades[i]["city"].ToString();
						WDTshop.district = jsontrades[i]["district"].ToString();
						WDTshop.address = jsontrades[i]["address"].ToString();
						WDTshop.contact = jsontrades[i]["contact"].ToString();
						WDTshop.zip = jsontrades[i]["zip"].ToString();
						WDTshop.mobile = jsontrades[i]["mobile"].ToString();
						WDTshop.telno = jsontrades[i]["telno"].ToString();
						db.T_WDTshop.Add(WDTshop);
						s += db.SaveChanges();
					}
               
            }
                page_no++;
            } while (c !=0);
            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }

		public JsonResult WDTExpressTb()
		{
			int page_no = 0;
			int c = 0;
			int s = 0;
			App_Code.GY gy = new App_Code.GY();
			
			
			//do
			//{

				dic.Clear();
				//dic.Add("mine", "0");
				dic.Add("sid", "hhs2");
				dic.Add("appkey", "hhs2-ot");
				dic.Add("timestamp", GetTimeStamp());
				//dic.Add("page_no", page_no.ToString());//页号
				var cmd = CreateParam(dic, true);
				string ret = gy.DoPostnew("http://121.41.177.115/openapi2/logistics.php", cmd, Encoding.UTF8);
				string ssx = Regex.Unescape(ret);
				JsonData jsonData = null;
				jsonData = JsonMapper.ToObject(ret);
				string iscode = jsonData["code"].ToString();
				if (iscode != "0")
				{
					return Json("", JsonRequestBehavior.AllowGet);
				}
				JsonData jsontrades = jsonData["logistics_list"];
				c = jsontrades.Count;


		
			return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
		}
		public partial class WDTGoodssa
        {
            public int ID { get; set; }
        }
         public JsonResult WDTGoodsTb()
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection())
            {

                string sql = "select * from dbo.T_Bas_Goods ";
                conn.ConnectionString = "Data Source=120.24.176.207;Initial Catalog=ebms3;User ID=erp_ggpt;Password=erp_ggpt123";
                conn.Open();
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(ds);

                dt = ds.Tables[0];

                List<T_WDTGoods> list = db.T_WDTGoods.ToList();

                //foreach (var deleteItem in list)
                //{
                //    db.T_WDTGoods.Remove(deleteItem);
                //}
               // db.SaveChanges();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string code = dt.Rows[i]["Code"].ToString();
                    T_WDTGoods lisst = list.FirstOrDefault(s => s.goods_no == code);
                    if (lisst == null)
                    {
                        T_WDTGoods WDTGoods = new T_WDTGoods();
                        WDTGoods.goods_no = dt.Rows[i]["Code"].ToString();
                        WDTGoods.goods_name = dt.Rows[i]["Name"].ToString();
                        WDTGoods.short_name = dt.Rows[i]["Name"].ToString();
                        WDTGoods.goods_type = 1;
                        WDTGoods.spec_count = 1;
                        WDTGoods.spec_code = dt.Rows[i]["Code"].ToString();
                        WDTGoods.spec_no = dt.Rows[i]["Code"].ToString();
                        WDTGoods.barcode = dt.Rows[i]["BarCode"].ToString();
                        WDTGoods.spec_name = dt.Rows[i]["Spec"].ToString();
                        WDTGoods.lowest_price = decimal.Parse(dt.Rows[i]["Price"].ToString());
                        WDTGoods.wholesale_price = decimal.Parse(dt.Rows[i]["Price"].ToString());
                        WDTGoods.retail_price = decimal.Parse(dt.Rows[i]["Price"].ToString());
                        db.T_WDTGoods.Add(WDTGoods);
                        db.SaveChanges();
                    }
                }
            }
            // App_Code.GY gy = new App_Code.GY();
            // dic.Clear();
            //// dic.Add("spec_no", "80005200026840");
            // dic.Add("sid", "hhs2");
            // dic.Add("appkey", "hhs2-ot");
            // dic.Add("timestamp", GetTimeStamp());
            // var cmd = CreateParam(dic, true);
            // string ret = gy.DoPostnew("http://121.41.177.115/openapi2/goods_query.php", cmd, Encoding.UTF8);
            // string ssx = Regex.Unescape(ret);
            // JsonData jsonData = null; 
            // jsonData = JsonMapper.ToObject(ret);
            //string iscode = jsonData["code"].ToString();
            //if (iscode != "0")
            //{
            //    return Json("", JsonRequestBehavior.AllowGet);
            //}
            //JsonData jsontrades = jsonData["shoplist"];
            //List<T_WDTshop> delMod = db.T_WDTshop.AsQueryable().ToList();
            //foreach (var item in delMod)
            //{
            //    db.T_WDTshop.Remove(item);
            //}
            //db.SaveChanges();
            //int s = 0;
            //for (int i = 0; i < jsontrades.Count; i++)
            //{
            //    T_WDTshop WDTshop = new T_WDTshop();
            //    WDTshop.platform_id = jsontrades[i]["platform_id"].ToString();
            //    WDTshop.sub_platform_id = jsontrades[i]["sub_platform_id"].ToString();
            //    WDTshop.shop_id = jsontrades[i]["shop_id"].ToString();
            //    WDTshop.shop_no = jsontrades[i]["shop_no"].ToString();
            //    WDTshop.shop_name = jsontrades[i]["shop_name"].ToString();
            //    WDTshop.account_id = jsontrades[i]["account_id"].ToString();
            //    WDTshop.account_nick = jsontrades[i]["account_nick"].ToString();
            //    WDTshop.province = jsontrades[i]["province"].ToString();
            //    WDTshop.city = jsontrades[i]["city"].ToString();
            //    WDTshop.district = jsontrades[i]["district"].ToString();
            //    WDTshop.address = jsontrades[i]["address"].ToString();
            //    WDTshop.contact = jsontrades[i]["contact"].ToString();
            //    WDTshop.zip = jsontrades[i]["zip"].ToString();
            //    WDTshop.mobile = jsontrades[i]["mobile"].ToString();
            //    WDTshop.telno = jsontrades[i]["telno"].ToString();
            //    db.T_WDTshop.Add(WDTshop);
            //    s += db.SaveChanges();
            //}
            //if (s > 0)
            //{
            //    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            //}
            return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
        }

       
        //单个产品同步
        public JsonResult WDTGoodsTbS(string query)
        {
            if (query == "" || query == null)
            {
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
            App_Code.GY gy = new App_Code.GY();
			JsonData jsonData = null;
			 //string ret = gy.httpGetStr("http://192.168.8.89:3980/material/GetMaterialData?code=" + query.Trim());
			try
			{
				string ret = gy.httpGetStr("http://222.240.26.98:3980/material/GetMaterialData?code=" + query.Trim());
				string ssx = Regex.Unescape(ret);

				jsonData = JsonMapper.ToObject(ret);
			}
			catch (WebException ex)
			{
				return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
              
               
			//if (jsonData[0][0]==null)
			//{
			//    return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);

			//}
			if (jsonData!=null&&jsonData.Count > 0)
			{
				for (int i = 0; i < jsonData.Count; i++)
				{
					string SPEC = "";
					if (jsonData[i]["SPEC"] != null)
					{
						SPEC = jsonData[i]["SPEC"].ToString();

					}

					string NAME = jsonData[i]["NAME"].ToString();
					string UNIT = jsonData[i]["UNIT"].ToString();
					string BARCODE = "";
					if (jsonData[i]["BARCODE"] != null)
					{
						BARCODE = jsonData[i]["BARCODE"].ToString();

					}
					string code = jsonData[i]["CODE"].ToString(); ;
					T_WDTGoods list = db.T_WDTGoods.FirstOrDefault(s => s.goods_no == code);
					if (list == null)
					{
						T_WDTGoods WDTGoods = new T_WDTGoods();
						WDTGoods.goods_no = code;
						WDTGoods.goods_name = NAME;
						WDTGoods.short_name = NAME;
						WDTGoods.goods_type = 1;
						WDTGoods.unit_name = UNIT;
						WDTGoods.spec_count = 1;
						WDTGoods.spec_code = code;
						WDTGoods.spec_no = code;
						WDTGoods.barcode = BARCODE;
						WDTGoods.spec_name = SPEC;
						WDTGoods.lowest_price = 0;
						WDTGoods.wholesale_price = 0;
						WDTGoods.retail_price = 0;
						db.T_WDTGoods.Add(WDTGoods);
						db.SaveChanges();
					}
					else
					{

						list.unit_name = UNIT;
						list.spec_name = SPEC;
						db.Entry<T_WDTGoods>(list).State = System.Data.Entity.EntityState.Modified;
						db.SaveChanges();
					}

				}

				return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				return Json(new { State = "Faile", Message="NC不存在"+ query }, JsonRequestBehavior.AllowGet);
			}
			
        }


        [HttpPost]
        //产品禁用/启用
        public JsonResult DisableEditSave(int ids, int type)
        {
            //string username = Server.UrlDecode(Request.Cookies["username"].Value);
            T_WDTGoods model = db.T_WDTGoods.SingleOrDefault(a => a.ID == ids);// 
            int s = 0;
            if (type == 1)//禁用
            {
                model.spec_aux_unit_name = "1";
                db.Entry<T_WDTGoods>(model).State = System.Data.Entity.EntityState.Modified;
            }
            if (type == 2)//启用
            {
                model.spec_aux_unit_name = null;
                db.Entry<T_WDTGoods>(model).State = System.Data.Entity.EntityState.Modified;
            }
            T_OperaterLog log = new T_OperaterLog()
            {
                Module = "启用/禁用",
                OperateContent = type+"产品"+ model.goods_no+ "1禁用2启用产品" ,
                Operater = Server.UrlDecode(Request.Cookies["Nickname"].Value),
                OperateTime = DateTime.Now,
                PID = ids
            };
            db.T_OperaterLog.Add(log);
            
            s = db.SaveChanges();
            if (s > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region 快递
        public JsonResult ExpressAddSave(T_Express model)
        {
            model.Isdelete = "0";
            db.T_Express.Add(model);
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ExpressEditSave(T_Express model)
        {
            T_Express editModel = db.T_Express.Find(model.ID);
            editModel.Name = model.Name;
            editModel.Code = model.Code;
            db.Entry<T_Express>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ExpressDelete(int ID)
        {
            T_Express editModel = db.T_Express.Find(ID);
            editModel.Isdelete = "1";
            db.Entry<T_Express>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region 报销借支账号
        [HttpPost]
        public JsonResult ViewExpenseAcountAddSave(string comPany, T_ExpenseAcount model)
        {
            try
            {
                T_ExpenseAcount account = db.T_ExpenseAcount.SingleOrDefault(s => s.ComPany.Equals(comPany));
                T_ExpenseAcount models = new T_ExpenseAcount
                {
                    type = account.type,
                    Number = model.Number
                };
                db.T_ExpenseAcount.Add(models);
                db.SaveChanges();
                return Json(new { State = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { State = "Faile", Message = ex.Message });
            }
        }
        public JsonResult ExpenseAcountDelete(int ID)
        {
            T_ExpenseAcount editModel = db.T_ExpenseAcount.Find(ID);
            db.T_ExpenseAcount.Remove(editModel);
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ExpenseAcountEditSave(T_ExpenseAcount model)
        {
            T_ExpenseAcount editModel = db.T_ExpenseAcount.Find(model.ID);
            editModel.ComPany = model.ComPany;
            editModel.Number = model.Number;

            db.Entry<T_ExpenseAcount>(editModel).State = System.Data.Entity.EntityState.Modified;
            try
            {
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region 同步管易商品
        public ContentResult TBguanyi()
        {

           GY gy = new GY();

            string cmd = "{" +
                           "\"appkey\":\"171736\"," +
                           "\"method\":\"gy.erp.items.get\"," +
                           "\"page_no\":1," +
                           "\"page_size\":50," +

                           "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                           "}";

            string sign = gy.Sign(cmd);
            cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
            string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
            JsonData jsonData = null;
            jsonData = JsonMapper.ToObject(ret);
            //   jsonData["success"]
            int shuliang = int.Parse(jsonData["total"].ToString());
            int i = 0;
            int yeshu = shuliang / 50 + (shuliang % 50 == 0 ? 0 : 1);
            for (int x = 1; x <= yeshu; x++)
            {
                cmd = "{" +
                       "\"appkey\":\"171736\"," +
                       "\"method\":\"gy.erp.items.get\"," +
                       "\"page_no\":" + x + "," +
                       "\"page_size\":50," +

                       "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                       "}";

                sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                jsonData = null;
                jsonData = JsonMapper.ToObject(ret);
                T_goodsGY model = new T_goodsGY();
                int ss = 50;
                if (x == yeshu)
                {
                    ss = int.Parse(shuliang.ToString().Substring(shuliang.ToString().Length - 2, 2));

                }
                for (int z = 0; z < ss; z++)
                {
                    i++;
                    JsonData skus = jsonData["items"][z];

                    DateTime create_date = new DateTime();
                    string name = "";
                    string code = "";
                    string note = "";
                    string combine = "";
                    double weight = 0;
                    string simple_name = "";
                    string category_code = "";
                    string category_name = "";
                    string supplier_code = "";
                    string item_unit_code = "";
                    double package_point = 0;
                    double sales_point = 0;
                    decimal sales_price = 0;
                    decimal purchase_price = 0;
                    decimal agent_price = 0;
                    decimal cost_price = 0;
                    string pic_url = "";
                    string stock_status_code = "";
                    if (skus["create_date"] != null)
                    {
                        create_date = DateTime.Parse(skus["create_date"].ToString());
                    }
                    if (skus["name"] != null)
                    {
                        name = skus["name"].ToString();
                    }
                    if (skus["code"] != null)
                    {
                        code = skus["code"].ToString();
                    }
                    if (skus["note"] != null)
                    {
                        note = skus["note"].ToString();
                    }
                    if (skus["combine"] != null)
                    {
                        combine = skus["combine"].ToString();

                    }
                    if (skus["weight"] != null)
                    {
                        weight = double.Parse(skus["weight"].ToString());

                    }
                    if (skus["simple_name"] != null)
                    {
                        simple_name = skus["simple_name"].ToString();

                    }
                    if (skus["category_code"] != null)
                    {
                        category_code = skus["category_code"].ToString();

                    }
                    if (skus["category_name"] != null)
                    {
                        category_name = skus["category_name"].ToString();

                    }
                    if (skus["supplier_code"] != null)
                    {
                        supplier_code = skus["supplier_code"].ToString();

                    }
                    if (skus["item_unit_code"] != null)
                    {
                        item_unit_code = skus["item_unit_code"].ToString();

                    }
                    if (skus["package_point"] != null)
                    {
                        package_point = double.Parse(skus["package_point"].ToString());

                    }
                    if (skus["sales_point"] != null)
                    {
                        sales_point = double.Parse(skus["sales_point"].ToString());

                    }
                    if (skus["sales_price"] != null)
                    {
                        sales_price = decimal.Parse(skus["sales_price"].ToString());

                    }
                    if (skus["purchase_price"] != null)
                    {
                        purchase_price = decimal.Parse(skus["purchase_price"].ToString());

                    }
                    if (skus["agent_price"] != null)
                    {
                        agent_price = decimal.Parse(skus["agent_price"].ToString());

                    }
                    if (skus["cost_price"] != null)
                    {
                        cost_price = decimal.Parse(skus["cost_price"].ToString());


                    }
                    if (skus["pic_url"] != null)
                    {
                        pic_url = skus["pic_url"].ToString();


                    }
                    if (skus["stock_status_code"] != null)
                    {
                        stock_status_code = skus["stock_status_code"].ToString();

                    }
                    List<T_goodsGY> Querymodel = db.T_goodsGY.Where(a => a.code == code).ToList();
                    if (Querymodel.Count == 0)
                    {
                        model.create_date = create_date;
                        model.name = name;
                        model.code = code;
                        model.note = note;
                        model.weight = weight;
                        model.combine = combine;
                        model.simple_name = simple_name;
                        model.category_code = category_code;
                        model.category_name = category_name;
                        model.supplier_code = supplier_code;
                        model.item_unit_code = item_unit_code;
                        model.package_point = package_point;
                        model.sales_point = sales_point;
                        model.sales_price = sales_price;
                        model.purchase_price = purchase_price;
                        model.agent_price = agent_price;
                        model.cost_price = cost_price;
                        model.stock_status_code = stock_status_code;
                        model.pic_url = pic_url;
                        db.T_goodsGY.Add(model);
                        db.SaveChanges();

                    }
                }

            }

            if (i == shuliang)
            {
                return Content(i.ToString());
            }
            else
            {
                return Content("0");
            }

        }
        #endregion
        #endregion
    }
}
