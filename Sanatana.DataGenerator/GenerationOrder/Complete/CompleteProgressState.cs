using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using Sanatana.DataGenerator.GenerationOrder.Contracts;

namespace Sanatana.DataGenerator.GenerationOrder.Complete
{
    public class CompleteProgressState : IProgressState
    {
        //fields
        protected Dictionary<Type, EntityContext> _entityContexts;


        //properties
        public List<Type> CompletedEntityTypes { get; protected set; }
        public List<EntityContext> NotCompletedEntities { get; protected set; }


        //init
        public CompleteProgressState(Dictionary<Type, EntityContext> entityContexts)
        {
            _entityContexts = entityContexts;

            CompletedEntityTypes = entityContexts.Values
                .Where(x => x.EntityProgress.CurrentCount >= x.EntityProgress.TargetCount)
                .Select(x => x.Type)
                .ToList();

            NotCompletedEntities = entityContexts.Values
                .Where(x => x.EntityProgress.CurrentCount < x.EntityProgress.TargetCount)
                .ToList();
        }


        //methods
        /// <summary>
        /// Update NextNodeFinder internal counters updates entity was generated
        /// </summary>
        /// <param name="updateEntityContext"></param>
        public virtual void UpdateCounters(EntityContext updateEntityContext, IList generatedEntities)
        {
            EntityProgress progress = updateEntityContext.EntityProgress;
            bool isCompleted = progress.TargetCount <= progress.CurrentCount;

            if (isCompleted)
            {
                NotCompletedEntities.Remove(updateEntityContext);
                CompletedEntityTypes.Add(updateEntityContext.Type);
            }
        }

        public virtual bool GetIsFinished()
        {
            return NotCompletedEntities.Count == 0;
        }

        public virtual decimal GetCompletionPercents()
        {
            IEnumerable<EntityContext> entityContexts = _entityContexts
                .Select(x => x.Value);

            return ConvertEntitiesCompletionPercents(entityContexts);
        }

        protected virtual decimal ConvertEntitiesCompletionPercents(IEnumerable<EntityContext> entityContexts)
        {
            long totalCurrentCount = entityContexts
                //to prevent showing more that 100% percent do not take larget than Target number
                .Select(x => Math.Min(x.EntityProgress.CurrentCount, x.EntityProgress.TargetCount))
                .Sum();
            long totalTargetCount = entityContexts
                .Select(x => x.EntityProgress.TargetCount)
                .Sum();

            decimal completedPercent = totalCurrentCount / totalTargetCount;

            //convert to percents
            completedPercent *= 100;
            if (completedPercent > 100)
            {
                completedPercent = 100;
            }

            completedPercent = Math.Round(completedPercent, 2);
            return completedPercent;
        }
    }
}
