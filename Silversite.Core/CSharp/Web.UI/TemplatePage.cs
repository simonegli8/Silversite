using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Configuration;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using System.ComponentModel;
using Silversite.Web;


namespace Silversite.Web.UI {

	public class TemplatePageConfigurationElement: ConfigurationElement {

		public const string DefaultContents = "head=head;page";
		public const string DefaultMaster = "~/UI/master/default.master";

		[ConfigurationProperty("Class", IsRequired=false, DefaultValue=null, IsKey=true)]
		public string Class { get { return (string)this["Class"]; } set { this["Class"] = value; } }

		[ConfigurationProperty("File", IsRequired=false, DefaultValue=null, IsKey=true)]
		public string File { get { return (string)this["File"]; } set { this["File"] = value; } }

		[ConfigurationProperty("MasterPage", IsRequired=false, DefaultValue=DefaultMaster, IsKey=true)]
		public string MasterPage { get { return (string)this["MasterPage"] ?? DefaultMaster; } set { this["MasterPage"] = value; } }
	
		[ConfigurationProperty("Contents", IsRequired=false, DefaultValue=DefaultContents)]
		public string Contents { get { return (string)this["Contents"] ?? DefaultContents; } set { this["Contents"] = value; } }

		Dictionary<string, string> mappings = null;
		public Dictionary<string, string> Mappings {
			get {
				if (mappings == null) {
					mappings = new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(Contents)) {
						var contents = Contents.Split(';');
						foreach (string mapping in contents) {
							var tokens = mapping.Split('=');
							if (tokens.Length == 1) {
								mappings.Add("*", tokens[0]);
							} else if (tokens.Length >= 2) {
								mappings.Add(tokens[0], tokens[1]);
							}
						}
					}
				}
				return mappings;
			}
		}
	}

	public class TemplatePageConfigurationElementCollection: ConfigurationElementCollection, IEnumerable<TemplatePageConfigurationElement> {
		public TemplatePageConfigurationElement this[int index] {
			get {
				return base.BaseGet(index) as TemplatePageConfigurationElement;
			}
			set {
				if (base.BaseGet(index) != null) {
					base.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}

		protected override ConfigurationElement CreateNewElement() {
			return new TemplatePageConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element) {
			return ((TemplatePageConfigurationElement)element).MasterPage;
		}

		public new IEnumerator<TemplatePageConfigurationElement> GetEnumerator() {
			return this.OfType<TemplatePageConfigurationElement>().GetEnumerator();
		}

	}

	public class TemplatePageConfigurationSection: ConfigurationSection {

		public const string DefaultContents = TemplatePageConfigurationElement.DefaultContents;
		public const string DefaultMaster = TemplatePageConfigurationElement.DefaultMaster;

		TemplatePageConfigurationElement defaultElement = new TemplatePageConfigurationElement { Class = "*", File = string.Empty, MasterPage = DefaultMaster, Contents = DefaultContents };

		public TemplatePageConfigurationSection(): base() { }

		[ConfigurationProperty("", IsRequired=false, IsKey=false, IsDefaultCollection=true)]
		public TemplatePageConfigurationElementCollection Elements { get { return this[string.Empty] as TemplatePageConfigurationElementCollection; } }

		Dictionary<string, TemplatePageConfigurationElement> classes = null, files = null;

		public Dictionary<string, TemplatePageConfigurationElement> Classes {
			get {
				if (classes == null) {
					classes = new Dictionary<string, TemplatePageConfigurationElement>();
					foreach (var element in Elements.Where(e => !string.IsNullOrEmpty(e.Class) && string.IsNullOrEmpty(e.File))) {
						classes.Add(element.Class, element);
					}

					// add default element
					var de = Elements.FirstOrDefault(e => string.IsNullOrEmpty(e.Class) && string.IsNullOrEmpty(e.File));
					if (de != null) {
						de.Class = "*";
						defaultElement = de;
					}
					classes.Add(defaultElement.Class, defaultElement);
				}
				return classes;
			}
		}

		public Dictionary<string, TemplatePageConfigurationElement> Files {
			get {
				if (files == null) {
					files = new Dictionary<string, TemplatePageConfigurationElement>();
					foreach (var element in Elements.Where(e => !string.IsNullOrEmpty(e.File))) {
						files.Add(element.Class, element);
					}
				}
				return files;
			}
		}

		public TemplatePageConfigurationElement Get(Page page) {
			TemplatePageConfigurationElement element;
			if (!Files.TryGetValue(page.AppRelativeVirtualPath, out element)) {
				var type = GetType();
				while (type.IsSubclassOf(typeof(Page)) && !Classes.TryGetValue(type.Name, out element) && !Classes.TryGetValue(type.FullName, out element)) type = type.BaseType;
				if (element == null) element = defaultElement;
			}
			return element;
		}
	}

	public class TemplatePage: Page {

		public static TemplatePageConfigurationSection Configuration = new TemplatePageConfigurationSection();

		TemplatePageConfigurationElement config = null;
		
		protected override void OnPreInit(EventArgs e) {
			base.OnPreInit(e);
			if (config == null) config = Configuration.Get(this);
			MasterPageFile = config.MasterPage;
		}

	}

	[ParseChildren(DefaultProperty="Controls")]
	public class TemplateContent: Control {

		[Browsable(true)]
		public string ContentPlaceHolderID { get; set; }

		ContentPlaceHolder FindPlaceHolder(List<string> ids, MasterPage master) {
			ContentPlaceHolder h;
			if (ids.Count >= 0) {
				h = master.Controls.OfType<ContentPlaceHolder>().Select(c => new { Control = c, Index = ids.IndexOf(c.ID) } ).OrderBy(x => x.Index).FirstOrDefault(x => x.Index >= 0).Control;
			} else {
				h = master.Controls.OfType<ContentPlaceHolder>().FirstOrDefault();
			}
			// apply recursive to parent master pages.
			if (h == null) h = FindPlaceHolder(ids, master.Master);
			return h;
		}

		protected override void OnInit(EventArgs e) {
			var ids = ContentPlaceHolderID.Split(',', ';').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToList();
			
			int i = 0;
			string id;
			var config = TemplatePage.Configuration.Get(Page);
			while (i < ids.Count) {
				if (config.Mappings.TryGetValue(ids[i], out id)) ids[i] = id;
				i++;
			}

			var placeHolder = 	FindPlaceHolder(ids, Page.Master);
			if (placeHolder.Controls.Count == 0) placeHolder.Controls.Add(this);

			base.OnInit(e);
		}
	}

}
