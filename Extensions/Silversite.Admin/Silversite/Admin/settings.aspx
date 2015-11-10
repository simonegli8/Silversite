<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Silversite.Admin.silversite.settings" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Services" Assembly="Silversite.Core" %>

<asp:Content ID="Content1" ContentPlaceHolderID="page" runat="server">

    <ss:FileManager ID="manager" runat="server" Root="Settings" />
    
</asp:Content>
