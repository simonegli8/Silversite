using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

[assembly: Silversite.Services.DependsOn(typeof(Silversite.Services.Lazy))]

namespace Silversite.Services {

	public class Lazy: IAutostart {

		public const string RootPath = "~/Bin/Lazy";
		public const bool ShadowCopy = false;

		public static LazyConfiguration Configuration = null; // new LazyConfiguration();

		private static string AssemblyPath(string TypeAssemblyQualifiedName) {
			var c1 = TypeAssemblyQualifiedName.IndexOf(',');
			if (c1 < 0) return null;
			var c2 = TypeAssemblyQualifiedName.IndexOf(',', c1);
			if (c2 < 0) c2 = TypeAssemblyQualifiedName.Length;
			var assembly =  TypeAssemblyQualifiedName.Substring(c1, c2-c1-1).Trim();
			if (assembly.IsNullOrEmpty()) return null;
			return Silversite.Services.Paths.Combine(RootPath, assembly);
		}

		public abstract class Collection<TKey, TInfo, TConfigCollection, TConfigElement>: KeyedCollection<TKey, TInfo>
			where TKey: class
			where TConfigElement: System.Configuration.ConfigurationElement, new()
			where TConfigCollection: Configuration.Collection<TKey, TConfigElement>
		{

			protected bool loaded = false, saved = true;

			public virtual void Save(TConfigCollection conf, Func<TInfo, TConfigElement> converter) {
				lock (this) {
					if (loaded) {
						conf.Clear();
						conf.AddRange(this.Select(info => converter(info)));
					}
					saved = loaded;
				}
			}

			public virtual void Load(TConfigCollection conf, Func<TConfigElement, TInfo> converter) {
				lock (this) {
					if (saved) Clear();
					foreach (TConfigElement e in conf) {
						Add(converter(e));
					}
					loaded = true;
					if (!saved) Save();
				}
			}

			public abstract void Load();
			public abstract void Save();


			public virtual string TypeName(string TypeAssemblyQualifiedName) { // strip assembly version & public key
				return Services.Types.InvariantName(TypeAssemblyQualifiedName);
			}

		}


		public static readonly LazyLoading.AssemblyCollection Assemblies;
		public static readonly LazyLoading.TypeCollection Types;
		public static readonly LazyLoading.PathCollection Paths;
		public static readonly LazyLoading.ProviderCollection Providers;
		public static readonly LazyLoading.DbProviderCollection DbProviders;
		public static readonly LazyLoading.ControlCollection Controls;
		public static readonly LazyLoading.HandlerCollection Handlers;

		public static bool IsLazy(Assembly a) { return Assemblies.Contains(a.FullName); }
		public static bool IsLazy(Type t) { var lazy = IsLazy(t.Assembly); if (lazy && !Types.Contains(t.AssemblyQualifiedName)) Types.Register(t.AssemblyQualifiedName); return lazy; }
		public static bool IsLazy(object obj) { return IsLazy(obj.GetType()); }

#if DEBUG
		public string DisplayLoaded {
			get { return Assemblies.Where(a => a.Loaded).Select(a => a.Assembly.DisplayName()).OrderBy(n => n).StringList(Environment.NewLine); }
		}
#endif

		// TODO remove functions

		public void Shutdown() { }

		static Lazy() {
			Assemblies = new LazyLoading.AssemblyCollection();
			Types = new LazyLoading.TypeCollection();
			Paths = new LazyLoading.PathCollection();
			Providers = new LazyLoading.ProviderCollection();
			DbProviders = new LazyLoading.DbProviderCollection();
			Controls = new LazyLoading.ControlCollection();
			Handlers = new LazyLoading.HandlerCollection();

			/*	
			Configuration = new LazyConfiguration();
			Types.Load();
			Controls.Load();
			Paths.Load();
			Providers.Load();
			Handlers.Load();
			*/
		}

		public static void Save() { Configuration.Save(); }

		// recursively find a file under a root path
		public System.IO.FileInfo FindFile(string path, string name) {
			var dir = new System.IO.DirectoryInfo(path);
			var file = dir.GetFiles().FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
			if (file != null) return file;
			foreach (var d in dir.GetDirectories()) {
				if ((Environment.Is64BitProcess && d.Name != "x86") || (!Environment.Is64BitProcess && d.Name != "x64")) {
					file = FindFile(d.FullName, name);
					if (file != null) return file;
				}
			}
			return null;
		}

		[ThreadStatic]
		static int? recursionLevel;
		static int RecursionLevel {
			get { if (!recursionLevel.HasValue) recursionLevel = 0; return recursionLevel.Value; }
			set { recursionLevel = value; }
		}

		public void Startup() {
			Configuration = new LazyConfiguration();

			// register custom assembly loader
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
				try {
					if (RecursionLevel++ > 0) return null;
					var name = new AssemblyName(args.Name);
					// check loaded assemblies
					var loaded = AppDomain.CurrentDomain.GetAssemblies()
						.FirstOrDefault(a => a.FullName.StartsWith(name.FullName));
					if (loaded != null) return loaded;
					// check lazy folder
					var file = Services.Paths.Combine(RootPath, name.Name + ".dll");
					string vfile = null;
					if (name.Version != null) vfile = Services.Paths.Combine(RootPath, name.Name + "." + name.Version.ToString(), name.Name + ".dll");
					if (!Files.FileExists(file) && (vfile == null || !Files.Exists(file = vfile))) {
						var fi = FindFile(Services.Paths.Map(RootPath), name.Name + ".dll");
						if (fi != null) {
							name.CodeBase = new Uri(fi.FullName).ToString();
						} else {
							return null;
						}
					} else {
						name.CodeBase = new Uri(Services.Paths.Map(file)).ToString();
					}

					Debug.Message("Lazy Loading Assembly {0}.", args.Name);

					return Assembly.Load(name);
				} catch (Exception ex) {
				} finally {
					RecursionLevel--;
				}
				return null;
			};
			if (!AppDomain.CurrentDomain.RelativeSearchPath.ToLower().Contains("bin\\lazy")) {
				AppDomain.CurrentDomain.AppendPrivatePath(Services.Paths.Map(RootPath));
			}

			Types.Load();
			Controls.Load();
			Paths.Load();
			Providers.Load();
			DbProviders.Load();
			Handlers.Load();

		}
	}
}