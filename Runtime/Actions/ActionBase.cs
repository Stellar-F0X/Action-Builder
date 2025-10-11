using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class ActionBase : ScriptableObject
    {
        public Sprite icon;


        [TextArea(3, 10)]
        public string description;

        public ActionDuration durationType;

        public StatSet usingStatsTemplate;


        [SerializeReference]
        protected List<EffectBase> _effects = new List<EffectBase>();


        [SerializeField]
        protected List<UGUID> _channelIds = new List<UGUID>();


        /// <summary> Runtime only </summary>
        protected List<EventChannelBase> _channels;


        public ActionController owner
        {
            get;
            set;
        }

        public GameObject target
        {
            get;
            set;
        }

        public virtual float duration
        {
            get { return this._effects.Max(e => e.duration); }
        }

        internal List<EffectBase> effects
        {
            get { return _effects; }
        }



        internal void Restart() { }

        
        internal void Start() { }

        

        public virtual void OnStart() { }


        public virtual void OnUpdate(float deltaTime) { }


        public virtual void OnPause() { }


        public virtual void OnCancel() { }


        public virtual void OnEnd() { }
    }
}