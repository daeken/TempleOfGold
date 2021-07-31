using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Esprima;
using Esprima.Ast;
using Esprima.Utils;
using MoreLinq;

namespace Runtime {
	public class ExecutionContext {
		public readonly Dictionary<string, JsObject> Values = new();

		public JsObject this[string name] {
			get => Values[name];
			set => Values[name] = value;
		}

		public dynamic Eval(string code) {
			var ast = new JavaScriptParser(code).ParseScript();
			var body = ast.Body.ToList();
			if(body.Count != 0 && body[^1] is ExpressionStatement es)
				body[^1] = new ReturnStatement(es.Expression);
			ast.Body.AsNodes();
			var funcAst = new FunctionExpression(null, new NodeList<Expression>(),
				new BlockStatement(NodeList.Create(body)), false, false,
				false);
			var func = new JsFunc(this, funcAst);
			return ((dynamic) func)();
		}
	}
}