///<register path="~/Silversite/Master/Standard.master" />

Type.registerNamespace("Silversite");

function Silversite.ImageButton$Init(image, hoverImageUrl, activeImageUrl) {
	var b = image.ImageButton = new function(img) {
		var nimg = new Image; nimg.src = img.src;
		this.NormalImage = nimg;
		this.HoverImage = null;
		this.ActiveImage = null;
		this.Mode = "normal";
	}
	
	if (hoverImageUrl != null) {
		b.HoverImage = new Image; b.HoverImage.src = hoverImageUrl;
		image.onmouseover = function () { Silversite.ImageButton$Set(image, 'hover'); }
		image.onmouseout = function () { Silversite.ImageButton$Set(image, 'normal'); };
	}
	if (activeImageUrl != null) {
		b.ActiveImage = new Image; b.ActiveImage.src = activeImageUrl;
		image.onmousedown = function() { Silversite.ImageButton$Set(image, 'active'); }
		image.onmouseup = function () { Silversite.ImageButton$Set(image, 'normal'); }
	}
}

function Silversite.ImageButton$Set(image, mode) {
	var b = image.ImageButton;

	if (b.Mode !== mode) {

		switch (mode) {
		case "normal": image.src = b.NormalImage.src; b.Mode = mode; break;
		case "hover":
			if (b.HoverImage !== null) {
				image.src = b.HoverImage.src;
				b.Mode = mode;
			}
			break;
		case "active":
			if (b.ActiveImage !== null) {
				image.src = b.ActiveImage.src;
				b.Mode = mode;
			}
			break;
		}

	}
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();