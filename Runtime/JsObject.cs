using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Runtime {
	public class JsObject : DynamicObject {
		public readonly dynamic BaseObject;
		public readonly Dictionary<string, dynamic> HashTable = new();

		public JsObject() => BaseObject = null;
		public JsObject(dynamic baseObject) => BaseObject = baseObject;

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
			if(indexes.Length != 1) throw new NotSupportedException();

			var index = indexes[0] is string sindex ? sindex : indexes[0].ToString();

			if((object) BaseObject != null)
				try {
					var val = BaseObject[index];
					if(val != JsUndefined.Instance) {
						result = val;
						return true;
					}
				} catch(Exception) {}

			if(HashTable.TryGetValue(index, out result)) return true;

			result = JsUndefined.Instance;
			return true;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
			if(indexes.Length != 1) throw new NotSupportedException();

			var index = indexes[0] is string sindex ? sindex : indexes[0].ToString();

			if((object) BaseObject != null)
				try {
					BaseObject[index] = value;
					return true;
				} catch(Exception) {}

			HashTable[index] = value;
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			var memberName = binder.Name;
			
			if((object) BaseObject != null)
				try {
					var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
					var val = callsite.Target(callsite, BaseObject);
					if(val != JsUndefined.Instance) {
						result = val;
						return true;
					}
				} catch(Exception) {}
			
			if(HashTable.TryGetValue(memberName, out result)) return true;

			result = JsUndefined.Instance;
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value) {
			if((object) BaseObject != null)
				try {
					var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
					callsite.Target(callsite, BaseObject, value);
					return true;
				} catch(Exception) {}

			HashTable[binder.Name] = value;
			return true;
		}
	}
}