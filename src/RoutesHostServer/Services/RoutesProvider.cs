using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

namespace RoutesHostServer.Services
{
	public class RoutesProvider
	{
		public static Lazy<RoutesProvider> m_Instance = new Lazy<RoutesProvider>(() =>
		{
			return new RoutesProvider();
		}, true);

		internal ConcurrentDictionary<string, List<Models.Route>> RoutesRepository { get; private set; }
		internal ConcurrentBag<string> PingUriList { get; private set; }
		protected string Sender { get; set; }

		private RoutesProvider()
		{
			RoutesRepository = new ConcurrentDictionary<string, List<Models.Route>>();
			Sender = System.Configuration.ConfigurationManager.AppSettings["TopicName"];
		}
		
		public static RoutesProvider Current
		{
			get
			{
				return m_Instance.Value;
			}
		}

		public Guid Register(Models.Route item, bool fromTopic = false)
		{
			var message = new Models.RouteMessage()
			{
				Action = "register",
				Route = item,
				Sender = Sender
			};
			if (!fromTopic)
			{
				RouteSynchronizer.Current.Bus.Send("RoutesHostAction", message);
			}
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
						var route = routes.SingleOrDefault(i => $"{i.ApiKey}|{i.ServiceName}".Equals(key, StringComparison.InvariantCultureIgnoreCase) 
												&& i.Priority == item.Priority);
						if (route == null)
						{
							id = item.Id = Guid.NewGuid();
							routes.Add(item);
						}
						else
						{
							route.WebApiAddress = item.WebApiAddress;
							route.PingPath = item.PingPath;
							id = route.Id;
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

		public void UnRegister(Guid routeId, bool fromTopic = false)
		{
			var message = new Models.RouteMessage()
			{
				Action = "unregister",
				RouteId = routeId,
				Sender = Sender
			};
			if (!fromTopic)
			{
				RouteSynchronizer.Current.Bus.Send("RoutesHostAction", message);
			}
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

		public void UnRegisterService(string apiKey, string serviceName, bool fromTopic = false)
		{
			var message = new Models.RouteMessage()
			{
				Action = "unregisterservice",
				ApiKey = apiKey,
				ServiceName = serviceName,
				Sender = Sender
			};

			if (!fromTopic)
			{
				RouteSynchronizer.Current.Bus.Send("RoutesHostAction", message);
			}

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
				var item = routes.OrderByDescending(i => i.Priority).FirstOrDefault();
				return item.WebApiAddress;
			}
			return null;
		}

		public void Flush(string repositoryFolder)
		{
			var fileList = (from f in System.IO.Directory.GetFiles(repositoryFolder, "*.route.json")
						   select f).ToList();

			foreach (var item in this.RoutesRepository)
			{
				foreach (var route in item.Value)
				{
					var fileName = System.IO.Path.Combine(repositoryFolder, $"{Guid.NewGuid()}.route.json");
					var content = Newtonsoft.Json.JsonConvert.SerializeObject(route);

					if (System.IO.File.Exists(fileName))
					{
						System.IO.File.Delete(fileName);
					}
					System.IO.File.WriteAllText(fileName, content);
					fileList.Remove(fileName);
				}
			}
			RoutesRepository.Clear();

			foreach (var item in fileList)
			{
				System.IO.File.Delete(item);
			}
		}

		public void Hydrate(string repositoryFolder)
		{
			var fileList = from f in System.IO.Directory.GetFiles(repositoryFolder, "*.route.json")
						   select f;

			foreach (var fileName in fileList)
			{
				var content = System.IO.File.ReadAllText(fileName);
				var item = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Route>(content);

				Register(item);
			}
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