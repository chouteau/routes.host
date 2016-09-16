using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace RoutesHostServer.Controllers
{
	[RoutePrefix("api/admin")]
	public class AdminRoutesController : ApiController
	{
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
			return Services.RoutesProvider.Current.RoutesRepository;
		}

	}
}