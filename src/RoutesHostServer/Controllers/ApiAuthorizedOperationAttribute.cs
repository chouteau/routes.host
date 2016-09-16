using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RoutesHostServer.Controllers
{
	public class ApiAuthorizedOperationAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			var apiKey = System.Configuration.ConfigurationManager.AppSettings["apiKey"];

			var auth = actionContext.Request.Headers.Authorization;
			if (auth == null)
			{
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				actionContext.Response.ReasonPhrase = "this service required an api key";
			}
			else if (auth.Scheme == null)
			{
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				actionContext.Response.ReasonPhrase = "this service required an api key";
			}
			else if (apiKey == null)
			{
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				actionContext.Response.ReasonPhrase = "this service required an api key";
			}
			else if (auth.Scheme != apiKey)
			{
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				actionContext.Response.ReasonPhrase = "invalid api key";
			}

			return base.OnActionExecutingAsync(actionContext, cancellationToken);
		}
	}
}