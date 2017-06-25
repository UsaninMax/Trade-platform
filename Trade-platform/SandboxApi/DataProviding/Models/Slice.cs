﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TradePlatform.SandboxApi.DataProviding.Models
{
    public class Slice
    {
        public DateTime @DateTime { get; private set; }
        public IDictionary<string, Tick> Ticks { get; private set; }
        public IDictionary<string, Candle> Candles { get; private set; }
        public IDictionary<string, Indicator> Indicators { get; private set; }

        private Slice()
        {
        }

        public class Builder
        {
            private DateTime _dateTime;
            private IDictionary<string, Tick> _ticks;
            private IDictionary<string, Candle> _candles;
            private IDictionary<string, Indicator> _indicators;

            public Builder WithDate(DateTime value)
            {
                _dateTime = value;
                return this;
            }

            public Builder WithTicks(ICollection<Tick> values)
            {
                _ticks = values.ToDictionary(x => x.Name, y => y);
                return this;
            }

            public Builder WithCandles(ICollection<Candle> values)
            {
                _candles = values.ToDictionary(x => x.Name, y => y);
                return this;
            }

            public Builder WithIndicators(ICollection<Indicator> values)
            {
                _indicators = values.ToDictionary(x => x.Name, y => y);
                return this;
            }

            public Slice Build()
            {
                return new Slice()
                {
                    @DateTime = _dateTime,
                    Ticks = _ticks,
                    Candles = _candles,
                    Indicators = _indicators
                };
            }

            public override string ToString()
            {
                return $"{nameof(_dateTime)}: {_dateTime}," +
                       $" {nameof(_ticks)}: {_ticks}," +
                       $" {nameof(_candles)}: {_candles}," +
                       $" {nameof(_indicators)}: {_indicators}";
            }
        }
    }
}