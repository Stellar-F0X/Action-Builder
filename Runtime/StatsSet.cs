using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace StatController.Runtime
{
    [CreateAssetMenu(fileName = "New stats set", menuName = "Stat set")]
    public class StatsSet : ScriptableObject
    {
        [SerializeReference]
        private List<IStatKey> _statKeys; 
        
        [SerializeReference]
        private List<Stat> _stats; 
        
        
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
            Assert.IsTrue(_statKeys.Count == _stats.Count);
            StatsSetInstance instance = new StatsSetInstance();

            if (_statKeys.Count == 0)
            {
                return instance;
            }

            for (int index = 0; index < _statKeys.Count; ++index)
            {
                instance.AddStat(_statKeys[index], _stats[index]);
            }

            return instance;
        }
    }
}