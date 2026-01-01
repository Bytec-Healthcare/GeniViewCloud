using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Models
{
    public class CycleStatusModel
    {
        public int ActiveCount { get; set; }
        public int IdleCount { get; set; }
        public int SvcCount { get; set; }

        public int TotalCount { get; set; }

        public decimal ActivePercent { get; set; }
        public decimal IdlePercent { get; set; }
        public decimal SvcPercent { get; set; }
    }
}