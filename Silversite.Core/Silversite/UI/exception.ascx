<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="exception.ascx.cs" Inherits="Silversite.Web.UI.ExceptionControlBase" ClassName="Silversite.Web.UI.ExceptionControl" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Homesell" %>

<asp:Panel runat="server" ID="ExceptionContainer" CssClass="ExceptionContainer">

	<ss:Head runat="server">
		<style type="text/css">
			.source, .stack, .info {
				background-color: #D7D7D7;
				width:95%;
				border: 1 solid #AAA;
				margin: 15px;
				padding: 15px;
				border-radius:5px;
				font-size:10pt;
			}
		</style>
	</ss:Head>

	<h2><asp:Label ID="title" runat="server"/></h2>

	<asp:Panel runat="server" CssClass="source">
		<asp:Literal ID="source" runat="server" />
		<hr/>
		<strong>Source File:</strong> <asp:Label ID="file" runat="server" /> &nbsp; &nbsp; &nbsp; &nbsp; <strong>Line:</strong> <asp:Label ID="line" runat="server" /><br />
	</asp:Panel>

	<asp:Panel runat="server" CssClass="stack" ID="stack">
		<strong>[<asp:Label ID="stacktitle" runat="server" />]</strong><br />
		<asp:Panel runat="server" ID="frames" />
	</asp:Panel>

	<asp:Panel runat="server" ID="infopanel" CssClass="info">
		<asp:Literal ID="info" runat="server" />
	</asp:Panel>

</asp:Panel>
