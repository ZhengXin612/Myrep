using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
namespace EBMS.Controllers
{
    public class MeetShareController : Controller
    {
        //
        // GET: /MeetShare/
        EBMSEntities db = new EBMSEntities();
        public string getSeasonList() {
            List<T_MeetShareSeason> seasonList = db.T_MeetShareSeason.OrderByDescending(a => a.ID).AsQueryable().ToList();
            return JsonConvert.SerializeObject(seasonList);
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Record(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        public ActionResult List()
        {
            ViewData["seasonList"] = getSeasonList();
            return View();
        }
        public ActionResult Ctrl()
        {
            int Season = int.Parse(db.T_MeetShareSeason.AsEnumerable().Last().Season.ToString());
            ViewData["Season"] = Season;
            ViewData["seasonList"] = getSeasonList();
            return View();
        }
        
        public ActionResult Add()
        {
            int Season = int.Parse(db.T_MeetShareSeason.AsEnumerable().Last().Season.ToString());
            ViewData["Season"] = Season;
            return View();
        }
        public ActionResult UserTable()
        {
            return View();
        }
        //用户
        [HttpPost]
        public ContentResult GetUserList(Lib.GridPager pager, string queryStr)
        {

            //查询出所有未删除的
            IQueryable<T_User> queryData = db.T_User.AsQueryable();
            //根据订单号，提交者姓名，会员号码查询
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Nickname.Contains(queryStr) || a.Name.Contains(queryStr));
            }
            //根据状态查询
            //分页
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_User> list = new List<T_User>();
            foreach (var item in queryData)
            {
                T_User i = new T_User();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //新增
        public JsonResult shareAdd(T_MeetShare model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    var modFlag = db.T_MeetShare.FirstOrDefault(a => a.Season == model.Season && a.Name == model.Name && a.Finals == model.Finals && a.IsDel == 0);
                    if (modFlag != null)
                    {
                        string resultMsg = "该员工本期已经分享过";
                        if (model.Finals == 1)
                        {
                            resultMsg = "该员工本期已经参加过决赛了";
                        }
                        return Json(new { State = "Faile", Message = resultMsg }, JsonRequestBehavior.AllowGet);
                    }
                    var modFlag2 = db.T_MeetShare.FirstOrDefault(a => a.Season == model.Season && a.ShareDate == model.ShareDate && a.IsDel==0);
                    if (modFlag2 != null)
                    {
                        return Json(new { State = "Faile", Message = model.ShareDate+",已经设置了分享人员" }, JsonRequestBehavior.AllowGet);
                    }
                    T_MeetShare mod = new T_MeetShare();
                    mod.IsDel = 0;
                    mod.IsEnd = 0;
                    mod.Name = model.Name;
                    mod.ShareDate = model.ShareDate;
                    mod.Score = 0;
                    mod.Total = 0;
                    mod.Finals = model.Finals;
                    mod.Season = model.Season;
                    db.T_MeetShare.Add(mod);
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message = "保存成功" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }
        //管理列表
        [HttpPost]
        public ContentResult GetList(Lib.GridPager pager, string queryStr, string seasonSel, string startSendTime, string endSendTime, string finalsSel)
        {
            var list = db.T_MeetShare.Where(a=>a.IsDel==0).AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                list = list.Where(a => a.Name == queryStr);
            }
             if (!string.IsNullOrEmpty(seasonSel))
            {
                int _seasonSel = int.Parse(seasonSel);
                list = list.Where(a => a.Season == _seasonSel);
            }
             if (!string.IsNullOrEmpty(finalsSel))
             {
                 int _finalsSel = int.Parse(finalsSel);
                 list = list.Where(a => a.Finals == _finalsSel);
             }
             //根据日期查询
             if (!string.IsNullOrWhiteSpace(startSendTime) && !string.IsNullOrWhiteSpace(endSendTime))
             {

                 DateTime startTime = DateTime.Parse(startSendTime);
                 DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                 list = list.Where(s => s.ShareDate >= startTime && s.ShareDate <= endTime);
             }
             else if (!string.IsNullOrWhiteSpace(startSendTime))
             {
                 DateTime startTime = DateTime.Parse(startSendTime);
                 DateTime endTime = startTime.AddDays(5);
                 list = list.Where(s => s.ShareDate >= startTime);
             }
             else if (!string.IsNullOrWhiteSpace(endSendTime))
             {
                 DateTime endTime = DateTime.Parse(endSendTime + " 23:59:59");
                 DateTime startTime = endTime.AddDays(-5);
                 list = list.Where(s => s.ShareDate <= endTime);
             }



            pager.totalRows = list.Count();
            var queryData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).Select(item => new
            {
                item.ID,
                item.Name,
                item.Score,
                item.ShareDate,
                item.Total,
                item.Season,
                item.IsEnd,
                item.Finals
            });
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        [HttpPost]
        public ContentResult GetRecordList(Lib.GridPager pager, string queryStr,int ID)
        {
            var list = db.T_MeetShareRecord.Where(a => a.Pid == ID).AsQueryable();
              if (!string.IsNullOrEmpty(queryStr))
            {
                list = list.Where(a => a.Name == queryStr);
            }
            pager.totalRows = list.Count();
            var queryData = list.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).Select(item => new
            {
                item.ID,
                item.Name,
                item.Score
            });
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public JsonResult End(int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    T_MeetShare mod = db.T_MeetShare.Find(ID);
                    mod.IsEnd = 1;
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message = "你已经关闭投票" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }
        public JsonResult Del(int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    T_MeetShare mod = db.T_MeetShare.Find(ID);
                    mod.IsDel = 1;
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message = "删除成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }
        public JsonResult NextSeason()
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    int Season = int.Parse(db.T_MeetShareSeason.AsEnumerable().Last().Season.ToString());
                   int _Season = Season+1;
                    T_MeetShareSeason mod = new T_MeetShareSeason();
                    mod.Season = _Season;
                    db.T_MeetShareSeason.Add(mod);
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message = "已经进入第(" + _Season + ")期分享" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
        }
    }
}
