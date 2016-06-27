﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RoutesHostServer.Controllers
{
    public class RoutesController : ApiController
    {
		[HttpGet]
		public object Ping()
		{
			return new
			{
				Date = DateTime.Now
			};
		}

		[HttpPut]
		public string Register(Models.Route route)
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
			var uri = new Uri(route.WebApiAddress);
			var result = Services.RoutesProvider.Current.Register(route);
			if (result != Guid.Empty)
			{
				return result.ToString();
			}
			return null;
		}

		[HttpDelete]
		public void UnRegister(string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException();
			}
			Services.RoutesProvider.Current.UnRegister(id);
		}

		[HttpDelete]
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
		public string Resolve(string apiKey, string serviceName)
		{
			if (apiKey == null
				|| serviceName == null)
			{
				throw new ArgumentNullException();
			}
			return Services.RoutesProvider.Current.Resolve(apiKey, serviceName);
		}
	}
}