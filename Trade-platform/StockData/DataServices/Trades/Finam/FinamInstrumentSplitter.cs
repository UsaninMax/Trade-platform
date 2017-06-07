﻿using System;
using System.Collections.Generic;
using TradePlatform.StockData.Models;

namespace TradePlatform.StockData.DataServices.Trades.Finam
{
    public class FinamInstrumentSplitter : IInstrumentSplitter
    {
        public IEnumerable<Instrument> Split(Instrument instrument)
        {
            IList<Instrument> instruments = new List<Instrument>();
            for (DateTime date = instrument.From; date <= instrument.To; date = date.AddDays(1))
            {
                instruments.Add(new Instrument.Builder()
                    .WithFrom(date)
                    .WithTo(date)
                    .WithParent(instrument)
                    .Build());
            }
            return instruments;
        }
    }
}