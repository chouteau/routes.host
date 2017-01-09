using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RoutesHostServer
{
	public class AppViewEngine : RazorViewEngine
	{
		public AppViewEngine() : base()
		{
			base.ViewLocationFormats = new string[]
			{
				"/wwwroot/app/{0}.cshtml",
				"/wwwroot/app/{1}/{0}.cshtml",
			};
		}
	}
}