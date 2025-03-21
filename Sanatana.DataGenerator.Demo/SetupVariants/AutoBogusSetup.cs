﻿using Sanatana.DataGenerator.Demo.Entities;
using System;
using Sanatana.DataGenerator.Demo.Database;
using Sanatana.DataGenerator.AutoBogus;
using System.Linq;
using AutoBogus;
using Sanatana.DataGenerator.AutoBogus.Binders;

namespace Sanatana.DataGenerator.Demo.SetupVariants
{
    public class AutoBogusSetup
    {
        public static void Start()
        {
            Console.WriteLine($"{nameof(AutoBogusSetup)} generation started");

            //Arrange
            Func<ProcurementDbContext> dbContextFactory = () => new ProcurementDbContext();
            using (ProcurementDbContext context = dbContextFactory())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            AutoFaker.Configure((IAutoFakerDefaultConfigBuilder builder) =>
            {
                //exclude navigation properties from populating by AutoBogus
                builder.WithNavigationPropertiesSkip(dbContextFactory);

                builder.WithBinder(new ExclusionAutoBinder(
                    //Exclude properties that duplicate enum properties as strings to save in database.
                    new ExcludeEnumAutoBinderModule("{0}Db"),
                    //Exclude properties that are foreign keys to same table.
                    new ExcludeSelfReferenceForeignKeyAutoBinderModule(dbContextFactory)
                ));
            });

            var setup = new GeneratorSetup()
                .SetDefaultSettings(def => def
                    .SetGenerator(new AutoBogusGenerator()) //will populate random values for entity instances
                    .SetTargetCount(100)    //set TargetCount for PurchaseOrder, other entities will override it
                )
                .SetupWithEntityFrameworkCoreSqlServer(dbContextFactory, efSetup => efSetup
                    //1. Add entities to GeneratorSetup from EF Model;
                    //2. Set Required entities in GeneratorSetup from EF Model foreign keys;
                    //3. Set InsertToPersistentStorageBeforeUse=true in GeneratorSetup if entity has autogenerated properties in EF Model;
                    //4. Add default modifier EfCoreSetPrimaryKeysModifier to increment primary key if primary key is not configured to auto increment by database and is of type in or long;
                    //5. Add default modifier EfCoreSetForeignKeysModifier to set foreign keys on generated entities;
                    //6. Add default storage EfCorePersistentStorage for generated entities.
                    .SetupFullEfSettingsBundle()
                )
                .EditEntity<Buyer>(entity => entity
                    .SetTargetCount(10)
                )
                .EditEntity<Supplier>(entity => entity
                    .SetTargetCount(10)
                );

            //Act
            setup.Generate();

            //Assert
            PurchaseOrder[] purchaseOrders = null;
            Supplier[] suppliers = null;
            using (ProcurementDbContext dbContext = dbContextFactory())
            {
                purchaseOrders = dbContext.PurchaseOrders.ToArray();
                suppliers = dbContext.Suppliers.ToArray();
            }
            Console.WriteLine($"Generated {purchaseOrders.Length} instances of {nameof(PurchaseOrder)}");
            Console.WriteLine($"Generated {suppliers.Length} instances of {nameof(Supplier)}");
            Console.WriteLine($"{nameof(AutoBogusSetup)} generation completed");
            Console.WriteLine();
        }

    }
}
