using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Transactions;
using System.Web;

using Sprocket;
using Sprocket.Data;
using Sprocket.Mail;
using Sprocket.Utility;
using Sprocket.Web;

namespace Sprocket.Security
{
	[ModuleDependency(typeof(DatabaseManager))]
	[ModuleDependency(typeof(EmailHandler))]
	[ModuleTitle("Sprocket Security Provider")]
	[ModuleDescription("The default security implementation for Sprocket. Handles users, roles and permissions.")]
	public partial class SecurityProvider : ISprocketModule
	{
		public static SecurityProvider Instance
		{
			get { return (SecurityProvider)Core.Instance[typeof(SecurityProvider)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(Instance_OnDatabaseHandlerLoaded);
			WebEvents.Instance.OnBeforeLoadExistingFile += new WebEvents.RequestedPathEventHandler(Instance_OnBeforeLoadExistingFile);
		}

		ISecurityProviderDataLayer dataLayer = null;
		public ISecurityProviderDataLayer DataLayer
		{
			get { return dataLayer; }
		}

		void Instance_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(ISecurityProviderDataLayer)))
			{
				ISecurityProviderDataLayer layer = (ISecurityProviderDataLayer)Activator.CreateInstance(t);
				if (layer.DatabaseHandlerType == source.GetType())
				{
					dataLayer = layer;
					break;
				}
			}
			source.OnInitialise += new InterruptableEventHandler(Database_OnInitialise);
		}

		void Database_OnInitialise(Result result)
		{
			if (dataLayer == null)
				result.SetFailed("SecurityProvider has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
			{
				Result r = dataLayer.InitialiseDatabase();
				if (!r.Succeeded)
					result.SetFailed(r.Message);
				long forceClientInit = ClientSpaceID;
			}
		}

		void Instance_OnBeforeLoadExistingFile(System.Web.HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (sprocketPath.ToLower() == "datastore/clientspace.id") // deny access
				handled.Set();
		}

		private static long clientSpaceID = -1;
		public static long ClientSpaceID
		{
			get
			{
				lock (WebUtility.GetSyncObject("DefaultClientSpaceID"))
				{
					if (clientSpaceID != -1)
						return clientSpaceID;
					string path = WebUtility.MapPath("datastore/ClientSpace.ID");
					if (!File.Exists(path))
					{
						clientSpaceID = DatabaseManager.GetUniqueID();
						Result result = Instance.dataLayer.InitialiseClientSpace(clientSpaceID);
						if (!result.Succeeded)
							throw new Exception(result.Message);
						using (FileStream file = File.OpenWrite(path))
						{
							file.Write(BitConverter.GetBytes(clientSpaceID), 0, sizeof(long));
							file.Close();
						}
					}
					else
					{
						byte[] bytes = new byte[sizeof(long)];
						using (FileStream file = File.OpenRead(path))
						{
							file.Read(bytes, 0, bytes.Length);
							file.Close();
						}
						clientSpaceID = BitConverter.ToInt64(bytes, 0);
					}
					return clientSpaceID;
				}
			}
		}

		//public static class RoleCodes
		//{
		//    public static readonly string SuperUser = "SUPERUSER";
		//}

		//public static class PermissionTypeCodes
		//{
		//    public static readonly string UserAdministrator = "USERADMINISTRATOR";
		//    public static readonly string RoleAdministrator = "ROLEADMINISTRATOR";
		//}
	}
}
