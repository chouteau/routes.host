using RoutesHostClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClientTests
{
	public class RouteServerTest 
	{
		public RouteServerTest()
		{
		}


		public Guid Register(Route clientRoute)
		{
			var serverRoute = new RoutesHostServer.Models.Route();
			serverRoute.ApiKey = clientRoute.ApiKey;
			serverRoute.Priority = clientRoute.Priority;
			serverRoute.ServiceName = clientRoute.ServiceName;
			serverRoute.WebApiAddress = clientRoute.WebApiAddress;
			serverRoute.PingPath = clientRoute.PingPath;
			serverRoute.MachineName = clientRoute.MachineName;
			var result = RoutesHostServer.Services.RoutesProvider.Current.Register(serverRoute);
			return result;
		}

		public string Resolve(string apiKey, string serviceName)
		{
			return RoutesHostServer.Services.RoutesProvider.Current.Resolve(apiKey, serviceName);
		}

		public void UnRegister(Guid routeId)
		{
			RoutesHostServer.Services.RoutesProvider.Current.UnRegister(routeId);
		}
	}
}
