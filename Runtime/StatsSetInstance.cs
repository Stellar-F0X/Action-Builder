using System.Collections.Generic;

namespace StatController.Runtime
{
    public abstract class StatsSetInstance { }


    public class StatsSetInstance<TKey> : StatsSetInstance
    {
        public Dictionary<TKey, Stat> stats = new Dictionary<TKey, Stat>();
    }
}