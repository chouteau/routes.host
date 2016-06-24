using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace RoutesHostClientTests
{
	public static class MiniServer
	{
		private static IDisposable m_Host;

		public static void Start()
		{
			m_Host = Microsoft.Owin.Hosting.WebApp.Start("http://localhost:65432", (appBuilder) =>
			{
				var config = new HttpConfiguration();

				config.MapHttpAttributeRoutes();

				appBuilder.UseWebApi(config);
			});
		}

		public static void Stop()
		{
			m_Host.Dispose();
		}
	}
}
