﻿using System.Collections.Generic;
using System.Threading;
using TradePlatform.Sandbox.Bots;
using TradePlatform.Sandbox.DataProviding.Predicates;
using TradePlatform.Vizualization.Builders.Predicates;
using TradePlatform.Vizualization.Populating.Predicates;

namespace TradePlatform.Sandbox
{
    public interface ISandbox
    {
        void SetToken(CancellationToken token);
        ICollection<IPredicate> SetUpData();
        void SetUpBots(ICollection<IBot> bots);
        void BuildData();
        void Execution();
        void AfterExecution();
        void CleanMemory();

        IEnumerable<PanelViewPredicate> SetUpCharts();
        void CreateCharts();
        void PopulateCharts(ICollection<ChartPredicate> predicates);
        void StoreCustomData(string key, IList<object> data);
        void CleanCustomeStorage();
    }
}
