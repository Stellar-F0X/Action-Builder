using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("StatController.Tool")]
namespace StatController.Runtime
{
    [CreateAssetMenu(fileName = "New stats set", menuName = "Stat set")]
    public class StatsSet : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeReference]
        private protected IStatKey _previewKey;
        
        [SerializeReference]
        private protected Stat _previewStat;
#endif
        
        [SerializeField]
        private List<StatPair> _statPairs;


        public List<StatPair> statPairs
        {
            get { return _statPairs; }
        }
        
        
        //TODO: ToJson 구현
        public string ToJson()
        {
            return string.Empty;
        }
        
        
        //TODO: ToXml 구현
        public string ToXml()
        {
            return string.Empty;
        }


        public StatsSetInstance CreateInstance()
        {
            StatsSetInstance instance = new StatsSetInstance();

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