using Sanatana.DataGenerator.Csv;
using Sanatana.DataGenerator.Demo.Entities;
using Sanatana.DataGenerator.Demo.SetupVariants;
using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoBogusSetup.Start();
            CsvStorageSetup.Start();
            SubsetGenerationSetup.Start();
            Console.ReadKey();
        }

    }
    
}
