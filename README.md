# Routes.Host

Route provider for micro-services

## Installation

Via Nuget
PM> Install-Package RoutesHostClient

## Enregistrement d'un micro service

```c#

var route = new RoutesHostClient.Route();
route.ApiKey = "{clé}";
route.ServiceName = "IOrder";
route.WebApiAddress = "http://localhost:65432/";
route.Priority = 1;

var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);

```

Il est possible d'enregistrer un meme service et une adresse differente avec une autre priorité, lors de la résolution de l'adresse du service, la valeur de l'adresse retournée sera toujours celle de la route avec la plus haute priorité. La priorité par défaut est 1

#### Suppression d'une route

pour supprimer un service, il faut récupérer la valeur de retour de l'enregistrement et la passer à la methode **unregister**

```c#

RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);

```

A noter , il est possible de supprimer toutes les routes d'un service via l'appel à la methode **unregisterservice**

```c#

RoutesHostClient.RoutesProvider.Current.UnRegisterService(apiKey, serviceName);

```



## Résolution d'adresse d'un micro service

```c#

var webapiClient = new RoutesHostClient.WebApiClient("{clé}", "IOrder");

var order = webapiClient.ExecuteRetry<Models.Order>(client =>
{
	var result = client.GetAsync("api/orders/ORD1234").Result;
	return result;
}, true);

```

#### Configuration globale

Configurer le nombre de rééssais et la durée entre chaque

```c#
var webApiClient = new RoutesHostClient.WebApiClient("{clé}", "IOrder");
webApiClient.RetryCount = 10;
webapiClient.RetryIntervalInSecond = 5;
```

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



