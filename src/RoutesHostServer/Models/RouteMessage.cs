using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Models
{
	public class RouteMessage
	{
		public Route Route { get; set; }
		public ProxyRoute ProxyRoute { get; set; }
		public string Sender { get; set; }
		public Guid RouteId { get; set; }
		public string ApiKey { get; set; }
		public string ServiceName { get; set; }
		public string Action { get; set; }
	}
}