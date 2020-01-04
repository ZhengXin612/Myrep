using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EBMS.Controllers
{
    /// <summary>
    /// 退货退款
    /// </summary>
    public class RefundController : BaseController
    {
        //
        // GET: /Refund/

        public ActionResult Index()
        {
            return View();
        }

    }
}
