using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.EnterpriseServices;
using EBMS.App_Code;
using System.Transactions;
using controll= EBMS.Controllers;
namespace EBMS.Controllers
{
    /// <summary>
    /// 人事
    /// </summary>
    public class PersonnelController : BaseController
    {
        //
        // GET: /Personnel/
        EBMSEntities db = new EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }
        #region 其他方法或类
        public class PersonnelDetail
        {
            public T_PersonnelFile perInfo { get; set; }
            public List<T_PersonnelEduBackgroud> EduBackgroud { get; set; }
            public List<T_PersonnelFamily> Family { get; set; }
            public List<T_PersonnelPerformance> Performance { get; set; }
            public List<T_PersonnelQuit> Quit { get; set; }
            public List<T_PersonnelReward> Reward { get; set; }
            public List<T_PersonnelTransfer> Transfer { get; set; }
            public List<T_PersonnelWorkExperience> WorkExperience { get; set; }
        }
        public List<SelectListItem> DateType()
        {
            List<SelectListItem> DateType = new List<SelectListItem> {
            new SelectListItem { Text = "==请选择阴历或阳历==", Value = "" },
            new SelectListItem { Text = "阴历", Value = "阴历" },
            new SelectListItem { Text = "阳历", Value = "阳历" },
            };
            return DateType;
        }
        public List<SelectListItem> ConfigList(string type)
        {
            var list = db.T_EmployDemandConfig.Where(a => a.Type == type);
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> MarryStateList()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "已婚", Value = "已婚" },
            new SelectListItem { Text = "未婚", Value = "未婚" },
            };
            return MarryStateList;
        }
        //部门结构 树的数据
        public string DepartmentTree(int pid = -1)
        {
            string resultStr = string.Empty;

            List<T_Department> queryData = db.T_Department.Where(a => a.parentId == pid).ToList();
            if (queryData.Count > 0)
            {
                resultStr += "[";
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\",\"text\":\"{1}\"", oid, name);
                    resultStr += getDepartmentStr(int.Parse(oid.ToString()));
                    resultStr += "}";
                }
                resultStr += string.Format("]");
            }
            return resultStr;
        }
        //部门结构 树的子节点数据
        public string getDepartmentStr(int id)
        {
            string resultStr = "";
            List<T_Department> queryData = db.T_Department.Where(a => a.parentId == id).ToList();
            if (queryData.Count > 0)
            {
                resultStr += string.Format(",children: [");
                for (int i = 0; i < queryData.Count; i++)
                {
                    int oid = queryData[i].ID;
                    string name = queryData[i].Name;
                    resultStr += "{";
                    resultStr += string.Format("\"id\": \"{0}\", \"text\": \"{1}\"", oid, name);
                    resultStr += getDepartmentStr(oid);
                    resultStr += "}";
                    if (queryData.Count - 1 == i)
                    {
                        resultStr += string.Format("]");
                    }
                    else
                    {
                        resultStr += ",";
                    }
                }
            }
            return resultStr;
        }

        public string getbool(string val)
        {
            if (val == "1")
            {
                return "是";
            }
            else
            {
                return "否";
            }
        }
        #endregion
        #region  View
        [Description("人事档案新增")]
        public ActionResult ViewAdd()//添加人事档案
        {
            List<SelectListItem> sex = new List<SelectListItem>();

            sex.Add(new SelectListItem { Text = "女", Value = "女" });
            sex.Add(new SelectListItem { Text = "男", Value = "男" });

            ViewData["SexList"] = sex;
           
           
            List<SelectListItem> zichan = new List<SelectListItem>();

            zichan.Add(new SelectListItem { Text = "显示器", Value = "显示器" });
            zichan.Add(new SelectListItem { Text = "主机", Value = "主机" });
            zichan.Add(new SelectListItem { Text = "键盘", Value = "键盘" });
            zichan.Add(new SelectListItem { Text = "鼠标", Value = "鼠标" });
            zichan.Add(new SelectListItem { Text = "办公椅", Value = "办公椅" });
            zichan.Add(new SelectListItem { Text = "立柜", Value = "立柜" });

            ViewBag.zichan = zichan;

            List<SelectListItem> entry_data = new List<SelectListItem>();

            entry_data.Add(new SelectListItem { Text = "录用审批表", Value = "录用审批表" });
            entry_data.Add(new SelectListItem { Text = "身份证复印件", Value = "身份证复印件" });
            entry_data.Add(new SelectListItem { Text = "驾驶证", Value = "驾驶证" });
            entry_data.Add(new SelectListItem { Text = "入职登记表", Value = "入职登记表" });
            entry_data.Add(new SelectListItem { Text = "学历证书复印件", Value = "学历证书复印件" });
            entry_data.Add(new SelectListItem { Text = "工牌", Value = "工牌" });
            entry_data.Add(new SelectListItem { Text = "应聘申请表", Value = "应聘申请表" });
            entry_data.Add(new SelectListItem { Text = "体检报告", Value = "体检报告" });
            entry_data.Add(new SelectListItem { Text = "工服", Value = "工服" });

            ViewBag.entry_data = entry_data;
            ViewData["MarryStateList"] = MarryStateList();
            ViewData["BoolList"] = Com.BoolList; 
            ViewData["JobList"] = ConfigList("岗位");
            ViewData["DateType"] =DateType();
            return View();
        }
        public ActionResult ViewUploadPic()  //上传寸照
        {

            return View();
        }
        [Description("人事档案列表")]
        public ActionResult ViewList()//人事列表
        {
            string treeData = DepartmentTree(-1);
            ViewData["tree"] = treeData;
            return View();
        }
        public ActionResult ViewDetail(int ID=0)//人事详情
        {
           

            T_PersonnelFile model = db.T_PersonnelFile.Find(ID);
            model.CanBusinessTravel = getbool(model.CanBusinessTravel);
            if (model == null)
            {
                return HttpNotFound();
            }
            else
            {
                List<T_PersonnelEduBackgroud> EduBackgroud=db.T_PersonnelEduBackgroud.Where(a=>a.PID==ID).ToList();
                  List<T_PersonnelFamily> Family=db.T_PersonnelFamily.Where(a=>a.Pid==ID).ToList();
                  List<T_PersonnelPerformance> Performance=db.T_PersonnelPerformance.Where(a=>a.Pid==ID).ToList();
                  List<T_PersonnelQuit> Quit=db.T_PersonnelQuit.Where(a=>a.Pid==ID).ToList();
                  List<T_PersonnelReward> Reward=db.T_PersonnelReward.Where(a=>a.Pid==ID).ToList();
                  List<T_PersonnelTransfer> Transfer=db.T_PersonnelTransfer.Where(a=>a.Pid==ID).ToList();
                  List<T_PersonnelWorkExperience> WorkExperience=db.T_PersonnelWorkExperience.Where(a=>a.PID==ID).ToList();
                PersonnelDetail perDetail=new PersonnelDetail(); 
                perDetail.perInfo = model;
                perDetail.EduBackgroud = EduBackgroud;
                perDetail.Family = Family;
                perDetail.Performance=Performance;
                perDetail.Quit=Quit;
                perDetail.Reward=Reward;
                perDetail.Transfer=Transfer;
                perDetail.WorkExperience = WorkExperience;
                return View(perDetail);
            }
        }
        [Description("人事档案管理")]
        public ActionResult ViewManagement()//人事管理
        {
            string treeData = DepartmentTree(-1);
            ViewData["tree"] = treeData;
            return View();
        }
        public ActionResult ViewEdit(int ID)//编辑
        {
            T_PersonnelFile model = db.T_PersonnelFile.Find(ID);
            if (model == null)
            {
                return HttpNotFound();
            }
            else
            {
                List<SelectListItem> onjob = new List<SelectListItem>();

                onjob.Add(new SelectListItem { Text = "在职", Value = "0" });
                onjob.Add(new SelectListItem { Text = "离职", Value = "1" });
                onjob.Add(new SelectListItem { Text = "已调线下", Value = "2" });
             
                 ViewData["onjobList"] = onjob;


                List<SelectListItem> sex = new List<SelectListItem>();

                sex.Add(new SelectListItem { Text = "女", Value = "女" });
                sex.Add(new SelectListItem { Text = "男", Value = "男" });
               
                ViewData["sexList"] = sex;
                //sex.Add(new SelectListItem { Text = "已收货", Value = "3" });
                List<SelectListItem> marry = new List<SelectListItem>();

                marry.Add(new SelectListItem { Text = "否", Value = "否" });
                marry.Add(new SelectListItem { Text = "是", Value = "是" });
                ViewData["marry"] = marry;
                List<SelectListItem> zichan = new List<SelectListItem>();

                zichan.Add(new SelectListItem { Text = "显示器", Value = "显示器" });
                zichan.Add(new SelectListItem { Text = "主机", Value = "主机" });
                zichan.Add(new SelectListItem { Text = "键盘", Value = "键盘" });
                zichan.Add(new SelectListItem { Text = "鼠标", Value = "鼠标" });
                zichan.Add(new SelectListItem { Text = "办公椅", Value = "办公椅" });
                zichan.Add(new SelectListItem { Text = "立柜", Value = "立柜" });

                ViewBag.zichan = zichan;

                List<SelectListItem> entry_data = new List<SelectListItem>();

                entry_data.Add(new SelectListItem { Text = "录用审批表", Value = "录用审批表" });
                entry_data.Add(new SelectListItem { Text = "身份证复印件", Value = "身份证复印件" });
                entry_data.Add(new SelectListItem { Text = "驾驶证", Value = "驾驶证" });
                entry_data.Add(new SelectListItem { Text = "入职登记表", Value = "入职登记表" });
                entry_data.Add(new SelectListItem { Text = "学历证书复印件", Value = "学历证书复印件" });
                entry_data.Add(new SelectListItem { Text = "工牌", Value = "工牌" });
                entry_data.Add(new SelectListItem { Text = "应聘申请表", Value = "应聘申请表" });
                entry_data.Add(new SelectListItem { Text = "体检报告", Value = "体检报告" });
                entry_data.Add(new SelectListItem { Text = "工服", Value = "工服" });

                ViewBag.entry_data = entry_data;
                return View(model);
            }
        }
        public ActionResult ViewAddOtherInfo(int ID, int flag)//添加其它信息
        {
            ViewData["id"]=ID;
            ViewBag.flag = flag;
            T_PersonnelFile person = db.T_PersonnelFile.Find(ID);
            if (person == null)
            { 
                return HttpNotFound(); 
            }
            personelModel model = new personelModel();
            model.person = person;
            if (flag == 4)
            {
                List<SelectListItem> quit_data = new List<SelectListItem>();

                quit_data.Add(new SelectListItem { Text = "离职申请表", Value = "离职申请表" });
                quit_data.Add(new SelectListItem { Text = "离职交接表", Value = "离职交接表" });
                ViewBag.quit_data = quit_data;

            }
            if (flag == 5)
            {
                List<SelectListItem> tran_type = new List<SelectListItem>();

                tran_type.Add(new SelectListItem { Text = "线上调岗", Value = "线上调岗" });
                tran_type.Add(new SelectListItem { Text = "调线下", Value = "调线下" });
                ViewData["tran_type"] = tran_type;
            }
            
            List<T_PersonnelFamily> fam = db.T_PersonnelFamily.Where(a => a.Pid == ID).ToList();
            List<T_PersonnelReward> rew = db.T_PersonnelReward.Where(a => a.Pid == ID).ToList();
            List<T_PersonnelPerformance> performance = db.T_PersonnelPerformance.Where(a => a.Pid == ID).ToList();
            List<T_PersonnelQuit> quit = db.T_PersonnelQuit.Where(a => a.Pid == ID).ToList();
            List<T_PersonnelTransfer> tran = db.T_PersonnelTransfer.Where(a => a.Pid == ID).ToList();
          
            if (fam.Count != 0)
            {
                ViewBag.family = fam;
            }
            else { ViewBag.family = null; }
            if (rew.Count != 0)
            {
                ViewBag.rew = rew;
            }
            else { ViewBag.rew = null; }
            if (performance.Count != 0)
            {
                ViewBag.performance = performance;
            }
            else { ViewBag.performance = null; }
            if (quit.Count != 0)
            {
                ViewBag.quit = quit;
            }
            else { ViewBag.quit = null; }
            if (tran.Count != 0)
            {
                ViewBag.tran = tran;
            }
            else { ViewBag.tran = null; }
            return View(model);
        }
        public ActionResult ViewEditOtherInfo(int id, string flag)//编辑其它信息
        {
            ViewData["id"] = id;
            ViewBag.flag = flag;
            if (flag == "4")
            {
                List<SelectListItem> quit_data = new List<SelectListItem>();

                quit_data.Add(new SelectListItem { Text = "离职申请表", Value = "离职申请表" });
                quit_data.Add(new SelectListItem { Text = "离职交接表", Value = "离职交接表" });
                ViewBag.quit_data = quit_data;

            }
            if (flag == "5")
            {
                List<SelectListItem> tran_type = new List<SelectListItem>();

                tran_type.Add(new SelectListItem { Text = "线上调岗", Value = "线上调岗" });
                tran_type.Add(new SelectListItem { Text = "调线下", Value = "调线下" });
                ViewData["tran_type"] = tran_type;
            }
            personelModel viewModel = new personelModel();
            //personnel model = db.personnel.SingleOrDefault(a => a.id == id);
            T_PersonnelFamily fam = db.T_PersonnelFamily.SingleOrDefault(a => a.ID == id);
            T_PersonnelReward rew = db.T_PersonnelReward.SingleOrDefault(a => a.ID == id);
            T_PersonnelPerformance performance = db.T_PersonnelPerformance.SingleOrDefault(a => a.ID == id);
            T_PersonnelQuit quit = db.T_PersonnelQuit.SingleOrDefault(a => a.ID == id);
            T_PersonnelTransfer tran = db.T_PersonnelTransfer.SingleOrDefault(a => a.ID == id);
            if (fam != null)
            {
                viewModel.family = fam;
            }
            else { viewModel.family = null; }
            if (rew != null)
            {
                viewModel.reward = rew;
            }
            else { viewModel.reward = null; }
            if (performance != null)
            {
                viewModel.performance = performance;
            }
            else { viewModel.performance = null; }
            //if (quit != null)
            //{
            //    viewModel.quit = quit;
            //}
            //else { viewModel.quit = null; }
            //if (tran != null)
            //{
            //    viewModel.trans = tran;
            //}
            //else { viewModel.trans = null; }
            //if (model.marital_status == "1") { model.marital_status = "是"; }
            //else { model.marital_status = "否"; }
            //if (model.sex == "1") { model.marital_status = "男"; }
            //else { model.sex = "女"; }

            //viewModel.person = model;

            return View(viewModel);
           
        }
        public ActionResult ViewImportExcel()
        {
            return View();
        }
        #endregion
        #region save
        [HttpPost]
        public JsonResult ViewAddSave(EBMS.Controllers.RecruitController.EmploymentRegistration model)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {

                    T_PersonnelFile PerFile = model.perInfo;
                    T_User user = db.T_User.FirstOrDefault(a => a.Nickname == PerFile.NickName);

                    PerFile.oid = user.ID;
                    PerFile.OnJob = 0;
                    PerFile.IsDelete = 0;
                    PerFile.CurrentInterviewer = "";
                    PerFile.InterviewStep = 99;
                    PerFile.isZhuanzheng = 0;
                    PerFile.InterviewState = 1;
                    PerFile.IsDelete = 0;
                    PerFile.online = "电子商务部";
                    db.T_PersonnelFile.Add(PerFile);
                    db.SaveChanges();
                   
                   
                    foreach (var EduBackgroud in model.EduBackgroud)
                    {
                        if (!string.IsNullOrWhiteSpace(EduBackgroud.School))
                        {
                            EduBackgroud.PID = PerFile.ID;
                            db.T_PersonnelEduBackgroud.Add(EduBackgroud);
                        }
                    }
                    foreach (var WorkExperience in model.WorkExperience)
                    {
                        if (!string.IsNullOrWhiteSpace(WorkExperience.Job))
                        {
                            WorkExperience.PID = PerFile.ID;
                            db.T_PersonnelWorkExperience.Add(WorkExperience);
                        }
                    }
                    foreach (var Family in model.Family)
                    {
                        if (!string.IsNullOrWhiteSpace(Family.Name))
                        {
                            Family.Pid = PerFile.ID;
                            db.T_PersonnelFamily.Add(Family);
                        }
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
        }//
        [HttpPost]
        public JsonResult ViewEditSave(T_PersonnelFile model)
        {
            T_PersonnelFile editModel = db.T_PersonnelFile.Find(model.ID);
            if (editModel == null)
            {
                return Json(-1);
            }
            else
            {
                editModel.Pic = model.Pic;
                editModel.OnJob = model.OnJob;
                editModel.NickName = model.NickName;
                editModel.TrueName = model.TrueName;
                editModel.Sex = model.Sex;
                editModel.MaritalStatus = model.MaritalStatus;
                editModel.Code = model.Code;
                editModel.IDnum = model.IDnum;
                editModel.Tel = model.Tel;
                editModel.Department = model.Department;
                editModel.Job = model.Job;
                editModel.Edu = model.Edu;
                editModel.School = model.School;
                editModel.Profession = model.Profession;
                editModel.CET = model.CET;
                editModel.NCRE = model.NCRE;
                editModel.OtherCertificate = model.OtherCertificate;
                editModel.Nation = model.Nation;
                editModel.NativePlace = model.NativePlace;
                editModel.PoliticsStatus = model.PoliticsStatus;
                editModel.HouseholdRegister = model.HouseholdRegister;
                editModel.PresentAddress = model.PresentAddress;
                editModel.EmergencyContactName = model.EmergencyContactName;
                editModel.EmergencyContactTel = model.EmergencyContactTel;
                editModel.DormitoryNum = model.DormitoryNum;
                editModel.HireMaster = model.HireMaster;
                editModel.Hiredate = model.Hiredate;
                editModel.zhuanzheng_date = model.zhuanzheng_date;
                editModel.ContractFirstStartTime = model.ContractFirstStartTime;
                editModel.ContractFirstDeadline = model.ContractFirstDeadline;
                editModel.ContractSecondDeadline = model.ContractSecondDeadline;
                editModel.ContractThirdDeadline = model.ContractThirdDeadline;
                editModel.PaySSDate = model.PaySSDate;
                editModel.CupboardNun = model.CupboardNun;
                editModel.WorkNum = model.WorkNum;
                editModel.EntryData = model.EntryData;
                editModel.FixedAssets = model.FixedAssets;
                editModel.HealthState = model.HealthState;
                editModel.Character = model.Character;
                editModel.CanEntryTime = model.CanEntryTime;
                editModel.ExpectedSalary = model.ExpectedSalary;
                editModel.SignCompany = model.SignCompany;
                db.Entry<T_PersonnelFile>(editModel).State = System.Data.Entity.EntityState.Modified;
                try {
                    int i = db.SaveChanges();
                    return Json(i);
                }
                catch (Exception e)
                {
                    throw e;

                }
                
            }
           
        }
         [HttpPost]
        public JsonResult DeletePersonnel(int ID)//删除
        {
            T_PersonnelFile delModel = db.T_PersonnelFile.Find(ID);
            if (delModel == null)
            {
                return Json(0);
            }
            else
            {
                delModel.IsDelete = 1;
                db.Entry<T_PersonnelFile>(delModel).State = System.Data.Entity.EntityState.Modified;
                int i = db.SaveChanges();
                return Json(i);
            }
        }
        [HttpPost]
         public JsonResult AddOtherInfo(personelModel model, int id, string flag)//添加其它信息
         {
             if (flag == "1")
             {
                 T_PersonnelFamily Addmodel = new T_PersonnelFamily();
                 Addmodel.Name = model.family.Name;
                 Addmodel.Pid = id;
                 Addmodel.Job = model.family.Job;
                 Addmodel.Relation = model.family.Relation;
                 Addmodel.WorkUnit = model.family.WorkUnit;
                 db.T_PersonnelFamily.Add(Addmodel);
             }
             else if (flag == "2")
             {
                 T_PersonnelReward Addmodel = model.reward;
                 Addmodel.Pid = id;
                 DateTime t = (DateTime)model.reward.RewardPunishDate;
                 Addmodel.RewardPunishDate = t;
                 db.T_PersonnelReward.Add(Addmodel);
             }
             else if (flag == "3")
             {

                 T_PersonnelPerformance Addmodel = model.performance;
                 Addmodel.Pid = id;
                 db.T_PersonnelPerformance.Add(Addmodel);
             }
             //else if (flag == "4")
             //{
             //    T_PersonnelFile Editmodel = db.T_PersonnelFile.SingleOrDefault(a => a.ID == id);
             //    Editmodel.OnJob = 1;
             //    T_PersonnelQuit Addmodel = model.quit;
             //    Addmodel.Pid = id;
             //    db.T_PersonnelQuit.Add(Addmodel);
             //}
             //else if (flag == "5")
             //{
             //    T_PersonnelFile Editmodel = db.T_PersonnelFile.SingleOrDefault(a => a.ID == id);

             //    T_PersonnelTransfer Addmodel = model.trans;
             //    if (model.trans.Type == "调线下")
             //    {
             //        Editmodel.OnJob = 2;
             //    }
             //    if (model.trans.Type == "线上调岗")
             //    {
             //        Editmodel.OnJob = 3;
             //    }
             //    Addmodel.Pid = id;
             //    db.T_PersonnelTransfer.Add(Addmodel);
             //}
             else if (flag == "6")
             {
                 T_PersonnelWorkExperience Addmodel = new T_PersonnelWorkExperience();
                 Addmodel.Company = model.experience.Company;
                 Addmodel.PID = id;
                 Addmodel.Job = model.experience.Job;
                 Addmodel.QuitReason = model.experience.QuitReason;
                 Addmodel.StartFinishTime = model.experience.StartFinishTime;
                 Addmodel.Winter = model.experience.Winter;
                 Addmodel.WinterJob = model.experience.WinterJob;
                 Addmodel.WinterTel = model.experience.WinterTel;
                 db.T_PersonnelWorkExperience.Add(Addmodel);
             }
             else if (flag == "7")
             {
                 T_PersonnelEduBackgroud Addmodel = new T_PersonnelEduBackgroud();
                 Addmodel.School = model.EduBackgroud.School;
                 Addmodel.PID = id;
                 Addmodel.Specialty = model.EduBackgroud.Specialty;
                 Addmodel.StartFinishTime = model.EduBackgroud.StartFinishTime;
                 db.T_PersonnelEduBackgroud.Add(Addmodel);
             }
             int i = db.SaveChanges();
             return Json(i, JsonRequestBehavior.AllowGet);
         }
        [HttpPost]
         public JsonResult EditOtherInfo(personelModel model, int id, string flag)//编辑其它信息
         {
             if (flag == "1")
             {
                 T_PersonnelFamily Editmodel = db.T_PersonnelFamily.SingleOrDefault(a => a.ID == id);
                 Editmodel.Name = model.family.Name;

                 Editmodel.Job = model.family.Job;
                 Editmodel.Relation = model.family.Relation;
                 Editmodel.WorkUnit = model.family.WorkUnit;

                 db.Entry<T_PersonnelFamily>(Editmodel).State = System.Data.Entity.EntityState.Modified;
             }
             else if (flag == "2")
             {
                 T_PersonnelReward Editmodel = db.T_PersonnelReward.SingleOrDefault(a => a.ID == id);
                 Editmodel.RewardPunishiDetails = model.reward.RewardPunishiDetails;
                 Editmodel.RewardPunishDate = model.reward.RewardPunishDate;

                 db.Entry<T_PersonnelReward>(Editmodel).State = System.Data.Entity.EntityState.Modified;
             }
             else if (flag == "3")
             {
                 T_PersonnelPerformance Editmodel = db.T_PersonnelPerformance.SingleOrDefault(a => a.ID == id);
                 Editmodel.PerformanceDate = model.performance.PerformanceDate;
                 Editmodel.PerformanceDetails = model.performance.PerformanceDetails;

                 db.Entry<T_PersonnelPerformance>(Editmodel).State = System.Data.Entity.EntityState.Modified;
             }
             //else if (flag == "4")
             //{
             //    T_PersonnelQuit Editmodel = db.T_PersonnelQuit.SingleOrDefault(a => a.ID == id);
             //    Editmodel.QuitData = model.quit.QuitData;
             //    Editmodel.QuitDate = model.quit.QuitDate;

             //    db.Entry<T_PersonnelQuit>(Editmodel).State = System.Data.Entity.EntityState.Modified;
             //}
             //else if (flag == "5")
             //{
             //    T_PersonnelTransfer Editmodel = db.T_PersonnelTransfer.SingleOrDefault(a => a.ID == id);
             //    Editmodel.Type = model.trans.Type;
             //    Editmodel.TransferDate = model.trans.TransferDate;
             //  //  Editmodel.TransferDetails = model.trans.TransferDetails;

             //    db.Entry<T_PersonnelTransfer>(Editmodel).State = System.Data.Entity.EntityState.Modified;
             //}
             int i = db.SaveChanges();
             return Json(i, JsonRequestBehavior.AllowGet);
         }
        public JsonResult DeleteOtherInfo(int ID, string flag)//删除其他信息
        {
            if (flag == "1")
            {
                T_PersonnelFamily delmodel = db.T_PersonnelFamily.SingleOrDefault(a => a.ID == ID);
                db.T_PersonnelFamily.Remove(delmodel);

            }
            else if (flag == "2")
            {
                T_PersonnelReward delmodel = db.T_PersonnelReward.SingleOrDefault(a => a.ID == ID);
                db.T_PersonnelReward.Remove(delmodel);
            }
            else if (flag == "3")
            {
                T_PersonnelPerformance delmodel = db.T_PersonnelPerformance.SingleOrDefault(a => a.ID == ID);
                db.T_PersonnelPerformance.Remove(delmodel);
            }
            else if (flag == "4")
            {
                T_PersonnelQuit delmodel = db.T_PersonnelQuit.SingleOrDefault(a => a.ID == ID);
                db.T_PersonnelQuit.Remove(delmodel);
            }
            else if (flag == "5")
            {
                T_PersonnelTransfer delmodel = db.T_PersonnelTransfer.SingleOrDefault(a => a.ID == ID);
                db.T_PersonnelTransfer.Remove(delmodel);
            }
            int i = db.SaveChanges();
            return Json(i, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region queryData
        public ContentResult GetList(Lib.GridPager pager, string queryStr, int onJob, int dId=0)
        {
            IQueryable<T_PersonnelFile> queryData = db.T_PersonnelFile.Where(a=>a.IsDelete==0);
            T_Department departmentModel = db.T_Department.Find(dId);
            if (departmentModel == null)
            {
                return Content("{\"total\":\"0 \",\"rows\":\"0\"");
            }

            if (departmentModel.employees != null && departmentModel.employees != "")
            {
                string[] Employees = departmentModel.employees.Split(',');
                int[] EmployeesArry = new int[Employees.Length];
                for (int i = 0; i < Employees.Length; i++)
                {
                    EmployeesArry[i] = Convert.ToInt32(Employees[i]);
                }
                queryData = from r in queryData
                            where EmployeesArry.Contains(r.oid)
                            select r;
            }
            else
            {
                return Content("{\"total\":\"0 \",\"rows\":\"0\"");
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.NickName != null && a.NickName.Contains(queryStr)) || (a.TrueName != null && a.TrueName.Contains(queryStr)));
            }
            if (onJob!=4)
            {
                queryData = queryData.Where(a =>a.OnJob==onJob);
            }
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                //分页
                //queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
                List<T_PersonnelFile> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
               
                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
                return Content(json);
            }
            else
            {
                return Content("{\"total\":\"0 \",\"rows\":\"0\"");
            }
        }
        #endregion
        #region 导入导出excel
        public string ImportExcel()//导入excel
        {
            using (TransactionScope sc = new TransactionScope())
            {
                AboutExcel AE = new AboutExcel();
                // string name = Server.UrlDecode(Request.Cookies["Account"].Value);
                string message = "导入成功";//给用户的提示消息
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase postFile = Request.Files[file];//get post file 

                    if (postFile.ContentLength == 0)
                    {
                        return " <script > alert('文件为空，请重新选择');window.history.go(-1);  </script>";
                    }
                    else
                    {
                        string noAccount = "";//记录没有账户的人

                        string newFilePath = Server.MapPath("~/Upload/PersonnelExcel/");//save path 
                        string fileName = Path.GetFileName(postFile.FileName);
                        postFile.SaveAs(newFilePath + fileName);//save file 
                        DataTable dt = AE.ImportExcelFile(newFilePath + fileName);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string name = dt.Rows[i][28].ToString();
                            IQueryable<T_User> users = db.T_User.Where(a => a.Nickname == name);
                            int z = users.Count();
                            if (z > 0)//检查该用户是否已经存在账号，如果没有添加账号
                            {
                                T_User user = users.First();
                                if (dt.Rows[i][2].ToString() != "")//如果有名字的话就进行插入
                                {

                                    string ID_num = dt.Rows[i][9].ToString().Trim();
                                    T_PersonnelFile cc = db.T_PersonnelFile.FirstOrDefault(a => a.IDnum == ID_num && ID_num != "");  //根据身份证号查重,如果无重复就做新增，重复的就不做任何处理
                                    if (cc == null)
                                    {
                                        T_PersonnelFile model = new T_PersonnelFile();
                                        model.InterviewStep = 99;
                                        model.oid = user.ID;
                                        model.IsDelete = 0;
                                        model.OnJob = 0;
                                        model.Code = dt.Rows[i][1].ToString();
                                        model.TrueName = dt.Rows[i][2].ToString().Trim();

                                        model.Sex = dt.Rows[i][3].ToString();
                                        model.online = dt.Rows[i][4].ToString();
                                        model.Department = dt.Rows[i][5].ToString();
                                        model.Job = dt.Rows[i][6].ToString();
                                        model.Tel = dt.Rows[i][7].ToString();
                                        try
                                        {
                                            if (dt.Rows[i][8] != null && dt.Rows[i][8].ToString().Trim() != "")
                                            {
                                                model.Birthday = Convert.ToDateTime(dt.Rows[i][8].ToString().Trim());
                                            }
                                            else
                                            {
                                                model.Birthday = null;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            model.Birthday = null;

                                        }

                                        model.IDnum = dt.Rows[i][9].ToString();


                                        model.MaritalStatus = dt.Rows[i][10].ToString();
                                        model.School = dt.Rows[i][11].ToString();
                                        model.Edu = dt.Rows[i][12].ToString();
                                        model.Profession = dt.Rows[i][13].ToString();
                                        model.Nation = dt.Rows[i][14].ToString();
                                        model.NativePlace = dt.Rows[i][15].ToString();

                                        model.PoliticsStatus = dt.Rows[i][16].ToString();
                                        model.HouseholdRegister = dt.Rows[i][17].ToString();
                                        model.PresentAddress = dt.Rows[i][18].ToString();//现居住地址也是家庭地址
                                        model.EmergencyContactTel = dt.Rows[i][19].ToString();//紧急联系人电话也是家庭电话
                                        if (dt.Rows[i][20] != null && dt.Rows[i][20].ToString() != "")
                                        {
                                            model.Hiredate = Convert.ToDateTime(dt.Rows[i][20].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.Hiredate = null;
                                        }

                                        if (dt.Rows[i][21] != null && dt.Rows[i][21].ToString() != "")
                                        {
                                            model.OwnUnitsWorkYears = int.Parse(dt.Rows[i][21].ToString());
                                        }
                                        else
                                        {
                                            model.OwnUnitsWorkYears = null;
                                        }
                                        model.Insurance = dt.Rows[i][22].ToString().Trim();//保险办理
                                        if (dt.Rows[i][23] != null && dt.Rows[i][23].ToString() != "")
                                        {
                                            model.WorkYears = int.Parse(dt.Rows[i][23].ToString().Trim());
                                        }
                                        else { model.WorkYears = null; }
                                        model.Memo = dt.Rows[i][24].ToString().Trim();
                                        //25离职日期暂未保存
                                        if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString() != "")
                                        {
                                            model.QuitDate = dt.Rows[i][25].ToString().Trim();
                                        }
                                        else
                                        {
                                            model.QuitDate = null;
                                        }

                                        model.OnJob = int.Parse(dt.Rows[i][26].ToString());


                                        if (dt.Rows[i][27] != null && dt.Rows[i][27].ToString().Trim() != "")
                                        {
                                            model.SalaryCard = dt.Rows[i][27].ToString().Trim();
                                        }
                                        model.NickName = dt.Rows[i][28].ToString().Trim();
                                        if (dt.Rows[i][29] != null && dt.Rows[i][29].ToString() != "")
                                        {
                                            model.ContractFirstStartTime = Convert.ToDateTime(dt.Rows[i][29].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractFirstStartTime = null;
                                        }
                                        if (dt.Rows[i][30] != null && dt.Rows[i][30].ToString() != "")
                                        {
                                            model.ContractFirstDeadline = Convert.ToDateTime(dt.Rows[i][30].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractFirstDeadline = null;
                                        }
                                        if (dt.Rows[i][32] != null && dt.Rows[i][32].ToString() != "")
                                        {
                                            model.ContractSecondDeadline = Convert.ToDateTime(dt.Rows[i][32].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractSecondDeadline = null;
                                        }

                                        model.SignCompany = dt.Rows[i][34].ToString().Trim();
                                        db.T_PersonnelFile.Add(model);
                                        var zz = dt.Rows[i][25];
                                        //if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString().Trim() != "")
                                        //{
                                        //    db.SaveChanges();
                                        //    T_PersonnelQuit quitmodel = new T_PersonnelQuit();
                                        //    quitmodel.QuitDate = Convert.ToDateTime(dt.Rows[i][25].ToString().Trim());//离职时间
                                        //    quitmodel.Pid = model.ID;
                                        //    quitmodel.QuitData = null;//离职资料(导入表无此项)
                                        //    db.T_PersonnelQuit.Add(quitmodel);

                                        //}
                                    }

                                }
                            }
                            else//没有ebms账号
                            {
                                T_User newUser = new T_User();
                                newUser.Name = dt.Rows[i][2].ToString().Trim();
                                newUser.IsManagers = "0";
                                newUser.DepartmentId = "1";
                                newUser.Nickname = dt.Rows[i][28].ToString().Trim();
                                newUser.PassWord = "123456";
                                newUser.Power = "6";
                                newUser.Tel = dt.Rows[i][7].ToString();
                                db.T_User.Add(newUser);
                                db.SaveChanges();

                                T_Department dept = db.T_Department.Find(1);
                                dept.employees = dept.employees + "," + newUser.ID;
                                db.Entry<T_Department>(dept).State = System.Data.Entity.EntityState.Modified;
                                if (dt.Rows[i][2].ToString() != "")//
                                {

                                    string ID_num = dt.Rows[i][9].ToString().Trim();
                                    T_PersonnelFile cc = db.T_PersonnelFile.FirstOrDefault(a => a.IDnum == ID_num && ID_num != "");  //根据身份证号查重,如果无重复就做新增，重复的就不做任何处理
                                    if (cc == null)
                                    {
                                        T_PersonnelFile model = new T_PersonnelFile();
                                        model.InterviewStep = 99;
                                        model.oid = newUser.ID;
                                        model.IsDelete = 0;
                                        model.OnJob = 0;
                                        model.Code = dt.Rows[i][1].ToString();
                                        model.TrueName = dt.Rows[i][2].ToString().Trim();

                                        model.Sex = dt.Rows[i][3].ToString();
                                        model.online = dt.Rows[i][4].ToString();
                                        model.Department = dt.Rows[i][5].ToString();
                                        model.Job = dt.Rows[i][6].ToString();
                                        model.Tel = dt.Rows[i][7].ToString();
                                        try
                                        {
                                            if (dt.Rows[i][8] != null && dt.Rows[i][8].ToString().Trim() != "")
                                            {
                                                model.Birthday = Convert.ToDateTime(dt.Rows[i][8].ToString().Trim());
                                            }
                                            else
                                            {
                                                model.Birthday = null;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            model.Birthday = null;

                                        }

                                        model.IDnum = dt.Rows[i][9].ToString();


                                        model.MaritalStatus = dt.Rows[i][10].ToString();
                                        model.School = dt.Rows[i][11].ToString();
                                        model.Edu = dt.Rows[i][12].ToString();
                                        model.Profession = dt.Rows[i][13].ToString();
                                        model.Nation = dt.Rows[i][14].ToString();
                                        model.NativePlace = dt.Rows[i][15].ToString();

                                        model.PoliticsStatus = dt.Rows[i][16].ToString();
                                        model.HouseholdRegister = dt.Rows[i][17].ToString();
                                        model.PresentAddress = dt.Rows[i][18].ToString();//现居住地址也是家庭地址
                                        model.EmergencyContactTel = dt.Rows[i][19].ToString();//紧急联系人电话也是家庭电话
                                        if (dt.Rows[i][20] != null && dt.Rows[i][20].ToString() != "")
                                        {
                                            model.Hiredate = Convert.ToDateTime(dt.Rows[i][20].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.Hiredate = null;
                                        }

                                        if (dt.Rows[i][21] != null && dt.Rows[i][21].ToString() != "")
                                        {
                                            model.OwnUnitsWorkYears = int.Parse(dt.Rows[i][21].ToString());
                                        }
                                        else
                                        {
                                            model.OwnUnitsWorkYears = null;
                                        }
                                        model.Insurance = dt.Rows[i][22].ToString().Trim();//保险办理
                                        if (dt.Rows[i][23] != null && dt.Rows[i][23].ToString() != "")
                                        {
                                            model.WorkYears = int.Parse(dt.Rows[i][23].ToString().Trim());
                                        }
                                        else { model.WorkYears = null; }
                                        model.Memo = dt.Rows[i][24].ToString().Trim();
                                        //25离职日期暂未保存
                                        if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString() != "")
                                        {
                                            model.QuitDate = dt.Rows[i][25].ToString().Trim();
                                        }
                                        else
                                        {
                                            model.QuitDate = null;
                                        }

                                        model.OnJob = int.Parse(dt.Rows[i][26].ToString());


                                        if (dt.Rows[i][27] != null && dt.Rows[i][27].ToString().Trim() != "")
                                        {
                                            model.SalaryCard = dt.Rows[i][27].ToString().Trim();
                                        }
                                        model.NickName = dt.Rows[i][28].ToString().Trim();
                                        if (dt.Rows[i][29] != null && dt.Rows[i][29].ToString() != "")
                                        {
                                            model.ContractFirstStartTime = Convert.ToDateTime(dt.Rows[i][29].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractFirstStartTime = null;
                                        }
                                        if (dt.Rows[i][30] != null && dt.Rows[i][30].ToString() != "")
                                        {
                                            model.ContractFirstDeadline = Convert.ToDateTime(dt.Rows[i][30].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractFirstDeadline = null;
                                        }
                                        if (dt.Rows[i][31] != null && dt.Rows[i][31].ToString() != "")
                                        {
                                            model.ContractSecondDeadline = Convert.ToDateTime(dt.Rows[i][31].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractSecondDeadline = null;
                                        }
                                        if (dt.Rows[i][32] != null && dt.Rows[i][32].ToString() != "")
                                        {
                                            model.ContractThirdDeadline = Convert.ToDateTime(dt.Rows[i][32].ToString().Trim());
                                        }
                                        else
                                        {
                                            model.ContractThirdDeadline = null;
                                        }
                                        model.SignCompany = dt.Rows[i][33].ToString().Trim();
                                        db.T_PersonnelFile.Add(model);
                                        var zz = dt.Rows[i][25];
                                        //if (dt.Rows[i][25] != null && dt.Rows[i][25].ToString().Trim() != "")
                                        //{
                                        //    db.SaveChanges();
                                        //    T_PersonnelQuit quitmodel = new T_PersonnelQuit();
                                        //    quitmodel.QuitDate = Convert.ToDateTime(dt.Rows[i][25].ToString().Trim());//离职时间
                                        //    quitmodel.Pid = model.ID;
                                        //    quitmodel.QuitData = null;//离职资料(导入表无此项)
                                        //    db.T_PersonnelQuit.Add(quitmodel);

                                        //}
                                    }

                                }
                            }


                        }
                        try
                        {
                            db.SaveChanges();
                            sc.Complete();
                        }
                        catch (Exception e)
                        {
                            throw e;

                        }
                        if (noAccount != "")
                        {
                            message = noAccount + "用户没有EBMS账号，请创建后再导入该用户档案信息,其他人员导入成功";
                        }
                    }

                }


                return "<script>  alert('" + message + "');parent.$('#ImportDiv').dialog('close');  </script>";
            }
        }
        public FileResult ExportExcel(string queryStr, int onJob)//导出excel
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");

            //sheet1.Rows.RowHeight = 25;
            //worksheet.Columns.ColumnWidth = 24;

            //获取list数据

            IQueryable<T_PersonnelFile> qs = null;

            qs = db.T_PersonnelFile;
            if (queryStr != null && queryStr.Trim() != "")
            {
                qs = qs.Where(a => a.NickName.Contains(queryStr));

            }
            if (onJob != 4 && onJob != 0)
            {
                qs = qs.Where(a => a.OnJob == onJob);
            }
            else if (onJob == 0)
            {
                qs = qs.Where(a => a.OnJob == 0 || a.OnJob == 3);

            }
            List<T_PersonnelFile> ListInfo = qs.ToList();
            NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("序号");
            row1.CreateCell(1).SetCellValue("编号");
            row1.CreateCell(2).SetCellValue("姓名");
            row1.CreateCell(3).SetCellValue("性别");
            row1.CreateCell(4).SetCellValue("部门");
            row1.CreateCell(5).SetCellValue("岗位");
            row1.CreateCell(6).SetCellValue("职务");
            row1.CreateCell(7).SetCellValue("手机");
            row1.CreateCell(8).SetCellValue("出生年月");
            row1.CreateCell(9).SetCellValue("身份证号");
            row1.CreateCell(10).SetCellValue("婚姻状况");
            row1.CreateCell(11).SetCellValue("毕业学校");
            row1.CreateCell(12).SetCellValue("学历");
            row1.CreateCell(13).SetCellValue("专业");
            row1.CreateCell(14).SetCellValue("民族");
            row1.CreateCell(15).SetCellValue("籍贯");
            row1.CreateCell(16).SetCellValue("政治面貌");
            row1.CreateCell(17).SetCellValue("户籍地址");
            row1.CreateCell(18).SetCellValue("家庭地址");
            row1.CreateCell(19).SetCellValue("家庭电话");
            row1.CreateCell(20).SetCellValue("入职日期");
            row1.CreateCell(21).SetCellValue("本单位工龄");
            row1.CreateCell(22).SetCellValue("保险办理");
            row1.CreateCell(23).SetCellValue("工龄");
            row1.CreateCell(24).SetCellValue("备注");
            row1.CreateCell(25).SetCellValue("离职日期");
            row1.CreateCell(26).SetCellValue("是否离职");
            row1.CreateCell(27).SetCellValue("是否办理工资卡");
            row1.CreateCell(28).SetCellValue("花名");
            row1.CreateCell(29).SetCellValue("第一次劳动合同开始时间");
            row1.CreateCell(30).SetCellValue("第一次劳动合同结束时间");
            row1.CreateCell(31).SetCellValue("第二次劳动合同结束时间");
            row1.CreateCell(32).SetCellValue("第三次劳动合同结束时间");
            row1.CreateCell(32).SetCellValue("第三次劳动合同结束时间");
            row1.CreateCell(33).SetCellValue("签订公司");
            sheet1.SetColumnWidth(0, 5 * 256);
            sheet1.SetColumnWidth(1, 5 * 256);
            sheet1.SetColumnWidth(2, 10 * 256);
            sheet1.SetColumnWidth(3, 10 * 256);
            sheet1.SetColumnWidth(4, 15 * 256);
            sheet1.SetColumnWidth(5, 15 * 256);
            sheet1.SetColumnWidth(6, 15 * 256);
            sheet1.SetColumnWidth(7, 15 * 256);
            sheet1.SetColumnWidth(8, 15 * 256);
            sheet1.SetColumnWidth(9, 20 * 256);
            sheet1.SetColumnWidth(10, 10 * 256);
            sheet1.SetColumnWidth(11, 15 * 256);
            sheet1.SetColumnWidth(12, 10 * 256);
            sheet1.SetColumnWidth(13, 15 * 256);
            sheet1.SetColumnWidth(14, 10 * 256);
            sheet1.SetColumnWidth(15, 10 * 256);
            sheet1.SetColumnWidth(16, 15 * 256);
            sheet1.SetColumnWidth(17, 20 * 256);
            sheet1.SetColumnWidth(18, 20 * 256);
            sheet1.SetColumnWidth(19, 15 * 256);
            sheet1.SetColumnWidth(20, 15 * 256);
            sheet1.SetColumnWidth(21, 10 * 256);
            sheet1.SetColumnWidth(22, 15 * 256);
            sheet1.SetColumnWidth(23, 15 * 256);
            sheet1.SetColumnWidth(24, 15 * 256);
            sheet1.SetColumnWidth(25, 15 * 256);
            sheet1.SetColumnWidth(26, 15 * 256);
            sheet1.SetColumnWidth(27, 10 * 256);
            sheet1.SetColumnWidth(28, 10 * 256);
            sheet1.SetColumnWidth(29, 20 * 256);
            sheet1.SetColumnWidth(30, 20 * 256);
            sheet1.SetColumnWidth(31, 20 * 256);
            sheet1.SetColumnWidth(32, 20 * 256);
            sheet1.SetColumnWidth(33, 10 * 256);
            for (int i = 0; i < ListInfo.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue((i + 1));
                rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                rowtemp.Cells[0].CellStyle.WrapText = false;
                rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";
                rowtemp.CreateCell(1).SetCellValue(ListInfo[i].Code);
                rowtemp.CreateCell(2).SetCellValue(ListInfo[i].TrueName);
                rowtemp.CreateCell(3).SetCellValue(ListInfo[i].Sex);
                rowtemp.CreateCell(4).SetCellValue(ListInfo[i].online);
                rowtemp.CreateCell(5).SetCellValue(ListInfo[i].Department);
                rowtemp.CreateCell(6).SetCellValue(ListInfo[i].Job);
                rowtemp.CreateCell(7).SetCellValue(ListInfo[i].Tel);
                if (ListInfo[i].Birthday != null)
                {
                    rowtemp.CreateCell(8).SetCellValue(ListInfo[i].Birthday.Value.ToShortDateString());
                }
                else { rowtemp.CreateCell(8).SetCellValue(""); }
                if (ListInfo[i].IDnum != null)
                {
                    rowtemp.CreateCell(9).SetCellValue(ListInfo[i].IDnum.ToString());
                }
                else { rowtemp.CreateCell(9).SetCellValue(""); }
                rowtemp.CreateCell(10).SetCellValue(ListInfo[i].MaritalStatus);
                rowtemp.CreateCell(11).SetCellValue(ListInfo[i].School);
                rowtemp.CreateCell(12).SetCellValue(ListInfo[i].Edu);
                rowtemp.CreateCell(13).SetCellValue(ListInfo[i].Profession);
                rowtemp.CreateCell(14).SetCellValue(ListInfo[i].Nation);
                rowtemp.CreateCell(15).SetCellValue(ListInfo[i].NativePlace);
                rowtemp.CreateCell(16).SetCellValue(ListInfo[i].PoliticsStatus);
                rowtemp.CreateCell(17).SetCellValue(ListInfo[i].HouseholdRegister);
                rowtemp.CreateCell(18).SetCellValue(ListInfo[i].PresentAddress);
                rowtemp.CreateCell(19).SetCellValue(ListInfo[i].EmergencyContactTel);
                if (ListInfo[i].Hiredate != null)
                {
                    rowtemp.CreateCell(20).SetCellValue(ListInfo[i].Hiredate.Value.ToShortDateString());
                }
                else
                {
                    rowtemp.CreateCell(20).SetCellValue("");
                }
                if (ListInfo[i].OwnUnitsWorkYears != null)
                {
                    rowtemp.CreateCell(21).SetCellValue(ListInfo[i].OwnUnitsWorkYears.Value);
                }
                else
                {
                    rowtemp.CreateCell(21).SetCellValue("");
                }

                rowtemp.CreateCell(22).SetCellValue(ListInfo[i].Insurance);
                if (ListInfo[i].WorkYears != null)
                {
                    rowtemp.CreateCell(23).SetCellValue(ListInfo[i].WorkYears.Value);
                }
                rowtemp.CreateCell(24).SetCellValue(ListInfo[i].Memo);
                try
                {
                    //T_PersonnelQuit quitModel = db.T_PersonnelQuit.FirstOrDefault(a => a.Pid == ListInfo[i].ID);
                    rowtemp.CreateCell(25).SetCellValue(ListInfo[i].QuitDate);
                }
                    
                catch (Exception)
                {
                    rowtemp.CreateCell(25).SetCellValue("");
                }
                rowtemp.CreateCell(26).SetCellValue(ListInfo[i].OnJob.Value);
                rowtemp.CreateCell(27).SetCellValue(ListInfo[i].SalaryCard);
                rowtemp.CreateCell(28).SetCellValue(ListInfo[i].NickName);
                if (ListInfo[i].ContractFirstStartTime.HasValue)
                {
                    rowtemp.CreateCell(29).SetCellValue(ListInfo[i].ContractFirstStartTime.Value.ToString());
                }
                if (ListInfo[i].ContractFirstDeadline.HasValue)
                {
                    rowtemp.CreateCell(30).SetCellValue(ListInfo[i].ContractFirstDeadline.Value.ToString());
                }
                if (ListInfo[i].ContractSecondDeadline.HasValue)
                {
                    rowtemp.CreateCell(31).SetCellValue(ListInfo[i].ContractSecondDeadline.Value.ToString());
                }
                if (ListInfo[i].ContractThirdDeadline.HasValue)
                {
                    rowtemp.CreateCell(32).SetCellValue(ListInfo[i].ContractThirdDeadline.Value.ToString());
                }
               
               
                rowtemp.CreateCell(33).SetCellValue(ListInfo[i].SignCompany);
            }

            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;

            return File(ms, "application/vnd.ms-excel", "电商人员.xls");
        }
        #endregion
        [HttpPost]
        public String UploadPic()
        {
            string finaly = "";


            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file];//get post file 
                if (postFile.ContentLength == 0)
                {
                    return " <script > alert('文件为空，请重新选择');window.location.href='ViewUploadPic';  </script>";
                }

                else
                {

                    string newFilePath = Server.MapPath("~/Upload/InchPhoto/");//save path 
                    string fileName = Path.GetFileName(postFile.FileName);

                    var lastIndex = fileName.LastIndexOf(".");
                    var result = fileName.Substring(lastIndex + 1);
                    if ("jpg" != result && "gif" != result && "png" != result)
                    {

                        return " <script > alert('请选择图片文件');window.location.href='ViewUploadPic'  </script>";
                    }
                    else
                    {
                        finaly = "/Upload/InchPhoto/" + fileName;
                        postFile.SaveAs(newFilePath + fileName);//save file 
                    }


                }
            }// alert('保存成功！');
            //"<script> parent.$('#uploadDiv').dialog('close');  </script>";//
            // return "<script >parent.uploadclose('" + finaly + "');alert('上传成功');parent.$('#uploadDiv').dialog('close');</script>";
            return "<script >parent.$('#perInfo_Pic').val('" + finaly + "');alert('上传成功');parent.$('#uploadDiv').dialog('close');</script>";
        }
        public JsonResult checkUser(string name)
        {
            IQueryable<T_User> users = db.T_User.Where(a => a.Nickname == name);//检查该用户是否已经存在账号，如果没有需要先添加账号
            int i = users.Count();
            if (i < 1)
            {
                return Json(0);
            }
            else 
            {
                IQueryable<T_PersonnelFile> person = db.T_PersonnelFile.Where(a => a.NickName == name&&a.IsDelete==0);//检查该用户是否已存在档案，如果有不允许重复创建
                int p = person.Count();
                if (p > 0)
                {
                    return Json(1);
                }
                else
                {
                    return Json(2);
                }
            }
        }
        [HttpPost]
        public JsonResult GetPicById(string id)
        {
            int ID = int.Parse(id);
            T_PersonnelFile p = db.T_PersonnelFile.Find(ID);
            if (p!=null)
            {
                if (p.Pic != null && p.Pic != "")
                {
                    var json = new
                    {
                        pic = p.Pic,


                    };
                    return Json(json);
                }
                else
                {
                    var json = new
                    {
                        pic = "/Upload/InchPhoto/defalt.jpg",

                    };
                    return Json(json);
                }
            }
           
            else
            {
                var json = new
                {
                    pic = "/Upload/InchPhoto/defalt.jpg",

                };
                return Json(json);
            }
        }
        public class personelModel
        {
            public T_PersonnelFile person { get; set; }
            public T_PersonnelFamily family { get; set; }
            public T_PersonnelReward reward { get; set; }
            public T_PersonnelPerformance performance { get; set; }
            //public T_PersonnelTransfer trans { get; set; }
            //public T_PersonnelQuit quit { get; set; }
            public T_PersonnelWorkExperience experience { get; set; }
            public T_PersonnelEduBackgroud EduBackgroud { get; set; }

        }

    }
}