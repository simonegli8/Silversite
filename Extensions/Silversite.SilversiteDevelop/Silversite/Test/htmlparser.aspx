<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="htmlparser.aspx.cs" Inherits="Silversite.Test.Parser" MasterPageFile="~/Silversite/Test/master/test.master" %>

<asp:Content runat="server" ContentPlaceHolderID="content">

	<ss:

	<ss:Head runat="server">
		<style type="text/css">
			div.tree, div.errors { background: white; font-size: 8 }
			div.errors td {  font: 8 Arial, Sans-Serif; }
			div.code { border: 1 solid black; background: white;  font-size: 8 }
			table.code { text-align: left; width: 10px;}
			div.code td { vertical-align: top; }
			td.Original, td.Writer { width: 500px; border: 1 }
			td.LineNumbers { width: 20px; border: 1}
			div.LineNumbers { width: 20px; color: teal; }
			div.Original { width: 1000px; border-right: 1 solid gray; ; overflow: auto; }
			div.Writer { width: 500px; padding: 0 10 0 10; overflow: auto; }
			.popup { background-color: #ddd; position:fixed; top:10%; left:5%; width:90%; height: 80%; z-index: 9999; border: 1 solid #888; border-radius:7px; display: none; }
			.popup .text { position:absolute; width:92%; height: 92%; top:4%; left:4%; text-align:left; margin: 7px; overflow:auto; }
			.popup .menu { position:absolute; right:7px; top:7px; z-index:10000; }
		</style>	
	</ss:Head> 

	<div style="position:relative;">

		<div class="popup">
			<div class="menu"><img alt="" src="../images/del.gif" onclick="$('.popup').hide();" /></div>
			<div class="text"></div>
		</div>

		<script language="javascript" type="text/javascript">
			PopupText = function(text) {
				$('.popup .text').html(text);
				$('.popup').show();
			}
		</script>

		<h2>Parse Html File</h2>
		
		<p>
			Server File: <asp:TextBox runat="server" ID="file" Width="300"/> <asp:Button Text="Parse..." runat="server" OnClick="Parse" />
		</p>

		<p>
			<asp:Literal ID="message" runat="server" />	&nbsp; &nbsp; &nbsp; <asp:Button ID="windiff" runat="server" Text="WinDiff..." OnClick="WinDiff" />
		</p>

		<p>
			Element Tree:
		</p>

		<div class="tree">
			<asp:TreeView ID="tree" runat="server" />
		</div>
		
		<p>Errors:</p>

		<div class="errors">
			<asp:GridView ID="errors" runat="server" AutoGenerateColumns="true"  />
		</div>

		<p>Html</p>

		<div class="code">
			<table class="code">
				<tr>
					<td class="LineNumbers">
						<div class="LineNumbers">
							<pre>
<asp:Literal runat="server" ID="LineNumbers" />
							</pre>
						</div>
					</td>
					<td class="Original">
						<div class="Original">
							<pre>
<asp:Literal runat="server" ID="Original" />
							</pre>
						</div>
					</td>
					<!--
					<td class="Writer">
						<div class="Writer">
							<pre>
<asp:Literal runat="server" ID="Writer" />
							</pre>
						</div>
					</td>
					-->
				</tr>
			</table>
		</div>

	</div>

</asp:Content>