using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public abstract class ActionBase : ScriptableObject
    {
        public event Action<ActionBase> onStarted;
        public event Action<ActionBase> onEnded;
        public event Action<ActionBase> onPaused;
        public event Action<ActionBase> onResumed;
        public event Action<ActionBase> onCancelled;


        [SerializeField, Space(-15)]
        protected ActionData _actionData;

        [Space(5)]
        public ActionDurationData durationData;
        
        [SerializeReference, SubclassSelector]
        public ConditionBase endCondition;
        
        [Space(5)]
        public StatSet usingStatsTemplate;
        
        
        /// <summary> Action 동작 경과 시간 </summary>
        private float _elapsedTime;
        
        /// <summary> 마지막으로 실행이 종료되었던 시간 </summary>
        private float _lastQuitTime;

        /// <summary> 현재 동작 중인 상태 </summary>
        private ActionState _currentState;
        
        /// <summary> Runtime only </summary>
        protected List<EventChannelBase> _channels;




#region Properties

        public ActionState currentState
        {
            get { return _currentState; }
        }

        
        public bool isActive
        {
            get { return _currentState is ActionState.Playing or ActionState.Paused; }
        }

        
        public bool isOnCooldown
        {
            get { return _lastQuitTime + durationData.cooldownTime > Time.time; }
        }

        
        public StatController statController 
        { 
            get;
            set; 
        }

        
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

        
        public virtual float duration
        {
            get { return _actionData.effects.Max(static e => e.duration); }
        }
        
        
        public IReadOnlyList<EffectBase> effects
        {
            get { return _actionData.effects; }
        }

        
        internal List<EffectBase> internalEffects
        {
            get { return _actionData.effects; }
        }

#endregion
        

        internal void OnCreate()
        {
            _actionData = new ActionData(this.name);
        }


        internal void Initialize(ActionController owner)
        {
            this.owner = owner;
            this._channels = new List<EventChannelBase>();
            this.statController = owner.GetComponent<StatController>();
            this.internalEffects.ForEach(e => e.referencedAction = this);

            if (usingStatsTemplate == null)
            {
                return;
            }

            if (statController == null)
            {
                Debug.LogError("StatController is not found in ActionController's GameObject");
                return;
            }

            if (usingStatsTemplate.keyType != statController.keyType)
            {
                Debug.LogError("Using Stats Template Key Type is not same as StatController Key Type");
            }
        }

        

        internal void InitializeChannels(Dictionary<UGUID, EventChannelBase> allChannels)
        {
            
        }



#region Control Methods

        internal bool Trigger(bool force = false)
        {
            if (isOnCooldown && force == false)
            {
                return false;
            }

            if (isActive && force)
            {
                this.OnEnd();
                _elapsedTime = 0f;
            }

            _elapsedTime = 0f;
            _currentState = ActionState.Playing;

            this.OnStart();
            onStarted?.Invoke(this);
            return true;
        }

        
        internal void Pause()
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            _currentState = ActionState.Paused;
            this.OnPause();
            onPaused?.Invoke(this);
        }

        
        internal void Resume()
        {
            if (_currentState != ActionState.Paused)
            {
                return;
            }

            _currentState = ActionState.Playing;
            this.OnResume();
            onResumed?.Invoke(this);
        }

        
        internal void Cancel()
        {
            if (isActive == false)
            {
                return;
            }

            _currentState = ActionState.Cancelled;
            this.OnCancel();
            onCancelled?.Invoke(this);
            _lastQuitTime = Time.time;

            this.OnEnd();
            onEnded?.Invoke(this);
        }

        
        internal void Execute(float deltaTime)
        {
            if (_currentState != ActionState.Playing)
            {
                return;
            }

            switch (durationData.durationType)
            {
                case ActionDuration.Duration:
                {
                    if (_elapsedTime > duration)
                    {
                        _lastQuitTime = Time.time;
                        _currentState = ActionState.Finished;
                        this.OnEnd();
                        onEnded?.Invoke(this);
                        return;
                    }

                    _elapsedTime += deltaTime;
                    this.OnUpdate(deltaTime);
                    break;
                }

                case ActionDuration.Instant:
                {
                    this.OnUpdate(deltaTime);
                    _lastQuitTime = Time.time;
                    _currentState = ActionState.Finished;
                    this.OnEnd();
                    onEnded?.Invoke(this);
                    break;
                }

                case ActionDuration.Infinite:
                {
                    _elapsedTime += deltaTime;
                    this.OnUpdate(deltaTime);
                    break;
                }
            }
        }
        
        
        public override string ToString()
        {
            return $"{typeof(ActionBase)} {actionName} (State: {_currentState}, Cooldown: {isOnCooldown}, Elapsed: {_elapsedTime:F2}s)";
        }
        
        
#endregion
        

        protected virtual void OnUpdate(float deltaTime) { }
        
        
        protected virtual void OnStart() { }
        
        
        protected virtual void OnPause() { }
        
        
        protected virtual void OnResume() { }
        
        
        protected virtual void OnCancel() { }
        
        
        protected virtual void OnEnd() { }
    }
}