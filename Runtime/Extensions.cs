using System;
using System.Linq.Expressions;

namespace Runtime {
	public static class Extensions {
		public static Expression Call<T1, TR>(this Func<T1, TR> functor, Expression e1) =>
			Expression.Call(Expression.Constant(functor.Target), functor.Method, e1);

		public static Expression Call<T1, T2, TR>(this Func<T1, T2, TR> functor, Expression e1, Expression e2) =>
			Expression.Call(Expression.Constant(functor.Target), functor.Method, e1, e2);

		public static Expression Call<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> functor, Expression e1, Expression e2, Expression e3) =>
			Expression.Call(Expression.Constant(functor.Target), functor.Method, e1, e2, e3);

		public static Expression IsFalse(this Expression expr) =>
			((Func<dynamic, bool>) (v => v switch {
				bool bv => !bv, 
				_ => false
			})).Call(expr);

		public static Expression Apply(this Expression expr, Func<dynamic, dynamic> functor) =>
			functor.Call(expr);
		
		public static Expression Apply(this (Expression, Expression) exprs, Func<dynamic, dynamic, dynamic> functor) =>
			functor.Call(exprs.Item1, exprs.Item2);
		
		public static Expression Apply(this (Expression, Expression, Expression) exprs, Func<dynamic, dynamic, dynamic, dynamic> functor) =>
			functor.Call(exprs.Item1, exprs.Item2, exprs.Item3);
	}
}