<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/Master/test.master" AutoEventWireup="true" CodeBehind="splitter.aspx.cs" Inherits="Silversite.SilversiteDevelop.Silversite.Test.splitter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">

	<ss:Splitter runat="server">
		<Left>
			<asp:Panel runat="server" ID="pane1">
				<h1>Left</h1>
			</asp:Panel>
		</Left>
		<Center>
			<asp:Panel runat="server" ID="pane2">
				<h1>Right</h1>
			</asp:Panel>
		</Center>
	</ss:Splitter>

</asp:Content>
