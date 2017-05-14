using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

using Newtonsoft.Json.Serialization;

namespace RoutesHostServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			// Use camel case for JSON data.
			config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
			config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
			config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;

			var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
			config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Services.Add(typeof(IExceptionLogger), new WebApiExceptionLogger());

			config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

		public class WebApiExceptionLogger : ExceptionLogger
		{
			public override void Log(ExceptionLoggerContext context)
			{
				context.Exception.Data.Add("ApiMethod", context.Request.Method.ToString());
				context.Exception.Data.Add("RequestUri", context.Request.RequestUri.ToString());
				context.Exception.Data.Add("Content", context.Request.Content.ToString());
				Services.Logger.Writer.Error(context.Exception);
			}
		}

	}
}
