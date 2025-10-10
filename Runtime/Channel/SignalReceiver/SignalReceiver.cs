using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class SignalReceiver
    {
        [HideInInspector]
        public float time;
        
        public SignalReceiveType receiveType;
        
        [SerializeReference]
        private List<ConditionBase> _conditions;
        
        
        public IReadOnlyList<ConditionBase> conditions
        {
            get { return _conditions; }
        }
        
        internal List<ConditionBase> conditionsInternal
        {
            get { return _conditions; }
        }
    }
}