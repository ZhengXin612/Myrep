using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HHSOA.Content
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            string code = context.Request.QueryString["code"];
            if (!string.IsNullOrEmpty(code))
            {
                Lib.BarCode bar = new Lib.BarCode(true);
                bar.DrawingBarCode39(code,9, true);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}