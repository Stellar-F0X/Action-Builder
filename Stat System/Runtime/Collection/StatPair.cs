using System;
using UnityEngine;

namespace StatSystem.Runtime
{
    [Serializable]
    public struct StatPair<TKey>
    {
        public StatPair(TKey key, Stat stat)
        {
            this.statKey = key;
            this.stat = stat;
        }
        
        [SerializeReference]
        public TKey statKey; 
        
        [SerializeReference]
        public Stat stat; 
    }
}