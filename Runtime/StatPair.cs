using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public struct StatPair
    {
        public StatPair(IStatKey key, Stat stat)
        {
            this.statKey = key;
            this.stat = stat;
        }
        
        [SerializeReference]
        public IStatKey statKey; 
        
        [SerializeReference]
        public Stat stat; 
    }
}