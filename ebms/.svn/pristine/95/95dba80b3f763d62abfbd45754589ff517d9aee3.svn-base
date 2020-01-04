using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using System.Transactions;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Data.Entity.Infrastructure;

namespace EBMS.Controllers
{
    public class PrintController : Controller
    {
		//
		// GET: /Print/
		EBMS.Models.EBMSEntities db = new Models.EBMSEntities();
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult ViewPrintReturn()
		{
			return View();
		}

		public ActionResult ViewPrintRecord()
		{
			return View();
		} 

		public JsonResult PrintReturn(string ISqualified)
		{
			string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
		
			using (TransactionScope  sc=new TransactionScope())
			{
				
				if (ISqualified == "合格")//合格数据
				{
					T_ReturnPrint print = new T_ReturnPrint()
					{
						PostTime = DateTime.Now,
						PostUser = name,
						PrintNO = "POA" + DateTime.Now.Ticks,
						PrintType = "合格"
					};
					string PrintNO = print.PrintNO;
					db.T_ReturnPrint.Add(print);
					var returntostorageDetails = db.ReturnPrintQualifiedData(name).ToList();
					foreach (var item in returntostorageDetails)
					{
						string goods_no = item.item_code;
						
						T_ReturnPrintDetail detail = new T_ReturnPrintDetail()
						{
							PrintNO= print.PrintNO,
							ProductCode= goods_no,
							ProductName=item.item_name,
							Qty=item.qty,
							SpecName="",
							UnitName=""
						};
						T_WDTGoods goods = db.T_WDTGoods.FirstOrDefault(a => a.goods_no == goods_no);
						if (goods != null)
						{
							detail.SpecName = goods.spec_name;
							detail.UnitName = goods.unit_name;
						}
						db.T_ReturnPrintDetail.Add(detail);
					}
					string sql = "update  T_ReturnToStoragelet set PrintNO_qualified='" + print.PrintNO + "' where PrintNO_qualified is null and Pid in ( select ID from T_ReturnToStorage where SortingName='" + name + "') ";
					//	db.T_ReturnToStoragelet.SqlQuery(sql);
					try
					{
						db.Database.ExecuteSqlCommand(sql);

						db.SaveChanges();

						List<T_ReturnPrintDetail> printDetails = db.T_ReturnPrintDetail.
							Where(a => a.PrintNO == PrintNO).ToList();
						sc.Complete();
						return Json(new { state = "Success", rows = printDetails, printInfo = print });

					}
					catch (DbUpdateException e)
					{
						return Json(new { state = "Fail", rows = "", printInfo = print });
					}
					

				}

				else //不合格数据
				{

					T_ReturnPrint print = new T_ReturnPrint()
					{
						PostTime = DateTime.Now,
						PostUser = name,
						PrintNO = "POB" + DateTime.Now.Ticks,
						PrintType="不合格"
					};
					string PrintNO = print.PrintNO;
					db.T_ReturnPrint.Add(print);
					var returntostorageDetails = db.ReturnPrintUnqualifiedData(name).ToList();
					string sql = "update  T_ReturnToStoragelet set PrintNO_unqualified='" + print.PrintNO + "' where PrintNO_unqualified is null Pid in ( select ID from T_ReturnToStorage where SortingName='" + name + "') ";
					//	db.T_ReturnToStoragelet.SqlQuery(sql);

					db.Database.ExecuteSqlCommand(sql);
					foreach (var item in returntostorageDetails)
					{
						string goods_no = item.item_code;
						T_ReturnPrintDetail detail = new T_ReturnPrintDetail()
						{
							PrintNO = PrintNO,
							ProductCode = goods_no,
							ProductName = item.item_name,
							Qty = item.qty,
							SpecName = "",
							UnitName = ""
						};
						T_WDTGoods goods = db.T_WDTGoods.FirstOrDefault(a => a.goods_no == goods_no);
						if (goods != null)
						{
							detail.SpecName = goods.spec_name;
							detail.UnitName = goods.unit_name;
						}
						db.T_ReturnPrintDetail.Add(detail);
					}
					db.SaveChanges();
				
					List<T_ReturnPrintDetail> printDetails = db.T_ReturnPrintDetail.
						Where(a => a.PrintNO == PrintNO).ToList();
					sc.Complete();
					return Json(new { state = "Success", rows = printDetails, printInfo= print });
				}
				

			}
		



		}

		public JsonResult RePrint(string printNO)
		{
			List<T_ReturnPrintDetail> printDetails = db.T_ReturnPrintDetail.
						Where(a => a.PrintNO == printNO).ToList();
			T_ReturnPrint print = db.T_ReturnPrint.FirstOrDefault(a => a.PrintNO == printNO);
			return Json(new { state = "Success", rows = printDetails, printInfo = print }, JsonRequestBehavior.AllowGet);
		}

		public class ReturnPrintModel
		{
			public string item_code { get; set; }
			public string item_name { get; set; }
			public int? qty { get; set; }
		}
		public JsonResult GetPrintReturn(Lib.GridPager pager,string ISqualified)
		{
			try
			{
				string name = Server.UrlDecode(Request.Cookies["Nickname"].Value);
				List<int> PidList = db.T_ReturnToStorage.Where(a => a.SortingName == name).Select(a => a.ID).ToList();

				
				if (ISqualified == "合格")//合格数据
				{
					List< ReturnPrintQualifiedData_Result > list= db.ReturnPrintQualifiedData(name).ToList();
					pager.totalRows = list.Count;

					 list = list.OrderByDescending(s => s.item_code).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
					return Json(new { total = pager.totalRows, rows = list }, JsonRequestBehavior.AllowGet);
				}
				else
				{
					List<ReturnPrintUnqualifiedData_Result> list = db.ReturnPrintUnqualifiedData(name).ToList();
					pager.totalRows = list.Count;

					list = list.OrderByDescending(s => s.item_code).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
					return Json(new { total = pager.totalRows, rows = list }, JsonRequestBehavior.AllowGet);
				}


				

				//string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list, Lib.Comm.setTimeFormat()) + "}";
				
			}
			catch (Exception e)
			{
				return Json(new { total =0 }, JsonRequestBehavior.AllowGet);
			}
			
		}

		public JsonResult GetPrintRecord(Lib.GridPager pager, string ISqualified,string queryStr)
		{
			IQueryable<T_ReturnPrint> queryData = db.T_ReturnPrint;
			if (!string.IsNullOrWhiteSpace(queryStr))
			{
				queryData = queryData.Where(a => a.PrintNO.Contains(queryStr)||a.PostUser.Contains(queryStr));
			}
			if (!string.IsNullOrWhiteSpace(ISqualified))
			{
				queryData = queryData.Where(a => a.PrintType == ISqualified);
			}
			pager.totalRows = queryData.Count();
			List<T_ReturnPrint> list = queryData.OrderByDescending(s => s.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
			return Json(new { total = pager.totalRows, rows = list }, JsonRequestBehavior.AllowGet);
		}



	}
}
