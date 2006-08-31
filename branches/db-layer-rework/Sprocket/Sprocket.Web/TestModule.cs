using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Sprocket;
using Sprocket.SystemBase;
using Sprocket.Utility;

namespace Sprocket.Web
{
	[AjaxMethodHandler()]
	[ModuleDependency("WebEvents")]
	class TestModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(OnLoadRequestedPath);
		}

		void OnLoadRequestedPath(HttpApplication app, string path, string[] pathSections, HandleFlag handled)
		{
			if (path != "test")
				return;
			handled.Set();

			string html = WebUtility.AbsoluteBasePath;
			string scripts = ((WebClientScripts)SystemCore.Instance["WebClientScripts"]).BuildScriptTags();
			HttpContext.Current.Response.Write(scripts + html.Replace(Environment.NewLine, "<br />"));
		}

		[AjaxMethod()]
		public string[] MethodTest(string x, int y, string[] z)
		{
			return new string[] { "{x:" + x + "}", "{y:" + y + "}", "{z.length:" + z.Length + "}" };
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "TestModule"; }
		}

		public string Title
		{
			get { return "Testing Module"; }
		}

		public string ShortDescription
		{
			get { return "A module for writing test code."; }
		}
	}
}
