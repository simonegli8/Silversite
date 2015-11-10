using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Globalization;

namespace Silversite.Web.UI {

	public class ItemInfo {

		public enum Classes { Directory, Item, Control, Page, Link };

		public virtual ItemInfo Parent {
			get {
				var dir = Services.Paths.Directory(this.Path);
				if (string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(dir) || Path == "~" || Path == "~/" || Path == "/") return null;
				return new FileItemInfo(dir, Container);
			}
		}
		public virtual ItemInfo Root { get { if (Parent == null) return this; return Parent.Root; } }
		public virtual CultureInfo Culture { get; set; }
		public virtual Classes Class { get; set; }
		public virtual string Path { get; set; }
		public virtual string NativePath { get; set; }
		public virtual string Name { get; set; }
		public virtual string NativeName { get; set; }
		public virtual string DownloadUrl { get; set; }
		public virtual Image Icon(int size, bool preview) { return null; }
		public virtual Image Icon(FileManager.Views size, bool preview) { return null; }
		public virtual long Size { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual DateTime Modified { get; set; }
		public virtual Control Summary { get; set; }
		public virtual IEnumerable<ItemInfo> Children { get { yield break; } }
		public virtual IFileManager Container { get; set; }
		public virtual void Open() { Container.Path = Path; }
		public ItemInfo(string path, IFileManager container) {
			Path = path;
			Container = container;
		}
	}

	public class FileItemInfo: ItemInfo {
		string path;

		public override string Path {
			get { return path; }
			set {
				path = value;
				var ext = Services.Paths.Extension(path);
				var info = Services.Files.Info(path);
				if (info == null) {
					var vinfo = Services.Files.Virtual(path);
					if (vinfo != null && vinfo is System.Web.Hosting.VirtualDirectory) Class = Classes.Directory;
					else Class = Classes.Item;
					Created = DateTime.MinValue;
					Modified = DateTime.MinValue;
					Size = 0;
				} else {
					if (info is System.IO.DirectoryInfo) {
						Class = Classes.Directory;
						Created = info.CreationTimeUtc;
						Modified = info.LastWriteTimeUtc;
						Size = 0;
					} else {
						Class = Classes.Item;
						Created = info.CreationTimeUtc;
						Modified = info.LastWriteTimeUtc;
						Size = ((FileInfo)info).Length;
					}
				}
				Name = NativeName = (path != "~" && path != "~/") ? Services.Paths.File(path) : "~";
				if (Services.Paths.Extension(Services.Paths.WithoutExtension(Name)) == "fm") {
					Name = Services.Paths.WithoutExtension(Services.Paths.WithoutExtension(Name));
					var dot = Name.IndexOf('.');
					if (dot > 0 && Name.Substring(0, dot).Any(ch => '0' <= ch && ch <= '9')) Name = Name.Substring(dot + 1);
				}
				if (Class == Classes.Item) {
					if (ext == "ascx") Class = Classes.Control;
					else if (ext == "aspx" || ext == "cshtml" || ext == "vbhtml" || ext == "html" || ext == "htm" || ext == "php" || ext == "jsp") {
						Class = Classes.Page;
					}
				}
			}
		}
		public override string NativePath { get { return Parent != null ? Services.Paths.Combine(Parent.NativePath, NativeName) : Services.Paths.Combine(Services.Paths.Directory(Path), NativeName); } set { } }
		public override string NativeName { get { return Name; } set { } }
		public override string DownloadUrl { get { return Services.Paths.Url(Path); } set { } }

		public override IEnumerable<ItemInfo> Children {
			get {
				if (Class == Classes.Directory) {
					var dir = new System.IO.DirectoryInfo(Services.Paths.Map(Path));
					var items = dir.EnumerateFileSystemInfos()
						.Select(p => new FileItemInfo(Services.Paths.Unmap(p.FullName), Container));
					return items.Where(p => p.Class == Classes.Directory)
						.Append(items.Where(p => p.Class != Classes.Directory));
				}
				return base.Children;
			}
		}
		public FileItemInfo(string path, IFileManager container) : base(path, container) { }
	}

	public class ItemPresenter<T, C> : Presenter {
		public virtual T Item { get; set; }
		public virtual C Container { get; set; }
	}

	public class ContentPresenter<T, C> : ItemPresenter<T, C> { }
	public class HierarchyPresenter<T, C> : ItemPresenter<T, C> { }
	public class MenuPresenter<T, C> : ItemPresenter<T, C> { }
	public class MainPanelPresenter<T, C>: ItemPresenter<T, C> {
		public CssStyleCollection Style { get; set; }
		public string CssClass { get; set; }
		public Unit Width { get; set; }
		public Unit Height { get; set; }
	}

	public class FileSelectedArgs : EventArgs {
		public List<string> SelectedFiles { get; set; }
	}

	public class FileManagerService<Self> : ExtensiblePresenterService<Self> where Self: FileManagerService<Self> , new() {
		public ItemInfo Info(FileManager<Self> fm) { try { return ((FileManagerProvider<Self>)Provider).Info(fm); } catch { } return null; }
		public ContentPresenter<ItemInfo, FileManager<Self>> Content(FileManager<Self> fm) { try { return ((FileManagerProvider<Self>)Provider).Content(fm); } catch { } return null; }
		public HierarchyPresenter<ItemInfo, FileManager<Self>> Hierarchy(FileManager<Self> fm) { try { return ((FileManagerProvider<Self>)Provider).Hierarchy(fm); } catch { } return null; }
		public MenuPresenter<ItemInfo, FileManager<Self>> Menu(FileManager<Self> fm) { try { return ((FileManagerProvider<Self>)Provider).Menu(fm); } catch { } return null; }
		public MainPanelPresenter<ItemInfo, FileManager<Self>> MainPanel(FileManager<Self> fm) { try { return ((FileManagerProvider<Self>)Provider).MainPanel(fm); } catch { } return null; }
	}
	public abstract class FileManagerProvider<Service> : ExtensiblePresenterProvider<Service> where Service: FileManagerService<Service>, new() {
		public abstract ItemInfo Info(FileManager<Service> fm);
		public abstract ContentPresenter<ItemInfo, FileManager<Service>> Content(FileManager<Service> fm);
		public abstract HierarchyPresenter<ItemInfo, FileManager<Service>> Hierarchy(FileManager<Service> fm);
		public abstract MenuPresenter<ItemInfo, FileManager<Service>> Menu(FileManager<Service> fm);
		public abstract MainPanelPresenter<ItemInfo, FileManager<Service>> MainPanel(FileManager<Service> fm);

	}

	public interface IFileManager {
		FileManager.Modes Mode { get; set; }
		FileManager.Views View { get; set; }
		FileManager.Roots Root { get; set; }
		string RootPath { get; set; }
		string Path { get; set; }
		bool ShowPreview { get; set; }
		ItemInfo Info { get; }
		HashSet<string> Selection { get; }
		CssStyleCollection Style { get; }
		string CssClass { get; set; }
		Unit Width { get; set; }
		Unit Height { get; set; }
	}

	public class FileManager<T>: ExtensiblePresenter<T>, IFileManager where T: FileManagerService<T>, new() {

		public virtual string Paths(FileManager.Roots path) {
			switch (path) {
				case FileManager.Roots.Root: return AppRoot;
				case FileManager.Roots.Home: return Home;
				case FileManager.Roots.Public: return Public;
				case FileManager.Roots.Documents: return Documents;
				case FileManager.Roots.Images: return Images;
				case FileManager.Roots.Media: return Media;
				case FileManager.Roots.SystemSettings: return "~/Silversite/Admin/Settings";
				case FileManager.Roots.UserSettings: return Home + "/Settings";
				case FileManager.Roots.Custom: return RootPath;
				default: return Home;
			}
		}

		public const string AppRoot = "~/";
		public virtual string Public {
			get {
				var pub = Services.Files.PublicUserFilesFolder;
				return Services.Domains.Path(pub);
			}
		}
		public virtual string Home { get { return Services.Persons.Current != null ? Services.Persons.Current.HomePath : Public; } }
		public virtual string Documents { get { return Home + "/Documents"; } }
		public virtual string Images { get { return Home + "/Images"; } }
		public virtual string Media { get { return Home + "/Media"; } }


		public virtual FileManager.Modes Mode {
			get { return (FileManager.Modes)(ViewState["Mode"] ?? FileManager.Modes.Explore); }
			set { ViewState["Mode"] = value; }
		}

		bool childrenCreated = false;
		string path = null;
		public virtual string Path {
			get { return (string)(ViewState["Path"] ?? path ?? Home); }
			set {
				path = value;
				if (ViewState["Path"] != value) {
					ViewState["Path"] = value;
					Info.Open();
					if (childrenCreated) { childrenCreated = false; CreateChildControls(); }
				}
			}
		}
		public virtual FileManager.Roots Root {
			get { return (FileManager.Roots)(ViewState["Root"] ?? FileManager.Roots.Home); }
			set { ViewState["Root"] = value; }
		}
		string rootpath = null;
		public virtual string RootPath {
			get {
				if (rootpath != null) return rootpath;
				return Paths(Root);
			}
			set { rootpath = value; }
		}

		public virtual FileManager.Views View {
			get { return (FileManager.Views)(ViewState["View"] ?? FileManager.Views.MediumIcons); }
			set { ViewState["View"] = value; }
		}

		public virtual bool ShowPreview { get; set; }

		public virtual ItemInfo Info { get { var info = Service.Info(this); info.Container = this; return info; } }

		public virtual HashSet<string> Selection { get { return (HashSet<string>)ViewState["Selection"]; } private set { ViewState["Selection"] = value; } }

		Panel style = new Panel();
		public virtual CssStyleCollection Style {
			get {
				return MainPanel != null ? MainPanel.Style : style.Style;
			}
		}
		public virtual string CssClass {
			get {
				return MainPanel != null ? MainPanel.CssClass : style.CssClass;
			}
			set {
				style.CssClass = value;
				if (MainPanel != null) MainPanel.CssClass = value;
			}
		}
		public virtual Unit Width {
			get {
				return MainPanel != null ? MainPanel.Width : style.Width;
			}
			set {
				style.Width = value;
				if (MainPanel != null) MainPanel.Width = value;
			}
		}
		public virtual Unit Height {
			get {
				return MainPanel != null ? MainPanel.Height : style.Height;
			}
			set {
				style.Height = value;
				if (MainPanel != null) MainPanel.Height = value;
			}
		}

		public virtual ContentPresenter<ItemInfo, FileManager<T>> Content { get; set; }
		public virtual HierarchyPresenter<ItemInfo, FileManager<T>> Hierarchy { get; set; }
		public virtual MenuPresenter<ItemInfo, FileManager<T>> Menu { get; set; }
		public virtual MainPanelPresenter<ItemInfo, FileManager<T>> MainPanel { get; set; }

		public FileManager()  {
			Selection = new HashSet<string>();
			ShowPreview = true;
		}

		protected override void OnInit(EventArgs e) {
			//EnsureChildControls();
			base.OnInit(e);
		}
		bool viewStateLoaded = false;
		protected override void LoadViewState(object savedState) {
			base.LoadViewState(savedState);
			viewStateLoaded = true;
			if (Page.Request.QueryString["mode"] != null) Mode = Page.Request.QueryString["mode"].Parse<FileManager.Modes>();
			if (Page.Request.QueryString["path"] != null) Path = Page.Request.QueryString["path"];
			CreateChildControls();
		}
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			viewStateLoaded = true;
			if (Page.Request.QueryString["mode"] != null) Mode = Page.Request.QueryString["mode"].Parse<FileManager.Modes>();
			if (Page.Request.QueryString["path"] != null) Path = Page.Request.QueryString["path"];
			CreateChildControls();
			//if (Page.Request.QueryString["root"] != null) Path = Page.Request.QueryString["root"].Parse<Roots>();
		}

		public void Apply() { CreateChildControls(); }

		protected override void CreateChildControls() {
			if (!childrenCreated && viewStateLoaded) {
				if (viewStateLoaded) base.CreateChildControls();
				if (style.CssClass != null) CssClass = style.CssClass;
				if (style.Width != Unit.Empty) Width = style.Width;
				if (style.Height != Unit.Empty) Height = style.Height;
				if (style.Style.Count > 0) {
					foreach (var key in style.Style.Keys.OfType<string>()) Style.Add(key, style.Style[key]);
				}
				childrenCreated = true;
			}
		}
	}

	public class FileManagerService: FileManagerService<FileManagerService> { }
	public abstract class FileManagerProvider: FileManagerProvider<FileManagerService> {
		public override ItemInfo Info(FileManager<FileManagerService> fm) { return new FileItemInfo(fm.Path, fm); }
		public override void CreateChildControls(Presenter p) { p.Controls.Clear(); }
	}
	public class FileManager: FileManager<FileManagerService>, IFileManager {
		public enum Modes { SelectFile, SelectFolder, SelectImage, Explore }
		public enum Roots { Root, Home, Public, Documents, Images, Media, SystemSettings, UserSettings, Custom }
		public enum Views { HugeIcons = 0, BigIcons = 1, MediumIcons = 2, SmallIcons = 3, List = 4, Details = 5, Summary = 6 }
	}
}
