using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RoutesHostClientTests
{
	[RoutePrefix("api/pingservice")]
	public class PingController : ApiController
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
	}
}
