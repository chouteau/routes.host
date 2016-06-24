# routes.host
Route provider for micro-services

## Installation

Via Nuget
PM> Install-Package RoutesHostClient

## Enregistrement d'un micro service

```c#

var route = new RoutesHost.Models.Route();
route.ApiKey = "{clé}";
route.ServiceName = "IOrder";
route.WebApiAddress = "http://localhost:65432/";

RoutesHostClient.RoutesProvider.Current.Register(route);

```

## Appel d'un micro service

```c#

var webapiClient = new RoutesHostClient.WebApiClient("{clé}", "IOrder");

var order = webapiClient.ExecuteRetry<Models.Order>(client =>
{
	var result = client.GetAsync("api/orders/ORD1234").Result;
	return result;
}, true);

```

#### Configuration globale

Il est possible de changer le logger par défaut du client, dans ce cas il faut implementer l'interface suivante :

```c#
public interface ILogger
{
	void Debug(string message);
	void Debug(string message, params object[] prms);
	void Error(Exception x);
	void Error(string message);
	void Error(string message, params object[] prms);
	void Fatal(string message);
	void Fatal(Exception x);
	void Fatal(string message, params object[] prms);
	void Info(string message);
	void Info(string message, params object[] prms);
	void Notification(string message);
	void Notification(string message, params object[] prms);
	void Warn(string message);
	void Warn(string message, params object[] prms);
}
```

Et engistrer celle-ci dans la configuration du client comme ceci :
```c#
RoutesHostClient.GlobalConfiguration.Configuration.Logger = new MyLogger();
```

Il est possible de configurer le nombre de rééssais et la durée entre chaque

```c#
var webApiClient = new RoutesHostClient.WebApiClient("{clé}", "IOrder");
webApiClient.RetryCount = 10;
webapiClient.RetryIntervalInSecond = 5;
```




