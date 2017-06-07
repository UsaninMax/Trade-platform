﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using TradePlatform.StockData.DataServices.Serialization;
using TradePlatform.StockData.Models;

namespace TradePlatform.StockData.Holders
{
    public class DownloadedInstrumentsHolder : IDownloadedInstrumentsHolder
    {
        private readonly HashSet<Instrument> _instrumnnets = new HashSet<Instrument>();
        private readonly IInstrumentsStorage _instrumentsStorage;

        public DownloadedInstrumentsHolder()
        {
            _instrumentsStorage = ContainerBuilder.Container.Resolve<IInstrumentsStorage>();
        }

        public void Put(Instrument instrument)
        {
            _instrumnnets.Add(instrument);
        }

        public void Remove(Instrument instrument)
        {
            _instrumnnets.Remove(instrument);
        }

        public ISet<Instrument> GetAll()
        {
            return _instrumnnets;
        }

        public void Restore()
        {
            try
            {
                _instrumentsStorage
                    .ReStore()
                    .ForEach(i => _instrumnnets.Add(i));
            }
            catch (Exception e)
            {
                //TODO: log
            }
        }

        public void Store()
        {
            _instrumentsStorage.Store(_instrumnnets);
        }
    }
}