using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMS.Models;
using Newtonsoft.Json;

namespace EBMS.Controllers
{
    public class NCExceptionController : Controller
    {
        //
        // GET: /NCException/
        WDTSoOrderEntities db = new WDTSoOrderEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult returnstorageException()
        {
            return View();
        }

        public ContentResult GetReturnstorageExceptionList(Lib.GridPager pager, string queryStr)
        {
            IQueryable<ebms_returnstorage> queryData = db.ebms_returnstorage.Where(a=>a.IsInsertToNC==2);
            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                queryData = queryData.Where(a => a.expressnumber == queryStr||a.goods_no== queryStr);
            }
            pager.totalRows = queryData.Count();
            List<ebms_returnstorage> list = queryData.OrderByDescending(a => a.ID).Skip((pager.page - 1) * pager.rows).Take(pager.rows).ToList();
            string json = "{\"total\":" + pager.totalRows + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
            return Content(json);
        }

    }
}
