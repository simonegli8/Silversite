<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Silversite/Test/master/test.master" CodeBehind="entitytest.aspx.cs" Inherits="Silversite.Web.UI.Pages.Base.entitytest" %>

<asp:Content runat="server" ContentPlaceHolderID="content">

	<ss:Head runat="server">
		<style type="text/css">
			.messages {
				border: 1 solid black;
				border-radius: 8px;
				width:800px;
				height:600px;
			}
		</style>
	</ss:Head>

	<asp:Button runat="server" OnClick="StartTest" Text="Start Test" />

	<ss:ProviderMessagesMonitor runat="server" CssClass="messages" ID="msg" />

</asp:Content>