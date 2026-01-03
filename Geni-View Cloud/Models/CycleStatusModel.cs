using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Models
{
    public class CycleStatusModel
    {
        // Counts per category
        public int LowCount { get; set; }
        public int HighCount { get; set; }
        public int EndOfLifeCount { get; set; }

        // Displayed under the title
        public int PowerModulesCount { get; set; }

        public int TotalCount { get; set; }

        // Percentages for the stacked bar
        public decimal LowPercent { get; set; }
        public decimal HighPercent { get; set; }
        public decimal EndOfLifePercent { get; set; }

        // Average of the three category counts
        public int AverageCycleCount { get; set; }
    }
}