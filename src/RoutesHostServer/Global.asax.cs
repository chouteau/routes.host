﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Ariane;

namespace RoutesHostServer
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
			var topicName = System.Configuration.ConfigurationManager.AppSettings["TopicName"];

			// Services.RouteSynchronizer.Current.Bus.Register.AddAzureTopicWriter("RoutesHostAction");
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["RoutesHostAction"];
			if (cs != null)
			{
				Services.RouteSynchronizer.Current.Bus.Register.AddAzureTopicReader("RoutesHostAction", topicName, typeof(Services.RouteMessageReader));
				Services.RouteSynchronizer.Current.Bus.StartReading();
			}

			var repositoryFolder = System.Configuration.ConfigurationManager.AppSettings["repositoryFolder"];
			if (repositoryFolder != null)
			{
				Services.RoutesProvider.Current.Hydrate(repositoryFolder);
			}

			RouteConfig.RegisterRoutes(RouteTable.Routes);
			ViewEngines.Engines.Add(new AppViewEngine());
		}

		protected void Application_End()
		{
			var repositoryFolder = System.Configuration.ConfigurationManager.AppSettings["repositoryFolder"];
			if (repositoryFolder != null)
			{
				try
				{
					Services.RoutesProvider.Current.Flush(repositoryFolder);
				}
				catch(Exception ex)
				{
					System.Diagnostics.EventLog.WriteEntry("Application", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
				}
			}

			Services.RouteSynchronizer.Current.Bus.StopReading();
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			try
			{
				var ex = Context.Server.GetLastError();
				Services.Logger.Writer.Error(ex);
			}
			catch (Exception ex)
			{
				System.Diagnostics.EventLog.WriteEntry("Application", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
		}

	}
}
