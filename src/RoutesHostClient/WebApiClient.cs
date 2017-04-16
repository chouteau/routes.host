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
		public Exception LastException { get; set; }

		public void ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate)
		{
			ExecuteRetry<object>(predicate, false);
		}

		public T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, bool hasReturn = false)
		{
			T result = default(T);

			var resolvedList = RoutesProvider.Current.Resolve(ApiKey, ServiceName);
			if (resolvedList == null
				|| resolvedList.Count == 0)
			{
				RoutesProvider.Current.RemoveCache(ApiKey, ServiceName);
				resolvedList = RoutesProvider.Current.Resolve(ApiKey, ServiceName);
				if (resolvedList == null
					|| resolvedList.Count == 0)
				{
					var errorMessage = $"baseAddress not found for needed service {ServiceName} with key {ApiKey.Substring(0, 5)}...{ApiKey.Substring(ApiKey.Length - 5)}";
					GlobalConfiguration.Configuration.Logger.Error(errorMessage);
					LastException = new Exception(errorMessage);
					return result;
				}
			}

			var handler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				AllowAutoRedirect = false,
				UseCookies = false
			};

			var errorCount = 0;
			while (true)
			{
				var availableAddress = GetAvailableAddress(resolvedList);
				if (availableAddress == null)
				{
					RoutesProvider.Current.RemoveCache(ApiKey, ServiceName);
					resolvedList = RoutesProvider.Current.Resolve(ApiKey, ServiceName);
					if (resolvedList == null)
					{
						var message = $"baseAddress not found for needed service {ServiceName} with key {ApiKey.Substring(0, 2)}...{ApiKey.Substring(ApiKey.Length - 2)}";
						GlobalConfiguration.Configuration.Logger.Error(message);
						LastException = new Exception(message);
						return result;
					}
					availableAddress = GetAvailableAddress(resolvedList);
					if (availableAddress == null)
					{
						var message = $"Not available baseAddress found for needed service {ServiceName} with key {ApiKey.Substring(0, 2)}...{ApiKey.Substring(ApiKey.Length - 2)}";
						GlobalConfiguration.Configuration.Logger.Error(message);
						LastException = new Exception(message);
						return result;
					}
				}

				using (var httpClient = new System.Net.Http.HttpClient(handler, false))
				{
					httpClient.DefaultRequestHeaders.Add("UserAgent", $"RouteHostClient/{GlobalConfiguration.Configuration.Version} (http://routes.host)");
					httpClient.BaseAddress = new Uri(availableAddress.Address);

					if (GlobalConfiguration.Configuration.Authorization != null)
					{
						httpClient.DefaultRequestHeaders.Authorization = GlobalConfiguration.Configuration.Authorization;
					}
					foreach (var headerName in RequestHeaders.AllKeys)
					{
						httpClient.DefaultRequestHeaders.Add(headerName, RequestHeaders[headerName]);
					}

					HttpResponseMessage response = null;
					try
					{
						response = predicate.Invoke(httpClient);
						response.EnsureSuccessStatusCode();
						availableAddress.LastAccessDate = DateTime.Now;
						availableAddress.UseCount++;
					}
					catch(Exception ex)
					{
						var x = ex;
						while (true)
						{
							if (x.InnerException == null)
							{
								GlobalConfiguration.Configuration.Logger.Error(x.Message);
								if (x is System.Net.WebException)
								{
									var webex = x as System.Net.WebException;
									//if (webex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
									//{
									//	errorCount = RetryCount;
									//}
									if (resolvedList.Count == 1)
									{
										errorCount = RetryCount;
									}
									GlobalConfiguration.Configuration.Logger.Warn($"Adresse Base : {availableAddress.Address} not found");
									RoutesProvider.Current.MarkAddressAsUnavailable(availableAddress);
								}
								else if (x is System.Net.Sockets.SocketException)
								{
									var sockex = x as System.Net.Sockets.SocketException;
									// if (sockex.NativeErrorCode == 10061) // Connection Refused
									// {
									// 	errorCount = RetryCount;
									// }
									if (resolvedList.Count == 1)
									{
										errorCount = RetryCount;
									}
									GlobalConfiguration.Configuration.Logger.Warn($"Adresse Base : {availableAddress.Address} not found");
									RoutesProvider.Current.MarkAddressAsUnavailable(availableAddress);
								}
								else if (x is System.Net.Http.HttpRequestException)
								{
									var reqex = x as System.Net.Http.HttpRequestException;
									GlobalConfiguration.Configuration.Logger.Warn($"Service : {response.RequestMessage.RequestUri} not found");
									errorCount = RetryCount;
								}
								else if (x is System.ObjectDisposedException)
								{
									errorCount = RetryCount;
								}
								LastException = x;
								break;
							}
							x = x.InnerException;
						}
						errorCount++;
					}

					if (response == null
						|| !response.IsSuccessStatusCode)
					{
						if (errorCount > RetryCount)
						{
							break;
						}
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
				System.Threading.Thread.Sleep(RetryIntervalInSecond * 1000);
			}
			return result;
		}

		private ResolvedRoute GetAvailableAddress(List<ResolvedRoute> list)
		{
			var result = list
						.OrderBy(i => i.Order)
						.FirstOrDefault(i => !i.IsAvailable.HasValue
						|| i.IsAvailable.Value
						|| i.ReleaseDate <= DateTime.Now);

			return result;
		}

	}
}
