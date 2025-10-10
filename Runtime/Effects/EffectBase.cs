using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public abstract class EffectBase
    {
        public string name;
        
        [HideInInspector]
        public bool enable = true;
        
        [TextArea(2, 5)]
        public string description;

        [Space(3)]
        public EffectPlayOption playOption;
        public EffectFinishOption finishOption;

        public int level;
        public float duration;

        private bool _isPlaying;
        private bool _applied;

        [SerializeReference, HideInInspector]
        private ActionBase _referencedAction;

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool isExpanded = true;
#endif


        public ActionBase referencedAction
        {
            get { return _referencedAction; }
            
            internal set { _referencedAction = value; }
        }

        public bool isPlaying
        {
            get { return _isPlaying; }
        }

        public bool isApplied
        {
            get { return _applied; }
        }


        public void Start()
        {
            _isPlaying = true;
            OnStart();
        }


        public void End()
        {
            OnEnd();
            _isPlaying = true;
        }

        
        public void Apply()
        {
            _applied = true;
            OnApply();
        }
        

        public void Release()
        {
            _applied = false;
            OnRelease();
        }


        public virtual void OnApply() { }

        public virtual void OnRelease() { }


        public virtual void OnStart() { } 

        public virtual void OnUpdate(float deltaTime) { } 

        public virtual void OnEnd() { } 
    }
}