<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="db.aspx.cs" Inherits="Silversite.SilversiteDevelop.Web.UI.Pages.db" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

	<p>

		<asp:Button runat="server" Text="Create DB" OnClick="CreateDb" />
		<asp:Button runat="server" Text="Drop DB" OnClick="DropDb" />

	</p>

	<h2>Context pool</h2>

	<asp:GridView runat="server" ID="list">
		<Columns>
			<asp:BoundField 
		</Columns> 
	</asp:GirdView>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="adds" runat="server"></asp:Content>
