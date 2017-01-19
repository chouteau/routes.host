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
				|| route.ServiceName == null
				|| route.WebApiAddress == null)
			{
				throw new ArgumentException("route is not valid");
			}
			route.Ip = GetClientIpAddress();
			var uri = new Uri(route.WebApiAddress);
			var result = Services.RoutesProvider.Current.Register(route);
			return result;
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
			if (apiKey == null
				|| serviceName == null)
			{
				throw new ArgumentNullException();
			}
			Services.RoutesProvider.Current.UnRegisterService(apiKey, serviceName);
		}

		[HttpGet]
		[Route("resolve")]
		public Models.ResolveResult Resolve(string apiKey, string serviceName)
		{
			if (apiKey == null
				|| serviceName == null)
			{
				throw new ArgumentNullException();
			}
			var result = Services.RoutesProvider.Current.Resolve(apiKey, serviceName);
			return new Models.ResolveResult()
			{
				Address = result
			};
		}

		public HttpRequestBase GetRequestBase()
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

		public string GetClientIpAddress()
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
