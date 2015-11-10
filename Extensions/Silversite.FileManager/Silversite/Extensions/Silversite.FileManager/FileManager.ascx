<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileManager.ascx.cs" Inherits="Silversite.Providers.FileManager.UserControls.FileManager" %>
<%@ Register Src="~/Silversite/Extensions/Silversite.FileManager/Menu.ascx" TagPrefix="fm" TagName="Menu" %>
<%@ Register  Src="~/Silversite/Extensions/Silversite.FileManager/Hierarchy.ascx" TagPrefix="fm" TagName="Hierarchy" %>
<%@ Register Src="~/Silversite/Extensions/Silversite.FileManager/Content.ascx" TagPrefix="fm" TagName="Content" %>
 

<ss:Css runat="server" File="~/Silversite/Extensions/Silversite.FileManager/FileManager.css" />

<fm:Menu ID="menu" runat="server" />

<fm:Hierarchy ID="hierarchy" runat="server" />

<fm:Content ID="content" runat="server" />