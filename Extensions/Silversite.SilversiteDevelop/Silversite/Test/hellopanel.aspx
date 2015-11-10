<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master"  %>

<%@ Assembly Name="Silversite.Test.ControlLib" %>
<%@ Register tagPrefix="sx" namespace="Silversite.Web.UI" assembly="Silversite.Test.ControlLib" %>
<%@ Assembly Name="AjaxControlToolkit"   %>
<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>


<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">
    
    <h1>Imported Control:</h1>
    
    <sx:HelloPanel ID="hello" runat="server" Border="1"/>
    
    <ajax:DropShadowExtender runat="server" TargetControlID="hello" />

</asp:Content>
