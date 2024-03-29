using System;
using System.Collections;
using System.Web;

using Sprocket;
using Sprocket.Web;
using Sprocket.Utility;

namespace Sprocket.Web
{
	public delegate Result LoginAuthenticationHandler(string username, string passwordHash);
	public delegate bool PermissionVerificationHandler(string permissionTypeCode);

	[ModuleDependency(typeof(WebClientScripts))]
	[ModuleDescription("Provides an interface for authenticating web requests.")]
	[ModuleTitle("Web Authentication Manager")]
	public class WebAuthentication : ISprocketModule
	{
		private const string cookieKey = "Sprocket_Auth_Key";
		public delegate void AjaxAuthKeyStoredHandler(string username, Guid authKey);
		public event AjaxAuthKeyStoredHandler OnAjaxAuthKeyStored;
		public LoginAuthenticationHandler Authenticate = null;
		public PermissionVerificationHandler VerifyUserAccess = null;

		private Hashtable usersByKey = new Hashtable();
		private Hashtable keysByUser = new Hashtable();

		public static WebAuthentication Instance
		{
			get { return (WebAuthentication)Core.Instance[typeof(WebAuthentication)].Module; }
		}

		public void AttachEventHandlers(ModuleRegistry registry)
		{
			if (registry.IsRegistered("WebClientScripts"))
				WebClientScripts.Instance.OnBeforeRenderJavaScript += new Sprocket.Web.WebClientScripts.BeforeRenderJavaScriptHandler(OnPreRenderJavaScript);
		}

		public bool CheckAjaxAuthKey(Guid key)
		{
			return usersByKey.ContainsKey(key);
		}

		public string GetUsername(Guid key)
		{
			if (!usersByKey.ContainsKey(key))
				return "";
			return usersByKey[key].ToString();
		}

		public string GetAuthKey(string username)
		{
			if (!keysByUser.ContainsKey(username))
				return "";
			return keysByUser[username].ToString();
		}

		public Result ValidateLogin(string username, string passwordHash)
		{
			Result result = new Result();
			if (Authenticate != null)
				return Authenticate(username, passwordHash);
			return result;
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid)
		{
			WriteAuthenticationCookie(username, passwordHash, ajaxGuid, 525600);
		}

		public void WriteAuthenticationCookie(string username, string passwordHash, Guid ajaxGuid, int timeoutMinutes)
		{
			string key = PassKeyFromPasswordHash(passwordHash);
			HttpCookie cookie = new HttpCookie(cookieKey);
			cookie.Values.Add("a", username);
			cookie.Values.Add("k", key);
			cookie.Values.Add("c", Guid.NewGuid().ToString());
			cookie.Expires = DateTime.Now.AddMinutes(timeoutMinutes);
#warning how is this affected by UTC?
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		private string PasswordHashFromPassKey(string passKey)
		{
			string startIP = HttpContext.Current.Request.UserHostAddress;
			startIP = startIP.Substring(0, startIP.LastIndexOf('.'));
			string encKey = SprocketSettings.GetValue("EncryptionKeyWord");
			if (encKey == null)
				throw new Exception("Please add a kay named \"EncryptionKeyWord\" to your Web.Config file. This is a secret keyword or phrase of your choice.");
			return Crypto.RC2Decrypt(StringUtilities.BytesFromHexString(passKey), encKey, startIP);
		}

		private string PassKeyFromPasswordHash(string passwordHash)
		{
			string startIP = HttpContext.Current.Request.UserHostAddress;
			startIP = startIP.Substring(0, startIP.LastIndexOf('.'));
			string encKey = SprocketSettings.GetValue("EncryptionKeyWord");
			return StringUtilities.HexStringFromBytes(Crypto.RC2Encrypt(passwordHash, encKey, startIP));
		}

		public void ClearAuthenticationCookie()
		{
			HttpCookie cookie = new HttpCookie(cookieKey);
			cookie.Expires = DateTime.Now.AddYears(-1);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		public static bool IsLoggedIn
		{
			get
			{
				if (CurrentRequest.Value["CurrentUser_Authenticated"] == null)
				{
					string a, k;
					Guid guid;
					ReadLoginInfo(out a, out k, out guid);
					//HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieKey];
					//bool noCookie = false;
					//if (cookie == null || cookie.Value == "")
					//    noCookie = true;

					string passkey;
					bool result;
					try
					{
						//string a, k;
						//if (noCookie)
						//{
						//    a = HttpContext.Current.Request.QueryString["$usr"];
						//    k = HttpContext.Current.Request.QueryString["$key"];
						//}
						//else
						//{
						//    a = cookie["a"];
						//    k = cookie["k"];
						//}
						CurrentRequest.Value["WebAuthentication.CurrentUsername"] = a;
						WebAuthentication auth = Instance;
						passkey = auth.PasswordHashFromPassKey(k);
						result = auth.ValidateLogin(a, passkey).Succeeded;
					}
					catch
					{
						result = false;
					}

					CurrentRequest.Value["CurrentUser_Authenticated"] = result;
					return result;
				}
				else
					return (bool)CurrentRequest.Value["CurrentUser_Authenticated"];
			}
		}

		private static void ReadLoginInfo(out string username, out string passwordHash, out Guid ajaxKey)
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieKey];
			bool noCookie = false;
			if (cookie == null || cookie.Value == "")
				noCookie = true;

			if (noCookie)
			{
				username = HttpContext.Current.Request.QueryString["$usr"];
				passwordHash = HttpContext.Current.Request.QueryString["$key"];
				ajaxKey = Guid.Empty;
			}
			else
			{
				username = cookie["a"];
				passwordHash = cookie["k"];
				try
				{
					ajaxKey = new Guid(cookie["c"]);
				}
				catch
				{
					ajaxKey = Guid.Empty;
				}
			}
		}

		/// <summary>
		/// Some modules provide functionality that can be divided into areas with differing security requirements.
		/// The security provider in use should determine if the current end user, whether authenticated or not,
		/// has been assigned (directly or indirectly) the specified permission type code.
		/// </summary>
		/// <param name="permissionTypeCode">A unique string specifying the permission to check for</param>
		/// <returns>True if the current user has access, otherwise False</returns>
		public static bool VerifyAccess(string permissionTypeCode)
		{
			WebAuthentication auth = Instance;
			if (auth.VerifyUserAccess == null)
				return true;
			if (!IsLoggedIn)
				return false;
			return auth.VerifyUserAccess(permissionTypeCode);
		}

		public static bool VerifyAccess(Enum permissionType)
		{
			return VerifyAccess(permissionType.GetType().Name + "." + permissionType.ToString());
		}

		public bool ProcessLoginForm(string usernameFieldName, string passwordFieldName, string preserveLoginCheckboxName)
		{
			return ProcessLoginForm(usernameFieldName, passwordFieldName, HttpContext.Current.Request.Form[preserveLoginCheckboxName] != null);
		}

		public bool ProcessLoginForm(string usernameFieldName, string passwordFieldName, bool persistLogin)
		{
			string u = HttpContext.Current.Request.Form[usernameFieldName];
			string p = HttpContext.Current.Request.Form[passwordFieldName];
			int timeout = persistLogin ? 525600 : 60;
			string hash = p == "" ? "" : Crypto.EncryptOneWay(p);
			if (ValidateLogin(u, hash).Succeeded)
			{
				WriteAuthenticationCookie(u, hash, StoreAjaxAuthKey(u), timeout);
				return true;
			}
			ClearAuthenticationCookie();
			return false;
		}

		public void QuickLogin(string username, string password)
		{
			QuickLogin(username, password, false);
		}

		public Guid QuickLogin(string username, string password, bool persistLogin)
		{
			Guid auth = StoreAjaxAuthKey(username);
			WriteAuthenticationCookie(username, Crypto.EncryptOneWay(password), auth, persistLogin ? 525600 : 60);
			CurrentRequest.Value["CurrentUser_Authenticated"] = true;
			return auth;
		}

		public Guid StoreAjaxAuthKey(string username)
		{
			Guid key = Guid.NewGuid(); //new unique key for the user
			usersByKey.Add(key, username); //add the new key to the list
			if (keysByUser.ContainsKey(username)) //if an existing login for this user exists, remove it
			{
				usersByKey.Remove(keysByUser[username]); //only one login window at a time, please
				keysByUser.Remove(username); //remove the old username-to-key mapping
			}
			keysByUser.Add(username, key); //add a new username-to-key mapping

			if (OnAjaxAuthKeyStored != null)
				OnAjaxAuthKeyStored(username, key);

			return key;
		}

		public string CurrentUsername
		{
			get
			{
				HttpCookie cookie = HttpContext.Current.Request.Cookies[cookieKey];
				if (cookie == null)
					return HttpContext.Current.Request.QueryString["$usr"];
				return cookie["a"];
			}
		}

		private void OnPreRenderJavaScript(JavaScriptCollection scripts)
		{
			HttpContext c = HttpContext.Current;
			HttpCookie authcookie = c.Request.Cookies[cookieKey];
			if (authcookie == null) return;
			scripts.SetKey(AuthKeyPlaceholder, authcookie["c"]);
		}

		public static string AuthKeyPlaceholder
		{
			get { return "$AUTHKEY$"; }
		}
	}
}
