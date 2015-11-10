<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="logtest.aspx.cs" Inherits="Silversite.silversite.test.LogTest" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

	<h1>Write to Log:</h1>
	<p>
		Categroy: <asp:TextBox ID="cat" runat="server" Width="200" /><br />
		Message:<br />
		<asp:TextBox ID="msg" runat="server" Width="500" Height="400" TextMode="MultiLine" /><br />
		Immediate: <asp:CheckBox ID="im" runat="server" /><br/>
		With exception: <asp:CheckBox ID="ex" runat="server" />
	</p>
	<p>
		<asp:Button runat="server" Text="Write" OnClick="WriteClick" /><br />
	</p>
	
	<asp:Button runat="server" OnClick="LogClick" Text="Show Log" />
</asp:Content>
