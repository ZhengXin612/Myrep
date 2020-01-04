using EBMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
namespace EBMS.Areas.APP.Controllers
{
    public class AppMeetShareController : Controller
    {
        //
        // GET: /APP/AppMeetShare/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            
            DateTime toDay = DateTime.Now;
            string _Year = DateTime.Now.Year.ToString();
            string _Month = DateTime.Now.Month.ToString();
            string _Day = DateTime.Now.Day.ToString();
            string sql = "SELECT * FROM [EBMS].[dbo].[T_MeetShare] where IsDel = 0 and YEAR(ShareDate)  = " + _Year + " and month(ShareDate) = " + _Month + " and DAY(ShareDate)=" + _Day;
            //T_MeetShare mod = db.T_MeetShare.FirstOrDefault(a => a.IsDel == 0 && a.ShareDate < toDay);
            List<T_MeetShare> mod = db.T_MeetShare.SqlQuery(sql).ToList();
            if (mod.Count>0)
            {
                ViewData["ID"] = mod[0].ID;
                ViewData["Name"] = mod[0].Name;
                ViewData["Total"] = mod[0].Total;
                ViewData["Score"] = mod[0].Score;
            }
            else {
                ViewData["ID"] = "";
                ViewData["Name"] = "";
                ViewData["Total"] = "";
                ViewData["Score"] ="";
            }
            return View();
        }
        public JsonResult writeScore(int ID, int Score, string CurUser)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    T_MeetShareRecord flagRecord = db.T_MeetShareRecord.FirstOrDefault(a => a.Pid == ID && a.Name == CurUser);
                    if (flagRecord != null)
                    {
                        return Json(new { State = "Faile", Message = "你投过票了！" });
                    }
                    T_MeetShare mod = db.T_MeetShare.Find(ID);
                    if (mod.IsEnd == 1) {
                        return Json(new { State = "Faile", Message = "投票结束了！" });
                    }
                    mod.Score += Score;
                    mod.Total++;
                    db.SaveChanges();
                    T_MeetShareRecord modRecord = new T_MeetShareRecord();
                    modRecord.Pid = ID;
                    modRecord.Score = Score;
                    modRecord.Name = CurUser;
                    db.T_MeetShareRecord.Add(modRecord);
                    db.SaveChanges();
                    sc.Complete();
                    return Json(new { State = "Success", Message ="投票成功" });
                }
                catch (Exception ex)
                {
                    return Json(new { State = "Faile", Message = ex.Message });
                }
            }
          
        }
    }
}
