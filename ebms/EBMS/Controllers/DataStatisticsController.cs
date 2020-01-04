using EBMS.App_Code;
using EBMS.Models;
using LitJson;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class DataStatisticsController : BaseController
    {
        //
        // GET: /DataStatistics/
        EBMSEntities db = new EBMSEntities();

        public List<SelectListItem> GetManufactor()
        {
          // List<T_ProductClass> ProductClass = db.T_ProductClass.ToList();
           var list = db.T_ProductClass.AsQueryable();
           var selectList = new SelectList(list, "ClassName", "ClassName");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }
        public List<SelectListItem> Getclassify()
        {
            var list = db.T_ProductManufactor.AsQueryable();
            var selectList = new SelectList(list, "ManufactorName", "ManufactorName");
            List<SelectListItem> selecli = new List<SelectListItem>();
            selecli.Add(new SelectListItem { Text = "==请选择==", Value = "" });
            selecli.AddRange(selectList);
            return selecli;
        }


        public ActionResult ViewSingleProduChoice()
        {
            return View();
        }
        public ActionResult ViewSingleProductsplitAdd()
        {
            return View();
        }
        public ActionResult ProductPartsDetail(string Code)
        {
            ViewData["Code"] = Code;
            return View();
        }
        public ActionResult DataStatistics()
        {
            return View();
        }
        public ActionResult ProductPartsAdd(string tid)
        {
            ViewData["tid"] = tid;
            return View();
        }
        
        public ActionResult ProductCodeGenerate()
        {
            string Code = RandomNumber("Z");
           ViewData["Code"] = Code;
            return View();
        }

        public ActionResult ViewSingleProductsplitCheck()
        {
       
            return View();
        }
        public ActionResult ViewSingleProductParts()
        {
       
            return View();
        }
        


        public ActionResult ViewSingleProduct()
        {
          
            return View();
        }
        public ActionResult ViewSingleProductAdd()
        {
            string Code = RandomNumberGoods("X");

            ViewData["Code"] = Code;

            return View();
        }
        public ActionResult ViewProductCodeGenerate()
        {
          
            return View();
        }

        public ActionResult ProductPartsPartsDetail(string code)
        {


            ViewData["Code"] = code;

            return View();
        }
        public ActionResult ViewSingleProductGiftAdd()
        {
            string Code = RandomNumberGoods("P");
            ViewData["Code"] = Code;
            return View();
        }
        public ActionResult ViewProductCodeGenerateDetail(int tid)
        {
            ViewData["tid"] = tid;
            return View();
        }
        public ActionResult ViewSingleProductsplit()
        {
          
            return View();
        }
        
        public ActionResult ViewSingleProductPartsAdd()
        {
         
            ViewData["classifyList"] = GetManufactor();
            ViewData["SpecificationsList"] = Getclassify();
            return View();
        }
        public string RandomNumber(string head)
        {

            Random rd = new Random();
            string str = "1234567890";
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                result += str[rd.Next(str.Length)];
            }
            string Number = head + result;
            List<T_ProductCodeGenerate> ProductCodeGenerate = db.T_ProductCodeGenerate.Where(a => a.Code == Number).ToList();
            if (ProductCodeGenerate.Count > 0)
            {
                return RandomNumber(head);
            }
            return Number;
        }
        public string RandomNumberGoods(string head)
        {

            Random rd = new Random();
            string str = "1234567890";
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                result += str[rd.Next(str.Length)];
            }
            string Number = head + result;
            List<T_goodsGY> ProductCodeGenerate = db.T_goodsGY.Where(a => a.code == Number).ToList();
            if (ProductCodeGenerate.Count > 0)
            {
                return RandomNumberGoods(head);
            }
            return Number;
        }
        public class MapItem
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public int Qty { get; set; }
        }
        
        public JsonResult getMapItem()
        {
            string name = Server.UrlDecode(Request.Cookies["NickName"].Value);

            //string power = db.T_User.FirstOrDefault(s => s.Nickname == name).Power;
            //int pow = Convert.ToInt32(power);
            //string access = db.T_Role.FirstOrDefault(a => a.ID == pow).Access;
            //筛选出要在桌面上显示的模块
            //要在桌面上添加一个模块的显示
            //step1：在下面这个sql语句的 Name in ('模块名1','模块名2')位置加上该模块的Name
          //  List<MapItem> MapList = db.Database.SqlQuery<MapItem>("select Name ,Url, Qty=0 from T_SysModule where ID in (" + access + ") and Name in ('资金调拨未审核','资金冻结未审核','报损未审核','入职审批未审核','快递赔付待处理','活动未审核')").ToList();
            List<MapItem> MapList = new List<MapItem>();
            for (int x = 0; x < 3;x++ )
            {
                MapItem i = new MapItem();
                if (x == 0)
                {
                    i.Name = "今日返现";
                }
                else if (x == 1)
                {
                    i.Name = "今日退款";
                }
                else
                {
                    i.Name = "今日订单";
                }
                MapList.Add(i);
            }
            DateTime datetime = DateTime.Now;
            foreach (MapItem item in MapList)
            {
                // step2 查询该模块的数据
                if (item.Name == "今日返现")
                {
                    item.Qty = db.T_CashBack.Where(a => a.PostTime >= datetime).Count();
                }
                else if (item.Name == "今日退款")
                {
                    item.Qty = db.T_Retreat.Where(a => a.Retreat_date >= datetime).Count();
                }
                else if (item.Name == "今日订单")
                {
                    item.Qty = db.T_OrderList.Where(a => a.createtime >= datetime).Count();
                
                }
            }
            return Json(MapList);
        }
        /// <summary>
        /// 查询商品详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult ProductPartsPartsDetailList(Lib.GridPager pager, string query, string Code)
        {
            List<T_GoodsGyParts> model = db.T_GoodsGyParts.Where(a => a.PartsCode == Code).ToList();

            string goodsCode = "";
            for (int i = 0; i < model.Count; i++)
            {
                if (i == 0)
                {
                    goodsCode += "'" + model[i].goodsGyCode + "'";
                }
                else
                {
                    goodsCode += "," + "'" + model[i].goodsGyCode + "'";
                }
            }
            string sql = "select * from T_goodsGY where   code in (" + goodsCode + ")";
            IQueryable<T_goodsGY> list = db.Database.SqlQuery<T_goodsGY>(sql).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

        /// <summary>
        /// 查询配件详情
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodsGyPartsList(Lib.GridPager pager, string query,string Code)
        {
            List<T_GoodsGyParts> model = db.T_GoodsGyParts.Where(a=>a.goodsGyCode==Code).ToList();

            string goodsCode = "";
            for (int i = 0; i < model.Count; i++)
            {
                if(i==0)
                {
                    goodsCode += "'"+model[i].PartsCode+"'";
                }
                else
                {
                    goodsCode += "," + "'" + model[i].PartsCode + "'";
                }
            }
            string sql = "select * from T_goodsGY where   code in (" + goodsCode + ")";
            IQueryable<T_goodsGY> list = db.Database.SqlQuery<T_goodsGY>(sql).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 未审核数据
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodssplitCheckList(Lib.GridPager pager, string query)
        {
            IQueryable<T_goodsGY> list = db.T_goodsGY.Where(a => a.TheCode != "" && a.TheCode != null && a.ExamineName != null && a.ExamineName != ""&&a.isexamine=="0");
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 拆分件数据
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodssplitList(Lib.GridPager pager, string query)
        {
            IQueryable<T_goodsGY> list = db.T_goodsGY.Where(a =>a.TheCode!=""&&a.TheCode!=null&&a.ExamineName!=null&&a.ExamineName!="");
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 绑定配件数据
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodsPartsList(Lib.GridPager pager, string query)
        {
            IQueryable<T_goodsGY> list = db.T_goodsGY.Where(a => a.Manufactor != "" && a.Manufactor != null && a.Specifications != null && a.Specifications != "" && a.classify != null && a.classify!="");
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        /// <summary>
        /// 绑定商品数据
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public ContentResult GetGoodsList(Lib.GridPager pager, string query)
        {
            IQueryable<T_goodsGY> list = db.T_goodsGY.Where(a => (a.Manufactor == "" || a.Manufactor == null) && (a.Specifications == null || a.Specifications == "") && (a.classify == null || a.classify == "") && (a.TheCode == null || a.TheCode == "") && (a.ExamineName == null || a.ExamineName == ""));
            if (!string.IsNullOrWhiteSpace(query))
                list = list.Where(s => s.code.Contains(query) || s.name.Contains(query));
            pager.totalRows = list.Count();
            List<T_goodsGY> queryData = list.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(queryData, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }

          /// <summary>
        /// 组合商品保存
        /// </summary>
        /// <param name="model"></param>
        /// <param name="detailList"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public JsonResult ProductCodeGenerateAddSave(T_ProductCodeGenerate model, string detailList)
        {
          
             

            using (TransactionScope sc = new TransactionScope())
            {
                try
                {
                    List<T_ProductCodeGenerateDetails> details = Com.Deserialize<T_ProductCodeGenerateDetails>(detailList);
                    string codetype = "";
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (i == 0)
                        {
                            codetype += details[i].CpCode + "|" + details[i].CpNumber;
                        }
                        else
                        {
                            codetype += "|"+details[i].CpCode + "|" + details[i].CpNumber;
                        }
                    }
                    T_ProductCodeGenerate GenerateModel = db.T_ProductCodeGenerate.SingleOrDefault(a => a.CodeType == codetype);
                    if (GenerateModel != null)
                    {
                        return Json(new { State = "Faile", Message = "该组合已存在" });
                    }
                    string Nickname = Server.UrlDecode(Request.Cookies["Nickname"].Value);
                    model.CodeType = codetype;
                    model.CreateDate = DateTime.Now;
                    model.CreateName = Nickname;
                    db.T_ProductCodeGenerate.Add(model);
                    db.SaveChanges();
                    foreach (var item in details)
                    {
                        item.Oid = model.ID;
                        db.T_ProductCodeGenerateDetails.Add(item);
                    }
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
        //组合商品列表  
        [HttpPost]
        public ContentResult ShowProductCodeGenerateList(Lib.GridPager pager, string queryStr)
        {

            IQueryable<T_ProductCodeGenerate> queryData = db.T_ProductCodeGenerate.AsQueryable();
            if (!string.IsNullOrEmpty(queryStr))
            {
                queryData = queryData.Where(a => (a.Code != null && a.Code.Contains(queryStr) || a.Name != null && a.Name.Contains(queryStr)));
            }
           
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ProductCodeGenerate> list = new List<T_ProductCodeGenerate>();
            foreach (var item in queryData)
            {
                T_ProductCodeGenerate i = new T_ProductCodeGenerate();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);

        }
        //组合商品列表  
        [HttpPost]
        public ContentResult ShowProductCodeGenerateDetailsList(Lib.GridPager pager,int tid)
        {

            IQueryable<T_ProductCodeGenerateDetails> queryData = db.T_ProductCodeGenerateDetails.Where(a => a.Oid == tid).AsQueryable();
        
            pager.totalRows = queryData.Count();
            //分页
            queryData = queryData.OrderByDescending(c => c.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows);
            List<T_ProductCodeGenerateDetails> list = new List<T_ProductCodeGenerateDetails>();
            foreach (var item in queryData)
            {
                T_ProductCodeGenerateDetails i = new T_ProductCodeGenerateDetails();
                i = item;
                list.Add(i);
            }
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
            return Content(json);
        }
        public class OutDetail
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string simpleName { get; set; }
            public string Remarks { get; set; }
            public string CpCode { get; set; }
            public int CpNumber { get; set; }
            public decimal Price { get; set; }
            public double CpWeight { get; set; }
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult OutPutExcel(string name)
        {
            string sql = "";

            if (!string.IsNullOrEmpty(name))
            {
                sql = string.Format(@"select (select Code from T_ProductCodeGenerate where ID=s.oid) Code,(select Name from T_ProductCodeGenerate where ID=s.oid) Name,(select simpleName from T_ProductCodeGenerate where ID=s.oid) simpleName, (select Price  from T_ProductCodeGenerate where ID=s.oid) Price,(select Remarks from T_ProductCodeGenerate where ID=s.oid) Remarks,CpCode,CpNumber,CpWeight from T_ProductCodeGenerateDetails s where (Oid=(select ID from T_ProductCodeGenerate where Code like '%" + name + "%') or  Oid=(select ID from T_ProductCodeGenerate where Name like '%" + name + "%')) ");
            }
            else
            {
                sql = string.Format(@"select (select Code from T_ProductCodeGenerate where ID=s.oid) Code,(select Name from T_ProductCodeGenerate where ID=s.oid) Name,(select simpleName from T_ProductCodeGenerate where ID=s.oid) simpleName, (select Price  from T_ProductCodeGenerate where ID=s.oid) Price,(select Remarks from T_ProductCodeGenerate where ID=s.oid) Remarks,CpCode,CpNumber,CpWeight from T_ProductCodeGenerateDetails s  "); ;
            }
          
            List<OutDetail> list = db.Database.SqlQuery<OutDetail>(sql).ToList();
            //创建Excel文件的对象
            HSSFWorkbook book = new HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            IRow row1 = sheet1.CreateRow(0);
            row1.Height = 3 * 265;
            IFont cfont = book.CreateFont();
            cfont.FontName = "宋体";
            cfont.FontHeight = 1 * 256;
            row1.CreateCell(0).SetCellValue("组合商品代码（必填）");
            row1.CreateCell(1).SetCellValue("组合商品名称（必填）");
            row1.CreateCell(2).SetCellValue("组合商品简称");
            row1.CreateCell(3).SetCellValue("标准单价");
            row1.CreateCell(4).SetCellValue("商品条码");
            row1.CreateCell(5).SetCellValue("商品代码（必填）");
            row1.CreateCell(6).SetCellValue("规格代码");
            row1.CreateCell(7).SetCellValue("数量（必填）");
            row1.CreateCell(8).SetCellValue("权重比（必填）");
            row1.CreateCell(9).SetCellValue("备注");
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 30 * 256);
            sheet1.SetColumnWidth(2, 15 * 256);
            sheet1.SetColumnWidth(3, 15 * 256);
            sheet1.SetColumnWidth(4, 20 * 256);
            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                //rowtemp.Height = 3 * 265;
                //rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].StoreName) ? "" : list[i].StoreName);
                //rowtemp.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                //rowtemp.Cells[0].CellStyle.VerticalAlignment = VerticalAlignment.Center;
                //rowtemp.Cells[0].CellStyle.WrapText = true;
                //rowtemp.Cells[0].CellStyle.GetFont(book).FontName = "宋体";

              //  rowtemp.Cells[0].CellStyle.GetFont(book).FontHeight = 1 * 256;

                rowtemp.CreateCell(0).SetCellValue(string.IsNullOrWhiteSpace(list[i].Code) ? "" : list[i].Code);
                rowtemp.CreateCell(1).SetCellValue(string.IsNullOrWhiteSpace(list[i].Name) ? "" : list[i].Name);
                rowtemp.CreateCell(2).SetCellValue(string.IsNullOrWhiteSpace(list[i].simpleName) ? "" : list[i].simpleName);
                rowtemp.CreateCell(3).SetCellValue(Convert.ToDouble(list[i].Price).ToString("0.00"));
                rowtemp.CreateCell(4).SetCellValue("");
                rowtemp.CreateCell(5).SetCellValue(string.IsNullOrWhiteSpace(list[i].CpCode) ? "" : list[i].CpCode);
                rowtemp.CreateCell(6).SetCellValue("");
                rowtemp.CreateCell(7).SetCellValue(Convert.ToInt32(list[i].CpNumber).ToString("0"));
                rowtemp.CreateCell(8).SetCellValue(Convert.ToDecimal(list[i].CpWeight / 100).ToString("0.00"));
                rowtemp.CreateCell(9).SetCellValue(string.IsNullOrWhiteSpace(list[i].Remarks) ? "" : list[i].Remarks);

            }
            Response.ContentType = "application/vnd.ms-excel;charset=UTF-8";
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Flush();
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", "Reun.xls");
        }

        public JsonResult SingleProductAddSave(T_goodsGY model)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            using (TransactionScope sc = new TransactionScope())
            {
                model.create_date = DateTime.Now;
                model.create_Name = name;
                db.T_goodsGY.Add(model);
                try
                {
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        App_Code.GY gy = new App_Code.GY();
                        string cmd = "";
                        cmd = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"skus\":[]" +
                                    "}";
                        string sign = gy.Sign(cmd);
                        string comcode = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"sign\":\"" + sign + "\"," +
                                "\"skus\":[]" +
                                "}";
                        string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                         JsonData jsonData = null;
                     jsonData = JsonMapper.ToObject(ret);
                    string sd = jsonData[0].ToString();
                    if (sd == "True")
                    {
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile"}, JsonRequestBehavior.AllowGet);
                    }
                 }
                  
                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ViewSingleProductPartsAddSave(T_goodsGY model)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            using (TransactionScope sc = new TransactionScope())
            {

                T_ProductClass ProductClassmodel = db.T_ProductClass.SingleOrDefault(a => a.ClassName == model.classify);
                string ClassCode = ProductClassmodel.ClassCode;
                T_ProductManufactor ProductManufactormodel = db.T_ProductManufactor.SingleOrDefault(a => a.ManufactorName == model.Manufactor);
                string ManufactorCode = ProductManufactormodel.ManufactorCode;
                string Specifications = model.Specifications;

                string code = "S" + ClassCode + ManufactorCode + Specifications;

                List<T_goodsGY> goodsList = db.T_goodsGY.Where(a => a.code == code).ToList() ;
                if (goodsList.Count > 0)
                {
                    return Json(new { State = "Faile", Message = "该组合方式的配件已存在" }, JsonRequestBehavior.AllowGet);
                }
                
                model.code = code;

                model.create_date = DateTime.Now;
                model.create_Name = name;
                db.T_goodsGY.Add(model);
                try
                {
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        App_Code.GY gy = new App_Code.GY();
                        string cmd = "";
                        cmd = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"skus\":[]" +
                                    "}";
                        string sign = gy.Sign(cmd);
                        string comcode = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"sign\":\"" + sign + "\"," +
                                "\"skus\":[]" +
                                "}";
                        string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string sd = jsonData[0].ToString();
                        if (sd == "True")
                        {
                            sc.Complete();
                            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { State = "Faile", Message="保存失败" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { State = "Faile", Message="保存失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ProductPartsAddSave(string Code, string tid)
        {
            string[] listid = Code.Split(',');
            int x = 0;
             string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            for (int i = 0; i < listid.Count(); i++)
            {
                T_GoodsGyParts model = new T_GoodsGyParts();
                model.create_date = DateTime.Now;
                model.create_Name = name;
                model.goodsGyCode = tid;
                model.PartsCode = listid[i];
                db.T_GoodsGyParts.Add(model);
                int s = db.SaveChanges();
                x = x + s;
            }
            if (x > 0)
            {
                return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { State = "Faile", Message = "保存失败" }, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult ViewSingleProductsplitAddSave(T_goodsGY model)
        {
            string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
            using (TransactionScope sc = new TransactionScope())
            {
                string theCode=model.TheCode;
                int splitNumber =int.Parse(model.splitNumber.ToString());
                T_goodsGY GoodsGy = db.T_goodsGY.SingleOrDefault(a => a.TheCode == theCode && a.splitNumber == splitNumber);

                if (GoodsGy != null)
                {
                    return Json(new { State = "Faile", Message = "该产品及数量已拆分过" }, JsonRequestBehavior.AllowGet);
                }

                model.create_date = DateTime.Now;
                model.create_Name = name;
                model.isexamine = "0";
                model.ExamineName = "雨婷";
                model.code = RandomNumberGoods("C");
                db.T_goodsGY.Add(model);
                try
                {
                    int i = db.SaveChanges();
                    if (i >0)
                    {
                        sc.Complete();
                        return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { State = "Faile", Message="保存失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { State = "Faile" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ViewSingleProductsplitCheckSave(string code)
        {
         
            using (TransactionScope sc = new TransactionScope())
            {
                 T_goodsGY model = db.T_goodsGY.SingleOrDefault(a=>a.code==code);
                 model.isexamine = "1";
                 db.Entry<T_goodsGY>(model).State = System.Data.Entity.EntityState.Modified;
                try
                {
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        App_Code.GY gy = new App_Code.GY();
                        string cmd = "";
                        cmd = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"skus\":[]" +
                                    "}";
                        string sign = gy.Sign(cmd);
                        string comcode = "{" +
                                "\"appkey\":\"171736\"," +
                                "\"method\":\"gy.erp.item.add\"," +
                                "\"sessionkey\":\"f5885504d9c84d1d8146200a4841f4b7\"," +
                                "\"code\":\"" + model.code + "\"," +
                                "\"name\":\"" + model.name + "\"," +
                                "\"simple_name\":\"" + model.simple_name + "\"," +
                                "\"sign\":\"" + sign + "\"," +
                                "\"skus\":[]" +
                                "}";
                        string ret = gy.DoPost("http://api.guanyierp.com/rest/erp_open", comcode);
                        JsonData jsonData = null;
                        jsonData = JsonMapper.ToObject(ret);
                        string sd = jsonData[0].ToString();
                        if (sd == "True")
                        {
                            sc.Complete();
                            return Json(new { State = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
                catch (DbEntityValidationException ex)
                {
                    return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { State = "Faile", Message = "审核失败" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
