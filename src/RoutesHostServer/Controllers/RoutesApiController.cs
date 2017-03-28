using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;

namespace RoutesHostServer.Controllers
{
	[RoutePrefix("api/routes")]
	public class RoutesApiController : ApiController
    {
		[Route("ping")]
		[HttpGet]
		public object Ping()
		{
			return new
			{
				Date = DateTime.Now
			};
		}

		[HttpPut]
		[HttpPost]
		[Route("register")]
		public Guid Register(Models.Route route)
		{
			if (route == null)
			{
				throw new ArgumentNullException();
			}
			if (route.ApiKey == null
				|| string.IsNullOrWhiteSpace(route.ServiceName)
				|| string.IsNullOrWhiteSpace(route.WebApiAddress))
			{
				throw new ArgumentException("route is not valid");
			}
			route.Ip = GetClientIpAddress();
			var uri = new Uri(route.WebApiAddress);
			var result = Services.RoutesProvider.Current.Register(route);
			return result;
		}

		[HttpPut]
		[HttpPost]
		[Route("registerproxy")]
		public void RegisterProxy(Models.ProxyRoute proxy)
		{
			if (proxy == null
				|| string.IsNullOrWhiteSpace(proxy.ApiKey)
				|| string.IsNullOrWhiteSpace(proxy.WebApiAddress))
			{
				throw new ArgumentNullException();
			}

			Services.RoutesProvider.Current.RegisterProxy(proxy);
		}

		[HttpDelete]
		[Route("unregister/{id:guid}")]
		public void UnRegister(Guid id)
		{
			if (id == null || id == Guid.Empty)
			{
				throw new ArgumentNullException();
			}
			Services.RoutesProvider.Current.UnRegister(id);
		}

		[HttpDelete]
		[Route("unregisterservice")]
		public void UnRegisterService(string apiKey, string serviceName)
		{
			if (string.IsNullOrWhiteSpace(apiKey)
				|| string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentNullException();
			}
			Services.RoutesProvider.Current.UnRegisterService(apiKey, serviceName);
		}

		[HttpGet]
		[Route("resolve")]
		public Models.ResolveResult Resolve(string apiKey, string serviceName, bool useProxy)
		{
			if (string.IsNullOrWhiteSpace(apiKey)
				|| string.IsNullOrWhiteSpace(serviceName))
			{
				throw new ArgumentNullException();
			}
			var result = Services.RoutesProvider.Current.Resolve(apiKey, serviceName, useProxy);
			return new Models.ResolveResult()
			{
				Address = result
			};
		}

		private HttpRequestBase GetRequestBase()
		{
			if (Request == null
				|| Request.Properties == null)
			{
				return null;
			}

			if (Request.Properties.ContainsKey("MS_HttpContext"))
			{
				return ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request;
			}
			return null;
		}

		private string GetClientIpAddress()
		{
			var requestBase = GetRequestBase();
			if (requestBase != null)
			{
				return requestBase.UserHostAddress;
			}
			else if (Request == null)
			{
				return null;
			}
			else if (Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			{
				RemoteEndpointMessageProperty prop;
				prop = (RemoteEndpointMessageProperty)Request.Properties[RemoteEndpointMessageProperty.Name];
				return prop.Address;
			}
			else
			{
				return null;
			}
		}

	}
}
