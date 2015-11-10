<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Silversite.js.aspx.cs" Inherits="Silversite.Web.UI.Silversite_js" %>

Type.registerNamespace("Silversite");
Type.registerNamespace("Silversite.Web.UI");
Type.registerNamespace("Silversite.Paths");

// common functions

Silversite.Paths.AppRoot = "<%= this.ResolveUrl("~") %>";

Silversite.Paths.Client = function(serverPath) {

	if (serverPath === "~") serverPath = "~/";

	var dirs = document.location.pathname.split('/').length;
	var approotdirs =  Silversite.Paths.AppRoot.split('/').length;

	var path="";
	for( var i=0; i < dirs-approotdirs; i++) path += "../";

	var ss = serverPath.split('/');
	for (var i=1; i < ss.length-1; i++) path += (ss[i] + '/');

	path += ss[ss.length-1];

	return path;
}


if (typeof (jQuery) !== 'undefined') {
	// include jQuery first.

	(function ($) {
		var options = {
			silversite: function () { //you don't have to call it 'root', of course :)
				//identify the function from within itself with arguments.callee
				var fn = arguments.callee;

				//'this' at this level is the jquery object list matching your given selector
				//we equate fn.prototype to this 
				//thus when we call a new instance of root, we are returned 'this'
				fn.prototype = this;

				/*fn.value= function(){
					//Note: "new this" will work in the below line of code as well,
					//because in the current context, 'this' is fn;
					//I use fn to make the code more intuitive to understand;
					var context= new fn; 

					console.log(context, fn.prototype); //test
					return context.html(); //test
				}*/

				return this;
			}
		}

		/*
		//you can obviously append additional nested properties in this manner as well
		options.root.position= function(){
		var context= new this; //in this context, fn is undefined, so we leverage 'this'

		console.log(context, this.prototype); //test
		return context.offset(); //test
		}*/

		//don't forget to extend in the end :)
		$.fn.extend(options);

	})(jQuery);
}

Silversite.Copy = function (a, b) {
	for (var element in a) {
		b[element] = a[element];
	}
}

Silversite.Clone = function (obj) {
	var copy = new Object();
	Copy(obj, copy);
	return copy;
}

// hack for safari
Sys.Browser.WebKit = {}; //Safari 3 is considered WebKit
if (navigator.userAgent.indexOf('WebKit/') > -1) {
	Sys.Browser.agent = Sys.Browser.WebKit;
	Sys.Browser.version = parseFloat(navigator.userAgent.match(/WebKit\/(\d+(\.\d+)?)/)[1]);
	Sys.Browser.name = 'WebKit';
}

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();