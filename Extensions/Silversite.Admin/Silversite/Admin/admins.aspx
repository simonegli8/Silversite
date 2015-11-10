<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/admin.master" AutoEventWireup="true" CodeBehind="admins.aspx.cs" Inherits="Silversite.Admin.Admins" %>
<%@ Register TagPrefix="sc" TagName="Admins" Src="~/Silversite/ui/admins.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="page" runat="server">

	<sc:Admins runat="server" ID="admins" />

</asp:Content>
