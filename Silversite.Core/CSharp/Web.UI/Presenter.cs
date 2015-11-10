using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Silversite.Web.UI {

	public class Presenter : UserControl { }

	public class ExtensiblePresenter<PresenterService> : Presenter where PresenterService: ExtensiblePresenterService<PresenterService>, new() {
		PresenterService service = null;
		public PresenterService Service { get { if (service == null) service = new PresenterService(); return service; } }
		protected override void CreateChildControls() {
			Controls.Clear();
			base.CreateChildControls();
			Service.CreateChildControls(this);
		}
	}

	public class ExtensiblePresenterService<Self> : Services.Service<ExtensiblePresenterProvider<Self>> where Self: ExtensiblePresenterService<Self>, new() {
		public void CreateChildControls(Presenter p) { try { Provider.CreateChildControls(p); } catch { } }
	}

	public abstract class ExtensiblePresenterProvider<Service> : Services.Provider<Service> {
		public abstract void CreateChildControls(Presenter p);
	}
	
}