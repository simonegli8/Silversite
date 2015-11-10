<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="htmlparseall.aspx.cs" Inherits="Silversite.TestPages.silversite.test.parseall" MasterPageFile="~/Silversite/Test/master/test.master" %>

<asp:Content runat="server" ContentPlaceHolderID="content">

	<ss:Head runat="server">
		<style type="text/css">
			div.tree, div.errors { background: white; font-size: 8 }
			div.errors td {  font: 8 Arial, Sans-Serif; }
			div.code { border: 1 solid black; background: white;  font-size: 8 }
			table.code { text-align: left; width: 10px;}
			div.code td { vertical-align: top; }
			td.Original, td.Writer { width: 500px }
			td.LineNumbers { width: 20px; }
			div.LineNumbers { width: 20px; padding-right: 10px; border-right: 1 solid gray; margin-right: 10px; color: teal; }
			div.Original { float: left; width: 500px; border-right: 1 solid gray; padding: 0 10 0 10; overflow: auto; }
			div.Writer { float: left; width: 500px; padding: 0 10 0 10; overflow: auto; }
		</style>
	</ss:Head>

	<h2>Test parser with all pages</h2>

	<div class="tree">
		<asp:TreeView ID="tree" runat="server" />
	</div>

</asp:Content>