using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Models
{
    public class DeviceEventNotification
    {
        public DeviceEventNotification()
        {

        }
        internal DeviceEventNotification(string description, Guid uid, bool sendNotifications)
        {
            Description = description;
            UID = uid;
            SendNotifications = sendNotifications;
        }
        public long ID { get; set; }
        public Guid UID { get; set; }
        public string Description { get; set; }
        public bool SendNotifications { get; set; }

        /// <summary>
        /// Used to seed database with pre-defined device events.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DeviceEventNotification> Seed()
        {
            List<DeviceEventNotification> model = new List<DeviceEventNotification>();

            #region DeviceLog
            model.Add(new DeviceEventNotification("Battery Attached",
                new Guid("{30DF2195-2AF9-4766-B2EB-3A7950BA3584}"),
                false));

            model.Add(new DeviceEventNotification("Battery Removed",
                new Guid("{337893EB-B8C0-4E10-974A-16EEBDF92A2B}"),
                false));

            model.Add(new DeviceEventNotification("External Power Input Applied",
               new Guid("{B4E22FB3-A4C4-45BD-A80E-17A268659A85}"),
               false));

            model.Add(new DeviceEventNotification("External Power Input Removed",
                new Guid("{12D74D9D-0EA0-42EC-B19F-F15F8CA35ABF}"),
                false));

            model.Add(new DeviceEventNotification("Standby Mode Activated",
                new Guid("{1A1E26D4-267A-4576-9C37-FE1D77949B8F}"),
                false));

            model.Add(new DeviceEventNotification("Standby Mode Deactivated",
                new Guid("{74688061-D7EA-4BA6-A3B8-A24A614863AB}"),
                false));

            model.Add(new DeviceEventNotification("Low Battery Warning",
               new Guid("{FC9C06CD-5B49-4761-90A2-4A86150FDBA0}"),
               false));

            model.Add(new DeviceEventNotification("Battery Authentication Failure",
               new Guid("{E838C770-AEA9-400B-BB37-321CAD14FFCE}"),
               false));

            model.Add(new DeviceEventNotification("Load Present",
               new Guid("{7F9E2EBA-05AE-4A34-B6B1-27977E2C5C30}"),
               false));

            model.Add(new DeviceEventNotification("Load Removed",
               new Guid("{B38D5D53-70E1-453F-9BA0-D1099CE3218A}"),
               false));

            model.Add(new DeviceEventNotification("Overload Warning",
                new Guid("{F8D0E91C-2150-48E0-82A4-CD21EF272130}"),
                false));

            model.Add(new DeviceEventNotification("Low Temperature Warning",
                new Guid("{81B6E8F6-D2A7-4E56-92EB-F82400B23AA9}"),
                false));

            model.Add(new DeviceEventNotification("High Temperature Warning",
                new Guid("{64C99D9F-F37E-458A-B1E3-8C73D574A903}"),
                false));

            model.Add(new DeviceEventNotification("SM Bus Communication Issue",
                new Guid("{C37937F0-9378-42D8-B3A9-D8C2758215AC}"),
                false));

            model.Add(new DeviceEventNotification("System Power-up",
                new Guid("{039A1FDE-8EFF-4BB6-B87B-F6340FB57A2E}"),
                false));

            model.Add(new DeviceEventNotification("System Power-down",
                new Guid("{7E862E40-BD1C-4A4F-8DC0-37E0F507EE0D}"),
                false));

            model.Add(new DeviceEventNotification("Automatic Power-down",
                new Guid("{1A4296CC-B977-43BD-910F-C53914FC68FF}"),
                false));

            model.Add(new DeviceEventNotification("Lost Comms with Fuel Gauge",
                new Guid("{1752A0DE-1DD8-46BF-8F51-156D2B361B7B}"),
                false));

            model.Add(new DeviceEventNotification("Factory Defaults Reset",
                new Guid("{17744954-240F-4E4A-A2B9-011F004F078E}"),
                false));

            model.Add(new DeviceEventNotification("Serial Number Updated",
                new Guid("{E000A3A2-D619-4475-B56A-FE5612D91860}"),
                false));

            model.Add(new DeviceEventNotification("Set Device to Firmware Mode",
                new Guid("{E36D8342-B648-47C4-BC9C-33A2D603708B}"),
                false));

            model.Add(new DeviceEventNotification("Sync Device Clock",
                new Guid("{69127B83-A6A2-49B2-A0A3-513B7B96D0C8}"),
                false));

            model.Add(new DeviceEventNotification("Battery 1 Switch OFF",
                new Guid("{541F52AB-C575-4576-AE7B-D647F2C81C11}"),
                false));

            model.Add(new DeviceEventNotification("Battery 2 Switch OFF",
                new Guid("{3B0A4528-E073-4677-9170-8F31EE8529D7}"),
                false));

            model.Add(new DeviceEventNotification("FET 1 Switch OFF",
                new Guid("{4E13901C-45B7-4157-ABBA-B3D7A82F878B}"),
                false));

            model.Add(new DeviceEventNotification("FET 2 Switch OFF",
                new Guid("{501D0BB1-F15F-4680-B5A5-48CBC1FEB65B}"),
                false));

            model.Add(new DeviceEventNotification("Service Indicator Reset",
                new Guid("{EEBBBA37-67FA-49E8-B3D0-8A1B7E78009A}"),
                false));

            model.Add(new DeviceEventNotification("OEM ID Check Failed",
                new Guid("{1A6833A3-ADD1-47FE-A7A3-8917F61FBC78}"),
                false));

            model.Add(new DeviceEventNotification("Battery 1 Service Indicator Reset",
               new Guid("{1E8DACFA-E5CA-4C02-9A28-610AF6E8C541}"),
               false));

            model.Add(new DeviceEventNotification("Battery 2 Service Indicator Reset",
                new Guid("{A806710D-B1E9-4D50-BBB0-8E85938BCC20}"),
                false));

            model.Add(new DeviceEventNotification("Battery 3 Service Indicator Reset",
                new Guid("{D64D1511-5E93-4C51-BAF4-E4C676239AB2}"),
                false));

            model.Add(new DeviceEventNotification("Battery 4 Service Indicator Reset",
                new Guid("{BE006E23-E7FF-4D11-8724-B76A29D88226}"),
                false));

            model.Add(new DeviceEventNotification("Battery 5 Service Indicator Reset",
                new Guid("{40FBCAD4-4955-49C6-AC5B-CDC8EC5EB2BB}"),
                false));
            #endregion

            #region DeviceData
            model.Add(new DeviceEventNotification("Battery 1 Failed Encryption",
                new Guid("{5EE641CF-A90E-4A4D-A674-2004B2330C5E}"),
                true));

            model.Add(new DeviceEventNotification("Battery 2 Failed Encryption",
                new Guid("{BA51396C-713F-4C72-9E34-821902C2954A}"),
                true));

            model.Add(new DeviceEventNotification("Battery 1 Over Temperature",
                new Guid("{3D902146-4CF4-48C5-A866-CD60EF724FE0}"),
                true));

            model.Add(new DeviceEventNotification("Battery 2 Over Temperature",
                new Guid("{8748D6FC-5398-426A-8833-8207DAFFEE43}"),
                true));

            model.Add(new DeviceEventNotification("Battery 1 Under Temperature",
                new Guid("{D335ABCF-2D9F-4DC9-A74A-40CFA6BE6BBB}"),
                true));

            model.Add(new DeviceEventNotification("Battery 2 Under Temperature",
                new Guid("{61589C6A-BF7C-4451-A1EA-5095EA336699}"),
                true));

            model.Add(new DeviceEventNotification("Battery 1 FET Failed",
                new Guid("{F5DF6384-6122-45E0-B52C-D5C09831269D}"),
                true));

            model.Add(new DeviceEventNotification("Battery 2 FET Failed",
                new Guid("{A4CDD5C9-E185-4571-BFD9-5F46AE238BAA}"),
                true));

            model.Add(new DeviceEventNotification("Short Circuit Protection Tripped",
                new Guid("{FB4C31C9-C80C-4E91-B69C-A24676A77EFC}"),
                true));

            model.Add(new DeviceEventNotification("LTC Overcurrent Protection Tripped",
                new Guid("{96488729-20BB-4B56-A6A7-6D9E563BA9B1}"),
                true));

            model.Add(new DeviceEventNotification("Firmware Overcurrent Detected",
                new Guid("{234F2890-3AA6-4FCF-97F5-C8CEAD62732E}"),
                true));

            model.Add(new DeviceEventNotification("Firmware Overcurrent Tripped",
                new Guid("{4562ADE9-740C-4AB5-A3E5-0ECD39D76E43}"),
                true));

            model.Add(new DeviceEventNotification("Battery 1 OEM Identification Incorrect",
                new Guid("{56D10889-B928-41FC-8C4F-CD1A48940F83}"),
                true));

            model.Add(new DeviceEventNotification("Battery 2 OEM Identification Incorrect",
                new Guid("{427E8889-240A-41B2-8643-AF49A6DD46F9}"),
                true));

            model.Add(new DeviceEventNotification("Device Power Level Low",
             new Guid("{44DD2CFF-CCF0-4DC5-A36B-3F2CD138A75B}"),
             true));

            model.Add(new DeviceEventNotification("Device Power Level Critical",
                new Guid("{1E8E3422-24F3-4064-A9EC-DE3C0EF6C063}"),
                true));

            #endregion

            return model;
        }
    }
}