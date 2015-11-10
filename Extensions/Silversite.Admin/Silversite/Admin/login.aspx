<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" AutoEventWireup="true"
	Inherits="Silversite.silversite.admin.Login" ClassName="Silversite.Web.UI.Login" %>
<%@ Register TagPrefix="ss" TagName="Login" Src="~/Silversite/ui/login.ascx" %>

<asp:Content ContentPlaceHolderID="page" runat="server" >

	<div style="text-align:center;margin: 0 auto;width:400px;">
		<div>
			<h1>Login</h1>
			<ss:Login runat="server" />
		</div>
	</div>

</asp:Content>