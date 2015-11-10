<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/default.master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="Silversite.silversite.admin.Default"
 ClassName="Silversite.Web.UI.Default" %>

<asp:Content ID="Content2" ContentPlaceHolderID="page" runat="server">
	<h1>Start Page</h1>

	<ul>
	<li><asp:HyperLink runat="server" NavigateUrl="~/admin/common/log.aspx" Text="Log" /></li>
	<li><asp:HyperLink runat="server" NavigateUrl="~/admin/common/test/logtest.aspx" Text="Log Test" /></li>
	<li><asp:HyperLink runat="server" NavigateUrl="~/admin/common/test/virtualpagetest.aspx" Text="Virtual Page Test" /></li>
	<li><asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/admin/common/test.aspx" Text="Virtual Page Test Page" /></li>
	<li><asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="~/admin/common/test/virtualpagetest.aspx.virtual" Text="Virtual Page Test" /></li>
	</ul>	

</asp:Content>
