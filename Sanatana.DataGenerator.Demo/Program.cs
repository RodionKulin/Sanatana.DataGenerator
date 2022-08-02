using Sanatana.DataGenerator.Demo.SetupVariants;
using System;

namespace Sanatana.DataGenerator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            CsvStorageSetup.Start();
            //AutoBogusSetup.Start();
            //SubsetGenerationSetup.Start();

            Sanatana.DataGenerator.RandomPicker.NextBoolean(0.5);

            Console.WriteLine($"Press any key to exit");
            Console.ReadKey();
        }

    }
    
}
