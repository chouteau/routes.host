using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;


namespace RoutesHostServerTests
{
	[TestClass]
	public class RouteProviderTests
	{
		[TestMethod]
		public void Flush_And_Hydrate_Routes()
		{
			var folder = System.Configuration.ConfigurationManager.AppSettings["repositoryFolder"];

			var list = from f in System.IO.Directory.GetFiles(folder, "*.route.json")
					   select f;

			foreach (var file in list)
			{
				System.IO.File.Delete(file);
			}

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";

			RoutesHostServer.Services.RoutesProvider.Current.Register(route);

			RoutesHostServer.Services.RoutesProvider.Current.Flush(folder);

			RoutesHostServer.Services.RoutesProvider.Current.Hydrate(folder);

			var address = RoutesHostServer.Services.RoutesProvider.Current.Resolve(route.ApiKey, route.ServiceName);

			Check.That(address).IsEqualTo(route.WebApiAddress);
		}

	}
}
