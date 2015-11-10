using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Silversite.FileManager {

	public abstract class Handler {

		public virtual string Files { get { return "*"; } }

		public virtual Panel ContextMenu { get { return null; } }

		public abstract void Open(Silversite.Web.UI.FileManager m, string path);

		public abstract ObjectClass Class { get; }
	}

	public abstract class DirectoryHandler: Handler {
		public override ObjectClass Class { get { return ObjectClass.Directory; } } 
	}

	public abstract class FileHandler : Handler {
		public override ObjectClass Class { get { return ObjectClass.File; } }
	}

}