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

		private System.Collections.Concurrent.ConcurrentDictionary<string, List<ResolvedRoute>> m_Cache;
		private System.Collections.Concurrent.ConcurrentBag<UnavailableRoute> m_UnavailableRouteList;

		private RoutesProvider()
		{
			m_Cache = new System.Collections.Concurrent.ConcurrentDictionary<string, List<ResolvedRoute>>();
			m_UnavailableRouteList = new System.Collections.Concurrent.ConcurrentBag<UnavailableRoute>();
			RouteServerList = new List<RouteServer>();
		}

		public static RoutesProvider Current
		{
			get
			{
				return m_LazyInstance.Value;
			}
		}

		public string ResolvedTestUrl { get; set; }
		internal List<RouteServer> RouteServerList { get; set; }

		public Guid Register(Route route)
		{
			if (ResolvedTestUrl != null)
			{
				return Guid.NewGuid();
			}
			route.MachineName = route.MachineName ?? System.Environment.MachineName;

			var result = ExecuteRetry<string>((client) =>
			{
				var routeId = client.PutAsJsonAsync($"api/routes/register", route).Result;
				return routeId;
			}, true);

			GlobalConfiguration.Configuration.Logger.Info($"Route for service {route.ServiceName} registered with address {route.WebApiAddress}");
			return new Guid(result);
		}

		public void RegisterProxy(ProxyRoute proxy)
		{
			ExecuteRetry<object>((client) =>
			{
				return client.PostAsJsonAsync($"api/routes/registerproxy", proxy).Result;
			}, false);

			GlobalConfiguration.Configuration.Logger.Info($"Proxy for service {proxy.ServiceName} was registered");
		}

		public void UnRegister(Guid routeId)
		{
			if (ResolvedTestUrl != null)
			{
				return;
			}
			ExecuteRetry<object>((client) =>
			{
				return client.DeleteAsync($"api/routes/unregister/{routeId}").Result;
			}, false);

			GlobalConfiguration.Configuration.Logger.Info($"Route for service {routeId} was unregistered");
		}

		public void UnRegisterService(string apiKey, string serviceName)
		{
			if (ResolvedTestUrl != null)
			{
				return;
			}
			ExecuteRetry<object>((client) =>
			{
				return client.DeleteAsync($"api/routes/unregisterservice/?apiKey={apiKey}&serviceName={serviceName}").Result;
			}, false);

			GlobalConfiguration.Configuration.Logger.Info($"All routes for service {serviceName} was unregistered");
		}

		public List<ResolvedRoute> Resolve(string apiKey, string serviceName)
		{
			if (ResolvedTestUrl != null)
			{
				return new List<ResolvedRoute>()
				{
					new ResolvedRoute()
					{
						Address = ResolvedTestUrl
					}
				};
			}
			var key = $"{apiKey}|{serviceName}|{GlobalConfiguration.Configuration.UseProxy}";
			if (m_Cache.ContainsKey(key))
			{
				return m_Cache[key];
			}

			var result = ExecuteRetry<RoutesHostServer.Models.ResolveResult>((client) =>
			{
				var url = $"api/routes/resolve/?apiKey={apiKey}&serviceName={serviceName}&useproxy={GlobalConfiguration.Configuration.UseProxy}";
				return client.GetAsync(url).Result;
			}, true);

			var resolvedList = new List<ResolvedRoute>();
			if (result != null
				&& result.AddressList.Count > 0)
			{
				if (!m_Cache.ContainsKey(key))
				{
					int order = 0;
					foreach (var item in result.AddressList)
					{
						if (IsUnavailbleAddress(item))
						{
							continue;
						}
						resolvedList.Add(new ResolvedRoute()
						{
							Address = item,
							Order = order++
						});
					}
					m_Cache.TryAdd(key, resolvedList);
				}
			}

			return resolvedList;
		}

		internal void MarkAddressAsUnavailable(ResolvedRoute address)
		{
			address.IsAvailable = false;
			address.ReleaseDate = DateTime.Now.AddSeconds(20);

			if (m_UnavailableRouteList.Any(i => i.Address == address.Address))
			{
				return;
			}

			m_UnavailableRouteList.Add(new UnavailableRoute()
			{
				Address = address.Address,
				ReleaseDate = address.ReleaseDate.Value
			});
		}

		internal void RemoveCache(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}|{GlobalConfiguration.Configuration.UseProxy}";
			if (!m_Cache.ContainsKey(key))
			{
				return;
			}
			List<ResolvedRoute> resolvedList = null;
			m_Cache.TryRemove(key, out resolvedList);
		}

		internal void AddBaseAddress(string baseAddress)
		{
			if (string.IsNullOrWhiteSpace(baseAddress))
			{
				throw new ArgumentNullException();
			}
			if (RouteServerList.Any(i => i.BaseAdresse.Equals(baseAddress, StringComparison.InvariantCultureIgnoreCase)))
			{
				return;	
			}
			RouteServerList.Add(new RouteServer()
			{
				BaseAdresse = baseAddress,
			});
		}

		internal void ResetBaseAddressList()
		{
			RouteServerList.Clear();
		}

		private RouteServer GetAvailableRouteServer()
		{
			var result = RouteServerList
							.OrderByDescending(i => i.UseCount)
							.FirstOrDefault(i => !i.IsAvailable.HasValue 
							|| i.IsAvailable.Value 
							|| i.ReleaseDate <= DateTime.Now);

			return result;
		}

		private T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, bool hasReturn = false)
		{
			var loop = 0;
			T result = default(T);
			while (true)
			{
				using (var httpClient = new System.Net.Http.HttpClient())
				{
					var routeServer = GetAvailableRouteServer();
					if (routeServer == null)
					{
						GlobalConfiguration.Configuration.Logger.Error("internet access unavailable");
						return default(T);
					}
					httpClient.DefaultRequestHeaders.Add("UserAgent", $"RouteHostClient/{GlobalConfiguration.Configuration.Version} (http://routes.host)");
					httpClient.BaseAddress = new Uri(routeServer.BaseAdresse);

					HttpResponseMessage response = null;
					try
					{
						response = predicate.Invoke(httpClient);
						response.EnsureSuccessStatusCode();
					}
					catch(HttpRequestException hex) when (hex.Message.LastIndexOf("404") != -1)
					{
						loop = 4;
					}
					catch(Exception ex)
					{
						var x = ex;
						while(true)
						{
							if (x.InnerException == null)
							{
								GlobalConfiguration.Configuration.Logger.Error(x.Message);
								if (x is System.Net.WebException)
								{
									var webex = x as System.Net.WebException;
									if (webex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
									{
										loop = 4;
									}
								}
								break;
							}
							x = x.InnerException;
						}
					}
					
					if (response == null
						|| !response.IsSuccessStatusCode)
					{
						if (loop > 3)
						{
							routeServer.IsAvailable = false;
							routeServer.ReleaseDate = DateTime.Now.AddMinutes(5);
							routeServer.FailAccessCount++;
							loop = -1;
							GlobalConfiguration.Configuration.Logger.Warn($"routehost : server route {routeServer.BaseAdresse} is down");
						}
						loop++;
					}
					else
					{
						if (hasReturn)
						{
							result = response.Content.ReadAsAsync<T>().Result;
						}
						routeServer.IsAvailable = true;
						routeServer.LastAccessDate = DateTime.Now;
						routeServer.UseCount++;
						break;
					}
				}
				System.Threading.Thread.Sleep(4 * 1000);
			}
			return result;
		}

		private bool IsUnavailbleAddress(string address)
		{
			var loop = 0;
			while(true)
			{
				var remove = m_UnavailableRouteList.FirstOrDefault(i => i.ReleaseDate < DateTime.Now);
				if (remove == null)
				{
					break;
				}
				if (loop > 3)
				{
					break;
				}
				UnavailableRoute expired = null;
				if (!m_UnavailableRouteList.TryTake(out expired))
				{
					loop++;
					System.Threading.Thread.Sleep(100);
				}
			}
			var result = m_UnavailableRouteList.Any(i => i.Address.Equals(address, StringComparison.InvariantCultureIgnoreCase));
			return result;
		}
	}
}
