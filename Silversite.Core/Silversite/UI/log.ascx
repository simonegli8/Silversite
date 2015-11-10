<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="log.ascx.cs" Inherits="Silversite.Web.UI.LogControlBase" ClassName="Silversite.Web.UI.LogControl" %>
<%@ Assembly Name="AjaxControlToolkit" %>
<%@ Assembly Name="EntityFramework" %>
<%@ Register TagPrefix="ajax" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Homesell" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web" Assembly="Homesell" %>

<asp:Panel runat="server" ID="LogContainer" CssClass="LogContainer">

	<%--<ajax:ToolkitScriptManager runat="server" EnablePageMethods="false" EnableScriptGlobalization="true" EnableScriptLocalization="true" AjaxFrameworkMode="Enabled" EnableCdn="true" />--%>

	<ss:Head runat="server">
		<style type="text/css">
			.mGrid {
				width:100%;
				font-size:14px;
				background-color: #fff;   
				border: 1px solid #525252;   
				border-collapse:collapse; 
			}
			.datediv {
				color: white;
				border: 0px solid blue;
				background-color:#474747;
				padding: 0px 8px 0px 8px;
				margin: 0px 8px 0px 8px;
			}
			.headtable td {
				padding: 0px 8px 0px 8px;
			}
		</style>
	</ss:Head>

	<table style="width:600px;" border="0" class=".headtable">
		<tr>
			<td class="tabcell">Category:</td>
			<td class="tabcell"><ss:LogCategory ID="category" runat="server" Width="200px" AutoPostBack="true" /></td>
			<td>
				Time: <asp:TextBox ID="time" runat="server" Width="100" />
				<ajax:CalendarExtender runat="server" TargetControlID="time" Format="yyyy/MM/dd" />
			</td>
			<td>Page Size:
				<asp:DropDownList ID="pagesize" runat="server" AutoPostBack="true">
					<asp:ListItem>30</asp:ListItem>
					<asp:ListItem>40</asp:ListItem>
					<asp:ListItem>50</asp:ListItem>
					<asp:ListItem>100</asp:ListItem>
					<asp:ListItem>500</asp:ListItem>
					<asp:ListItem>1000</asp:ListItem>
				</asp:DropDownList>
			</td>
			<td><asp:Button runat="server" OnClick="ClearClick" Text="Delete" /></td>
		</tr>
	</table>

	<hr />

	<ss:LogSource ID="source" runat="server" />
	
	<asp:GridView ID="grid" CssClass="mGrid" runat="server" AutoGenerateColumns="false" ShowHeader="false" GridLines="None" AllowPaging="true" PageSize="100" Width="100%" DataSourceID="source" DataKeyNames="Key" PagerSettings-Position="TopAndBottom" >
		<Columns>
			<asp:TemplateField ShowHeader="false" >
				<ItemTemplate>
					<div class="datediv" style="float:left;margin-right:14px;">
						<span>
							<%# GetDateString((DateTime)Eval("Date")) %>
						</span>
					</div>
					<%# Eval("Text") %>
				</ItemTemplate>
			</asp:TemplateField>
		</Columns>
	</asp:GridView>

</asp:Panel>
