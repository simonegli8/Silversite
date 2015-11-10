using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;


namespace Silversite.Services {
	
	public class Service: IDisposable {
		public Service(Providers context): this() { this.Providers = Providers; }
		public Service() { ServiceRegistry.Register(this); }

		Providers providers = null;
		public Providers Providers { get { if (providers == null) providers = Providers.Global; return providers; } set { providers = value; } }

		Provider provider = null;
		public virtual Provider Provider { get { return provider ?? Providers.Get(this); } set { provider = value; } }

		public bool HasProvider { get { return Providers.Contains(this.GetType()); } }

		public virtual void OnProviderChanging(object sender, ProviderEventArgs args) { }
		public virtual void OnProviderChanged(object sender, ProviderEventArgs args) { }

		public virtual bool IsAvailable { get { return HasProvider && (Providers.IsLazy(this) || !Provider.IsDefault); } }

		public virtual void Dispose() { ServiceRegistry.Forget(this); }
		~Service() { Dispose(); }
	}

	public class Service<P>: Service where P: Provider /*, new() */ {
		// public Service() : base() { var p = new P(); p.Init(null); Providers.Register(new P()); }
		public new virtual P Provider { get { return (P)base.Provider; } set { base.Provider = value; } }
	}

	public interface IDataService {
		void Backup(Stream stream);
		void Restore(Stream stream);
	}

	public class ServiceCollection: HashSet<Service> {
		public IEnumerable<Service> this[Type service] { get { return this.Where(s => s.GetType().IsInstanceOfType(service)); } }
	}

	public class ServiceRegistry {

		public static readonly ServiceCollection Registered  = new ServiceCollection();

		static ServiceRegistry() {
			Providers.ProviderChanging += ProviderChanging;
			Providers.ProviderChanged += ProviderChanged;
		}

		public static void Register(Service service) { Registered.Add(service); }
		public static void Forget(Service service) { Registered.Remove(service); }

		public static void ProviderChanging(object sender, ProviderEventArgs args) { foreach (var s in Registered[args.ServiceType].Where(t => t.Providers == sender)) s.OnProviderChanging(sender, args); }
		public static void ProviderChanged(object sender, ProviderEventArgs args) { foreach (var s in Registered[args.ServiceType].Where(t => t.Providers == sender)) s.OnProviderChanged(sender, args); }

		/*
		public static void Backup(Stream stream) {
			using (var zip = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Compress)) {
				foreach (var s in Registered) {
					if (s is IDataService) {
						using (var w = new StreamWriter(zip)) w.WriteLine(s.GetType().AssemblyQualifiedName);
						((IDataService)s).Backup(zip);
					}
				}
			}
		}

		public static void Restore(Stream stream) {
			using (var zip = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress)) {
				string type;
				using (var r = new StreamReader(zip)) type = r.ReadLine();
				while (type != null) {

					if (Registered.Contains(id) && Registered[id] is IDataService) {
						((IDataService)Registered[id]).Restore(zip);
					}
					obj = f.Deserialize(zip);
				}
			}
		} */
	}

	public class StaticService<SelfT, P>: Service<P>
		where SelfT: StaticService<SelfT, P>, new()
		where P: Provider {

		public static readonly Service Self = new SelfT();

		static StaticService() {
			ServiceRegistry.Register(Self);
		}

		public new static Providers Providers { get { return Self.Providers; } set { Self.Providers = value; } }
		public new static P Provider { get { return (P)Providers[typeof(SelfT)]; } }
		public new static bool HasProvider { get { return Providers.Contains(typeof(SelfT)); } }
		public new static bool IsAvailable { get { return HasProvider && !Provider.IsDefault; } }
	}

}