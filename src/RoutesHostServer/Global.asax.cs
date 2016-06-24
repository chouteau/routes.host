using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RoutesHostServer
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
		}
	}
}
