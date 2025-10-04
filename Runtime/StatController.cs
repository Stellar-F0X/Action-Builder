using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    [DefaultExecutionOrder(-1)]
    public class StatController : MonoBehaviour
    {
        [SerializeField]
        private StatsSet _statsSet;
        
        protected Type _statsSetKeyType;
        protected StatsSetInstance _runtimeStats;


        
        public Type keyType
        {
            get { return _statsSetKeyType.GenericTypeArguments[0]; }
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


        public IEnumerator<Stat> GetStats<TKey>(params TKey[] keys)
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
