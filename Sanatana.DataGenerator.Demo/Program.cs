﻿using Sanatana.DataGenerator.Csv;
using Sanatana.DataGenerator.Demo.Entities;
using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Setup
            var setup = new GeneratorSetup()
                .SetMaxParallelInserts(1)
                .RegisterEntity<Buyer>(entity => entity
                    .SetGenerator(GenerateBuyer)
                    .AddPersistentStorage(new CsvPersistentStorage("Set-Buyers.csv"))
                    .SetTargetCount(100)
                )
                .RegisterEntity<Supplier>(entity => entity
                    .SetGenerator(GenerateSupplier)
                    .AddPersistentStorage(new CsvPersistentStorage("Set-Suppliers.csv"))
                    .SetTargetCount(100)
                )
                .RegisterEntity<PurchaseOrder>(entity => entity
                    .SetGenerator<Supplier, Buyer>(GeneratePurchaseOrder)
                    .AddMultiModifier<Supplier, Buyer>(ModifyPurchaseOrders)
                    .AddPersistentStorage(new CsvPersistentStorage("Set-PurchaseOrders.csv"))
                    .SetSpreadStrategy(new CartesianProductSpreadStrategy())
                    .SetTargetCount(10000)
                )
                .SetProgressHandler(PrintProgress);

            //Generate
            setup.Generate();
            Console.WriteLine("Completed");
            Console.ReadKey();
        }

        private static void PrintProgress(decimal percent)
        {
            Console.CursorTop = 0;
            Console.WriteLine(percent.ToString("F"));
        }

        private static Buyer GenerateBuyer(GeneratorContext context)
        {
            int id = (int)IdIterator.GetNextId(context.Description.Type);
            return new Buyer()
            {
                BuyerId = id,
                Name = "Company " + id,
                Latitude = RandomPicker.Random.Next(75),
                Longitude = RandomPicker.Random.Next(75)
            };
        }

        private static Supplier GenerateSupplier(GeneratorContext context)
        {
            int id = (int)IdIterator.GetNextId(context.Description.Type);
            return new Supplier()
            {
                SupplierId = id,
                Name = "Company " + id,
                Latitude = RandomPicker.Random.Next(75),
                Longitude = RandomPicker.Random.Next(75)
            };
        }

        private static PurchaseOrder GeneratePurchaseOrder(GeneratorContext context,
            Supplier supplier, Buyer buyer)
        {
            //accept offer based on distanse
            double distance = MathNet.Numerics.Distance.Euclidean(
                new[] { buyer.Longitude, buyer.Latitude },
                new[] { supplier.Longitude, supplier.Latitude });
            bool purchasePositive = distance <= 50;

            int id = (int)IdIterator.GetNextId(context.Description.Type);
            return new PurchaseOrder()
            {
                PurchaseOrderId = id,
                SupplierId = supplier.SupplierId,
                BuyerId = buyer.BuyerId,
                Accepted = purchasePositive
            };
        }

        private static List<PurchaseOrder> ModifyPurchaseOrders(GeneratorContext context,
           List<PurchaseOrder> purchaseOrders, Supplier supplier, Buyer buyer)
        {
            purchaseOrders.ForEach(x => x.RegisteredDate = DateTime.Now);
            return purchaseOrders;
        }
    }
    
}
