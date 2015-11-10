<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="admins.ascx.cs" Inherits="Silversite.Web.UI.AdminsControlBase" ClassName="Silversite.Web.UI.AdminsControl" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web" Assembly="Silversite.Core" %>

<asp:Panel runat="server" ID="root" CssClass="AdminsContainer">

	<h1>Benutzer verwalten</h1>

	<p class="tools">
		<asp:Button ID="Button1" runat="server" Text="Neuen Benutzer erstellen" OnClick="OnNewAdmin" /> <br />
		<asp:Label ID="msg" runat="server" EnableViewState="False" />
	</p>
	
	<ss:UsersSource ID="source" runat="server" />
    
    <asp:GridView ID="grid" runat="server" DataSourceID="source" AllowPaging="true" PageSize="20" DataKeyNames="UserName" CssClass="grid"
		OnRowCommand="RowCommand" AutoGenerateColumns="false" >
		<Columns>
			<ss:IconField Set="Delete:ConfirmText=Do you really want to delete this Administrator?" IconCssClass="icons" />
			<asp:TemplateField HeaderText="Email" SortExpression="Email" >
				<ItemTemplate>
					<asp:HyperLink ID="HyperLink1" runat="server" Text='<%# Eval("Email") %>' NavigateUrl='<%# Eval("Email", "mailto:{0}")  %>' />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField HeaderText="Sperre">
				<ItemTemplate>
					<asp:LinkButton ID="LinkButton1" runat="server" CommandArgument='<%# Eval("UserName") %>' CommandName="Unlock" Text="Sperre aufheben"
						Visible='<%# Eval("User.IsLockedOut") %>' />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="Title" />
			<asp:BoundField DataField="FirstName" />
			<asp:BoundField DataField="LastName" />
			<asp:BoundField DataField="Address" />
			<asp:BoundField DataField="Zip" />
			<asp:BoundField DataField="City" />
			<asp:BoundField DataField="Country" />
			<asp:BoundField DataField="TimeZone" />
			<asp:BoundField DataField="Culture" />
			<asp:BoundField DataField="Phone" />
			<asp:BoundField DataField="Email" />
		</Columns>
    </asp:GridView>

	<asp:DetailsView ID="details" runat="server" AutoGenerateRows="true" DataKeyNames="Email" DataSourceID="source" />

</asp:Panel>
