﻿using System;
using System.Collections.Generic;
using TradePlatform.Sandbox.Models;
using TradePlatform.Sandbox.Transactios.Models;

namespace TradePlatform.Sandbox.Bots
{
    public interface IBot
    {
        string GetId();
        void SetUpId(string id);
        void SetUpData(IList<Tuple<DateTime, IEnumerable<IData>, IEnumerable<Tick>>> data);
        void SetUpPredicate(BotPredicate predicate);
        void Execute();
        void Execution(IEnumerable<IData> slice);
        int Score();
        void SetUpCosts(IEnumerable<BrokerCost> value);
        void SetUpBalance(double value);
        void OpenPosition(ImmediatePositionRequest request);
        Guid OpenPosition(PostponedPositionRequest request);
    }
}
