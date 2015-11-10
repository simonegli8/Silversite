using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.UI;

namespace Silversite.Web.UI {
	public class Container: EditableContent {

		[Category("Behavior")]
		[Browsable(true)]
		public string ItemType { get { return (string)ViewState["ItemType"]; } set { ViewState["ItemType"] = value; } }

		public Icon AddButton = new AddIcon();
		public Icon RemoveButton = new RemoveIcon();

		protected override void OnMenuCommand(System.Web.UI.WebControls.CommandEventArgs c) {
			base.OnMenuCommand(c);
			switch (c.CommandName) {
			case "Add":
				var e = new Html.Element();
				e.Type = Type.GetType(ItemType);
				e.IsServerControl = true;
				Element.Children.Add(e);
				Preview();
				Refresh();
				break;
			case "Remove":
				int n = int.Parse((string)c.CommandArgument);
				var re = Element.Children.OfType<Html.Element>().Where(ce => ce.Type == Type.GetType(ItemType)).Skip(n).FirstOrDefault();
				Element.Children.Remove(re);
				Preview();
				Refresh();
				break;
			default: break;
			}
		}

		protected override void CreateChildControls() {
			base.CreateChildControls();

			EditButton.Visible = false;
			RemoveButton.OnClientClick = "$('.Silversite_Container_RemoveButton').toggle();";

			ButtonsMenu.Controls.AddAt(1, AddButton);
			ButtonsMenu.Controls.AddAt(2, RemoveButton);

			int n = 0;
			foreach (EditableContent child in Controls) {
				var removeButton = new RemoveIcon();
				removeButton.Style[HtmlTextWriterStyle.Visibility] = "hidden";
				removeButton.CssClass = "Silversite_Container_RemoveButton";
				removeButton.CommandArgument = n.ToString();
				child.ButtonsMenu.Controls.AddAt(0, removeButton);
			}
		}

	}
}