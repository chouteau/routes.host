using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace RoutesHostClientTests
{
	[TestClass]
	public class WebApiClientTests
	{
		public string Server
		{
			get
			{
				return System.Configuration.ConfigurationManager.AppSettings["server"];
			}
		}

		[TestMethod]
		public void Register_And_Resolve()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress(Server);

			var route = new RoutesHostClient.Route();
			var apiKey = route.ApiKey = Guid.NewGuid().ToString();
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
			var apiKey = route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "myservice";
			route.WebApiAddress = "http://localhost:1234";

			var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);
			RoutesHostClient.RoutesProvider.Current.RegisterProxy(new RoutesHostClient.ProxyRoute()
			{
				ApiKey = apiKey,
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
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress(Server);

			var baseAddress = "http://localhost:65432/";
			MiniServer.Start(baseAddress);
			var routeServer = new RouteServerTest();
			
			var route = new RoutesHostClient.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "ping";
			route.WebApiAddress = baseAddress;

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

		[TestMethod]
		public void Call_Bad_Service()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress(Server);

			var baseAddress = "http://localhost:65432/";
			MiniServer.Start(baseAddress);
			var routeServer = new RouteServerTest();

			var route = new RoutesHostClient.Route();
			var apiKey = route.ApiKey = "a40e9ae9-1374-4076-ac79-320c80727e27";
			route.ServiceName = "ping"; // <- not exists
			route.WebApiAddress = baseAddress;

			var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);
			var webapiClient = new RoutesHostClient.WebApiClient(apiKey, "ping");

			var content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping2").Result;
				return result;
			}, true);

			Check.That(content).IsNull();
			Check.That(webapiClient.LastException).IsNotNull();
			MiniServer.Stop();

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);
		}


		[TestMethod]
		public void Call_Fail_Route_And_Use_Alternative_Route()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress(Server);

			string baseAddress = "http://localhost:65431/";
			MiniServer.Start(baseAddress);
			var routeServer = new RouteServerTest();

			string apiKey = "07444299-0489-48cc-9d40-889a9ffc9b37";
			RoutesHostClient.RoutesProvider.Current.UnRegisterService(apiKey, "ping");

			var route1 = new RoutesHostClient.Route();
			route1.ApiKey = apiKey;
			route1.ServiceName = "ping";
			route1.Priority = 2;
			route1.WebApiAddress = "http://localhost:65432/";

			var routeId1 = RoutesHostClient.RoutesProvider.Current.Register(route1);

			var webapiClient = new RoutesHostClient.WebApiClient(apiKey, "ping");
			var content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping").Result;
				return result;
			}, true);

			Check.That(content).IsNull();

			var route2 = new RoutesHostClient.Route();
			route2.ApiKey = apiKey;
			route2.ServiceName = "ping";
			route2.Priority = 1;
			route2.WebApiAddress = baseAddress;

			var routeId2 = RoutesHostClient.RoutesProvider.Current.Register(route2);

			content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping").Result;
				return result;
			}, true);

			Check.That(content).IsNotNull();

			MiniServer.Stop();

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId1);
			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId2);
		}

		[TestMethod]
		public void Call_Service_With_Bad_Route_And_Alternative_Route()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress(Server);

			string baseAddress = "http://localhost:65431/";
			MiniServer.Start(baseAddress);
			var routeServer = new RouteServerTest();

			string apiKey = "06a72795-82b5-41b5-955a-8e3b923ead2f";
			RoutesHostClient.RoutesProvider.Current.UnRegisterService(apiKey, "ping");

			var badRoute = new RoutesHostClient.Route();
			badRoute.ApiKey = apiKey;
			badRoute.ServiceName = "ping";
			badRoute.Priority = 2;
			badRoute.WebApiAddress = "http://localhost:65432/";

			var routeId1 = RoutesHostClient.RoutesProvider.Current.Register(badRoute);

			var goodRoute = new RoutesHostClient.Route();
			goodRoute.ApiKey = apiKey;
			goodRoute.ServiceName = "ping";
			goodRoute.Priority = 1;
			goodRoute.WebApiAddress = baseAddress;

			var routeId2 = RoutesHostClient.RoutesProvider.Current.Register(goodRoute);

			var webapiClient = new RoutesHostClient.WebApiClient(apiKey, "ping");
			var content = webapiClient.ExecuteRetry<object>(client =>
			{
				var result = client.GetAsync("api/pingservice/ping").Result;
				return result;
			}, true);

			Check.That(content).IsNotNull();

			MiniServer.Stop();

			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId1);
			RoutesHostClient.RoutesProvider.Current.UnRegister(routeId2);
		}

		[TestMethod]
		public void Resolve_Route()
		{
			RoutesHostClient.GlobalConfiguration.Configuration.AddAddress("http://route1.creastore.pro");
			var resolve = RoutesHostClient.RoutesProvider.Current.Resolve("74c967f8-d5c6-40c6-a069-bc36e634661e", "CreaStore.Services.ITrackerService");
		}

	}
}
