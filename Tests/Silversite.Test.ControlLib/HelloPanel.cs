using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI
{
    public class HelloPanel: Panel
    {

		 protected override void CreateChildControls() {
			 base.CreateChildControls();
			 var l = new Literal();
			 l.Text = "Hello";
			 Controls.Add(l);
		 }

    }
}
