using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RoutesHostServer.Models;

namespace RoutesHostServer.Services
{
	public class RouteMessageReader : Ariane.MessageReaderBase<Models.RouteMessage>
	{
		public RouteMessageReader()
		{
			RegisteredSender = System.Configuration.ConfigurationManager.AppSettings["TopicName"];
		}

		protected string RegisteredSender { get; private set; }

		public override void ProcessMessage(Models.RouteMessage message)
		{
			if (message == null)
			{
				return;
			}

			if (RegisteredSender.Equals(message.Sender, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}

			switch (message.Action?.ToLower())
			{
				case "register":
					RoutesProvider.Current.Register(message.Route, true);
					break;
				case "unregister":
					RoutesProvider.Current.UnRegister(message.RouteId, true);
					break;
				case "unregisterservice":
					RoutesProvider.Current.UnRegisterService(message.ApiKey, message.ServiceName, true);
					break;
				default:
					break;
			}

		}
	}
}