//using Sanatana.DataGenerator.Entities;
//using Sanatana.DataGenerator.Storages;
//using Sanatana.DataGenerator.TotalCountProviders;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;

//namespace Sanatana.DataGenerator.Internals
//{
//    public class SingularGeneratorSetup
//    {
//        //fields
//        private GeneratorSetup _generatorSetup;


//        //init
//        public SingularGeneratorSetup(GeneratorSetup generatorSetup)
//        {
//            _generatorSetup = generatorSetup;
//            SetupSingleTargetCount();
//        }


//        //setup methods
//        protected virtual void SetupSingleTargetCount()
//        {
//            _generatorSetup = _generatorSetup.ModifyEntity((IEntityDescription[] all) =>
//            {
//                foreach (IEntityDescription description in all)
//                {
//                    description.TotalCountProvider = new StrictTotalCountProvider(1);
//                }
//                return all;
//            });
//        }

//        protected virtual void SetupMemoryStorage()
//        {
//            var memoryStorage = new InMemoryStorage();

//            _generatorSetup = _generatorSetup.ModifyEntity((IEntityDescription[] all) =>
//            {
//                foreach (IEntityDescription description in all)
//                {
//                    description.PersistentStorages.Clear();
//                    description.PersistentStorages.Add(memoryStorage);
//                }
//                return all;
//            });
//        }



//        //generate methods
//        public virtual object[] ReturnMulti(Type entityType)
//        {
//            //validate entities
//            if (!_generatorSetup.EntityDescriptions.ContainsKey(entityType))
//            {
//                throw new ArgumentException($"Entity of type {entityType.Name} not found in {nameof(_generatorSetup.EntityDescriptions)}. Need to invoke {nameof(GeneratorSetup)}.{nameof(GeneratorSetup.RegisterEntity)} method first.");
//            }

//            //generate
//            _generatorSetup.Generate();

//            //get InMemoryStorage
//            IEntityDescription description = _generatorSetup._entityDescriptions[entityType];
//            InMemoryStorage[] inMemoryStorages = description.PersistentStorages.OfType<InMemoryStorage>().ToArray();
//            if (inMemoryStorages.Length == 0)
//            {
//                throw new ArgumentException($"Method can only be called when entity has {nameof(InMemoryStorage)} in {nameof(IEntityDescription)}.{nameof(IEntityDescription.PersistentStorages)}.");
//            }
//            InMemoryStorage storage = inMemoryStorages[0];

//            //get instance generated
//            object[] instancesGenerated = storage.Select(entityType);
//            if (instancesGenerated.Length == 0)
//            {
//                throw new ArgumentException($"No instances of type {entityType.Name} were found in {nameof(InMemoryStorage)}.");
//            }
//            return instancesGenerated;
//        }

//        public virtual object Return(Type entityType)
//        {
//            object[] instancesGenerated = ReturnMulti(entityType);
//            return instancesGenerated[0];
//        }

//        public virtual Dictionary<Type, object[]> ReturnMulti(Type[] entityTypes)
//        {
//            //validate entities have setup
//            string[] missingTypes = entityTypes
//                .Where(entityType => !_generatorSetup.EntityDescriptions.ContainsKey(entityType))
//                .Select(entityType => $"Entity of type {entityType.Name} not found in {nameof(_generatorSetup.EntityDescriptions)}. Need to invoke {nameof(GeneratorSetup)}.{nameof(GeneratorSetup.RegisterEntity)} method first.")
//                .ToArray();
//            if (missingTypes.Length > 0)
//            {
//                throw new ArgumentException(string.Join(", ", missingTypes));
//            }

//            //validate duplicates
//            string[] duplicateTypes = entityTypes.GroupBy(type => type)
//                .Where(group => group.Count() > 1)
//                .Select(group => $"Entity of type {group.Key.Name} included multiple times in {nameof(entityTypes)} parameter. Duplicates are not allowed.")
//                .ToArray();
//            if (duplicateTypes.Length > 0)
//            {
//                throw new ArgumentException(string.Join(", ", duplicateTypes));
//            }

//            //generate
//            _generatorSetup.Generate();

//            //get InMemoryStorage
//            List<InMemoryStorage>[] inMemoryStorages = entityTypes
//               .Select(entityType => _generatorSetup._entityDescriptions[entityType].PersistentStorages)
//               .Select(persistentStorages => persistentStorages.OfType<InMemoryStorage>().ToList())
//               .ToArray();

//            string[] missingInMemoryStorages = inMemoryStorages
//                .Select((inMemoryStorage, i) => new { inMemoryStorage, type = entityTypes[i]})
//                .Where(entityStorage => entityStorage.inMemoryStorage.Count == 0)
//                .Select(entityStorage => $"Method {nameof(Return)} on entity type {entityStorage.type.Name} can only be called when entity has {nameof(InMemoryStorage)} as {nameof(IEntityDescription)}.{nameof(IEntityDescription.PersistentStorages)}.")
//                .ToArray();
//            if (missingInMemoryStorages.Length > 0)
//            {
//                throw new ArgumentException(string.Join(", ", missingInMemoryStorages));
//            }

//            //get instances generated
//            object[][] instanceArrays = inMemoryStorages
//                .Select(storages => storages.First())
//                .Select((storage, i) => storage.Select(entityTypes[i]))
//                .ToArray();
//            string[] missingInstances = instanceArrays
//              .Select((instances, i) => new { instances, type = entityTypes[i] })
//              .Where(entityInstances => entityInstances.instances.Length == 0)
//              .Select(entityInstances => $"No instances of type {entityInstances.type.Name} were found in {nameof(InMemoryStorage)}.")
//              .ToArray();
//            if (missingInstances.Length > 0)
//            {
//                throw new ArgumentException(string.Join(", ", missingInstances));
//            }
                 
//            return instanceArrays
//              .Select((instances, i) => new { instances, type = entityTypes[i] })
//              .ToDictionary(x => x.type, x => x.instances);
//        }

//        public virtual Dictionary<Type, object> Return(Type[] entityTypes)
//        {
//            Dictionary<Type, object[]> instancesGenerated = ReturnMulti(entityTypes);
//            return instancesGenerated.ToDictionary(x => x.Key, x => x.Value[0]);
//        }

//        public virtual TEntity[] ReturnMulti<TEntity>()
//        {
//            object[] instances = ReturnMulti(typeof(TEntity));
//            return instances.Cast<TEntity>().ToArray();
//        }

//        public virtual TEntity Return<TEntity>()
//        {
//            object instance = Return(typeof(TEntity));
//            return (TEntity)instance;
//        }
//    }
//}
