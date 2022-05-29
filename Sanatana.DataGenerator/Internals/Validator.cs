using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanatana.DataGenerator.Internals
{
    /// <summary>
    /// Configuration validator that will throw errors on missing or inconsistent setup
    /// </summary>
    public class Validator
    {
        //fields
        protected GeneratorSetup _generatorSetup;

        //init
        public Validator(GeneratorSetup generatorSetup)
        {
            _generatorSetup = generatorSetup;
        }




        //Prestart checks for GeneratorSetup
        /// <summary>
        /// Validate that all required parameters were set for each entity or among default parameters.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckGeneratorSetupComplete(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            ISupervisor supervisor = _generatorSetup.Supervisor;
            if (supervisor == null)
            {
                throw new ArgumentNullException(nameof(_generatorSetup.Supervisor));
            }

            foreach (IEntityDescription description in entityDescriptions.Values)
            {
                string msgFormat = $"Entity description [{description.Type}] did not have {{0}} configured and {nameof(GeneratorSetup)} {{1}} was not provided";

                ITotalCountProvider totalCountProvider = description.TotalCountProvider
                    ?? _generatorSetup.Defaults.TotalCountProvider;
                if (totalCountProvider == null)
                {
                    string defName = nameof(_generatorSetup.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.TotalCountProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }
                
                IGenerator generator = description.Generator ?? _generatorSetup.Defaults.Generator;
                if (generator == null)
                {
                    string defName = nameof(_generatorSetup.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.TotalCountProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                List<IPersistentStorage> persistentStorages = description.PersistentStorages
                    ?? _generatorSetup.Defaults.PersistentStorages;
                if (persistentStorages == null || persistentStorages.Count == 0)
                {
                    string defName = nameof(_generatorSetup.Defaults.PersistentStorages);
                    string msg = string.Format(msgFormat
                        , nameof(description.PersistentStorages), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                IFlushStrategy flushTrigger = description.FlushStrategy
                    ?? _generatorSetup.Defaults.FlushStrategy;
                if (flushTrigger == null)
                {
                    string defName = nameof(_generatorSetup.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.FlushStrategy), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                if (description.Required != null)
                {
                    foreach (RequiredEntity required in description.Required)
                    {
                        ISpreadStrategy spreadStrategy = required.SpreadStrategy
                            ?? _generatorSetup.Defaults.SpreadStrategy;
                        if (spreadStrategy == null)
                        {
                            string defName = nameof(_generatorSetup.Defaults.SpreadStrategy);
                            string msg = $"Entity description [{description.Type}] with required type [{required.Type}] did not have {nameof(RequiredEntity.SpreadStrategy)} configured and {nameof(GeneratorSetup)} {defName} was not provided";
                            throw new ArgumentNullException(defName, msg);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate that Required entities were configured by themselves,
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckRequiredEntitiesPresent(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            var allDescriptions = new List<IEntityDescription>(entityDescriptions.Values);
            var resolvedTypesList = new List<Type>();

            allDescriptions.ForEach(
                entity => entity.Required = entity.Required ?? new List<RequiredEntity>());

            while (allDescriptions.Count > 0)
            {
                List<IEntityDescription> resolvedDependencyEntities = allDescriptions.Where(
                    p => p.Required.Count == 0
                    || p.Required.Select(req => req.Type)
                        .Distinct()
                        .All(req => resolvedTypesList.Contains(req))
                    )
                    .ToList();

                resolvedTypesList.AddRange(resolvedDependencyEntities.Select(p => p.Type));
                allDescriptions.RemoveAll(p => resolvedDependencyEntities.Contains(p));

                if (resolvedDependencyEntities.Count == 0 && allDescriptions.Count > 0)
                {
                    StringBuilder msg = new StringBuilder();
                    for (int i = 0; i < allDescriptions.Count; i++)
                    {
                        string typeName = allDescriptions[i].Type.FullName;
                        string[] unresolvedRequired = allDescriptions[i].Required
                            .Select(x => x.Type)
                            .Except(resolvedTypesList)
                            .Select(x => x.FullName)
                            .ToArray();
                        string unresolvedRequiredJoined = string.Join(", ", unresolvedRequired);
                        msg.AppendLine($"Could not resolve type {typeName}. Following required entities were not configured, or also were not resolved: {unresolvedRequiredJoined}.");
                    }

                    throw new DataMisalignedException(msg.ToString());
                }
            }
        }

        /// <summary>
        /// Validate that there is no circle of required entities
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckCircularDependencies(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            //find first entity with circle of dependencies
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                List<Type> circleOfDependencies = FindCircularDependency(entity, null, entityDescriptions);
                if(circleOfDependencies != null)
                {
                    string listOfTypesJoined = string.Join(" > ", circleOfDependencies);
                    throw new NotSupportedException($"Circle of dependencies found starting with type {listOfTypesJoined}");
                }
            }
        }

        protected virtual List<Type> FindCircularDependency(IEntityDescription entity, 
            List<Type> requiredEntities, Dictionary<Type, IEntityDescription> allEntityDescriptions)
        {
            requiredEntities = requiredEntities ?? new List<Type>();

            bool isCircle = requiredEntities.Contains(entity.Type);
            requiredEntities.Add(entity.Type);
            if (isCircle)
            {
                return requiredEntities;
            }

            //return null if no circle dependencies found
            if (entity.Required == null)
            {
                return null;
            }

            foreach (RequiredEntity required in entity.Required)
            {
                List<Type> dependenciesBranch = new List<Type>(requiredEntities);
                IEntityDescription requiredEntity = allEntityDescriptions[required.Type];
                List<Type> circleOfDependencies = FindCircularDependency
                    (requiredEntity, dependenciesBranch, allEntityDescriptions);
                if (circleOfDependencies != null)
                {
                    return circleOfDependencies;
                }
            }

            //return null if no circle dependencies found
            return null;
        }



        //Prestart checks for Generators and Modifiers
        /// <summary>
        /// Validate that Parameterized Modifiers have the same parameters as list of Required types.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckGeneratorsParams(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                //check that no duplicates in Required types
                Type[] requiredEntitiesTypes = entity.Required
                    .Select(x => x.Type)
                    .ToArray();
                IGenerator generator = _generatorSetup.Defaults.GetGenerator(entity);
                Type generatorInstanceType = generator.GetType();
                CheckDuplicates(requiredEntitiesTypes, entity, generatorInstanceType);

                //Compare Required types from IEntityDescription.Required vs IDelegateParameterizedGenerator to make sure it was not changed manually before start.
                if (!(generator is IDelegateParameterizedGenerator))
                {
                    continue;
                }
                Type[] generatorParams = (generator as IDelegateParameterizedGenerator)
                    .GetRequiredEntitiesFuncParameters()
                    .ToArray();
                CompareToRequiredParams(requiredEntitiesTypes, generatorParams, entity, generatorInstanceType);
            }
        }

        /// <summary>
        /// Validate that Parameterized Modifiers have the same parameters as list of Required types.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckModifiersParams(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                Type[] requiredEntitiesTypes = entity.Required.Select(x => x.Type).ToArray();
                List<IModifier> modifiers = _generatorSetup.Defaults.GetModifiers(entity);
                modifiers.Where(x => x is IDelegateParameterizedModifier).ToList();

                for (int i = 0; i < modifiers.Count; i++)
                {
                    IModifier modifier = modifiers[i];
                    Type modifierType = modifier.GetType();
                    Type[] modifierParams = (modifier as IDelegateParameterizedModifier)
                        .GetRequiredEntitiesFuncParameters()
                        .ToArray();

                    CheckDuplicates(modifierParams, entity, modifierType);
                    CompareToRequiredParams(requiredEntitiesTypes, modifierParams, entity, modifierType);
                }
            }
        }

        /// <summary>
        /// Compare Required types from IEntityDescription.Required vs IDelegateParameterizedGenerator to make sure it was not changed manually before start.
        /// </summary>
        /// <param name="requiredParams"></param>
        /// <param name="actualParams"></param>
        /// <param name="entity"></param>
        /// <param name="whereFound"></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void CompareToRequiredParams(Type[] requiredParams, Type[] actualParams,
            IEntityDescription entity, Type whereFound)
        {
            string[] extraParams = requiredParams.Except(actualParams)
                .Select(x => x.FullName)
                .ToArray();
            if (extraParams.Length > 0)
            {
                string paramsJoined = string.Join(", ", extraParams);
                string message = $"Entity {entity.Type.FullName} {whereFound.FullName} contains less parameters than configured in Entities's Required. Those are missing parameters: {paramsJoined}.";
                throw new NotSupportedException(message);
            }

            extraParams = actualParams.Except(requiredParams)
                .Select(x => x.FullName)
                .ToArray();
            if (extraParams.Length > 0)
            {
                string paramsJoined = string.Join(", ", extraParams);
                string message = $"Entity {entity.Type.FullName} {whereFound.FullName} contains more parameters than configured in Entities's Required. Those are extra parameters: {paramsJoined}.";
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Check that Required types don't have duplicates
        /// </summary>
        /// <param name="paramsTypes"></param>
        /// <param name="entity"></param>
        /// <param name="whereFound"></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void CheckDuplicates(Type[] paramsTypes, IEntityDescription entity, Type whereFound)
        {
            List<string> actualParamsDuplicates = paramsTypes
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key.FullName)
                .ToList();

            if (actualParamsDuplicates.Count > 0)
            {
                string duplicates = string.Join(", ", actualParamsDuplicates);
                string message = $"Entity {entity.Type.FullName} {whereFound.FullName} contains duplicate parameter(s) with type: {duplicates}.";
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Validate IEntityDescription are supported Generators.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckEntitySettingsForGenerators(
           Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            var entitiesWithGenerators = entityDescriptions.Values.Where(x => x.Generator != null);
            foreach (IEntityDescription entity in entitiesWithGenerators)
            {
                entity.Generator.ValidateEntitySettings(entity);                
            }
        }



        //Generation runtime checks
        /// <summary>
        /// Validate that number of generated entities is greater than 0 and returned type is List<>
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="entityType"></param>
        /// <param name="generator"></param>
        public virtual void CheckGeneratedCount(IList entities, Type entityType, IGenerator generator)
        {
            string generatorType = generator.GetType().FullName;
            string resultCountMsg = "Number of entities returned must be greater than 0. " +
                $"{nameof(IGenerator)} {generatorType} for entity {entityType} returned 0 entities.";

            if (entities == null)
            {
                throw new NotSupportedException(resultCountMsg);
            }

            Type entitiesListType = entities.GetType();
            if (entitiesListType.IsAssignableFrom(typeof(List<>)))
            {
                string resultTypeMessage = $"List returned from {nameof(IGenerator)} must be a generic List<>. {nameof(IGenerator)} {generatorType} returned list of type {entitiesListType}.";

                throw new NotSupportedException(resultTypeMessage);
            }

            if (entities.Count == 0)
            {
                throw new NotSupportedException(resultCountMsg);
            }
        }

        /// <summary>
        /// Validate that number of modified entities is greater than 0 and returned type is List<>
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="entityType"></param>
        /// <param name="modifier"></param>
        public virtual void CheckModifiedCount(IList entities, Type entityType, IModifier modifier)
        {
            string modifierType = modifier.GetType().FullName;
            string resultCountMsg = "Number of entities returned must be greater than 0. " +
                $"Modifier {modifierType} for entity {entityType} returned 0 entities.";

            if (entities == null)
            {
                throw new NotSupportedException(resultCountMsg);
            }

            Type entitiesListType = entities.GetType();
            if (entitiesListType.IsAssignableFrom(typeof(List<>)))
            {
                string resultTypeMessage = $"List returned from {nameof(IModifier)} must be a generic List<>. Modifier {modifierType} returned list of type {entitiesListType}.";
                throw new NotSupportedException(resultTypeMessage);
            }

            if (entities == null || entities.Count == 0)
            {
                throw new NotSupportedException(resultCountMsg);
            }
        }

        /// <summary>
        /// Throw exception on Next node finding misfunction, when next node is not found.
        /// </summary>
        /// <param name="progressState"></param>
        public virtual void ThrowNoNextGeneratorFound(IProgressState progressState)
        {
            string[] completedNames = progressState.CompletedEntityTypes
                .Select(x => $"[{x.Name}]")
                .ToArray();
            string[] notCompletedNames = progressState.NotCompletedEntities
                .Select(x => $"[{x.Type.Name}:{x.EntityProgress.TargetCount - x.EntityProgress.CurrentCount}]")
                .ToArray();

            string completedList = string.Join(", ", completedNames);
            string notCompletedList = string.Join(", ", notCompletedNames);

            throw new NullReferenceException("Could not find next entity to generate. "
                + $"Following list of entities generated successfully: {completedList}. "
                + $"Following list of entities still was not fully generated [TypeName:remainingCount]: {notCompletedList}");
        }


    }
}
