using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;
using Sprocket.Web.CMS.Script.Parser;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class TemplateExpression : IFunctionExpression
	{
		IExpression expr = null;
		Token token = null;

		public object Evaluate(ExecutionState state)
		{
			if (expr == null)
				throw new InstructionExecutionException("I can't show a template because you didn't specify which one to show here.", token);
			string name = expr.Evaluate(state).ToString();
			Template t = ContentManager.Templates[name];
			if (name == null)
				throw new InstructionExecutionException("I can't show the template because the one you specified doesn't seem to exist.", token);
			return t.Script.ExecuteToResolveExpression(state);
		}

		public void PrepareExpression(Token expressionToken, List<Token> tokens, ref int nextIndex, Stack<int?> precedenceStack)
		{
		}

		public void SetFunctionArguments(List<FunctionArgument> arguments, Token functionCallToken)
		{
			token = functionCallToken;
			if(arguments.Count != 1)
				throw new TokenParserException("The \"template\" expression requires one argument specifying which template to load", token);
			expr = arguments[0].Expression;
		}
	}

	class TemplateExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "template"; }
		}

		public IExpression Create()
		{
			return new TemplateExpression();
		}
	}
}