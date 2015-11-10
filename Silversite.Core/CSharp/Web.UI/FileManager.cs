using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Silversite.Web.UI {

	public class ItemPresenter<T, C> : Presenter {
		public virtual T Item { get; set; }
		public virtual C Container { get; set; }
	}

	public class ContentPresenter<T, C> : ItemPresenter<T, C> { }
	public class HierarchyPresenter<T, C> : ItemPresenter<T, C> { }
	public class MenuPresenter<T, C> : ItemPresenter<T, C> { }

	public class FileSelectedArgs : EventArgs {
		public List<string> SelectedFiles { get; set; }
	}

	public class FileManagerService : ExtensiblePresenterService<FileManagerService> { }
	public abstract class FileManagerProvider : ExtensiblePresenterProvider<FileManagerService> { }

	public class FileManager: ExtensiblePresenter<FileManagerService> {

		public enum Modes { SelectFile, SelectFolder, SelectImage, Explore }
		public enum Roots { Root, Home, Public, Documents, Images, Media, SystemSettings, UserSettings }
		public enum Views { HugeIcons = 0, BigIcons = 1, MediumIcons = 2, SmallIcons = 3, List = 4, Details = 5 }

		public static string Paths(Roots path) {
			switch (path) {
				case Roots.Root: return AppRoot;
				case Roots.Home: return Home;
				case Roots.Public: return Public;
				case Roots.Documents: return Documents;
				case Roots.Images: return Images;
				case Roots.Media: return Media;
				default: return Home;
			}
		}

		public const string AppRoot = "~/";
		public static string Public {
			get {
				if (Services.Domains.HasCurrentSeparateUsers) {
					return "~/Silversite/Users/Public";
				} else {
					return Services.Domains.Path("~/Silversite/Users/Public/");
				}
			}
		}
		public static string Home { get { return Services.Persons.Current != null ? Services.Persons.Current.HomePath : Public; } }
		public static string Documents { get { return Home + "/Documents"; } }
		public static string Images { get { return Home + "/Images"; } }
		public static string Media { get { return Home + "/Media"; } }


		public Modes Mode {
			get { return (Modes)(ViewState["Mode"] ?? Modes.Explore); }
			set { ViewState["Mode"] = value; }
		}

		public string Path {
			get { return (string)(ViewState["Path"] ?? Home); }
			set { ViewState["Path"] = value; }
		}
		public Roots Root {
			get { return (Roots)(ViewState["Root"] ?? Roots.Home); }
			set { ViewState["Root"] = value; }
		}
		public Views View {
			get { return (Views)(ViewState["View"] ?? Views.MediumIcons); }
			set { ViewState["View"] = value; }
		}

		public HashSet<string> Selection { get { return (HashSet<string>)ViewState["Selection"]; } private set { ViewState["Selection"] = value; } }

		public ContentPresenter<FileSystemInfo, FileManager> Content { get; set; }
		public HierarchyPresenter<FileSystemInfo, FileManager> Hierarchy { get; set; }
		public MenuPresenter<FileSystemInfo, FileManager> Menu { get; set; }

		public FileManager()  {
			Selection = new HashSet<string>();
			Root = Roots.Root;
			Path = AppRoot;
		}

		protected override void OnInit(EventArgs e) {
			//EnsureChildControls();
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			EnsureChildControls();
			//if (Page.Request.QueryString["root"] != null) Path = Page.Request.QueryString["root"].Parse<Roots>();
			if (Page.Request.QueryString["mode"] != null) Mode = Page.Request.QueryString["mode"].Parse<Modes>();
			if (Page.Request.QueryString["path"] != null) Path = Page.Request.QueryString["path"];
		}

		public void Apply() { CreateChildControls(); }

		protected override void CreateChildControls() {
			if (!FileManagerService.HasProvider) Services.Lazy.Types["Silversite.Providers.FileManager.Provider, Silversite.FileManager"].Load();
			base.CreateChildControls();
		}
	}
}
