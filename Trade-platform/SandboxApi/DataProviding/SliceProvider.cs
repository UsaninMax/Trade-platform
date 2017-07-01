﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using TradePlatform.SandboxApi.DataProviding.Checks;
using TradePlatform.SandboxApi.DataProviding.Predicates;
using TradePlatform.SandboxApi.Models;
using Microsoft.Practices.Unity;
using TradePlatform.Commons.Info;
using TradePlatform.Commons.Info.Model.Message;
using TradePlatform.DataSet.DataServices;
using TradePlatform.SandboxApi.DataProviding.Transformers;

namespace TradePlatform.SandboxApi.DataProviding
{
    public class SliceProvider : ISliceProvider
    {
        private readonly IDataSetService _dataSetService;
        private readonly IPredicateChecker _predicateChecker;
        private readonly IInfoPublisher _infoPublisher;
        private IEnumerable<IData> _data = new List<IData>();
        private readonly ICollection<DataPredicate> _dataPredicates = new List<DataPredicate>();
        private readonly ICollection<IndicatorPredicate> _indicatorPredicates = new List<IndicatorPredicate>();
        private readonly ICollection<TickPredicate> _tickPredicate = new List<TickPredicate>();

        public SliceProvider()
        {
            _predicateChecker = ContainerBuilder.Container.Resolve<IPredicateChecker>();
            _infoPublisher = ContainerBuilder.Container.Resolve<IInfoPublisher>();
            _dataSetService = ContainerBuilder.Container.Resolve<IDataSetService>();
        }

        public IList<Slice> Get(ICollection<IPredicate> predicates)
        {
            GatherPredicates(predicates);
            CheckPredicateStructure();
            GatherTickPredicates();

            _infoPublisher.PublishInfo(new SandboxInfo { Message = " constract data series " });
            _dataPredicates.ForEach(ConstructSeries);
            _infoPublisher.PublishInfo(new SandboxInfo { Message = " constract indicator data " });
            _indicatorPredicates.ForEach(ConstructSeries);
            _infoPublisher.PublishInfo(new SandboxInfo { Message = " add tick data for support calculation " });
            _tickPredicate.ForEach(ConstructSeries);
            _infoPublisher.PublishInfo(new SandboxInfo { Message = " constract slices of data " });
            return GroupSlices();
        }

        private IList<Slice> GroupSlices()
        {
            IList<Slice> slices = new List<Slice>();
            List<IData> asList = _data.ToList();
            asList.Sort((x, y) => DateTime.Compare(x.Date(), y.Date()));
            var results = _data.GroupBy(p => p.Date(), p => p, (key, g) => new {Date = key, Data = g.ToList()});
            results.ForEach(x =>
            {
                Slice.Builder builder = new Slice.Builder();
                builder.WithDate(x.Date);
                x.Data.ForEach(y =>
                {
                    var candle = y as Candle;
                    if (candle != null)
                    {
                        builder.WithCandle(candle);
                        builder.WithBotUsage();
                    }

                    var indicator = y as Indicator;
                    if (indicator != null)
                    {
                        builder.WithIndicator(indicator);
                        builder.WithBotUsage();
                    }

                    var tick = y as Tick;
                    if (tick != null)
                    {
                        builder.WithTick(tick);
                    }
                });
                slices.Add(builder.Build());
            });

            _data = new List<IData>();
            return slices;
        }

        private void ConstructSeries(DataPredicate predicate)
        {
            ITransformer dataAggregator = ContainerBuilder.Container.Resolve<ITransformer>();
            _data = _data.Concat(dataAggregator.Transform(_dataSetService.Get(predicate.ParentId)
                .Where(m => predicate.From == DateTime.MinValue || m.Date >= predicate.From)
                .Where(m => predicate.To == DateTime.MinValue || m.Date <= predicate.To)
                .ToList(), predicate));
        }

        private void ConstructSeries(TickPredicate predicate)
        {
            ITransformer dataAggregator = ContainerBuilder.Container.Resolve<ITransformer>();
            _data = _data.Concat(dataAggregator.Transform(_dataSetService.Get(predicate.Id)
                .Where(m => predicate.From == DateTime.MinValue || m.Date >= predicate.From)
                .Where(m => predicate.To == DateTime.MinValue || m.Date <= predicate.To)
                .ToList(), predicate));
        }

        private void ConstructSeries(IndicatorPredicate predicate)
        {
            IIndicatorProvider provider = Activator.CreateInstance(predicate.Indicator) as IIndicatorProvider;
            if (provider == null)
            {
                _infoPublisher.PublishInfo(new SandboxInfo { Message = predicate.Indicator + " cannot be instantiated " });
                return;
            }
            ITransformer dataAggregator = ContainerBuilder.Container.Resolve<ITransformer>();
            List<Candle> candles = dataAggregator.Transform(_dataSetService.Get(predicate.DataPredicate.ParentId)
                .Where(m => predicate.DataPredicate.From == DateTime.MinValue || m.Date >= predicate.DataPredicate.From)
                .Where(m => predicate.DataPredicate.To == DateTime.MinValue || m.Date <= predicate.DataPredicate.To)
                .ToList(), predicate.DataPredicate).ToList();
            _data = _data.Concat(provider.Get(candles).Select(c => { c.Id = predicate.Id; return c; }).ToList());
        }

        private void GatherTickPredicates()
        {
            ISet<string> parentIds = new HashSet<string>();
            _dataPredicates.ForEach(x => parentIds.Add(x.ParentId));
            _indicatorPredicates.ForEach(x => parentIds.Add(x.DataPredicate.ParentId));
            parentIds.ForEach(x => _tickPredicate.Add(ConstractTickPredicate(x)));
        }

        private TickPredicate ConstractTickPredicate(string parentId)
        {
            var predicate = new TickPredicate.Builder().NewId(parentId);

            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MaxValue;

            _dataPredicates.Where(x => x.ParentId.Equals(parentId)).ForEach(x =>
            {
                from = new DateTime(Math.Max(from.Ticks, x.From.Ticks));
                to = new DateTime(Math.Min(to.Ticks, x.To.Ticks));
            });

            _indicatorPredicates.Where(x => x.DataPredicate.ParentId.Equals(parentId)).ForEach(x =>
            {
                from = new DateTime(Math.Max(from.Ticks, x.DataPredicate.From.Ticks));
                to = new DateTime(Math.Min(to.Ticks, x.DataPredicate.To.Ticks));
            });

            return predicate.From(from).To(to).Build();
        }

        private void GatherPredicates(ICollection<IPredicate> predicates)
        {
            predicates.ForEach(x =>
            {
                if (x is DataPredicate)
                {
                    _dataPredicates.Add((DataPredicate)x);
                }
                if (x is IndicatorPredicate)
                {
                    _indicatorPredicates.Add((IndicatorPredicate)x);
                }
            });
        }

        private void CheckPredicateStructure()
        {
            if (!_dataPredicates.All(_predicateChecker.Check))
            {
                throw new Exception("Data predicate was not ready for use");
            }

            if (!_indicatorPredicates.All(_predicateChecker.Check))
            {
                throw new Exception("Indicator predicate was not ready for use");
            }
        }
    }
}
