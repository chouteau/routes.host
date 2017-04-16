using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Models
{
	public class ResolveResult
	{
		public ResolveResult()
		{
			AddressList = new List<string>();
		}

		[Obsolete("Use AddressList instead (Property removed in next version)", false)]
		public string Address { get; set; }

		public List<string> AddressList { get; set; }
	}
}