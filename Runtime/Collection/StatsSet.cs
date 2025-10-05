using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

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


        public abstract bool ContainsKey(object key);

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


        public override bool ContainsKey(object key)
        {
            EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
            Assert.IsNotNull(comparer, "comparer is null");

            TKey convertedKey = (TKey)key;

            return _statPairs.Any(s => comparer.Equals(s.statKey, convertedKey));
        }
        

        internal override StatsSetInstance CreateInstance()
        {
            Assert.IsNotNull(_statPairs);
            
            StatsSet<TKey> instantiatedSet = Object.Instantiate(this);
            Assert.IsNotNull(instantiatedSet);
            
            List<StatPair<TKey>> statPairs = instantiatedSet._statPairs;
            StatsSetInstance<TKey> instance = new StatsSetInstance<TKey>();
            
            for (int index = 0; index < statPairs.Count; ++index)
            {
                instance.stats.Add(statPairs[index].statKey, statPairs[index].stat);
            }

            return instance;
        }
    }
}