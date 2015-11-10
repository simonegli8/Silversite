<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" AutoEventWireup="true" CodeBehind="exception.aspx.cs" Inherits="Silversite.Admin.silversite.exception" %>
<%@ Register TagPrefix="ss" TagName="ExceptionInfo" Src="~/Silversite/ui/exception.ascx" %>

<asp:Content ContentPlaceHolderID="page" runat="server">
	
	<h2>Exception</h2>

	<ss:ExceptionInfo runat="server" ID="info" />

</asp:Content>