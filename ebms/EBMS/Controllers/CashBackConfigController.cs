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
    public class CashBackConfigController : BaseController
    {
        /// <summary>
        /// 返现流程配置
        /// </summary>
        /// <returns></returns>
        /// 
        #region 公共方法
        EBMSEntities db = new EBMSEntities();
        //返现理由下拉框
        public List<SelectListItem> GetReason()
        {
            var list = db.T_CashBackReason.AsQueryable();
            var selectList = new SelectList(list, "Name", "Name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.AddRange(selectList);
            return selecli;
        }
        #endregion
        #region 视图
        public ActionResult Index()
        {
            return View();
        }
        [Description("配置审核流程页面")]
        public ActionResult Method()
        {
            return View();
        }
        [Description("配置支付关系")]
        public ActionResult CashBackPaymentList()
        {
            return View();
        }
        //返现理由下拉框
        public List<SelectListItem> GetWDTshop()
        {

            var list = db.T_WDTshop.AsQueryable();
            var selectList = new SelectList(list, "shop_name", "shop_name");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        [Description("新增配置支付关系")]
        public ActionResult CashBackPaymentAdd()
        {
            ViewData["ShopName"] = GetWDTshop();

            
            return View();
        }
        [Description("选择审核流程页面")]
        public ActionResult CashBackSelect()
        {
            ViewData["Reason"] = GetReason();
            return View();
        }
        [Description("新增流程页面")]
        public ActionResult MethodAdd()
        {
            return View();
        }
        [Description("选择店铺页面")]
        public ActionResult ShopList()
        {
            return View();
        }
        [Description("选择审核流程页面")]
        public ActionResult MethodList()
        {
            return View();
        }
        [Description("选择审核人员页面")]
        public ActionResult UserTable(string ID)
        {
            ViewData["ID"] = ID;
            return View();
        }
        #endregion
        #region 方法

        class MethodMod
        {
            public Nullable<int> Method { get; set; }
            public string Name { get; set; }
        }
      
        //  支付列表 
        [HttpPost]
        public ContentResult GetPaymentList(Lib.GridPager pager, string queryStr, string method)
        {
            List<T_CashBackFrom> queryData = db.T_CashBackFrom.AsQueryable().ToList();
           
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.Name.Contains(queryStr) || a.ShopName.Contains(queryStr)).ToList();
            }
            //分页
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderBy(c =>c.ID.ToString()).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        //  审核流程列表   
        [HttpPost]
        public ContentResult GetMethodList(Lib.GridPager pager, string queryStr, string method)
        {
            List<T_CashbackMethod> queryData = db.T_CashbackMethod.AsQueryable().ToList();
            string sql = "select Method from T_CashbackMethod where 1=1 ";
            if (!string.IsNullOrEmpty(method))
            {
                sql += " and    Method = " + method + " ";
            }
            sql += "   group by Method ";
            List<MethodMod> model = db.Database.SqlQuery<MethodMod>(sql).ToList();

            for (int i = 0; i < model.Count(); i++)
            {
                string manList = "";
                int _method = int.Parse(model[i].Method.ToString());
                List<T_CashbackMethod> MethodList = db.T_CashbackMethod.Where(a => a.Method == _method).ToList();
                for (int j = 0; j < MethodList.Count(); j++)
                {
                    manList += MethodList[j].Name + ">";
                }
                model[i].Name = manList.Substring(0, manList.Length - 1);
            }
            if (!string.IsNullOrEmpty(queryStr))
            {
                model = model.Where(a => a.Name.Contains(queryStr)).ToList();
            }
            //分页
            pager.totalRows = model.Count();
            model = model.OrderBy(c => int.Parse(c.Method.ToString())).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(model, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //新增一条流程
        [HttpPost]
        public JsonResult MethodAdd(string method, int cashier)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                T_CashbackMethod MOD_Method = new T_CashbackMethod();
                //查出最后一个流程的序号 加1 作为当前新增流程的序号
                List<T_CashbackMethod> MOD = db.Database.SqlQuery<T_CashbackMethod>("select top 1 * from T_CashbackMethod ORDER by Method DESC").ToList();
                int _method = 1;
                if (MOD.Count() != 0)
                {
                    _method = int.Parse(MOD[0].Method.ToString()) + 1;
                }
                string[] methodArry = method.Split(',');
                int t = 0;
                for (int i = 0; i < methodArry.Length; i++)
                {
                    MOD_Method.Name = methodArry[i];
                    MOD_Method.Step = i;
                    MOD_Method.Method = _method;
                    if (i == cashier)
                    {
                        //设置出纳
                        MOD_Method.Cashier = 1;
                    }
                    db.T_CashbackMethod.Add(MOD_Method);
                    t = db.SaveChanges();
                }
                sc.Complete();
                string result = "保存失败";
                if (t > 0)
                {
                    result = "保存成功";
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        //查询店铺列表
        [HttpPost]
        public ContentResult GetShopList(Lib.GridPager pager, string queryStr)
        {

            //查询出所有未删除的
            IQueryable<T_ShopFromGY> queryData = db.T_ShopFromGY.Where(a => a.Isdelete == "0");
            //根据订单号，提交者姓名，会员号码查询
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => a.name.Contains(queryStr) || a.code.Contains(queryStr));
            }
            //根据状态查询
            //分页
            pager.totalRows = queryData.Count();
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ShopFromGY> list = new List<T_ShopFromGY>();
            foreach (var item in queryData)
            {
                T_ShopFromGY i = new T_ShopFromGY();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        //查询该配置是否存在
        public JsonResult NextStep(string shop, string reason, int money, string roles)
        {
            List<T_CashbackApproveConfig> findMod = db.T_CashbackApproveConfig.Where(a => a.Reason == reason && a.Shop == shop && a.Money == money && a.Roles == roles).ToList();
            string Result = "";
            string configId = "T";
            if (findMod.Count > 0)
            {
                if (findMod[0].Method != null)
                {
                    int _Method = int.Parse(findMod[0].Method.ToString());
                    Result += "已有该返现配置，审核流程编号为：" + _Method;
                    List<T_CashbackMethod> MethodList = db.T_CashbackMethod.Where(a => a.Method == _Method).ToList();
                    string manList = "";
                    for (int j = 0; j < MethodList.Count(); j++)
                    {
                        manList += MethodList[j].Name + ">";
                    }
                    manList = manList.Substring(0, manList.Length - 1);
                    Result += ",审批流程：" + manList;

                }
                else
                {
                    Result += "已有该返现配置,未选择审核流程";
                }
                configId = findMod[0].ID.ToString();
            }
            else
            {
                Result = "该返现配置没有流程";
            }
            return Json(new { Result = Result, Id = configId }, JsonRequestBehavior.AllowGet);
        }
        //选择流程保存  
        public JsonResult SelectSave(string method, string id, string shop, string reason, int money, string roles)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                string result = "保存失败";
                if (id == "T")
                {
                    //新增
                    T_CashbackApproveConfig MOD_Config = new T_CashbackApproveConfig();
                    MOD_Config.Shop = shop;
                    MOD_Config.Roles = roles;
                    MOD_Config.Reason = reason;
                    MOD_Config.Method = int.Parse(method);
                    MOD_Config.Money = money;
                    db.T_CashbackApproveConfig.Add(MOD_Config);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        result = "保存成功";
                    }
                }
                else
                {
                    //修改
                    int _id = int.Parse(id);
                    T_CashbackApproveConfig MOD = db.T_CashbackApproveConfig.Find(_id);
                    if (MOD == null)
                    {
                        return Json("保存失败", JsonRequestBehavior.AllowGet);
                    }
                    MOD.Shop = shop;
                    MOD.Roles = roles;
                    MOD.Reason = reason;
                    MOD.Method = int.Parse(method);
                    MOD.Money = money;
                    db.Entry<T_CashbackApproveConfig>(MOD).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        result = "保存成功";
                    }
                }
                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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
        #endregion

        //返现新增保存
        [HttpPost]
        public JsonResult AddSave(T_CashBackFrom model)
        {
            string shopName = model.ShopName;
            string IsBlending = model.IsBlending;
            if(IsBlending=="1")
            { 
            List<T_CashBackFrom> CashBackList = db.T_CashBackFrom.Where(a => a.ShopName == shopName && a.IsBlending == "1").ToList();
            if (CashBackList.Count > 0)
            {
                return Json(new { State = "Faile", Message = "保存失败,该店铺已有主账号" }, JsonRequestBehavior.AllowGet);
            }

            }

            db.T_CashBackFrom.Add(model);
                int i=db.SaveChanges();
            if (i > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "保存失败,请联系店铺负责人添加审核流程" }, JsonRequestBehavior.AllowGet);
            }
        }
        //删除返现
        public JsonResult Del(int ID)
        {
            using (TransactionScope sc = new TransactionScope())
            {
                T_CashBackFrom MOD = db.T_CashBackFrom.Find(ID);
                if (MOD != null)
                {
                    db.T_CashBackFrom.Remove(MOD);
                }
               int i=db.SaveChanges();
                string result = "";
                if (i > 0)
                {

                    result = "删除成功";
                } 
                else
                {
                    result = "删除失败";
                }

                sc.Complete();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
