using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.Diagnostics;
using System.Threading;
using Sys = System.Configuration;

namespace Silversite.Configuration {

	public class SectionAttribute: Attribute {
		public string Path { get; set; }
		public string Name { get; set; }
		public string Group { get; set; }
		public SectionAttribute() { }
		public SectionAttribute(string name) { Name = name; }
	}

	public class Section: Sys.ConfigurationSection {

		public const string ConfigRoot = "~/Silversite/Config";

		public string TypeName {
			get {
				var name = GetType().Name;
				if (name.EndsWith("Group")) name = name.Remove(name.Length - "Group".Length);
				if (name.EndsWith("Section")) name = name.Remove(name.Length - "Section".Length);
				if (name.EndsWith("Configuration")) name = name.Remove(name.Length - "Configuration".Length);
				name = name.TrimEnd('.');
				if (char.IsUpper(name[0])) name = char.ToLower(name[0]) + name.Substring(1);
				return name;
			}
		}

		public string Path {
			get {
				var t = GetType();
				var a = (SectionAttribute)t.GetCustomAttributes(typeof(SectionAttribute), true).FirstOrDefault();
				if (a != null) { return a.Path ?? string.Empty; }
				return string.Empty;
			}
		}

		public string SectionName {
			get {
				string name = string.Empty;
				string group = string.Empty;

				var t = GetType();
				var a = (SectionAttribute)t.GetCustomAttributes(typeof(SectionAttribute), true).FirstOrDefault();
				if (a != null) {
					if (!a.Name.IsNullOrEmpty()) name = a.Name;
					if (!a.Group.IsNullOrEmpty()) group = a.Group;
				}

				if (name.IsNullOrEmpty()) name = TypeName;

				if (group.IsNullOrEmpty()) return name;
				else return group + "/" + name;
			}
		}

		static Dictionary<string, Sys.Configuration> config = new Dictionary<string,Sys.Configuration>();
		Sys.Configuration Config {
			get {
				var path = Services.Paths.Normalize(Path).ToLower();
				if (path == string.Empty || path == "~" || path == "~/" || path == "~/web.config" || !System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(path))) path = "~/web.config";
				if (!config.ContainsKey(path)) {
					try {
						if (path == "~/web.config") config["~/web.config"] = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath); // open ~/web.config
						else {
							ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
							configMap.ExeConfigFilename = System.Web.Hosting.HostingEnvironment.MapPath(path);
							config[path] = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
						}
					} catch (Exception ex) {
						config[path] = null;
						Services.Log.Error("Error reading the configuration for {0}.", ex, GetType().FullName);
						throw ex;
					}
				}
				return config[path];
			}
		}

		[ThreadStatic]
		static bool reentrance = false;

		Section current = null;
		Section Current {
			get {
				if (current == null) {
					try {
						if (!reentrance) {
							reentrance = true;
							current = Config.GetSection(SectionName) as Section;
							reentrance = false;
						}
					} catch (Exception ex) {
						Services.Log.Error("Error reading the configuration for {0}.", ex, GetType().FullName);
						//throw ex;
					}
				} 
				return current;				
			}
		}

		public Sys.ConnectionStringSettingsCollection ConnectionStrings {
			get { return Config.ConnectionStrings.ConnectionStrings; }
		}

		public void Copy(Section src, Section dest) {
			if (src == null || src == dest) return;
			foreach (ConfigurationProperty key in src.Properties) dest[key] = src[key];
		}

		public void Load() { Copy(Current, this); }
		public void Save() { Copy(this, Current); Config.Save(); }

		public Section() : base() { Load(); }
	}
	
	public class Element: Sys.ConfigurationElement { }

	public abstract class Collection<TKey, T>: System.Configuration.ConfigurationElementCollection,  IEnumerable<T>, IDictionary<TKey, T> where T: Sys.ConfigurationElement, new() where TKey: class {
		public void Add(TKey key, T value) { BaseAdd(value); }
		public bool ContainsKey(TKey key) { try { return BaseGet(key) != null; } catch { return false; } }
		public ICollection<TKey> Keys { get { return ((IEnumerable<T>)this).Select<T, TKey>(c => (TKey)GetElementKey(c)).ToList(); } }
		public bool Remove(TKey key) { var res = ContainsKey(key); BaseRemove(key); return res; }
		public bool TryGetValue(TKey key, out T value) { try { value = (T)BaseGet(key); return value != null; } catch { value = null; return false; } }
		public ICollection<T> Values { get { return new List<T>(this); } }
		public T this[TKey key] { get { return (T)BaseGet(key); } set { if (ContainsKey(key)) Remove(key); Add(key, value); } }
		public void Add(KeyValuePair<TKey,T> item) { Add(item.Key, item.Value); }
		public bool Contains(KeyValuePair<TKey,T> item) { return BaseIndexOf(item.Value) >= 0; }
		public void  CopyTo(KeyValuePair<TKey,T>[] array, int arrayIndex) {
			foreach (KeyValuePair<TKey, T> p in (IDictionary<TKey, T>)this) {
				if (arrayIndex >= array.Length) break;
				array[arrayIndex++] = p;
			}
		}
		public new bool  IsReadOnly { get { return false; } }
		public bool Remove(KeyValuePair<TKey,T> item) { return Remove(item.Key); } 
		public void Add(T e) { BaseAdd(e); }
		public void AddRange(IEnumerable<T> types) { foreach (var e in types) Add(e); }
		public void Clear() { BaseClear(); }
		public void Remove(T item) { BaseRemove(GetElementKey(item)); }
		public bool Contains(T item) { return ContainsKey((TKey)GetElementKey(item)); }
		protected override System.Configuration.ConfigurationElement CreateNewElement() { return new T(); }
		public void CopyTo(T[] array, int arrayIndex) {
			foreach (T x in this) {
				if (arrayIndex >= array.Length) break;
				array[arrayIndex++] = x;
			}
		}
		public new IEnumerator<T> GetEnumerator() { foreach (T t in ((System.Configuration.ConfigurationElementCollection)this)) yield return t; }
		IEnumerator IEnumerable.GetEnumerator() { foreach (T t in ((System.Configuration.ConfigurationElementCollection)this)) yield return t; }
		IEnumerator<KeyValuePair<TKey, T>> IEnumerable<KeyValuePair<TKey, T>>.GetEnumerator() { foreach (var kv in (IEnumerable<KeyValuePair<TKey, T>>)this) yield return kv; }
	}

	//public class Database: global::Silversite.Data.DatabaseConfiguration { }
	public class Mail: global::Silversite.Services.MailConfiguration { }
	public class LazyLoading: global::Silversite.Services.LazyConfiguration { }
	public class Providers: global::Silversite.Services.ProvidersConfiguration { }
	public class EditableContent: global::Silversite.Web.UI.EditableConfiguration { }
	public class Log: global::Silversite.Services.LogConfiguration { }
	public class Modules: global::Silversite.Web.ModulesConfiguration { }
	public class Develop: global::Silversite.Services.DevelopConfiguration { }
	public class Files : global::Silversite.Services.FilesConfiguration { }

	public class Silversite : System.Configuration.ConfigurationSectionGroup {

		public Silversite() : base() { }

		//public Database Database { get { return Sections["database"] as Database; } }
		public Mail Mail { get { return Sections["mail"] as Mail; } }
		public LazyLoading LazyLoading { get { return Sections["lazyLoading"] as LazyLoading; } }
		public Providers Providers { get { return Sections["providers"] as Providers; } }
		public EditableContent EditableContent { get { return Sections["editableContent"] as EditableContent; } }
		public Log Log { get { return Sections["log"] as Log; } }
		public Modules Modules { get { return Sections["modules"] as Modules; } }
		public Develop Develop { get { return Sections["develop"] as Develop; } }
		public Files Files { get { return Sections["files"] as Files; } }
	}

	public class VirtualConfig: Services.IAutoLoader {
		public void Startup() { Services.Modules.DependsOn<Services.VirtualFiles>(); }
		public void Shutdown() { }

		public void AssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
			var a = args.LoadedAssembly;
			var files = Services.DictionaryVirtualPathProvider.Current.FilesAndDirectories
				.OfType<Services.ResourceVirtualFile>()
				.Where(rf => {
					if (rf.Assembly != a) return false;
					var f = Services.Paths.Normalize(rf.VirtualPath);
					return Services.Paths.Match(Section.ConfigRoot + "*.config;~/Silversite/Extensions/*.config;*/web.config", f) && !Services.Files.Exists(f);
				});

			files.ForEach(file => {
				using (var s = file.Open()) {
					var path = Services.Paths.Normalize(file.VirtualPath);
					Services.Files.Save(s, path);
				}
			});
		}
	}
	
}
