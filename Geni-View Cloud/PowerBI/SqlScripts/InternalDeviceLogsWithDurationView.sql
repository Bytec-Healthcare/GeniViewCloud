if exists(select 1 from sys.views where name = 'InternalDeviceLogsWithDuration' and type = 'v')
 drop view InternalDeviceLogsWithDuration
	 exec('
		Create view InternalDeviceLogsWithDuration as
		select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					end as Duration, *
			 from (
				 select (
						Select MAX(Timestamp)maxTimestamp
						  from dbo.InternalDeviceLogs t2
						where t2.device_id = t1.device_id
							and t2.Timestamp < t1.Timestamp
							) prevRowTimeStamp
							, ROW_NUMBER ( )  OVER (PARTITION BY device_id order by timestamp)  rk
							, * 
		  from dbo.InternalDeviceLogs  t1
		)Duration
	 ');