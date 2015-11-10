using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Silversite {
	
	public static class New {
		public static T Object<T>() { return Activator.CreateInstance<T>(); }
		public static T Object<T>(params object[] args) where T: class {
			if (args.Any(p => p == null)) {
				try {
					return (T)Activator.CreateInstance(typeof(T), args);
				} catch {
					return null;
				}
			} else {
				if (typeof(T).GetConstructor(args.Select(a => a.GetType()).ToArray()) != null) {
					return (T)Activator.CreateInstance(typeof(T), args);
				} else {
					return null;
				}
			}
		}
		public static object Object(Type type) { return Activator.CreateInstance(type); }
		public static object Object(Type type, params object[] args) {
			if (args.Any(p => p == null)) {
				try {
					return Activator.CreateInstance(type, args);
				} catch {
					return null;
				}
			} else {
				if (type.GetConstructor(args.Select(a => a.GetType()).ToArray()) != null) {
					return Activator.CreateInstance(type, args);
				} else {
					return null;
				}
			}
		}
	}
}

namespace Silversite.Services {

	public static class Types {

		public static event EventHandler LoadCustomAssemblies;

		static bool initialized = false;
		static object Lock = new object();

		// static int recursionDepth = 0;

		public static readonly string[] BinPaths = new string[] { "~/bin", "~/Silversite/BinLazy" };

		public static void LoadAllAssemblies() {
			if (!initialized) {
				var binpaths = BinPaths.Select(path => Services.Paths.Map(path));
				var dlls = binpaths.SelectMany(path => {
					if (!Services.Files.DirectoryExists(path)) return new FileInfo[0];
					return new DirectoryInfo(path).EnumerateFiles("*.dll", SearchOption.AllDirectories);
				}).ToList();

				foreach (var dll in dlls) {
					lock (Lock) {
						var assemblies = Assemblies;
						var dllsloaded = assemblies.Where(a => !a.IsDynamic && binpaths.Any(path => a.Location.StartsWith(path))).Select(a => a.Location).ToList();
						if (!dllsloaded.Contains(dll.FullName)) Assembly.LoadFrom(dll.FullName);
					}
				}
					
				if (LoadCustomAssemblies != null) LoadCustomAssemblies(null, EventArgs.Empty);
				initialized = true;
			}
		}

		public static  IQueryable<Assembly> Assemblies { get { return AppDomain.CurrentDomain.GetAssemblies().AsQueryable(); } }
		public static bool IsCustom(Assembly a) {
			return !a.FullName.StartsWith("System") && !a.FullName.StartsWith("mscorlib") && !a.FullName.StartsWith("Microsoft.") && !a.FullName.StartsWith("EntityFramework");
		}
		public static IQueryable<Assembly> CustomAssemblies { get { return Assemblies.Where(a => IsCustom(a)); } }

		public static bool Implements(Type type, Type _interface, bool inherit) {
			return type != null && type.GetInterfaces().Contains(_interface) &&
				(inherit || !Implements(type.BaseType, _interface, inherit));
		}
		public static bool Implements(Type type, Type _interface) { return Implements(type, _interface, true); }

		public static IEnumerable<Type> GetAll(Type type, Assembly assembly, bool inherit) {
			var types = assembly.GetLoadableTypes();
			if (type.IsInterface) {
				return types.Where(t => !t.IsInterface && t.Implements(type, inherit));
			} else if (type.IsSubclassOf(typeof(Attribute))) {
				return types.Where(t => t.GetCustomAttributes(type, inherit).Length > 0);
			} else {
				if (inherit) return types.Where(t => t.IsSubclassOf(type));
				else return types.Where(t => t == type);
			}
		}

		public static IEnumerable<Type> GetAll<T>(Assembly assembly, bool inherit) {
			return GetAll(typeof(T), assembly, inherit);
		}

		public static IEnumerable<Type> GetAll<T>(IQueryable<Assembly> assemblies, bool inherit) {
			return assemblies.SelectMany(a => GetAll<T>(a, inherit));
		}

		public class Attributes<T> where T: Attribute {
			public Type Type { get; set; }
			public Assembly Assembly { get; set; }
			public List<T> AllAttributes { get; set; }
			public bool IsAssembly { get { return Type == null; } }
		}


		public static IEnumerable<Attributes<T>> GetAllAttributes<T>(Assembly assembly, bool inherit) where T: Attribute {
			var types = assembly.GetLoadableTypes();
			var type = typeof(T);
			return types.Where(t => t.GetCustomAttributes(type, inherit).Length > 0).Select(t => new Attributes<T> { Assembly = t.Assembly, Type = t, AllAttributes = t.GetCustomAttributes(type, inherit).OfType<T>().ToList<T>() });
		}

		public static IEnumerable<Attributes<T>> GetAllAttributes<T>(IQueryable<Assembly> assemblies, bool inherit) where T: Attribute {
			var type = typeof(T); 
			return assemblies.SelectMany(a => GetAllAttributes<T>(a, inherit))
				.Concat(assemblies.Where(a => a.GetCustomAttributes(type, inherit).Length > 0).Select(a => new Attributes<T> { Assembly = a, Type = null, AllAttributes = a.GetCustomAttributes(type, inherit).OfType<T>().ToList<T>() })); ;
		}

		public static IEnumerable<Type> GetAll<T>(bool inherit) { return GetAll<T>(Assemblies, inherit); }
		public static IEnumerable<Type> GetAllCustom<T>(bool inherit) { return GetAll<T>(CustomAssemblies, inherit); }
		public static IEnumerable<Type> GetAll<T>() { return GetAll<T>(Assemblies, true); }
		public static IEnumerable<Type> GetAllCustom<T>() { return GetAll<T>(CustomAssemblies, true); }

		public static IEnumerable<Attributes<T>> GetAllAttributes<T>(bool inherit) where T: Attribute { return GetAllAttributes<T>(Assemblies, inherit); } 
 		public static IEnumerable<Attributes<T>> GetAllCustomAttributes<T>(bool inherit) where T: Attribute { return GetAllAttributes<T>(CustomAssemblies, inherit); }
		public static IEnumerable<Attributes<T>> GetAllAttributes<T>() where T: Attribute { return GetAllAttributes<T>(true); }
		public static IEnumerable<Attributes<T>> GetAllCustomAttributes<T>() where T: Attribute { return GetAllAttributes<T>(true); }

		public static string InvariantName(string TypeAssemblyQualifiedName) { // strip assembly version & public key
			int comma = TypeAssemblyQualifiedName.IndexOf(',');
			comma = TypeAssemblyQualifiedName.IndexOf(',', comma+1);
			if (comma != -1) return TypeAssemblyQualifiedName.Substring(0, comma).Trim();
			return TypeAssemblyQualifiedName;
		}
		public static string InvariantName(Type type) { return InvariantName(type.AssemblyQualifiedName); }

		public static Type[] Signature<T>() { return new[] { typeof(T) }; }
		public static Type[] Signature<T, U>() { return new[] { typeof(T), typeof(U) }; }
		public static Type[] Signature<T, U, V>() { return new[] { typeof(T), typeof(U), typeof(V) }; }
		public static Type[] Signature<T, U, V, W>() { return new[] { typeof(T), typeof(U), typeof(V), typeof(W) }; }
		public static Type[] Signature<T, U, V, W, X>() { return new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X) }; }
		public static Type[] Signature<T, U, V, W, X, Y>() { return new[] { typeof(T), typeof(U), typeof(V), typeof(W), typeof(X), typeof(Y) }; }

	}
}
