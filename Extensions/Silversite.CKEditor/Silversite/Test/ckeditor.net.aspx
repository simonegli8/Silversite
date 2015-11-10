<%@ Page Title="" Language="C#" MasterPageFile="~/silversite/master/admin.master" AutoEventWireup="true" CodeBehind="ckeditor.net.aspx.cs" Inherits="Silversite.CKEditor.silversite.test.CKEditorNet" %>
<%@ Register Namespace="CKEditor.NET" Assembly="Silversite.CKEditor.dll" TagPrefix="ck" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="page" runat="server">
	
	<h1>CKEditor Control</h1>

	<ck:CKEditorControl runat="server" ID="editor" >
	
	</ck:CKEditorControl>

</asp:Content>
