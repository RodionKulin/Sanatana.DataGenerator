using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.CombineStrategies.ProportionalCombine
{
    public class ProportionalCombineService
    {
        //methods
        public virtual TProcessor GetNextProcessor<TProcessor>(List<TProcessor> processors, List<Chunk> chunks,
            Dictionary<Type, IEnumerator<ChunkRange>> chunkRanges, GeneratorContext generatorContext)
        {
            Type entityType = generatorContext.Description.Type;
            string processorName = nameof(TProcessor);

            if (processors.Count == 0)
            {
                throw new NotSupportedException($"Provided empty list of {processorName} for entity {generatorContext.Description.Type.FullName} in {nameof(ProportionalGeneratorsCombiner)}.");
            }
            if (!chunkRanges.ContainsKey(entityType))
            {
                List<Chunk> absoluteCountChunks = DistributeAbsoluteCount(chunks, generatorContext);
                chunkRanges[entityType] = GetLargeChunkRanges(absoluteCountChunks).GetEnumerator();
            }

            IEnumerator<ChunkRange> rangeEnumerator = chunkRanges[entityType];
            int nextGeneratorIndex = GetNextProcessorIndex(rangeEnumerator, generatorContext);
            if (nextGeneratorIndex >= processors.Count)
            {
                throw new IndexOutOfRangeException($"Received {nameof(ChunkRange)} with out of range {nameof(ChunkRange.ChunkIndex)}={nextGeneratorIndex} for {entityType.FullName} item {nameof(generatorContext.CurrentCount)}={generatorContext.CurrentCount}. Total {processors.Count} {processorName}s provided.");
            }

            return processors[nextGeneratorIndex];
        }

        public virtual int GetNextProcessorIndex(IEnumerator<ChunkRange> rangeEnumerator, GeneratorContext generatorContext)
        {
            Type entityType = generatorContext.Description.Type;
            ChunkRange range = rangeEnumerator.Current;

            for (int i = 1; i <= 2; i++)
            {
                //Scenario 1: range not empty if previous rangeEnumerator.Current is still valid;
                //Scenario 2: range is null if it is first item generated;
                //Scenario 3: range is null if previous item was at ChunkRange end boundary.
                if (range != null
                    && range.StartIndexInclusive <= generatorContext.CurrentCount
                    && generatorContext.CurrentCount < range.EndIndexExclusive)
                {
                    //is within range, so can keep using same ChunkRange
                    break;
                }

                //1 cycle checks previous ChunkRange
                //2 cycle checks next ChunkRange
                //If next ChunkRange is immediatly not within range then it is an exception in ProportionalCombineService
                if (i == 2)
                {
                    throw new NotSupportedException($"Not receied valid {nameof(ChunkRange)} for {entityType.FullName} item {nameof(generatorContext.CurrentCount)}={generatorContext.CurrentCount} from {i} try.");
                }

                if (!rangeEnumerator.MoveNext())
                {
                    throw new NotSupportedException($"Not able to generate {entityType.FullName} item {nameof(generatorContext.CurrentCount)}={generatorContext.CurrentCount}. It is of bounds of chunk ranges.");
                }
                range = rangeEnumerator.Current;
            }

            return range.ChunkIndex;
        }

        public virtual List<Chunk> DistributeAbsoluteCount(List<Chunk> chunks, GeneratorContext generatorContext)
        {
            List<Chunk> chunkPlans = chunks
                .Select((chunk, index) => chunk.Clone())
                .ToList();

            //set absolute count if chunk is proportional
            long sumOfAbsoluteCount = chunkPlans.Sum(x => x.Absolute);
            long remainingCount = generatorContext.TargetCount - sumOfAbsoluteCount;
            long proportionSum = chunkPlans.Where(x => !x.IsAbsolute)
                .Sum(x => x.Proportion.Value);
            chunkPlans.Where(x => !x.IsAbsolute)
                .ToList()
                .ForEach(plan => plan.Absolute = (long)Math.Floor((decimal)remainingCount / proportionSum * plan.Proportion.Value));

            //set division remains of proportional chunks
            sumOfAbsoluteCount = chunkPlans.Sum(x => x.Absolute);
            remainingCount = generatorContext.TargetCount - sumOfAbsoluteCount;
            if (remainingCount > 0)
            {
                Chunk largestProportionChunk = chunkPlans
                    .Where(x => !x.IsAbsolute)
                    .Aggregate((c1, c2) => c1.Proportion.Value > c2.Proportion.Value ? c1 : c2);
                largestProportionChunk.Absolute += remainingCount;
            }

            return chunkPlans;
        }

        public virtual IEnumerable<ChunkRange> GetLargeChunkRanges(List<Chunk> absoluteCountChunks)
        {
            long sum = 0;

            for (int i = 0; i < absoluteCountChunks.Count; i++)
            {
                Chunk chunk = absoluteCountChunks[i];
                var range = new ChunkRange
                {
                    StartIndexInclusive = sum,
                    EndIndexExclusive = sum + chunk.Absolute,
                    Chunk = chunk,
                    ChunkIndex = i
                };

                sum += chunk.Absolute;

                yield return range;
            }
        }

        public virtual void ValidateChunksCount<TProcessor>(IEntityDescription entity, List<Chunk> chunks, List<TProcessor> processors)
        {
            //Validate that number of Chunks match generators or modifier sets count.

            string entityDescription = $"for entity {entity.Type.FullName}";
            if(chunks.Count != processors.Count)
            {
                throw new ArgumentException($"Provided number of {nameof(Chunk)}s {chunks.Count} is different from number of {nameof(TProcessor)} {processors.Count} {entityDescription}.");
            }
        }

        public virtual void ValidateAbsoluteOnlyChunksSum(EntityContext entityContext, List<Chunk> chunks)
        {
            //If only absolute chunks exist, than sum of numbers should match targetCount.

            string entityDescription = $"for entity {entityContext.Type.FullName}";
        
            bool isOnlyAbsoluteChunks = chunks.All(x => x.IsAbsolute);

            long absoluteCount = chunks.Sum(x => x.Absolute);
            long target = entityContext.EntityProgress.TargetCount;
            bool absoluteCountMatchTarget = absoluteCount == target;

            if (isOnlyAbsoluteChunks && !absoluteCountMatchTarget)
            {
                throw new ArgumentException($"Provided sum of absolute count in {nameof(Chunk)}s {absoluteCount} is different from TargetCount {target} {entityDescription}. " +
                    $"You can add one proportional {nameof(Chunk)}, that will be used for any number of instances above sum of absolute {nameof(Chunk)}s.");
            }
        }

        public virtual void ValidateMixedChunksSum(EntityContext entityContext, List<Chunk> chunks)
        {
            //If both absolute & proportion chunks exist, then validate sum of numbers should match targetCount or be lower.
          
            string entityDescription = $"for entity {entityContext.Type.FullName}";
          
            bool hasProportionalChunks = chunks.Any(x => !x.IsAbsolute);
            bool hasAbsoluteChunks = chunks.Any(x => x.IsAbsolute);

            long target = entityContext.EntityProgress.TargetCount;
            long absoluteCount = chunks.Sum(x => x.Absolute);
            bool absoluteCountLowerOrEqualTarget = absoluteCount <= target;

            if (hasProportionalChunks && hasAbsoluteChunks && !absoluteCountLowerOrEqualTarget)
            {
                throw new ArgumentException($"Provided sum of absolute count in {nameof(Chunk)}s {absoluteCount} is higher than TargetCount {target} {entityDescription}. " +
                    $"It should be lower or equal to TargetCount.");
            }
        }
    }
}
