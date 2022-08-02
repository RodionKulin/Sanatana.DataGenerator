using Sanatana.DataGenerator.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Collections
{
    public class RangeSet<TEntity>
    {
        protected Dictionary<TEntity, (TEntity, long)> _existingInstancesCache;
        protected IEqualityComparer<TEntity> _comparer;
        protected long? _cacheMaxId = null;
        protected Func<TEntity, long> _idSelector = null;


        //init
        public RangeSet(Func<TEntity, long> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
            _comparer = new KeyEqualityComparer<TEntity, long>(idSelector);
            _existingInstancesCache = new Dictionary<TEntity, (TEntity, long)>(_comparer);
        }


        //methods
        public bool IsWithinKnownRange(long[] nextIds)
        {
            return _cacheMaxId != null && nextIds.All(nextId => nextId <= _cacheMaxId);
        }

        public void AddRange(List<TEntity> items)
        {
            foreach (TEntity item in items)
            {
                if (!_existingInstancesCache.ContainsKey(item))
                {
                    long id = _idSelector(item);
                    _existingInstancesCache.Add(item, (item, id));
                }
            }

            _cacheMaxId = _existingInstancesCache.Values.Select(x => x.Item2).Max();
        }

        public void RemoveItemsBeforeNewIds(long[] newIds)
        {
            long newIdsMinId = newIds.Min();

            _existingInstancesCache = _existingInstancesCache.Values
                .Where(x => x.Item2 >= newIdsMinId)
                .ToDictionary(x => x.Item1, _comparer);
        }

        public bool TryGetValue(TEntity newItem, out TEntity existingItem)
        {
            bool exist = _existingInstancesCache.TryGetValue(newItem, out (TEntity, long) existing);
            existingItem = exist
                ? existing.Item1
                : default(TEntity);

            return exist;
        }

        public bool ContainsKey(TEntity item)
        {
            return _existingInstancesCache.ContainsKey(item);
        }
    }
}
