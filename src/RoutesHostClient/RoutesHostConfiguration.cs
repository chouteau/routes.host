using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class RoutesHostConfiguration
	{
		public RoutesHostConfiguration()
		{
		}
		public ILogger Logger { get; set; }
		public string BaseAddress { get; set; }
	}
}
