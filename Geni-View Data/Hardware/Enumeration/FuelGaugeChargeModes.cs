using System.ComponentModel;

namespace GeniView.Data.Hardware
{
    public enum FuelGaugeChargeModes
    {
        Unknown = 0x40000000,

        [Description("Normal")]
        Normal = 0,
        [Description("UPS Mode 1")]
        UPSModeOne = 0x1,

        //TODO: Fix this, looks like spec has problem.
        [Description("UPS Mode 2")]
        UpsModeTwo = 0x2,
    }
}
