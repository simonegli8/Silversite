using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Silversite.Services;

namespace Silversite.FileManager {

	public enum ObjectClass { File, Directory }

	public class FileHandlerConfigurationElement : Configuration.Element {
		[System.Configuration.ConfigurationProperty("paths", IsRequired = true)]
		public string Paths { get { return (string)base["paths"]; } set { base["paths"] = value; } }
		[System.Configuration.ConfigurationProperty("type", IsRequired = true, IsKey = true)]
		public string Type { get { return (string)base["type"]; } set { base["type"] = value; } }
		[System.Configuration.ConfigurationProperty("class", IsRequired = false, IsKey = true, DefaultValue=ObjectClass.File)]
		public ObjectClass Class { get { return (ObjectClass)(base["class"] ?? ObjectClass.File); } set { base["class"] = value; } }
		[System.Configuration.ConfigurationProperty("priority", IsRequired = false, DefaultValue = 50.0)]
		public double Priority { get { return (double)(base["priority"] ?? 50); } set { base["priority"] = value; } }
	}

	public class FileHandlerConfigurationCollection : Configuration.Collection<string, FileHandlerConfigurationElement> {
		protected override object GetElementKey(System.Configuration.ConfigurationElement element) { return ((FileHandlerConfigurationElement)element).Type; }
	}

	[Configuration.Section(Path = FileManagerConfiguration.ConfigPath, Name = "filemanager")]
	public class FileManagerConfiguration : Configuration.Section {

		public const string ConfigPath = ConfigRoot + "/FileManager.config";

		[System.Configuration.ConfigurationProperty("handlers")]
		[System.Configuration.ConfigurationCollection(typeof(FileHandlerConfigurationCollection))]
		public FileHandlerConfigurationCollection RegisteredHandlers { get { return (FileHandlerConfigurationCollection)base["handlers"]; } }
	}

	public class Handlers: IAutostart {

		public static FileManagerConfiguration Configuration = new FileManagerConfiguration();

		public static IEnumerable<FileHandlerConfigurationElement> ForFile(string path) {
			return Configuration.RegisteredHandlers
				.OfType<FileHandlerConfigurationElement>()
				.Where(h =>
					Services.Paths.Match(h.Paths, path) ||
					h.Paths.Tokens()
						.Any(pattern => pattern.Contains('/') && pattern == Services.MimeType.OfExtension(path)))
				.ToList();
		}

		public static Handler Get(string path) {
			var handlers = ForFile(path);
			FileHandlerConfigurationElement handler = null;
			if (Files.FileExists(path)) handler = handlers.FirstOrDefault(h => h.Class == ObjectClass.File);
			else if (Files.DirectoryExists(path)) handler = handlers.FirstOrDefault(h => h.Class == ObjectClass.Directory);
			if (handler == null) return null;
			return (Handler)Lazy.Types.New(handler.Type);
		}

		public static void Register(double priority, Handler handler) {
			var type = handler.GetType().InvariantName();
			var existing = Configuration.RegisteredHandlers
				.OfType<FileHandlerConfigurationElement>()
				.FirstOrDefault(h => h.Type == type);
			if (existing == null) {
				var e = new FileHandlerConfigurationElement() {
					Paths = handler.Files,
					Type = type,
					Class = handler is DirectoryHandler ? ObjectClass.Directory : ObjectClass.File,
					Priority = priority
				};
				Configuration.RegisteredHandlers.Add(e);
				var handlers = Configuration.RegisteredHandlers
					.OfType<FileHandlerConfigurationElement>()
					.OrderByDescending(h => h.Priority)
					.ToList();
				Configuration.RegisteredHandlers.Clear();
				Configuration.RegisteredHandlers.AddRange(handlers);
				Configuration.Save();

				Lazy.Types.Register(type);
				Lazy.Types.Save();
			}
		}

		public void Startup() {
			Register(0, new NoActionHandler());
			Register(33, new DownloadHandler());
			Register(66, new DisplayHandler());
			Register(50, new DefaultDirectoryHandler());

		}

		public void Shutdown() { }
	}

}

namespace Silversite.Configuration {

	public class FileManager : Silversite.FileManager.FileManagerConfiguration { }
}