﻿using Sanatana.DataGenerator.Csv;
using Sanatana.DataGenerator.Demo.Entities;
using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;


namespace Sanatana.DataGenerator.Demo.SetupVariants
{
    public class CsvStorageSetup
    {

        public static void Start()
        {
            Console.WriteLine($"{nameof(CsvStorageSetup)} generation started");

            //Arrange
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
                    .AddModifier<Supplier, Buyer>(ModifyPurchaseOrders)
                    .AddPersistentStorage(new CsvPersistentStorage("Set-PurchaseOrders.csv"))
                    .SetSpreadStrategy(new CartesianProductSpreadStrategy())
                    .SetTargetCount(10000)
                )
                .SetProgressHandler(PrintProgress);

            //Act
            setup.Generate();

            //Assert
            Console.WriteLine($"{nameof(CsvStorageSetup)} generation completed");
            Console.WriteLine();
        }

        static int? _progressCursorPos;
        private static void PrintProgress(decimal percent)
        {
            _progressCursorPos = _progressCursorPos ?? Console.CursorTop;
            Console.CursorTop = _progressCursorPos.Value;
            Console.WriteLine(percent.ToString("F"));
        }

        private static Buyer GenerateBuyer(GeneratorContext context)
        {
            int id = (int)context.CurrentCount;
            return new Buyer()
            {
                Id = id,
                Name = "Company " + id,
                Latitude = RandomPicker.Random.Next(75),
                Longitude = RandomPicker.Random.Next(75)
            };
        }

        private static Supplier GenerateSupplier(GeneratorContext context)
        {
            int id = (int)context.CurrentCount;
            return new Supplier()
            {
                Id = id,
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

            int id = (int)context.CurrentCount;
            return new PurchaseOrder()
            {
                Id = id,
                SupplierId = supplier.Id,
                BuyerId = buyer.Id,
                Accepted = purchasePositive
            };
        }

        private static void ModifyPurchaseOrders(GeneratorContext context,
           PurchaseOrder purchaseOrder, Supplier supplier, Buyer buyer)
        {
            purchaseOrder.RegisteredTime = DateTime.Now;
        }
    }
}
