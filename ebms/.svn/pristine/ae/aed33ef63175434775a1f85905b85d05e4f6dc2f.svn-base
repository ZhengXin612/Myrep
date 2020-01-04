using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using EBMS.App_Code;
using System.Transactions;
using Newtonsoft.Json;
using LitJson;
namespace EBMS.Controllers
{
    public class RespiratorController : BaseController
    {
        //
        // GET: /Respirator/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        /// <summary>
        /// 创建出货单号600XXXX
        /// </summary>
        /// <returns></returns>
        public string getSaleNumber()
        {
            string code = "";
            string date = "600";
            List<T_RespiratorOrder> ordermodel = db.T_RespiratorOrder.Where(a => a.SaleNumbers.Contains(date)).OrderByDescending(c => c.ID).ToList();
            if (ordermodel.Count == 0)
            {
                code += date + "0001";
            }
            else
            {
                int bianhao = int.Parse(ordermodel[0].SaleNumbers) + 1;
                code = bianhao.ToString();
            }

            return code;
        }
        private string isNULL(object data)
        {
            if (data == null) return "";
            else return data.ToString();
        }
        //根据订单号查询查询数据
        public JsonResult QueryDataByCode(string Ordercode)
        {
            if (!string.IsNullOrWhiteSpace(Ordercode))
            {
                List<T_RespiratorOrder> orders = db.T_RespiratorOrder.Where(a => a.OrderCode == Ordercode.Trim()).ToList();
                if (orders.Count > 0)
                {
                    return Json(-1);
                }
            }
            if (Ordercode != "" && Ordercode != null)
            {
                App_Code.GY gy = new App_Code.GY();
                string cmd = "";
                cmd = "{" +
                            "\"appkey\":\"171736\"," +
                            "\"method\":\"gy.erp.trade.get\"," +
                            "\"page_no\":1," +
                            "\"page_size\":10," +
                            "\"platform_code\":\"" + Ordercode + "\"," +
                            "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                            "}";
                string sign = gy.Sign(cmd);
                cmd = cmd.Replace("}", ",\"sign\":\"" + sign + "\"}");
                string retnum = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmd);
                JsonData jsonDatanum = null;
                jsonDatanum = JsonMapper.ToObject(retnum);
                int successnum = int.Parse(jsonDatanum[7].ToString());
                if (successnum > 0)
                {
                    //赋值

                    JsonData orders = jsonDatanum["orders"][0];
                    //收货人
                    string receiver_name = isNULL(orders["receiver_name"]).ToString();
                    //收货人电话
                    string receiver_phone = isNULL(orders["receiver_phone"]).ToString();
                    //收货人手机
                    string receiver_mobile = isNULL(orders["receiver_mobile"]).ToString();
                    //收货人地址
                    string receiver_address = isNULL(orders["receiver_address"]).ToString();
                    T_RespiratorOrder model = new T_RespiratorOrder();
                    model.CustomerName = receiver_name;
                    if (receiver_phone != "")
                    {
                        model.Customerphone = receiver_phone;
                    }
                    else
                    {
                        model.Customerphone = receiver_mobile;
                    }

                    model.Customeraddress = receiver_address;

                    JsonData details = orders["details"];
                    List<T_RespiratorOrderDetails> detalislist = new List<T_RespiratorOrderDetails>();
                    for (int s = 0; s < details.Count; s++)
                    {
                        T_RespiratorOrderDetails detalis = new T_RespiratorOrderDetails();
                        JsonData detailsxiangqing = details[s];
                        detalis.ProductName = isNULL(detailsxiangqing["item_name"]).ToString();
                        detalis.Company = isNULL(detailsxiangqing["item_simple_name"]).ToString();
                        detalislist.Add(detalis);

                    }
                    return Json(new { i = model, d = detalislist }, JsonRequestBehavior.AllowGet);
                    //return View(model);
                }
                else
                {
                    App_Code.GY gynum = new App_Code.GY();
                   
                    string cmdnum = "";
                    cmdnum = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.trade.history.get\"," +
                                "\"page_no\":1," +
                                "\"page_size\":10," +
                                "\"platform_code\":\"" + Ordercode + "\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"" +
                                "}";
                    string signnum = gynum.Sign(cmdnum);
                    cmdnum = cmdnum.Replace("}", ",\"sign\":\"" + signnum + "\"}");
                    retnum = gy.DoPost("http://api.guanyierp.com/rest/erp_open", cmdnum);
                    jsonDatanum = null;
                    jsonDatanum = JsonMapper.ToObject(retnum);
                    successnum = int.Parse(jsonDatanum[7].ToString());
                    if (successnum > 0)
                    {
                        //赋值

                        JsonData orders = jsonDatanum["orders"][0];
                        //收货人
                        string receiver_name = isNULL(orders["receiver_name"]).ToString();
                        //收货人电话
                        string receiver_phone = isNULL(orders["receiver_phone"]).ToString();
                        //收货人手机
                        string receiver_mobile = isNULL(orders["receiver_mobile"]).ToString();
                        //收货人地址
                        string receiver_address = isNULL(orders["receiver_address"]).ToString();
                        T_RespiratorOrder model = new T_RespiratorOrder();
                        model.CustomerName = receiver_name;
                        if (receiver_phone != "")
                        {
                            model.Customerphone = receiver_phone;
                        }
                        else
                        {
                            model.Customerphone = receiver_mobile;
                        }

                        model.Customeraddress = receiver_address;

                        JsonData details = orders["details"];
                        List<T_RespiratorOrderDetails> detalislist = new List<T_RespiratorOrderDetails>();
                        for (int s = 0; s < details.Count; s++)
                        {
                            T_RespiratorOrderDetails detalis = new T_RespiratorOrderDetails();
                            JsonData detailsxiangqing = details[s];
                            detalis.ProductName = detailsxiangqing["item_name"].ToString();
                            detalis.Company = detailsxiangqing["item_simple_name"].ToString();
                            detalis.ProductCode = detailsxiangqing["item_code"].ToString();
                            detalis.Number = Convert.ToInt32(detailsxiangqing["qty"].ToString());
                            detalislist.Add(detalis);

                        }
                        return Json(new { i = model, d = detalislist }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            int i = 0;

            return Json(i, JsonRequestBehavior.AllowGet);
            //if (!string.IsNullOrWhiteSpace(Ordercode))
            //{
            //    List<T_RespiratorOrder> RespiratorOrder = db.T_RespiratorOrder.Where(a => a.OrderCode == Ordercode.Trim()).ToList();





            //    if (RespiratorOrder.Count > 0)
            //    {
            //        return Json(-1);//已添加该订单
            //    }
            //    else
            //    {
            //        T_OrderList Order = db.T_OrderList.SingleOrDefault(a => a.platform_code == Ordercode);
            //        if (Order != null)
            //        {
            //            string oid=Order.code;
            //            List<T_OrderDetail> detail = db.T_OrderDetail.Where(a => a.oid == oid).ToList();
            //            return Json(new { i = Order,d=detail }, JsonRequestBehavior.AllowGet);
            //        }
            //        else
            //        {
            //            return Json(0, JsonRequestBehavior.AllowGet);
            //        }
            //    }
            //}
            //else
            //{
            //    return Json(0, JsonRequestBehavior.AllowGet);
            //}
           

           
        }
        #endregion
        #region 视图
        [Description("呼吸机新增")]
        public ActionResult ViewRespiratorAdd()
        {
            ViewData["SaleNumbers"] = getSaleNumber();
            return View();
        }
        [Description("呼吸机列表")]
        public ActionResult ViewRespiratorList()
        {
            return View();
        }
        [Description("呼吸机管理")]
        public ActionResult ViewRespiratorManage()
        {
            return View();
        }
         [Description("售后跟进")]
        public ActionResult ViewRespiratorAfterSale(int ID)
        {
            ViewData["RID"] = ID;
            return View();
        }
        [Description("呼吸机明细")]
         public ActionResult ViewRespiratorDetail(int ID)
         {
             ViewData["OrderID"] = ID;
             return View();
         }
        [Description("销售单打印")]
         public ActionResult ViewPrintSale(int ID)
         {
             T_RespiratorOrder Ordermodel = db.T_RespiratorOrder.SingleOrDefault(a => a.ID == ID);
             ViewData["ID"] = ID;
             return View(Ordermodel);
         }
        [Description("保证书打印")]
         public ActionResult ViewPrintGuarantee(int ID)
         {
             T_RespiratorOrder Ordermodel = db.T_RespiratorOrder.SingleOrDefault(a => a.ID == ID);
             ViewData["ID"] = ID;
             return View(Ordermodel);
         }
        [Description("呼吸机编辑")]
        public ActionResult ViewRespiratorEdit(int ID)
         {
             ViewData["ID"] = ID;
             T_RespiratorOrder model = db.T_RespiratorOrder.SingleOrDefault(a => a.ID == ID);
             return View(model);
         }
        #endregion
        #region 绑定数据
        [HttpPost]
        public ContentResult GetRespiratorOrderList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<T_RespiratorOrder> queryData = db.T_RespiratorOrder;
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.OrderCode != null && a.OrderCode.Contains(queryStr));
            }
            if (queryData != null)
            {
                List<T_RespiratorOrder> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else { return Content(""); }
        }//获取呼吸机订单表数据
        [HttpPost]
        public ContentResult GetRespiratorDetailList(Lib.GridPager pager,int RID=0)//获取呼吸机明细数据
        {
            if (RID != 0)
            {
                IQueryable<T_RespiratorOrderDetails> queryDataD = db.T_RespiratorOrderDetails.Where(a => a.OrderID == RID);
                List<T_RespiratorOrderDetails> listD = queryDataD.OrderBy(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryDataD.Count();
                //string json = "{\"total\":" + queryDataD.Count() + ",\"rows\":" + JsonConvert.SerializeObject(listD, Lib.Comm.setTimeFormat()) + "}";
                string json = "{\"total\":" + queryDataD.Count() + ",\"rows\":" + JsonConvert.SerializeObject(listD, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            return Content("");
        }
         [HttpPost]
        public ContentResult GetRespiratorAfterSaleList(Lib.GridPager pager, int RID=0)//获取呼吸机售后记录数据
        {
            if (RID != 0)
            {
                IQueryable<T_RespiratorOrderRecord> queryDataR = db.T_RespiratorOrderRecord.Where(a => a.RID == RID);
                List<T_RespiratorOrderRecord> listR = queryDataR.OrderBy(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryDataR.Count();
                string json = "{\"total\":" + queryDataR.Count() + ",\"rows\":" + JsonConvert.SerializeObject(listR, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            return Content("");
        }
        [HttpPost]
         public JsonResult QueryOrderDetailsByID(int ID)
         {
            List<T_RespiratorOrderDetails> modelList = db.T_RespiratorOrderDetails.Where(a => a.OrderID == ID).ToList();
            return Json(modelList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 增删改
        [HttpPost]
        public JsonResult RespiratorAddSave(T_RespiratorOrder model, string jsonStr)
        {
            using (TransactionScope sc=new TransactionScope())
           {
               try
               {
                   string user = Server.UrlDecode(Request.Cookies["NickName"].Value);
                   model.CreateUser = user;
                   model.SaleDate = DateTime.Now;
                   db.T_RespiratorOrder.Add(model);
                   int i = db.SaveChanges();
                   if (i > 0)
                   {
                       List<T_RespiratorOrderDetails> detail = Com.Deserialize<T_RespiratorOrderDetails>(jsonStr);
                       if (detail.Count == 0)
                       {
                           return Json(new { State = "Faile", Message ="请添加详情" }, JsonRequestBehavior.AllowGet);
                       }
                       else
                       {
                           foreach (var item in detail)
                           {
                               item.OrderID = model.ID;
                               db.T_RespiratorOrderDetails.Add(item);
                           }
                           db.SaveChanges();
                       }
                   }
                   else
                   {
                       return Json(new { State = "Faile", Message ="保存失败"}, JsonRequestBehavior.AllowGet);
                   }
                   sc.Complete();
                   return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
               }
               catch (Exception ex)
               {
                   return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);

               }
             
           }
           
        }
        public JsonResult RespiratorEditSave(T_RespiratorOrder model, string jsonStr)
        { 
         using (TransactionScope sc=new TransactionScope())
           {
               try
               {
                   T_RespiratorOrder order = db.T_RespiratorOrder.Find(model.ID);
                   if (order !=null)
                   {
                       List<T_RespiratorOrderDetails> detail = Com.Deserialize<T_RespiratorOrderDetails>(jsonStr);
                       if (detail.Count == 0)
                       {
                           return Json(new { State = "Faile", Message ="请添加详情" }, JsonRequestBehavior.AllowGet);
                       }
                       else
                       {
                           foreach (var item in detail)
                           {
                               T_RespiratorOrderDetails editDetail = db.T_RespiratorOrderDetails.Find(item.ID);
                               editDetail.Company = item.Company;
                               editDetail.Number = item.Number;
                               editDetail.Price = item.Price;
                               editDetail.ProductCode = item.ProductCode;
                               editDetail.ProductName = item.ProductName;
                               editDetail.Subtotal = item.Subtotal;
                               db.Entry<T_RespiratorOrderDetails>(editDetail).State = System.Data.Entity.EntityState.Modified;
                           }
                           db.SaveChanges();
                           sc.Complete();
                           return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                       }
                   }
                   else
                   {
                       return Json(new { State = "Faile", Message ="保存失败"}, JsonRequestBehavior.AllowGet);
                   }
                 
               }
               catch (Exception ex)
               {
                   return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);

               }
             
           }
        }
        [HttpPost]
        public JsonResult RespiratorAfterSaleSave(T_RespiratorOrderRecord model)
        {
            try
            {
                model.Date = DateTime.Now;
                model.Name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                db.T_RespiratorOrderRecord.Add(model);
                db.SaveChanges();
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RespiratorDelete(int ID)
        {
            T_RespiratorOrder delModel = db.T_RespiratorOrder.Find(ID);
            List<T_RespiratorOrderDetails> delDetail = db.T_RespiratorOrderDetails.Where(a => a.OrderID == ID).ToList();
            if (delModel != null)
            {
                db.T_RespiratorOrder.Remove(delModel);
                foreach (var item in delDetail)
                {
                    db.T_RespiratorOrderDetails.Remove(item);
                }
                int i=db.SaveChanges();
                return Json(i);
            }
            else
            {
                return Json(-1);
            }
        }
        #endregion
    }
}
