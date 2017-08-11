﻿using System.Collections.Generic;

namespace TradePlatform.Charts.Vizualization.Configurations
{
    public abstract class ChartViewPredicate
    {
        public IEnumerable<string> Ids { get; set; } = new List<string>();
        public int YSize { get; set; } = 600;
    }
}
