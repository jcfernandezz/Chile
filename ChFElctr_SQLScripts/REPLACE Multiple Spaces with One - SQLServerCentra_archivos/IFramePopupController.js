function IFramePopup_Open(sender, evt, url, width, popupGroup, postbackEventRaiser) {
	evt = evt || window.event;
	var openerElement = evt.target || evt.srcElement;
	
	// Cross-browser mouse position detection
	var posx = 0, posy = 0;
	
	if (evt.pageX || evt.pageY) {
		posx = evt.pageX;
		posy = evt.pageY;
	} else if (evt.clientX || evt.clientY) {
		posx = evt.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
		posy = evt.clientY + document.body.scrollTop + document.documentElement.scrollTop;
	}	
	
	// Instantiate controller
	(new IFramePopupController(posx, posy, url, width, popupGroup, postbackEventRaiser, openerElement)).Show();
}

IFramePopupController = function(XPos, YPos, Url, Width, PopupGroup, PostbackEventRaiser, OpenerElement) {
	this.m_XPos = XPos;
	this.m_YPos = YPos;	
	this.m_Url = Url;
	this.m_Width = Width;
	this.m_PopupGroup = PopupGroup;
	this.m_PostbackEventRaiser = PostbackEventRaiser;
	this.m_OpenerElement = OpenerElement;
}

IFramePopupController._popupGroups = new Array();

IFramePopupController.prototype = {
	m_XPos : null,
	m_YPos : null,
	m_Url : null,
	m_Width : null,
	m_PopupGroup : null,
	m_PostbackEventRaiser : null,
	m_ContainerElement : null,
	m_IFrameElement : null,
	m_IsClosed : false,
	m_PostbackArgumentMarker : "[ifpe_arg]", // Matches constant in IFramePopupExtender.cs
	m_OpenerElement : null,
	
	Show : function() {
		// If a popup group is specified, make sure this is the only
		// active instance in that group
		if(this.m_PopupGroup != null) {
			var current = IFramePopupController._popupGroups[this.m_PopupGroup];
			if(current != null)
				current.Close();
			IFramePopupController._popupGroups[this.m_PopupGroup] = this;
		}
		
		// Create the container DIV
		var elem = this.m_ContainerElement = document.createElement("DIV");
		elem.style.position = 'absolute';
		elem.style.border = '1px solid gray';
		elem.style.backgroundColor = 'white';
		elem.style.left = this.m_XPos + "px";
		elem.style.top = this.m_YPos + "px";	
		elem.style.zIndex = 1000;	
		elem.style.zOrder = 1000;
		if(typeof(elem.style.MozBorderRadius) != "undefined") {
			// Eye-candy for mozilla users
			elem.style.padding = "4px";
			elem.style.MozBorderRadius = "10px";		
			elem.style.border = '2px solid #999';
		}
		// Eye-candy for IE users
		elem.style.filter = "progid:DXImageTransform.Microsoft.Shadow(color:#888888, strength:4, direction:135)";
		
		var frame = this.m_IFrameElement = document.createElement("IFRAME");
		frame.style.width = this.m_Width + 'px';
		frame.style.height = '40px';
		frame.frameBorder = '0';
		frame.scrolling = 'no';
		
		elem.appendChild(frame);
		document.body.insertBefore(elem, document.body.childNodes[0]);
		
		// Insert "Loading..." message		
		var doc = frame.contentDocument;
		if (doc == undefined || doc == null) doc = frame.contentWindow.document;
		doc.open();
		doc.write("<span style='font-family:helvetica; color:gray; font-size:small;'>Loading...</span>");
		doc.close();
		doc.IFramePopupController = this;
		this._keepInsideClientArea();
		
		// Load the real contents
		this._addIFrameOnLoadedEvent(frame, this._handleIFramePageLoad);
		frame.src = this.m_Url;
	},	
	
	Close : function(bRaisePostback, commandArgument) {
		if(!this.m_IsClosed) {
			this.m_ContainerElement.parentNode.removeChild(this.m_ContainerElement);
			this.m_IsClosed = true;
		}
		if(bRaisePostback) {
			var cmd = this.m_PostbackEventRaiser.replace(this.m_PostbackArgumentMarker, commandArgument);
			eval(cmd);
		}
	},
	
	RefreshPopupSize : function() {
		// If there's exactly one form, display it. Otherwise display the whole document.
		var doc = this._getPopupDocumentElement();
		var displayElement = doc;
		var forms = doc.getElementsByTagName("FORM");
		
		if(forms.length == 1)
			displayElement = forms[0];
		
		// Adjust height of popup to get this item in view
		padding = navigator.userAgent.indexOf("MSIE") >= 0 ? 0 : parseInt(doc.body.style.marginTop,10) + parseInt(doc.body.style.marginBottom,10);
		if(isNaN(padding)) padding = 0;
		var height = displayElement.offsetHeight;

		this._resizeView(null, height + padding);	
	},
	
	GetOpenerElement : function() {
		return this.m_OpenerElement;
	},
	
	_getPopupDocumentElement : function() {
		var doc = this.m_IFrameElement.contentDocument;
		if (doc == undefined || doc == null) doc = this.m_IFrameElement.contentWindow.document;		
		return doc;	
	},
	
	_resizeView : function(width, height) {
		if(width != null)
			this.m_IFrameElement.style.width = width + 'px';
		if(height != null)
			this.m_IFrameElement.style.height = height + 'px';
		this._keepInsideClientArea();
	},
	
	_getViewportDimensions : function() {
		// The very worst of the browser-compatibility nightmare
		var size;
		if(typeof(window.innerWidth) != 'undefined') // Standards-compliant
			size = [window.innerWidth, window.innerHeight];
		else if((typeof(document.documentElement) != 'undefined') && (typeof(document.documentElement.clientWidth) != 'undefined') && (document.documentElement.clientWidth != 0)) // IE 7
			size = [document.documentElement.clientWidth, document.documentElement.clientHeight];
		else // Older IE
			size = [document.body.clientWidth, document.body.clientHeight];
		var scrollTop = Math.max(document.documentElement.scrollTop, document.body.scrollTop);
        var scrollLeft = Math.max(document.documentElement.scrollLeft, document.body.scrollLeft);
              
		return { "size" : {"width":size[0],"height":size[1]}, "scroll" : {"left":scrollLeft, "top":scrollTop} };
	},
	
	_keepInsideClientArea : function() {
		var elem = this.m_ContainerElement;
		var clientArea = this._getViewportDimensions();

		if(elem.offsetLeft + elem.offsetWidth + 30 > clientArea.size.width + clientArea.scroll.left)
			elem.style.left = clientArea.size.width + clientArea.scroll.left - elem.offsetWidth - 30 + "px";
		if(elem.offsetTop + elem.offsetHeight + 30 > clientArea.size.height + clientArea.scroll.top)
			elem.style.top = clientArea.size.height + clientArea.scroll.top - elem.offsetHeight - 30 + "px";
	},
	
	_handleIFramePageLoad : function() {
		try {	
			this.RefreshPopupSize();
			
			// Supply it with reference to controller
			this._getPopupDocumentElement().IFramePopupController = this;
		} catch(ex) {
			// Cross-domain scripting prohibited when login control forces the use of SSL
			// Can't refresh popup size until it comes back out of SSL. We just have to swallow the exception.
		}
	},
	
	_addIFrameOnLoadedEvent : function(iframe, eventHandler) {
		// Firefox / Safari / Opera
		var self = this;
		iframe.onload = function() { eventHandler.call(self); }
		
		// IE
		if(navigator.userAgent.indexOf("MSIE") >= 0) {
			// IE, bizarrely, reports the document is complete three times. We have to wait for the third occasion. This is "by design" (http://support.microsoft.com/kb/q188763/) 			
			var count = 0;
			iframe.onreadystatechange = function() {
				if(iframe.document) {
					count++;
					if(count >= 3) {
						eventHandler.call(self);
						count = 0; // Reset counter in preparation for next page load
					}
				}
			}
		}
	}
}