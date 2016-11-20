using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Services
{
	public class RouteSynchronizer
	{
		private static Lazy<RouteSynchronizer> m_LazySynchronizer = new Lazy<RouteSynchronizer>(() =>
		{
			return new RouteSynchronizer();
		}, true);

		public RouteSynchronizer()
		{
			Bus = new Ariane.BusManager();
		}

		public static RouteSynchronizer Current
		{
			get
			{
				return m_LazySynchronizer.Value;
			}
		}

		public Ariane.IServiceBus Bus { get; private set; }


	}
}