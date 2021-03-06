﻿using System;

namespace TradePlatform.SandboxApi.Models
{
    public class Candle : IData
    {
        private DateTime _date;
        private string _id;
        public double High { get; private set; }
        public double Low { get; private set; }
        public double Open { get; private set; }
        public double Close { get; private set; }
        public double Volume { get; private set; }

        private Candle() { }

        public class Builder
        {
            private DateTime _date;
            private string _id;
            private double _high;
            private double _low;
            private double _open;
            private double _close;
            private double _volume;

            public Builder WithDate(DateTime value)
            {
                _date = value;
                return this;
            }

            public Builder WithId(string value)
            {
                _id = value;
                return this;
            }

            public Builder WithHigh(double value)
            {
                _high = value;
                return this;
            }

            public Builder WithLow(double value)
            {
                _low = value;
                return this;
            }

            public Builder WithOpen(double value)
            {
                _open = value;
                return this;
            }

            public Builder WithClose(double value)
            {
                _close = value;
                return this;
            }

            public Builder WithVolume(double value)
            {
                _volume = value;
                return this;
            }


            public Candle Build()
            {
                return new Candle()
                {
                    _date = _date,
                    _id = _id,
                    High = _high,
                    Low = _low,
                    Open = _open,
                    Close = _close,
                    Volume = _volume
                };
            }
        }

        public override string ToString()
        {
            return
                $" {nameof(_date)}: {_date}," +
                $"{nameof(_id)}: {_id}," +
                $" {nameof(High)}: {High}," +
                $" {nameof(Low)}: {Low}," +
                $" {nameof(Open)}: {Open}," +
                $" {nameof(Close)}: {Close}," +
                $" {nameof(Volume)}: {Volume}";
        }

        public DateTime Date()
        {
            return _date;
        }

        public string Id()
        {
            return _id;
        }
    }
}
