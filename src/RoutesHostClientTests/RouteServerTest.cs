using RoutesHostClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClientTests
{
	public class RouteServerTest : RoutesHostClient.IRoutesServer
	{
		public RouteServerTest()
		{
		}


		public void Register(Route clientRoute)
		{
			var serverRoute = new RoutesHostServer.Models.Route();
			serverRoute.ApiKey = clientRoute.ApiKey;
			serverRoute.Priority = clientRoute.Priority;
			serverRoute.ServiceName = clientRoute.ServiceName;
			serverRoute.WebApiAddress = clientRoute.WebApiAddress;
			RoutesHostServer.Services.RoutesProvider.Current.Register(serverRoute);
		}

		public string Resolve(string apiKey, string serviceName)
		{
			return RoutesHostServer.Services.RoutesProvider.Current.Resolve(apiKey, serviceName);
		}

		public void UnRegister(string routeId)
		{
			RoutesHostServer.Services.RoutesProvider.Current.UnRegister(routeId);
		}
	}
}
