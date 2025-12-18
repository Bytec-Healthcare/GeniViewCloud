using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace GeniView.Cloud.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDeviceDataPublishService" in both code and config file together.
    [ServiceContract]
    public interface IDeviceDataPublishService
    {
        [OperationContract]
        PublishedAgentDataInformation GetPublishedAgentDeviceLogsInformation(string serialNumber, DateTime fromDate, DateTime toDate);

        [OperationContract]
        PublishedAgentDataInformation GetPublishedAgentBatteryLogsInformation(long serialNumber, DateTime fromDate, DateTime toDate);

        [OperationContract]
        PublishedSettingsInformation GetPublishedDeviceSettingsInformation(string serialNumber, DateTime fromDate, DateTime toDate);

        [OperationContract]
        PublishedSettingsInformation GetPublishedBatterySettingsInformation(long serialNumber, DateTime fromDate, DateTime toDate);

        [OperationContract]
        PublishedInternalLogsInformation GetPublishedInternalDeviceLogsInformation(string serialNumber, long fromLogIndex, long toLogIndex);

        [OperationContract]
        PublishedInternalLogsInformation GetPublishedInternalBatteryLogsInformation(long serialNumber, long fromLogIndex, long toLogIndex);

        [OperationContract]
        PublishedEventsInformation GetPublishedDeviceEventsInformation(string serialNumber, DateTime fromDate, DateTime toDate);

        [OperationContract]
        IEnumerable<DateTime> GetMissingAgentDeviceLogDates(string serialNumber, IEnumerable<DateTime> availableDataDates);

        [OperationContract]
        IEnumerable<DateTime> GetMissingAgentBatteryLogDates(long serialNumber, IEnumerable<DateTime> availableDataDates);

        [OperationContract]
        IEnumerable<DateTime> GetMissingDeviceSettingsDates(string serialNumber, IEnumerable<DateTime> availableSettingsDates);

        [OperationContract]
        IEnumerable<DateTime> GetMissingBatterySettingsDates(long serialNumber, IEnumerable<DateTime> availableSettingsDates);

        [OperationContract]
        IEnumerable<long> GetMissingInternalDeviceLogIndices(string serialNumber, IEnumerable<long> availableLogIndices);

        [OperationContract]
        IEnumerable<string> GetMissingInternalBatteryLogUniqueIndices(long serialNumberCode, IEnumerable<string> availableUniqueLogIndices);

        [OperationContract]
        IEnumerable<DateTime> GetMissingDeviceEventDates(string serialNumber, IEnumerable<DateTime> availableEventDates);

        [OperationContract]
        void PublishDeviceData(Device device, IEnumerable<Battery> batteries, Agent agent);

        [OperationContract]
        void PublishBatteryData(Battery battery, Agent agent);
    }

    public class PublishedAgentDataInformation
    {
        public int Count { get; set; }

        public DateTime? LatestEntryDate { get; set; }
    }

    public class PublishedSettingsInformation : PublishedAgentDataInformation
    { }

    public class PublishedEventsInformation : PublishedAgentDataInformation
    { }

    public class PublishedInternalLogsInformation
    {
        public int Count { get; set; }

        public long? LatestEntryIndex { get; set; }
    }
}
