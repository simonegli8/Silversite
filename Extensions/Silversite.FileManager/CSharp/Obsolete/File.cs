using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Silversite;
using Silversite.Services;

namespace Silversite.FileManager {

	public class File {

		public static string Stats(string Path) {
			if (Files.FileExists(Path)) {
				var info = Files.FileInfo(Path);
				var units = new string[] { "Bytes", "KB", "MB", "GB", "TB" };
				var size = (double)info.Length;
				int n = 0;
				while (n <= 4 && size >= 1000) {
					n++;
					size = size / 1024;
				}

				return info.LastWriteTime + ";    " + size.ToString("D:3") + " " + units[n];
			}
			return null;
		}

		public static void SetupButton(Silversite.Web.UI.FileManager m, LinkButton button, string path) {
			var p = path;
			button.Click += (sender, args) => {
				m.Path = p;
				m.Apply();
			};
		}

		public static void Add(Silversite.Web.UI.FileManager m, Handler h, string Path) {

			Panel details = m.DetailsView;
			var button = new LinkButton();
			var View = m.View;
			var Selected = m.Selection.Contains(Path);

			// icon

			int width = 81;
			int height = 60;

			switch (View) {
				case Silversite.Web.UI.FileManager.Views.HugeIcons: width *= 2; height *= 3; break;
				case Silversite.Web.UI.FileManager.Views.BigIcons: break;
				case Silversite.Web.UI.FileManager.Views.MediumIcons:
					width = width * 4 / 7;
					height = height * 4 / 7;
					break;
				case Silversite.Web.UI.FileManager.Views.SmallIcons:
				case Silversite.Web.UI.FileManager.Views.List:
				case Silversite.Web.UI.FileManager.Views.Details:
					width = width / 5;
					height = height / 5;
					break;
			}
			var icon = new Icon(Path, width, height);
			icon.ToolTip = Stats(Path);

			var label = new Label();
			label.ToolTip = Stats(Path);
			label.Font.Size = new FontUnit(8);
			label.Style.Add("line-height", "0.8em");
			label.Font.Names = new string[] { "Tahoma", "Arial", "Helvetica", "Sans serif" };
			switch (View) {
				case Silversite.Web.UI.FileManager.Views.List:
				case Silversite.Web.UI.FileManager.Views.SmallIcons:
				case Silversite.Web.UI.FileManager.Views.MediumIcons:
				case Silversite.Web.UI.FileManager.Views.BigIcons:
					label.Text = Paths.File(Path);
					break;
				case Silversite.Web.UI.FileManager.Views.Details:
					//label.Text = Stats.Replace("", "&nbsp;&nbsp;");
					break;
			}

			var div = new Panel();
			var outerdiv = new Panel();
			var innerdiv = new Panel();
			outerdiv.Style.Add("background", "white");
			//outerdiv.Style.Add("border", "1px solid blue");
			div.Controls.Add(innerdiv);
			outerdiv.Controls.Add(div);
			outerdiv.Style.Add("overflow", "hidden");
			var str = new StringBuilder();
			foreach (char ch in label.Text) {
				str.Append(ch);
				str.Append("<wbr/>");
			}
			div.Style.Add("border-radius", "3px");
			switch (View) {
				case Silversite.Web.UI.FileManager.Views.HugeIcons:
					innerdiv.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					innerdiv.Style.Add("margin", "auto");
					div.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					outerdiv.Style.Add("float", "left");
					outerdiv.Style.Add("padding", "5px");

					innerdiv.Controls.Add(icon);
					innerdiv.Controls.Add(new Literal() { Text = "<br/>" });
					label.Text = str.ToString();
					innerdiv.Controls.Add(label);

					div.Width = 200;
					div.Height = 298;
					outerdiv.Height = 318;
					outerdiv.Width = 210;
					button.Controls.Add(outerdiv);
					details.Controls.Add(button);
					SetupButton(m, button, Path);
					break;
				case Silversite.Web.UI.FileManager.Views.BigIcons:
					innerdiv.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					innerdiv.Style.Add("margin", "auto");
					div.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					outerdiv.Style.Add("float", "left");
					outerdiv.Style.Add("padding", "5px");

					innerdiv.Controls.Add(icon);
					innerdiv.Controls.Add(new Literal() { Text = "<br/>" });
					label.Text = str.ToString();
					innerdiv.Controls.Add(label);

					div.Width = 105;
					div.Height = 144;
					outerdiv.Height = 154;
					outerdiv.Width = 115;
					button.Controls.Add(outerdiv);
					details.Controls.Add(button);
					SetupButton(m, button, Path);
					break;
				case Silversite.Web.UI.FileManager.Views.MediumIcons:
					innerdiv.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					innerdiv.Style.Add("margin", "auto");
					div.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "center");
					outerdiv.Style.Add("float", "left");
					outerdiv.Style.Add("padding", "5px");

					innerdiv.Controls.Add(icon);
					innerdiv.Controls.Add(new Literal() { Text = "<br/>" });
					label.Text = str.ToString();
					innerdiv.Controls.Add(label);

					div.Width = 60;
					div.Height = 81;
					outerdiv.Height = 91;
					outerdiv.Width = 65;
					button.Controls.Add(outerdiv);
					details.Controls.Add(button);
					SetupButton(m, button, Path);
					break;
				case Silversite.Web.UI.FileManager.Views.SmallIcons:
				case Silversite.Web.UI.FileManager.Views.List:
					div.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "left");
					innerdiv.Style.Add(System.Web.UI.HtmlTextWriterStyle.TextAlign, "left");
					innerdiv.Style.Add("margin", "auto");
					innerdiv.Controls.Add(icon);
					innerdiv.Controls.Add(new Literal() { Text = "&nbsp;" });
					innerdiv.Controls.Add(label);
					div.Controls.Add(innerdiv);
					div.Width = 81;
					div.Height = 20;
					div.Style.Add("text-align", "left");
					div.Style.Add("border-radius", "3px");
					outerdiv.Width =91;
					outerdiv.Height = 24;
					outerdiv.Style.Add("padding", "5px 2px 5px 2px");
					outerdiv.Style.Add("float", "left");
					button.Controls.Add(outerdiv);
					details.Controls.Add(button);
					SetupButton(m, button, Path);
					break;
				case Silversite.Web.UI.FileManager.Views.Details:
					var tr = new Literal() { Text = "<tr style=\"background:white;\"><td>" };
					details.Controls.Add(tr);
					var iconbutton = new LinkButton();
					iconbutton.Controls.Add(icon);
					details.Controls.Add(iconbutton);
					var textbutton = new LinkButton(){ Text = Paths.File(Path) };
					details.Controls.Add(textbutton);
					var datebutton = new LinkButton() { Text = Stats(Path).Tokens().First() };
					details.Controls.Add(datebutton);
					var sizebutton = new LinkButton() { Text = Stats(Path).Tokens().Last() };
					details.Controls.Add(sizebutton);
					SetupButton(m, iconbutton, Path);
					SetupButton(m, datebutton, Path);
					SetupButton(m, sizebutton, Path);
					details.Controls.Add(new Literal() { Text = "</td></tr>" });
					break;
			}
		}
	}
}