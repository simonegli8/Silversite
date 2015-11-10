<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Item.ascx.cs" Inherits="Silversite.Providers.FileManager.UserControls.Item" %>

<asp:Panel ID="icons" runat="server" Visible="true" Style="text-align:center">
	<asp:Image ID="iconimage" runat="server" /><br />
	<asp:Label ID="iconlabel" runat="server" CssClass="Silversite-FileManagerIconLabel"/>
</asp:Panel>

<tr ID="list" runat="server" visible="false">
	<td><asp:Image ID="detailimage" runat="server" /></td>
	<td><asp:Label ID="detailname" runat="server" /></td>
	<td><asp:Label ID="detailsize" runat="server" /></td>
	<td><asp:Label ID="detaildate" runat="server" /></td>
</tr>

