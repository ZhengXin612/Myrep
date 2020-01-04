using EBMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class BackupsRecruitController : Controller
    {

        EBMSEntities db = new EBMSEntities();

        public class EmploymentRegistration
        {
            public R_Basics Basics { get; set; }
            public List<R_Basics_Education> Education { get; set; }
            public List<R_Basics_Family> Family { get; set; }
            public List<R_Basics_Relative> Relative { get; set; }
            public List<R_Basics_Reward> Reward { get; set; }
            public List<R_Basics_Title> Title { get; set; }
            public List<R_Basics_WorkExperience> WorkExperience { get; set; }
        }
        //
        // GET: /BackupsRecruit/
        public List<SelectListItem> ConfigList(string type)
        {
            var list = db.T_EmployDemandConfig.Where(a => a.Type == type);
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> DStudyType()
        {
            List<SelectListItem> SexList = new List<SelectListItem> {
            new SelectListItem { Text = "请选择", Value = "" },
            new SelectListItem { Text = "统招", Value = "统招" },
            new SelectListItem { Text = "非统招", Value = "非统招" },
            };
            return SexList;
        }
      
        public List<SelectListItem> SexList()
        {
            List<SelectListItem> SexList = new List<SelectListItem> {
            new SelectListItem { Text = "男", Value = "男" },
            new SelectListItem { Text = "女", Value = "女" },
            };
            return SexList;
        }
        public  List<SelectListItem> BoolList()
        {
            List<SelectListItem> SexList = new List<SelectListItem> {
            new SelectListItem { Text = "是", Value = "1" },
            new SelectListItem { Text = "否", Value = "0" },
            };
            return SexList;
        }
        public List<SelectListItem> MarryStateList()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "已婚", Value = "已婚" },
            new SelectListItem { Text = "未婚", Value = "未婚" },
            new SelectListItem { Text = "丧偶", Value = "丧偶" },
            new SelectListItem { Text = "离婚", Value = "离婚" },
            };
            return MarryStateList;
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
        public List<SelectListItem> MarryHealthState()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "健康", Value = "健康" },
            new SelectListItem { Text = "一般", Value = "一般" },
            new SelectListItem { Text = "有慢性病", Value = "有慢性病" },
            new SelectListItem { Text = "心血管病", Value = "心血管病" },
            new SelectListItem { Text = "脑血管病", Value = "脑血管病" },
            new SelectListItem { Text = "慢性呼吸系统病", Value = "慢性呼吸系统病" },
            new SelectListItem { Text = "慢性消化系统病", Value = "慢性消化系统病" },
            new SelectListItem { Text = "结核病", Value = "结核病" },
            new SelectListItem { Text = "糖尿病", Value = "糖尿病" },
            new SelectListItem { Text = "神经或精神疾病", Value = "神经或精神疾病" },
            new SelectListItem { Text = "癌症", Value = "癌症" },
            new SelectListItem { Text = "其他慢性病", Value = "其他慢性病" },
            new SelectListItem { Text = "其他慢性病", Value = "其他慢性病" },
            new SelectListItem { Text = "残疾", Value = "残疾" },
            new SelectListItem { Text = "视力残疾", Value = "视力残疾" },
            new SelectListItem { Text = "听力残疾", Value = "听力残疾" },
            new SelectListItem { Text = "语言残疾", Value = "语言残疾" },
            new SelectListItem { Text = "智力残疾", Value = "智力残疾" },
            new SelectListItem { Text = "精神残疾", Value = "精神残疾" },
            new SelectListItem { Text = "多重残疾", Value = "多重残疾" },
            new SelectListItem { Text = "其他残疾", Value = "其他残疾" },
            };
            return MarryStateList;
        }
        public List<SelectListItem> MarryEducation()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "小学", Value = "小学" },
            new SelectListItem { Text = "初中", Value = "初中" },
            new SelectListItem { Text = "技工", Value = "技工" },
            new SelectListItem { Text = "职业高中", Value = "职业高中" },
            new SelectListItem { Text = "普通高中", Value = "普通高中" },
            new SelectListItem { Text = "中专", Value = "中专" },
            new SelectListItem { Text = "非统招大专", Value = "非统招大专" },
            new SelectListItem { Text = "统招大专", Value = "统招大专" },
            new SelectListItem { Text = "非统招本科", Value = "非统招本科" },
            new SelectListItem { Text = "统招本科", Value = "统招本科" },
            new SelectListItem { Text = "硕士", Value = "硕士" },
            new SelectListItem { Text = "博士", Value = "博士" },
            };
            return MarryStateList;
        }
        public List<SelectListItem> MarryPoliticsStatus()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "群众", Value = "群众" },
            new SelectListItem { Text = "中国共产党党员", Value = "中国共产党党员" },
            new SelectListItem { Text = "中国共产党预备党员", Value = "中国共产党预备党员" },
            new SelectListItem { Text = "中国共产主义青年团团员", Value = "中国共产主义青年团团员" },
            new SelectListItem { Text = "中国国民党革命委员会会员", Value = "中国国民党革命委员会会员" },
            new SelectListItem { Text = "中国民主同盟盟员", Value = "中国民主同盟盟员" },
            new SelectListItem { Text = "中国民主建国会会员", Value = "中国民主建国会会员" },
            new SelectListItem { Text = "中国民主促进会会员", Value = "中国民主促进会会员" },
            new SelectListItem { Text = "中国农工民主党党员", Value = "中国农工民主党党员" },
            new SelectListItem { Text = "中国致公党党员", Value = "中国致公党党员" },
            new SelectListItem { Text = "九三学社社员", Value = "九三学社社员" },
            new SelectListItem { Text = "台湾民主自治同盟盟员", Value = "台湾民主自治同盟盟员" },
            new SelectListItem { Text = "无党派民主人士", Value = "无党派民主人士" },
            };
            return MarryStateList;
        }
        public List<SelectListItem> MarryHukouBookType()
        {
            List<SelectListItem> MarryStateList = new List<SelectListItem> {
            new SelectListItem { Text = "非农业家庭户口", Value = "非农业家庭户口" },
            new SelectListItem { Text = "农业家庭户口", Value = "农业家庭户口" },
   
            };
            return MarryStateList;
        }
        [AllowAnonymous]
        public ActionResult BackupsEmploymentRegistration()
        {
            
                   ViewData["StudyTypeList"] = DStudyType();
            ViewData["HukouBookTypeList"] = MarryHukouBookType();
            ViewData["HealthStateList"] = MarryHealthState();
            ViewData["EducationList"] = MarryEducation();
            ViewData["PoliticsStatusList"] = MarryPoliticsStatus();
            ViewData["JobList"] = ConfigList("岗位");
            ViewData["SexList"] = SexList();
            ViewData["BoolList"] = BoolList();
            ViewData["MarryStateList"] = MarryStateList();
            ViewData["DateType"] = DateType();
            return View();
        }

        public ActionResult ViewUploadPic()  //上传寸照
        {

            return View();
        }


        
          public ActionResult BackupsEmploymentRegistrationEdit(int  ID=0)  //查询页面
        {
            R_Basics model = db.R_Basics.Find(ID);
      
            if (model == null)
            {
                return HttpNotFound();
            }
            else
            {
                List<R_Basics_Education> EduBackgroud = db.R_Basics_Education.Where(a => a.BasicsID == ID).ToList();
                for (int i = 0; i < 4; i++)
                {
                    if (i >= EduBackgroud.Count)
                    {
                        R_Basics_Education sds = new R_Basics_Education();
                        EduBackgroud.Add(sds);
                    }
                }
                List<R_Basics_Family> Family = db.R_Basics_Family.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Relative> Relative = db.R_Basics_Relative.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Reward> Reward = db.R_Basics_Reward.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Title> Title = db.R_Basics_Title.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_WorkExperience> WorkExperience = db.R_Basics_WorkExperience.Where(a => a.BasicsID == ID).ToList();
                EmploymentRegistration perDetail = new EmploymentRegistration();

                ViewData["StudyTypeList"] = DStudyType();
                ViewData["HukouBookTypeList"] = MarryHukouBookType();
                ViewData["HealthStateList"] = MarryHealthState();
                ViewData["EducationList"] = MarryEducation();
                ViewData["PoliticsStatusList"] = MarryPoliticsStatus();
                ViewData["JobList"] = ConfigList("岗位");
                ViewData["SexList"] = SexList();
                ViewData["BoolList"] = BoolList();
                ViewData["MarryStateList"] = MarryStateList();
                ViewData["DateType"] = DateType();


                perDetail.Basics = model;
                perDetail.Education = EduBackgroud;
                perDetail.Family = Family;
                perDetail.Relative = Relative;
                perDetail.Reward = Reward;
                perDetail.Title = Title;
                perDetail.WorkExperience = WorkExperience;
                return View(perDetail);
            }
           
        }
        public ActionResult BackupsEmploymentRegistrationGrid()  //查询页面
        {
            return View();
        }
        public ContentResult GetList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<R_Basics> queryData = db.R_Basics.Where(a => a.IsDelete == 0);
       
          

            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.NickName != null && a.NickName.Contains(queryStr)) || (a.Name != null && a.Name.Contains(queryStr)));
            }
          
            if (queryData != null)
            {
                pager.totalRows = queryData.Count();
                //分页
                //queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
                List<R_Basics> list = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();

                string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
                return Content(json);
            }
            else
            {
                return Content("{\"total\":\"0 \",\"rows\":\"0\"");
            }
        }

        //详情页面
        public ActionResult BackupsEmploymentRegistrationDetail(int ID = 0)//人事详情
        {


            R_Basics model = db.R_Basics.Find(ID);
        
            if (model == null)
            {
                return HttpNotFound();
            }
            else
            {
                List<R_Basics_Education> EduBackgroud = db.R_Basics_Education.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Family> Family = db.R_Basics_Family.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Relative> Relative = db.R_Basics_Relative.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Reward> Reward = db.R_Basics_Reward.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_Title> Title = db.R_Basics_Title.Where(a => a.BasicsID == ID).ToList();
                List<R_Basics_WorkExperience> WorkExperience = db.R_Basics_WorkExperience.Where(a => a.BasicsID == ID).ToList();
                EmploymentRegistration perDetail = new EmploymentRegistration();
                perDetail.Basics = model;
                perDetail.Education = EduBackgroud;
                perDetail.Family = Family;
                perDetail.Relative = Relative;
                perDetail.Reward = Reward;
                perDetail.Title = Title;
                perDetail.WorkExperience = WorkExperience;
                return View(perDetail);
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
            return "<script >parent.$('#Basics_Pic').val('" + finaly + "');alert('上传成功');parent.$('#uploadDiv').dialog('close');</script>";
        }
        [AllowAnonymous]
        [HttpPost]
        public JsonResult BackupsEmploymentRegistrationAddSave(EmploymentRegistration model)
        {
         
            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    R_Basics PerFile = model.Basics;
                    PerFile.IsDelete = 0;
                    PerFile.ApplyDate = DateTime.Now;
                    db.R_Basics.Add(PerFile);
                    db.SaveChanges();
                    foreach (var InterviewRecord in model.Education)
                    {
                        if (!string.IsNullOrWhiteSpace(InterviewRecord.School))
                        {
                            InterviewRecord.BasicsID = PerFile.ID;
                            db.R_Basics_Education.Add(InterviewRecord);
                        }
                    }
                 
                    foreach (var Family in model.Family)
                    {
                        if (!string.IsNullOrWhiteSpace(Family.Name))
                        {
                            Family.BasicsID = PerFile.ID;
                            db.R_Basics_Family.Add(Family);
                        }
                    }

                    
                    foreach (var Reward in model.Reward)
                    {
                        if (!string.IsNullOrWhiteSpace(Reward.CompanyName))
                        {
                            Reward.BasicsID = PerFile.ID;
                            db.R_Basics_Reward.Add(Reward);
                        }
                    }
                
                    foreach (var Relative in model.Relative)
                    {
                        if (!string.IsNullOrWhiteSpace(Relative.Name))
                        {
                            Relative.BasicsID = PerFile.ID;
                            db.R_Basics_Relative.Add(Relative);
                        }
                    }
                 
                    foreach (var WorkExperience in model.WorkExperience)
                    {
                        if (!string.IsNullOrWhiteSpace(WorkExperience.CorporateName))
                        {
                            WorkExperience.BasicsID = PerFile.ID;
                            db.R_Basics_WorkExperience.Add(WorkExperience);
                        }
                    }
                    foreach (var Title in model.Title)
                    {
                        if (!string.IsNullOrWhiteSpace(Title.TitleInfo))
                        {
                            Title.BasicsID = PerFile.ID;
                            db.R_Basics_Title.Add(Title);
                        }
                    }
                    db.SaveChanges();
                    //   ModularByZP();
                    sc.Complete();
                    return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { State = "Faile", Message = e.Message }, JsonRequestBehavior.AllowGet);
                }


            }
        }

    }
}
