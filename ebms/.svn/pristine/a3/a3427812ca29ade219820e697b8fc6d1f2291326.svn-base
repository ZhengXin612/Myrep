using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Transactions;
using Newtonsoft.Json;
using EBMS.App_Code;
namespace EBMS.Controllers
{
    public class MeetingRoomController : BaseController
    {
        //
        // GET: /MeetingRoom/
        EBMSEntities db=new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他
        public List<SelectListItem> getApproveName( int step)
        {
           
            try
            {
                string approveName = db.T_MeetingRoomConfig.FirstOrDefault(a => a.Step == step).Name;
                string[] approveArr = approveName.Split(',');
                List<SelectListItem> approveList = new List<SelectListItem>();
                foreach (string item in approveArr)
                {
                    SelectListItem selectItem = new SelectListItem();
                    selectItem.Text = item;
                    selectItem.Value = item;
                    approveList.Add(selectItem);
                }
                return approveList;

            }
            catch (Exception)
            {
                List<SelectListItem> approveList = new List<SelectListItem> { 
                new SelectListItem { Text="请联系人事配置审核流程",Value=""}
                };
                return approveList;
            }
        }

        public List<SelectListItem> TypeList()
        {
            List<SelectListItem> TypeList = new List<SelectListItem> {
            new SelectListItem { Text = "审核人", Value = "审核人" },
             new SelectListItem { Text = "会议室", Value = "会议室" },
            };
            return TypeList;
        }

        public List<SelectListItem> getMeetingRoomName()
        {
            
            var list = db.T_MeetingRoomConfig.Where(a=>a.Type=="会议室").AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }

        public JsonResult CheckUseState(string MeetingRoom, DateTime StartTime, DateTime EndTime)
        {
            List<T_MeetingRoom> Applylist = db.T_MeetingRoom.Where(a => a.IsDelete == 0 && a.Status != 2 && a.MeetingRoom == MeetingRoom && ((a.StartTime <= StartTime && a.EndTime > StartTime) || (a.StartTime < EndTime && a.EndTime >= EndTime) || (a.StartTime >= StartTime && a.EndTime <= EndTime))).ToList();

            return Json(new { i = Applylist.Count, list = Applylist });
        }
        #endregion

        #region 视图

          [Description("会议室申请")]
        public ActionResult ViewMeetingRoomApply()
        {
              ViewData["ApproveList"] = getApproveName(1);
              ViewData["RoomName"] = getMeetingRoomName();
              return View();
        }
          public ActionResult ViewEdit(int ID)
          {
              T_MeetingRoom model = db.T_MeetingRoom.Find(ID);
              ViewData["ApproveList"] = getApproveName(1);
              ViewData["RoomName"] = getMeetingRoomName();
              if (model != null)
              {
                  return View(model);
              }
              else
              {
                  return HttpNotFound();
              }
          }
          [Description("申请列表")]
        public ActionResult ViewList()
        {
           
            return View();
        }

          [Description("未审核列表")]
        public ActionResult ViewUncheckList()
        {
            return View();
        }

          [Description("配置列表")]
        public ActionResult ViewConfigList()
        {
            return View();
        }

         [Description("审核页面")]
        public ActionResult ViewCheck(int PID)
        {
            T_MeetingRoomApprove model = db.T_MeetingRoomApprove.FirstOrDefault(a => a.PID == PID && a.Status == 0);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
             
        }

         [Description("配置新增")]
        public ActionResult ViewConfigAdd()
        {
            int configCount = db.T_MeetingRoomConfig.Count();
            int maxStep = 1;
            if (configCount > 0)
            {
                maxStep = db.T_MeetingRoomConfig.Max(a => a.Step) + 1;
            }
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            ViewData["maxStep"] = maxStep;
            return View();
        }

        [Description("配置编辑")]
        public ActionResult ViewConfigEdit(int ID)
        {
            T_MeetingRoomConfig model = db.T_MeetingRoomConfig.Find(ID);
            ViewData["boolList"] = Com.BoolList;
            ViewData["TypeList"] = TypeList();
            if (model != null)
            {
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult ViewCheckDetail(int PID)
        {
            ViewData["PID"] = PID;
            return View();
        }

        public ActionResult ViewMyList()
        {
            return View();
        }

       
        #endregion

        #region 绑定数据
        [Description("列表数据")]
          public ContentResult GetList(Lib.GridPager pager, string queryStr,int isUncheck=0,int isMy=0)
          {
              string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
              IQueryable<T_MeetingRoom> queryData = db.T_MeetingRoom.Where(a => a.IsDelete == 0);
              if (isUncheck == 1)//未审核
              {
                  int[] step = (from r in db.T_MeetingRoomConfig
                                where r.isMultiple == "1" && r.Name.Contains(name)
                                select r.Step).ToArray();
                  queryData = queryData.Where(a => (a.Status == 0 || a.Status == -1) &&(a.CurrentApprove == name || step.Contains(a.Step)));
              }
              else if (isMy == 1)
              {
                  queryData = queryData.Where(a => a.PostName == name);
              }
              if (!string.IsNullOrEmpty(queryStr))
              {
                  queryData = queryData.Where(a => a.PostName != null && a.PostName.Contains(queryStr));
              }
              
              pager.totalRows = queryData.Count();
              //分页
              List<T_MeetingRoom> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
              string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
              return Content(json);
          }

        [Description("配置列表数据")]
          public ContentResult GetConfigList(Lib.GridPager pager, string queryStr)
          {
              IQueryable<T_MeetingRoomConfig> queryData = db.T_MeetingRoomConfig;
              if (!string.IsNullOrEmpty(queryStr))
              {
                  queryData = queryData.Where(a => a.Type != null && a.Type.Contains(queryStr));
              }
              pager.totalRows = queryData.Count();
              //分页
              List<T_MeetingRoomConfig> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
              string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
              return Content(json);
          }

        public ContentResult GetCheckDetailList(int PID)
        {
            List<T_MeetingRoomApprove> list = db.T_MeetingRoomApprove.Where(a => a.PID == PID).ToList();
            string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        #endregion

        #region 增删改
          #region 审核及记录
          public JsonResult MeetingroomApplySave(T_MeetingRoom model)
          {
              using (TransactionScope sc = new TransactionScope())
              {
                  try
                  {
                      string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                      model.IsDelete = 0;
                      model.PostTime = DateTime.Now;
                      model.Status = -1;
                      model.Step = 1;
                      model.PostName = name;
                      //model.CurrentApprove = approveName;
                      db.T_MeetingRoom.Add(model);
                      db.SaveChanges();

                      T_MeetingRoomApprove newApprove = new T_MeetingRoomApprove();
                      newApprove.ApproveName = model.CurrentApprove;
                      newApprove.PID = model.ID;
                      newApprove.Status = 0;
                      newApprove.Step = 1;
                      db.T_MeetingRoomApprove.Add(newApprove);
                      db.SaveChanges();
                      sc.Complete();
                      return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                  }
                  catch (Exception e)
                  {
                      return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                  }
              }
          }

          public JsonResult EditSave(T_MeetingRoom model)
          {
              using (TransactionScope sc = new TransactionScope())
              {
                  try
                  {
                      T_MeetingRoom editModel = db.T_MeetingRoom.Find(model.ID);
                      editModel.MeetingRoom = model.MeetingRoom;
                      editModel.StartTime = model.StartTime;
                      editModel.EndTime = model.EndTime;
                      editModel.Host = model.Host;
                      editModel.MeetingTheme = model.MeetingTheme;
                      editModel.Memo = model.Memo;
                      editModel.PeopleNum = model.PeopleNum;
                      editModel.CurrentApprove = model.CurrentApprove;

                      editModel.EndTime = model.EndTime;


                      editModel.StartTime = model.StartTime;

                      if (editModel.Status == 2)
                      {
                          editModel.Status = -1;
                          editModel.Step = 1;

                          T_MeetingRoomApprove newApprove = new T_MeetingRoomApprove();
                          newApprove.ApproveName = model.CurrentApprove;
                          newApprove.PID = model.ID;
                          newApprove.Status = 0;
                          newApprove.Step = 1;
                          db.T_MeetingRoomApprove.Add(newApprove);
                      }


                      db.SaveChanges();
                      sc.Complete();
                      return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                  }
                  catch (Exception e)
                  {
                      return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                  }
              }

          }

          public JsonResult Delete(int ID)
          {
              try
              {
                  T_MeetingRoom delModel = db.T_MeetingRoom.Find(ID);
                  delModel.IsDelete = 1;
                  db.Entry<T_MeetingRoom>(delModel).State = System.Data.Entity.EntityState.Modified;
                  db.SaveChanges();
                  return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
              }
              catch (Exception e)
              {
                  return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
              }
          }
          [HttpPost]
          public JsonResult CheckSave(T_MeetingRoomApprove model)
          {
              using (TransactionScope sc = new TransactionScope())
              {
                  try
                  {
                      string name = Server.UrlDecode(Request.Cookies["NickName"].Value);
                      T_MeetingRoomApprove editModel = db.T_MeetingRoomApprove.Find(model.ID);
                      editModel.Status = model.Status;
                      editModel.ApproveTime = DateTime.Now;
                      editModel.ApproveName = name;
                      editModel.Memo = model.Memo;
                      db.Entry<T_MeetingRoomApprove>(editModel).State = System.Data.Entity.EntityState.Modified;
                      T_MeetingRoom MeetingModel = db.T_MeetingRoom.Find(editModel.PID);
                      if (model.Status == 1)//同意
                      {
                          int nextStep = editModel.Step + 1;
                          List<T_MeetingRoomConfig> ConfigList = db.T_MeetingRoomConfig.Where(a => a.Step == nextStep).ToList();
                          if (ConfigList.Count > 0)//下一步
                          {
                              MeetingModel.Status = 0;
                              MeetingModel.Step = nextStep;
                              MeetingModel.CurrentApprove = ConfigList.First().Name;

                              T_MeetingRoomApprove newApprove = new T_MeetingRoomApprove();
                              newApprove.ApproveName = ConfigList.First().Name;
                              newApprove.PID = editModel.PID;
                              newApprove.Status = 0;
                              newApprove.Step = nextStep;
                              db.T_MeetingRoomApprove.Add(newApprove);
                          }
                          else//结束
                          {

                              MeetingModel.Status = 1;
                              MeetingModel.Step = 99;
                              
                          }
                      }
                      else//不同意结束
                      {
                          MeetingModel.Status = 2;
                          MeetingModel.Step = 99;
                      }
                      db.Entry<T_MeetingRoom>(MeetingModel).State = System.Data.Entity.EntityState.Modified;
                      db.SaveChanges();
                      sc.Complete();
                      return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                  }
                  catch (Exception e)
                  {
                      return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                  }
              }


          }
          #endregion
          #region 配置
          [HttpPost]
          public JsonResult ConfigAddSave(T_MeetingRoomConfig model)
          {
              try
              {
                  db.T_MeetingRoomConfig.Add(model);
                  db.SaveChanges();
                  return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
              }
              catch (Exception e)
              {
                  return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
              }
          }

          [HttpPost]
          public JsonResult ConfigEditSave(T_MeetingRoomConfig model)
          {
              try
              {
                  T_MeetingRoomConfig editModel = db.T_MeetingRoomConfig.Find(model.ID);
                  editModel.Name = model.Name;
                  editModel.Step = model.Step;
                  editModel.Type = model.Type;
                  editModel.isMultiple = model.isMultiple;
                  db.Entry<T_MeetingRoomConfig>(editModel).State = System.Data.Entity.EntityState.Modified;
                  db.SaveChanges();
                  return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
              }
              catch (Exception e)
              {
                  return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
              }
          }

          [HttpPost]
          public JsonResult ConfigDelete(int ID)
          {
              try
              {
                  T_MeetingRoomConfig delModel = db.T_MeetingRoomConfig.Find(ID);
                  db.T_MeetingRoomConfig.Remove(delModel);
                  db.SaveChanges();
                  return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
              }
              catch (Exception e)
              {
                  return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
              }
          }
          #endregion
        #endregion

    }
}
