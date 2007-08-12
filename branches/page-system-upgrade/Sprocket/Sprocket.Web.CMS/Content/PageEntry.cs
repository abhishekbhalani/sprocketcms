using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sprocket.Web;
using Sprocket.Web.Cache;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class PageEntry : IPropertyEvaluatorExpression
	{
		#region Fields and Properties

		private string path = "", template = "", pageCode = "", contentType = "text/html";
		private Guid internalID = Guid.NewGuid();
		private PageEntry parent = null;
		private List<PageEntry> pages = new List<PageEntry>();
		private bool handleSubPaths = false;

		private TemplateRegistry sourceTemplateRegistry = null;

		public bool HandleSubPaths
		{
			get { return handleSubPaths; }
			set { handleSubPaths = value; }
		}

		public string ContentType
		{
			get { return contentType; }
			set { contentType = value; }
		}

		public List<PageEntry> Pages
		{
			get { return pages; }
			set { pages = value; }
		}

		public string PageCode
		{
			get { return pageCode; }
			set { pageCode = value; }
		}

		public Guid InternalID
		{
			get { return internalID; }
			set { internalID = value; }
		}

		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		public string TemplateName
		{
			get { return template; }
			set { template = value; }
		}

		public Template Template
		{
			get { return sourceTemplateRegistry[template]; }
		}

		public PageEntry Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		#endregion

		public PageEntry(TemplateRegistry sourceTemplateRegistry)
		{
			this.sourceTemplateRegistry = sourceTemplateRegistry;
		}

		public string Render(ExecutionState state)
		{
			return Template.RenderIsolated(state);
		}

		public string Render(params KeyValuePair<string, object>[] preloadedVariables)
		{
			string cachePath;
			if (pageCode != null && pageCode != "")
				cachePath = "$pagecode[" + PageCode + "]";
			else
				cachePath = path;

			ContentManager.PageStack.Push(this);
			Template t = Template;
			string output;
			if (t == null)
				output = "[Unable to render page; Referenced template name has not been defined]";
			else
				output = t.Render(preloadedVariables);
			ContentManager.PageStack.Pop();
			return output;
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "path":
				case "code":
				case "templatename":
				case "contenttype":
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "path": return path;
				case "code": return pageCode;
				case "templatename": return template;
				case "contenttype": return contentType;
				default: return VariableExpression.InvalidProperty;
			}
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			Token t = state.SourceToken;
			state.SourceToken = contextToken.Next;
			string s = Render(state);
			state.SourceToken = t;
			return s;
		}
	}
}
