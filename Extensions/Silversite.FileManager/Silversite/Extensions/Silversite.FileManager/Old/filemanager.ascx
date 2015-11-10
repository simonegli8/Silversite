<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="filemanager.ascx.cs" Inherits="Silversite.Web.UI.Controls.Extensions.FileManager" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>

<ss:Css runat="server" Href="~/Silversite/Extensions/Silversite.JSFileManager/styles/reset.css" />
<ss:Css runat="server" Href="~/Silversite/Extensions/Silversite.JSFileManager/scripts/jquery.filetree/jqueryFileTree.css" />
<ss:Css runat="server" Href="~/Silversite/Extensions/Silversite.JSFileManager/scripts/jquery.contextmenu/jquery.contextMenu.css" />
<ss:Css runat="server" Href="~/Silversite/Extensions/Silversite.JSFileManager/styles/filemanager.css" />
<!--[if IE]>
	<ss:Css runat="server" Href="~/Silversite/Extensions/Silversite.JSFileManager/styles/ie.css" />
<![endif]-->

<div>
	<form id="uploader" method="post">
		<button id="home" name="home" type="button" value="Home">&nbsp;</button>
		<h1></h1>
		<div id="uploadresponse"></div>
		<input id="mode" name="mode" type="hidden" value="add" /> 
		<input id="currentpath" name="currentpath" type="hidden" /> 
		<input	id="newfile" name="newfile" type="file" />
		<button id="upload" name="upload" type="submit" value="Upload"></button>
		<button id="newfolder" name="newfolder" type="button" value="New Folder"></button>
		<button id="grid" class="ON" type="button">&nbsp;</button>
		<button id="list" type="button">&nbsp;</button>
	</form>
	<div id="splitter">
		<div id="filetree"></div>
		<div id="fileinfo">
			<h1></h1>
		</div>
	</div>
	
	<ul id="itemOptions" class="contextMenu">
		<li class="select"><a href="#select"></a></li>
		<li class="download"><a href="#download"></a></li>
		<li class="rename"><a href="#rename"></a></li>
		<li class="delete separator"><a href="#delete"></a></li>
	</ul>

	<ss:jQuery runat="server" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.form-2.63.js" />
	<ss:Sript Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.splitter/jquery.splitter-1.5.1.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.filetree/jqueryFileTree.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.contextmenu/jquery.contextMenu-1.01.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.impromptu-3.1.min.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/jquery.tablesorter-2.0.5b.min.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/filemanager.config.js" />
	<ss:Script Path="~/silveriste/extensions/Silversite.FileManager/scripts/filemanager.js" />
			
</div>
