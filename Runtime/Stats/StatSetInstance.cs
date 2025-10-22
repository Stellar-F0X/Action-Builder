using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionBuilder.Runtime
{
    internal abstract class StatSetInstance
    {
        public abstract int statCount
        {
            get;
        }

        public abstract Type keyType
        {
            get;
        }
        
        public abstract IEnumerable<KeyValuePair<string, Stat>> GetStatPairs();
    }


    [Serializable]
    internal class StatSetInstance<TKey> : StatSetInstance, ISerializationCallbackReceiver
    {
        public Dictionary<TKey, Stat> stats = new Dictionary<TKey, Stat>();

        
        [SerializeField]
        private List<StatPair<TKey>> _pairList = new List<StatPair<TKey>>(); //PropertyDrawer 용도.

        

        public override int statCount
        {
            get { return stats.Count; }
        }

        public override Type keyType
        {
            get { return typeof(TKey); }
        }

        

        public override IEnumerable<KeyValuePair<string, Stat>> GetStatPairs()
        {
            Assert.IsNotNull(stats, "_statPairs is null");
            
            if (stats.Count == 0)
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