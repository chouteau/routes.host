using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutesHost.Models;

namespace RoutesHostClientTests
{
	public class RouteServerTest : RoutesHostClient.IRoutesServer
	{
		public RouteServerTest()
		{
		}


		public void Register(Route route)
		{
			RoutesHostServer.Services.RoutesProvider.Current.Register(route);
		}

		public string Resolve(string apiKey, string serviceName)
		{
			return RoutesHostServer.Services.RoutesProvider.Current.Resolve(apiKey, serviceName);
		}

		public void UnRegister(string apiKey, string serviceName)
		{
			RoutesHostServer.Services.RoutesProvider.Current.UnRegister(apiKey, serviceName);
		}
	}
}
