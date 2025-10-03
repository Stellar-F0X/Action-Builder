using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public class StatsSetInstance
    {
        private Dictionary<IStatKey, Stat> _stats = new Dictionary<IStatKey, Stat>(StatKeyEqualityComparer.Comparer);


        internal bool AddStat(IStatKey key, Stat stat)
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


        internal bool RemoveStat(IStatKey key)
        {
            return _stats.Remove(key);
        }


        public Stat GetStat(IStatKey stat)
        {
            this._stats.TryGetValue(stat, out Stat statValue);

            Assert.IsNotNull(statValue);

            return statValue;
        }


        public IEnumerator<IStatKey> GetStatTypes()
        {
            if (this._stats is null || this._stats.Count == 0)
            {
                yield break;
            }

            foreach (KeyValuePair<IStatKey, Stat> stat in this._stats)
            {
                yield return stat.Key;
            }
        }
    }
}