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
			var repositoryFolder = System.Configuration.ConfigurationManager.AppSettings["repositoryFolder"];
			if (repositoryFolder != null)
			{
				Services.RoutesProvider.Current.Hydrate(repositoryFolder);
			}
		}

		protected void Application_End()
		{
			var repositoryFolder = System.Configuration.ConfigurationManager.AppSettings["repositoryFolder"];
			if (repositoryFolder != null)
			{
				Services.RoutesProvider.Current.Flush(repositoryFolder);
			}
		}
	}
}
