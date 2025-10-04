using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    public abstract class StatsSet : ScriptableObject
    {
        //TODO: ToJson 구현
        public virtual string ToJson()
        {
            return string.Empty;
        }
        
        
        //TODO: ToXml 구현
        public virtual string ToXml()
        {
            return string.Empty;
        }


        internal abstract StatsSetInstance CreateInstance();
    }
    
    
    public class StatsSet<TKey> : StatsSet
    {
#if UNITY_EDITOR
        [SerializeField]
        private protected TKey _previewKey;
        
        [SerializeReference]
        private protected Stat _previewStat;
#endif
        
        [SerializeField]
        protected List<StatPair<TKey>> _statPairs;


        internal override StatsSetInstance CreateInstance()
        {
            StatsSetInstance<TKey> instance = new StatsSetInstance<TKey>();
            
            for (int index = 0; index < _statPairs.Count; ++index)
            {
                instance.stats.Add(_statPairs[index].statKey, _statPairs[index].stat);
            }

            return instance;
        }
    }
}