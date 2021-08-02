using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Esprima.Ast;
using Esprima.Utils;
using Microsoft.CSharp.RuntimeBinder;
using MoreLinq;
using Binder = System.Reflection.Binder;
using Expression = System.Linq.Expressions.Expression;

namespace Runtime {
	public static class FuncCompiler {
		public static void Compile(JsFunc func) {
			Debug.Assert(func.Method == null);

			var def = func.Definition;
			var returnLabel = Expression.Label(typeof(object));

			var vars = new FuncVars();
			
			Expression Compile(Node node) {
				switch(node) {
					case BlockStatement bs:
						vars.Push();
						var block = Expression.Block(bs.Body.Select(Compile));
						vars.Pop();
						return block;
					case ReturnStatement rs:
						return Expression.Return(returnLabel,
							rs.Argument == null
								? Expression.Constant(null)
								: Expression.Convert(Compile(rs.Argument), typeof(object)));
					case BinaryExpression be:
						var left = Expression.Convert(Compile(be.Left), typeof(object));
						var right = Expression.Convert(Compile(be.Right), typeof(object));
						var bf = (Func<dynamic, dynamic, dynamic>) (be.Operator switch {
							BinaryOperator.Plus => (a, b) => a + b, 
							BinaryOperator.Minus => (a, b) => a - b, 
							BinaryOperator.Times => (a, b) => a * b, 
							BinaryOperator.Divide => (a, b) => a / b, 
							BinaryOperator.Equal => (a, b) => a == b, 
							BinaryOperator.NotEqual => (a, b) => a != b, 
							BinaryOperator.Less => (a, b) => a < b, 
							BinaryOperator.LessOrEqual => (a, b) => a <= b, 
							BinaryOperator.Greater => (a, b) => a > b, 
							BinaryOperator.GreaterOrEqual => (a, b) => a >= b, 
							_ => throw new NotImplementedException($"Unimplemented binary operation: {be.Operator}")
						});
						return bf.Call(left, right);
					case UpdateExpression ue:
						var uv = Compile(ue.Argument);
						var inc = ue.Operator == UnaryOperator.Increment;
						if(ue.Prefix)
							return Expression.Block(
								Expression.Assign(uv, inc ? uv.Apply(v => v + 1) : uv.Apply(v => v - 1)), 
								uv
							);
						else {
							var nvar = vars.DefConst();
							return Expression.Block(
								Expression.Assign(nvar, uv),
								Expression.Assign(uv, inc ? nvar.Apply(v => v + 1) : nvar.Apply(v => v - 1)),
								nvar
							);
						}
					case ExpressionStatement es:
						return Compile(es.Expression);
					case Literal literal:
						return Expression.Constant(literal.Value);
					case VariableDeclaration vd:
						var defBlock = new List<Expression>();
						foreach(var dec in vd.Declarations) {
							var name = ((Identifier) dec.Id).Name;
							var var = vd.Kind switch {
								VariableDeclarationKind.Let => vars.DefLet(name), 
								VariableDeclarationKind.Var => vars.DefVar(name), 
								VariableDeclarationKind.Const => vars.DefConst(name), 
								_ => throw new NotSupportedException($"Unknown variable declaration kind '{vd.Kind}'")
							};
							if(dec.Init == null) continue;
							defBlock.Add(Expression.Assign(var, Expression.Convert(Compile(dec.Init), typeof(object))));
						}
						return Expression.Block(defBlock);
					case AssignmentExpression ae:
						var aright = Compile(ae.Right);
						Expression BinAssign(string op) =>
							Compile(new BinaryExpression(op, ae.Left, ae.Right));
						var val = ae.Operator switch {
							AssignmentOperator.Assign => aright, 
							AssignmentOperator.PlusAssign => BinAssign("+"), 
							AssignmentOperator.MinusAssign => BinAssign("-"), 
							AssignmentOperator.TimesAssign => BinAssign("*"), 
							AssignmentOperator.DivideAssign => BinAssign("/"), 
							_ => throw new NotImplementedException($"Unsupported assignment operator '{ae.Operator}'")
						};
						val = Expression.Convert(val, typeof(object));
						return ae.Left switch {
							Identifier id => Expression.Assign(Compile(id), val), 
							ComputedMemberExpression cme => (Compile(cme.Object), Compile(cme.Property), val).Apply((obj, index, value) => obj[index] = value), 
							_ => throw new NotImplementedException($"Unimplemented assignment lvalue '{ae.Left}'")
						};
					case Identifier id:
						return vars.Get(id.Name);
					case ForStatement fs:
						var init = Compile(fs.Init);
						var loopEnd = Expression.Label();
						var test = Compile(fs.Test);
						var update = Compile(fs.Update);
						var body = Compile(fs.Body);
						return Expression.Block(
							init, 
							Expression.Loop(Expression.Block(
								Expression.IfThen(
									test.IsFalse(), Expression.Goto(loopEnd)
								), 
								update, 
								body
							), loopEnd));
					case ObjectExpression oe:
						return Expression.New(typeof(JsObject));
					case ComputedMemberExpression cme:
						return (Compile(cme.Object), Compile(cme.Property)).Apply((a, b) => a[b]);
					case StaticMemberExpression sme when sme.Property is Identifier id:
						var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, id.Name,
							typeof(FuncCompiler), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
						return Expression.Dynamic(binder, typeof(object), Compile(sme.Object));
					default:
						Console.WriteLine(AstJson.ToJsonString(node, "\t"));
						throw new NotImplementedException($"Unsupported type {node.GetType().Name}");
				}
			}
			
			var bodyBlock = Compile(def.Body);
			var wrapperBlock = Expression.Block(
				vars.AllVars, 
				bodyBlock, 
				Expression.Label(returnLabel, Expression.Constant(null))
			);

			func.Method = Expression.Lambda(wrapperBlock, null).Compile();
		}
	}
}