using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHost.Models
{
	public class Route
	{
		public string ServiceName { get; set; }
		public string Owner { get; set; }
		public string WebApiAddress { get; set; }
		public DateTime CreationDate { get; set; }
		public string ApiKey { get; set; }
	}
}