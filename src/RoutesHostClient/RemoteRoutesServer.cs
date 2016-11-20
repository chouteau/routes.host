using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	internal class RemoteRoutesServer : IRoutesServer
	{
		private static Lazy<Uri> m_LazyBaseAddress = new Lazy<Uri>(() =>
		{
			return new Uri(RoutesHostClient.GlobalConfiguration.Configuration.BaseAddress);
		}, true);

		public RemoteRoutesServer()
		{
		}

		protected Uri BaseAddress
		{
			get
			{
				return m_LazyBaseAddress.Value;
			}
		}

		public Guid Register(Route route)
		{
			var result = ExecuteRetry<string>((client) =>
			{
				var routeId = client.PutAsJsonAsync($"api/routes/register", route).Result;
				return routeId;
			}, true);

			return new Guid(result);
		}

		public void UnRegister(Guid routeId)
		{
			ExecuteRetry<object>((client) =>
			{
				return client.DeleteAsync($"api/routes/unregister/{routeId}").Result;
			}, false);
		}

		public string Resolve(string apiKey, string serviceName)
		{
			var result = ExecuteRetry<RoutesHostServer.Models.ResolveResult>((client) =>
			{
				var url = $"api/routes/resolve/?apiKey={apiKey}&serviceName={serviceName}";
				return client.GetAsync(url).Result;
			}, true);

			return result.Address;
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
						httpClient.BaseAddress = BaseAddress;
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
