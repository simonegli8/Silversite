using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class TypeConfigurationElement: Configuration.Element {
		[System.Configuration.ConfigurationProperty("type", IsRequired=true, IsKey=true)]
		public string Type { get { return (string)base["type"]; } set { base["type"] = value; } }
		[System.Configuration.ConfigurationProperty("auto", IsRequired=false, DefaultValue=null)]
		public TimeSpan? Auto { get { TimeSpan t; var s = (string)base["auto"]; if (s != null && TimeSpan.TryParse(s, out t)) return t; return null; } set { base["auto"] = value.ToString(); } }
	}
	public class TypeConfigurationCollection: Configuration.Collection<string, TypeConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((TypeConfigurationElement)element).Type; }
	}
	
}

namespace Silversite.Services.LazyLoading {

	public class TypeInfo {
		public AssemblyInfo Assembly { get; private set; }
		string typeName = null;
		public string TypeAssemblyQualifiedName {
			get { return typeName; }
			set {
				typeName = value;
				Assembly = Lazy.Assemblies.Get(AssemblyName);
			}
		}
		public string TypeName {
			get {
				var c = TypeAssemblyQualifiedName.IndexOf(',');
				if (c < 0) return TypeAssemblyQualifiedName;
				else return TypeAssemblyQualifiedName.Substring(0, c);
			}
		}
		public string AssemblyName {
			get {
				var c1 = TypeAssemblyQualifiedName.IndexOf(',');
				if (c1 < 0) return null;
				var c2 = TypeAssemblyQualifiedName.IndexOf(',', c1+1);
				if (c2 < 0) c2 = TypeAssemblyQualifiedName.Length;
				var assembly =  TypeAssemblyQualifiedName.Substring(c1+1, c2-c1-1).Trim();
				if (string.IsNullOrEmpty(assembly)) throw new ArgumentException("TypeAssemblyQualifiedName must contain an assembly.");
				return assembly;
			}
		}
		System.Type type = null;
		public System.Type Type {
			get {
				if (type == null) {
					Assembly.Load();
					if (Assembly.Assembly != null) {
						type = Assembly.Assembly.GetType(TypeName);
						if (type == null) throw new TypeAccessException("Could not load type " + TypeName + " from assembly " + Assembly.AssemblyName);
					} else {
						type = System.Type.GetType(TypeAssemblyQualifiedName);
						if (type == null) throw new TypeAccessException("The type " + TypeAssemblyQualifiedName + " could not be found.");
					}
				}
				return type;
			}
		}
		public object New(params object[] args) { return Silversite.New.Object(Type, args); }
		public T New<T>(params object[] args) { return (T)Silversite.New.Object(Type, args); }
		public bool Loaded { get { return Assembly.Loaded; } }
		public void Load() { Assembly.Load(); }
		public TimeSpan? Auto { get; set; }
	}

	public class TypeCollection: Lazy.Collection<string, TypeInfo, TypeConfigurationCollection, TypeConfigurationElement> {
		protected override string GetKeyForItem(TypeInfo item) { return TypeName(item.TypeAssemblyQualifiedName); }

		public override void Save() { Save(Lazy.Configuration.RegisteredTypes, info => new TypeConfigurationElement { Type = info.TypeAssemblyQualifiedName, Auto = info.Auto }); }
		public override void Load() {
			Load(Lazy.Configuration.RegisteredTypes, e => {
				var info = new TypeInfo { TypeAssemblyQualifiedName = e.Type, Auto = e.Auto };
				if (e.Auto != null) Tasks.DoLater(e.Auto.Value, () => info.Load());
				return info;
			});
		}

		protected virtual void Add(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			Add(new TypeInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName });
		}

		public new virtual bool Contains(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			return base.Contains(TypeAssemblyQualifiedName);
		}

		public virtual void Register(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					Add(TypeAssemblyQualifiedName);
					Save();
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string TypeAssemblyQualifiedName, System.IO.Stream Assembly) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new TypeInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName };
					Lazy.Assemblies.Register(t.AssemblyName, Assembly);
					Add(t);
					Lazy.Save();
				}
			}
		}
		public virtual void Register(string TypeAssemblyQualifiedName, string assemblyPath) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (!Contains(TypeAssemblyQualifiedName)) {
					var t = new TypeInfo { TypeAssemblyQualifiedName = TypeAssemblyQualifiedName };
					Lazy.Assemblies.Register(t.AssemblyName, assemblyPath);
					Add(t);
					Lazy.Save();
				}
			}
		}

		public virtual Type Type(string TypeAssemblyQualifiedName) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (Contains(TypeAssemblyQualifiedName)) {
					return this[TypeAssemblyQualifiedName].Type;
				} else {
					var type = System.Type.GetType(TypeAssemblyQualifiedName);
					if (type == null) throw new TypeAccessException("The type " + TypeAssemblyQualifiedName + " could not be found.");
					return type;
				}
			}
		}

		public virtual object New(string TypeAssemblyQualifiedName, params object[] constructorArgs) {
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			lock (this) {
				if (Contains(TypeAssemblyQualifiedName)) {
					return this[TypeAssemblyQualifiedName].New(constructorArgs);
				} else {
					var type = System.Type.GetType(TypeAssemblyQualifiedName);
					if (type == null) throw new TypeAccessException("The type " + TypeAssemblyQualifiedName + " could not be found.");
					return Silversite.New.Object(type, constructorArgs);
				}
			}
		}

		public virtual TypeInfo Info(string TypeAssemblyQualifiedName) {
			Register(TypeAssemblyQualifiedName);
			TypeAssemblyQualifiedName = TypeName(TypeAssemblyQualifiedName);
			if (Contains(TypeAssemblyQualifiedName)) return this[TypeAssemblyQualifiedName];
			return null;
		}
	}
}
