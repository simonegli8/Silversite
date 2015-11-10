<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" AutoEventWireup="true" CodeBehind="db.aspx.cs" Inherits="Silversite.silversite.admin.DbVersion" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Admin" %>

<asp:Content ID="Content2" ContentPlaceHolderID="page" runat="server">

	<ss:Head runat="server">
		<style type="text/css">
			.versionsrow { padding: 0px 4px 0px 4px; }
		</style>
	</ss:Head>

	<h1>Database Contexts</h1>

	<ss:DbVersionsDataSource runat="server" ID="source" />
	
	<asp:GridView ID="grid" runat="server" OnRowCommand="RowCommand" DataSourceID="source" AutoGenerateColumns="true" DataKeyNames="Assembly" RowStyle-CssClass="versionsrow" >
		<%--
		<Columns>
			<asp:BoundField ReadOnly="true" DataField="ContextClass" HeaderText="ContextClass" />
			<asp:BoundField ReadOnly="true" DataField="Version" HeaderText="Version" />
		</Columns> --%>
	</asp:GridView>

</asp:Content>
