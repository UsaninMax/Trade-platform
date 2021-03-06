﻿using System.Collections.Generic;
using Microsoft.Practices.Unity;
using SEMining.Commons.Setting;
using SEMining.Commons.Sistem;
using SEMining.StockData.Models;

namespace SEMining.StockData.DataServices.Serialization
{
    public class XmlInstrumentStorage : IInstrumentsStorage
    {
        private static string Path => "Settings\\FinamInstruments.xml";
        private readonly IFileManager _fileManager;
        private readonly ISettingSerializer _serializer;

        public XmlInstrumentStorage()
        {
            _fileManager = ContainerBuilder.Container.Resolve<IFileManager>();
            _serializer = ContainerBuilder.Container.Resolve<ISettingSerializer>();
        }

        public void Store(IEnumerable<Instrument> instruments)
        {
            CreateRoot();
            _serializer.Serialize(instruments, Path);
        }

        private void CreateRoot()
        {
            _fileManager.CreateFolder("Settings");
        }

        public IEnumerable<Instrument> ReStore()
        {
            if (!_fileManager.IsFileExist(Path))
            {
                return new List<Instrument>();
            }

            return _serializer.Deserialize<IEnumerable<Instrument>>(Path);
        }
    }
}
