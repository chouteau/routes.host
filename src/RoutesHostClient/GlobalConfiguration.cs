using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutesHostClient
{
	public class GlobalConfiguration
	{
		private static Lazy<RoutesHostConfiguration> m_Configuration
		= new Lazy<RoutesHostConfiguration>(() =>
		{
			return new RoutesHostConfiguration()
			{
				BaseAddress = "http://routes.host",
				Logger = new DiagnosticsLogger()
			};
		}, true);

		public static RoutesHostConfiguration Configuration
		{
			get
			{
				return m_Configuration.Value;
			}
		}
	}
}
