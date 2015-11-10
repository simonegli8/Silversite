<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="iconbar.aspx.cs" Inherits="Silversite.silversite.test.IconBar" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
	<ss:IconBar runat="server" Set="Delete|Edit:Label=Hallo;|Print:Label=Test" />
	<hr />
	<ss:IconBar runat="server" Set="Delete|*|Print">
		<ss:EditIcon runat="server" />
	</ss:IconBar>
</asp:Content>
