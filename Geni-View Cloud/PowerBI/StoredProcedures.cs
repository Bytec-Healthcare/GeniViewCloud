namespace GeniView.Cloud.PowerBI
{
    // PostgreSQL CREATE OR REPLACE VIEW / FUNCTION statements.
    // All are idempotent — safe to re-execute on every startup.
    public static class StoredProcedures
    {
        // ── Analytics views (Power BI) ────────────────────────────────────────

        public static string AgentBatteryLogsWithDurationView => """
            CREATE OR REPLACE VIEW "AgentBatteryLogsWithDuration" AS
            SELECT
                CASE
                    WHEN "prevRowTimeStamp" IS NULL THEN NULL
                    WHEN EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint > 3600 THEN NULL
                    ELSE EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint
                END AS "Duration",
                inner_q.*
            FROM (
                SELECT
                    (
                        SELECT MAX(t2."Timestamp")
                        FROM "AgentBatteryLogs" t2
                        WHERE t2."Battery_ID" = t1."Battery_ID"
                          AND t2."Timestamp" < t1."Timestamp"
                    ) AS "prevRowTimeStamp",
                    ROW_NUMBER() OVER (PARTITION BY "Battery_ID", "Bay" ORDER BY "Timestamp") AS "rk",
                    t1.*
                FROM "AgentBatteryLogs" t1
            ) AS inner_q
            """;

        public static string AgentDeviceLogsWithDurationView => """
            CREATE OR REPLACE VIEW "AgentDeviceLogsWithDuration" AS
            SELECT
                CASE
                    WHEN "prevRowTimeStamp" IS NULL THEN NULL
                    WHEN EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint > 3600 THEN NULL
                    ELSE EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint
                END AS "Duration",
                inner_q.*
            FROM (
                SELECT
                    (
                        SELECT MAX(t2."Timestamp")
                        FROM "AgentDeviceLogs" t2
                        WHERE t2."Device_ID" = t1."Device_ID"
                          AND t2."Timestamp" < t1."Timestamp"
                    ) AS "prevRowTimeStamp",
                    ROW_NUMBER() OVER (PARTITION BY "Device_ID" ORDER BY "Timestamp") AS "rk",
                    t1.*
                FROM "AgentDeviceLogs" t1
            ) AS inner_q
            """;

        public static string InternalBatteryLogsWithDurationView => """
            CREATE OR REPLACE VIEW "InternalBatteryLogsWithDuration" AS
            SELECT
                CASE
                    WHEN "prevRowTimeStamp" IS NULL THEN NULL
                    WHEN EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint > 3600 THEN NULL
                    ELSE EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint
                END AS "Duration",
                inner_q.*
            FROM (
                SELECT
                    (
                        SELECT MAX(t2."Timestamp")
                        FROM "InternalBatteryLogs" t2
                        WHERE t2."Battery_ID" = t1."Battery_ID"
                          AND t2."Timestamp" < t1."Timestamp"
                    ) AS "prevRowTimeStamp",
                    ROW_NUMBER() OVER (PARTITION BY "Battery_ID", "Bay" ORDER BY "Timestamp") AS "rk",
                    t1.*
                FROM "InternalBatteryLogs" t1
            ) AS inner_q
            """;

        public static string InternalDeviceLogsWithDurationView => """
            CREATE OR REPLACE VIEW "InternalDeviceLogsWithDuration" AS
            SELECT
                CASE
                    WHEN "prevRowTimeStamp" IS NULL THEN NULL
                    WHEN EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint > 3600 THEN NULL
                    ELSE EXTRACT(EPOCH FROM ("Timestamp" - "prevRowTimeStamp"))::bigint
                END AS "Duration",
                inner_q.*
            FROM (
                SELECT
                    (
                        SELECT MAX(t2."Timestamp")
                        FROM "InternalDeviceLogs" t2
                        WHERE t2."Device_ID" = t1."Device_ID"
                          AND t2."Timestamp" < t1."Timestamp"
                    ) AS "prevRowTimeStamp",
                    ROW_NUMBER() OVER (PARTITION BY "Device_ID" ORDER BY "Timestamp") AS "rk",
                    t1.*
                FROM "InternalDeviceLogs" t1
            ) AS inner_q
            """;

        // ── Dashboard functions ───────────────────────────────────────────────
        // Each function mirrors the original SQL Server stored procedure.
        // Called from DashboardDataRepository via: SELECT * FROM function_name(...)

        public static string FnGetLatestBatteryCycleCount => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryCycleCount()
            RETURNS TABLE(
                "Battery_ID"              bigint,
                "Timestamp"               timestamp,
                "OperatingData_CycleCount" int
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp",
                    l."OperatingData_CycleCount"
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetLatestBatteryStateOfCharge => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryStateOfCharge()
            RETURNS TABLE(
                "Battery_ID"                              bigint,
                "Timestamp"                               timestamp,
                "SlowChangingDataA_RelativeStateOfCharge" int
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp",
                    l."SlowChangingDataA_RelativeStateOfCharge"
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetLatestBatteryTimestamp => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryTimestamp()
            RETURNS TABLE(
                "Battery_ID" bigint,
                "Timestamp"  timestamp
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp"
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetLatestBatteryTemperature => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryTemperature()
            RETURNS TABLE(
                "Battery_ID"                              bigint,
                "SlowChangingDataB_BatteryInternalTemperature" int,
                "EventCode"                               int
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Temperature"  AS "SlowChangingDataB_BatteryInternalTemperature",
                    l."EventCodeRaw" AS "EventCode"
                FROM "InternalBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetLatestBatteryEfficiency => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryEfficiency()
            RETURNS TABLE(
                "Battery_ID"                        bigint,
                "SlowChangingDataA_RemainingCapacity" double precision,
                "Remaining_Capacity"                double precision,
                "EventCode"                         int
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."RemainingCapacity" AS "SlowChangingDataA_RemainingCapacity",
                    l."RemainingCapacity" AS "Remaining_Capacity",
                    l."EventCodeRaw"      AS "EventCode"
                FROM "InternalBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetLatestBatteryStatus => """
            CREATE OR REPLACE FUNCTION usp_GetLatestBatteryStatus()
            RETURNS TABLE(
                "Battery_ID"          bigint,
                "Timestamp"           timestamp,
                "DeviceSerialNumber"  text,
                "OperatingData_Current" double precision,
                "BatteryStatus"       text
            )
            LANGUAGE sql STABLE AS $$
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp",
                    l."DeviceSerialNumber",
                    l."OperatingData_Current",
                    l."StatusText" AS "BatteryStatus"
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" IS NOT NULL
                ORDER BY l."Battery_ID", l."Timestamp" DESC;
            $$;
            """;

        public static string FnGetBatteryActivityHistory => """
            CREATE OR REPLACE FUNCTION usp_GetBatteryActivityHistory(
                p_community_id          bigint,
                p_group_id              bigint,
                p_include_all_subgroups boolean
            )
            RETURNS TABLE(
                "ActivityDate"          date,
                "TotalBatteriesInScope" int,
                "BatteriesOnline"       int,
                "BatteriesOffline"      int
            )
            LANGUAGE plpgsql STABLE AS $$
            DECLARE
                v_start date := CURRENT_DATE - 6;
                v_end   date := CURRENT_DATE;
            BEGIN
                RETURN QUERY
                WITH RECURSIVE subgroups AS (
                    SELECT g."ID" FROM "Groups" g WHERE g."ID" = p_group_id
                    UNION ALL
                    SELECT g."ID" FROM "Groups" g
                    JOIN subgroups s ON g."ParentGroup_ID" = s."ID"
                ),
                in_scope AS (
                    SELECT b."ID" AS bid
                    FROM "Batteries" b
                    WHERE NOT b."IsDeactivated"
                      AND (p_community_id IS NULL OR b."Community_ID" = p_community_id)
                      AND (
                          p_group_id IS NULL
                          OR (p_include_all_subgroups     AND b."Group_ID" IN (SELECT "ID" FROM subgroups))
                          OR (NOT p_include_all_subgroups AND b."Group_ID" = p_group_id)
                      )
                ),
                total AS (SELECT COUNT(*)::int AS n FROM in_scope),
                date_series AS (
                    SELECT d::date AS d
                    FROM generate_series(v_start::timestamp, v_end::timestamp, '1 day'::interval) AS d
                ),
                daily AS (
                    SELECT
                        l."Timestamp"::date                AS log_day,
                        COUNT(DISTINCT l."Battery_ID")::int AS online_cnt
                    FROM "AgentBatteryLogs" l
                    WHERE l."Battery_ID" IN (SELECT bid FROM in_scope)
                      AND l."Timestamp" >= v_start::timestamp
                      AND l."Timestamp" <  (v_end + 1)::timestamp
                    GROUP BY l."Timestamp"::date
                )
                SELECT
                    ds.d                                        AS "ActivityDate",
                    t.n                                         AS "TotalBatteriesInScope",
                    COALESCE(dl.online_cnt, 0)                  AS "BatteriesOnline",
                    (t.n - COALESCE(dl.online_cnt, 0))          AS "BatteriesOffline"
                FROM date_series ds
                CROSS JOIN total t
                LEFT JOIN daily dl ON dl.log_day = ds.d
                ORDER BY ds.d;
            END;
            $$;
            """;

        public static string FnGetDeviceActivityHistory => """
            CREATE OR REPLACE FUNCTION usp_GetDeviceActivityHistory(
                p_community_id          bigint,
                p_group_id              bigint,
                p_include_all_subgroups boolean
            )
            RETURNS TABLE(
                "ActivityDate"        date,
                "TotalDevicesInScope" int,
                "DevicesOnline"       int,
                "DevicesOffline"      int
            )
            LANGUAGE plpgsql STABLE AS $$
            DECLARE
                v_start date := CURRENT_DATE - 6;
                v_end   date := CURRENT_DATE;
            BEGIN
                RETURN QUERY
                WITH RECURSIVE subgroups AS (
                    SELECT g."ID" FROM "Groups" g WHERE g."ID" = p_group_id
                    UNION ALL
                    SELECT g."ID" FROM "Groups" g
                    JOIN subgroups s ON g."ParentGroup_ID" = s."ID"
                ),
                in_scope AS (
                    SELECT d."ID" AS did
                    FROM "Devices" d
                    WHERE NOT d."IsDeactivated"
                      AND (p_community_id IS NULL OR d."Community_ID" = p_community_id)
                      AND (
                          p_group_id IS NULL
                          OR (p_include_all_subgroups     AND d."Group_ID" IN (SELECT "ID" FROM subgroups))
                          OR (NOT p_include_all_subgroups AND d."Group_ID" = p_group_id)
                      )
                ),
                total AS (SELECT COUNT(*)::int AS n FROM in_scope),
                date_series AS (
                    SELECT d::date AS d
                    FROM generate_series(v_start::timestamp, v_end::timestamp, '1 day'::interval) AS d
                ),
                daily AS (
                    SELECT
                        l."Timestamp"::date               AS log_day,
                        COUNT(DISTINCT l."Device_ID")::int AS online_cnt
                    FROM "AgentDeviceLogs" l
                    WHERE l."Device_ID" IN (SELECT did FROM in_scope)
                      AND l."Timestamp" >= v_start::timestamp
                      AND l."Timestamp" <  (v_end + 1)::timestamp
                    GROUP BY l."Timestamp"::date
                )
                SELECT
                    ds.d                                        AS "ActivityDate",
                    t.n                                         AS "TotalDevicesInScope",
                    COALESCE(dl.online_cnt, 0)                  AS "DevicesOnline",
                    (t.n - COALESCE(dl.online_cnt, 0))          AS "DevicesOffline"
                FROM date_series ds
                CROSS JOIN total t
                LEFT JOIN daily dl ON dl.log_day = ds.d
                ORDER BY ds.d;
            END;
            $$;
            """;

        public static string FnPopupDashboard => """
            CREATE OR REPLACE FUNCTION sp_popupDashboard(p_battery_ids bigint[])
            RETURNS TABLE(
                "Battery_ID"    bigint,
                "PowerModules"  text,
                "AttachedTo"    text,
                "DeviceType"    text,
                "SoC"           int,
                "CycleCount"    int,
                "Temperature"   int,
                "Status"        text,
                "LastAttached"  text,
                "LastCharged"   timestamp,
                "LastDischarged" timestamp
            )
            LANGUAGE sql STABLE AS $$
            WITH
            latest_agent AS (
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."SlowChangingDataA_RelativeStateOfCharge" AS soc,
                    l."OperatingData_CycleCount"                AS cycle_count,
                    l."StatusText"                             AS status,
                    l."DeviceSerialNumber"                     AS device_serial
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" = ANY(p_battery_ids)
                ORDER BY l."Battery_ID", l."Timestamp" DESC
            ),
            latest_temp AS (
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Temperature" AS temp
                FROM "InternalBatteryLogs" l
                WHERE l."Battery_ID" = ANY(p_battery_ids)
                ORDER BY l."Battery_ID", l."Timestamp" DESC
            ),
            last_charged AS (
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp" AS charged_at
                FROM "InternalBatteryLogs" l
                WHERE l."Battery_ID" = ANY(p_battery_ids)
                  AND l."EventCodeRaw" = 3
                ORDER BY l."Battery_ID", l."Timestamp" DESC
            ),
            last_discharged AS (
                SELECT DISTINCT ON (l."Battery_ID")
                    l."Battery_ID",
                    l."Timestamp" AS discharged_at
                FROM "InternalBatteryLogs" l
                WHERE l."Battery_ID" = ANY(p_battery_ids)
                  AND l."EventCodeRaw" = 4
                ORDER BY l."Battery_ID", l."Timestamp" DESC
            )
            SELECT
                b."ID"                                              AS "Battery_ID",
                COALESCE(b."SerialNumber", b."ID"::text)            AS "PowerModules",
                COALESCE(la.device_serial, '-')                     AS "AttachedTo",
                CASE d."DeviceType" WHEN 0 THEN 'Charger' WHEN 1 THEN 'Dock' ELSE '' END AS "DeviceType",
                la.soc                                              AS "SoC",
                la.cycle_count                                      AS "CycleCount",
                NULLIF(lt.temp, 0)                                  AS "Temperature",
                COALESCE(la.status, '')                             AS "Status",
                COALESCE(la.device_serial, '-')                     AS "LastAttached",
                lc.charged_at                                       AS "LastCharged",
                ld.discharged_at                                    AS "LastDischarged"
            FROM "Batteries" b
            LEFT JOIN latest_agent   la ON la."Battery_ID" = b."ID"
            LEFT JOIN latest_temp    lt ON lt."Battery_ID" = b."ID"
            LEFT JOIN last_charged   lc ON lc."Battery_ID" = b."ID"
            LEFT JOIN last_discharged ld ON ld."Battery_ID" = b."ID"
            LEFT JOIN "Devices"       d  ON d."SerialNumber" = la.device_serial
            WHERE b."ID" = ANY(p_battery_ids)
            ORDER BY b."SerialNumber";
            $$;
            """;

        // ── Device / Battery list functions (called from list pages) ─────────
        // These are recreated on every startup via DROP + CREATE so that any
        // signature change (added/removed RETURNS TABLE columns) is applied
        // without requiring a manual psql DROP.

        public static string FnGetDevicesList => """
            DROP FUNCTION IF EXISTS get_devices_list(bigint);
            CREATE OR REPLACE FUNCTION get_devices_list(p_community_id bigint)
            RETURNS TABLE(
                device_id           bigint,
                serial_number       text,
                community_id        bigint,
                community_name      text,
                group_id            bigint,
                group_name          text,
                first_seen_on       timestamp,
                last_seen_on        timestamp,
                temperature         int,
                device_capacity     int,
                ext_power_applied   boolean,
                power_out_voltage   double precision,
                power_out_current   double precision,
                bays                int
            )
            LANGUAGE sql STABLE AS $$
            SELECT
                d."ID"                                          AS device_id,
                d."SerialNumber"::text                          AS serial_number,
                d."Community_ID"                                AS community_id,
                c."Name"::text                                  AS community_name,
                d."Group_ID"                                    AS group_id,
                g."Name"::text                                  AS group_name,
                agg.first_ts                                    AS first_seen_on,
                ll."Timestamp"                                  AS last_seen_on,
                COALESCE(ll."Status_Temperature", 0)            AS temperature,
                COALESCE(ll."DeviceCapacity", 0)                AS device_capacity,
                ll."IsExternalPowerInputApplied"                AS ext_power_applied,
                COALESCE(ll."PowerOutput_Voltage", 0.0)         AS power_out_voltage,
                COALESCE(ll."PowerOutput_Current", 0.0)         AS power_out_current,
                COALESCE(ls."Bays", 0)                          AS bays
            FROM "Devices" d
            LEFT JOIN "Communities" c  ON c."ID" = d."Community_ID"
            LEFT JOIN "Groups"      g  ON g."ID" = d."Group_ID"
            LEFT JOIN LATERAL (
                SELECT MIN(l."Timestamp") AS first_ts
                FROM "AgentDeviceLogs" l
                WHERE l."Device_ID" = d."ID"
            ) agg ON true
            LEFT JOIN LATERAL (
                SELECT l."Timestamp",
                       l."Status_Temperature",
                       l."DeviceCapacity",
                       l."IsExternalPowerInputApplied",
                       l."PowerOutput_Voltage",
                       l."PowerOutput_Current"
                FROM "AgentDeviceLogs" l
                WHERE l."Device_ID" = d."ID"
                ORDER BY l."Timestamp" DESC
                LIMIT 1
            ) ll ON true
            LEFT JOIN LATERAL (
                SELECT s."Bays"
                FROM "DeviceSettings" s
                WHERE s."Device_ID" = d."ID"
                ORDER BY s."Timestamp" DESC
                LIMIT 1
            ) ls ON true
            WHERE d."IsDeactivated" = false
              AND (p_community_id IS NULL OR d."Community_ID" = p_community_id)
            $$;
            """;

        public static string FnGetBatteriesList => """
            DROP FUNCTION IF EXISTS get_batteries_list(bigint);
            CREATE OR REPLACE FUNCTION get_batteries_list(p_community_id bigint)
            RETURNS TABLE(
                battery_id          bigint,
                serial_number       text,
                serial_number_code  bigint,
                community_id        bigint,
                community_name      text,
                group_id            bigint,
                group_name          text,
                first_seen_on       timestamp,
                last_seen_on        timestamp,
                relative_soc        int,
                internal_temp       int,
                operating_current   double precision
            )
            LANGUAGE sql STABLE AS $$
            SELECT
                b."ID"                                                          AS battery_id,
                b."SerialNumber"::text                                          AS serial_number,
                b."SerialNumberCode"                                            AS serial_number_code,
                b."Community_ID"                                                AS community_id,
                c."Name"::text                                                  AS community_name,
                b."Group_ID"                                                    AS group_id,
                g."Name"::text                                                  AS group_name,
                agg.first_ts                                                    AS first_seen_on,
                ll."Timestamp"                                                  AS last_seen_on,
                COALESCE(ll."SlowChangingDataA_RelativeStateOfCharge", 0)       AS relative_soc,
                COALESCE(ll."SlowChangingDataB_BatteryInternalTemperature", 0)  AS internal_temp,
                COALESCE(ll."OperatingData_Current", 0.0)                       AS operating_current
            FROM "Batteries" b
            LEFT JOIN "Communities" c  ON c."ID" = b."Community_ID"
            LEFT JOIN "Groups"      g  ON g."ID" = b."Group_ID"
            LEFT JOIN LATERAL (
                SELECT MIN(l."Timestamp") AS first_ts
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" = b."ID"
            ) agg ON true
            LEFT JOIN LATERAL (
                SELECT l."Timestamp",
                       l."SlowChangingDataA_RelativeStateOfCharge",
                       l."SlowChangingDataB_BatteryInternalTemperature",
                       l."OperatingData_Current"
                FROM "AgentBatteryLogs" l
                WHERE l."Battery_ID" = b."ID"
                ORDER BY l."Timestamp" DESC
                LIMIT 1
            ) ll ON true
            WHERE b."IsDeactivated" = false
              AND (p_community_id IS NULL OR b."Community_ID" = p_community_id)
            $$;
            """;
    }
}
