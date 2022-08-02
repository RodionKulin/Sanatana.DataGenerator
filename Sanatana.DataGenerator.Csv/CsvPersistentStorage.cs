using CsvHelper;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Csv
{
    public class CsvPersistentStorage : IPersistentStorage
    {
        //fields
        private string _csvFilePath;
        private CsvWriter _csvWriter;


        //init
        public CsvPersistentStorage(string csvFilePath)
        {
            _csvFilePath = csvFilePath;
        }



        //methods
        public virtual Task Insert<TEntity>(List<TEntity> entities) 
            where TEntity : class
        {
            CsvWriter csvWriter = GetCsvWriter();
            csvWriter.WriteRecords(entities);

            return Task.FromResult(0);
        }

        protected virtual CsvWriter GetCsvWriter()
        {
            if(_csvWriter == null)
            {
                var writer = new StreamWriter(_csvFilePath);
                _csvWriter = new CsvWriter(writer);
            }
          
            return _csvWriter;
        }

        public virtual void Setup()
        {
            if (File.Exists(_csvFilePath))
            {
                File.Delete(_csvFilePath);
            }
        }

        public virtual void Dispose()
        {
            if(_csvWriter != null)
            {
                _csvWriter.Dispose();
                _csvWriter = null;
            }
        }
    }
}
