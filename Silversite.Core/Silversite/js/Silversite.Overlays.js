/// <register path="~/silversite/master/Standard.master" />
/// <reference path="~/silversite/js/jQuery/jquery-1.6.2-vsdoc.js"/>

Type.registerNamespace("Silversite.Overlays");

Silversite.Overlays.delayImage = new Image();
Silversite.Overlays.delayImage.src = Silversite.Paths.Client("~/silversite/images/wait.gif");
Silversite.Overlays.zIndex = 10000;
Silversite.Overlays.DivIndex = 1;
Silversite.Overlays.ConIndex = 1;
Silversite.Overlays.WaitDiv = "<div style='margin-top:auto;margin-bottom:auto;vertical-align:middle;text-align:center;width:100%;height:100%;'><img src='" + Silversite.Overlays.delayImage.src + "'></div>";

Silversite.Overlays.HtmlWait = function ($element) {
	var id = $element.attr('id');
	if (Silversite.Overlays.Reset($element, 'HtmlWait')) {
		$element = $("#" + id);
		$element.prop('ssoverlaysd', { modified: 'HtmlWait', oldHtml: $element.html() });
		$element.html("<img src='" + Silversite.Overlays.delayImage.src + "'/>");
	}
};

Silversite.Overlays.Add = function ($element, opacity, content, style, color) {
	var id = $element.attr('id');
	if (Silversite.Overlays.Reset($element, 'Add')) {
		$element = $("#" + id);
		$element.prop('ssoverlaysd', { modified: 'Add' });
		if (typeof (color) === 'undefined' || color === null) color = '#000000';
		if (typeof (style) === 'undefined' || style === null) style = '';
		if (typeof (content) === 'undefined' || content === null) content = '';
		if (typeof (opacity) === 'undefined' || opacity === null) opacity = 0.2;
		$element.wrap("<div style='position:relative'></div>");
		$element.before("<div style='position:absolute;top:0;bottom:0;left:0;right:0;width:100%;'><div style='position:absolute;top:0;bottom:0;left:0;right:0;width:100%;-ms-filter:\"progid:DXImageTransform.Microsoft.Alpha(Opacity=" + opacity * 100 + ")\";filter:alpha(opacity=" + opacity * 100 + ");-moz-opacity:" + opacity + ";-khtml-opacity:" + opacity + ";opacity:" + opacity + ";background-color:" + color + ";vertical-align:middle;text-align:center;z-index:10000;" + style + "'  />" + content + "</div>");
	}
};

Silversite.Overlays.GetBackground = function ($element) {
	//var bk = $element.curStyles('background-color', 'background-image');
	var element = $element.get(0);
	var bk = { backgroundImage: $.curCSS(element, 'background-image', true), backgroundColor: $.curCSS(element, 'background-color', true) };
	//var bk = { backgroundImage: Silversite.Overlays.GetStyle($element, 'background-image'), backgroundColor: Silversite.Overlays.GetStyle($element, 'background-color') };
	if (bk !== null && bk.backgroundImage !== null && bk.backgroundImage !== '' && bk.backgroundImage !== 'none') return null;
	else if (bk !== null && bk.backgroundColor !== null && bk.backgroundColor !== '' && bk.backgroundColor !== 'transparent' && bk.backgroundColor !== 'rgba(0, 0, 0, 0)') return bk.backgroundColor;
	else {
		var parent = $element.parent();
		if (parent.length > 0) return Silversite.Overlays.GetBackground(parent);
		return 'white';
	}
};

Silversite.Overlays.InitPopup = function (id) {
	var $element = $("#" + id);
	$element.addClass('ssoverlays_popup');
	$element.hover(function (event) {
		event.stopPropagation();
		Silversite.Overlays.Popup($("#" + id));
	}, function (event) {
		event.stopPropagation();
		Silversite.Overlays.Reset($("#" + id));
	});
}

Silversite.Overlays.DisablePopup = function() { $('.ssoverlays_popup').unbind('mouseenter mouseleave'); }
Silversite.Overlays.EnablePopup = function () {
	$('.ssoverlays_popup').each(function () {
		var $e = $(this);
		var id = $e.attr('id');
		$e.hover(function (event) {
			event.stopPropagation();
			Silversite.Overlays.Popup($("#" + id));
		}, function (event) {
			event.stopPropagation();
			Silversite.Overlays.Reset($("#" + id));
		});
	});
}

Silversite.Overlays.Popup = function ($element, mouse, opacity, color) {
	var id = $element.attr('id');
	if ($element.length == 1 && Silversite.Overlays.Reset($element, 'Popup')) {
		$element = $("#" + id);

		if (typeof (opacity) == 'undefined' || opacity === null) opacity = 0.7;
		if (typeof (color) == 'undefined' || color === null) color = '#000000';
		var bck = Silversite.Overlays.GetBackground($element);

		if (bck !== null && bck != '') {

			//console.log(Silversite.Overlays.ConIndex++ + ': Popup ' + $element.attr('id'));

			var clone = $element.clone(true);
			clone.attr('id', null);

			//clone.css('visibility', 'hidden');
			$element.prop('ssoverlaysd', { modified: 'Popup' });

			var width = $element.width();

			$element.wrap("<div style='position:relative;margin:0;padding:0;border-width:0'></div>");
			$element.css('background-color', bck);
			$element.css('z-index', 9999);
			$element.css('top', 0);
			$element.css('left', 0);
			$element.css('width', width);
			$element.css('position', 'absolute');
			// $element.unbind('mouseenter', 'mouseleave');
			$element.parent().after(clone);
			clone.css('z-index', 9998);
			if ($('.ssoverlays_dim').length == 0) {
				$element.before("<div class='ssoverlays_dim' style='position:fixed;top:0;bottom:0;left:0;right:0;z-index:9997;background-color:" + color + ";width:100%;-ms-filter:\"progid:DXImageTransform.Microsoft.Alpha(Opacity=" + (opacity * 100) + ")\";filter:alpha(opacity=" + (opacity * 100) + ");-moz-opacity:" + opacity + ";-khtml-opacity:" + opacity + ";opacity:" + opacity + ";' />");
			}
		}
	}
};

Silversite.Overlays.Reset = function ($element, op) {
	if (op === 'Popup') {
		var parent = $element.parents().each(function (i) {
			Silversite.Overlays.ResetElement($(this), 'PopupParents');
		});
	} else {
		var parent = $element.parents().each(function (i) {
			Silversite.Overlays.ResetElement($(this), null);
		});
	}
	return Silversite.Overlays.ResetElement($element, op);
}

Silversite.Overlays.ResetElement = function ($element, op) {
	var props = $element.prop('ssoverlaysd');
	if (typeof (props) !== 'undefined' && props !== null) {
		if (props.modified !== op) {
			switch (props.modified) {
				case 'HtmlWait': this.html(props.oldHtml); break;
				case 'Add':
					$element.prev().remove();
					$element.unwrap();
					break;
				case 'Popup':
					var id = $element.attr('id');

					//console.log(Silversite.Overlays.ConIndex++ + ': Reset ' + id);

					var clone = $element.parent().next();
					//clone.css('visibility', 'visible');
					if (op !== 'PopupParents') $('.ssoverlays_dim').remove();
					$element.parent().remove();
					clone.prop('id', id);
					return true;
			}
			props = null;
			$element.prop('ssoverlaysd', null);
			return true;
		}
		return false;
	}
	return true;
};

Silversite.Overlays.Wait = function ($element, opacity, color, style, color) {
	Silversite.Overlays.Add($element, opacity, "<img src='" + Silversite.Overlays.delayImage.src + "'  style='position:absolute;left:50%;top:50%;margin-top:-8px;margin-left:-8px;z-index:10001;'/>", style, color);
};


if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();