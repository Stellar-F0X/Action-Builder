using System.Collections.Generic;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class ActionBase : ScriptableObject
    {
        public Sprite icon;
        public string description;
        public ActionDuration durationType;

        [SerializeReference]
        protected List<EffectBase> _effects = new List<EffectBase>();

        [SerializeField]
        protected List<UGUID> _channelIds = new List<UGUID>();
        
        /// <summary> Runtime only </summary>
        protected List<EventChannelBase> _channels;


        public virtual float duration
        {
            get;
            set;
        }
        
        internal List<EffectBase> effects
        {
            get { return _effects; }
        }


#region Init Process
        
        internal void SetChannels(Dictionary<UGUID, EventChannelBase> channels)
        {
            if (_channelIds is null || _channelIds.Count == 0)
            {
                return;
            }
            
            _channels = new List<EventChannelBase>(channels.Count);
            
            foreach (UGUID channelId in _channelIds)
            {
                if (channelId.IsEmpty())
                {
                    Debug.LogWarning($"{typeof(ActionBase)}: channel Id is empty");
                    continue;
                }
                
                if (channels.TryGetValue(channelId, out EventChannelBase channel))
                {
                    _channels.Add(channel);
                }
            }
        }
        
#endregion

        internal void Restart()
        {
            
        }

        internal void Start()
        {
            
        }
        
        
        public virtual void OnStart() { }

        
        public virtual void OnUpdate(float deltaTime) { }

        
        public virtual void OnPause() { }


        public virtual void OnCancel()
        {
            
        }

        
        public virtual void OnEnd() { } 
    }
}