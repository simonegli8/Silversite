using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Sys=System.Configuration;


namespace Silversite.Services {

	public abstract class Provider: IAutostart {
		public abstract Type ServiceType { get; }
		// public abstract Context NewContext();

		public virtual bool IsDefault {
			get {
				var t = GetType();
				var at = t.GetCustomAttributes(typeof(DefaultProviderAttribute), false);
				return at != null && at.Length > 0;
			}
		}
	
		public Provider() { Providers.Register(this); }
		public virtual void Startup() { Modules.DependsOn<Providers>(); Providers.Register(this); }
		public virtual void Shutdown() { }
	}

	public class Provider<Service>: Provider {
		public override Type ServiceType { get { return typeof(Service); } }
		public Provider(): base() { }
	}

	public class LazyProvider: Provider {
		public LazyLoading.ProviderInfo LazyType { get; set; } 
		public override Type ServiceType { get { return LazyType != null ? Type.GetType(LazyType.Service) : null; } }
		public LazyProvider(): base() { }
	}

	public class DefaultProviderAttribute: Web.AutostartPriorityAttribute {
		public DefaultProviderAttribute() : base(int.MaxValue) { }
	}

	/*
	public class Context: IDisposable {
		public virtual void Dispose() { }
		public virtual Provider Provider { get; set; }
		public Context(Provider p) { Provider = p; }
		public Context() { }
	}
	 * */


	public class ProviderEventArgs {
		public Type ServiceType { get; set; }
		public Provider OldProvider { get; set; }
		public Provider NewProvider { get; set; }
	}

	public delegate void ProviderEventHandler(object sender, ProviderEventArgs args); 
	
	public class RegisteredProvidersCollection: KeyedCollection<Type, Provider> {
		protected override Type  GetKeyForItem(Provider item) {
			return item != null ? item.GetType() : null;
		}
	}

	[Configuration.Section(Path=ProvidersConfiguration.Path)]
	public class ProvidersConfiguration: Configuration.Section {
	
		public new const string Path = ConfigRoot + "/silversite.config";

		[Sys.ConfigurationProperty("defaults")]
		[System.Configuration.ConfigurationCollection(typeof(TypeConfigurationCollection))]
		public TypeConfigurationCollection DefaultProviders { get { return (TypeConfigurationCollection)base["defaults"]; } }
	}

	public class Providers: KeyedCollection<Type, Provider>, IAutostart { //TODO support custom providers per user.
		protected override Type  GetKeyForItem(Provider p) { return p.ServiceType; }

		public readonly static RegisteredProvidersCollection Registered = new RegisteredProvidersCollection();
		public static ProvidersConfiguration Config = new ProvidersConfiguration();
		public static Providers Global = null;
		
		static Providers() {
			Global = new Providers();
			Global.RegisterDefaults();
			Modules.DependsOn<Providers>();
		}

		public Providers() {
			if (Global != null) {
				foreach (var e in Global) Add(e);
			}
		}

		public Provider Get(object service) {
			try {
				return this[service.GetType()];
			} catch (Exception ex) {
				throw new NotSupportedException("There is no registered provider for this Service.", ex);
			}
		}

		public new Provider this[Type service] {
			get {
				if (!base.Contains(service)) return null; // RegisterDefaults();
				var p = base[service];
				if (p is LazyProvider) {
					p = (Provider)((LazyProvider)p).LazyType.New();
					Register(p);
				}
				return p;
			}
		}

		public bool IsLazy(Type service) { return base.Contains(service) && base[service] is LazyProvider; }
		public bool IsLazy(object service) { return IsLazy(service.GetType()); }

		public void RegisterDefaults() {
			
			// search all loaded assemblies for providers
			/* var providers = Types.GetAllCustomAttributes<DefaultProviderAttribute>().Select(a => a.Type);
			foreach (var pt in providers) {
				var p = (Provider)New.Object(pt);
				Register(p);
			} */

			// setup lazy providers
			foreach (var conf in Lazy.Providers) {
				try {
					var p = new LazyProvider { LazyType = conf };
					if (p.ServiceType != null) Register(p);
				} catch (Exception ex) { 
					Log.Error("Error in {0} registering LazyProvider {1} for service {2}.", ex, ProvidersConfiguration.Path, conf.TypeAssemblyQualifiedName, conf.Service); }
			}

			// set default providers
			// 
			var defaults = ((IEnumerable<TypeConfigurationElement>)Config.DefaultProviders).Select(p => p.Type).ToList();

			foreach (var p in this) {
				if (p is LazyProvider) {
					if (defaults.Contains(((LazyProvider)p).LazyType.TypeAssemblyQualifiedName)) Use(p);
				} else {
					if (defaults.Contains(p.GetType().AssemblyQualifiedName) || defaults.Contains(p.GetType().FullName)) Use(p);
				}
			}
		}
		
		public void Use(string Type) {
			foreach (var p in this) {
				if (p is LazyProvider) {
					if (Type == ((LazyProvider)p).LazyType.TypeAssemblyQualifiedName) Use(p);
				} else {
					if (Type == p.GetType().AssemblyQualifiedName || Type == p.GetType().FullName) Use(p);
				}
			}
		}

		public static void RegisterDefault(string Type) {
			if (!Config.DefaultProviders.ContainsKey(Type)) Config.DefaultProviders.Add(new TypeConfigurationElement { Type = Type });
			Config.Save();
			Global.Use(Type);
		}

		public static void RemoveDefault(string Type) {
			if (Config.DefaultProviders.ContainsKey(Type)) Config.DefaultProviders.Remove(Type);
			Config.Save();
		}

		public static void Register(Provider provider) {
			if (provider == null)
				System.Diagnostics.Debugger.Break();
			if (provider.ServiceType == null) return;
			bool use = false;
			lock (Registered) {
				//var t = provider.GetType();
				//var c = Registered.Contains(t);
				//var c1 = Registered.Contains(provider);
				if (!Registered.Contains(provider.GetType())) {
					use = true;
					Registered.Add(provider);
				} else if (provider is LazyProvider) {
					use = true;
				}
			}

			if (use) Global.Use(provider);
		}

		public static event ProviderEventHandler ProviderChanging;
		public static event ProviderEventHandler ProviderChanged;

		public static bool HasProvider(Type service) { return Global[service] != null; }
		// public bool HasProvider(Type service) { return this[service] != null; }

		public void Use(Provider provider) {
			var type = provider.ServiceType;
			Provider oldProvider = null;
			lock (this) {
				if (Contains(type)) oldProvider = this[type];
			}
			if (oldProvider != null && provider.GetType().GetCustomAttributes(false).Any(a => a is DefaultProviderAttribute))
				return;
			var args = new ProviderEventArgs { ServiceType = type, NewProvider = provider, OldProvider = oldProvider };

			if (ProviderChanging != null) ProviderChanging(this, args);
			lock(this) {
				if (Contains(type)) Remove(type);
				Add(provider);
			}
			if (ProviderChanged != null) ProviderChanged(this, args);
		}

		public class MessageInfo {
			public object Sender { get; set; }
			public string Text { get; set; }
			public Exception Exception { get; set; }
			public bool Finished { get; set; }
		}
		public delegate void MessageHandler(object sender, MessageInfo e);

		public static ConcurrentQueue<MessageInfo> Messages = new ConcurrentQueue<MessageInfo>();
		public static event MessageHandler MessageReceived;

		public static void Message(MessageInfo message) {
			Messages.Enqueue(message);
			//Log.Write( "Provider Messages", message.Text);
			if (MessageReceived != null) MessageReceived(message.Sender, message);
		}

		public static void Message(object sender, string msg, params object[] args) { Message(new MessageInfo { Sender = sender, Text = args == null ? msg : string.Format(msg, args), Exception = null, Finished = false }); }
		public static void Exception(object sender, string msg, Exception ex, params object[] args) { Message(new MessageInfo { Sender = sender, Text = string.Format(msg, args), Exception = ex, Finished = false }); Log.Error(msg, ex, args); }
		public static void Finished(object sender) { Message(new MessageInfo { Sender = sender, Finished = true, Exception = null, Text = null }); }
		public static void Message(string msg) { Message(null, msg, null); }

		public void Dispose() { foreach (var p in Registered) if (p is IDisposable) ((IDisposable)p).Dispose(); }
		public void Startup() { Modules.DependsOn<Lazy>(); }
		public void Shutdown() { }
	}
	
}
