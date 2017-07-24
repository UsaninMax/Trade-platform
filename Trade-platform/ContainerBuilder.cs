﻿
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Prism.Events;
using System.Net;
using TradePlatform.Commons.Info;
using TradePlatform.Commons.Info.Exception;
using TradePlatform.Commons.Info.ViewModels;
using TradePlatform.Commons.Setting;
using TradePlatform.Commons.Sistem;
using TradePlatform.DataSet.DataServices;
using TradePlatform.DataSet.DataServices.Serialization;
using TradePlatform.DataSet.Holders;
using TradePlatform.DataSet.Models;
using TradePlatform.DataSet.Presenters;
using TradePlatform.DataSet.ViewModels;
using TradePlatform.Main.ViewModels;
using TradePlatform.Main.Views;
using TradePlatform.Sandbox;
using TradePlatform.Sandbox.DataProviding;
using TradePlatform.Sandbox.DataProviding.Checks;
using TradePlatform.Sandbox.DataProviding.Transformers;
using TradePlatform.Sandbox.Presenters;
using TradePlatform.Sandbox.Providers;
using TradePlatform.Sandbox.Transactios;
using TradePlatform.Sandbox.Transactios.Models;
using TradePlatform.StockData.DataServices.SecuritiesInfo;
using TradePlatform.StockData.DataServices.SecuritiesInfo.Finam;
using TradePlatform.StockData.DataServices.Serialization;
using TradePlatform.StockData.DataServices.Trades;
using TradePlatform.StockData.DataServices.Trades.Finam;
using TradePlatform.StockData.Holders;
using TradePlatform.StockData.Models;
using TradePlatform.StockData.Presenters;
using TradePlatform.StockData.ViewModels;
using TradePlatform.Vizualization.ViewModels;
using TradePlatform.Vizualization.Holders;
using TradePlatform.Vizualization.Builders;
using TradePlatform.Vizualization.Populating;

namespace TradePlatform
{
    public static class ContainerBuilder
    {

        private static IUnityContainer _container;

        public static IUnityContainer Container
        {
            set { _container = value; }
            get { return _container ?? (_container = new UnityContainer()); }

        }

        public static void Initialize()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Container.RegisterType<ShellView>();
            Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ShellView>();
            Container.RegisterType<IShellModel, ShellModel>();

            Container.RegisterType<IFileManager, FileManager>();
            Container.RegisterType<IInfoViewModel, InfoViewModel>();
            Container.RegisterInstance(new ExceptionActualizer());
            Container.RegisterType<ISettingSerializer, XmlSettingSerializer>();
            Container.RegisterType<IInfoPublisher, InfoPublisher>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IHistoryInstrumentsViewModel, HistoryInstrumentsViewModel>();
            Container.RegisterType<IDownloadedInstrumentsViewModel, DownloadedInstrumentsViewModel>();
            Container.RegisterType<IDownloadNewInstrumentViewModel, FinamDownloadNewInstrumentViewModel>();
            Container.RegisterType<ISecuritiesInfoUpdater, FinamSecuritiesInfoUpdater>();
            Container.RegisterType<ISecuritiesInfoDownloader, FinamSecuritiesInfoDownloader>();
            Container.RegisterType<ISecuritiesInfoParser, FinamSecuritiesInfoParser>();
            Container.RegisterType<SecuritiesInfoHolder>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInstrumentSplitter, FinamInstrumentSplitter>();
            Container.RegisterType<IInstrumentService, FinamInstrumentService>();
            Container.RegisterType<ITradesDownloader, FinamTradesDownloader>();
            Container.RegisterType<IInstrumentsStorage, XmlInstrumentStorage>();
            Container.RegisterType<IDounloadInstrumentPresenter, DounloadInstrumentPresenter>(new InjectionConstructor(typeof(Instrument)));
            Container.RegisterType<IDownloadedInstrumentsHolder, DownloadedInstrumentsHolder>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IDataSetElementViewModel, DataSetElementViewModel>("DataSet");
            Container.RegisterType<IDataSetElementViewModel, ShowDataSetElementViewModel>("ShowDataSet");
            Container.RegisterType<IDataSetElementViewModel, CopyDataSetElementViewModel>("CopyDataSet");
            Container.RegisterType<IDataSetListViewModel, DataSetListViewModel>();
            Container.RegisterType<IInstrumentChooseListViewModel, InstrumentChooseListViewModel>();
            Container.RegisterType<IDataSetHolder, DataSetHolder>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDataSetPresenter, DataSetPresenter>(new InjectionConstructor(typeof(DataSetItem)));
            Container.RegisterType<IDataSetStorage, XmlDataSetStorage>();
            Container.RegisterType<IDataSetService, DataSetService>();
            Container.RegisterType<IDataTickProvider, DataTickProvider>();
            Container.RegisterType<IDataTickStorage, XmlDataTickStorage>();
            Container.RegisterType<IDataTickParser, FinamDataTickParser>();

            Container.RegisterType<ISandboxPresenter, SandboxPresenter>(new InjectionConstructor(typeof(ISandbox) , typeof(string)));
            Container.RegisterType<ISandboxProvider, SandboxProvider>();

            Container.RegisterType<IPredicateChecker, SlicePredicateChecker>();
            Container.RegisterType<IDataProvider, DataProvider>();
            Container.RegisterType<ITransformer, DataTransformer>();
            Container.RegisterType<IIndicatorBuilder, IndicatorBuilder>();

            Container.RegisterType<ITransactionsContext, TransactionsContext>(new InjectionConstructor(typeof(IDictionary<string, BrokerCost>)));
            Container.RegisterType<ITransactionHolder, TransactionHolder>(new InjectionConstructor(typeof(IDictionary<string, BrokerCost>)));
            Container.RegisterType<IBalance, Balance>();
            Container.RegisterType<ITransactionBuilder, TransactionBuilder>();
            Container.RegisterType<IWorkingPeriodHolder, WorkingPeriodHolder>();

            Container.RegisterType<IChartViewModel, LiveChartViewModel>();
            Container.RegisterType<IChartsBuilder, ChartsBuilder>();
            Container.RegisterType<IChartsHolder, ChartsHolder>();
            Container.RegisterType<IChartsPopulator, ChartsPopulator>();
        }
    }
}

