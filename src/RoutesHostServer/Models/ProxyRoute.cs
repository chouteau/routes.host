using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostServer.Models
{
	public class ProxyRoute
	{
		public string ApiKey { get; set; }
		public string ServiceName { get; set; }
		public string WebApiAddress { get; set; }
	}
}
