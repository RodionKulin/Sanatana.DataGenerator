using Sanatana.DataGenerator.Demo.SetupVariants;

namespace Sanatana.DataGenerator.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //CsvStorageSetup.Start();
            AutoBogusSetup.Start();
            //SubsetGenerationSetup.Start();

            Console.WriteLine($"Press any key to exit");
            Console.ReadKey();
        }
    }
}