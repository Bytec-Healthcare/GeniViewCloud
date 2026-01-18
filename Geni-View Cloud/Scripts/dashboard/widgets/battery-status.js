(function (window, $) {
    "use strict";

    function clampPercent(v) {
        if (v < 0) return 0;
        if (v > 100) return 100;
        return v;
    }

    function toNumber(v) {
        if (typeof v === "number") return v;
        var n = parseFloat(v);
        return isFinite(n) ? n : 0;
    }

    function updateBatteryStatus(model) {
        if (!model) return;

        var total = toNumber(model.PowerModulesCount);

        var onCharging = toNumber(model.OnDeviceChargingCount);
        var onDischarging = toNumber(model.OnDeviceDischargingCount);
        var onIdle = toNumber(model.OnDeviceIdleCount);
        var offCharging = toNumber(model.OffDeviceChargingCount);
        var offIdle = toNumber(model.OffDeviceIdleCount);

        var efficiency = model.EfficiencyScorePercent;
        if (typeof efficiency !== "number") efficiency = parseInt(efficiency, 10);
        if (!isFinite(efficiency)) efficiency = 0;

        $("#bsPowerModules").text(total);
        $("#bsEfficiencyScore").text(efficiency);

        $("#bsOnCharging").text(onCharging);
        $("#bsOnDischarging").text(onDischarging);
        $("#bsOnIdle").text(onIdle);
        $("#bsOffCharging").text(offCharging);
        $("#bsOffIdle").text(offIdle);

        // Use percent fields from backend if present; else compute from counts.
        var p1 = model.OnDeviceChargingPercent;
        var p2 = model.OnDeviceDischargingPercent;
        var p3 = model.OnDeviceIdlePercent;
        var p4 = model.OffDeviceChargingPercent;
        var p5 = model.OffDeviceIdlePercent;

        if (typeof p1 !== "number" || typeof p2 !== "number" || typeof p3 !== "number" || typeof p4 !== "number" || typeof p5 !== "number") {
            if (total > 0) {
                p1 = (onCharging * 100) / total;
                p2 = (onDischarging * 100) / total;
                p3 = (onIdle * 100) / total;
                p4 = (offCharging * 100) / total;
                p5 = (offIdle * 100) / total;
            } else {
                p1 = p2 = p3 = p4 = p5 = 0;
            }
        }

        p1 = clampPercent(toNumber(p1));
        p2 = clampPercent(toNumber(p2));
        p3 = clampPercent(toNumber(p3));
        p4 = clampPercent(toNumber(p4));
        p5 = clampPercent(toNumber(p5));

        // Fix drift
        var sum = p1 + p2 + p3 + p4 + p5;
        if (sum > 0 && sum !== 100) {
            p5 = clampPercent(100 - (p1 + p2 + p3 + p4));
        }

        $("#bsBarOnCharging").css("width", p1 + "%");
        $("#bsBarOnDischarging").css("width", p2 + "%");
        $("#bsBarOnIdle").css("width", p3 + "%");
        $("#bsBarOffCharging").css("width", p4 + "%");
        $("#bsBarOffIdle").css("width", p5 + "%");
    }

    function load(url) {
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                updateBatteryStatus(data);
            },
            error: function (xhr, status, error) {
                if (window.console && window.console.error) {
                    console.error("Failed to load Battery Status:", status, error);
                }
            }
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.BatteryStatus = {
        init: function (options) {
            if (!options || !options.url) return;
            load(options.url);
        }
    };

})(window, window.jQuery);
