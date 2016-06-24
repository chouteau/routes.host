using RoutesHost.Models;

namespace RoutesHostClient
{
	public interface IRoutesServer
	{
		void Register(Route route);
		string Resolve(string apiKey, string serviceName);
		void UnRegister(string apiKey, string serviceName);
	}
}