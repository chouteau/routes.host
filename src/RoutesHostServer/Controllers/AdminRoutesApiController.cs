using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace RoutesHostServer.Controllers
{
	[RoutePrefix("api/admin")]
	public class AdminRoutesApiController : ApiController
	{
		public const string COOKIE_NAME = "AdminRoutesHost";

		[HttpGet]
		[Route("ping")]
		public object Ping()
		{
			return new { date = DateTime.Now };
		}

		[ApiAuthorizedOperation]
		[Route("routelist")]
		[HttpGet]
		public object GetRoutes()
		{
			var list = from route in Services.RoutesProvider.Current.RoutesRepository
					   select new
					   {
						   Key = route.Key,
						   Value = route.Value
					   };

			return list;
		}

		[ApiAuthorizedOperation]
		[Route("pingroutelist")]
		[HttpGet]
		public List<string> GetPingUriList()
		{
			var list = new List<string>();
			foreach (var item in Services.RoutesProvider.Current.RoutesRepository)
			{
				foreach (var route in item.Value)
				{
					var pingUri = $"{route.WebApiAddress}{route.PingPath}";
					if (!list.Contains(pingUri))
					{
						list.Add(pingUri);
					}
				}
			}
			return list;
		}

		[Route("authenticate")]
		[HttpPost]
		public HttpResponseMessage Authenticate(Newtonsoft.Json.Linq.JObject json)
		{
			if (json == null)
			{
				var msg = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
				{
					ReasonPhrase = "this service required an api key"
				};
				throw new HttpResponseException(msg);
			}
			dynamic post = json;
			var key = (string)post.key;
			if (key == null)
			{
				var msg = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
				{
					ReasonPhrase = "this service required an api key"
				};
				throw new HttpResponseException(msg);
			}
			var apiKey = System.Configuration.ConfigurationManager.AppSettings["apiKey"];
			if (key != apiKey)
			{
				var msg = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
				{
					ReasonPhrase = "this service required an api key"
				};
				throw new HttpResponseException(msg);
			}

			var response = new HttpResponseMessage();
			response.Headers.Remove(COOKIE_NAME);

			var cookie = new System.Net.Http.Headers.CookieHeaderValue(COOKIE_NAME, key);
			//cookie.HttpOnly = true;
			cookie.Path = FormsAuthentication.FormsCookiePath;
			cookie.Secure = FormsAuthentication.RequireSSL;
			var cookieDomain = FormsAuthentication.CookieDomain
								?? Request.RequestUri.Host;

			cookie.Domain = cookieDomain;
			cookie.Expires = DateTime.Now.AddMonths(1);
			response.Headers.AddCookies(new System.Net.Http.Headers.CookieHeaderValue[]
			{
				cookie
			});

			return response;
		}

		[Route("userinfo")]
		[HttpGet]
		public Models.AdminUser UserInfo()
		{
			var cookie = Request.Headers.GetCookies(COOKIE_NAME).FirstOrDefault();
			if (cookie == null)
			{
				var msg = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
				{
					ReasonPhrase = "this service required an api key"
				};
				throw new HttpResponseException(msg);
			}

			var key = cookie[COOKIE_NAME].Value;
			if (key == null)
			{
				var msg = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
				{
					ReasonPhrase = "this key is invalid"
				};
				throw new HttpResponseException(msg);
			}

			var result = new Models.AdminUser();
			result.Key = key;
			return result;

		}
	}
}