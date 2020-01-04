using EBMS.App_Code;
using EBMS.Models;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class FixedAssetsController : BaseController
    {
        //
        // GET: /FixedAssets/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region
        public int GetMode(string reason)
        {
            T_AssetsReason r = db.T_AssetsReason.FirstOrDefault(a => a.Reason == reason);
            if (r != null && r.Mode != null)
            {
                return r.Mode;
            }
            else
            {
                return 2;
            }
        }
        #endregion
        #region 视图
        [Description("资产类型管理")]
        public ActionResult ViewAssetsTypeManagement()
        {
            return View();
        }
        [Description("资产类型新增")]
        public ActionResult ViewAssetsTypeAdd()
        {
            return View();
        }
        [Description("资产类型编辑")]
        public ActionResult ViewAssetsTypeEdit(string AssetsTypecode)
        {
            T_AssetsType model = db.T_AssetsType.Find(AssetsTypecode);
            return View(model);
        }
        [Description("固定资产新增")]
        public ActionResult ViewAssetsAdd()
        {
            IQueryable<T_AssetsType> TypeList = db.T_AssetsType;
            var selectList = new SelectList(TypeList, "AssetsTypecode", "AssetsTypeName");
            List<SelectListItem> ListTypeItem = new List<SelectListItem>();
            ListTypeItem.Add(new SelectListItem { Text = "请选择类型", Value = "" });
            ListTypeItem.AddRange(selectList);
            ViewData["TypeCode"] = ListTypeItem;
            return View();
        }
        [Description("上传图片")]
        public ActionResult ViewUploadPic()
        {
            return View();
        }
        [Description("固定资产扫码详情页")]//这个详情页有变更记录信息
        public ActionResult ViewAssetsDetail(string Code)
        {
            T_Assets model = db.T_Assets.Find(Code);
            return View(model);
        }
        [Description("固定资产列表")]
        public ActionResult ViewAssetsList()
        {
            return View();
        }
        [Description("固定资产详情")]//这个详情页有变更记录信息
        [AllowAnonymous]
        public ActionResult ViewScanShow(string Code)
        {

            T_Assets model = db.T_Assets.Find(Code);

            IQueryable<T_AssetsTransferRecord> queryData = db.T_AssetsTransferRecord.Where(a => a.Code == Code);
            if (queryData != null && queryData.Count() > 0)
            {
                List<T_AssetsTransferRecord> list = queryData.OrderByDescending(s => s.ID).ToList();
                ViewData["list"] = list;
            }
            else
            {
                ViewData["list"] = null;
            }
            return View(model);
        }

        [Description("固定资产详情")]//这个详情页没有变更记录信息
        public ActionResult ViewAssetsDetailPart(string Code)
        {
            T_Assets model = db.T_Assets.Find(Code);
            return View(model);
        }

        [Description("固定资产管理")]
        public ActionResult ViewAssetsManagement()
        {
            return View();
        }
        [Description("固定资产编辑")]
        public ActionResult ViewAssetsEdit(string Code, string isManager)
        {
            T_Assets model = db.T_Assets.Find(Code);
            IQueryable<T_AssetsType> TypeList = db.T_AssetsType.AsQueryable();

            SelectList selectList = new SelectList(TypeList, "AssetsTypecode", "AssetsTypeName", model.TypeCode);

            List<SelectListItem> ListTypeItem = new List<SelectListItem>();

            ListTypeItem.AddRange(selectList);
            ViewData["TypeCodeList"] = ListTypeItem;

            ViewData["isManager"] = isManager;
            return View(model);
        }
        [Description("我的固定资产")]
        public ActionResult ViewAssetsMyList(int isApply = 0)
        {
            ViewData["isApply"] = isApply;
            return View();
        }
        [Description("资产报废列表")]
        public ActionResult ViewAssetsScrapList()
        {
            return View();
        }
        [Description("固定资产分配")]
        public ActionResult ViewAssetsAssign(string Code)
        {
            T_Assets AssetsModel = db.T_Assets.Find(Code);
            if (AssetsModel == null)
            {
                return HttpNotFound();
            }
            else
            {
                T_AssetsTransferRecord model = new T_AssetsTransferRecord();
                model.Code = AssetsModel.Code;
                model.Department = AssetsModel.Department;
                model.Name = AssetsModel.Name;
                model.Owner = AssetsModel.Owner;
                model.Place = AssetsModel.Place;
                model.Responsible = AssetsModel.Responsible;
                return View(model);
            }

        }
        public ActionResult ViewAssetsTransferApply()
        {
            return View();
        }
        //[Description("资产变更申请")]
        //public ActionResult ViewAssetsTransferApplyAdd(string Code)
        //{
        //    List<SelectListItem> ListType = new List<SelectListItem> { 
        //        new SelectListItem { Text = "转移", Value = "转移" }, 
        //        new SelectListItem { Text = "更换", Value = "更换" } };
        //    ViewData["ListType"] = ListType;
        //    T_Assets AssetsModel = db.T_Assets.Find(Code);
        //    if (AssetsModel == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    else
        //    {
        //        T_AssetsTransferApply model = new T_AssetsTransferApply();
        //        model.Code = AssetsModel.Code;
        //        model.Department = AssetsModel.Department;
        //        model.Name = AssetsModel.Name;
        //        model.Owner = AssetsModel.Owner;
        //        model.Place = AssetsModel.Place;
        //        model.Responsible = AssetsModel.Responsible;
        //        return View(model);
        //    }


        //}
        [Description("资产变更申请")]
        public ActionResult ViewAssetsTransferApplyAdd()
        {
            List<SelectListItem> ListType = new List<SelectListItem> { 
                 new SelectListItem { Text = "转移", Value = "转移" }, 
                  new SelectListItem { Text = "报废", Value = "报废" },
                new SelectListItem { Text = "上交", Value = "上交" }
             };
            IEnumerable list = db.T_User.AsEnumerable();
            SelectList selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> ListUser = new List<SelectListItem>();
            ListUser.Add(new SelectListItem { Text = "请选择接收人,上交流程无需选择", Value = "" });
            ListUser.AddRange(selectList);
            ViewBag.ListUser = ListUser;
            ViewData["ListUser"] = ListUser;
            ViewData["ListType"] = ListType;
            return View();
        }
        [Description("资产变更编辑")]
        public ActionResult ViewAssetsTransferEdit(int ID)
        {
            T_AssetsTransferApply model = db.T_AssetsTransferApply.Find(ID);
            if (model != null)
            {
                List<SelectListItem> ListType = new List<SelectListItem> { 
                 new SelectListItem { Text = "转移", Value = "转移" }, 
                 new SelectListItem { Text = "报废", Value = "报废" },
                new SelectListItem { Text = "上交", Value = "上交" }
             };
                IEnumerable list = db.T_User.AsEnumerable();
                SelectList selectList = new SelectList(list, "Name", "Name");
                List<SelectListItem> ListUser = new List<SelectListItem>();
                ListUser.Add(new SelectListItem { Text = "请选择接收人,上交流程无需选择", Value = "" });
                ListUser.AddRange(selectList);
                ViewBag.ListUser = ListUser;
                ViewData["ListUser"] = ListUser;
                ViewData["ListType"] = ListType;
                return View(model);
            }
            else
            {

                return HttpNotFound();
            }
        }
        [Description("我的资产变更")]
        public ActionResult ViewAssetsTransferApplyMyList()
        {
            return View();
        }
        [Description("资产变更待审核")]
        public ActionResult ViewAssetsTransferApproveList()
        {
            return View();
        }
        [Description("资产变更审核")]
        public ActionResult ViewAssetsTransferApprove(int id = 0)
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            T_AssetsApprove model = db.T_AssetsApprove.FirstOrDefault(a => a.ApplyID == id && a.ApproveName == name && a.State == 0);
            ViewData["ID"] = model.ID;
            if (model == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View(model);
            }
        }
        [Description("资产变更已审核")]
        public ActionResult ViewAssetsTransferApprovedList()
        {
            return View();
        }
        [Description("资产转移审核详情")]
        public ActionResult ViewTransferApproveDetail(int ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        [Description("资产领用申请")]
        public ActionResult ViewAssetsRequest()
        {

            string NickName = Server.UrlDecode(Request.Cookies["NickName"].Value);
            T_User user = db.T_User.FirstOrDefault(a => a.Nickname == NickName);
            ViewData["Name"] = user.Name;
            ViewData["dept"] = getDept(user.DepartmentId);
            return View();
        }
        public ActionResult ViewAssetsNotInUseList(int NotInUse)
        {
            ViewData["NotInUse"] = NotInUse;
            return View();
        }
        public ActionResult ViewImportExcel()
        {
            return View();
        }
        [Description("资产统计列表")]
        public ActionResult ViewAssetsCountList()
        {
            return View();
        }
        public ActionResult ViewAssetsCountDetail(string TypeCode)
        {
            ViewData["TypeCode"] = TypeCode;
            return View();
        }
        public ActionResult ViewAssetsMyManagement()
        {
            return View();
        }
        #endregion
        #region  绑定数据getList
        [HttpPost]
        public ContentResult GetAssetsTypeList(Lib.GridPager pager, string TypeCode)//获取资产类型列表
        {
            IQueryable<T_AssetsType> queryData = db.T_AssetsType;
            if (!string.IsNullOrWhiteSpace(TypeCode))
            {
                queryData = queryData.Where(a => a.AssetsTypecode.Contains(TypeCode) || a.AssetsTypeName.Contains(TypeCode));
            }
            if (queryData != null && queryData.Count() > 0)
            {
                List<T_AssetsType> list = queryData.OrderByDescending(s => s.AssetsTypecode).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        public ContentResult GetViewAssetsList(Lib.GridPager pager, string queryStr, string supplier, string TypeCode, int isMy = 0, int isScrap = 0, int NotInUse = 0)//获取资产列表
        {
            IQueryable<T_Assets> queryData = db.T_Assets.Where(a => a.isDelete == "0");
            if (isScrap == 1)//如果是资产报废列表就显示已报废数据
            {
                queryData = queryData.Where(a => a.isScrap == "1");
            }
            else
            {
                queryData = queryData.Where(a => a.isScrap == "0");
            }
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.Code.Contains(queryStr) || a.Name.Contains(queryStr) || a.Owner.Contains(queryStr));
            }
            if (!string.IsNullOrWhiteSpace(supplier))
            {
                queryData = queryData.Where(a => a.BuyFrom != null && a.BuyFrom.Contains(supplier));
            }
            if (isMy == 1)//如果是我的固定资产页面则获取使用人的数据
            {
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                queryData = queryData.Where(a => a.Owner.Contains(name));
            }
            if (NotInUse == 1)//获取待分配列表数据
            {
                queryData = queryData.Where(a => a.Owner == "待分配" || a.Owner == "" || a.Owner == null);
            }
            if (!string.IsNullOrWhiteSpace(TypeCode))
            {
                queryData = queryData.Where(a => a.TypeCode == TypeCode);
            }
            if (queryData != null && queryData.Count() > 0)
            {
                List<T_Assets> list = queryData.OrderByDescending(s => s.Code).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        public ContentResult GetViewAssetsTransferList(Lib.GridPager pager, string Code)//获取资产转移记录
        {
            IQueryable<T_AssetsTransferRecord> queryData = db.T_AssetsTransferRecord.Where(a => a.Code == Code);
            if (queryData != null && queryData.Count() > 0)
            {
                List<T_AssetsTransferRecord> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
                pager.totalRows = queryData.Count();
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        public ContentResult GetViewAssetsTransferApplyList(Lib.GridPager pager, string queryStr, int isMy = 0, int isApprove = 0)//获取资产转移申请记录
        {
            string name = Server.UrlDecode(Request.Cookies["Name"].Value);
            IQueryable<T_AssetsTransferApply> queryData = db.T_AssetsTransferApply.Where(a => a.IsDelete == 0);
            if (isMy == 1)//我的转移申请记录
            {
                queryData = queryData.Where(a => a.PostUserName == name || (a.TransferOwner.Contains(name) && a.TransferType == "领用"));
            }
            if (isApprove == 1)//未审核记录
            {
                queryData = queryData.Where(a => a.CurrentApproveName.Contains(name));
            }
            if (isApprove == 2)//已审核记录
            {
                if (name != db.T_AssetsConfig.FirstOrDefault(a => a.Step == 2).Name)
                {
                    queryData = queryData.Where(a => (a.PostUserName == name && a.Step > 1) || (a.LastApproveName.Contains(name) && a.Step > 3));
                }
                else
                {
                    queryData = queryData.Where(a => (a.PostUserName == name && a.Step > 1) || (a.LastApproveName.Contains(name) && a.Step > 3) || a.Step > 2);
                }
            }
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.Code.Contains(queryStr) || a.Name.Contains(queryStr));
            }
            List<T_AssetsTransferApply> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            pager.totalRows = queryData.Count();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        public ContentResult GetTransferApproveDetailList(int ApplyID = 0)//获取申请转移的id为ApplyID的审核记录
        {
            if (ApplyID > 0)
            {
                List<T_AssetsApprove> list = db.T_AssetsApprove.Where(a => a.ApplyID == ApplyID).ToList();
                string json = "{\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";//\"total\":" + pager.totalRows + ",
                return Content(json);
            }
            else
            {
                return Content("");
            }
        }
        public class assetsCount
        {
            public string typeCode { get; set; }
            public string typeName { get; set; }
            public int cnt { get; set; }
        }
        public ContentResult GetViewAssetsCountList(Lib.GridPager pager, string queryStr)//获取资产统计列表
        {
            var queryData = (from num in
                                 (
                                 from assets in db.T_Assets
                                 group assets by assets.TypeCode into g
                                 select new
                                 {
                                     typeCode = g.Key,
                                     //typeName = typeName(g.Key),
                                     cnt = g.Count()
                                 }
                             )

                             select new assetsCount
                             {
                                 typeCode = num.typeCode,

                                 cnt = num.cnt
                             });//.ToList();



            if (queryData != null && queryData.Count() > 0)
            {
                List<assetsCount> list = queryData.ToList();
                foreach (var ac in list)
                {
                    ac.typeName = typeName(ac.typeCode);
                }
                if (!string.IsNullOrWhiteSpace(queryStr))
                {
                    list = list.Where(a => a.typeCode.Contains(queryStr) || a.typeName.Contains(queryStr)).ToList();//;
                }
                pager.totalRows = list.Count();
                list = list.OrderByDescending(s => s.typeCode).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();


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
        #region 资产类型管理
        [HttpPost]
        public JsonResult ViewAssetsTypeAddSave(T_AssetsType model)//资产类型新增保存
        {
            db.T_AssetsType.Add(model);
            int i = db.SaveChanges();
            return Json(i);
        }
        public JsonResult ViewAssetsTypeEditSave(T_AssetsType model)//资产类型编辑保存
        {
            T_AssetsType EditModel = db.T_AssetsType.Find(model.AssetsTypecode);
            EditModel.AssetsTypeName = model.AssetsTypeName;
            db.Entry<T_AssetsType>(EditModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i);
        }
        [HttpPost]
        public JsonResult CheckAssetsTypeDelete(string AssetsTypecode)//资产类型删除时检查资产表中是否有该类型编码的资产,若有不允许删除
        {
            IQueryable<T_Assets> assets = db.T_Assets.Where(a => a.TypeCode == AssetsTypecode);
            if (assets.Count() > 0)
            {
                return Json(-1);
            }
            else
            {
                return Json(1);
            }
        }
        [HttpPost]
        public JsonResult AssetsTypeDelete(string AssetsTypecode)//资产类型删除
        {
            T_AssetsType DelModel = db.T_AssetsType.Find(AssetsTypecode);
            db.T_AssetsType.Remove(DelModel);
            int i = db.SaveChanges();
            return Json(i);
        }
        #endregion
        #region 固定资产管理
        [HttpPost]
        public JsonResult ViewAssetsAddSave(T_Assets model)//固定资产新增
        {
            model.isDelete = "0";
            model.isScrap = "0";
            db.T_Assets.Add(model);
            int i = db.SaveChanges();
            return Json(i);
        }
        public JsonResult ViewAssetsEditSave(T_Assets model)//固定资产编辑
        {
            T_Assets editModel = db.T_Assets.Find(model.Code);
            editModel.Barcode = model.Barcode;
            editModel.BuyDate = model.BuyDate;
            editModel.Buyer = model.Buyer;
            editModel.BuyFrom = model.BuyFrom;
            editModel.Code = model.Code;
            editModel.Cost = model.Cost;
            editModel.Department = model.Department;
            editModel.Guarantee = model.Guarantee;
            editModel.Memo = model.Memo;
            editModel.Name = model.Name;
            editModel.Owner = model.Owner;
            editModel.Pic = model.Pic;
            editModel.Place = model.Place;
            editModel.Responsible = model.Responsible;
            editModel.Spec = model.Spec;
            editModel.TypeCode = model.TypeCode;
            db.Entry<T_Assets>(editModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i);

        }
        public JsonResult AssetsDelete(string Code)//固定资产删除
        {
            T_Assets DelModel = db.T_Assets.Find(Code);
            DelModel.isDelete = "1";
            db.Entry<T_Assets>(DelModel).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();
            return Json(i);
        }
        public JsonResult ViewAssetsAssignAddSave(T_AssetsTransferRecord model)//固定资产分配
        {
            try
            {
                T_Assets AssetsModel = db.T_Assets.Find(model.Code);
                AssetsModel.Owner = model.TransferOwner;
                AssetsModel.Department = model.TransferDepartment;
                AssetsModel.Place = model.TransferPlace;
                AssetsModel.Responsible = model.TransferResponsible;
                db.Entry<T_Assets>(AssetsModel).State = System.Data.Entity.EntityState.Modified;//修改固定资产的使用人等

                model.TransferDate = DateTime.Now;
                db.T_AssetsTransferRecord.Add(model);//添加一条资产转移记录
                int i = db.SaveChanges();
                return Json(i);
            }
            catch (Exception e)
            {
                return Json(0);
            }
        }
        #endregion
        #region 资产转移
        [HttpPost]
        public JsonResult AssetsTransferApplySave(T_AssetsTransferApply model)//资产转移申请保存
        {
            int i = 0;
            using (TransactionScope sc = new TransactionScope())
            {
                string name = Server.UrlDecode(Request.Cookies["Name"].Value);
                string assetsManagement = db.T_AssetsConfig.FirstOrDefault(a => a.Type == "资产管理员").Name;//资产管理员姓名
                string deptID = db.T_User.FirstOrDefault(a => a.Name == name).DepartmentId;
                T_User Manager = db.T_User.FirstOrDefault(a => a.DepartmentId == deptID && a.IsManagers == "1");//主管

                model.IsDelete = 0;
                model.PostDate = DateTime.Now;
                model.State = -1;
                model.Step = 1;
                model.PostUserName = name;
                int Mode = 0;
                T_AssetsConfig config = new T_AssetsConfig();
                if (model.TransferType == "转移")//转移流程如果申请人在负责人里,则第一步为自己审核，否则第一步为主管审核,
                {
                    T_AssetsReason Reason = db.T_AssetsReason.FirstOrDefault(a => a.Reason == model.TransferType);
                    if (Reason == null)
                    {

                    }
                    Mode = Reason.Mode;
                    config = db.T_AssetsConfig.FirstOrDefault(a => a.Mode == Mode && a.Step == 1);
                    model.TransferOwner = model.Owner.Replace(name, model.TransferOwner);  //转移后的使用人

                    if (model.Responsible != null && model.Responsible.Contains(name))//如果转移前的责任人中有申请人，则第一步审核人就是申请人
                    {
                        model.CurrentApproveName = name;
                        model.TransferResponsible = model.Responsible.Replace(name, model.LastApproveName);
                    }
                    else//申请人不在责任人里
                    {

                        if (Manager == null && string.IsNullOrWhiteSpace(Manager.Name))
                        {
                            model.CurrentApproveName = assetsManagement;//如果申请人没有主管,则给资产管理员审核
                        }
                        else
                        {
                            model.TransferResponsible = model.Responsible.Replace(Manager.Name, model.LastApproveName);
                            model.CurrentApproveName = Manager.Name;//主管
                        }
                    }
                }
                else if (model.TransferType == "上交")
                {
                    T_AssetsReason Reason = db.T_AssetsReason.FirstOrDefault(a => a.Reason == model.TransferType);
                    if (Reason == null)
                    {

                    }
                    Mode = Reason.Mode;
                    config = db.T_AssetsConfig.FirstOrDefault(a => a.Mode == Mode && a.Step == 1);
                    model.CurrentApproveName = config.Name;
                    // model.CurrentApproveName = assetsManagement;//资产管理员
                    if (model.Owner.Contains(","))//认为是一个资产多个人使用,其中一个人上交,则转移前后地点和部门不变,无接收人
                    {
                        model.TransferDepartment = model.Department;
                        model.TransferPlace = model.Place;
                        model.LastApproveName = "";
                        int Oindex = model.Owner.IndexOf(name);
                        if (Oindex == 0)//如果名字在第一个,则删去名字及名字后面的逗号,否则删去名字及名字前面的一个逗号
                        {
                            model.TransferOwner = model.Owner.Replace(name + ",", "");
                        }
                        else
                        {
                            model.TransferOwner = model.Owner.Replace("," + name, "");
                        }
                        int Rindex = model.Responsible.IndexOf(name);
                        if (Rindex == 0)
                        {
                            model.TransferResponsible = model.Responsible.Replace(name + ",", "");
                        }
                        else
                        {
                            model.TransferResponsible = model.Responsible.Replace("," + name, "");
                        }
                    }
                    else//一个人使用的资产上交后则为待分配,接收人则是资产管理员
                    {
                        model.TransferDepartment = db.T_AssetsConfig.FirstOrDefault(a => a.Type == "部门").Name;
                        model.TransferOwner = "待分配";
                        model.TransferPlace = db.T_AssetsConfig.FirstOrDefault(a => a.Type == "地点").Name;
                        model.TransferResponsible = assetsManagement;
                        model.LastApproveName = assetsManagement;
                    }
                }
                else if (model.TransferType == "领用")//领用只做了资产管理员一步审核
                {
                    T_AssetsReason Reason = db.T_AssetsReason.FirstOrDefault(a => a.Reason == model.TransferType);
                    if (Reason == null)
                    {

                    }
                    Mode = Reason.Mode;
                    config = db.T_AssetsConfig.FirstOrDefault(a => a.Mode == Mode && a.Step == 1);
                    model.CurrentApproveName = config.Name;
                    // model.CurrentApproveName = assetsManagement;//资产管理员
                    if (model.Owner == "待分配" || model.Owner == "" || model.Owner == null)
                    {
                        model.TransferResponsible = model.LastApproveName;
                    }
                    else
                    {
                        model.TransferOwner = model.Owner + model.TransferOwner;
                        model.TransferResponsible = model.Responsible + model.LastApproveName;
                    }
                    // model.LastApproveName = assetsManagement;
                }
                else if (model.TransferType == "报废")
                {
                    T_AssetsReason Reason = db.T_AssetsReason.FirstOrDefault(a => a.Reason == model.TransferType);
                    if (Reason == null)
                    {

                    }
                    Mode = Reason.Mode;
                    config = db.T_AssetsConfig.FirstOrDefault(a => a.Mode == Mode && a.Step == 1);
                    model.CurrentApproveName = config.Name;
                    // model.CurrentApproveName = assetsManagement;//资产管理员
                    model.TransferDepartment = "无";
                    model.TransferPlace = "无";
                    model.TransferOwner = "报废";
                    model.TransferResponsible = "无";
                    model.LastApproveName = "";
                }
                db.T_AssetsTransferApply.Add(model);//添加一条申请记录
                db.SaveChanges();

                T_AssetsApprove approve = new T_AssetsApprove();
                approve.ApplyID = model.ID;
                approve.ApproveName = model.CurrentApproveName;
                approve.Code = model.Code;
                approve.Memo = "";
                approve.State = 0;
                approve.Step = "1";
                approve.Mode = Mode;
                approve.ApproveType = config.Type;
                db.T_AssetsApprove.Add(approve);//添加一条审核记录
                i = db.SaveChanges();
                sc.Complete();
            }

           // ModularByZP();
            return Json(i);
        }
        [HttpPost]
        public JsonResult AssetsTransferApplyEditSave(T_AssetsTransferApply model)//资产转移申请记录编辑保存
        {
            T_AssetsTransferApply editApply = db.T_AssetsTransferApply.Find(model.ID);
            if (editApply == null)
            {
                return Json(-1);
            }
            else
            {

                editApply.Memo = model.Memo;
                editApply.TransferDepartment = model.TransferDepartment;
                editApply.TransferOwner = editApply.TransferOwner.Replace(editApply.LastApproveName, model.LastApproveName);
                editApply.TransferPlace = model.TransferPlace;
                editApply.TransferResponsible = editApply.TransferResponsible.Replace(editApply.LastApproveName, model.LastApproveName);
                if (model.TransferType == "转移")
                {
                    editApply.LastApproveName = model.LastApproveName;
                }
                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();

                //ModularByZP();

                return Json(i);
            }

        }
        [HttpPost]
        public JsonResult AssetsTransferApproveSave(T_AssetsApprove model)//资产审核保存
        {
            T_AssetsApprove editApprove = db.T_AssetsApprove.Find(model.ID);
            T_AssetsTransferApply editApply = db.T_AssetsTransferApply.Find(model.ApplyID);
            T_Assets editAssets = db.T_Assets.Find(editApply.Code);
            int i = 0;
            if (editApprove != null)//修改审核记录
            {
                editApprove.State = model.State;
                editApprove.Memo = model.Memo;
                editApprove.ApproveDate = DateTime.Now;
                db.Entry<T_AssetsApprove>(editApprove).State = System.Data.Entity.EntityState.Modified;
            }
            if (editApply != null)
            {
                if (model.State == 1)//同意
                {
                    int nextStep = int.Parse(editApprove.Step) + 1;
                    int Mode = editApprove.Mode;
                    T_AssetsConfig nextConfig = db.T_AssetsConfig.FirstOrDefault(a => a.Mode == editApprove.Mode && a.Step == nextStep);
                    #region 接收人审核时修改资产信息
                    if (editApprove.ApproveType == "接收人")
                    {
                        List<T_AssetsApprove> ReceivedList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step && a.State == 1).ToList();//接收过的数据
                        List<T_AssetsApprove> ApproveList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step && a.State == 0).ToList();//待接收的数据
                        List<T_AssetsApprove> ApproveALl = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step).ToList();
                        if (ApproveALl.Count > 1)//这条数据的接收人有多个
                        {
                            editApply.CurrentApproveName = editApply.CurrentApproveName.Replace(editApprove.ApproveName, "");
                            if (ReceivedList.Count == 0)//没有人接收过
                            {
                                //修改资产的使用人等信息
                                editAssets.Department = editApply.TransferDepartment;
                                editAssets.Owner = editAssets.Owner.Replace(editApply.PostUserName, editApprove.ApproveName);
                                editAssets.Place = editApply.TransferPlace;
                                editAssets.Responsible = editAssets.Responsible.Replace(editApply.PostUserName, editApprove.ApproveName);
                                db.Entry<T_Assets>(editAssets).State = System.Data.Entity.EntityState.Modified;

                                T_AssetsTransferRecord TransferRecord = new T_AssetsTransferRecord();//添加一条资产变更记录
                                TransferRecord.Code = editApply.Code;
                                TransferRecord.Department = editApply.Department;
                                TransferRecord.Memo = editApply.Memo;
                                TransferRecord.Name = editApply.Name;
                                TransferRecord.Owner = editApply.Owner;
                                TransferRecord.Place = editApply.Place;
                                TransferRecord.Responsible = editApply.Responsible;
                                TransferRecord.TransferDate = DateTime.Now;
                                TransferRecord.TransferDepartment = editApply.TransferDepartment;
                                TransferRecord.TransferOwner = editApply.Owner.Replace(editApply.PostUserName, editApprove.ApproveName);
                                TransferRecord.TransferPlace = editApply.TransferPlace;
                                TransferRecord.TransferResponsible = editApply.Responsible.Replace(editApply.PostUserName, editApprove.ApproveName);
                                TransferRecord.PostUserName = editApply.PostUserName;
                                TransferRecord.Receiver = editApprove.ApproveName;//第一个人接收时 则接收人是这个人
                                TransferRecord.ApplyID = editApply.ID;
                                TransferRecord.TransferType = editApply.TransferType;
                                db.T_AssetsTransferRecord.Add(TransferRecord);
                            }
                            else
                            {
                                //修改资产的使用人等信息
                                editAssets.Department = editApply.TransferDepartment;
                                editAssets.Owner = editAssets.Owner + "," + editApprove.ApproveName;
                                editAssets.Place = editApply.TransferPlace;
                                editAssets.Responsible = editAssets.Responsible + "," + editApprove.ApproveName;
                                db.Entry<T_Assets>(editAssets).State = System.Data.Entity.EntityState.Modified;

                                T_AssetsTransferRecord EditRecord = db.T_AssetsTransferRecord.FirstOrDefault(a => a.ApplyID == editApply.ID);
                                EditRecord.Receiver = EditRecord.Receiver + "," + editApprove.ApproveName;//第二个开始来接收的人开始则加上这个人的名字
                                EditRecord.TransferOwner = EditRecord.TransferOwner + "," + editApprove.ApproveName;
                                EditRecord.TransferResponsible = EditRecord.TransferResponsible + "," + editApprove.ApproveName;
                                db.Entry<T_AssetsTransferRecord>(EditRecord).State = System.Data.Entity.EntityState.Modified;
                            }


                        }
                        else//这条数据只有一个人接收
                        {
                            //修改资产的使用人等信息
                            editAssets.Department = editApply.TransferDepartment;
                            editAssets.Owner = editAssets.Owner.Replace(editApply.PostUserName, editApply.LastApproveName);
                            editAssets.Place = editApply.TransferPlace;
                            editAssets.Responsible = editApply.TransferResponsible;
                            db.Entry<T_Assets>(editAssets).State = System.Data.Entity.EntityState.Modified;

                            T_AssetsTransferRecord TransferRecord = new T_AssetsTransferRecord();//添加一条资产变更记录
                            TransferRecord.Code = editApply.Code;
                            TransferRecord.Department = editApply.Department;
                            TransferRecord.Memo = editApply.Memo;
                            TransferRecord.Name = editApply.Name;
                            TransferRecord.Owner = editApply.Owner;
                            TransferRecord.Place = editApply.Place;
                            TransferRecord.Responsible = editApply.Responsible;
                            TransferRecord.TransferDate = DateTime.Now;
                            TransferRecord.TransferDepartment = editApply.TransferDepartment;
                            TransferRecord.TransferOwner = editApply.TransferOwner;
                            TransferRecord.TransferPlace = editApply.TransferPlace;
                            TransferRecord.TransferResponsible = editApply.TransferResponsible;
                            TransferRecord.PostUserName = editApply.PostUserName;
                            TransferRecord.Receiver = editApply.LastApproveName;
                            TransferRecord.ApplyID = editApply.ID;
                            TransferRecord.TransferType = editApply.TransferType;
                            db.T_AssetsTransferRecord.Add(TransferRecord);
                        }
                        IQueryable<T_AssetsTransferApply> TransferApplyList = db.T_AssetsTransferApply.Where(a => (a.State == -1 || a.State == 0) && a.Code == editApply.Code && a.ID != editApply.ID);
                        if (TransferApplyList.Count() > 0)
                        {
                            foreach (T_AssetsTransferApply item in TransferApplyList)//一个资产同时发生多条变更记录的时候,同意了其中一个的转移，就将其他未审核完成的记录的接收人和接收责任人等修改
                            {
                                item.TransferOwner = item.TransferOwner.Replace(editApply.PostUserName, editApply.LastApproveName);
                                item.TransferResponsible = item.TransferResponsible.Replace(editApply.PostUserName, editApply.LastApproveName);
                                item.Owner = item.Owner.Replace(editApply.PostUserName, editApply.LastApproveName);
                                item.Responsible = item.Responsible.Replace(editApply.PostUserName, editApply.LastApproveName);
                            }
                        }
                    }
                    #endregion
                    if (nextConfig != null)//存在下一步
                    {
                        string approvename = nextConfig.Name;
                        if (approvename == "接收人" && editApply.LastApproveName.Contains(","))
                        {

                            string[] approveArr = approvename.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var approve in approveArr)
                            {
                                T_AssetsApprove newApprove = new T_AssetsApprove();
                                newApprove.ApplyID = editApprove.ApplyID;
                                newApprove.ApproveName = approve;
                                newApprove.Code = editApprove.Code;
                                newApprove.State = 0;
                                newApprove.Step = nextStep.ToString();
                                newApprove.ApproveType = nextConfig.Type;
                                db.T_AssetsApprove.Add(newApprove);
                            }
                        }
                        
                        else
                        {
                            T_AssetsApprove newApprove = new T_AssetsApprove();
                            newApprove.ApplyID = editApprove.ApplyID;
                            newApprove.ApproveName = nextConfig.Name;
                            newApprove.Code = editApprove.Code;
                            newApprove.State = 0;
                            newApprove.Step = nextStep.ToString();
                            newApprove.Mode = editApprove.Mode;
                            newApprove.ApproveType = nextConfig.Type;
                            if (nextConfig.Name == "接收人")
                            {
                                newApprove.ApproveName = editApply.LastApproveName;

                            }
                            db.T_AssetsApprove.Add(newApprove);
                        }
                        if (approvename == "接收人")
                        {
                            approvename = editApply.LastApproveName;
                          
                        }
                        editApply.Step = nextStep;
                        editApply.State = 0;//审核中
                        editApply.CurrentApproveName = approvename;
                        db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                    }
                    else//同意结束
                    {
                        editApply.Step = 9;
                        editApply.State = 1;//已同意
                        editApply.CurrentApproveName = "流程结束";
                        db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;

                        if (editApply.TransferType != "转移")//上交 /报废/领用
                        {
                            T_AssetsTransferRecord TransferRecord = new T_AssetsTransferRecord();//添加一条资产变更记录
                            TransferRecord.Code = editApply.Code;
                            TransferRecord.Department = editApply.Department;
                            TransferRecord.Memo = editApply.Memo;
                            TransferRecord.Name = editApply.Name;
                            TransferRecord.Owner = editApply.Owner;
                            TransferRecord.Place = editApply.Place;
                            TransferRecord.Responsible = editApply.Responsible;
                            TransferRecord.TransferDate = DateTime.Now;
                            TransferRecord.TransferDepartment = editApply.TransferDepartment;
                            TransferRecord.TransferOwner = editApply.TransferOwner;
                            TransferRecord.TransferPlace = editApply.TransferPlace;
                            TransferRecord.TransferResponsible = editApply.TransferResponsible;
                            TransferRecord.PostUserName = editApply.PostUserName;
                            TransferRecord.Receiver = editApply.LastApproveName;
                            TransferRecord.ApplyID = editApply.ID;
                            TransferRecord.TransferType = editApply.TransferType;
                            db.T_AssetsTransferRecord.Add(TransferRecord);
                           
                            editAssets.Department = editApply.TransferDepartment;
                            editAssets.Owner = editApply.TransferOwner;
                            editAssets.Place = editApply.TransferPlace;
                            editAssets.Responsible = editApply.TransferResponsible;
                            db.Entry<T_Assets>(editAssets).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                   
                }

                else//不同意
                {
                    #region 转移
                    if (editApply.TransferType == "转移" && editApprove.ApproveType == "接收人")//转移的第三步有可能有多个人接收就会有多个人审核
                    {
                        List<T_AssetsApprove> ReceivedList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step && a.State == 1).ToList();//接收过的数据
                        List<T_AssetsApprove> ApproveList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step && a.State == 0).ToList();//待接收的数据
                        List<T_AssetsApprove> ApproveALl = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == editApprove.Step).ToList();
                        if (ApproveALl.Count > 1)//这条数据的接收人有多个
                        {
                            if (ApproveList.Count > 1)//还有多个人要审核
                            {
                                editApply.CurrentApproveName = editApply.CurrentApproveName.Replace(editApprove.ApproveName, "");
                                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                            }
                            else//最后一个接收人审核
                            {
                                editApply.Step = 9;
                                if (ReceivedList.Count > 0)
                                {
                                    editApply.State = 1;//有人接收表示已同意
                                }
                                else
                                {
                                    editApply.State = 2;//没有任何一个人接收则为不同意
                                }
                                editApply.CurrentApproveName = "流程结束";
                                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        else//一人接收
                        {
                            editApply.Step = 9;
                            editApply.State = 2;//不同意
                            editApply.CurrentApproveName = "流程结束";
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    #endregion
                    #region 其他
                    else
                    {
                        editApply.Step = 9;
                        editApply.State = 2;//不同意
                        editApply.CurrentApproveName = "流程结束";
                        db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.Entity.EntityState.Modified;
                    }
                    #endregion
                }
            }
            i = db.SaveChanges();
            try
            {
               // ModularByZP();


            }
            catch (DbUpdateException e)
            {

            }
            return Json(i);
        }
        public void ModularByZP()
        {
            List<T_ModularNotaudited> ModularNotaudited = db.T_ModularNotaudited.Where(a => a.ModularName == "资产变更").ToList();
            if (ModularNotaudited.Count > 0)
            {
                foreach (var item in ModularNotaudited)
                {
                    db.T_ModularNotaudited.Remove(item);
                }
                db.SaveChanges();
            }

            string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_AssetsApprove where  ApplyID in ( select ID from T_AssetsTransferApply where isDelete=0 and (status=-1 or status=0) ) and  State=0 and ApproveDate is null GROUP BY ApproveName  ";
            List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
            string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
            {
                string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资产变更" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资产变更";
                    ModularNotauditedModel.RejectNumber = 0;
                    ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    ModularNotauditedModel.ToupdateName = Nickname;
                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
                }
                db.SaveChanges();
            }
            //增加驳回数据
            string RejectNumberSql = " select PostUserName as PendingAuditName,COUNT(*) as  NotauditedNumber  from T_AssetsTransferApply where State='2' and IsDelete=0   GROUP BY PostUserName ";
            List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

            for (int e = 0; e < RejectNumberQuery.Count; e++)
            {
                string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资产变更" && a.PendingAuditName == PendingAuditName);
                if (NotauditedModel != null)
                {
                    NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                    ModularNotauditedModel.ModularName = "资产变更";
                    ModularNotauditedModel.NotauditedNumber = 0;
                    ModularNotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                    ModularNotauditedModel.PendingAuditName = RejectNumberQuery[e].PendingAuditName;
                    ModularNotauditedModel.ToupdateDate = DateTime.Now;
                    ModularNotauditedModel.ToupdateName = Nickname;
                    db.T_ModularNotaudited.Add(ModularNotauditedModel);
                }
                db.SaveChanges();
            }
        }


        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
        [HttpPost]
        public JsonResult AssetsTransferDelete(int ID)//资产转移申请记录删除
        {
            T_AssetsTransferApply delApply = db.T_AssetsTransferApply.Find(ID);
            if (delApply == null)
            {
                return Json(0);
            }
            else
            {
                IQueryable<T_AssetsApprove> delApprove = db.T_AssetsApprove.Where(a => a.ApplyID == ID);
                if (delApprove.Count() > 0)
                {
                    foreach (T_AssetsApprove item in delApprove)
                    {
                        item.ApproveDate = DateTime.Now;
                        item.State = 9;
                        item.Memo = "申请记录已删除";
                    }
                }
                delApply.IsDelete = 1;
            }
            db.Entry<T_AssetsTransferApply>(delApply).State = System.Data.Entity.EntityState.Modified;
            int i = db.SaveChanges();

            //ModularByZP();


            return Json(i);
        }
        #endregion
        #endregion
        #region 其他
        public string typeName(string typeCode)//根据类型代码查找类型名称
        {
            T_AssetsType typeModel = db.T_AssetsType.Find(typeCode);
            if (typeModel == null)
            {
                return "";
            }
            else
            {
                return typeModel.AssetsTypeName;
            }
        }
        [HttpPost]
        public JsonResult checkTypeCode(string AssetsTypecode)//检查类型编码是否唯一
        {
            T_AssetsType model = db.T_AssetsType.Find(AssetsTypecode);
            if (model == null)
            {
                return Json(0);
            }
            else
            {
                return Json(1);
            }
        }
        [HttpPost]
        public JsonResult checkAssetsCode(string Code)//检查资产编码是否唯一
        {
            T_Assets model = db.T_Assets.Find(Code);
            if (model == null)
            {
                return Json(0);
            }
            else
            {
                return Json(1);
            }
        }
        [HttpPost]
        public String UploadPic()
        {
            string finaly = "";
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file];//get post file 
                if (postFile.ContentLength == 0)
                {
                    return " <script > alert('文件为空,请重新选择');window.location.href='ViewUploadPic';  </script>";
                }

                else
                {

                    string newFilePath = Server.MapPath("~/Upload/AssetsImage/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName);

                    var lastIndex = fileName.LastIndexOf(".");
                    var result = fileName.Substring(lastIndex + 1);
                    if ("jpg" != result && "gif" != result && "png" != result)
                    {

                        return " <script > alert('请选择图片文件');window.location.href='ViewUploadPic'  </script>";
                    }
                    else
                    {
                        finaly = "/Upload/AssetsImage/" + fileName;
                        postFile.SaveAs(newFilePath + fileName);//save file 
                    }


                }
            }
            return "<script >parent.$('#Pic').val('" + finaly + "');alert('上传成功');parent.$('#uploadDiv').dialog('close');</script>";
        }
        public string getDept(string id)//根据部门id获取部门名称
        {

            int ID = Convert.ToInt32(id);
            T_Department deptModel = db.T_Department.Find(ID);
            if (deptModel != null)
            {
                string deptName = deptModel.Name;
                return deptName;
            }
            else
            {
                return "";
            }

        }
        [HttpPost]
        public JsonResult getUser(string name)//根据真名获取user
        {
            T_User user = db.T_User.FirstOrDefault(a => a.Name == name);
            if (user != null)
            {
                string deptName = getDept(user.DepartmentId);
                return Json(deptName);
            }
            else
            {
                return Json("");
            }
        }
        [HttpPost]
        public JsonResult checkTransferApply(string Code)//检查是否存在该资产的正在审核中的申请
        {
            IQueryable<T_AssetsApprove> approves = db.T_AssetsApprove.Where(a => a.Code == Code && a.State == 0);
            IQueryable<T_AssetsTransferApply> applys = from apply in db.T_AssetsTransferApply
                                                       where (from approve in approves select approve.ApplyID).Contains(apply.ID)
                                                              && apply.TransferType == "转移"
                                                       select apply;


            int i = applys.Count();
            return Json(i);
        }


        public string ImportExcel()//导入excel
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    AboutExcel AE = new AboutExcel();
                    int s = 0;

                    string message = "导入成功";//给用户的提示消息
                    string repeatCode = "";
                    foreach (string file in Request.Files)
                    {
                        HttpPostedFileBase postFile = Request.Files[file];//get post file 

                        if (postFile.ContentLength == 0)
                        {
                            return " <script > alert('文件为空，请重新选择');window.history.go(-1);  </script>";
                        }
                        else
                        {
                            string newFilePath = Server.MapPath("~/Upload/AssetsExcel/");//save path 
                            string fileName = Path.GetFileName(postFile.FileName);
                            postFile.SaveAs(newFilePath + fileName);//save file 
                            DataTable dt = AE.ImportExcelFile(newFilePath + fileName);
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                string code = "";
                                if (dt.Rows[i][4] == null || string.IsNullOrWhiteSpace(dt.Rows[i][4].ToString()))
                                {
                                    return " <script > parent.$('#openDivImport').dialog('close');  alert('不允许上传空的资产代码！')  </script>";
                                }
                                else
                                {
                                    code = dt.Rows[i][4].ToString();
                                }
                                List<T_Assets> AssetsBycode = db.T_Assets.Where(a => a.Code == code).ToList();//根据资产编码查重
                                if (AssetsBycode.Count == 0)
                                {
                                    T_Assets Assets = new T_Assets();
                                    Assets.Pic = "/" + dt.Rows[i][1].ToString();
                                    Assets.Name = dt.Rows[i][2].ToString();
                                    Assets.Spec = dt.Rows[i][3].ToString();
                                    Assets.Code = dt.Rows[i][4].ToString();
                                    if (dt.Rows[i][5] == null || dt.Rows[i][5].ToString().Trim() == "")
                                    {
                                        return " <script > parent.$('#openDivImport').dialog('close');  alert('请填写所有代码的成本价后再上传！')  </script>";
                                    }
                                    else
                                    {
                                        Assets.Cost = decimal.Parse(dt.Rows[i][5].ToString());
                                    }
                                    Assets.Buyer = dt.Rows[i][6].ToString();
                                    Assets.BuyFrom = dt.Rows[i][7].ToString();
                                    Assets.Guarantee = dt.Rows[i][8].ToString();

                                    try
                                    {
                                        Assets.BuyDate = DateTime.Parse(dt.Rows[i][9].ToString());
                                    }
                                    catch (Exception)
                                    {
                                        return " <script > parent.$('#openDivImport').dialog('close');  alert('Excel日期格式不正确！')  </script>";
                                    }


                                    Assets.Department = dt.Rows[i][10].ToString();
                                    Assets.Owner = dt.Rows[i][11].ToString();
                                    Assets.Place = dt.Rows[i][12].ToString();
                                    Assets.Responsible = dt.Rows[i][13].ToString();
                                    Assets.Memo = dt.Rows[i][14].ToString();
                                    Assets.Barcode = dt.Rows[i][15].ToString();
                                    Assets.isScrap = "0";
                                    Assets.isDelete = "0";
                                    Assets.TypeCode = "001";

                                    //上传需要上传资产类型
                                    db.T_Assets.Add(Assets);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    repeatCode += code;
                                }
                                s++;

                            }
                            if (repeatCode != "")
                            {
                                message += "," + repeatCode + "代码重复,请核实";
                            }


                            sc.Complete();
                        }
                    }
                    return "<script> parent.$('#ImportDiv').dialog('close');  alert('" + message + "'); </script>";
                }
                catch (Exception e)
                {
                    return "<script> parent.$('#ImportDiv').dialog('close');  alert('" + e.Message + "'); </script>";
                }
            }

        }
        #endregion
    }
}
