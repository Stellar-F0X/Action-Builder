using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine;

namespace StatController.Runtime
{
    public abstract class StatSet : ScriptableObject
    {
        internal abstract bool ContainsKey(object key);

        internal abstract StatSetInstance CreateInstance();
    }

    
    public abstract class StatSet<TKey> : StatSet
    {
#if UNITY_EDITOR
        [SerializeField]
        private protected TKey _previewKey;

        [SerializeReference]
        private protected Stat _previewStat;
#endif

        [SerializeField]
        protected List<StatPair<TKey>> _statPairs = new List<StatPair<TKey>>();


        internal override bool ContainsKey(object key)
        {
            EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
            Assert.IsNotNull(comparer, "comparer is null");
            TKey convertedKey = (TKey)key;

            return _statPairs.Any(s => comparer.Equals(s.statKey, convertedKey));
        }


        internal override StatSetInstance CreateInstance()
        {
            StatSet<TKey> instantiatedSet = Object.Instantiate(this);
            Assert.IsNotNull(instantiatedSet, "Failed to instantiate StatSet");
            
            List<StatPair<TKey>> statPairs = instantiatedSet._statPairs;
            Assert.IsNotNull(statPairs, "statPairs is null");
            
            StatSetInstance<TKey> instance = new StatSetInstance<TKey>();

            for (int index = 0; index < statPairs.Count; ++index)
            {
                instance.stats.Add(statPairs[index].statKey, statPairs[index].stat);
            }

            return instance;
        }

        
        public override string ToString()
        {
            return string.Empty;
        }
    }
}