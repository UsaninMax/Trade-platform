﻿using System;
using System.ComponentModel;
using TradePlatform.DataSet.ViewModels;
using Microsoft.Practices.Unity;
using TradePlatform.Commons.BaseModels;

namespace TradePlatform.DataSet.Views
{
    public partial class CopyDataSetElementView
    {
        public CopyDataSetElementView()
        {
            InitializeComponent();
            var modelWiew = ContainerBuilder.Container.Resolve<IDataSetElementViewModel>("CopyDataSet");
            DataContext = modelWiew;
            IClosableWindow closableWindow = modelWiew as IClosableWindow;

            if (closableWindow != null)
            {
                closableWindow.CloseWindowNotification += CloseWindowNotificationHandler;
            }
        }

        private void CloseWindowNotificationHandler(object source, EventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var disposable = DataContext as IDisposable;
            disposable?.Dispose();
            base.OnClosing(e);
        }
    }
}

