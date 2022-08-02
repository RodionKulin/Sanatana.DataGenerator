using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that Required entities were configured.
    /// </summary>
    public class RequiredEntitiesExistSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            var allDescriptions = new List<IEntityDescription>(generatorServices.EntityDescriptions.Values);
            var resolvedTypesList = new List<Type>();

            allDescriptions.ForEach(
                entity => entity.Required = entity.Required ?? new List<RequiredEntity>());

            while (allDescriptions.Count > 0)
            {
                //Find entities that not have Requried or include Required already resolved.
                //Count them as resolved.
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
                        msg.AppendLine($"Could not resolve type {typeName}. Following required entities not configured or also not resolved: {unresolvedRequiredJoined}.");
                    }

                    throw new NotSupportedException(msg.ToString());
                }
            }
        }


    }
}
