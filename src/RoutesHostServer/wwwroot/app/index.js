(function ()
{
    'use strict';

	angular
		.module('routesHost', [
			'ngResource',
			'ngMaterial',
			'ngAria',
			'ngMessages',
			'ngCookies',
			'ui.router'
		])
		.run(runBlock)
		.config(routeConfig)
		.factory('webApiService', webApiService)
		.controller('LoginController', LoginController)
		.controller('RoutesController', RoutesController)
		.controller('ResolveRouteController', ResolveRouteController);

    function runBlock($rootScope, $timeout, $state, $cookies, $location, $http)
    {
        // Activate loading indicator
        var stateChangeStartEvent = $rootScope.$on('$stateChangeStart', function ()
        {
            $rootScope.loadingProgress = true;
        });

        // De-activate loading indicator
        var stateChangeSuccessEvent = $rootScope.$on('$stateChangeSuccess', function ()
        {
            $timeout(function ()
            {
                $rootScope.loadingProgress = false;
            });
        });

        // Store state in the root scope for easy access
        $rootScope.state = $state;

        // Cleanup
        $rootScope.$on('$destroy', function ()
        {
            stateChangeStartEvent();
            stateChangeSuccessEvent();
        })

		if ($cookies.get('AdminRoutesHost') == null) {
			$location.path('/login');
		}
		else
		{
			$http({
				method: 'GET',
				url: '/api/admin/userinfo'
			}).then(function(response) {
				$rootScope.loggedUser = response.data;
				$rootScope.$broadcast('userLogged');
			});
		}
    }

	function routeConfig($stateProvider, $urlRouterProvider, $locationProvider)
    {
        $locationProvider.html5Mode(false).hashPrefix('!');

		$urlRouterProvider.otherwise('/resolve');

        // State definitions
		$stateProvider
			.state('app', {
				abstract: true,
				views: {
					'main@': {
						templateUrl: '/wwwroot/app/layouts/vertical.html',
						controller: 'MainController as vm'
					},
					'navigation@app': {
						templateUrl: '/wwwroot/app/layouts/navigation.html',
						controller: 'NavigationController as vm'
					}
				}
			})
			.state('app.pages_auth_login', {
				url: '/login',
				views: {
					'main@': {
						templateUrl: '/wwwroot/app/layouts/content-only.html'
					},
					'content@app.pages_auth_login': {
						templateUrl: '/wwwroot/app/auth/login.html',
						controller: 'LoginController as vm'
					}
				}
			})
			.state('app.resolve', {
				url: '/resolve',
				views: {
					'main@': {
						templateUrl: '/wwwroot/app/layouts/vertical.html'
					},
					'content@app.resolve': {
						templateUrl: '/wwwroot/app/main/resolve.html',
						controller: 'ResolveRouteController as vm'
					}
				}
			})
			.state('app.routes', {
				url: '/routes',
				views: {
					'main@': {
						templateUrl: '/wwwroot/app/layouts/vertical.html'
					},
					'content@app.routes': {
						templateUrl: '/wwwroot/app/main/routes.html',
						controller: 'RoutesController as vm'
					}
				}
			});
    }

	function webApiService($rootScope, $log, $http) {

		var svc = {
			authenticate : authenticate,
			getRoutes : getRoutes,
			unRegister : unRegister,
			unRegisterService: unRegisterService,
			resolve : resolve,
			waitForUser: waitForUser
		};

		return svc;

		function authenticate(apiKey) {
			var result = $http({
				method: 'POST',
				url: '/api/admin/authenticate',
				data: {
					key : apiKey
				}
			});
			return result;
		}

		function getRoutes() {
			var result = $http({
				method : 'GET',
				url : '/api/admin/routelist',
				headers : {
					'Authorization' : $rootScope.loggedUser.key
				}
			});
			return result;
		}

		function unRegister(id) {
			var result = $http({
				method : 'DELETE',
				url : '/api/routes/unregister/' + id
			});
			return result;
		}

		function unRegisterService(apiKey, serviceName) {
			var result = $http({
				method : 'DELETE',
				url : '/api/routes/unregisterservice',
				params : {
					apiKey : apiKey,
					serviceName : serviceName
				}
			});
			return result;
		}

		function resolve(apiKey, serviceName) {
			var result = $http({
				method: 'GET',
				url: '/api/routes/resolve',
				params: {
					apiKey: apiKey,
					serviceName: serviceName,
					useProxy : false
				}
			});
			return result;
		}

		function waitForUser(callback) {
			$rootScope.loadingProgress = true;

			if ($rootScope.loggedUser != null) {
				callback.apply(this, []);
			}
			else {
				$rootScope.$on('userLogged', callback);
			}
		}
	}

	function LoginController($rootScope, $location, webApiService)
    {
        var vm = this;
		
		vm.form = {
			apiKey : null
		};

		vm.authenticate = authenticate;

		function authenticate() {
			webApiService.authenticate(vm.form.apiKey).then(function(response) {
				$rootScope.loggedUser = response.data;
				$rootScope.$broadcast('userLogged');
				$location.path('/');
			});
		}
    }

	function RoutesController($rootScope, $location, webApiService) {
		var vm = this;

		vm.routes = [];

		vm.deleteRoute = deleteRoute;
		vm.deleteService = deleteService;

		function loadRoutes() {
			webApiService.getRoutes().then(function(response) {
				angular.forEach(response.data, function(route) {
					vm.routes.push(route);
				});
			});
		}

		function deleteService(service) {
			console.log(service);
			var key = service.key.split('|');
			var apiKey = key[0];
			var serviceName = key[1];
			webApiService.unRegisterService(apiKey, serviceName).then(function(response) {
				var index = vm.routes.indexOf(service);
				vm.routes.splice(index,1);
			});
		}

		function deleteRoute(service, route) {
			webApiService.unRegister(route.id).then(function(response) {
				var index = service.value.indexOf(route);
				service.value.splice(index,1);
			});
		}

		webApiService.waitForUser(function() 
		{
			loadRoutes();
		});
	}

	function ResolveRouteController($rootScope, $location, webApiService) {
		var vm = this;

		vm.apiKey = null;
		vm.serviceName = null;
		vm.routeList = [];

		vm.resolveRoute = resolveRoute;

		function resolveRoute() {
			console.log('try to resolve route');
			webApiService.resolve(vm.apiKey, vm.serviceName).then(function (response) {
				vm.routeList = response.data;
			});
		}
	}

})();