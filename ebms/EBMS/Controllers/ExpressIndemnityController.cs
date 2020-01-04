using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using Newtonsoft.Json;
using EBMS.App_Code;
using System.Data.Entity.Validation;
using System.Transactions;
using System.IO;
using System.Data.Entity.Infrastructure;
using LitJson;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace EBMS.Controllers
{
    public class ExpressIndemnityController : BaseController
    {
        //
        // GET: /ExpressIndemnity/
        EBMSEntities db=new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        public List<SelectListItem> GetStateList()
        {
            List<SelectListItem> State = new List<SelectListItem> 
            {
                 new SelectListItem { Text = "==可选择处理结果==", Value = "" },
                 new SelectListItem { Text = "未处理", Value = "0"},
                 new SelectListItem { Text = "已赔付", Value = "1"},
                 new SelectListItem { Text = "已仲裁", Value = "2"},
                 new SelectListItem { Text = "财务直接扣款", Value = "3" },
                 new SelectListItem { Text = "无法仲裁", Value = "4" },
                 new SelectListItem { Text = "货物已追回", Value = "5" },
                 new SelectListItem { Text = "弃件", Value = "6" }
            };
            return State;
        }
        //绑定快递记录无默认值
        public List<SelectListItem> GetExpressTypeList()
        {
            List<SelectListItem> ExpressType = new List<SelectListItem>
            {
                 new SelectListItem { Text = "==可选择处理结果==", Value = "" },
                 new SelectListItem { Text = "未处理", Value = "wcl"},
                 new SelectListItem { Text = " 已仲裁", Value = "已仲裁"},
                 new SelectListItem { Text = "已答应私了但未赔付", Value = "已答应私了但未赔付"},
                 new SelectListItem { Text = "已QQ发快递客服核实", Value = "已QQ发快递客服核实"},
            };
            return ExpressType;
        }
        public List<SelectListItem> GetSourceList()
        {
            List<SelectListItem> State = new List<SelectListItem> 
            {
                 new SelectListItem { Text = "==可选择数据来源==", Value = "" },
                 new SelectListItem { Text = "补发", Value = "补发"},
                 new SelectListItem { Text = "换货", Value = "换货"},
                 new SelectListItem { Text = "快递赔付", Value = "快递赔付"},
                
            };
            return State;
        }
        public List<SelectListItem> GetDatatypeList()
        {
            List<SelectListItem> State = new List<SelectListItem> 
            {
                 new SelectListItem { Text = "==可选择类型==", Value = "" },
                 new SelectListItem { Text = "破损", Value = "破损"},
                 new SelectListItem { Text = "丢件", Value = "丢件"},
                 new SelectListItem { Text = "弃件", Value = "弃件"},
                  new SelectListItem { Text = "延误", Value = "延误"},
                
            };
            return State;
        }
        public List<SelectListItem> getTypeList()
        {
            List<SelectListItem> typeList = new List<SelectListItem>{
             new SelectListItem{Text="钱款去向",Value="钱款去向"},
             new SelectListItem{Text="地址",Value="地址"}
             };
            return typeList;
        }
        #endregion
        #region 视图
        [Description("快递赔付申请")]
        public ActionResult ViewExpressIndemnityApply(string retreat, string dingdantype)
        {
            //var shoplist = db.T_ShopFromGY.AsQueryable();
            //var shopselectList = new SelectList(shoplist, "name", "name");
            //List<SelectListItem> shopselecli = new List<SelectListItem>();
            //shopselecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            //shopselecli.AddRange(shopselectList);
            ViewData["shop"] = Com.Shop();
            var ExpressNamelist = db.T_Express.AsQueryable();
            var ExpressNameselectList = new SelectList(ExpressNamelist, "Name", "Name");
            List<SelectListItem> ExpressNameselecli = new List<SelectListItem>();
            ExpressNameselecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            ExpressNameselecli.AddRange(ExpressNameselectList);
            ViewData["ExpName"] = ExpressNameselecli;
             return View();
        }
         [Description("快递赔付待处理")]
         public ActionResult ViewExpressIndemnityApproveList()
         {
             ViewData["Datatype"] = GetDatatypeList();
             ViewData["Source"] = GetSourceList();
            ViewData["ExpressType"] = GetExpressTypeList();
            ViewData["ExpressName"] = Com.GetExpressName();
            return View();
         }

        [Description("快递记录")]
        public ActionResult ExpressIndemnityJilu(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("快递记录新增")]
        public ActionResult ExpressIndemnityJiluAdd(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("快递记录新增保存")]
        public JsonResult JiluSave(T_ExpressIndemnityRecord model, string id, string selected_val)
        {
            int ID = int.Parse(id);
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            model.TrackRecord_Name = name;
            model.TrackRecord_Date = DateTime.Now;
            model.Oid = ID;
            model.TrackRecord_Situation = selected_val;
            db.T_ExpressIndemnityRecord.Add(model);

            int i = db.SaveChanges();


            return Json(i, JsonRequestBehavior.AllowGet);
        }
        //[Description("快递记录删除")]
        //public JsonResult JiluDel(int ID)
        //{
        //    T_InterceptExpressRecord editModel = db.T_InterceptExpressRecord.Find(ID);
        //    db.T_InterceptExpressRecord.Remove(editModel);
        //    try
        //    {
        //        db.SaveChanges();
        //        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [Description("快递记录数据查询")]
        public ContentResult GetExpressIndemnityRecord(Lib.GridPager pager, string ID)
        {
            int id = int.Parse(ID);
            IQueryable<T_ExpressIndemnityRecord> queryData = db.T_ExpressIndemnityRecord.Where(a => a.Oid == id);

            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ExpressIndemnityRecord> list = new List<T_ExpressIndemnityRecord>();
            foreach (var item in queryData)
            {
                T_ExpressIndemnityRecord i = new T_ExpressIndemnityRecord();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);


        }
        [Description("我的快递赔付")]
         public ActionResult ViewExpressIndemnityMyList()
         {
            ViewData["State"] = GetStateList();
            return View();
         }
         [Description("快递赔付已处理")]
        public ActionResult ViewExpressIndemnityApprovedList()
        {
            return View();
        }
         [Description("快递赔付统计")]
         public ActionResult ViewExpressIndemnityCountList()
         {
             ViewData["State"] = GetStateList();
             ViewData["Datatype"] = GetDatatypeList();
             ViewData["Source"] = GetSourceList();
             return View();
         }
         [Description("快递赔付编辑")]
         public ActionResult ViewExpressIndemnityEdit(int ID)
         {
             T_ExpressIndemnity model = db.T_ExpressIndemnity.Find(ID);
             if (model != null)
             {
                 return View(model);
             }
             else
             {
                 return HttpNotFound();
             }
         }
        [Description("快递赔付处理")]
         public ActionResult ViewExpressIndemnityCheck(int ID, int second, double Omoney, string ExpName)
         {
            
             var list = db.T_ExpressIndemnityConfig.Where(a=>a.Step==-1).AsQueryable();
             var moneyWhereList = new SelectList(list.Where(a=>a.Type=="钱款去向"), "Name", "Name");
             List<SelectListItem> selectI = new List<SelectListItem>();
            
             selectI.AddRange(moneyWhereList);
             ViewData["moneyWhere"] = selectI;

             var AddressList = new SelectList(list.Where(a => a.Type == "地址"), "Name", "Name");
             List<SelectListItem> selectAddress = new List<SelectListItem>();
             selectAddress.AddRange(AddressList);
             ViewData["addressList"] = selectAddress;
             T_ExpressIndemnity model = db.T_ExpressIndemnity.Find(ID);
             //T_ExpressIndemnityApprove model = new T_ExpressIndemnityApprove();
             //model.EID = ID;
             //if (second == 1)
             //{
             //    model.Step = 2;
             //}
             //else
             //{
             //    model.Step = 1;
             //}
             ViewData["second"] = second;
             ViewData["Omoney"] = Omoney;
             ViewData["ExpName"] = ExpName;
             return View(model);
         }
        public ActionResult ViewLoadImg(int ID)
        {
           
            ViewData["EID"] = ID;
           return View();
        }
        [Description("快递赔付再处理")]
        public ActionResult ViewExpressIndemnityApproveSecond()
        {
            return View();
        }
        [Description("审核详情")]
        public ActionResult ViewExpressIndemnityApproveDetail(int ID)
        {
            ViewData["EID"] = ID;
            return View();
        }
         [Description("快递赔付配置维护")]
        public ActionResult ViewExpressIndemnityConfig()
        {
            return View();
        }
         [Description("快递赔付配置编辑")]
         public ActionResult ViewExpressIndemnityConfigEdit(int ID)
         {
             T_ExpressIndemnityConfig model = db.T_ExpressIndemnityConfig.Find(ID);
             ViewData["typeList"] = getTypeList();
             if (model != null)
             {
                 return View(model);
             }
             else
             {
                 return HttpNotFound();
             }
         }
         [Description("快递赔付配置新增")]
         public ActionResult ViewExpressIndemnityConfigAdd()
         {
             ViewData["typeList"] = getTypeList();
             return View();
         }
        #endregion
        #region 绑定数据

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
       
         public JsonResult GetInfoByOrder(string Num)//根据订单号或物流单号在订单表查询订单信息
         {
            try
            {
                GY gy = new GY();
                    dic.Clear();
                    dic.Add("src_tid", Num);
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
                            string receiver_address = trades["receiver_address"].ToString();
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
                            string paid = trades["paid"].ToString();
                            //快递公司名称
                            string logistics_name = trades["logistics_name"].ToString();
                            //商品详情
                            JsonData goods_list = trades["goods_list"];
                            //查询3次。对应到具体的省市区
                            if (receiver_province != null)
                            {
                                DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_province);
                                if (commonarea != null)
                                {
                                    receiver_province = commonarea.REGION_NAME;
                                }
                            }
                            if (receiver_city != null)
                            {
                                DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_city);
                                if (commonarea != null)
                                {
                                    receiver_city = commonarea.REGION_NAME;
                                }

                                if (receiver_city == "市辖区")
                                {
                                    receiver_city = receiver_province;
                                    receiver_province = receiver_province.Substring(0, receiver_province.Length - 1);
                                }
                            }
                            if (receiver_district != null)
                            {
                                DEMO_REGION commonarea = db.DEMO_REGION.SingleOrDefault(a => a.REGION_CODE == receiver_district);
                                if (commonarea != null)
                                {
                                    receiver_district = commonarea.REGION_NAME;
                                }
                            }
                            string ssq = receiver_province + "-" + receiver_city + "-" + receiver_district;
                            //查询一次..
                            string shop_Code = "";
                            if (shop_name != null)
                            {
                                T_WDTshop commonarea = db.T_WDTshop.SingleOrDefault(a => a.shop_name == shop_name);
                                shop_Code = commonarea.shop_no;
                                //shop_Code = "tm004";
                            }
                            T_ExpressIndemnity inde = new T_ExpressIndemnity
                            {
                                OrderNum = Num,
                                ShopName = shop_name,
                                OrderMoney = double.Parse(paid),
                                wangwang = customer_name,
                                RetreatExpressNum = logistics_no,
                                ExpressName = logistics_name
                            };
                            List<T_ExpressIndemnity> list = db.T_ExpressIndemnity.Where(a => a.OrderNum == Num && a.IsDelete == 0).ToList();
                            string repeat = "";
                            if (list.Count > 0)
                            {
                                repeat = "注意：已存在该订单号相关的快递赔付";
                            }
                            return Json(new { State = "Success", ModelList = inde, repeat = repeat }, JsonRequestBehavior.AllowGet);
                        }
                    }
                return Json(new { State = "Fail", Message = "请自行输入相关信息" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Fail", Message = "请自行输入相关信息" }, JsonRequestBehavior.AllowGet);
            }
            
         }

   /// <summary>
   /// 快递赔付再处理
   /// </summary>
            public partial class ExpressIndemnityShenHe
        {
                public int ID { get; set; }
                public string PostUserName { get; set; }
                public Nullable<System.DateTime> Date { get; set; }
                public string OrderNum { get; set; }
                public string wangwang { get; set; }
                public string RetreatReason { get; set; }
                public string ShopName { get; set; }
                public string RetreatExpressNum { get; set; }
                public string Memo { get; set; }
                public string State { get; set; }
                public Nullable<double> OrderMoney { get; set; }
                public string Type { get; set; }
                public string Second { get; set; }
                public string Pic { get; set; }
                public string CurrentApproveName { get; set; }
                public int IsDelete { get; set; }
                public string ExpressName { get; set; }
                public Nullable<double> IndemnityMoney { get; set; }
                public string Address { get; set; }
                public string MoneyWhereAbout { get; set; }
                public string Module { get; set; }
                public string Repeat { get; set; }
                public string KDJL { get; set; }
        }
        public ContentResult GetExpressIndemnityList(Lib.GridPager pager, string queryStr, string ExpressType, string ExpressName, string isApprove, string isSecond, string State, DateTime? startSendTime, DateTime? endSendTime,string Source,string Datatype, int isMy = 0)//获取快递赔付数据
         {
             //IQueryable<T_ExpressIndemnity> queryData = db.T_ExpressIndemnity.Where(a=>a.IsDelete==0);
              string name=Server.UrlDecode(Request.Cookies["NickName"].Value);

            string sql = "select  *,isnull((select top 1 TrackRecord_Situation from T_ExpressIndemnityRecord where Oid=r.ID order by T_ExpressIndemnityRecord.ID desc ),'') as KDJL From T_ExpressIndemnity r where Isdelete = '0'";
			//  IQueryable<ExpressIndemnityShenHe> queryData = db.Database.SqlQuery<ExpressIndemnityShenHe>(sql).AsQueryable();
			IQueryable<T_ExpressIndemnity> queryData = db.T_ExpressIndemnity.Where(a => a.IsDelete == 0);
			if (isApprove == "0")//未处理
             {
                 queryData = queryData.Where(a => a.State == "0");
             }
             else if (isApprove == "1")//已处理
             {
                 queryData = queryData.Where(a => a.State != "0");
             }
             if (isMy == 1)
             {
                 queryData = queryData.Where(a => a.PostUserName == name);
             }
             if (isSecond == "0")
             {
                 queryData = queryData.Where(a => a.Second == isSecond&&a.State!="0");
             }
             if (!string.IsNullOrWhiteSpace(Source))
             {
                 queryData = queryData.Where(a => a.Module == Source);
             }
             if (!string.IsNullOrWhiteSpace(Datatype))
             {
                 queryData = queryData.Where(a => a.Type == Datatype);
             }
             if (!string.IsNullOrWhiteSpace(State))
             {
                 queryData = queryData.Where(a => a.State == State);
             }
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => (a.OrderNum != null && a.OrderNum.Equals(queryStr))||(a.RetreatExpressNum!=null&&a.RetreatExpressNum.Equals(queryStr)));
             }
             if (startSendTime != null)
             {
                 queryData = queryData.Where(a => a.Date >= startSendTime);
             }
             if (endSendTime != null)
             {
                 endSendTime = endSendTime.Value.AddDays(1);
                 queryData = queryData.Where(a => a.Date <= endSendTime);
             }
			if (!string.IsNullOrWhiteSpace(ExpressName) && !ExpressName.Equals("==请选择=="))
			{
				queryData = queryData.Where(a => a.ExpressName.Equals(ExpressName));
			}
			List<int> RecordID = db.T_ExpressIndemnityRecord.GroupBy(a => a.Oid).Select(g => g.Max(k => k.ID)).ToList();
			IQueryable<T_ExpressIndemnityRecord> queryDataRecord = db.T_ExpressIndemnityRecord.Where(a=>RecordID.Contains(a.ID));
			List<T_ExpressIndemnityRecord> listRecord = new List<T_ExpressIndemnityRecord>();
			if (ExpressType != null && ExpressType != "") //快递货物追踪状态查询条件

			{

				if (ExpressType != "wcl")
				{
					queryDataRecord = queryDataRecord.Where(a => a.TrackRecord_Situation == ExpressType);
					listRecord = queryDataRecord.ToList();
					List<int?> Oids = listRecord.Select(a => a.Oid).ToList();
					queryData = queryData.Where(a => Oids.Contains(a.ID));
				}
				else
				{
					listRecord = queryDataRecord.ToList();
					List<int?> Oids = listRecord.Select(a => a.Oid).ToList();
					queryData = queryData.Where(a => !Oids.Contains(a.ID));
				}

				//if (ExpressType == "wcl")
				//            {
				//                queryData = queryData.Where(a => a.KDJL == "");
				//            }
				//            else
				//            {
				//                queryData = queryData.Where(a => a.KDJL == ExpressType);
				//            }
			}
			else
			{
				listRecord = queryDataRecord.ToList();
			}
			
			
			if (queryData != null )
             {
				pager.totalRows = queryData.Count();
				List<T_ExpressIndemnity> listIndemnity = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
				
				//List<int?> Oids = listRecord.Select(a => a.Oid).ToList();
				var list = (from a in listIndemnity
							 join r in listRecord
							 on a.ID equals r.Oid into
							 IndemnityShenHeList
							 from b in IndemnityShenHeList.DefaultIfEmpty(new T_ExpressIndemnityRecord() { TrackRecord_Situation=""})
							 select new  {
								 ID = a.ID,
								 Address = a.Address,
								 
								 CurrentApproveName = a.CurrentApproveName,
								 Date = a.Date,
								 ExpressName = a.ExpressName,
								
								 IndemnityMoney = a.IndemnityMoney,
								 IsDelete = a.IsDelete,
								 KDJL =   b.TrackRecord_Situation,
								 Memo = a.Memo,
								 Module = a.Module,
								 MoneyWhereAbout = a.MoneyWhereAbout,
								 OrderMoney = a.OrderMoney,
								 OrderNum = a.OrderNum,
								 Pic = a.Pic,
								 PostUserName = a.PostUserName,
								 Repeat = a.Repeat,
								 RetreatExpressNum = a.RetreatExpressNum,
								 RetreatReason = a.RetreatReason,
								 Second = a.Second,
								 ShopName = a.ShopName,
								 State = a.State,
								 Type = a.Type,
								 wangwang = a.wangwang
								}
							).ToList();



				//List<ExpressIndemnityShenHe> list = listIndemnity.Join(listRecord, a => a.ID, b => b.Oid, (a, b) => new ExpressIndemnityShenHe()
				//{
				//	Address=a.Address,
				//	CurrentApproveName=a.CurrentApproveName,
				//	Date=a.Date,
				//	ExpressName=a.ExpressName,
				//	ID=a.ID,
				//	IndemnityMoney=a.IndemnityMoney,
				//	IsDelete=a.IsDelete,
				//	KDJL=b.TrackRecord_Situation,
				//	Memo=a.Memo,
				//	Module=a.Module,
				//	MoneyWhereAbout=a.MoneyWhereAbout,
				//	OrderMoney=a.OrderMoney,
				//	OrderNum=a.OrderNum,
				//	Pic=a.Pic,
				//	PostUserName=a.PostUserName,
				//	Repeat=a.Repeat,
				//	RetreatExpressNum=a.RetreatExpressNum,
				//	RetreatReason=a.RetreatReason,
				//	Second=a.Second,
				//	ShopName=a.ShopName,
				//	State=a.State,
				//	Type=a.Type,
				//	wangwang=a.wangwang
				
				//}).ToList();
					//  List<ExpressIndemnityShenHe> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
				string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             else
             {
                 return Content("");
             }
         }
         public JsonResult GetPicUrl(int EID)
         {
             string[] PicUrls = (from Urls in db.T_ExpressIndemnityPic where Urls.EID == EID select Urls.PicURL).ToArray();
             List<T_ExpressIndemnityPic> PicList = db.T_ExpressIndemnityPic.Where(a => a.EID == EID).ToList();
             if (PicList.Count > 0)
             {
                 return Json(PicList);
             }
             else
             {
                 return Json(0);
             }
         }
         public ContentResult GetExpressIndemnityApproveList(int EID)//获取快递赔付审核数据
         {
             List<T_ExpressIndemnityApprove> queryData = db.T_ExpressIndemnityApprove.Where(a=>a.EID==EID).ToList();
            
             if (queryData != null )
             {
                 string json = "{\"total\":" + queryData.Count() + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             return Content("");
         }
         public ContentResult GetExpressIndemnityConfigList(Lib.GridPager pager, string queryStr)//获取快递赔付配置表数据
         {
             IQueryable<T_ExpressIndemnityConfig> queryData = db.T_ExpressIndemnityConfig;
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
             }
             if (queryData != null)
             {
                 pager.totalRows = queryData.Count();
                 List<T_ExpressIndemnityConfig> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                 string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                 return Content(json);
             }
             else
             {
                 return Content("");
             }
         }
        #endregion
        #region 增删改
         public JsonResult ExpressIndemnityApplySave(T_ExpressIndemnity model, string picUrls)//新增保存
         {
             using (TransactionScope sc = new TransactionScope())
             {
                 try {
                     //T_OrderList Order = db.T_OrderList.FirstOrDefault(a => a.platform_code == model.OrderNum || a.mail_no == model.RetreatExpressNum);
                     string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                     model.PostUserName = name;
                     model.Date = DateTime.Now;
                     model.Second = "0";
                     model.IsDelete = 0;
                     model.Address = "";
                     model.IndemnityMoney = 0;
                     model.MoneyWhereAbout = "";
                     model.Module = "快递赔付";
                     if (model.Type == "弃件")//弃件则直接处理了
                     {
                         model.State = "6";
                        
                         model.CurrentApproveName = name;
                         //if (Order != null)
                         //{
                         //    Order.Status_ExpressIndemnity = 2;
                         //    db.Entry<T_OrderList>(Order).State = System.Data.Entity.EntityState.Modified;
                         //}
                     }
                     else
                     {
                         model.State = "0";
                         model.CurrentApproveName = "快递组";
                         //if (Order != null)
                         //{
                         //    Order.Status_ExpressIndemnity = 1;
                         //    db.Entry<T_OrderList>(Order).State = System.Data.Entity.EntityState.Modified;
                         //}
                     }
                     db.T_ExpressIndemnity.Add(model);
                     int i = db.SaveChanges();

                     if (picUrls.Length > 0)
                     {
                         string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                         foreach (string Purl in picArr)
                         {
                             T_ExpressIndemnityPic IndemnityPic = new T_ExpressIndemnityPic();
                             IndemnityPic.EID = model.ID;
                             IndemnityPic.PicURL = Purl;
                             db.T_ExpressIndemnityPic.Add(IndemnityPic);
                         }
                         db.SaveChanges();
                     }
                     if (model.Type == "弃件")//弃件流程申请时，同时该申请人为审核人,生成无钱款去向的一条审核记录
                     {
                         T_ExpressIndemnityApprove approve = new T_ExpressIndemnityApprove();
                         approve.EID = model.ID;
                         approve.State = "6";
                         approve.Step = 1;
                         approve.ApproveData = DateTime.Now;
                         approve.ApproveName = name;
                         approve.Memo = model.Memo;
                         approve.Money = 0;
                         approve.MoneyWhereAbout = "无钱款去向";

                         db.T_ExpressIndemnityApprove.Add(approve);
                     }
                     //else //快递组的人都可以审核，不指定审核人
                     //{
                     //    T_ExpressIndemnityApprove approve = new T_ExpressIndemnityApprove();
                     //    approve.EID = model.ID;
                     //    approve.State = "0";
                     //    approve.Step = 1;
                     //    db.T_ExpressIndemnityApprove.Add(approve);
                     //}
                     db.SaveChanges();



                     //string RetreatAppRoveSql = "  select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpressIndemnityApprove where  EID in ( select ID from T_ExpressIndemnity where IsDelete=0 ) and  State=-1 and ApproveData is null GROUP BY ApproveName  ";
                     //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                     //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                     //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                     //{
                     //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                     //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "快递赔付待处理" && a.PendingAuditName == PendingAuditName);
                     //    if (NotauditedModel != null)
                     //    {
                     //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                     //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                     //    }
                     //    else
                     //    {
                     //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                     //        ModularNotauditedModel.ModularName = "快递赔付待处理";
                     //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                     //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                     //        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                     //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                     //    }
                     //    db.SaveChanges();
                     //}

                     sc.Complete();
                     return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                 }
                 catch(DbEntityValidationException ex)
                 {
                     return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                 }
                
             }

             
         }
         public partial class Modular
         {

             public string ModularName { get; set; }
             public int NotauditedNumber { get; set; }
             public string PendingAuditName { get; set; }
         }
         public JsonResult ExpressIndemnityDelete(int ID)
         {
             T_ExpressIndemnity delModel = db.T_ExpressIndemnity.Find(ID);
             delModel.IsDelete = 1;
             //T_OrderList Order = db.T_OrderList.FirstOrDefault(a => a.platform_code == delModel.OrderNum || a.mail_no == delModel.RetreatExpressNum);
             //Order.Status_ExpressIndemnity = 0;
             //db.Entry<T_ExpressIndemnity>(delModel).State = System.Data.Entity.EntityState.Modified;
             //db.Entry<T_OrderList>(Order).State = System.Data.Entity.EntityState.Modified;
             int i = db.SaveChanges();
             return Json(i);
         }//删除
         public JsonResult ExpressIndemnityEditSave(T_ExpressIndemnity model, string picUrls)//编辑
         {
             T_ExpressIndemnity editModel = db.T_ExpressIndemnity.Find(model.ID);
             if (editModel == null)
             {
                 return Json(-1);
             }
             else
             {
                 editModel.Type = model.Type;
                 editModel.OrderNum = model.OrderNum;
                 editModel.RetreatExpressNum = model.RetreatExpressNum;
                 editModel.ShopName = model.ShopName;
                 editModel.OrderMoney = model.OrderMoney;
                 editModel.ExpressName = model.ExpressName;
                 editModel.wangwang = model.wangwang;
                 editModel.Memo = model.Memo;
                 db.Entry<T_ExpressIndemnity>(editModel).State = System.Data.Entity.EntityState.Modified;
                 if (picUrls.Length > 0)
                 {

                     string[] picArr = picUrls.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                     foreach (string Purl in picArr)
                     {
                         T_ExpressIndemnityPic IndemnityPic = new T_ExpressIndemnityPic();
                         IndemnityPic.EID = model.ID;
                         IndemnityPic.PicURL = Purl;
                         db.T_ExpressIndemnityPic.Add(IndemnityPic);

                     }
                    
                 }

                 int i = db.SaveChanges();

                 //string RetreatAppRoveSql = "  select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpressIndemnityApprove where  EID in ( select ID from T_ExpressIndemnity where IsDelete=0 ) and  State=-1 and ApproveData is null GROUP BY ApproveName  ";
                 //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                 //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                 //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                 //{
                 //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                 //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "快递赔付待处理" && a.PendingAuditName == PendingAuditName);
                 //    if (NotauditedModel != null)
                 //    {
                 //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                 //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                 //    }
                 //    else
                 //    {
                 //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                 //        ModularNotauditedModel.ModularName = "快递赔付待处理";
                 //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                 //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                 //        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                 //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                 //    }
                 //    db.SaveChanges();
                 //}
                 return Json(i);
             }
         }
         public JsonResult ExpressIndemnityApproveSave(T_ExpressIndemnity model,int second)//审核保存
         {
             string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
             T_ExpressIndemnity editModel = db.T_ExpressIndemnity.Find(model.ID);
             editModel.IndemnityMoney = model.IndemnityMoney;
             editModel.MoneyWhereAbout = model.MoneyWhereAbout;
             editModel.Address = model.Address;
             editModel.State = model.State;

             T_ExpressIndemnityApprove newApprove = new T_ExpressIndemnityApprove();
             newApprove.ApproveData = DateTime.Now;
             newApprove.ApproveName = name;
             newApprove.EID = model.ID;
             newApprove.Memo = model.Memo;
             newApprove.State = model.State;
             newApprove.Address = model.Address;
             newApprove.Money = model.IndemnityMoney;
             newApprove.MoneyWhereAbout = model.MoneyWhereAbout;
             //T_OrderList EditOrder = db.T_OrderList.FirstOrDefault(a => a.platform_code == editModel.OrderNum || a.mail_no == editModel.RetreatExpressNum);
             //EditOrder.Status_ExpressIndemnity = 2;
             //db.Entry<T_OrderList>(EditOrder).State = System.Data.Entity.EntityState.Modified;
             if (second == 1)//二次审核时修改主记录判断二次审核字段状态,且修改应收的对应记录
             {
                 editModel.Second = "1";
                // db.T_ExpressIndemnityApprove.Add(model);
                 //T_AR EditAR = db.T_AR.FirstOrDefault(a => a.BillType == "快递赔付" && a.BillFromCode == Emodel.OrderNum);
                 //EditAR.BillMoney = (double)model.Money;
                 //EditAR.BillCompany = model.MoneyWhereAbout;
                
                 
             }
             else//审核时添加一条应收
             {
                // editModel.CurrentApproveName = name;
                 //T_AR AR = new T_AR();
                 //AR.BillType = "快递赔付";
                 //AR.CreateTime = DateTime.Now;
                 //AR.CreatUser = name;
                 //AR.ReceivedMony = 0;
                 //AR.BillMoney =(double)model.Money;
                 //AR.BillFromCode = Emodel.OrderNum;
                 //AR.BillCompany = model.MoneyWhereAbout;
                 //AR.BillCode = "";  //添加一条数据到应收
             }

             db.T_ExpressIndemnityApprove.Add(newApprove);
             db.Entry<T_ExpressIndemnity>(editModel).State = System.Data.Entity.EntityState.Modified;
             try
             {
                 //string RetreatAppRoveSql = "  select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_ExpressIndemnityApprove where  EID in ( select ID from T_ExpressIndemnity where IsDelete=0 ) and  State=-1 and ApproveData is null GROUP BY ApproveName  ";
                 //List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                 //string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                 //for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                 //{
                 //    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                 //    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "快递赔付待处理" && a.PendingAuditName == PendingAuditName);
                 //    if (NotauditedModel != null)
                 //    {
                 //        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                 //        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;

                 //    }
                 //    else
                 //    {
                 //        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                 //        ModularNotauditedModel.ModularName = "快递赔付待处理";
                 //        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                 //        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                 //        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                 //        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                 //    }
                 //    db.SaveChanges();
                 //}
                 int i = db.SaveChanges();
                 return Json(i);
             }
             catch (DbUpdateConcurrencyException ex) { return Json(ex.Message); }

         }
         public JsonResult ExpressIndemnityConfigEditSave(T_ExpressIndemnityConfig model)//配置表编辑
         {
             T_ExpressIndemnityConfig editModel = db.T_ExpressIndemnityConfig.Find(model.ID);
             editModel.Name = model.Name;
             editModel.Type = model.Type;
             db.Entry<T_ExpressIndemnityConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
             int i = db.SaveChanges();
             return Json(i);
         }
         public JsonResult ExpressIndemnityConfigAddSave(T_ExpressIndemnityConfig model)
         {
             model.Step = -1;
             db.T_ExpressIndemnityConfig.Add(model);
             int i = db.SaveChanges();
             return Json(i);
         }
         public JsonResult ExpressIndemnityConfigDelete(int ID)
         {
             T_ExpressIndemnityConfig model = db.T_ExpressIndemnityConfig.Find(ID);
             if (model != null)
             {
                 db.T_ExpressIndemnityConfig.Remove(model);
                 int i = db.SaveChanges();
                 return Json(i);

             }
             else
             {
                 return Json(0);
             }
         }
        #endregion
        #region 下载图片.excel
        //下载图片
         public FileResult DownPicture(int id)
         {
             T_ExpressIndemnityPic model = db.T_ExpressIndemnityPic.Find(id);

             if (model != null)
             {
                 T_ExpressIndemnity Emodel = db.T_ExpressIndemnity.Find(model.EID);
                 string filename = Emodel.RetreatExpressNum;//以快递单号命名下载的图片
                 string urlStr = model.PicURL;
                 //string[] sArr = urlStr.Split(new string[] { "^^" }, StringSplitOptions.RemoveEmptyEntries);
                 Response.ContentType = "image/jpeg";
                 return File(urlStr, "image/jpeg", filename + ".jpeg");
             }
             else { return File("", "image/jpeg"); }
         }
         public FileResult DownExcel(string queryStr, string State, DateTime? startSendTime, DateTime? endSendTime)
         {
             IQueryable<T_ExpressIndemnity> queryData = db.T_ExpressIndemnity.Where(a=>a.State!="0");

           
             if (!string.IsNullOrWhiteSpace(State))
             {
                 queryData = queryData.Where(a => a.State == State);
             }
             if (!string.IsNullOrWhiteSpace(queryStr))
             {
                 queryData = queryData.Where(a => (a.OrderNum != null && a.OrderNum.Contains(queryStr)) || (a.RetreatExpressNum != null && a.RetreatExpressNum.Contains(queryStr)));
             }
             if (startSendTime != null)
             {
                 queryData = queryData.Where(a => a.Date >= startSendTime);
             }
             if (endSendTime != null)
             {
                 endSendTime = endSendTime.Value.AddDays(1);
                 queryData = queryData.Where(a => a.Date <= endSendTime);
             }
             List<T_ExpressIndemnity> modelList =queryData.ToList();
			List<int> IDs = modelList.Select(a => a.ID).ToList();
			List<T_ExpressIndemnityApprove> ApproveList = db.T_ExpressIndemnityApprove.Where(a => IDs.Contains(a.EID)).ToList();
             //创建Excel文件的对象
             NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
             //添加一个sheet
             NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
             NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
             
             row1.CreateCell(0).SetCellValue("日期");
             row1.CreateCell(1).SetCellValue("旺旺号");
             row1.CreateCell(2).SetCellValue("店铺");
             row1.CreateCell(3).SetCellValue("地点");
             row1.CreateCell(4).SetCellValue("快递公司");
             row1.CreateCell(5).SetCellValue("货物状态");
             row1.CreateCell(6).SetCellValue("结果");//state
             row1.CreateCell(7).SetCellValue("订单金额");
             row1.CreateCell(8).SetCellValue("钱款去向");
             row1.CreateCell(9).SetCellValue("赔付金额");
             row1.CreateCell(10).SetCellValue("订单号");
             row1.CreateCell(11).SetCellValue("物流单号");
             row1.CreateCell(12).SetCellValue("处理备注");

             sheet1.SetColumnWidth(0, 20 * 256);
             sheet1.SetColumnWidth(1, 15 * 256);
             sheet1.SetColumnWidth(2, 15 * 256);
             sheet1.SetColumnWidth(3, 15 * 256);
             sheet1.SetColumnWidth(4, 20 * 256);
             sheet1.SetColumnWidth(5, 20 * 256);
             sheet1.SetColumnWidth(6, 20 * 256);
             sheet1.SetColumnWidth(7, 10 * 256);
             sheet1.SetColumnWidth(8, 15 * 256);
             sheet1.SetColumnWidth(9, 10 * 256);
             sheet1.SetColumnWidth(10, 20 * 256);
             sheet1.SetColumnWidth(11, 20 * 256);
             sheet1.SetColumnWidth(12, 20 * 256);
            
             for (int i = 0; i < modelList.Count; i++)
             {
                 NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                 rowtemp.CreateCell(0).SetCellValue(modelList[i].Date.Value.ToString());
                 rowtemp.CreateCell(1).SetCellValue(modelList[i].wangwang);
                 rowtemp.CreateCell(2).SetCellValue(modelList[i].ShopName);
               
                 rowtemp.CreateCell(4).SetCellValue(modelList[i].ExpressName);
                 rowtemp.CreateCell(7).SetCellValue(modelList[i].OrderMoney.Value);
              
                 rowtemp.CreateCell(5).SetCellValue(modelList[i].Type);
                 if (modelList[i].State == "1")
                 {
                     rowtemp.CreateCell(6).SetCellValue("已赔付");
                 }
                 else if (modelList[i].State == "2")
                 {
                     rowtemp.CreateCell(6).SetCellValue("已仲裁");
                 }
                 else if (modelList[i].State == "3")
                 {
                     rowtemp.CreateCell(6).SetCellValue("财务直接扣款");
                 }
                 else if (modelList[i].State == "4")
                 {
                     rowtemp.CreateCell(6).SetCellValue("无法仲裁");
                 }
                 else if (modelList[i].State == "5")
                 {
                     rowtemp.CreateCell(6).SetCellValue("货物已追回");
                 }
                 else if (modelList[i].State == "6")
                 {
                     rowtemp.CreateCell(6).SetCellValue("弃件");
                 }
                 
                 else
                 {
                     rowtemp.CreateCell(6).SetCellValue("");
                 }
                 rowtemp.CreateCell(10).SetCellValue(modelList[i].OrderNum);
                 rowtemp.CreateCell(11).SetCellValue(modelList[i].RetreatExpressNum);
               
                 int id = modelList[i].ID;
                 if (modelList[i].Second == "0")
                 {
                     T_ExpressIndemnityApprove AppModel = ApproveList.FirstOrDefault(a => a.EID == id);
                     if (AppModel != null)
                     {
                         rowtemp.CreateCell(3).SetCellValue(AppModel.Address);
                         rowtemp.CreateCell(8).SetCellValue(AppModel.MoneyWhereAbout);
                         rowtemp.CreateCell(9).SetCellValue(AppModel.Money != null ? AppModel.Money.Value : 0);
                         rowtemp.CreateCell(12).SetCellValue(AppModel.Memo);
                     }
                 }
                 else
                 {
                     T_ExpressIndemnityApprove AppModel = ApproveList.Where(a => a.EID == id).OrderByDescending(a=>a.ID).First(); 
                     if (AppModel != null)
                     {
                         rowtemp.CreateCell(3).SetCellValue(AppModel.Address);
                         rowtemp.CreateCell(8).SetCellValue(AppModel.MoneyWhereAbout);
                         rowtemp.CreateCell(9).SetCellValue(AppModel.Money != null ? AppModel.Money.Value : 0);
                         rowtemp.CreateCell(12).SetCellValue(AppModel.Memo);
                     }
                 }
             }

             Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
             // 写入到客户端 
             System.IO.MemoryStream ms = new System.IO.MemoryStream();

             book.Write(ms);
             ms.Seek(0, SeekOrigin.Begin);
             ms.Flush();
             ms.Position = 0;

             return File(ms, "application/vnd.ms-excel", "快递赔付.xls");
         }
        #endregion
    }
}
