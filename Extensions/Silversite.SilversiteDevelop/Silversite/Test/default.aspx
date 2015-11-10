<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/Master/test.master" CodeBehind="default.aspx.cs" Inherits="Silversite.silversite.test.Default"  %>

<asp:Content ID="Content2" ContentPlaceHolderID="adds" runat="server">
	
	<ss:EditableContent runat="server">
		<ul>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin/log.aspx" Text="Log" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin" Text="Admin" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin/log.aspx" Text="Log" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/logtest.aspx" Text="Log Test" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/test.aspx" Text="Virtual Page Test Page" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin/login.aspx" Text="Virtual Login Page" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/iconbar.aspx" Text="Iconbar Tests" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin/storesversion.aspx" Text="Stores Versions" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/editablecontent.aspx?silversite.pagemode=edit" Text="EditableContent" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/ckeditor.net.aspx" Text="CKEditor.NET" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/createadmin.aspx" Text="Create Admin" /></li>			
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Admin/login.aspx" Text="Login" /></li>
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/autostartmodule.aspx" Text="Autostart" /></li>		
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/overlays.aspx" Text="Overlays" /></li>		
			<li><asp:HyperLink runat="server" NavigateUrl="~/Silversite/Test/filemanager.aspx" Text="Filemanager" /></li> 
			<li><asp:HyperLink  runat="server" NavigateUrl="~/Silversite/Test/parser.test.aspx" Text="Html Parser" /></li>
			<li><asp:HyperLink  runat="server" NavigateUrl="~/Silversite/Test/splitter.aspx" Text="Splitter" /></li> 
		</ul>
     
 	</ss:EditableContent>

</asp:Content>
