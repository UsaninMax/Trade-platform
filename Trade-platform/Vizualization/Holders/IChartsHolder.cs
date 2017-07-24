﻿using System.Collections.Generic;
using TradePlatform.Vizualization.ViewModels;

namespace TradePlatform.Vizualization.Holders
{
    public interface IChartsHolder
    {
        void Set(IDictionary<string, IChartViewModel> map);
        IChartViewModel Get(string key);
        bool Exist(string key);
    }
}
