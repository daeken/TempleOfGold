using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Runtime {
	public class FuncVars {
		public readonly Dictionary<string, ParameterExpression> TopLevel = new();
		public readonly Stack<Dictionary<string, ParameterExpression>> Scopes = new();
		public readonly List<ParameterExpression> AllVars = new();

		int AnonymousId = 0;

		public FuncVars() => Scopes.Push(new());

		public string Anonymous => $"$__anonymous_{AnonymousId++}";

		public void Push() => Scopes.Push(new(Scopes.Peek()));
		public void Pop() => Scopes.Pop();

		ParameterExpression MakeVar(string name) {
			var var = Expression.Variable(typeof(object), name);
			AllVars.Add(var);
			return var;
		}

		public ParameterExpression Get(string name) {
			if(Scopes.Peek().TryGetValue(name, out var scopeVar)) return scopeVar;
			if(TopLevel.TryGetValue(name, out var tlVar)) return tlVar;
			return null;
		}

		public ParameterExpression DefVar(string name = null) {
			name ??= Anonymous;
			if(Get(name) != null) throw new Exception($"Variable '{name}' already defined");
			return TopLevel[name] = MakeVar(name);
		}
		
		public ParameterExpression DefLet(string name = null) {
			name ??= Anonymous;
			if(Get(name) != null) throw new Exception($"Variable '{name}' already defined");
			return Scopes.Peek()[name] = MakeVar(name);
		}
		
		public ParameterExpression DefConst(string name = null) {
			name ??= Anonymous;
			if(Get(name) != null) throw new Exception($"Variable '{name}' already defined");
			return Scopes.Peek()[name] = MakeVar(name);
		}
	}
}