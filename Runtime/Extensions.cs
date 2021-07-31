using System;
using System.Linq.Expressions;

namespace Runtime {
	public static class Extensions {
		public static Expression Call<T1, TR>(this Func<T1, TR> functor, Expression e1) =>
			Expression.Call(Expression.Constant(functor.Target), functor.Method, e1);

		public static Expression Call<T1, T2, TR>(this Func<T1, T2, TR> functor, Expression e1, Expression e2) =>
			Expression.Call(Expression.Constant(functor.Target), functor.Method, e1, e2);

		public static Expression IsFalse(this Expression expr) =>
			((Func<dynamic, bool>) (v => v switch {
				bool bv => !bv, 
				_ => false
			})).Call(expr);
	}
}