using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace ActionBuilder.Runtime
{
    public abstract class StatSet : ScriptableObject
    {
        public abstract Type keyType
        {
            get;
        }
        
        internal abstract bool ContainsKey(object key);


        internal abstract IEnumerable<KeyValuePair<string, Stat>> GetStatPairs();

        
        internal abstract StatSetInstance CreateInstance();
    }

    
    public class StatSet<TKey> : StatSet
    {
#if UNITY_EDITOR
        [SerializeField]
        private protected TKey _previewKey;

        [SerializeReference]
        private protected Stat _previewStat;
#endif

        [SerializeField]
        protected List<StatPair<TKey>> _statPairs = new List<StatPair<TKey>>();
        
        
        public override Type keyType
        {
            get { return typeof(TKey); }
        }


        internal override bool ContainsKey(object key)
        {
            EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
            Assert.IsNotNull(comparer, "comparer is null");
            TKey convertedKey = (TKey)key;

            return _statPairs.Any(s => comparer.Equals(s.statKey, convertedKey));
        }

        
        internal override IEnumerable<KeyValuePair<string, Stat>> GetStatPairs()
        {
            Assert.IsNotNull(_statPairs, "_statPairs is null");
            
            if (_statPairs.Count == 0)
            {
                yield break;
            }
            
            foreach (StatPair<TKey> pair in _statPairs)
            {
                yield return new KeyValuePair<string, Stat>(pair.statKey.ToString(), pair.stat);
            }
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