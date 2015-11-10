<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="editablecontent.aspx.cs" Inherits="Silversite.silversite.test.Editor" ValidateRequest="true"%>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>
<%@ Register src="~/Silversite/ui/login.ascx" tagname="Login" tagprefix="ss" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

	<h2>Editable Content</h2            >

	<p>
		<asp:HyperLink runat="server" NavigateUrl="editablecontent.aspx?silversite.pagemode=edit" Text="Edit Mode..." /> &nbsp; &nbsp;
		<asp:HyperLink runat="server" NavigateUrl="editablecontent.aspx" Text="View Mode..." />
	</p            >


	<ss:EditableContent runat="server">
        <ss:DocumentInfo runat="server" Categories="*" ContentKey="9" Revision="1" Published="10.11.2012 00:25" Author="admin" />

		Hello David!<br />
		<br />
		How are you today?<br />
		<br />
		<a href="iconbar.aspx">This is a link.</a               ><br />
		<br />
		This is a Server Control:<br />

		<asp:Calendar ID="server" runat="server" />

		This is a EditableContent inside an EditableContent:<br />

		<ss:EditableContent runat="server">
            <ss:DocumentInfo runat="server" Categories="*" ContentKey="8" Revision="1" Published="10.11.2012 00:19" Author="admin" />
			Hello again...
		</ss:EditableContent               >

	</ss:EditableContent            >
	<hr />
	<h2>Login</h2            >

	<ss:Login runat="server" />

	<hr />

	<%--
	<hr />

	<%@ Register Namespace="CKEditor.NET" Assembly="Silversite.CKEditor" TagPrefix="ck" %>

	<h2>CKEditor NET Control</h2>

	<ck:CKEditorControl runat="server" ID="editor3" Width="600" Height="400" > Hallo 2 </ck:CKEditorControl>
	
	<h2>CKEditor</h2>
	<textarea ID="cktext" style="width:600px;height:400px" rows="20" cols="60">
		Hallo.
	</textarea>

	<script type="text/javascript" language="javascript">
		$(function() { $('#cktext').ckeditor(); });
	</script>
	--%>
</asp:Content            >
