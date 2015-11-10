<%@ Assembly Name="Silversite.Admin" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" Theme="default" AutoEventWireup="true" CodeBehind="~/Silversite/Admin/log.aspx.cs" Inherits="Silversite.silversite.admin.Log" ClassName="Silversite.Web.UI.Log" %>
<%@ Register TagPrefix="ss" TagName="LogControl" Src="~/Silversite/ui/log.ascx" %>

<asp:Content ContentPlaceHolderID="page" runat="server">

	<ss:LogControl runat="server" ID="log" />

</asp:Content>
