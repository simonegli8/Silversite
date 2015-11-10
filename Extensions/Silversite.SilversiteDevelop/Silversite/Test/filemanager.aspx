<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="filemanager.aspx.cs" Inherits="Silversite.SilversiteDevelop.silversite.test.filemanager" %>

<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">

    <ss:FileManager ID="files" runat="server" />

</asp:Content>
