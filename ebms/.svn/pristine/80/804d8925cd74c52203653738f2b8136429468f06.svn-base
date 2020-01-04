using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class offerController : BaseController
    {


        EBMSEntities db = new EBMSEntities();
        //
        // GET: /offer/

        public ActionResult ViewofferAdd()
        {
            return View();
        }
        public ActionResult Viewoffer()
        {
            return View();
        }
        public ActionResult ViewofferChase()
        {
            return View();
        }
        public ActionResult ViewofferDetails(int pId)
        {
            ViewData["pId"] = pId;
            return View();
        }

        /// <summary>
        /// 获取报价详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="reissueId"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewofferDetails(Lib.GridPager pager, int pId)
        {
            IQueryable<T_offerDetails> list = db.T_offerDetails.Where(s => s.offerID == pId).AsQueryable();
            pager.totalRows = list.Count();
            List<T_offerDetails> querData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(querData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        
        [Description("访问单位页面查询方法")]
        [HttpPost]
        public ContentResult Geoffer(Lib.GridPager pager, string name)
        {

            IQueryable<T_offer> queryData = db.T_offer.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                queryData = queryData.Where(a => a.inquirerName != null && a.inquirerName.Contains(name)|| a.offerName.Contains(name));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_offer> list = new List<T_offer>();
            foreach (var item in queryData)
            {
                T_offer i = new T_offer();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }
        /// <summary>
        /// 报价未审核
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="code"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult GetViewofferChaseList(Lib.GridPager pager, string queryStr)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            List<T_offerApprove> ApproveMod = db.T_offerApprove.Where(a => a.ApproveName == name && a.ApproveTime == null).ToList();
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].PID.ToString());
            }
            IQueryable<T_offer> queryData = from r in db.T_offer
                                               where Arry.Contains(r.ID)  && (r.Status == -1 || r.Status == 0 || r.Status == 2)
                                               select r;
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.inquirerName != null && a.inquirerName.Contains(queryStr) || a.offerName != null && a.offerName.Contains(queryStr));
            }
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_offer> list = new List<T_offer>();
            foreach (var item in queryData)
            {
                T_offer i = new T_offer();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 报价保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public JsonResult ViewofferAddSave(T_offer model, string detailList, decimal costTotal, decimal offerTotal)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_offerDetails> details = Com.Deserialize<T_offerDetails>(detailList);
                    if (string.IsNullOrWhiteSpace(detailList))
                        return Json(new { State = "Faile", Message = "详情不能为空" });

                    model.offerName = name;
                    model.offerDate = DateTime.Now;
                    model.Step = 0;
                    model.Status = -1;
                     model.costTotal = costTotal;
                    model.offerTotal=offerTotal;
                    db.T_offer.Add(model);
                    db.SaveChanges();
                 
                    foreach (var item in details)
                    {
                        item.offerID = model.ID;
                        db.T_offerDetails.Add(item);
                    }
                    db.SaveChanges();

                    T_offerApprove approve = new T_offerApprove
                    {
                        ApproveName ="武装色",
                        Status = -1,
                        PID = model.ID
                    };
                    db.T_offerApprove.Add(approve);
                    db.SaveChanges();

                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ViewofferChaseShenHe(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_offerApprove offerApprove = db.T_offerApprove.SingleOrDefault(a => a.PID == id && a.ApproveTime == null);
                    if (offerApprove == null)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" });

                    }
                    offerApprove.ApproveTime = DateTime.Now;
                    offerApprove.Status = 1;
                    db.SaveChanges();

                    T_offer model = db.T_offer.Find(id);
                    model.Status = 1;
                    model.Step = 1;
                    db.SaveChanges();
                   
               
                
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult NotViewofferChaseShenHe(int id)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_offerApprove offerApprove = db.T_offerApprove.SingleOrDefault(a => a.PID == id && a.ApproveTime == null);
                    if (offerApprove == null)
                    {
                        return Json(new { State = "Faile", Message = "该数据已审核" });

                    }
                    offerApprove.ApproveTime = DateTime.Now;
                    offerApprove.Status = 2;
                    db.SaveChanges();

                    T_offer model = db.T_offer.Find(id);
                    model.Status = 2;
                    model.Step = 1;
                    db.SaveChanges();



                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
