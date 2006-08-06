using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

using Sprocket.SystemBase;
using Sprocket.Web;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Pages
{
	public partial class PageRequestHandler
	{
		HttpRequest Request
		{
			get { return HttpContext.Current.Request; }
		}
		HttpResponse Response
		{
			get { return HttpContext.Current.Response; }
		}

		public static PageRequestHandler Instance
		{
			get { return (PageRequestHandler)SystemCore.Instance["PageRequestHandler"]; }
		}

		private Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers = new Dictionary<string, IPlaceHolderRenderer>();
		public delegate void RegisteringPlaceHolderRenderers(Dictionary<string, IPlaceHolderRenderer> placeHolderRenderers);
		public event RegisteringPlaceHolderRenderers OnRegisteringPlaceHolderRenderers;
		internal Dictionary<string, IPlaceHolderRenderer> PlaceHolderRenderers
		{
			get { return placeHolderRenderers; }
		}
		private void RegisterPlaceHolderRenderers()
		{
			placeHolderRenderers.Add("xml", new XmlPlaceHolderRenderer());
			placeHolderRenderers.Add("externalxml", new ExternalXmlPlaceHolderRenderer());
			placeHolderRenderers.Add("embed", new EmbedPlaceHolderRenderer());
			placeHolderRenderers.Add("list", new ListPlaceHolderRenderer());
			placeHolderRenderers.Add("pageentry", new PageEntryPlaceHolderRenderer());
			placeHolderRenderers.Add("path", new PathPlaceHolderRenderer());
			if (OnRegisteringPlaceHolderRenderers != null)
				OnRegisteringPlaceHolderRenderers(placeHolderRenderers);
		}

		private Dictionary<string, IOutputFormatter> outputFormatters = new Dictionary<string, IOutputFormatter>();
		public delegate void RegisteringOutputFormatters(Dictionary<string, IOutputFormatter> outputFormatters);
		public event RegisteringOutputFormatters OnRegisteringOutputFormatters;
		internal Dictionary<string, IOutputFormatter> OutputFormatters
		{
			get { return outputFormatters; }
		}
		private void RegisterOutputFormatters()
		{
			outputFormatters.Add("", new DefaultOutputFormatter());
			outputFormatters.Add("DateTime", new DateTimeOutputFormatter());
			if (OnRegisteringOutputFormatters != null)
				OnRegisteringOutputFormatters(outputFormatters);
		}

		void OnLoadRequestedPath(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (handled.Handled) return;

			switch (sprocketPath)
			{
				case "$reset":
					PageRegistry.UpdateValues();
					TemplateRegistry.Reload();
					ListRegistry.Reload();
					OutputFormatRegistry.Reload();
					GeneralRegistry.Reload();
					ContentCache.ClearCache();
					WebUtility.Redirect("");
					break;

				default:
					PageRegistry.CheckDate();

					PageEntry page = PageRegistry.Pages.FromPath(sprocketPath);
					if(page == null)
						return;
					string output = page.Render();
					if (output == null)
						return;
					Response.Write(output);
					break;
			}

			handled.Set();
		}

		void OnPathNotFound(HttpApplication app, string sprocketPath, string[] pathSections, HandleFlag handled)
		{
			if (!sprocketPath.Contains(".")) return;
			string urlpath;
			if (pathSections.Length == 1)
				urlpath = "";
			else
				urlpath = sprocketPath.Substring(0, sprocketPath.Length - pathSections[pathSections.Length - 1].Length - 1);
			XmlElement node = (XmlElement)PagesXml.SelectSingleNode("//Page[@Path='" + urlpath + "']");
			if (node == null) return;
			string newurl = "resources/content/" + node.GetAttribute("ContentFile");
			newurl = WebUtility.BasePath + newurl.Substring(0, newurl.LastIndexOf('/') + 1) + pathSections[pathSections.Length - 1];
			if (!File.Exists(HttpContext.Current.Server.MapPath(newurl)))
				return;
			HttpContext.Current.Response.TransmitFile(HttpContext.Current.Server.MapPath(newurl));
			handled.Set();
		}

		private static Dictionary<string, XmlDocument> xmlCache = null;
		public static Dictionary<string, XmlDocument> XmlCache
		{
			get { return PageRequestHandler.xmlCache; }
		}

		internal XmlDocument GetXmlDocument(string sprocketPath)
		{
			if (XmlCache.ContainsKey(sprocketPath))
				return XmlCache[sprocketPath];
			XmlDocument doc = new XmlDocument();
			string path = WebUtility.MapPath(sprocketPath);
			if (!File.Exists(path))
				return null;
			doc.Load(path);
			XmlCache.Add(sprocketPath, doc);
			return doc;
		}

		void OnEndHttpRequest(HttpApplication app)
		{
			if (xmlCache != null)
				xmlCache.Clear();
			xmlCache = null;
		}

		void OnBeginHttpRequest(HttpApplication app, HandleFlag handled)
		{
			xmlCache = new Dictionary<string, XmlDocument>();
		}

		XmlDocument PagesXml
		{
			get
			{
				XmlDocument pages = null;
				string path = WebUtility.MapPath("resources/definitions/pages.xml");
				if (!File.Exists(path))
					return null;

				FileInfo pgxmlfile = new FileInfo(path);
				HttpApplicationState app = HttpContext.Current.Application;
				app.Lock();
				if (app["PagesXmlModified"] != null && app["PagesXmlDocument"] != null)
					if ((DateTime)app["PagesXmlModified"] == pgxmlfile.LastWriteTime)
						pages = (XmlDocument)app["PagesXmlDocument"];
				if (pages == null)
				{
					pages = new XmlDocument();
					pages.Load(path);
					app["PagesXmlModified"] = pgxmlfile.LastWriteTime;
					app["PagesXmlDocument"] = pages;
				}
				app.UnLock();
				return pages;
			}
		}

		XmlDocument ListsXml
		{
			get
			{
				XmlDocument lists = null;
				string path = WebUtility.MapPath("resources/definitions/lists.xml");
				FileInfo xmlfile = new FileInfo(path);
				HttpApplicationState app = HttpContext.Current.Application;
				app.Lock();
				if (app["ListsXmlModified"] != null && app["ListsXmlDocument"] != null)
					if ((DateTime)app["ListsXmlModified"] == xmlfile.LastWriteTime)
						lists = (XmlDocument)app["ListsXmlDocument"];
				if (lists == null)
				{
					lists = new XmlDocument();
					lists.Load(path);
					app["ListsXmlModified"] = xmlfile.LastWriteTime;
					app["ListsXmlDocument"] = lists;
				}
				app.UnLock();
				return lists;
			}
		}
	}
}