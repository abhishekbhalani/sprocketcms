using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content.Expressions
{
	public class BasePathExpression : IExpression
	{
		public object Evaluate(ExecutionState state) { return WebUtility.BasePath; }
		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
		}
	}

	public class BasePathExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "basepath"; } }
		public IExpression Create() { return new BasePathExpression(); }
	}
}
