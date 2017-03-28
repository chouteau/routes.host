using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace RoutesHostClientTests
{
	[TestClass]
	public class WebApiClientTests
	{
		[TestMethod]
		public void Register_And_Resolve()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress("http://routes.host");

			var route = new RoutesHostClient.Route();
			route.ApiKey = "test";
			route.ServiceName = "myservice";
			route.WebApiAddress = "http://localhost:1234";

			var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);

			var address = RoutesHostClient.RoutesProvider.Current.Resolve(route.ApiKey, route.ServiceName);

			Check.That(address).IsEqualTo(route.WebApiAddress);

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);
		}

		[TestMethod]
		public void Register_Proxy_Resolve()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress("http://route2.creastore.pro");

			var route = new RoutesHostClient.Route();
			route.ApiKey = "test";
			route.ServiceName = "myservice";
			route.WebApiAddress = "http://localhost:1234";

			var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);
			RoutesHostClient.RoutesProvider.Current.RegisterProxy(new RoutesHostClient.ProxyRoute()
			{
				ApiKey = "test",
				ServiceName = "myservice",
				WebApiAddress = "http://proxy:1234"
			});

			RoutesHostClient.GlobalConfiguration.Configuration.UseProxy = true;
			var address = RoutesHostClient.RoutesProvider.Current.Resolve(route.ApiKey, route.ServiceName);

			Check.That(address).IsEqualTo("http://proxy:1234");

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);
		}


		[TestMethod]
		public void Call_Service()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress("http://routes.host");

			MiniServer.Start();
			var routeServer = new RouteServerTest();
			
			var route = new RoutesHostClient.Route();
			route.ApiKey = "test";
			route.ServiceName = "ping";
			route.WebApiAddress = "http://localhost:65432/";

			var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);
			var webapiClient = new RoutesHostClient.WebApiClient("test", "ping");

			var content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping").Result;
				return result;
			}, true);

			Check.That(content).IsNotNull();
			MiniServer.Stop();

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);
		}
	}
}
