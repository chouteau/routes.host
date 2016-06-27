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
			RoutesHostClient.GlobalConfiguration.Configuration.RouteServer = new RouteServerTest();

			var route = new RoutesHostClient.Route();
			route.ApiKey = "test";
			route.ServiceName = "myservice";
			route.WebApiAddress = "http://localhost:1234";

			RoutesHostClient.RoutesProvider.Current.Register(route);

			var address = RoutesHostClient.RoutesProvider.Current.Resolve(route.ApiKey, route.ServiceName);

			Check.That(address).IsEqualTo(route.WebApiAddress);
		}

		[TestMethod]
		public void Call_Service()
		{
			MiniServer.Start();
			RoutesHostClient.GlobalConfiguration.Configuration.RouteServer = new RouteServerTest();
			
			var route = new RoutesHostClient.Route();
			route.ApiKey = "test";
			route.ServiceName = "ping";
			route.WebApiAddress = "http://localhost:65432/";

			RoutesHostClient.RoutesProvider.Current.Register(route);

			var webapiClient = new RoutesHostClient.WebApiClient("test", "ping");

			var content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping").Result;
				return result;
			}, true);

			Check.That(content).IsNotNull();
			MiniServer.Stop();
		}
	}
}
