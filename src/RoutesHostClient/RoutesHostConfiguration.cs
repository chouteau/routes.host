using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class RoutesHostConfiguration
	{
		private static Lazy<string> m_version = new Lazy<string>(() =>
		{
			var att = (System.Reflection.AssemblyInformationalVersionAttribute)typeof(GlobalConfiguration).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)[0];
			return att.InformationalVersion;
		}, true);

		public ILogger Logger { get; set; }
		public bool UseProxy { get; set; }
		public string Version
		{
			get
			{
				return m_version.Value;
			}
		}
		public System.Net.Http.Headers.AuthenticationHeaderValue Authorization { get; set; }

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
