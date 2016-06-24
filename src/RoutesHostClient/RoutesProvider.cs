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
		}

		public static RoutesProvider Current
		{
			get
			{
				return m_LazyInstance.Value;
			}
		}

		public void Register(RoutesHost.Models.Route route)
		{
			ExecuteRetry<object>((client) =>
			{
				return client.PutAsJsonAsync($"api/routes/register", route).Result;
			}, false);
		}

		public void UnRegister(string apiKey, string serviceName)
		{
			ExecuteRetry<object>((client) =>
			{
				return client.DeleteAsync($"api/routes/unregister/?apikey={apiKey}&serviceName={serviceName}").Result;
			}, false);
		}

		public string Resolve(string apiKey, string serviceName)
		{
			var key = $"{apiKey}|{serviceName}";
			if (m_Cache.ContainsKey(key))
			{
				return m_Cache[key];
			}

			var result = ExecuteRetry<string>((client) =>
			{
				return client.GetAsync($"api/routes/resolve/?apiKey={apiKey}&serviceName={serviceName}").Result;
			}, true);

			if (result != null)
			{
				m_Cache.Add(key, result);
			}

			return result;
		}

		public T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, bool hasReturn = false)
		{
			var loop = 0;
			T result = default(T);
			while (true)
			{
				try
				{
					using (var httpClient = new System.Net.Http.HttpClient())
					{
						httpClient.BaseAddress = new Uri(GlobalConfiguration.Configuration.BaseAddress);
						// httpClient.DefaultRequestHeaders.Add("apiKey", GlobalConfiguration.Configuration.Settings.ApiKey);
						var response = predicate.Invoke(httpClient);
						if (!response.IsSuccessStatusCode)
						{
							if (loop > 4)
							{
								break;
							}
							loop++;
							var errorMessage = response.ReasonPhrase;
							errorMessage += $" {response.RequestMessage.RequestUri}";
							GlobalConfiguration.Configuration.Logger.Error(errorMessage);
						}
						else
						{
							if (hasReturn)
							{
								result = response.Content.ReadAsAsync<T>().Result;
							}
							break;
						}
					}
				}
				catch (Exception ex)
				{
					GlobalConfiguration.Configuration.Logger.Error(ex);
					if (loop > 4)
					{
						break;
					}
					loop++;
				}
				System.Threading.Thread.Sleep(5 * 1000);
			}
			return result;
		}

	}
}
