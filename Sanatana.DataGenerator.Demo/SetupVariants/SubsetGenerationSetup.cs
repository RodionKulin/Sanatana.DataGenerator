﻿using Sanatana.DataGenerator.Demo.Entities;
using System;
using Sanatana.DataGenerator.Demo.Database;
using Sanatana.DataGenerator.AutoBogus;
using Sanatana.DataGenerator.Supervisors.Subset;
using System.Linq;
using Sanatana.DataGenerator.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;

namespace Sanatana.DataGenerator.Demo.SetupVariants
{
    public class SubsetGenerationSetup
    {
        public static void Start()
        {
            Console.WriteLine($"{nameof(SubsetGenerationSetup)} generation started");

            //Arrange
            Func<ProcurementDbContext> dbContextFactory = () => new ProcurementDbContext();
            using (ProcurementDbContext context = dbContextFactory())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            var repositoryFactory = new SqlRepositoryFactory(dbContextFactory);
            var efCoreSqlServerStorage = new EfCorePersistentStorage(repositoryFactory);

            var setup = new GeneratorSetup()
                .SetDefaultSettings(def => def
                    .SetGenerator(new AutoBogusGenerator()) //will populate random values for instance properties
                )
                .SetupWithEntityFrameworkCoreSqlServer(dbContextFactory, efSetup => efSetup
                    //1. Register entities from EF Model;
                    //2. Register Required entities from EF Model foreign keys;
                    //3. Set InsertToPersistentStorageBeforeUse=true if entity has autogenerated properties;
                    //4. Increment primary keys instead of random ints populated by AutoBogus;
                    //5. Set foreign keys in EfCoreSetForeignKeysModifier;
                    //6. Set EfCorePersistentStorage as default.
                    .SetupFullEfSettingsBundle()
                );

            //Act
            PurchaseOrder purchaseOrder = setup.ToSubsetSetup<PurchaseOrder>()
                .SetTargetCountSingle(EntitiesSelection.All)
                .AddInMemoryStorage(EntitiesSelection.Target)
                .AddStorage(EntitiesSelection.Target, efCoreSqlServerStorage)
                .GetSingleTarget();

            //Assert
            PurchaseOrder[] purchaseOrders = null;
            using (ProcurementDbContext dbContext = dbContextFactory())
            {
                purchaseOrders = dbContext.PurchaseOrders.ToArray();
            }

            Console.WriteLine($"Generated {nameof(PurchaseOrder)} with Id={purchaseOrder.Id}");
            Console.WriteLine($"{nameof(SubsetGenerationSetup)} generation completed");
            Console.WriteLine();
        }

    }
}
