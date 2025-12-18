if exists(select 1 from sys.views where name = 'AgentDeviceLogsWithDuration' and type = 'v')
 drop view AgentDeviceLogsWithDuration
	 exec('
		Create view AgentDeviceLogsWithDuration as
		select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					end as Duration, *
			 from (
				 select (
						Select MAX(Timestamp)maxTimestamp
						  from dbo.AgentDeviceLogs t2
						where t2.device_id = t1.device_id
							and t2.Timestamp < t1.Timestamp
							) prevRowTimeStamp
							, ROW_NUMBER ( )  OVER (PARTITION BY device_id order by timestamp)  rk
							, * 
		  from dbo.AgentDeviceLogs  t1
		)Duration
	 ');