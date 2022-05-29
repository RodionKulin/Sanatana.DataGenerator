using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Comparers
{
    public class IdEqualityComparer<TEntity> : IEqualityComparer<TEntity>
    {
        protected Func<TEntity, long> _idSelector;


        //init
        public IdEqualityComparer(Func<TEntity, long> idSelector)
        {
            _idSelector = idSelector;
        }


        //IEqualityComparer<TEntity> methods
        public virtual bool Equals(TEntity x, TEntity y)
        {
            if(x == null || y == null)
            {
                return false;
            }

            long xId = _idSelector(x);
            long yId = _idSelector(y);
            return xId == yId;
        }

        public virtual int GetHashCode(TEntity obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}
