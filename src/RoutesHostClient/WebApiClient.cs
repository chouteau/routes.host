using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class WebApiClient
	{
		public WebApiClient(string apiKey, string serviceName, int retryCount = 4, int intervalInSecond = 5)
		{
			RetryCount = retryCount;
			RetryIntervalInSecond = intervalInSecond;
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
				throw new Exception($"baseAddress not found for needed service {ServiceName} with key {ApiKey.Substring(0,5)}...{ApiKey.Substring(ApiKey.Length-5)}");
			}
			var loop = 0;
			T result = default(T);
			while (true)
			{
				try
				{
					var handler = new HttpClientHandler()
					{
						AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
						AllowAutoRedirect = false,
						UseCookies = true,
					};

					using (var httpClient = new System.Net.Http.HttpClient(handler))
					{
						httpClient.DefaultRequestHeaders.Add("UserAgent", $"RouteHostClient/{GlobalConfiguration.Configuration.Version} (http://routes.host)");
						httpClient.BaseAddress = new Uri(baseAddress);
						if(GlobalConfiguration.Configuration.Authorization != null)
						{
							httpClient.DefaultRequestHeaders.Authorization = GlobalConfiguration.Configuration.Authorization;
						}
						foreach (var headerName in RequestHeaders.AllKeys)
						{
							httpClient.DefaultRequestHeaders.Add(headerName, RequestHeaders[headerName]);
						}

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
