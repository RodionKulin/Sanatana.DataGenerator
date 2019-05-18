﻿using Sanatana.DataGenerator.Csv;
using Sanatana.DataGenerator.Demo.Entities;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Setup
            var setup = new GeneratorSetup();

            //CSV does not need multithreaded writes
            setup.TemporaryStorage.MaxTasksRunning = 1;

            setup.RegisterEntity(new EntityDescription<Buyer>()
                .SetGenerator(GenerateBuyer)
                .SetPersistentStorage(new CsvPersistentStorage("Set-Buyers.csv"))
                .SetTargetCount(10));
            setup.RegisterEntity(new EntityDescription<Supplier>()
                .SetGenerator(GenerateSupplier)
                .SetPersistentStorage(new CsvPersistentStorage("Set-Suppliers.csv"))
                .SetTargetCount(10));
            setup.RegisterEntity(new EntityDescription<PurchaseOrder>()
                .SetGenerator<Supplier, Buyer>(GeneratePurchaseOrder)
                .AddMultiModifier<Supplier, Buyer>(ModifyPurchaseOrders)
                .SetPersistentStorage(new CsvPersistentStorage("Set-PurchaseOrders.csv"))
                .SetSpreadStrategy(new CartesianProductSpreadStrategy())
                .SetTargetCount(100));

            //Generate
            setup.Generate();
            Console.WriteLine("Finished");
            Console.ReadLine();
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
