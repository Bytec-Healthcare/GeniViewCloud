namespace GeniView.Cloud.Models
{
    public sealed class EffectiveRotationModel
    {
        public int GoodCount { get; set; }
        public int AverageCount { get; set; }
        public int PoorCount { get; set; }

        public int PowerModulesCount { get; set; }
        public int TotalCount { get; set; }

        public decimal GoodPercent { get; set; }
        public decimal AveragePercent { get; set; }
        public decimal PoorPercent { get; set; }

        public int EfficiencyScorePercent { get; set; }
    }
}
