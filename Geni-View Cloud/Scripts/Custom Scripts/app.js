/* swap open/close side menu icons */
$('[data-toggle=collapse]').click(function () {
    $(this).find("i").toggleClass("fa-chevron-down fa-stack-1x fa-chevron-right fa-stack-1x");
});

$(function () {
    $('.subnav li a').filter(function () { return this.href == location.href }).parent().addClass('selected').siblings().removeClass('selected')
    $('.subnav li a').click(function () {
        $(this).parent().addClass('selected').siblings().removeClass('selected')
    })
})

// Scroll
var btn = $('#go-top-button');

$(window).scroll(function () {
    if ($(window).scrollTop() > 200) {
        btn.addClass('show');
    } else {
        btn.removeClass('show');
    }
});

btn.on('click', function (e) {
    e.preventDefault();
    $('html, body').animate({ scrollTop: 0 }, '300');
});

//Theme Selector
$("#select-theme").val(localStorage.getItem("theme"));

function SelectTheme(control) {
    try {
        var storTest = window['localStorage'];
        storTest.setItem("", ".");
        storTest.removeItem("");
    } catch (e) {
        console.log("Cannot save theme...")
    }

    var root = document.getElementsByTagName('html')[0];
    root.classList.remove(localStorage.getItem("theme"));
    localStorage.setItem("theme", control.value);
    root.classList.add(localStorage.getItem("theme"));
}

$(function () {
    $('.collapsable .btn').click(function (e) {
        e.stopPropagation();
    });
});

$(function () {
    $('.panel-heading .disableScroll').click(function (e) {
        e.stopPropagation();
    });
});

function RefreshPage() {
    window.location.reload();
}

// Sidebar: Expanded ↔ Collapsed (icon-only)
(function () {
    "use strict";

    var sidebarStorageKey = "gv.sidebar.state"; // "expanded" | "collapsed"

    function updateToggleIcons(isCollapsed) {
        var $icon = $("#sidebar-toggle i");
        if ($icon.length) {
            $icon.toggleClass("fa-angle-double-right", isCollapsed);
            $icon.toggleClass("fa-angle-double-left", !isCollapsed);
        }
    }

    function applySidebarState(state) {
        var isCollapsed = state === "collapsed";
        var $wrapper = $("#wrapper");

        $wrapper.toggleClass("sidebar-collapsed", isCollapsed);
        $wrapper.toggleClass("toggled", !isCollapsed);

        updateToggleIcons(isCollapsed);
    }

    function getInitialSidebarState() {
        var saved = localStorage.getItem(sidebarStorageKey);
        if (saved === "collapsed" || saved === "expanded") {
            return saved;
        }
        return "expanded";
    }

    function toggleSidebar(e) {
        if (e) e.preventDefault();

        var nextState = $("#wrapper").hasClass("sidebar-collapsed")
            ? "expanded"
            : "collapsed";

        localStorage.setItem(sidebarStorageKey, nextState);
        applySidebarState(nextState);
    }

    $(function () {
        applySidebarState(getInitialSidebarState());

        // Top navbar toggle (existing)
        $("#menu-toggle").off("click").on("click.gvSidebar", toggleSidebar);

        // In-sidebar toggle (UX)
        $("#sidebar-toggle").off("click").on("click.gvSidebar", toggleSidebar);
    });
})();
