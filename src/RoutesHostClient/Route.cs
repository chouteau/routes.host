using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class Route
	{
		public string ServiceName { get; set; }
		public string WebApiAddress { get; set; }
		public string ApiKey { get; set; }
		public int Priority { get; set; }
		public string PingPath { get; set; }
		public string MachineName { get; set; }
	}
}
