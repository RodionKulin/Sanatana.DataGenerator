﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Internals;
using System.Collections;

namespace Sanatana.DataGenerator.SpreadStrategies
{
    public class CartesianProductSpreadStrategy : CombinatoricsSpreadStrategy
    {
        //methods
        public override long GetTotalCount()
        {
            List<long> targetCounts = _parentEntities.Values
                .Select(x => x.EntityProgress.TargetCount)
                .ToList();

            if(targetCounts.Count == 0)
            {
                return 0;
            }

            long totalCount = targetCounts
                .Aggregate((long)1, (targetCount, total) => total * targetCount);
            return totalCount;
        }

        protected override IEnumerator<long[]> GetCombinationsEnumerator(List<long> sequencesLengths)
        {
            List<IEnumerator<long>> sequences = sequencesLengths
               .Select(x => GetSequentialEnumerator(x))
               .ToList();

            //get first item of each sequence
            sequences.ForEach(g => g.MoveNext());

            while (true)
            {
                // yield current values
                long[] currentCombination = sequences.Select(x => x.Current).ToArray();
                yield return currentCombination;

                for (int i = 0; i < sequences.Count; i++)
                {
                    IEnumerator<long> sequence = sequences[i];

                    // reset the slot if it couldn't move next
                    if (!sequence.MoveNext())
                    {
                        // stop when the last enumerator resets
                        if (sequence == sequences.Last())
                        {
                            yield break;
                        }
                        sequences[i] = GetSequentialEnumerator(sequencesLengths[i]);
                        sequences[i].MoveNext();
                        continue;
                    }
                    break;
                }
            }
        }
        
        public virtual IEnumerator<long> GetSequentialEnumerator(long maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                yield return i;
            }
        }

    }
}
