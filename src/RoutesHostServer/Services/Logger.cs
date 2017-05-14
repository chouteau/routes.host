using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Services
{
	public class Logger
	{
		static Logger()
		{
			Writer = new LogR.DiagnosticsLogger();
		}

		public static LogR.ILogger Writer { get; }
	}
}