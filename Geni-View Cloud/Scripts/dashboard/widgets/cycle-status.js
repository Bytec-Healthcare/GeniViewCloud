(function (window, $) {
    "use strict";

    function clampPercent(value) {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }

    function updateCycleStatus(model) {
        if (!model) return;

        var total = model.TotalCount || 0;
        var activePct = clampPercent(parseFloat(model.ActivePercent || 0));
        var idlePct = clampPercent(parseFloat(model.IdlePercent || 0));
        var svcPct = clampPercent(parseFloat(model.SvcPercent || 0));

        var sum = activePct + idlePct + svcPct;
        if (sum > 0 && sum !== 100) {
            svcPct = clampPercent(100 - activePct - idlePct);
        }

        $("#cycleActiveCount").text(model.ActiveCount || 0);
        $("#cycleIdleCount").text(model.IdleCount || 0);
        $("#cycleSvcCount").text(model.SvcCount || 0);

        $("#cycleBarActive").css("width", activePct + "%");
        $("#cycleBarIdle").css("width", idlePct + "%");
        $("#cycleBarSvc").css("width", svcPct + "%");

        var uptime = 0;
        if (total > 0) {
            uptime = Math.round(((model.ActiveCount + model.IdleCount) * 100) / total);
        }
        $("#cycleUptimePercent").text(uptime + "%");
    }

    function loadCycleStatus(url) {
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                updateCycleStatus(data);
            }
        });
    }

    // Expose a very small API so the Razor view can provide the correct MVC URL.
    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.CycleStatus = {
        init: function (options) {
            if (!options || !options.url) return;
            loadCycleStatus(options.url);
        }
    };

})(window, window.jQuery);
