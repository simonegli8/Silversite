<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="resolveurl.aspx.cs" Inherits="Silversite.SilversiteDevelop.silversite.test.resolveurl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">
    
    <h1>Resolve Url Test</h1>

    <p><asp:TextBox ID="url" runat="server" Text="~/Silversite/Test/test.aspx" /></p>

    <p>Resolve:<asp:Literal runat="server" ID="resolve" /></p>
    
   <p>ClientResolve:<asp:Literal runat="server" ID="clientresolve" /></p>
    
    <p><asp:Button runat="server" Text="Refresh"/></p>
    
    <p><asp:HyperLink runat="server" ID="selflink" NavigateUrl="~/Silversite/Test/resolveurl.aspx" Text="Link to self..." /></p>

</asp:Content>
