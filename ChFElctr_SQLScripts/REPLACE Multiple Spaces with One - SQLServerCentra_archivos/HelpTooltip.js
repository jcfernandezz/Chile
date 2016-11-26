$(document).ready(function() {
    $(".HelpTooltipToggle").click(function(event) {
        var tooltip = $(this).next();
        tooltip.css({
            position: "absolute",
            left: event.pageX + 10,
            top: event.pageY + 10,
            width: "300px",
            padding: "1em",
            border: "solid black 1px",
            "background-color": "#ffc",
            "text-align": "left"
        });
        
        tooltip.toggle();
        return false;
    });
    $(".HelpTooltipClose").click(function() {
        $(this).parents(".HelpTooltip").hide();
    });
});