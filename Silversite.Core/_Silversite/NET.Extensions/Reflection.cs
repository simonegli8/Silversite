using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Silversite.Reflection {

	public class Method {

		public object Object { get; set; }
		public MethodInfo Info { get; set; }
		public object[] Parameters { get; set; }

		public object Invoke(params object[] p) { return Info.Invoke(Object, p); }
		public object Invoke() { if (Parameters != null) return Info.Invoke(Object, Parameters); return Info.Invoke(Object, null); }
		public T Invoke<T>(params object[] p) { return (T)Invoke(p); }
		public T Invoke<T>() { if (Parameters != null) return Invoke<T>(Parameters); return (T)Invoke(); }

		public object Call(params object[] p) { return Info.Invoke(Object, p); }
		public object Call() { return Info.Invoke(Object, null); }
		public T Call<T>(params object[] p) { return (T)Invoke(p); }
		public T Call<T>() {  if (Parameters != null) return (T)Invoke(Parameters); return (T)Invoke(); }

		public Method Generic(params Type[] args) {
			var mi = Info.MakeGenericMethod(args);
			return new Method { Object = Object, Info = mi, Parameters = Parameters };
		}
		public Method Generic<T>() { return Generic(typeof(T)); }
		public Method Generic<T, U>() { return Generic(typeof(T), typeof(U)); }
		public Method Generic<T, U, V>() { return Generic(typeof(T), typeof(U), typeof(V)); }
		public Method Generic<T, U, V, W>() { return Generic(typeof(T), typeof(U), typeof(V), typeof(W)); }
		public Method Generic<T, U, V, W, X>() { return Generic(typeof(T), typeof(U), typeof(V), typeof(W), typeof(X)); }
		public Method Generic<T, U, V, W, X, Y>() { return Generic(typeof(T), typeof(U), typeof(V), typeof(W), typeof(X), typeof(Y)); }

	}

	public class Property {

		public object Object { get; set; }
		public PropertyInfo Info { get; set; }

		public Type Type { get { return Info.PropertyType; } }
		public object Value { get { return Info.GetValue(Object); } set { Info.SetValue(Object, value); } }
	}


	public static class ReflectionObjectExtensions {

		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.Static;

		public static Method Method(this object obj, string name) { 
			var method = obj.GetType().GetMethods(Flags).FirstOrDefault(m => m.Name == name);
			if (method == null) throw new System.ArgumentException(string.Format("No method {0} on type {1} found.", name, obj.GetType().FullName));
			return new Method { Object = obj, Info = method, Parameters = null };
		}

		public static Method Method(this object obj, string name, params object[] parameters) {
			var method = obj.GetType().GetMethods(Flags).FirstOrDefault(m => {
				if (m.Name != name) return false;
				var pars = m.GetParameters();
				if (pars.Length != parameters.Length) return false;
				for (int i = 0; i< pars.Length; i++) {
					if (parameters[i] != null && pars[i].ParameterType != parameters[i].GetType()) return false;
				}
				return true;
			});
			if (method == null) throw new System.ArgumentException(string.Format("No method {0} on type {1} found.", name, obj.GetType().FullName));
			return new Method { Object = obj, Info = method, Parameters = parameters };
		}

		public static Method Method(this object obj, string name, params Type[] signature) {
			MethodInfo method = null;
			try {
				method = obj.GetType().GetMethod(name, Flags, null, signature, null);
			} catch { }
			if (method == null) throw new System.ArgumentException(string.Format("No method {0} on type {1} found.", name, obj.GetType().FullName));
			return new Method { Object = obj, Info = method, Parameters = null };
		}
		public static Method Method<T>(this object obj, string name) { return Method(obj, name, new[] { typeof(T) }); }
		public static Method Method<T, U>(this object obj, string name) { return Method(obj, name, new[] { typeof(T), typeof(U) }); }
		public static Method Method<T, U, V>(this object obj, string name) { return Method(obj, name, new[] { typeof(T), typeof(U), typeof(V) }); }
		public static Method Method<T, U, V, W>(this object obj, string name) { return Method(obj, name, new[] { typeof(T), typeof(U), typeof(V), typeof(W) }); }
		public static Method Method<T, U, V, W, X>(this object obj, string name) { return Method(obj, name, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }); }
		public static Method Method<T, U, V, W, X, Y>(this object obj, string name) { return Method(obj, name, new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X), typeof(Y) }); }

		public static Property Property(this object obj, string name) {
			PropertyInfo p = null;
			try {
				p = obj.GetType().GetProperty(name, Flags & ~BindingFlags.InvokeMethod);
			} catch { }
			if (p == null) throw new System.ArgumentException(string.Format("No property {0} on type {1} found.", name, obj.GetType().FullName));
			return new Property { Object = obj, Info = p };
		}
	}

}