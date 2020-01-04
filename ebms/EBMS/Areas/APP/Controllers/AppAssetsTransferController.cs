using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
namespace EBMS.Areas.APP.Controllers
{
    public class AppAssetsTransferController : Controller
    {
        //资产变更
        // GET: /APP/AppAssetsTransfer/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(int id)
        {
            ViewData["ID"] = id;
            return View();
        }
        class resultItem
        {
            public string title { get; set; }//申请人
            public int uid { get; set; }//id
            public string subTitle { get; set; }//理由
            public int remark { get; set; }//状态

        }
        //部门 ID转换中文名
        public string GetDaparementString(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int cid = int.Parse(id);
                List<T_Department> model = db.T_Department.Where(a => a.ID == cid).ToList();
                if (model.Count > 0)
                {
                    return model[0].Name;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        //资产变更列表
        public JsonResult GetList(string CurUser, int page, int pageSize, int Status = 9999, int myList = 0)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == CurUser);
            //真名
            string name = MOD_User.Name;
            List<T_AssetsApprove> ApproveMod = new List<T_AssetsApprove>();
            if (Status == 9999)
            {
                ApproveMod = db.T_AssetsApprove.Where(a => a.ApproveName == name).ToList();
            }
            else
            {
                ApproveMod = db.T_AssetsApprove.Where(a => a.ApproveName == name && a.State == Status).ToList();
            }
            int[] Arry = new int[ApproveMod.Count];
            for (int i = 0; i < ApproveMod.Count; i++)
            {
                Arry[i] = int.Parse(ApproveMod[i].ApplyID.ToString());
            }
            IQueryable<T_AssetsTransferApply> queryData = null;
            //myList 我申请的？ 0 否 1是
            if (myList == 0)
            {
                queryData = from r in db.T_AssetsTransferApply
                            where Arry.Contains(r.ID) && r.IsDelete == 0 && r.State != 3
                            select r;
            }
            else
            {
                queryData = from r in db.T_AssetsTransferApply
                            where r.IsDelete == 0 && r.PostUserName == name && r.State!= 3
                            select r;
            }
            //pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((page - 1) * pageSize).Take(pageSize);
            List<resultItem> list = new List<resultItem>();
            foreach (var item in queryData)
            {
                resultItem i = new resultItem();
                i.uid = item.ID;
                string str = item.Code;
                if (str == null)
                {
                    str = "";
                }
                if (str.Length >= 9)
                {
                    str = str.Substring(0, 9) + "...";
                }
                i.subTitle = "资产代码：" + str;
                i.title = "申请人：" + item.PostUserName;

                i.remark = int.Parse(item.State.ToString());
                list.Add(i);
            }
            string json = "{\"lists\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //主表CLASS 
        class mainItem
        {
            public int ID { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Owner { get; set; }
            public string Department { get; set; }
            public string Place { get; set; }
            public string Responsible { get; set; }
            public string TransferOwner { get; set; }
            public string TransferDepartment { get; set; }
            public string TransferPlace { get; set; }
            public string UserName { get; set; }
            public string TransferResponsible { get; set; }
            public string Memo { get; set; }
            public string TransferType { get; set; }
            public System.DateTime PostDate { get; set; }
            public string ReplaceCode { get; set; }
            public string CurrentApproveName { get; set; }
            public int State { get; set; }
            public int Step { get; set; }
            public int IsDelete { get; set; }
            public string PostUserName { get; set; }
            public string LastApproveName { get; set; }
        }
        //详情数据加载
        public JsonResult GetDetail(int ID, string UserName)
        {
            T_User MOD_User = db.T_User.FirstOrDefault(a => a.Nickname == UserName);
            //真名
            string name = MOD_User.Name;
            string result = "";
            //主表
            T_AssetsTransferApply mod = db.T_AssetsTransferApply.Find(ID);
            mainItem list = new mainItem();
            list.ID = mod.ID;
            list.Code = mod.Code;
            list.Name = mod.Name;
            list.Owner = mod.Owner;
            list.Department = mod.Department;
            list.Place = mod.Place;
            list.Responsible = mod.Responsible;
            list.TransferOwner = mod.TransferOwner;
            list.TransferDepartment = mod.TransferDepartment;
            list.TransferPlace = mod.TransferPlace;
            list.UserName = mod.UserName;
            list.TransferResponsible = mod.TransferResponsible;
            list.Memo = mod.Memo;
            list.TransferType = mod.TransferType;
            list.PostDate = mod.PostDate;
            list.ReplaceCode = mod.ReplaceCode;
            list.CurrentApproveName = mod.CurrentApproveName;
            list.State = mod.State;
            list.Step = mod.Step;
            list.PostUserName = mod.PostUserName;
            list.LastApproveName = mod.LastApproveName;
            list.IsDelete = mod.IsDelete;
            string modJson = JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat());

            //审核记录
            IQueryable<T_AssetsApprove> mod_Approve = db.T_AssetsApprove.Where(a => a.ApplyID == ID);
            string approve = JsonConvert.SerializeObject(mod_Approve, Lib.Comm.setTimeFormat());

            T_AssetsApprove Approve = db.T_AssetsApprove.FirstOrDefault(s => !s.ApproveDate.HasValue && s.ApplyID == ID);
            if (Approve == null)
            {
                Approve = db.T_AssetsApprove.FirstOrDefault(s => s.ApproveDate.HasValue && s.ApplyID == ID);
            }


            //用于判断是不是我审核 0不是 1是
            int myCheck = 0;
            T_AssetsApprove MyApprove = db.T_AssetsApprove.FirstOrDefault(a => a.ApplyID == ID && a.ApproveName == name && a.ApproveDate == null);
            if (MyApprove != null)
            {
                myCheck = 1;
            }
            result += "{\"Main\":[" + modJson + "],\"Approve\":" + approve + ",\"myCheck\":" + myCheck + ",\"approveId\":" + Approve.ID +",\"ID\":" + ID+ "}";
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Check(T_AssetsApprove model, string CurUser)//资产审核保存
        {
            T_AssetsApprove editApprove = db.T_AssetsApprove.Find(model.ID);
            T_AssetsTransferApply editApply = db.T_AssetsTransferApply.Find(model.ApplyID);
            T_Assets editAssets = db.T_Assets.Find(editApply.Code);
            int i =0;
            if (editApprove != null)//修改审核记录
            {
                editApprove.State = model.State;
                editApprove.Memo = model.Memo;
                editApprove.ApproveDate = DateTime.Now;
                db.Entry<T_AssetsApprove>(editApprove).State = System.Data.EntityState.Modified;
            }
            if (editApply != null)
            {
                if (model.State == 1)//同意
                {
                    #region 转移流程
                    #region 第一步和第二步审核
                    if (editApply.TransferType == "转移")//转移
                    {
                        if (editApply.Step == 1)//第一步,当前责任人同意转出后
                        {
                            string approvename = db.T_AssetsConfig.FirstOrDefault(a => a.Step == 2).Name;//添加第二步审核人(资产管理员)
                            editApply.Step = 2;
                            editApply.State = 0;//审核中
                            editApply.CurrentApproveName = approvename;
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                            T_AssetsApprove newApprove = new T_AssetsApprove();
                            newApprove.ApplyID = editApprove.ApplyID;
                            newApprove.ApproveName = approvename;
                            newApprove.Code = editApprove.Code;
                            newApprove.State = 0;
                            newApprove.Step = "2";
                            db.T_AssetsApprove.Add(newApprove);
                        }
                        else if (editApply.Step == 2)//第二步,资产管理员同意转移发生
                        {
                            string approvename = editApply.LastApproveName;//添加第三步审核人(转移后责任人)
                            editApply.Step = 3;
                            editApply.State = 0;//审核中
                            editApply.CurrentApproveName = approvename;
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;
                            if (editApply.LastApproveName.Contains(","))
                            {
                               
                                string[] approveArr = approvename.Split(new string[] { ","}, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var approve in approveArr)
                                {
                                    T_AssetsApprove newApprove = new T_AssetsApprove();
                                    newApprove.ApplyID = editApprove.ApplyID;
                                    newApprove.ApproveName = approve;
                                    newApprove.Code = editApprove.Code;
                                    newApprove.State = 0;
                                    newApprove.Step = "3";
                                    db.T_AssetsApprove.Add(newApprove);
                                }
                            }
                            else
                            {
                                T_AssetsApprove newApprove = new T_AssetsApprove();
                                newApprove.ApplyID = editApprove.ApplyID;
                                newApprove.ApproveName = approvename;
                                newApprove.Code = editApprove.Code;
                                newApprove.State = 0;
                                newApprove.Step = "3";
                                db.T_AssetsApprove.Add(newApprove);
                            }
                        }
                    #endregion
                        #region 第三步审核
                        else if (editApply.Step == 3)//第三步,接收责任人同意即确认接收到该资产,流程结束,
                        {
                            List<T_AssetsApprove> ReceivedList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3"&&a.State==1).ToList();//接收过的数据
                            List<T_AssetsApprove> ApproveList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3" && a.State == 0).ToList();//待接收的数据
                            List<T_AssetsApprove> ApproveALl = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3").ToList();
                            if (ApproveALl.Count > 1)//这条数据的接收人有多个
                            {
                                if (ApproveList.Count > 1)//还有多个人要审核
                                {
                                    editApply.CurrentApproveName = editApply.CurrentApproveName.Replace(editApprove.ApproveName, "");
                                    if (ReceivedList.Count == 0)//没有人接收过
                                    {
                                       //修改资产的使用人等信息
                                        editAssets.Department = editApply.TransferDepartment;
                                        editAssets.Owner = editAssets.Owner.Replace(editApply.PostUserName, editApprove.ApproveName);
                                        editAssets.Place = editApply.TransferPlace;
                                        editAssets.Responsible = editAssets.Responsible.Replace(editApply.PostUserName, editApprove.ApproveName);
                                        db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

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
                                        editAssets.Owner = editAssets.Owner+"," + editApprove.ApproveName;
                                        editAssets.Place = editApply.TransferPlace;
                                        editAssets.Responsible = editAssets.Responsible+"," + editApprove.ApproveName;
                                        db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

                                        T_AssetsTransferRecord EditRecord = db.T_AssetsTransferRecord.FirstOrDefault(a => a.ApplyID == editApply.ID);
                                        EditRecord.Receiver = EditRecord.Receiver + "," + editApprove.ApproveName;//第二个开始来接收的人开始则加上这个人的名字
                                        EditRecord.TransferOwner = EditRecord.TransferOwner + "," + editApprove.ApproveName;
                                        EditRecord.TransferResponsible = EditRecord.TransferResponsible + "," + editApprove.ApproveName;
                                        db.Entry<T_AssetsTransferRecord>(EditRecord).State = System.Data.EntityState.Modified;
                                    }
                                }
                                else//最后一个接收人审核
                                {
                                    editApply.Step = 9;
                                    editApply.State = 1;//已同意
                                    editApply.CurrentApproveName = "流程结束";
                                    db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                                    //修改资产的使用人等信息
                                    editAssets.Department = editApply.TransferDepartment;
                                    editAssets.Owner = editAssets.Owner+"," + editApprove.ApproveName;
                                    editAssets.Place = editApply.TransferPlace;
                                    editAssets.Responsible = editAssets.Responsible+"," + editApprove.ApproveName;
                                    db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

                                    T_AssetsTransferRecord EditRecord = db.T_AssetsTransferRecord.FirstOrDefault(a => a.ApplyID == editApply.ID);
                                    EditRecord.Receiver = EditRecord.Receiver + "," + editApprove.ApproveName;//最后来接收的人则加上这个人的名字
                                    EditRecord.TransferOwner = EditRecord.TransferOwner + "," + editApprove.ApproveName;
                                    EditRecord.TransferResponsible = EditRecord.TransferResponsible + "," + editApprove.ApproveName;
                                    db.Entry<T_AssetsTransferRecord>(EditRecord).State = System.Data.EntityState.Modified;
                                }
                            }
                            else//这条数据只有一个人接收
                            {
                                editApply.Step = 9;
                                editApply.State = 1;//已同意
                                editApply.CurrentApproveName = "流程结束";
                                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                                //修改资产的使用人等信息
                                editAssets.Department = editApply.TransferDepartment;
                                editAssets.Owner = editAssets.Owner.Replace(editApply.PostUserName, editApply.LastApproveName);
                                editAssets.Place = editApply.TransferPlace;
                                editAssets.Responsible = editApply.TransferResponsible;
                                db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

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
                            IQueryable<T_AssetsTransferApply> TransferApplyList = db.T_AssetsTransferApply.Where(a => (a.State == -1 || a.State == 0) && a.Code == editApply.Code&&a.ID!=editApply.ID);
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
                    }
                    #endregion
                    #region 上交
                    else if (editApply.TransferType == "上交" )
                    {
                        if (editApply.Step == 1)//第一步,资产管理员确认资产已上交,流程结束,添加一条资产变更记录
                        {
                            editApply.Step = 9;
                            editApply.State = 1;//已同意
                            editApply.CurrentApproveName = "流程结束";
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                            //修改资产的使用人等信息
                            editAssets.Department = editApply.TransferDepartment; ;
                            editAssets.Owner = editApply.TransferOwner;
                            editAssets.Place = editApply.TransferPlace;
                            editAssets.Responsible = editApply.TransferResponsible;//资产管理员负责待分配的资产
                            db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

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
                            TransferRecord.Receiver = "";
                            TransferRecord.ApplyID = editApply.ID;
                            TransferRecord.TransferType = editApply.TransferType;
                            db.T_AssetsTransferRecord.Add(TransferRecord);

                            IQueryable<T_AssetsTransferApply> TransferApplyList = db.T_AssetsTransferApply.Where(a =>(a.State == -1 || a.State == 0) && a.Code == editApply.Code);
                            if (TransferApplyList.Count() > 0)
                            {
                                foreach (T_AssetsTransferApply item in TransferApplyList)
                                {
                                    int Oindex = item.TransferOwner.IndexOf(editApply.PostUserName);
                                    if (Oindex == 0)//如果名字在第一个,则删去名字及名字后面的逗号,否则删去名字及名字前面的一个逗号
                                    {
                                        item.TransferOwner = item.TransferOwner.Replace(editApply.PostUserName+",", "");
                                    }
                                    else
                                    {
                                        item.TransferOwner = item.TransferOwner.Replace("," + editApply.PostUserName, "");
                                    }
                                    int Rindex = item.TransferResponsible.IndexOf(editApply.PostUserName);
                                    if (Rindex == 0)
                                    {
                                        item.TransferResponsible = item.TransferResponsible.Replace(editApply.PostUserName + ",", "");
                                    }
                                    else
                                    {
                                        item.TransferResponsible = item.TransferResponsible.Replace("," + editApply.PostUserName, "");
                                    }
                                   
                                }
                            }
                        }
                    }
                    #endregion
                    #region 领用
                    else if (editApply.TransferType == "领用")
                    {
                        if (editApply.Step == 1)//第一步,资产管理员确认资产已上交,流程结束,添加一条资产变更记录
                        {
                            editApply.Step = 9;
                            editApply.State = 1;//已同意
                            editApply.CurrentApproveName = "流程结束";
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                           
                            editAssets.Department = editApply.TransferDepartment; ;
                            editAssets.Owner = editApply.TransferOwner;
                            editAssets.Place = editApply.TransferPlace;
                            editAssets.Responsible = editApply.TransferResponsible;//资产管理员负责待分配的资产
                            db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

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
                            TransferRecord.Receiver = TransferRecord.TransferOwner;
                            TransferRecord.ApplyID = editApply.ID;
                            TransferRecord.TransferType = editApply.TransferType;
                            db.T_AssetsTransferRecord.Add(TransferRecord);
                        }
                    }
                    #endregion
                    #region 报废
                    else if (editApply.TransferType == "报废")
                    {
                        if (editApply.Step == 1)//第一步,资产管理员确认资产报废,流程结束,添加一条资产变更记录
                        {
                            editApply.Step = 9;
                            editApply.State = 1;//已同意
                            editApply.CurrentApproveName = "流程结束";
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;

                           
                            editAssets.Department = "无";
                            editAssets.Owner = "报废";
                            editAssets.Place = "无";
                            editAssets.Responsible = "无";
                            editAssets.isScrap = "1";
                            db.Entry<T_Assets>(editAssets).State = System.Data.EntityState.Modified;

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
                            TransferRecord.Receiver = "";
                            TransferRecord.ApplyID = editApply.ID;
                            TransferRecord.TransferType = editApply.TransferType;
                            db.T_AssetsTransferRecord.Add(TransferRecord);
                        }
                    }
                    #endregion
                }
              
                else//不同意
                {
                    #region 转移
                    if (editApply.TransferType == "转移" && editApply.Step == 3)//转移的第三步有可能有多个人接收就会有多个人审核
                    {
                        List<T_AssetsApprove> ReceivedList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3" && a.State == 1).ToList();//接收过的数据
                        List<T_AssetsApprove> ApproveList = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3" && a.State == 0).ToList();//待接收的数据
                        List<T_AssetsApprove> ApproveALl = db.T_AssetsApprove.Where(a => a.ApplyID == model.ApplyID && a.Step == "3").ToList();
                        if (ApproveALl.Count > 1)//这条数据的接收人有多个
                        {
                            if (ApproveList.Count > 1)//还有多个人要审核
                            {
                                editApply.CurrentApproveName = editApply.CurrentApproveName.Replace(editApprove.ApproveName, "");
                                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;
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
                                    editApply.State =2;//没有任何一个人接收则为不同意
                                }
                                editApply.CurrentApproveName = "流程结束";
                                db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;
                            }
                        }
                        else//一人接收
                        {
                            editApply.Step = 9;
                            editApply.State = 2;//不同意
                            editApply.CurrentApproveName = "流程结束";
                            db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;
                        }
                    }
                    #endregion
                    #region 其他
                    else
                    {
                        editApply.Step = 9;
                        editApply.State = 2;//不同意
                        editApply.CurrentApproveName = "流程结束";
                        db.Entry<T_AssetsTransferApply>(editApply).State = System.Data.EntityState.Modified;
                    }
                    #endregion
                }
            }
            i = db.SaveChanges();
            try
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
                string RetreatAppRoveSql = " select ApproveName as PendingAuditName,COUNT(*) as NotauditedNumber from T_AssetsApprove where  ApplyID in ( select ID from T_AssetsTransferApply where isDelete=0 ) and  State=0 and ApproveDate is null GROUP BY ApproveName  ";
                List<Modular> RetreatAppRoveQuery = db.Database.SqlQuery<Modular>(RetreatAppRoveSql).ToList();
                string Nickname = CurUser;
                for (int e = 0; e < RetreatAppRoveQuery.Count; e++)
                {
                    string PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;

                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资产变更" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;

                    }
                    else
                    {
                        T_ModularNotaudited ModularNotauditedModel = new T_ModularNotaudited();
                        ModularNotauditedModel.ModularName = "资产变更";
                        ModularNotauditedModel.RejectNumber = 0;
                        ModularNotauditedModel.NotauditedNumber = RetreatAppRoveQuery[e].NotauditedNumber;
                        ModularNotauditedModel.PendingAuditName = RetreatAppRoveQuery[e].PendingAuditName;
                        ModularNotauditedModel.ToupdateDate = DateTime.Now; ModularNotauditedModel.ToupdateName = Nickname;
                        db.T_ModularNotaudited.Add(ModularNotauditedModel);
                    }
                    db.SaveChanges();
                }

                //增加驳回数据
                string RejectNumberSql = " select PostUserName as PendingAuditName,COUNT(*) as  NotauditedNumber  from T_AssetsTransferApply where State='2'  GROUP BY PostUserName ";
                List<Modular> RejectNumberQuery = db.Database.SqlQuery<Modular>(RejectNumberSql).ToList();

                for (int e = 0; e < RejectNumberQuery.Count; e++)
                {
                    string PendingAuditName = RejectNumberQuery[e].PendingAuditName;

                    T_ModularNotaudited NotauditedModel = db.T_ModularNotaudited.SingleOrDefault(a => a.ModularName == "资产变更" && a.PendingAuditName == PendingAuditName);
                    if (NotauditedModel != null)
                    {
                        NotauditedModel.RejectNumber = RejectNumberQuery[e].NotauditedNumber;
                        db.Entry<T_ModularNotaudited>(NotauditedModel).State = System.Data.EntityState.Modified;
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
            catch (DbUpdateException e)
            { 

            }
            return Json(i);
        }
       
        public partial class Modular
        {

            public string ModularName { get; set; }
            public int NotauditedNumber { get; set; }
            public string PendingAuditName { get; set; }
        }
    }
}
