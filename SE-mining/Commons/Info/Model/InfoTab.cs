﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SEMining.Commons.Info.Events;
using SEMining.Commons.Utils;
using SE_mining_base.Info.Message;

namespace SEMining.Commons.Info.Model
{
    public class InfoTab : BindableBase, IInfoTab
    {
        public string Id { get; private set; }
        private ObservableCollection<InfoItem> _messages = new FixedSizeObservableCollection<InfoItem>(1000);
        public ICommand CloseTabCommand { get; private set; }

        public ObservableCollection<InfoItem> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        public InfoTab(string id)
        {
            Id = id;
            CloseTabCommand = new DelegateCommand(Close);
        }

        public string TabId()
        {
            return Id;
        }

        public void Add(InfoItem item)
        {
            Messages.Add(item);
        }

        public void Close()
        {
            IEventAggregator eventAggregator = ContainerBuilder.Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<CloseTabEvent>().Publish(this);
        }

        public int MessageCount()
        {
            return _messages.Count;
        }
    }
}
