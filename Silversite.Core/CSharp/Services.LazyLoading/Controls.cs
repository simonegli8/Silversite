using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services.LazyLoading {

	public class ControlCollection: TypeCollection {
		public override void Save() { Save(Lazy.Configuration.RegisteredControls, info => new TypeConfigurationElement { Type = info.TypeAssemblyQualifiedName }); }
		public override void Load() { Load(Lazy.Configuration.RegisteredControls, e => new TypeInfo { TypeAssemblyQualifiedName = e.Type }); }
	}

}