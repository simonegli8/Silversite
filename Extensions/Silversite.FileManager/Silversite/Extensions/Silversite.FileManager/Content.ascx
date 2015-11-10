<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Content.ascx.cs" Inherits="Silversite.Providers.FileManager.UserControls.Content" %>
<%@ Register TagPrefix="fm" TagName="Item" Src="~/Silversite/Extensions/Silversite.FileManager/Item.ascx" %>

<asp:Panel ID="ContentPanel" runat="server" CssClass="Silversite-FileManagerBox Silversite-FileManagerContent">

	<asp:Panel ID="icons" runat="server" Visible="true">
		<asp:PlaceHolder ID="iconsContent" runat="server" />
	</asp:Panel>

	<asp:Panel ID="details" runat="server" Visible="false">
		<table class="Silversite-FileManagerDetailTable">
			<tr>
				<th>&nbsp;</th>
				<th>Name</th>
				<th>Size</th>
				<th>Last Modified</th>
			</tr>
			<asp:PlaceHolder ID="detailsContent" runat="server" />
		</table>
	</asp:Panel>

	<asp:Panel ID="content" runat="server" Visible="false">
	</asp:Panel>

</asp:Panel>
