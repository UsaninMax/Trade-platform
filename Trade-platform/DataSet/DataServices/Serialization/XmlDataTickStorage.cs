﻿using System.Collections.Generic;
using Microsoft.Practices.Unity;
using TradePlatform.Commons.Setting;
using TradePlatform.Commons.Sistem;
using TradePlatform.StockData.Models;

namespace TradePlatform.DataSet.DataServices.Serialization
{
    public class XmlDataTickStorage : IDataTickStorage
    {
        private readonly IFileManager _fileManager;
        private readonly ISettingSerializer _serializer;

        public XmlDataTickStorage()
        {
            _fileManager = ContainerBuilder.Container.Resolve<IFileManager>();
            _serializer = ContainerBuilder.Container.Resolve<ISettingSerializer>();
        }

        public void Store(IEnumerable<DataTick> ticks, string path, string file)
        {
            _fileManager.CreateFolder(path);
            _serializer.Serialize(ticks, path + "\\" + file + ".xml");
        }

        public IEnumerable<DataTick> ReStore(string path)
        {
            return _serializer.Deserialize<IEnumerable<DataTick>>(path);
        }
    }
}
