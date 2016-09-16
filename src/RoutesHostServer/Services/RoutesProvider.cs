using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Services
{
	public class RoutesProvider
	{
		public static Lazy<RoutesProvider> m_Instance = new Lazy<RoutesProvider>(() =>
		{
			return new RoutesProvider();
		}, true);

		internal ConcurrentDictionary<string, List<Models.Route>> RoutesRepository { get; private set; }

		private RoutesProvider()
		{
			RoutesRepository = new ConcurrentDictionary<string, List<Models.Route>>();
		}
		
		public static RoutesProvider Current
		{
			get
			{
				return m_Instance.Value;
			}
		}

		public Guid Register(Models.Route item)
		{
			var key = $"{item.ApiKey}|{item.ServiceName}".ToLower();
			var existing = RoutesRepository.ContainsKey(key);
			Guid id = Guid.Empty;
			if (existing)
			{
				List<Models.Route> routes = null;
				Retry((list) =>
				{
					var result = RoutesRepository.TryGetValue(key, out routes);
					if (result)
					{
						var route = routes.SingleOrDefault(i => $"{i.ApiKey}|{i.ServiceName}".Equals(key, StringComparison.InvariantCultureIgnoreCase) && i.Priority == item.Priority);
						if (route == null)
						{
							id = item.Id = Guid.NewGuid();
							routes.Add(item);
						}
					}
					return result;
				});
				return id;
			}
			item.CreationDate = DateTime.Now;
			Retry((list) =>
			{
				var routes = new List<Models.Route>();
				id = item.Id = Guid.NewGuid();
				routes.Add(item);
				var result = RoutesRepository.TryAdd(key, routes);
				return result;
			});

			return id;
		}

		public void UnRegister(Guid routeId)
		{
			foreach (var key in RoutesRepository.Keys)
			{
				List<Models.Route> routes = null;
				Retry((list) =>
				{
					var result = RoutesRepository.TryGetValue(key, out routes);
					if (result)
					{
						var route = routes.SingleOrDefault(i => i.Id == routeId);
						if (route != null)
						{
							routes.Remove(route);
						}
					}
					if (routes.Count == 0)
					{
						result = RoutesRepository.TryRemove(key, out routes);
						return result;
					}
					return true;
				});
			}
		}

		public void UnRegisterService(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}".ToLower();
			var exists = RoutesRepository.ContainsKey(key);
			if (!exists)
			{
				return;
			}

			List<Models.Route> routes = null;
			Retry((list) =>
			{
				var result = RoutesRepository.TryRemove(key, out routes);
				return result;
			});
		}

		public string Resolve(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}".ToLower();
			var exists = RoutesRepository.ContainsKey(key);
			if (!exists)
			{
				return null;
			}
			List<Models.Route> routes = null;
			Retry((list) =>
			{
				var result = list.TryGetValue(key, out routes);
				return result;
			});
			if (routes != null)
			{
				var item = routes.OrderBy(i => i.Priority).FirstOrDefault();
				return item.WebApiAddress;
			}
			return null;
		}

		private void Retry(Func<ConcurrentDictionary<string, List<Models.Route>>, bool> predicate, int retryCount = 3)
		{
			var loop = 0;
			while (true)
			{
				var result = predicate.Invoke(RoutesRepository);
				if (result || loop > retryCount)
				{
					break;
				}
				loop++;
				System.Threading.Thread.Sleep(500);
			}
		}
	}
}