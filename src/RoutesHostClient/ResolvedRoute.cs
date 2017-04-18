using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class ResolvedRoute
	{
		public string Address { get; set; }
		public int Order { get; set; }
		public DateTime? LastAccessDate { get; set; }
		public DateTime? ReleaseDate { get; set; }
		public bool? IsAvailable { get; set; }
		public int UseCount { get; set; }
		public int FailCount { get; set; }
	}
}
