/// <reference path="~/silversite/master/Standard.master" />
/// <reference path="~/silversite/js/jQuery/jquery-1.6.2-vsdoc.js"/>

Type.registerNamespace("Silversite");


// Menu
Silversite.Menu = function () {
	this.timeout = 300;
	this.closetimer = null;
	this.openMenus = new Array();
	this.menuItemOpen = false;
}

Silversite.Menu.prototype = {
	
	SetupMenuEvents: function () {
		$(".SubMenuParent").mouseenter(function (eventObj) {
			var t = $(this);
			var tparent = t.offsetParent(".SubMenu").prev(".SubMenuParent");
			this.CloseMenus(tparent, t);
			this.AddMenuInArray(t);
			var cm = t.next(".SubMenu");
			if (t.hasClass("HorzMenu")) {
				if (cm.hasClass("MenuDirectionLeft")) {
					cm.css("left", t.position().left - cm.width());
				} else {
					cm.css("left", t.position().left + t.width());
				}
				if (cm.hasClass("MenuDirectionUp")) {
					cm.css("top", t.position().top + t.height() - cm.height());
				} else {
					cm.css("top", t.position().top);
				}
			}
			if (t.hasClass("VertMenu")) {
				if (cm.hasClass("MenuDirectionUp")) {
					cm.css("top", t.position().top - cm.height());
				}
				if (cm.hasClass("MenuDirectionLeft")) {
					cm.css("left", t.position().left + t.width() - cm.width());
				}
			}

			if (cm.hasClass("fadeMenu"))
				cm.fadeIn("def");
			else if (cm.hasClass("slideMenu"))
				if (cm.hasClass("MenuDirectionUp")) cm.slideUp("def");
				else cm.slideDown("def");
			else if (cm.hasClass("toggleMenu"))
				cm.toggle("def");
			else
				cm.css("display", "block");
			this.menuItemOpen = true;
			eventObj.stopPropagation();
		}).mouseleave(function (eventObj) {
			var s = $(eventObj.relatedTarget).offsetParent(".SubMenu").prev(".SubMenuParent");
			var tp = $(this).offsetParent(".SubMenu").prev(".SubMenuParent");
			if (s.length == 0 || s.get(0) != tp.get(0)) {
				this.SetMenuTimer();
			}
			else if (s.get(0) == tp.get(0)) {
				this.CancelTimer();
				closetimer = window.setTimeout(function () { this.CloseMenus(tp); }, timeout);
			}
			else {
				this.CloseMenus(s);
			}
		});
		$(".SubMenu").mouseenter(function (eventObj) {
			this.CloseMenus($(this).prev(".SubMenuParent").eq(0));
			eventObj.stopPropagation();

		}).mouseleave(function (eventObj) {
			this.SetMenuTimer();
		});
	},

	SetMenuTimer: function () {
		this.CancelTimer();
		closetimer = window.setTimeout("closeMenus()", timeout);
	},

	CloseMenus: function (inMenParent, inMen) {
		if (typeof inMenParent == "number")
			inMenParent = null;
		this.CancelTimer();
		for (var i = this.openMenus.length - 1; i >= 0; i--) {
			if (inMenParent == null || inMenParent == 0 || inMenParent.get(0) != this.openMenus[i].get(0)) {
				if (inMen != null && inMen.get(0) == this.openMenus[i].get(0))
					break;
				var cm = this.openMenus[i].next(".SubMenu");
				if (cm.hasClass("fadeMenu"))
					cm.fadeOut("def");
				else if (cm.hasClass("slideMenu"))
					if (cm.hasClass("MenuDirectionUp")) cm.slideDown("def");
					else cm.slideUp("def");
				else if (cm.hasClass("toggleMenu"))
					cm.toggle("def");
				else
					cm.css("display", "none");
				this.openMenus.pop();
			}
			else if (inMenParent.get(0) == this.openMenus[i].get(0))
				break;
		}
		if (openMenus.length == 0)
			this.menuItemOpen = false;
	},

	CancelTimer: function () {
		if (this.closetimer) {
			window.clearTimeout(this.closetimer);
			this.closetimer = null;
		}
	},

	AddMenuInArray: function (men) {
		for (var i = 0; i < this.openMenus.length - 1; i++)
			if (men.get(0) == this.openMenus[i].get(0))
				return;
		this.openMenus.push(men);
	}
};

Silversite.Menu.registerClass("Silversite.Menu");

$(document).ready((new Silversite.Menu()).SetupMenuEvents());

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();