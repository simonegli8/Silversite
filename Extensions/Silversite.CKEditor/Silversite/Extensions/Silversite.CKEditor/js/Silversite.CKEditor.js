/// <reference path="jquery-1.8.3.intellisense.js" />
/// <reference path="../ckeditor/ckeditor.js" />

Type.registerNamespace("Silversite.CKEditor");

Silversite.CKEditor.WaitImg = new Image();
Silversite.CKEditor.WaitImg.src = Silversite.Paths.Client("~/Silversite/Images/wait.gif");
Silversite.CKEditor.ErrImg = new Image();
Silversite.CKEditor.ErrImg.src = Silversite.Paths.Client("~/Silversite/Images/error.png");
/*
CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here. For example:
    // config.language = 'fr';
    // config.uiColor = '#AADC6E';
    config.extraPlugins += (config.extraPlugins ? ',syntaxhighlight' : 'syntaxhighlight');
    config.toolbar_Full.push(['Code']);
};*/

Silversite.CKEditor.Config = function (editor) {
    var writer = editor.dataProcessor.writer;
	writer.lineBreakChars = '\r\n';
	writer.selfClosingEnd = ' />';
	for (var e in ['p', 'div', 'html', 'head', 'body', 'form', 'td', 'tr', 'table', 'script']) {
		writer.setRules(e, { indent: true, breakBeforeOpen: true, breakAfterOpen: true, breakBeforeClose: true, breakAfterClose: true });
	}
}

Silversite.CKEditor.Open = function (containerid, menuid, contentid, commandScript, textclass, menu) {

	var $container = $("#" + containerid);
	Silversite.Overlays.Wait($container);

	var $menu = $("#" + menuid);
	var $content = $("#" + contentid);

	var text = null;
	var editor = null;

	commandScript("Load", "",
		function (arg, context) { // load callback
			Silversite.Overlays.Reset($container);
			text = arg;
			if (editor !== null) editor.setData(text);
		}, function (arg, context) { // error callback
			Silversite.Overlays.Reset($container);
			text = $(Silversite.CKEditor.ErrImg).html();
			if (editor !== null) edior.setData(text);
		}
	);

	if (typeof (Silversite.Overlays) !== 'undefined') Silversite.Overlays.DisablePopup();

	var opts = {
		fullPage: true,
		extraPlugins: 'syntaxhighlight',
		filebrowserBrowseUrl: '../filemanager/default.aspx'
	};

	if (menu == 'Full') {
		opts.toolbar =
			[
				['Save', '-', 'Source', 'Code'],
				['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Print', 'SpellChecker', 'Scayt'],
				['Undo', 'Redo', '-', 'Find', 'Replace', '-', 'RemoveFormat', '-', 'BidiLtr', 'BidiRtl'],
			'/',
				['Bold', 'Italic', 'Underline', 'Strike', '-', 'Subscript', 'Superscript'],
				['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', 'Blockquote', 'CreateDiv'],
				['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
				['Link', 'Unlink', 'Anchor'],
				['Image', 'Flash', 'Table', 'HorizontalRule', 'SpecialChar', 'PageBreak', 'CreatePlaceholder'],
			'/',
				['Styles', 'Format', 'Font', 'FontSize'],
				['TextColor', 'BGColor'],
				['Maximize', 'ShowBlocks']
			];
	} else if (menu == 'Basic') {
		opts.toolbar = [['Save', 'Format', '-', 'Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Bold', 'Italic', 'NumberedList', 'BulletedList', '-', 'Link', 'Table', 'Image', 'Flash']];
	} else if (menu == 'BasicNoFiles') {
		opts.toolbar = [['Save', 'Format', '-', 'Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Bold', 'Italic', 'NumberedList', 'BulletedList', '-', 'Link', 'Table']];
	} else {
		opts.toolbar == menu;
	}

	$menu.hide();

	$content.ckeditor(
		function () {
			Silversite.CKEditor.Config(this);
			this.SilversiteEditor = {
				Command: commandScript,
				Menu: $menu,
				Content: $content,
				Container: $container
			};
			editor = this;

			if (text !== null) editor.setData(text);
		}, opts
	);

};

CKEDITOR.plugins.registered['save'] = {
	init: function (editor) {
		var command = editor.addCommand('save', {
			modes: { wysiwyg: 1, source: 1 },
			canUndo: false,
			exec: function (editor) {
				var d = editor.SilversiteEditor;
				var text = editor.getData();
				CKEDITOR.remove(editor);
				editor.destroy();

				Silversite.Overlays.Wait(d.Container);

				editor.SilversiteEditor.Command("Save", text, function (args, context) {
					Silversite.Overlays.Reset(d.Container);
					d.Container.html(args);
					if (typeof (Silversite.Overlays) !== 'undefined') Silversite.Overlays.EnablePopup();
				}, function (args, context) { // save error
					Silversite.Overlays.Reset(d.Container);
					d.Content.html("<img src='" + Silversite.CKEditor.ErrImg.src + "'> Error: " + args);
					if (typeof (Silversite.Overlays) !== 'undefined') Silversite.Overlays.EnablePopup();
				});
				d.Menu.show();
			}
		});
		editor.ui.addButton('Save', { label: 'Save', command: 'save' });
	}
}

if (Sys) Sys.Application.notifyScriptLoaded();