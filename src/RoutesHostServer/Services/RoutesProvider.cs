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

		protected ConcurrentDictionary<string, RoutesHost.Models.Route> RoutesRepository { get; private set; }

		private RoutesProvider()
		{
			RoutesRepository = new ConcurrentDictionary<string, RoutesHost.Models.Route>();
		}
		
		public static RoutesProvider Current
		{
			get
			{
				return m_Instance.Value;
			}
		}

		public void Register(RoutesHost.Models.Route item)
		{
			var key = $"{item.ApiKey}|{item.ServiceName}";
			var existing = RoutesRepository.ContainsKey(key);
			if (existing)
			{
				return;
			}
			item.CreationDate = DateTime.Now;
			Retry((list) =>
			{
				var result = RoutesRepository.TryAdd(key, item);
				return result;
			});
		}

		public void UnRegister(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}";
			var exists = RoutesRepository.ContainsKey(key);
			if (!exists)
			{
				return;
			}
			RoutesHost.Models.Route item = null;
			Retry((list) =>
			{
				var result = RoutesRepository.TryRemove(key, out item);
				return result;
			});
		}

		public string Resolve(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}";
			var exists = RoutesRepository.ContainsKey(key);
			if (!exists)
			{
				return null;
			}
			RoutesHost.Models.Route item = null;
			Retry((list) =>
			{
				var result = list.TryGetValue(key, out item);
				return result;
			});
			if (item != null)
			{
				return item.WebApiAddress;
			}
			return null;
		}

		private void Retry(Func<ConcurrentDictionary<string, RoutesHost.Models.Route>, bool> predicate, int retryCount = 3)
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