<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
		<asp:ScriptManager ID="ScriptManager" ScriptMode="Debug" runat="server">
			<Scripts>
				<asp:ScriptReference Path="~/Silversite/JavaScript/jQuery/jquery-1.8.3.intellisense.js" />
				<asp:ScriptReference Path="~/Silversite/Extensions/Silversite.CKEditor/ckeditor/ckeditor.js" />
				<asp:ScriptReference Path="~/Silversite/Extensions/Silversite.CKEditor/ckeditor/adapters/jquery.js" />
				<asp:ScriptReference Path="~/Silversite/Extensions/Silversite.CKEditor/js/Silversite.CKEditor.js" />
			</Scripts>
		</asp:ScriptManager>
    </form>
</body>
</html>
