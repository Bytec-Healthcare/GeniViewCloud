namespace GeniView.Cloud.Models
{
    public sealed class BatteryEfficiencyModel
    {
        public int PowerModulesCount { get; set; }

        public decimal InUseRemainingCapacitySum { get; set; }
        public decimal TotalRemainingCapacitySum { get; set; }

        public int EfficiencyScorePercent { get; set; }

        public decimal InUsePercent { get; set; }
        public decimal IdlePercent { get; set; }
    }
}
