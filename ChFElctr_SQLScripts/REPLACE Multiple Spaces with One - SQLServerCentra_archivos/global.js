function ssc_showPostsSinceLastVisit() {
	// Make a form we can post
	// (don't use the existing one as it contains unhelpful VIEWSTATE info and its name varies)
	var form = document.createElement("FORM");
	form.action = "/Forums/Default.aspx";
	form.method = "POST";
	form.style.display = "none";
	document.body.appendChild(form);
	var target = form.appendChild(document.createElement("INPUT"));
	target.name = "__EVENTTARGET";
	target.value = "smRecentPosts";
	var argument = form.appendChild(document.createElement("INPUT"));
	argument.name = "__EVENTARGUMENT";
	argument.value = "butViewPostsSinceLastVisit";
	form.submit();
}

function GetIntendedContentWidth(element) {
	while(element.tagName != "TD") {
		element = element.parentNode;
		if(!element)
			return 500;
	}
	return element.offsetWidth - 10;
}