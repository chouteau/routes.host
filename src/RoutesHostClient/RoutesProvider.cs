using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
    public class RoutesProvider 
	{
		private static Lazy<RoutesProvider> m_LazyInstance = new Lazy<RoutesProvider>(() =>
		{
			return new RoutesProvider();
		}, true);

		private Dictionary<string, string> m_Cache;

		private RoutesProvider()
		{
			m_Cache = new Dictionary<string, string>();
			RouteServer = GlobalConfiguration.Configuration.RouteServer;
			TestMode = false;
		}

		protected IRoutesServer RouteServer { get; private set; }

		public static RoutesProvider Current
		{
			get
			{
				return m_LazyInstance.Value;
			}
		}

		public bool TestMode { get; set; }

		public Guid Register(Route route)
		{
			if (TestMode)
			{
				return Guid.NewGuid();
			}
			var result = RouteServer.Register(route);
			GlobalConfiguration.Configuration.Logger.Info($"Route for service {route.ServiceName} registered with address {route.WebApiAddress}");
			return result;
		}

		public void UnRegister(Guid routeId)
		{
			if (TestMode)
			{
				return;
			}
			RouteServer.UnRegister(routeId);
			GlobalConfiguration.Configuration.Logger.Info($"Route for service {routeId} was unregistered");
		}

		public string Resolve(string apiKey, string serviceName)
		{
			if (TestMode)
			{
				return null;
			}
			var key = $"{apiKey}|{serviceName}";
			if (m_Cache.ContainsKey(key))
			{
				return m_Cache[key];
			}

			var result = RouteServer.Resolve(apiKey, serviceName);
			if (result != null)
			{
				if (!m_Cache.ContainsKey(key))
				{
					m_Cache.Add(key, result);
				}
			}

			return result;
		}

		internal void RemoveCache(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}";
			if (!m_Cache.ContainsKey(key))
			{
				return;
			}
			m_Cache.Remove(key);
		}

		public void RegisterPing(string uri)
		{

		}
	}
}
