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

// Sidebar: Expanded ↔ Collapsed (icon-only) - Plain JS (no jQuery)
(function () {
    "use strict";

    var sidebarStorageKey = "gv.sidebar.state"; // "expanded" | "collapsed"

    function setIcon(isCollapsed) {
        var icon = document.querySelector('#sidebar-toggle i');
        if (!icon) return;
        icon.classList.toggle('fa-angle-double-right', isCollapsed);
        icon.classList.toggle('fa-angle-double-left', !isCollapsed);
    }

    function applyState(state) {
        var wrapper = document.getElementById('wrapper');
        if (!wrapper) return;

        var isCollapsed = state === 'collapsed';

        // Make sure sidebar is visible using legacy class
        wrapper.classList.add('toggled');
        wrapper.classList.toggle('sidebar-collapsed', isCollapsed);

        setIcon(isCollapsed);
    }

    function getInitialState() {
        try {
            var saved = window.localStorage.getItem(sidebarStorageKey);
            return (saved === 'collapsed' || saved === 'expanded') ? saved : 'expanded';
        } catch (e) {
            return 'expanded';
        }
    }

    function toggleState() {
        var wrapper = document.getElementById('wrapper');
        if (!wrapper) return;

        var next = wrapper.classList.contains('sidebar-collapsed') ? 'expanded' : 'collapsed';
        try {
            window.localStorage.setItem(sidebarStorageKey, next);
        } catch (e) {
            // ignore
        }

        applyState(next);
        console.log('Sidebar toggled');
    }

    document.addEventListener('DOMContentLoaded', function () {
        console.log('Sidebar script loaded');

        applyState(getInitialState());

        var toggleBtn = document.getElementById('sidebar-toggle');
        if (toggleBtn && !toggleBtn.__gvBound) {
            toggleBtn.__gvBound = true;
            toggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleState();
            });
        }
    });
})();
