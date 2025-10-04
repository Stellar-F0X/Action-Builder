using System.Collections.Generic;

namespace StatController.Runtime
{
    public class StatKeyEqualityComparer : IEqualityComparer<IStatKey>
    {
        public readonly static StatKeyEqualityComparer Comparer = new StatKeyEqualityComparer();
        
        
        
        public bool Equals(IStatKey x, IStatKey y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return Equals(x.boxedKeyObject, y.boxedKeyObject);
        }
        

        public int GetHashCode(IStatKey obj)
        {
            return obj.boxedKeyObject != null ? obj.boxedKeyObject.GetHashCode() : 0;
        }
    }
}