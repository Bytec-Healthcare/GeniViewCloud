if exists(select 1 from sys.views where name = 'InternalBatteryLogsWithDuration' and type = 'v')
 drop view InternalBatteryLogsWithDuration
	 exec('
		Create view InternalBatteryLogsWithDuration as
		select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					end as Duration, * 
			 from (
				 select (
						Select MAX(Timestamp)maxTimestamp
						  from dbo.InternalBatteryLogs t2
						where t2.Battery_ID = t1.battery_id
							and t2.Timestamp < t1.Timestamp
							) prevRowTimeStamp
							, ROW_NUMBER ( )  OVER (PARTITION BY battery_id, bay order by timestamp)  rk
							, * 
		  from dbo.InternalBatteryLogs  t1
		)Duration
	 ');