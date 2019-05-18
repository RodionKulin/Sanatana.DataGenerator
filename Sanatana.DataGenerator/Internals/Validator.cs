using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.GenerationOrder;
using Sanatana.DataGenerator.GenerationOrder.Complete;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanatana.DataGenerator.Internals
{
    public class Validator
    {
        //fields
        protected GeneratorSetup _generatorSetup;

        //init
        public Validator(GeneratorSetup generatorSetup)
        {
            _generatorSetup = generatorSetup;
        }


        //Generation prestart checks
        /// <summary>
        /// Validate that all required parameters were set for each entity or among default parameters.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckGeneratorSetupComplete(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            IOrderProvider orderProvider = _generatorSetup.OrderProvider;
            if (orderProvider == null)
            {
                throw new ArgumentNullException(nameof(_generatorSetup.OrderProvider));
            }

            foreach (IEntityDescription description in entityDescriptions.Values)
            {
                string msgFormat = $"Entity description [{description.Type}] did not have {{0}} configured and {nameof(GeneratorSetup)} {{1}} was not provided";

                IQuantityProvider quantityProvider = description.QuantityProvider
                    ?? _generatorSetup.DefaultQuantityProvider;
                if (quantityProvider == null)
                {
                    string defName = nameof(_generatorSetup.DefaultFlushTrigger);
                    string msg = string.Format(msgFormat
                        , nameof(description.QuantityProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }
                
                IGenerator generator = description.Generator
                    ?? _generatorSetup.DefaultGenerator;
                if (generator == null)
                {
                    string defName = nameof(_generatorSetup.DefaultFlushTrigger);
                    string msg = string.Format(msgFormat
                        , nameof(description.QuantityProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                IPersistentStorage persistentStorage = description.PersistentStorage
                    ?? _generatorSetup.DefaultPersistentStorage;
                if (persistentStorage == null)
                {
                    string defName = nameof(_generatorSetup.DefaultPersistentStorage);
                    string msg = string.Format(msgFormat
                        , nameof(description.PersistentStorage), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                IFlushTrigger insertTrigger = description.FlushTrigger
                    ?? _generatorSetup.DefaultFlushTrigger;
                if (insertTrigger == null)
                {
                    string defName = nameof(_generatorSetup.DefaultFlushTrigger);
                    string msg = string.Format(msgFormat
                        , nameof(description.FlushTrigger), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                if (description.Required != null)
                {
                    foreach (RequiredEntity required in description.Required)
                    {
                        ISpreadStrategy spreadStrategy = required.SpreadStrategy
                            ?? _generatorSetup.DefaultSpreadStrategy;
                        if (spreadStrategy == null)
                        {
                            string defName = nameof(_generatorSetup.DefaultSpreadStrategy);
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

            while (allDescriptions.Count > 0)
            {
                List<IEntityDescription> resolvedDependencyEntities = allDescriptions.Where(
                    p => p.Required == null
                    || p.Required.Count == 0
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



        //Generation prestart checks for Generators and Post Processors
        /// <summary>
        /// Validate that Parameterized PostProcessors have the same parameters as list of Required types.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckGeneratorsParams(
           Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                List<Type> requiredEntitiesTypes = entity.Required
                    .Select(x => x.Type)
                    .ToList();
                IGenerator generator = _generatorSetup.GetGenerator(entity);

                Type generatorInstaceType = generator.GetType();
                if (!(generator is IDelegateParameterizedGenerator))
                {
                    continue;
                }

                Type[] generatorParams = (generator as IDelegateParameterizedGenerator)
                    .GetRequiredEntitiesFuncParameters()
                    .ToArray();

                CheckDuplicates(generatorParams, entity, generatorInstaceType);
                CompareToRequiredParams(requiredEntitiesTypes, generatorParams, entity, generatorInstaceType);
            }
        }

        /// <summary>
        /// Validate that Parameterized PostProcessors have the same parameters as list of Required types.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        public virtual void CheckModifiersParams(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                List<Type> requiredEntitiesTypes = entity.Required.Select(x => x.Type).ToList();
                List<IModifier> modifiers = _generatorSetup.GetModifiers(entity);
                modifiers.Where(x => x is IDelegateParameterizedModifier)
                    .ToList();

                for (int i = 0; i < modifiers.Count; i++)
                {
                    IModifier modifier = modifiers[i];
                    Type processorType = modifier.GetType();
                    Type[] processorParams = (modifier as IDelegateParameterizedModifier)
                        .GetRequiredEntitiesFuncParameters()
                        .ToArray();

                    CheckDuplicates(processorParams, entity, processorType);
                    CompareToRequiredParams(requiredEntitiesTypes, processorParams, entity, processorType);
                }
            }
        }

        protected virtual void CompareToRequiredParams(List<Type> requiredParams, Type[] actualParams,
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
        /// <param name="orderProgress"></param>
        public virtual void ThrowNoNextGeneratorFound(IProgressState orderProgress)
        {
            string[] completedNames = orderProgress.CompletedEntityTypes
                .Select(x => $"[{x.Name}]")
                .ToArray();
            string[] notCompletedNames = orderProgress.NotCompletedEntities
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
