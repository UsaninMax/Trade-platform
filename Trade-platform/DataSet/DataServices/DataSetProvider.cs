﻿using System.Collections.Generic;
using TradePlatform.DataSet.Models;
using Microsoft.Practices.Unity;
using System.Linq;
using TradePlatform.Commons.BaseModels;
using TradePlatform.StockData.DataServices.Trades;

namespace TradePlatform.DataSet.DataServices
{
    public class DataSetProvider : IDataSetProvider
    {
        private readonly IDataTickParser _parser;

        public DataSetProvider()
        {
            _parser = ContainerBuilder.Container.Resolve<IDataTickParser>();
        }

        public IList<DataTick> Get(DataSetItem item)
        {
            List<DataTick> fullDataSet = new List<DataTick>();
            foreach (SubInstrument subInstrument in item.SubInstruments)
            {
                IList<DataTick> subDataSet = _parser.Parse(subInstrument)
                    .Where(m => m.Date >= subInstrument.SelectedFrom 
                    && m.Date <= subInstrument.SelectedTo)
                    .ToList();
                fullDataSet.AddRange(subDataSet);
            }

            fullDataSet.Sort((obj1, obj2) => obj1.Date.CompareTo(obj2.Date));
            return fullDataSet;
        }
    }
}
