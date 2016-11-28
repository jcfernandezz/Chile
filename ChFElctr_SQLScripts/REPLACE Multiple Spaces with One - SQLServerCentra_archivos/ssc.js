(function() {
    if (window.SSC == null) {
        window.SSC = {};
    }
    SSC.define = function(namespace) {
        var namespaces = namespace.split(".");
        var parent = window;
        for (var i = 0; i < namespaces.length; i++) {
            parent[namespaces[i]] = parent[namespaces[i]] || {};
            parent = parent[namespaces[i]];
        }
    };

    $(document).ready(function() {
        var opacity = 0.6;
        $(".image-popup").colorbox({
            maxHeight: 500,
            scalePhotos: false,
            close: "Close",
            opacity: opacity
        });
        $(".iframe-popup").colorbox({
            height: 500,
            close: "Close",
            width: 900,
            iframe: true,
            fastIframe: false,
            opacity: opacity,
            onComplete: function() {
                var iframe = $(".cboxIframe");
                var iframeUrl = iframe.contents().get(0).location.href;
                iframe.contents().find("a").each(function(index, element) {
                    var href = element.getAttribute("href");
                    if (href !== null && href.charAt(0) !== "#" && href.indexOf(iframeUrl + "#") !== 0) {
                        element.setAttribute("target", "_blank");
                    }
                });
            }
        });
    });
})();


(function() {
    $(document).ready(function() {
        var mouseButtonClicked = function(event) {
            if (event.which === 1) {
                return "LeftClick";
            }
            if (event.which === 2) {
                return "MiddleClick";
            }
            if (event.which === 3) {
                return "RightClick";
            }
            return "Other";
        };

        $("a.track-clicks").each(function(index, element) {
            var category = element.getAttribute("data-event-category");
            $(element).one("mouseup", function(event) {
                _gaq.push(['_trackEvent', category, mouseButtonClicked(event), location.href + " to " + element.href]);
            });
        });
    });
})();






