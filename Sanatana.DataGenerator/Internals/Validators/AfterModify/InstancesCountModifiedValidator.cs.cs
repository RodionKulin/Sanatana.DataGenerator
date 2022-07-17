using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Internals.Validators.AfterModify
{
    /// <summary>
    /// Validate that number of modified entities is greater than 0 and returned type is List&lt;&gt;
    /// </summary>
    public class InstancesCountModifiedValidator : IModifyValidator
    {
        public virtual void ValidateModified(IList entities, Type entityType, IModifier modifier)
        {
            string modifierType = modifier.GetType().FullName;
            string resultCountMsg = "Number of entities returned must be greater than 0. " +
                $"Modifier {modifierType} for entity {entityType} returned 0 entities. " + 
                $"If you want to allow such behavior, then remove {nameof(InstancesCountModifiedValidator)} from Validators.";

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

    }
}
