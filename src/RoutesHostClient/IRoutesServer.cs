namespace RoutesHostClient
{
	public interface IRoutesServer
	{
		System.Guid Register(Route route);
		string Resolve(string apiKey, string serviceName);
		void UnRegister(System.Guid routeId);
	}
}