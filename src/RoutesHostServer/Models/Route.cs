﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutesHostServer.Models
{
	public class Route : ICloneable
	{
		public Guid Id { get; set; }
		public string ServiceName { get; set; }
		public string WebApiAddress { get; set; }
		public string ProxyWebApiAddress { get; set; }
		public DateTime CreationDate { get; set; }
		public string ApiKey { get; set; }
		public int Priority { get; set; }
		public string PingPath { get; set; }
		public string MachineName { get; set; }
		public string Ip { get; set; }

		public object Clone()
		{
			var clone = (Route) this.MemberwiseClone();
			clone.Id = Guid.NewGuid();
			return clone;
		}
	}
}