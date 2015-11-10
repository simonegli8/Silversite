using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using System.Diagnostics.Contracts;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(Silversite.Web.Modules), "Startup")]

namespace Silversite.Services {

	public interface IAutostart {
		void Startup();
		void Shutdown();
	}
	public interface IAutoLoader : IAutostart {
		void AssemblyLoaded(object sender, AssemblyLoadEventArgs args);
	}
	public class Modules {
		public static void DependsOn(Type type) { Web.Modules.DependsOn(type); }
		public static void DependsOn<T>() { Web.Modules.DependsOn<T>(); }
		public static void DependsOn<T>(HttpApplication app) where T : IHttpModule { Web.Modules.DependsOn<T>(app); }
		public static void Remove(object obj) { Web.Modules.Remove(obj); }
		public static void Remove(Type type) { Web.Modules.Remove(type); }
		public static void Remove<T>() { Web.Modules.Remove<T>(); }
		public static void Register(Type type) { DependsOn(type); }
		public static void Register<T>() { DependsOn<T>(); }
		public static void Register<T>(HttpApplication app) where T : IHttpModule { DependsOn<T>(app); }
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public class DependsOnAttribute : System.Attribute {
		public Type Class { get; set; }
		//public int Priority { get; set; }
		public DependsOnAttribute() { }
		public DependsOnAttribute(Type Class) { this.Class = Class; }
	}
}

namespace Silversite.Web {

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public class DependsOnAttribute : Services.DependsOnAttribute {
		public DependsOnAttribute() : base() { }
		public DependsOnAttribute(Type Class) : base(Class) { }
	}

	[Configuration.Section(Path = ModulesConfiguration.Path)]
	public class ModulesConfiguration : Configuration.Section {

		public new const string Path = ConfigRoot + "/Silversite.config";

		[System.Configuration.ConfigurationProperty("disabled", IsRequired = true, IsKey = true)]
		public string DisabledModules { get { return (string)base["disabled"]; } set { base["disabled"] = value; disabledList = null; } }

		HashSet<string> disabledList = null;

		public bool IsDisabled(Type type) {
			if (disabledList == null) disabledList = new HashSet<string>(DisabledModules.SplitList(',', ';'));
			return disabledList.Contains(type.Name) || disabledList.Contains(type.FullName);
		}
	}

	public abstract class HttpModule : IHttpModule {

		public class ModuleCollection : KeyedCollection<Type, object> {
			protected override Type GetKeyForItem(object item) {
				Contract.Requires(item is IHttpModule || item is Services.IAutostart || item is Services.IAutoLoader);
				return item.GetType();
			}
			public void AddRange(IEnumerable<object> set) {
				foreach (var m in set) Add(m);
			}
			public IEnumerable<Type> Keys { get { foreach (var m in this) { yield return m.GetType(); } } }

			public new bool Contains(object item) {
				return base.Contains(item.GetType());
			}
			public new bool Contains(Type type) {
				return base.Contains(type);
			}
		}

		public static ModuleCollection RegisteredModules = new ModuleCollection();

		public abstract void Dispose();
		public abstract void Init(HttpApplication context);

		public static ModuleCollection InitializedModules {
			get {
				var context = HttpContext.Current;
				if (context.Items[typeof(ModuleCollection)] == null) context.Items[typeof(ModuleCollection)] = new ModuleCollection();
				return (ModuleCollection)context.Items[typeof(ModuleCollection)];
			}
		}


		public static object DependsOn(Type type, HttpApplication context) {
			try {

				if (!type.IsAbstract && !type.IsInterface && !type.ContainsGenericParameters && !Modules.IsDisabled(type)) {
					object module = null;
					bool firstinit = false;
					lock (RegisteredModules) {
						if (!RegisteredModules.Contains(type)) module = New.Object(type);
						if (!RegisteredModules.Contains(type)) {
							if (module != null && module is IHttpModule || module is Services.IAutostart || module is IAutoLoader) {
								RegisteredModules.Add(module);
								if (module is Services.IAutostart) {
									firstinit = true;
								}
							} else {
								Services.Log.Error("Error creating module instance of {0}. Specified type probably is no module (Either IHttpModule or IAutostart).", type, Services.Log.ErrorClass.Severe);
								return null;
							}
						} else {
							module = RegisteredModules[type];
						}
					}

					if (module is IHttpModule && context != null) {
						bool init = false;
						var modules = InitializedModules;
						lock (modules) {
							if (!modules.Contains(module)) {
								modules.Add(module);
								init = true;
							}
						}
						if (init) ((IHttpModule)module).Init(context);
					}
					if (module is Services.IAutostart && firstinit) {
#if DEBUG
						//Debug.Message(1, "Debug", "Inititalizing Module {0}.", module.GetType().FullName);
						Debug.Message("Initializing Module {0}.", module.GetType().FullName);
						var t0 = DateTime.Now;
#endif
						((Services.IAutostart)module).Startup();
#if DEBUG
						var t = ((float)(DateTime.Now - t0).Milliseconds) / 1000;
						//Debug.Message(1, "Debug", "Module {0} initialized in {1} s.", module.GetType().FullName, t);
						Debug.Message("Module {0} initialized in {1} s.", module.GetType().FullName, t);
#endif
					}
					return module;
				}
				return null;
			} catch (Exception ex) {
				Services.Log.Error("Error loading module {0}.", ex, type.Name);
				throw ex;
			}
		}

		public static void DependsOn(Type type) {
			if (type.Implements(typeof(Services.IAutostart)) || type.Implements(typeof(Services.IAutoLoader)) || type.Implements(typeof(IHttpModule))) {
				DependsOn(type, null);
			} else {
				throw new NotSupportedException(string.Format("Type {0} is no IAutostart.", type.FullName));
			}
		}

		public static void DependsOn<T>() { DependsOn(typeof(T)); }
		public static IHttpModule DependsOn<T>(HttpApplication app) where T : IHttpModule { return DependsOn(typeof(T), app) as IHttpModule; }
		public static void Register(Type type) { DependsOn(type); }
		public static void Register<T>() { DependsOn<T>(); }
		public static void Register<T>(HttpApplication app) where T : IHttpModule { DependsOn<T>(app); }

		public static void Remove(Type type) {
			lock (RegisteredModules) {
				if (RegisteredModules.Contains(type)) RegisteredModules.Remove(type);
			}
		}
		public static void Remove<T>() { Remove(typeof(T)); }
		public static void Remove(object obj) { Remove(obj.GetType()); }

		public static void Load(string typeName, HttpApplication context) {
			Type type = null;
			try {
				type = Type.GetType(typeName);
			} catch { }
			if (type != null) DependsOn(type, context);
		}

	}

	public interface IHttpAutoModule : IHttpModule { }

	public interface IAutostart : Services.IAutostart { }
	public interface IAutoLoader : Services.IAutoLoader { }


	public class AutostartPriorityAttribute : Attribute {
		public AutostartPriorityAttribute(int priority) { Priority = priority; }
		public int Priority { get; set; }
	}

	public class Modules : HttpModule {

		static Services.ConcurrentQueue<Type> lazyLoadedModules;
		static HashSet<Assembly> loadedAssemblies;

		public static ModulesConfiguration Configuration;

		static Modules() {
			lazyLoadedModules = new Services.ConcurrentQueue<Type>();
			loadedAssemblies = new HashSet<Assembly>();
			Configuration = new ModulesConfiguration();
		}

		public static bool IsDisabled(Type type) { return Configuration.IsDisabled(type); }
		public static bool IsDisabled<T>() { return IsDisabled(typeof(T)); }

		public static void AssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
			try {
				var a = args.LoadedAssembly;
				if (!Services.Types.IsCustom(a)) return;
				bool loaded = false;
				lock (loadedAssemblies) {
					loaded = loadedAssemblies.Contains(args.LoadedAssembly);
					if (!loaded) loadedAssemblies.Add(args.LoadedAssembly);
				}
				if (!a.IsDynamic && !loaded) {

					// run all registered IAutoLoader's
					foreach (var m in RegisteredModules.OfType<Services.IAutoLoader>().ToList()) {
						try {
							m.AssemblyLoaded(sender, args);
						} catch (Exception ex) {
							Services.Log.Error("Error startup loading IAutoLoader {0}:", m.GetType().FullName);
						}
					}
					// call all DependsOn attributes
					var autos = a.GetCustomAttributes(typeof(Services.DependsOnAttribute), true)
						.OfType<Services.DependsOnAttribute>();
					foreach (var auto in autos) DependsOn(auto.Class);
				}
			} catch (Exception ex) {
				Services.Log.Error("Modules.AssemblyLoaded Exception", ex, Services.Log.ErrorClass.Severe);
			}
		}

		public static void Startup() {
			try {
				AppDomain.CurrentDomain.DomainUnload += Shutdown;

				DynamicModuleUtility.RegisterModule(typeof(Modules)); // register the module Modules

				DependsOn<Services.Lazy>();
				//DependsOn<Data.ContextPoolManager>();
				DependsOn<Services.Providers>();
				//DependsOn<Data.EntityDatabaseProvider>();
				DependsOn<Services.Log>();

				Services.Log.Write("Debug", "####    Application Start, AppDomain {0}", AppDomain.CurrentDomain.Id);

				DependsOn<Services.DictionaryVirtualPathProvider>();
				DependsOn<Services.VirtualFiles>();

				AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
				foreach (var a in Services.Types.CustomAssemblies) AssemblyLoaded(null, new AssemblyLoadEventArgs(a));
			} catch (Exception ex) {
				Services.Log.Error("Modules.Startup Exception", ex, Services.Log.ErrorClass.Severe);
			}
		}

		public override void Init(HttpApplication app) {
			Services.Log.Critical(() => {
#if DEBUG
				try {
					Debug.Message("Request: {0}", app.Context.Request.Url);
				} catch { }
#endif
				Modules.DependsOn<Services.Paths>(app);
				Modules.DependsOn<Web.DomainRewriteModule>(app);
				var registered = RegisteredModules.OfType<IHttpModule>().ToList();
				foreach (var m in registered) DependsOn(m.GetType(), app);
				while (lazyLoadedModules.Count > 0) DependsOn(lazyLoadedModules.Dequeue(), app);
			}, ex => {
				Services.Log.Error("Modules.Init Exception", ex, Services.Log.ErrorClass.Severe);
			});
		}

		public static void Shutdown(object sender, EventArgs e) {
			Services.Log.Critical(() => {
				Services.Log.Write("Debug", "####    Application Shutdown");
				foreach (var m in RegisteredModules.Reverse()) {
					if (m is Services.IAutostart) {
						try {
							((Services.IAutostart)m).Shutdown();
						} catch (Exception ex) {
							Services.Log.Error("Exception shutting down IAutostart {0}.", m.GetType().FullName, ex);
						}
					}
				}
			}, ex => {
				Services.Log.Error("Modules.Shutdown Exception", ex, Services.Log.ErrorClass.Severe);
			});
		}

		public override void Dispose() {
			Services.Log.Critical(() => {
				Debug.Message("Modules.Dispose");
				foreach (var m in RegisteredModules.Reverse()) {
					if (m is IHttpModule) {
						try {
							((IHttpModule)m).Dispose();
						} catch (Exception ex) {
							Services.Log.Error("Exception disposing IHttpModule {0}.", m.GetType().FullName, ex);
						}
					}
				}
				Debug.Message("Modules.Init finished." + Environment.NewLine);
			}, ex => {
				Services.Log.Error("Modules.Dispose Exception", ex, Services.Log.ErrorClass.Severe);
			});
		}
	}
}
