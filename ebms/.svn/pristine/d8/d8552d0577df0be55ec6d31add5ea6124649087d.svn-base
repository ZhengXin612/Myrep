using EBMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    public class SycmController : BaseController
    {
        EBMSEntities db = new EBMSEntities();

        //图形报表
        class MyChart
        {

            public DateTime x { get; set; }//日期
            public int y { get; set; }//我方数量
            public int y1 { get; set; }//对方数量   
            public int z { get; set; }//我方数量=》时间段内
            public int z1 { get; set; }//对方数量=》时间段内
            public int AVG { get; set; }//我方数量=》平均值
            public int AVG1 { get; set; }//对方数量=》平均值
            public int day { get; set; }//选择了几天的
        }
        class MyChartFoot
        {
            public string x { get; set; }
            public int y { get; set; }
            public int y1 { get; set; }
        }
        class DateTimeFootss
        {
            public DateTime start { get; set; }
            public DateTime end { get; set; }

        }

        public ActionResult SalesCompare(string shopname = "", string goodscode = "", string mycode = "", string start = "")
        {
            List<SelectListItem> shop_list = new List<SelectListItem>();
            List<SelectListItem> goods_list = new List<SelectListItem>();
            List<SelectListItem> hhsgoods = new List<SelectListItem>();
            IQueryable<string> shoplist = db.T_SYCM_Data.Where(a => !a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.ShopName).Select(a => a.Key);

            foreach (var item in shoplist)
            {
                SelectListItem i = new SelectListItem();
                i.Text = item;
                i.Value = item;
                shop_list.Add(i);

            };
            IQueryable<string> goodslist = db.T_SYCM_Data.Where(a => !a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.GoodsName).Select(a => a.Key);

            foreach (var item in goodslist)
            {
                SelectListItem i = new SelectListItem();
                i.Text = item;
                i.Value = item;
                goods_list.Add(i);

            };

            //好护士的商品下拉框
            IQueryable<string> hhsgoodslist = db.T_SYCM_Data.Where(a => a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.GoodsName).Select(a => a.Key);

            foreach (var item in hhsgoodslist)
            {
                SelectListItem i = new SelectListItem();
                i.Text = item;
                i.Value = item;
                hhsgoods.Add(i);

            };
            ViewData["shoplist"] = shop_list;
            ViewData["goodslist"] = goods_list;
            ViewData["hhsgoods"] = hhsgoods;
            ViewData["x"] = "";
            ViewData["y"] = "";
            ViewData["y1"] = "";

            if (shopname != "")
            {
                List<DateTimeFootss> DateTimeFootssModel = new List<DateTimeFootss>();
                string end = "";
                if (string.IsNullOrEmpty(start))
                {
                    start = DateTime.Now.ToString("yyyy-MM-dd");
                    end = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                }
                else
                {
                    DateTime startdates = DateTime.Parse(start);
                    start = startdates.AddDays(-10).ToString("yyyy-MM-dd");
                    end = startdates.AddDays(-30).ToString("yyyy-MM-dd");
                }

                DateTime startdate = DateTime.Parse(start);
                DateTime enddate = DateTime.Parse(end);
                var s = 29;
                for (int i = 0; i <= 29; i++)
                {
                    DateTimeFootss MyCharss = new DateTimeFootss();
                    MyCharss.start = startdate.AddDays(-s).Date;
                    string ss = MyCharss.start.ToString().Substring(0, 10) + " 23:59:59";
                    MyCharss.start = DateTime.Parse(ss);
                    MyCharss.end = enddate.AddDays(-s);
                    DateTimeFootssModel.Add(MyCharss);

                    s--;
                }
                List<MyChart> model = new List<MyChart>();

                for (int i = 0; i < DateTimeFootssModel.Count; i++)
                {


                    string sql = "select SUM(y) as y,SUM(y1) as y1 from (select cast(DayDate as date)  as x,Qty as y from T_SYCM_Data where ShopName='好护士器械旗舰店' and GoodsName='" + mycode + "' " +
                           ") as a join (select cast(DayDate as date)  as x1,Qty as y1 from T_SYCM_Data where ShopName='冠昌医疗器械旗舰店' and GoodsName='" + goodscode + "' " +
                          ") as b on cast(a.x as date) between '" + DateTimeFootssModel[i].end + "' and '" + DateTimeFootssModel[i].start + "' and a.x=b.x1 ";
                    MyChart data = db.Database.SqlQuery<MyChart>(sql, "").SingleOrDefault();

                    ViewData["x"] += "'" + DateTimeFootssModel[i].start.Date.ToString("yyyy-MM-dd") + "',";
                    ViewData["y"] += data.y + ",";
                    ViewData["y1"] += data.y1 + ",";

                    //if (ViewData["x"].ToString().Length > 0 && ViewData["y"].ToString().Length > 0)
                    //{
                    // ViewData["x"] = ViewData["x"].ToString().Substring(0, ViewData["x"].ToString().Length - 1);
                    // ViewData["y"] = ViewData["y"].ToString().Substring(0, ViewData["y"].ToString().Length - 1);
                    // ViewData["y1"] = ViewData["y1"].ToString().Substring(0, ViewData["y1"].ToString().Length - 1);
                    //}
                }
                return View();
            }
            else
            {
                return View();
            }
        }
        //public ActionResult SalesCompare(string shopname = "", string goodscode = "", string mycode = "", string start = "")
        //{
        //    List<SelectListItem> shop_list = new List<SelectListItem>();
        //    List<SelectListItem> goods_list = new List<SelectListItem>();
        //    List<SelectListItem> hhsgoods = new List<SelectListItem>();
        //    IQueryable<string> shoplist = db.T_SYCM_Data.Where(a => !a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.ShopName).Select(a => a.Key);

        //    foreach (var item in shoplist)
        //    {
        //        SelectListItem i = new SelectListItem();
        //        i.Text = item;
        //        i.Value = item;
        //        shop_list.Add(i);

        //    };
        //    IQueryable<string> goodslist = db.T_SYCM_Data.Where(a => !a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.GoodsName).Select(a => a.Key);

        //    foreach (var item in goodslist)
        //    {
        //        SelectListItem i = new SelectListItem();
        //        i.Text = item;
        //        i.Value = item;
        //        goods_list.Add(i);

        //    };

        //    //好护士的商品下拉框
        //    IQueryable<string> hhsgoodslist = db.T_SYCM_Data.Where(a => a.ShopName.Equals("好护士器械旗舰店")).GroupBy(a => a.GoodsName).Select(a => a.Key);

        //    foreach (var item in hhsgoodslist)
        //    {
        //        SelectListItem i = new SelectListItem();
        //        i.Text = item;
        //        i.Value = item;
        //        hhsgoods.Add(i);

        //    };
        //    ViewData["shoplist"] = shop_list;
        //    ViewData["goodslist"] = goods_list;
        //    ViewData["hhsgoods"] = hhsgoods;
        //    ViewData["x"] = "";
        //    ViewData["y"] = "";
        //    ViewData["y1"] = "";

        //    if (shopname != "")
        //    {
        //        List<DateTimeFootss> DateTimeFootssModel = new List<DateTimeFootss>();
        //        string end = "";
        //        if (string.IsNullOrEmpty(start))
        //        {
        //            start = DateTime.Now.ToString("yyyy-MM-dd");
        //            end = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        //        }
        //        else
        //        {
        //            DateTime startdates = DateTime.Parse(start);
        //            start = startdates.AddDays(-10).ToString("yyyy-MM-dd");
        //            end = startdates.AddDays(-30).ToString("yyyy-MM-dd");
        //        }

        //        DateTime startdate = DateTime.Parse(start);
        //        DateTime enddate = DateTime.Parse(end);
        //        var s = 29;
        //        for (int i = 0; i <= 29; i++)
        //        {
        //            DateTimeFootss MyCharss = new DateTimeFootss();
        //            MyCharss.start = startdate.AddDays(-s).Date;
        //            string ss = MyCharss.start.ToString().Substring(0, 10) + " 23:59:59";
        //            MyCharss.start = DateTime.Parse(ss);
        //            MyCharss.end = enddate.AddDays(-s);
        //            DateTimeFootssModel.Add(MyCharss);

        //            s--;
        //        }
        //        List<MyChart> model = new List<MyChart>();

        //        for (int i = 0; i < DateTimeFootssModel.Count; i++)
        //        {


        //            string sql = "select SUM(y) as y,SUM(y1) as y1 from (select cast(DayDate as date)  as x,Qty as y from T_SYCM_Data where ShopName='好护士器械旗舰店' and GoodsName='" + mycode + "' " +
        //                   ") as a join (select cast(DayDate as date)  as x1,Qty as y1 from T_SYCM_Data where ShopName='冠昌医疗器械旗舰店' and GoodsName='" + goodscode + "' " +
        //                  ") as b on cast(a.x as date) between '" + DateTimeFootssModel[i].end + "' and '" + DateTimeFootssModel[i].start + "' and a.x=b.x1 ";
        //            MyChart data = db.Database.SqlQuery<MyChart>(sql, "").SingleOrDefault();

        //            ViewData["x"] += "'" + DateTimeFootssModel[i].start.Date.ToString("yyyy-MM-dd") + "',";
        //            ViewData["y"] += data.y + ",";
        //            ViewData["y1"] += data.y1 + ",";

        //            //if (ViewData["x"].ToString().Length > 0 && ViewData["y"].ToString().Length > 0)
        //            //{
        //            // ViewData["x"] = ViewData["x"].ToString().Substring(0, ViewData["x"].ToString().Length - 1);
        //            // ViewData["y"] = ViewData["y"].ToString().Substring(0, ViewData["y"].ToString().Length - 1);
        //            // ViewData["y1"] = ViewData["y1"].ToString().Substring(0, ViewData["y1"].ToString().Length - 1);
        //            //}
        //        }
        //        return View();
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

    }
}
