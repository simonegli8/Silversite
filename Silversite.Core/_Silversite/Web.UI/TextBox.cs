using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Silversite.Web.UI {

	public class TextBox: System.Web.UI.WebControls.TextBox {
		static short IDIndex = 0;

		public string Valid { get; set; }
		public string ConfirmationOf { get; set; }
		public string Label { get; set; }
		public string Placeholder { get; set; }
		public bool Required { get; set; }
		public string InvalidMessage { get; set; }
		public string MissingMessage { get; set; }
		public string MismatchMessage { get; set; }
		
		string UniqueID { get { if (string.IsNullOrEmpty(ID)) ID = "TextBox" + ++IDIndex; return ID; } }

		protected virtual void PlaceLabel(Label label) {
			var ix = Parent.Controls.IndexOf(this);
			Parent.Controls.AddAt(ix, label);
		}

		protected override void CreateChildControls() {
			base.CreateChildControls();
			if (!string.IsNullOrEmpty(Label)) {
				if (Parent != null && Parent.Controls.Contains(this)) {
					var label = new Label();
					label.AssociatedControlID = UniqueID;
					label.Text = Label;
					PlaceLabel(label);
				}
			}
			if (!string.IsNullOrEmpty(Valid) && !string.IsNullOrEmpty(InvalidMessage)) {
				var val = new RegularExpressionValidator();
				val.ID = UniqueID + "_RegularExpressionValidator";
				val.ValidationExpression = Valid;
				val.Text = InvalidMessage;
				val.ControlToValidate = UniqueID;
				var callout = new AjaxControlToolkit.ValidatorCalloutExtender();
				callout.ID = UniqueID + "_RegularExpressionValidatorCalloutExtender";
				callout.TargetControlID = val.ID;
				Controls.Add(val);
				Controls.Add(callout);
			}
			if (!string.IsNullOrEmpty(ConfirmationOf) && !string.IsNullOrEmpty(MismatchMessage)) {
				var val = new CompareValidator();
				val.ID = UniqueID + "_CompareValidator";
				val.ControlToCompare = ConfirmationOf;
				val.Text = MismatchMessage;
				val.ControlToValidate = UniqueID;
				var callout = new AjaxControlToolkit.ValidatorCalloutExtender();
				callout.ID = UniqueID + "_CompareValidatorCalloutExtender";
				callout.TargetControlID = val.ID;
				Controls.Add(val);
				Controls.Add(callout);
			}
			if (Required) {
				var val = new RequiredFieldValidator();
				val.ID = UniqueID + "_RequiredFieldValidator";
				val.Text = MissingMessage;
				val.ControlToValidate = UniqueID;
				var callout = new AjaxControlToolkit.ValidatorCalloutExtender();
				callout.ID = UniqueID + "_RequiredFieldValidatorCalloutExtender";
				callout.TargetControlID = val.ID;
				Controls.Add(val);
				Controls.Add(callout);
			}
			if (!string.IsNullOrEmpty(Placeholder)) {
				Attributes["placeholder"] = "Placeholder";
			}
		}
	}

	public class TextBoxRow: TextBox {
	
		Label label = null;

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			writer.RenderBeginTag("tr");
			if (label != null) {
				writer.RenderBeginTag("td");
				label.RenderBeginTag(writer);
				label.RenderControl(writer);
				label.RenderEndTag(writer);
				writer.RenderEndTag();
			}
			writer.RenderBeginTag("td");
			base.Render(writer);
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		protected override void PlaceLabel(Label label) {
			this.label = label;
		}
	}

}