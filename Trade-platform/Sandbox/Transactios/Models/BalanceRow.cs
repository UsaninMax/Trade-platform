﻿using System;

namespace TradePlatform.Sandbox.Transactios.Models
{
    public class BalanceRow
    {
        public DateTime Date => _date;
        private DateTime _date;
        public double Coverage => _coverage;
        private double _coverage;
        public double TransactionCost => _transactionCost;
        private double _transactionCost;
        public double TransactionMargin => _transactionMargin;
        private double _transactionMargin;
        public double Total => _total;
        private double _total;

        private BalanceRow()
        {
        }

        public class Builder
        {
            private double _coverage;
            private double _transactionCost;
            private double _transactionMargin;
            private double _total;

            public Builder Coverage(double value)
            {
                _coverage = value;
                return this;
            }

            public Builder TransactionCost(double value)
            {
                _transactionCost = value;
                return this;
            }

            public Builder TransactionMargin(double value)
            {
                _transactionMargin = value;
                return this;
            }

            public Builder Total(double value)
            {
                _total = value;
                return this;
            }

            public BalanceRow Build()
            {
                return new BalanceRow()
                {
                    _date = DateTime.Now,
                    _coverage = _coverage,
                    _transactionCost = _transactionCost,
                    _transactionMargin = _transactionMargin,
                    _total = _total
                };
            }
        }

        public override string ToString()
        {
            return $"{nameof(Date)}: {Date}," +
                   $" {nameof(Coverage)}: {Coverage}," +
                   $" {nameof(TransactionCost)}: {TransactionCost}," +
                   $" {nameof(TransactionMargin)}: {TransactionMargin}, " +
                   $"{nameof(Total)}: {Total}";
        }
    }
}
