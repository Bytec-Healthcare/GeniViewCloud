if exists(select 1 from sys.views where name = 'AgentBatteryLogsWithDuration' and type = 'v')
 drop view AgentBatteryLogsWithDuration

 exec('
		Create view AgentBatteryLogsWithDuration as
		select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					end as Duration, *
				from (
					select (
						Select MAX(Timestamp)maxTimestamp
							from dbo.AgentBatteryLogs t2
						where t2.Battery_ID = t1.battery_id
							and t2.Timestamp < t1.Timestamp
							) prevRowTimeStamp
							, ROW_NUMBER ( )  OVER (PARTITION BY battery_id, bay order by timestamp)  rk
							, * 
			from dbo.AgentBatteryLogs  t1
		)Duration
  	  ');