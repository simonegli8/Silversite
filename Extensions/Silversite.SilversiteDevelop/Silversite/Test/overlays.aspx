<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="overlays.aspx.cs" Inherits="Silversite.silversite.test.overlays" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

	<ss:jQuery runat="server" />
	<ss:Script runat="server" Src="../js/jquery/jquery.curstyles.js" />
	<ss:Script runat="server" Src="../js/Silversite.Overlays.js" />

	<div style="background-color:Lime" />
	
		<h1>hello</h1>
		
		how are you?

		<div id="popup" style="border-width:1px; -moz-border-radius: 8px; border-radius: 8px; border-style:dashed;border-color:Gray;"><p>Coolo</p><p>ver cool</p>
			<asp:Button runat="server" OnClientClick="Silversite.Overlays.Add($('#popup')); return false;"  Text="Add" />
			<asp:Button runat="server" OnClientClick="Silversite.Overlays.Popup($('#popup')); return false;" Text="Popup" />
			<asp:Button runat="server" OnClientClick="Silversite.Overlays.Wait($('#popup')); return false;" Text="Wait" />
			<asp:Button runat="server" OnClientClick="Silversite.Overlays.Reset($('#popup')); return false;" Text="Reset" />
		</div>

		<asp:Button runat="server" OnClientClick="Silversite.Overlays.Add($('#popup')); return false;"  Text="Add" />
		<asp:Button runat="server" OnClientClick="Silversite.Overlays.Popup($('#popup')); return false;" Text="Popup" />
		<asp:Button runat="server" OnClientClick="Silversite.Overlays.Wait($('#popup')); return false;" Text="Wait" />
		<asp:Button runat="server" OnClientClick="Silversite.Overlays.Reset($('#popup')); return false;" Text="Reset" />

</asp:Content>
