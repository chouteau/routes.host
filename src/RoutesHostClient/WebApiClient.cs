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
		public event EventHandler ResolveAdressFailed;
		public event EventHandler<Exception> RequestFailed;
		public event EventHandler<string> NotResolvableAddress;

		public void ExecuteRetry(Func<HttpClient, HttpResponseMessage> predicate, int timeoutInSecond = 60)
		{
			ExecuteRetry<object>(predicate, false, timeoutInSecond);
		}

		public T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, int timeOutInSecond = 60)
		{
			return ExecuteRetry<T>(predicate, true, timeOutInSecond);
		}

		public T ExecuteRetry<T>(Func<HttpClient, HttpResponseMessage> predicate, bool hasReturn = false, int? timeOutInSecond = 60)
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
					if (ResolveAdressFailed != null)
					{
						ResolveAdressFailed(this, EventArgs.Empty);
					}
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
					if (timeOutInSecond.HasValue)
					{
						httpClient.Timeout = TimeSpan.FromSeconds(timeOutInSecond.Value);
					}

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
						availableAddress.FailCount = 0;
					}
					catch(Exception ex)
					{
						if (RequestFailed != null)
						{
							RequestFailed(this, ex);
						}
						var x = ex;
						while (true)
						{
							if (x.InnerException == null)
							{
								GlobalConfiguration.Configuration.Logger.Warn(x.Message);
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
									GlobalConfiguration.Configuration.Logger.Warn($"Address Base : {availableAddress.Address} name resolution failure");
									RoutesProvider.Current.MarkAddressAsUnavailable(availableAddress);
									if (NotResolvableAddress != null)
									{
										NotResolvableAddress(this, availableAddress.Address);
									}
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
									GlobalConfiguration.Configuration.Logger.Warn($"Address Base : {availableAddress.Address} Connection Refused");
									RoutesProvider.Current.MarkAddressAsUnavailable(availableAddress);
								}
								else if (x is System.Net.Http.HttpRequestException)
								{
									var reqex = x as System.Net.Http.HttpRequestException;
									GlobalConfiguration.Configuration.Logger.Warn($"Service : {availableAddress.Address} not found");
									errorCount = RetryCount;
								}
								else if (x is System.ObjectDisposedException)
								{
									errorCount = RetryCount;
								} 
								else if (x is System.Threading.Tasks.TaskCanceledException)
								{
									GlobalConfiguration.Configuration.Logger.Warn($"Service : {availableAddress.Address} has timeout");
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

		public ResolvedRoute GetAvailableRoute()
		{
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
					return null;
				}
			}

			var availableAddress = GetAvailableAddress(resolvedList);
			return availableAddress;
		}

		private ResolvedRoute GetAvailableAddress(List<ResolvedRoute> list)
		{
			var result = list
						.OrderBy(i => i.Order)
						.Where(i => i.FailCount < 10)
						.FirstOrDefault(i => !i.IsAvailable.HasValue
						|| i.IsAvailable.Value
						|| i.ReleaseDate <= DateTime.Now);

			return result;
		}

	}
}
