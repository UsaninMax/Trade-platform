﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Prism.Mvvm;
using TradePlatform.Commons.Sistem;
using TradePlatform.DataSet.Models;
using Microsoft.Practices.Unity;
using Prism.Events;
using TradePlatform.Commons.Info;
using TradePlatform.Commons.Info.Model.Message;
using TradePlatform.DataSet.DataServices;
using RemovePresenterFromListEvent = TradePlatform.DataSet.Events.RemovePresenterFromListEvent;

namespace TradePlatform.DataSet.Presenters
{
    public class DataSetPresenter : BindableBase, IDataSetPresenter
    {
        private readonly DataSetItem _dataSet;
        private readonly IInfoPublisher _infoPublisher;
        private readonly IDataSetService _dataSetService;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _buildDataSetTask;
        private string _statusMessage;
        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                RaisePropertyChanged();
            }
        }
        public String DataSetId => _dataSet.Id;

        public DataSetPresenter(DataSetItem dataSet)
        {
            _dataSet = dataSet;
            _infoPublisher = ContainerBuilder.Container.Resolve<IInfoPublisher>();
            _dataSetService = ContainerBuilder.Container.Resolve<IDataSetService>();
        }

        public void PrepareData()
        {

            if (IsPrepareActiveProgress())
            {
                return;
            }

            StatusMessage = Status.InProgress;
            _cancellationTokenSource = new CancellationTokenSource();
            _infoPublisher.PublishInfo(new DownloadInfo { Message = _dataSet + "- start preparing data set" });
            _buildDataSetTask = new Task(() => _dataSetService.BuildSet(_dataSet, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _buildDataSetTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    StatusMessage = Status.FailToDownloud;
                    _infoPublisher.PublishException(t.Exception);
                }
                else
                {
                    StatusMessage = Status.IsReady;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            _buildDataSetTask.Start();
        }

        public void DeleteData()
        {
            StatusMessage = Status.Deleteing;
            var delete = new Task(() => _dataSetService.Delete(_dataSet, _buildDataSetTask, _cancellationTokenSource));
            delete.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    StatusMessage = Status.FailToDelete;
                    _infoPublisher.PublishException(t.Exception);
                }
                else
                {
                    var eventAggregator = ContainerBuilder.Container.Resolve<IEventAggregator>();
                    eventAggregator.GetEvent<RemovePresenterFromListEvent>().Publish(this);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            delete.Start();
        }

        public void CheckData()
        {
            StatusMessage = Status.Checking;
            var checkTask = new Task<bool>(() => _dataSetService.CheckFiles(_dataSet));
            checkTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    StatusMessage = Status.FailToCheck;
                    _infoPublisher.PublishException(t.Exception);
                }
                else
                {
                    if (t.Result)
                    {
                        StatusMessage = Status.IsReady;
                    }
                    else
                    {
                        StatusMessage = Status.DataIsCorrapted;
                    }
                }
            });

            checkTask.Start();
        }

        public bool IsPrepareActiveProgress()
        {
            bool isActive = _buildDataSetTask != null
                            && !_buildDataSetTask.IsCompleted;
            if (isActive)
            {
                _infoPublisher.PublishInfo(new DownloadInfo { Message = this + "- currently in active data prepering process" });
            }
            return isActive;
        }

        private const string _rootFolder = "DATASETS";

        public void ShowDataInFolder()
        {
            try
            {
                var fileManager = ContainerBuilder.Container.Resolve<IFileManager>();
                fileManager.OpenFolder(_rootFolder + "\\" + _dataSet.Patch);
            }
            catch (Exception ex)
            {
                _infoPublisher.PublishException(ex);
            }

        }

        public DataSetItem DataSet()
        {
            return _dataSet;
        }

        public override string ToString()
        {
            return $"{nameof(_dataSet)}: {_dataSet}, {nameof(_statusMessage)}: {_statusMessage}, {nameof(StatusMessage)}: {StatusMessage}, {nameof(DataSetId)}: {DataSetId}";
        }
    }
}
