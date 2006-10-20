using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Sprocket.Web;
using Sprocket.Data;
using Sprocket;

namespace Sprocket.Web
{
	[ModuleDependency(typeof(DatabaseManager))]
	[ModuleDependency(typeof(WebEvents))]
	[ModuleDescription("Provides a web interface for database setip and initialisation")]
	[ModuleTitle("Database Setup Interface")]
	public class DatabaseSetup : ISprocketModule
	{
		void Instance_OnLoadRequestedPath(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (handled.Handled) return;
			if (sprocketPath == "$dbsetup")
			{
				Result result = DatabaseManager.DatabaseEngine.Initialise();
				if(result.Succeeded)
					HttpContext.Current.Response.Write("<p>Database setup completed.</p>");
				else
					HttpContext.Current.Response.Write("<h2>Unable to Initialise Database</h2><p>" + result.Message + "</p>");
				handled.Set();
			}
		}

		public static DatabaseSetup Instance
		{
			get { return (DatabaseSetup)Core.Instance[typeof(DatabaseSetup)].Module; }
		}

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(Instance_OnLoadRequestedPath);
		}

		#endregion
	}
}
