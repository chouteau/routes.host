using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	internal class RouteServer
	{
		public string BaseAdresse { get; set; }
		public DateTime? LastAccessDate { get; set; }
		public bool? IsAvailable { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public int FailAccessCount { get; set; }
		public int UseCount { get; set; }
	}
}
