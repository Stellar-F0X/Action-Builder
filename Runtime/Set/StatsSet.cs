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


        public abstract StatsSetInstance CreateInstance();
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


        public override StatsSetInstance CreateInstance()
        {
            StatsSetInstance<TKey> instance = new StatsSetInstance<TKey>();

            if (_statPairs.Count == 0)
            {
                return instance;
            }

            for (int index = 0; index < _statPairs.Count; ++index)
            {
                instance.AddStat(_statPairs[index].statKey, _statPairs[index].stat);
            }

            return instance;
        }
    }
}