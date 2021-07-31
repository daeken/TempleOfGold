using System;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Esprima.Ast;

namespace Runtime {
	public class JsFunc : JsObject {
		public readonly ExecutionContext Context;
		public readonly IFunction Definition;
		public Delegate Method;

		public JsFunc(ExecutionContext ec, IFunction def) {
			Context = ec;
			Definition = def;
		}

		public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
			if(Method == null)
				FuncCompiler.Compile(this);
			result = Method.DynamicInvoke(args);
			return true;
		}
	}
}