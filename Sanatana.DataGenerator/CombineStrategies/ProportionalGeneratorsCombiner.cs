using Sanatana.DataGenerator.CombineStrategies.ProportionalCombine;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Each generator is called in proportion provided and in circular order.
    /// </summary>
    public class ProportionalGeneratorsCombiner : IGeneratorsCombiner
    {
        //fields
        protected ProportionalCombineService _proportionService;
        protected List<Chunk> _chunks;
        protected Dictionary<Type, IEnumerator<ChunkRange>> _chunkRanges;


        //init
        /// <summary>
        /// Initialize with list of chunks per generator. Chunk determines how often generator will be used.
        /// </summary>
        /// <param name="chunks"></param>
        public ProportionalGeneratorsCombiner(IEnumerable<Chunk> chunks)
        {
            _proportionService = new ProportionalCombineService();
            _chunks = chunks.ToList();
        }

        /// <summary>
        /// Initialize with list of chunks per generator. Chunk determines how often generator will be used.
        /// List of chunks in formats like:
        /// "\d"  for Absolute
        /// "\d*" for Proportion
        /// </summary>
        /// <param name="chunks"></param>
        public ProportionalGeneratorsCombiner(IEnumerable<string> chunks)
        {
            _proportionService = new ProportionalCombineService();
            _chunks = chunks.Select(x => Chunk.FromString(x)).ToList();
        }


        //methods
        public virtual IGenerator GetNext(List<IGenerator> generators, GeneratorContext generatorContext)
        {
            return _proportionService.GetNextProcessor(generators, _chunks, _chunkRanges, generatorContext);
        }

        public virtual void Setup(List<IGenerator> generators, GeneratorServices generatorServices)
        {
            //Case 1: can be setup for multiple entities with only default Generator;
            //Case 2: can be setup for one entity;
            //Case 3: can be setup for multiple entities sharing same Combiner.
            //Setup is called only once in any case.

            foreach (IEnumerator<ChunkRange> item in _chunkRanges.Values)
            {
                item.Dispose();
            }
            _chunkRanges.Clear();
        }

        public virtual void ValidateBeforeSetup(List<IGenerator> generators, IEntityDescription entity, DefaultSettings defaults)
        {
            _proportionService.ValidateChunksCount(entity, _chunks, generators);
        }

        public virtual void ValidateAfterSetup(List<IGenerator> generators, EntityContext entityContext, DefaultSettings defaults)
        {
            _proportionService.ValidateAbsoluteOnlyChunksSum(entityContext, _chunks);
            _proportionService.ValidateMixedChunksSum(entityContext, _chunks);
        }

    }
}
