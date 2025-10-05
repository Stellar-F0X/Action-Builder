using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    internal abstract class StatsSetInstance { }


    [Serializable]
    internal class StatsSetInstance<TKey> : StatsSetInstance, ISerializationCallbackReceiver
    {
        public Dictionary<TKey, Stat> stats = new Dictionary<TKey, Stat>();

        [SerializeField]
        private List<StatPair<TKey>> _pairList = new List<StatPair<TKey>>();


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