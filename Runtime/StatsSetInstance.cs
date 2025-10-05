using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    internal abstract class StatsSetInstance
    {
        public abstract int statCount
        {
            get;
        }
        
        public abstract IEnumerable<Stat> GetStats();

        public abstract IEnumerable<string> GetStatNames();
        
        public abstract IEnumerable<KeyValuePair<string, Stat>> GetStatPairs();
    }


    [Serializable]
    internal class StatsSetInstance<TKey> : StatsSetInstance, ISerializationCallbackReceiver
    {
        public Dictionary<TKey, Stat> stats = new Dictionary<TKey, Stat>();

        [SerializeField]
        private List<StatPair<TKey>> _pairList = new List<StatPair<TKey>>();


        public override int statCount
        {
            get { return stats.Count; }
        }

        
        public override IEnumerable<Stat> GetStats()
        {
            if (stats is null || stats.Count == 0)
            {
                yield break;
            }
            
            foreach (Stat stat in stats.Values)
            {
                yield return stat;
            }
        }

        
        public override IEnumerable<string> GetStatNames()
        {
            if (stats is null || stats.Count == 0)
            {
                yield break;
            }
            
            foreach (TKey key in stats.Keys)
            {
                yield return key.ToString();
            }
        }

        
        public override IEnumerable<KeyValuePair<string, Stat>> GetStatPairs()
        {
            if (stats is null || stats.Count == 0)
            {
                yield break;
            }
            
            foreach (KeyValuePair<TKey, Stat> pair in stats)
            {
                yield return new KeyValuePair<string, Stat>(pair.Key.ToString(), pair.Value);
            }
        }


        public void OnBeforeSerialize()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (stats.Count == 0)
            {
                return;
            }

            _pairList.Clear();

            foreach (KeyValuePair<TKey, Stat> pair in this.stats)
            {
                _pairList.Add(new StatPair<TKey>(pair.Key, pair.Value));
            }
        }


        public void OnAfterDeserialize() { }
    }
}