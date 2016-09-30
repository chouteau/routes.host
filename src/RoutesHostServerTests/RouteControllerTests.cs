using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace RoutesHostServerTests
{
	[TestClass]
	public class RouteControllerTests
	{
		[TestMethod]
		public void Add_Null_Route()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var isNull = false;
			try
			{
				controller.Register(null);
			}
			catch(ArgumentNullException)
			{
				isNull = true;
			}

			Check.That(isNull).IsTrue();
		}

		[TestMethod]
		public void Add_Invalid_Route()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var isInvalid = false;
			try
			{
				controller.Register(new RoutesHostServer.Models.Route());
			}
			catch (ArgumentException)
			{
				isInvalid = true;
			}

			Check.That(isInvalid).IsTrue();
		}

		[TestMethod]
		public void Register_Route()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			controller.Register(route);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsEqualTo(route.WebApiAddress);
		}

		[TestMethod]
		public void Register_Existing_Route_With_Same_Priority()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			controller.Register(route);
			controller.Register(route);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);
			Check.That(result.Address).IsEqualTo(route.WebApiAddress);
		}

		[TestMethod]
		public void Register_Existing_Route_With_Less_Priority()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			route.Priority = 1;
			controller.Register(route);

			var lessRoute = (RoutesHostServer.Models.Route) route.Clone();
			lessRoute.Priority = 2;

			controller.Register(lessRoute);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);
			Check.That(result.Address).IsEqualTo(route.WebApiAddress);
		}


		[TestMethod]
		public void Resolve_Unknown_Route()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var result = controller.Resolve("dummy", "unknown");
			Check.That(result.Address).IsNull();
		}


		[TestMethod]
		public void Unregister_Route()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			controller.Register(route);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsEqualTo(route.WebApiAddress);

			controller.UnRegister(route.Id);

			result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsNull();
		}

		[TestMethod]
		public void Add_Routes_With_Priorities_And_Unregister_First()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			controller.Register(route);

			var lessRoute =(RoutesHostServer.Models.Route)route.Clone();
			lessRoute.Priority = 2;
			controller.Register(lessRoute);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsEqualTo(route.WebApiAddress);

			controller.UnRegister(route.Id);

			result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsNotNull();
		}

		[TestMethod]
		public void Unregister_Service()
		{
			var controller = new RoutesHostServer.Controllers.RoutesController();

			var route = new RoutesHostServer.Models.Route();
			route.ApiKey = Guid.NewGuid().ToString();
			route.ServiceName = "Test";
			route.WebApiAddress = "http://test.com";
			controller.Register(route);

			var result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsEqualTo(route.WebApiAddress);

			controller.UnRegisterService(route.ApiKey, route.ServiceName);

			result = controller.Resolve(route.ApiKey, route.ServiceName);

			Check.That(result.Address).IsNull();
		}

	}
}
