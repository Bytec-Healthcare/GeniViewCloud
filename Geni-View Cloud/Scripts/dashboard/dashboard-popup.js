(function (window, $) {
    "use strict";

    var state = {
        cardKey: null,
        pageNumber: 1,
        pageSize: 10,
        search: ""
    };

    var activeRequestId = 0;

    function openPopup() {
        $("#dashboardPopupOverlay").addClass("is-open");
        $("body").css("overflow", "hidden");
    }

    function closePopup() {
        $("#dashboardPopupOverlay").removeClass("is-open");
        $("body").css("overflow", "");
    }

    function debounce(fn, ms) {
        var t = null;
        return function () {
            var args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(null, args); }, ms);
        };
    }

    function showLoading() {
        var $tb = $("#gvPopupTbody").empty();
        $tb.append("<tr><td colspan='10' style='text-align:center;'>Loading...</td></tr>");

        $("#gvPopupPager").empty();
        $("#gvPopupInfo").text("");
        $("#gvPopupSubtitle").text("Power Modules : -");
    }

    function formatDate(v) {
        if (!v) return "-";

        if (typeof v === "string") {
            var m = /\/Date\((\-?\d+)\)\//.exec(v);
            if (m && m[1]) {
                v = parseInt(m[1], 10);
            }
        }

        var d = new Date(v);
        if (isNaN(d.getTime())) return "-";

        var mm = (d.getMonth() + 1).toString().padStart(2, "0");
        var dd = d.getDate().toString().padStart(2, "0");
        var yyyy = d.getFullYear();
        return dd + "/" + mm + "/" + yyyy;
    }

    function escapeHtml(v) {
        if (v == null) return "";
        return String(v)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function getSocClass(v) {
        if (v == null) return "";
        if (v > 70) return "gv-soc--high";
        if (v >= 30) return "gv-soc--low";
        return "gv-soc--charge";
    }

    function renderCycleCount(v) {
        if (v == null) return "-";
        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='/Content/Img/widgets/popupcc.svg' alt='' />" +
            "<span>" + escapeHtml(v) + "</span>" +
            "</span>"
        );
    }

    function renderTemp(v) {
        if (v == null) return "-";

        var icon = v > 30
            ? "/Content/Img/widgets/popuptemp1.svg"
            : "/Content/Img/widgets/poptemp2.svg";

        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='" + icon + "' alt='' />" +
            "<span>" + escapeHtml(v) + "°C</span>" +
            "</span>"
        );
    }

    function getStatusIcon(status) {
        var s = (status || "").toString();

        if (s === "On Device - Charging" || s === "Off Device - Charging") {
            return "/Content/Img/widgets/oncharging.svg";
        }

        if (s === "On Device - Discharging") {
            return "/Content/Img/widgets/ondischarging.svg";
        }

        if (s === "On Device - Idle" || s === "Off Device - Idle") {
            return "/Content/Img/widgets/popupidle.svg";
        }

        return null;
    }

    function renderStatus(status) {
        var text = status || "-";
        var icon = getStatusIcon(text);

        if (!icon) return escapeHtml(text);

        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='" + icon + "' alt='' />" +
            "<span>" + escapeHtml(text) + "</span>" +
            "</span>"
        );
    }

    function renderRows(items) {
        var $tb = $("#gvPopupTbody").empty();

        if (!items || !items.length) {
            $tb.append("<tr><td colspan='10' style='text-align:center;'>No records found</td></tr>");
            return;
        }

        for (var i = 0; i < items.length; i++) {
            var r = items[i];

            var socText = (r.SoC == null ? "-" : (r.SoC + "%"));
            var socClass = getSocClass(r.SoC);

            $tb.append(
                "<tr>" +
                "<td>" + escapeHtml(r.PowerModules || "") + "</td>" +
                "<td>" + escapeHtml(r.AttachedTo || "-") + "</td>" +
                "<td>" + escapeHtml(r.DeviceType || "-") + "</td>" +
                "<td><span class='" + socClass + "'>" + escapeHtml(socText) + "</span></td>" +
                "<td>" + renderCycleCount(r.CycleCount) + "</td>" +
                "<td>" + renderTemp(r.Temperature) + "</td>" +
                "<td>" + renderStatus(r.Status) + "</td>" +
                "<td>" + escapeHtml(r.LastAttached || "-") + "</td>" +
                "<td>" + escapeHtml(formatDate(r.LastCharged)) + "</td>" +
                "<td>" + escapeHtml(formatDate(r.LastDischarged)) + "</td>" +
                "</tr>"
            );
        }
    }

    function renderPager(total, pageNumber, pageSize) {
        var $pager = $("#gvPopupPager").empty();
        var $info = $("#gvPopupInfo").empty();

        if (!total) {
            $info.text("Showing 0 to 0 out of 0");
            return;
        }

        var start = ((pageNumber - 1) * pageSize) + 1;
        var end = Math.min(pageNumber * pageSize, total);
        $info.text("Showing " + start + " to " + end + " out of " + total);

        var totalPages = Math.ceil(total / pageSize);
        if (totalPages <= 1) return;

        function addBtn(text, page, disabled, current) {
            var $a = $("<a href='#' class='gv-page'></a>").text(text);
            if (disabled) $a.addClass("disabled");
            if (current) $a.addClass("current");
            $a.on("click", function (e) {
                e.preventDefault();
                if (disabled) return;
                state.pageNumber = page;
                loadData();
            });
            $pager.append($a);
        }

        addBtn("<", pageNumber - 1, pageNumber <= 1, false);

        var maxButtons = 4;
        var startPage = Math.max(1, pageNumber - Math.floor(maxButtons / 2));
        var endPage = Math.min(totalPages, startPage + maxButtons - 1);
        startPage = Math.max(1, endPage - maxButtons + 1);

        for (var p = startPage; p <= endPage; p++) {
            addBtn(String(p), p, false, p === pageNumber);
        }

        addBtn(">", pageNumber + 1, pageNumber >= totalPages, false);
    }

    function setHeader(powerModulesCount) {
        $("#gvPopupSubtitle").text("Power Modules : " + (powerModulesCount || 0));
    }

    function loadData() {
        var requestId = ++activeRequestId;

        showLoading();

        $.ajax({
            type: "GET",
            dataType: "json",
            url: "/DashboardPopup/GetSocPopupData",
            data: {
                cardKey: state.cardKey,
                search: state.search,
                pageNumber: state.pageNumber,
                pageSize: state.pageSize
            }
        }).done(function (resp) {
            if (requestId !== activeRequestId) return;

            var items = resp && resp.Items ? resp.Items : [];
            renderRows(items);
            renderPager(resp.Total || 0, resp.PageNumber || 1, resp.PageSize || state.pageSize);
            setHeader(resp.PowerModulesCount || 0);
        }).fail(function (xhr, status, err) {
            if (requestId !== activeRequestId) return;

            if (window.console && window.console.error) {
                console.error("[Popup] GetSocPopupData failed:", status, err);
                console.error("[Popup] HTTP:", (xhr && xhr.status), (xhr && xhr.responseText));
            }

            renderRows([]);
            renderPager(0, state.pageNumber, state.pageSize);
            setHeader(0);
        });
    }

    function openFor(cardKey) {
        state.cardKey = cardKey;
        state.pageNumber = 1;
        state.search = "";
        $("#gvPopupSearch").val("");

        openPopup();
        loadData();
    }

    function bindDelegatedClicks() {
        $(document)
            .off("click.dashboardPopup", "#socWidget .soc-status__card")
            .on("click.dashboardPopup", "#socWidget .soc-status__card", function () {
                var key = ($(this).data("card-key") || "").toString();
                if (!key) return;
                openFor(key);
            });

        $(document)
            .on("mouseenter.dashboardPopup", "#socWidget .soc-status__card", function () { $(this).css("cursor", "pointer"); });
    }

    $(document).ready(function () {
        bindDelegatedClicks();

        $("#dashboardPopupClose").on("click", closePopup);

        $("#dashboardPopupOverlay").on("click", function (e) {
            if (e.target && e.target.id === "dashboardPopupOverlay") {
                closePopup();
            }
        });

        $(document).on("keydown.dashboardPopup", function (e) {
            if (e.key === "Escape" && $("#dashboardPopupOverlay").hasClass("is-open")) {
                closePopup();
            }
        });

        $("#gvPopupSearch").on("input", debounce(function () {
            state.search = $("#gvPopupSearch").val();
            state.pageNumber = 1;
            loadData();
        }, 300));
    });

})(window, window.jQuery);