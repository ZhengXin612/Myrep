using log4net;
using System.Web;
using System.Web.Mvc;

namespace EBMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
			filters.Add(new ExceptionAttribute());
		}
		
    }

	public class ExceptionAttribute : HandleErrorAttribute
	{
		ILog log = LogManager.GetLogger(typeof(ExceptionAttribute));
		public override void OnException(ExceptionContext filterContext)
		{
			if (!filterContext.ExceptionHandled)
			{
				string message = string.Format("消息类型：{0}\r\n消息内容：{1}\r\n引发异常的方法：{2}\r\n引发异常源：{3}"
					, filterContext.Exception.GetType().Name
					, filterContext.Exception.Message
					 , filterContext.Exception.TargetSite
					 , filterContext.Exception.Source + filterContext.Exception.StackTrace
					 );

				//记录日志
				LogHelper.Default.WriteError(message);
				////转向
				//filterContext.ExceptionHandled = true;
				//filterContext.Result = new RedirectResult("/Common/Error");
			}
			base.OnException(filterContext);
		}
	}
}