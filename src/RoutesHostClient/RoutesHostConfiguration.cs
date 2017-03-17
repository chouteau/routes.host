using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class RoutesHostConfiguration
	{
		public ILogger Logger { get; set; }
		public void AddAddress(string baseAddress)
		{
			RoutesProvider.Current.AddBaseAddress(baseAddress);
		}
		public void ResetBaseAddressList()
		{
			RoutesProvider.Current.ResetBaseAddressList();
		}
	}
}
