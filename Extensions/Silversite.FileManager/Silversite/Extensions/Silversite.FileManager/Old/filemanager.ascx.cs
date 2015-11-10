using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Silversite.Services;

namespace Silversite.Web.UI.Controls.Extensions {
	public partial class FileManager : System.Web.UI.Control {

		class jqueryGlob { }
		class jqueryForm { }
		class jquerySplitter { }
		class jqueryFileTree { }
		class jqueryContextMenu { }
		class jqueryImpromptu { }
		class jqueryTablesorter { }
		class jqueryFilemanagerConfig { }
		class jqueryFilemanager { }

		protected override void OnLoad(EventArgs e) {
			Web.UI.Scripts.jQuery.Register(Page);
			base.OnLoad(e);
		}

		//TODO
		public string Path { get { return (string)ViewState["root"]; } set { ViewState["root"] = value; } }

		//TODO
		public event EventHandler<Silversite.Web.UI.FileSelectedArgs> FileSelected;
		
		//TODO
		public Web.UI.FileManager.Modes Mode { 
			get { return (Web.UI.FileManager.Modes)(ViewState["mode"] ?? Web.UI.FileManager.Modes.Explore); }
			set { ViewState["mode"] = value; }
		}

	}
}