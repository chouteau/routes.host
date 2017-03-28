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
			var result = new RoutesHostConfiguration()
			{
				Logger = new DiagnosticsLogger(),
				UseProxy = false
			};
			return result;
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
