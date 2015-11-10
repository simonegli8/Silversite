using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Silversite.Web.UI {

	[ParseChildren(true)]
	public class IconField: TemplateField {

		public IconField(): base() { }

		[Category("Behavior")]
		[Browsable(true)]
		public string Set { get { return (string)ViewState["Set"]; } set { ViewState["Set"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public string EditSet { get { return (string)ViewState["EditSet"]; } set { ViewState["EditSet"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public string InsertSet { get { return (string)ViewState["InsertSet"]; } set { ViewState["InsertSet"] = value;  } }

		[Category("Behavior")]
		[Browsable(true)]
		public string CommandArgument { get { return (string)ViewState["CommandArgument"]; } set { ViewState["CommandArgument"] = value; } }

		[Category("Behavior")]
		[Browsable(true)]
		public bool CausesValidation { get { return (bool)(ViewState["CausesValidation"] ?? true); } set { ViewState["CausesValidation"] = value; } }
	
		[Category("Appearance")]
		[Browsable(true)]
		public string IconCssClass { get { return (string)ViewState["IconCssClass"]; } set { ViewState["IconCssClass"] = value; } }

		/* void SetTemplate() {
			if (!string.IsNullOrEmpty(Set)) ItemTemplate = new IconBar(Set, CommandArgument, IconCssClass);
			if (!string.IsNullOrEmpty(EditSet)) EditItemTemplate =  new IconBar(EditSet, CommandArgument, IconCssClass);
			if (!string.IsNullOrEmpty(InsertSet)) InsertItemTemplate = new IconBar(InsertSet, CommandArgument, IconCssClass);
		} */

		public override ITemplate EditItemTemplate {
			get { return base.EditItemTemplate ?? new IconBar(EditSet, CommandArgument, IconCssClass); }
			set { base.EditItemTemplate = value; }
		}

		public override ITemplate ItemTemplate {
			get { return base.ItemTemplate ?? new IconBar(Set, CommandArgument, IconCssClass); }
			set { base.ItemTemplate = value; }
		}

		public override ITemplate InsertItemTemplate {
			get { return base.InsertItemTemplate ?? new IconBar(InsertSet, CommandArgument, IconCssClass); }
			set { base.InsertItemTemplate = value; }
		}

		public override ITemplate AlternatingItemTemplate {
			get { return base.AlternatingItemTemplate ?? new IconBar(Set, CommandArgument, IconCssClass); }
			set { base.AlternatingItemTemplate = value; }
		}


		protected override void CopyProperties(DataControlField newField) {
			base.CopyProperties(newField);
			if (newField is IconField) {
				var nf = (IconField)newField;
				nf.Set = Set; nf.EditSet = EditSet; nf.InsertSet = InsertSet; nf.IconCssClass = IconCssClass;
			}
		}

		protected override DataControlField CreateField() {
			return new IconField();
		}
		
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
			base.InitializeCell(cell, cellType, rowState, rowIndex);
			if ((cellType != DataControlCellType.Header) && (cellType != DataControlCellType.Footer)) {
				IPostBackContainer container = base.Control as IPostBackContainer;
				int n = cell.Controls.Count;
				if ((rowState & DataControlRowState.Edit) != 0) EditItemTemplate.InstantiateIn(cell);
				else if ((rowState & DataControlRowState.Insert) != 0) InsertItemTemplate.InstantiateIn(cell);
				else if ((rowState & DataControlRowState.Alternate) != 0) AlternatingItemTemplate.InstantiateIn(cell);
				else ItemTemplate.InstantiateIn(cell);
				var icons = cell.Controls.OfType<Icon>();
				foreach (var icon in icons) {
					if (string.IsNullOrEmpty(icon.CommandArgument)) icon.CommandArgument = rowIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
					icon.CausesValidation = CausesValidation;
				}
			}
		}

 

 

		// public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) { base.InitializeCell(cell, cellType, rowState, rowIndex); }
	}

}
