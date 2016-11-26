$(document).ready(function() {
    var loadScript = function(src) {
        var scriptTag = document.createElement("script");
        scriptTag.type = "text/javascript";
        scriptTag.async = true;
        scriptTag.src = document.location.protocol + "//" + src;
        $("head").append(scriptTag);
    };

    window.fbAsyncInit = function() {
        FB.init({
            status: true, // check login status
            cookie: true, // enable cookies to allow the server to access the session
            xfbml: true,  // parse XFBML
            channelUrl: document.location.protocol + "//" + document.location.host + "/facebook/channel.html"
        });
    };

    loadScript("widgets.digg.com/buttons.js");
    loadScript("platform.twitter.com/widgets.js");
    loadScript("platform.linkedin.com/in.js");
    loadScript("connect.facebook.net/en_US/all.js");
});