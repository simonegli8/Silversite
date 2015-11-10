/// <reference path="~/silversite/master/Standard.master" />
/// <reference path="~/silversite/js/jQuery/jquery-1.6.2-vsdoc.js"/>

Type.registerNamespace("Silversite");

Silversite.Zoom = new function () {
	this.Speed = 200;
	this.Factor = 3;
}

Silversite.Zoom$Init = function() {
	$('.Silversite.Zoom').each(function(i) {
		var element = this;
		var $element = $(element);

		var w = $element.width();
		var h = $element.height();
		var zindex = $element.css('z-index');

		if (!element.zoomdata) element.zoomdata = new Object();
		var d = element.zoomdata;
		d.width = w;
		d.height = h;
		d.zindex = zindex;

		$element.wrap('<div style=\"position:relative; width:' + w + 'px; height:' + h + 'px;\"></div>');
		//$element.css({ position: 'static', 'z-index': d.zindex });
		$element.css({ position: 'absolute', top: 0, left: 0 });
		element.onmouseover = function() { Silversite.Zoom$Zoom(element); };
		element.onmouseout = function() { Silversite.Zoom$Shrink(element); };
	});
}

Silversite.Zoom$Zoom = function(element) {
	element.onmouseover = null;
	var $element = $(element);
	var d = element.zoomdata;

	var speed = d.speed;
	if (!speed) speed = Silversite.Zoom.Speed;
	var factor = d.factor;
	if (!factor) factor = Silversite.Zoom.Factor;

	var w = d.width * factor;
	var h = d.height * factor;
	var x = (d.width - w) / 2;
	var y = (d.height - h) / 2;
	$element.css({ 'z-index': 10000 });
	$element.animate({ left: x, top: y, width: w, height: h }, speed);
}

Silversite.Zoom$Shrink = function(element) {
	var $element = $(element);
	var d = element.zoomdata;

	var speed = d.speed;
	if (!speed) speed = Silversite.Zoom.Speed;

	$element.animate({ top: 0, left: 0, width: d.width, height: d.height }, speed, null,
		function() {
			element.onmouseover = function() { Silversite.Zoom$Zoom(element); };
			$element.css({ 'z-index': d.zindex });
		}
	);
}

Silversite.Zoom$Set = function(element, speed, factor) {
	if (!element.zoomdata) element.zoomdata = new Object();
	element.zoomdata.factor = factor;
	element.zoomdata.speed = speed;
}

$(document).ready(function() {
	Silversite.Zoom$Init();
});

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();