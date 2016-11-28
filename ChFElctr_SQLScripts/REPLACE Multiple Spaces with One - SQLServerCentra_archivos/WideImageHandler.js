var WideImageHandler = {
	AddSupportForWideImages : function() {
		// We'll only look for images which are descendents of TABLE nodes with class "DynamicContent"
		// (i.e. the containers for articles/scripts/questions)
		var tables = document.getElementsByTagName("TABLE");	
		for(var i = 0; tables[i]; i++)
			if(tables[i].className == "DynamicContent") {
			
				var imgs = tables[i].getElementsByTagName("IMG");
				for(var j = 0; imgs[j]; j++) {
					var img = imgs[j];
			
					// Check if the image is too wide
					var columnCell = WideImageHandler._findContainerColumn(img);
					if(columnCell && (img.offsetWidth >= columnCell.offsetWidth - 5))
						WideImageHandler._handleWideImage(img);
				}		
			}
	},

	_findContainerColumn : function(elem) {
		var lastTD = null;
		do {
			if(elem.tagName == "TD")
				lastTD = elem;
			elem = elem.parentNode;
			if(!elem) return null;
		} while((elem.tagName != "TABLE") || (elem.className != "DynamicContent"));
		return lastTD;
	},

	_handleWideImage : function(img) {
		// If it's in a div with class "img_container", leave it alone
		if(img.parentNode.className != "img_container") {
			// First scale it down to fit the column
			img.style.display = "block";
			img = WideImageHandler._resizeImageToFitParentHorizontally(img, 2);
			
			// Now add the "zoom" buttons and functionality
			WideImageHandler._addZoomingControls(img);
		}
	},

	_addZoomingControls : function(img) {
		// Prepare a border
		var containerDiv = document.createElement("DIV");
		containerDiv.className = "wideImageContainer";
		containerDiv.style.width = img.offsetWidth + "px";
		
		// Move the image inside the border
		img.parentNode.insertBefore(containerDiv, img);
		img.parentNode.removeChild(img);
		containerDiv.appendChild(img);	
		
		// Add other zooming controls
		var zoomControls = containerDiv.appendChild(document.createElement("DIV"));
		var zoomIcon = zoomControls.appendChild(document.createElement("IMG"));
		zoomIcon.src = "/Resources/Images/zoom.gif";
		zoomIcon.align = "bottom";
		zoomIcon.style.margin = "0px 3px 0px 3px";
		var link = document.createElement("A");
		link.href = "javascript:;";
		link.innerHTML = "Zoom in";
		var isZoomedIn = false;
		img.style.cursor = document.all ? "pointer, hand" : "-moz-zoom-in";
		link.onclick = img.onclick = function() { 
			if(!isZoomedIn) {			
				// Allow the box to extend beyond the column
				containerDiv.style.position = "relative";
				containerDiv.style.zIndex = containerDiv.style.zOrder = 1;
				containerDiv.style.width = img.originalSize[0] + "px";
				if(typeof(link.innerText) == "string") // Work around IE bug whereby "Object exception" is thrown when you modify .innerHTML under some obscure conditions (which happen here)
					link.innerText = "Zoom out";
				else
					link.innerHTML = "Zoom out";
					
				// Put a full-size image into it
				var fullSizeImage = document.createElement("IMG");
				fullSizeImage.style.cursor = document.all ? "pointer, hand" : "-moz-zoom-out";
				fullSizeImage.onclick = link.onclick;
				link.fullSizeImage = fullSizeImage;
				fullSizeImage.src = img.src;
				setTimeout(function() { 
					img.parentNode.removeChild(img);
						containerDiv.insertBefore(fullSizeImage, containerDiv.childNodes[0]) 
					}, 0);
				isZoomedIn = true;
			} else {
				// Restore the previous "zoomed out" state
				if(typeof(link.innerText) == "string")
					link.innerText = "Zoom in";
				else
					link.innerHTML = "Zoom in";
				link.fullSizeImage.parentNode.insertBefore(img, link.fullSizeImage);
				link.fullSizeImage.parentNode.removeChild(link.fullSizeImage);
				containerDiv.style.position = "static";
				containerDiv.style.width = img.offsetWidth + "px";
				isZoomedIn = false;
			}
		};
		zoomControls.appendChild(link);
		
		var separator = document.createElement("SPAN");
		separator.innerHTML = "&nbsp;&nbsp;|&nbsp;&nbsp;";
		zoomControls.appendChild(separator);
		
		// Link to open image in pop-up window
		var launchLink = document.createElement("A");
		launchLink.href = "javascript:;";
		launchLink.innerHTML = "Open in new window";
		zoomControls.appendChild(launchLink);
		launchLink.onclick = function() { 
				window.open(img.src, "", "width="+(img.originalSize[0]+20)+",height="+(img.originalSize[1]+20));
			};	
	},

	_resizeImageToFitParentHorizontally : function(img, padding) {
		if(img.getAttribute("width")) img.removeAttribute("width");	
		if(img.getAttribute("height")) img.removeAttribute("height");	
		var originalSize = [img.width, img.height];

		if(typeof(img.style.msInterpolationMode) == "string") {
			// IE7 has a very handy "interpolation mode" CSS style
			img.style.msInterpolationMode = "bicubic";
			img.style.maxWidth = (WideImageHandler._findContainerColumn(img).offsetWidth - 2*padding) + "px"; 
		}
		else if(typeof(img.style.filter) == "string") {
			// IE5.5+ can do something like bilinear interpolation using the "AlphaImageLoader" filter
			// (but it's a pain to determine the intended width, since "offsetWidth" is broken)
			var tmpdiv = document.createElement("DIV");
			tmpdiv.style.border = "1px solid red"; // For some reason IE6 needs this or it ignores the layout-fixed style
			WideImageHandler._findContainerColumn(img).appendChild(tmpdiv);		
			var newWidth = tmpdiv.offsetWidth - 2*padding;
			tmpdiv.parentNode.removeChild(tmpdiv);			
			var newHeight = (newWidth / img.width) * img.height;

			var container = document.createElement("DIV");
			container.innerHTML = "<DIV STYLE=\"width:"+newWidth+"px; height:"+newHeight+"px; filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + img.src + "', sizingMethod='scale');\" >&nbsp;</div>";
			img.parentNode.insertBefore(container, img);		
			img.parentNode.removeChild(img);
			container.src = img.src;
			img = container;
		} 
		else {
			// Firefox has no way of doing any other type of interpolation, but setting the correct size is easy.
			// Apparently the "Cairo" rendering engine in Firefox 3 will pick a nice resampling method by 
			// default when images are resized.
			img.style.maxWidth = WideImageHandler._findContainerColumn(img).offsetWidth + "px"; 		
		}
		img.originalSize = originalSize;
		return img;
	}
};