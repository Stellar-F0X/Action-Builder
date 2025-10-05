using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    [DefaultExecutionOrder(-1)]
    public class StatController : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)]
        private StatsSet _statsSet;
        private Type _statsSetKeyType;

        [SerializeReference, ReadOnly]
        private protected StatsSetInstance _runtimeStats;
        

        public Type keyType
        {
            get { return _statsSetKeyType?.GenericTypeArguments[0]; }
        }

        

        protected virtual void Awake()
        {
            _statsSetKeyType = _statsSet?.GetType();
            _runtimeStats = _statsSet?.CreateInstance();
        }


        public Stat GetStat<TKey>(TKey key)
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                return null;
            }

            if (instance.stats.TryGetValue(key, out Stat stat))
            {
                return stat;
            }
            else
            {
                return null;
            }
        }


        public bool TryGetStat<TKey>(TKey key, out Stat stat)
        {
            stat = this.GetStat(key);

            if (stat != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool ExistStat<TKey>(TKey key)
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                return false;
            }
            else
            {
                return instance.stats.ContainsKey(key);
            }
        }


        public IEnumerable<KeyValuePair<TKey, Stat>> GetStatPairs<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (KeyValuePair<TKey, Stat> stat in instance.stats)
            {
                yield return stat;
            }
        }


        public IEnumerator<Stat> GetStats<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (Stat stat in instance.stats.Values)
            {
                yield return stat;
            }
        }


        public IEnumerator<TKey> GetKeys<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (TKey stat in instance.stats.Keys)
            {
                yield return stat;
            }
        }
    }
}