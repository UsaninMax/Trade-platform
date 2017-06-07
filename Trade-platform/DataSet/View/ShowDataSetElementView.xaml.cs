﻿using System;
using System.ComponentModel;
using TradePlatform.DataSet.ViewModel;
using Microsoft.Practices.Unity;
using System.Windows;

namespace TradePlatform.DataSet.View
{
    public partial class ShowDataSetElementView : Window
    {
        public ShowDataSetElementView()
        {
            this.InitializeComponent();
            this.DataContext = ContainerBuilder.Container.Resolve<IDataSetElementViewModel>("ShowDataSet");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var disposable = this.DataContext as IDisposable;
            disposable?.Dispose();
            base.OnClosing(e);
        }
    }
}
