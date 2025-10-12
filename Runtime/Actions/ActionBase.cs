using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class ActionBase : ScriptableObject
    {
        [SerializeField, Space(-15)]
        protected ActionData _actionData;

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
        
        public Sprite icon
        {
            get { return _actionData.icon; }
        }
        
        public string actionName
        {
            get { return _actionData.name; }
        }
        
        public string description
        {
            get { return _actionData.description; }
        }
        
        public float cooldownTime
        {
            get { return _actionData.cooldownTime; }
        }

        public virtual float duration
        {
            get { return _actionData.effects.Max(e => e.duration); }
        }

        internal List<EffectBase> effects
        {
            get { return _actionData.effects; }
        }


        internal void OnCreate()
        {
            _actionData = new ActionData(this.name);
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