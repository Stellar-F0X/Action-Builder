using System.Collections.Generic;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class EventChannelBase : ScriptableObject
    {
        public List<SignalReceiver> signals;
        
        [SerializeField, HideInInspector]
        private UGUID _channelGuid = UGUID.Create(); 
        


        public void Register()
        {
            
        }

        public void Unregister()
        {
            
        }

        public bool HasRegistered()
        {
            return false;
        }
    }
}