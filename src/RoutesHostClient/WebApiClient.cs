using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class WebApiClient
	{
		public WebApiClient(string apiKey, string serviceName)
		{
			RetryCount = 4;
			RetryIntervalInSecond = 5;
			ApiKey = apiKey;
			ServiceName = serviceName;
			RequestHeaders = new System.Collections.Specialized.NameValueCollection();
		}

		public string ApiKey { get; set; }
		public string ServiceName { get; set; }
		public int RetryCount { get; private set; }
		public int RetryIntervalInSecond { get; private set; }
		public System.Collections.Specialized.NameValueCollection RequestHeaders { get; set; }

		public T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, bool hasReturn = false)
		{
			var baseAddress = RoutesProvider.Current.Resolve(ApiKey, ServiceName);
			if (baseAddress == null)
			{
				throw new Exception($"baseAddress not found for needed service {ServiceName} with key {ApiKey}");
			}
			var loop = 0;
			T result = default(T);
			while (true)
			{
				try
				{
					using (var httpClient = new System.Net.Http.HttpClient())
					{
						httpClient.BaseAddress = new Uri(baseAddress);
						foreach (var headerName in RequestHeaders.AllKeys)
						{
							httpClient.DefaultRequestHeaders.Add(headerName, RequestHeaders[headerName]);
						}

						// httpClient.DefaultRequestHeaders.Add("apiKey", GlobalConfiguration.Configuration.Settings.ApiKey);
						var response = predicate.Invoke(httpClient);
						if (!response.IsSuccessStatusCode)
						{
							if (loop > RetryCount)
							{
								if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
								{
									RoutesProvider.Current.RemoveCache(ApiKey, ServiceName);
								}
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
					ex.Data.Add("apiKey", ApiKey);
					ex.Data.Add("serviceName", ServiceName);
					GlobalConfiguration.Configuration.Logger.Error(ex);
					if (loop > RetryCount)
					{
						RoutesProvider.Current.RemoveCache(ApiKey, ServiceName);
						break;
					}
					loop++;
				}
				System.Threading.Thread.Sleep(RetryIntervalInSecond * 1000);
			}
			return result;
		}

	}
}
