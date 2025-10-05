using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatController.Runtime
{
    [DefaultExecutionOrder(-1)]
    public class StatController : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)]
        private StatsSet _statsSet;
        
        [SerializeField]
        private bool _debug;

        private Type _statsSetKeyType;

        [SerializeReference]
        private protected StatsSetInstance _runtimeStats;


        public Type keyType
        {
            get { return _statsSetKeyType?.GenericTypeArguments[0]; }
        }



        protected virtual void Awake()
        {
            _statsSetKeyType = _statsSet?.GetType();
            _runtimeStats = _statsSet?.CreateInstance();
        }


        public Stat GetStat<TKey>(TKey key)
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                return null;
            }

            if (instance.stats.TryGetValue(key, out Stat stat))
            {
                return stat;
            }
            else
            {
                return null;
            }
        }


        public bool TryGetStat<TKey>(TKey key, out Stat stat)
        {
            stat = this.GetStat(key);

            if (stat != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool ExistStat<TKey>(TKey key)
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                return false;
            }
            else
            {
                return instance.stats.ContainsKey(key);
            }
        }


        public IEnumerable<KeyValuePair<TKey, Stat>> GetStatPairs<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (KeyValuePair<TKey, Stat> stat in instance.stats)
            {
                yield return stat;
            }
        }


        public IEnumerator<Stat> GetStats<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (Stat stat in instance.stats.Values)
            {
                yield return stat;
            }
        }


        public IEnumerator<TKey> GetKeys<TKey>()
        {
            if (_runtimeStats is not StatsSetInstance<TKey> instance)
            {
                yield break;
            }

            foreach (TKey stat in instance.stats.Keys)
            {
                yield return stat;
            }
        }


#if UNITY_EDITOR
        private void OnGUI()
        {
            if (this._debug == false)
            {
                return;
            }

            const float textHeight = 30f;
            
            GUI.Box(new Rect(2f, 2f, 200f, _runtimeStats.statCount * textHeight), string.Empty);
            GUI.Label(new Rect(4f, 2f, 100f, 30f), $"{this.name}'s Stats");

            Rect textRect = new Rect(4f, 22f, 150f, textHeight);
            Rect plusButtonRect = new Rect(textRect.x + textRect.width, textRect.y, 20f, 20f);
            Rect minusButtonRect = new Rect(plusButtonRect.x + 22, plusButtonRect.y, plusButtonRect.width, plusButtonRect.height);

            foreach (KeyValuePair<string, Stat> stat in _runtimeStats.GetStatPairs())
            {
                GUI.Label(textRect, $"{stat.Key}: {stat.Value.value:0.##;-0.##}");
                
                if (GUI.Button(plusButtonRect, "+"))
                {
                    stat.Value.value += 1f;
                }
                
                if (GUI.Button(minusButtonRect, "-"))
                {
                    stat.Value.value -= 1f;
                }
                
                textRect.y += 22f;
                plusButtonRect.y = textRect.y;
                minusButtonRect.y = textRect.y;
            }
        }
#endif
    }
}