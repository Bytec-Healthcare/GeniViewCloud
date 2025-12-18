using Hangfire;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using GeniView.Cloud.Controllers;
using GeniView.Cloud.Repository;
using GeniView.Cloud.Controllers.API;
using NLog;

namespace GeniView.Cloud.Common
{
	/// <summary>
	/// Hang Fire scheduler
	/// </summary>
	public class HFScheduler
	{
		HangfireRepository _db = new HangfireRepository();
		private static Logger _logger = LogManager.GetCurrentClassLogger();

		MQTTMsgParser _MQTTMsgParser = new MQTTMsgParser();
		BatteriesController _batteriesController = new BatteriesController();
		LogApiController _logApiController = new LogApiController();



		public HFScheduler()
		{
			_db = new HangfireRepository();
		}

		public void Setting()
		{
			//Create CreatePatrolCheckInLogs job
			string every1Sec   = "*/1 * * * * *";
			string every5Sec   = "*/5 * * * * *";
			string every10Sec = "*/10 * * * * *";
			string every30Sec = "*/30 * * * * *";

			string every1Min   = "0 * * ? * *";
			string every2Min   = "*/2 * * * *";
			string every5Min = "*/5 * * * *";
			string every10Min = "*/10 * * * *";
			string every1Hour = "0 * * * *";
			string every12Hour = "0 */12 * * *";
			string every00Hour = "0 0 0 * * ?";//Every day at 00:00:00 clear and create jobs

			RemoveAllJobs();

			ClearAndCreateJobs(); //initialize

			//Release
			RecurringJob.AddOrUpdate(
				"ProcessLog",
				() => _logApiController.ProcessLogJob(CancellationToken.None),
				every1Sec
				);

			//RecurringJob.AddOrUpdate(
			//	"TestJob",
			//	() => _logApiController.TestQueueJob(),
			//	every5Sec
			//	);


			//Develop

			//RecurringJob.AddOrUpdate(
			//    "CheckDateTime",
			//             () => batteriesController.TestCheckDateTime(),
			//             every1Min
			//         );


			//RecurringJob.AddOrUpdate(
			//	"TestProcessJob",
			//	() => batteriesController.TestDequeue(),
			//	every1Sec
			//);
		}

        public string ClearAndCreateJobs()
		{
			StringBuilder sb = new StringBuilder();
			Stopwatch watch = Stopwatch.StartNew();

			try
			{

				_logger.Info($"HFScheduler ClearAndCreateJobs start");

				//_db.uspDrop_HangfireTemporaryTables();


				_logger.Info($"HFScheduler ClearAndCreateJobs Finish");
			}
			catch (Exception ex)
			{
				var msg = ex.ToString();
				_logger.Error($"HFScheduler ClearAndCreateJobs Exception:{msg}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Remove hang fire exist jobs
		/// </summary>
		private void RemoveAllJobs()
		{
            //Remoce any exit recurringJob
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }
        }

		public void TestDelay()
		{
			Thread.Sleep(30000);
		}

		public void GetAmount()
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var queues = monitoringApi.Queues();
			var toDelete = new List<string>();
		}

		public int ClearProcessJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.EnqueuedCount(queueName);

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				//var jobs = monitoringApi.ProcessingJobs(count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobs = monitoringApi.ProcessingJobs(count, page);

				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;

				foreach (var jobId in jobIds)
				{
					BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}

		public int ClearEnQueueJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.EnqueuedCount(queueName);

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				//var jobs = monitoringApi.EnqueuedJobs("default", count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobs = monitoringApi.EnqueuedJobs("default", count, page);

				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;
				foreach (var jobId in jobIds)
				{
					BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}

		public int ClearDeleteJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.DeletedListCount();

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				var jobs = monitoringApi.DeletedJobs(count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;
				foreach (var jobId in jobIds)
				{
					var result = BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}



	}
}