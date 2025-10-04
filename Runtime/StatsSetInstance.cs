using System.Collections.Generic;
using UnityEngine.Assertions;

namespace StatController.Runtime
{
    public abstract class StatsSetInstance
    {
        public abstract IEnumerator<Stat> GetStats();

        public abstract int statsCount
        {
            get;
        }
    }


    public class StatsSetInstance<TKey> : StatsSetInstance
    {
        private Dictionary<TKey, Stat> _stats = new Dictionary<TKey, Stat>();
        
        public override int statsCount
        {
            get { return _stats.Count; }
        }
        
        
        internal bool AddStat(TKey key, Stat stat)
        {
            if (_stats.ContainsKey(key))
            {
                return false;
            }
            else
            {
                _stats.Add(key, stat);
                return true;
            }
        }


        internal bool RemoveStat(TKey key)
        {
            return _stats.Remove(key);
        }


        public Stat GetStat(TKey stat)
        {
            this._stats.TryGetValue(stat, out Stat statValue);

            Assert.IsNotNull(statValue);

            return statValue;
        }


        public IEnumerator<TKey> GetStatKeys()
        {
            if (this._stats is null || this._stats.Count == 0)
            {
                yield break;
            }

            foreach (KeyValuePair<TKey, Stat> stat in this._stats)
            {
                yield return stat.Key;
            }
        }


        public override IEnumerator<Stat> GetStats()
        {
            if (this._stats is null || this._stats.Count == 0)
            {
                yield break;
            }

            foreach (KeyValuePair<TKey, Stat> stat in this._stats)
            {
                yield return stat.Value;
            }
        }
    }
}