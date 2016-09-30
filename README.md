# Routes.Host

Route provider for micro-services

## Installation

Via Nuget
PM> Install-Package RoutesHostClient

## Enregistrement d'un micro service

```c#

var route = new RoutesHostClient.Route();
route.ApiKey = "{cl�}";
route.ServiceName = "IOrder";
route.WebApiAddress = "http://localhost:65432/";
route.Priority = 1;

var routeId = RoutesHostClient.RoutesProvider.Current.Register(route);

```

Il est possible d'enregistrer un meme service et une adresse differente avec une autre priorit�, lors de la r�solution de l'adresse du service, la valeur de l'adresse retourn�e sera toujours celle de la route avec la plus haute priorit�. La priorit� par d�faut est 1

#### Suppression d'une route

pour supprimer un service, il faut r�cup�rer la valeur de retour de l'enregistrement et la passer � la methode **unregister**

```c#

RoutesHostClient.RoutesProvider.Current.UnRegister(routeId);

```

A noter , il est possible de supprimer toutes les routes d'un service via l'appel � la methode **unregisterservice**

```c#

RoutesHostClient.RoutesProvider.Current.UnRegisterService(apiKey, serviceName);

```



## R�solution d'adresse d'un micro service

```c#

var webapiClient = new RoutesHostClient.WebApiClient("{cl�}", "IOrder");

var order = webapiClient.ExecuteRetry<Models.Order>(client =>
{
	var result = client.GetAsync("api/orders/ORD1234").Result;
	return result;
}, true);

```

#### Configuration globale

Configurer le nombre de r��ssais et la dur�e entre chaque

```c#
var webApiClient = new RoutesHostClient.WebApiClient("{cl�}", "IOrder");
webApiClient.RetryCount = 10;
webapiClient.RetryIntervalInSecond = 5;
```

Il est possible de changer le logger par d�faut du client, dans ce cas il faut implementer l'interface suivante :

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



