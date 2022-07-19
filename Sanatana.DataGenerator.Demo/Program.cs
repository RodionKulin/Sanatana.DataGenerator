using Sanatana.DataGenerator.Demo.SetupVariants;
using System;

namespace Sanatana.DataGenerator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            CsvStorageSetup.Start();
            AutoBogusSetup.Start();
            SubsetGenerationSetup.Start();

            Console.WriteLine($"Press any key to exit");
            Console.ReadKey();
        }

    }
    
}
