<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Master/xhtml5.master" AutoEventWireup="true" CodeBehind="login.aspx.cs"
	Inherits="Silversite.Web.UI.Pages.Base.Login" ClassName="Silversite.Web.UI.Pages.Login" %>
<%@ Register TagPrefix="ss" TagName="Login" Src="~/Silversite/UI/login.ascx" %>

<asp:Content ContentPlaceHolderID="body" runat="server">

	<div style="text-align:center;margin: 0 auto;width:400px;">
		<div>
			<h1>Login</h1>
			<ss:Login runat="server" />
		</div>
	</div>

</asp:Content>
